using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace TaskMaster
{
    /// <summary>
    /// Partial of <see cref="AppOlObjects"/> holding the archive-root resolution seam introduced by
    /// issue #736 finding 1. The <c>ArchiveRootPath</c> getter in <c>AppOlObjects.cs</c> previously
    /// evaluated two live Outlook COM reads while composing the arguments it handed to
    /// <c>ArchiveRootPathGuard.RequireResolvedArchiveRoot</c>, and C# evaluates arguments before
    /// entering the callee, so a transient COM failure escaped a member whose documented contract
    /// admits only <see cref="InvalidOperationException"/>.
    /// <para>
    /// The core below is delegate-driven and free of Outlook COM types, so it is unit-testable
    /// without a live Outlook process. The shape follows <c>ResolveCurrentUserEmailAddress</c> and
    /// <c>TryGetSmtpAddress</c>, already present in this class: a thin COM-touching wrapper plus a
    /// static core that carries the decision logic.
    /// </para>
    /// </summary>
    public partial class AppOlObjects
    {
        /// <summary>
        /// Resolves the validated archive-root path from two supplied read delegates and forwards
        /// the resolved pair to the frozen guard <c>ArchiveRootPathGuard.RequireResolvedArchiveRoot</c>.
        /// </summary>
        /// <param name="readComposedArchiveRootPath">Reads the archive root path composed from the
        /// default store root. Evaluated first, matching the guard's parameter order.</param>
        /// <param name="readResolvedArchiveFolderPath">Reads the full path of the folder that
        /// resolved for the archive root, or null when no folder resolved.</param>
        /// <param name="logDiagnostic">Sink for the redacted diagnostic, invoked before any throw
        /// so the failure is recorded even when a caller absorbs the exception.</param>
        /// <returns>The validated archive root path.</returns>
        /// <exception cref="InvalidOperationException">The archive root is unresolvable, lies
        /// outside the composed path, or could not be read from Outlook at all. The diagnostic
        /// names the rule only; the path is withheld because it carries a mailbox address (#602).
        /// A transient <see cref="COMException"/> raised by either read is normalized to this type
        /// and preserved as the inner exception.</exception>
        internal static string ResolveValidatedArchiveRootPath(
            Func<string> readComposedArchiveRootPath,
            Func<string> readResolvedArchiveFolderPath,
            Action<string> logDiagnostic
        )
        {
            string composedArchiveRootPath;
            string resolvedArchiveFolderPath;

            try
            {
                composedArchiveRootPath = readComposedArchiveRootPath();
                resolvedArchiveFolderPath = readResolvedArchiveFolderPath();
            }
            catch (COMException comFailure)
            {
                // The getter's documented contract admits only InvalidOperationException, so a
                // transient Outlook failure is normalized here rather than left to escape a
                // member no consumer in the repository is written to handle. The diagnostic is
                // emitted before the throw, matching the frozen guard's own ordering.
                logDiagnostic?.Invoke(ArchiveRootPathGuard.UnresolvableRule);
                throw new InvalidOperationException(
                    ArchiveRootPathGuard.UnresolvableRule,
                    comFailure
                );
            }

            return ArchiveRootPathGuard.RequireResolvedArchiveRoot(
                composedArchiveRootPath,
                resolvedArchiveFolderPath,
                logDiagnostic
            );
        }

        /// <summary>
        /// Thin COM-touching wrapper that supplies the two live Outlook reads and the logger sink
        /// to the delegate-driven core of the same name.
        /// </summary>
        /// <remarks>
        /// Excluded from coverage by inspection: every expression in this member is either an
        /// Outlook COM crossing or a delegate literal wrapping one, so the member cannot execute
        /// without a live Outlook process. The decision logic it delegates to is covered directly
        /// by the delegate-driven core. This mirrors the exclusion already carried by
        /// <c>ResolveInboxForStore</c> in <c>AppOlObjects.StoreRehook.cs</c>.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        internal string ResolveValidatedArchiveRootPath()
        {
            return ResolveValidatedArchiveRootPath(
                () => Path.Combine(Root.FolderPath, "Archive"),
                () => ArchiveRoot?.FolderPath,
                message => logger.Error(message)
            );
        }
    }
}
