# Feature Audit: EFC and QFC folder tree selection and nesting (Issue #1002)

- Review timestamp: 2026-10-22T14-10 (re-audit after remediation cycle 2; prior audits `feature-audit.2026-10-22T10-39.md` and `feature-audit.2026-10-22T13-37.md`)
- Reviewer: feature-review agent

## Scope and Baseline

- Branch: `bug/efc-qfc-folder-tree-selection-and-nesting-1002`, head `54b36112abf0a4ac26384226694b02520842061b`.
- Resolved base branch: `main` (`origin/main`), merge base `8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7` (supplied by the caller and confirmed with `git merge-base main HEAD` and `git merge-base origin/main HEAD`).
- Diff: 136 files (`git diff --stat 8e4ad1ecb...HEAD`): 12 production C# files (2 new), `QuickFiler/Resources/FolderBreadcrumb.js`, 9 C# test files, 1 node suite, 4 project files, feature documents and evidence, and agent memory notes.
- Work mode: `full-bug` (`- Work Mode: full-bug` in `issue.md`); AC source: `spec.md` only. No `user-story.md` is required.
- PR context: `artifacts/pr_context.summary.txt` and `.appendix.txt`, generated for head `54b36112a` (current).
- Evidence basis: executor projections under `evidence/`, the canonical coverage artifact `artifacts/csharp/coverage.xml` (cycle-2 run, written after the trx and after every cycle-2 source edit), the cycle-2 trx counters and test outcomes, and reviewer reads and scans of the diff.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md`, `## Acceptance Criteria`. 26 checkbox items, AC1 to AC26, in four groups. All 26 are checked `[x]` at head. AC23 text was amended in cycle 2 (recorded under `## Planner Amendments`, 2026-10-22).

| Group | Criteria |
|---|---|
| A: EFC recursive subfolder expansion and selection | AC1 to AC10 |
| B: EFC trash button selects the trash entry | AC11 to AC13 |
| C: QFC nested row affordance and indentation | AC14 to AC16 |
| D: Cross-cutting scope, quality and evidence | AC17 to AC26 |

## Acceptance Criteria Evaluation

Cycle 2 changed production behaviour only in the transient `subfolderResult` render of the EFC bridge script; all other verdicts are re-confirmed against the head diff and the cycle-2 test run (7599 of 7599 passed).

| AC | Verdict | Evidence |
|---|---|---|
| AC1 Codec contract | PASS | Codec unchanged since cycle 0; `BreadcrumbMessageCodecIssue1002Tests` pass in the 7599-test run. |
| AC2 Row state | PASS | Unchanged; `BreadcrumbDescendantNode.cs` absent from the diff. |
| AC3 First-level expansion keeps keys | PASS | Router unchanged; the message sequence (`subfolderResult` then row render) is unchanged. Cycle 2 changes only how the client draws the transient `subfolderResult` children (name in a `span.name`), not the message sequence. |
| AC4 Descendant toggle | PASS | Unchanged; router tests pass. |
| AC5 Descendant activation is the filing destination | PASS | Unchanged; tests pass. |
| AC6 Identity validation | PASS | Unchanged; tests pass. |
| AC7 Descendant stale-completion guard | PASS | Unchanged; barrier tests pass. |
| AC8 First-level stale-completion guard | PASS | Unchanged. |
| AC9 Recursive rendering | PASS | `BreadcrumbHtmlRenderer.cs` has no cycle-2 change; `RenderRowFragment_KeylessChildren_RenderUnchanged` passes with its assertions unchanged, so key-less children still render exactly as before. The orchestrator AC9 ruling (remediation plan 2026-10-22T13-43) confines the N-5 wrap to the bridge script's client-side render, which AC9 does not govern. |
| AC10 EFC bridge script contract | PASS | The branch-order assertions of AC10 are untouched by the `subfolderResult` edit (it is in the message listener, not the click or keydown handlers); `BridgeJs_*` ordering tests and `BaseCss_Child_IsFlexRow` pass. |
| AC11 Router preselection in the same bind | PASS | Unchanged; trash router tests pass. |
| AC12 Router compatibility | PASS | Unchanged. |
| AC13 Controller end to end | PASS | `ApplyDeleteGestureAsync` unchanged; controller tests pass. |
| AC14 Affordance after the name | PASS | `FolderBreadcrumb.js` unchanged since cycle 0. |
| AC15 One indent step per level | PASS | Unchanged. |
| AC16 Node behavioural suite | PASS | `node-suites-run.2026-10-22T10-14.md`; no JavaScript asset change since. |
| AC17 Fail-before and pass-after | PASS | Six cycle-0 projections present; cycles 1 and 2 add their own fail-before and pass-after projections. |
| AC18 Scope fences | PASS | Reviewer filter of the diff for every excluded file: empty; `evidence/qa-gates/cycle2-scope-fences.2026-10-22T14-06.md`. No arrow-key handling added. |
| AC19 Thread affinity | PASS | Reviewer grep: 0 `ConfigureAwait(false)` and 0 `lock (` in `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`; cycle 2 changed only a comment in `EfcFormController.Actions.cs`. |
| AC20 File size | PASS | Reviewer counts: maximum 487 (test) and 471 (`BreadcrumbBridgeRouter.cs`); cycle-2 files 339, 368, 185, 206; no near-limit test file from Constraints is in the diff. |
| AC21 Test policy and registration | PASS | Both cycle-2 tests use MSTest and FluentAssertions, the strict-mock fixture where mocks are needed, no sleeps, delays or files; both test names `outcome="Passed"` in the final trx. |
| AC22 Existing tests | PASS | 7599 of 7599 passed; no pre-existing test file is in the diff, and the cycle-2 helper change touches only the new `Issue1002` partial. |
| AC23 Coverage | PASS | New files 100% line (82/82, 63/63); every instrumented changed line covered; the only exclusions added are the five member-level attributes on the five members named in the amended text (reviewer count 5), with no `coverage.config` entry and no class-level exclusion; repository 86.31% / 80.59% recorded against 80 / 75 and not lowered in `cycle2-coverage-comparison.2026-10-22T14-06.md` (and against the branch baseline `baseline-coverage.2026-10-22T09-54.md`, 86.10% / 80.31%). Cycle-1 R-1 resolved. |
| AC24 Toolchain | PASS | Cycle-2 complete pass in CLAUDE.md order with CLAUDE.md commands: `cycle2-format`, `cycle2-format-check`, `cycle2-analyzers`, `cycle2-typecheck`, `cycle2-tests` (all 2026-10-22T14-06, exit 0, 0 warnings). |
| AC25 Evidence form | PASS | No `.trx`, `.coverage`, `.xml` or Cobertura file in the diff; all evidence is Markdown under `evidence/`. |
| AC26 Manual Outlook check recorded | PASS | `manual-outlook-check.2026-10-22T10-17.md` states PENDING HUMAN INTERACTION and "Executed by an agent: no". |

## Summary

26 of 26 acceptance criteria PASS. The cycle-1 blocking finding R-1 is resolved by the AC23 amendment and its dated planner-amendment record, and the optional items N-1, N-3 and N-5 were delivered with fail-before evidence for the two behavioural ones. No blocking finding remains; the verdict is **PASS**. The manual Outlook check (AC26) remains a non-blocking human step for the maintainer.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md`
- Total AC items: 26
- Checked off (delivered): 26
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All 26 items were already `- [x]` in `spec.md` when this review began, and all 26 are evaluated PASS, so no check-off change was needed. This review did not modify `spec.md`.
