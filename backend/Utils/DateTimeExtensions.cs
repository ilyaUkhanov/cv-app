namespace backend.Utils
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Creates a DateTime with UTC kind for the specified date
        /// </summary>
        public static DateTime ToUtc(this DateTime date)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        /// <summary>
        /// Creates a DateTime with UTC kind for the specified date and time
        /// </summary>
        public static DateTime ToUtc(this DateTime date, int hour, int minute, int second = 0)
        {
            return DateTime.SpecifyKind(new DateTime(date.Year, date.Month, date.Day, hour, minute, second), DateTimeKind.Utc);
        }

        /// <summary>
        /// Creates a UTC DateTime for the specified date (at midnight UTC)
        /// </summary>
        public static DateTime UtcDate(int year, int month, int day)
        {
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Creates a UTC DateTime for the specified date and time
        /// </summary>
        public static DateTime UtcDateTime(int year, int month, int day, int hour, int minute, int second = 0)
        {
            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }
    }
}
