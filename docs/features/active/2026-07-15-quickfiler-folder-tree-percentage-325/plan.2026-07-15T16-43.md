# quickfiler-folder-tree-percentage — Atomic Implementation Plan

- **Issue:** #325
- **Epic:** `folder-tree-percentage-ui` (child feature 9003, wave 1, complexity C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-15T16-43
- **Status:** Preparation (preflight-only; no execution in this run)
- **Work Mode:** full-feature
- **Depends on:** `folder-probability-plumbing` (epic placeholder issue 9001) — merges into the integration branch before this plan executes
- **AC source:** `spec.md` (§ Acceptance Criteria) and `user-story.md` (§ Acceptance Criteria)
- **Authoritative current-state source:** `research/2026-07-15T16-43-quickfiler-folder-tree-percentage-research.md`

## Scope Summary

Deliver, in the QuickFiler folder dropdown on the single runtime-live `ItemViewer`: (a) expandable
tree nodes for folders that contain subfolders (plus expands, minus collapses; Right arrow expands
the highlighted node, Left arrow collapses it); and (b) each suggestion's prediction probability
right-aligned in whole-number percent (no decimals). The percentage is consumed verbatim from the
upstream 9001 `FolderSuggestion` contract; scores are not recomputed. Shared logic is factored into
four host-neutral, testable seams (`FolderNodeViewModel`, `PercentageFormatter`,
`FolderHierarchyBuilder`, `FolderTreeStateModel`) that are NOT coverage-exempt.

## Evidence Location Invariant

All evidence artifacts resolve to
`docs/features/active/2026-07-15-quickfiler-folder-tree-percentage-325/evidence/<kind>/`
(`baseline/`, `regression-testing/`, `qa-gates/`). No `artifacts/`-rooted evidence path is used.

## Coverage Targets

New host-neutral seams target line `>= 90%` (new-module). Repository floor for touched code is the
stricter of the two policy documents: line `>= 85%`, branch `>= 75%`. `ItemViewer`,
`*.Designer.cs`, and `KeyboardHandler` are COM/WinForms `[ExcludeFromCodeCoverage]` glue and are not
in the seam-coverage denominator.

## Non-Interference Constraint (9004)

This plan shares no files with sibling 9004. The following files MUST NOT be modified:
`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`,
`QuickFiler/Viewers/ItemViewer.WebViewThread.cs`, `QuickFiler/Viewers/WebView2CoreInitializer.cs`,
`QuickFiler/Viewers/IWebViewCoreInitializer.cs`, and the `NavigateToString`/WebView2 members on
`IItemViewer`/`ItemViewer`.

## Toolchain Commands (run in this exact order; restart from step 1 on any failure or auto-fix)

1. `dotnet tool run csharpier .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`

Both test projects are non-SDK `net4.8.1` (no glob compile); every new `.cs` file (production and
test) requires an explicit `<Compile Include>` entry in its `.csproj`.

---

### Phase 0 — Baseline Capture, Policy Reads, and 9001 Dependency Verification

- [ ] [P0-T1] Read the policy documents in policy-compliance order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read in an evidence artifact.
  - Acceptance: `evidence/baseline/phase0-instructions-read.md` exists containing `Timestamp:`, `Policy Order:`, and the explicit list of the four files read.
- [ ] [P0-T2] Capture the csharpier baseline by running `dotnet tool run csharpier . --check` at repo root and recording the result.
  - Acceptance: `evidence/baseline/baseline-csharpier.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of unformatted files).
- [ ] [P0-T3] Capture the .NET analyzer baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and recording the result.
  - Acceptance: `evidence/baseline/baseline-analyzers.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [ ] [P0-T4] Capture the nullable/type-check baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` and recording the result.
  - Acceptance: `evidence/baseline/baseline-nullable.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, nullable warning counts).
