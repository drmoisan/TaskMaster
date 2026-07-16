using System;

namespace UtilitiesCS
{
    /// <summary>
    /// Pure, host-neutral formatter that renders a prediction probability (a <c>double</c> fraction
    /// in <c>[0,1]</c>, sourced verbatim from <see cref="FolderScore.Probability"/>) as a
    /// whole-number percentage string such as <c>"43%"</c>. Out-of-range input is clamped to
    /// <c>[0,1]</c> and midpoint values round away from zero. This seam carries the
    /// percentage-formatting rule for the QuickFiler folder dropdown and is NOT coverage-exempt.
    /// </summary>
    public static class PercentageFormatter
    {
        /// <summary>
        /// Formats a probability in <c>[0,1]</c> as a whole-number percentage string (for example
        /// <c>0.4267 =&gt; "43%"</c>, <c>1.0 =&gt; "100%"</c>, <c>0.0 =&gt; "0%"</c>). Input outside
        /// <c>[0,1]</c> is clamped before formatting; the scaled value is rounded to the nearest
        /// integer with midpoint rounding away from zero.
        /// </summary>
        /// <param name="probability">A relative-confidence fraction; clamped to <c>[0,1]</c>.</param>
        /// <returns>The whole-number percentage followed by a percent sign.</returns>
        public static string Format(double probability)
        {
            // Clamp to [0,1]. Math.Clamp is unavailable on this net48 target, so the bounds are
            // applied explicitly with the identical semantics.
            double clamped = probability < 0.0 ? 0.0 : (probability > 1.0 ? 1.0 : probability);
            int percent = (int)Math.Round(clamped * 100.0, MidpointRounding.AwayFromZero);
            return percent + "%";
        }
    }
}
