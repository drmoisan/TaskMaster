# Feature Audit: EFC and QFC folder tree selection and nesting (Issue #1002)

- Review timestamp: 2026-10-22T10-39
- Reviewer: feature-review agent

## Scope and Baseline

- Branch: `bug/efc-qfc-folder-tree-selection-and-nesting-1002`, head `e96e65302de00d3b0f8174736c90a59f82525dc9`.
- Resolved base branch: `main` (`origin/main`), merge base `8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7` (supplied by the caller and confirmed with `git merge-base main HEAD`).
- Diff: 75 files (`git diff --numstat 8e4ad1ecb...HEAD`): 12 production C# files (2 new), `QuickFiler/Resources/FolderBreadcrumb.js`, 8 C# test files, 1 node suite, 4 project files, feature documents and evidence, 2 task-researcher memory notes.
- Work mode: `full-bug` (`- Work Mode: full-bug` in `issue.md`); AC source: `spec.md` only. No `user-story.md` exists or is required.
- PR context: `artifacts/pr_context.summary.txt` and `.appendix.txt`, generated for head `e96e65302` (current).
- Evidence basis: executor projections under `evidence/`, the canonical coverage artifact `artifacts/csharp/coverage.xml` (byte-identical to the executor final Cobertura), and reviewer reads and scans of the diff.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md`, `## Acceptance Criteria`. 26 checkbox items, AC1 to AC26, in four groups. All 26 were already checked off by the executor before this review.

