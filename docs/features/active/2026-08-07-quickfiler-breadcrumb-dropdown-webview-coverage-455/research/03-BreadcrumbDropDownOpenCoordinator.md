# Research: `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`

- Feature: #455 (`quickfiler-breadcrumb-dropdown-webview-coverage`), epic child F13 of epic #136
- Production file: `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` — **309 lines**
- Researched: 2026-08-07
- Complexity band: C3

---

## 0. Measured baseline and deviation notices

### 0.1 Measured baseline (indicative, from committed Cobertura)

Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`, line 10608:

```
<class line-rate="0.982544" branch-rate="0.920455" complexity="98"
       name="QuickFiler.Viewers.BreadcrumbDropDownOpenCoordinator"
       filename="QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs">
```

| Metric | Measured | Gate | Status |
| --- | --- | --- | --- |
| Line coverage | **98.25%** (225/229 coverable) | >= 80% (issue #136 AC1) | PASS |
| Branch coverage | **92.05%** | >= 75% (`.claude/rules/general-unit-test.md`) | PASS |

Exactly **four** lines are uncovered: `:93`, `:107`, `:187`, `:242`.

Line-number alignment verified against the report's per-method breakdown, which names methods
explicitly: `get_CurrentOpenTask` at `:63-66`, `RequestOpen` at `:85-98`, `BeginOpenCore` at
`:181-199`, `FinishOpenCore` at `:202-216`, `CloseCore` at `:238-267`, `IsCurrentCore` at `:295`.
All match the current worktree file exactly.

### 0.2 DEVIATION — the branch-coverage premise in the delegation brief is disproved

Measured branch coverage is **92.05%**, 17 points above the 75% floor. This is the highest of the
three files. The brief's expectation of an unmet branch gate does not hold here either.

### 0.3 Note on the report's per-method breakdown

Unlike the two sibling files, this class's Cobertura entry carries a full `<methods>` breakdown with
per-method rates. Three methods sit below 100%:

| Method | Lines | Line rate | Branch rate |
| --- | --- | --- | --- |
| `RequestOpen` | `:85-98` | 92.3% | 90% |
| `BeginOpenCore` | `:181-199` | 93.8% | 50% |
| `CloseCore` | `:238-267` | 96.2% | 90% |
| `get_CurrentOpenTask` | `:63-66` | 100% | 50% |

`OpenCoreAsync` (`:161-178`) and `RollbackAsync` (`:218-235`) have no `<method>` entry because they
compile to async state machines; their lines appear only in the aggregated `<lines>` block. **A
per-file harness that reads `<methods>` will under-count this file.** This is a second concrete
argument for the epic's "aggregate per file, not per class/method" directive (epic.md:530).

---

## 1. Structural map

`internal sealed class BreadcrumbDropDownOpenCoordinator` (`:12`). Single type. Not `IDisposable` —
its terminal operation is `Release()` (`:150`), not `Dispose()`.

### 1.1 State

| Field | Line | Mutability | Guarded by |
| --- | --- | --- | --- |
| `static readonly Task<bool> ClosedTask = Task.FromResult(false)` | `:14` | immutable | n/a |
| `readonly object _sync` | `:16` | — | the lock itself |
| `readonly BreadcrumbPopupUiOperations _operations` | `:17` | immutable | n/a |
| `readonly IBreadcrumbDropDownHost _host` | `:18` | immutable | n/a |
| `readonly Func<int> _rowCount` | `:19` | immutable | n/a |
| `readonly Func<bool> _isSelectorOpen` | `:20` | immutable | n/a |
| `readonly Func<bool> _openSelector` | `:21` | immutable | n/a |
| `readonly Action _cancelSelector` | `:22` | immutable | n/a |
| `readonly Action _detachPopupMessenger` | `:23` | immutable | n/a |
| `Func<Rectangle> _anchorBounds` | `:24` | mutable | `_sync` (`:76-81`, `:184-190`) |
| `Func<Rectangle> _workingArea` | `:25` | mutable | `_sync` (same) |
| `Task<bool>? _currentOpenTask` | `:26` | mutable | `_sync` (`:64`, `:90`, `:95`, `:282`) |
| `int _generation` | `:27` | mutable | `_sync` (`:259-260`, `:281`, `:295`) |
| `bool _closePending` | `:28` | mutable | `_sync` (`:92`, `:94`, `:241`-`:245`, `:272`, `:283`) |
| `bool _released` | `:29` | mutable | `_sync` (`:88`, `:241`, `:279`, `:284`, `:295`, `:300`, `:305`) |

### 1.2 Members

| Member | Lines | Access |
| --- | --- | --- |
| ctor (9 parameters, all delegate/interface seams) | `:31-56` | `internal` |
| `Host` | `:58` | `internal` |
| `CurrentOpenTask` | `:60-67` | `internal` |
| `UpdateRequestProviders(Func<Rectangle>, Func<Rectangle>)` | `:69-82` | `internal` |
| `RequestOpen()` | `:84-98` | `internal` |
| `SetDroppedDown(bool)` | `:100-117` | `internal` |
| `HandleSelectorOpenStateChanged()` | `:119-132` | `internal` |
| `Reset()` | `:134-148` | `internal` |
| `Release()` | `:150-159` | `internal` |
| `OpenCoreAsync(int)` | `:161-178` | private `async` |
| `BeginOpenCore(int)` | `:180-199` | private |
| `FinishOpenCore(int, bool)` | `:201-216` | private |
| `RollbackAsync(int)` | `:218-235` | private `async` |
| `CloseCore(BreadcrumbDropDownCloseReason)` | `:237-267` | private |
| `ClearClosePending()` | `:269-273` | private |
| `Invalidate(bool release)` | `:275-287` | private |
| `IsCurrent(int)` | `:289-293` | private |
| `IsCurrentCore(int)` | `:295` | private |
| `IsReleased()` | `:297-301` | private |
| `ThrowIfReleased()` | `:303-307` | private |

### 1.3 Seams

**This is the most seam-friendly of the three files.** Every dependency is an interface or a
delegate injected through the single constructor:

- `BreadcrumbPopupUiOperations _operations` — adapter; its `BreadcrumbUiDispatcher` takes an
  arbitrary `SynchronizationContext` plus an error sink (`BreadcrumbUiDispatcher.cs:25`).
- `IBreadcrumbDropDownHost _host` — **interface seam** (`Viewers/IBreadcrumbDropDownHost.cs:19`).
  Tests substitute a hand-written fake (`ControlledHost`,
  `BreadcrumbDropDownOpenCoordinatorTests.cs:374-445`) with queued open results, settable
  `CloseResult`/`CloseFailure`, and call counters. No WinForms, no WebView2, no COM.
- Six delegates: `_anchorBounds`, `_workingArea`, `_rowCount`, `_isSelectorOpen`, `_openSelector`,
  `_cancelSelector`, `_detachPopupMessenger`.

**No seam addition is required for any recommended test in §8.**

---

## 2. Branch inventory — the core deliverable

| # | Line | Construct | Sides | Gap | Covering test |
| --- | --- | --- | --- | --- | --- |
| B1 | `:43` | `operations ?? throw ArgumentNullException` | 2/2 | none | `ConstructorAndProviderUpdates_GuardEveryRequiredDelegate` (`BreadcrumbDropDownOpenCoordinatorTests.cs:21`) |
| B2 | `:44` | `host ?? throw` | 2/2 | none | same |
| B3 | `:45` | `anchorBounds ?? throw` | 2/2 | none | same |
| B4 | `:46` | `workingArea ?? throw` | 2/2 | none | same |
| B5 | `:47` | `rowCount ?? throw` | 2/2 | none | same |
| B6 | `:48` | `isSelectorOpen ?? throw` | 2/2 | none | same |
| B7 | `:50` | `openSelector ?? throw` | 2/2 | none | same |
| B8 | `:51` | `cancelSelector ?? throw` | 2/2 | none | same |
| B9 | `:53` | `detachPopupMessenger ?? throw` | 2/2 | none | same |
| B10 | `:65` | `_currentOpenTask ?? ClosedTask` | **1/2** | **the `ClosedTask` fallback (no open in flight) is uncovered** | — |
| B11 | `:74` | `anchorBounds ?? throw` in `UpdateRequestProviders` | 2/2 | none | `ConstructorAndProviderUpdates_*` (`:149`) |
| B12 | `:75` | `workingArea ?? throw` | 2/2 | none | same (`:152`) |
| B13 | `:88` | `if (_released) return ClosedTask;` in `RequestOpen` | 2/2 | none | `ResetReleaseAndCloseResults_PreserveRetryAndBlockReleasedWork` (`Part2.cs:143`, line `:176`) |
| B14 | `:90` | `if (_currentOpenTask != null && !_currentOpenTask.IsCompleted)` | 4/4 | none | `RequestOpen_ConcurrentCallersShareOneUiBoundSnapshot` (`:164`) |
| B15 | `:92` | `if (_closePending && _host.IsOpen)` | **3/4** | **`_host.IsOpen == true` while `_closePending` is uncovered; `:93` `return ClosedTask;` never executes** | `_closePending == false` side covered broadly |
| B16 | `:102` | `if (IsReleased()) return;` — outer guard of `SetDroppedDown` | 2/2 | none | `SetDroppedDown_AfterRelease_PostsNothingAndLeavesHostStateUntouched` (`Part2.cs:192`) |
| B17 | `:106` | `if (IsReleased()) return;` — **inside the posted lambda** | **1/2** | **released-at-drain-time side uncovered; `:107` never executes** | — |
| B18 | `:108` | `if (droppedDown)` | 2/2 | none | `SetDroppedDown_MouseAndKeyboardPathsShareRequestAndCloseUncommitted` (`Part2.cs:95`) |
| B19 | `:111` | `if (!changed && _isSelectorOpen())` | 4/4 | none | same |
| B20 | `:121` | `if (IsReleased()) return;` — outer guard of `HandleSelectorOpenStateChanged` | 2/2 | none | `HandleSelectorOpenStateChanged_AfterRelease_PostsNothingAndSkipsSelectorPredicate` (`Part2.cs:224`) |
| B21 | `:125` | `if (IsReleased()) return;` — inside the posted lambda | 2/2 | none | `HandleSelectorOpenStateChanged_QueuedBodyDrainedAfterRelease_PerformsNoWork` (`Part2.cs:249`) |
| B22 | `:127` | `if (_isSelectorOpen())` | 2/2 | none | `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` (`Part2.cs:121`) |
| B23 | `:136` | `if (!Invalidate(release: false)) return;` in `Reset` | 2/2 | none | `Reset_AfterRelease_PostsNothingAndNeverDetachesOrResetsHost` (`Part2.cs:279`) |
| B24 | `:140` | `(!_host.IsOpen \|\| !_host.Close(Uncommitted)) && _isSelectorOpen()` | 6/6 | none | `Reset_HostAlreadyClosedWithOpenSelector_CancelsExactlyOnce` (`Part2.cs:20`) + `ResetReleaseAndCloseResults_*` |
| B25 | `:152` | `if (!Invalidate(release: true)) return;` in `Release` | 2/2 | none | `ResetReleaseAndCloseResults_*` (double `Release()`) |
| B26 | `:186` | `if (!IsCurrentCore(generation))` in `BeginOpenCore` | **1/2** | **stale-generation side uncovered; `:187` `return ClosedTask;` never executes** | — |
| B27 | `:195` | `_host.OpenAsync(...) ?? throw new InvalidOperationException("The breadcrumb popup host returned no open task.")` | **1/2** | **the null-task throw is uncovered** | — |
| B28 | `:204` | `if (!opened)` in `FinishOpenCore` | 2/2 | none | `RequestOpen_FalseResultCancelsOnceAndPermitsRetry` (`:222`) |
| B29 | `:206` | `if (current && _isSelectorOpen())` | 4/4 | none | `RequestOpen_HostSideCancellationBeforeFalseCompletionIsNotDuplicated` (`Part2.cs:60`) |
| B30 | `:210` | `if (!current \|\| !_isSelectorOpen())` | **3/4** | **`!current == true` (stale generation at finish) is uncovered** | `!_isSelectorOpen()` side covered by `RequestOpen_SelectorClosesBeforeSuccess_ClosesLatePopupExplicitly` (`Part2.cs:77`) |
| B31 | `:225` | `if (IsCurrent(generation) && _isSelectorOpen())` in `RollbackAsync` | **3/4** | **`IsCurrent == false` (stale generation at rollback) is uncovered** | `RequestOpen_SynchronousAndAsynchronousFaultsAreObserved` (`:241`) covers the current path |
| B32 | `:241` | `if (_released) return false;` in `CloseCore` | **1/2** | **released side uncovered; `:242` never executes** | — |
| B33 | `:243` | `if (_closePending) return true;` | 2/2 | none | `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` (`:263`) |
| B34 | `:257` | `if (closed)` | 2/2 | none | `PendingToggleClose_RejectedHostPerformsOneFallbackCancellation` (`:283`) |
| B35 | `:264` | `if (reason == Uncommitted && _isSelectorOpen())` | 4/4 | none | `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` (`:302`) + `ResetReleaseAndCloseResults_*` |
| B36 | `:279` | `if (_released) return false;` in `Invalidate` | 2/2 | none | `Reset_AfterRelease_*`, `ResetReleaseAndCloseResults_*` |
| B37 | `:295` | `!_released && generation == _generation` | 4/4 | none | broad |
| B38 | `:305` | `if (_released) throw new ObjectDisposedException` | 2/2 | none | `ResetReleaseAndCloseResults_*` (`Part2.cs:177-184`) |

Non-branching control flow carrying behavior:

| Line(s) | Construct | Currently asserted? |
| --- | --- | --- |
| `:163-177` | `try` / `catch { return await RollbackAsync(...) }` in `OpenCoreAsync` | Yes — `RequestOpen_SynchronousAndAsynchronousFaultsAreObserved` (`:241`) |
| `:169` | `opening.GetAwaiter().GetResult()` — synchronous unwrap after `ObserveReadinessAsync` | Yes (implicitly) |
| `:220-234` | `try` / `catch { return false; }` in `RollbackAsync` | Yes — `RequestOpen_RollbackOperationThrows_CompletesFalseWithoutSurfacingSecondary` (`Part2.cs:302`) |
| `:248-256` | `try { closed = _host.Close(reason); } catch { ClearClosePending(); throw; }` | Yes — `SetDroppedDown_CloseThrows_ReportsOnceAndAllowsRetry` (`Part2.cs:33`) |

**Uncovered lines:** `:93`, `:107`, `:187`, `:242`. No others.

### 2.1 Reachability triage of the eight uncovered outcomes

| Gap | Reachable? | How | Behavioral weight |
| --- | --- | --- | --- |
| B10 `:65` | **Yes, trivially** | Read `CurrentOpenTask` on a freshly constructed coordinator before any `RequestOpen`. Assert it is a completed `Task<bool>` with `Result == false`. | Low — but it pins the "no open in flight reads as closed" contract that `BreadcrumbItemViewerLifecycleCoordinator.cs:56` mirrors. |
| B15 `:92`/`:93` | **Yes** | `ControlledHost.SetOpen(true)` with `CloseResult = false`, so `CloseCore` sets `_closePending = true`, the host refuses to close, `ClearClosePending()` runs — **so this path needs the host to *accept* the close (leaving `_closePending` true) while still reporting `IsOpen == true`.** `ControlledHost.Close` sets `IsOpen = false` when `CloseResult` is true (`:420-421`), so the fake must be extended with a "claim without closing" mode, or a purpose-built fake used. | **High.** See D7 in §10 — this branch silently drops a reopen request. |
| B17 `:106`/`:107` | **Yes** | Exact mirror of the already-covered `HandleSelectorOpenStateChanged_QueuedBodyDrainedAfterRelease_PerformsNoWork` (`Part2.cs:249`): call `SetDroppedDown(true)` to queue the body, then `Release()`, then `DrainOne()`. The sibling test proves the technique works. | Medium — release-during-queued-toggle. |
| B26 `:186`/`:187` | **Yes** | `RequestOpen()` queues `OpenCoreAsync`; before draining, call `Reset()` (which advances `_generation` via `Invalidate`); then drain. `BeginOpenCore` observes a stale generation and returns `ClosedTask` without consulting `_anchorBounds`/`_rowCount` or calling `_host.OpenAsync`. | Medium — reset-during-open-request. |
| B27 `:195` | **Yes** | Extend/replace the fake host so one enqueued result yields `null` from `OpenAsync`. Assert the resulting task completes `false` and the `InvalidOperationException("The breadcrumb popup host returned no open task.")` reaches the error sink exactly once via `RollbackAsync`. | Medium — contract violation by a host implementation. |
| B30 `:210` `!current` | **Yes** | Open with a pending `TaskCompletionSource`; `DrainOne()` past `BeginOpenCore`; call `Reset()` to advance the generation; complete the pending task with `true`; drain. `FinishOpenCore` sees `current == false` and closes the late popup with `ExplicitCommit`. | **High** — this is the "open completes after the coordinator moved on" ordering rule. |
| B31 `:225` `!IsCurrent` | **Yes** | Same shape but with a *faulting* open: enqueue a throw, `DrainOne()`, `Reset()`, drain. `RollbackAsync` must not call `_cancelSelector` for a stale generation. | **High** — prevents a stale failure from cancelling a live selection. Directly analogous to `ResetLifecycle_LateFailureCannotOverwriteLaterSuccessfulLifecycle` in the lifetime file. |
| B32 `:241`/`:242` | **Yes** | `Release()` first, then invoke `CloseCore` indirectly. **But `SetDroppedDown`/`HandleSelectorOpenStateChanged` both return at their outer `IsReleased()` guards (`:102`, `:121`) before posting, and their inner guards (`:106`, `:125`) return before reaching `CloseCore`.** The only route is the *queued-body-drained-after-release* shape: `SetDroppedDown(false)` queues the body, then `Release()`, then drain — but that hits `:106`/`:107` (B17) and returns. Therefore `CloseCore`'s released guard is **unreachable through the public surface**; it is reachable only by calling the private `CloseCore` reflectively. | See below. |

**B32 determination:** `CloseCore` has exactly three call sites — `:115` (inside `SetDroppedDown`'s
posted lambda, after the `:106` guard), `:130` (inside `HandleSelectorOpenStateChanged`'s posted
lambda, after the `:125` guard), and `:212` (inside `FinishOpenCore`). All three are preceded by a
released check on the same `_sync`-guarded flag within the same synchronous execution, so `_released`
cannot flip to `true` in between on a single-threaded pump. `:241`/`:242` is **defensive
duplication** and should be recorded as an irreducible branch remainder rather than reached by
reflection. Reaching it reflectively would test the guard's own tautology, not a behaviour.

---

## 3. Concurrency and ordering invariants

### 3.1 State machine

Three orthogonal flags plus a generation counter:

| State | Representation |
| --- | --- |
| Idle | `_currentOpenTask == null \|\| IsCompleted`, `!_closePending`, `!_released` |
| Opening | `_currentOpenTask != null && !IsCompleted` |
| ClosePending | `_closePending == true` |
| Released | `_released == true` — **terminal**; `UpdateRequestProviders` throws `ObjectDisposedException` (`:305`), `RequestOpen` returns `ClosedTask` (`:88`), every other entry point becomes a no-op |

Legal transitions:

| From | Trigger | To | Code |
| --- | --- | --- | --- |
| Idle | `RequestOpen()` | Opening | `:95` |
| Opening | `RequestOpen()` again | Opening (same task returned) | `:90-91` |
| Opening | open completes `true` and generation current and selector open | Idle, popup open | `:215` |
| Opening | open completes `true` but generation stale or selector closed | Idle, popup closed `ExplicitCommit` | `:210-213` |
| Opening | open completes `false` | Idle, `_cancelSelector()` if current+open | `:204-208` |
| Opening | open throws (sync or async) | Idle via `RollbackAsync` | `:176` |
| any non-released | `CloseCore` with host accepting | ClosePending -> generation advanced | `:245`, `:259-261` |
| any non-released | `CloseCore` with host refusing | `_closePending` cleared, fallback `_cancelSelector` if `Uncommitted` | `:263-266` |
| any non-released | `Reset()` | generation advanced, `_currentOpenTask = null`, `_closePending = false` | `:281-283` |
| any non-released | `Release()` | Released (terminal) | `:284` |

Illegal / defended-against:

- **Double open** — `:90` returns the in-flight task. Covered
  (`RequestOpen_ConcurrentCallersShareOneUiBoundSnapshot`, `:164`, asserts
  `second.Should().BeSameAs(first)` and `Requests.Should().ContainSingle()`).
- **Open during close** — `:92` guards it; **uncovered** (B15). See D7.
- **Close during close** — `:243` returns `true` for the second caller. Covered
  (`PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`, `:263`, calls
  `SetDroppedDown(false)` twice and asserts a single `CloseReason`).
- **Release during a queued toggle** — `:106`; **uncovered** (B17). Mirror of the covered
  `:125` case.
- **Double release** — `:279` in `Invalidate`. Covered.
- **A stale open completing after `Reset`** — `:210`; **uncovered** (B30). This is the highest-value
  gap in the file.
- **A stale open *failing* after `Reset`** — `:225`; **uncovered** (B31).
- **Re-entrant `_openSelector`** — `SetDroppedDown`'s lambda calls `_openSelector()` (`:110`), which
  in production is `() => _bridgeCoordinator?.OpenSelector() == true`
  (`BreadcrumbItemViewerLifecycleCoordinator.cs:137`) and can synchronously raise
  `SelectorOpenStateChanged` -> `HandleSelectorOpenStateChanged` -> another post. Modelled by the
  test harness at `BreadcrumbDropDownOpenCoordinatorTests.cs:357-365` and covered by
  `SetDroppedDown_MouseAndKeyboardPathsShareRequestAndCloseUncommitted` (`Part2.cs:95`).

### 3.2 Primitives with file:line

| Primitive | Line(s) |
| --- | --- |
| `lock (_sync)` | `:64`, `:76`, `:87`, `:184`, `:239`, `:259`, `:271`, `:278`, `:291`, `:299` — ten acquisitions |
| `static readonly Task<bool> ClosedTask` | `:14` (shared completed task; safe — `Task` is immutable once completed) |
| `Task<bool>` | `:26`, `:60`, `:84`, `:161`, `:180`, `:218` |
| `async` methods | `:161` (`OpenCoreAsync`), `:218` (`RollbackAsync`) |
| `async void` | **none** |
| Fire-and-forget `_ = _operations.PostAsync(...)` | `:104`, `:123`, `:138`, `:154` — four sites |
| Fire-and-forget `_ = RequestOpen()` | `:113`, `:128` |
| `.ConfigureAwait(false)` | `:167`, `:168`, `:172`, `:176`, `:229` |
| `GetAwaiter().GetResult()` | `:169` — synchronous unwrap, safe only because `ObserveReadinessAsync` has already awaited `opening` |
| `CancellationToken` / `Interlocked` / `volatile` / `SemaphoreSlim` / timers / threads | **none** |

### 3.3 Lock discipline and one hazard

The discipline is generally correct: `_host.Close(reason)` is called **outside** `_sync`
(`:247-256`), `_cancelSelector()` is called outside `_sync` (`:265`), and `_generation++` on the
success path re-takes the lock (`:259-260`).

**One exception (see D8 in §10):** `RequestOpen` evaluates `_host.IsOpen` at `:92` **while holding
`_sync`**. For the production `BreadcrumbDropDownHost` this is a plain field read
(`BreadcrumbDropDownHost.cs:191` -> `:225`) and is harmless, but the parameter is typed
`IBreadcrumbDropDownHost` — an interface any implementation may satisfy. An implementation whose
`IsOpen` takes its own lock creates a lock-ordering pair with `CloseCore`'s outside-the-lock
`_host.Close`, i.e. a potential inversion.

### 3.4 Thread affinity

All four public mutating entry points (`SetDroppedDown`, `HandleSelectorOpenStateChanged`, `Reset`,
`Release`) marshal their bodies onto the owner boundary via `_operations.PostAsync` before touching
the host. `RequestOpen` and `UpdateRequestProviders` do not post — they are lock-protected and are
called from already-posted contexts (`BreadcrumbItemViewerLifecycleCoordinator.cs:120-152` posts
before calling `UpdateRequestProviders` at `:145`). `BeginOpenCore` and `FinishOpenCore` are only
ever invoked through `_operations.RunAsync` (`:166`, `:171`, `:223`), so they run on the boundary.

---

## 4. Time dependence

**No wall-clock read, no timer, no delay, no timeout anywhere in this file.** Verified across all
309 lines. The only numeric constants are the popup sizing bounds at `:194`
(`Math.Min(320, Math.Max(120, rows * 26))`), which are pixel dimensions, not durations.

**No clock seam is needed and none should be added.** The issue.md constraint about "an injected
clock and fake timers" (`issue.md:66`) does not apply to this file — report as a deviation.

Deterministic-test requirements:

1. **`CapturingSynchronizationContext`** (`BreadcrumbSelectorToggleUiBoundaryTests.cs:346`) — already
   the established vehicle for this file's tests (`BreadcrumbDropDownOpenCoordinatorTests.cs:12`
   aliases it). `DrainOne()` (`:404`) is the single-step primitive; `PendingCount` (`:358`) and
   `PostCount` (`:359`) let a test assert that a guard returned *before* posting, which is exactly
   how `SetDroppedDown_AfterRelease_PostsNothingAndLeavesHostStateUntouched` (`Part2.cs:192`)
   distinguishes `:102` from `:106`.
2. **`TaskCompletionSource<bool>` per open attempt** — `NewCompletion()`
   (`BreadcrumbDropDownOpenCoordinatorTests.cs:320`) creates them with
   `RunContinuationsAsynchronously`, which is required so the pump — not the completing thread —
   runs the continuation.
3. **A counting/faultable fake host** — `ControlledHost` (`:374`) and `CountingCoordinatorProbe`
   (`Part2.cs:330`) already exist.

---

## 5. Error paths

| # | Line | Construct | Kind | Reachable today? | Seam needed |
| --- | --- | --- | --- | --- | --- |
| E1 | `:43-55` | nine `?? throw ArgumentNullException` | guard | Yes | none |
| E2 | `:74-75` | two `?? throw` in `UpdateRequestProviders` | guard | Yes | none |
| E3 | `:78` | `ThrowIfReleased()` -> `ObjectDisposedException` (`:306`) | guard | Yes | none |
| E4 | `:88-89` | `if (_released) return ClosedTask;` | guard | Yes | none |
| E5 | `:92-93` | `if (_closePending && _host.IsOpen) return ClosedTask;` | **silent request drop** | **No** | none — needs a fake host that claims a close without clearing `IsOpen` |
| E6 | `:102`, `:106`, `:121`, `:125` | released guards, early return | guard | `:106` **No**; others Yes | none |
| E7 | `:174-177` | `catch { return await RollbackAsync(generation); }` — **catch-all, no filter** | rollback | Yes | none |
| E8 | `:186-187` | stale-generation early return | guard | **No** | none |
| E9 | `:195-198` | `?? throw new InvalidOperationException("The breadcrumb popup host returned no open task.")` | fail-fast | **No** | none — needs a fake returning `null` |
| E10 | `:230-234` | `catch { return false; }` — **catch-all secondary swallow** in `RollbackAsync` | swallow | Yes — `RequestOpen_RollbackOperationThrows_CompletesFalseWithoutSurfacingSecondary` (`Part2.cs:302`) | none |
| E11 | `:241-242` | `if (_released) return false;` in `CloseCore` | guard | **No — unreachable via the public surface** (§2.1) | n/a |
| E12 | `:252-256` | `catch { ClearClosePending(); throw; }` around `_host.Close` | rethrow with state restoration | Yes — `SetDroppedDown_CloseThrows_ReportsOnceAndAllowsRetry` (`Part2.cs:33`) | none |

Two catch-all handlers with no exception filter (`:174`, `:230`). Both are at genuine boundaries
(the whole open pipeline, and the rollback of a failed open) and both add context by routing the
exception to the dispatcher's error sink — `_operations.RunAsync` reports before faulting
(`BreadcrumbUiDispatcher.cs:217-218`). `RequestOpen_SynchronousAndAsynchronousFaultsAreObserved`
(`:241`) asserts `harness.Errors.Should().Equal(synchronous, asynchronous)`, so the reporting
contract is pinned, not merely the swallow.

**No new seam is required for any error path.** E5, E9 need a *richer fake*, not a production
change.

---

## 6. Coupling to sibling-owned files

| Referenced type | Line(s) | Owner | Blocking? |
| --- | --- | --- | --- |
| `BreadcrumbPopupUiOperations` | `:17`, `:43`, `:104`, `:123`, `:138`, `:154`, `:165`, `:168`, `:170`, `:222` | **F13** | no |
| `IBreadcrumbDropDownHost` | `:18`, `:33`, `:58` (+ `.IsOpen`, `.Close`, `.OpenAsync`, `.Reset`, `.Dispose` at `:92`, `:141`, `:146`, `:157`, `:195`, `:250`) | **F13** (`Viewers/IBreadcrumbDropDownHost.cs:19`) | no |
| `BreadcrumbDropDownCloseReason` | `:115`, `:130`, `:141`, `:212`, `:237`, `:264` | **F13** (same file) | no |
| `Rectangle`, `Size`, `Math`, `Task` | various | .NET | n/a |

**No reference to any F12-owned type; no reference to F14's `ItemViewer.Breadcrumb.cs`.**

The coupling runs inbound only, and it is a single edge:

- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:130-140` (**F12-owned**) is the
  sole production construction site. It supplies the nine constructor arguments, five of which are
  closures over `_bridgeCoordinator` (`:135-138`).
