using System;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    /// <summary>
    /// COM-bound production implementation of <see cref="IOutlookReadinessGate"/> (Issue #207).
    /// Probes Outlook store readiness with a cheap, non-throwing default-folder access and
    /// discriminates the known transient "not-ready" COM HRESULTs so the readiness
    /// coordinator routes them to retry instead of failing the startup action.
    /// </summary>
    /// <remarks>
    /// This class touches <c>Microsoft.Office.Interop.Outlook</c> directly (it holds and reads a
    /// live <see cref="Application"/>) and has no injectable seam below the COM boundary; it is
    /// therefore COM/VSTO coverage-exempt by inspection per the repository's documented
    /// COM/VSTO exemption. The pure decision logic that consumes this gate lives in
    /// <c>HookReadinessCoordinator</c> and is unit-tested separately with a mock gate.
    /// Declared <c>public</c> (with <c>public const</c> HRESULT constants) because the
    /// constants are referenced cross-assembly from <c>TaskMaster.AppOlObjects.LoadInboxes</c>
    /// and <c>UtilitiesCS</c> does not grant <c>InternalsVisibleTo("TaskMaster")</c>.
    /// </remarks>
    public class OutlookReadinessGate : IOutlookReadinessGate
    {
        /// <summary>
        /// Transient "store not ready" HRESULT observed during Outlook cold start
        /// (<c>0xDAC40111</c>). Treated as not-ready (retry), not a permanent failure.
        /// </summary>
        public const uint TransientStoreNotReadyHResult = 0xDAC40111;

        /// <summary>
        /// Transient "operation failed because store not ready" HRESULT observed during
        /// Outlook cold start (<c>0x8E640111</c>). Treated as not-ready (retry).
        /// </summary>
        public const uint TransientOperationFailedHResult = 0x8E640111;

        /// <summary>
        /// Transient Outlook readiness HRESULT observed during startup hook polling
        /// (<c>0x90740111</c>). Treated as not-ready (retry).
        /// </summary>
        public const uint TransientStartupReadinessHResult = 0x90740111;

        private readonly Application _app;

        /// <summary>
        /// Creates a readiness gate over a live Outlook <see cref="Application"/>.
        /// </summary>
        /// <param name="app">The live Outlook application; must not be null.</param>
        public OutlookReadinessGate(Application app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        /// <summary>
        /// Cheap, non-throwing probe of Outlook store readiness. Returns <c>true</c> when the
        /// default store's default inbox folder is reachable; returns <c>false</c> (never
        /// throws) when the store is not yet ready, including when the probe raises a
        /// <see cref="COMException"/>. Returns a bool only — the folder reference is never
        /// retained, so no extra COM object lifetime is created.
        /// </summary>
        public bool IsReady()
        {
            try
            {
                return _app.Session?.DefaultStore?.GetDefaultFolder(OlDefaultFolders.olFolderInbox)
                    != null;
            }
            catch (COMException)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns <c>true</c> only for the known transient "not-ready" COM HRESULTs
        /// (<see cref="TransientStoreNotReadyHResult"/> / <see cref="TransientOperationFailedHResult"/> /
        /// <see cref="TransientStartupReadinessHResult"/>),
        /// which should be retried; returns <c>false</c> for any other <see cref="COMException"/>.
        /// </summary>
        public bool IsTransientError(COMException e)
        {
            if (e is null)
            {
                return false;
            }

            uint hresult = unchecked((uint)e.ErrorCode);
            return hresult == TransientStoreNotReadyHResult
                || hresult == TransientOperationFailedHResult
                || hresult == TransientStartupReadinessHResult;
        }
    }
}
