# Feature Audit: QuickFiler recursive folder selection (#997)

Audit Date: 2026-10-18T01-58 UTC. Template: MCP policy_audit.feature_audit_template.

## Scope and Baseline

- Base: origin/main at 09a4522eccece6aeb0294bb2e4f9888cce24dd08.
- Head: bug/quickfiler-recursive-folder-selection-997 at 996dab2f5fbeaad7abda2cc0f8903f65803afa47.
- Merge base: same base SHA; timestamp 2026-10-17T19:20:49-05:00.
- Primary: artifacts/pr_context.summary.txt generated 2026-10-18 01:57:56 UTC at current head.
- Secondary: artifacts/pr_context.appendix.txt, exact full feature-vs-base diff.
- Active feature: docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997, selected from material scoping docs and branch issue suffix 997.
- Work Mode: full-bug, persisted in issue.md; sole AC source spec.md.
- Complete branch includes C#, deployed JavaScript, Node tests, resource/project registration and feature evidence. No changed language was narrowed out.

## Acceptance Criteria Inventory

1. Every displayed folder, including intermediate descendants and leaves at three or more levels, commits through the existing top-level selection interaction; the bound-root repro yields `Projects\Apollo\Alpha` accepted by `ArchiveStemContract.RequireArchiveRelativeStem` and exactly one selection notification/popup close.

2. Clicking any plus/minus only expands/collapses that node, preserves the committed destination even when its ancestor collapses, and emits no selection notification; reopen/Escape retains the committed descendant.

3. Every displayed folder with children has an independent expansion control at every depth, expanding reveals its immediate children, and leaves have no expansion control.

4. Rooted-provider/relative-presented-target selection, selected-descendant collapse/readback, reopen/Escape, and recursive asset regressions have recorded fail-before/pass-after evidence and passing named MSTest coverage.

5. Unknown, unloaded, cross-row, malformed, and out-of-root descendant requests cannot commit; controlled late/reversed fetches after collapse or population replacement cannot attach stale children or change selection.

6. Existing no-root full-path selection, duplicate identity/score contracts, immediate-child message compatibility, and EFC compatibility suites pass; expansion performs no eager whole-subtree enumeration.

7. The full ordered CSharpier, analyzer rebuild, warnings-as-errors rebuild, and MSTest/coverage pass succeeds with baseline comparisons, at least 80% repository line coverage, at least 90% new-code coverage, no reduced changed-line coverage, and no added exclusions; source/test/script files remain within 500 physical lines.

8. This spec and implementation evidence describe delivered behavior, and all required CI checks succeed against the final PR head.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
| --- | --- | --- | --- | --- | --- |
| 1 | Descendant selection, valid relative target, one notification/close | PASS | C# selection/activation fixtures and full independent C# run | Full MSTest coverage wrapper | Root-bound intermediate/leaf destinations verified; script sends exact known address once. |
| 2 | Independent toggles and durable collapse/reopen/Escape | PASS | C# collapse/Escape tests; actual script toggle/committed-display fixtures | Full MSTest; Node 78-fixture run | Click/Enter/Space expansion propagation stopped; host selection preserved. |
| 3 | Recursive controls at every depth, leaves omitted | PASS | Three-level actual-script DOM tests and host projection/provider tests | Node actual-script execution; full MSTest | Nonleaf controls and immediate-child ordering verified without eager traversal. |
| 4 | Recorded fail-before/pass-after and named regression coverage | PASS | Four original fail-before pairs plus legacy indexed boundary pair | Named issue-997 tests plus full MSTest | Original behavioral assertions preserved. |
| 5 | Invalid requests and stale fetches rejected | PASS | Strict token/root/identity tests and controlled TaskCompletionSource ordering | Full MSTest coverage wrapper | Unknown/unloaded/cross-row/out-of-root and collapse/replacement/reversed completion cases covered. |
| 6 | No-root/duplicates/scores/legacy/EFC/lazy compatibility | PASS | 332 UtilitiesCS + 513 QuickFiler compatibility cases, provider-call assertions, common Node baseline/final suite | Compatibility filter, full MSTest and Node common fixtures | No EFC production change; existing compatibility remains. |
| 7 | Full QA, numeric coverage/no regression and file limit | PASS | Complete C# ordered QA; actual-script numeric baseline/final/new/changed proof; clean whitespace and line counts | Current C# task commands; native Node coverage/syntax; git diff --check origin/main...HEAD | C# 86.10% lines/80.31% branches, new 98.70%, changed 99.09%; JavaScript 100% full/new/changed lines and branches; no exclusions added. |
| 8 | Delivered documentation and all required final PR-head CI | PARTIAL | Delivered docs are current; no final PR-head CI exists in context | Pending required final-head CI, orchestrator-owned | R3, Remediability: awaiting_ci. Earlier 08d02c07 workflow success is not current/final-head proof. |

## Summary

REVIEW_STATUS: AWAITING_CI
Overall local acceptance: PASS for AC1-AC7. Overall delivery remains pending AC8.
PASS: 7; PARTIAL: 1; FAIL: 0; UNVERIFIED: 0.
Autonomous blocking findings: 0. Sole open delivery finding R3 is awaiting_ci. Accordingly no new remediation plan/delegation target is created. Current wait inputs document the required checks.

Earlier R1/R2 were repaired by delegated atomic-planner/atomic-executor remediation; all 17 plan tasks are complete and the existing plan passes canonical validation. Current policy audit validates with real C#/JavaScript metrics, resolving R4 for this new audit. Prior rejected audits retain their historical status.

## Acceptance Criteria Check-off

AC1-AC6 were already checked and remain checked. This review newly checks AC7 after examining complete runtime coverage, no-regression and ordered QA evidence. Only the checkbox is changed; criterion wording is preserved. AC8 remains unchecked until the item's own orchestrator verifies final PR-head CI, commits/pushes its check-off and re-verifies the resulting final head.

### Acceptance Criteria Status

- Source: docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/spec.md
- Total AC items: 8
- Checked off (delivered): 7
- Remaining (unchecked): 1
- Items remaining: This spec and implementation evidence describe delivered behavior, and all required CI checks succeed against the final PR head.
