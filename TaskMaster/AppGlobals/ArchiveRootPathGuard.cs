using System;

namespace TaskMaster
{
    /// <summary>
    /// Pure decision logic for #614 defect D6: whether a composed Outlook archive root path is
    /// actually backed by the folder that resolves for it. The logic is deliberately free of
    /// Outlook COM types so it can be unit-tested without a live Outlook process; the caller
    /// supplies the two already-resolved strings and a diagnostic sink.
    /// </summary>
    internal static class ArchiveRootPathGuard
    {
        internal const string UnresolvableRule =
            "The Outlook archive root folder could not be resolved in the default store. The path is withheld from this message because it contains a mailbox address.";

        internal const string CrossStoreRule =
            "The Outlook archive root resolved to a folder outside the composed archive root path, which indicates a cross-store or renamed archive. The paths are withheld from this message because they contain a mailbox address.";

        /// <summary>
        /// Returns <paramref name="composedArchiveRootPath"/> when the archive root it names is
        /// the folder that actually resolved; otherwise logs a redacted diagnostic and throws.
        /// </summary>
        /// <param name="composedArchiveRootPath">The archive root path composed from the default
        /// store root. Previously returned unverified, which is the D6 defect.</param>
        /// <param name="resolvedArchiveFolderPath">The full path of the folder that resolved for
        /// the archive root, or null when no folder resolved.</param>
        /// <param name="logDiagnostic">Sink for the redacted diagnostic, invoked before the
        /// throw so the failure is recorded even when a caller swallows the exception.</param>
        /// <returns>The validated archive root path.</returns>
        /// <exception cref="InvalidOperationException">The archive root is unresolvable, or the
        /// resolved folder lies outside the composed path.</exception>
        internal static string RequireResolvedArchiveRoot(
            string composedArchiveRootPath,
            string resolvedArchiveFolderPath,
            Action<string> logDiagnostic
        )
        {
            if (
                string.IsNullOrWhiteSpace(composedArchiveRootPath)
                || string.IsNullOrWhiteSpace(resolvedArchiveFolderPath)
            )
            {
                logDiagnostic?.Invoke(UnresolvableRule);
                throw new InvalidOperationException(UnresolvableRule);
            }

            if (
                !string.Equals(
                    composedArchiveRootPath,
                    resolvedArchiveFolderPath,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                logDiagnostic?.Invoke(CrossStoreRule);
                throw new InvalidOperationException(CrossStoreRule);
            }

            return composedArchiveRootPath;
        }
    }
}
