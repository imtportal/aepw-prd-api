using AEPW.Application.Services.Evidence;
using Xunit;

namespace AEPW.Application.Tests.Services.Evidence;

public sealed class EvidenceProcessingServiceAmountParsingTests
{
    [Theory]
    [InlineData("9.546.364,00", 9546364.00)]       // BRL/European: '.'=thousands, ','=decimal
    [InlineData("9,546,364.00", 9546364.00)]       // US/UK: ','=thousands, '.'=decimal
    [InlineData("1.234,5", 1234.5)]                // '.'=thousands, ','=decimal (1 digit after)
    [InlineData("1,234", 1234)]                    // single comma, 3 digits after -> thousands
    [InlineData("1234.56", 1234.56)]               // single dot, 2 digits after -> decimal
    [InlineData("até 9.546.364,00", 9546364.00)]   // text prefix stripped
    [InlineData("R$ 9.546.364,00", 9546364.00)]    // currency symbol/code stripped
    public void ParseFlexibleAmount_HandlesBothLocales(string raw, decimal expected)
    {
        var result = EvidenceProcessingService.ParseFlexibleAmount(raw);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("0,00")]
    public void ParseFlexibleAmount_ReturnsNullForUnparsableOrZero(string? raw)
    {
        var result = EvidenceProcessingService.ParseFlexibleAmount(raw);

        Assert.Null(result);
    }

    [Fact]
    public void ParseFlexibleAmount_PreservesLeadingMinusSign()
    {
        var result = EvidenceProcessingService.ParseFlexibleAmount("-1.234,56");

        Assert.Equal(-1234.56m, result);
    }
}
