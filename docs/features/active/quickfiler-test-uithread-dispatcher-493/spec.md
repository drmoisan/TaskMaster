# quickfiler-test-uithread-dispatcher (Spec)

- **Issue:** #493
- **Parent (optional):** epic `quickfiler-bug-family` (integration branch `epic/quickfiler-bug-family-integration`)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-24T11-30
- **Status:** Ready for Planning
- **Version:** 1.0

> **Acceptance-criteria authority.** Work Mode is `full-bug`. Per
> `.claude/skills/acceptance-criteria-tracking/SKILL.md`, this file (`spec.md`) is the **sole**
> authoritative acceptance-criteria source for this feature. `issue.md` deliberately does not
> restate the criteria, and `user-story.md` is intentionally absent (this is a bug fix, not a
> feature with a user story).

> **Primary design source.** The design below is transcribed from the completed research artifact
> `docs/features/active/quickfiler-test-uithread-dispatcher-493/research/2026-08-24T11-05-uithread-dispatcher-restore-scope-research.md`
> (cited throughout as "research §N"). It is not re-derived here. Where that document leaves a
> point open for the planner, this spec says so explicitly rather than inventing a resolution.

---

## Context

- **Summary.** `QfcItemControllerTestSupport.EnsureUiThreadDispatcher`
  (`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:238-249`) mutates the process-wide
  static `UtilitiesCS.Threading.UiThread._dispatcher` by reflection into the private backing field
  and never restores the prior value. The read-modify-write is a plain check-then-act with **no
  synchronization at all** (research §1.1). The static is process-wide, so one test class's
  mutation is visible to every other class in the same test host for the remainder of the run.
- **Policy violated.** `.claude/rules/general-unit-test.md` — **Independence** ("tests must be able
  to run in any order without impacting each other") and **Environment Stability** ("tests must not
  rely on mutable global state").
- **Observed environment(s).** `QuickFiler.Test` (net48, MSTest). The race is *dormant* in CI and
  *active* locally:
  - CI (`.github/workflows/_mstest-coverage.yml:83`) runs `vstest.console.exe ... /InIsolation`
    with **no `/Settings:`**, so `QuickFiler.Test` executes sequentially there.
  - `TaskMaster.runsettings:4-7` and `scripts/vscode/TaskMaster.cli.runsettings:4-7` both declare
    `<Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize>`, so local coverage
    runs activate the race (research §1.7).
- **Impact and severity.** This defect has already produced a real failure. During execution of
  issue #230, the Phase 8 toolchain loop failed its first iteration with two `[Timeout]` expiries,
  one from each of the two classes that swap this static
  (`QfcItemController_InitializationTests` via `.Part2.cs`, and `QfcItemController_SeamFactoryTests`).
  Under class-level parallelization one class's write reverted the process-wide static to a parked,
  never-pumped dispatcher while the other class's member under test was still awaiting a dispatcher
  operation, producing a deadlock that surfaced as a timeout rather than an assertion failure.
  Corroborated independently by repository memory
  `.claude/agent-memory/atomic-executor/project_uithread_dispatcher_static_swap_race.md`
  (research §1.8).
- **First observed / versions impacted.** First observed 2026-08-07 during issue #230 (PR #479,
  WinForms message-pump test seam); recorded as an out-of-scope finding at that time. Present on
  `main` at `988e819b` and on the epic integration base. Test-assembly only; no shipped product
  version is affected.
- **Partial prior fix (the reason this issue exists).** The #230 failure was fixed *locally* in the
  #230 fixture (`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:51`)
  with a private static `SemaphoreSlim(1,1)` (`UiThreadDispatcherGate`) held from fixture build
  through an idempotent `PumpHarness.Restore()`. The shared helper `EnsureUiThreadDispatcher` was
  not changed and still carries no gate and no restore.

## Repro & Evidence

- **Steps to reproduce (interleaving form).** The mechanism is a lost update on a check-then-act;
  the following four-step interleaving is the exact #230 sequence (research §2.3):
  1. `SwapUiThreadDispatcher` reads the previous value (`Part2.cs:155`).
  2. `EnsureUiThreadDispatcher` reads `null` (`TestSupport.cs:245`).
  3. `SwapUiThreadDispatcher` writes the live pump dispatcher (`Part2.cs:156`).
  4. `EnsureUiThreadDispatcher` writes the **parked** dispatcher over it (`TestSupport.cs:247`).
- **Steps to reproduce (suite form).** Run `QuickFiler.Test` with either repo runsettings file
  (class-level parallelization, `Workers=0`) so that a class calling `EnsureUiThreadDispatcher`
  executes concurrently with `QfcItemController_InitializationTests` or
  `QfcItemController_SeamFactoryTests`.
- **Expected vs actual.**
  - Expected: each test class observes the `UiThread` dispatcher state it established, and the
    process-wide static is not left mutated by a helper that never restores it.
  - Actual: after step 4 the pump fixture's awaits are posted to a dispatcher that never runs a
    frame, so they never complete; and independently of any race, the static keeps pointing at
    whatever dispatcher the last caller installed for the remainder of the process.
- **Error snippet / failure signature.** `[Timeout]` expiry (60 000 ms, `PumpTimeoutMs`) rather
  than an assertion failure; exactly one failure per swapping class; green in every filtered run
  (research §1.8).
- **Frequency / determinism.** Intermittent and scheduling-dependent under parallelized local runs;
  currently unreachable under the CI invocation because that invocation is sequential. The
  underlying unsynchronized check-then-act is present unconditionally.
- **Why the parked dispatcher hangs rather than throws.** `GetDedicatedDispatcher`
  (`TestSupport.cs:257-285`) lazily creates a singleton `Dispatcher` on an STA background thread
  that sets `Dispatcher.CurrentDispatcher`, signals, then blocks forever on `park.Wait()`
  (`:270`). It **never runs a dispatcher frame**, so any `InvokeAsync`/`BeginInvoke` posted to it
  is enqueued and never completes (research §1.1).

## Scope & Non-Goals

**In scope**

- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (owned).
- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` (owned).
- Two new files in the same `Qfc*` neighbourhood (research §8):
  - `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
  - `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- Two `<Compile Include>` additions to `QuickFiler.Test/QuickFiler.Test.csproj`.

**Out of scope / non-goals**

- **`UtilitiesCS/Threading/UiThread.cs` — NO CHANGE REQUIRED.** See § Proposed Fix and research §6.
- **`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` — must NOT be edited.**
  Sibling-owned; see § Constraint on the unowned call site.
- **No QuickFiler production source.** Every `QuickFiler/Controllers/QfcItemController.*`
  production partial belongs to sibling epic features #484, #444, or #489.
- **The injectable-seam replacement of the static** (`IUiDispatcher` everywhere) is explicitly
  deferred — ~62 references across 29 first-party production files (research §7).
- **Other mutators of the same static are not fixed here**: `WpfUiDispatcherTests.cs:42-51,83`
  (same assembly, unowned) and the `UtilitiesCS.Test` sites. Recorded as accepted residual risks
  R-1 and R-2 in § Risks & Mitigations.

**Explicitly excluded systems / datasets.** No production assembly, no VSTO surface, no
`InternalsVisibleTo` grant, no runsettings or CI workflow change, no data or configuration.

## Root Cause Analysis

**Confirmed root cause — a lost update on an unsynchronized check-then-act, made observable by a
process-wide static plus class-level test parallelism.**

1. **The unsynchronized check-then-act** (research §1.1, §2.3). `EnsureUiThreadDispatcher` reads
   `field.GetValue(null)`, tests it for `null`, and then writes `field.SetValue(null, ...)`
   (`TestSupport.cs:245-248`) with no lock. `SwapUiThreadDispatcher` (`Part2.cs:143-158`) performs
   its own unconditional read-then-write pair. Nothing makes either pair atomic, so the two can
   interleave and the later write silently discards the earlier one. This is the defect; it is a
   property of the *mutators*, not of the field.

2. **The static that makes it process-wide** (research §1.2).
   `UtilitiesCS/Threading/UiThread.cs:135-140` exposes
   `public static Dispatcher Dispatcher { get => _dispatcher; private set => _dispatcher = value; }`
   over `private static Dispatcher _dispatcher = null!;`. The getter is a plain field read with
   **no lazy `Init()` fallback** (contrast `UiSyncContext` at `:113-125` and `AutoScaleFactor` at
   `:147-158`, both of which call `Init()`, which shows a `SyncContextForm`). The setter is
   `private`, so a test can only write the value by reflection on `_dispatcher`. The file carries
   `#nullable enable` (`:1`).

3. **Why the field value is load-bearing.** Consumers reached by the affected tests are
   `UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs:218`
   (`UiThread.Dispatcher.InvokeAsync(...)` on the `async: true` branch) and
   `UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs:254,277`. Therefore: null field → an
   `NullReferenceException` at the consumer; parked field → queued-but-never-executed (exactly what
   the `async: true` theme tests want); live pumped field → executes (research §1.2).

4. **The parallelism that makes it observable** (research §1.7). `QuickFiler.Test` declares no
   `[assembly: Parallelize(...)]`; the repo runsettings force `ClassLevel` parallelization with
   `Workers=0` on every assembly. Repository memory
   `.claude/agent-memory/atomic-executor/project_mstest_donotparallelize_overlaps_parallel_bucket.md`
   records the measured finding that `[DoNotParallelize]` classes **do overlap** the parallel
   bucket in this repo's adapter, so `[DoNotParallelize]` is not a mutual-exclusion mechanism here.

5. **Absent restore, independent of any race.** Even with no concurrency, the helper leaves the
   parked dispatcher installed for the rest of the process. Every subsequent caller of
   `UiThread.Dispatcher` in that host observes a dispatcher that never runs a frame.

**Affected components (paths).**

| Path | Role |
| --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:238-249` | The defective helper (owned) |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:221-222, 251-285` | Parked-dispatcher singleton + factory (owned; one caller only) |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:36-51, 60-158, 306-352` | The duplicated local fix (owned) |
| `UtilitiesCS/Threading/UiThread.cs:135-140` | The mutated static (read-only reference; **not** changed) |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:452, 468` | The two unowned call sites (**not** changed) |

## Proposed Fix

### Design summary (what changes where)

Funnel **every** mutation of `UiThread._dispatcher` inside `QuickFiler.Test` through one new,
single-responsibility test fixture class that owns two distinct locks, and reduce both the leaky
helper and the duplicated #230 workaround to thin calls into it.

- New file `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` hosts
  `UiThreadDispatcherFixture` (static) and `UiThreadDispatcherTransaction` (`IDisposable`), plus the
  parked-dispatcher factory moved verbatim from `TestSupport.cs`.
- `QfcItemControllerTestSupport.EnsureUiThreadDispatcher` becomes a one-line delegating wrapper
  whose return type changes from `void` to `IDisposable`, so the fully-qualified name the unowned
  call site uses is unchanged.
- `QfcItemController.InitializationTests.Part2.cs` deletes its private `SemaphoreSlim` and its
  private `SwapUiThreadDispatcher` and consumes the shared transaction instead.
- New file `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` hosts
  regression tests R1-R5.

