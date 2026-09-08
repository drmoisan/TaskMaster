using System;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using FluentAssertions;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test
{
    /// <summary>
    /// Snapshots every process-global static <c>UiThread</c> owns, resets them all through that
    /// type's internal <c>ResetForTesting</c> hook, and restores the captured values on disposal.
    /// </summary>
    /// <remarks>
    /// This type is deliberately <b>not</b> internally synchronized. It performs an unguarded
    /// read-then-write against process-global statics, so two tests entering a scope concurrently
    /// would interleave and one would restore values the other had already replaced. Serialization
    /// of writers is provided instead by <c>[DoNotParallelize]</c> on every consuming test class. A
    /// future caller must not assume this type is thread-safe: adding a new consuming test class
    /// requires adding that attribute to the class as well.
    ///
    /// Reflection is required because <c>InternalsVisibleTo</c> exposes internal members only; it
    /// does not expose private ones, and every controlled field is private. Centralising the
    /// reflection here means each field name appears in exactly one place in this assembly.
    /// </remarks>
#nullable enable annotations
    internal sealed class UiThreadStateScope : IDisposable
    {
        private static readonly FieldInfo LoadedInfo = Resolve("_loaded");
        private static readonly FieldInfo UiSyncContextInfo = Resolve("_uiSyncContext");
        private static readonly FieldInfo AutoScaleFactorInfo = Resolve("_autoScaleFactor");
        private static readonly FieldInfo UiThreadIdInfo = Resolve("_uiThreadId");
        private static readonly FieldInfo DispatcherInfo = Resolve("_dispatcher");
        private static readonly FieldInfo SyncContextFormInfo = Resolve("_syncContextForm");
        private static readonly FieldInfo ThreadMonitorInfo = Resolve("_threadMonitor");
        private static readonly FieldInfo MonitorUiThreadInfo = Resolve("_monitorUiThread");
        private static readonly FieldInfo OnLockupDetectedInfo = Resolve("_onLockupDetected");
        private static readonly FieldInfo MonitorTimeProviderInfo = Resolve("_monitorTimeProvider");
        private static readonly FieldInfo LockupThresholdInfo = Resolve(
            "_lockupAttributionThresholdMs"
        );

        private readonly object?[] _priorValues;
        private readonly Func<IUiCaptureSource> _priorFactory;
        private bool _disposed;

        private UiThreadStateScope(object?[] priorValues, Func<IUiCaptureSource> priorFactory)
        {
            _priorValues = priorValues;
            _priorFactory = priorFactory;
        }

        /// <summary>
        /// The field-info objects this scope controls, in the order their prior values are
        /// captured and restored.
        /// </summary>
        private static FieldInfo[] ControlledFields =>
            new[]
            {
                LoadedInfo,
                UiSyncContextInfo,
                AutoScaleFactorInfo,
                UiThreadIdInfo,
                DispatcherInfo,
                SyncContextFormInfo,
                ThreadMonitorInfo,
                MonitorUiThreadInfo,
                OnLockupDetectedInfo,
                MonitorTimeProviderInfo,
                LockupThresholdInfo,
            };

        /// <summary>
        /// Captures every controlled field plus <c>UiThread.SyncContextFormFactory</c>, resets the
        /// statics, and returns a scope that restores the captured values when disposed.
        /// </summary>
        /// <returns>A scope whose disposal restores the captured prior state.</returns>
        internal static UiThreadStateScope Enter()
        {
            FieldInfo[] fields = ControlledFields;
            var prior = new object?[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                prior[i] = fields[i].GetValue(null);
            }

            Func<IUiCaptureSource> priorFactory = UiThread.SyncContextFormFactory;
            UiThread.ResetForTesting();
            return new UiThreadStateScope(prior, priorFactory);
        }

        /// <summary>Reads <c>UiThread._monitorUiThread</c> without going through a property.</summary>
        internal static bool MonitorUiThread => (bool)MonitorUiThreadInfo.GetValue(null);

        /// <summary>Reads <c>UiThread._onLockupDetected</c> without going through a property.</summary>
        internal static Action<LockupAttribution>? OnLockupDetected =>
            (Action<LockupAttribution>?)OnLockupDetectedInfo.GetValue(null);

        /// <summary>Reads <c>UiThread._monitorTimeProvider</c> without going through a property.</summary>
        internal static TimeProvider? MonitorTimeProvider =>
            (TimeProvider?)MonitorTimeProviderInfo.GetValue(null);

        /// <summary>Reads <c>UiThread._lockupAttributionThresholdMs</c> without going through a property.</summary>
        internal static int LockupAttributionThresholdMs => (int)LockupThresholdInfo.GetValue(null);

        /// <summary>
        /// Reads <c>UiThread._uiSyncContext</c> directly.
        /// </summary>
        /// <remarks>
        /// The <c>UiThread.UiSyncContext</c> property lazily calls <c>Init()</c> when the field is
        /// null, so a test that needs to observe the uninitialized state cannot use the property.
        /// </remarks>
        internal static SynchronizationContext? UiSyncContextField =>
            (SynchronizationContext?)UiSyncContextInfo.GetValue(null);

        /// <summary>
        /// Reads <c>UiThread._autoScaleFactor</c> directly, for the same reason as
        /// <see cref="UiSyncContextField"/>.
        /// </summary>
        internal static System.Drawing.SizeF? AutoScaleFactorField =>
            (System.Drawing.SizeF?)AutoScaleFactorInfo.GetValue(null);

        /// <summary>Reads <c>UiThread._uiThreadId</c> directly.</summary>
        internal static int UiThreadIdField => (int)UiThreadIdInfo.GetValue(null);

        /// <summary>
        /// Reads <c>UiThread._dispatcher</c> directly.
        /// </summary>
        /// <remarks>
        /// The <c>UiThread.Dispatcher</c> property throws when the field is unset, so a test that
        /// needs to observe the uninitialized state cannot use the property.
        /// </remarks>
        internal static Dispatcher? DispatcherField => (Dispatcher?)DispatcherInfo.GetValue(null);

        /// <summary>Reads <c>UiThread._syncContextForm</c> directly.</summary>
        internal static IUiCaptureSource? SyncContextFormField =>
            (IUiCaptureSource?)SyncContextFormInfo.GetValue(null);

        /// <summary>Reads <c>UiThread._threadMonitor</c> directly.</summary>
        internal static ThreadMonitor? ThreadMonitorField =>
            (ThreadMonitor?)ThreadMonitorInfo.GetValue(null);

        /// <summary>
        /// Installs a value into <c>UiThread._uiSyncContext</c> for the remainder of this scope.
        /// </summary>
        /// <param name="value">The value to install; may be null.</param>
        internal static void SetUiSyncContext(SynchronizationContext? value) =>
            UiSyncContextInfo.SetValue(null, value);

        /// <summary>
        /// Installs a value into <c>UiThread._uiThreadId</c> for the remainder of this scope.
        /// </summary>
        /// <param name="value">The managed thread id to install.</param>
        internal static void SetUiThreadId(int value) => UiThreadIdInfo.SetValue(null, value);

        /// <summary>
        /// Installs a value into <c>UiThread._dispatcher</c> for the remainder of this scope.
        /// </summary>
        /// <param name="value">The dispatcher to install; may be null.</param>
        internal static void SetDispatcher(Dispatcher? value) =>
            DispatcherInfo.SetValue(null, value);

        /// <summary>
        /// Restores every captured value, including a captured null, and restores the factory.
        /// </summary>
        /// <remarks>
        /// Each captured prior is written back unconditionally rather than being tested for null
        /// first: a null prior is a real state that must be restored, and skipping the write for it
        /// would leak an installed value into every later test on the same process-global static.
        /// A second call is a no-op.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            FieldInfo[] fields = ControlledFields;
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i].SetValue(null, _priorValues[i]);
            }

            UiThread.SyncContextFormFactory = _priorFactory;
            _disposed = true;
        }

        private static FieldInfo Resolve(string fieldName)
        {
            FieldInfo field = typeof(UiThread).GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Static
            );
            field
                .Should()
                .NotBeNull(
                    because: "UiThread.{0} backing field must exist for UiThreadStateScope to "
                        + "control it; a rename must fail loudly here rather than degrade to a "
                        + "silent no-op that restores nothing and still passes",
                    fieldName
                );
            return field;
        }
    }

#nullable restore annotations
}
