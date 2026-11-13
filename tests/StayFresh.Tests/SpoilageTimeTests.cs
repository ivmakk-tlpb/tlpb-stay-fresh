using StayFresh;
using Xunit;

public class SpoilageTimeTests
{
    [Fact]
    public void The_time_left_is_the_condition_over_the_rate_in_in_game_seconds()
    {
        // 50 / (0.018 x 1 x 1 x 0.5) scaled seconds, and one scaled second is 10 in-game seconds.
        Assert.Equal(55555.56f, SpoilageTime.SecondsLeft(50f, 0.018f, 0f, 1f, 0.5f).Value, 1);
    }

    [Fact]
    public void Salt_preservation_slows_the_rate_by_at_most_90_percent()
    {
        Assert.Equal(1000f / 0.55f, SpoilageTime.SecondsLeft(1f, 0.01f, 0.5f, 1f, 1f).Value, 1);
        Assert.Equal(10000f, SpoilageTime.SecondsLeft(1f, 0.01f, 1f, 1f, 1f).Value, 1);
    }

    [Theory]
    [InlineData(50f, 0f, 1f, 1f)]
    [InlineData(50f, 0.018f, 0f, 1f)]
    [InlineData(50f, 0.018f, 1f, 0f)]
    [InlineData(0f, 0.018f, 1f, 1f)]
    public void A_rate_of_0_or_a_condition_of_0_gives_no_time(float condition, float decayRate, float gameFactor, float modFactor)
    {
        Assert.Null(SpoilageTime.SecondsLeft(condition, decayRate, 0f, gameFactor, modFactor));
    }

    [Theory]
    [InlineData(20f, 1f)]
    [InlineData(5f, 1f)]
    [InlineData(2.5f, 0.6f)]
    [InlineData(0f, 0.2f)]
    [InlineData(-10f, 0.2f)]
    public void The_cold_slows_the_spoilage_below_5_degrees(float temperature, float factor)
    {
        Assert.Equal(factor, SpoilageTime.ColdFactor(temperature), 5);
    }

    private static readonly SpoilageTime.Words English = new SpoilageTime.Words("week", "weeks", "day", "days", "hour", "hours");

    [Theory]
    [InlineData(0.5f, "<1 hour")]
    [InlineData(1f, "1 hour")]
    [InlineData(5.9f, "5 hours")]
    [InlineData(24f, "1 day")]
    [InlineData(30.5f, "1 day 6 hours")]
    [InlineData(49f, "2 days 1 hour")]
    [InlineData(195f, "1 week 1 day")]
    [InlineData(509f, "3 weeks")]
    [InlineData(3000f, "17 weeks 6 days")]
    public void The_time_shows_the_largest_two_units(float hours, string text)
    {
        Assert.Equal(text, SpoilageTime.Format(hours * 3600f, English));
    }

    [Fact]
    public void The_text_starts_with_spoils_in()
    {
        Assert.Equal("Spoils in 1 day 6 hours", SpoilageTime.Text("1 day 6 hours"));
    }
}
