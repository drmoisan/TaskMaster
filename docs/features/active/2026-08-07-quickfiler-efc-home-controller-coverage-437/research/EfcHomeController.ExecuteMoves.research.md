# Research — `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs`

- **Feature:** `2026-08-07-quickfiler-efc-home-controller-coverage-437` (epic child F8, issue #437, parent epic #136)
- **Production file:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aea998f94efaa2eb4\QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs`
- **Size:** 144 lines (limit 500 — compliant, see § 10)
- **`[ExcludeFromCodeCoverage]`:** absent. The file is already inside the coverage denominator.
- **Research date:** 2026-08-07
- **Method:** static reading of production and test sources plus an existing Cobertura artifact already committed in this repository. No build, no test run.

---

## 1. Headline finding

**This file already exceeds the 80% per-file line-coverage target.** A Cobertura report committed in
this repository reports `line-rate="0.931624"` (93.16%) and `branch-rate="0.833333"` for this exact
file. The remaining work is therefore *not* "reach 80%" — it is closing eight specific uncovered
lines and three specific half-covered branches, all of which sit on behaviourally important paths
(re-entrancy reset, exception propagation, default metrics routing, production move fallback).

Any plan that proposes broad new test authoring for this file is duplicating work. The genuine gap
is small, precisely locatable, and listed in § 6.

---

## 2. Verified coverage evidence and its provenance

Source artifact (read-only, produced by a sibling in-flight feature, not by F8):

```
docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml
```

- Line 597 of that artifact:
  `<class line-rate="0.931624" branch-rate="0.833333" complexity="19" name="QuickFiler.EfcHomeController" filename="QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs">`
- Denominator reconciliation (verified by hand): the tool emits per-method `<lines>` (44 entries)
  plus a class-level `<lines>` block (73 entries) = 117 counted entries; 8 carry `hits="0"`;
  `109 / 117 = 0.931624`. The arithmetic reconciles exactly, which confirms the artifact was parsed
  correctly and that the `<class>` element for this filename is unique (no second partial entry).

**Provenance caveat.** This artifact was captured on feature branch `...-424`, not on the current
worktree HEAD (`74be1964`). It is treated here as a strong prior, not as F8's acceptance evidence.
Two independent structural checks support its applicability to the current file:

1. Every method's reported line set maps exactly onto the current file's line numbering
   (`TryBeginExecuteMoves` → 49–52, 55–57; `ResetExecuteMovesState` → 60–62; `MoveToFolderAsync` →
   93–109; `SelectMoveMetricsItems` → 116–120; `HandleMoveResult` → 128–142; `ExecuteMovesAsync` →
   32–46; `ExecuteMovesCoreAsync` → 65–84). A drifted file would not align this cleanly.
2. The test files that produce these hits are the ones present on HEAD today (they reference the
   post-#349 breadcrumb-router `SelectedFolder` derivation, which is merged).

**Authority for acceptance:** F8 must re-derive this number with the per-file coverage harness
delivered by upstream child **F1 (`quickfiler-coverage-ledger`)** and commit the result under
`<FEATURE>/evidence/qa-gates/`. F1's harness and its ratified exemption ledger at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` **do not exist on disk yet**;
they were not read and must not be fabricated. This file is expected to be classified `testable`
(not `ratified-exempt`) because it carries no `[ExcludeFromCodeCoverage]` and every host-bound
operation in it is already behind an injectable delegate.

---

## 3. Member-by-member inventory

Existing tests live in
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aea998f94efaa2eb4\QuickFiler.Test\Controllers\`.
Abbreviations: `ExecMovesTests` = `EfcHomeControllerExecuteMovesTests`, `HomeTests` =
`EfcHomeControllerTests`.

| Lines | Member / branch | Status | Evidence |
| --- | --- | --- | --- |
| 13–20 | `MoveToFolderAsyncAction` (auto-property `get`/`set`) | COVERED | `ExecMovesTests.MoveToFolderAsync_WithInjectedAction_UsesCapturedMoveOptions` sets it; `MoveToFolderAsync` reads it |
| 22–23 | `MoveFailureMessageAction` property + field initializer `text => MessageBox.Show(text)` | COVERED (initializer only) | Cobertura line 23 `hits="1"`, attributed to both `.ctor` overloads. The **lambda body is never invoked**, which is correct — invoking it would show a modal `MessageBox` and violate the unit-test policy. See § 7 warning. |
| 25–29 | `MoveMetricsAction` (auto-property `get`/`set`) | COVERED | `ExecMovesTests.HandleMoveResult_WhenMoveSucceeds_RoutesMetricsThroughInjectedAction` |
| 31–36 | `ExecuteMovesAsync` — guard `if (!TryBeginExecuteMoves()) return;` (lines 32–35) | COVERED (false-branch only) | `HomeTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields`. Cobertura line 33 `condition-coverage="50% (1/2)"` |
| 38–45 | `ExecuteMovesAsync` — `try { await ExecuteMovesCoreAsync(); } finally { ResetExecuteMovesState(); }` | **UNCOVERED** | Cobertura lines 39, 40, 41, 43, 44, 45 all `hits="0"` |
| 46 | `ExecuteMovesAsync` closing brace | COVERED | reached via the early-return path |
| 48–57 | `TryBeginExecuteMoves` — both branches | COVERED | `ExecMovesTests.TryBeginExecuteMoves_ReturnsFalseUntilExecutionStateIsReset`; Cobertura line 50 `100% (2/2)` |
| 59–62 | `ResetExecuteMovesState` | COVERED | same test |
| 64–84 | `ExecuteMovesCoreAsync` (all lines 65–84) | COVERED | `ExecMovesTests.ExecuteMovesCoreAsync_UsesFormOptionsAndRoutesSuccessfulMetrics` |
| 86–109 | `MoveToFolderAsync` — injected-delegate branch (lines 102–108) | COVERED | `ExecMovesTests.MoveToFolderAsync_WithInjectedAction_UsesCapturedMoveOptions` |
| 94–101 | `MoveToFolderAsync` — **`MoveToFolderAsyncAction is null` → `_dataModel.MoveToFolderAsync(...)` fallback** | **UNCOVERED BRANCH** | Cobertura line 94 `condition-coverage="50% (1/2)"`. Lines 95–101 report `hits="1"` only because the C# ternary emits one sequence point spanning the whole multi-line expression; the production-fallback arm itself is not executed by any test. |
| 111–120 | `SelectMoveMetricsItems` — both arms | COVERED | `ExecMovesTests.SelectMoveMetricsItems_WhenMovingConversation_ReturnsAllSameFolderItems` and `..._WhenMovingSingleItem_FiltersByCurrentMailEntryId`; Cobertura line 117 `100% (2/2)` |
| 128–132 | `HandleMoveResult` — failure arm (`!result` → `MoveFailureMessageAction`) | COVERED | `ExecMovesTests.HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction`; Cobertura line 129 `100% (2/2)` |
| 135–138 | `HandleMoveResult` — `MoveMetricsAction is not null` arm | COVERED | `ExecMovesTests.HandleMoveResult_WhenMoveSucceeds_RoutesMetricsThroughInjectedAction` |
| 141 | `HandleMoveResult` — **default arm `QuickFileMetrics_WRITE(globals.FS.Filenames.EmailSession, selectedFolder, movedItems)`** | **UNCOVERED** | Cobertura line 141 `hits="0"`; line 135 `condition-coverage="50% (1/2)"` |
| 142 | `HandleMoveResult` closing brace | COVERED | via the two returning arms |

**Uncovered line set (exactly, from the artifact): `{39, 40, 41, 43, 44, 45, 141}` at class-level
plus `141` again at method level = 8 counted misses.**
**Half-covered branch set: line 33, line 94, line 135.**

---

## 4. The move seam — it already exists; no new seam is required

### 4.1 Existing seam (name it exactly)

The move is **not** performed against a live Outlook store from this file. It is routed through an
**injectable-delegate seam** declared in this file:

```csharp
internal Func<string, bool, bool, bool, bool, Task<bool>> MoveToFolderAsyncAction { get; set; }   // lines 13-20
```

Consumed by `MoveToFolderAsync` (lines 86–109) with a null-coalescing dispatch: when
`MoveToFolderAsyncAction` is `null`, control falls back to the production implementation
`EfcDataModel.MoveToFolderAsync(string, bool, bool, bool, bool)`
(`QuickFiler/Controllers/EfcDataModel.cs`, lines 258–296), which builds an `EmailFilerConfig` and
calls `EmailFiler.SortAsync(...)`.

Two further delegate seams in this file complete the surface:

```csharp
internal Action<string> MoveFailureMessageAction { get; set; } = text => MessageBox.Show(text);    // lines 22-23
internal Action<IApplicationGlobals, string, List<MailItemHelper>> MoveMetricsAction { get; set; } // lines 25-29
```

**Conclusion: no additive seam is required for this file.** The seam hierarchy in
`.claude/rules/csharp.md` prefers an interface seam over a delegate, but converting these three
delegates to an interface would be a *breaking* change to a contract that sibling child F9 may
consume, and it would deliver no coverage benefit. Do not do it. See § 8.

### 4.2 Partial-failure semantics — precise, and different from the seeded assumption

`spec.md` § "Seeded Test Conditions" lists "partial failure mid-batch". **There is no batch loop in
this file.** The precise semantics are:

- `ExecuteMovesCoreAsync` issues **exactly one** call to the move seam (line 75), passing the folder
  path and four option flags. The batch-vs-single decision is expressed only as the
  `moveConversation` boolean handed to the seam; iteration over the conversation members happens
  downstream inside `EmailFiler.SortAsync`, which is outside F8's file set.
- The seam returns a **single `Task<bool>`**. There is no per-item result collection, no
  aggregate/partial result type, and no "continue on error" loop in this file. A partial downstream
  failure is flattened by `EfcDataModel.MoveToFolderAsync` into `false`.
- `result == false` → `HandleMoveResult` calls `MoveFailureMessageAction` and **returns without
  writing metrics** (lines 129–132). This is the "failure" state transition, and it is covered.
- `result == true` → metrics are written for **all** items selected by `SelectMoveMetricsItems`,
  regardless of whether individual downstream moves succeeded. This over-reporting is existing
  behaviour and must be preserved (epic NFR: no behaviour change).
- **If the seam *throws***, `ExecuteMovesCoreAsync` does not catch. The exception propagates out of
  `ExecuteMovesAsync`, but the `finally` block (lines 43–45) still runs `ResetExecuteMovesState()`,
  so `_isExecuting` returns to `false` and the controller remains usable. **This is the single most
  important untested invariant in the file** — it is the whole reason the try/finally exists, and
  lines 43–45 are currently `hits="0"`. See T1b in § 6.

### 4.3 Mid-batch cancellation semantics — none exist in this file

Verified by reading every line: `EfcHomeController.ExecuteMoves.cs` contains **zero**
`CancellationToken` parameters, **zero** `ThrowIfCancellationRequested()` calls, and **zero**
`IsCancellationRequested` reads. The controller does own `Token` / `TokenSource`
(`EfcHomeController.cs`, lines 393–409) and passes them to the data-model factories, but
`ExecuteMovesAsync` neither accepts nor observes a token, and the move seam's signature
(`Func<string,bool,bool,bool,bool,Task<bool>>`) has no token parameter.

Therefore:
- There is **no cancellation checkpoint to test**. Writing a "cancellation mid-batch" test would
  require adding a token parameter to the seam and a checkpoint to `ExecuteMovesCoreAsync`, which is
  (a) a behaviour change prohibited by the epic NFR, and (b) a breaking change to the seam signature.
- **Recommended disposition:** record the absence as a finding, do not implement it in F8, and
  promote "ExecuteMovesAsync does not observe the controller's CancellationToken" as a separate
  GitHub issue via the MCP promotion lifecycle. The seeded condition in `spec.md` should be amended
  to "cancellation is not observed — documented, deferred".

### 4.4 Empty-batch and single-item boundary behaviour

- **Single item** (`moveConversation == false`): `SelectMoveMetricsItems` filters `SameFolder` down
  to the entry whose `EntryId` equals `DataModel.Mail.EntryID` (line 119). Covered by
  `ExecMovesTests.SelectMoveMetricsItems_WhenMovingSingleItem_FiltersByCurrentMailEntryId` and, at
  the orchestration level, by `ExecMovesTests.ExecuteMovesCoreAsync_UsesFormOptionsAndRoutesSuccessfulMetrics`.
- **Empty batch** (`SameFolder` empty, or no `EntryId` match): `SelectMoveMetricsItems` returns an
  empty list. The move seam is still invoked (the move decision does not depend on `convInfo`), and
  on success `HandleMoveResult` forwards an empty `movedItems` list. Downstream,
  `EfcHomeController.Metrics.cs` line 18 short-circuits on `moved.Count == 0`, so nothing is written
  and no `DivideByZeroException` occurs. The `HandleMoveResult`-with-empty-list combination is
  covered at the `MoveMetricsAction`-injected level by
  `ExecMovesTests.HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction` (which passes
  `new List<MailItemHelper>()`), but **not** through the default arm at line 141 — which is exactly
  the cheapest way to close that uncovered line (T2 in § 6).
- **Zero-item ordering note:** `SelectMoveMetricsItems` is `static` and pure; it preserves the input
  enumeration order in both arms (`ToList()` and `Where(...).ToList()`). Order preservation is an
  invariant worth asserting but is already line-covered.

### 4.5 Ordering / state-transition invariants (the load-bearing part)

**I1 — Re-entrancy gate.** `_isExecuting` (declared `private volatile bool` in
`EfcHomeController.cs` line 389) transitions `false → true` in `TryBeginExecuteMoves` and
`true → false` in `ResetExecuteMovesState`. `ExecuteMovesAsync` must always restore the flag,
including on the exception path. Sequential contract covered; **exception path uncovered**.

*Concurrency note (do not "fix" in F8):* `TryBeginExecuteMoves` performs a non-atomic read-then-write
(`if (_isExecuting) return false; _isExecuting = true;`). `volatile` gives visibility, not
atomicity, so two threads can both observe `false` and both proceed. A deterministic unit test
cannot prove or disprove this race, so **do not** attempt a threading test. Promote
"TryBeginExecuteMoves check-then-set is not atomic; consider `Interlocked.CompareExchange`" as a
separate issue.

**I2 — Pre-await capture of `_globals`.** `ExecuteMovesCoreAsync` line 74 stores `var globals =
_globals;` *before* awaiting the move (line 75) and passes the captured value to `HandleMoveResult`
(line 83). This exists because `Cleanup()` (`EfcHomeController.cs` lines 342–350) nulls `_globals`,
and a re-entrant `Cleanup` during the await previously produced a `NullReferenceException` — the
defect `HomeTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields`
documents in its comment. The existing `ExecuteMovesCoreAsync` test uses a **synchronously
completing** seam (`Task.FromResult(true)`), so the await never actually suspends and the invariant
is **not exercised**. All lines are covered; the *ordering guarantee* is not. See T3 in § 6.

**I3 — Pre-await capture of form-controller options.** `selectedFolder` (line 66),
`moveConversation` (line 67) and `convInfo` (lines 68–72) are all read from `_formController` /
`DataModel` before the await. Mutating `_formController.SelectedFolder` during the await must not
change what `HandleMoveResult` receives. Same situation as I2: line-covered, ordering untested.

**I4 — Failure short-circuits metrics.** On `result == false`, `HandleMoveResult` returns at line
132 and metrics are never written. Covered.

**I5 — Metrics routing precedence.** `MoveMetricsAction` (test seam) takes precedence over the
production `QuickFileMetrics_WRITE` path. The precedence arm is covered; the **fallback arm is not**.

---

## 5. Seam inventory (what is already injectable)

In this file:

| Seam | Type | Kind |
| --- | --- | --- |
| `MoveToFolderAsyncAction` | `Func<string,bool,bool,bool,bool,Task<bool>>` | injectable delegate |
| `MoveFailureMessageAction` | `Action<string>` (default `MessageBox.Show`) | injectable delegate |
| `MoveMetricsAction` | `Action<IApplicationGlobals,string,List<MailItemHelper>>` | injectable delegate |

Reachable from the shared dependency contract (`EfcHomeControllerDependencies.cs`, F8-owned but
shared with F9): `DataModelFactory`, `AsyncDataModelFactory`, `ViewerFactory`,
`KeyboardHandlerFactory`, `ExplorerControllerFactory`, `FormControllerWithDataFactory`,
`FormControllerWithoutDataFactory`, `InitializeDataFields`, `SelectionLoader`, `MetricsNowFactory`,
`MetricsLineWriter`. Also `EfcHomeController.SetDefaultDependenciesFactory` /
`ResetDefaultDependenciesFactory` (`EfcHomeController.cs` lines 27–38) for the public static entry
points.

State arrangement patterns already proven in this test family and reusable without new production
code: `FormatterServices.GetUninitializedObject` for `EfcHomeController`, `EfcDataModel`,
`EfcFormController`, `EfcViewer`; the private `SetPrivateField` reflection helper; and
`CreateSelectedRouter` (`ExecMovesTests` lines 252–276), which drives a real `BreadcrumbBridgeRouter`
over mocked `IBreadcrumbWebHost` / `IFolderHierarchyProvider` seams to a selected state so
`EfcFormController.SelectedFolder` resolves deterministically.

**NEW ADDITIVE SEAM REQUIRED: none.** Every uncovered line in § 6 is reachable with existing seams
plus Moq fakes of already-public interfaces (`IApplicationGlobals`, `IFileSystemFolderPaths`,
`IAppStagingFilenames`).

---

## 6. Required tests — one per genuine gap

All are MSTest + Moq + FluentAssertions, Arrange–Act–Assert, deterministic, no temp files, no
external services, no live forms, no `Thread.Sleep` / `Task.Delay` / wall-clock waits.

### T1a — `ExecuteMovesAsync` success path drives the core and resets the guard
*Closes: lines 39, 40, 41, 43, 44, 45; branch line 33 true-arm.*

- **Arrange:** `FormatterServices.GetUninitializedObject(typeof(EfcHomeController))`. Inject
  `_formController` via `CreateSelectedRouter`-style helper (reuse the existing private helpers in
  `ExecMovesTests`), `_dataModel` via `CreateControllerDataModel`, `_globals` via a Moq
  `IApplicationGlobals`. Set `MoveToFolderAsyncAction` to a recording delegate returning
  `Task.FromResult(true)`. Set `MoveMetricsAction` to a recorder. **Set `MoveFailureMessageAction` to
  a no-op recorder regardless** (defence in depth against the default `MessageBox`).
- **Act:** `await controller.ExecuteMovesAsync();`
- **Assert:** the move recorder fired exactly once; the metrics recorder fired exactly once; and
  `controller.TryBeginExecuteMoves()` returns `true` afterwards, proving the `finally` reset ran.
- **Determinism:** the seam returns an already-completed task; no scheduling dependency.

### T1b — `ExecuteMovesAsync` resets the guard when the move seam throws
*Closes: the `finally`-on-exception path (lines 43–45) and invariant I1's error arm. Highest
behavioural value in this file.*

- **Arrange:** as T1a, but `MoveToFolderAsyncAction` returns a faulted task
  (`Task.FromException<bool>(new InvalidOperationException("move failed"))`). Using a pre-faulted
  task rather than a synchronous `throw` keeps the async state machine on the realistic path.
- **Act / Assert:** `Func<Task> act = () => controller.ExecuteMovesAsync();`
  `await act.Should().ThrowAsync<InvalidOperationException>();` then
  `controller.TryBeginExecuteMoves().Should().BeTrue("the finally block must reset _isExecuting even when the move seam faults");`
- **Note:** assert the *observable* consequence (`TryBeginExecuteMoves` succeeds again), not the
  private field, so the test survives a future `Interlocked` refactor.

### T2 — `HandleMoveResult` falls back to `QuickFileMetrics_WRITE` when no metrics action is injected
*Closes: line 141; branch line 135 false-arm.*

- **Arrange:** uninitialized `EfcHomeController` with `MoveMetricsAction` left `null`. Build a Moq
  `IApplicationGlobals` whose `FS` returns a Moq `IFileSystemFolderPaths` whose `Filenames` returns a
  Moq `IAppStagingFilenames` with `SetupGet(f => f.EmailSession).Returns("session.csv")`. Use
  `MockBehavior.Loose` or configure the strict chain explicitly — the existing
  `Mock<IApplicationGlobals>(MockBehavior.Strict)` used elsewhere in `ExecMovesTests` will throw on
  `FS` if left unconfigured.
- **Act:** `controller.HandleMoveResult(result: true, globals: globals, selectedFolder: "Archive/Target", movedItems: new List<MailItemHelper>());`
- **Assert:** `Filenames.EmailSession` was read exactly once (`names.VerifyGet(n => n.EmailSession, Times.Once)`)
  and the call does not throw.
- **Why an empty `movedItems` list:** it makes the downstream `QuickFileMetrics_WRITE` short-circuit
  at `Metrics.cs` line 18 *before* touching the null `_stopWatch`, so the test stays a focused
  ExecuteMoves-file test and does not accidentally become a Metrics-file test. This is the
  boundary-condition case identified in § 4.4.

### T3 — `ExecuteMovesCoreAsync` uses the pre-await globals even if `Cleanup` runs during the await
*Closes: invariant I2 (regression guard for the original `NullReferenceException`). Adds no new
covered lines but pins the reason the code is written this way.*

- **Arrange:** as T1a, but `MoveToFolderAsyncAction` returns `tcs.Task` from a
  `TaskCompletionSource<bool>` the test controls.
- **Act:** start `var task = controller.ExecuteMovesCoreAsync();` (do not await); then set
  `_globals` to `null` via the existing `SetPrivateField` helper (simulating `Cleanup()` without
  invoking the real `Cleanup`, which would also null `_formController` and is not what this test is
  about); then `tcs.SetResult(true); await task;`
- **Assert:** the metrics recorder received the **original** globals instance
  (`metricsCall.Globals.Should().BeSameAs(globals)`), and no exception was thrown.
- **Determinism:** `TaskCompletionSource` gives full control of the suspension point — no timers, no
  sleeps. This is the same pattern already used by
  `EfcHomeControllerSeamTests.HandleSelectionChangedAsync_SnapshotsSelectionBeforeAsyncDataLoad`.

### T4 — Form-controller option mutation during the await does not change the recorded move/metrics
*Closes: invariant I3. Optional hardening; adds no new covered lines.*

- Same `TaskCompletionSource` arrangement as T3; mutate `_formController.MoveConversation` and the
  router selection between starting and completing the task; assert the recorded `MoveRequest` and
  the metrics `selectedFolder` still carry the pre-await values.

### T5 — `MoveToFolderAsync` falls back to the data model when no action is injected
*Closes: branch line 94 false-arm (the production-fallback arm).*

- **Arrange:** uninitialized `EfcHomeController`; leave `MoveToFolderAsyncAction` `null`; set
  `_dataModel` to `FormatterServices.GetUninitializedObject(typeof(EfcDataModel))`.
- **Act:** `var result = await controller.MoveToFolderAsync("Archive/Target", true, true, true, false);`
- **Assert:** `result.Should().BeFalse("EfcDataModel.MoveToFolderAsync returns false when MailInfo is null");`
- **Why this is safe and deterministic — this is the key point for the "never against a live store"
  constraint:** `EfcDataModel.MoveToFolderAsync` (`EfcDataModel.cs` lines 266–269) returns `false`
  *on its first statement* when `MailInfo` is `null`. `MailInfo` is
  `ConversationResolver?.MailHelper`, and an uninitialized `EfcDataModel` has a null
  `_conversationResolver`. Execution therefore never reaches `Globals.FS.SpecialFolders`, never
  constructs an `EmailFilerConfig`, never constructs an `EmailFiler`, and never calls
  `SortAsync`. **No Outlook COM object is touched, no store is opened, no file is written.**
  Verified by reading `EfcDataModel.cs` lines 258–296 in full.
- If a future change makes that early return unreachable, drop T5 rather than weakening the
  isolation guarantee; the file would still be at ~93% line coverage without it.

---

## 7. Hard constraints to restate in the plan

- **Never leave `MoveFailureMessageAction` at its default in a test that can reach `result == false`.**
  The default is `text => MessageBox.Show(text)` (line 23), a modal popup requiring human
  interaction — a unit-test-policy violation and a CI hang. Every test that touches
  `HandleMoveResult` or `ExecuteMovesAsync` must assign a recorder to it, even when the test expects
  success.
- **Never exercise the move against a live store.** All move traffic goes through
  `MoveToFolderAsyncAction`, except T5, whose safety is proven by the `MailInfo is null` early return
  documented above.
- **No live forms, no `EfcViewer.Show()`, no UI thread dependency.** `EfcViewer` may only be created
  via `FormatterServices.GetUninitializedObject`, as the existing tests do.
- MSTest / Moq / FluentAssertions; Arrange–Act–Assert; independent, isolated, fast, deterministic;
  no temporary files; no external services; no mutable global state.
- No `Thread.Sleep`, no `Task.Delay`, no real wall-clock waits. Suspension points are controlled with
  `TaskCompletionSource` only.

---

## 8. Cross-child contract note (F9)

`EfcHomeControllerDependencies.cs` and `EfcHomeControllerDependencyFactories.cs` are the injection
contract for the whole EFC controller family, including `EfcFormController` and `EfcItemController`,
which belong to **sibling child F9 (`quickfiler-efc-form-item-controller-coverage`)**. F8 must not
edit F9's files.

**Determination for this file: no dependency-contract change is needed at all.** The three seams
this file relies on are instance properties on `EfcHomeController` itself, not on
`EfcHomeControllerDependencies`, so nothing F9 consumes is touched. Specifically, **do not** promote
`MoveToFolderAsyncAction` / `MoveFailureMessageAction` / `MoveMetricsAction` to an interface seam
during F8: it would be a breaking (non-additive) change to a shared surface for zero coverage gain,
and it would force an F9 edit.

If a future need arises, the only additive-safe shape is a *new* optional constructor parameter on
`EfcHomeControllerDependencies` with a `null` default that falls back to today's behaviour — the
pattern already used by `metricsNowFactory` and `metricsLineWriter`.

---

## 9. Latent defects observed — record, do not fix in F8

The epic NFR forbids behaviour change. Each of these should be promoted to its own GitHub issue via
the MCP promotion lifecycle rather than left as prose that disappears at merge.

1. **`ExecuteMovesAsync` ignores the controller's `CancellationToken`.** No checkpoint exists
   anywhere in the move path owned by this file (§ 4.3).
2. **`TryBeginExecuteMoves` check-then-set is not atomic** despite `_isExecuting` being `volatile`
   (§ 4.5 I1).
3. **Metrics are written for every selected item on `result == true`,** even though the seam's single
   boolean cannot distinguish a fully successful batch from a partially successful one (§ 4.2).

---

## 10. File-size compliance

- Current: **144 lines** against the 500-line ceiling in `.claude/rules/general-code-change.md`.
  Headroom: 356 lines. **No partial split is needed for this file, and none should be proposed.**
- The recommended tests add **no production lines** (no new seam), so post-change size remains 144.
- Test-side: `EfcHomeControllerExecuteMovesTests.cs` is currently 340 lines. Adding T1a, T1b, T2, T3,
  T4 and T5 with their arrangements will plausibly push it past 500. **Plan for a second test file**
  — for example `EfcHomeControllerExecuteMovesStateTests.cs` holding T1a/T1b/T3/T4 (the async
  state-transition and ordering tests) — and factor the shared `CreateController` /
  `CreateFormController` / `CreateSelectedRouter` / `SetPrivateField` helpers into an internal
  test-support class so neither file breaches 500 lines.

---

## 11. Do not duplicate — scenarios already covered

Do **not** author tests for any of the following; each already has a passing test, and re-covering
it adds maintenance cost without moving the per-file number:

| Already covered scenario | Existing test |
| --- | --- |
| `SelectMoveMetricsItems` returns all `SameFolder` items when `moveConversation == true` | `EfcHomeControllerExecuteMovesTests.SelectMoveMetricsItems_WhenMovingConversation_ReturnsAllSameFolderItems` |
| `SelectMoveMetricsItems` filters to the current `EntryId` when `moveConversation == false` | `EfcHomeControllerExecuteMovesTests.SelectMoveMetricsItems_WhenMovingSingleItem_FiltersByCurrentMailEntryId` |
| `TryBeginExecuteMoves` returns `false` on re-entry and `true` again after `ResetExecuteMovesState` | `EfcHomeControllerExecuteMovesTests.TryBeginExecuteMoves_ReturnsFalseUntilExecutionStateIsReset` |
| The injected move seam receives all five options verbatim | `EfcHomeControllerExecuteMovesTests.MoveToFolderAsync_WithInjectedAction_UsesCapturedMoveOptions` |
| `ExecuteMovesCoreAsync` reads form options and routes a successful move to metrics with the filtered item list | `EfcHomeControllerExecuteMovesTests.ExecuteMovesCoreAsync_UsesFormOptionsAndRoutesSuccessfulMetrics` |
| A failed move routes `"Cannot move to folderpath {folder}"` through `MoveFailureMessageAction` and writes no metrics | `EfcHomeControllerExecuteMovesTests.HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction` |
| A successful move routes globals/folder/items through `MoveMetricsAction` | `EfcHomeControllerExecuteMovesTests.HandleMoveResult_WhenMoveSucceeds_RoutesMetricsThroughInjectedAction` |
| A re-entrant `ExecuteMovesAsync` is dropped by the `_isExecuting` guard without dereferencing null fields | `EfcHomeControllerTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields` |

---

## 12. Recommended approach and rejected alternatives

**Recommended:** targeted gap closure — add T1a, T1b, T2, T5 (required to close every uncovered line
and branch) plus T3 and T4 (ordering invariants, no new lines but high regression value); change no
production code; re-measure with F1's harness; commit the per-file number under
`<FEATURE>/evidence/qa-gates/`.

**Rejected alternative A — introduce an `IMailMoveExecutor` interface seam and rewrite the tests
against it.** Rejected: the seam hierarchy's preference for interfaces is a design guideline, not a
licence to break a shared contract; this would be non-additive on a surface F9 may consume, would
require rewriting seven passing tests, and would not change coverage. The existing delegate seam
already provides complete isolation from the live store.

**Rejected alternative B — add a `CancellationToken` parameter to the move seam so a "cancellation
mid-batch" test can be written.** Rejected: it is a behaviour change (epic NFR), a breaking signature
change (F9 risk), and it would be implementing a feature under the guise of a coverage child. The
absence is documented in § 4.3 and promoted as an issue instead.

**Rejected alternative C — declare the file `ratified-exempt` in F1's ledger.** Rejected on the
evidence: the file is already at 93.16% line coverage with no COM dependency of its own, so it fails
the irreducible-remainder test decisively.
