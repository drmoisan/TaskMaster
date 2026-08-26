#nullable enable
using System;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Single authority for the archive-relative stem contract shared by the breadcrumb router,
    /// the EFC filing boundary, the EFC data model, and the Outlook-to-filesystem folder
    /// converter (#614). A filing "stem" is a path RELATIVE to the configured Outlook archive
    /// root; a full Outlook path (a store path such as a mailbox root, or any rooted path) is
    /// never a valid stem, because every downstream consumer concatenates the stem onto an
    /// ancestor and a rooted value produces either a crash or a silently misfiled item.
    /// <para>
    /// Pure by construction: no filesystem, network, COM, logging, or environment access, and no
    /// per-call allocation of a regular expression. All comparisons are ordinal.
    /// </para>
    /// </summary>
    public static class ArchiveStemContract
    {
        private const char BackslashSeparator = '\\';
        private const char ForwardSeparator = '/';

        /// <summary>
        /// Reports whether <paramref name="value"/> is a full (rooted) path rather than an
        /// archive-relative stem.
        /// <para>
        /// True for three shapes: a UNC/Outlook store path leading with <c>\\</c>; a value
        /// leading with a single <c>\</c> or <c>/</c>; and a drive-rooted value whose second
        /// character is <c>:</c> (for example <c>C:\Users</c>).
        /// </para>
        /// <para>
        /// Rationale for including the drive-rooted shape (recorded decision, #614): a
        /// drive-rooted value can never be a valid archive-relative stem, no legitimate stem
        /// carries a volume separator in position 1, and rejecting it costs nothing while
        /// providing defence in depth against any future producer that leaks a filesystem path
        /// into the Outlook filing chain.
        /// </para>
        /// </summary>
        /// <param name="value">The candidate stem. Null and empty return false.</param>
        /// <returns>True when the value is a rooted path; otherwise false.</returns>
        public static bool IsFullOutlookPath(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            char first = value[0];
            if (first == BackslashSeparator || first == ForwardSeparator)
            {
                // Covers both the \\-rooted store form and the single-separator-leading form.
                return true;
            }

            return value.Length > 1 && value[1] == ':';
        }

        /// <summary>
        /// Enforces the archive-relative stem contract at a boundary, throwing when the value is
        /// null, empty, whitespace-only, or a full Outlook path.
        /// </summary>
        /// <param name="value">The candidate stem.</param>
        /// <param name="paramName">The name of the offending parameter or property, reported to
        /// the caller so the failure is diagnosable without the value.</param>
        /// <exception cref="ArgumentException">The value violates the contract. The message names
        /// the parameter and the violated rule and NEVER embeds the value itself, because the
        /// value can carry a mailbox address or a user-profile path (#602, #614 AC21).</exception>
        public static void RequireArchiveRelativeStem(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    paramName
                        + " must be a non-empty path relative to the Outlook archive root; it was null, empty, or whitespace.",
                    paramName
                );
            }

            if (IsFullOutlookPath(value))
            {
                throw new ArgumentException(
                    paramName
                        + " must be a path relative to the Outlook archive root, but a full (rooted) Outlook or filesystem path was supplied. The value is withheld from this message because it can contain a mailbox address or user-profile path.",
                    paramName
                );
            }
        }

        /// <summary>
        /// Converts a full Outlook path to its archive-relative stem when, and only when, it is
        /// at or under <paramref name="archiveRoot"/>.
        /// <para>
        /// The match is prefix-anchored, ordinal case-insensitive, and separator-terminated, so a
        /// sibling whose name merely extends the root (Archive2 against the root Archive) is NOT
        /// treated as under the root. On failure the method returns false and yields an empty
        /// stem; it never passes the input through, which is the defect class this contract
        /// exists to eliminate.
        /// </para>
        /// </summary>
        /// <param name="fullPath">The candidate full Outlook path.</param>
        /// <param name="archiveRoot">The configured archive root. Trailing separators are ignored.</param>
        /// <param name="stem">The archive-relative stem with no leading separator, or
        /// <see cref="string.Empty"/> when <paramref name="fullPath"/> equals the root exactly or
        /// when the method returns false.</param>
        /// <returns>True when the path is at or under the root; otherwise false.</returns>
        public static bool TryMakeArchiveRelative(
            string fullPath,
            string archiveRoot,
            out string stem
        )
        {
            stem = string.Empty;
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrWhiteSpace(archiveRoot))
            {
                return false;
            }

            string root = archiveRoot.TrimEnd(BackslashSeparator, ForwardSeparator);
            if (root.Length == 0)
            {
                return false;
            }

            if (string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (
                fullPath.Length <= root.Length
                || !fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            )
            {
                return false;
            }

            char boundary = fullPath[root.Length];
            if (boundary != BackslashSeparator && boundary != ForwardSeparator)
            {
                return false;
            }

            stem = fullPath.Substring(root.Length).TrimStart(BackslashSeparator, ForwardSeparator);
            return true;
        }
    }
}
