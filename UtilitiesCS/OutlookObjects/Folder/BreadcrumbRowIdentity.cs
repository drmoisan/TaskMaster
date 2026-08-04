#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using UtilitiesCS;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>Creates stable, source-qualified identities for breadcrumb row occurrences.</summary>
    internal static class BreadcrumbRowIdentity
    {
        internal static string ForFolderRow(FolderRow row, int occurrence)
        {
            string outputValue = row.Score.HasValue ? row.Score.Value.FolderPath : row.Text;
            return Compose(SourceName(row.Kind), occurrence, outputValue);
        }

        internal static string ForPlainRow(string outputValue, int occurrence)
        {
            return Compose("plain", occurrence, outputValue);
        }

        internal static string Disambiguate(
            string proposedIdentity,
            IReadOnlyList<BreadcrumbStateRow> existingRows
        )
        {
            if (string.IsNullOrWhiteSpace(proposedIdentity))
            {
                throw new ArgumentException(
                    "A non-empty stable identity is required.",
                    nameof(proposedIdentity)
                );
            }

            string candidate = proposedIdentity;
            int duplicateOrdinal = 2;
            while (Contains(existingRows, candidate))
            {
                candidate =
                    proposedIdentity
                    + "~"
                    + duplicateOrdinal.ToString(CultureInfo.InvariantCulture);
                duplicateOrdinal++;
            }
            return candidate;
        }

        internal static void RequireUnique(IReadOnlyList<BreadcrumbStateRow> rows)
        {
            var identities = new HashSet<string>(StringComparer.Ordinal);
            foreach (BreadcrumbStateRow row in rows)
            {
                if (row == null || !identities.Add(row.Identity))
                {
                    throw new ArgumentException(
                        "A replacement snapshot must contain non-null rows with unique identities.",
                        nameof(rows)
                    );
                }
            }
        }

        private static string Compose(string source, int occurrence, string outputValue)
        {
            if (occurrence < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(occurrence), occurrence, null);
            }
            if (outputValue == null)
            {
                throw new ArgumentNullException(nameof(outputValue));
            }

            return source
                + ":"
                + occurrence.ToString(CultureInfo.InvariantCulture)
                + ":"
                + outputValue;
        }

        private static string SourceName(FolderRowKind kind)
        {
            switch (kind)
            {
                case FolderRowKind.Separator:
                    return "separator";
                case FolderRowKind.SearchResult:
                    return "search";
                case FolderRowKind.Suggestion:
                    return "suggestion";
                case FolderRowKind.Recent:
                    return "recent";
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static bool Contains(IReadOnlyList<BreadcrumbStateRow> rows, string identity)
        {
            for (int index = 0; index < rows.Count; index++)
            {
                if (string.Equals(rows[index].Identity, identity, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