**`UtilitiesCS/Threading/UiThread.cs` requires NO changes.** (Research §6, stated verbatim here
because the orchestrator's complexity-band record depends on it.) The reasoning:

1. **Nothing the fix needs is missing from `UiThread`.** The fix requires (a) atomic
   read-modify-write of the field and (b) mutual exclusion between long transactions. Both are
   properties of the *mutators*, not of the field, and both are provided entirely inside
   `QuickFiler.Test` by funneling all mutations through one lock.
2. **A production seam would require widening a production assembly's surface.** The setter is
   `private` (`UiThread.cs:138`) and `UtilitiesCS` grants `InternalsVisibleTo` only to
   `UtilitiesCS.Test` and `ToDoModel.Test` (`UtilitiesCS/Properties/AssemblyInfo.cs:19-20`); a prior
   attempt to grant it to `QuickFiler.Test` exists only as a commented-out line
   (`UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs:15`). Any `internal` test seam on
   `UiThread` would need that grant added — a production change with cross-assembly consequences,
   made solely for test convenience, on a bug whose scope is test isolation.
3. **A production-side lock would not close the residual gap anyway.** Three other mutators bypass
   any test-side discipline (`WpfUiDispatcherTests.cs:42-51`, plus the `UtilitiesCS.Test` sites). A
   lock *inside* `UiThread` would make each individual write atomic but would still not serialize a
   transaction's install/restore pair against an unrelated class — so it buys nothing the two-lock
   test-side design does not already provide, at strictly higher risk.
4. **Any change to the getter is actively dangerous.** Adding a lazy `Init()` fallback (the shape
   used by `UiSyncContext` at `:113-125`) would make the getter show a `SyncContextForm` (`:51-54`)
   in a unit test — a live WinForms form in a test host, forbidden by the unit-test policy and by
   the existing `NoLiveFormInTestAssemblyTests.cs` guard in this very project. The current
   plain-field getter (`:137`) is the safe shape and must stay.
5. **The 500-line rule is not a factor:** `UiThread.cs` is 163 lines.

Consequently, the delegation's conditional permission to edit `UtilitiesCS/Threading/UiThread.cs`
("only if the fix genuinely requires it") is **not exercised**. That file must appear unmodified in
the final diff.

### The two-lock design

A single shared gate — the obvious first hypothesis — was evaluated and **rejected** because it has
three failure modes (research §2.1):

1. **Unbounded block in an unowned, un-`[Timeout]`-ed test.** `FocusAndThemeTests.cs:452/468` would
   block for the full duration of a concurrently running pump test (up to `PumpTimeoutMs` = 60 000 ms).
2. **A pump-test failure becomes a permanent hang elsewhere.** If a pump test expires on its
   `[Timeout]` before `PumpHarness.Restore()` runs, the gate is never released, hanging
   `FocusAndThemeTests`, which has no `[Timeout]` — converting a bounded failure into an unbounded
   hang in a file this feature is forbidden to touch.
3. **The regression tests self-deadlock.** `SemaphoreSlim` is not reentrant, so a regression test
   that holds a transaction and then calls `EnsureUiThreadDispatcher` on the same thread would
   deadlock on itself — and that is exactly the shape of the most valuable regression scenario.

The recommended design separates the two concerns the single gate was conflating (research §2.2):

| Concern | Primitive | Hold duration | Acquired by |
| --- | --- | --- | --- |
| **Atomicity** of a single read-modify-write of `UiThread._dispatcher` | `private static readonly object FieldLock` (Monitor) | straight-line, no waits inside | *every* mutation path: `EnsureDispatcher`, `Transaction.Install`, `Transaction.Dispose`, `EnsureScope.Dispose` |
| **Mutual exclusion between long transactions** (install → test body → restore) | `private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1)` | build-start → restore (unchanged from today) | `BeginTransactionAsync` / `Transaction.Dispose` only |

`EnsureUiThreadDispatcher` takes **only** `FieldLock`. It never touches `TransactionGate`.

**Why this fixes the bug** (research §2.3): `FieldLock` makes the swap's read+write one atomic
region and the ensure's read+write another, so the four-step interleaving in § Repro is
unrepresentable. Both remaining orderings are benign:

- *Ensure first*: field goes `null` → parked; the transaction then captures `parked` as previous and
  installs the live dispatcher; on restore the field returns to `parked`. Non-null throughout. No hang.
- *Transaction first*: the field is already non-null when `Ensure` runs, so `Ensure` does nothing
  (its install-only-when-null rule, preserved verbatim).

`TransactionGate` continues to do exactly the job the #230 local gate does today — keeping
`QfcItemController_InitializationTests` and `QfcItemController_SeamFactoryTests` from interleaving
their install/restore pairs — with an unchanged hold window.

### Boundaries and invariants to preserve

- **Lock ordering: `TransactionGate` → `FieldLock`, never the reverse.** `FieldLock` is never held
  while acquiring or awaiting `TransactionGate`, and nothing inside a `FieldLock` region blocks,
  allocates a thread, or awaits. There is no cycle, therefore no deadlock (research §2.2).
- **`EnsureDispatcher` never acquires `TransactionGate`.** This is what keeps the unowned,
  un-`[Timeout]`-ed call sites bounded and what makes the regression tests writable at all.
- **The install-only-when-null rule is preserved verbatim** (`TestSupport.cs:245`).
- **The #230 gate hold window is unchanged**: acquired at build start (`Part2.cs:67`), released
  after restore.
- **`PumpHarness.Restore()` stays idempotent** and keeps restore-before-release ordering.
- **`StartRunningDispatcher` / `ShutdownDispatcher` must stay in `QfcItemControllerTestSupport`** —
  the unowned `WpfUiDispatcherTests.cs:48,84` calls them (research §8).
- **`EmailMoveMonitorTests`' cleanup invariant is not disturbed** (`EmailMoveMonitorTests.cs:53-60`
  asserts the static is unchanged across each of its tests). The design does not change the field's
  *steady-state* value relative to today, because the unowned `Ensure` callers still discard their
  scope and therefore still leak the parked dispatcher. A design that made `Ensure` "always
  restore" *would* have changed it; this one does not (research §9 R-3).
- **500-line ceiling** (`.claude/rules/general-code-change.md`) on every touched and new file.

### Dependencies or blocked work

- None blocking. The change is self-contained inside `QuickFiler.Test` plus two `<Compile Include>`
  lines in `QuickFiler.Test/QuickFiler.Test.csproj`.
- Coordination constraint only: sibling epic features #484, #444, #489 own the QuickFiler
  production partials and `QfcItemController.FocusAndThemeTests.cs`. This feature must not touch
  them, so no merge coupling is expected.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

| File | Disposition |
| --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | **New.** `UiThreadDispatcherFixture`, `UiThreadDispatcherTransaction`, moved parked-dispatcher factory. Est. 150-180 lines. |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | **New.** `[TestClass] public class QfcItemController_UiThreadDispatcherFixtureTests` hosting R1-R5. Est. 170-210 lines. |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | **Modified.** 365 → ≈340 lines. |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | **Modified.** 418 → ≈386 lines. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | **Modified.** Two `<Compile Include>` lines. |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | **Unmodified.** 497 lines. |
| `UtilitiesCS/Threading/UiThread.cs` | **Unmodified.** 163 lines. |

`<Compile Include>` insertion point: immediately after the existing
`<Compile Include="Controllers\QfcItemController.TestSupport.cs" />` entry, currently at
`QuickFiler.Test/QuickFiler.Test.csproj:146`, inside the grouped `QfcItemController.*` block
(`:138-156`). Verified present at that line. Two lines total, in the permitted `Qfc*` neighbourhood.

#### Functions/classes impacted

Proposed API surface (research §2.4):

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

`GetDedicatedDispatcher` is private and has exactly one caller (`TestSupport.cs:247`), verified by
repo-wide grep, so the move is safe (research §1.1).

#### Required semantics — this is the contract (research §2.5)

1. **`EnsureDispatcher` obtains the parked dispatcher *before* taking `FieldLock`.**
   `GetDedicatedDispatcher` starts a thread and waits on a `ManualResetEventSlim`
   (`TestSupport.cs:263-280`); doing that inside `FieldLock` would falsify the "straight-line, no
   waits" property. Creating the singleton unconditionally on first call is harmless (one
   background thread per process, at most once).
2. **`EnsureDispatcher` installs only when the field is `null`.** Preserved exactly
   (`TestSupport.cs:245`). This is what makes the unowned call sites' behavior unregressed and what
   makes the design provably unable to clobber a live install.
