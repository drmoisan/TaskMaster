#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using QuickFiler.Viewers;
using UtilitiesCS.Threading;

namespace UtilitiesCS
{
    public static class UiThread
    {
        public static void Init(
            bool monitorUiThread = false,
            Action<LockupAttribution>? onLockupDetected = null,
            TimeProvider? timeProvider = null,
            int lockupAttributionThresholdMs = 5000
        )
        {
            _monitorUiThread = monitorUiThread;
            if (onLockupDetected is not null)
            {
                _onLockupDetected = onLockupDetected;
            }
            if (timeProvider is not null)
            {
                _monitorTimeProvider = timeProvider;
            }
            _lockupAttributionThresholdMs = lockupAttributionThresholdMs;
            if (_loaded.CheckAndSetFirstCall)
            {
                Initialize();
            }
        }

        private static bool _monitorUiThread;
        private static Action<LockupAttribution>? _onLockupDetected;
        private static TimeProvider? _monitorTimeProvider;
        private static int _lockupAttributionThresholdMs = 5000;
        private static ThreadSafeSingleShotGuard _loaded = new ThreadSafeSingleShotGuard();

        private static void Initialize()
        {
            // Create a hidden form to initialize the synchronization context
            _syncContextForm = new SyncContextForm();
            _syncContextForm.ShowInTaskbar = false;
            _syncContextForm.WindowState = FormWindowState.Minimized;
            _syncContextForm.Show();

            // Set the synchronization context and auto-scale factor
            _syncContextForm.CaptureUiVariables();
            UiSyncContext = _syncContextForm.UiSyncContext;
            AutoScaleFactor = _syncContextForm.FormAutoScaleFactor;
            UiThreadId = _syncContextForm.UiThreadId;
            Dispatcher = _syncContextForm.UiDispatcher;

            // Optionally monitor the UI thread. When enabled (issue #264), the monitor is driven by
            // the injected clock (production TimeProvider.System) and raises the F4 lockup callback
            // when the attribution threshold is crossed.
            if (_monitorUiThread)
            {
                _threadMonitor = new ThreadMonitor(
                    Thread.CurrentThread,
                    delayThreshold: 300,
                    timeProvider: _monitorTimeProvider ?? TimeProvider.System,
                    lockupAttributionThresholdMs: _lockupAttributionThresholdMs,
                    onLockupDetected: _onLockupDetected
                );
                _threadMonitor.Run();
            }

            _syncContextForm.Hide();
        }

        private static SyncContextForm? _syncContextForm;

        #region UI Thread Synchronization

        public struct SynchronizationContextAwaiter : INotifyCompletion
        {
            private static readonly SendOrPostCallback _postCallback = state => ((Action)state)();

            private readonly SynchronizationContext _context;

            public SynchronizationContextAwaiter(SynchronizationContext? context)
            {
                if (context is null)
                {
                    throw new ArgumentNullException(nameof(context));
                }
                _context = context;
            }

            public bool IsCompleted => _context == SynchronizationContext.Current;

            public void OnCompleted(Action continuation) =>
                _context.Post(_postCallback, continuation);

            public void GetResult() { }
        }

        public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext context)
        {
            return new SynchronizationContextAwaiter(context);
        }

        public static SynchronizationContext UiSyncContext
        {
            get
            {
                if (_uiSyncContext is null)
                {
                    Init();
                }
                // Init() populates _uiSyncContext before returning.
                return _uiSyncContext!;
            }
            private set => _uiSyncContext = value;
        }
        private static SynchronizationContext? _uiSyncContext;

        public static int UiThreadId
        {
            get => _uiThreadId;
            private set => _uiThreadId = value;
        }
        private static int _uiThreadId = -1;

        internal const string DispatcherNotInitializedMessage =
            "The UI dispatcher has not been captured. Call UiThread.Init() on the UI (STA) thread during host startup before reading UiThread.Dispatcher.";

        /// <summary>
        /// Gets the dispatcher captured from the UI (STA) thread during host startup.
        /// </summary>
        /// <remarks>
        /// This accessor is deliberately not lazy. Unlike the sibling <see cref="UiSyncContext"/>
        /// and <see cref="AutoScaleFactor"/> accessors, it does not call <see cref="Init"/> to
        /// self-heal when the backing field is unset, because initialization has UI-thread
        /// affinity and must be performed once by the host rather than by an arbitrary reader.
        /// The contract is therefore strict: the caller must have completed startup
        /// initialization before reading this property.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the dispatcher has not been captured, that is when <see cref="Init"/> has
        /// not completed on the UI (STA) thread.
        /// </exception>
        public static Dispatcher Dispatcher
        {
            get
            {
                // Read the non-volatile static exactly once so the guard and the return value
                // cannot observe different values if another thread completes Init() in between.
                Dispatcher? captured = _dispatcher;
                if (captured is null)
                {
                    // Initialize() constructs and shows a hidden WinForms SyncContextForm, so it
                    // has UI-thread affinity. A lazy Init() from an arbitrary reader is therefore
                    // deliberately avoided here even though the sibling UiSyncContext and
                    // AutoScaleFactor accessors do self-heal.
                    throw new InvalidOperationException(DispatcherNotInitializedMessage);
                }
                return captured;
            }
            private set => _dispatcher = value;
        }
        private static Dispatcher? _dispatcher;
        #endregion UI Thread Synchronization

        #region Other UI Methods and Properties

        private static ThreadMonitor? _threadMonitor;

        public static System.Drawing.SizeF AutoScaleFactor
        {
            get
            {
                if (_autoScaleFactor is null)
                {
                    Init();
                }
                return _autoScaleFactor ?? new System.Drawing.SizeF(1f, 1f);
            }
            private set => _autoScaleFactor = value;
        }
        private static System.Drawing.SizeF? _autoScaleFactor = null;

        #endregion Other UI Methods and Properties
    }
}
