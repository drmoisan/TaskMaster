# Feature Audit: QuickFiler recursive folder selection (#997)

Audit Date: 2026-10-18T01-35 UTC. Template: MCP policy_audit.feature_audit_template.

## Scope and Baseline

Base: origin/main, 09a4522eccece6aeb0294bb2e4f9888cce24dd08. Head: bug/quickfiler-recursive-folder-selection-997, 08d02c07f8a981cb34aa626951931fd78af28fd7. Merge base equals base; timestamp 2026-10-17T19:20:49-05:00. Primary: artifacts/pr_context.summary.txt (fresh at entry); secondary exact diff: artifacts/pr_context.appendix.txt. Active feature docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997 selected by scoping docs and suffix 997. Work Mode: full-bug from issue.md; only spec.md is authoritative. Review covers the full branch, including C# and JavaScript.

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
| 1 | Descendant selection and one commit/close | PASS | Selection and activation issue-997 fixtures | Full MSTest coverage wrapper | Root-bound destination validated by ArchiveStemContract. |
| 2 | Independent toggle/durable collapse/Escape | PASS | Named collapse/Escape/toggle tests | Full MSTest coverage wrapper | C# behavior verified; browser execution gap is R1. |
| 3 | Recursive controls and no leaf toggle | PASS | Host projection tests, compiled resource regex contract, actual script inspection | node --check QuickFiler/Resources/FolderBreadcrumb.js; full MSTest | No live browser claim; JavaScript coverage is R1. |
| 4 | Fail-before/pass-after regressions | PASS | Four original fail-before files plus fifth legacy-boundary pair | Named regression tests and full MSTest | Assertions preserved; root-binding fixture adjustment disclosed. |
| 5 | Invalid addresses and stale fetches | PASS | Validation/messages/TaskCompletionSource ordering tests | Full MSTest coverage wrapper | No sleeps or external services. |
| 6 | Compatibility and lazy expansion | PASS | 332 UtilitiesCS + 513 QuickFiler compatibility cases | Compatibility filter and full MSTest | No-root, duplicate/store, legacy and EFC cases retained. |
| 7 | Full QA, coverage and file limits | PARTIAL | C# comparison and reviewer-local-qa; <=474-line code inventory | Current ordered C# task commands | R1: no numeric JavaScript baseline/final/new/changed-code coverage. |
| 8 | Delivered docs and final-head CI | PARTIAL | Delivered docs; canonical summary has no PR/CI result | Pending exact-head CI checks | R3 awaiting_ci; orchestrator owns final CI/check-off. |

## Summary

Overall Feature Readiness: NEEDS REVISION.
REVIEW_STATUS: REMEDIATION_REQUIRED
PASS: 6; PARTIAL: 2; FAIL: 0; UNVERIFIED: 0.
R1 is autonomous and blocks verification; R2 is nonblocking Markdown whitespace; R3 awaits final-head CI. Policy-audit MCP validation is rejected because absent JavaScript metrics cannot satisfy its numeric schema. The audit set is incomplete. Coordinating orchestrator must delegate the remediation inputs to atomic-planner and obtain a validated plan; target existence alone does not establish planning completion.

## Acceptance Criteria Check-off

AC1-AC6 remain checked. AC7 is reopened because JavaScript coverage is absent, preserving its wording. AC8 remains unchecked pending CI. No new criterion was checked.

### Acceptance Criteria Status

- Source: docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/spec.md
- Total AC items: 8
- Checked off (delivered): 6
- Remaining (unchecked): 2
- Items remaining: AC7 full QA/coverage/file-size criterion; AC8 final-head CI.