3. **When no install occurs, return a no-op scope** — not a scope that writes back the earlier
   observed value. Writing back an earlier-observed value is only a no-op if nothing changed in
   between; if a transaction has since installed a live dispatcher, that write would clobber it,
   reintroducing the #230 mechanism through the restore path. Because `EnsureDispatcher` installs
   only when the previous value was `null`, the only value an ensure-scope ever needs to restore is
   `null`, which makes the restore a single line whose correctness is self-evident.
4. **All restores are conditional (compare-then-write).** The ensure-scope's `Dispose` writes `null`
   only if the field still holds the exact instance it installed (`ReferenceEquals`);
   `Transaction.Dispose` writes the captured previous only if the field still holds the exact
   instance it installed. If another owner has since replaced the value, the restore is skipped,
   leaving the newer owner's value intact. This deliberately trades an exact restore for a bounded
   leak in a contended edge case, because the alternative is clobbering a live dispatcher — the
   defect being fixed.
5. **`Transaction.Dispose` restores *before* releasing the gate.** The ordering is load-bearing: it
   is what makes the R4 assertion deterministic (a waiter cannot observe the pre-restore value).
   `PumpHarness.Restore` already has this ordering (`Part2.cs:348` then `:350`); preserve it.
6. **Idempotency guards on both scope types**, mirroring `PumpHarness._restored`
   (`Part2.cs:309, 342-347`). A second `Dispose` must not re-write the field and must not call
   `TransactionGate.Release()` again — a second `Release` on a `SemaphoreSlim(1, 1)` throws
   `SemaphoreFullException`.

#### How `QfcItemController.InitializationTests.Part2.cs` changes (research §2.6)

| Current | Change |
| --- | --- |
| `:36-51` gate field + doc comment | **Delete.** Replaced by `UiThreadDispatcherFixture.TransactionGate`. Retain a 2-3 line comment pointing at the fixture so the #230 rationale is not lost. |
| `:60-77` `BuildPumpHarnessAsync` gate acquire + catch-release | Replace `await UiThreadDispatcherGate.WaitAsync()` with `await UiThreadDispatcherFixture.BeginTransactionAsync()`; replace `catch { UiThreadDispatcherGate.Release(); throw; }` with `catch { transaction.Dispose(); throw; }`. **Keep the acquire at build start.** |
| `:79-141` `BuildPumpHarnessCoreAsync` | Takes the transaction as a parameter; line `:138` becomes `transaction.Install(viewer.UiDispatcher);`. |
| `:143-158` `SwapUiThreadDispatcher` | **Delete.** This is the duplicated reflection logic the acceptance criteria name. |
| `:306-352` `PumpHarness` | Store the `UiThreadDispatcherTransaction` instead of `Dispatcher _previousUiThreadDispatcher`; `Restore()` keeps its `_restored` guard and calls `transaction.Dispose()` in place of `SwapUiThreadDispatcher(...)` + `Release()`. |

**Keep the two-phase shape (`BeginTransactionAsync` … `Install`); do not collapse it into a single
`SwapAsync(replacement)`.** The gate is deliberately acquired at build start (`:67`), well before
the install at `:138`; a single-call API would shorten the hold window to install→restore and
silently change the fixture's concurrency behavior during viewer construction and `SaveParameters`
— a behavior change outside this bug's scope. A single-call `SwapAsync` was considered and rejected
for exactly that reason; it is otherwise simpler.

Note the pump fixture is consumed by a second `[TestClass]`:
`QfcItemController.SeamFactoryTests.cs:313, 384` calls
`QfcItemController_InitializationTests.BuildPumpHarnessAsync` directly with `harness.Restore()` in
`finally`. That call site's signature must remain source-compatible (research §1.4).

#### Data flow and validation changes

None. No data, no serialization format, no parsing. The only "data" is a single
`System.Windows.Threading.Dispatcher` reference in a private static field, read and written by
reflection through a single cached `FieldInfo`.

#### Error handling and logging updates

- `UiThreadDispatcherTransaction.Install` **throws `InvalidOperationException`** if called twice —
  fail fast per `.claude/rules/general-code-change.md` § Error Handling.
- The cached `FieldInfo` lookup must assert the field exists, preserving the current
  `field.Should().NotBeNull(because: "UiThread._dispatcher backing field must exist")` intent from
  `TestSupport.cs:242-244`.
- Both `Dispose` implementations are idempotent and must not throw on a second call.
- No logging is added. This is test infrastructure; MSTest assertion output is the diagnostic
  surface.

#### Rollback / feature-flag considerations

Not applicable. Test-assembly-only change with no runtime toggle. Rollback is a revert of the
branch.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

- `UiThreadDispatcherFixture.EnsureDispatcher()` → `IDisposable`. Never `null`. Disposing it is
  optional for correctness (a discarded scope leaks exactly as today, no more).
- `UiThreadDispatcherFixture.BeginTransactionAsync()` → `Task<UiThreadDispatcherTransaction>`.
  Completes when `TransactionGate` is acquired. The returned transaction has **not** installed
  anything yet.
- `UiThreadDispatcherTransaction.Install(Dispatcher replacement)` → `void`. `replacement` may be
  `null` (R2 uses `Install(null)` to force a known null baseline).
- `UiThreadDispatcherFixture.Current` → `Dispatcher`, read under `FieldLock`. Test-observation only.
- `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` → `IDisposable` (was `void`).

#### Required configuration keys and defaults

None. No configuration, no runsettings change, no environment variable.

#### Backward-compatibility expectations

- The return-type change `void` → `IDisposable` is a **breaking change to the helper's contract
  within the test project**, so all callers were audited: exactly two, both in
  `QfcItemController.FocusAndThemeTests.cs` (`:452`, `:468`), confirmed by repo-wide grep
  (research §1.5). Both remain source-compatible; see the next section.
- No public or production API changes. `UtilitiesCS` is untouched, so no assembly's public surface
  moves.

#### Performance constraints

No latency or throughput budget. `FieldLock` is held for one reflection get plus one reflection set,
with no waits inside. `TransactionGate` contention is bounded by the existing `PumpTimeoutMs`
(60 000 ms) and is unchanged in scope from today; the regression tests add a small, acknowledged
increment of serialization against the pump tests (residual risk R-5).

### Constraint on the unowned call site — `QfcItemController.FocusAndThemeTests.cs`

**This file is owned by a sibling epic feature. It must NOT be edited by this feature.** The design
must therefore be safe for its two call sites without requiring them to cooperate. Research §3
establishes that it is, on four independent grounds:

