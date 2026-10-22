# Self-Review: Policy Audit, Code Review, and Feature Audit Checklists (#1002)

Timestamp: 2026-10-22T10-29
Command: review of `git diff $(git merge-base origin/main HEAD)` plus the untracked new files (`git status --porcelain --untracked-files=all`), against the checklists below
EXIT_CODE: 0
Output Summary: no FAIL row. Code and test diff: 15 tracked files changed (308 insertions, 43 deletions) plus 2 new production files, 8 new test files, and 1 new node suite.

Template resolution (deviation): `docs/features/templates/policy_audit/` does not exist in TaskMaster, and the template resolver named by `.claude/skills/policy-audit-template-usage/SKILL.md` (`mcp__drm-copilot__resolve_policy_audit_template_asset`) is not among the executor's tools. The policy-audit checklist below therefore uses that skill's canonical section list; the code-review and feature-audit checklists use the review dimensions those artifacts carry (correctness, design, error handling, tests, scope; acceptance criteria and evidence). The formal review artifacts remain the feature reviewer's to produce.

## Policy audit (canonical sections)

| # | Item | Result | Evidence |
|---|---|---|---|
| PA-1 | Executive summary: three defects fixed with fail-before and pass-after evidence | PASS | `efc-descendants-*`, `efc-trash-*`, `qfc-nesting-*` under regression-testing |
| PA-2 | General unit test policy: independent, isolated, fast, deterministic, readable | PASS | `csharp-isolation` (each class in a fresh process plus all in one process, 0 failures); TaskCompletionSource barriers only |
| PA-3 | General unit test policy: no temporary files, no external services, mocks for boundaries | PASS | `test-policy-audit` (0 banned-token matches; strict Moq mocks) |
| PA-4 | General code change policy: simplicity, separation of concerns, files under 500 lines | PASS | `file-sizes` (largest changed production file 471; largest new test file 487) |
| PA-5 | General code change policy: fail fast, project logging pattern | PASS | codec fails fast through `Fail`; router rejections use the existing `log.Error` pattern; provider faults logged at the I/O boundary |
| PA-6 | C# code change policy: CSharpier, analyzers, nullable | PASS | `final-toolchain-format`, `final-toolchain-analyzers`, `final-toolchain-typecheck` (loop pass 3, all exit 0, 0 warnings) |
| PA-7 | C# code change policy: `#nullable enable` on new production files; explicit Compile Include | PASS | both new production partials carry the directive; four project files register every new file |
| PA-8 | C# unit test policy: MSTest, Moq, FluentAssertions | PASS | `test-policy-audit` |
| PA-9 | `#nullable enable` on new test files | N/A (partial) | UtilitiesCS.Test files carry it; QuickFiler.Test compiles as C# 7.3, where the directive is a compile error, and none of its files carries it (spec: consistent with neighbors). Recorded in `efc-descendants-fail-before`. |
| PA-10 | Test coverage detail: new files and methods at least 90 percent, changed lines not reduced | PASS | `coverage-comparison` (all new files and methods 100 percent; changed lines 100 percent) |
| PA-11 | Repository coverage floors 80 line and 75 branch, not lowered | PASS | 86.10 to 86.14 percent line; 80.31 to 80.37 percent branch |
| PA-12 | Test execution metrics recorded from the trx | PASS | `final-toolchain-tests` (7579 passed, 0 failed) |
| PA-13 | Code quality checks: one clean four-step pass in CLAUDE.md order | PASS | loop pass 3 artifacts stamped 2026-10-22T10-23 and 10-26 |
| PA-14 | Gaps and exceptions documented | PASS | this file (Observations), `scope-fences` residue, `coverage-comparison` observation |
| PA-15 | No coverage exclusion added | PASS | `coverage-comparison` (coverage.config unchanged; 0 ExcludeFromCodeCoverage added) |
| PA-16 | Policy documents under `.claude/rules/` not modified | PASS | not in the diff |

## Code review

