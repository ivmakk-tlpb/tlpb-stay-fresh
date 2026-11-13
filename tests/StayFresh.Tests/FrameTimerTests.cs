using StayFresh;
using Xunit;

public class FrameTimerTests
{
    [Fact]
    public void It_gives_no_line_before_the_period_ends()
    {
        var timer = new FrameTimer("spoilage prefix", 10f);

        Assert.Null(timer.Add(0.2, frame: 1, now: 0f));
        Assert.Null(timer.Add(0.2, frame: 2, now: 9.9f));
    }

    [Fact]
    public void At_the_end_of_the_period_it_gives_the_mean_time_per_frame_and_starts_again()
    {
        var timer = new FrameTimer("spoilage prefix", 10f);
        timer.Add(0.1, frame: 1, now: 0f);
        timer.Add(0.1, frame: 1, now: 0f);
        timer.Add(0.2, frame: 2, now: 5f);

        var line = timer.Add(0.2, frame: 3, now: 10f);

        Assert.Equal("spoilage prefix: 0.200 ms per frame over 3 frames", line);
        Assert.Null(timer.Add(0.1, frame: 4, now: 11f));
    }
}
