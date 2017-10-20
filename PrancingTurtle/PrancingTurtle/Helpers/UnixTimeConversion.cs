using System;

namespace PrancingTurtle.Helpers
{
    public static class UnixTimeConversion
    {
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }
    }
}