# Code Review: QuickFiler recursive folder selection (#997)

Review Date: 2026-10-18T01-35 UTC
Base Branch: origin/main
Head: bug/quickfiler-recursive-folder-selection-997 at 08d02c07f8a981cb34aa626951931fd78af28fd7
Feature Folder: docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997
Template: MCP policy_audit.code_review_template.

## Executive Summary

Host implementation inspection found no demonstrated runtime blocker. Known descendant identities authorize selection; optional root binding converts rooted provider values to archive-relative filing stems; committed nodes survive collapse/Escape; request/population guards reject obsolete completions. Separate recursive controls stop activation events. Legacy indexed messages and EFC behavior retain compatibility coverage.

PR readiness is No-go. R1 blocks verification because the extracted/extended JavaScript executes in the product but has no JavaScript execution coverage. C# measurements and text/regex asset tests cannot replace that requirement. R3 is final-head CI, awaiting_ci. R2 is a nonblocking whitespace correction. Policy-audit validation additionally cannot accept missing JavaScript measurements, even when explicitly FAIL; retain that actual failed validator result until remediation supplies real numbers.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| Major / Blocking R1 | QuickFiler/Resources/FolderBreadcrumb.js | lines 1-347; recursive makeSubfolders at 139-192 | JavaScript baseline/final/changed-code coverage is absent. Remediability: autonomous. | Execute the real script with deterministic in-memory browser seams and existing Node tooling; collect numeric baseline, final, new/changed-line and branch coverage, without packages or exclusions. | Full canonical review requires coverage for every changed implementation language; C# string tests do not execute event propagation/render logic. | coverage-comparison.2026-10-18T01-27.md:15; self-review.2026-10-18T01-32.md:10; .claude/skills/feature-review-workflow/SKILL.md step 5 |
| Minor R2 | docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/evidence | changed Markdown; legacy fail-before evidence lines 26-47 | Full branch whitespace check fails; prior worktree-only command omitted committed changes. Remediability: autonomous; nonblocking. | Normalize changed Markdown evidence whitespace and verify git diff --check origin/main...HEAD. | Evidence should name the comparison actually checked. | reviewer-diff-whitespace evidence, exit 2 |
| Info / delivery gate R3 | docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/spec.md | AC8 | Final PR-head CI unavailable. Remediability: awaiting_ci. | Orchestrator opens/pushes PR, verifies all required checks at the exact final head, checks AC8, and repeats CI after check-off commit. | CI-dependent acceptance cannot be claimed from local tests. | canonical summary CI status unavailable, no PR exists |
| Info R4 | policy-audit artifact validator | MCP response | Published validator requires numeric coverage even for an explicit missing-evidence FAIL. Remediability: external_dependency for validator behavior. | Preserve failed response; rerun canonical validation after R1 evidence supplies actual metrics. Do not invent numeric measurements. | A structurally rejected policy audit is not complete. | validate_orchestration_artifacts responses during this review |

## Implementation Audit

C# host state separates descendant identity, raw provider path and selected filing value. BreadcrumbSelectionSession.Descendants validates row/node/root before mutation. FolderBreadcrumbBridgeRouter.Descendants awaits provider calls outside the lock and rechecks population, row reference, node occurrence, generation and expansion. Collapse preserves committed selection while hiding children; full cancellation snapshot restores the descendant. Invalid wire values are parsed strictly and return no selection/close effects.

QFC captures its configured root and uses FolderBreadcrumbDocument.Html for both collapsed/popup hosts. Existing unbound overloads preserve full-path semantics. Resource registration is exact and extraction keeps files under 500 lines. No package or coverage configuration changed.

JavaScript uses textContent for provider names and known IDs for wire messages. Toggle handlers prevent default and stop propagation; name activation is separate. Recursion renders immediate loaded children only. These observations are static inspection; executable event/DOM coverage remains R1.

## Test Quality Audit

Six new focused fixtures cover selection, validation, ordering, messages, host activation and asset composition. Strict Moq providers and TaskCompletionSource barriers avoid sleeps/live services. Existing no-root, duplicate/score, legacy message and EFC suites remain present. Four original repros plus one legacy root-boundary repro have fail-before/pass-after artifacts. Compiled asset tests prove script insertion and syntax checks parse it; neither provides JavaScript execution coverage.

Reviewer checks: CSharpier passed for 1664 files; analyzer Rebuild and warnings-as-errors Rebuild each zero warnings/errors; full MSTest 7504 passed, zero failures, 48.0895 seconds. Reviewer coverage 57271/66515 lines (86.10%), 13923/17337 branches (80.31%). Executor comparison: changed C# lines 97.57% to 99.09%, new measured files 98.70%, all 45 measured new methods >=90%. Existing COM/VSTO exemptions are unchanged.

## Security / Correctness Checks

PASS for inspected known-identity routing, strict token types, root-boundary rejection, duplicate/store separation, stale completion rejection and textContent use. No arbitrary browser path authorizes filing. No live Outlook/WebView session was claimed; behavioral DOM execution and JavaScript coverage remain unverified.

## Research Log

Primary canonical PR summary was read first; appendix supplied exact branch diff anchors. Full spec, completed plan, current coverage/regression/compatibility/isolation evidence, new fixtures, production state/router/session/projection/coordinator/resource changes and current task commands were inspected. Base/merge-base: 09a4522eccece6aeb0294bb2e4f9888cce24dd08, timestamp 2026-10-17T19:20:49-05:00. Feature chosen by changed scoping docs plus issue suffix 997; full-bug resolves spec-only acceptance.

## Verdict

REVIEW_STATUS: REMEDIATION_REQUIRED
blocking autonomous findings: 1 (R1).
Nonblocking cleanup: R2. Pending delivery gate: R3. Policy artifact validator limitation: R4.
No-go until JavaScript execution coverage is verified, the resulting audit set validates, and final-head CI satisfies AC8. No code was fixed during review. Planning is owned by the coordinating orchestrator and is not approved merely because a target exists.
