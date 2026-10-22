# Self-Review, remediation cycle 2 (#1002)

Timestamp: 2026-10-22T14-07
Command: review of the cycle-2 diff (git diff HEAD, git status --porcelain) and the branch diff from the merge base of origin/main and HEAD against the checklists below
EXIT_CODE: 0
Output Summary: no FAIL row. Cycle-2 edits: 4 modified source and test files, spec.md, the plan, cycle2 evidence files.

Template resolution (deviation): docs/features/templates/policy_audit/ does not exist in TaskMaster, so the checklists follow the cycle-1 self-review (self-review.2026-10-22T13-34.md): the policy-audit canonical sections, the code-review dimensions and the feature-audit items, restricted to the cycle-2 findings.

## Policy audit

| # | Item | Result | Evidence |
|---|---|---|---|
| PA-1 | Unit tests independent, isolated, deterministic, no sleeps or delays | PASS | the new tests build their own strict mocks; P3-T1 observes the fault synchronously; full parallel-default run 7599 passed, 0 failed (cycle2-tests) |
| PA-2 | No temporary files, no external services | PASS | in-memory mocks and constant text only |
| PA-3 | Files at or under 500 lines | PASS | cycle2-file-sizes (largest 368) |
| PA-4 | CSharpier, analyzers, nullable, tests, in order, single clean pass | PASS | cycle2-format, -format-check, -analyzers, -typecheck, -tests (all exit 0, 0 warnings) |
| PA-5 | Coverage floors not lowered; changed files at or above floor | PASS | cycle2-coverage-comparison (86.31% line, 80.59% branch; Actions.cs 92.86% / 81.25%; BreadcrumbDocumentAssets.cs has no instrumented line) |
| PA-6 | New tests cover the new behavior, fail-before observed | PASS | cycle2-delivered-wait-fail-before, cycle2-child-name-fail-before |
| PA-7 | Exclusions limited to the five ratified members; no coverage.config change | PASS | cycle2-scope-fences (5 attributes, empty coverage.config stat) |
| PA-8 | .claude/rules and .github/instructions unmodified; no raw trx or Cobertura in git | PASS | git status |
| PA-9 | No #nullable enable on the C# 7.3 QuickFiler.Test file | PASS | EfcFormControllerTests.Issue1002.cs unchanged header |

## Findings status

| Finding | Status | Evidence |
|---|---|---|
| R-1 | PASS: AC23 clause replaced, criterion still checked, Planner Amendments section appended, AC22 and AC24 unchanged, attribute count 5, empty coverage.config stat | cycle2-r1-verification, cycle2-r1-final |
| N-1 | PASS: comment above ActionCancelAsync names the EfcViewer UiSyncContext and Close; MessageBox count 5 to 4; attribute and body untouched | cycle2-n1-verification |
| N-3 | PASS: BoundaryErrorSink wired to TrySetException on the delivered source; new test failed before and passes after; 45 of 45 pass | cycle2-delivered-wait-fail-before, -pass-after |
| N-5 | PASS: subfolderResult handler wraps the display name in span.name; new test failed before and passes after; 29 of 29 renderer tests pass | cycle2-child-name-fail-before, -pass-after, -dispositions |

## Coordinator ruling (AC9)

| Item | Result | Evidence |
|---|---|---|
| AC9 stays authoritative and true: the key-less branch of BreadcrumbHtmlRenderer was deliberately left unedited because it has no production caller; the retained subfolderResult name-span wrap is the client-side transient render in BridgeJs, which AC9 (server-side key-less render) does not govern | PASS | git diff HEAD --stat of BreadcrumbHtmlRenderer.cs is empty; RenderRowFragment_KeylessChildren_RenderUnchanged passes unchanged |

## Observations

- The plan P4-T5 asserts an empty diff of BreadcrumbHtmlRenderer.cs against the merge base; that diff is non-empty because cycles 0 and 1 already changed the file, so the check was made against HEAD (no cycle-2 edit).
- BreadcrumbDocumentAssets.cs is not in the Cobertura document (string constants only), so its per-file floor is not evaluable.
