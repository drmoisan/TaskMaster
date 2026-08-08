# F4 per-file research — `QuickFiler/Helper Classes/ItemViewerQueue.cs`

Timestamp: 2026-08-07T22-40

Cluster: VIEWER-QUEUE. Companions: `09-ViewerQueueCore.md` (the generic core this file wraps),
`11-EfcViewerQueue.md` (the sibling wrapper), `12-QfEnums.md`. Cross-cutting facts are established in
`00-cluster-overview.md` and are cited, not re-derived.

Upstream contract: F1 owns the per-file coverage harness and the ratified exemption ledger at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`; neither exists on disk yet.
Authoritative numbers come from F1's harness at execution time. Numbers below are read from an
already-committed feature-#424 Cobertura artifact and are labelled indicative.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/ItemViewerQueue.cs` | — |
| Line count | **123** | file ends at `:123`; matches `epic.md:281` |
| 500-line limit | 123 / 500, **377 lines of headroom** | `.claude/rules/general-code-change.md` § File Size Limit |
| Compiled | yes — `<Compile Include="Helper Classes\ItemViewerQueue.cs" />` | `QuickFiler/QuickFiler.csproj:349` |
| `[ExcludeFromCodeCoverage]` | **absent** — confirmed by full read; no attribute on the type or any member | `ItemViewerQueue.cs:1-123` |
| Type declaration | `public static class ItemViewerQueue` | `:9` |
| Namespace | `QuickFiler` | `:7` |
| Internals visible to tests | yes | `QuickFiler/Properties/AssemblyInfo.cs:5` |
| Banned APIs (`Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `Random.Shared`) | **none present** | full read; no time dependence anywhere |

### 1.1 Relationship to `ViewerQueueCore<TViewer>`

`ItemViewerQueue` is a **thin static façade** over exactly one `ViewerQueueCore<ItemViewer>`
instance. It contributes no queue logic of its own. Its entire job is to (a) own the singleton core
(`:29`), (b) supply the four production collaborator delegates (`:11-27`), and (c) forward each
public call to the core with **hard-coded** `DispatcherPriority` and replacement-count arguments:

| Façade member | Forwards to | Hard-coded arguments |
| --- | --- | --- |
| `BuildQueueWhenIdle(int)` `:31-34` | `ViewerQueueCore.BuildQueue(int, DispatcherPriority)` | `ContextIdle` |
| `BuildQueueBackground(int)` `:36-39` | same | `Background` |
| `BuildQueue(int)` `:41-44` | `ViewerQueueCore.BuildQueue(int)` (synchronous overload) | none |
| `Dequeue(CancellationToken)` `:46-55` | `ViewerQueueCore.Dequeue` | `emptyQueuePriority = Render`, `cachedReplacementCount = 1`, `emptyReplacementCount = 1`, `replacementPriority = ContextIdle` |
| `DequeueChunk(int)` `:57-64` | `ViewerQueueCore.DequeueChunk` | `missingViewerPriority = Render`, `replacementPriority = ContextIdle` |

Those hard-coded values are the *only* behaviour this file owns, so they are exactly what its tests
must pin. All ordering/capacity/FIFO invariants belong to the core and are analysed in
`09-ViewerQueueCore.md` §6; this artifact does not restate them, it asserts the argument mapping.

### 1.2 Static mutable state — first-class finding

`ItemViewerQueue` is a `static` class that holds **five pieces of mutable process-global state**:

| # | Member | Line | Mutability | Risk |
| --- | --- | --- | --- | --- |
| S1 | `internal static Func<ItemViewer> ProductionViewerFactory { get; set; }` | `:11-12` | settable from any test | a leaked test factory changes what production code constructs |
| S2 | `internal static Action<Action> ProductionSynchronousScheduler { get; set; }` | `:14-15` | settable | a leaked no-op scheduler silently stops all synchronous enqueues |
| S3 | `internal static Action<Action, DispatcherPriority> ProductionPriorityScheduler { get; set; }` | `:17-21` | settable | as above for priority enqueues |
| S4 | `internal static Action<Action, DispatcherPriority> ProductionBlockingPriorityScheduler { get; set; }` | `:23-27` | settable | as above for the blocking path |
| S5 | `private static ViewerQueueCore<ItemViewer> _core` | `:29` | replaced by `SetCoreForTesting` `:69-72` / `ResetCoreForTesting` `:77-81` | **the queue itself, including queued viewer instances, survives across tests** |

This is a determinism and test-isolation hazard: any test that writes S1-S5 and does not restore
them changes the observed behaviour of every later test in the assembly. `.claude/rules/general-unit-test.md`
§ Core Principles requires independence ("run in any order") and isolation. §7 below analyses the
current mitigation and proposes the reset/injection seam.

---

## 2. Member inventory (coverage denominator)

Decision points: `if`, ternary, `??`, `?.`, loops, `catch`. There is no `switch`, no `await`, no
`lock`, and no `catch` in the file.

| # | Member | Signature | Lines | Decision points |
| --- | --- | --- | --- | --- |
| 1 | property + initializer | `internal static Func<ItemViewer> ProductionViewerFactory { get; set; } = CreateProductionViewer` | 11-12 | 0 |
| 2 | property + initializer lambda | `internal static Action<Action> ProductionSynchronousScheduler { get; set; } = action => action()` | 14-15 (lambda body `:15`) | 0 |
| 3 | property + initializer lambda | `ProductionPriorityScheduler = (action, priority) => _ = UiThread.Dispatcher.InvokeAsync(action, priority)` | 17-21 (lambda body `:21`) | 0 |
| 4 | property + initializer lambda | `ProductionBlockingPriorityScheduler = (action, priority) => UiThread.Dispatcher.Invoke(action, priority)` | 23-27 (lambda body `:27`) | 0 |
| 5 | field + initializer | `private static ViewerQueueCore<ItemViewer> _core = CreateProductionCore()` | 29 | 0 |
| 5a | implicit `.cctor` | compiler-generated; sequence points at `:12, 15, 21, 27, 29` | — | 0 |
| 6 | method | `public static void BuildQueueWhenIdle(int count)` | 31-34 | 0 |
| 7 | method | `public static void BuildQueueBackground(int count)` | 36-39 | 0 |
| 8 | method | `public static void BuildQueue(int count)` | 41-44 | 0 |
| 9 | method | `public static ItemViewer Dequeue(CancellationToken token)` | 46-55 | 0 |
| 10 | method | `public static IEnumerable<ItemViewer> DequeueChunk(int count)` | 57-64 | 0 |
| 11 | method | `internal static void SetCoreForTesting(ViewerQueueCore<ItemViewer> core)` | 69-72 | **1** (`??` throw at `:71`) |
| 12 | method | `internal static void ResetCoreForTesting()` | 77-81 | 0 |
| 13 | method | `internal static void ResetProductionCoreDefaultsForTesting()` | 83-91 | 0 (four assignments; three lambdas at `:86`, `:88`, `:90`) |
| 14 | method | `private static ViewerQueueCore<ItemViewer> CreateProductionCore()` | 93-101 | 0 |
| 15 | method | `private static ItemViewer CreateProductionViewer()` | 103-106 | 0 |
| 16 | method | `internal static ViewerQueueCore<ItemViewer> CreateProductionCore(Func<ItemViewer>, Action<Action>, Action<Action,DispatcherPriority>, Action<Action,DispatcherPriority>)` | 108-121 | 0 |

Totals: 1 static type, 4 static properties, 1 static field, 11 methods (5 public, 4 internal,
2 private), 6 lambdas (three in the `.cctor` at `:15/21/27`, three in
`ResetProductionCoreDefaultsForTesting` at `:86/88/90`). **1 decision point in the whole file.**

Cobertura reports **64 distinct sequence-point lines** for the main `QuickFiler.ItemViewerQueue`
class plus a compiler-generated closure class `QuickFiler.ItemViewerQueue.<>c` carrying the six
lambda bodies (`:15, 21, 27, 86, 88, 90`). Aggregation caveat for F1's harness: those six line
numbers appear in **both** class elements — in the main class as the *assignment* site (hit) and in
`<>c` as the *lambda body* (not hit). A per-file aggregation must merge classes sharing the same
`filename` and take the **maximum** hit count per line number, or it will produce a different (and
harsher) denominator. Evidence for the two-class shape:
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml:25839-25880`
(`name="QuickFiler.ItemViewerQueue.&lt;&gt;c"`, `line-rate="0"`, six lines `15, 21, 27, 86, 88, 90`).
The denominator convention is **F1's to fix**; this artifact reports the gap under both readings in
§13.

---

## 3. Existing test inventory

There is **no** `ItemViewerQueueTests.cs`. The type is exercised only by
`QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs` (336 lines, declared at
`QuickFiler.Test/QuickFiler.Test.csproj:165`), class `[DoNotParallelize] [TestClass]
ViewerQueueStaticWrapperTests` (`:11-13`) in namespace `QuickFiler.Test.HelperClasses`. That class
holds 8 `[TestMethod]`s split between the two wrappers; **four touch `ItemViewerQueue`.**

| Test method / fixture | Line | `ItemViewerQueue` members exercised |
| --- | --- | --- |
| `[TestCleanup] Cleanup()` | `:15-22` | `ResetProductionCoreDefaultsForTesting()` `:20` → `:83-91`; `ResetCoreForTesting()` `:21` → `:77-81` (and transitively `CreateProductionCore()` `:93-101`) |
| `ItemViewerQueue_BuildMethods_DelegateToInjectedCore` | `:88-116` | `SetCoreForTesting` `:105` happy path; `BuildQueueWhenIdle(1)` `:107`; `BuildQueueBackground(1)` `:108`; `BuildQueue(1)` `:109`. Asserts `created == 3`, `Count == 3`, and the recorded priorities `ContextIdle, Background` (only two entries, because `BuildQueue(1)` routes through the *synchronous* scheduler — `:113-115`) |
| `ItemViewerQueue_DequeueAndChunk_DelegateToInjectedCore` | `:118-154` | `Dequeue(CancellationToken.None)` `:143`; `DequeueChunk(2)` `:144`. Asserts `blockingPriorities == [Render, Render]` and `scheduledPriorities == [ContextIdle, ContextIdle]` (`:150-153`) |
| `ItemViewerQueue_CreateProductionCore_UsesProvidedDelegates` | `:198-236` | `CreateProductionCore(4 delegates)` `:204` → `:108-121`; then drives the *returned* core directly (`:223`), not the static one |
| `ItemViewerQueue_ResetCoreForTesting_UsesResettableProductionDefaults` | `:271-300` | writes `ProductionViewerFactory` `:277`, `ProductionPriorityScheduler` `:282`, `ProductionBlockingPriorityScheduler` `:287`; `ResetCoreForTesting()` `:293`; `Dequeue` `:294`. Proves S1/S3/S4 are honoured when the core is rebuilt |

Test-support helpers used: `CreateItemCore` (`:316-328`) constructs `ViewerQueueCore<ItemViewer>`
with **four** arguments — i.e. **no dispose delegate**; and `CreateUninitialized<TViewer>`
(`:330-334`) uses `System.Runtime.Serialization.FormatterServices.GetUninitializedObject` to
materialise an `ItemViewer` without running its constructor. That technique is what lets these tests
avoid constructing a live `UserControl` (see §5.2 for the caveat).

**Not exercised anywhere today:** `SetCoreForTesting(null)` throw path; `ProductionSynchronousScheduler`
default lambda; the restored-defaults lambdas; `CreateProductionViewer`.

---

## 4. Per-member coverage gap

Indicative baseline: feature-#424 artifact
`.../424/evidence/qa-gates/coverage-final.cobertura.xml:2394-2613`, class
`QuickFiler.ItemViewerQueue`, recorded `line-rate="0.936508"`, `branch-rate="0.9"`. Line numbers in
that artifact align exactly with the current file, so the file is unchanged since that run.

Zero-hit lines in the main class: **`:88`, `:90`, `:104`, `:105`, `:106`** (artifact `:2585, 2591,
2601-2603`). Zero-hit lines in the `<>c` closure class: `:15, 21, 27, 86, 88, 90` (baseline artifact
`:25872-25879`).

| Member | Status | Detail |
| --- | --- | --- |
| `.cctor` `:12,15,21,27,29` | covered (assignments) | but the **lambda bodies** at `:15`, `:21`, `:27` are uncovered in `<>c` |
| `BuildQueueWhenIdle` `:31-34` | covered | `ViewerQueueStaticWrapperTests.cs:107` |
| `BuildQueueBackground` `:36-39` | covered | `:108` |
| `BuildQueue` `:41-44` | covered | `:109` |
| `Dequeue` `:46-55` | covered | `:143` — **only with `CancellationToken.None`**; the token is never observed to be honoured through the façade |
| `DequeueChunk` `:57-64` | covered | `:144` |
| `SetCoreForTesting` `:69-72` | **partially covered (branch missed)** | line `:71` at `condition-coverage="50%"` (artifact `:2559-2563`) — the `ArgumentNullException` throw is never taken |
| `ResetCoreForTesting` `:77-81` | covered | `:21`, `:293` |
| `ResetProductionCoreDefaultsForTesting` `:83-91` | **partially covered** | method lines hit; lambda bodies at `:88` and `:90` **uncovered** (host-bound); lambda body at `:86` uncovered in `<>c` (coverable) |
| `CreateProductionCore()` `:93-101` | covered | via `ResetCoreForTesting` |
| `CreateProductionViewer()` `:103-106` | **uncovered** | `:104-106` zero hits — `new ItemViewer()` |
| `CreateProductionCore(4)` `:108-121` | covered | `:204` |

---

## 5. Testability classification per member

| Member | Classification | Reasoning |
| --- | --- | --- |
| `BuildQueueWhenIdle`, `BuildQueueBackground`, `BuildQueue`, `Dequeue`, `DequeueChunk` | `pure-testable-now` | each is a one-line forward to the injectable core; `SetCoreForTesting` `:69` is the existing seam |
| `SetCoreForTesting` (both paths) | `pure-testable-now` | pass `null` to reach `:71` |
| `ResetCoreForTesting` | `pure-testable-now` | already covered |
| `ResetProductionCoreDefaultsForTesting` (method body) | `pure-testable-now` | already covered |
| `ProductionSynchronousScheduler` default lambda `:15` and its restored twin `:86` (`action => action()`) | `pure-testable-now` | pure delegate — read the property and invoke it with a flag-setting action |
| `CreateProductionCore()` and `CreateProductionCore(4)` | `pure-testable-now` | already covered |
| `ProductionPriorityScheduler` default lambda `:21` and restored twin `:88` | **`host-bound-irreducible`** | see §5.1 |
| `ProductionBlockingPriorityScheduler` default lambda `:27` and restored twin `:90` | **`host-bound-irreducible`** | see §5.1 |
| `CreateProductionViewer` `:103-106` | **`host-bound-irreducible`** | see §5.2 |

No member touches Outlook Interop. `Microsoft.Office.Interop.Outlook` does not appear in the file;
the `using` list is `System`, `System.Collections.Generic`, `System.Threading`,
`System.Windows.Threading`, `UtilitiesCS` (`:1-5`). The Moq-on-Interop precedent catalogue in
`00-cluster-overview.md` §3 is therefore not needed for this file.

### 5.1 Why the two dispatcher lambdas are irreducible

Both bodies dereference `UtilitiesCS.Threading.UiThread.Dispatcher`
(`UtilitiesCS/Threading/UiThread.cs:135-140`), a `public static Dispatcher` whose backing field is
`private static Dispatcher _dispatcher = null!; // set in Initialize() before any access`
(`UiThread.cs:140`). Invoking either lambda in a unit test therefore requires initialising the
process-global `UiThread` singleton on a real WPF UI thread. Two independent blockers:

1. `System.Windows.Threading.Dispatcher` is **sealed** — Moq cannot proxy it, so no fake dispatcher
   can be supplied.
2. Initialising `UiThread` is process-global mutable state and pulls in `ThreadMonitor`
   (`UiThread.cs:145`) and auto-scale probing (`UiThread.cs:147-159`), well beyond the epic's STA
   allowance for "never-shown in-memory WinForms controls" (`epic.md` Shared Design §3).

Line cost: 4 lines (`:21`, `:27`, `:88`, `:90`).

### 5.2 Why `CreateProductionViewer` is irreducible

`:105` is `return new ItemViewer();`. `ItemViewer` is declared
`public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal`
(`QuickFiler/Viewers/ItemViewer.cs:21`) with `InitializeComponent` in a 6,224-line designer
(`QuickFiler/Viewers/ItemViewer.Designer.cs`). Constructing it is live-control construction, which
`epic.md` Shared Design §2 forbids in unit tests; the type is also **F14-owned**, so any change to
make it constructible would be a cross-child edit. Line cost: 3 lines (`:104-106`).

Note on the existing workaround: `ViewerQueueStaticWrapperTests.cs:330-334` produces `ItemViewer`
instances with `FormatterServices.GetUninitializedObject`, which allocates the object without running
any constructor. That is the reason the existing tests can name the concrete type at all, and it is
already-shipped practice in this file — do not remove it. One caveat to record: an uninitialised
`Control` still has a finaliser inherited from `System.ComponentModel.Component`, and
`Control.Dispose(bool)` reads fields that are `null` on such an object, so a finaliser-thread
exception is a theoretical (unobserved) process-stability risk. Mitigation, entirely inside F4-owned
test files and cheap: call `GC.SuppressFinalize` on the object inside the
`CreateUninitialized<TViewer>` helper, and prefer `ViewerQueueCore<FakeViewer>` (the
`09-ViewerQueueCore.md` pattern) wherever the API under test does not force the concrete type. This
is a recommendation, not a defect claim — the risk is reasoned from the framework's disposal
contract, not observed in a run.

---

## 6. Ordering, concurrency and static-state invariants

Queue-shape invariants (FIFO, no capacity limit, empty-queue behaviour, disposal, duplicate
enqueue, absence of synchronisation) belong to `ViewerQueueCore<TViewer>` and are enumerated with
evidence in `09-ViewerQueueCore.md` §6 (I1-I11). They are **not** restated here. The invariants this
file owns:

**W1 — Argument mapping is the file's entire contract.** `Dequeue` always passes
`(Render, 1, 1, ContextIdle)` (`:48-54`) and `DequeueChunk` always passes `(Render, ContextIdle)`
(`:59-63`). Deterministic test: inject a core built with recording schedulers and assert the exact
recorded priority sequence and resulting depth (I1 in §11: I5/I6/I7 pin each mapping).

**W2 — `BuildQueue(int)` is the only synchronous build.** It forwards to the core's *synchronous*
overload (`:43`), so it does **not** consult `ProductionPriorityScheduler`. Evidenced today by the
existing assertion at `ViewerQueueStaticWrapperTests.cs:113-115`, where three build calls produce
only two recorded priorities. Deterministic test: covered; not duplicated.

**W3 — Production priority builds are fire-and-forget.** The default
`ProductionPriorityScheduler` is `(action, priority) => _ = UiThread.Dispatcher.InvokeAsync(action, priority)`
(`:21`) — the discard makes the returned `DispatcherOperation` unobserved. Consequence:
`BuildQueueWhenIdle(n)` / `BuildQueueBackground(n)` return before any viewer exists, and the queue is
mutated later on the UI thread while `QuickFiler/Controllers/QfcQueue.cs:336` and
`QuickFiler/Controllers/QfcCollectionController.cs:617, 958` may `Dequeue` from another thread. This
is the same latent defect recorded as I8 in `09-ViewerQueueCore.md` §6, observed here at its
production wiring point. **Out of F4 scope** (no behaviour change; the fix would touch F2- and
F11-owned files). Recommended action: promote a separate issue. Deterministic test in F4: inject a
*deferring* scheduler that records but does not invoke, then assert `Count == 0` after
`BuildQueueWhenIdle(2)` — this makes the fire-and-forget semantics executable documentation without
changing behaviour (test I8 in §11).

**W4 — No thread-safety on the static state itself.** S1-S5 are plain static properties/fields with
no `volatile`, no `Interlocked`, no `lock`. Concurrent test classes writing them would race. The
mitigation is `[DoNotParallelize]`, already applied at `ViewerQueueStaticWrapperTests.cs:11`, and it
must be applied to every new test class that touches this type (§7).

**W5 — Static initialisation order is well-defined and safe.** Static field initialisers run in
declaration order, so `:11-27` (the four production delegates) are assigned before `:29`
(`_core = CreateProductionCore()`), which reads them at `:95-100`. First touch of the type therefore
builds a production core wired to the real dispatcher delegates — but constructs **no viewer**,
because `ViewerQueueCore`'s constructor only stores delegates
(`ViewerQueueCore.cs:26-34`). Merely referencing `ItemViewerQueue` in a test is safe.

**W6 — No time dependence.** No `DateTime`, no `Task.Delay`, no `Thread.Sleep`, no timer.
`TimeProvider` / `FakeTimeProvider` (`00-cluster-overview.md` §4) is **not** required for this file.
No banned-API finding.

---

## 7. Static-state test-isolation analysis

### 7.1 Can a test leave residue that affects another test? Yes.

Concrete residue paths:

- **R1 — a leaked injected core.** `SetCoreForTesting(core)` (`:69-72`) replaces S5 permanently.
  A later test that calls `Dequeue` without injecting would drive the previous test's core, seeing
  its queued viewers and its recorder lists.
- **R2 — a leaked production delegate.** Writing `ProductionViewerFactory` /
  `ProductionPriorityScheduler` / `ProductionBlockingPriorityScheduler` (as
  `ViewerQueueStaticWrapperTests.cs:277, 282, 287` does) changes what *any* subsequent
  `ResetCoreForTesting()` builds.
- **R3 — the dangerous default.** If S1-S5 are all at their true production values and some test
  calls `ItemViewerQueue.BuildQueue(1)` or `Dequeue(...)` **without** first calling
  `SetCoreForTesting`, the production factory runs `new ItemViewer()` (`:105`) — a live `UserControl`
  — and the production priority/blocking schedulers dereference `UiThread.Dispatcher`, which is
  `null!` in a test process (`UtilitiesCS/Threading/UiThread.cs:140`) → `NullReferenceException`.
  That is simultaneously a live-form violation and a confusing failure mode.

### 7.2 How `ViewerQueueStaticWrapperTests.cs` handles it today, and whether that is sound

Current mechanism, two parts:

1. `[DoNotParallelize]` at class level (`:11`) — removes the cross-class race in W4.
2. `[TestCleanup] Cleanup()` (`:15-22`) calling, per wrapper, `ResetProductionCoreDefaultsForTesting()`
   **then** `ResetCoreForTesting()` (`:20-21` for this file).

Assessment:

- **The ordering inside `Cleanup` is correct.** Defaults must be restored *before* the core is
  rebuilt, because `ResetCoreForTesting` (`:77-81`) rebuilds from the *current* S1-S4 values via
  `CreateProductionCore()` (`:93-101`). The existing code gets this right. It is, however, an
  order-sensitive two-call protocol that a future caller can easily invert — a latent trap.
- **`[TestCleanup]` alone is not sufficient for order-independence.** It guarantees this class cleans
  up after itself, but it does **not** guarantee this class *starts* from a known state. Any other
  class — a future F4 test file, or a sibling child's test that happens to touch the wrapper — that
  mutates S1-S5 without cleanup would corrupt this class's first test. The policy requirement is that
  tests "run in any order without impacting each other"
  (`.claude/rules/general-unit-test.md` § Core Principles), which needs a *pre-condition* guarantee,
  not only a post-condition one. **Verdict: adequate today because grep shows no other file touches
  these statics, but not robust and not order-independent by construction.**
- **`ResetCoreForTesting` does not dispose queued viewers in practice.** It calls `_core.Reset()`
  (`:79`), but the cores built by `CreateItemCore` (`ViewerQueueStaticWrapperTests.cs:316-328`) and
  by `CreateProductionCore` (`:115-120`) supply **no** dispose delegate, so
  `ViewerQueueCore.Reset`'s `_disposeViewer?.Invoke` (`ViewerQueueCore.cs:122`) is a no-op and
  viewers are merely dropped. Harmless for `GetUninitializedObject` instances; worth asserting
  once so the behaviour is pinned (test I12 in §11).
- **The post-`Cleanup` state is the dangerous default of R3.** After cleanup, S5 is a production core
  wired to `new ItemViewer()` and `UiThread.Dispatcher`. Nothing prevents a later test from invoking
  it. A guard is warranted.

### 7.3 Proposed mechanism (satisfies independence and isolation without relying on order)

Three parts. Parts A and B are **test-only**; part C is a small additive production change.

**A. `[TestInitialize]` in addition to `[TestCleanup]`, in every test class that touches the type.**

```
[TestInitialize] public void Initialize() { ItemViewerQueue.ResetForTesting(); }
[TestCleanup]    public void Cleanup()    { ItemViewerQueue.ResetForTesting(); }
```

This makes each test start from a known state regardless of what ran before, which is the
order-independence requirement. It is additive to `ViewerQueueStaticWrapperTests.cs` (add an
`[TestInitialize]`; leave the existing `[TestCleanup]` body intact so #426-style follow-on work does
not rebase-conflict — `00-cluster-overview.md` §8).

**B. `[DoNotParallelize]` on every new test class that touches the type**, mirroring
`ViewerQueueStaticWrapperTests.cs:11`.

**C. One additive production member that makes the reset order impossible to get wrong:**

```csharp
/// <summary>
/// Restores the production collaborator delegates and then rebuilds the queue core from them.
/// Intended for test setup/teardown so each test starts from a known state.
/// </summary>
internal static void ResetForTesting()
{
    ResetProductionCoreDefaultsForTesting();
    ResetCoreForTesting();
}
```

Rationale: it removes the order-sensitive two-call protocol identified above, is `internal` (no
public-surface growth, `.claude/rules/csharp.md` § Public surface), adds 2 executable lines that the
new `[TestInitialize]` covers on every single test, and — critically — **changes no existing
signature**, so no call site anywhere needs editing (§9). The two existing methods remain for the
current callers at `ViewerQueueStaticWrapperTests.cs:20-21`.

**Explicitly rejected alternative — an injectable backing *instance* replacing the static class**
(for example `internal static IViewerQueue Instance { get; set; }` with the static methods
forwarding). It would give the cleanest isolation story, but it is a larger production change whose
value is already delivered by the existing `SetCoreForTesting` seam, and it invites redesign of a
`public static` surface consumed from three sibling-owned files (§9). Rejected on
simplicity-first grounds (`.claude/rules/general-code-change.md` § Design Principles) and
conflict-avoidance grounds.

---

## 8. Seam proposal

Ranked per `.claude/rules/csharp.md` § DI Seams (interface > injectable delegate > adapter) and
`epic.md` Shared Design §2.

**Selected — the seam already exists; add one convenience reset only.**

| Item | Detail |
| --- | --- |
| Existing seam tier | *injectable property defaulting to the real implementation* — `SetCoreForTesting` `:69-72` (state seam) plus the four `internal static` `Production*` delegate properties `:11-27` (collaborator seams) |
| Production default | `_core = CreateProductionCore()` `:29`; delegates default to `CreateProductionViewer`, `action => action()`, `UiThread.Dispatcher.InvokeAsync`, `UiThread.Dispatcher.Invoke` |
| New member proposed | `internal static void ResetForTesting()` (§7.3 part C) — 2 executable lines |
| Injection point | none new; the existing static properties |
| Sibling impact | **requires no sibling-owned file change** — the addition is a new `internal static` method; no existing signature or member is altered |

**Considered and rejected:**

1. **Reuse `UtilitiesCS.Threading.IUiDispatcher`** (`UtilitiesCS/Threading/IUiDispatcher.cs:15`) to
   make the two dispatcher lambdas (§5.1) coverable. Its members are `Invoke(Action)`,
   `InvokeAsync(Action)`, `InvokeAsync(Action, DispatcherPriority, CancellationToken)`,
   `BeginInvoke(Action)`, and two generic `InvokeAsync<TResult>` overloads (`:18-41`). It has
   **no** `Invoke(Action, DispatcherPriority)` and no token-free
   `InvokeAsync(Action, DispatcherPriority)`, so the current call shapes at `:21` and `:27` are not
   representable without adding members to a `UtilitiesCS` interface — a file outside F4's set and
   outside this epic entirely. Substituting the nearest existing members would change observable
   dispatch behaviour (priority dropped on the blocking path; `Task` instead of
   `DispatcherOperation`), which `issue.md:69` forbids. **Rejected.**
2. **An injectable `Func<Dispatcher>` accessor** with production default `() => UiThread.Dispatcher`,
   so tests could substitute `Dispatcher.CurrentDispatcher`. This would make `:21`/`:27`/`:88`/`:90`
   executable (the blocking `Invoke` on the current thread runs inline; `InvokeAsync` queues without
   a loop, which still executes the *call* line). **Rejected**: it buys 4 lines out of 64 in a file
   already comfortably above the floor (§13), attaches a live `Dispatcher` to a pooled MSTest worker
   thread (process residue, contrary to the isolation objective of §7), and adds a production seam
   with no production consumer. Recorded so the planner does not re-derive it.
3. **Make `CreateProductionViewer` injectable to cover `new ItemViewer()`.** It already is —
   `ProductionViewerFactory` (`:11-12`) *is* that seam; the 3 uncovered lines are the production
   default itself. Covering them requires constructing a real `ItemViewer` (F14-owned, live control).
   **Rejected as irreducible** (§5.2).
4. **Extract an `IViewerQueue` interface and convert the static class to a forwarding façade.**
   Rejected in §7.3.

---

## 9. Cross-child conflict analysis

F4's file set is the 13 files under `QuickFiler/Helper Classes/` plus
`QuickFiler/Interfaces/IEmailMoveMonitor.cs` (`epic.md:276-283`). Every other QuickFiler file belongs
to a sibling. Repository-wide grep for `ItemViewerQueue` yields:

| Call site | Sibling owner | What it uses | Constraint imposed |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcQueue.cs:336` — `var viewer = ItemViewerQueue.Dequeue(_token);` | **F2** (`epic.md:257`) | `public static ItemViewer Dequeue(CancellationToken)` | signature is pinned |
| `QuickFiler/Controllers/QfcCollectionController.cs:617` — `var itemViewer = ItemViewerQueue.Dequeue(_homeController.Token);` | **F11** (`epic.md:332`) | same | signature is pinned |
| `QuickFiler/Controllers/QfcCollectionController.cs:958` — `var itemViewer = ItemViewerQueue.Dequeue(_homeController.Token);` | **F11** | same | signature is pinned |
| `QuickFiler/Viewers/ItemViewer.cs:21` (type `ItemViewer`) | **F14** (`epic.md:357`) | referenced as a *type* in the return/generic position | no edit needed; do not change the returned type |
| `QuickFiler/Properties/Settings.settings:23` and `QuickFiler/Properties/Settings.Designer.cs:98-104` — setting `ItemViewerQueueSize` | **F15** (`epic.md:371`) | **name coincidence only** — no code path reads it into `ItemViewerQueue`; repository grep finds no consumer outside the generated settings pair | no coupling; note as an apparently orphaned setting, out of scope |
| `QuickFiler/QuickFiler.csproj:349` | shared | existing `<Compile Include>` line | no edit needed |
| `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:20-21, 89-154, 199-236, 272-300, 316-328` | **F4** | test-only | F4-owned |
| `QuickFiler.Test/QuickFiler.Test.csproj:165` | shared | existing line | no edit needed |

**Nothing outside F4 calls `BuildQueue`, `BuildQueueWhenIdle`, `BuildQueueBackground`, or
`DequeueChunk`.** The only externally consumed member is `Dequeue(CancellationToken)`, from three
sibling-owned lines.

Explicit per-proposal statement:

- Selected proposal (add `internal static ResetForTesting()`): **requires no sibling-owned file
  change.** It adds a member; `Dequeue(CancellationToken)` and every other existing signature are
  untouched, so `QfcQueue.cs:336` and `QfcCollectionController.cs:617, 958` continue to compile
  byte-identically.
- Rejected proposal 2 (`Func<Dispatcher>` accessor): would also require no sibling change (a new
  `internal static` property), but is rejected on the merits above.
- Rejected proposal 4 (`IViewerQueue` façade): would keep the static methods and therefore also
  compile, but it enlarges a `public static` surface consumed from three sibling-owned lines and is
  rejected.
- **Hard prohibition carried from the sibling file:** do **not** add an optional parameter to any
  member consumed by a sibling. `EfcViewerQueue.Dequeue` is bound as a method group
  (`Func<EfcViewer> = EfcViewerQueue.Dequeue` at
  `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:40, 112`, F8-owned), and C# method
  group conversion does not fill optional parameters, so adding one there is a compile break in an
  F8-owned file. `ItemViewerQueue.Dequeue` is invoked normally at its three call sites, so the same
  hazard does not bite here — but the plan should apply the "new overload, never a new optional
  parameter" rule uniformly across the cluster (see `11-EfcViewerQueue.md` §9).

---

## 10. 500-line compliance

- `ItemViewerQueue.cs` — 123 lines; the selected proposal adds ~9 lines (method + XML doc) → ~132.
  **Compliant**, 368 lines of headroom. No partial split.
- **No new production file is proposed.** If one were, it would need a
  `<Compile Include="Helper Classes\<name>.cs" />` line in `QuickFiler/QuickFiler.csproj` inside the
  contiguous `Helper Classes\` block at `:342-354` — a shared file edited by all fourteen siblings
  and therefore a merge-conflict surface (`00-cluster-overview.md` §7.3). Avoided.
- **New test file required.** `ViewerQueueStaticWrapperTests.cs` is already 336 lines; adding
  sixteen tests (~14 lines each ≈ 224) would push it to ~560 and breach the 500-line limit, which
  applies to test code as well (`.claude/rules/general-code-change.md` § File Size Limit).
  Therefore create `QuickFiler.Test/Helper Classes/ItemViewerQueueTests.cs` (projected ~265 lines),
  which also fixes the "no test file named after the production file" gap noted in
  `00-cluster-overview.md` §2 finding 3. This needs one
  `<Compile Include="Helper Classes\ItemViewerQueueTests.cs" />` line in
  `QuickFiler.Test/QuickFiler.Test.csproj` inside the contiguous `Helper Classes\` block at
  `:158-165`, alphabetically after `EmailMoveMonitorTests.cs` (`:159`). Together with the cluster's
  other two additions this is a single contiguous git hunk (`00-cluster-overview.md` §1.3).

---

## 11. Recommended test cases (enumerated individually)

MSTest + FluentAssertions; Moq not required (all seams are delegates, matching the local pattern at
`ViewerQueueStaticWrapperTests.cs:316-328`). Every test class must carry `[DoNotParallelize]` and the
`[TestInitialize]`/`[TestCleanup]` pair from §7.3. Concrete `ItemViewer` instances come from
`FormatterServices.GetUninitializedObject` exactly as at
`ViewerQueueStaticWrapperTests.cs:330-334`; **no test constructs a live control and no test invokes
`ProductionViewerFactory` while it holds its production default.**

Destination: **[C]** = new `QuickFiler.Test/Helper Classes/ItemViewerQueueTests.cs`;
**[D]** = existing `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs` (additive only).

| # | `[TestMethod]` name | Arrange / Act / Assert | Category | Dest |
| --- | --- | --- | --- | --- |
| I1 | `SetCoreForTesting_WithNullCore_ThrowsArgumentNullException` | Act `SetCoreForTesting(null)`; Assert `ArgumentNullException` with `ParamName == "core"` (**closes the missed branch at `:71`**) | invalid-input | [C] |
| I2 | `SetCoreForTesting_WithNullCore_LeavesPreviouslyInjectedCoreInstalled` | Arrange inject recording core A; Act `SetCoreForTesting(null)` (swallow the throw), then `BuildQueue(1)`; Assert core A received the build (failed injection is not destructive) | error-handling | [C] |
| I3 | `BuildQueue_WithNegativeCount_PropagatesArgumentOutOfRangeException` | Arrange inject a core; Act `BuildQueue(-1)`; Assert `ArgumentOutOfRangeException` with `ParamName == "count"` and the injected factory was never called | invalid-input | [C] |
| I4 | `BuildQueueWhenIdle_WithNegativeCount_PropagatesArgumentOutOfRangeException` | as I3 via `BuildQueueWhenIdle(-1)`; Assert throw and the recorded priority list is empty | invalid-input | [C] |
| I5 | `BuildQueueBackground_WithNegativeCount_PropagatesArgumentOutOfRangeException` | as I4 via `BuildQueueBackground(-2)` | invalid-input | [C] |
| I6 | `DequeueChunk_WithNegativeCount_PropagatesArgumentOutOfRangeException` | Act `DequeueChunk(-1)`; Assert throw and injected core depth unchanged | invalid-input | [C] |
| I7 | `BuildQueue_WithZeroCount_LeavesInjectedCoreEmpty` | Arrange inject a core; Act `BuildQueue(0)`; Assert `Count == 0` and factory never called | boundary | [C] |
| I8 | `BuildQueueWhenIdle_WithDeferringScheduler_ReturnsBeforeAnyViewerIsQueued` | Arrange inject a core whose `priorityScheduler` records the priority but does **not** invoke the action; Act `BuildQueueWhenIdle(2)`; Assert recorded priorities `[ContextIdle, ContextIdle]` and `Count == 0` (executable documentation of W3 fire-and-forget) | boundary | [C] |
| I9 | `DequeueChunk_WithZeroCount_ReturnsEmptySequence` | Arrange inject a core with 2 queued; Act `DequeueChunk(0)`; Assert result is empty and blocking scheduler never invoked | boundary | [C] |
| I10 | `Dequeue_WithCanceledToken_ThrowsOperationCanceledException` | Arrange `CancellationTokenSource` cancelled, inject a core; Act `Dequeue(source.Token)`; Assert `OperationCanceledException` and factory never called (proves the façade forwards the token — every existing test uses `CancellationToken.None`) | error-handling | [C] |
| I11 | `Dequeue_FromStockedQueue_ReturnsCachedViewerAndQueuesExactlyOneReplacementAtContextIdle` | Arrange inject a recording core, `BuildQueue(1)`; Act `Dequeue(None)`; Assert the returned reference is the queued instance, `Count == 1`, blocking scheduler never invoked, recorded priority `[ContextIdle]` (pins the `(Render, 1, 1, ContextIdle)` mapping of W1 on the **cached** path, which no existing test does) | positive | [C] |
| I12 | `ResetCoreForTesting_AfterSetCoreForTesting_DrainsInjectedCoreThroughItsDisposer` | Arrange inject a core built **with** a recording dispose delegate and 2 queued viewers; Act `ResetCoreForTesting()`; Assert both viewers were passed to the disposer (covers the `_core.Reset()` forward at `:79` with a non-null disposer, which no existing test does) | boundary | [C] |
| I13 | `ProductionSynchronousScheduler_Default_InvokesActionInline` | Arrange after `[TestInitialize]`; Act `ItemViewerQueue.ProductionSynchronousScheduler(() => invoked = true)`; Assert `invoked` is `true` (**covers the `.cctor` lambda body at `:15`**) | positive | [C] |
| I14 | `ResetProductionCoreDefaultsForTesting_RestoresSynchronousSchedulerThatInvokesInline` | Arrange set `ProductionSynchronousScheduler = _ => { }`; Act `ResetProductionCoreDefaultsForTesting()`, then invoke the property with a flag-setting action; Assert the flag is set (**covers the restored lambda body at `:86`**) | positive | [C] |
| I15 | `ResetProductionCoreDefaultsForTesting_ReplacesAnInjectedViewerFactoryReference` | Arrange set `ProductionViewerFactory` to a sentinel delegate; Act reset; Assert the property is **not** the sentinel — reference comparison only, the restored factory is never invoked (it would construct a live `ItemViewer`) | positive | [C] |
| I16 | `CreateProductionCore_WithSuppliedDelegates_DoesNotReplaceTheStaticCore` | Arrange inject recording core A; Act call `CreateProductionCore(4 recording delegates)` and discard the result, then `BuildQueue(1)`; Assert core A received the build and the newly created core's own recorders stayed empty (isolation contract of the internal factory) | positive | [C] |
| I17 | `StaticState_AfterTestInitialize_IsAKnownProductionBaselineRegardlessOfOrder` | Arrange `[TestInitialize]` has run; Act read `ProductionSynchronousScheduler` and invoke it with a flag action; Assert it invokes inline, proving each test starts from a known baseline without depending on the previous test's cleanup (the explicit isolation guard of §7.3) | positive | [C] |
| I18 | `TestInitialize_RestoresDefaultsEvenWhenAPriorTestLeftAnInjectedCore` | Arrange (in this test) inject core A and mutate `ProductionSynchronousScheduler`; Act call `ItemViewerQueue.ResetForTesting()`; Assert the synchronous scheduler invokes inline again and a subsequent `BuildQueue(0)` no longer reaches core A | error-handling | [C] |
| I19 | `Cleanup_RestoresDefaultsBeforeRebuildingCore_SoTheRebuiltCoreUsesProductionDelegates` | Add a `[TestInitialize]` to the existing class and assert, in one new test, that after `ResetForTesting()` the class's own subsequent `Dequeue` path is not wired to any test recorder — pins the ordering requirement identified in §7.2 | positive | [D] |

**Total: 19 recommended test cases** (18 in the new file [C], 1 additive in [D]).

**Excluded as duplicates of existing coverage** (with the existing test cited):

- "`BuildQueueWhenIdle` / `BuildQueueBackground` / `BuildQueue` delegate to the injected core and use
  `ContextIdle` / `Background`" — `ViewerQueueStaticWrapperTests.cs:88-116`.
- "`Dequeue` and `DequeueChunk` delegate to the injected core with `Render` / `ContextIdle`" —
  `ViewerQueueStaticWrapperTests.cs:118-154`.
- "`CreateProductionCore(4 delegates)` returns a core that uses the supplied delegates" —
  `ViewerQueueStaticWrapperTests.cs:198-236`.
- "`ResetCoreForTesting` rebuilds the core from the current `Production*` delegates" —
  `ViewerQueueStaticWrapperTests.cs:271-300`.
- Any FIFO/capacity/empty-queue/duplicate-enqueue/disposal test — owned by
  `09-ViewerQueueCore.md` §11 (T9-T20); duplicating them through the façade would double-count.

---

## 12. STA determination

**No member requires an STA thread, and no `*.StaTests.cs` file is proposed for this file.**

Seam-hierarchy exhaustion argument (`epic.md` Shared Design §3): every member except the four
dispatcher lambdas and `CreateProductionViewer` is reachable through the existing
`SetCoreForTesting` / `Production*` seams (§5), so the clause is not reached for them. For the five
that are not reachable, STA is **not the remedy either**:

- `CreateProductionViewer` `:103-106` constructs `ItemViewer`, a `UserControl`
  (`QuickFiler/Viewers/ItemViewer.cs:21`) with a 6,224-line designer. The STA clause permits only
  *never-shown in-memory controls* such as `TableLayoutPanel`, `Label`, `Panel`, `CheckBox` — a
  fully designer-initialised `UserControl` from a sibling-owned file is outside that allowance, and
  running `InitializeComponent` would create real child controls and a WebView2 surface.
- The dispatcher lambdas `:21, 27, 88, 90` need an initialised process-global `UiThread` singleton on
  a live WPF UI thread (§5.1), not merely an STA apartment. `[STATestMethod]` alone would not make
  `UiThread.Dispatcher` non-null.

These five lines are therefore classified `host-bound-irreducible` and are handled by the coverage
arithmetic in §13, not by an STA file. Note also that `QuickFiler.Test` has **no** STA
infrastructure today (`00-cluster-overview.md` §5) — introducing the project's first `*.StaTests.cs`
for zero coverage gain would be unjustified.

---

## 13. Projected coverage

Indicative pre-change state, from the feature-#424 artifacts (§2, §4):

| Reading | Denominator | Uncovered | Line rate |
| --- | --- | --- | --- |
| Main class only, as recorded | — | `:88, 90, 104, 105, 106` | `line-rate="0.936508"`, `branch-rate="0.9"` |
| Per-file union over line numbers (main ∪ `<>c`, max hits) | 64 | `:88, 90, 104, 105, 106` (5) | 59/64 ≈ **92.2%** |
| Per-file sum of class elements without de-duplication (main 64 + `<>c` 6) | 70 | 5 + 6 = 11 | 59/70 ≈ **84.3%** |

The file is therefore **already above the 80% floor under either aggregation convention**, which is
the key planning fact: this phase is not a rescue. Its purpose is (a) the acceptance criterion that
coverage span invalid-input, boundary, and error-handling behaviour (`issue.md:65-66`), (b) the
static-state isolation hardening of §7, and (c) the ≥ 90% bar that
`.claude/rules/csharp.md:40` applies to the new `ResetForTesting()` member.

Projected post-change state:

- I1 closes the missed `??` branch at `:71` → branch coverage of the only decision point in the file
  reaches 100% (from `0.9` class branch-rate).
- I13 and I14 cover the `<>c` lambda bodies at `:15` and `:86`, removing 2 of the 6 uncovered lines
  in the closure class. Under the union reading those lines were already counted covered; under the
  sum reading the projection becomes 61/70 ≈ **87.1%**.
- The new `ResetForTesting()` member (2 executable lines) is executed by `[TestInitialize]` in every
  test in the class → **100% of new lines**, satisfying the ≥ 90%-for-new-code rule.
- Irreducible remainder: **7 lines** — `:21, 27, 88, 90` (dispatcher lambdas, §5.1) and
  `:104-106` (`new ItemViewer()`, §5.2). Under the union reading that is 4 lines counted against a
  64-line denominator (`:21`/`:27` are covered as assignment sites in the main class), giving a
  **ceiling of 60/64 ≈ 93.8%**; under the sum reading the ceiling is 61/70 ≈ 87.1%.

**Verdict: the file clears 80% before and after the change, with a ceiling of ~94% (union) /
~87% (sum). No exemption is requested.** Recommended F1 ledger classification: **`testable`**, with
a footnote that 7 lines are host-bound-irreducible (`UiThread.Dispatcher` dereference ×4 and
`new ItemViewer()` ×3) and that this remainder is *not* claimed as an exemption because the file
clears the floor without one. Authoritative numbers are produced by F1's harness at execution time
and committed under `<FEATURE>/evidence/qa-gates/`.

---

## 14. Findings to carry into the F4 plan

1. **Static mutable state (S1-S5, §1.2) is the file's principal risk**, and `[TestCleanup]`-only
   handling is order-dependent (§7.2). Remedy: `internal static ResetForTesting()` +
   `[TestInitialize]` + `[DoNotParallelize]` (§7.3).
2. **Three sibling-owned call sites** consume `Dequeue(CancellationToken)`: `QfcQueue.cs:336` (F2),
   `QfcCollectionController.cs:617` and `:958` (F11). All proposals here are additive and leave them
   compiling unchanged.
3. **Never add an optional parameter to a wrapper member**; the sibling `EfcViewerQueue.Dequeue` is
   consumed as a method group from F8-owned code, and the plan should apply the rule uniformly
   (§9).
4. **7 host-bound-irreducible lines** (`:21, 27, 88, 90, 104-106`) with the seam hierarchy exhausted
   and STA ruled out (§12).
5. **A per-file coverage aggregation caveat for F1**: this file emits two Cobertura `<class>`
   elements sharing one `filename`; union-with-max-hits is required (§2).
6. **No banned API, no time dependence** — no `TimeProvider` seam needed.
7. **The `ItemViewerQueueSize` user setting appears orphaned** (`QuickFiler/Properties/Settings.settings:23`,
   `Settings.Designer.cs:98-104`; no consumer in repository grep). F15-owned, out of scope, recorded
   only.
