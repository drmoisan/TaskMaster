#nullable enable
using System;
using System.Threading.Tasks;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Owns one collapsed-surface readiness generation and publishes only its current exact-success
    /// candidate through <see cref="ReadyMessenger"/>.
    /// </summary>
    internal sealed class BreadcrumbCollapsedSurfaceController : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            typeof(BreadcrumbCollapsedSurfaceController)
        );

        private readonly object _sync = new object();
        private TaskCompletionSource<bool> _generationCancellation = NewCompletionSource();
        private IWebViewMessenger? _pendingMessenger;
        private Task? _pendingReadiness;
        private IDisposable? _pendingReadinessLifetime;
        private Task<bool>? _pendingAttachment;
        private IWebViewMessenger? _readyMessenger;
        private long _generation;
        private bool _disposed;

        /// <summary>The messenger published by the current successful navigation.</summary>
        internal IWebViewMessenger? ReadyMessenger
        {
            get
            {
                lock (_sync)
                {
                    return _readyMessenger;
                }
            }
        }

        /// <summary>Waits for an externally supplied exact-navigation readiness task.</summary>
        internal Task<bool> AttachAsync(IWebViewMessenger messenger, Task readiness)
        {
            return AttachCore(messenger, readiness, readinessLifetime: null);
        }

        /// <summary>
        /// Waits for a shared navigation lifetime that can detach its handlers on reset/disposal.
        /// </summary>
        internal Task<bool> AttachAsync(
            IWebViewMessenger messenger,
            BreadcrumbNavigationReadiness readiness
        )
        {
            if (readiness == null)
            {
                throw new ArgumentNullException(nameof(readiness));
            }
            return AttachCore(messenger, readiness.Completion, readiness);
        }

        /// <summary>Invalidates the current generation and clears any published messenger.</summary>
        internal void Reset()
        {
            IWebViewMessenger? readyMessenger;
            IDisposable? readinessLifetime;
            lock (_sync)
            {
                ThrowIfDisposed();
                InvalidateGeneration();
                readyMessenger = _readyMessenger;
                readinessLifetime = _pendingReadinessLifetime;
                _readyMessenger = null;
                ClearPending();
            }

            SafeDispose(readinessLifetime);
            SafeDispose(readyMessenger as IDisposable);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            IWebViewMessenger? readyMessenger;
            IDisposable? readinessLifetime;
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                InvalidateGeneration();
                readyMessenger = _readyMessenger;
                readinessLifetime = _pendingReadinessLifetime;
                _readyMessenger = null;
                ClearPending();
            }

            SafeDispose(readinessLifetime);
            SafeDispose(readyMessenger as IDisposable);
        }

        private Task<bool> AttachCore(
            IWebViewMessenger messenger,
            Task readiness,
            IDisposable? readinessLifetime
        )
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }
            if (readiness == null)
            {
                throw new ArgumentNullException(nameof(readiness));
            }

            TaskCompletionSource<bool> completion;
            Task cancellation;
            long generation;
            IDisposable? replacedReadiness;
            IWebViewMessenger? replacedReadyMessenger;
            lock (_sync)
            {
                ThrowIfDisposed();
                if (ReferenceEquals(_readyMessenger, messenger))
                {
                    return Task.FromResult(true);
                }
                if (ReferenceEquals(_pendingMessenger, messenger))
                {
                    if (ReferenceEquals(_pendingReadiness, readiness))
                    {
                        return _pendingAttachment!;
                    }
                    throw new InvalidOperationException(
                        "The collapsed messenger already has a pending navigation."
                    );
                }
                if (ReferenceEquals(_pendingReadiness, readiness))
                {
                    throw new InvalidOperationException(
                        "The pending navigation already belongs to another collapsed messenger."
                    );
                }

                replacedReadiness = _pendingReadinessLifetime;
                replacedReadyMessenger = _readyMessenger;
                InvalidateGeneration();
                _readyMessenger = null;
                ClearPending();

                generation = _generation;
                cancellation = _generationCancellation.Task;
                completion = NewCompletionSource();
                _pendingMessenger = messenger;
                _pendingReadiness = readiness;
                _pendingReadinessLifetime = readinessLifetime;
                _pendingAttachment = completion.Task;
            }

            SafeDispose(replacedReadiness);
            SafeDispose(replacedReadyMessenger as IDisposable);
            _ = CompleteAttachmentAsync(
                messenger,
                readiness,
                readinessLifetime,
                generation,
                cancellation,
                completion
            );
            return completion.Task;
        }

        private async Task CompleteAttachmentAsync(
            IWebViewMessenger messenger,
            Task readiness,
            IDisposable? readinessLifetime,
            long generation,
            Task cancellation,
            TaskCompletionSource<bool> completion
        )
        {
            _ = ObserveLateFailureAsync(readiness);
            bool published = false;
            try
            {
                Task completed = await Task.WhenAny(readiness, cancellation).ConfigureAwait(false);
                if (!ReferenceEquals(completed, readiness))
                {
                    return;
                }

                await readiness.ConfigureAwait(false);
                lock (_sync)
                {
                    if (!IsCurrent(generation, cancellation, messenger))
                    {
                        return;
                    }
                    _readyMessenger = messenger;
                    ClearPending();
                    published = true;
                }
            }
            catch (Exception)
            {
                // Readiness failure is an expected false result; the task is observed here.
            }
            finally
            {
                if (!published)
                {
                    RejectPending(generation, messenger);
                }
                SafeDispose(readinessLifetime);
                completion.TrySetResult(published);
            }
        }

        private void RejectPending(long generation, IWebViewMessenger messenger)
        {
            bool disposeMessenger;
            lock (_sync)
            {
                if (generation == _generation && ReferenceEquals(_pendingMessenger, messenger))
                {
                    ClearPending();
                }
                disposeMessenger =
                    !ReferenceEquals(_pendingMessenger, messenger)
                    && !ReferenceEquals(_readyMessenger, messenger);
            }

            if (disposeMessenger)
            {
                SafeDispose(messenger as IDisposable);
            }
        }

        private bool IsCurrent(long generation, Task cancellation, IWebViewMessenger messenger)
        {
            return !_disposed
                && generation == _generation
                && ReferenceEquals(cancellation, _generationCancellation.Task)
                && !cancellation.IsCompleted
                && ReferenceEquals(_pendingMessenger, messenger);
        }

        private void InvalidateGeneration()
        {
            _generation++;
            _generationCancellation.TrySetResult(true);
            _generationCancellation = NewCompletionSource();
        }

        private void ClearPending()
        {
            _pendingMessenger = null;
            _pendingReadiness = null;
            _pendingReadinessLifetime = null;
            _pendingAttachment = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(BreadcrumbCollapsedSurfaceController));
            }
        }

        private static async Task ObserveLateFailureAsync(Task readiness)
        {
            try
            {
                await readiness.ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Cancellation and faults are deliberately observed after generation invalidation.
            }
        }

        private static void SafeDispose(IDisposable? disposable)
        {
            if (disposable == null)
            {
                return;
            }
            try
            {
                disposable.Dispose();
            }
            catch (Exception exception)
            {
                log.Error("Collapsed breadcrumb resource disposal failed.", exception);
            }
        }

        private static TaskCompletionSource<bool> NewCompletionSource()
        {
            return new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
        }
    }
}
