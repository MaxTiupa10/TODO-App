using TODO_App.Services.Helpers;

namespace TODO_App.Tests.Unit;

public class TaskQueryDateParserTests
{
    [Theory]
    [InlineData("2026-06-20")]
    [InlineData("2026/06/20")]
    public void TryParse_ValidDate_ReturnsDateOnly(string value)
    {
        var success = TaskQueryDateParser.TryParse(value, out var date, out var error);

        Assert.True(success);
        Assert.Null(error);
        Assert.Equal(new DateOnly(2026, 6, 20), date);
    }

    [Theory]
    [InlineData("20.06.2026")]
    [InlineData("not-a-date")]
    public void TryParse_InvalidDate_ReturnsError(string value)
    {
        var success = TaskQueryDateParser.TryParse(value, out var date, out var error);

        Assert.False(success);
        Assert.NotNull(error);
        Assert.Null(date);
    }

    [Fact]
    public void NormalizeRange_SwapsDatesWhenFromIsAfterTo()
    {
        var from = new DateOnly(2026, 6, 20);
        var to = new DateOnly(2026, 6, 10);

        var (normalizedFrom, normalizedTo) = TaskQueryDateParser.NormalizeRange(from, to);

        Assert.Equal(to, normalizedFrom);
        Assert.Equal(from, normalizedTo);
    }
}
