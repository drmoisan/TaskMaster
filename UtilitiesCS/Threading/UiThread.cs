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

        public static Dispatcher Dispatcher
        {
            get
            {
                if (_dispatcher is null)
                {
                    throw new InvalidOperationException(
                        "The UI dispatcher has not been captured. Call UiThread.Init() so that UiThread.Initialize() runs before reading UiThread.Dispatcher."
                    );
                }
                return _dispatcher;
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
