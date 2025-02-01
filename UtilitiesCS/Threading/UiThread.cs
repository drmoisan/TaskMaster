using QuickFiler.Viewers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using UtilitiesCS.Threading;
using System.Windows.Threading;

namespace UtilitiesCS
{
    public static class UiThread
    {
        public static void Init(bool monitorUiThread = false) 
        { 
            if (_loaded.CheckAndSetFirstCall)
            {
                Initialize(monitorUiThread);
            }
        }
        
        private static ThreadSafeSingleShotGuard _loaded = new ThreadSafeSingleShotGuard();
        private static void Initialize(bool monitorUiThread)
        {
            _syncContextForm = new SyncContextForm();
            UiSyncContext = _syncContextForm.UiSyncContext;
            AutoScaleFactor = _syncContextForm.FormAutoScaleFactor;
            UiThreadId = Thread.CurrentThread.ManagedThreadId;
            Dispatcher = Dispatcher.CurrentDispatcher;
            
            if (monitorUiThread)
            {
                _threadMonitor = new ThreadMonitor(Thread.CurrentThread, delayThreshold: 300);
                _threadMonitor.Run();
            }
        }
        
        private static SyncContextForm _syncContextForm;

        #region UI Thread Synchronization

        public struct SynchronizationContextAwaiter : INotifyCompletion
        {
            private static readonly SendOrPostCallback _postCallback = state => ((Action)state)();

            private readonly SynchronizationContext _context;
            public SynchronizationContextAwaiter(SynchronizationContext context)
            {
                if(context is null) { throw new ArgumentNullException(nameof(context)); }
                _context = context;
            }

            public bool IsCompleted => _context == SynchronizationContext.Current;

            public void OnCompleted(Action continuation) => _context.Post(_postCallback, continuation);

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
                if (_uiSyncContext is null) { Init(); }
                return _uiSyncContext;
            }
            private set => _uiSyncContext = value;
        }
        private static SynchronizationContext _uiSyncContext;
        
        public static int UiThreadId { get => _uiThreadId; private set => _uiThreadId = value; }
        private static int _uiThreadId = -1;
        
        public static Dispatcher Dispatcher { get => _dispatcher; private set => _dispatcher = value; }
        private static Dispatcher _dispatcher;

        #endregion UI Thread Synchronization

        #region Other UI Methods and Properties

        private static ThreadMonitor _threadMonitor;

        public static System.Drawing.SizeF AutoScaleFactor 
        {
            get 
            { 
                if (_autoScaleFactor is null) { Init();  }
                return (System.Drawing.SizeF)_autoScaleFactor; 
            }
            private set => _autoScaleFactor = value;
        }
        private static System.Drawing.SizeF? _autoScaleFactor = null;

        #endregion Other UI Methods and Properties
    }
}
