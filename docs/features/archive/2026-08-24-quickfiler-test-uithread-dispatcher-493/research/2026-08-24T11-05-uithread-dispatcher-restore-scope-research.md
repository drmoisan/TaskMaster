# Research — UiThread dispatcher static swap has no restore (Issue #493)

- Date: 2026-08-24T11-05
- Feature: `docs/features/active/quickfiler-test-uithread-dispatcher-493/`
- Issue: #493 (`uithread-dispatcher-static-swap-no-restore`), child of the `quickfiler-bug-family` epic
- Work mode: full-bug
- Scope of this document: research only. No production or test code, spec, or plan content is authored here.

---

## 0. Executive summary

| Question | Verdict |
| --- | --- |
| Does `UtilitiesCS/Threading/UiThread.cs` need to change? | **No.** See §6. The fix is entirely inside the test assembly, and a `UiThread` change would additionally require a new `InternalsVisibleTo("QuickFiler.Test")` grant on a production assembly while solving nothing the test-side design does not already solve. |
| Is the orchestrator's design hypothesis sound? | **Sound in intent, but it has one hole** that must be closed: if `EnsureUiThreadDispatcher` acquires the *same* long-lived transaction gate the Swap/PumpHarness path holds, it can (a) block a non-`[Timeout]`-bounded, unowned test for up to 60 s, (b) hang that test permanently if a pump test dies before its restore, and (c) self-deadlock the regression tests. §2 replaces the single gate with a **two-lock** design that keeps every stated property and removes all three failure modes. |
| Can `EnsureUiThreadDispatcher` return `IDisposable` without touching `FocusAndThemeTests.cs`? | **Yes.** Source-compatible, and analyzer-safe in this repo (§3). |
| Is the injectable-seam alternative warranted now? | **No — defer.** ~62 references across 29 production files (§7). |
| File-size headroom | Sufficient, but only with the layout in §8 (two new files). `FocusAndThemeTests.cs` is at 497/500 lines — a further reason it cannot absorb any change. |

---

## 1. Confirmed current state

### 1.1 The defective helper

`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:238-249`:

```csharp
internal static void EnsureUiThreadDispatcher()
{
    FieldInfo field = typeof(UiThread).GetField(
        "_dispatcher",
        BindingFlags.NonPublic | BindingFlags.Static
    );
    field.Should().NotBeNull(because: "UiThread._dispatcher backing field must exist");
    if (field.GetValue(null) == null)
    {
        field.SetValue(null, GetDedicatedDispatcher());
    }
}
```

Confirmed properties:

- Return type is `void`; there is no restore path anywhere in the file.
- The write is conditional on the field currently being `null` — it never overwrites a live value.
- The seeded value comes from `GetDedicatedDispatcher()` (`...TestSupport.cs:257-285`), a lazily created singleton `Dispatcher` on an STA background thread that sets `Dispatcher.CurrentDispatcher`, signals, then blocks forever on `park.Wait()` (line 270). It **never runs a dispatcher frame**, so any `InvokeAsync`/`BeginInvoke` posted to it is enqueued and never completes.
- `GetDedicatedDispatcher` is private and has exactly one caller (line 247) — verified by repo-wide grep. It is therefore movable.
- The read-modify-write at lines 245-248 is a plain check-then-act with **no synchronization at all**.

### 1.2 The static it mutates

`UtilitiesCS/Threading/UiThread.cs:135-140`:

```csharp
public static Dispatcher Dispatcher
{
    get => _dispatcher;
    private set => _dispatcher = value;
}
private static Dispatcher _dispatcher = null!; // set in Initialize() before any access
```

- The getter is a plain field read with **no lazy `Init()` fallback** (contrast `UiSyncContext` at lines 113-125 and `AutoScaleFactor` at 147-158, both of which call `Init()`, which shows a `SyncContextForm` — never acceptable in a unit test). So a null `_dispatcher` surfaces as a `NullReferenceException` at the consumer, not as a hang and not as form creation.
- The setter is `private`, so a test can only write the value by reflection on `_dispatcher`.
- File carries `#nullable enable` (line 1).

Consumers that make the value load-bearing for the two unowned call sites: `UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs:218` (`UiThread.Dispatcher.InvokeAsync(...)` on the `async: true` branch) and `UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs:254,277`. Hence: null field → NRE; parked field → queued-but-never-executed (which is exactly what the `async: true` theme tests want); live pumped field → executes.

### 1.3 The already-fixed sibling (#230 local workaround)

`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`:

- `:51` — `private static readonly SemaphoreSlim UiThreadDispatcherGate = new SemaphoreSlim(1, 1);` with the rationale documented at `:36-50` (two classes consume the fixture; class-level parallelization; a restore to the parked dispatcher while the other class awaits produces a `[Timeout]` expiry rather than an assertion failure).
- `:60-77` — `BuildPumpHarnessAsync` acquires the gate **at the start of the build** (`:67`), before any viewer construction, and releases it in a `catch` (`:74`) if the core build throws.
- `:79-141` — `BuildPumpHarnessCoreAsync`; the actual field swap happens **last**, at `:138`, `Dispatcher previousUiThreadDispatcher = SwapUiThreadDispatcher(viewer.UiDispatcher);`. The gate therefore covers build-start → install → test body → restore.
- `:143-158` — `private static Dispatcher SwapUiThreadDispatcher(Dispatcher replacement)`: unconditional reflection read of the previous value, unconditional write of the replacement, returns the previous. This is the duplicated reflection logic the acceptance criteria require to be removed.
- `:306-352` — `PumpHarness`, holding `_previousUiThreadDispatcher` and a `bool _restored`; `Restore()` at `:340-351` is idempotent via the `_restored` guard (`:342-347`), then calls `SwapUiThreadDispatcher(_previousUiThreadDispatcher)`, disposes the token source, and **releases the gate last** (`:350`).

### 1.4 Consumers of the pump fixture

- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:47, 90, 138` — same class (`partial class QfcItemController_InitializationTests`).
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:313, 384` — a **different** `[TestClass]`, calling `QfcItemController_InitializationTests.BuildPumpHarnessAsync` directly, with `harness.Restore()` in `finally` (`:356-359`, `:~420`). This is the second class the gate exists for.
- `[Timeout]` precedent: `QfcItemController.InitializationTests.cs:38` — `internal const int PumpTimeoutMs = 60000;`; `QfcItemController.SeamFactoryTests.cs:293` — `private const int PumpTimeoutMs = 60000;` with the documented rationale "every wait is on a deterministic completion signal; the attribute only converts a genuine deadlock into a test failure instead of a CI hang".

### 1.5 The unowned call site (hard constraint) — confirmed

`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`:

- `:452` and `:468` — `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` as a bare statement, return value discarded.
- Both call sites are inside **synchronous, non-`async`** `[TestMethod]`s: `SetThemeDark_FromNormal_SelectsDarkNormalTheme` (`:447-462`) and `SetThemeLight_FromNormal_SelectsLightNormalTheme` (`:464-478`).
- **Neither method carries `[Timeout]`.** Verified by reading the attribute block of both (`:447-448`, `:464-465`). A blocking wait introduced into `EnsureUiThreadDispatcher` would therefore be *unbounded* in these two tests.
- The file is **497 lines** long, i.e. 3 lines below the 500-line ceiling — it could not absorb an edit even if it were owned.
- Repo-wide grep confirms these are the **only two** call sites of `EnsureUiThreadDispatcher()` outside its own declaration.

### 1.6 Other mutators of the same static (not owned, but they bound the design)

Within `QuickFiler.Test`:

- `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:42-51, 83` — raw reflection swap of `UiThread._dispatcher` to a *running* dispatcher, restored in `finally` (`:83`). It does **not** take any gate. It is not in this feature's owned file set, so it will remain an ungated mutator after this fix (see §9, residual risk R-1).
- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:32-60` — `[DoNotParallelize]` class that snapshots `UiThread.Dispatcher` in `[TestInitialize]` and asserts `current.Should().BeSameAs(_capturedDispatcher)` in `[TestCleanup]`. This is a live cross-class invariant: the fix must not change the field's *steady-state* value between an unrelated test's setup and cleanup any more than today's code does.

Other assemblies mutate the same process-wide static too (`UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:421`, `ProgressTrackerAsync_Tests.cs:137`, `IdleAsyncQueue_Tests.cs:143`), which matters only if a single test host loads both assemblies. Out of scope; noted in §9.

### 1.7 Parallelization reality (materially changes the regression-test design)

- `QuickFiler.Test` has **no** `[assembly: Parallelize(...)]` — grep for `Parallelize` in that project returns only two `[DoNotParallelize]` class attributes (`EmailMoveMonitorTests.cs:22`, `ViewerQueueStaticWrapperTests.cs:11`).
- CI (`.github/workflows/_mstest-coverage.yml:83`) runs `vstest.console.exe $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:...` with **no `/Settings:`** — so in CI this assembly runs sequentially and the race is dormant.
- The repo runsettings files force parallelization on **every** assembly: `TaskMaster.runsettings:4-7` and `scripts/vscode/TaskMaster.cli.runsettings:4-7` both declare `<Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize>`. Local coverage runs therefore activate the race.
- Repository memory `.claude/agent-memory/atomic-executor/project_mstest_donotparallelize_overlaps_parallel_bucket.md` records the measured finding that `[DoNotParallelize]` classes **do overlap** the parallel bucket in this repo's adapter, so `[DoNotParallelize]` is not a mutual-exclusion mechanism here.

**Consequence for the regression test:** it must create its own concurrency (threads/tasks + real synchronization primitives) and must not rely on MSTest scheduling two classes concurrently, because under the CI invocation MSTest will not do so.

### 1.8 Corroborating repository memory

`.claude/agent-memory/atomic-executor/project_uithread_dispatcher_static_swap_race.md` independently records the #230 failure signature: a `[Timeout]` expiry rather than an assertion failure, exactly one failure per swapping class, green in every filtered run. Its stated remedy matches `Part2.cs` as landed.

---

## 2. Recommended design

### 2.1 The hole in the single-gate hypothesis

The hypothesis proposes one shared `SemaphoreSlim`, held long by the Swap/PumpHarness path and acquired briefly by `EnsureUiThreadDispatcher`. Three problems, all traceable to the fact that "briefly acquire a gate someone else holds for a whole test body" is not brief:

1. **Unbounded block in an unowned, un-`[Timeout]`-ed test.** `FocusAndThemeTests.cs:452/468` would block for the full duration of a concurrently running pump test (up to `PumpTimeoutMs` = 60 000 ms).
2. **A pump-test failure becomes a permanent hang elsewhere.** If a pump test expires on its `[Timeout]` before `PumpHarness.Restore()` runs, the gate is never released. Today that only affects the pump classes (which are `[Timeout]`-bounded). Under the single-gate design it would hang `FocusAndThemeTests`, which has no `[Timeout]` — converting a bounded failure into an unbounded hang in a file this feature is forbidden to touch. This is precisely the outcome the delegation's constraint 2 exists to prevent, reached by a different route than the one the constraint names.
3. **The regression tests self-deadlock.** `SemaphoreSlim` is not reentrant. A regression test that holds a transaction (to establish a deterministic field value) and then calls `EnsureUiThreadDispatcher` on the same thread would deadlock on itself. The most valuable regression scenario — "`Ensure` cannot clobber an active transaction's dispatcher" — is exactly this shape.

### 2.2 Two-lock design (recommended)

Separate the two distinct concerns that the single gate was conflating:

| Concern | Primitive | Hold duration | Acquired by |
| --- | --- | --- | --- |
| **Atomicity** of a single read-modify-write of `UiThread._dispatcher` | `private static readonly object FieldLock` (Monitor) | straight-line, no waits inside | *every* mutation path: `EnsureDispatcher`, `Transaction.Install`, `Transaction.Dispose`, `EnsureScope.Dispose` |
| **Mutual exclusion between long transactions** (install → test body → restore) | `private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1)` | build-start → restore (unchanged from today) | `BeginTransactionAsync` / `Transaction.Dispose` only |

`EnsureUiThreadDispatcher` takes **only** `FieldLock`. It never touches `TransactionGate`.

**Lock ordering:** `TransactionGate` → `FieldLock`, never the reverse. `FieldLock` is never held while acquiring or awaiting `TransactionGate`, and nothing inside a `FieldLock` region blocks, allocates a thread, or awaits. There is no cycle, therefore no deadlock.

### 2.3 Why this preserves every property the hypothesis wanted

The #230 mechanism is a *lost update on a check-then-act*, not merely "two long transactions overlapping". Concretely, today:

1. `SwapUiThreadDispatcher` reads previous (`Part2.cs:155`);
2. `EnsureUiThreadDispatcher` reads `null` (`TestSupport.cs:245`);
3. `SwapUiThreadDispatcher` writes the live pump dispatcher (`Part2.cs:156`);
4. `EnsureUiThreadDispatcher` writes the **parked** dispatcher over it (`TestSupport.cs:247`) → the pump fixture's awaits never complete → `[Timeout]`.

`FieldLock` makes steps 1+3 one atomic region and steps 2+4 another, so this interleaving is unrepresentable. The two possible orderings are both benign:

- `Ensure` first: field goes `null` → parked; the transaction then captures `parked` as previous and installs the live dispatcher; on restore the field returns to `parked`. Non-null throughout. No hang.
- Transaction first: the field is already non-null when `Ensure` runs, so `Ensure` does nothing (its install-only-when-null rule, preserved verbatim).

`TransactionGate` continues to do the job it does today — keeping `QfcItemController_InitializationTests` and `QfcItemController_SeamFactoryTests` from interleaving their install/restore pairs — and its hold window is unchanged.

### 2.4 Proposed API

Recommended placement: a new, cohesive, single-responsibility class in a new file (see §8), leaving `QfcItemControllerTestSupport.EnsureUiThreadDispatcher` as a thin delegating wrapper so the unowned call site's fully-qualified name is unchanged.

```csharp
// QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs  (new file)
internal static class UiThreadDispatcherFixture
{
    private static readonly object FieldLock;                       // atomicity of one read-modify-write
    private static readonly SemaphoreSlim TransactionGate;          // = new SemaphoreSlim(1, 1)
    private static readonly FieldInfo DispatcherField;              // typeof(UiThread), "_dispatcher", NonPublic|Static