- `BreadcrumbItemViewerLifecycleCoordinator.cs:53` exposes `_openCoordinator?.Host`, `:56` exposes
  `_openCoordinator?.CurrentOpenTask`, `:145` calls `UpdateRequestProviders`.

**F13 is not blocked on any F12 seam** — the interface and delegate seams this file needs are all
its own. The constraint is *outbound and contractual*: F13 must not change the coordinator's
constructor arity/order, `Host`, `CurrentOpenTask`, `UpdateRequestProviders`, `SetDroppedDown`,
`HandleSelectorOpenStateChanged`, `Reset`, or `Release` signatures, because F12's file binds to all
of them and F13 must not edit that file. None of the recommended tests in §8 requires any such
change.

---

## 7. Existing test inventory

| Test file | Lines | Cases | Notes |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | **447** | 8 `[TestMethod]` | `public sealed partial class`. Holds the shared `CoordinatorHarness` (`:323`) and `ControlledHost` fake (`:374`). Also asserts (`:155-159`) that the type is non-public and carries no `[ExcludeFromCodeCoverage]`. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | **381** | 11 `[TestMethod]` | Continuation partial; header comment (`:12-16`) documents the 500-line split. Holds `CountingCoordinatorProbe` (`:330`). |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 434 | — | adjacent selector behaviour |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 461 | — | retry behaviour |