1. **Source compatibility.** `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` is a
   statement-expression built from a method invocation. C# permits a method-invocation statement to
   discard a non-`void` return value; `CS0201` applies to non-invocation expressions and is not
   triggered here. Changing `void` → `IDisposable` therefore recompiles the file unchanged — no
   `using`, no `var`, no `_ =`.
2. **Analyzer compatibility — verified, not assumed.** The candidate objections `CA2000`, `CA1806`,
   and `IDISP004` are all neutralized by configuration: `.editorconfig:27` sets
   `dotnet_analyzer_diagnostic.severity = suggestion` as a global catch-all, introduced expressly
   (comment at `.editorconfig:23-25`) so new analyzer diagnostics cannot be promoted to errors
   under the nullable `TreatWarningsAsErrors` build; the sole exception at `.editorconfig:29` is
   `MSTEST0032`, which is unrelated. The two msbuild steps are disjoint in the relevant properties
   (step 2 enables analyzers without `TreatWarningsAsErrors`; step 3 sets `TreatWarningsAsErrors`
   without enabling analyzers), so neither can turn a suggestion into an error.
   `QuickFiler.Test.csproj` sets no `EnableNETAnalyzers`, `AnalysisMode`, or `TreatWarningsAsErrors`
   of its own (`:10-56`); its `Meziantou.Analyzer` import (`:3`) has all `MA####` rules pinned to
   `suggestion` (`.editorconfig:32+`).
3. **Behavioral non-regression.** The install-only-when-null rule is preserved verbatim, so both
   tests observe the same field state they observe today: the parked dispatcher if nothing else
   installed one, or whatever a concurrent transaction installed. The `async: true` theme paths
   (`ThemeControlGroup.cs:218`) only need *some* non-null dispatcher that does not execute the
   queued delegate against a handle-less control; both outcomes satisfy that. Both call sites sit in
   synchronous, non-`async` `[TestMethod]`s (`:447-462`, `:464-478`) that carry **no `[Timeout]`**
   (verified by reading both attribute blocks at `:447-448` and `:464-465`) — which is precisely why
   the design must never make `EnsureDispatcher` block on a gate someone else holds.
4. **No new hang is reachable from that call site.** The returned scope is discarded and never
   disposed; nothing waits on it, and it owns no semaphore permit because `EnsureDispatcher` never
   acquires one. The only lock the call site takes is `FieldLock`, whose maximum hold is one
   reflection get plus one reflection set. The pre-existing leak (the parked dispatcher stays
   installed for the rest of the process) persists for this caller exactly as today, which the
   delegation explicitly permits.

Independent second reason not to edit the file: it is **497 lines** against the 500-line ceiling and
could not absorb an edit even if it were owned.

### File layout and size projections (research §8)

| File | Before | After (projected) | Headroom after |
| --- | --- | --- | --- |
| `QfcItemController.TestSupport.cs` | 365 | ≈340 | ≈160 |
| `QfcItemController.InitializationTests.Part2.cs` | 418 | ≈386 | ≈114 |
| `QfcItemController.UiThreadDispatcherFixture.cs` (new) | — | ≈150-180 | ≈320-350 |
| `QfcItemController.UiThreadDispatcherFixtureTests.cs` (new) | — | ≈170-210 | ≈290-330 |
| `QfcItemController.FocusAndThemeTests.cs` (unowned) | 497 | 497 | 3 |
| `UtilitiesCS/Threading/UiThread.cs` | 163 | 163 | 337 |

Deletions that fund the projections: in `Part2.cs`, the gate field plus its doc block (`:36-51`,
16 lines) and `SwapUiThreadDispatcher` plus doc (`:143-158`, 16 lines) → ~32 lines freed before the
roughly size-neutral edits to `BuildPumpHarnessAsync` and `PumpHarness`. In `TestSupport.cs`, moving
`_dedicatedDispatcher`, `_dedicatedDispatcherLock`, and `GetDedicatedDispatcher` (`:221-222`,
`:251-285`, ~37 lines) and collapsing `EnsureUiThreadDispatcher` (`:238-249`) to a one-line wrapper
→ ~45 lines freed, offset by ~12 lines of retained/updated XML docs.

**Fallback if new files are disallowed** (not recommended): fixture inline in `TestSupport.cs`
(≈470-490 lines) and tests in `Part2.cs` (≈470). This trades a two-line csproj edit for two
near-ceiling files and effectively freezes both.

## Assumptions, Constraints, Dependencies

**Assumptions**

- The two `<Compile Include>` additions to `QuickFiler.Test/QuickFiler.Test.csproj` are permitted;
  the file is not owned by a sibling feature for the `Qfc*` neighbourhood, and the insertion point
  at `:146` was verified to exist.
- Line-count projections in research §8 are estimates, not measurements of written code; the
  executor must re-measure after implementation against the hard 500-line ceiling.
- `QuickFiler.Test` continues to target net48; per repository memory
  `reference_net48_no_init_record_struct`, `init` accessors, `record`, and `record struct` are
  unavailable and must not be used in the new types.

**Constraints**

- Owned file set: `QfcItemController.TestSupport.cs`,
  `QfcItemController.InitializationTests.Part2.cs`, and (permission not exercised)
  `UtilitiesCS/Threading/UiThread.cs`.