    /// Test-observation helper: reads the static under FieldLock.
    internal static Dispatcher Current { get; }

    /// Seeds the static with the parked dispatcher only when it is currently null.
    /// Returns a scope whose Dispose conditionally reverts that seeding.
    /// Never acquires TransactionGate; never blocks on anything a caller must release.
    internal static IDisposable EnsureDispatcher();

    /// Acquires TransactionGate and returns a transaction that is not yet installed.
    internal static Task<UiThreadDispatcherTransaction> BeginTransactionAsync();

    /// Moved verbatim from QfcItemControllerTestSupport (parked STA background thread, singleton).
    private static Dispatcher GetDedicatedDispatcher();
}

internal sealed class UiThreadDispatcherTransaction : IDisposable
{
    /// Captures the previous value and writes <paramref name="replacement"/>, atomically.
    /// Throws InvalidOperationException if called twice (fail fast per the repo error-handling rule).
    internal void Install(Dispatcher replacement);

    /// Conditionally restores the captured previous value, then releases TransactionGate.
    /// Idempotent via a _disposed guard, mirroring PumpHarness.Restore's _restored guard.
    public void Dispose();
}
```

and, in the owned file:

```csharp
// QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs  (replaces lines 238-249)
internal static IDisposable EnsureUiThreadDispatcher() => UiThreadDispatcherFixture.EnsureDispatcher();
```

### 2.5 Required semantics (these are the contract, state them in the spec)

1. **`EnsureDispatcher` obtains the parked dispatcher *before* taking `FieldLock`.** `GetDedicatedDispatcher` starts a thread and waits on a `ManualResetEventSlim` (`TestSupport.cs:263-280`); doing that inside `FieldLock` would make the "straight-line, no waits" property false. Creating the singleton unconditionally on first call is harmless (one background thread per process, at most once).
2. **`EnsureDispatcher` installs only when the field is `null`.** Preserved exactly (`TestSupport.cs:245`), which is what makes the unowned call site's behavior unregressed and what makes the design provably unable to clobber a live install.
3. **When no install occurs, return a no-op scope.** Recommended over "restore to the value we observed". Justification: writing back an *earlier-observed* value is only a no-op if nothing has changed in between; if a transaction has since installed a live dispatcher, that write would clobber it — reintroducing the #230 mechanism through the restore path. A no-op scope cannot do harm and is trivially idempotent. Since `EnsureDispatcher` installs only when the previous value was `null`, the *only* value an `EnsureScope` ever needs to restore is `null`, which makes the restore logic a single line and its correctness self-evident.
4. **All restores are conditional (compare-then-write).** `EnsureScope.Dispose` writes `null` only if the field still holds the exact instance it installed (`ReferenceEquals`); `Transaction.Dispose` writes the captured previous only if the field still holds the exact instance it installed. If some other owner has since replaced the value, the restore is skipped, leaving the newer owner's value intact. This trades an exact restore for a bounded leak in a contended edge case — the correct trade, because the alternative is clobbering a live dispatcher, which is the defect being fixed.
5. **`Transaction.Dispose` restores *before* releasing the gate.** Ordering is load-bearing: it is what makes the regression assertion in §5/R4 deterministic (a waiter cannot observe the pre-restore value). `PumpHarness.Restore` already has this ordering (`Part2.cs:348` then `:350`); preserve it.
6. **Idempotency guards on both scope types**, mirroring `PumpHarness._restored` (`Part2.cs:309, 342-347`). A second `Dispose` must not re-write the field and must not call `TransactionGate.Release()` again (a second `Release` on a `SemaphoreSlim(1, 1)` throws `SemaphoreFullException`).

### 2.6 How `InitializationTests.Part2.cs` consumes it

| Current | Change |
| --- | --- |
| `:36-51` gate field + doc comment | **Delete.** Replaced by `UiThreadDispatcherFixture.TransactionGate`. Retain a 2-3 line comment pointing at the fixture so the #230 rationale is not lost. |
| `:60-77` `BuildPumpHarnessAsync` gate acquire + catch-release | Replace `await UiThreadDispatcherGate.WaitAsync()` with `await UiThreadDispatcherFixture.BeginTransactionAsync()`; replace the `catch { UiThreadDispatcherGate.Release(); throw; }` with `catch { transaction.Dispose(); throw; }`. **Keep the acquire at build start** — see below. |
| `:79-141` `BuildPumpHarnessCoreAsync` | Takes the transaction as a parameter; line `:138` becomes `transaction.Install(viewer.UiDispatcher);`. |
| `:143-158` `SwapUiThreadDispatcher` | **Delete.** This is the duplicated reflection logic the acceptance criteria name. |
| `:306-352` `PumpHarness` | Store the `UiThreadDispatcherTransaction` instead of `Dispatcher _previousUiThreadDispatcher`; `Restore()` keeps its `_restored` guard and calls `transaction.Dispose()` in place of `SwapUiThreadDispatcher(...)` + `Release()`. |

**Keep the two-phase shape (`BeginTransactionAsync` … `Install`), do not collapse it into a single `SwapAsync(replacement)`.** The gate is deliberately acquired at build start (`:67`), well before the install at `:138`; a single-call API would shorten the hold window to install→restore and silently change the fixture's concurrency behavior during viewer construction and `SaveParameters`. That is a behavior change outside this bug's scope. (A single-call `SwapAsync` was considered and rejected for exactly this reason; it is otherwise simpler.)

---

## 3. `FocusAndThemeTests.cs` compatibility analysis

**Source compatibility.** `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` is a *statement-expression* built from a method invocation. C# permits a method-invocation statement to discard a non-`void` return value; `CS0201` ("only assignment, call, increment, decrement, await and new object expressions can be used as a statement") applies to non-invocation expressions and is not triggered here. Changing `void` → `IDisposable` therefore recompiles the file unchanged. No `using`, no `var`, no `_ =` is needed.

**Analyzer compatibility — verified, not assumed.** The candidate objections are `CA2000` (dispose before losing scope), `CA1806` (do not ignore method results), and `IDISP004` (don't ignore created `IDisposable`). All are neutralized by configuration:

- `.editorconfig:27` — `dotnet_analyzer_diagnostic.severity = suggestion`, a global catch-all explicitly introduced (per the comment at `.editorconfig:23-25`) so "all new analyzer diagnostics default to suggestion so they cannot be promoted to errors under the nullable `/p:TreatWarningsAsErrors=true` build (the protected CI gate)".
- `.editorconfig:29` — the sole exception is `dotnet_diagnostic.MSTEST0032.severity = warning`, which is unrelated.
- The two msbuild toolchain steps are disjoint in the relevant properties: step 2 enables analyzers without `TreatWarningsAsErrors`; step 3 sets `TreatWarningsAsErrors=true` without enabling analyzers. Neither can turn an analyzer suggestion into an error.
- `QuickFiler.Test.csproj` sets no `EnableNETAnalyzers`, `AnalysisMode`, or `TreatWarningsAsErrors` of its own (verified across `QuickFiler.Test.csproj:10-56`); it imports `Meziantou.Analyzer` (`:3`), whose `MA####` rules are all pinned to `suggestion` in `.editorconfig:32+`.