- [ ] [P0-T5] Capture the test + coverage baseline by running `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` and recording numeric coverage.
  - Acceptance: `evidence/baseline/baseline-tests.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including passed/failed test counts and numeric baseline line coverage and branch coverage percentages.
- [ ] [P0-T6] Verify at execution time that the upstream 9001 `FolderSuggestion` contract (folder identity + `[0,1]` probability) is present on the integration branch, and document the dependency; do not assume the type exists in the current preparation worktree.
  - Acceptance: `evidence/baseline/dependency-9001-verification.2026-07-15T16-43.md` exists recording the resolved type/namespace, the concrete member exposing `IReadOnlyList<FolderSuggestion>` (or equivalent), a grep confirmation that `FolderSuggestion` resolves in `UtilitiesCS`, and an explicit note that #325 consumes (does not implement) the 9001 contract present after 9001 merges. If the contract is absent at execution time, the outcome is BLOCKED (dependency not satisfied), not PASS.

### Phase 1 — Host-Neutral Seam: PercentageFormatter (Red-Then-Green)

- [ ] [P1-T1] Create `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` declaring `public static string Format(double probability)` (net48 plain static class, no record/init), and register it with an explicit `<Compile Include="OutlookObjects\Folder\PercentageFormatter.cs" />` in `UtilitiesCS/UtilitiesCS.csproj`.
  - Acceptance: `UtilitiesCS` compiles with the new file present and referenced in the `.csproj`.
- [ ] [P1-T2] Author `UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs` (MSTest + FluentAssertions) covering `0.4267 -> "43%"`, `1.0 -> "100%"`, `0.0 -> "0%"`, midpoint rounding away-from-zero, clamp of out-of-`[0,1]` input, and register `<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />` in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`; run the class and confirm it fails. `[expect-fail]`
  - Acceptance: `evidence/regression-testing/red-percentage-formatter.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` showing the tests fail against the unimplemented formatter.
- [ ] [P1-T3] Implement `PercentageFormatter.Format` as `percent = (int)Math.Round(Math.Clamp(p, 0.0, 1.0) * 100.0, MidpointRounding.AwayFromZero)` rendered as `percent + "%"`, and run `PercentageFormatterTests` to green.
  - Acceptance: `evidence/regression-testing/green-percentage-formatter.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing all `PercentageFormatterTests` pass.

### Phase 2 — Host-Neutral Seam: FolderNodeViewModel (Red-Then-Green)

- [ ] [P2-T1] Create `UtilitiesCS/OutlookObjects/Folder/FolderNodeViewModel.cs` as a net48 plain class exposing `FolderPath`, `DisplayName`, `Probability` (nullable `double?`), `Depth`, `HasChildren`, `Expanded`, derived `Glyph`, and formatted-percentage accessor (delegating to `PercentageFormatter`), and register `<Compile Include="OutlookObjects\Folder\FolderNodeViewModel.cs" />` in `UtilitiesCS/UtilitiesCS.csproj`.
  - Acceptance: `UtilitiesCS` compiles with the new file present and referenced in the `.csproj`.
- [ ] [P2-T2] Author `UtilitiesCS.Test/OutlookObjects/Folder/FolderNodeViewModelTests.cs` covering glyph bijection (INV4: `'+'` when `HasChildren && !Expanded`, `'-'` when `HasChildren && Expanded`, none when leaf), empty formatted percentage when `Probability` is null, and non-empty formatted percentage when set; register the `<Compile Include>`; run and confirm failure. `[expect-fail]`
  - Acceptance: `evidence/regression-testing/red-folder-node-viewmodel.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` showing the tests fail.
- [ ] [P2-T3] Implement `FolderNodeViewModel` behavior (glyph derivation, null-probability empty percentage) and run `FolderNodeViewModelTests` to green.
  - Acceptance: `evidence/regression-testing/green-folder-node-viewmodel.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing all `FolderNodeViewModelTests` pass.

### Phase 3 — Host-Neutral Seam: FolderHierarchyBuilder (Red-Then-Green)

- [ ] [P3-T1] Create `UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs` declaring a pure `public IReadOnlyList<TreeNode<FolderNodeViewModel>> Build(IReadOnlyList<FolderSuggestion> suggestions)` that reuses `UtilitiesCS/ReusableTypeClasses/Other/TreeNodeOfT.cs` `TreeNode<T>`, and register `<Compile Include="OutlookObjects\Folder\FolderHierarchyBuilder.cs" />` in `UtilitiesCS/UtilitiesCS.csproj`.
  - Acceptance: `UtilitiesCS` compiles with the new file present and referenced in the `.csproj`.
