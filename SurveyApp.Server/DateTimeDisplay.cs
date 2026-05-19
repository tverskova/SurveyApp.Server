namespace SurveyApp.Server
{
    public static class DateTimeDisplay
    {
        private static readonly TimeZoneInfo MoscowTimeZone = ResolveMoscowTimeZone();

        public static string Date(DateTime? value)
        {
            return value.HasValue ? ToMoscow(value.Value).ToString("dd.MM.yyyy") : "Не указано";
        }

        public static string DateTime(DateTime? value)
        {
            return value.HasValue ? ToMoscow(value.Value).ToString("dd.MM.yyyy HH:mm") : "Не указано";
        }

        public static System.DateTime ToMoscow(System.DateTime value)
        {
            if (value.Kind == DateTimeKind.Local)
                return value;

            var utc = value.Kind == DateTimeKind.Utc
                ? value
                : System.DateTime.SpecifyKind(value, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, MoscowTimeZone);
        }

        private static TimeZoneInfo ResolveMoscowTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            }
        }
    }
}
