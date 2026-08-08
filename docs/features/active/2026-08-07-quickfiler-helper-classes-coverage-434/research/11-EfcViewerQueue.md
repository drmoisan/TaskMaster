# F4 per-file research — `QuickFiler/Helper Classes/EfcViewerQueue.cs`

Timestamp: 2026-08-07T22-40

Cluster: VIEWER-QUEUE. Companions: `09-ViewerQueueCore.md` (the generic core this file wraps),
`10-ItemViewerQueue.md` (the sibling wrapper), `12-QfEnums.md`. Cross-cutting facts are established
in `00-cluster-overview.md` and are cited, not re-derived. Queue-shape invariants belong to the core
and are enumerated once in `09-ViewerQueueCore.md` §6 (I1-I11); they are not restated here.

Upstream contract: F1 owns the per-file coverage harness and the ratified exemption ledger at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`; neither exists on disk yet.
Authoritative numbers come from F1's harness at execution time; figures below are read from an
already-committed feature-#424 Cobertura artifact and are labelled indicative.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/EfcViewerQueue.cs` | — |
| Line count | **101** | file ends at `:101`; matches `epic.md:280` |
| 500-line limit | 101 / 500, **399 lines of headroom** | `.claude/rules/general-code-change.md` § File Size Limit |
| Compiled | yes — `<Compile Include="Helper Classes\EfcViewerQueue.cs" />` | `QuickFiler/QuickFiler.csproj:346` |
| `[ExcludeFromCodeCoverage]` | **absent** — confirmed by full read; no attribute on the type or any member | `EfcViewerQueue.cs:1-101` |
| Type declaration | `public static class EfcViewerQueue` | `:8` |
| Namespace | `QuickFiler` | `:6` |
| Internals visible to tests | yes | `QuickFiler/Properties/AssemblyInfo.cs:5` |
| Banned APIs (`Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `Random.Shared`) | **none present** | full read; no time dependence anywhere |

### 1.1 Relationship to `ViewerQueueCore<TViewer>`, and the two differences from `ItemViewerQueue`

`EfcViewerQueue` is a **thin static façade** over exactly one `ViewerQueueCore<EfcViewer>` instance
(`:27`). It owns no queue logic. Its contract is the hard-coded argument mapping:

| Façade member | Forwards to | Hard-coded arguments |
| --- | --- | --- |
| `BuildQueue(int)` `:29-32` | `ViewerQueueCore.BuildQueue(int, DispatcherPriority)` | `Background` |
| `Dequeue()` `:34-43` | `ViewerQueueCore.Dequeue` | `token = CancellationToken.None`, `emptyQueuePriority = Render`, `cachedReplacementCount = 1`, `emptyReplacementCount = 2`, `replacementPriority = Background` |

Two material differences from the sibling `ItemViewerQueue`, both of which change the testability
arithmetic:

1. **`ProductionBlockingPriorityScheduler` is not dispatcher-bound here.** It is
   `(action, priority) => action()` (`:25`) — a pure inline invocation, versus
   `UiThread.Dispatcher.Invoke(action, priority)` at `ItemViewerQueue.cs:27`. That makes two lambda
   bodies coverable in this file that are irreducible in the sibling (§5).
2. **`Dequeue()` takes no `CancellationToken`** and hard-codes `CancellationToken.None` (`:37`).
   Combined with §9 this is the file's sharpest constraint: the method is consumed as a **method
   group** from an F8-owned file, so its parameter list cannot change at all.

There is no `DequeueChunk`, no `BuildQueueWhenIdle`, and no `BuildQueueBackground` on this wrapper.

### 1.2 Static mutable state — first-class finding

`EfcViewerQueue` is a `static` class holding **five pieces of mutable process-global state**:

| # | Member | Line | Risk |
| --- | --- | --- | --- |
| S1 | `internal static Func<EfcViewer> ProductionViewerFactory { get; set; }` | `:10-11` | a leaked test factory changes what production constructs |
| S2 | `internal static Action<Action> ProductionSynchronousScheduler { get; set; }` | `:13-14` | a leaked no-op scheduler silently stops synchronous enqueues |
| S3 | `internal static Action<Action, DispatcherPriority> ProductionPriorityScheduler { get; set; }` | `:16-20` | as above for priority enqueues |
| S4 | `internal static Action<Action, DispatcherPriority> ProductionBlockingPriorityScheduler { get; set; }` | `:22-25` | as above for the blocking path |
| S5 | `private static ViewerQueueCore<EfcViewer> _core` | `:27` | **the queue and its viewer instances survive across tests** |

Same determinism/isolation hazard as the sibling wrapper. `.claude/rules/general-unit-test.md`
§ Core Principles requires order-independence and isolation; §7 analyses the current mitigation and
proposes the remedy.

---

## 2. Member inventory (coverage denominator)

Decision points: `if`, ternary, `??`, `?.`, loops, `catch`. No `switch`, no `await`, no `lock`, no
`catch` in the file.

| # | Member | Signature | Lines | Decision points |
| --- | --- | --- | --- | --- |
| 1 | property + initializer | `internal static Func<EfcViewer> ProductionViewerFactory { get; set; } = CreateProductionViewer` | 10-11 | 0 |
| 2 | property + initializer lambda | `ProductionSynchronousScheduler = action => action()` | 13-14 (body `:14`) | 0 |
| 3 | property + initializer lambda | `ProductionPriorityScheduler = (action, priority) => _ = UiThread.Dispatcher.InvokeAsync(action, priority)` | 16-20 (body `:20`) | 0 |
| 4 | property + initializer lambda | `ProductionBlockingPriorityScheduler = (action, priority) => action()` | 22-25 (body `:25`) | 0 |
| 5 | field + initializer | `private static ViewerQueueCore<EfcViewer> _core = CreateProductionCore()` | 27 | 0 |
| 5a | implicit `.cctor` | sequence points at `:11, 14, 20, 25, 27` | — | 0 |
| 6 | method | `public static void BuildQueue(int count)` | 29-32 | 0 |
| 7 | method | `public static EfcViewer Dequeue()` | 34-43 | 0 |
| 8 | method | `internal static void SetCoreForTesting(ViewerQueueCore<EfcViewer> core)` | 48-51 | **1** (`??` throw at `:50`) |
| 9 | method | `internal static void ResetCoreForTesting()` | 56-60 | 0 |
| 10 | method | `internal static void ResetProductionCoreDefaultsForTesting()` | 62-69 | 0 (four assignments; three lambdas at `:65`, `:67`, `:68`) |
| 11 | method | `private static ViewerQueueCore<EfcViewer> CreateProductionCore()` | 71-79 | 0 |
| 12 | method | `private static EfcViewer CreateProductionViewer()` | 81-84 | 0 |
| 13 | method | `internal static ViewerQueueCore<EfcViewer> CreateProductionCore(Func<EfcViewer>, Action<Action>, Action<Action,DispatcherPriority>, Action<Action,DispatcherPriority>)` | 86-99 | 0 |

Totals: 1 static type, 4 static properties, 1 static field, 8 methods (2 public, 4 internal,
2 private), 6 lambdas (`:14, 20, 25` in the `.cctor`; `:65, 67, 68` in
`ResetProductionCoreDefaultsForTesting`). **1 decision point in the whole file.**

Cobertura reports **50 distinct sequence-point lines** for the main `QuickFiler.EfcViewerQueue`
class plus a compiler-generated closure class `QuickFiler.EfcViewerQueue.<>c` carrying the six lambda
bodies (`:14, 20, 25, 65, 67, 68`). Evidence of the two-class shape:
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml:25797-25838`
(`name="QuickFiler.EfcViewerQueue.&lt;&gt;c"`, `line-rate="0"`, lines `14, 20, 25, 65, 67, 68`).
As in the sibling file, those six line numbers also appear in the main class as *assignment* sites,
so a per-file aggregation must merge same-`filename` classes with **max hits per line number**. The
denominator convention is F1's to fix; §13 reports the gap under both readings.

---

## 3. Existing test inventory

There is **no** `EfcViewerQueueTests.cs`. The type is exercised only by
`QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs` (336 lines, declared at
`QuickFiler.Test/QuickFiler.Test.csproj:165`), class `[DoNotParallelize] [TestClass]
ViewerQueueStaticWrapperTests` (`:11-13`), namespace `QuickFiler.Test.HelperClasses`. Four of its
eight `[TestMethod]`s touch `EfcViewerQueue`.

| Test method / fixture | Line | `EfcViewerQueue` members exercised |
| --- | --- | --- |
| `[TestCleanup] Cleanup()` | `:15-22` | `ResetProductionCoreDefaultsForTesting()` `:18` → `:62-69`; `ResetCoreForTesting()` `:19` → `:56-60` (transitively `CreateProductionCore()` `:71-79`) |
| `EfcViewerQueue_BuildQueue_DelegatesToInjectedCore` | `:24-50` | `SetCoreForTesting` `:41` happy path; `BuildQueue(2)` `:43`; asserts `created == 2`, `Count == 2`, recorded priorities `[Background, Background]` (`:47-49`) |
| `EfcViewerQueue_Dequeue_UsesInjectedCoreAndRestoresReplacementCount` | `:52-86` | `Dequeue()` `:77` on an **empty** queue; asserts `created == 3`, `Count == 2`, `blockingPriorities == [Render]`, `scheduledPriorities == [Background, Background]` (`:79-85`) — pins the `emptyReplacementCount = 2` mapping |
| `EfcViewerQueue_CreateProductionCore_UsesProvidedDelegates` | `:156-196` | `CreateProductionCore(4 delegates)` `:162` → `:86-99`; then drives the **returned** core directly (`:181`), not the static one |
| `EfcViewerQueue_ResetCoreForTesting_UsesResettableProductionDefaults` | `:238-269` | writes `ProductionViewerFactory` `:244`, `ProductionPriorityScheduler` `:249`, `ProductionBlockingPriorityScheduler` `:254`; `ResetCoreForTesting()` `:260`; `Dequeue()` `:261` |

Helpers: `CreateEfcCore` (`:302-314`) constructs `ViewerQueueCore<EfcViewer>` with **four**
arguments — **no dispose delegate**; `CreateUninitialized<TViewer>` (`:330-334`) uses
`System.Runtime.Serialization.FormatterServices.GetUninitializedObject` so no live `Form` is ever
constructed.

**Not exercised anywhere today:** `SetCoreForTesting(null)` throw path; the **cached** `Dequeue`
path (every existing test dequeues from an empty queue); `ProductionSynchronousScheduler` and
`ProductionBlockingPriorityScheduler` default lambdas; the restored-defaults lambdas;
`CreateProductionViewer`.

---

## 4. Per-member coverage gap

Indicative baseline: feature-#424 artifact
`.../424/evidence/qa-gates/coverage-final.cobertura.xml:2213-2393`, class
`QuickFiler.EfcViewerQueue`, recorded `line-rate="0.929293"`, `branch-rate="0.9"`. Line numbers in
that artifact align exactly with the current file, so the file is unchanged since that run.

Zero-hit lines in the main class: **`:67`, `:82`, `:83`, `:84`** (artifact `:2366`, `:2381-2383`).
Zero-hit lines in the `<>c` closure class: `:14, 20, 25, 65, 67, 68` (baseline artifact
`:25831-25836`).

| Member | Status | Detail |
| --- | --- | --- |
| `.cctor` `:11, 14, 20, 25, 27` | covered (assignments) | **lambda bodies** at `:14`, `:20`, `:25` uncovered in `<>c` |
| `BuildQueue` `:29-32` | covered | `ViewerQueueStaticWrapperTests.cs:43` |
| `Dequeue` `:34-43` | **partially covered** | all lines hit, but **only the empty-queue path**; no test dequeues from a stocked queue through this façade |
| `SetCoreForTesting` `:48-51` | **partially covered (branch missed)** | `:50` at `condition-coverage="50%"` (artifact `:2338-2344`) — the `ArgumentNullException` throw is never taken |
| `ResetCoreForTesting` `:56-60` | covered | `:19`, `:260` |
| `ResetProductionCoreDefaultsForTesting` `:62-69` | **partially covered** | method lines hit; lambda body at `:67` **uncovered** (host-bound); lambda bodies at `:65` and `:68` uncovered in `<>c` (both coverable) |
| `CreateProductionCore()` `:71-79` | covered | via `ResetCoreForTesting` |
| `CreateProductionViewer()` `:81-84` | **uncovered** | `:82-84` zero hits — `new EfcViewer()` |
| `CreateProductionCore(4)` `:86-99` | covered | `:162` |

---

## 5. Testability classification per member

| Member | Classification | Reasoning |
| --- | --- | --- |
| `BuildQueue`, `Dequeue` | `pure-testable-now` | one-line forwards to the injectable core; `SetCoreForTesting` `:48` is the existing seam |
| `SetCoreForTesting` (both paths) | `pure-testable-now` | pass `null` to reach `:50` |
| `ResetCoreForTesting`, `ResetProductionCoreDefaultsForTesting` (method bodies) | `pure-testable-now` | already covered |
| `ProductionSynchronousScheduler` default lambda `:14` and restored twin `:65` (`action => action()`) | `pure-testable-now` | pure delegate — read the property and invoke it |
| **`ProductionBlockingPriorityScheduler` default lambda `:25` and restored twin `:68` (`(action, priority) => action()`)** | **`pure-testable-now`** | **pure delegate — this is the file's advantage over `ItemViewerQueue`, where the same member dereferences `UiThread.Dispatcher`** |
| `CreateProductionCore()` and `CreateProductionCore(4)` | `pure-testable-now` | already covered |
| `ProductionPriorityScheduler` default lambda `:20` and restored twin `:67` | **`host-bound-irreducible`** | §5.1 |
| `CreateProductionViewer` `:81-84` | **`host-bound-irreducible`** | §5.2 |

No Outlook Interop type appears in this file; the `using` list is `System`, `System.Threading`,
`System.Windows.Threading`, `UtilitiesCS` (`:1-4`). The Moq-on-Interop catalogue in
`00-cluster-overview.md` §3 is not needed here.

### 5.1 Why the priority-scheduler lambda is irreducible

`:20` and `:67` are `_ = UiThread.Dispatcher.InvokeAsync(action, priority)`. `UiThread.Dispatcher`
is `public static Dispatcher` (`UtilitiesCS/Threading/UiThread.cs:135-139`) backed by
`private static Dispatcher _dispatcher = null!; // set in Initialize() before any access`
(`UiThread.cs:140`), so invoking the lambda requires the process-global `UiThread` singleton to have
been initialised on a real WPF UI thread. `System.Windows.Threading.Dispatcher` is **sealed**, so no
Moq fake can be substituted. Line cost: **2 lines** (`:20`, `:67`) — half the sibling wrapper's cost,
because the blocking scheduler here is pure.

### 5.2 Why `CreateProductionViewer` is irreducible

`:83` is `return new EfcViewer();`. `EfcViewer` is declared
`public partial class EfcViewer : Form` (`QuickFiler/Viewers/EfcViewer.cs:21`) with
`InitializeComponent` in a 4,276-line designer (`QuickFiler/Viewers/EfcViewer.Designer.cs`).
Constructing it is live-**form** construction — prohibited by `epic.md` Shared Design §2 and
explicitly outside the STA last-resort allowance, which covers only never-shown in-memory *controls*
(`epic.md` Shared Design §3). The type is **F9-owned** (`epic.md:318`), so making it constructible
would be a cross-child edit. Line cost: **3 lines** (`:82-84`).

Note on the existing workaround: `ViewerQueueStaticWrapperTests.cs:330-334` materialises `EfcViewer`
with `FormatterServices.GetUninitializedObject`. That is already-shipped practice and is why the
existing tests can name the concrete type at all — keep it. The finaliser caveat and the
`GC.SuppressFinalize` mitigation recorded in `10-ItemViewerQueue.md` §5.2 apply identically here and
are not repeated.

---

## 6. Ordering, concurrency and static-state invariants

Core-owned invariants (FIFO, no capacity limit, empty-queue behaviour, disposal, duplicate enqueue,
absence of synchronisation) are in `09-ViewerQueueCore.md` §6. This file owns:

**W1 — Argument mapping.** `BuildQueue` always passes `Background` (`:31`); `Dequeue` always passes
`(CancellationToken.None, Render, 1, 2, Background)` (`:36-42`). The `emptyReplacementCount = 2`
value is already pinned by `ViewerQueueStaticWrapperTests.cs:81` (`Count == 2`); the
`cachedReplacementCount = 1` value is **not** pinned anywhere, because no existing test dequeues from
a stocked queue. Deterministic test: E5 in §11.

**W2 — `Dequeue()` is uncancellable by design.** `CancellationToken.None` is hard-coded at `:37`, so
`ViewerQueueCore`'s two cancellation guards (`ViewerQueueCore.cs:71`, `:135`) can never fire through
this façade. This is a real behavioural contract worth pinning, and it contrasts with
`ItemViewerQueue.Dequeue(CancellationToken)`. Deterministic test: E9 asserts that a long-running
build/dequeue sequence completes without any cancellation observation and that the core receives
`CancellationToken.None` (observed via a core built with a recording blocking scheduler that
inspects nothing time-related). It is **not** a defect to fix in F4 — changing the signature is
forbidden by §9.

**W3 — Production priority builds are fire-and-forget; production blocking builds are inline.**
`ProductionPriorityScheduler` is `_ = ...InvokeAsync(...)` (`:20`), so `BuildQueue(n)` returns before
any viewer exists and the queue is later mutated on the UI thread. `ProductionBlockingPriorityScheduler`
is `(action, priority) => action()` (`:25`), i.e. it runs **on the calling thread, ignoring the
priority** — meaning the empty-queue `Dequeue` path constructs the `EfcViewer` on whatever thread
called `Dequeue`, not on the UI thread. Combined with the unsynchronised `Queue<T>` (I8 in
`09-ViewerQueueCore.md` §6) this is a latent cross-thread hazard at the production wiring point.
**Out of F4 scope** (no behaviour change; the consumer is F8-owned). Recommended action: fold into
the same promoted issue proposed in `09-ViewerQueueCore.md` §14 item 3. Deterministic documentation
tests in F4: E8 (blocking default runs inline and ignores the priority) and E10 (a deferring priority
scheduler leaves `Count == 0`).

**W4 — No thread-safety on the static state.** S1-S5 are plain statics with no `volatile`,
`Interlocked`, or `lock`. Mitigation is `[DoNotParallelize]`, already at
`ViewerQueueStaticWrapperTests.cs:11`, and it must be applied to every new class that touches the
type (§7).

**W5 — Static initialisation order is safe.** Field initialisers run in declaration order, so
`:10-25` precede `:27`, which reads them at `:73-78`. First touch of the type builds a core but
constructs no viewer (`ViewerQueueCore.cs:26-34` only stores delegates).

**W6 — No time dependence.** No `DateTime`, timer, `Task.Delay`, or `Thread.Sleep`. `TimeProvider` /
`FakeTimeProvider` (`00-cluster-overview.md` §4) is **not** required. No banned-API finding.

---

## 7. Static-state test-isolation analysis

### 7.1 Residue paths

- **R1 — leaked injected core.** `SetCoreForTesting(core)` (`:48-51`) permanently replaces S5; a
  later test calling `Dequeue()` without injecting would drive the previous test's core.
- **R2 — leaked production delegate.** Writing S1/S3/S4 (as
  `ViewerQueueStaticWrapperTests.cs:244, 249, 254` does) changes what any subsequent
  `ResetCoreForTesting()` builds, because `:56-60` rebuilds from the *current* values via
  `CreateProductionCore()` (`:71-79`).
- **R3 — the dangerous default.** With S1-S5 at true production values, a test that calls
  `EfcViewerQueue.BuildQueue(1)` or `Dequeue()` without injecting will (a) dereference
  `UiThread.Dispatcher`, which is `null!` in a test process → `NullReferenceException`, and/or
  (b) run `new EfcViewer()` (`:83`) inline via the blocking scheduler at `:25` — a **live `Form`**.
  Path (b) is strictly worse here than in the sibling wrapper, because the sibling's blocking
  scheduler would fail fast on the null dispatcher **before** constructing anything, whereas this
  file's inline blocking scheduler constructs the `Form` first. Recording this asymmetry is the main
  isolation finding for this file.

### 7.2 How `ViewerQueueStaticWrapperTests.cs` handles it today, and whether that is sound

Mechanism: `[DoNotParallelize]` (`:11`) plus `[TestCleanup] Cleanup()` (`:15-22`) calling
`EfcViewerQueue.ResetProductionCoreDefaultsForTesting()` then `EfcViewerQueue.ResetCoreForTesting()`
(`:18-19`).

- **Ordering is correct** (defaults before rebuild), for the reason in R2. It is nevertheless an
  order-sensitive two-call protocol that is easy to invert.
- **`[TestCleanup]` alone does not give order-independence.** It guarantees the class cleans up after
  itself but not that it *starts* from a known state. `.claude/rules/general-unit-test.md`
  § Core Principles requires tests to run in any order without impacting each other, which is a
  pre-condition guarantee. **Verdict: adequate today (grep shows no other file touches these
  statics) but not robust and not order-independent by construction.**
- **`ResetCoreForTesting` disposes nothing in practice.** `_core.Reset()` (`:58`) is a no-op for
  disposal because neither `CreateEfcCore` (`ViewerQueueStaticWrapperTests.cs:302-314`) nor
  `CreateProductionCore` (`:93-98`) supplies a dispose delegate, so `ViewerQueueCore.cs:122`'s
  `?.Invoke` short-circuits. Worth pinning once (E12).
- **Post-cleanup state is R3.** A guard test is warranted.

### 7.3 Proposed mechanism

Identical in shape to `10-ItemViewerQueue.md` §7.3, applied to this type. Parts A and B are
test-only; part C is a small additive production change.

**A.** Add `[TestInitialize]` alongside the existing `[TestCleanup]` in every class touching the
type, calling the single reset entry point below, so each test starts from a known state regardless
of order. Leave the existing `[TestCleanup]` body intact so future work on this file
(`00-cluster-overview.md` §8) does not rebase-conflict.

**B.** `[DoNotParallelize]` on every new test class touching the type, mirroring
`ViewerQueueStaticWrapperTests.cs:11`.

**C.** One additive `internal` production member that removes the order-sensitive protocol:

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

`internal`, 2 executable lines, covered by `[TestInitialize]` on every test, **no existing signature
changed** (§9). The two existing methods remain for the current callers at
`ViewerQueueStaticWrapperTests.cs:18-19`.

**Rejected alternative — replace the static class with an injectable backing instance**
(`internal static IViewerQueue Instance { get; set; }`). Cleanest isolation story, but a larger
production change whose value is already delivered by `SetCoreForTesting`, and it would redesign a
`public static` surface consumed as a **method group** from F8-owned code (§9). Rejected on
simplicity-first (`.claude/rules/general-code-change.md` § Design Principles) and
conflict-avoidance grounds.

---

## 8. Seam proposal

Ranked per `.claude/rules/csharp.md` § DI Seams and `epic.md` Shared Design §2.

**Selected — the seam already exists; add one convenience reset only.**

| Item | Detail |
| --- | --- |
| Existing seam tier | *injectable property defaulting to the real implementation* — `SetCoreForTesting` `:48-51` (state seam) plus the four `internal static` `Production*` delegate properties `:10-25` (collaborator seams) |
| Production default | `_core = CreateProductionCore()` `:27`; delegates default to `CreateProductionViewer`, `action => action()`, `UiThread.Dispatcher.InvokeAsync`, `(action, priority) => action()` |
| New member proposed | `internal static void ResetForTesting()` (§7.3 part C) — 2 executable lines |
| Injection point | none new; the existing static properties |
| Sibling impact | **requires no sibling-owned file change** — a new `internal static` method; `Dequeue()`'s parameterless signature and every other member are untouched, so the method-group bindings at `EfcHomeControllerDependencyFactories.cs:40, 112` keep compiling |

**Considered and rejected:**

1. **Reuse `UtilitiesCS.Threading.IUiDispatcher`** (`UtilitiesCS/Threading/IUiDispatcher.cs:15`) to
   cover `:20`/`:67`. Its members (`:18-41`) include no token-free
   `InvokeAsync(Action, DispatcherPriority)`, so the current call shape is not representable without
   adding a member to a `UtilitiesCS` interface — outside F4's file set and outside this epic.
   Substituting the nearest overload would change observable dispatch behaviour, forbidden by
   `issue.md:69`. **Rejected.**
2. **An injectable `Func<Dispatcher>` accessor** with production default `() => UiThread.Dispatcher`.
   Would make `:20`/`:67` executable against `Dispatcher.CurrentDispatcher`. **Rejected**: 2 lines of
   gain in a file already above the floor (§13), leaves a live `Dispatcher` attached to a pooled
   MSTest worker thread (contrary to §7's isolation objective), and adds a production seam with no
   production consumer.
3. **Add an optional `CancellationToken token = default` parameter to `Dequeue()`** so the
   cancellation paths become reachable through the façade. **Rejected — this is a compile break in a
   sibling-owned file.** `EfcViewerQueue.Dequeue` is bound as a *method group* to `Func<EfcViewer>`
   at `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:40` and `:112` (F8-owned), and
   C# method-group conversion does not fill optional parameters, so `Func<EfcViewer>` would no longer
   bind. **Alternative that does not require a sibling edit:** add a new **overload**
   `internal static EfcViewer Dequeue(CancellationToken token)` and leave the existing parameterless
   `Dequeue()` forwarding to it with `CancellationToken.None`. That preserves the method group,
   keeps both call sites compiling byte-identically, and is additive. It is **not** recommended for
   F4 because nothing in production needs it and it would add uncovered production surface; it is
   recorded so a future child does not repeat the optional-parameter mistake.
4. **Make `CreateProductionViewer` injectable.** It already is — `ProductionViewerFactory`
   (`:10-11`) *is* that seam; the 3 uncovered lines are the production default itself.
   **Rejected as irreducible** (§5.2).

---

## 9. Cross-child conflict analysis

F4's file set is the 13 files under `QuickFiler/Helper Classes/` plus
`QuickFiler/Interfaces/IEmailMoveMonitor.cs` (`epic.md:276-283`). Repository-wide grep for
`EfcViewerQueue` yields:

| Call site | Sibling owner | What it uses | Constraint imposed |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:40` — `internal static Func<EfcViewer> ProductionViewerFactory { get; set; } = EfcViewerQueue.Dequeue;` | **F8** (`epic.md:313`) | **method group conversion** of `public static EfcViewer Dequeue()` | **the parameter list of `Dequeue()` is frozen** — an added optional parameter breaks this binding |
| `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:112` — `ProductionViewerFactory = EfcViewerQueue.Dequeue;` (inside `ResetProductionFactoriesForTesting`) | **F8** | same | same |
| `QuickFiler/Viewers/EfcViewer.cs:21` (type `EfcViewer`) | **F9** (`epic.md:318`) | referenced as a *type* in the return/generic position | no edit needed; do not change the returned type |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:19` | **F13** (`epic.md:350`) | **comment only** — "…idempotently for pooled-viewer re-initialization (EfcViewerQueue)." | no code coupling |
| `QuickFiler/QuickFiler.csproj:346` | shared | existing `<Compile Include>` line | no edit needed |
| `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:18-19, 25-86, 157-196, 239-269, 302-314` | **F4** | test-only | F4-owned |
| `QuickFiler.Test/QuickFiler.Test.csproj:165` | shared | existing line | no edit needed |
| `docs/features/potential/promoted/2026-07-16-efcviewer-breadcrumb-webview2.md:47` | future (promoted, **no active folder yet**) | cites `QuickFiler/Helper Classes/EfcViewerQueue.cs:83` as the sole runtime instantiation of `EfcViewer` | documentation-only; a line-number shift in this file makes that citation stale (non-blocking). Keep `:81-84` at its current position if practical, i.e. append the new `ResetForTesting()` **after** `:99` rather than inserting mid-file |

**Nothing outside F4 calls `BuildQueue`.** The only externally consumed member is the parameterless
`Dequeue()`, from two F8-owned lines, and it is consumed by **method group**, which is a stricter
constraint than an ordinary call.

Explicit per-proposal statement:

- Selected proposal (add `internal static ResetForTesting()`, appended after `:99`): **requires no
  sibling-owned file change.**
- Rejected proposal 3a (optional parameter on `Dequeue()`): **requires editing
  `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:40` and `:112`, owned by F8** —
  therefore forbidden. The alternative that does not (a new **overload**, keeping the parameterless
  member) is stated in §8 item 3.
- Rejected proposals 1, 2, 4: 1 would require editing `UtilitiesCS/Threading/IUiDispatcher.cs`
  (outside the epic entirely); 2 and 4 would require no sibling change but are rejected on the
  merits.

---

## 10. 500-line compliance

- `EfcViewerQueue.cs` — 101 lines; the selected proposal adds ~9 lines (method + XML doc) → ~110.
  **Compliant**, 390 lines of headroom. No partial split.
- **No new production file is proposed.** If one were, it would need a
  `<Compile Include="Helper Classes\<name>.cs" />` line in `QuickFiler/QuickFiler.csproj` inside the
  contiguous `Helper Classes\` block at `:342-354` — a shared file edited by all fourteen siblings
  and therefore a merge-conflict surface (`00-cluster-overview.md` §7.3). Avoided.
- **New test file required.** `ViewerQueueStaticWrapperTests.cs` is already 336 lines and
  `10-ItemViewerQueue.md` §10 already allocates the sibling's tests to a separate new file; adding
  this file's fourteen tests on top would breach the 500-line limit, which applies to test code
  (`.claude/rules/general-code-change.md` § File Size Limit). Create
  `QuickFiler.Test/Helper Classes/EfcViewerQueueTests.cs` (projected ~215 lines), which also fixes
  the "no test file named after the production file" gap in `00-cluster-overview.md` §2 finding 3.
  This needs one `<Compile Include="Helper Classes\EfcViewerQueueTests.cs" />` line in
  `QuickFiler.Test/QuickFiler.Test.csproj` inside the contiguous `Helper Classes\` block at
  `:158-165`, alphabetically after `ConversationResolverTests.cs` (`:158`). Together with the
  cluster's other two additions this is one contiguous git hunk (`00-cluster-overview.md` §1.3).

---

## 11. Recommended test cases (enumerated individually)

MSTest + FluentAssertions; Moq not required (all seams are delegates, matching
`ViewerQueueStaticWrapperTests.cs:302-314`). Every test class carries `[DoNotParallelize]` and the
`[TestInitialize]`/`[TestCleanup]` pair from §7.3. Concrete `EfcViewer` instances come from
`FormatterServices.GetUninitializedObject` exactly as at `ViewerQueueStaticWrapperTests.cs:330-334`;
**no test constructs a live `Form`, and no test invokes `ProductionViewerFactory` while it holds its
production default.**

Destination: **[E]** = new `QuickFiler.Test/Helper Classes/EfcViewerQueueTests.cs`.

| # | `[TestMethod]` name | Arrange / Act / Assert | Category | Dest |
| --- | --- | --- | --- | --- |
| E1 | `SetCoreForTesting_WithNullCore_ThrowsArgumentNullException` | Act `SetCoreForTesting(null)`; Assert `ArgumentNullException` with `ParamName == "core"` (**closes the missed branch at `:50`**) | invalid-input | [E] |
| E2 | `SetCoreForTesting_WithNullCore_LeavesPreviouslyInjectedCoreInstalled` | Arrange inject recording core A; Act `SetCoreForTesting(null)` (swallow the throw) then `BuildQueue(1)`; Assert core A received the build | error-handling | [E] |
| E3 | `BuildQueue_WithNegativeCount_PropagatesArgumentOutOfRangeException` | Arrange inject a core; Act `BuildQueue(-1)`; Assert `ArgumentOutOfRangeException` with `ParamName == "count"`, factory never called, recorded priorities empty | invalid-input | [E] |
| E4 | `BuildQueue_WithZeroCount_LeavesInjectedCoreEmpty` | Act `BuildQueue(0)`; Assert `Count == 0`, factory never called, recorded priorities empty | boundary | [E] |
| E5 | `Dequeue_FromStockedQueue_ReturnsCachedViewerAndQueuesExactlyOneReplacementAtBackground` | Arrange inject a recording core, `BuildQueue(1)`; Act `Dequeue()`; Assert the returned reference is the queued instance, `Count == 1`, blocking scheduler never invoked, recorded priority `[Background]` (**pins `cachedReplacementCount = 1`, the untested half of W1**) | positive | [E] |
| E6 | `Dequeue_WhenViewerFactoryThrows_PropagatesExceptionThroughTheFacade` | Arrange inject a core whose factory throws `InvalidOperationException`, empty queue; Act `Dequeue()`; Assert the same exception surfaces and `Count == 0` | error-handling | [E] |
| E7 | `ProductionSynchronousScheduler_Default_InvokesActionInline` | Arrange after `[TestInitialize]`; Act invoke `EfcViewerQueue.ProductionSynchronousScheduler` with a flag action; Assert the flag is set (**covers the `.cctor` lambda body at `:14`**) | positive | [E] |
| E8 | `ProductionBlockingPriorityScheduler_Default_InvokesActionInlineIgnoringPriority` | Act invoke `EfcViewerQueue.ProductionBlockingPriorityScheduler(() => flag = true, DispatcherPriority.Render)`; Assert the flag is set and no exception (**covers the `.cctor` lambda body at `:25`**; executable documentation of W3's inline blocking semantics) | positive | [E] |
| E9 | `Dequeue_HardCodesCancellationTokenNone_SoAnEmptyQueueDequeueNeverCancels` | Arrange inject a core whose blocking scheduler records the fact it was invoked; Act `Dequeue()` on an empty queue; Assert no `OperationCanceledException` is thrown and a viewer is returned (pins W2 — the façade offers no cancellation surface) | boundary | [E] |
| E10 | `BuildQueue_WithDeferringPriorityScheduler_ReturnsBeforeAnyViewerIsQueued` | Arrange inject a core whose `priorityScheduler` records the priority but does **not** invoke the action; Act `BuildQueue(2)`; Assert recorded priorities `[Background, Background]` and `Count == 0` (executable documentation of W3 fire-and-forget) | boundary | [E] |
| E11 | `ResetProductionCoreDefaultsForTesting_RestoresSynchronousSchedulerThatInvokesInline` | Arrange set `ProductionSynchronousScheduler = _ => { }`; Act reset, then invoke the property with a flag action; Assert the flag is set (**covers the restored lambda body at `:65`**) | positive | [E] |
| E12 | `ResetProductionCoreDefaultsForTesting_RestoresBlockingSchedulerThatInvokesInline` | Arrange set `ProductionBlockingPriorityScheduler = (a, p) => { }`; Act reset, then invoke with a flag action; Assert the flag is set (**covers the restored lambda body at `:68`**) | positive | [E] |
| E13 | `ResetProductionCoreDefaultsForTesting_ReplacesAnInjectedViewerFactoryReference` | Arrange set `ProductionViewerFactory` to a sentinel; Act reset; Assert the property is **not** the sentinel — reference comparison only, the restored factory is never invoked (it would construct a live `Form`) | positive | [E] |
| E14 | `ResetCoreForTesting_AfterSetCoreForTesting_DrainsInjectedCoreThroughItsDisposer` | Arrange inject a core built **with** a recording dispose delegate and 2 queued viewers; Act `ResetCoreForTesting()`; Assert both viewers reached the disposer (covers the `_core.Reset()` forward at `:58` with a non-null disposer, which no existing test does) | boundary | [E] |
| E15 | `CreateProductionCore_WithSuppliedDelegates_DoesNotReplaceTheStaticCore` | Arrange inject recording core A; Act call `CreateProductionCore(4 recording delegates)`, discard the result, then `BuildQueue(1)`; Assert core A received the build and the new core's recorders stayed empty | positive | [E] |
| E16 | `StaticState_AfterTestInitialize_IsAKnownProductionBaselineRegardlessOfOrder` | Arrange `[TestInitialize]` has run; Act invoke `ProductionSynchronousScheduler` with a flag action and `ProductionBlockingPriorityScheduler` with another; Assert both invoke inline — proving each test starts from a known baseline without depending on the previous test's cleanup (the explicit isolation guard of §7.3) | positive | [E] |
| E17 | `ResetForTesting_AfterAnInjectedCoreAndMutatedDelegates_RestoresBothInTheCorrectOrder` | Arrange inject core A and set `ProductionSynchronousScheduler = _ => { }`; Act `EfcViewerQueue.ResetForTesting()`; Assert the synchronous scheduler invokes inline again **and** a subsequent `BuildQueue(0)` no longer reaches core A (pins the defaults-before-rebuild ordering of §7.2 and covers the new member) | error-handling | [E] |

**Total: 17 recommended test cases**, all in the new file [E].

**Excluded as duplicates of existing coverage** (with the existing test cited):

- "`BuildQueue` delegates to the injected core and uses `Background`" —
  `ViewerQueueStaticWrapperTests.cs:24-50`.
- "`Dequeue` on an **empty** queue creates one viewer and queues two replacements at `Background`,
  with `Render` on the blocking path" — `ViewerQueueStaticWrapperTests.cs:52-86`. (E5 is the
  **cached**-queue counterpart and is not a duplicate.)
- "`CreateProductionCore(4 delegates)` returns a core that uses the supplied delegates" —
  `ViewerQueueStaticWrapperTests.cs:156-196`.
- "`ResetCoreForTesting` rebuilds the core from the current `Production*` delegates" —
  `ViewerQueueStaticWrapperTests.cs:238-269`.
- Any FIFO/capacity/duplicate-enqueue/disposal test of the queue itself — owned by
  `09-ViewerQueueCore.md` §11.

---

## 12. STA determination

**No member requires an STA thread, and no `*.StaTests.cs` file is proposed for this file.**

Seam-hierarchy exhaustion (`epic.md` Shared Design §3): every member except the priority-scheduler
lambda pair and `CreateProductionViewer` is reachable through the existing `SetCoreForTesting` /
`Production*` seams (§5). For the five lines that are not:

- `CreateProductionViewer` `:81-84` constructs `EfcViewer`, which is `: Form`
  (`QuickFiler/Viewers/EfcViewer.cs:21`). The STA clause permits only never-shown in-memory
  **controls** (`TableLayoutPanel`, `Label`, `Panel`, `CheckBox`); a **form-derived** type with a
  4,276-line designer is explicitly outside that allowance, and the type is F9-owned.
- The priority-scheduler lambdas `:20`, `:67` need an initialised process-global `UiThread`
  singleton on a live WPF UI thread (§5.1), not merely an STA apartment; `[STATestMethod]` alone
  would leave `UiThread.Dispatcher` null.

These five lines are `host-bound-irreducible` and are handled by the arithmetic in §13.
`QuickFiler.Test` has no STA infrastructure today (`00-cluster-overview.md` §5); introducing the
project's first `*.StaTests.cs` for zero coverage gain would be unjustified.

---

## 13. Projected coverage

Indicative pre-change state, from the feature-#424 artifacts (§2, §4):

| Reading | Denominator | Uncovered | Line rate |
| --- | --- | --- | --- |
| Main class only, as recorded | — | `:67, 82, 83, 84` | `line-rate="0.929293"`, `branch-rate="0.9"` |
| Per-file union over line numbers (main ∪ `<>c`, max hits) | 50 | `:67, 82, 83, 84` (4) | 46/50 = **92.0%** |
| Per-file sum of class elements without de-duplication (main 50 + `<>c` 6) | 56 | 4 + 6 = 10 | 46/56 ≈ **82.1%** |

The file is **already above the 80% floor under either convention**, so this phase is not a rescue.
Its purpose is the acceptance criterion that coverage span invalid-input, boundary, and
error-handling behaviour (`issue.md:65-66`), the static-state hardening of §7, and the ≥ 90% bar
that `.claude/rules/csharp.md:40` applies to the new `ResetForTesting()` member.

Projected post-change state:

- E1 closes the missed `??` branch at `:50` → branch coverage of the file's only decision point
  reaches 100% (from `0.9` class branch-rate).
- E7, E8, E11, E12 cover four of the six `<>c` lambda bodies (`:14`, `:25`, `:65`, `:68`), leaving
  only `:20` and `:67`. Under the union reading those lines were already counted covered as
  assignment sites; under the sum reading the projection becomes 50/56 ≈ **89.3%**.
- The new `ResetForTesting()` member (2 executable lines) is executed by `[TestInitialize]` on every
  test and is directly asserted by E17 → **100% of new lines**, satisfying the ≥ 90%-for-new-code
  rule.
- Irreducible remainder: **5 lines** — `:20`, `:67` (`UiThread.Dispatcher.InvokeAsync`, §5.1) and
  `:82-84` (`new EfcViewer()`, §5.2). Under the union reading `:20` is covered as an assignment
  site, so the counted remainder is 4 lines against a 50-line denominator → **ceiling 46/50 = 92.0%**
  (unchanged, because those four lines are exactly today's uncovered set); under the sum reading the
  ceiling is 50/56 ≈ 89.3%.

**Verdict: the file clears 80% before and after the change, with a ceiling of ~92% (union) /
~89% (sum). No exemption is requested.** Recommended F1 ledger classification: **`testable`**, with
a footnote that 5 lines are host-bound-irreducible (`UiThread.Dispatcher.InvokeAsync` ×2 and
`new EfcViewer()` ×3) and that this remainder is *not* claimed as an exemption because the file
clears the floor without one. Authoritative numbers are produced by F1's harness at execution time
and committed under `<FEATURE>/evidence/qa-gates/`.

---

## 14. Findings to carry into the F4 plan

1. **Hard constraint: `Dequeue()`'s parameter list is frozen.** It is bound as a method group at
   `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:40` and `:112` (F8-owned); C#
   method-group conversion does not fill optional parameters, so adding one is a compile break in a
   sibling-owned file. Use a new **overload** if a token surface is ever needed (§8 item 3).
2. **Static mutable state (S1-S5) with an asymmetric failure mode.** Because this file's blocking
   scheduler is inline (`:25`), the "no injection" accident constructs a live `Form` **before** any
   null-dispatcher failure — strictly worse than the sibling wrapper (§7.1 R3). Remedy:
   `internal static ResetForTesting()` + `[TestInitialize]` + `[DoNotParallelize]` (§7.3).
3. **Two coverable lambda bodies the sibling does not have** (`:25`, `:68`), because
   `ProductionBlockingPriorityScheduler` is pure here (§5). E8 and E12 collect them.
4. **The cached-`Dequeue` path is entirely untested** through this façade; every existing test
   dequeues from an empty queue (E5 closes it).
5. **5 host-bound-irreducible lines** (`:20`, `:67`, `:82-84`) with the seam hierarchy exhausted and
   STA ruled out (§12).
6. **Append the new member after `:99`**, not mid-file, so the line-number citation at
   `docs/features/potential/promoted/2026-07-16-efcviewer-breadcrumb-webview2.md:47`
   (`EfcViewerQueue.cs:83`) does not go stale (§9).
7. **Per-file coverage aggregation caveat for F1**: this file emits two Cobertura `<class>` elements
   sharing one `filename`; union-with-max-hits is required (§2).
8. **No banned API, no time dependence** — no `TimeProvider` seam needed.
9. **Latent cross-thread hazard at the production wiring point** (W3): fire-and-forget
   `InvokeAsync` builds combined with an inline blocking scheduler and an unsynchronised `Queue<T>`.
   Out of F4 scope; fold into the promoted issue proposed in `09-ViewerQueueCore.md` §14 item 3.
