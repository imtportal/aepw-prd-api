namespace AEPW.Application.Interfaces.Services.ExchangeRate;

/// <summary>
/// Resolves a historical currency-to-USD exchange rate (Frankfurter). Never throws - callers
/// treat a null result as "rate unavailable, leave the FX fields empty for manual entry".
/// </summary>
public interface IExchangeRateService
{
    /// <summary>
    /// Returns the Source-currency-to-USD rate effective on <paramref name="date"/>, and the
    /// actual date that rate applies to (Frankfurter falls back to the nearest prior working
    /// day for weekends/holidays, so this may differ from <paramref name="date"/>). Returns
    /// null when the rate can't be resolved (HTTP failure, unsupported currency, timeout, etc.).
    /// </summary>
    Task<(decimal Rate, DateOnly RateDate)?> GetRateToUsdAsync(
        string sourceCurrency,
        DateOnly date,
        CancellationToken cancellationToken = default);
}
