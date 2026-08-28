#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuickFiler.Viewers
{
    /// <summary>Identifies one coordinator-owned suggestion-population generation.</summary>
    internal sealed class BreadcrumbUpgradeLease
    {
        private readonly CancellationTokenSource _source;

        internal BreadcrumbUpgradeLease(long generation, CancellationTokenSource source)
        {
            Generation = generation;
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        internal long Generation { get; }
        internal CancellationToken Token => _source.Token;
        internal bool CancellationStarted { get; set; }
        internal bool Cancelled { get; set; }
        internal bool Settled { get; set; }
        internal bool SourceDisposed { get; set; }

        internal void Cancel() => _source.Cancel();

        internal void DisposeSource() => _source.Dispose();
    }

    /// <summary>
    /// Owns the generation and cancellation lifetime for asynchronous suggestion populations.
    /// Superseded sources remain valid until their associated provider operation settles.
    /// </summary>
    internal sealed class BreadcrumbCoordinatorUpgradeLifetime : IDisposable
    {
        private readonly Action<Exception> _report;
        private readonly object _sync = new object();
        private BreadcrumbUpgradeLease? _current;
        private long _generation;
        private bool _disposed;

        internal BreadcrumbCoordinatorUpgradeLifetime(Action<Exception> report)
        {
            _report = report ?? throw new ArgumentNullException(nameof(report));
        }

        internal BreadcrumbUpgradeLease BeginPopulation(
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            CancellationTokenSource source = cancellationToken.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                : new CancellationTokenSource();
            BreadcrumbUpgradeLease? superseded;
            BreadcrumbUpgradeLease lease;
            lock (_sync)
            {
                if (_disposed)
                {
                    source.Dispose();
                    throw new ObjectDisposedException(nameof(BreadcrumbCoordinatorUpgradeLifetime));
                }
                superseded = _current;
                lease = new BreadcrumbUpgradeLease(++_generation, source);
                _current = lease;
            }
            CancelLease(superseded);
            return lease;
        }

        internal bool Invalidate()
        {
            BreadcrumbUpgradeLease? superseded;
            lock (_sync)
            {
                if (_disposed)
                {
                    return false;
                }
                _generation++;
                superseded = _current;
                _current = null;
            }
            CancelLease(superseded);
            return true;
        }

        internal void Abandon(BreadcrumbUpgradeLease lease)
        {
            lock (_sync)
            {
                if (ReferenceEquals(_current, lease))
                {
                    _generation++;
                    _current = null;
                }
            }
            CancelLease(lease);
            Complete(lease);
        }

        internal bool IsCurrent(BreadcrumbUpgradeLease lease)
        {
            lock (_sync)
            {
                return IsGenerationCurrentCore(lease) && !lease.Token.IsCancellationRequested;
            }
        }

        /// <summary>
        /// Runs <paramref name="operation"/> under <paramref name="lease"/>'s currency guard and
        /// reports whether it ran.
        /// </summary>
        /// <returns>
        /// <c>true</c> when the guarded action was invoked at entry-time currency; <c>false</c> when it
        /// was skipped because the lease was not current, in which case the lease HAS BEEN SETTLED via
        /// <see cref="Abandon"/> before this method returns, so no
        /// <see cref="System.Threading.CancellationTokenSource"/> is leaked (I-502.3).
        /// </returns>
        /// <remarks>
        /// Issue #502: the caller must consume this value. `SetSuggestionsCore` replaces its stale
        /// `SuggestionsUpgrade` handle on <c>false</c> and `AddItems` settles its lease on <c>false</c>.
        /// The skip-path <see cref="Abandon"/> here is idempotent with respect to a caller that also
        /// calls it: <see cref="Abandon"/> returns early once the lease's cancellation has started, and
        /// both disposal predicates are guarded by <c>!lease.SourceDisposed</c>.
        /// </remarks>
        internal bool RunSynchronous(BreadcrumbUpgradeLease lease, Action operation)
        {
            try
            {
                if (TryRunCurrent(lease, operation))
                {
                    return true;
                }
                Abandon(lease);
                return false;
            }
            catch
            {
                Abandon(lease);
                throw;
            }
        }

        internal Action Guard(BreadcrumbUpgradeLease? lease, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            return lease == null ? action : new Action(() => TryRunCurrent(lease, action));
        }

        /// <summary>
        /// Runs <paramref name="action"/> if <paramref name="lease"/> is current, and reports whether
        /// it was invoked.
        /// </summary>
        /// <returns>
        /// The ENTRY-TIME currency verdict: <c>true</c> when the action was invoked because the lease
        /// was current at entry, <c>false</c> when it was skipped. This verdict is captured under
        /// <c>_sync</c> before the action runs and MUST NEVER be recomputed after the action returns.
        /// Folding a post-action re-check into this value would make <c>false</c> ambiguous between
        /// "did not run" and "ran but was superseded", which is what the #502 call sites branch on;
        /// a caller that wants a post-action verdict must call <see cref="IsCurrent"/> separately.
        /// </returns>
        /// <remarks>
        /// Issue #500 (I-500.1): the action is invoked with <c>_sync</c> RELEASED, so no foreign or
        /// out-of-process call is ever made under the lifetime's monitor. Documented consequence: two
        /// threads can now both pass the currency check and run their actions concurrently, where the
        /// re-entrant monitor previously serialized them. That is not reachable on current wiring —
        /// every guarded action runs on the captured <c>BreadcrumbUiDispatcher</c> boundary, and
        /// <see cref="RunSynchronous"/> is reached only from the viewer thread.
        /// </remarks>
        internal bool TryRunCurrent(BreadcrumbUpgradeLease lease, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            bool current;
            lock (_sync)
            {
                current = IsGenerationCurrentCore(lease) && !lease.Token.IsCancellationRequested;
            }
            if (!current)
            {
                return false;
            }
            action();
            return true;
        }

        internal async Task RunAsync(
            BreadcrumbUpgradeLease lease,
            Func<CancellationToken, Task> operation
        )
        {
            if (lease == null)
            {
                throw new ArgumentNullException(nameof(lease));
            }
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }
            try
            {
                lease.Token.ThrowIfCancellationRequested();
                await operation(lease.Token).ConfigureAwait(false);
                lease.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) when (!IsGenerationCurrent(lease))
            {
                // Coordinator-owned invalidation settles stale work without surfacing cancellation.
            }
            finally
            {
                Complete(lease);
            }
        }

        internal async Task RunAsync<T>(
            BreadcrumbUpgradeLease lease,
            Func<CancellationToken, Task<T>> operation,
            Func<T, Task> publishCurrent
        )
        {
            try
            {
                lease.Token.ThrowIfCancellationRequested();
                T result = await operation(lease.Token).ConfigureAwait(false);
                lease.Token.ThrowIfCancellationRequested();
                if (IsCurrent(lease))
                {
                    await publishCurrent(result).ConfigureAwait(false);
                    lease.Token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException) when (!IsGenerationCurrent(lease)) { }
            finally
            {
                Complete(lease);
            }
        }

        internal bool TryDispose()
        {
            BreadcrumbUpgradeLease? superseded;
            lock (_sync)
            {
                if (_disposed)
                {
                    return false;
                }
                _disposed = true;
                _generation++;
                superseded = _current;
                _current = null;
            }
            CancelLease(superseded);
            return true;
        }

        public void Dispose()
        {
            if (TryDispose())
            {
                GC.SuppressFinalize(this);
            }
        }

        private bool IsGenerationCurrent(BreadcrumbUpgradeLease lease)
        {
            lock (_sync)
            {
                return IsGenerationCurrentCore(lease);
            }
        }

        private bool IsGenerationCurrentCore(BreadcrumbUpgradeLease lease) =>
            !_disposed && ReferenceEquals(_current, lease) && lease.Generation == _generation;

        private void Complete(BreadcrumbUpgradeLease lease)
        {
            bool dispose;
            lock (_sync)
            {
                lease.Settled = true;
                dispose = lease.Cancelled && !lease.SourceDisposed;
                if (dispose)
                {
                    lease.SourceDisposed = true;
                }
            }
            if (dispose)
            {
                DisposeLease(lease);
            }
        }

        private void CancelLease(BreadcrumbUpgradeLease? lease)
        {
            if (lease == null)
            {
                return;
            }
            lock (_sync)
            {
                if (lease.CancellationStarted)
                {
                    return;
                }
                lease.CancellationStarted = true;
            }
            try
            {
                lease.Cancel();
            }
            catch (Exception exception)
            {
                _report(exception);
            }

            bool dispose;
            lock (_sync)
            {
                lease.Cancelled = true;
                dispose = lease.Settled && !lease.SourceDisposed;
                if (dispose)
                {
                    lease.SourceDisposed = true;
                }
            }
            if (dispose)
            {
                DisposeLease(lease);
            }
        }

        private void DisposeLease(BreadcrumbUpgradeLease lease)
        {
            try
            {
                lease.DisposeSource();
            }
            catch (Exception exception)
            {
                _report(exception);
            }
        }
    }
}
