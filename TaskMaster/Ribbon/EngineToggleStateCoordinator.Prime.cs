using System;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;

namespace TaskMaster
{
    internal sealed partial class EngineToggleStateCoordinator
    {
        /// <summary>
        /// The registration marker for an engine key, exposed so tests can await the outcome of
        /// its prime deterministically instead of polling or sleeping. The marker is not the
        /// prime task itself: it is registered before the prime starts and is completed only after
        /// the prime outcome has been observed and, on a fault or cancellation, reported.
        /// </summary>
        /// <param name="engineName">The engine key; ordinal, case-sensitive.</param>
        /// <returns>
        /// The registered marker, or <see cref="Task.CompletedTask"/> when no marker is registered
        /// for the key. The marker never faults or cancels: a prime fault is observed by
        /// <see cref="CompletePrime"/> and reported through <c>logError</c>, and the marker is
        /// completed in a <c>finally</c> after that observation. For a key whose prime did not
        /// run to completion, the marker is cleared only after that report has returned or
        /// thrown, so a caller that receives <see cref="Task.CompletedTask"/> can rely on the
        /// report having been attempted or deliberately suppressed as a repeat of a kind already
        /// reported.
        /// </returns>
        internal Task GetPrimeTask(string engineName)
        {
            if (string.IsNullOrEmpty(engineName))
            {
                return Task.CompletedTask;
            }

            return _primeTasks.TryGetValue(engineName, out var prime) ? prime : Task.CompletedTask;
        }

        /// <summary>
        /// Starts the single prime for an engine key, unless one is already registered or the
        /// engines are not yet available.
        /// </summary>
        private void StartPrimeIfNeeded(string engineName, string controlId)
        {
            var engines = _enginesAccessor();
            if (engines is null)
            {
                return;
            }

            lock (_primeGate)
            {
                if (_primeTasks.ContainsKey(engineName))
                {
                    return;
                }

                // Registration precedes the start (issue #944): a prime can complete on any
                // thread, including before StartObservedPrime returns, and it must always find
                // its own marker to remove; registering afterwards let a finished prime's
                // removal run first and leave a stale marker that blocked every later re-prime.
                var marker = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
                _primeTasks[engineName] = marker.Task;
                StartObservedPrime(engines, engineName, controlId, marker);
            }
        }

        /// <summary>
        /// Runs <see cref="ApplyPrimeAsync"/> and attaches the fault observer.
        /// </summary>
        /// <remarks>
        /// The observer is a continuation rather than a <c>catch</c> clause. The two
        /// <c>catch</c> clauses in this type are the click boundary in
        /// <see cref="HandleToggleClickAsync"/> and the single sink guard in
        /// <see cref="TryInvokeSink"/>. Reading <see cref="Task.Exception"/> inside
        /// <see cref="CompletePrime"/> marks the fault observed, so no unobserved task remains.
        /// The continuation task itself is discarded; the value a test awaits is the marker,
        /// which the continuation completes only through <c>SetResult</c> in a <c>finally</c>
        /// after <see cref="CompletePrime"/> exits, so it never faults or cancels. Because
        /// <see cref="CompletePrime"/> routes its sink call through <see cref="TryInvokeSink"/>,
        /// the discarded continuation has no remaining throw source of its own.
        /// </remarks>
        private void StartObservedPrime(
            IAppItemEngines engines,
            string engineName,
            string controlId,
            TaskCompletionSource<bool> marker
        )
        {
            _ = ApplyPrimeAsync(engines, engineName, controlId)
                .ContinueWith(
                    completed =>
                    {
                        try
                        {
                            CompletePrime(completed, engineName);
                        }
                        finally
                        {
                            marker.SetResult(true);
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default
                );
        }

        /// <summary>
        /// Reads the real activation state once, stores it, and invalidates the mapped control.
        /// Contains no <c>catch</c>: a fault propagates into the returned task, where
        /// <see cref="CompletePrime"/> observes it.
        /// </summary>
        private async Task ApplyPrimeAsync(
            IAppItemEngines engines,
            string engineName,
            string controlId
        )
        {
            // The ticket is taken immediately before the activation read, so a prime whose
            // observation began before a toggle's cannot overwrite the toggle's newer result.
            var sequence = _pressedState.NextSequence();
            var active = await engines.EngineActiveAsync(engineName).ConfigureAwait(false);

            if (_pressedState.TryApplyState(engineName, active, sequence))
            {
                _invalidateControl(controlId);
            }
        }

        /// <summary>
        /// Observes the outcome of a prime. On any outcome other than ran-to-completion the cache
        /// is left unset — so the key still reports unchecked — the failure is reported through
        /// <c>logError</c> unless the same failure kind was already reported for this engine, a
        /// sink failure is contained by <see cref="TryInvokeSink"/>, and only then is the marker
        /// cleared for a later re-prime.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The status is tested rather than the exception. A CANCELED task carries a null
        /// <see cref="Task.Exception"/>, so a handler keyed on the exception returned early for a
        /// cancellation: nothing was logged, the cache stayed unset, and the in-flight marker stayed
        /// registered, which blocked any re-prime for the rest of the session. When there is no
        /// exception to unwrap a <see cref="TaskCanceledException"/> is synthesized so the sink
        /// always receives one. The faulted path is unchanged and still reports the unwrapped base
        /// exception.
        /// </para>
        /// <para>
        /// The sink call is guarded (issue #947) through <see cref="TryInvokeSink"/>, the guard
        /// this type uses at every sink call site (issue #964). The sink is the last reporting
        /// channel of this type, so a failure inside it has nowhere else to go; letting it escape
        /// skipped the clear below, which left a stale marker that blocked every later re-prime,
        /// and faulted the discarded continuation unobserved. With the sink contained, the
        /// continuation in <see cref="StartObservedPrime"/> has no remaining throw source of its
        /// own, so it completes rather than faulting.
        /// </para>
        /// <para>
        /// Repeat suppression (issue #948): each pair of engine key and base-exception type is
        /// reported once, then recorded in <see cref="_reportedPrimeFaults"/> by the only
        /// statement of the branch taken when <see cref="TryInvokeSink"/> reports that the sink
        /// returned normally, so a sink that throws leaves the report owed. Moving that record
        /// before the sink call, or out of that branch, suppresses it for the session.
        /// </para>
        /// </remarks>
        private void CompletePrime(Task completed, string engineName)
        {
            if (completed.Status == TaskStatus.RanToCompletion)
            {
                return;
            }

            var failure =
                (Exception)completed.Exception?.GetBaseException()
                ?? new TaskCanceledException(completed);

            // Report-then-clear is load-bearing: the marker stays registered until the
            // report (if any) has returned or thrown, so a caller that observes the marker absent,
            // including one that fetched the prime handle after the fault, is guaranteed the report
            // has already been attempted or was deliberately skipped as an already reported kind.
            var reportKey = (EngineName: engineName, FaultType: failure.GetType());
            if (!_reportedPrimeFaults.ContainsKey(reportKey))
            {
                if (
                    TryInvokeSink(
                        () => _logError(BuildPrimeFailedMessage(engineName), failure),
                        out _
                    )
                )
                {
                    _reportedPrimeFaults[reportKey] = 0;
                }
            }

            _primeTasks.TryRemove(engineName, out _);
        }
    }
}
