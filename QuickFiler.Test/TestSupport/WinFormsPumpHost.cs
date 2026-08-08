using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace QuickFiler.Test.TestSupport
{
    /// <summary>
    /// Runs a real WinForms message pump (<see cref="Application.Run(ApplicationContext)"/>) on a
    /// dedicated STA background thread so tests can deterministically await continuations captured
    /// by a <see cref="WindowsFormsSynchronizationContext"/>. This is the WinForms analogue of the
    /// WPF <c>StaDispatcherHost</c> in <c>UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs</c>.
    /// </summary>
    /// <remarks>
    /// Awaiting <c>control.UiSyncContext</c> on a thread-pool MSTest thread hangs indefinitely
    /// because nothing drains the posted continuation; this host supplies the missing message loop.
    /// Usage contract: one host per test (or per test class), always released in
    /// <c>finally</c>/<c>using</c>. Construct the control under test through
    /// <see cref="InvokeAsync{TResult}(Func{TResult})"/> so no <see cref="SynchronizationContext"/>
    /// is ever installed on the MSTest thread. Only <see cref="Task"/>-returning members are
    /// exposed, which structurally prevents the test and pump threads from blocking on each other.
    /// </remarks>
    internal sealed class WinFormsPumpHost : IDisposable
    {
        private readonly Thread _thread;
        private readonly ManualResetEventSlim _ready = new ManualResetEventSlim(false);
        private readonly TaskCompletionSource<bool> _stopped = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        private readonly List<Exception> _pumpExceptions = new List<Exception>();
        private readonly object _pumpExceptionsLock = new object();
        private readonly List<Action> _pendingFaults = new List<Action>();
        private readonly object _pendingFaultsLock = new object();
        private readonly object _stopLock = new object();

        private volatile SynchronizationContext _syncContext;
        private volatile Exception _initializationError;
        private volatile bool _stopRequested;
        private volatile bool _disposed;
        private int _threadId;
        private Task _stopTask;

        /// <summary>
        /// Starts the pump thread and blocks until its synchronization context has been installed
        /// and captured. Never returns a half-initialized host: any failure recorded during startup
        /// is rethrown here with its original type and stack trace preserved.
        /// </summary>
        internal WinFormsPumpHost()
        {
            _thread = new Thread(RunPumpThread)
            {
                IsBackground = true,
                Name = "QuickFiler.Test.WinFormsPumpHost",
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
            _ready.Wait();

            Exception startupFailure = _initializationError;
            if (startupFailure != null)
            {
                _thread.Join();
                _ready.Dispose();
                ExceptionDispatchInfo.Capture(startupFailure).Throw();
            }
        }

        /// <summary>Gets the pump thread's <see cref="WindowsFormsSynchronizationContext"/>.</summary>
        internal SynchronizationContext SyncContext => _syncContext;

        /// <summary>Gets the pump thread's <see cref="Thread.ManagedThreadId"/>.</summary>
        internal int ThreadId => _threadId;

        /// <summary>
        /// Runs synchronous work on the pump thread. The returned task completes when the work
        /// returns, or faults with the original exception if the work throws.
        /// </summary>
        internal Task InvokeAsync(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            TaskCompletionSource<bool> completion = CreateCompletion<bool>();
            Post(
                completion,
                delegate
                {
                    try
                    {
                        action();
                        completion.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        completion.TrySetException(ex);
                    }
                }
            );
            return completion.Task;
        }

        /// <summary>
        /// Runs a synchronous factory on the pump thread and returns its value, for example
        /// <c>host.InvokeAsync(() =&gt; new QuickFiler.ItemViewer())</c>.
        /// </summary>
        internal Task<TResult> InvokeAsync<TResult>(Func<TResult> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            TaskCompletionSource<TResult> completion = CreateCompletion<TResult>();
            Post(
                completion,
                delegate
                {
                    try
                    {
                        completion.TrySetResult(factory());
                    }
                    catch (Exception ex)
                    {
                        completion.TrySetException(ex);
                    }
                }
            );
            return completion.Task;
        }

        /// <summary>
        /// Starts asynchronous work on the pump thread. The returned task completes when the inner
        /// task completes, propagating the original (unwrapped) exception on failure.
        /// </summary>
        internal Task RunAsync(Func<Task> asyncWork)
        {
            if (asyncWork == null)
            {
                throw new ArgumentNullException("asyncWork");
            }

            TaskCompletionSource<bool> completion = CreateCompletion<bool>();
            Post(
                completion,
                delegate
                {
                    try
                    {
                        Task inner = asyncWork();
                        if (inner == null)
                        {
                            completion.TrySetException(NullTaskFailure());
                            return;
                        }

                        ContinueWithOnCompletion(inner, () => CompleteVoid(inner, completion));
                    }
                    catch (Exception ex)
                    {
                        completion.TrySetException(ex);
                    }
                }
            );
            return completion.Task;
        }

        /// <summary>
        /// Starts asynchronous work on the pump thread and returns its value with unwrapped
        /// completion semantics.
        /// </summary>
        internal Task<TResult> RunAsync<TResult>(Func<Task<TResult>> asyncWork)
        {
            if (asyncWork == null)
            {
                throw new ArgumentNullException("asyncWork");
            }

            TaskCompletionSource<TResult> completion = CreateCompletion<TResult>();
            Post(
                completion,
                delegate
                {
                    try
                    {
                        Task<TResult> inner = asyncWork();
                        if (inner == null)
                        {
                            completion.TrySetException(NullTaskFailure());
                            return;
                        }

                        ContinueWithOnCompletion(inner, () => CompleteResult(inner, completion));
                    }
                    catch (Exception ex)
                    {
                        completion.TrySetException(ex);
                    }
                }
            );
            return completion.Task;
        }

        /// <summary>
        /// Retires any WPF dispatcher the pump thread created, posts
        /// <see cref="Application.ExitThread"/> onto the pump, awaits loop exit, joins the thread,
        /// and rethrows any exception recorded by the <see cref="Application.ThreadException"/>
        /// recorder. Idempotent: repeated calls return the same task.
        /// </summary>
        internal Task StopAsync()
        {
            lock (_stopLock)
            {
                if (_stopTask == null)
                {
                    _stopRequested = true;
                    _stopTask = StopCoreAsync();
                }

                return _stopTask;
            }
        }

        /// <summary>
        /// Idempotent synchronous bridge to <see cref="StopAsync"/>. Safe on the MSTest thread,
        /// which has no context bound to the pump. A second call is a no-op.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            StopAsync().GetAwaiter().GetResult();
        }

        private async Task StopCoreAsync()
        {
            Dispatcher pumpDispatcher = Dispatcher.FromThread(_thread);
            if (pumpDispatcher != null)
            {
                pumpDispatcher.InvokeShutdown();
            }

            SynchronizationContext context = _syncContext;
            if (context != null)
            {
                context.Post(
                    delegate
                    {
                        Application.ExitThread();
                    },
                    null
                );
            }

            await _stopped.Task.ConfigureAwait(false);
            _thread.Join();
            if (_thread.IsAlive)
            {
                throw new InvalidOperationException(
                    "The WinForms pump thread did not terminate after the message loop exited."
                );
            }

            FaultPendingWork();
            _ready.Dispose();

            Exception[] recorded;
            lock (_pumpExceptionsLock)
            {
                recorded = _pumpExceptions.ToArray();
            }

            if (recorded.Length == 1)
            {
                ExceptionDispatchInfo.Capture(recorded[0]).Throw();
            }

            if (recorded.Length > 1)
            {
                throw new AggregateException(
                    "The WinForms pump thread recorded unhandled exceptions.",
                    recorded
                );
            }
        }

        private void RunPumpThread()
        {
            ApplicationContext applicationContext = null;
            bool subscribed = false;
            try
            {
                try
                {
                    SynchronizationContext.SetSynchronizationContext(
                        new WindowsFormsSynchronizationContext()
                    );
                    _syncContext = SynchronizationContext.Current;
                    _threadId = Thread.CurrentThread.ManagedThreadId;
                }
                catch (Exception ex)
                {
                    _initializationError = ex;
                }
                finally
                {
                    _ready.Set();
                }

                if (_initializationError != null)
                {
                    return;
                }

                Application.ThreadException += RecordThreadException;
                subscribed = true;
                applicationContext = new ApplicationContext();
                Application.Run(applicationContext);
            }
            catch (Exception ex)
            {
                RecordPumpException(ex);
            }
            finally
            {
                if (subscribed)
                {
                    Application.ThreadException -= RecordThreadException;
                }

                if (applicationContext != null)
                {
                    applicationContext.Dispose();
                }

                _stopped.TrySetResult(true);
            }
        }

        private void RecordThreadException(object sender, ThreadExceptionEventArgs e) =>
            RecordPumpException(e.Exception);

        private void RecordPumpException(Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            lock (_pumpExceptionsLock)
            {
                _pumpExceptions.Add(exception);
            }
        }

        private TaskCompletionSource<TResult> CreateCompletion<TResult>() =>
            new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Posts <paramref name="work"/> to the pump, or fails fast when the host has stopped.
        /// The completion source is registered so a post that races with shutdown is faulted at
        /// stop rather than left pending forever.
        /// </summary>
        private void Post<TResult>(
            TaskCompletionSource<TResult> completion,
            SendOrPostCallback work
        )
        {
            if (_stopRequested)
            {
                completion.TrySetException(CreateStoppedException());
                return;
            }

            lock (_pendingFaultsLock)
            {
                _pendingFaults.Add(() => completion.TrySetException(CreateStoppedException()));
            }

            _syncContext.Post(work, null);
        }

        private void FaultPendingWork()
        {
            Action[] faults;
            lock (_pendingFaultsLock)
            {
                faults = _pendingFaults.ToArray();
                _pendingFaults.Clear();
            }

            for (int i = 0; i < faults.Length; i++)
            {
                faults[i]();
            }
        }

        private static ObjectDisposedException CreateStoppedException() =>
            new ObjectDisposedException(
                "WinFormsPumpHost",
                "The WinForms pump host has been stopped; no further work can be posted to it."
            );

        private static InvalidOperationException NullTaskFailure() =>
            new InvalidOperationException("The asynchronous work delegate returned a null Task.");

        private static void ContinueWithOnCompletion(Task inner, Action onCompleted)
        {
            inner.ContinueWith(
                t => onCompleted(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
            );
        }

        private static void CompleteVoid(Task inner, TaskCompletionSource<bool> completion)
        {
            if (inner.IsFaulted)
            {
                completion.TrySetException(Unwrap(inner.Exception));
            }
            else if (inner.IsCanceled)
            {
                completion.TrySetCanceled();
            }
            else
            {
                completion.TrySetResult(true);
            }
        }

        private static void CompleteResult<TResult>(
            Task<TResult> inner,
            TaskCompletionSource<TResult> completion
        )
        {
            if (inner.IsFaulted)
            {
                completion.TrySetException(Unwrap(inner.Exception));
            }
            else if (inner.IsCanceled)
            {
                completion.TrySetCanceled();
            }
            else
            {
                completion.TrySetResult(inner.Result);
            }
        }

        /// <summary>
        /// Reduces a task's <see cref="AggregateException"/> to the original exception when there
        /// is exactly one, so callers observe the same exception type the work threw.
        /// </summary>
        private static Exception Unwrap(AggregateException aggregate)
        {
            if (aggregate == null)
            {
                return new InvalidOperationException(
                    "The pumped work faulted without recording an exception."
                );
            }

            AggregateException flattened = aggregate.Flatten();
            if (flattened.InnerExceptions.Count == 1)
            {
                return flattened.InnerExceptions[0];
            }

            return flattened;
        }
    }
}
