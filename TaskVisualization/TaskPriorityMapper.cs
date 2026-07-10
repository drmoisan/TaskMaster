using Microsoft.Office.Interop.Outlook;

namespace TaskVisualization
{
    /// <summary>
    /// Host-neutral mapping between <see cref="OlImportance"/> and its display string
    /// ("High" / "Low" / "Normal"), extracted from the mapping formerly inlined in the
    /// controller's <c>Initialize</c> and <c>Assign_Priority</c> methods.
    /// </summary>
    public static class TaskPriorityMapper
    {
        /// <summary>High-priority display string.</summary>
        public const string High = "High";

        /// <summary>Low-priority display string.</summary>
        public const string Low = "Low";

        /// <summary>Normal-priority display string.</summary>
        public const string Normal = "Normal";

        /// <summary>
        /// Maps an <see cref="OlImportance"/> to its display string. Any value other than
        /// High or Low maps to <see cref="Normal"/>, matching the former inline mapping.
        /// </summary>
        public static string ToDisplayString(OlImportance importance)
        {
            switch (importance)
            {
                case OlImportance.olImportanceHigh:
                    return High;
                case OlImportance.olImportanceLow:
                    return Low;
                default:
                    return Normal;
            }
        }

        /// <summary>
        /// Maps a display string to its <see cref="OlImportance"/>. "High" maps to
        /// <see cref="OlImportance.olImportanceHigh"/>, "Low" to
        /// <see cref="OlImportance.olImportanceLow"/>, and any other value (unknown input)
        /// falls back to <see cref="OlImportance.olImportanceNormal"/>, matching the former
        /// inline mapping in <c>Assign_Priority</c>.
        /// </summary>
        public static OlImportance FromDisplayString(string display)
        {
            if (display == High)
            {
                return OlImportance.olImportanceHigh;
            }

            if (display == Low)
            {
                return OlImportance.olImportanceLow;
            }

            return OlImportance.olImportanceNormal;
        }
    }
}
