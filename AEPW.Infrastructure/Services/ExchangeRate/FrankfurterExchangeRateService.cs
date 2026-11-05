using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using AEPW.Application.Interfaces.Services.ExchangeRate;
using Microsoft.Extensions.Logging;

namespace AEPW.Infrastructure.Services.ExchangeRate;

/// <summary>
/// Calls the Frankfurter API (https://api.frankfurter.dev) for a historical Source-currency-to-USD
/// rate. Historical rates are immutable, so successful (and failed) lookups are cached in-memory
/// for the process lifetime, keyed by (currency, date).
/// </summary>
public sealed class FrankfurterExchangeRateService : IExchangeRateService
{
    private static readonly ConcurrentDictionary<(string Currency, DateOnly Date), (decimal Rate, DateOnly RateDate)?> Cache = new();

    private readonly HttpClient _httpClient;
    private readonly ILogger<FrankfurterExchangeRateService> _logger;

    public FrankfurterExchangeRateService(HttpClient httpClient, ILogger<FrankfurterExchangeRateService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<(decimal Rate, DateOnly RateDate)?> GetRateToUsdAsync(
        string sourceCurrency,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var currency = sourceCurrency.ToUpperInvariant();
        var cacheKey = (currency, date);

        if (Cache.TryGetValue(cacheKey, out var cached))
        {
            _logger.LogInformation(
                "[FX] Cache hit for Currency={Currency}, Date={Date:yyyy-MM-dd} - returning cached result (Rate={Rate}, RateDate={RateDate}) without calling Frankfurter.",
                currency,
                date,
                cached?.Rate,
                cached?.RateDate);

            return cached;
        }

        (decimal Rate, DateOnly RateDate)? result;

        try
        {
            var requestUri = $"v1/{date:yyyy-MM-dd}?base={currency}&symbols=USD";

            _logger.LogInformation(
                "[FX] Building Frankfurter request payload - Currency={Currency}, Date={Date:yyyy-MM-dd}, Method=GET, RequestUri={BaseAddress}{RequestUri}",
                currency,
                date,
                _httpClient.BaseAddress,
                requestUri);

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation(
                "[FX] Frankfurter response received - StatusCode={StatusCode}, Body={Body}",
                (int)response.StatusCode,
                rawBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[FX] Frankfurter returned {StatusCode} for {Currency} on {Date}.",
                    (int)response.StatusCode,
                    currency,
                    date);

                result = null;
            }
            else
            {
                var payload = JsonSerializer.Deserialize<FrankfurterResponse>(rawBody);

                if (payload?.Rates is null || !payload.Rates.TryGetValue("USD", out var rate))
                {
                    _logger.LogWarning(
                        "[FX] Frankfurter response for {Currency} on {Date} had no USD rate.",
                        currency,
                        date);

                    result = null;
                }
                else
                {
                    var rateDate = DateOnly.TryParse(payload.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
                        ? parsedDate
                        : date;

                    _logger.LogInformation(
                        "[FX] Parsed Frankfurter payload - Base={Base}, Currency={Currency}, RequestedDate={RequestedDate:yyyy-MM-dd}, Rate={Rate}, EffectiveRateDate={RateDate:yyyy-MM-dd}.",
                        payload.Base,
                        currency,
                        date,
                        rate,
                        rateDate);

                    result = (rate, rateDate);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[FX] Frankfurter call failed for {Currency} on {Date}.", currency, date);
            result = null;
        }

        Cache[cacheKey] = result;
        return result;
    }

    private sealed class FrankfurterResponse
    {
        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("base")]
        public string? Base { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
