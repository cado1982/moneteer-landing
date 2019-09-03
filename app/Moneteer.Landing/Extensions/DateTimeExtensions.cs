using System;

namespace Moneteer.Landing.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime EndOfDay(this DateTime input)
        {
            return new DateTime(input.Year, input.Month, input.Day, 23, 59, 59, 999);
        }
    }
}