# Fail-Before Exception Dossier — C10 and C02 (P4-T9, SD13)

Timestamp: 2026-09-05T22-42

Command:

```powershell
git show pre-782-base:UtilitiesCS.Test/Threading/UiThread_Tests.cs
git show pre-782-base:UtilitiesCS/Threading/UiThread.cs
```

EXIT_CODE: 0

Output Summary:

Neither C10 nor C02 yields a deterministic in-suite failing test. A failing run is therefore
recorded as structurally impossible rather than asserted, and this dossier supplies the alternative
proof in its place. Both pre-change extracts below were read directly out of the `pre-782-base`
commit through `git show`, so they are the committed text rather than a recollection of it.

WhyFailingRunImpossible:

C10's hazard is a leaked, never-shut dispatcher created on a pooled MTA worker. It manifests only
when a later test scheduled onto that same pooled thread resolves `Dispatcher.FromThread` and gets
the leaked instance instead of null. Reproducing it requires controlling which test runs after which
on which pooled thread, which is order dependence, and the General Unit Test Policy requires that
tests run in any order without affecting each other. A test that only fails in one ordering would
violate the policy it is written to protect.

C02's hazard is a torn double read of a non-volatile static: the pre-change getter tested
`_dispatcher` for null and then returned `_dispatcher` again, so a concurrent writer completing
`Init()` between the two reads could make the guard pass while the return value differs from the
value the guard inspected. Forcing that interleaving requires a timing construct — a sleep, a spin,
or a wall-clock wait — inside the test. The same policy prohibits `Thread.Sleep`, `Task.Delay`, and
real wall-clock waits in test code, and `BannedSymbols.txt` names the first two directly.

SearchScope: `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/`
SearchPatterns: `p4-t7-fail-before.md`, `p4-t8-pass-after.md`, `fail-before-exception.*.md`
SearchResult: `p4-t7-fail-before.md` and `p4-t8-pass-after.md` exist and cover the three AC7 tests
only; no failing run exists for C10 or C02, which is what this dossier records.

## Alternative proof — C10

### Pre-change source, `UtilitiesCS.Test/Threading/UiThread_Tests.cs` at `pre-782-base`

`Dispatcher.CurrentDispatcher` is resolved at line 166, inside a plain `[TestMethod]` running on a
pooled MSTest worker. There is no `BeginInvokeShutdown`, no thread join, and no disposal: the
dispatcher the call creates is affinitized to the pooled thread and outlives the test.

```csharp
        [TestMethod]
        public void Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance()
        {
            // Arrange
            var field = DispatcherField();
            var prior = field.GetValue(null);
            var expected = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            field.SetValue(null, expected);
            try
            {
                // Act / Assert
                UiThread.Dispatcher.Should().BeSameAs(expected);
            }
            finally
            {
                field.SetValue(null, prior);
            }
        }
```

### Post-change source, same file on the delivered tree

The sentinel now comes from a dedicated STA thread owned by a disposable host. The host is
constructed inside a `using` statement, so `BeginInvokeShutdown` and the join run on every exit
path, including a failing assertion, and nothing is left affinitized to the pooled worker.

```csharp
        [TestMethod]
        public void Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance()
        {
            // Arrange: establish a known null prior explicitly rather than relying on the ambient
            // value. QfcHomeControllerRunAsyncTests calls UiThread.Init(false), which populates the
            // same process-global static, and QuickFiler.Test and UtilitiesCS.Test run in a single
            // vstest invocation, so an ambient non-null prior would be restored by the inner
            // disposal and the round-trip assertion below would fail for a reason outside this
            // delivery.
            using (UiThreadDispatcherScope.InstallNull())
            using (var host = new StaDispatcherHost())
            {
                var expected = host.Dispatcher;

                using (UiThreadDispatcherScope.Install(expected))
                {
                    // Act / Assert
                    UiThread.Dispatcher.Should().BeSameAs(expected);
                }

                // Assert: the inner scope restored the null prior it captured.
                UiThreadDispatcherScope.Current.Should().BeNull();
            }
        }
```

```csharp
        private sealed class StaDispatcherHost : IDisposable
        {
            private readonly AutoResetEvent _ready = new AutoResetEvent(false);
            private readonly Thread _thread;

            public StaDispatcherHost()
            {
                _thread = new Thread(() =>
                {
                    Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                    _ready.Set();
                    System.Windows.Threading.Dispatcher.Run();
                });
                _thread.IsBackground = true;
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Start();
                _ready.WaitOne();
            }

            public Dispatcher Dispatcher { get; private set; }

            public void Dispose()
            {
                Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                _thread.Join();
                _ready.Dispose();
            }
        }
```

The difference is observable by reading rather than by running: the pre-change text contains no
shutdown call at all, and the post-change text contains one on a guaranteed path.

## Alternative proof — C02

### Pre-change getter, `UtilitiesCS/Threading/UiThread.cs` at `pre-782-base`

Two separate reads of the non-volatile static: the guard reads it at line 139 and the return
statement reads it again at line 145.

```csharp
        public static Dispatcher Dispatcher
        {
            get
            {
                if (_dispatcher is null)
                {
                    throw new InvalidOperationException(
                        "The UI dispatcher has not been captured. Call UiThread.Init() so that UiThread.Initialize() runs before reading UiThread.Dispatcher."
                    );
                }
                return _dispatcher;
            }
            private set => _dispatcher = value;
        }
```

### Post-change getter, same file on the delivered tree

One read into a local, which both the guard and the return statement then use. No interleaving can
make the two disagree, because there is only one.

```csharp
        public static Dispatcher Dispatcher
        {
            get
            {
                // Read the non-volatile static exactly once so the guard and the return value
                // cannot observe different values if another thread completes Init() in between.
                Dispatcher? captured = _dispatcher;
                if (captured is null)
                {
                    // Initialize() constructs and shows a hidden WinForms SyncContextForm, so it
                    // has UI-thread affinity. A lazy Init() from an arbitrary reader is therefore
                    // deliberately avoided here even though the sibling UiSyncContext and
                    // AutoScaleFactor accessors do self-heal.
                    throw new InvalidOperationException(DispatcherNotInitializedMessage);
                }
                return captured;
            }
            private set => _dispatcher = value;
        }
```

The read count is the whole of the property: two field reads before, one after. That is decidable
from the text and needs no interleaving to demonstrate, which is precisely why no failing test is
recorded for it.
