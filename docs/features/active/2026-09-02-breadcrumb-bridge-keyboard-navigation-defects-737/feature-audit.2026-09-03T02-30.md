# Feature Audit — breadcrumb-bridge-keyboard-navigation-defects (Issue #737)

- Timestamp: 2026-09-03T02-30
- Work Mode: `full-bug` (from `issue.md` line 12)
- AC Source (per `acceptance-criteria-tracking`, `full-bug` -> `spec.md` only): `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`, `## Acceptance Criteria` section, 7 items.
- Baseline for comparison: `origin/main` at `b13d5b7b` (confirmed as the exact `git merge-base origin/main HEAD`).

Each AC below was independently re-verified against the actual diff and evidence artifacts rather than trusting the pre-existing `[x]` state.

## AC1 — Scroll-into-view (Finding #640)

> In `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`'s `BridgeJs` constant, the inbound message listener scrolls the current `.rowwrap.selected` element into view (`scrollIntoView({ block: 'nearest' })`) after a `render` or `subfolderResult` DOM update, addressing Finding 1 (#640).

**Verification**: Read `BreadcrumbDocumentAssets.cs` directly. Lines 141-142:
```
var scrollTarget = document.querySelector('.rowwrap.selected');
if (scrollTarget) { scrollTarget.scrollIntoView({ block: 'nearest' }); }
```
placed inside the `window.chrome.webview.addEventListener('message', ...)` callback, after the `render`/`subfolderResult` `if`/`else if` block. Confirmed present via `git diff origin/main...HEAD` (new lines, not present on `origin/main`). Independently confirmed via the new test `Issue737BridgeJsPostsRowSelectedOnEnterAndScrollsSelectedRowIntoView`, which passed in isolation (`evidence/qa-gates/qa-vstest-phase3-new-test.2026-09-02T00-00.md`, exit 0, 1/1 passed).

**Verdict: PASS.**

## AC2 — Enter-key binding (Finding #641)