| Group | Criteria |
|---|---|
| A: EFC recursive subfolder expansion and selection | AC1 to AC10 |
| B: EFC trash button selects the trash entry | AC11 to AC13 |
| C: QFC nested row affordance and indentation | AC14 to AC16 |
| D: Cross-cutting scope, quality and evidence | AC17 to AC26 |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 Codec contract | PASS | Codec diff registers both types and requires a non-empty string `nodeId`; `BreadcrumbMessageCodecIssue1002Tests` 8 of 8; `BreadcrumbMessageCodecTests` 26 of 26. |
| AC2 Row state | PASS | `BreadcrumbRow.Descendants.cs` read; `ClearDescendants` added at the four reset points in `BreadcrumbRow.cs`; `BreadcrumbDescendantNode.cs` absent from the diff; `BreadcrumbRowDescendantsIssue1002Tests` 24 of 24; `BreadcrumbRowStateTests` 22 of 22. |
| AC3 First-level expansion keeps keys | PASS | `.Arrows.cs` diff calls `SetLeafDescendants(children)` and still posts `subfolderResult` then the row render; `ExpandLeaf_KeyedChildren_RenderNodeAffordances` and `ExpandLeaf_CurrentRow_StillPostsSubfolderResultThenRender` pass. |
| AC4 Descendant toggle | PASS | `HandleDescendantToggleAsync` read: collapse without provider call, one fetch with the node key, one row render, no `subfolderResult`, no selection change; four named router tests pass. |
| AC5 Descendant activation is the filing destination | PASS | `ActivateDescendant` commits through `SelectHierarchyPath`; highlight applied only after root checks; `DescendantActivate_Grandchild_CommitsArchiveRelativeStem`, `..._ThenRowConfirmed_RaisesConfirmationOnce`, `..._OutsideArchiveRoot_LeavesSelectionAndHighlightUnchanged` pass. The descendant is marked in the posted render by attribute; its visual styling is code-review finding CR-B4, which the criterion text does not require. |
| AC6 Identity validation | PASS | Unknown row (existing `FindRow` path), unknown node, cross-row node and leaf toggle are logged and rejected; three named tests pass. |
| AC7 Descendant stale-completion guard | PASS | `ReferenceEquals(FindRow(row.RowId), row)` plus generation-checked `TryAttachDescendantChildren`; fault and cancellation call `TryAbandonDescendantExpand`; five barrier tests pass. |
| AC8 First-level stale-completion guard | PASS | Same-instance guard after the await in `ExpandLeafAsync`; `ExpandLeaf_CompletesAfterRebind_PostsNothingForReplacedRow` passes. |
| AC9 Recursive rendering | PASS | `AppendDescendants` read: identity, depth, name span, affordance after the name only with children, sibling nested container, HTML encoding, `aria-selected` on the selected row only, pinned prefix kept; 10 renderer tests and 15 existing renderer tests pass. |
| AC10 EFC bridge script contract | PASS | `BridgeJs` read: node branch precedes keyed, key-less and leaf branches; keyed branch precedes `renderedChildActivate`; Enter or Space on a node affordance returns before row confirmation; `.child` is a flex row. |
| AC11 Router preselection in the same bind | PASS | `SelectPreferredRow` runs before `DeliverDocument`, posts nothing, raises one change event; shares `TryGetRowSelection` with `SelectRow`; 7 trash router tests pass. |
| AC12 Router compatibility | PASS | Optional parameter; no-preference and absent-row tests pass; all existing router classes pass. |
| AC13 Controller end to end | PASS | `ApplyDeleteGestureAsync` awaits `BindBreadcrumbRowsAsync(_folderRows, TrashRowText)` with the null guard preserved; 4 controller tests and `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` pass. |
| AC14 Affordance after the name | PASS | JS diff moves `appendChild(name)` before the toggle; `MakeSubfolders_AppendsNameBeforeToggle` passes. |
| AC15 One indent step per level | PASS | `depth = depth || 0`, `data-depth`, `padding-left: 24 + 16 * depth`, recursive `depth + 1`; `render`, `makeRow`, FolderBreadcrumb.html untouched; nesting tests and pinned-string suites pass. |
| AC16 Node behavioural suite | PASS | `node-suites-run.2026-10-22T10-14.md`: 80 of 80 in normal, reversed and shuffled order. |
| AC17 Fail-before and pass-after | PASS | Six regression-testing projections present with expected exit codes. |
| AC18 Scope fences | PASS | Reviewer filter of the diff for every excluded file: empty; no arrow-key additions in the EFC router or bridge script. |
| AC19 Thread affinity | PASS | Reviewer grep: 0 `ConfigureAwait(false)` and 0 `lock (` in `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`; none added in `EfcFormController.Actions.cs`. |
| AC20 File size | PASS | Reviewer line counts: maximum 487 (test) and 471 (`BreadcrumbBridgeRouter.cs`); no near-limit test file in the diff. |
| AC21 Test policy and registration | PASS | Strict mocks 6 of 6, FluentAssertions only, no sleeps or temp files, Compile entries present; new names in the final executed list. |
| AC22 Existing tests | PASS | `final-toolchain-tests.2026-10-22T10-26.md` kept-green table: 0 failures; no existing test file in the diff. |
| AC23 Coverage | PASS | New files 100% line; every new method at least 90%; 292 of 292 changed lines covered; no exclusion added; repository 86.14% / 80.37% recorded against 80 / 75 and not lowered. (Whole-file sub-floor figures for three modified files are policy findings B-1 to B-3, outside this criterion's text.) |
| AC24 Toolchain | PASS | Loop pass 3 artifacts: format, analyzers, type check and tests, all exit 0, no file change. |
| AC25 Evidence form | PASS | No `.trx`, `.coverage`, `.xml` or Cobertura file in the diff; all evidence is Markdown under `evidence/`. |
| AC26 Manual Outlook check recorded | PASS | `manual-outlook-check.2026-10-22T10-17.md` states PENDING HUMAN INTERACTION and "Executed by an agent: no". |

## Summary

All 26 acceptance criteria PASS. The review verdict is nevertheless **REMEDIATION_REQUIRED** because of four blocking findings that sit outside the literal criterion texts: modified-file coverage below the CLAUDE.md floors (B-1 lowered by this change, B-2 and B-3 pre-existing) and the absent visual highlight for the selected EFC descendant (B-4), which would make the spec's manual validation step 3 fail. B-1, B-2 and B-4 are autonomous; B-3 requires a maintainer decision on the COM/VSTO/WinForms exemption or a seam follow-up.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md`
- Total AC items: 26
- Checked off (delivered): 26
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All 26 items were already `- [x]` in `spec.md` when this review began, and each is evaluated PASS above, so no check-off change was made and none was reverted. No criterion text was modified.
