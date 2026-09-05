# 2026-09-05-pr-778-post-merge-review-residuals - Refactor Spec

- **Issue:** #782
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-05T15-47
- **Status:** Draft
- **Version:** 0.1

## Intent & Outcomes

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


## Invariants (must not change)

List the behaviors, contracts, and external surfaces that must remain identical (CLIs, APIs, outputs, data formats, paths).
- Performance characteristics to preserve (latency/throughput/memory):
- Compatibility guarantees (CLI flags, config schemas, versions):

## Scope (structural changes)

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


## Non-Goals

What is explicitly out of scope (new behavior, perf changes, UX changes, flags).

## Dependencies / Touchpoints

Upstream/downstream modules, CLIs, data paths, automation, or external consumers that rely on current structure.
- Required coordination (other teams, CI/CD, release tooling):

## Risks & Mitigations

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


## Technical Specifications

- Files/modules expected to change:
- Public interfaces/contracts affected (even if behavior is unchanged):
- Data flow or validation adjustments:
- Logging/telemetry updates (if any):
- Migration or backfill needs (if any):

## Test Strategy

- Regression tests to add or update:
- Invariant validation tests (ensuring outputs/behavior unchanged):
- Edge cases and negative scenarios (import/path stability, CLI flags):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):

## Definition of Done

- [ ] Structure matches this spec; legacy paths retired or redirected
- [ ] Invariants validated with tests or comparisons
- [ ] Imports/tooling/entry points updated
- [ ] Edge cases and error handling verified
- [ ] Tests, linting, and type checks clean
- [ ] Docs updated (initiative/README/tasks as needed)
- [ ] Toolchain pass completed (format → lint → type-check → test)

## Seeded Test Conditions (from potential)
- [ ] `UiThread_Tests`: populated-branch test on an STA thread with shutdown; unpopulated-branch
- [ ] test asserts `*UiThread.Init()*`.
- [ ] `WpfDispatcherYieldTests`: production fallback provider throws with the shared message.
- [ ] `ProgressTrackerAsync_Tests`: `InitializeAsync` with null dispatcher throws synchronously.
- [ ] `EmailMoveMonitorTests`: order-independence guard fails, not passes, if the fixture cannot
- [ ] resolve the field.
- [ ] `IdleActionQueue_Tests`: cleanup leaves no queued entries or heartbeat subscription.
- [ ] Split `ProgressTracker_Tests` files: all prior tests still discovered and passing.