> In the same `BridgeJs` constant, the `keydown` listener includes an `Enter` branch that posts `{ type: 'rowSelected', rowId: id }` using the same `.rowwrap.selected` lookup the arrow-key handler already uses, addressing Finding 2 (#641), and requires no new C#-side message type, codec branch, or router case.

**Verification**: Read `BreadcrumbDocumentAssets.cs` lines 102-108 — the `Enter` branch is present, uses `document.querySelector('.rowwrap.selected')` (same selector as the arrow-key branch at line 112), and posts `{ type: 'rowSelected', rowId: id }`. Confirmed the "no new C#-side plumbing" clause by reading `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (context-only file, not in the Write Set, not modified per the branch diff) and `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` / `BreadcrumbMessages.cs` (also not in the branch diff): the `RowSelected` message type, its `IsKnownInboundType` recognition, and the router's `case BreadcrumbMessageTypes.RowSelected: SelectRow(row); break;` all pre-exist unmodified.

**Verdict: PASS.**

## AC3 — New MSTest test for AC1/AC2 (Finding #640/#641 JS-content verification)

> A new MSTest test method in `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`, following the existing `Issue439...` string-containment precedent, asserts the rendered document (or the `BreadcrumbDocumentAssets.BridgeJs` constant directly) contains the Enter-triggered `rowSelected` post and the `scrollIntoView` call, with the JS-execution-harness limitation documented in the test's own comment or docstring.

**Verification**: `Issue737BridgeJsPostsRowSelectedOnEnterAndScrollsSelectedRowIntoView` (new `[TestMethod]`, `BreadcrumbHtmlRendererTests.cs`) reads `BreadcrumbDocumentAssets.BridgeJs` directly and asserts `.Should().Contain(...)` for `"e.key === 'Enter'"`, `"post({ type: 'rowSelected', rowId: id });"`, and `"scrollIntoView({ block: 'nearest' })"`. XML-doc `<summary>` explicitly states the JS-execution-harness limitation ("verifies the JS text is present and correctly shaped -- not that it executes correctly in a real WebView2/Chromium document"). Placed immediately after the precedent test `Issue439ActiveAncestorChildrenAndEmbeddedBridgeUseTypedStoppedActivation`, matching its style. Test execution confirmed passing (see AC1 evidence above).

**Verdict: PASS.**

## AC4 — Router test captures discarded results and asserts `RenderMessage` (Finding #693)

> In `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`, `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` captures both previously-discarded `ArrowAsync(router, "left")` results and asserts each parses to a `RenderMessage`, addressing Finding 3 (#693), without modifying the `ArrowAsync` helper signature or any shared provider-mock/router factory in the file.

**Verification**: Read `FolderBreadcrumbBridgeRouterTests.cs` lines 368-389 directly. The two previously-discarded `await ArrowAsync(router, "left")` calls are now captured as `firstPress`/`secondPress`, each asserted `.Should().ContainSingle()` then `BreadcrumbBridgeSerializer.Parse(...).Should().BeOfType<RenderMessage>()`. Diffed the full file against `origin/main`: only this one test method's body changed; `ArrowAsync` (helper), `PopulatedRouterAsync`, `ProviderMock`, `StemProviderMock`, and `ParentSubfolderProviderMock` are byte-identical to `origin/main`. Isolated test run passed (`evidence/qa-gates/qa-vstest-phase4-modified-test.2026-09-02T00-00.md`, exit 0, 1/1 passed).

**Verdict: PASS.**

## AC5 — #440 ancestor-walk contract preserved

> The fix for Finding 3 preserves the #440 ancestor-walk contract already documented in the test's in-code comment (two presses to reach the root on the three-segment fixture; `UnhandledArrowMessage` only on the third press), and is consistent with the sibling test `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition`.

**Verification**: The test's pre-existing Arrange comment (unchanged) states the three-segment/two-press-to-root contract. The new assertions are `RenderMessage` on presses 1 and 2, and the pre-existing third-press assertion (unchanged) is `UnhandledArrowMessage` with `Direction == Left` — exactly matching the documented contract, not weakening it. The sibling test `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition` was run in isolation as an independent cross-check (`evidence/qa-gates/qa-vstest-phase4-sibling-test.2026-09-02T00-00.md`, exit 0, 1/1 passed) and is itself unmodified by this branch.

**Verdict: PASS.**

## AC6 — No file outside the Write Set modified

> No file outside the Write Set is modified. In particular, no Qfc-pipeline file (`QuickFiler/Resources/FolderBreadcrumb.html`, `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`, `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`, `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`) and no #440 production logic in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` is altered.

**Verification**: `git diff --name-only origin/main...HEAD` (full three-dot diff against the confirmed merge-base, independently re-run in this audit — see `policy-audit.2026-09-03T02-30.md`) returns exactly: the 3 Write Set files + this feature's own `issue.md`/`spec.md`/`plan.*.md`/`research/*`/`evidence/**`. None of the five named Qfc-pipeline files, nor `BreadcrumbBridgeRouter.Arrows.cs`, appear anywhere in the diff. This audit additionally confirmed the two `origin/main` merges brought in only `Directory.Build.props` and `scripts/vscode/Invoke-MSTest*.ps1` (both already merged into `origin/main` via unrelated PRs #730/#733, zero diff against this branch), neither of which is a Qfc-pipeline or #440 production file.

**Verdict: PASS.**

## AC7 — Full C# toolchain passes cleanly, no coverage reduction on changed lines

> The full C# toolchain (csharpier format/check, analyzer rebuild, nullable rebuild, vstest with coverage) passes cleanly in a single pass, per CLAUDE.md and `.claude/rules/general-code-change.md`, with no reduction in coverage on changed lines.

**Verification**: All four stages independently re-checked against their evidence artifacts, each `EXIT_CODE: 0`:
- CSharpier format (scoped) + check (repo-wide, 1571 files): PASS, hashes independently re-verified against current on-disk files.
- Analyzer rebuild (`-EnableNETAnalyzers -EnforceCodeStyleInBuild`): PASS, 0 errors, warning count matches pre-existing baseline.
- Nullable rebuild (`-TreatWarningsAsErrors`): PASS, 0 errors, warning count matches pre-existing baseline.
- vstest with coverage (full-repo, 6956 tests): PASS, 0 failures; Cobertura coverage independently re-read from `coverage/breadcrumb-737-final.cobertura.xml`: line-rate 85.3867% (up from same-session baseline 85.3836%), branch-rate 79.4649%, both clearing the applicable floors; the sole modified production file has zero coverable lines (confirmed absent from the Cobertura class list).
- No coverage reduction on changed lines: confirmed via same-session baseline-vs-final delta (`lines-covered` 55139 -> 55141, `lines-valid` unchanged), and via the file-specific zero-coverable-lines basis for `BreadcrumbDocumentAssets.cs`.

**Verdict: PASS.**

## Acceptance Criteria Status

- Source: `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md`
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

All 7 items were already checked `[x]` in `spec.md` by the atomic-executor prior to this review; this audit independently re-verified each against the actual diff, evidence artifacts, and (for AC1-AC5) direct reads of the changed source, and confirms all 7 check-offs are accurate. No AC required reversion.

## Overall Feature-Audit Verdict

**PASS — ready to merge.** All 7 acceptance criteria are independently verified as delivered. No blocking findings in policy, code-quality, or acceptance-criteria review.
