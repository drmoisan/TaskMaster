using System;

namespace UtilitiesCS.Threading
{
    /// <summary>
    /// A single-writer/single-reader ambient holder for the identity of the Outlook store
    /// currently being processed on the STA/UI thread (issue #264, epic #260). The STA thread is
    /// the only writer (it opens a scope around each per-store COM sequence); the
    /// <see cref="ThreadMonitor"/> watchdog's background thread is the only reader (it reads
    /// <see cref="Current"/> when it confirms a UI lockup, to attribute the stall to a store).
    /// </summary>
    /// <remarks>
    /// Backed by a <c>volatile string</c> field, which gives the required cross-thread visibility
    /// for a single reference read/write without a lock. This is deliberately NOT an
    /// <c>AsyncLocal</c>: an <c>AsyncLocal</c> value flows along one logical async/await chain and
    /// would not be observed by the watchdog's independent <c>Task.Run</c> loop, which never awaited
    /// anything from the STA thread's synchronous COM sequence (research §3.1). The type is
    /// host-neutral: it performs no COM access, reads no clock, and uses no threading primitive
    /// beyond <c>volatile</c>.
    /// </remarks>
    public static class CurrentStoreContext
    {
        private static volatile string _current;

        /// <summary>
        /// The identity of the store currently being processed, or <see langword="null"/> when no
        /// per-store scope is active ("no context"). This is a plain in-memory field read: no COM,
        /// no blocking. Safe to call from the watchdog's background thread.
        /// </summary>
        public static string Current => _current;

        /// <summary>
        /// Opens an ambient scope naming the store currently being processed. Captures the previous
        /// value, sets <see cref="Current"/> to the normalized identity, and returns an
        /// <see cref="IDisposable"/> that restores the previous value on <see cref="IDisposable.Dispose"/>.
        /// A <see langword="null"/>, whitespace, or <c>"&lt;unavailable&gt;"</c> identity normalizes to
        /// <see langword="null"/> ("no context"), so an unresolved store never produces a false
        /// attribution. Intended for use with a <c>using</c> statement so the scope is disposed even
        /// when the wrapped COM call throws.
        /// </summary>
        /// <param name="storeIdentity">
        /// The already-cached store identity (typically <c>DisplayName</c>). No COM read is performed.
        /// </param>
        /// <returns>An <see cref="IDisposable"/> that restores the previous ambient value on dispose.</returns>
        public static IDisposable Begin(string storeIdentity)
        {
            var previous = _current;
            _current = Normalize(storeIdentity);
            return new Scope(previous);
        }

        private static string Normalize(string storeIdentity)
        {
            if (string.IsNullOrWhiteSpace(storeIdentity))
            {
                return null;
            }

            if (string.Equals(storeIdentity, "<unavailable>", StringComparison.Ordinal))
            {
                return null;
            }

            return storeIdentity;
        }

        private sealed class Scope : IDisposable
        {
            private readonly string _previous;
            private bool _disposed;

            internal Scope(string previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _current = _previous;
            }
        }
    }
}
