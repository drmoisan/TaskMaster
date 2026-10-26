# Code Review: EFC and QFC arrow keys skip expanded subfolders (Issue #1007)

- Branch: `bug/efc-arrow-keys-skip-subfolders-1007`
- Head: `e804f5d6b`; merge base `3b9a1ebdd` (`main`, current `origin/main`)
- Review timestamp: 2026-10-26T00-14 (re-audit after remediation cycle 1)
- Previous review: `code-review.2026-10-25T23-25.md`
- Reviewer: feature-review agent
- Scope: the full branch diff `3b9a1ebdd..HEAD` (149 files: 25 production C# files, 1 page script, 1 page HTML resource, 26 test C# files, 3 node test files, 4 project files, feature documents and evidence, 1 promotion record). The remediation commit `e804f5d6b` was read hunk by hunk; the rest of the diff is unchanged since the first review and was re-read for the areas the cycle touches.

## Executive Summary

The cycle resolves the one blocking finding and both minor findings of the first review, with the smallest change that fits each.

- B-1: the selector list of the existing committed-subfolder rule becomes `.sub.selected, .sub.active`. The pending descendant now has the same background and foreground as a committed subfolder, and because both values are CSS variables redefined in the explicit dark theme and the `prefers-color-scheme: dark` block, the contrast holds in both themes. `FolderBreadcrumb.js` is unchanged. The new C# contract searches only the `<style>` block, matches the selector list entry exactly, and checks the `.sub.selected` rule first as a positive control, so neither the script's `"sub active"` string nor a missing style block can satisfy it. Its fail-before run failed on the `.sub.active` lookup alone.
- N-1: four characterization tests now reach the non-anchored restore paths in the EFC router, the QFC router and the state model, and the coordinator's `unhandledArrow` echo. Each test asserts the restored state and the negative (no subfolder query, no leaf expansion, committed folder and open state unchanged).
- N-2: the six names are rewritten to their asserted behaviour; summaries, attributes and bodies are unchanged.

No C# production file changed in the cycle. The design and correctness conclusions of the first review stand.

Verdict: **PASS (0 blocking, 0 minor, 5 informational)**.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Informational | `QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs` | `RuleDeclarations` comment | CR-I1: the comment says the helper returns the declaration block of the "innermost" matching rule; the loop keeps the last matching rule in document order. The test result is unaffected because the stylesheet has one rule for each selector. | Reword the comment to "the last rule" if the file is edited again. | Comments must stay synchronized with behaviour (CLAUDE.md C#6). | Source read of the added helper in `e804f5d6b`. |
| Informational | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs` | whole file | N-3 (carried): branch coverage 80.00% (48 of 60), up from 78.33%; line 93.44%. | Add tests for the null-entry and refused-expansion returns of `PendingArrowAsync` when the file is next touched. | CLAUDE.md asks new modules to target 90%; the line figure meets it and the branch figure is above the 75% floor. | Reviewer Cobertura recomputation. |
| Informational | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | whole file | N-4 (carried): 492 lines (446 at baseline). | Keep further session logic in the Tree partial. | 500-line ceiling still met. | `awk` line count. |
| Informational | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`; `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs` | `AppendSuggestionRow`; `ProjectRow` | N-5 (carried): after a zero-children fetch, a Left collapse re-shows `+` on an EFC row shortened at its active segment and on a QFC row. | Accept, or suppress on `HasNoLoadedChildren` regardless of expansion in a later item. | AC14 defence in depth; the `MapNode` source fix holds. | Source read (first review). |
| Informational | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`; `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs` | `ExpandLeafAsync`; `MoveToParent` | O-4 and O-5 (carried): the open-list Ctrl+Left row shape can persist after a different commit (displayed and filed folders agree); the pre-existing segment-activation race during an in-flight fetch is unchanged. | Document O-4 in the PR body; consider O-5 in a later item. | Pre-existing or consistent with S9. | Source read (first review). |

## Remediation Verification

### B-1 (resolved)

- `QuickFiler/Resources/FolderBreadcrumb.html` line 158: `.sub.selected, .sub.active { background: var(--bc-sel-back); color: var(--bc-sel-fore); }`. The rule sits inside `<style>`; `--bc-sel-back` and `--bc-sel-fore` are set at lines 14 to 15 (light), 26 to 27 (explicit dark) and 39 to 40 (`prefers-color-scheme: dark`).
- `FolderBreadcrumb.js` line 169 emits `sub active` for a pending node that is not the committed subfolder and `sub selected active` when it is; both now match a styled selector. `render` still removes the row's `active` class while the pending node owns the highlight, so exactly one element in the list is highlighted.
- The `.sub` base rule (lines 151 to 157) sets no background, and the combined rule follows it, so no later rule overrides the pending style.
- Test `Stylesheet_StylesPendingDescendantLikeSelectedSubfolder`: `Stylesheet()` extracts the text between `<style>` and `</style>`; `RuleDeclarations` matches `selectors { body }` without nested braces (the `@media` wrapper is skipped because its selector text cannot contain a brace) and compares each comma-separated selector after `Trim()` for equality. Fail-before: 1 of 9 failed with `Expected declarations not to be <null> because the QFC stylesheet must style the pending descendant (.sub.active).`; pass-after: 24 of 24 with `FolderBreadcrumbAssetContractTests`.
- The EFC page needed no change: its selected descendant is styled by `.child[aria-selected="true"]` (`BreadcrumbDocumentAssets.cs` lines 36 and 43).

### N-1 (resolved)

| Test | Path reached | Assertions |
|---|---|---|
| `LeafExpandToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChain` | EFC `HandleLeafToggleAsync` non-anchored restore, reached from inbound messages (double-click at segment 1, then `segmentActivate` at 0) | Beta segment rendered again; no fetch for Acme |
| `AffordanceToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChainWithoutQuery` | QFC `ToggleAsync` non-anchored restore through the public `CollapseAfter` contract | one render output; `CollapsedAfterIndex` null; `LeafExpanded` false; no subfolder query for Alpha or Projects |
| `RightArrow_OnRowCollapsedAwayFromActiveSegment_RestoresChain` | `BreadcrumbStateModel.Expand` non-anchored restore | handled; chain restored; leaf not expanded; active index 2 |
| `OpenUnhandledArrowLeft_OnExpandedPendingRow_RaisesUnhandledArrowFromRouterOutput` | coordinator `PublishRouterOutputs` and `RaiseSyntheticArrowKey` `UnhandledArrowMessage` arms | one `unhandled:Left` event; one synthetic key; list still open; committed folder unchanged |

Every N-1 body line moved from 0 hits to at least 1 (`coverage-comparison.2026-10-26T00-04.md`). The plan's reachability argument (D3) is sound: the EFC state is reachable from inbound messages, and the QFC states are reachable through a public method that accepts any non-leaf index, so keeping the branches and testing them is preferable to deleting them inside a cycle that must not change the state machine.

### N-2 (resolved)

The six renames in `BreadcrumbBridgeRouterTests.cs` and `BreadcrumbStateModelSequenceTests.cs` change one signature line each; the diff hunks show no body change. Each new name matches the summary and assertions (for example `LeftArrow_FromOpenLeafExpansion_CollapsesThenIsUnhandled`).

## Design and Correctness Review

The first review's conclusions on the shared visible order, the EFC router, the QFC session, router and coordinator, the AC14 source fix, the page scripts and the removed #440 behaviour are unchanged, because the cycle edited no C# production file and no page script. The cycle's test additions do not depend on internal state beyond what sibling tests in the same partial classes already use (`Harness`, `RouterAsync`, `ExpandAlphaAsync`, `ThreeSegmentAsync`, `ModelWithSuggestion`). The new partial files declare no member that the main partials declare and carry no `[TestClass]` of their own; MSTest discovers their methods through the main declaration.

## Best-Practice Checklist

| Area | Verdict | Note |
|---|---|---|
| Naming and documentation | PASS | N-2 resolved; CR-I1 comment wording is informational. |
| Error handling | PASS | Unchanged. |
| Async correctness | PASS | New async tests await every call; no blocking waits. |
| Thread safety | PASS | Unchanged. |
| File size | PASS | Largest changed code file 500 lines (unchanged); cycle-edited files at most 477. |
| Language level | PASS | QuickFiler.Test partial uses C# 7.3 syntax; UtilitiesCS.Test partial carries `#nullable enable` and uses tuple deconstruction already used by its main partial. |
| UI state has visible styling | PASS | B-1 resolved. |
| Test coverage of changed behaviour | PASS | N-1 resolved; N-3 informational. |

## Recommendation

No remediation is required. Before the pull request, regenerate the PR-context artifacts (O-1), let CI run on the PR head (O-2), and complete the manual Outlook verification recorded by the orchestrator (O-3).
