# Feature Audit: EFC and QFC folder tree selection and nesting (Issue #1002)

- Review timestamp: 2026-10-22T13-37 (re-audit after remediation cycle 1; prior audit `feature-audit.2026-10-22T10-39.md`)
- Reviewer: feature-review agent

## Scope and Baseline

- Branch: `bug/efc-qfc-folder-tree-selection-and-nesting-1002`, head `b72ac1dbbfdb6b45d01a14dde9e91428a6bf41ac`.
- Resolved base branch: `main` (`origin/main`), merge base `8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7` (supplied by the caller and confirmed with `git merge-base main HEAD` and `git merge-base origin/main HEAD`).
- Diff: 105 files (`git diff --numstat 8e4ad1ecb...HEAD`): 12 production C# files (2 new), `QuickFiler/Resources/FolderBreadcrumb.js`, 9 C# test files, 1 node suite, 4 project files, feature documents and evidence, 2 task-researcher and 2 feature-review memory notes.
- Work mode: `full-bug` (`- Work Mode: full-bug` in `issue.md`); AC source: `spec.md` only. No `user-story.md` is required.
- PR context: `artifacts/pr_context.summary.txt` and `.appendix.txt`, generated for head `b72ac1dbb` (current).
- Evidence basis: executor projections under `evidence/`, the canonical coverage artifact `artifacts/csharp/coverage.xml` (byte-identical to the cycle-1 run Cobertura), the cycle-1 trx counters and test outcomes, and reviewer reads and scans of the diff.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md`, `## Acceptance Criteria`. 26 checkbox items, AC1 to AC26, in four groups. All 26 are checked `[x]` at head.

| Group | Criteria |
|---|---|
| A: EFC recursive subfolder expansion and selection | AC1 to AC10 |
| B: EFC trash button selects the trash entry | AC11 to AC13 |
| C: QFC nested row affordance and indentation | AC14 to AC16 |
| D: Cross-cutting scope, quality and evidence | AC17 to AC26 |

## Acceptance Criteria Evaluation

Cycle 1 changed no behaviour covered by AC1 to AC19 other than adding styling and tests; those verdicts are re-confirmed against the head diff and the cycle-1 test run.

| AC | Verdict | Evidence |
|---|---|---|
| AC1 Codec contract | PASS | Codec unchanged in cycle 1; `BreadcrumbMessageCodecIssue1002Tests` pass in the 7597-test run. |
| AC2 Row state | PASS | Unchanged in cycle 1; `BreadcrumbDescendantNode.cs` absent from the diff. |
| AC3 First-level expansion keeps keys | PASS | Unchanged; `BreadcrumbBridgeRouter.Arrows.cs:201` calls `SetLeafDescendants`. |
| AC4 Descendant toggle | PASS | Unchanged; router tests pass. |
| AC5 Descendant activation is the filing destination | PASS | Unchanged commit path; the selected descendant is now also visibly highlighted by the cycle-1 `.child[aria-selected="true"]` rules (cycle-0 CR-B4 resolved). |
| AC6 Identity validation | PASS | Unchanged; tests pass. |
| AC7 Descendant stale-completion guard | PASS | Unchanged; barrier tests pass. |
| AC8 First-level stale-completion guard | PASS | Unchanged. |
| AC9 Recursive rendering | PASS | Renderer unchanged; selection remains an attribute, now targeted by an attribute selector rather than a class. |
| AC10 EFC bridge script contract | PASS | `BridgeJs` unchanged; `.child` remains a flex row (`BaseCss_Child_IsFlexRow` and `BaseCss_ChildName_CarriesEllipsisRule` pass). |
| AC11 Router preselection in the same bind | PASS | Unchanged; trash router tests pass. |
| AC12 Router compatibility | PASS | Unchanged. |
| AC13 Controller end to end | PASS | `ApplyDeleteGestureAsync` unchanged; null-viewer arm now tested; controller tests pass. |
| AC14 Affordance after the name | PASS | `FolderBreadcrumb.js` unchanged in cycle 1. |
| AC15 One indent step per level | PASS | Unchanged. |
| AC16 Node behavioural suite | PASS | `node-suites-run.2026-10-22T10-14.md`; no JavaScript change since. |
| AC17 Fail-before and pass-after | PASS | Six cycle-0 projections present; cycle 1 adds `css-highlight-fail-before` and `css-highlight-pass-after`. |
| AC18 Scope fences | PASS | Reviewer filter of the diff for every excluded file: empty; `evidence/qa-gates/scope-fences-remediation.2026-10-22T13-33.md`. |
| AC19 Thread affinity | PASS | Reviewer grep: 0 `ConfigureAwait(false)` and 0 `lock (` in `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`; cycle 1 adds none to `EfcFormController.Actions.cs`. |
| AC20 File size | PASS | Reviewer counts: maximum 487 (test) and 471 (`BreadcrumbBridgeRouter.cs`); none of the seven near-limit test files in Constraints is in the diff. |
| AC21 Test policy and registration | PASS | Cycle-1 tests use MSTest, FluentAssertions and the strict-mock fixture, no sleeps or files; `BreadcrumbMessagesIssue1002Tests.cs` registered in `UtilitiesCS.Test.csproj`; all 18 cycle-1 test names `outcome="Passed"` in the final trx. |
| AC22 Existing tests | PASS | 7597 of 7597 passed; no existing test file in the diff. |
| AC23 Coverage | FAIL | Coverage clauses are met (new files 100% line; every changed instrumented line covered; repository 86.30% / 80.58% recorded against 80 / 75 and not lowered in `coverage-comparison.2026-10-22T13-33.md`). The clause "no coverage exclusion is added" is not met as written: the diff adds five member-level `[ExcludeFromCodeCoverage]` attributes. The exemption is maintainer-ratified (B-3 decision, 2026-10-22T10:50Z), but AC23 was not amended to record it (finding R-1). |
| AC24 Toolchain | PASS | Cycle-1 complete pass in CLAUDE.md order with CLAUDE.md commands: `remediation-format`, `remediation-format-check`, `remediation-analyzers`, `remediation-typecheck`, `remediation-tests` (all 2026-10-22T13-32, exit 0, no file change on the last format run). The cycle-0 `final-toolchain` projections remain for the cycle-0 tree. |
| AC25 Evidence form | PASS | No `.trx`, `.coverage`, `.xml` or Cobertura file in the diff; all evidence is Markdown under `evidence/`. |
| AC26 Manual Outlook check recorded | PASS | `manual-outlook-check.2026-10-22T10-17.md` states PENDING HUMAN INTERACTION and "Executed by an agent: no". |

## Summary

25 of 26 acceptance criteria PASS; AC23 FAILS on its literal "no coverage exclusion is added" clause. All four cycle-0 blocking findings (B-1 to B-4) are resolved and verified against the canonical coverage artifact and the cycle-1 test run. The verdict is **REMEDIATION_REQUIRED** with one blocking finding, R-1 (autonomous): amend AC23 in `spec.md` to record the maintainer-ratified member-level exemption, after which AC23 can be re-evaluated PASS without any code change.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md`
- Total AC items: 26
- Checked off (delivered): 26
- Remaining (unchecked): 0
- Items remaining: none unchecked; AC23 is checked but evaluated FAIL in this review pending the R-1 amendment.

## Acceptance Criteria Check-off

All 26 items were already `- [x]` in `spec.md` when this review began. 25 are evaluated PASS, so no check-off change was needed for them. AC23 is evaluated FAIL; this review did not modify `spec.md` (no box changed, no criterion text changed). The remediation inputs direct the AC23 text amendment, after which the next review re-evaluates the criterion.
