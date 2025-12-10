using System;
using System.Collections.Generic;
using System.Globalization;

namespace ECommerce.Application.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly CultureInfo TrCulture = new("tr-TR");
        private const string TurkeyTimeZoneId = "Turkey Standard Time"; // Windows için
        // Linux/macOS için: "Europe/Istanbul"

        // Şu an UTC
        public static DateTime UtcNow() => DateTime.UtcNow;

        // Türkiye saati
        public static DateTime NowTurkey()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(TurkeyTimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        // UTC -> TimeZone
        public static DateTime ConvertUtcToTz(DateTime utc, string timeZoneId)
        {
            if (utc.Kind != DateTimeKind.Utc) utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
        }

        // Local -> UTC
        public static DateTime ConvertLocalToUtc(DateTime local, string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            if (local.Kind != DateTimeKind.Unspecified)
                local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(local, tz);
        }

        // ISO8601 format
        public static string ToIso8601Utc(DateTime dtUtc) =>
            dtUtc.Kind == DateTimeKind.Utc
                ? dtUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                : DateTime.SpecifyKind(dtUtc, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        // Türkçe formatlar
        public static string ToTurkishDate(DateTime dt) => dt.ToString("dd.MM.yyyy", TrCulture);
        public static string ToTurkishDateTime(DateTime dt) => dt.ToString("dd.MM.yyyy HH:mm", TrCulture);

        // Gün/hafta/ay sınırları
        public static DateTime StartOfDay(DateTime dt) => new(dt.Year, dt.Month, dt.Day, 0, 0, 0, dt.Kind);
        public static DateTime EndOfDay(DateTime dt) => new(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999, dt.Kind);
        public static DateTime StartOfWeek(DateTime dt, DayOfWeek start = DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - start)) % 7;
            return dt.Date.AddDays(-diff);
        }
        public static DateTime StartOfMonth(DateTime dt) => new(dt.Year, dt.Month, 1);
        public static DateTime EndOfMonth(DateTime dt) => new(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month), 23, 59, 59, 999);

        // Yaş hesaplama
        public static int AgeInYears(DateTime birthDate, DateTime? reference = null)
        {
            var today = (reference ?? DateTime.Today).Date;
            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        public static int AgeInMonths(DateTime birthDate, DateTime? reference = null)
        {
            var refDate = (reference ?? DateTime.Today).Date;
            int months = (refDate.Year - birthDate.Year) * 12 + (refDate.Month - birthDate.Month);
            if (refDate.Day < birthDate.Day) months--;
            return Math.Max(months, 0);
        }

        // İş günü kontrolü
        public static bool IsBusinessDay(DateTime dt, HashSet<DateTime>? holidays = null)
        {
            bool weekend = dt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            bool holiday = holidays?.Contains(dt.Date) ?? false;
            return !weekend && !holiday;
        }

        public static bool IsWithinBusinessHours(DateTime dtLocal, int startHour = 9, int endHour = 18)
        {
            int h = dtLocal.Hour;
            return h >= startHour && h < endHour;
        }

        public static DateTime AddBusinessDays(DateTime start, int days, HashSet<DateTime>? holidays = null)
        {
            int added = 0;
            var current = start;
            int step = days >= 0 ? 1 : -1;
            while (added != days)
            {
                current = current.AddDays(step);
                if (IsBusinessDay(current, holidays))
                    added += step;
            }
            return current;
        }

        // Parse işlemleri
        public static bool TryParseTurkish(string input, out DateTime result)
        {
            string[] formats =
            {
                "dd.MM.yyyy",
                "dd.MM.yyyy HH:mm",
                "dd.MM.yyyy HH:mm:ss",
                "yyyy-MM-dd",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss.fffZ"
            };
            return DateTime.TryParseExact(input, formats, TrCulture,
                DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out result);
        }

        public static bool TryParseIso8601Utc(string input, out DateTime result)
        {
            if (DateTime.TryParse(input, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt))
            {
                result = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                return true;
            }
            result = default;
            return false;
        }

        // Yuvarlama
        public static DateTime RoundToMinute(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind)
                .AddSeconds(dt.Second >= 30 ? 60 : 0);
        }

        public static DateTime FloorTo5Minutes(DateTime dt)
        {
            int minutes = (dt.Minute / 5) * 5;
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, minutes, 0, dt.Kind);
        }

        public static DateTime CeilingToHour(DateTime dt)
        {
            if (dt.Minute == 0 && dt.Second == 0 && dt.Millisecond == 0) return dt;
            var baseHour = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, dt.Kind);
            return baseHour.AddHours(1);
        }

        public static bool IsWithinWindow(DateTime dt, DateTime windowStart, DateTime windowEnd)
            => dt >= windowStart && dt <= windowEnd;
    }
}
