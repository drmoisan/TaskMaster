# pr-778-post-merge-review-residuals (Issue #782)

- Date captured: 2026-09-05
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/pr-778-post-merge-review-residuals/ (Issue #782)

- Issue: #782
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/782
- Last Updated: 2026-09-05
## Summary

Consolidate every actionable finding from the three-phase post-merge code review recorded on PR #778
(merge commit `1c3b210c`, issue #584, `UiThread.Dispatcher` null guard). The review returned zero
Blocking findings, 7 Should-fix, 25 Nit, and 6 Refuted. This entry carries all of them into one
Refactor delivery so that none is lost when the #584 feature folder is archived.

Source of record: the section `## Post-merge code review (three-phase, 2026-09-05)` in the body of
https://github.com/drmoisan/TaskMaster/pull/778. Finding identifiers below (C01..C26, S2-1, S3-1..S3-9,
S4-1, S4-2) refer to that section.

## Problem / Why

PR #778 changed `UiThread.Dispatcher` from a null-returning accessor to one that throws
`InvalidOperationException`. The review confirmed the fix is correct and found no regression, but it
identified a set of residuals across production code, tests, and the feature folder's documentation
and evidence that are individually small and collectively worth one coordinated pass:

- A latent test hang (C10) and a latent double-read race in the new getter (C02).
- A reflection-based order-independence guard in QuickFiler.Test that degrades to a no-op on a
  field rename (C18), while a lock-guarded fixture in the same assembly already exposes the value.
- Comments and reason strings in three test files that still describe the pre-#778
  `NullReferenceException` mechanism (C19, S2-1) and a false comment in `WpfDispatcherYield.cs` (C20).
- A 514-line test file that exceeds the 500-line limit in `CLAUDE.md` (C16) with no rule-level exemption.
- Six independent reflection sites on `UiThread._dispatcher`, each handling a missing field
  differently, where `InternalsVisibleTo("UtilitiesCS.Test")` permits an internal seam (C12, C13).
- Audit artifacts in the #584 feature folder that misstate the formatter command that was run (S3-2),
  the evidence count (S3-3), ordering prose that the timestamps contradict (S3-1), and several
  smaller consistency defects (S3-4..S3-9).

## Finding Disposition

### In scope: Should-fix (7)

| ID | Location | Change |
|---|---|---|
| C10 | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | Obtain the sentinel dispatcher on a dedicated STA thread and shut it down in `finally`; keep the populated-branch test. |
| C02 | `UtilitiesCS/Threading/UiThread.cs` getter | Read `_dispatcher` once into a local (or `Volatile.Read`) before the null check and return. |
| C18 | `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | Replace the local `FieldInfo` and both `?.` reads with `UiThreadDispatcherFixture.Current`; remove the two WindowsBase comment fragments (with C25). |
| C19 | `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | Rewrite the P27-T2 docstring, Act comment, and `NotThrow` reason to describe the synchronous `InvalidOperationException` path. |
| C20 | `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | Correct the comment; route both throws through one shared message constant; add a `WithMessage` assertion in `YieldAsync_WithoutDispatcher_RemainsStrict`. |
| C16 | `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` (514 lines) | Split into two cohesive files under 500 lines each; register the new file in the test csproj. |
| S3-2 | `#584` feature folder `policy-audit` and `feature-audit` | Correct the formatter command cells, amend row 3.1, add the section 8 gap entry. |

### In scope: Nits, code and tests (14)

- C03 `UiThread.Init()`: set the single-shot latch only after `Initialize()` succeeds so a failed
  initialization can be retried; word the message accordingly.
- C05 `UiThread.Dispatcher`: add a two-line comment stating why this accessor does not lazily call
  `Init()` (`Initialize()` shows a hidden WinForms window and must run on the UI thread).
- C06: shorten the message to name only the public `Init()`; assert `*UiThread.Init()*` in the test.
- C08: add `<summary>`, `<remarks>`, and `<exception cref="InvalidOperationException">` XML docs.
- C09 (message only): append the STA/UI-thread requirement to the message text. The behavioral
  follow-up (make `Init()` reject non-STA callers) is out of scope; see below.
- C11: move the null guard into `DispatcherField()`; use expression-bodied throw-assertion lambdas.
- C12, C13: add one internal `IDisposable` install scope on `UiThread` (or under
  `UtilitiesCS.Test/TestHelpers/`) and migrate the four UtilitiesCS.Test reflection sites to it.
- C14: add a `TestCleanup` to `IdleActionQueue_Tests` that drains entries and unsubscribes the
  heartbeat.
- C15: expand `[TestClass, DoNotParallelize]` to two attributes when the file is split (C16).
- C21: add one `[DoNotParallelize]` test that nulls the static, calls `YieldAsync` from a thread
  with no dispatcher, and asserts `InvalidOperationException` with `*UiThread.Init*`.
- C25: delete the two "avoid WindowsBase" comment clauses in `EmailMoveMonitorTests.cs`.
- C26: add `InitializeAsync_WhenDispatcherNotCaptured_ThrowsInvalidOperationException` in
  `ProgressTrackerAsync_Tests.cs`.
- S2-1: correct the Arrange comment in
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`.

### In scope: Nits, documentation and evidence in the #584 feature folder (8)

- S3-1: soften the ordering sentences in `p1-t4-expect-fail.md`, `p3-t1-analyzer-build.md`,
  `feature-audit.md`, and `policy-audit.md`.
- S3-3: correct "34 evidence artifacts" to the `git ls-tree` count (38).
- S3-4: note the filename/`Timestamp:` mismatch on `issue-584.2026-09-02T09-02.md` in place
  (do not rename a committed evidence file).
- S3-5: normalize `EXIT_CODE:` to a single integer in the three named evidence files.
- S3-6: set `spec.md` Status to reflect the merged state and reconcile the three file lists.
- S3-7: reconcile the call-site counts in `spec.md` to the grep-verified figure.
- S3-8: replace evaluative wording flagged under `tonality.md` with neutral phrasing.
- S3-9: record whether the ProgressTrackerAsync_Tests synchronization follow-up was promoted; if
  not, it is satisfied by C26 in this delivery and the artifacts should say so.

### In scope: optional cleanups from Refuted items (2)

- C01 `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`: remove the now-dead `dispatcher != null`
  comparisons.
- C23 `ProgressTracker.cs` and `ProgressTrackerAsync.cs`: pass the captured `UiDispatcher` into the
  `InvokeAsync` lambda instead of re-reading the static.

### No action

- C04, C07, C22, C24: refuted with no cleanup recommended.
- C17: class-level `[DoNotParallelize]` is defensible per plan rationale and repository precedent.
- S4-2: evidence-scope observation only; CI ran every test assembly.

### Out of scope, tracked separately

- C09 behavioral follow-up (make `Init()` reject non-STA callers): a production behavior change
  that would break the existing worker-thread `UiThread.Init(false)` call in
  `QfcHomeControllerRunAsyncTests.cs`. Promote as its own entry.
- S4-1 stale `.claude/agent-memory/task-researcher/` notes and the S3-1 request to define
  `Timestamp:` semantics in the `evidence-and-timestamp-conventions` skill: both live under
  `.claude/`, which is overwritten by push-down from drm-copilot. Fix upstream.

## Proposed Behavior

After this refactor:

- `UiThread.Dispatcher` reads its backing field once, carries XML documentation, and throws a
  message that names only `Init()` and states the UI-thread requirement. `Init()` can be retried
  after a failed `Initialize()`.
- `WpfDispatcherYield` and `UiThread` share one message constant for the not-initialized precondition.
- All UtilitiesCS.Test manipulation of `UiThread._dispatcher` goes through one disposable install
  scope; QuickFiler.Test reads it through `UiThreadDispatcherFixture.Current`.
- No test creates an unshut dispatcher on a pooled MTA thread.
- No test file in the touched set exceeds 500 lines.
- Comments and reason strings describe the synchronous `InvalidOperationException` mechanism.
- The #584 feature folder's audits and evidence are internally consistent and neutral in tone.

## Acceptance Criteria (early draft)

- [ ] AC1: Each of the 7 Should-fix findings (C10, C02, C18, C19, C20, C16, S3-2) is resolved as
      specified in the Finding Disposition table, with a test or artifact diff as evidence.
- [ ] AC2: Each of the 14 in-scope code/test nits is resolved, or its omission is recorded with a
      reason in the delivery's code review artifact.
- [ ] AC3: Each of the 8 in-scope documentation/evidence nits is resolved in the #584 feature folder.
- [ ] AC4: The two optional refuted-item cleanups (C01, C23) are applied.
- [ ] AC5: The `UiThread._dispatcher` reflection sites in UtilitiesCS.Test are reduced to one shared
      seam; `EmailMoveMonitorTests.cs` contains no `FieldInfo` for `_dispatcher`.
- [ ] AC6: `ProgressTracker_Tests.cs` and its split sibling are each under 500 lines and both are
      compiled by `UtilitiesCS.Test.csproj`.
- [ ] AC7: New tests C21 and C26 fail if the corresponding throw is removed and pass on the current code.
- [ ] AC8: The C09 behavioral follow-up is promoted as a separate potential entry, and the S4-1
      upstream fix is recorded as a follow-up for drm-copilot.
- [ ] AC9: The full C# toolchain (csharpier, analyzers, nullable, vstest with coverage) passes, and
      changed-line coverage does not decrease.

## Constraints & Risks

- `UiThread.cs` is 172 lines and `WpfDispatcherYield.cs` is 77; `EmailMoveMonitorTests.cs` is 320
  and `QfcItemController.InitializationTests.Part2.cs` is 393. Only `ProgressTracker_Tests.cs`
  (514) is over the limit.
- The shared dispatcher install scope must be `[DoNotParallelize]`-safe: every writer of the static
  remains serialized, and the scope must restore the prior value in `Dispose` even when the prior
  value is null.
- The STA sentinel for C10 must call `BeginInvokeShutdown` and join the thread so no dispatcher
  outlives the test.
- Changing the exception message text (C06, C09) must update every test that asserts on it; grep
  for `UiThread.Initialize()` across all test projects before and after.
- Documentation edits touch committed evidence files. Edit content in place; do not rename files or
  alter `Timestamp:` values.
- `.claude/**` is push-down-owned and must not be edited in this repository.
- This is a Refactor, not a Bug: the bugfix workflow (regression test first) applies only to C10
  and C02 within the plan.

## Test Conditions to Consider

- [ ] `UiThread_Tests`: populated-branch test on an STA thread with shutdown; unpopulated-branch
      test asserts `*UiThread.Init()*`.
- [ ] `WpfDispatcherYieldTests`: production fallback provider throws with the shared message.
- [ ] `ProgressTrackerAsync_Tests`: `InitializeAsync` with null dispatcher throws synchronously.
- [ ] `EmailMoveMonitorTests`: order-independence guard fails, not passes, if the fixture cannot
      resolve the field.
- [ ] `IdleActionQueue_Tests`: cleanup leaves no queued entries or heartbeat subscription.
- [ ] Split `ProgressTracker_Tests` files: all prior tests still discovered and passing.

## Next Step

- [ ] Promote to GitHub issue (refactor)
- [ ] Create `docs/features/active/pr-778-post-merge-review-residuals/` folder from the template
