# F4 per-file research — `QuickFiler/Helper Classes/ViewerQueueCore.cs`

Timestamp: 2026-08-07T22-40

Cluster: VIEWER-QUEUE. Companion artifacts: `10-ItemViewerQueue.md`, `11-EfcViewerQueue.md`,
`12-QfEnums.md`. Cross-cutting facts (test-project wiring, Moq/Interop patterns, `TimeProvider`
seam, STA infrastructure) are established in `00-cluster-overview.md` and are cited rather than
re-derived.

Upstream contract: child F1 owns the per-file line-coverage harness and the ratified exemption
ledger at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Neither exists on
disk yet. Every *authoritative* numeric per-file figure is produced at execution time by F1's
harness. Where a number appears below it is read from an already-committed Cobertura artifact
belonging to feature #424 and is labelled as indicative, not authoritative.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/ViewerQueueCore.cs` | — |
| Line count | **161** | file ends at `:161`; matches `epic.md:283` |
| 500-line limit | 161 / 500, **439 lines of headroom** | `.claude/rules/general-code-change.md` § File Size Limit |
| Compiled | yes — `<Compile Include="Helper Classes\ViewerQueueCore.cs" />` | `QuickFiler/QuickFiler.csproj:354` |
| `[ExcludeFromCodeCoverage]` | **absent** — confirmed by full read of the file; no attribute on the type or any member | `ViewerQueueCore.cs:1-161` |
| Type declaration | `internal sealed class ViewerQueueCore<TViewer> where TViewer : class` | `:8-9` |
| Visible to tests | yes — `[assembly: InternalsVisibleTo("QuickFiler.Test")]` | `QuickFiler/Properties/AssemblyInfo.cs:5` |
| Namespace | `QuickFiler` (not `QuickFiler.Helper_Classes`) | `:6` |
| Banned APIs (`Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `Random.Shared`) | **none present** | full read; the file has no time dependence at all |

### 1.1 Relationship to the two static wrappers

`ViewerQueueCore<TViewer>` is the **only** implementation. It holds all queue state and all queue
logic. `ItemViewerQueue` and `EfcViewerQueue` are thin `public static` façades that own one
`private static ViewerQueueCore<T>` instance each and forward every public call to it with
hard-coded `DispatcherPriority` and replacement-count arguments:

- `QuickFiler/Helper Classes/ItemViewerQueue.cs:29` — `private static ViewerQueueCore<ItemViewer> _core = CreateProductionCore();`
- `QuickFiler/Helper Classes/EfcViewerQueue.cs:27` — `private static ViewerQueueCore<EfcViewer> _core = CreateProductionCore();`

The core itself holds **no static state**. All mutable state is instance state
(`_queue` at `:16`). The determinism and test-isolation hazard therefore lives entirely in the two
wrappers, not here; see `10-ItemViewerQueue.md` §7 and `11-EfcViewerQueue.md` §7.

Consequence that dominates this artifact: because the type is `internal` and is referenced only
from the two F4-owned wrapper files and from F4-owned tests, **every seam change to this file is
F4-internal and cannot conflict with any sibling child** (see §9).

---

## 2. Member inventory (coverage denominator)

Decision points counted: `if`, ternary, `??`, `?.`, loop conditions, `catch`. No `switch`, no
`await`, no `lock`, no `catch` anywhere in the file.

| # | Member | Signature | Lines | Decision points |
| --- | --- | --- | --- | --- |
| 1 | field | `private readonly Func<TViewer> _viewerFactory` | 11 | 0 |
| 2 | field | `private readonly Action<Action> _synchronousScheduler` | 12 | 0 |
| 3 | field | `private readonly Action<Action, DispatcherPriority> _priorityScheduler` | 13 | 0 |
| 4 | field | `private readonly Action<Action, DispatcherPriority> _blockingPriorityScheduler` | 14 | 0 |
| 5 | field | `private readonly Action<TViewer> _disposeViewer` | 15 | 0 |
| 6 | field initializer | `private readonly Queue<TViewer> _queue = new Queue<TViewer>()` | 16 | 0 |
| 7 | ctor | `internal ViewerQueueCore(Func<TViewer>, Action<Action>, Action<Action,DispatcherPriority>, Action<Action,DispatcherPriority> = null, Action<TViewer> = null)` | 18-35 | **4** (`??` throw ×3 at 26/28/31; `??` fallback at 33) |
| 8 | property | `internal int Count => _queue.Count` | 37 | 0 |
| 9 | method | `internal int BuildQueue(int count)` | 39-50 | **1** (`for` at 44) |
| 10 | method | `internal void BuildQueue(int count, DispatcherPriority priority)` | 52-61 | **1** (`for` at 57) |
| 10a | lambda | `() => _queue.Enqueue(_viewerFactory())` | 59 | 0 |
| 11 | method | `internal TViewer Dequeue(CancellationToken, DispatcherPriority, int, int, DispatcherPriority)` | 63-85 | **1** (`if` at 75) + 1 implicit throw at 71 |
| 12 | method | `internal IReadOnlyList<TViewer> DequeueChunk(int, DispatcherPriority, DispatcherPriority)` | 87-114 | **2** (`if` at 96; `for` at 108) |
| 12a | lambda | `() => BuildQueue(count - originalCount)` | 99 | 0 |
| 13 | method | `internal void Reset()` | 116-124 | **2** (`while` at 119; `?.Invoke` at 122) |
| 14 | method | `private TViewer CreateWithPriority(DispatcherPriority, CancellationToken)` | 126-142 | 0 + 1 implicit throw at 135 |
| 14a | lambda | multi-line closure containing the cancellation check | 133-138 | 0 |
| 15 | method | `private void EnqueueWith(Action<Action> scheduler)` | 144-147 | 0 |
| 15a | lambda | `() => _queue.Enqueue(_viewerFactory())` | 146 | 0 |
| 16 | method | `private static void ValidateCount(int count)` | 149-159 | **1** (`if` at 151) |

Totals: 1 type, 6 fields, 1 ctor, 1 property, 8 methods, 4 lambdas. **~13 decision points.**
Cobertura reports 97 distinct sequence-point lines for this file (§4).

---

## 3. Existing test inventory

Single file: `QuickFiler.Test/Helper Classes/ViewerQueueCoreTests.cs` (195 lines, declared at
`QuickFiler.Test/QuickFiler.Test.csproj:164`). `[TestClass] ViewerQueueCoreTests` in namespace
`QuickFiler.Test.HelperClasses` (`:12-13`). Six `[TestMethod]`s. It tests the core through a private
`FakeViewer` type (`:185-193`), so no WinForms type is ever constructed — this is the correct
pattern and should be preserved.

| Test method | Line | Production members exercised |
| --- | --- | --- |
| `BuildQueue_WithSynchronousScheduler_CreatesRequestedViewers` | `:16` | `BuildQueue(int)` `:39-50`, `EnqueueWith` `:144-147`, `ValidateCount` non-throwing `:149-151,159`, `Count` `:37`, ctor `:18-35` |
| `Dequeue_WithCachedViewer_ReturnsCachedAndSchedulesReplacement` | `:29` | `Dequeue` cached branch `:75-80`, `BuildQueue(int,priority)` `:52-61` + lambda `:59` |
| `Dequeue_WithEmptyQueue_CreatesViewerAndSchedulesConfiguredReplacementCount` | `:58` | `Dequeue` empty branch `:82-84`, `CreateWithPriority` `:126-142` incl. lambda `:133-138` |
| `Dequeue_WithCanceledToken_ThrowsBeforeCreatingViewer` | `:95` | `Dequeue` entry guard `:71` |
| `DequeueChunk_WhenQueueIsShort_FillsShortfallAndSchedulesOriginalCountReplacement` | `:119` | `DequeueChunk` shortfall branch `:96-102`, replenish `:104`, dequeue loop `:106-113` |
| `Reset_DisposesQueuedViewersAndClearsQueue` | `:153` | `Reset` `:116-124` with a non-null `_disposeViewer` |

Additional indirect exercise: `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs`
constructs `ViewerQueueCore<EfcViewer>` (`:308`) and `ViewerQueueCore<ItemViewer>` (`:322`) and
drives them through the static wrappers, which re-covers the same core members with different
priority/count arguments. It adds no coverage of members that `ViewerQueueCoreTests.cs` misses.

**No existing test covers any argument-validation path.**

---

## 4. Per-member coverage gap

Indicative baseline read from the already-committed artifact
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml:3771-4111`
(class `QuickFiler.ViewerQueueCore<TViewer>`, recorded `line-rate="0.937173"`,
`branch-rate="0.833333"`). Line numbers in that artifact align exactly with the current file, so the
file is unchanged since that run. Authoritative figures come from F1's harness at execution time.

**Every zero-hit line in the file is `:152-157`** — the body of the `ArgumentOutOfRangeException`
throw in `ValidateCount` (artifact `:4103-4108`). There are no other uncovered lines.

| Member | Status | Detail |
| --- | --- | --- |
| ctor `:18-35` | **partially covered (branches missed)** | lines all hit; `:26`, `:28`, `:31` each at `condition-coverage="50%"` (artifact `:3974-3991`) — the three `ArgumentNullException` throws are never taken. `:33` at 100% (both the null and non-null `blockingPriorityScheduler` paths are exercised). |
| `Count` `:37` | covered | — |
| `BuildQueue(int)` `:39-50` | covered | loop branch at `:44` 100% |
| `BuildQueue(int, priority)` `:52-61` | covered | loop branch at `:57` 100%; lambda `:59` covered |
| `Dequeue` `:63-85` | covered | `if` at `:75` 100% (both branches). The *throw* inside `ThrowIfCancellationRequested` at `:71` is exercised by `Dequeue_WithCanceledToken_ThrowsBeforeCreatingViewer`. |
| `DequeueChunk` `:87-114` | covered | `if` at `:96` and `for` at `:108` both reported 100%. See §6-I5: the no-shortfall post-condition is nevertheless unasserted anywhere. |
| `Reset` `:116-124` | covered | `while` at `:119` and `?.` at `:122` both 100% |
| `CreateWithPriority` `:126-142` | **partially covered** | all lines hit; the `ThrowIfCancellationRequested` at `:135` never throws in any existing test |
| `EnqueueWith` `:144-147` | covered | — |
| `ValidateCount` `:149-159` | **partially covered (uncovered lines 152-157; branch at `:151` 50%)** | the only zero-hit lines in the file |

---

## 5. Testability classification per member

Every member is **`pure-testable-now`**. There is no Outlook Interop type, no WinForms type, no
`System.Windows.Forms` reference, and no `UtilitiesCS.UiThread` reference anywhere in the file. The
only framework type is `System.Windows.Threading.DispatcherPriority` (`:4`), which is a plain
`enum` passed as data and never dereferenced — the file never touches a `Dispatcher` instance.

| Member | Classification | Note |
| --- | --- | --- |
| ctor, `Count`, both `BuildQueue` overloads, `Dequeue`, `DequeueChunk`, `Reset`, `CreateWithPriority`, `EnqueueWith`, `ValidateCount` | `pure-testable-now` | all four collaborators are already constructor-injected delegates (`:11-15`); the existing test helper at `ViewerQueueCoreTests.cs:169-183` proves the seam is sufficient |

`needs-seam`: none. `host-bound-irreducible`: none. No Moq mock is required — the seams are
delegates, and hand-written lambdas are both simpler and the established pattern in the existing
test file.

---

## 6. Ordering, concurrency and static-state invariants

This is the load-bearing section for the cluster. Each invariant below is stated, evidenced, and
paired with a deterministic test strategy. No invariant requires time, so **no `TimeProvider` /
`FakeTimeProvider` is needed for this file** (`00-cluster-overview.md` §4 remains the rule for any
F4 file that does need time).

**I1 — FIFO, strictly.** The backing store is `System.Collections.Generic.Queue<TViewer>` (`:16`);
`Enqueue` at `:59`/`:146` and `Dequeue` at `:77`/`:110`/`:121`. There is no priority reordering: the
`DispatcherPriority` arguments select *when the scheduler runs the enqueue action*, not the queue
order. Deterministic test: inject a synchronous scheduler and a counting factory, then assert the
dequeued identity sequence equals the creation sequence. (Already asserted for the chunk path at
`ViewerQueueCoreTests.cs:145`.)

**I2 — No capacity limit; `DequeueChunk` can grow the queue without bound.** `DequeueChunk`
replenishes with `BuildQueue(originalCount, replacementPriority)` at `:104` — by the *pre-call*
queue depth, not by `count`. Post-condition: `final = originalCount + max(0, count - originalCount)
+ originalCount - count`. For `originalCount=1, count=3` that is `1` (matches the assertion at
`ViewerQueueCoreTests.cs:147`). For `originalCount=5, count=2` it is `8`; for `originalCount=2,
count=0` it is `4`. **Repeated chunk dequeues from a well-stocked queue therefore grow the queue
geometrically.** This is a documented-behaviour finding, not a defect F4 may fix (no behaviour
change; §9). Deterministic test: T11 and T12 below assert the exact post-condition counts so the
invariant becomes executable documentation.

**I3 — Empty-queue behaviour on `Dequeue`.** When `_queue.Count == 0` (`:75` false) the viewer is
created through `_blockingPriorityScheduler` at `emptyQueuePriority` and is **returned without ever
being enqueued** (`:82-84`); replacements are then built with `emptyReplacementCount`. Deterministic
test: existing test at `ViewerQueueCoreTests.cs:58`.

**I4 — `CreateWithPriority` returns `null` if the injected scheduler does not invoke the action.**
`viewer` is initialised to `null` at `:131` and is only assigned inside the closure (`:136`). A
scheduler that queues without executing (which is exactly what the production
`Dispatcher.InvokeAsync` fire-and-forget delegate does — `ItemViewerQueue.cs:21`,
`EfcViewerQueue.cs:20`) makes `Dequeue` return `null`. Deterministic test: T15 (inject a
no-op blocking scheduler; assert `null` is returned and no exception is thrown).

**I5 — `DequeueChunk` shortfall fill runs through the *blocking* scheduler; replenishment runs
through the *priority* scheduler.** `:98-101` uses `_blockingPriorityScheduler` with
`missingViewerPriority`; `:104` uses `BuildQueue(int, priority)` which uses `_priorityScheduler`.
If the queue is already deep enough, the blocking scheduler is never invoked. Deterministic test:
T12 (assert the blocking-scheduler recorder list is empty).

**I6 — Validation happens before any mutation.** `ValidateCount` is called at `:41`, `:54`, `:72`,
`:73`, `:93` before any enqueue/dequeue. `Dequeue` validates *both* replacement counts (`:72`,
`:73`) before touching the queue. Deterministic test: T6, T7 assert the queue depth is unchanged
after the throw.

**I7 — Cancellation is checked twice and the second check is a real, reachable path.** `:71` at
entry, and `:135` *inside* the blocking-scheduler closure. The second check exists so that a
cancellation raised while the action waits in the dispatcher queue is honoured. Deterministic test
without any wall-clock wait: inject a blocking scheduler that calls `source.Cancel()` and *then*
invokes the action; assert `OperationCanceledException` and that the factory was never called (T14).

**I8 — No thread-safety whatsoever.** The file contains no `lock`, no `Interlocked`, no
`Concurrent*` type, and `Queue<T>` is explicitly not thread-safe. Combined with the production
`ProductionPriorityScheduler` being fire-and-forget `Dispatcher.InvokeAsync`
(`ItemViewerQueue.cs:21`, `EfcViewerQueue.cs:20`), a production `BuildQueue(count, priority)` call
returns *before* any `_queue.Enqueue` has run, and those enqueues later execute on the WPF UI thread
while callers such as `QuickFiler/Controllers/QfcQueue.cs:336` may call `Dequeue` from a different
thread. **This is a latent concurrency defect, not an F4 deliverable.** F4 is a coverage child bound
by "no behaviour change to observable QuickFiler flows" (`issue.md:69`), and a fix would touch
`QfcQueue.cs` (F2-owned) and `QfcCollectionController.cs` (F11-owned). Recommended action: promote a
separate issue through the promotion lifecycle titled
`viewerqueuecore-unsynchronised-queue-across-dispatcher-boundary`, and in F4 only *document* the
invariant by asserting the observable consequence deterministically: after
`BuildQueue(n, priority)` with a scheduler that defers (does not invoke) the action, `Count` is
still `0` (T10 covers the zero-count case; the deferral case is folded into T10's assertion shape).

**I9 — Enqueuing the same viewer instance twice is permitted and produces two queue entries.**
There is no identity check. A factory that returns a singleton yields two entries pointing at one
object; `Reset` then invokes `_disposeViewer` **twice on the same instance** (`:121-122`), which is
a double-dispose. Deterministic tests: T19 (two dequeues return the same reference in FIFO order)
and T20 (the disposer is invoked once per queue entry, i.e. twice for one instance). T20 is the test
that makes the double-dispose hazard visible; it asserts current behaviour and must not be written
as an aspiration.

**I10 — Disposal semantics.** `Reset` is the only cleanup path (`:116-124`). It drains the queue and
invokes `_disposeViewer` per entry if supplied; if `_disposeViewer` is `null` the viewers are simply
dropped (`?.` at `:122`). `ViewerQueueCore` itself is not `IDisposable` and holds no unmanaged
resource. The two static wrappers construct their cores **without** a dispose delegate
(`ItemViewerQueue.cs:115-120`, `EfcViewerQueue.cs:93-98` pass only four arguments), so in production
pooled viewers are dropped, never disposed. Documented, not changed.

**I11 — No static mutable state in this file.** Confirmed by full read: the only `static` member is
`private static void ValidateCount(int)` (`:149`), which is pure. Therefore no reset seam is needed
here and no test in this file can leave residue for another test.

---

## 7. Static-state test-isolation analysis

Not applicable to this file (see I11). `ViewerQueueCoreTests.cs` constructs a fresh
`ViewerQueueCore<FakeViewer>` per test via the helper at `:169-183`; every test is independent and
order-insensitive by construction. The class carries no `[DoNotParallelize]` and needs none.

The isolation analysis that *does* matter for this cluster lives in `10-ItemViewerQueue.md` §7 and
`11-EfcViewerQueue.md` §7.

---

## 8. Seam proposal

**No new seam is required.** The file is already at the top of the hierarchy in
`.claude/rules/csharp.md` § DI Seams for its situation: all four collaborators are
constructor-injected `Func<>`/`Action<>` delegates with production defaults supplied by the callers
(`:18-24`), which is exactly the "injectable delegate seam" tier, and an interface seam would be
excessive for single-call-path collaborators. `blockingPriorityScheduler` and `disposeViewer` are
already optional with safe defaults (`:22-23`, `:33-34`) — the shape that
`00-cluster-overview.md` §7.2 names as the cluster's preferred additive pattern.

Ranked evaluation:

1. **Selected — keep the existing injectable-delegate seam unchanged.** Zero production diff, zero
   sibling risk, and the existing test helper (`ViewerQueueCoreTests.cs:169-183`) already proves it
   reaches every member. The only uncovered lines (`:152-157`) are reachable through the public
   surface with a negative argument; no seam is involved.
2. **Rejected — extract an `IViewerScheduler` interface** (`Invoke(Action)`,
   `Invoke(Action, DispatcherPriority)`, `InvokeBlocking(Action, DispatcherPriority)`). It would
   replace three delegate parameters with one interface and enable `Mock<IViewerScheduler>`
   verification. Rejected: it changes the `internal` constructor signature, forcing edits to
   `ItemViewerQueue.cs:115-120` and `EfcViewerQueue.cs:93-98` (both F4-owned, so no sibling
   conflict) **and** to `ViewerQueueStaticWrapperTests.cs:308-313, 322-327` — churn with no coverage
   gain, and `.claude/rules/csharp.md:52` prefers the delegate seam when the call path is single.
3. **Rejected — inject `TimeProvider`.** The file has no time dependence (§1). Adding one would be
   dead weight.

---

## 9. Cross-child conflict analysis

Repository-wide grep for `ViewerQueueCore` (case-sensitive, all file types) returns the following.

**Production references — all inside F4's own file set:**

| Call site | Owner | Nature |
| --- | --- | --- |
| `QuickFiler/Helper Classes/ItemViewerQueue.cs:29` | **F4** | field declaration `ViewerQueueCore<ItemViewer> _core` |
| `QuickFiler/Helper Classes/ItemViewerQueue.cs:69` | **F4** | `SetCoreForTesting(ViewerQueueCore<ItemViewer>)` parameter |
| `QuickFiler/Helper Classes/ItemViewerQueue.cs:93` | **F4** | return type of `CreateProductionCore()` |
| `QuickFiler/Helper Classes/ItemViewerQueue.cs:108` | **F4** | return type of `CreateProductionCore(4 delegates)` |
| `QuickFiler/Helper Classes/ItemViewerQueue.cs:115` | **F4** | `new ViewerQueueCore<ItemViewer>(...)` — constructor call |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs:27` | **F4** | field declaration |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs:48` | **F4** | `SetCoreForTesting` parameter |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs:71` | **F4** | return type |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs:86` | **F4** | return type |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs:93` | **F4** | `new ViewerQueueCore<EfcViewer>(...)` |

**Test references — all F4-owned:** `QuickFiler.Test/Helper Classes/ViewerQueueCoreTests.cs:13, 169,
176`; `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:302, 308, 316, 322`.

**Build references:** `QuickFiler/QuickFiler.csproj:354` (shared file — existing line, no edit
needed); `QuickFiler.Test/QuickFiler.Test.csproj:164` (shared file — existing line).

**Documentation references (non-code, no conflict):** `epic.md:283`,
`docs/features/active/2026-08-07-.../issue.md:23`, `spec.md:18`, `user-story.md:21`,
`00-cluster-overview.md:127`.

**Verdict: zero cross-child call sites.** The type is `internal` to `QuickFiler.dll`, so only
QuickFiler and `QuickFiler.Test` (via `InternalsVisibleTo`,
`QuickFiler/Properties/AssemblyInfo.cs:5`) can name it, and within QuickFiler the only referrers are
the two F4-owned wrappers. Explicit statement for each proposal in §8: **"requires no sibling-owned
file change."** Because the selected proposal is a no-op production diff, the file's production
content is not modified at all by F4 — the entire F4 change for this file is new test methods.

---

## 10. 500-line compliance

- `ViewerQueueCore.cs` — 161 lines, **compliant**, 439 lines of headroom. No partial split needed;
  none is proposed, since the selected seam proposal adds zero production lines.
- **No new production file is proposed for this file.** If a future phase did add one, it would
  require a `<Compile Include="Helper Classes\<name>.cs" />` line in `QuickFiler/QuickFiler.csproj`
  inside the contiguous `Helper Classes\` block at `:342-354` — a shared-file edit touched by all
  fourteen siblings and therefore a merge-conflict surface
  (`00-cluster-overview.md` §7.3). Avoided here.
- **Test-file budget.** `ViewerQueueCoreTests.cs` is 195 lines. Nineteen new tests at the density of
  the existing file (~13 lines each after CSharpier) project to ~445 lines — within 500 but with no
  margin for reflow. **Recommendation: put the thirteen argument-validation and boundary tests in a
  new file** `QuickFiler.Test/Helper Classes/ViewerQueueCoreValidationTests.cs` (projected ~215
  lines) and add only the six behavioural tests to the existing file (projected ~275 lines). That
  needs one `<Compile Include="Helper Classes\ViewerQueueCoreValidationTests.cs" />` line in
  `QuickFiler.Test/QuickFiler.Test.csproj`, inserted inside the contiguous `Helper Classes\` block at
  `:158-165` alongside the cluster's other two additions, producing a single git hunk
  (`00-cluster-overview.md` §1.3).

---

## 11. Recommended test cases (enumerated individually)

MSTest + FluentAssertions. Moq is **not** used for this file — the seams are delegates and the
existing file's hand-written lambdas are both simpler and the established local pattern
(`ViewerQueueCoreTests.cs:169-183`). All tests use the private `FakeViewer` type; **no WinForms or
Interop type is constructed by any test below.**

Destination key: **[A]** = existing `QuickFiler.Test/Helper Classes/ViewerQueueCoreTests.cs`;
**[B]** = new `QuickFiler.Test/Helper Classes/ViewerQueueCoreValidationTests.cs`.

| # | `[TestMethod]` name | Arrange / Act / Assert | Category | Dest |
| --- | --- | --- | --- | --- |
| T1 | `Constructor_WithNullViewerFactory_ThrowsArgumentNullException` | Arrange non-null schedulers; Act construct with `viewerFactory: null`; Assert `ArgumentNullException` with `ParamName == "viewerFactory"` (covers branch at `:26`) | invalid-input | [B] |
| T2 | `Constructor_WithNullSynchronousScheduler_ThrowsArgumentNullException` | Act construct with `synchronousScheduler: null`; Assert `ParamName == "synchronousScheduler"` (`:28`) | invalid-input | [B] |
| T3 | `Constructor_WithNullPriorityScheduler_ThrowsArgumentNullException` | Act construct with `priorityScheduler: null`; Assert `ParamName == "priorityScheduler"` (`:31`) | invalid-input | [B] |
| T4 | `BuildQueue_WithNegativeCount_ThrowsArgumentOutOfRangeException` | Arrange a counting factory; Act `BuildQueue(-1)`; Assert `ArgumentOutOfRangeException` with `ParamName == "count"`, `ActualValue == -1`, message contains "Queue counts cannot be negative", and the factory was never called (**first coverage of `:152-157`**) | invalid-input | [B] |
| T5 | `BuildQueueWithPriority_WithNegativeCount_ThrowsAndDoesNotInvokeScheduler` | Act `BuildQueue(-3, DispatcherPriority.Background)`; Assert throw and the recorded priority list is empty | invalid-input | [B] |
| T6 | `Dequeue_WithNegativeCachedReplacementCount_ThrowsBeforeDequeuing` | Arrange `BuildQueue(2)`; Act `Dequeue(None, Render, -1, 1, ContextIdle)`; Assert throw and `Count == 2` (validation precedes mutation, I6) | invalid-input | [B] |
| T7 | `Dequeue_WithNegativeEmptyReplacementCount_ThrowsBeforeCreatingViewer` | Arrange empty queue; Act `Dequeue(None, Render, 1, -1, ContextIdle)`; Assert throw and factory never called | invalid-input | [B] |
| T8 | `DequeueChunk_WithNegativeCount_ThrowsArgumentOutOfRangeException` | Act `DequeueChunk(-2, Render, ContextIdle)`; Assert throw and queue depth unchanged | invalid-input | [B] |
| T9 | `BuildQueue_WithZeroCount_ReturnsExistingDepthAndCreatesNothing` | Arrange `BuildQueue(2)`; Act `BuildQueue(0)`; Assert return `== 2`, `created == 2` — documents that the return value is total depth, not the number built | boundary | [B] |
| T10 | `BuildQueueWithPriority_WithZeroCount_DoesNotInvokeScheduler` | Act `BuildQueue(0, Background)`; Assert recorded priority list empty and `Count == 0` | boundary | [B] |
| T11 | `DequeueChunk_WithZeroCount_ReturnsEmptyAndDoublesQueueDepth` | Arrange `BuildQueue(2)`; Act `DequeueChunk(0, Render, ContextIdle)`; Assert result empty, blocking scheduler never invoked, `Count == 4` (executable documentation of I2) | boundary | [B] |
| T12 | `DequeueChunk_WhenQueueAlreadyDeepEnough_SkipsBlockingSchedulerAndGrowsQueue` | Arrange `BuildQueue(5)`; Act `DequeueChunk(2, Render, ContextIdle)`; Assert blocking list empty, returned ids `1,2`, `Count == 8` (I2, I5) | boundary | [B] |
| T13 | `Reset_WithNoDisposeViewer_ClearsQueueWithoutThrowing` | Arrange core built with `disposeViewer: null`, `BuildQueue(3)`; Act `Reset()`; Assert `Count == 0`, no throw (`?.` null path at `:122`) | boundary | [B] |
| T14 | `Reset_OnEmptyQueue_DoesNotInvokeDisposer` | Arrange core with a recording disposer, empty queue; Act `Reset()`; Assert disposer list empty (`while` zero-iteration path) | boundary | [B] |
| T15 | `Dequeue_WhenTokenCanceledInsideBlockingScheduler_ThrowsOperationCanceled` | Arrange empty queue and a blocking scheduler that calls `source.Cancel()` then invokes the action; Act `Dequeue(source.Token, ...)`; Assert `OperationCanceledException` and factory never called (**first coverage of the `:135` throw**, I7) | error-handling | [A] |
| T16 | `Dequeue_WhenBlockingSchedulerDoesNotInvokeAction_ReturnsNull` | Arrange empty queue and a no-op blocking scheduler; Act `Dequeue(...)`; Assert result is `null` and no exception (I4) | error-handling | [A] |
| T17 | `Dequeue_WhenViewerFactoryThrows_PropagatesExceptionAndLeavesQueueUnchanged` | Arrange factory throwing `InvalidOperationException`, empty queue; Act `Dequeue`; Assert the same exception type surfaces and `Count == 0` | error-handling | [A] |
| T18 | `Constructor_WithNullBlockingPriorityScheduler_FallsBackToPriorityScheduler` | Arrange core with `blockingPriorityScheduler: null` and a recording `priorityScheduler`; Act `Dequeue` on an empty queue; Assert the recorder saw the `emptyQueuePriority` value, proving the `:33` fallback is wired | positive | [A] |
| T19 | `Dequeue_WithSingletonFactory_ReturnsSameInstanceForEachQueueEntry` | Arrange a factory returning one shared `FakeViewer`; `BuildQueue(2)`; Act two `Dequeue` calls; Assert both results are `ReferenceEquals` to the shared instance (I9 — no duplicate detection) | positive | [A] |
| T20 | `Reset_WithSingletonFactory_InvokesDisposerOncePerQueueEntry` | Arrange singleton factory + recording disposer, `BuildQueue(2)`; Act `Reset()`; Assert the disposer was invoked **twice** with the same instance (I9 double-dispose, asserted as current behaviour) | error-handling | [A] |

**Total: 20 recommended test cases** (6 in [A] plus 14 in [B]).

**Excluded as duplicates of existing coverage:**

- A FIFO-ordering test across a chunk shortfall — already asserted at
  `ViewerQueueCoreTests.cs:145` (`viewers.Select(v => v.Id).Should().Equal(1, 2, 3)`).
- A "`Dequeue` on a non-empty queue returns the cached viewer and schedules one replacement" test —
  `ViewerQueueCoreTests.cs:29-55`.
- A "`Dequeue` on an empty queue creates via the blocking scheduler" test —
  `ViewerQueueCoreTests.cs:58-92`.
- A "cancelled token throws before creating" test — `ViewerQueueCoreTests.cs:95-116`.
- A "`Reset` disposes queued viewers and clears" test — `ViewerQueueCoreTests.cs:153-167`.
- A "`BuildQueue(n)` with a synchronous scheduler creates n viewers" test —
  `ViewerQueueCoreTests.cs:16-26`.
- Wrapper-level delegation assertions — owned by `10-ItemViewerQueue.md` and
  `11-EfcViewerQueue.md`; duplicating them here would double-count.

---

## 12. STA determination

**No member of this file requires an STA thread, and no `*.StaTests.cs` file is proposed.**

Rationale against the exhaustion standard in `epic.md` Shared Design §3: the STA clause applies only
where no seam can isolate the logic. Here every collaborator is already a constructor-injected
delegate (§5), no WinForms control type appears in the file, and `DispatcherPriority` is a value-type
`enum` that is never dereferenced. The existing six tests run today with no apartment attribute in a
project that has no STA infrastructure at all (`00-cluster-overview.md` §5), which is direct evidence
that the seam is sufficient. The clause is therefore not reached.

---

## 13. Projected coverage

Indicative pre-change state (feature-#424 artifact, §4): 97 sequence-point lines, 6 uncovered
(`:152-157`), recorded class `line-rate="0.937173"`, `branch-rate="0.833333"`. Already above the 80%
floor.

Projected post-change state:

- T4 (and T5-T8) execute `:152-157`, removing **all six** remaining uncovered lines → **projected
  line coverage 97/97 = 100%.**
- T1-T3 take the three missed `??`-throw branches at `:26`, `:28`, `:31`; T15 takes the `:135`
  cancellation throw. Combined with the branches already at 100%, projected branch coverage is
  100% of the branches Cobertura reports for this class, up from `0.833333`.

Argument that the 80% floor is cleared: the file has no host-bound, COM-bound, or WinForms-bound
line — §5 classifies every member `pure-testable-now` — so there is no irreducible remainder to
subtract. The floor is cleared with a large margin both before and after the change; the work in
this phase is closing the argument-validation and invariant-documentation gap required by the
acceptance criterion at `issue.md:65-66`, not rescuing a failing file.

**No exemption is requested for this file.** It should be recorded in F1's ledger
(`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`) as **`testable`**, with the
authoritative numeric verification produced by F1's harness at execution time and committed under
`<FEATURE>/evidence/qa-gates/`.

---

## 14. Findings to carry into the F4 plan

1. **Zero cross-child conflict** for this file (§9) — the safest file in the cluster to change.
2. **The only uncovered lines are the `ValidateCount` throw** (`:152-157`); one test closes them.
3. **Latent concurrency defect (I8)** — unsynchronised `Queue<T>` mutated across the WPF dispatcher
   boundary while `QfcQueue.cs:336` (F2) and `QfcCollectionController.cs:617, 958` (F11) dequeue.
   Out of F4 scope; **promote as a separate issue** rather than recording it only in this artifact.
4. **Latent double-dispose (I9)** — `Reset` invokes `_disposeViewer` per queue entry, so a pooled
   viewer enqueued twice is disposed twice. Currently unreachable in production because neither
   wrapper supplies a dispose delegate (I10). Assert current behaviour (T20); do not "fix".
5. **Unbounded `DequeueChunk` growth (I2)** — replenishment by `originalCount` rather than `count`.
   Assert current behaviour (T11, T12); do not "fix".
6. **No banned API and no time dependence** — no `TimeProvider` seam is required for this file.
