# Self-Review, remediation cycle 1 (#1002)

Timestamp: 2026-10-22T13-34
Command: review of the branch diff from the merge base of origin/main and HEAD plus the untracked cycle-1 files (git status --porcelain), against the checklists below
EXIT_CODE: 0
Output Summary: no FAIL row. Cycle-1 edits: 6 modified tracked source files, 1 new test file, evidence files.

Template resolution (deviation): `docs/features/templates/policy_audit/` does not exist in TaskMaster and the template resolver tool is not among the executor's tools, so the checklists follow the first-cycle self-review (`self-review.2026-10-22T10-29.md`): the policy-audit canonical sections, the code-review dimensions and the feature-audit items, restricted to the cycle-1 findings.

## Policy audit

| # | Item | Result | Evidence |
|---|---|---|---|
| PA-1 | Unit test policy: independent, isolated, deterministic, no sleeps or delays | PASS | new tests use strict mocks and a TaskCompletionSource; full parallel-default run 7597 passed, 0 failed (remediation-tests) |
| PA-2 | No temporary files, no external services | PASS | tests build in-memory mocks only |
| PA-3 | Files at or under 500 lines | PASS | file-sizes-remediation (largest 343) |
| PA-4 | CSharpier, analyzers, nullable, tests, in order, single clean pass | PASS | remediation-format, -format-check, -analyzers, -typecheck, -tests (all exit 0, 0 warnings) |
| PA-5 | Coverage floors 80 line / 75 branch not lowered; changed files at or above floor | PASS | coverage-comparison (86.30% / 80.58%) |
| PA-6 | New tests target full coverage of new code | PASS | b1/b2 verification; 8+7+3 new tests pass |
| PA-7 | Exclusions limited to ratified members; none whole-file; no coverage.config change | PASS | scope-fences-remediation (5 attributes on the named members) |
| PA-8 | `.claude/rules/` and `.github/instructions/` unmodified; no raw trx or Cobertura in git | PASS | git status |
| PA-9 | `#nullable enable` on the new UtilitiesCS.Test file; none on C# 7.3 QuickFiler.Test file | PASS | BreadcrumbMessagesIssue1002Tests.cs; EfcFormControllerTests.Issue1002.cs |

## Code review

| # | Item | Result | Evidence |
|---|---|---|---|
| CR-1 | `.child` flex container keeps layout; ellipsis moved to `.child .name`; flex test unchanged | PASS | BaseCss_ChildName_CarriesEllipsisRule, BaseCss_Child_IsFlexRow |
| CR-2 | Keyless leaf-child and subfolderResult emitters render bare text without `.name` (observation, out of scope) | N/A | child-name-markup-check |
| CR-3 | Selected descendant highlight via attribute selector, no new class (AC9 preserved) | PASS | Light/DarkThemeCss tests, b4-grep |
| CR-4 | Null-viewer arm of ApplyDeleteGestureAsync tested | PASS | ApplyDeleteGestureAsync_NullViewer_RetainsTrashRowAndDoesNotBind |
| CR-5 | Production change limited to attributes and CSS constants | PASS | diff of QuickFiler/Controllers shows attributes, comments and using directives only (plus formatter modifier reorder on ActionOkAsync) |
| CR-6 | Attribute form compiles on net48 | PASS | exclude-attribute-form (COMMENT form, CS0246 for Justification) |

## Findings status

| Finding | Status | Evidence |
|---|---|---|
| B-1 | PASS: Breadcrumb.cs 93/101 lines, 34/42 branches (thresholds 84 and 24) | b1-per-file-coverage |
| B-2 | PASS: BreadcrumbMessages.cs seven guard lines 2/2, 14/14 branches | b2-per-file-coverage |
| B-3 | PASS: exemption ratified by the maintainer, member-level attributes; Actions.cs 92.86% / 81.25%, Breadcrumb.cs 92.08% / 80.95% | b3-per-file-coverage |
| B-4 | PASS: 2 aria-selected rules | b4-grep |
| CR-1 | PASS | css-highlight-pass-after |
| CR-4 | PASS | efc-bind-tests-pass |

## Observations

- The plan states 9 expected Phase 1 tests and 19 new tests; the Phase 1 task list defines 8 tests (P1-T6 is two methods), so 8 + 7 + 3 = 18 new tests and the pass count rose from 7579 to 7597.
