#nullable enable
using System;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// The predicates deciding what an EFC selection may be used for (#614 D9). The OK/filing
    /// action and the folder-creation path each delegate to the predicate scoped to their own
    /// rule set, so neither path can silently inherit the other's strictness, and a value can
    /// never be accepted by one route into the same operation and rejected by another.
    /// </summary>
    internal static class EfcSelectionGuard
    {
        /// <summary>Prefix of the non-selectable suggestion banner rows.</summary>
        private const string BannerPrefix = "===";

        /// <summary>
        /// Shortest name the folder-creation path accepts. This is a creation rule only: it
        /// guards against a new archive folder being created under an accidental one- or
        /// two-keystroke name, and it has never applied to filing into an existing folder.
        /// </summary>
        private const int MinimumCreationLength = 3;

        /// <summary>
        /// Diagnostic recorded when the configured archive root cannot be resolved. The text is
        /// fixed and names no mailbox address, host name, account, or path, so it is safe to log
        /// verbatim (#602 redaction requirement).
        /// </summary>
        internal const string RootUnavailableDiagnostic =
            "The Outlook archive root is unavailable; rooted filing selections are rejected until it resolves. The underlying failure was logged when the root was resolved.";

        /// <summary>
        /// Reports whether <paramref name="selection"/> is a usable filing destination.
        /// <para>
        /// Rejects null, empty, and whitespace-only values and banner sentinels. A value that is
        /// not a full (rooted) path is an archive-relative stem and is accepted.
        /// </para>
        /// <para>
        /// The filing predicate carries NO minimum-length rule (CR-1). Filing into an existing
        /// archive folder whose name is one or two characters long is ordinary, and the length
        /// rule lives only in <see cref="IsValidCreationSelection"/>, where a new folder is being
        /// named rather than an existing one selected.
        /// </para>
        /// <para>
        /// A rooted value is rejected only when it is NOT resolvable against
        /// <paramref name="archiveRoot"/> (CR-2). This mirrors the scope-pinning already applied
        /// to the breadcrumb router's row selection, which passes an at-or-under-root rooted
        /// target through verbatim; without the mirror a value the router admits would be
        /// selectable but unfilable. The resolution test is prefix-anchored,
        /// separator-terminated, and ordinal case-insensitive, so a store root, a cross-store
        /// path, an above-root path, and a sibling that merely extends the root name all still
        /// fail — the D1/D4/D9 protection is unchanged. A null, empty, or whitespace-only root,
        /// which is also what the degrade path yields when the root cannot be resolved, rejects
        /// every rooted value.
        /// </para>
        /// </summary>
        /// <param name="selection">The candidate selection, typically a folder row's text.</param>
        /// <param name="archiveRoot">The resolved archive root, or null/empty when it could not
        /// be resolved.</param>
        /// <returns>True when the value may be filed to; otherwise false.</returns>
        internal static bool IsValidFilingSelection(string? selection, string? archiveRoot)
        {
            if (string.IsNullOrWhiteSpace(selection))
            {
                return false;
            }

            string value = selection!;
            if (value.StartsWith(BannerPrefix, System.StringComparison.Ordinal))
            {
                return false;
            }

            if (!ArchiveStemContract.IsFullOutlookPath(value))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(archiveRoot)
                && ArchiveStemContract.TryMakeArchiveRelative(value, archiveRoot!, out _);
        }

        /// <summary>
        /// Reports whether <paramref name="selection"/> may be used as the parent selection for
        /// creating a new archive folder.
        /// <para>
        /// Rejects null, empty, and whitespace-only values; banner sentinels; names shorter than
        /// the creation minimum; and any full (rooted) path, because the creation path
        /// concatenates the selection beneath the archive root and a rooted value is therefore
        /// never a valid creation stem. The minimum-length rule lives here and only here (CR-1);
        /// applying it to filing would make an existing two-character archive folder unfilable.
        /// </para>
        /// </summary>
        /// <param name="selection">The candidate selection, typically a folder row's text.</param>
        /// <returns>True when the value may be used for folder creation; otherwise false.</returns>
        internal static bool IsValidCreationSelection(string? selection)
        {
            if (string.IsNullOrWhiteSpace(selection))
            {
                return false;
            }

            string value = selection!;
            return value.Length >= MinimumCreationLength
                && !value.StartsWith(BannerPrefix, System.StringComparison.Ordinal)
                && !ArchiveStemContract.IsFullOutlookPath(value);
        }

        /// <summary>
        /// Reads the archive root through <paramref name="readArchiveRoot"/>, degrading to an
        /// empty root instead of propagating the one documented failure the accessor is
        /// contracted to signal when the root is unresolvable or resolves outside the default
        /// store.
        /// <para>
        /// The catch is deliberately narrow: only that one documented failure mode is degraded,
        /// and every other failure propagates so a genuine defect still fails fast. Degrading is
        /// required because the accessor is invoked on the OK-button path, where an unhandled
        /// failure would tear down the form; an empty root instead causes the filing predicate to
        /// reject every rooted selection while relative stems continue to file normally.
        /// </para>
        /// <para>
        /// The sink receives a fixed, value-free message: the underlying cause is already logged
        /// by the accessor before it signals, and repeating any part of the failing value here
        /// would risk leaking a host identifier (#602).
        /// </para>
        /// </summary>
        /// <param name="readArchiveRoot">Accessor for the configured archive root.</param>
        /// <param name="logDiagnostic">Sink for the fixed degrade diagnostic.</param>
        /// <returns>The resolved archive root, or <see cref="string.Empty"/> when it degraded.</returns>
        internal static string ResolveArchiveRootOrEmpty(
            Func<string> readArchiveRoot,
            Action<string> logDiagnostic
        )
        {
            try
            {
                return readArchiveRoot();
            }
            catch (InvalidOperationException)
            {
                logDiagnostic(RootUnavailableDiagnostic);
                return string.Empty;
            }
        }
    }
}
