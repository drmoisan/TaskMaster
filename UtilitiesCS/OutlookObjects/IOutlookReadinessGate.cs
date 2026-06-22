using System.Runtime.InteropServices;

namespace UtilitiesCS
{
    /// <summary>
    /// Testable boundary for Outlook store readiness during cold start (Issue #207).
    /// Implementations provide a cheap, non-throwing probe of whether the default store
    /// is reachable, plus discrimination of the known transient "not-ready" COM HRESULTs
    /// so the readiness coordinator can route them to retry instead of failing the startup
    /// action. The non-throwing-probe contract follows the precedent of
    /// <c>AppOlObjects.ResolveCurrentUserEmailAddress</c>, which wraps the COM access in a
    /// <c>try { ... } catch (COMException) { ... }</c> and never lets the exception escape.
    /// </summary>
    /// <remarks>
    /// Declared <c>public</c> because the consuming <c>HookReadinessCoordinator</c> and
    /// <c>AppEvents</c> live in the separate <c>TaskMaster</c> assembly and <c>UtilitiesCS</c>
    /// does not grant <c>InternalsVisibleTo("TaskMaster")</c>. This matches the repo
    /// convention for UtilitiesCS types consumed by TaskMaster (e.g. <c>StoresWrapper</c>,
    /// <c>IdleAsyncQueue</c>, <c>UiThread</c>).
    /// </remarks>
    public interface IOutlookReadinessGate
    {
        /// <summary>
        /// Cheap, non-throwing probe of Outlook store readiness. Returns <c>true</c> when the
        /// default store and its default inbox folder are reachable; returns <c>false</c>
        /// (never throws) when the store is not yet ready, including when the probe raises a
        /// <see cref="COMException"/>.
        /// </summary>
        bool IsReady();

        /// <summary>
        /// Returns <c>true</c> only for the known transient "not-ready" COM HRESULTs that
        /// should be treated as not-ready (retry) rather than as a permanent failure; returns
        /// <c>false</c> for any other <see cref="COMException"/>.
        /// </summary>
        bool IsTransientError(COMException e);
    }
}
