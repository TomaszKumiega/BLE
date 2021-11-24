using System;

namespace ERGBLE.Services.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime Trim(this DateTime dateTime, long roundTicks)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % roundTicks), dateTime.Kind);
        }
    }
}
