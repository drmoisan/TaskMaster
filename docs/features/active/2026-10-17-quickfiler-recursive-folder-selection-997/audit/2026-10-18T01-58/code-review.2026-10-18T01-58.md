# Code Review: QuickFiler recursive folder selection (#997)

Review Date: 2026-10-18T01-58 UTC
Template: MCP policy_audit.code_review_template.
Feature Folder: docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997
Base: origin/main; merge base 09a4522eccece6aeb0294bb2e4f9888cce24dd08.
Head: bug/quickfiler-recursive-folder-selection-997 at 996dab2f5fbeaad7abda2cc0f8903f65803afa47.

## Executive Summary

No autonomous blocking code or verification finding remains. Host-owned recursive identity/state, validated archive-relative descendant selection, durable collapse/Escape selection and obsolete-fetch rejection have deterministic C# proof. The same unchanged deployed JavaScript is now executed through in-memory DOM/WebView/event seams, with full numeric baseline/final/new/changed-code coverage. Remediation adds tests and evidence; production code remains identical to the independently reviewed source head.

Prior R1 coverage and R2 whitespace are resolved. The new policy audit passed canonical validation, closing the prior R4 structural rejection for this audit set; rejected historical audits remain historical. PR readiness is conditional on R3 final PR-head CI. No source fixes were made during re-review.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| Blocking delivery gate R3 | docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/spec.md | AC8 | Required final PR-head CI is not yet available. Remediability: awaiting_ci. | Orchestrator creates/pushes PR, verifies all seven required jobs at final head, checks AC8, then pushes/reverifies the check-off head. | Earlier source CI does not establish current/final PR-head acceptance. | Fresh canonical context has no PR/current CI; live run 39087748686 is success on earlier 08d02c07 |
| Info, resolved R1 | QuickFiler/Resources/FolderBreadcrumb.js | Full script | Actual execution coverage now exists for full original/final script and changed/new behavior. | Retain both numeric baselines and native/raw reconciliation. | String assertions are now supplemented by executable behavior and measured source coverage. | issue-997-javascript-coverage-comparison.2026-10-18T01-54.md |
| Info, resolved R2 | Feature Markdown evidence | Full branch | Previous trailing whitespace/EOF findings corrected. | Retain full comparison rather than worktree-only check. | Full branch check exits 0 at current commit. | git diff --check origin/main...HEAD |
| Info, resolved R4 | New policy-audit artifact | Canonical validation | Numeric C#/JavaScript schema accepted by MCP. | Validate the remaining new audit artifacts independently. | Rejected prior artifacts were not relabeled as accepted. | validate_orchestration_artifacts policy-audit success in this review |

## Implementation Audit

Host state retains provider keys and known node occurrence identities separately from selected filing values. Optional QFC root binding validates archive-relative stems before mutation; unbound callers retain existing full-path contracts. Selection effects commit/notify/close once. Toggle effects change expansion without changing the destination. Collapse hides descendants without discarding committed identity; selector cancellation restores the original descendant snapshot.

Provider awaits occur outside state locks. Completion checks population generation, containing row reference, known node occurrence, node-request generation and current expansion before attachment. Negative tests cover invalid/unloaded/cross-row/out-of-root requests and late/reversed completions. Existing duplicate/store/score and legacy indexed aliases are preserved.

Recursive JavaScript controls render loaded immediate children, omit leaf toggles, stop click/Enter/Space propagation and send host-known row/node addresses. textContent renders labels without HTML interpretation. One resource-composed document is used by both hosts. Existing project registrations are exact; files remain within 500 lines.

No policy, dependency, coverage exclusion, CI or EFC production change appears in the branch. Four Node modules are test support/fixtures; source I/O is isolated in the outer adapter. The browser seam implements element relationships, attribute/class queries, listeners/bubbling, focus/scroll and native messaging without implementing production selection/render decisions.

## Test Quality Audit

MSTest/Moq/FluentAssertions coverage remains valid because C#/resource bytes did not change during remediation. Full independent ordered C# run passed 7504 tests, zero failures, with zero analyzer/nullable warnings/errors; 86.10% production line and 80.31% branch coverage. Changed C# lines rise from 97.57% to 99.09%; measured new files 98.70%; all 45 new measured methods >=90%.

JavaScript common fixture exercises existing contracts against real merge-base inline script and final script. Recursive fixtures assert three-level DOM/ARIA/message behavior, click/Enter/Space independence and committed child presentation. Baseline 51 tests, final 78 tests. Re-review replay passed all 78 with zero skipped/cancelled/failures. Normal/reversed/shuffled full fixture orders (seed 997) pass with fresh VMs.

Complete original JavaScript coverage: 314/314 source lines, 139/140 branches. Final: 347/347 lines,162/162 branches. Extraction-aware changed lines: old 10/10 to final 43/43; changed branches 4/4 to 25/25. All four new functions are fully covered. Native counts and LCOV records reconcile with raw V8; no source regions are excluded.

## Security / Correctness Checks

PASS for inspected root boundary enforcement, known-node identity authorization, strict JSON tokens, store/row occurrence separation, stale-request rejection, textContent use and stopped expansion-event propagation. Host behavior and browser-seam execution are verified. Live Outlook/WebView2 integration was not run and is not claimed.

## Research Log

Primary fresh summary read first, then exact appendix, complete feature scoping/plan, original and remediation evidence, prior findings, complete changed-language inventory and new test modules. Full production changes remain those already independently inspected in the initial review; remediation production diff is empty. Full current branch whitespace exits 0. Early workflow queried live: all seven jobs success on 08d02c07, not current final PR head. Canonical completed remediation plan also passes its plan validator.

## Verdict

REVIEW_STATUS: AWAITING_CI
Autonomous blocking findings: 0.
Open blocking delivery finding: R3; Remediability: awaiting_ci.
REMEDIATION_PLAN: NONE for this wait-only result.
Local review is acceptable for proceeding to PR/CI. Merge readiness remains pending final-head CI and orchestrator completion gates.
