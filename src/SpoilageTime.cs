namespace StayFresh
{
    // The in-game time left before an item spoils, at its spoilage rate of the moment. The game takes the condition loss
    // as decayRate x (1 - preserved x 0.9) x decay factor for each scaled second, and one scaled second is 10 in-game
    // seconds.
    public static class SpoilageTime
    {
        public const float InGameSecondsPerScaledSecond = 10f;
        private const float MaxSaltSlowdown = 0.9f;
        private const float ColdStart = 5f;
        private const float ColdMinimum = 0.2f;

        // The unit words of the game, in the language of the game.
        public readonly struct Words
        {
            public readonly string Week, Weeks, Day, Days, Hour, Hours;

            public Words(string week, string weeks, string day, string days, string hour, string hours)
            {
                Week = week;
                Weeks = weeks;
                Day = day;
                Days = days;
                Hour = hour;
                Hours = hours;
            }
        }

        // Null when the stack does not spoil at the moment.
        public static float? SecondsLeft(float condition, float decayRate, float preserved, float gameFactor, float modFactor)
        {
            var rate = decayRate * (1f - preserved * MaxSaltSlowdown) * gameFactor * modFactor;
            if (rate <= 0f || condition <= 0f) return null;
            return condition / rate * InGameSecondsPerScaledSecond;
        }

        // The game slows spoilage below 5 degrees, down to 0.2 of the speed at 0 degrees and below.
        public static float ColdFactor(float temperature)
        {
            if (temperature >= ColdStart) return 1f;
            var warmth = System.Math.Clamp(1f - (ColdStart - temperature) / ColdStart, 0f, 1f);
            return warmth * (1f - ColdMinimum) + ColdMinimum;
        }

        // The largest two units of weeks, days, and hours, such as "1 day 6 hours", and "<1 hour" below one hour.
        public static string Format(float seconds, Words words)
        {
            var totalHours = (long)(seconds / 3600f);
            if (totalHours < 1) return "<1 " + words.Hour;
            long weeks = totalHours / 168, days = totalHours % 168 / 24, hours = totalHours % 24;
            if (weeks > 0) return Join(Unit(weeks, words.Week, words.Weeks), days, words.Day, words.Days);
            if (days > 0) return Join(Unit(days, words.Day, words.Days), hours, words.Hour, words.Hours);
            return Unit(hours, words.Hour, words.Hours);
        }

        public static string Text(string time) => "Spoils in " + time;

        private static string Unit(long count, string one, string many) => count + " " + (count == 1 ? one : many);

        private static string Join(string first, long count, string one, string many) =>
            count > 0 ? first + " " + Unit(count, one, many) : first;
    }
}
