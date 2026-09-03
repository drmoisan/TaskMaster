using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TaskMaster
{
    /// <summary>
    /// The versioned last-known-activation-state cache behind the engine toggle checkboxes: a
    /// monotonic observation ticket source plus a compare-and-apply store that refuses a write
    /// whose observation began earlier than one already recorded.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The freshness of a cached activation value is determined by when its underlying read BEGAN,
    /// not by when its write lands. Two writers exist — the user-initiated toggle path and the lazy
    /// prime — and they overlap trivially, because the prime is started from a cache miss during a
    /// ribbon paint while the toggle is started from a click. Completion order does not track
    /// observation order, so a writer that stores unconditionally can overwrite a newer value with
    /// stale data and leave the checkbox displaying the opposite of the engine's real state. Each
    /// writer therefore takes a ticket immediately before its read and stores through
    /// <see cref="TryApplyState"/>, which applies the write only when no newer observation is
    /// already cached for that key.
    /// </para>
    /// <para>
    /// Extracted from <see cref="EngineToggleStateCoordinator"/> so that type stays within the
    /// repository's 500-line file ceiling. The coordinator is the only consumer.
    /// </para>
    /// <para>
    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
    /// decision logic with no COM, no <c>Microsoft.Office.*</c> reference, no WinForms type and no
    /// logger reference, and is fully unit-tested.
    /// </para>
    /// </remarks>
    internal sealed class EngineTogglePressedStateCache
    {
        /// <summary>
        /// Monotonic ticket source for activation observations. Read and written only through
        /// <see cref="Interlocked"/>.
        /// </summary>
        private long _stateSequence;

        /// <summary>
        /// Last-known activation state per engine key, each stamped with the ticket of the read
        /// that produced it. A key absent from this map has never been primed successfully.
        /// </summary>
        private readonly ConcurrentDictionary<string, PressedState> _pressedState =
            new ConcurrentDictionary<string, PressedState>(StringComparer.Ordinal);

        /// <summary>
        /// Issues the next monotonic observation ticket.
        /// </summary>
        /// <returns>A ticket strictly greater than every ticket previously issued.</returns>
        /// <remarks>
        /// A single process-wide counter is sufficient even though the cache is per-key, because
        /// tickets are only ever compared within a key.
        /// </remarks>
        internal long NextSequence() => Interlocked.Increment(ref _stateSequence);

        /// <summary>
        /// Reads the cached activation state for an engine key.
        /// </summary>
        /// <param name="engineName">The engine key; ordinal, case-sensitive.</param>
        /// <param name="active">The cached activation state, or <see langword="false"/> when the
        /// key has no cached observation.</param>
        /// <returns><see langword="true"/> when a cached observation exists for the key.</returns>
        /// <remarks>
        /// A dictionary read only. This never awaits, never blocks and never throws, which is what
        /// lets the coordinator answer Office's synchronous <c>getPressed</c> poll from it.
        /// </remarks>
        internal bool TryGetActive(string engineName, out bool active)
        {
            if (_pressedState.TryGetValue(engineName, out var cached))
            {
                active = cached.Active;
                return true;
            }

            active = false;
            return false;
        }

        /// <summary>
        /// Stores an observation only when no newer observation is already cached for the key.
        /// </summary>
        /// <param name="engineName">The engine key; ordinal, case-sensitive.</param>
        /// <param name="active">The observed activation state.</param>
        /// <param name="sequence">The ticket taken before the observation began.</param>
        /// <returns>
        /// <see langword="true"/> when the write was applied, so the caller can invalidate the
        /// control only on a real change.
        /// </returns>
        /// <remarks>
        /// An explicit compare-and-swap loop is used rather than an add-or-update factory, because
        /// such a factory may run more than once under contention, which makes "did my write land?"
        /// non-obvious to a reader. The loop terminates: each iteration either returns or observes
        /// a strictly newer stored ticket.
        /// </remarks>
        internal bool TryApplyState(string engineName, bool active, long sequence)
        {
            while (true)
            {
                if (!_pressedState.TryGetValue(engineName, out var existing))
                {
                    if (_pressedState.TryAdd(engineName, new PressedState(active, sequence)))
                    {
                        return true;
                    }

                    continue;
                }

                if (existing.Sequence >= sequence)
                {
                    return false;
                }

                if (
                    _pressedState.TryUpdate(
                        engineName,
                        new PressedState(active, sequence),
                        existing
                    )
                )
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// One cached activation observation: the value, plus the ticket of the read that produced
        /// it.
        /// </summary>
        /// <remarks>
        /// Deliberately a reference type. <see cref="ConcurrentDictionary{TKey, TValue}.TryUpdate"/>
        /// compares the supplied comparand with the stored value, and for a reference type with no
        /// equality override that comparison is reference identity — exactly the compare-and-swap
        /// semantic <see cref="TryApplyState"/> needs. A value tuple would be compared structurally,
        /// so an unrelated writer that happened to store an equal value would satisfy the comparand
        /// check and the guard would silently weaken to "the value looked the same".
        /// </remarks>
        private sealed class PressedState
        {
            internal PressedState(bool active, long sequence)
            {
                Active = active;
                Sequence = sequence;
            }

            /// <summary>The observed activation state.</summary>
            internal bool Active { get; }

            /// <summary>The monotonic ticket taken before the observation began.</summary>
            internal long Sequence { get; }
        }
    }
}