- Forbidden: any QuickFiler production source; `QfcItemController.FocusAndThemeTests.cs`.
- 500-line ceiling per file (`.claude/rules/general-code-change.md`).
- MSTest + Moq + FluentAssertions only (CLAUDE.md § C# Unit Test Policy).
- No `Thread.Sleep`, `Task.Delay`, wall-clock waits, or temporary files
  (`.claude/rules/general-unit-test.md`).
- Base branch: worktree branched from `epic/quickfiler-bug-family-integration`, identical to
  `origin/main` at `988e819b`.

**External dependencies**

- None. No new NuGet package. The design uses `System.Threading.SemaphoreSlim`,
  `System.Threading.Monitor`, `System.Threading.ManualResetEventSlim`, and
  `System.Windows.Threading.Dispatcher`, all already referenced by `QuickFiler.Test`.

## Data / API / Config Impact

- **User-facing or API changes:** none. No production assembly is modified; no public surface moves.
- **Test-internal API change:** `QfcItemControllerTestSupport.EnsureUiThreadDispatcher` changes
  return type `void` → `IDisposable`. Two call sites audited; both source-compatible (see above).
- **Data or migration considerations:** none.
- **Logging / telemetry updates:** none. Test infrastructure adds no logging.
- **Compatibility notes:** no CLI flag, config schema, runsettings, or CI workflow change. The two
  new `<Compile Include>` entries are the only build-configuration change.

## Test Strategy

**Location.** New file `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`,
hosting `[TestClass] public class QfcItemController_UiThreadDispatcherFixtureTests`. Neither owned
file has comfortable headroom, and a dedicated class keeps the fixture's contract tests together
(research §5).

**Bounding.** A file-local `private const int GateTimeoutMs = 60000;` and `[Timeout(GateTimeoutMs)]`
on **every** test, matching the precedent and rationale at `QfcItemController.SeamFactoryTests.cs:288-293`
("every wait is on a deterministic completion signal; the attribute only converts a genuine deadlock
into a test failure instead of a CI hang"). The corresponding precedent constant is
`QfcItemController.InitializationTests.cs:38` (`internal const int PumpTimeoutMs = 60000;`).

**Determinism rules honored** (`.claude/rules/general-unit-test.md`): no `Thread.Sleep`, no
`Task.Delay`, no wall-clock waits. All cross-thread coordination uses `ManualResetEventSlim` and
awaited `Task` completion. Distinct dispatcher instances come from the existing
`QfcItemControllerTestSupport.StartRunningDispatcher()` (`TestSupport.cs:297-317`) with
`ShutdownDispatcher` in `finally` (`:323-326`). No temporary files.

**Isolation from the rest of the suite.** Every test that needs a known field value first acquires a
transaction via `BeginTransactionAsync` and installs that value, so it is mutually excluded from the
pump fixtures for its whole body. This is only possible because `EnsureDispatcher` does **not** take
`TransactionGate`.

**Regression scenarios (research §5).**

| # | Test | Scenario | Deterministic assertion |
| --- | --- | --- | --- |
| R1 | `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | The exact #230 clobber precondition. Begin transaction; `Install(liveA)`; call `EnsureUiThreadDispatcher()`; dispose that scope; then dispose the transaction. | `Current` is `liveA` after the `Ensure` call **and** after disposing the `Ensure` scope; equals the original value after the transaction is disposed. |
| R2 | `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | AC "restore called when no prior dispatcher existed". Begin transaction; `Install(null)` to force a known null baseline; `Ensure`; dispose the `Ensure` scope; dispose the transaction. | `Current` is non-null after `Ensure`; `null` after disposing the `Ensure` scope; original after disposing the transaction. |
| R3 | `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | AC "restore called twice". R2's shape plus a second `Dispose()`. | Second `Dispose` does not throw; `Current` unchanged between the two disposals. |
| R4 | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | AC "two callers racing install+restore in parallel". Task A begins a transaction and installs `liveA`, signals an MRE, waits for permission; Task B calls `BeginTransactionAsync` and records `Current` **immediately on acquisition, before installing**; main releases A, awaits B. | B's recorded value equals the original, **never** `liveA`. Guaranteed by the restore-before-release ordering. |
| R5 | `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` | Double-dispose of a transaction. | No `SemaphoreFullException`; a subsequent `BeginTransactionAsync()`/`Dispose()` round trip completes within the `[Timeout]`. |

**Honest limitation of R4 — stated rather than overclaimed.** Under a *correct* implementation R4
passes deterministically. Under a *broken* implementation (gate removed) it fails only
**probabilistically**, because nothing can force Task B to reach the acquisition point while Task A
still holds the gate, and there is no deterministic way to prove "B is currently blocked" without a
timed wait, which the determinism rules forbid. **R1 is therefore the primary regression assertion
and R4 is a supporting one.** R1 proves the clobber itself is unreachable with no concurrency at
all, and the clobber — not the scheduling — is the actual #230 mechanism.

**Fail-before evidence.** R1 and R2 fail against the current code *by construction*:
`EnsureUiThreadDispatcher` returns `void` at `HEAD`, so they will not compile against it. The plan
must capture fail-before evidence as a two-step artifact — the pre-change source excerpt at
`TestSupport.cs:238-249` plus a compile-level demonstration — rather than claiming a red test run
that cannot exist as compiled code.

**Edge cases and negative scenarios covered.** Field currently `null` (R2); field currently holding
a live foreign dispatcher (R1); double dispose of the ensure-scope (R3); double dispose of the
transaction, including the `SemaphoreFullException` over-release path (R5); a second `Install` on
the same transaction (must throw `InvalidOperationException`) — the planner should add this as a
sixth small test if it fits within the file's projected size, or fold the assertion into R5.

**Error-handling verification.** `Install`-twice throws `InvalidOperationException`; both `Dispose`
paths are verified not to throw on a second call (R3, R5). The `FieldInfo` existence assertion is
exercised implicitly by every test.

**Coverage impact and targets.** Test-only change: no production line is added or altered, so there
is no production coverage delta to defend. The new fixture is test infrastructure and sits inside
the test-file exclusion. No repository-wide coverage floor is asserted as an acceptance criterion
for this feature, because no production code is in the diff.

**Toolchain commands to run (in this exact order, restart from step 1 on any failure or auto-fix).**

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Notes for the executor: step 3 must **not** add `/p:Nullable=enable` (CLAUDE.md § C#1.3 — that
property is a solution-wide opt-in that conscripts files which never adopted the pragma, and CI
omits it deliberately). Step 4 must be run with the same `/InIsolation` flag CI uses and must
exclude `.claude` worktree copies of the test assemblies. A supplementary run using
`TaskMaster.runsettings` (class-level parallelization) is recommended for this feature specifically,
because the CI invocation is sequential and would not exercise the concurrency the fix targets;
record it as supporting evidence, not as the gating run.

**Manual validation steps.** None required.

## Acceptance Criteria

Every item below is traceable to the early-draft criteria in
`docs/features/potential/promoted/2026-08-07-uithread-dispatcher-static-swap-no-restore.md:49-53,57-58`
and to the traceability table in research §10. Items marked **[spec addition]** are added by this
spec and carry a stated justification.

- [x] **AC-1 — Restore exists and is idempotent.** `EnsureUiThreadDispatcher` returns an
  `IDisposable` scope whose `Dispose` restores the previous `UiThread._dispatcher` value, and a
  second `Dispose` neither re-writes the field nor throws. Restores are conditional
  (`ReferenceEquals` compare-then-write), and a call that performed no install returns a no-op
  scope. Evidenced by tests R2 and R3.
  *(Promoted doc criterion 1; research §2.4, §2.5 items 3/4/6.)*
- [x] **AC-2 — Concurrent callers cannot interleave install and restore against the shared static.**
  Every mutation of `UiThread._dispatcher` inside `QuickFiler.Test`'s owned files goes through
  `UiThreadDispatcherFixture` and holds `FieldLock` for the whole read-modify-write; long
  install→test-body→restore transactions additionally hold `TransactionGate`. `EnsureDispatcher`
  never acquires `TransactionGate`. Lock ordering is `TransactionGate` → `FieldLock`, never the
  reverse. Evidenced by tests R1 and R4.
  *(Promoted doc criterion 2; research §2.2, §2.3.)*
- [x] **AC-3 — A bounded regression test demonstrates the #230 deadlock scenario is unreachable.**
  Regression tests R1-R5 exist in
  `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, each carrying
  `[Timeout(GateTimeoutMs)]` with `GateTimeoutMs = 60000`, so a regression fails rather than hangs.
  R1 is recorded in the test's own documentation as the primary deterministic assertion and R4 as
  the supporting probabilistic one.
  *(Promoted doc criterion 3; research §5.)*
- [x] **AC-4 — The #230 local workaround is removed, not duplicated.**
  `QfcItemController.InitializationTests.Part2.cs` no longer declares its own
  `SemaphoreSlim UiThreadDispatcherGate` (`:51`) and no longer declares its own
  `SwapUiThreadDispatcher` (`:143-158`); both are replaced by calls into the shared fixture. Exactly
  one implementation of the reflection swap exists in the test assembly's owned files. The two-phase
  `BeginTransactionAsync` … `Install` shape is preserved so the gate is still acquired at build
  start, and `PumpHarness.Restore()` remains idempotent with restore-before-release ordering.
  *(Promoted doc criterion 4; research §2.6.)*
- [x] **AC-5 — No `Thread.Sleep`, `Task.Delay`, or wall-clock waits are introduced.** All
  cross-thread coordination in the new and modified files uses `ManualResetEventSlim` or awaited
  `Task` completion. No temporary files are created.
  *(Promoted doc criterion 5; `.claude/rules/general-unit-test.md`; research §5.)*
- [x] **AC-6 — `QfcItemController.FocusAndThemeTests.cs` is unmodified and unregressed.** The file
  is byte-identical to its base-branch version (still 497 lines), both call sites at `:452` and
  `:468` compile unchanged against the new `IDisposable` return type, and both
  `SetThemeDark_FromNormal_SelectsDarkNormalTheme` and
  `SetThemeLight_FromNormal_SelectsLightNormalTheme` pass. No analyzer diagnostic is raised at
  either call site under toolchain steps 2 and 3.
  *(Promoted doc "existing callers must be audited" constraint; issue.md § Constraints; research §1.5, §3.)*
- [x] **AC-7 — `UtilitiesCS/Threading/UiThread.cs` is unmodified.** The file does not appear in the
  feature's diff, no `InternalsVisibleTo("QuickFiler.Test")` grant is added to `UtilitiesCS`, and no
  production assembly is changed by this feature.
  *(Research §6; issue.md § Constraints — the conditional permission is deliberately not exercised.)*
- [x] **AC-8 — Every owned and new file is at or under 500 lines.** Measured after implementation:
  `QfcItemController.TestSupport.cs`, `QfcItemController.InitializationTests.Part2.cs`,
  `QfcItemController.UiThreadDispatcherFixture.cs`, and
  `QfcItemController.UiThreadDispatcherFixtureTests.cs`. The two `<Compile Include>` entries are
  added in the `Qfc*` neighbourhood of `QuickFiler.Test/QuickFiler.Test.csproj` immediately after
  the `QfcItemController.TestSupport.cs` entry.
  *(Research §8; `.claude/rules/general-code-change.md` § File Size Limit. **[spec addition]** —
  justification: research §8 identifies the ceiling as a live constraint on this specific change,
  with `FocusAndThemeTests.cs` already at 497/500, so it must be gated rather than assumed.)*
- [x] **AC-9 — Full C# toolchain passes in a single final pass, in order.**
  `dotnet tool run csharpier check .` clean; the analyzer msbuild step clean; the
  `TreatWarningsAsErrors` msbuild step clean (without `/p:Nullable=enable`); and
  `vstest.console.exe ... /EnableCodeCoverage /InIsolation` green for `QuickFiler.Test`, using
  MSTest, Moq, and FluentAssertions only. The commands run are stated explicitly in the completion
  report.
  *(CLAUDE.md § CUT3 and § C# Toolchain; promoted doc's implicit delivery bar. **[spec addition]** —
  justification: the repository's mandatory toolchain loop is a delivery precondition for any code
  change and must be checkable as an acceptance item.)*
- [x] **AC-10 — Fail-before evidence is captured in the form the defect permits.** The evidence
  artifact records the pre-change source excerpt at `TestSupport.cs:238-249` and a compile-level
  demonstration that R1/R2 cannot build against the base branch, rather than asserting a red test
  run that cannot exist. Written to `<FEATURE>/evidence/<kind>/` per
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
  *(Research §5 "Fail-before evidence". **[spec addition]** — justification: the repo's Bugfix
  Workflow requires a failing regression test first; this criterion records the specific, honest
  form that requirement takes when the fix is a signature change, so an executor does not fabricate
  a red run.)*

## Risks & Mitigations

**Delivery risks and mitigations**

| Risk | Mitigation |
| --- | --- |
| The return-type change breaks an unaudited caller. | Repo-wide grep confirmed exactly two call sites, both in `FocusAndThemeTests.cs` (research §1.5). Source and analyzer compatibility verified against `.editorconfig` and both msbuild steps (research §3). AC-6 gates it. |
| A `SemaphoreSlim`-based design deadlocks or blocks an un-`[Timeout]`-ed test. | Rejected the single-gate hypothesis for exactly this reason; the two-lock design keeps `EnsureDispatcher` off `TransactionGate` entirely (research §2.1, §2.2). |
| Line-count projections prove optimistic and a file exceeds 500 lines. | Two-new-file layout gives ≈114-160 lines headroom in both owned files; AC-8 re-measures after implementation. |
| The regression tests serialize against the pump fixtures and slow local runs. | Accepted; each pump test is `[Timeout]`-bounded at 60 s and R1-R5 hold the gate briefly (residual risk R-5). |
| The fix is unverifiable in CI because CI runs sequentially. | R1 is deterministic and requires no concurrency at all, so it verifies the actual mechanism under the CI invocation. The parallelized runsettings run is supporting evidence only (research §1.7, §5). |

**Accepted residual risks — out of scope for this feature, report-only (research §9)**

These are explicitly **not** defects this feature must fix. They are recorded so a reviewer does not
mistake them for gaps in the delivery.

- **R-1 — `WpfUiDispatcherTests.cs` remains an ungated mutator.** `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:42-51,83`
  swaps `UiThread._dispatcher` to a *running* dispatcher with a plain `finally` restore and no
  participation in either lock. After this fix it can still lose an update against a transaction.
  The file is not in this feature's owned set. Candidate follow-up: "route `WpfUiDispatcherTests`'
  static swap through the shared `UiThreadDispatcherFixture`". Low risk in CI (that assembly runs
  sequentially there) and it is a single-class, short-lived swap.
- **R-2 — Cross-assembly mutators.** `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:421`,
  `ProgressTrackerAsync_Tests.cs:137`, and `IdleAsyncQueue_Tests.cs:143` mutate the same
  process-wide static. Relevant only if a single test host loads both assemblies. No test-side lock
  inside `QuickFiler.Test` can reach them.
- **R-3 — `EmailMoveMonitorTests`' cleanup invariant.** `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:32-60`
  snapshots `UiThread.Dispatcher` in `[TestInitialize]` and asserts `BeSameAs` in `[TestCleanup]`.
  The recommended design does not change the field's steady-state value relative to today, because
  the unowned `Ensure` callers still discard their scope and still leak the parked dispatcher, so
  the exposure is unchanged. Stated explicitly because a "make `Ensure` always restore" design
  *would* have changed it.
- **R-4 — `[DoNotParallelize]` is not mutual exclusion in this repo.** Per the measured finding in
  `.claude/agent-memory/atomic-executor/project_mstest_donotparallelize_overlaps_parallel_bucket.md`,
  `[DoNotParallelize]` classes do overlap the parallel bucket in this repo's adapter. Do not accept
  `[DoNotParallelize]` as an alternative to the gate during review.
- **R-5 — Gate contention cost.** R1-R5 hold `TransactionGate` briefly; under the runsettings-forced
  parallel configuration they serialize against the pump tests (each ≤ 60 s). Negligible, but a
  real serialization the plan should acknowledge.

## Rollout & Follow-up

**Release / rollout steps**

- Standard branch → pull request into `epic/quickfiler-bug-family-integration`, referencing #493.
  No feature flag, no staged rollout, no runtime configuration. The change is confined to the test
  assembly plus two `<Compile Include>` lines.
- No production behavior changes, so no post-release monitoring applies.

**Post-fix clean-up and follow-up items**

1. **Injectable-seam conversion — deferred, do not attempt here (research §7).** The promoted doc
   floats replacing the mutable static with an injectable seam and asks that it be evaluated rather
   than assumed. Evaluation result: the seam already exists and is partially adopted
   (`UtilitiesCS/Threading/IUiDispatcher.cs`, `UtilitiesCS/Threading/WpfUiDispatcher.cs`, whose
   default constructor is literally `: this(() => UiThread.Dispatcher)` at `WpfUiDispatcher.cs:25`,
   and `QfcItemController._uiDispatcher`, which the pump fixture already injects at
   `Part2.cs:110-114`). Remaining static consumers number approximately **62 references across 29
   first-party production files**, concentrated in `QuickFiler/Controllers/QfcCollectionController.cs`
   (8), `QuickFiler/Controllers/QfcQueue.cs` (4), `QuickFiler/Helper Classes/ItemViewerQueue.cs` (4),
   `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` (4), and ~25 other files. Converting them is a
   multi-phase production refactor across three assemblies with a live VSTO surface, no behavioral
   defect of its own, and no bounded blast radius. It is disproportionate to a test-isolation bug
   and would swamp the regression evidence for #493.

2. **Overlap check against issue #584 — result of the check requested by research §7.** Research
   §7 flagged `#584 "UiThread.Dispatcher null race"` as adjacent and asked that overlap be confirmed
   before promoting a duplicate. Findings from reading the repository:
   - `docs/features/epics/quickfiler-suite-determinism-foundation/epic-status.md:168` lists #584 as
     a follow-up issue promoted during that epic by child #449, and `:97`/`:117` record it as
     deliberately left OPEN.
   - The archived policy audit
     `docs/features/archive/2026-08-07-quickfiler-explorer-controller-latent-defects-449/policy-audit.2026-08-22T10-58.md:75`
     gives #584's actual root cause: a `UtilitiesCS.Test` flake
     (`ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`,
     NRE at `UtilitiesCS/Threading/ProgressTrackerAsync.cs:35`) promoted with the structural
     analysis that "`UiThread.Dispatcher` [is] backed by a `null!`-initialised static with no lazy
     initialisation".
   - **Assessment: adjacent, materially overlapping, not identical.** #584 is about the *null* state
     of the static (an NRE at consumers when nothing has initialized it); #493 is about *unrestored
     and unsynchronized mutation* of the same static by tests. They share a root object and would
     share a remedy: the injectable-seam conversion would dissolve both. #584's recorded structural
     analysis already names the static itself as the defect, which is the same target the seam
     conversion would address.
   - **Recommendation: do NOT promote a new issue for the seam conversion.** Instead, record the
     seam-conversion scope (the ~62 references / 29 files measurement above) as a comment on the
     existing #584 and, if the maintainer agrees, widen #584's title and body to cover
     "replace the `UiThread.Dispatcher` static with the existing `IUiDispatcher` seam", citing #493
     as the second motivating defect. This avoids a third issue tracking the same static. If the
     maintainer prefers #584 to stay scoped to the null race specifically, then promote the seam
     conversion as a new issue and cross-link both.

3. **Consider promoting residual risk R-1** ("route `WpfUiDispatcherTests`' static swap through the
   shared `UiThreadDispatcherFixture`") as its own small issue once this fix lands, since the shared
   fixture it would call into will then exist. Do not fold it into this feature — the file is not in
   the owned set.

4. **Ambiguities flagged for the planner** (research is otherwise complete and unambiguous):
   - Research §5 does not name a test for the "`Install` called twice throws
     `InvalidOperationException`" contract stated in §2.4, although the contract itself is explicit.
     The Test Strategy above proposes adding it as a sixth test or folding it into R5; the planner
     should choose one and state it. This spec does not invent a resolution.
   - Line-count figures in research §8 are projections; the planner should treat AC-8 as requiring a
     fresh measurement rather than a restatement of the projection.

**Links**

- Issue: https://github.com/drmoisan/TaskMaster/issues/493
- Promoted record: `docs/features/potential/promoted/2026-08-07-uithread-dispatcher-static-swap-no-restore.md`
- Research: `docs/features/active/quickfiler-test-uithread-dispatcher-493/research/2026-08-24T11-05-uithread-dispatcher-restore-scope-research.md`
- Origin of the defect report: issue #230 / PR #479 (WinForms message-pump test seam)
- Adjacent open issue: #584 (`UiThread.Dispatcher` null race)
- Epic: `quickfiler-bug-family`, integration branch `epic/quickfiler-bug-family-integration`
