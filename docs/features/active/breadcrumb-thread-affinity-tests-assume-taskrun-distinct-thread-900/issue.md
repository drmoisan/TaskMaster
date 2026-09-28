# Bug: Breadcrumb thread-affinity tests assume Task.Run yields a distinct thread

- Issue: #900
- Work Mode: full-bug

## Summary
Two tests assert a thread-identity property while obtaining their "worker thread" from `Task.Run`:

- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs:204`
  - `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic`
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs:237`
  - `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic`

**`Task.Run` guarantees only *a* thread-pool thread, never a *different* one.** Under parallel test
execution the test body is itself running on a pool thread, so the constructing thread is also
pooled, and thread reuse can make the guard's `CheckAccess()` return true - at which point the
expected boundary diagnostic is never raised and the assertion fails.

`.GetAwaiter().GetResult()` blocking a pool thread while awaiting another compounds the pressure
under contention.

This is a **determinism defect**, not noise. It is the same family as the repository's standing rule
that a suite needing serial execution has already violated unit-test isolation: these tests pass or
fail depending on scheduler behaviour the test never controls.

Note (orchestrator, verified 2026-09-16): the line numbers cited above are the issue's original
citations and have drifted. In the current tree (branch
bug/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900, based on origin/main at
91746d2e4776a59ee1db1856c5c490a009c4958b), the two test names appear in the OPPOSITE order and at
different lines:
- `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` is at line 204.
- `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` is at line 237.
The spec.md and plan for this feature use the re-derived, current line numbers, not the numbers
in this section.

## Environment
(not provided in potential file)

## Steps to Reproduce
(not provided in potential file)

## Expected Behavior
(not provided in potential file)

## Actual Behavior
(not provided in potential file)

## Logs / Screenshots
(not provided in potential file)

## Impact / Severity
(not provided in potential file)

## Source
From: docs/features/potential/2026-09-14-breadcrumb-thread-affinity-tests-assume-taskrun-gives-distinct-thread.md

Note (orchestrator, verified 2026-09-16): this potential document is not present in git history or
the current working tree of this worktree under `docs/features/potential/` or
`docs/features/potential/promoted/`. GitHub issue #900 already existed, was OPEN, and carried the
`bug` label and the `- Work Mode: full-bug` marker at the start of this run, so promotion via
`potential_to_issue` was intentionally skipped (it always creates a NEW issue and would have
duplicated #900). This issue.md was authored directly from the verified `gh issue view 900` body
because `new_active_feature_folder` had no promoted source file to move into place.