**Behavioral non-regression.** The rule "install only when the field is currently `null`; never overwrite an active value" is preserved verbatim (§2.5 item 2), so both tests see the same field state they see today: parked dispatcher if nothing else installed one, or whatever a concurrent transaction installed. The `async: true` theme paths (`ThemeControlGroup.cs:218`) only need *some* non-null dispatcher that does not execute the queued delegate against a handle-less control; both outcomes satisfy that.

**No new hang or deadlock is reachable from this call site.** The returned scope is discarded and never disposed. Under the two-lock design that means:

- Nothing waits on that scope's `Dispose`. It owns no semaphore permit — only `TransactionGate` permits are waited on, and `EnsureDispatcher` never acquires one.
- The only lock the call site takes is `FieldLock`, held by every holder for a straight-line region with no waits inside; the maximum block is the duration of one reflection get + one reflection set.
- The leak (the parked dispatcher stays installed for the rest of the process) persists for this caller exactly as today, which the delegation explicitly permits.

**Do not edit the file** for a second, independent reason: it is 497 lines against a 500-line ceiling.

---

## 4. Design alternatives evaluated

Recorded briefly, per the "one recommended approach" rule.

- **Single shared gate for both paths (the delegation's hypothesis as literally stated).** Rejected: three failure modes in §2.1. The two-lock design in §2.2 keeps every property the hypothesis wanted.
- **`Interlocked.CompareExchange` on the field instead of a Monitor.** Not implementable: `Interlocked` requires a `ref` to the field, and the field is a private static in another assembly reachable only via `FieldInfo`. A Monitor around the reflection pair is the equivalent primitive available here.
- **Single-call `SwapUiThreadDispatcherAsync(replacement)`.** Rejected: shortens the documented gate hold window (`Part2.cs:65-67`) from build-start to install-time, a silent behavior change to the #230 fixture.
- **Keep `EnsureUiThreadDispatcher` `void` and add a new `EnsureUiThreadDispatcherScoped()` beside it.** Rejected: leaves the leaky method as the path of least resistance for the next author and does not satisfy AC-1 as written, while §3 shows the return-type change costs nothing.
- **Injectable seam replacing the static entirely.** Deferred — see §7.

---

## 5. Regression-test plan

**Location.** A new file, `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, hosting `[TestClass] public class QfcItemController_UiThreadDispatcherFixtureTests`. Rationale: neither owned file has comfortable headroom (§8), and a dedicated class keeps the fixture's contract tests together.

**Bounding.** A file-local `private const int GateTimeoutMs = 60000;` and `[Timeout(GateTimeoutMs)]` on every test, matching the precedent and its stated rationale at `QfcItemController.SeamFactoryTests.cs:288-293` ("every wait is on a deterministic completion signal; the attribute only converts a genuine deadlock into a test failure instead of a CI hang").

**Determinism rules honored.** No `Thread.Sleep`, no `Task.Delay`, no wall-clock waits. All cross-thread coordination uses `ManualResetEventSlim` / awaited `Task` completion. Distinct dispatcher instances come from the existing `QfcItemControllerTestSupport.StartRunningDispatcher()` (`TestSupport.cs:297-317`) with `ShutdownDispatcher` in `finally` (`:323-326`). No temporary files.

**Isolation from the rest of the suite.** Every test that needs a known field value first acquires a transaction (`BeginTransactionAsync`) and installs that value, so it is mutually excluded from the pump fixtures for its whole body. This is only possible because `EnsureDispatcher` does *not* take `TransactionGate` (§2.1 item 3).

| # | Test | Scenario | Deterministic assertion |
| --- | --- | --- | --- |
| R1 | `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | The exact #230 clobber precondition. Begin transaction; `Install(liveA)`; call `EnsureUiThreadDispatcher()`; dispose that scope; then dispose the transaction. | `Current` is `liveA` after the `Ensure` call **and** after disposing the `Ensure` scope; equals the original value after the transaction is disposed. |
| R2 | `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | AC "restore called when no prior dispatcher existed". Begin transaction; `Install(null)` to force a known null baseline; `Ensure`; dispose the `Ensure` scope; dispose the transaction. | `Current` is non-null after `Ensure`; `null` after disposing the `Ensure` scope; original after disposing the transaction. |
| R3 | `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | AC "restore called twice". R2's shape plus a second `Dispose()`. | Second `Dispose` does not throw; `Current` unchanged between the two disposals. |
| R4 | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | AC "two callers racing install+restore in parallel". Task A begins a transaction and installs `liveA`, signals an MRE, waits for permission; Task B calls `BeginTransactionAsync` and records `Current` **immediately on acquisition, before installing**; main releases A, awaits B. | B's recorded value equals the original, **never** `liveA`. Guaranteed by the restore-before-release ordering of §2.5 item 5. |
| R5 | `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` | Double-dispose of a transaction. | No `SemaphoreFullException`; a subsequent `BeginTransactionAsync()`/`Dispose()` round trip completes within the `[Timeout]`. |

**Honest limitation of R4** (state it in the spec rather than overclaiming): under a *correct* implementation R4 passes deterministically. Under a *broken* implementation (gate removed) it fails probabilistically, because nothing can force Task B to reach the acquisition point while Task A still holds the gate. There is no deterministic way to prove "B is currently blocked" without a timed wait, which the determinism rules forbid. R1 is the deterministic counterpart: it proves the clobber itself is unreachable with no concurrency at all, and the clobber — not the scheduling — is the actual #230 mechanism (§2.3). R1 is therefore the primary regression assertion and R4 the supporting one.

**Fail-before evidence.** R1 and R2 fail against the current code by construction: `EnsureUiThreadDispatcher` returns `void` today, so they will not compile against `HEAD`. The plan should capture the fail-before evidence as a two-step artifact (the pre-change source excerpt at `TestSupport.cs:238-249` plus a compile-level demonstration), rather than claiming a red test run that cannot exist.

**Coverage impact.** Test-only change; no production line is added or altered, so there is no production coverage delta to defend. The new fixture is test infrastructure and sits in the test-file exclusion.

---

## 6. Verdict on `UtilitiesCS/Threading/UiThread.cs` — NO CHANGE REQUIRED

Stated prominently because the orchestrator's complexity band depends on it.

**Verdict: `UtilitiesCS/Threading/UiThread.cs` must not be modified by this feature.** Reasoning, independently derived:

1. **Nothing the fix needs is missing from `UiThread`.** The fix requires (a) atomic read-modify-write of the field and (b) mutual exclusion between long transactions. Both are properties of the *mutators*, not of the field. Both can be — and under §2 are — provided entirely inside `QuickFiler.Test` by funneling all mutations through one lock.
2. **A production seam would require widening a production assembly's surface.** The setter is `private` (`UiThread.cs:138`) and `UtilitiesCS` grants `InternalsVisibleTo` only to `UtilitiesCS.Test` and `ToDoModel.Test` (`UtilitiesCS/Properties/AssemblyInfo.cs:19-20`); a prior attempt to grant it to `QuickFiler.Test` exists only as a commented-out line (`UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs:15`). Any `internal` test seam on `UiThread` would need that grant added — a production change with cross-assembly consequences, made solely for test convenience, on a bug whose scope is test isolation.
3. **A production-side lock would not close the residual gap anyway.** Three other mutators bypass any test-side discipline (`WpfUiDispatcherTests.cs:42-51`, plus the `UtilitiesCS.Test` sites in §1.6). A lock *inside* `UiThread` would make each individual write atomic but would still not serialize a transaction's install/restore pair against an unrelated class — so it buys nothing the two-lock test-side design does not already provide, at strictly higher risk.
4. **Any change to the getter is actively dangerous.** Adding a lazy `Init()` fallback (the shape used by `UiSyncContext` at `:113-125`) would make the getter show a `SyncContextForm` (`:51-54`) in a unit test — a live WinForms form in a test host, forbidden by the unit-test policy and by the existing `NoLiveFormInTestAssemblyTests.cs` guard in this very project. The current plain-field getter (`:137`) is the safe shape and must stay.
5. **The 500-line rule is not a factor:** `UiThread.cs` is 163 lines.

---

## 7. Injectable-seam alternative — defer, do not attempt here

The promoted doc floats replacing the mutable static with an injectable seam and explicitly says it "should be evaluated on its merits rather than assumed". Evaluation:

- The seam already exists and is partially adopted: `UtilitiesCS/Threading/IUiDispatcher.cs`, `UtilitiesCS/Threading/WpfUiDispatcher.cs` (whose default ctor is literally `: this(() => UiThread.Dispatcher)`, `WpfUiDispatcher.cs:25`), and `QfcItemController._uiDispatcher`, which the pump fixture already injects (`Part2.cs:110-114`).
- Remaining static consumers: approximately **62 references across 29 first-party production files** (measured by repo-wide grep excluding `*Test*`, docs, and `.claude`), concentrated in `QuickFiler/Controllers/QfcCollectionController.cs` (8), `QuickFiler/Controllers/QfcQueue.cs` (4), `QuickFiler/Helper Classes/ItemViewerQueue.cs` (4), `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` (4), and ~25 other files.
- Converting those is a multi-phase production refactor across three assemblies with a live VSTO surface, no behavioral defect of its own, and no bounded blast radius. It is disproportionate to a test-isolation bug and would swamp the regression evidence for #493.

**Recommendation:** deliver §2, and promote the seam conversion as its own issue if it is not already tracked (note `#584 "UiThread.Dispatcher null race"` is recorded as an open follow-up at `docs/features/epics/quickfiler-suite-determinism-foundation/epic-status.md:168`, which is adjacent but not the same concern; check for overlap before promoting a duplicate).

---

## 8. File-size headroom (500-line ceiling, `.claude/rules/general-code-change.md`)

Current line counts (measured from full reads):

| File | Lines | Headroom |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 365 | 135 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 418 | 82 |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (unowned) | 497 | 3 |
| `UtilitiesCS/Threading/UiThread.cs` | 163 | 337 |

**Deletions available in the owned files:**

- `Part2.cs`: gate field + its 15-line doc block (`:36-51`, 16 lines) and `SwapUiThreadDispatcher` + doc (`:143-158`, 16 lines) → **~32 lines freed**, before accounting for the roughly size-neutral edits to `BuildPumpHarnessAsync` and `PumpHarness`. Projected ≈ 386 lines, ~114 headroom.
- `TestSupport.cs`: moving `_dedicatedDispatcher`, `_dedicatedDispatcherLock`, and `GetDedicatedDispatcher` (`:221-222` and `:251-285`, ~37 lines) into the new fixture, and collapsing `EnsureUiThreadDispatcher` (`:238-249`) to a one-line delegating wrapper → **~45 lines freed**, offset by ~12 lines of retained/updated XML docs. Projected ≈ 340 lines, ~160 headroom. Confirmed safe: `GetDedicatedDispatcher` has exactly one caller; `StartRunningDispatcher`/`ShutdownDispatcher` must **stay** in `QfcItemControllerTestSupport` because the unowned `WpfUiDispatcherTests.cs:48,84` calls them.

**Recommended layout (two new files, both in the `Qfc*` neighbourhood):**

| File | Contents | Est. lines |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (new) | `UiThreadDispatcherFixture` + `UiThreadDispatcherTransaction` + the moved parked-dispatcher factory | ~150-180 |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (new) | R1-R5 | ~170-210 |

Both names begin with `QfcItemController.`, so their `<Compile Include>` entries belong in the permitted `Qfc*` neighbourhood of `QuickFiler.Test/QuickFiler.Test.csproj` — the natural insertion point is immediately after `:146` (`Controllers\QfcItemController.TestSupport.cs`), which is where the existing `QfcItemController.*` entries are grouped (`:138-156`). Two `<Compile Include>` lines total.

**Fallback if new files are disallowed:** put the fixture inline in `TestSupport.cs` (projected ≈ 470-490 lines, i.e. 10-30 lines of headroom — workable but leaves the file effectively frozen) and the tests in `Part2.cs` (projected ≈ 470). Not recommended; it trades a two-line csproj edit for two near-ceiling files.

---

## 9. Residual risks and follow-ups (report only; not in scope)

- **R-1 — `WpfUiDispatcherTests.cs:42-51` remains an ungated mutator.** It swaps `UiThread._dispatcher` to a *running* dispatcher with a plain `finally` restore and no participation in either lock. After this fix it can still lose an update against a transaction. The file is not in this feature's owned set. Candidate for promotion to its own issue: "route `WpfUiDispatcherTests`' static swap through the shared `UiThreadDispatcherFixture`". Low risk in CI (that assembly runs sequentially there, §1.7) and it is a single-class, short-lived swap.
- **R-2 — Cross-assembly mutators.** `UtilitiesCS.Test/Threading/{ProgressTracker_Tests.cs:421, ProgressTrackerAsync_Tests.cs:137, IdleAsyncQueue_Tests.cs:143}` mutate the same process-wide static. Relevant only if a single test host loads both assemblies. No test-side lock in `QuickFiler.Test` can reach them.
- **R-3 — `EmailMoveMonitorTests`' cleanup invariant.** `EmailMoveMonitorTests.cs:53-60` asserts the static is unchanged across each of its tests. The recommended design does not change the field's steady-state value relative to today (the unowned `Ensure` callers still leak the parked dispatcher), so the exposure is unchanged — but the spec should state this explicitly, because "make `Ensure` always restore" would have changed it.
- **R-4 — `[DoNotParallelize]` is not mutual exclusion in this repo.** Per the measured finding in `.claude/agent-memory/atomic-executor/project_mstest_donotparallelize_overlaps_parallel_bucket.md`, do not let anyone propose `[DoNotParallelize]` as an alternative to the gate during review.
- **R-5 — Gate contention cost.** Regression tests R1-R5 hold `TransactionGate` briefly; under the runsettings-forced parallel configuration they serialize against pump tests (each ≤ 60 s). Negligible, but it is a real serialization the plan should acknowledge.

---

## 10. Acceptance-criteria traceability

| AC (promoted doc `docs/features/potential/promoted/2026-08-07-uithread-dispatcher-static-swap-no-restore.md:49-53`) | Where satisfied |
| --- | --- |
| `EnsureUiThreadDispatcher` restores the previous value, idempotently | §2.4 return type + §2.5 items 3, 4, 6; tests R2, R3 |
| Concurrent callers cannot interleave install and restore | §2.2/§2.3 (`FieldLock` makes each mutation atomic; `TransactionGate` serializes transactions); tests R1, R4 |
| Regression test demonstrates the #230 deadlock is unreachable, `[Timeout]`-bounded | §5 R1 (primary, deterministic) and R4 (supporting), all `[Timeout(GateTimeoutMs)]` |
| The #230 local `SemaphoreSlim` is removed, not duplicated | §2.6 — `Part2.cs:51` deleted, `Part2.cs:143-158` deleted, single implementation in the fixture |
| No `Thread.Sleep` / `Task.Delay` / wall-clock waits | §5 "Determinism rules honored"; all coordination via `ManualResetEventSlim` and awaited `Task`s |
| Existing callers audited (return-type change is breaking within the test project) | §1.5 and §3 — exactly two call sites, both source-compatible and analyzer-safe |
