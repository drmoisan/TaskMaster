using System;
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
    /// <see cref="UiThreadDispatcherTransaction.Dispose"/>. Lock ordering is <c>TransactionGate</c>
    /// then <c>FieldLock</c>, never the reverse, so no cycle and therefore no deadlock exists.
    /// </para>
    /// <para>
    /// <see cref="EnsureDispatcher"/> deliberately never acquires <c>TransactionGate</c>. Callers of
    /// the <c>QfcItemControllerTestSupport.EnsureUiThreadDispatcher</c> wrapper live in test files
    /// that carry no <c>[Timeout]</c>, so making them wait on a gate another test class holds for a
    /// whole test body would convert a bounded failure elsewhere into an unbounded hang there.
    /// </para>
    /// </summary>
    internal static class UiThreadDispatcherFixture
    {
        private static readonly object FieldLock = new object();
        private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1);
        private static readonly object ParkedDispatcherLock = new object();
        private static readonly FieldInfo DispatcherField = ResolveDispatcherField();
        private static Dispatcher _parkedDispatcher = null;

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
            TransactionGate.Release();
        }

        /// <summary>
        /// Seeds the static with the parked dispatcher only when it is currently <c>null</c>, and
        /// returns a scope whose <c>Dispose</c> conditionally reverts that seeding. Never acquires
        /// <c>TransactionGate</c> and never blocks on anything a caller must release. Disposing the
        /// returned scope is optional: a discarded scope leaks exactly as the pre-fix helper did.
        /// </summary>
        internal static IDisposable EnsureDispatcher()
        {
            // Obtained before FieldLock is taken: GetParkedDispatcher starts a thread and waits on a
            // ManualResetEventSlim, which would falsify FieldLock's "straight-line, no waits" property.
            Dispatcher parked = GetParkedDispatcher();

            lock (FieldLock)
            {
                if (DispatcherField.GetValue(null) == null)
                {
                    DispatcherField.SetValue(null, parked);
                    return new EnsureScope(parked);
                }
            }

            return new EnsureScope(null);
        }

        /// <summary>
        /// Acquires <c>TransactionGate</c> and returns a transaction that has not installed anything
        /// yet. The two-phase shape is deliberate: consumers acquire the gate at fixture-build start,
        /// well before the install, which preserves the issue #230 hold window.
        /// </summary>
        internal static async Task<UiThreadDispatcherTransaction> BeginTransactionAsync()
        {
            await TransactionGate.WaitAsync().ConfigureAwait(false);
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
        /// The scope returned by <see cref="EnsureDispatcher"/>. Reverts the seeding only when the
        /// static still holds the exact instance this scope installed. A scope that installed nothing
        /// carries <c>null</c> and is a no-op, which is what keeps a discarded scope from clobbering a
        /// value some other owner installed in the meantime.
        /// </summary>
        private sealed class EnsureScope : IDisposable
        {
            private readonly Dispatcher _installed;
            private bool _disposed = false;

            internal EnsureScope(Dispatcher installed)
            {
                _installed = installed;
                _disposed = false;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                if (_installed != null)
                {
                    UiThreadDispatcherFixture.CompareExchange(_installed, null);
                }
            }
        }
    }

    /// <summary>
    /// A single install-to-restore transaction over the process-wide static
    /// <c>UtilitiesCS.UiThread._dispatcher</c>, holding <c>TransactionGate</c> for its whole lifetime.
    /// Obtained from <see cref="UiThreadDispatcherFixture.BeginTransactionAsync"/> and released by
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