What they already assert about this file: constructor null guards (all nine) and
`UpdateRequestProviders` null guards; shared-task semantics for concurrent `RequestOpen`;
snapshot-failure cancellation and retry; false-result cancellation and retry; synchronous and
asynchronous open faults observed exactly once; pending-toggle close with host accepting and
refusing; automatic close requesting `ExplicitCommit`; reset with host already closed; close
throwing then retrying; host-side cancellation not duplicated; selector closing before success;
mouse/keyboard toggle sharing one request; selector state transitions; reset/release/close-result
retry and released blocking; four released-guard cases at `:102`, `:121`, `:125`, `:136`; rollback
secondary failure containment.

**This is already a thorough suite.** The residual gaps are the eight enumerated in §2.

**Line-count headroom:** the primary partial has 53 lines, Part2 has 119. Part2 is the correct home
for new cases up to roughly five short tests; beyond that a `Part3` is required. The `partial`
pattern is already established, so adding cases needs **no new harness** — `CoordinatorHarness` and
`ControlledHost` are visible from any partial.

**One extension to `ControlledHost` is required** (in the test project only): a mode where `Close`
returns `true` without clearing `IsOpen` (for B15/T2), and a mode where `OpenAsync` returns `null`
(for B27/T5). Both are additive properties on the existing fake at
`BreadcrumbDropDownOpenCoordinatorTests.cs:374-445`, costing roughly 8 lines against 53 lines of
headroom in that file.

