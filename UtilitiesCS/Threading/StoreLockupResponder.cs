#nullable enable
using System;
using System.Reflection;
using log4net;
using UtilitiesCS.Dialogs;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Threading
{
    /// <summary>
    /// Shows the modeless three-button store-lockup notification for the given store identity. The
    /// three actions are the F1 service calls wired by <see cref="StoreLockupResponder"/>. Defaults
    /// to the in-assembly modeless composition (<see cref="MyBoxModeless.ShowStoreLockupNotification(string, Action, Action, Action)"/>);
    /// tests substitute a non-displaying stub.
    /// </summary>
    /// <param name="identity">The cached store identity displayed in the message.</param>
    /// <param name="disableSessionOnly">Action for the "Disable This Session Only" button.</param>
    /// <param name="disableForFutureSessions">Action for the "Disable for Future Sessions" button.</param>
    /// <param name="reenable">Action for the "Reenable" button.</param>
    public delegate void StoreLockupNotifier(
        string identity,
        Action disableSessionOnly,
        Action disableForFutureSessions,
        Action reenable
    );

    /// <summary>
    /// Host-neutral orchestrator invoked by the <see cref="ThreadMonitor"/> lockup callback
    /// (issue #264, epic #260). On the watchdog's background thread it auto-disables the attributed
    /// store (F1's <see cref="IStoreDisableService.DisableSessionOnly"/>), emits one
    /// <c>[store-lockup]</c> WARN line, then marshals a modeless notification onto the UI thread via
    /// <see cref="IUiDispatcher.BeginInvoke"/> (fire-and-forget, never <c>Invoke</c>, never modal).
    /// All dependencies are interface/delegate seams, so the class is Moq-testable without Outlook.
    /// It makes no direct COM call and no direct F3 call: F1's <see cref="IStoreDisableService.ReenableAsync"/>
    /// orchestrates the F3 rehook internally.
    /// </summary>
    public sealed class StoreLockupResponder
    {
        private static readonly ILog Log = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType
        );

        private readonly IStoreDisableService _disableService;
        private readonly IUiDispatcher _dispatcher;
        private readonly StoreLockupNotifier _notify;
        private readonly Action<string> _logSink;

        /// <summary>
        /// Creates a responder.
        /// </summary>
        /// <param name="disableService">F1's disable/enable service.</param>
        /// <param name="dispatcher">The UI-dispatch seam used for the non-blocking notify hop.</param>
        /// <param name="notify">
        /// The modeless-notify composition seam. When null, defaults to the in-assembly
        /// <see cref="MyBoxModeless.ShowStoreLockupNotification(string, Action, Action, Action)"/>.
        /// </param>
        /// <param name="logSink">
        /// The WARN sink for the <c>[store-lockup]</c> line. When null, defaults to log4net WARN.
        /// </param>
        public StoreLockupResponder(
            IStoreDisableService disableService,
            IUiDispatcher dispatcher,
            StoreLockupNotifier? notify = null,
            Action<string>? logSink = null
        )
        {
            _disableService =
                disableService ?? throw new ArgumentNullException(nameof(disableService));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _notify = notify ?? MyBoxModeless.ShowStoreLockupNotification;
            _logSink = logSink ?? (message => Log.Warn(message));
        }

        /// <summary>
        /// Handles a confirmed UI lockup. Enforces the no-context and already-disabled guards, then
        /// (when it acts) auto-disables the store, emits the WARN line, and shows the modeless
        /// notification, strictly in that order. Intended as the <c>onLockupDetected</c> callback for
        /// <see cref="ThreadMonitor"/>.
        /// </summary>
        /// <param name="attribution">The confirmed lockup attribution carrying identity and stall duration.</param>
        public void OnLockupDetected(LockupAttribution attribution)
        {
            var displayName = attribution.StoreIdentity;

            // Guard — no context: a stall with no attributed store performs no disable, no notify,
            // and no attributed WARN line.
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return;
            }

            var identity = StoreIdentity.Resolve(displayName);
            if (
                string.Equals(
                    identity.Value,
                    StoreIdentity.UnresolvedSentinel,
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }

            // Guard — enumeration phase (issue #292): a stall attributed to the raw Namespace.Stores
            // enumeration carries a non-null, non-store phase identity. Emit one attributed WARN with
            // autoDisabled: false and return WITHOUT any IStoreDisableService call. This must precede
            // the already-disabled guard and every disable-service call: during the fresh-build window
            // the disabled-store model does not yet exist, so DisableSessionOnly ->
            // GetModelForWriteOrThrow would throw InvalidOperationException and crash the watchdog
            // thread; and a phase identity is not a real store to disable (which would also pollute the
            // #265 disabled-store UI).
            if (
                string.Equals(
                    displayName,
                    CurrentStoreContext.StoresEnumerationPhaseIdentity,
                    StringComparison.Ordinal
                )
            )
            {
                _logSink(
                    StoreLockupAttribution.FormatLine(
                        displayName,
                        attribution.StallDuration,
                        autoDisabled: false
                    )
                );
                return;
            }

            // Guard — already disabled: no second disable and no duplicate notification.
            if (_disableService.IsDisabled(identity))
            {
                return;
            }

            // Auto-disable immediately (pure in-memory state change per F1; safe off the STA).
            _disableService.DisableSessionOnly(identity);

            // Log exactly one [store-lockup] WARN line via the injected sink.
            _logSink(
                StoreLockupAttribution.FormatLine(
                    displayName,
                    attribution.StallDuration,
                    autoDisabled: true
                )
            );

            // Notify (fire-and-forget) on the UI thread. Never Invoke, never modal.
            _dispatcher.BeginInvoke(() =>
                // displayName is guaranteed non-null here by the IsNullOrWhiteSpace guard above
                // (net481 IsNullOrWhiteSpace does not refine null-state, so an explicit ! is needed).
                _notify(
                    displayName!,
                    () => _disableService.DisableSessionOnly(identity),
                    () => _disableService.DisableForFutureSessions(identity),
                    () => _ = _disableService.ReenableAsync(identity)
                )
            );
        }
    }
}