- [ ] [P3-T2] Author `UtilitiesCS.Test/OutlookObjects/Folder/FolderHierarchyBuilderTests.cs` covering path-segment splitting on `\`, find-or-add ancestor synthesis, probability attached only at the full-folder leaf, synthesized ancestors carrying no probability but `HasChildren=true`, sentinel/recents/"Trash to Delete" as depth-0 leaf rows with no probability, `DisplayName` = last segment, full path retained as node key; register the `<Compile Include>`; run and confirm failure. `[expect-fail]`
  - Acceptance: `evidence/regression-testing/red-folder-hierarchy-builder.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` showing the tests fail.
- [ ] [P3-T3] Implement `FolderHierarchyBuilder.Build` (segment splitting, find-or-add insertion, leaf probability placement, ancestor synthesis) and run `FolderHierarchyBuilderTests` to green.
  - Acceptance: `evidence/regression-testing/green-folder-hierarchy-builder.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing all `FolderHierarchyBuilderTests` pass.

### Phase 4 — Host-Neutral Seam: FolderTreeStateModel INV1-INV8 (Red-Then-Green)

- [ ] [P4-T1] Create `UtilitiesCS/OutlookObjects/Folder/FolderTreeStateModel.cs` declaring the pure transitions `Expand`, `Collapse`, `Toggle`, `Highlight`, `RightArrow`, `LeftArrow` and a `GetVisibleRows()` pre-order-DFS projection over a `TreeNode<FolderNodeViewModel>` forest, and register `<Compile Include="OutlookObjects\Folder\FolderTreeStateModel.cs" />` in `UtilitiesCS/UtilitiesCS.csproj`.
  - Acceptance: `UtilitiesCS` compiles with the new file present and referenced in the `.csproj`.
- [ ] [P4-T2] Author `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeStateModelTests.cs` exhaustively covering INV1 (no leaf expansion), INV2 (visibility iff all ancestors expanded; roots always visible), INV3 (single highlight), INV4 (glyph bijection), INV5 (descendant-state preservation across collapse/re-expand), INV6 (toggle involution), INV7 (indent equals `Depth`), INV8 (stable pre-order DFS order with descending-score then ordinal tie-break), plus arrow-key no-ops at root and leaf and the collapse/re-expand round-trip; register the `<Compile Include>`; run and confirm failure. `[expect-fail]`
  - Acceptance: `evidence/regression-testing/red-folder-tree-state-model.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` showing the INV1-INV8 tests fail.
