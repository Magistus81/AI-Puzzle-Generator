namespace PuzzleGenerator.Services
{
    public static class TimeFormatter
    {
        public static string FormatTimerLabel(int totalSeconds)
        {
            return $"Time: {totalSeconds} sec";
        }

        public static string FormatTopTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            return $"{minutes} min {seconds} sec";
        }

        public static string FormatCompletionTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            return $"{minutes} minutes and {seconds} seconds";
        }
    }
}