---

## 8. Recommended test-case list

MSTest + Moq + FluentAssertions, Arrange–Act–Assert, deterministic, no temp files, no live forms, no
popups, no `Thread.Sleep`/`Task.Delay`.

**Target files:**
- Fake-host extensions -> `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs`
  (53 lines headroom; the fake lives there).
- T1–T3 -> `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`
  (119 lines headroom).
- T4–T7 -> new `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`
  (~200 lines projected), declared `public sealed partial class BreadcrumbDropDownOpenCoordinatorTests`
  in `namespace QuickFiler.Test.Viewers`, mirroring the Part2 header comment.

| # | Test name | Closes | Mechanism |
| --- | --- | --- | --- |
| T0 | *(enabler, not a test)* extend `ControlledHost` with `ClaimCloseWithoutClearingOpen` and `ReturnNullOpenTask` | supports T2, T5 | additive properties on the existing fake |
| T1 | `CurrentOpenTask_BeforeAnyRequest_IsACompletedClosedTask` | B10 `:65` | Fresh `CoordinatorHarness`; assert `CurrentOpenTask.IsCompleted` and `Result == false` without any `RequestOpen`. |
| T2 | `RequestOpen_WhileClosePendingAndHostStillOpen_ReturnsClosedWithoutRequesting` | B15 `:92`/`:93` | Host open; `Close` claims but leaves `IsOpen == true`; `SetDroppedDown(false)`; drain; then `RequestOpen()`. Assert the returned task is completed `false`, `Host.Requests` is empty, and `_closePending` was not cleared. **Also documents D7.** |
| T3 | `SetDroppedDown_QueuedBodyDrainedAfterRelease_PerformsNoWork` | B17 `:106`/`:107` | Exact mirror of `HandleSelectorOpenStateChanged_QueuedBodyDrainedAfterRelease_PerformsNoWork` (`Part2.cs:249`): `SetDroppedDown(true)`; `Release()`; `PendingCount == 2`; `DrainOne()`; assert `OpenSelectorCalls == 0`, `SelectorOpenReads == 0`, host untouched. |
| T4 | `RequestOpen_ResetBeforeBeginOpenDrains_NeverConsultsProvidersOrHost` | B26 `:186`/`:187` | `RequestOpen()`; `Reset()`; `DrainAll()`. Assert `Host.Requests` empty, and (via `CountingCoordinatorProbe`) that the row-count/anchor providers were never invoked. |
| T5 | `RequestOpen_HostReturnsNullOpenTask_ReportsContractViolationAndRollsBack` | B27 `:195` | Fake returns `null`. Assert the request completes `false`, and the error sink holds exactly one `InvalidOperationException` whose `Message` is `"The breadcrumb popup host returned no open task."`. |
| T6 | `RequestOpen_ResetWhileOpenPending_LateSuccessIsClosedWithExplicitCommit` | B30 `:210` | Pending `TaskCompletionSource`; `DrainOne()` past `BeginOpenCore`; `Reset()`; `SetResult(true)`; drain. Assert the request completes `false`, `Host.CloseReasons` equals `[ExplicitCommit]`, and `CancelCount == 0`. |
| T7 | `RequestOpen_ResetWhileOpenPending_LateFailureDoesNotCancelTheLiveSelection` | B31 `:225` | Pending `TaskCompletionSource`; `DrainOne()`; `Reset()`; `SetException(...)`; drain. Assert the request completes `false`, `CancelCalls` unchanged, and the failure reached the error sink exactly once. |