- [ ] [P4-T3] Implement `FolderTreeStateModel` transitions and visible-row projection to satisfy INV1-INV8, then run `FolderTreeStateModelTests` to green.
  - Acceptance: `evidence/regression-testing/green-folder-tree-state-model.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing all INV1-INV8 tests pass.

### Phase 5 — ItemViewer Owner-Draw, Hit-Test, Keyboard, and Controller Injection (Consume 9001)

- [ ] [P5-T1] Add the additive intent member `void SetFolderSuggestions(IReadOnlyList<FolderSuggestion> suggestions)` to `QuickFiler/Viewers/IItemViewer.cs`, leaving `SetFolderItems(string[])`, `ClearFolderItems()`, `FolderContains(string)`, and `GetFolderItems()` unchanged.
  - Acceptance: `IItemViewer.cs` declares `SetFolderSuggestions` alongside the retained members; `QuickFiler` compiles; no WebView2/`NavigateToString` member is altered.
- [ ] [P5-T2] Implement `SetFolderSuggestions` on `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` to build the forest via `FolderHierarchyBuilder`, drive `FolderTreeStateModel`, project visible rows, and rebind `CboFolders.Items`; add the `CboFolders` owner-draw `DrawItem` paint (indent by `Depth`, `+`/`-` glyph, display name, right-aligned percentage via `PercentageFormatter` into an `e.Bounds.Right`-anchored rectangle) and the glyph mouse hit-test that toggles the node and re-projects.
  - Acceptance: `ItemViewer.FolderSearch.cs` implements `SetFolderSuggestions`, `DrawItem`, and glyph hit-test; selection still returns the full folder-path string; `QuickFiler` compiles.
- [ ] [P5-T3] Configure `CboFolders.DrawMode = DrawMode.OwnerDrawFixed` and wire the `DrawItem` handler on the runtime-live `ItemViewer` in `QuickFiler/Viewers/ItemViewer.Designer.cs` (or `ItemViewer.cs`), touching no other viewer variant.
  - Acceptance: `CboFolders` on `ItemViewer` is configured for owner-draw and its `DrawItem` event is bound; none of the nine dead design-time variants is modified.
- [ ] [P5-T4] Route the Right/Left arrow keys through `QuickFiler/Controllers/KeyboardHandler.cs` to `FolderTreeStateModel.RightArrow`/`LeftArrow` on the highlighted-row node, then re-project `CboFolders.Items`.
  - Acceptance: `KeyboardHandler.cs` invokes the state-model arrow transitions on the highlighted node; the transition logic itself remains in the unit-tested `FolderTreeStateModel`.
- [ ] [P5-T5] Inject suggestions from `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` by calling `_itemViewer.SetFolderSuggestions(...)` with folder identity + probability from the 9001 contract, while retaining the existing `SetFolderItems(string[])` calls for sentinels, recents, and "Trash to Delete".
  - Acceptance: `QfcItemController.FolderHandling.cs` calls `SetFolderSuggestions` with the 9001 suggestion list; existing `SetFolderItems(string[])` call sites are preserved; `QuickFiler` compiles.
- [ ] [P5-T6] Author `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs` (MSTest + Moq + FluentAssertions) verifying `QfcItemController` hands the identity+probability suggestions to `SetFolderSuggestions` via `Mock<IItemViewer>`, and that existing `SetFolderItems(string[])` expectations (sentinels, "Trash to Delete", index-1/predetermined selection) remain satisfied; register `<Compile Include="Controllers\QfcItemController.FolderSuggestionsTests.cs" />` in `QuickFiler.Test/QuickFiler.Test.csproj`; run to green.
  - Acceptance: `evidence/regression-testing/green-controller-injection.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing the new controller-injection tests pass and existing `SetFolderItems` expectations remain green.

### Phase 6 — Final QC Loop, Coverage Delta, and Non-Interference Verification

- [ ] [P6-T1] Run `dotnet tool run csharpier .` at repo root; if it reformats any file, restart the toolchain loop from this task.
  - Acceptance: `evidence/qa-gates/final-csharpier.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming no files required reformatting on the final pass.
- [ ] [P6-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  - Acceptance: `evidence/qa-gates/final-analyzers.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing build succeeded with zero analyzer errors.
- [ ] [P6-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  - Acceptance: `evidence/qa-gates/final-nullable.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing build succeeded with warnings treated as errors and zero nullable warnings.
- [ ] [P6-T4] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` and record numeric post-change coverage.
  - Acceptance: `evidence/qa-gates/final-tests.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` including all-tests-pass and numeric post-change line and branch coverage percentages.
- [ ] [P6-T5] Compare baseline coverage (P0-T5) against post-change coverage (P6-T4) and report the four new-seam module coverages (`PercentageFormatter`, `FolderNodeViewModel`, `FolderHierarchyBuilder`, `FolderTreeStateModel`) against the thresholds.
  - Acceptance: `evidence/qa-gates/coverage-delta.2026-07-15T16-43.md` exists reporting baseline line/branch coverage, post-change line/branch coverage, and per-seam new-code coverage, with an explicit pass/fail against line `>= 85%`, branch `>= 75%`, and new-module `>= 90%`. If any threshold is unmet the outcome is remediation-required, not PASS.
- [ ] [P6-T6] Verify non-interference with 9004 by confirming the diff for this feature modifies none of `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`, `QuickFiler/Viewers/ItemViewer.WebViewThread.cs`, `QuickFiler/Viewers/WebView2CoreInitializer.cs`, `QuickFiler/Viewers/IWebViewCoreInitializer.cs`, or the WebView2/`NavigateToString` members, and modifies none of the nine dead design-time viewer variants.
  - Acceptance: `evidence/qa-gates/non-interference-9004.2026-07-15T16-43.md` exists listing the `git diff --name-only` file set and confirming zero overlap with the 9004 body-render files and zero changes to the nine dead viewer variants.
