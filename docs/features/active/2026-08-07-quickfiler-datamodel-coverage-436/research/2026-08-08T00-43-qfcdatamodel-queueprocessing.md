# Research: `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`

- Feature: `quickfiler-datamodel-coverage` (issue #436), child F5 of epic `quickfiler-per-file-coverage` (#136)
- Target file: `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` — 177 lines, no `[ExcludeFromCodeCoverage]` of its own
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a923053598cf4ccea`
- Created: 2026-08-08T00-43
- Scope: this one production file. Sibling partials `QfcDatamodel.cs` and `QfcDatamodel.FrameBuilding.cs`,
  and `EfcDataModel.cs`, are researched separately and appear here only where a cross-file consequence
  is unavoidable.
- Companion artifact (read first, built upon here):
  `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-qfcdatamodel.md`

---

## 0. Executive summary

1. **This file needs no new seam.** It is the only file in the cluster that dereferences **zero**
   Outlook COM members. `MailItem` appears only as a generic type argument and as a list element;
   nothing in the file reads a property or calls a method on it. Every collaborator is already behind
   an existing seam: `IEmailMoveMonitor` (`_moveMonitor`), `IApplicationGlobals` (`_globals`),
   `TimeProvider` (`QfcDatamodel.cs:112`), and the pure `LockingLinkedList<MailItem>`
   (`_masterQueue`). The single transitive COM reach is the scorer method group
   `ScoreRemainingQueueMailItemAsync` handed to the gate at line 119, which the sibling artifact's
   seam **S1** (`IFolderScoringService ScoringService`) already resolves. §6 recommends **reuse S1,
   add nothing**.
2. **Confirmed independently:** the `[ExcludeFromCodeCoverage]` at `QfcDatamodel.cs:25` is
   type-scoped and currently removes this file from the coverage denominator (§3.1). Verified by
   direct search of the committed Cobertura artifact and its companion delta note.
3. **Current read-derived line coverage of this file, once the attribute is removed, is roughly
   57% (~27 of ~47 executable lines) — below the 80% floor.** Four existing tests reach the file;
   `DequeueNextItemGroup(int)` (the synchronous entry point, lines 132–143), `UndoMove` (24–27), the
   null-batch arm of `UnhookDequeuedNodes` (147–150), its rethrow boundary (160–164), and the entire
   failure path of `TryUnhookOrReplace` (47–62) are wholly untouched. Confidence: medium (hand count,
   §3.3).
4. **Nineteen concurrency/ordering invariants are enumerated in §2 with line citations.** Four of
   them are currently pinned by no test at all and are the highest-value targets: the permanent
   discard of below-threshold candidates from the master queue (I6), the null-vs-empty return
   asymmetry between normal and high-confidence mode at `quantity <= 0` (I5), the batch-shrink
   interaction between `UnhookDequeuedNodes`' pre-captured `max` and `TryUnhookOrReplace`'s
   range guard (I13), and the first-batch-deadline propagation from the two-argument overload (I7).
5. **Two latent defects were found and are recorded as promote-to-issue candidates, not as changes**
   (AC7 forbids behavior change): the `quantity <= 0` null return that `QfcHomeController.Iteration.cs:25`
   dereferences without a null check (§9 R2), and a batch node that can be returned to the caller
   still hooked in the move monitor after a shrink (§9 R3).
6. **No new production seam, no file split, and no `[ExcludeFromCodeCoverage]` for this file.** The
   file stays at 177 lines. All test cases in §7 land in four new test files so this phase does not
   serialise against `QfcDatamodelTests.cs` (317 lines) or `QfcDatamodelLivenessTests.cs` (255).

---

## 1. Method and evidence basis

Every claim below is grounded in a file read in this session. Claims that could not be verified
without building or running are marked **INFERRED** with the reason.

Files read in full:

| Path | Purpose |
| --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | subject |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | the collaborator that owns deadline/poll/scan semantics (F2-owned, read-only) |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | cross-child contract |
| `QuickFiler/Interfaces/IEmailMoveMonitor.cs` (grepped members) | move-monitor seam |
| `UtilitiesCS/ReusableTypeClasses/Locking/LockingLinkedList.cs` (lines 54–435) | `_masterQueue` semantics, incl. `TryTakeFirst` overloads |
| `QuickFiler/Controllers/QfcDatamodel.cs` lines 20–33, 95–135, 344–379 | attribute, `TimeProvider`/seam properties, scorer body |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs`, `QfcHomeController.cs:240–320` | the only production call sites |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` (317), `QfcDatamodelLivenessTests.cs` (255), `QfcQueuePurePathsTests.cs` (136) | existing tests |
| `QuickFiler/Properties/AssemblyInfo.cs` | `InternalsVisibleTo("QuickFiler.Test")` at line 5 |
| `QuickFiler/packages.config`, `QuickFiler.Test/packages.config` | clock/testing package availability |
| `CLAUDE.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md` | policy |
| `docs/features/epics/quickfiler-per-file-coverage/epic.md`, `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/issue.md` | contract |
| the sibling research artifact for `QfcDatamodel.cs` | composition baseline |

Coverage-reality method: F1's per-file harness does not exist on disk yet (expected — F1 is prepared
concurrently). Current coverage is therefore derived by (a) reading every test that can reach this
file and mapping each to the lines it drives, and (b) cross-checking against the committed Cobertura
report from issue #424. F1's harness remains the authority; the plan must record its numeric output
under `<FEATURE>/evidence/qa-gates/`.

---

## 2. Member inventory and the concurrency / ordering invariants

### 2.1 Member inventory

`public partial class QfcDatamodel`, namespace `QuickFiler.Controllers`. Usings 1–6; namespace/class
8–11; closing braces 176–177.

| # | Member | Lines | Vis. | Behavior (one line) |
| --- | --- | --- | --- | --- |
| N1 | `_remainingLoadActive` | 12–21 | `private volatile bool` | Issue #424 producer-liveness flag; written in `QfcDatamodel.cs` (`Worker_DoWork` / `InitEmailQueue`), read here at lines 123 and 170. No IL of its own (no initializer). |
| N2 | `UndoMove()` | 23–27 | public | `IQfcDatamodel` member. Unconditionally `throw new NotImplementedException()`; carries a `//TODO` at line 23. |
| N3 | `TryUnhookOrReplace(ref List<MailItem> nodes, int i)` | 29–64 | internal | Unhooks `nodes[i]` from the move monitor; on failure, drops that node and substitutes the head of `_masterQueue` at the same index, retrying until an unhook succeeds or the master queue is exhausted. Guards null/empty/out-of-range by logging and returning. |
| N4 | `DequeueNextItemGroupAsync(int quantity, int timeOut)` | 66–76 | public async | `IQfcDatamodel` member. Delegates to N5 with `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` and a null progress sink. |
| N5 | `DequeueNextItemGroupAsync(int, int, TimeSpan, Action<int,int,int>)` | 78–99 | public async | `IQfcDatamodel` member (issue #424 overload). Throws if `_token` is cancelled; routes to N7 when high-confidence mode is on, else to N6 (dropping both extra arguments). |
| N6 | `DequeueDirectAsync(int quantity)` | 101–108 | private async | Awaits `WaitForQueue` only when the queue is short, then takes the first `quantity` items and unhooks them. |
| N7 | `DequeueWithHighConfidenceGateAsync(int, int, TimeSpan?, Action<int,int,int>)` | 110–130 | private async | Constructs a fresh `QfcStreamingDequeueConfidenceGate` per call from eight positional arguments, awaits `DequeueAsync`, unhooks the accepted nodes. |
| N8 | `DequeueNextItemGroup(int quantity)` | 132–143 | public | `IQfcDatamodel` member. Synchronous sibling of N5. Throws if `_token` is cancelled; in high-confidence mode calls N7 with `timeOut: 0` and a null deadline via `.GetAwaiter().GetResult()`; otherwise takes directly with no wait. |
| N9 | `UnhookDequeuedNodes(List<MailItem> nodes)` | 145–166 | private | Returns `null` for a null batch; otherwise loops `i` from 0 to the **pre-captured** `nodes.Count`, calling N3; logs and rethrows anything that escapes N3. |
| N10 | `WaitForQueue(int quantity, CancellationToken token)` | 168–175 | internal async | Polls at 200 ms through the injected `TimeProvider` while the producer is live **and** the queue is short; checks cancellation each iteration. |

### 2.2 Invariants

Each invariant is stated, cited, and paired with the shape a regression would take. The `[pinned]`
marker records whether any test in the repository asserts it today.

**I1 — Two-argument overload delegates with the default first-batch deadline and no progress sink.**
Lines 70–75 pass `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline`
(`QfcStreamingDequeueConfidenceGate.cs:22`, 12 s) and `null`. `[not pinned]`
*Regression shape:* passing `default(TimeSpan)` makes the gate constructor throw
`ArgumentOutOfRangeException` (gate lines 75–82), so every legacy two-argument caller
(`QfcHomeController.cs:261`, `QfcHomeController.Iteration.cs:21,63`, `QfcQueue.cs:476`) fails at
runtime; passing `Timeout.InfiniteTimeSpan` silently reintroduces the unbounded issue-#424 startup
stall.

**I2 — Cancellation is checked before any queue work, on both public entry points, from the field
token.** Line 85 and line 134: `_token.ThrowIfCancellationRequested()`. Note both read the **field**
`_token`, never a parameter; `WaitForQueue`'s `token` parameter is only ever fed `_token` (line 104).
`[not pinned]`
*Regression shape:* a dequeue proceeding after `Cleanup()` has cancelled the source, touching a
`_masterQueue`/`_moveMonitor` that `Cleanup()` has already nulled — an NRE during teardown.

**I3 — Mode selection is null-tolerant on globals and settings.** Lines 87 and 136 both use
`_globals?.QfSettings?.HighConfidenceModeEnabled == true`, so a null `_globals` or null `QfSettings`
selects the **direct** path rather than throwing. `[not pinned]`
*Regression shape:* replacing `?.` with `.` produces an NRE on any dequeue that races `Cleanup()`
(which sets `_globals = null`, `QfcDatamodel.cs:82` region).

**I4 — Once high-confidence mode is confirmed, `_globals.QfSettings` is dereferenced without
null-guarding.** Line 120 (`_globals.QfSettings.HighConfidenceThreshold`) has no `?.`, unlike line
87. This is a deliberate consequence of I3: the `== true` test proves both are non-null at that
instant. `[not pinned]`
*Regression shape:* if `_globals` is nulled between line 87 and line 120 by a concurrent `Cleanup()`,
line 120 NREs. This is a genuine (narrow) race; it is **not** in scope to fix.

**I5 — Return-shape asymmetry at `quantity <= 0`: normal mode returns `null`, high-confidence mode
returns an empty list.** Direct path: `_masterQueue.TryTakeFirst(quantity)` returns `null` when
`n < 1` (`LockingLinkedList.cs:403–406`), and `UnhookDequeuedNodes(null)` returns `null`
(lines 147–150). High-confidence path: the gate returns an empty `List<MailItem>` for
`quantity <= 0` (gate lines 96–99), which `.ToList()` and `UnhookDequeuedNodes` pass through as an
empty list. `[not pinned]`
*Regression shape:* this is already load-bearing — `QfcHomeController.Iteration.cs:25` evaluates
`listObjects.Count > 0` with no null check, so a zero/negative `ItemsPerIteration` in normal mode
NREs inside `IterateQueueAsync`'s `try`, is caught by the broad handler at line 42, and rethrows.
See §9 R2. A test must **pin the current shape**, not fix it.

**I6 — High-confidence dequeue permanently discards below-threshold candidates from the master
queue, and leaves them hooked.** The gate's take closure is `() => _masterQueue.TryTakeFirst()`
(line 118), a destructive single-item pop (`LockingLinkedList.cs:354–376`). The gate adds to
`accepted` only when `score >= _cutoff` (gate lines 138–141) and never returns rejected items to any
queue. Only accepted nodes reach `UnhookDequeuedNodes` (line 129). `[not pinned]`
*Regression shape:* two distinct failures. (a) If a future change requeues rejects, the gate loops
forever on a below-threshold head. (b) The current behavior leaks: a rejected item remains registered
with `IEmailMoveMonitor` for the process lifetime and its `BeforeItemMove` callback still fires
`x => _masterQueue.Remove(x)` (`QfcDatamodel.cs:358`) against a queue it has already left.

**I7 — The first-batch deadline is a per-call budget measured from gate construction, and the
synchronous path opts into the default.** `firstBatchDeadline` flows line 92 → line 124 → gate
constructor, where `null` selects the default (gate line 74). The gate timestamps at construction-time
`DequeueAsync` entry (gate line 102) and re-checks at the top of every iteration (gate line 110). N8
calls `DequeueWithHighConfidenceGateAsync(quantity, 0)` (line 138), taking the optional-parameter
defaults `firstBatchDeadline: null` and `progress: null` (line 113–114) — so the synchronous path
gets the 12 s default. `[not pinned]`
*Regression shape:* a deadline computed once per *instance* rather than per call would make the
second and later iterations return empty immediately; a deadline of zero throws at construction.

**I8 — The synchronous entry point blocks on the asynchronous gate.** Line 138:
`.GetAwaiter().GetResult()`. It is called from `QfcHomeController.Run()` (`QfcHomeController.cs:254`
region) and `Iterate()` (`Iteration.cs:66` selects the direct sibling, but `Iteration.cs:63` blocks
the async overload the same way). The gate's own awaits use `.ConfigureAwait(false)` (gate lines 128,
133), and `timeOut: 0` means the gate never reaches its delay (gate line 120), so today there is no
UI-thread pump requirement. `[not pinned]`
*Regression shape:* removing a `ConfigureAwait(false)` anywhere in the awaited chain — including in
`ScoreRemainingQueueMailItemAsync` (`QfcDatamodel.cs:371`) — deadlocks the Outlook UI thread. This is
**not** directly unit-testable without a hanging test; see §9 R5.

**I9 — The producer-liveness signal handed to the gate is the datamodel-owned volatile flag, not
`BackgroundWorker.IsBusy`.** Line 123 (`() => _remainingLoadActive`) supplies the gate's
`sourceActive`; the flag is `volatile` for cross-thread visibility (line 21 with the rationale in the
doc comment 12–20). `[pinned]` by `QfcDatamodelLivenessTests.cs:80` and `QfcDatamodelTests.cs:103`.
*Regression shape:* the original issue-#424 defect — `Worker_DoWork` is `async void`, so `IsBusy`
goes false at the first yielding await and the gate mistakes a transiently empty queue for an
exhausted source, returning an early partial batch (gate lines 119–123).

**I10 — `WaitForQueue` polls at exactly 200 ms through the injected `TimeProvider`, never through
wall-clock `Task.Delay`.** Line 173. `Task.Delay` and `Thread.Sleep` are banned symbols
(`.claude/rules/csharp.md` § Analyzer Stack). `[partially pinned]` — `QfcDatamodelTests.cs:284`
proves the delay is injected and completes after a 200 ms advance, but does not prove the interval is
exactly 200 ms (it never advances by less).
*Regression shape:* a wall-clock delay makes every dependent test time-dependent and flaky; a
zero-interval loop busy-spins a core during queue fill.

**I11 — `WaitForQueue` has two independent exit conditions and both are evaluated before the first
delay.** Line 170: `while (_remainingLoadActive && (_masterQueue?.Count < quantity))`. If the producer
is idle, or the queue already holds `quantity`, the method returns without touching `TimeProvider` and
**without checking cancellation** (the `ThrowIfCancellationRequested` at line 172 is inside the loop
body). Note the null-conditional on `_masterQueue`: for a null queue, `null < quantity` on `int?` is
`false`, so the loop exits. `[partially pinned]` — the producer-goes-idle exit is covered
(`QfcDatamodelTests.cs:309`); the queue-fills exit and the null-queue arm are not.
*Regression shape:* dropping the `_remainingLoadActive` conjunct hangs the dequeue forever once the
loader finishes with a short queue; dropping the count conjunct adds a 200 ms latency floor to every
dequeue.

**I12 — `WaitForQueue` is entered only when the queue is short.** Lines 103–104 guard the call.
`[not pinned]` (the existing test invokes `WaitForQueue` by reflection, bypassing the guard).
*Regression shape:* an unconditional wait imposes a 200 ms floor on every direct dequeue, i.e. the
whole normal-mode filing loop.

**I13 — `UnhookDequeuedNodes` iterates to a batch size captured *before* the loop, and relies on
`TryUnhookOrReplace`'s range guard to absorb shrinkage.** Line 154 captures `max = nodes.Count`;
`TryUnhookOrReplace` can permanently shrink `nodes` when the master queue is exhausted (line 52 with
no re-insert). The guard at line 31 (`nodes.Count < i + 1`) then logs and returns for the trailing
indices. `[not pinned]`
*Regression shape:* removing the guard produces `ArgumentOutOfRangeException` from `nodes[i]`
(line 38); recomputing `max` inside the loop changes which nodes get unhook attempts. Consequence in
the current code: after a shrink, the surviving tail nodes are returned to the caller **without ever
being unhooked** — see §9 R3.

**I14 — `TryUnhookOrReplace` retries the same index until an unhook succeeds or the master queue is
empty, and inserts each replacement at that same index.** Lines 39–63: `processing` stays true while
`UnhookItem` throws; each failure does `nodes.Remove(node)` (line 52), pops the master-queue head
(line 53), and `nodes.Insert(i, node)` (line 60) — preserving positional order for the rest of the
batch. Termination is guaranteed because each iteration removes exactly one master-queue element and
`TryTakeFirst()` returns `null` on empty (`LockingLinkedList.cs:354–361`), setting `processing = false`
at line 56. `[not pinned]`
*Regression shape:* a single-attempt version silently returns a still-hooked item; `nodes.Add`
instead of `Insert(i, ...)` scrambles batch order relative to the caller's slot assignment; failing to
null-check line 54 loops forever on an empty master queue.

**I15 — Every node returned to a caller has been through the unhook path.** All three dequeue paths
funnel their result through `UnhookDequeuedNodes` — lines 107, 129, 142. `[partially pinned]`
(`QfcQueuePurePathsTests.cs:105` proves it for the direct async path only).
*Regression shape:* a returned `MailItem` still hooked in `IEmailMoveMonitor` fires
`x => _masterQueue.Remove(x)` on a user-initiated move, mutating a queue the item has already left.

**I16 — `UnhookDequeuedNodes` is a logging boundary that rethrows; it does not swallow.** Lines
160–164: `logger.Error(...); throw;` (bare rethrow, stack preserved). Because `TryUnhookOrReplace`'s
inner `catch` (line 47) absorbs everything thrown by `_moveMonitor.UnhookItem`, the only exceptions
that reach line 160 originate *inside* that catch block — `nodes.Remove`, `_masterQueue.TryTakeFirst()`,
or `nodes.Insert`. `[not pinned]`
*Regression shape:* swallowing here hands the caller a partially unhooked batch with no signal.

**I17 — The gate is constructed fresh per call from eight positional arguments in a fixed order.**
Lines 117–126: `tryTakeNext`, `scoreLoader`, `threshold`, `TimeProvider`, `debugLog: null`,
`sourceActive`, `firstBatchDeadline`, `progressCallback`. `[not pinned]`
*Regression shape:* the compiler type-checks most positions, but `debugLog` (`Action<string>`) is
silently passed as `null` — if a future change passes a real logger into the wrong slot, or omits
`sourceActive`, I9 regresses silently (the gate treats a null `sourceActive` as "not active", gate
line 119).

**I18 — The extra issue-#424 arguments are honoured only in high-confidence mode and are dropped in
normal mode.** Lines 97–98 with the explanatory comment; `DequeueDirectAsync` takes only `quantity`.
`[not pinned]`
*Regression shape:* invoking the progress sink from the direct path would drive
`QfcScanProgressBandMapper` (`QfcHomeController.cs:298`) with counts that have no scanning semantics,
corrupting the 0→30 startup progress band.

**I19 — `timeOut` is the gate's empty-queue poll interval and is ignored entirely in normal mode.**
The gate delays `TimeSpan.FromMilliseconds(timeOut)` on an empty queue (gate lines 126–128) and
returns immediately when `timeOut <= 0` (gate line 120); the normal path never receives it and polls
at its own fixed 200 ms (line 173). `[not pinned]`
*Regression shape:* threading `timeOut` into `WaitForQueue` would change the pre-UI startup wait
that issue #424 deliberately tuned to 200 ms at `QfcHomeController.cs:301`.

---

## 3. Current coverage reality

### 3.1 The type-scoped exclusion — verified independently

The sibling artifact's central finding is confirmed against the cited evidence, by two searches
performed in this session:

- `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`:
  a search for `QfcDatamodel` returns exactly **one** hit, at line 21903, and it is an unrelated
  `set_DataModel` method signature `(QuickFiler.Interfaces.IQfcDatamodel)` on another type. There is
  **no class entry** for `QuickFiler.Controllers.QfcDatamodel`.
- `.../coverage-delta.2026-08-07T00-48.md:26` states verbatim:
  `| QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | n/a | n/a | same partial class, therefore also excluded |`,
  and line 39 records the changed methods in both partials as `excluded`.

**Conclusion.** `[ExcludeFromCodeCoverage]` at `QfcDatamodel.cs:25` is applied to the type, and a
partial type's attributes are unioned across declarations, so this file is outside the denominator
even though it carries no attribute. Its measured coverage today is not 0% — it is **absent**. Every
existing test listed in §3.2 does real work that is currently counted for nothing.

### 3.2 Test-to-line map (read-derived)

Four tests can reach this file. `QfcDatamodelTests.cs:21–219` (the five `TryQueueRemainingMailItemAsync_*`
tests) were checked and confirmed **not** to reach it: each calls `CreateQueueAdmission(...)`
(line 21) and then `admission.TryQueueAsync(...)`, constructing `QfcRemainingQueueAdmission` directly.

| Test | Lines of this file driven | Confidence |
| --- | --- | --- |
| `QfcDatamodelTests.cs:103` `DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive` | N4 (70–75); N5 (85, 87, 89); N7 (117–126, 128, 129) with an **empty** queue so the scorer is never invoked; the closures at 118 and 123; N9 (147 false-arm, 152–158 with zero iterations, 165) | high |
| `QfcDatamodelLivenessTests.cs:80` `DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` | identical line set to the above | high |
| `QfcDatamodelTests.cs:284` `WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay` | N10 in full (170, 172, 173, 175) via reflection; bypasses the guard at 103 | high |
| `QfcQueuePurePathsTests.cs:105` `DequeueNextItemGroupAsync_HighConfidenceDisabled_PreservesDirectBatchDequeue` | N4; N5 (85, 87 false-arm, 98); N6 (103 false-arm, 106, 107); N9 (147, 152–158, 165); N3 happy path (31 false-arm, 38, 39, 40, 42, 44, 45, 63) | high |

### 3.3 Genuinely uncovered lines

| Member | Uncovered lines | Why |
| --- | --- | --- |
| N2 `UndoMove` | 26 | No test invokes it. |
| N3 `TryUnhookOrReplace` | 33, 36 (guard true-arm); 47–62 (the entire failure/replacement path) | No test supplies a null/short batch or a throwing `IEmailMoveMonitor`. |
| N6 `DequeueDirectAsync` | 104 (`await WaitForQueue`) | The only direct-path test has `Count == quantity`, so the guard at 103 is false. `WaitForQueue` itself is covered only through reflection. |
| N8 `DequeueNextItemGroup` | 134, 136, 138, 141, 142 — **all** | No test calls the synchronous entry point. |
| N9 `UnhookDequeuedNodes` | 149 (null return); 160–164 (rethrow boundary) | Neither a null batch nor an escaping exception is produced by any test. |
| N7 (partial) | the gate's accept/reject/deadline/progress behavior as observed *through this file* | Both gate-driving tests use an empty queue, so no candidate is ever scored, accepted, rejected, or reported. |

Rough executable-line arithmetic: ~47 sequence points in the file, of which ~27 are reached — about
**57%**. **Confidence: medium.** This is a hand count of sequence points and does not model
compiler-generated async state-machine attribution; F1's harness is the authority and the plan must
record its numeric output.

Do **not** duplicate what already exists: the empty-queue poll behavior (I9), the injected-delay
mechanism (I10), and the direct-path happy case (I15 for that path) are already asserted. §7 targets
only the gaps.

---

## 4. Timing and clock dependencies

### 4.1 Does the repository already have a clock abstraction? — Yes; do not invent one

Searched for `interface IClock` across all `*.cs`: **no matches**. The established seam is
`System.TimeProvider`, mandated by `.claude/rules/csharp.md` § "Time seam (TimeProvider)":

- Production side: `Microsoft.Bcl.TimeProvider` **10.0.10** is already referenced by
  `QuickFiler/packages.config:19`, so `TimeProvider` and its
  `System.Threading.Tasks.TimeProviderTaskExtensions.Delay(this TimeProvider, TimeSpan, CancellationToken)`
  extension resolve under `net481` with the `using System.Threading.Tasks;` already present at line 5
  of this file.
- Test side: `Microsoft.Extensions.TimeProvider.Testing` is referenced by
  `QuickFiler.Test/packages.config:85`, supplying `FakeTimeProvider`, already used at
  `QfcDatamodelTests.cs:107,254,288` and `QfcDatamodelLivenessTests.cs:84`.
- The datamodel already exposes the seam: `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;`
  at `QfcDatamodel.cs:112`.

**No new clock abstraction is needed or permitted.**

### 4.2 Every time-dependent site in this file

| # | Site | Line | Nature | Deterministic mechanism |
| --- | --- | --- | --- | --- |
| C1 | `TimeProvider` handed to the gate | 121 | Feeds the gate's `GetTimestamp()` / `GetElapsedTime()` deadline clock (gate 102, 110) **and** its empty-queue `Delay` (gate 126–128) | Assign `model.TimeProvider = new FakeTimeProvider()`; advance with `fake.Advance(...)`. Deadline expiry is driven by advancing the fake clock from inside the injected scorer callback (see test 21). |
| C2 | `TimeProvider.Delay(TimeSpan.FromMilliseconds(200), token)` | 173 | The `WaitForQueue` poll | Same `FakeTimeProvider`; `fake.Advance(TimeSpan.FromMilliseconds(199))` must leave the task pending and a further 1 ms must complete it (test 27). |
| C3 | `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` | 73 | A compile-time-constant `TimeSpan` (12 s), not a clock read | Observable only as elapsed budget through C1. |
| C4 | `firstBatchDeadline` parameter | 81, 92, 113, 124 | Caller-supplied budget, including `Timeout.InfiniteTimeSpan` | Passed literally; assert via scan-count under C1. |
| C5 | `timeOut` (ms) | 79, 91, 112, 138 | The gate's poll interval, and the `timeOut <= 0` immediate-return switch (gate 120) | Pass `0` for immediate-return tests; pass a positive value plus fake-clock advances for polling tests. |

There is **no** `DateTime.Now`, `DateTime.UtcNow`, `Stopwatch`, `Thread.Sleep`, or `Task.Delay` in
this file — verified by reading all 177 lines. All wall-clock exposure is mediated by C1/C2.

### 4.3 The construction trap that must be written into every timing test

`TimeProvider` at `QfcDatamodel.cs:112` is an **auto-property with an initializer**, so the assignment
runs in the instance constructor. The established test construction path
`FormatterServices.GetUninitializedObject(typeof(QfcDatamodel))` (`QfcDatamodelTests.cs:231`) skips
constructors, leaving the backing field **null** — as it also leaves `_masterQueue` and `_moveMonitor`
null.

Consequence, and it is silent: with a null `TimeProvider`, `QfcStreamingDequeueConfidenceGate`'s
constructor falls back to `TimeProvider.System` (gate line 69). A high-confidence test that forgets
`model.TimeProvider = fake` therefore runs against the **real wall clock and a real 12-second
deadline** with no error. `WaitForQueue` fails loudly instead (a null `this` on the `Delay` extension).

**Mandate for the plan:** every test in files C and D of §7 assigns `model.TimeProvider` in Arrange,
and each of those test classes should route construction through one shared local
`CreateModelWithFakeClock(out FakeTimeProvider fake)` helper so the assignment cannot be forgotten.

---

## 5. Testability blockers, per uncovered member

| Member | Blocker analysis | Blocking? |
| --- | --- | --- |
| N2 `UndoMove` | None. Public, no state read. | No |
| N3 `TryUnhookOrReplace` | None. `internal`, and `QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`, so it is callable **directly** (`ref` argument included) with no reflection. `_moveMonitor` is `IEmailMoveMonitor` (mocked at `QfcQueuePurePathsTests.cs:119`); `_masterQueue` is the pure `LockingLinkedList<MailItem>`. `MailItem` is only a list element — never dereferenced. | No |
| N6 `DequeueDirectAsync` line 104 | None. Reachable by arranging `_masterQueue.Count < quantity`. Needs `TimeProvider` only if the loop body actually runs. | No |
| N7 with a **non-empty** queue | **Yes — one blocker.** The scorer method group at line 119 is `QfcDatamodel.ScoreRemainingQueueMailItemAsync`, whose body hard-codes `new FolderScoringService()` (`QfcDatamodel.cs:368`). `FolderScoringService` is COM-bound (`MailItemHelper.FromMailItemAsync` + `FolderPredictor`) and is itself `[ExcludeFromCodeCoverage]`. Resolved entirely by the sibling artifact's **S1**; nothing is needed in this file. Secondary note: the gate logs `mailItem.Subject` / `mailItem.EntryID` (gate line 165) — a loose `Mock<MailItem>` returns null and the interpolation tolerates it; a `MockBehavior.Strict` mock would throw, so tests must use loose mocks (the existing convention, `QfcDatamodelTests.cs:54`). | Yes → S1 |
| N8 `DequeueNextItemGroup` | None beyond the N7 blocker for its high-confidence arm. The `.GetAwaiter().GetResult()` at line 138 is safe in MSTest because no `SynchronizationContext` is installed on the test thread. | No |
| N9 lines 147–150 | None. Reached by `quantity <= 0` in normal mode. | No |
| N9 lines 160–164 | Reachable but only by an indirect trigger. `TryUnhookOrReplace`'s inner `catch` absorbs everything from `UnhookItem`, so the escaping exception must originate inside that catch: set `_masterQueue = null` and use a throwing `IEmailMoveMonitor`, so `_masterQueue.TryTakeFirst()` at line 53 raises `NullReferenceException`, which escapes to line 160. Because `_masterQueue` is null, the batch cannot come from a public dequeue, so `UnhookDequeuedNodes` (private) is invoked by reflection — an established pattern (`QfcDatamodelTests.cs:263,298`). Recorded as slightly indirect; it is the only trigger available without adding a seam. | No |
| N10 `WaitForQueue` uncovered arms | None. `internal`, directly callable. Requires `TimeProvider` assignment only for arms that reach line 173. | No |
| Construction | `FormatterServices.GetUninitializedObject` is the established path and leaves `_masterQueue`, `_moveMonitor`, `_token`, `_globals`, and `TimeProvider` unset; each test assigns what it reads via the existing `SetPrivateField` helper. `FormatterServices` is obsolete on .NET 5+ but the target framework here is `net481`, where it is current. Not a blocker. | No |

**Summary: exactly one blocker, and it is already solved by a sibling seam in the same feature.**

---

## 6. Seam proposals

### 6.1 Recommendation — reuse sibling **S1**; introduce no new seam in this file

Ranked against the hierarchy in `.claude/rules/csharp.md` § DI Seams (interface > delegate > adapter):

| Rank | Seam | Owner | Verdict |
| --- | --- | --- | --- |
| 1 | **S1 — `internal IFolderScoringService ScoringService { get; set; }`** on `QfcDatamodel`, consumed at `QfcDatamodel.cs:368` as `ScoringService ?? new FolderScoringService()` | sibling file `QfcDatamodel.cs` (declared in the proposed `QfcDatamodel.Construction.cs`) | **Adopt.** Interface seam — the highest tier. The interface already exists at `QfcHighConfidencePreFilter.cs:130` with signature `Task<(long Score, string TopFolder)> ScoreAsync(MailItem, IApplicationGlobals, CancellationToken)`, so no new abstraction is created. It unblocks every high-confidence test of this file (N7, N8's gate arm) because line 119's method group resolves through it. |
| 2 | Existing `TimeProvider` (`QfcDatamodel.cs:112`) | already present | **Reuse as-is.** No change. |
| 3 | Existing `IEmailMoveMonitor` (`_moveMonitor`) | already present | **Reuse as-is.** No change. |
| — | **Q2 (rejected by default) — `internal Func<MailItem, CancellationToken, Task<long>> RemainingItemScorer { get; set; }`**, consumed at line 119 as `RemainingItemScorer ?? ScoreRemainingQueueMailItemAsync` | would be this file's only new seam | **Reject unless phase ordering forbids depending on S1.** It is a lower-tier delegate seam that duplicates S1's purpose on the same code path, violating "introduce the smallest seam that enables reliable unit testing". Its only merit is decoupling this file's test phase from the `QfcDatamodel.cs` phase. If the plan does adopt it, it must be declared in `QfcDatamodel.Construction.cs` per the sibling's one-DI-surface coordination note, **not** in `QfcDatamodel.QueueProcessing.cs`. |
| — | An interface around `QfcStreamingDequeueConfidenceGate` | would require editing an F2-owned file | **Prohibited.** `QfcStreamingDequeueConfidenceGate.cs` belongs to sibling F2 and must not be modified. |
| — | A gate-factory delegate seam on the datamodel | would be this file's own | **Reject.** It would replace the real gate in tests and therefore *weaken* rather than pin invariants I6/I7/I17/I18. §7 shows all four are observable through the real gate using only `FakeTimeProvider` plus S1. |

### 6.2 Additivity confirmation

| Item | Touches `IQfcDatamodel`? | Touches a public signature? | Cross-child impact |
| --- | --- | --- | --- |
| No production change to `QfcDatamodel.QueueProcessing.cs` | No | No | None |
| S1 (sibling-owned) | No | No | None |

All four `IQfcDatamodel` members implemented in this file — `DequeueNextItemGroupAsync(int,int)`
(`IQfcDatamodel.cs:26`), `DequeueNextItemGroupAsync(int,int,TimeSpan,Action<int,int,int>)`
(`IQfcDatamodel.cs:40`), `DequeueNextItemGroup(int)` (`IQfcDatamodel.cs:46`), and `UndoMove()`
(`IQfcDatamodel.cs:47`) — keep byte-identical signatures. Verified consumers that must not break:
`QfcHomeController.Iteration.cs:21,63,66`, `QfcHomeController.cs:261,299`, `QfcQueue.cs:476` (F7 and
F2 territory), plus the Moq setups in `QfcHomeControllerIterationTests.cs`,
`QfcHomeControllerRunAsyncTests.cs`, `QfcHomeControllerRunAsyncHighConfidenceTests.cs`, and
`QfcHomeControllerIssue218Tests.cs`.

**No cross-child contract note for `spec.md` is required from this file.** No breaking change is
proposed.

### 6.3 `[ExcludeFromCodeCoverage]` disposition and file size

- **This file must receive no exemption of any kind — not type-level, not member-level.** After S1
  there is no irreducible remainder: no COM dereference, no WinForms type, no modal dialog, no UI
  thread dependency, no `Designer` code. Under the epic's ratified reconciliation (epic.md § Shared
  Design 1), an exemption here would be a **Blocking** finding.
- The type-scoped attribute at `QfcDatamodel.cs:25` must be removed; that task is owned by the
  `QfcDatamodel.cs` phase and, per the sibling artifact §6, must be sequenced **last**, after
  `FrameBuilding.cs` has either seamed or member-level-exempted its `DfDeedle`-bound members. This
  file's tests can be authored and will pass before that removal; they simply will not be *counted*
  until it lands.
- **File size:** 177 lines, unchanged (no production edit proposed). The partial family after the
  sibling's split is `QfcDatamodel.cs` ~311 + `QfcDatamodel.Construction.cs` ~168 +
  `QfcDatamodel.QueueProcessing.cs` 177 + `QfcDatamodel.FrameBuilding.cs` 154 — all four under 500.
  No split is needed here and none is proposed.

---

## 7. Enumerated test cases

Each numbered item is intended to become a single atomic plan task. All use MSTest
`[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, and Arrange–Act–Assert. None uses `Thread.Sleep`,
`Task.Delay`, a real wall-clock wait, a temporary file, an external service, a live form, a modal
dialog, or the UI thread. All timing is driven by `FakeTimeProvider`.

Shared arrangement helpers (`CreateUninitializedDatamodel`, `SetPrivateField`, and a new
`CreateModelWithFakeClock`) are duplicated per test file, following the convention documented at
`QfcDatamodelLivenessTests.cs:18–24`. `MailItem` instances are loose `new Mock<MailItem>().Object`
(convention: `QfcDatamodelTests.cs:54`). `IEmailMoveMonitor` and `IApplicationGlobals` are Moq mocks;
`_masterQueue` is a real `LockingLinkedList<MailItem>`.

**Dependency:** tests 18–26 require sibling seam **S1** to have landed. Tests 1–17 and 27–36 do not.

### T-file A — `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs` (new)

Entry points, mode routing, cancellation, and quantity boundaries. Estimated ~330 lines.

| # | Test method | Member | Invariant | Category | Arrange / Act / Assert sketch |
| --- | --- | --- | --- | --- | --- |
| 1 | `UndoMove_IsNotImplemented_Throws` | N2 | I— | error-handling | **A:** uninitialized model. **Act/Assert:** `model.Invoking(m => m.UndoMove()).Should().Throw<NotImplementedException>()`. Pins the declared-but-unimplemented `IQfcDatamodel` member (line 26). |
| 2 | `DequeueNextItemGroupAsync_WithCancelledToken_ThrowsBeforeTouchingTheQueue` | N5 | I2 | error-handling | **A:** `_token` from an already-cancelled `CancellationTokenSource`; `_masterQueue` seeded with two items; `_globals` **not** set. **Act/Assert:** `await act.Should().ThrowAsync<OperationCanceledException>()`; `masterQueue.Count.Should().Be(2)` — proving line 85 runs before line 87. |
| 3 | `DequeueNextItemGroup_WithCancelledToken_ThrowsBeforeTouchingTheQueue` | N8 | I2 | error-handling | Same shape for the synchronous entry point; pins line 134. |
| 4 | `DequeueNextItemGroupAsync_WithNullGlobals_UsesTheDirectPath` | N5, N6 | I3 | invalid-input | **A:** `_globals` left null; `_masterQueue` with two items; strict `IEmailMoveMonitor`. **Act:** `await model.DequeueNextItemGroupAsync(2, 0)`. **Assert:** both items returned in order; no throw — proving the `?.` at line 87 selects the direct path. |
| 5 | `DequeueNextItemGroupAsync_WithNullQfSettings_UsesTheDirectPath` | N5, N6 | I3 | invalid-input | **A:** `Mock<IApplicationGlobals>` whose `QfSettings` returns null. **Assert:** as #4 — proving the second `?.` at line 87. |
| 6 | `DequeueNextItemGroupAsync_NormalMode_ReturnsQueueHeadInFifoOrder` | N6 | I4, I15 | positive/ordering | **A:** three items `i1,i2,i3` added with `AddLast`; high-confidence disabled; `IEmailMoveMonitor` strict, expecting `UnhookItem(i1)` and `UnhookItem(i2)`. **Act:** `DequeueNextItemGroupAsync(2, 0)`. **Assert:** `result.Should().Equal(i1, i2)`; `masterQueue.Count == 1` and its head is `i3`; both unhooks verified once. Distinct from `QfcQueuePurePathsTests.cs:105`, which drains the whole queue and cannot show that the *prefix* is taken in order. |
| 7 | `DequeueNextItemGroupAsync_NormalModeZeroQuantity_ReturnsNull` | N6, N9 | I5 | boundary | **A:** two items queued; producer idle. **Act:** `DequeueNextItemGroupAsync(0, 0)`. **Assert:** `result.Should().BeNull()`; queue still holds two items. Documents current behavior (`LockingLinkedList.cs:403–406` → line 149); **does not fix it** — see §9 R2. |
| 8 | `DequeueNextItemGroupAsync_NormalModeNegativeQuantity_ReturnsNull` | N6, N9 | I5 | invalid-input | As #7 with `-1`. |
| 9 | `DequeueNextItemGroupAsync_HighConfidenceZeroQuantity_ReturnsEmptyListNotNull` | N7 | I5 | boundary | **A:** high-confidence globals; `FakeTimeProvider` assigned; empty queue. **Act:** `DequeueNextItemGroupAsync(0, 0)`. **Assert:** `result.Should().NotBeNull().And.BeEmpty()`. Paired with #7, this pins the asymmetry explicitly. |
| 10 | `DequeueNextItemGroupAsync_NormalMode_NeverScoresCandidates` | N5, N6 | I18 | negative | **A:** high-confidence **disabled**; S1 `ScoringService` mock configured to throw `AssertFailedException` if called (mirrors the idiom at `QfcDatamodelTests.cs:61`). **Assert:** the batch returns normally and the scorer is never invoked. Requires S1 only for the assertion mechanism, so schedule with tests 18+ if S1 sequencing demands it. |
| 11 | `DequeueNextItemGroupAsync_NormalMode_IgnoresProgressSink` | N5, N6 | I18 | negative | **A:** high-confidence disabled; progress delegate that throws when invoked. **Act:** the four-argument overload with `Timeout.InfiniteTimeSpan` and that sink. **Assert:** no throw; sink never invoked — pins that lines 97–98 drop the argument. |
| 12 | `DequeueNextItemGroup_NormalMode_TakesWithoutWaitingAndUnhooks` | N8 | I15, I19 | positive | **A:** two items; high-confidence disabled; strict move monitor. **Act:** `model.DequeueNextItemGroup(2)`. **Assert:** both returned in order, both unhooked, queue empty. Covers lines 141–142, wholly uncovered today. Note `TimeProvider` is not required: the synchronous direct path never waits. |
| 13 | `DequeueNextItemGroup_NormalModeZeroQuantity_ReturnsNull` | N8, N9 | I5 | boundary | Synchronous twin of #7; covers the `UnhookDequeuedNodes(null)` return through line 142. |
| 14 | `DequeueNextItemGroupAsync_QueueShorterThanQuantityAndProducerIdle_ReturnsWhatIsAvailable` | N6, N10 | I12 | boundary | **A:** one item queued; `_remainingLoadActive = false`; `FakeTimeProvider` assigned but never advanced. **Act:** `DequeueNextItemGroupAsync(2, 0)`. **Assert:** exactly one item returned; the fake clock was never advanced and the task completed. Covers line 104 — the only line of N6 not reached today — and proves `WaitForQueue` short-circuits. |

### T-file B — `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs` (new)

`TryUnhookOrReplace` and `UnhookDequeuedNodes`. `TryUnhookOrReplace` is called **directly** (it is
`internal`, and `AssemblyInfo.cs:5` grants `InternalsVisibleTo`). Estimated ~300 lines.

| # | Test method | Member | Invariant | Category | Sketch |
| --- | --- | --- | --- | --- | --- |
| 15 | `TryUnhookOrReplace_NullNodeList_LogsAndReturnsWithoutThrowing` | N3 | I13 | invalid-input | **A:** `List<MailItem> nodes = null`; move monitor strict with no setups. **Act:** `model.TryUnhookOrReplace(ref nodes, 0)`. **Assert:** no throw; `nodes` still null; `UnhookItem` never called. Covers lines 31–36 (`nodes is null` disjunct). |
| 16 | `TryUnhookOrReplace_EmptyNodeList_ReturnsWithoutThrowing` | N3 | I13 | boundary | Empty list, index 0. **Assert:** no throw; no unhook. Covers the `nodes.Count == 0` disjunct. |
| 17 | `TryUnhookOrReplace_IndexBeyondListLength_ReturnsWithoutThrowing` | N3 | I13 | boundary | One-element list, index 1. **Assert:** no throw; list unchanged; no unhook. Covers the `nodes.Count < i + 1` disjunct — the guard that absorbs the batch shrink of test 20. |
| 18 | `TryUnhookOrReplace_WhenUnhookFails_ReplacesFailedNodeInPlaceFromQueueHead` | N3 | I14 | state-transition/ordering | **A:** `nodes = [bad, tail]`; `_masterQueue` = `[repl]`; move monitor throws `COMException` for `bad`, succeeds for `repl`. **Act:** `TryUnhookOrReplace(ref nodes, 0)`. **Assert:** `nodes.Should().Equal(repl, tail)` — the replacement is at **index 0**, not appended; `masterQueue` empty; `UnhookItem(bad)` once and `UnhookItem(repl)` once. Covers lines 47–61. |
| 19 | `TryUnhookOrReplace_WhenReplacementsAlsoFail_RetriesUntilTheQueueIsExhausted` | N3 | I14 | error-handling/loop-termination | **A:** `nodes = [bad]`; `_masterQueue` = `[r1, r2]`; move monitor throws for all three. **Assert:** `UnhookItem` called exactly three times; `masterQueue` empty; `nodes.Should().BeEmpty()` (the last removal at line 52 is followed by a null `TryTakeFirst`, setting `processing = false` at line 56 with no re-insert). Pins loop termination. |
| 20 | `UnhookDequeuedNodes_WhenTheBatchShrinks_StopsAtTheGuardAndReturnsTheSurvivor` | N9 + N3 | I13 | state-transition/ordering | **A:** high-confidence disabled; `_masterQueue = [n1, n2]`; move monitor throws for `n1`, is never set up for `n2`. **Act:** `await DequeueNextItemGroupAsync(2, 0)`. **Assert:** `result.Should().Equal(n2)`; `UnhookItem(n2)` **never** called. This is the shrink path: `max` was captured as 2 (line 154), `TryUnhookOrReplace(0)` drops `n1` and finds no replacement, then index 1 hits the guard at line 31. Pins current behavior and documents §9 R3. |
| 21 | `UnhookDequeuedNodes_NullBatch_ReturnsNull` | N9 | I5 | boundary | Covered transitively by test 7; add a direct reflection-invoked assertion only if F1's harness shows lines 147–149 still uncovered. Marked **conditional** to avoid duplication. |
| 22 | `UnhookDequeuedNodes_WhenUnhookingItselfThrows_LogsAndRethrows` | N9 | I16 | error-handling | **A:** `_masterQueue = null`; move monitor throws for the single node; invoke the private `UnhookDequeuedNodes` by reflection with `new List<MailItem> { n1 }`. **Act/Assert:** the reflected call raises `TargetInvocationException` whose `InnerException` is `NullReferenceException` (raised at line 53 inside `TryUnhookOrReplace`'s catch and rethrown at line 163). Covers lines 160–164. The indirect trigger is documented in §5 and in the test's XML doc. |

### T-file C — `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs` (new)

Real-gate behavior as observed through this file. **Requires S1.** All tests assign
`model.TimeProvider = fake`. Estimated ~380 lines; split into a `.Part2.cs` if it exceeds 500, as
`QfcStreamingDequeueConfidenceGateTests` already does. A local helper configures
`Mock<IFolderScoringService>.ScoreAsync` to return a per-item score from a dictionary.

| # | Test method | Member | Invariant | Category | Sketch |
| --- | --- | --- | --- | --- | --- |
| 23 | `DequeueNextItemGroupAsync_HighConfidence_ReturnsOnlyAboveThresholdItemsInQueueOrder` | N7 | I6 | positive/ordering | **A:** threshold `0.90` (cutoff 900, gate line 68); queue `[i1, i2, i3]` scoring 950 / 100 / 990; `timeOut: 0`; move monitor strict expecting `UnhookItem(i1)`, `UnhookItem(i3)`. **Act:** `DequeueNextItemGroupAsync(3, 0)`. **Assert:** `result.Should().Equal(i1, i3)`. |
| 24 | `DequeueNextItemGroupAsync_HighConfidence_DiscardsRejectedCandidatesFromTheMasterQueue` | N7 | I6 | state-transition | Same arrangement as #23. **Assert:** `masterQueue.Should().BeEmpty()` — `i2` is neither returned nor requeued; and `moveMonitor.Verify(m => m.UnhookItem(i2), Times.Never)` — it stays hooked. Pins the discard-and-leak behavior explicitly so a later change cannot alter it silently. §9 R3. |
| 25 | `DequeueNextItemGroupAsync_HighConfidence_StopsAtTheRequestedQuantity` | N7 | I6 | boundary | **A:** four items all scoring above cutoff; quantity 2. **Assert:** `result.Should().Equal(i1, i2)`; `masterQueue` still holds `i3, i4`; the scorer was invoked exactly twice — proving the gate does not over-scan. |
| 26 | `DequeueNextItemGroupAsync_HighConfidence_EmptyQueueWithIdleProducer_ReturnsEmpty` | N7 | I9 | boundary | **A:** empty queue; `_remainingLoadActive = false`; `timeOut: 0`. **Assert:** empty (not null) result, no clock advance required. Complements the two existing polling tests, which only cover the *active* producer. |
| 27 | `DequeueNextItemGroupAsync_TwoArgumentOverload_AppliesTheTwelveSecondDefaultDeadline` | N4, N7 | I1, I7 | ordering/timeout | **A:** 20 items all scoring **below** cutoff so `accepted` never fills; quantity 5; `timeOut: 0`; the `IFolderScoringService` mock callback calls `fake.Advance(TimeSpan.FromSeconds(1))` on each invocation. **Act:** `await model.DequeueNextItemGroupAsync(5, 0)`. **Assert:** result empty; the scorer was invoked **exactly 12 times** (gate line 110 checks `elapsed >= 12 s` at the top of iteration 13, elapsed 0…11 on iterations 1…12). Deterministic: the clock advances only from inside the scorer. This is the only way to pin line 73's constant without a wall-clock wait. |
| 28 | `DequeueNextItemGroupAsync_FourArgumentOverload_HonoursAnExplicitDeadline` | N5, N7 | I7 | boundary | As #27 but the four-argument overload with `TimeSpan.FromSeconds(3)`. **Assert:** scorer invoked exactly 3 times; result empty. Proves the parameter is threaded, not ignored. |
| 29 | `DequeueNextItemGroupAsync_WithInfiniteDeadline_ScansTheWholeQueue` | N5, N7 | I7 | boundary | As #27 with `Timeout.InfiniteTimeSpan` and 20 below-cutoff items. **Assert:** scorer invoked exactly 20 times; result empty; queue empty. Covers the deadline-disabled arm (gate line 101) — the pre-#424 behavior. |
| 30 | `DequeueNextItemGroupAsync_ReportsProgressPerScoredCandidateAfterTheAcceptDecision` | N5, N7 | I18 | ordering | **A:** three items scoring 950 / 100 / 990; quantity 5; `timeOut: 0`; a recording `Action<int,int,int>`. **Assert:** the recorded triples are exactly `(1,1,5), (2,1,5), (3,2,5)` — proving the sink is invoked once per scored candidate, in scan order, *after* the accept decision (gate lines 143–145). |
| 31 | `DequeueNextItemGroupAsync_WhenTheProgressSinkThrows_TheExceptionPropagates` | N5, N7 | I18 | error-handling | **A:** one above-cutoff item; a sink that throws `InvalidOperationException`. **Assert:** `await act.Should().ThrowAsync<InvalidOperationException>()` — pins the documented fail-fast contract (`IQfcDatamodel.cs:36–39`, gate line 143). |
| 32 | `DequeueNextItemGroupAsync_WhenCancelledMidScan_ThrowsAndStopsScanning` | N5, N7 | I2 | error-handling/concurrency | **A:** three items; the scorer callback cancels the CTS backing `_token` on its **first** invocation. **Assert:** `await act.Should().ThrowAsync<OperationCanceledException>()`; the scorer was invoked exactly once (gate line 134 checks immediately after the score). Deterministic — no timing dependency. |
| 33 | `DequeueNextItemGroup_HighConfidence_BlocksOnTheGateAndReturnsTheFilteredBatch` | N8, N7 | I7, I8 | positive | **A:** items scoring 950 / 100; high-confidence enabled; `FakeTimeProvider` assigned. **Act:** `model.DequeueNextItemGroup(2)` (synchronous). **Assert:** returns `[i1]`; `masterQueue` empty. Covers lines 136 and 138, wholly uncovered today, and pins that the synchronous path uses `timeOut: 0` (it returns without any clock advance). |

### T-file D — `QuickFiler.Test/Controllers/QfcDatamodelWaitForQueueTests.cs` (new)

`WaitForQueue` called directly (`internal`). Estimated ~200 lines.

| # | Test method | Member | Invariant | Category | Sketch |
| --- | --- | --- | --- | --- | --- |
| 34 | `WaitForQueue_WhenProducerIsIdle_ReturnsWithoutDelaying` | N10 | I11 | boundary | **A:** `_remainingLoadActive = false`; empty queue; `FakeTimeProvider` assigned but never advanced. **Act:** `await model.WaitForQueue(5, CancellationToken.None)`. **Assert:** the task is already completed. Pins the first disjunct's short-circuit. |
| 35 | `WaitForQueue_WhenQueueAlreadyHoldsQuantity_ReturnsWithoutDelaying` | N10 | I11 | boundary | **A:** `_remainingLoadActive = true`; queue with two items; quantity 2. **Assert:** completed with no clock advance. Pins the second disjunct — currently untested. |
| 36 | `WaitForQueue_WhenTheQueueFillsWhileWaiting_ExitsOnTheNextPoll` | N10 | I11 | state-transition | **A:** producer active; empty queue; quantity 1. **Act:** start the task, assert pending, then `masterQueue.AddLast(item)` and `fake.Advance(200 ms)`. **Assert:** the task completes and the item is still in the queue (`WaitForQueue` does not consume). Complements the existing test, which exits via the *producer-idle* arm only. |
| 37 | `WaitForQueue_DelayIsExactlyTwoHundredMilliseconds` | N10 | I10 | boundary | **A:** producer active; empty queue; quantity 1. **Act:** advance 199 ms → assert still pending; make the exit condition true and advance 1 ms → assert completed. Pins the interval magnitude, which no existing test does. |
| 38 | `WaitForQueue_WhenCancelledWhileWaiting_ThrowsOperationCanceled` | N10 | I2 | error-handling | **A:** producer active; empty queue; a real `CancellationTokenSource`. **Act:** start, then `cts.Cancel()`, then `fake.Advance(200 ms)`. **Assert:** `await act.Should().ThrowAsync<OperationCanceledException>()` (satisfied by either the `Delay` cancellation or line 172 on the next iteration). |
| 39 | `WaitForQueue_WithNullMasterQueue_ReturnsWithoutDelaying` | N10 | I11 | invalid-input | **A:** `_masterQueue` left null; `_remainingLoadActive = true`. **Assert:** completed immediately — `null < quantity` on `int?` is false. Documents the `?.` at line 170 and its inconsistency with the unguarded `_masterQueue.Count` at line 103. |

### 7.1 Scenario-completeness check

Against `.claude/rules/general-unit-test.md` § Scenario Completeness:

- **Positive:** 6, 12, 23, 25, 30, 33, 34, 35
- **Invalid input:** 4, 5, 8, 15, 39
- **Boundary:** 7, 9, 13, 14, 16, 17, 25, 26, 28, 29, 35, 37
- **Error handling:** 1, 2, 3, 19, 22, 31, 32, 38
- **State transitions:** 18, 20, 24, 36
- **Concurrency / ordering:** 6, 18, 20, 23, 27, 28, 29, 30, 32
- **Negative (must-not-happen):** 10, 11, 24

Every invariant I1–I19 is covered by at least one test except **I8** (sync-over-async deadlock
safety), which is not unit-testable without a hanging test and is recorded as risk R5 instead.

### 7.2 Expected coverage outcome

The 39 tests reach every executable line of the file except none identified — the file has **no
irreducible remainder**. Read-derived projection: **100% line coverage attainable**, comfortably
above the 80% floor and above the >= 90% new-code target for any lines the plan touches.
**Confidence: medium-high** for reachability (line-by-line reasoning above), **low** for the exact
percentage until F1's harness reports it.

---

## 8. Files this phase would touch

| Path | Action |
| --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | **No change.** No seam, no split, no attribute. |
| `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs` | **New.** Tests 1–14 |
| `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs` | **New.** Tests 15–22 |
| `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs` | **New.** Tests 23–33 |
| `QuickFiler.Test/Controllers/QfcDatamodelWaitForQueueTests.cs` | **New.** Tests 34–39 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `<Compile Include>` entries for the four new test files (legacy non-SDK project; explicit items are required — precedent recorded at `.../424/evidence/qa-gates/file-line-counts.2026-08-07T00-22.md:51`) |

Explicitly **not** touched: `coverage.config`, any shared build property file, `QfcQueue.cs`,
`QfcStreamingDequeueConfidenceGate.cs`, `QfcHighConfidencePreFilter.cs`,
`QfcRemainingQueueAdmission.cs` (all F2), `QfcHomeController*.cs` (F7),
`QfcCollectionController.cs` (F11), `IQfcDatamodel.cs`, `QfcDatamodel.cs`,
`QfcDatamodel.FrameBuilding.cs`, `EfcDataModel.cs`, `LockingLinkedList.cs`, or any existing test file.

---

## 9. Risks and open questions

| ID | Item | Impact | Handling |
| --- | --- | --- | --- |
| **R1** | **Intra-feature sequencing on S1.** Tests 10 and 18–33 require `IFolderScoringService ScoringService` on `QfcDatamodel`, which the sibling `QfcDatamodel.cs` phase owns. | 12 of 39 tests are blocked until that phase's seam task lands. | Order the plan so the `QfcDatamodel.cs` seam phase precedes this file's T-file C phase. If the plan requires phase independence, fall back to Q2 (§6.1) declared in `QfcDatamodel.Construction.cs` — a deliberate, documented downgrade from an interface seam to a delegate seam. |
| **R2** | **Latent defect — `quantity <= 0` returns `null` in normal mode** (I5). `QfcHomeController.Iteration.cs:25` dereferences `listObjects.Count` with no null guard, so a zero or negative `ItemsPerIteration` NREs inside `IterateQueueAsync`. Verified by reading `LockingLinkedList.cs:403–406`, `QueueProcessing.cs:147–150`, and `Iteration.cs:21–25`. | Real crash path, but not caused by this feature and not reachable with the current `ItemsPerIteration` settings (**INFERRED** — settings validation was not read). | **Do not fix.** AC7 forbids behavior change, and the fix would sit in F7's file. Tests 7, 8, 9, 13 pin the current shape. Promote to a separate GitHub issue through the MCP promotion lifecycle and record it in `spec.md` as a cross-child observation for F7. |
| **R3** | **Latent defect — items can leave the master queue still hooked** (I6, I13). Below-threshold candidates discarded by the gate are never unhooked, and a batch node surviving a shrink is returned to the caller unhooked. Verified against gate lines 116/138–141 and `QueueProcessing.cs:31,52,154`. | The stale `BeforeItemMove` callback runs `_masterQueue.Remove(x)` (`QfcDatamodel.cs:358`) against a queue the item has left — a no-op today, but it holds a COM reference for the process lifetime. | **Do not fix.** Tests 20 and 24 pin current behavior. Promote to a separate issue; record in `spec.md`. |
| **R4** | **F1 ledger disagreement.** This artifact asserts `QfcDatamodel.QueueProcessing.cs` = `testable`, zero exempt members. F1's ledger does not exist on disk yet — expected, since F1 is prepared concurrently. | If F1 ratifies any exemption touching this file, §6.3 and issue.md AC1/AC2 change. | Treat the ledger at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` as authoritative on arrival and re-read §6.3 at plan time. |
| **R5** | **I8 is asserted only by reading.** The sync-over-async bridge at line 138 is deadlock-safe today because the awaited chain uses `ConfigureAwait(false)` throughout and MSTest installs no `SynchronizationContext`. A deterministic unit test would have to install a single-threaded context and then block the only pumping thread — i.e. hang on failure. | An unguarded regression in `ConfigureAwait` discipline would not be caught by any test in §7. | Do not attempt a deadlock test. Record the constraint as a review rule in `spec.md`: any change to `ScoreRemainingQueueMailItemAsync` or to the gate's await chain must preserve `ConfigureAwait(false)`. |
| **R6** | **`FakeTimeProvider` omission is silent** in high-confidence tests (§4.3): the gate falls back to `TimeProvider.System` (gate line 69) rather than failing. | A forgotten assignment turns tests 27–29 into 12-second wall-clock tests, or makes them flaky. | Mandate a single `CreateModelWithFakeClock(out FakeTimeProvider)` helper per test file; state this in each atomic task for T-files C and D. |
| **R7** | **`Mock<MailItem>` strictness.** The gate logs `mailItem.Subject` / `mailItem.EntryID` (gate line 165) on every scored candidate. | A `MockBehavior.Strict` `MailItem` throws inside the gate, producing a confusing failure. | Use loose mocks (repo convention, `QfcDatamodelTests.cs:54`); note it in each T-file C task. |
| **R8** | **F2 owns the gate.** Tests 23–33 assert behavior that is jointly produced by this file and `QfcStreamingDequeueConfidenceGate.cs`. If F2 changes the gate's scan or deadline semantics on the integration branch, these tests fail. | Cross-child test coupling at fan-in. | These are the correct assertions for *this* file's contract (it constructs the gate and owns the argument marshalling). Record in `spec.md` that T-file C is sensitive to F2 gate changes so the epic's integration rebase treats a failure there as a coordination signal, not a defect in F5. |
| **Q1** | `TryUnhookOrReplace` takes `ref List<MailItem> nodes` but never reassigns the parameter (lines 29–63) — the `ref` is vestigial. | Cosmetic; removing it would change an `internal` signature. | **Leave as-is.** Removing it is a non-additive change with no coverage benefit. Tests 15–19 pass `ref`. Note as a cleanup candidate in `spec.md`. |
| **Q2** | Should tests call `WaitForQueue` / `TryUnhookOrReplace` directly, or by reflection as `QfcDatamodelTests.cs:298` does? | Readability and refactor-safety. | **Call directly.** `InternalsVisibleTo("QuickFiler.Test")` is present (`AssemblyInfo.cs:5`) and direct calls are compile-time-checked. Reserve reflection for the genuinely private `UnhookDequeuedNodes` (test 22). Do not retrofit the existing reflection-based test — that would churn a file this phase does not own. |
| **Q3** | Should test 21 (`UnhookDequeuedNodes_NullBatch`) be authored, given tests 7 and 13 already reach lines 147–149? | One redundant test. | Marked **conditional** in §7: author it only if F1's harness reports those lines uncovered. |
