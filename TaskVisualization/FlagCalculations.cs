using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;
using UtilitiesCS;

namespace TaskVisualization
{
    /// <summary>
    /// Host-neutral flag-selection calculations extracted from
    /// <see cref="FlagTasks"/>. These pure statics contain no WinForms or Outlook
    /// Interop dependency; the multi-selection dialog is supplied as an injected
    /// delegate so the logic is fully unit-testable.
    /// </summary>
    public static class FlagCalculations
    {
        /// <summary>
        /// Determines which flags to set. A single selected item sets
        /// <see cref="Enums.FlagsToSet.All"/>; multiple items delegate the choice to
        /// <paramref name="flagSelector"/> (the flag-selection dialog seam) and
        /// convert the returned strings to the enum.
        /// </summary>
        public static Enums.FlagsToSet GetFlagsToSet(
            int selectionCount,
            Func<SortedDictionary<string, bool>, List<string>> flagSelector
        )
        {
            // If more than one item selected, ask user which flags to set
            if (selectionCount > 1)
            {
                var symbolSelectionDict = GetSymbolsDictionary();
                var flagStrings = flagSelector(symbolSelectionDict);
                return ConvertFlagStringsToEnum(flagStrings);
            }
            // Else set them All
            else
            {
                return Enums.FlagsToSet.All;
            }
        }

        /// <summary>
        /// Converts a list of flag-name strings to a bit-or'd
        /// <see cref="Enums.FlagsToSet"/>. An empty list returns
        /// <see cref="Enums.FlagsToSet.All"/>; unparseable strings are ignored.
        /// </summary>
        public static Enums.FlagsToSet ConvertFlagStringsToEnum(List<string> flagStrings)
        {
            if (flagStrings.Count == 0)
            {
                return Enums.FlagsToSet.All;
            }
            else
            {
                Enums.FlagsToSet flag;
                var flagsList = (
                    from x in flagStrings
                    where Enum.TryParse(x, out flag)
                    select Enum.Parse(typeof(Enums.FlagsToSet), x)
                )
                    .ToList()
                    .OfType<Enums.FlagsToSet>();

                Enums.FlagsToSet selectedFlags = (Enums.FlagsToSet)
                    Conversions.ToInteger(GenericBitwiseStatic<Enums.FlagsToSet>.Or(flagsList));
                return selectedFlags;
            }
        }

        /// <summary>
        /// Builds the sorted symbol-selection dictionary for the flag dialog,
        /// excluding the <see cref="Enums.FlagsToSet.All"/> and
        /// <see cref="Enums.FlagsToSet.None"/> members. All values start unselected.
        /// </summary>
        public static SortedDictionary<string, bool> GetSymbolsDictionary()
        {
            Enums.FlagsToSet[] excludedMembers = new[]
            {
                Enums.FlagsToSet.All,
                Enums.FlagsToSet.None,
            };
            var symbolsDict = Enum.GetValues(typeof(Enums.FlagsToSet))
                .Cast<Enums.FlagsToSet>()
                .ToList()
                .AsEnumerable()
                .Where(x => excludedMembers.Contains(x) == false)
                .Select(x => x)
                .ToDictionary(x => Enum.GetName(typeof(Enums.FlagsToSet), x), x => x);

            var symbolSelectionDict = (from x in symbolsDict select x.Key)
                .ToDictionary(x => x, x => false)
                .ToSortedDictionary();
            return symbolSelectionDict;
        }
    }
}
