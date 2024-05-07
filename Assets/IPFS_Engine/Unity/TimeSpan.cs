using System;
using System.Text;

namespace Ipfs.Unity
{
    public static class TimeSpanExtension
    {
        /// <summary>
        ///   Returns a short string representation of the duration in seconds to days
        /// </summary>
        /// <param name="t">The duration</param>
        /// <returns>The duration, like "1m7s" or "1d" </returns>
        public static string ToShortString(this TimeSpan t)
        {
            StringBuilder sb = new();

            if (t < TimeSpan.Zero)
            {
                sb.Append("-");
                t = t.Negate();
            }

            if (t.Days > 0)
                sb.Append($"{t.Days}d");

            if (t.Hours > 0)
                sb.Append($"{t.Hours}h");

            if (t.Minutes > 0)
                sb.Append($"{t.Minutes}m");

            if (t.Seconds > 0)
                sb.Append($"{t.Seconds}s");

            return sb.ToString();
        }
    }
}