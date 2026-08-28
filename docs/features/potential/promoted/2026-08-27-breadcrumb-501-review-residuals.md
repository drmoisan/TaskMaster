# breadcrumb-501-review-residuals (Issue #657)

- Date captured: 2026-08-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-501-review-residuals/ (Issue #657)

- Issue: #657
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/657
- Last Updated: 2026-08-27
## Summary

Three non-blocking findings from the feature review of #501 that would otherwise disappear when that
feature folder is merged. None blocks #501; all three are real and independently verified by the
reviewer. They concern the breadcrumb messenger hub and bridge coordinator surface.

## Problem / Why

The #501 review returned zero Blocking findings but five non-blocking ones. Two were documentation
defects and were fixed in place. The three below describe code and verification residuals that need a
separate decision, and are recorded here rather than left as prose in a feature folder.

### R-1 — AC-11's logging assertion is source-level, and the stated obstacle was overstated

`QuickFiler/Viewers/BreadcrumbMessengerHub.cs:163-168` logs a per-surface delivery failure through
log4net. #501 verified that by source inspection. The plan's original justification (ruling PD-2)
claimed the test project has no log4net reference, which is false; #501 corrected that to a file-budget
argument, namely that `BreadcrumbMessengerHubTests.cs` sits at 492 of 500 lines.

The reviewer showed the replacement argument is also too strong. `BreadcrumbMessengerHubCoverageTests.cs`
is 478 lines, already carries a `<Compile Include>` entry at `QuickFiler.Test.csproj:97`, is
hub-cohesive, and is not a new file, so the no-new-test-file constraint does not reach it. A reusable
MemoryAppender pattern already exists at `BreadcrumbBridgeRouterIssue614Tests.cs:338-345`.

Mitigating fact: the hub is 306/306 line-covered, so the catch block demonstrably executes at runtime.
Only the log record's content and level are unasserted.

### R-2 — the superseded-AddItems skip branch is inert, and lease-settlement ownership is duplicated

`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` ends `AddItemsCore` with
`if (!ran) { _upgradeLifetime.Abandon(lease); }`. `RunSynchronous` already calls `Abandon` on every
`false` return, so this block has no observable effect. The reviewer verified the double call is
genuinely idempotent by reading `Abandon`, `CancelLease` and `Complete`.

The consequence is that the test added for this branch,
`AddItemsCore_SupersededLeaseSkipsAppendAndSettlesTheLease`, would still pass if the block were deleted:
it asserts `dead.Settled`, which the inner `Abandon` already set. The branch is covered but not
discriminated, and settlement responsibility now sits in two layers held together only by an XML comment.

Decide one of: delete the caller-side `Abandon` and let `RunSynchronous` own settlement exclusively; or
keep it and give the test an assertion that fails when it is removed.

### R-3 — an uncovered line leaves an AC-03 clause unexercised

`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:313` has zero hits, so the AC-03 clause "after
`Release()`, `CloseCore` returns false" is structurally unexercised. This is a pre-existing coverage
shape rather than a regression introduced by #501.

## Proposed Behavior

Resolve each residual explicitly: add the runtime log assertion in the coverage test file, settle the
ownership question in R-2 with a discriminating test or a deletion, and cover the R-3 line.

## Acceptance Criteria (early draft)

- [ ] A runtime assertion proves the hub logs a per-surface delivery failure, with its level and message.
- [ ] Lease settlement has exactly one owner, and a test fails if that owner's call is removed.
- [ ] `BreadcrumbDropDownOpenCoordinator.cs:313` is executed by a test.

## Constraints & Risks

- `BreadcrumbSelectorCoordinatorTests.cs` is at exactly 500 lines, so it has zero headroom.
- `BreadcrumbMessengerHubTests.cs` is at 492 lines; prefer `BreadcrumbMessengerHubCoverageTests.cs` at 478.
- Repository line coverage clears its 85% floor by only 0.1448 pp, so any change that adds uncovered
  production lines needs matching tests.
- A log4net appender test mutates a process-wide logging repository; attach to the specific logger and
  remove the appender in a finally block.

## Test Conditions to Consider

- [ ] MemoryAppender assertion on the hub's per-surface failure log
- [ ] A test that fails when the caller-side Abandon is deleted, if that call is kept
- [ ] Coverage for the post-Release CloseCore path

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create an active folder when scheduled

## References

- Source review: `docs/features/active/breadcrumb-coordinator-hub-defects-501/code-review.2026-08-27T23-48.md`
- Feature audit: `docs/features/active/breadcrumb-coordinator-hub-defects-501/feature-audit.2026-08-27T23-48.md`
- Related: #501, #462, #500, #502
