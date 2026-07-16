#nullable enable
using System;
using System.Globalization;

namespace UtilitiesCS
{
    /// <summary>
    /// Host-neutral formatter for the EfcViewer suggestion percentage column. Converts a consumed
    /// probability in <c>[0,1]</c> into a whole-number percent string (no decimal places), and yields
    /// an empty string when no probability is available. The value is never recomputed here.
    /// </summary>
    public static class PercentageFormatter
    {
        /// <summary>
        /// Formats a probability as a whole-number percent string.
        /// </summary>
        /// <param name="probability">
        /// The consumed probability in <c>[0,1]</c>, or <c>null</c> when the row carries no probability.
        /// </param>
        /// <returns>
        /// The rounded whole-number percent followed by <c>"%"</c> (for example <c>0.732 -> "73%"</c>),
        /// rounding at the midpoint away from zero; an empty string when <paramref name="probability"/> is null.
        /// </returns>
        public static string FormatPercent(double? probability)
        {
            if (probability == null)
            {
                return string.Empty;
            }

            long percent = (long)Math.Round(probability.Value * 100, MidpointRounding.AwayFromZero);
            return percent.ToString(CultureInfo.InvariantCulture) + "%";
        }
    }
}
