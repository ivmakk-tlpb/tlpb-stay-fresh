using StayFresh;
using Xunit;

public class TooltipOrderTests
{
    [Fact]
    public void The_line_goes_right_after_the_weight_line()
    {
        var lines = new[] { "Cooked Deer Meat", "A good source of protein", "6<sprite=123>", "100% condition", "+35", "\n\nRight click for options", "Spoils in 1 day" };

        Assert.Equal(3, TooltipOrder.Target(lines, 6));
    }

    [Fact]
    public void With_no_weight_line_the_line_goes_above_the_hint()
    {
        var lines = new[] { "Berry Juice", "100% condition", "\n\nRight click for options", "Spoils in 1 day" };

        Assert.Equal(2, TooltipOrder.Target(lines, 3));
    }

    [Fact]
    public void Lines_that_are_not_shown_are_skipped()
    {
        var lines = new[] { "Cooked Deer Meat", null, "6<sprite=123>", "Spoils in 1 day" };

        Assert.Equal(-1, TooltipOrder.Target(lines, 3));
        Assert.Equal(-1, TooltipOrder.Target(new[] { "Cooked Deer Meat", "100% condition", "Spoils in 1 day" }, 2));
    }

    [Fact]
    public void Only_lines_above_the_own_line_count()
    {
        Assert.Equal(-1, TooltipOrder.Target(new[] { "Spoils in 1 day", "6<sprite=123>" }, 0));
    }
}
