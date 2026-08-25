# Epic Kickoff: quickfiler-bug-family

Planned by epic-planner on 2026-08-25T17-20. All twelve child features are prepared: issues
promoted, active folders created, research complete, spec written, and atomic plans approved and
committed, with preflight cleared on all twelve. Verification detail is recorded under
"Preflight Verification Status" below. Planning state:
artifacts/orchestration/epic-planner-state.json (branch:
epic/quickfiler-bug-family-integration).

## Invocation Prompt

Run `/epic-run quickfiler-bug-family` to execute this epic, or paste the prompt below.

Use the epic-orchestrator subagent to execute the prepared epic at
docs/features/epics/quickfiler-bug-family/epic.md. The integration branch
epic/quickfiler-bug-family-integration already contains every prepared feature folder and
approved atomic plan; child features resume at atomic execution from their committed plan-path
rather than re-planning. Execute per the epic-orchestrate skill: wave-scheduled child
orchestrator runs in isolated worktrees, merge-on-green fan-in to the integration branch, and
the final integration-to-main PR.

## Execution Preconditions

Two conditions must hold before `/epic-run` can resolve the dependency graph:

1. `docs/features/epics/quickfiler-bug-family/epic.md` and `epic-planner-resume-state.md` are
   present in the integration worktree but UNCOMMITTED. The preimplementation gate denied every
   staging attempt during planning. Commit both from a session without that gate. Until then the
   manifest is not on the branch and the DAG cannot be read.
2. `main` moved three times during planning (PRs #605, #610, #611), each time invalidating line
   citations in one or more prepared plans. Re-run the corpus-overlap check against `main` before
   wave 0 and re-scope any plan whose cited files moved again.

## Standing Guidance for Executing Children

- Absolute assertions over files a feature does not own go stale on any unrelated PR. Every
  child that hit this converged on baseline-relative comparison recorded at execution time.
  Prefer that form when repairing a stale gate; do not re-introduce an absolute count.
- QuickFiler.csproj and QuickFiler.Test.csproj are legacy non-SDK projects with explicit
  `Compile Include` entries, so nearly every child touches them. Contention is partitioned by
  alphabetical region in the manifest NFRs, not by a dependency edge. Keep each child inside its
  own region.
- Issue 446's plan is the newest content in the corpus: a defect introduced by its own round-4
  insertion was fixed in round 5 and confirmed in a separate round. It carries no outstanding
  defect, but it has had the least settling time of any plan here.

## Preflight Verification Status

All twelve features hold an on-disk `PREFLIGHT: ALL CLEAR` verified against their own
checkpoint's result field, not against a summary claim. Two required a confirming round after
`main` moved during planning; both cleared on the first of two permitted rounds:

- **446** — its re-scope against PR #610 corrected eleven stale citations and converted two
  absolute count gates to baseline-relative form, then exhausted its five-round bound with one
  blocking defect outstanding. The confirming round verified that fix correct, re-verified all
  eleven corrected citations, and found zero new defects. Note for execution: `[P1-T19]` reuses
  `ArrangeIterate` without adjusting that helper's return shape, so the three new tests must
  override the datamodel setup with a throwing one and obtain a real `CancellationTokenSource`.
  Both routes were verified available; this is a heads-up, not a defect.
- **498** — corrected for PR #611 in `ac331702`, chiefly `[P0-T8]`, which described the #439
  test file as seven methods when it now has ten. Sixteen stale occurrences were corrected
  across the plan and spec, including a fifth stale figure in a file not in the originally
  named set, found by diffing the PR's actual changed-file set rather than working from a list.

## Feature Summary

| issue_num | feature_folder | wave | complexity | plan-path |
| --- | --- | --- | --- | --- |
| 442 | quickfiler-home-controller-metrics-442 | 0 | C3 | docs/features/active/quickfiler-home-controller-metrics-442/plan.2026-08-24T09-40.md |
| 446 | quickfiler-bug-family-446 | 0 | C3 | docs/features/active/quickfiler-bug-family-446/plan.2026-08-24T09-37.md |
| 468 | qfc-collection-controller-defects-468 | 0 | C3 | docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md |
| 476 | webview2-host-initializer-defects-476 | 0 | C3 | docs/features/active/webview2-host-initializer-defects-476/plan.2026-08-24T09-38.md |
| 484 | qfc-item-controller-defects-484 | 0 | C3 | docs/features/active/qfc-item-controller-defects-484/plan.2026-08-24T09-36.md |
| 493 | quickfiler-test-uithread-dispatcher-493 | 0 | C2 | docs/features/active/quickfiler-test-uithread-dispatcher-493/plan.md |
| 498 | breadcrumb-router-navigation-defects-498 | 0 | C3 | docs/features/active/breadcrumb-router-navigation-defects-498/plan.2026-08-24T09-39.md |
| 501 | breadcrumb-coordinator-hub-defects-501 | 0 | C3 | docs/features/active/breadcrumb-coordinator-hub-defects-501/plan.2026-08-24T09-40.md |
| 444 | quickfiler-keyboard-action-defects-444 | 1 | C3 | docs/features/active/quickfiler-keyboard-action-defects-444/plan.2026-08-24T20-33.md |
| 464 | efc-controller-surface-defects-464 | 2 | C3 | docs/features/active/efc-controller-surface-defects-464/plan.2026-08-25T07-01.md |
| 489 | itemviewer-surface-defects-489 | 2 | C3 | docs/features/active/itemviewer-surface-defects-489/plan.2026-08-25T01-04.md |
| 488 | itemviewer-breadcrumb-lifecycle-defects-488 | 3 | C3 | docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/plan.2026-08-25T09-53.md |