| # | Item | Result | Evidence |
|---|---|---|---|
| CR-1 | Descendant identity validated against the addressed row's own tree before any provider call or state change (I1) | PASS | `TryCollapseDescendant` / `TryBeginDescendantExpand` / `GetDescendantPath` search only the row's nodes; tests `DescendantRequest_UnknownNode_IsRejectedWithoutStateChange`, `DescendantRequest_NodeFromAnotherRow_IsRejected` |
| CR-2 | Stale completion: same row instance and request generation checked after every await (I2), including first-level `ExpandLeafAsync` | PASS | `BreadcrumbBridgeRouter.Descendants.cs`, `ExpandLeafAsync`; the seven stale-completion tests |
| CR-3 | Toggle never changes selection (I3) | PASS | `DescendantToggle_DoesNotChangeSelection` |
| CR-4 | Activation commits only through `SelectHierarchyPath`; rejection leaves selection and highlight unchanged (I4) | PASS | highlight applied immediately before `CommitSelection`; `DescendantActivate_OutsideArchiveRoot_LeavesSelectionAndHighlightUnchanged` |
| CR-5 | Trash selection is inside the delivered document, with no follow-up render (I5) | PASS | `SelectPreferredRow` runs before `DeliverDocument`; `BindRows_PreferredTrashRow_PostsNoFollowUpRender` |
| CR-6 | Thread affinity: no lock and no `ConfigureAwait(false)` in router files (I6) | PASS | `thread-affinity` |
| CR-7 | `SelectRow` value computation shared, not duplicated | PASS | `TryGetRowSelection` used by `SelectRow` and `SelectPreferredRow` |
| CR-8 | Async-void boundary unchanged: handlers log and do not throw new exception types | PASS | `OnHostMessageReceived` unchanged; provider faults caught in the handler |
| CR-9 | Delete-gesture faults still reported at the `BindBreadcrumbRowsAsync` boundary; null-router guard preserved | PASS | `ActionDeleteAsync_BindFault_IsReportedAtBindBoundary`, `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` |
| CR-10 | Rendering: names and paths HTML-encoded; pinned first-level prefix kept; key-less rows unchanged | PASS | `RenderRowFragment_DescendantNames_AreHtmlEncoded`, `RenderRowFragment_KeylessChildren_RenderUnchanged`, BreadcrumbHtmlRendererTests green |
| CR-11 | Bridge script branch order (node affordance, keyed child, key-less child, leaf affordance; Enter/Space before row confirmation) | PASS | the three `BridgeJs_` tests |
| CR-12 | `BreadcrumbDescendantNode` reused unchanged; QFC router files untouched | PASS | `scope-fences` |
| CR-13 | Public API additive and source-compatible (optional parameters, new members only) | PASS | every existing caller compiled unchanged (`seam-build-*`) |
| CR-14 | Unused or dead code introduced | PASS | none; `BindFolderRows` is still used by `BindSourceFolderRows` |

## Feature audit

| # | Item | Result | Evidence |
|---|---|---|---|
| FA-1 | AC1 to AC26 each traced to a task and evidence and checked off | PASS | `ac-reconciliation` (26 of 26) |
| FA-2 | Group A fail-before set failed before and passed after | PASS | `efc-descendants-fail-before`, `efc-descendants-pass-after` |
| FA-3 | Group B fail-before set failed before and passed after | PASS | `efc-trash-fail-before`, `efc-trash-pass-after` |
| FA-4 | Group C fail-before set failed before and passed after (C# and node) | PASS | `qfc-nesting-fail-before`, `qfc-nesting-pass-after` |
| FA-5 | Node suites pass in normal, reversed, and seeded shuffled order | PASS | `node-suites-run` (seed 1002; the shuffle coincided with the reversed order and is recorded as such) |
| FA-6 | Scope fences: no excluded file in the diff | PASS | `scope-fences`; the three orchestrator-owned paths committed before execution are recorded there |
| FA-7 | Evidence committed as Markdown projections without host paths | PASS | `evidence-form` |
| FA-8 | Manual Outlook check recorded as pending and not claimed as executed | PASS | `manual-outlook-check` |
| FA-9 | Existing tests kept green without edits to their assertions | PASS | `final-toolchain-tests` kept-green table; no existing test file is in the diff |

## Observations (not FAIL rows)

1. `EfcFormController.Breadcrumb.cs` whole-file line coverage fell from 67.83 to 64.46 percent, because the unchanged private `BindFolderRows` lost its only covered caller when the delete gesture moved to the awaited bind (P5-T2). Its remaining caller, `BindSourceFolderRows`, is untested at baseline and now. A reviewer may want a test for `BindSourceFolderRows`.
2. Two tests outside the Test Name Register were added during the Phase 9 loop to bring `SetLeafDescendants` and `SelectPreferredRow` to 100 percent: `SetLeafDescendants_NullChildren_Throws` and `BindRows_PreferredRowOutsideArchiveRoot_SelectsNothing`.
3. The P8-T5 isolation runs used the class set before those two tests were added; both tests passed in the final full run.
4. One Group A test expectation was corrected during P3-T10 (grandchild depth attribute 1, not 2), recorded in `efc-descendants-pass-after`.
5. `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs` is 487 lines; new tests belong in a new file.
