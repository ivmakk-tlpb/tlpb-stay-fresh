using System.Globalization;

namespace StayFresh
{
    // Sums the time that a patch takes and gives one line with the mean time for each frame at the end of each period.
    public sealed class FrameTimer
    {
        private readonly string name;
        private readonly float period;
        private double milliseconds;
        private int frames;
        private int lastFrame = -1;
        private float periodStart = -1f;

        public FrameTimer(string name, float period)
        {
            this.name = name;
            this.period = period;
        }

        // Null until the period ends.
        public string Add(double elapsedMilliseconds, int frame, float now)
        {
            milliseconds += elapsedMilliseconds;
            if (frame != lastFrame)
            {
                frames++;
                lastFrame = frame;
            }
            if (periodStart < 0f) periodStart = now;
            if (now - periodStart < period) return null;
            var line = string.Format(CultureInfo.InvariantCulture, "{0}: {1:0.000} ms per frame over {2} frames", name, milliseconds / frames, frames);
            milliseconds = 0;
            frames = 0;
            lastFrame = -1;
            periodStart = now;
            return line;
        }
    }
}
