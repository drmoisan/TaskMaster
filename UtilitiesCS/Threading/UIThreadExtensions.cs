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
    public static class UIThreadExtensions
    {
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
    
        private static SyncContextForm _syncContextForm;

        private static SynchronizationContext _uiContext;
        
        public static System.Drawing.SizeF AutoScaleFactor 
        {
            get 
            { 
                if (_autoScaleFactor is null) { InitUiContext();  }
                return (System.Drawing.SizeF)_autoScaleFactor; 
            }
        }
        private static System.Drawing.SizeF? _autoScaleFactor = null;


        public static void InitUiContext(bool monitorUiThread = true)
        {
            _syncContextForm = new SyncContextForm();
            _uiContext = _syncContextForm.UiSyncContext;
            _autoScaleFactor = _syncContextForm.FormAutoScaleFactor;
            //Debug.WriteLine($"Ui Thread Id: {Thread.CurrentThread.ManagedThreadId}");
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            if (monitorUiThread)
            {
                _threadMonitor = new ThreadMonitor(Thread.CurrentThread, delayThreshold: 300);
                _threadMonitor.Run();
            }
        }
        
        public static SynchronizationContext GetUiContext()
        { 
            if (_uiContext is null) { InitUiContext(); }
            return _uiContext;
        }

        private static ThreadMonitor _threadMonitor;
        
        public static Dispatcher UiDispatcher => _uiDispatcher;
        private static Dispatcher _uiDispatcher;
    }
}
