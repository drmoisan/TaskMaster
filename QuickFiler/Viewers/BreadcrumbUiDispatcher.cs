#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Schedules breadcrumb UI work on the synchronization boundary captured at construction.
    /// Scheduling and action failures are reported through one observable error sink.
    /// </summary>
    internal sealed class BreadcrumbUiDispatcher
    {
        [ThreadStatic]
        private static BreadcrumbUiDispatcher? _executingDispatcher;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            typeof(BreadcrumbUiDispatcher)
        );

        private readonly SynchronizationContext? _context;
        private readonly Action<Exception> _errorSink;
        private readonly int? _ownerThreadId;

        internal BreadcrumbUiDispatcher(SynchronizationContext context, Action<Exception> errorSink)
            : this(
                context ?? throw new ArgumentNullException(nameof(context)),
                errorSink,
                ownerThreadId: null
            ) { }

        private BreadcrumbUiDispatcher(
            SynchronizationContext? context,
            Action<Exception> errorSink,
            int? ownerThreadId
        )
        {
            _context = context;
            _errorSink = errorSink ?? throw new ArgumentNullException(nameof(errorSink));
            _ownerThreadId = ownerThreadId;
        }

        /// <summary>Captures the current production UI synchronization boundary.</summary>
        internal static BreadcrumbUiDispatcher CaptureCurrent()
        {
            SynchronizationContext context =
                SynchronizationContext.Current
                ?? throw new InvalidOperationException(
                    "Breadcrumb UI components must be constructed on an owning UI synchronization context."
                );
            return new BreadcrumbUiDispatcher(
                context,
                LogFailure,
                Environment.CurrentManagedThreadId
            );
        }

        /// <summary>
        /// Creates an owner-thread-only boundary for host-neutral unit tests without a UI pump.
        /// Cross-thread work is reported instead of being scheduled on a generic context.
        /// </summary>
        internal static BreadcrumbUiDispatcher CreateForCurrentThreadTests()
        {
            return new BreadcrumbUiDispatcher(null, LogFailure, Environment.CurrentManagedThreadId);
        }

        /// <summary>
        /// Executes inline on the captured boundary or schedules without blocking elsewhere.
        /// The returned task completes after the action runs or its failure is reported.
        /// </summary>
        internal Task Dispatch(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (IsCurrentBoundary())
            {
                BreadcrumbUiDispatcher? previousDispatcher = _executingDispatcher;
                _executingDispatcher = this;
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Report(exception);
                }
                finally
                {
                    _executingDispatcher = previousDispatcher;
                }
                return Task.CompletedTask;
            }

            if (_context == null)
            {
                Report(
                    new InvalidOperationException(
                        "The owner-thread-only test dispatcher cannot marshal cross-thread UI work."
                    )
                );
                return Task.CompletedTask;
            }

            var completion = new TaskCompletionSource<object?>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            int failureReported = 0;

            void ReportOnce(Exception exception)
            {
                if (Interlocked.Exchange(ref failureReported, 1) == 0)
                {
                    Report(exception);
                }
            }

            try
            {
                _context.Post(
                    _ =>
                    {
                        BreadcrumbUiDispatcher? previousDispatcher = _executingDispatcher;
                        _executingDispatcher = this;
                        try
                        {
                            action();
                        }
                        catch (Exception exception)
                        {
                            ReportOnce(exception);
                        }
                        finally
                        {
                            _executingDispatcher = previousDispatcher;
                            completion.TrySetResult(null);
                        }
                    },
                    null
                );
            }
            catch (Exception exception)
            {
                ReportOnce(exception);
                completion.TrySetResult(null);
            }

            return completion.Task;
        }

        /// <summary>
        /// Schedules one synchronous value-producing operation and propagates scheduling or action
        /// failure through both the observable sink and the returned task.
        /// </summary>
        internal Task<T> DispatchValue<T>(Func<T> action, bool reportFailure = true)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // Only a currently executing synchronous dispatcher callback proves that inline
            // control access is safe. Ambient context and thread identity do not survive awaits.
            if (ReferenceEquals(_executingDispatcher, this))
            {
                try
                {
                    return Task.FromResult(action());
                }
                catch (Exception exception)
                {
                    if (reportFailure)
                        Report(exception);
                    return Task.FromException<T>(exception);
                }
            }

            if (_context == null)
            {
                var failure = new InvalidOperationException(
                    "The owner-thread-only test dispatcher cannot marshal cross-thread UI work."
                );
                if (reportFailure)
                    Report(failure);
                return Task.FromException<T>(failure);
            }

            var completion = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            int failureReported = 0;

            void ReportOnce(Exception exception)
            {
                if (Interlocked.Exchange(ref failureReported, 1) == 0)
                {
                    if (reportFailure)
                        Report(exception);
                }
            }

            try
            {
                _context.Post(
                    _ =>
                    {
                        BreadcrumbUiDispatcher? previousDispatcher = _executingDispatcher;
                        _executingDispatcher = this;
                        try
                        {
                            completion.TrySetResult(action());
                        }
                        catch (Exception exception)
                        {
                            ReportOnce(exception);
                            completion.TrySetException(exception);
                        }
                        finally
                        {
                            _executingDispatcher = previousDispatcher;
                        }
                    },
                    null
                );
            }
            catch (Exception exception)
            {
                ReportOnce(exception);
                completion.TrySetException(exception);
            }

            return completion.Task;
        }

        /// <summary>Reports an already-observed boundary failure through the configured sink.</summary>
        internal void Report(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            try
            {
                _errorSink(exception);
            }
            catch (Exception sinkException)
            {
                log.Error("Breadcrumb UI error sink failed.", sinkException);
            }
        }

        private bool IsCurrentBoundary()
        {
            return ReferenceEquals(_executingDispatcher, this)
                || (_context != null && ReferenceEquals(SynchronizationContext.Current, _context))
                || (
                    _ownerThreadId.HasValue
                    && Environment.CurrentManagedThreadId == _ownerThreadId.Value
                );
        }

        private static void LogFailure(Exception exception)
        {
            log.Error("Breadcrumb UI dispatch failed.", exception);
        }
    }
}