T1–T7 are seven independent atomic plan tasks; T0 is an eighth (test-infrastructure) task that must
precede T2 and T5.

### 8.1 Explicit non-goals (record on the irreducible-remainder ledger, do not test)

| Item | Reason |
| --- | --- |
| B32 `:241`/`:242` (`CloseCore` released guard) | Unreachable through the public surface: all three `CloseCore` call sites are preceded by a released check on the same `_sync`-guarded flag within the same synchronous execution (§2.1). Reaching it by reflection would assert a tautology. |

After T1–T7 the projected state is **100% line** (all four uncovered lines reached) and **~98.9%
branch** (one unreachable outcome remaining).

---

## 9. 500-line compliance

- **Current: 309 lines. Headroom: 191 lines.** No pressure whatsoever.
- **No production change is required for any recommended test case.** Every seam needed already
  exists on the constructor. **No new production file, therefore no `QuickFiler/QuickFiler.csproj`
  edit and no new ledger row.** This is the cleanest of the three files in that respect and the plan
  should say so explicitly, because avoiding a csproj edit removes one fan-in conflict surface with
  the other 13 wave-1 children (epic.md:594-617).
- The only new *test* file is `BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`. **`QuickFiler.Test`
  is also a non-SDK project with explicit `<Compile Include>` entries and no globbing** — verified at
  `QuickFiler.Test/QuickFiler.Test.csproj:81-82`, which lists
  `Viewers\BreadcrumbDropDownOpenCoordinatorTests.cs` and `...Part2.cs` individually. Every new test
  file therefore needs an entry adjacent to line 82. Preserve CRLF; use the Edit tool, not `sed -i`.
  `QuickFiler.Test.csproj` is not under concurrent multi-child edit the way `QuickFiler.csproj` is,
  so the conflict risk is low but not zero.

