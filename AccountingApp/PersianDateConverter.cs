using System;
using System.Globalization;

namespace AccountingApp
{
    public static class PersianDateConverter
    {
        private static readonly PersianCalendar _persianCalendar = new PersianCalendar();

        // Extension methods
        public static string ToPersianDate(this DateTime date)
        {
            return $"{_persianCalendar.GetYear(date)}/{_persianCalendar.GetMonth(date):D2}/{_persianCalendar.GetDayOfMonth(date):D2}";
        }

        public static string ToPersianDateTime(this DateTime date)
        {
            return $"{_persianCalendar.GetYear(date)}/{_persianCalendar.GetMonth(date):D2}/{_persianCalendar.GetDayOfMonth(date):D2} {date.Hour:D2}:{date.Minute:D2}";
        }

        public static string ToPersianDateShort(this DateTime date)
        {
            return $"{_persianCalendar.GetYear(date)}/{_persianCalendar.GetMonth(date):D2}/{_persianCalendar.GetDayOfMonth(date):D2}";
        }

        // Static methods for direct use
        public static string ConvertToPersianDate(DateTime date)
        {
            return $"{_persianCalendar.GetYear(date)}/{_persianCalendar.GetMonth(date):D2}/{_persianCalendar.GetDayOfMonth(date):D2}";
        }

        public static string ConvertToPersianDateTime(DateTime date)
        {
            return $"{_persianCalendar.GetYear(date)}/{_persianCalendar.GetMonth(date):D2}/{_persianCalendar.GetDayOfMonth(date):D2} {date.Hour:D2}:{date.Minute:D2}";
        }

        public static DateTime FromPersianDate(string persianDate)
        {
            var parts = persianDate.Split('/');
            if (parts.Length == 3)
            {
                var year = int.Parse(parts[0]);
                var month = int.Parse(parts[1]);
                var day = int.Parse(parts[2]);
                return _persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
            }
            return DateTime.Now;
        }

        public static string GetPersianMonthName(int month)
        {
            var monthNames = new[]
            {
                "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
                "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
            };
            return monthNames[month - 1];
        }

        public static string GetPersianDayName(DayOfWeek dayOfWeek)
        {
            var dayNames = new[]
            {
                "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه"
            };
            return dayNames[(int)dayOfWeek];
        }
    }
} 