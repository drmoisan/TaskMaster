# injectable-logging-seam-for-qfchomecontroller-metrics (Issue #710)

- Date captured: 2026-08-31
- Author: Dan Moisan

- Status: Promoted -> docs/features/active/injectable-logging-seam-for-qfchomecontroller-metrics/ (Issue #710)

- Issue: #710
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/710
- Last Updated: 2026-08-31
## Problem / Why

Issue #647 added a failure branch to the metrics flush in `QuickFiler/Controllers/QfcHomeController.Metrics.cs`: when `MetricsFileWriter` returns `false`, the controller logs an error. That branch has no test, and one cannot usefully be written today, because the only observable effect of the branch is a call on a static log4net field. A test could enter the branch but could assert nothing about it.

The measurable consequence is a coverage regression on a file the change touched. The file measured about 80.18 percent before the change and 77.05 percent after, with the six added lines uncovered. The feature review for #647 recorded this as a non-blocking finding and recommended a logging seam as the correct remedy.

## Proposed Behavior

Introduce an injectable logging seam for `QfcHomeController` so a test can observe that a failed metrics write produces an error log entry. The seam should follow whatever pattern the repository already uses for injectable collaborators on this controller, so that the change is local and does not require a repository-wide logging migration.

## Acceptance Criteria (early draft)

- [ ] `QfcHomeController` obtains its logger through an injectable member rather than only through a static field, with the production default unchanged.
- [ ] A deterministic test drives `WriteMetricsAsync` with a `MetricsFileWriter` double returning `false` and asserts that exactly one error entry is recorded through the seam.
- [ ] The test asserts the log entry's content, not merely that some call occurred.
- [ ] Line coverage for `QuickFiler/Controllers/QfcHomeController.Metrics.cs` is at least the pre-#647 figure of 80.18 percent.
- [ ] No production behavior changes when the seam is left at its default.

## Constraints & Risks

- The controller is Outlook-Interop-bound, so the seam must be reachable without constructing a live Outlook object; the existing `MetricsFileWriter` property is the precedent to follow.
- A repository-wide logging abstraction is out of scope. Scope this to the one controller unless a shared seam already exists.
- `QuickFiler.Test` runs class-level parallel, so the seam must be an instance member rather than static mutable state.

## Test Conditions to Consider

- [ ] Unit coverage areas: the false-result branch, the true-result branch, and the case where the writer throws.
- [ ] Integration scenarios: confirm the production default still writes through log4net when no seam is supplied.
- [ ] CLI/API examples: not applicable.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/injectable-logging-seam-for-qfchomecontroller-metrics/` folder from the template