---

## 10. Latent defects

**D7 — a reopen request can be silently dropped after a close claim (`:92-93`).** `CloseCore` sets
`_closePending = true` (`:245`) and, on a successful host close, **never clears it** — it advances
`_generation` and returns (`:257-261`). `_closePending` is cleared only by `ClearClosePending()`
(reached on the host-refused and host-threw paths, `:254`, `:263`), by `Invalidate()` (`:283`, i.e.
`Reset`/`Release`), or by `RequestOpen` itself at `:94` — which is reached **only if the guard at
`:92` lets it through**. Because `IBreadcrumbDropDownHost.Close` is a *claim*, not a completion (see
D1 in artifact 01 and D5 in artifact 02), `_host.IsOpen` can still be `true` at the moment a new
`RequestOpen` arrives, in which case `:93` returns `ClosedTask` and `_closePending` is never
cleared. The user-visible symptom would be a drop-down that ignores the next open request after a
fast close-then-open. This branch is measured as never executed, so there is no evidence it occurs
in practice — but it is the file's only uncovered outcome with a plausible user-facing consequence.
**Recommend promotion to a GitHub issue.** T2 in §8 pins the current behaviour so a later fix has a
regression anchor.

**D8 — `_host.IsOpen` is evaluated while holding `_sync` (`:92`).** `_host` is typed as the
`IBreadcrumbDropDownHost` interface, so an implementation is free to take its own lock in `IsOpen`.
`CloseCore` deliberately calls `_host.Close` *outside* `_sync` (`:247-256`), which establishes the
intended discipline; `:92` violates it. Harmless for the only production implementation
(`BreadcrumbDropDownHost.IsOpen` is a plain field read, `BreadcrumbDropDownHost.cs:191` -> `:225`),
but it is a lock-order inversion waiting for a second implementation. Snapshot `_host.IsOpen` before
taking the lock, or move the check out. Report-only; recommend promotion as a hardening change.

**D9 — stale XML-doc line references in `Part2.cs`.** The doc comments at
`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:188`, `:220`, `:245`, `:275`, `:297` cite
"Coordinator line 99", "line 118", "line 122", "line 133", "lines 224-226". The current
corresponding source lines are `:101-102`, `:119-121`, `:123-125`, `:134-136`, and `:223-228` — each
off by one to three lines. These are documentation drift, not defects, but they will mislead the
next reader. In scope for F13's own change (comment-only, no behaviour). Prefer naming the *member*
rather than the line number when updating, so the reference cannot drift again.

Per the epic's "Latent Defect Promotion" section, promote D7 and D8 via the MCP promotion lifecycle
rather than leaving them as prose here. D9 is fixable in-scope.
