using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Single owner of every mutation of the process-wide static <c>UtilitiesCS.UiThread._dispatcher</c>
    /// made from this test assembly's owned files (issue #493).
    /// <para>
    /// Two distinct locks guard two distinct concerns. <c>FieldLock</c> makes one read-modify-write of
    /// the static atomic and is held only for a straight-line region with no wait, no thread creation,
    /// and no await inside it. <c>TransactionGate</c> provides mutual exclusion between long
    /// install-to-restore transactions and is held from transaction start until
    /// <see cref="UiThreadDispatcherTransaction.Dispose"/>; acquisition is bounded by
    /// <see cref="TransactionGateAcquireTimeoutMs"/> and throws <see cref="TimeoutException"/> on
    /// expiry. Lock ordering is <c>TransactionGate</c>
    /// then <c>FieldLock</c>, never the reverse, so no cycle and therefore no deadlock exists.
    /// </para>
    /// <para>
    /// <see cref="EnsureDispatcher"/> deliberately never acquires <c>TransactionGate</c>. Callers of
    /// the <c>QfcItemControllerTestSupport.EnsureUiThreadDispatcher</c> wrapper live in test files
    /// that carry no <c>[Timeout]</c>, so making them wait on a gate another test class holds for a
    /// whole test body would convert a bounded failure elsewhere into an unbounded hang there.
    /// </para>
    /// <para>
    /// Issue #968: ensure pins are reference counted. A pin counter and an install-ownership flag
    /// live under <c>FieldLock</c>. The first pin on a <c>null</c> field seeds the parked dispatcher
    /// and sets the flag; the last release writes <c>null</c> back only when the flag is set and the
    /// field still holds the parked instance, then clears the flag. A discarded scope therefore
    /// pins for the process lifetime and leaves the parked dispatcher installed, so every caller
    /// disposes its scope, and every pin is acquired and released while its caller holds a
    /// transaction, which keeps the count at zero whenever a transaction is acquired. Residual: a
    /// transaction that installs over a pinned parked value and restores it after the last pin
    /// released leaves the parked value installed with zero pins and the flag set; the next pin
    /// cycle reverts it. No test in this assembly installs over a pinned value, so the residual is
    /// documented rather than exercised.
    /// </para>
    /// </summary>
    internal static class UiThreadDispatcherFixture
    {
        private static readonly object FieldLock = new object();
        private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1);
        private static readonly object ParkedDispatcherLock = new object();
        private static readonly FieldInfo DispatcherField = ResolveDispatcherField();
        private static Dispatcher _parkedDispatcher = null;

        // Issue #968: the count of live ensure scopes and whether the fixture itself seeded the parked
        // dispatcher into a null field. Both are read and written only while FieldLock is held.
        private static int _pinCount;
        private static bool _fixtureInstalledParked;

        // Issue #743 AC1 observable: three monotonic counters over TransactionGate. A contended
        // acquisition is one that observed CurrentCount == 0 immediately before waiting. In a serial
        // run no live holder can exist when a test begins its transaction, so a non-zero contended
        // count there can only come from a leaked or late-released transaction.
        private static int _transactionAcquisitions = 0;
        private static int _transactionReleases = 0;
        private static int _contendedAcquisitions = 0;

        /// <summary>Monotonic count of completed <c>TransactionGate</c> acquisitions.</summary>
        internal static int TransactionAcquisitions => Volatile.Read(ref _transactionAcquisitions);

        /// <summary>Monotonic count of <c>TransactionGate</c> releases.</summary>
        internal static int TransactionReleases => Volatile.Read(ref _transactionReleases);

        /// <summary>
        /// Monotonic count of acquisitions that found the permit held immediately before waiting.
        /// </summary>
        internal static int ContendedAcquisitions => Volatile.Read(ref _contendedAcquisitions);

        /// <summary>
        /// Reads the current value of the static under <c>FieldLock</c>. Test observation only.
        /// </summary>
        internal static Dispatcher Current
        {
            get
            {
                lock (FieldLock)
                {
                    return (Dispatcher)DispatcherField.GetValue(null);
                }
            }
        }

        /// <summary>
        /// Atomically reads the previous value of the static, writes <paramref name="replacement"/>,
        /// and returns the previous value. Straight-line under <c>FieldLock</c>.
        /// </summary>
        internal static Dispatcher Exchange(Dispatcher replacement)
        {
            lock (FieldLock)
            {
                var previous = (Dispatcher)DispatcherField.GetValue(null);
                DispatcherField.SetValue(null, replacement);
                return previous;
            }
        }

        /// <summary>
        /// Writes <paramref name="restoreTo"/> only when the static still holds the exact instance
        /// <paramref name="expected"/>, and reports whether the write happened. A restore that finds
        /// a newer owner's value in place is skipped rather than clobbering it.
        /// </summary>
        internal static bool CompareExchange(Dispatcher expected, Dispatcher restoreTo)
        {
            lock (FieldLock)
            {
                if (!ReferenceEquals(DispatcherField.GetValue(null), expected))
                {
                    return false;
                }

                DispatcherField.SetValue(null, restoreTo);
                return true;
            }
        }

        /// <summary>
        /// Releases one <c>TransactionGate</c> permit. Called only by
        /// <see cref="UiThreadDispatcherTransaction.Dispose"/>, and only once per transaction.
        /// </summary>
        internal static void ReleaseTransactionGate()
        {
            Interlocked.Increment(ref _transactionReleases);
            TransactionGate.Release();
        }

        /// <summary>
        /// Takes one counted pin on the shared static (issue #968). The first pin on a <c>null</c>
        /// field seeds the parked dispatcher and records that the fixture owns the seeding; a pin
        /// taken while the field is non-null installs nothing. Disposing the returned scope releases
        /// the pin, and the field reverts to <c>null</c> only on the last release, only when the
        /// fixture owns the seeding, and only when the field still holds the parked instance. Never
        /// acquires <c>TransactionGate</c> and never blocks on anything a caller must release. A
        /// discarded scope pins for the process lifetime, so every caller disposes its scope.
        /// </summary>
        internal static IDisposable EnsureDispatcher()
        {
            // Obtained before FieldLock is taken: GetParkedDispatcher starts a thread and waits on a
            // ManualResetEventSlim, which would falsify FieldLock's "straight-line, no waits" property.
            Dispatcher parked = GetParkedDispatcher();

            lock (FieldLock)
            {
                _pinCount++;
                if (DispatcherField.GetValue(null) == null)
                {
                    DispatcherField.SetValue(null, parked);
                    _fixtureInstalledParked = true;
                }
            }

            return new EnsureScope(parked);
        }

        /// <summary>
        /// Upper bound on a <c>TransactionGate</c> acquisition through the parameterless
        /// <see cref="BeginTransactionAsync()"/> overload (issue #882): twice the 60000 ms MSTest
        /// timeout that bounds the longest legitimate hold, and half the four-minute runner hang
        /// guard, so an expired bound is reported as a named failure rather than as a hang dump.
        /// </summary>
        internal const int TransactionGateAcquireTimeoutMs = 120000;

        /// <summary>
        /// Acquires <c>TransactionGate</c> with the production bound and returns a transaction that
        /// has not installed anything yet. The two-phase shape is deliberate: consumers acquire the
        /// gate at fixture-build start, well before the install, which preserves the issue #230 hold
        /// window. Throws <see cref="TimeoutException"/> when the permit is not obtained within
        /// <see cref="TransactionGateAcquireTimeoutMs"/>; no transaction exists on that path.
        /// </summary>
        internal static Task<UiThreadDispatcherTransaction> BeginTransactionAsync()
        {
            return BeginTransactionAsync(
                TimeSpan.FromMilliseconds(TransactionGateAcquireTimeoutMs)
            );
        }

        /// <summary>
        /// Bounded acquisition (issue #882). Tests supply <see cref="TimeSpan.Zero"/> to observe the
        /// failure branch deterministically while they hold the permit. On failure the method throws
        /// before any <see cref="UiThreadDispatcherTransaction"/> exists and without touching the
        /// acquisitions or releases counter, so there is no release to omit; the contended pre-check
        /// stays before the wait because a failed probe did observe a held permit.
        /// </summary>
        internal static async Task<UiThreadDispatcherTransaction> BeginTransactionAsync(
            TimeSpan bound
        )
        {
            if (TransactionGate.CurrentCount == 0)
            {
                Interlocked.Increment(ref _contendedAcquisitions);
            }

            bool acquired = await TransactionGate.WaitAsync(bound).ConfigureAwait(false);
            if (!acquired)
            {
                throw new TimeoutException(
                    "TRANSACTIONGATE_ACQUIRE_TIMEOUT: UiThreadDispatcherFixture.TransactionGate was not acquired within "
                        + bound.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture)
                        + " ms. The probable cause is a permit held by a test the runner has already reported as finished (issue #882)."
                );
            }

            Interlocked.Increment(ref _transactionAcquisitions);
            return new UiThreadDispatcherTransaction();
        }

        /// <summary>
        /// Resolves and caches the private static backing field of <c>UiThread.Dispatcher</c>,
        /// asserting that it exists. Preserves the intent of the pre-change assertion in
        /// <c>QfcItemControllerTestSupport.EnsureUiThreadDispatcher</c>.
        /// </summary>
        private static FieldInfo ResolveDispatcherField()
        {
            FieldInfo field = typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            field.Should().NotBeNull(because: "UiThread._dispatcher backing field must exist");
            return field;
        }

        /// <summary>
        /// Lazily creates a single dispatcher hosted on a background thread that grabs its dispatcher
        /// and then parks indefinitely without ever running a dispatcher frame, so any operation posted
        /// to it stays queued and never executes. The thread is a background thread reclaimed at process
        /// exit; no message loop, WinForms form, or timing dependency is created.
        /// </summary>
        private static Dispatcher GetParkedDispatcher()
        {
            lock (ParkedDispatcherLock)
            {
                if (_parkedDispatcher == null)
                {
                    using (var ready = new ManualResetEventSlim(false))
                    {
                        // Parked forever; keeps the thread (and its dispatcher) alive without pumping.
                        var park = new ManualResetEventSlim(false);
                        var thread = new Thread(() =>
                        {
                            _parkedDispatcher = Dispatcher.CurrentDispatcher;
                            ready.Set();
                            park.Wait();
                        })
                        {
                            IsBackground = true,
                            Name = "UiThreadDispatcherFixture.ParkedDispatcher",
                        };
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        ready.Wait();
                    }
                }

                return _parkedDispatcher;
            }
        }

        /// <summary>
        /// The scope returned by <see cref="EnsureDispatcher"/>: one counted pin. Disposal is
        /// idempotent and performs the decrement and the conditional revert inline in one
        /// <c>FieldLock</c> critical section, so no other pin can interleave between them. The revert
        /// writes <c>null</c> only when this release brings the count to zero, the fixture itself
        /// seeded the parked dispatcher, and the field still holds that instance; a value some other
        /// owner installed in the meantime is left in place.
        /// </summary>
        private sealed class EnsureScope : IDisposable
        {
            private readonly Dispatcher _parked;
            private bool _disposed = false;

            internal EnsureScope(Dispatcher parked)
            {
                _parked = parked;
                _disposed = false;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                lock (FieldLock)
                {
                    _pinCount--;
                    if (
                        _pinCount == 0
                        && _fixtureInstalledParked
                        && ReferenceEquals(DispatcherField.GetValue(null), _parked)
                    )
                    {
                        DispatcherField.SetValue(null, null);
                        _fixtureInstalledParked = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// A single install-to-restore transaction over the process-wide static
    /// <c>UtilitiesCS.UiThread._dispatcher</c>, holding <c>TransactionGate</c> for its whole lifetime.
    /// Obtained from <see cref="UiThreadDispatcherFixture.BeginTransactionAsync()"/> and released by
    /// <see cref="Dispose"/>, which restores strictly before it releases the gate so a waiter can
    /// never observe the pre-restore value.
    /// </summary>
    internal sealed class UiThreadDispatcherTransaction : IDisposable
    {
        private Dispatcher _previous;
        private Dispatcher _installedValue;
        private bool _hasInstalled;
        private bool _disposed;

        internal UiThreadDispatcherTransaction()
        {
            _previous = null;
            _installedValue = null;
            _hasInstalled = false;
            _disposed = false;
        }

        /// <summary>
        /// Captures the previous value of the static and writes <paramref name="replacement"/>,
        /// atomically. <paramref name="replacement"/> may be <c>null</c>. Throws
        /// <see cref="InvalidOperationException"/> when called a second time on the same transaction,
        /// because a second install would discard the captured previous value and make the restore
        /// unsound.
        /// </summary>
        internal void Install(Dispatcher replacement)
        {
            if (_hasInstalled)
            {
                throw new InvalidOperationException(
                    "UiThreadDispatcherTransaction.Install has already been called on this transaction."
                );
            }

            _hasInstalled = true;
            _previous = UiThreadDispatcherFixture.Exchange(replacement);
            _installedValue = replacement;
        }

        /// <summary>
        /// Conditionally restores the captured previous value, then releases <c>TransactionGate</c>.
        /// Idempotent: a second call neither re-writes the static nor releases the gate again, because
        /// a second release on a <c>SemaphoreSlim(1, 1)</c> throws <c>SemaphoreFullException</c>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_hasInstalled)
            {
                UiThreadDispatcherFixture.CompareExchange(_installedValue, _previous);
            }

            UiThreadDispatcherFixture.ReleaseTransactionGate();
        }
    }
}
