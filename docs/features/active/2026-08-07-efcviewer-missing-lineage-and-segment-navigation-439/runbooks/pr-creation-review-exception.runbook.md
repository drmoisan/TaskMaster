# Issue #439 Pull-Request Review Exception Runbook

## Cue

Use this runbook after the user-authorized, one-time exception permits opening the Issue #439 pull request while the latest feature review remains `REMEDIATION_REQUIRED`. The exception applies only to pull-request creation and review visibility. It does not mark the findings resolved, assert policy compliance, authorize merge, or waive required GitHub checks.

## Prerequisites

- Repository write access sufficient to push `bug/efcviewer-missing-lineage-and-segment-navigation-439` and open a pull request against `main`.
- Review of `policy-audit.2026-08-24T22-20.md`, `code-review.2026-08-24T22-20.md`, and `feature-audit.2026-08-24T22-20.md`.
- Acknowledgement that `BreadcrumbBridgeRouter.cs` is 596 lines, `BreadcrumbBridgeRouterIssue439Tests.cs` is 531 lines, and `EfcFormController.cs` coverage is 81/721 = 11.234397%.
- Acknowledgement that the headless audit passed across 18 relevant test sources and that the controller feasibility analysis projects at most 176/721 = 24.410541% coverage within the narrow Issue #439 scope.
- Awareness that GitHub issue #452 owns the broader EFC controller extraction and coverage work.

## Step-by-step Instructions

1. Push the committed Issue #439 branch without rewriting its history.
2. Open a pull request from `bug/efcviewer-missing-lineage-and-segment-navigation-439` to `main`.
3. State the three unresolved review findings and this one-time PR-creation exception in the pull-request description. Do not describe the latest review as passing.
4. Review the pull request one changed file at a time, with particular attention to breadcrumb lineage construction, bridge message decoding, selection state, child expansion, and preservation of filing-target and probability semantics.
5. Inspect the pull request's GitHub checks for the exact current head commit. Treat pending or failed checks as unresolved.
6. Keep merge as a separate decision. Before merging, either resolve the recorded findings through issue #452 or document a separate, explicit merge authorization that identifies the remaining findings and current check results.

## Verification

- A GitHub pull request exists with base `main` and head `bug/efcviewer-missing-lineage-and-segment-navigation-439`.
- The pull-request description discloses the latest `REMEDIATION_REQUIRED` result and its three findings.
- The pull request does not claim that the 80% controller coverage floor or 500-line file-size limits passed.
- The orchestration checkpoint records `response: "exception"` and this runbook's repository-relative path.
- The branch head shown by GitHub matches the locally pushed commit, and check conclusions are reported without treating pending checks as passing.

## Source and Citation

- GitHub Docs, "About pull requests," https://docs.github.com/en/pull-requests/get-started/about-pull-requests — updated_at: 2026-08-24. This documents pull requests as the review and automated-check surface. No GitHub documentation MCP source was available in this session, so the official vendor documentation was used.
- GitHub Docs, "Quickstart for reviewing pull requests," https://docs.github.com/en/pull-requests/get-started/reviewing-pull-requests-quickstart — updated_at: 2026-08-24. This documents file-by-file review and the approve, comment, and request-changes outcomes.
