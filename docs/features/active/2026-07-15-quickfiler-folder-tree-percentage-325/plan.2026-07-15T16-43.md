# quickfiler-folder-tree-percentage — Atomic Implementation Plan

- **Issue:** #325
- **Epic:** `folder-tree-percentage-ui` (child feature 9003, wave 1, complexity C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-15T16-43
- **Status:** Preparation (preflight-only; no execution in this run)
- **Work Mode:** full-feature
- **Depends on:** `folder-probability-plumbing` (epic child 9001; delivered by issue #324, merged via PR #333 onto this integration branch)
- **AC source:** `spec.md` (§ Acceptance Criteria) and `user-story.md` (§ Acceptance Criteria)
- **Authoritative current-state source:** `research/2026-07-15T16-43-quickfiler-folder-tree-percentage-research.md`

## Scope Summary

Deliver, in the QuickFiler folder dropdown on the single runtime-live `ItemViewer`: (a) expandable
tree nodes for folders that contain subfolders (plus expands, minus collapses; Right arrow expands
the highlighted node, Left arrow collapses it); and (b) each suggestion's prediction probability
right-aligned in whole-number percent (no decimals). The percentage is consumed verbatim from the
upstream `folder-probability-plumbing` contract (concrete types `FolderScore`, `FolderRow`, and
`FolderPredictor.FolderRowArray`/`FindFolderRows` in `UtilitiesCS`); scores are not recomputed.
Shared logic is factored into four host-neutral, testable seams (`FolderNodeViewModel`,
`PercentageFormatter`, `FolderHierarchyBuilder`, `FolderTreeStateModel`) that are NOT
coverage-exempt.

## Consumed Upstream Contract (concrete — retargeted from the hypothetical `FolderSuggestion`)

The plan and spec were written against a hypothetical upstream type `FolderSuggestion` with
`FolderPath` + `Probability` and a `GetFolderSuggestions()` member. That type does not exist. The
merged dependency delivered a richer, unified row model. This plan is retargeted to the concrete
contract, exactly as the spec anticipated ("#325 plans against 'folder identity plus its
probability' and adapts to the concrete member at epic execution time"). This is an adaptation, not
a re-scope.

Concrete contract (all in namespace `UtilitiesCS`; consumed, not implemented, by #325):

- `UtilitiesCS/OutlookObjects/Folder/FolderScore.cs` — `public readonly struct FolderScore`
  (`FolderScore(string folderPath, long score, double probability)`; `FolderPath` scoring key;
  `Score` raw unbounded ranking score; `Probability` max-normalized `[0,1]` relative display value,
  `Score/TopScore`, `0` when the top score is `0`).
- `UtilitiesCS/OutlookObjects/Folder/FolderRow.cs` — `public enum FolderRowKind { Separator,
  SearchResult, Suggestion, Recent }` and `public readonly struct FolderRow`
  (`FolderRow(string text, FolderRowKind kind, FolderScore? score)`; `Text` is the exact legacy
  string at this position; `Score` is non-null ONLY for `Kind == Suggestion`, null for
  `Separator`/`SearchResult`/`Recent`).
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — `public FolderRow[] FolderRowArray`
  (ordered: SUGGESTIONS separator, top-5 `Suggestion` rows with `FolderScore`, RECENT SELECTIONS
  separator, `Recent` rows) and `public FolderRow[] FindFolderRows(string searchString, object
  objItem, bool reloadCTFStagingFiles = true, List<string> emailSearchRoots = null, bool
  recalcSuggestions = false, IEnumerable<(string root, string excludedFolder, bool excludeChildren)>
  exclusions = null)` (ordered: SEARCH RESULTS separator + `SearchResult` rows, then SUGGESTIONS
  separator + `Suggestion` rows with `FolderScore`, then RECENT SELECTIONS separator + `Recent`
  rows).

Retargeting decisions encoded by this plan:

1. **Probability source** is `FolderScore.Probability` (a `double` in `[0,1]`). `PercentageFormatter`
   is unchanged in intent (`Format(double) : string`, `double -> "NN%"`) and consumes
   `FolderScore.Probability`.
2. **Presentation input** is `FolderRow[]` (from `FolderPredictor.FolderRowArray` /
   `FindFolderRows`), NOT a flat `FolderSuggestion` list. `FolderHierarchyBuilder` accepts
   `Build(IReadOnlyList<FolderRow> rows)`.
3. **Row classification** is carried by the contract: `Kind == Suggestion` rows (with non-null
   `Score`) are the ONLY rows that carry a probability. The builder splits `FolderScore.FolderPath`
   (equivalently `FolderRow.Text`) on `\`, attaches the probability at the full-folder leaf, and
   synthesizes ancestors with no probability. `Separator`, `SearchResult`, and `Recent` rows carry
   no `FolderScore` and render as depth-0 leaf rows with an empty percentage field, preserving their
   `Text` verbatim and their order. This REPLACES the earlier design that routed sentinels/recents
   through a separate `SetFolderItems(string[])` path — the `FolderRow[]` contract already
   classifies them.
4. **`SetFolderItems(string[])` retention (explicit decision):** the existing
   `SetFolderItems(string[])`, `ClearFolderItems()`, `FolderContains(string)`, and
   `GetFolderItems()` members are RETAINED unchanged as additive/back-compat seams because live
   controller call sites still require them: the "Trash to Delete" append
   (`QfcItemController.MailActions.cs:206` and `:218`), the predetermined/index selection path
   (`QfcItemController.FolderHandling.cs:176-179`), and the folder-list reads
   (`QfcItemController.MailActions.cs:31,40`). The new `SetFolderSuggestions(IReadOnlyList<FolderRow>)`
   member is additive and becomes the suggestion-population path; it does not replace these members.
5. **`FolderNodeViewModel.Probability`** stays nullable `double?`, sourced from
   `FolderRow.Score?.Probability` (null when `Score` is null).
6. **No WebView2/`NavigateToString` member is altered** (9004 non-interference).

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
test) requires an explicit `<Compile Include>` entry in its `.csproj`. net48 constraints apply: no
`record`, no `record struct`, no `init` accessors (no `IsExternalInit` polyfill); DTOs and view
models are plain classes or `readonly struct`s with explicit constructors.

---

### Phase 0 — Baseline Capture, Policy Reads, and Dependency Verification

- [x] [P0-T1] Read the policy documents in policy-compliance order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read in an evidence artifact.
  - Acceptance: `evidence/baseline/phase0-instructions-read.md` exists containing `Timestamp:`, `Policy Order:`, and the explicit list of the four files read.
- [x] [P0-T2] Capture the csharpier baseline by running `dotnet tool run csharpier . --check` at repo root and recording the result.
  - Acceptance: `evidence/baseline/baseline-csharpier.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of unformatted files).
- [x] [P0-T3] Capture the .NET analyzer baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and recording the result.
  - Acceptance: `evidence/baseline/baseline-analyzers.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [x] [P0-T4] Capture the nullable/type-check baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` and recording the result.
  - Acceptance: `evidence/baseline/baseline-nullable.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, nullable warning counts).
- [x] [P0-T5] Capture the test + coverage baseline by running `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` and recording numeric coverage.
  - Acceptance: `evidence/baseline/baseline-tests.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including passed/failed test counts and numeric baseline line coverage and branch coverage percentages.
- [x] [P0-T6] Verify at execution time that the upstream `folder-probability-plumbing` concrete contract resolves in `UtilitiesCS`: `FolderScore` (with `FolderPath`, `Score`, `Probability`), `FolderRow` + `FolderRowKind` (with `Text`, `Kind`, nullable `FolderScore? Score`), and `FolderPredictor.FolderRowArray` / `FolderPredictor.FindFolderRows(...)` returning `FolderRow[]`. Record that #325 consumes (does not implement) this contract.
  - Acceptance: `evidence/baseline/dependency-verification.2026-07-15T16-43.md` exists recording, per type, the resolved namespace/file (`UtilitiesCS/OutlookObjects/Folder/FolderScore.cs`, `FolderRow.cs`, `FolderPredictor.cs`), a grep/compile confirmation that `FolderScore`, `FolderRow`, `FolderRowKind`, `FolderPredictor.FolderRowArray`, and `FolderPredictor.FindFolderRows` resolve in `UtilitiesCS`, and an explicit note that #325 consumes (does not implement) the contract. Absence of any of these concrete types is BLOCKED (dependency not satisfied), not PASS; presence satisfies the dependency.

### Phase 1 — Host-Neutral Seam: PercentageFormatter (Red-Then-Green)

- [x] [P1-T1] Create `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` declaring `public static string Format(double probability)` (net48 plain static class, no record/init), consuming a `FolderScore.Probability`-shaped `double` in `[0,1]`, and register it with an explicit `<Compile Include="OutlookObjects\Folder\PercentageFormatter.cs" />` in `UtilitiesCS/UtilitiesCS.csproj`.
  - Acceptance: `UtilitiesCS` compiles with the new file present and referenced in the `.csproj`.
- [x] [P1-T2] Author `UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs` (MSTest + FluentAssertions) covering `0.4267 -> "43%"`, `1.0 -> "100%"`, `0.0 -> "0%"`, midpoint rounding away-from-zero, clamp of out-of-`[0,1]` input, and register `<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />` in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`; run the class and confirm it fails. `[expect-fail]`
  - Acceptance: `evidence/regression-testing/red-percentage-formatter.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` showing the tests fail against the unimplemented formatter.
- [x] [P1-T3] Implement `PercentageFormatter.Format` as `percent = (int)Math.Round(Math.Clamp(p, 0.0, 1.0) * 100.0, MidpointRounding.AwayFromZero)` rendered as `percent + "%"`, and run `PercentageFormatterTests` to green.
  - Acceptance: `evidence/regression-testing/green-percentage-formatter.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing all `PercentageFormatterTests` pass.

### Phase 2 — Host-Neutral Seam: FolderNodeViewModel (Red-Then-Green)

- [x] [P2-T1] Create `UtilitiesCS/OutlookObjects/Folder/FolderNodeViewModel.cs` as a net48 plain class exposing `FolderPath`, `DisplayName`, `Probability` (nullable `double?`, sourced from `FolderRow.Score?.Probability`), `Depth`, `HasChildren`, `Expanded`, derived `Glyph`, and a formatted-percentage accessor (delegating to `PercentageFormatter`; empty string when `Probability` is null), and register `<Compile Include="OutlookObjects\Folder\FolderNodeViewModel.cs" />` in `UtilitiesCS/UtilitiesCS.csproj`.
  - Acceptance: `UtilitiesCS` compiles with the new file present and referenced in the `.csproj`.
- [x] [P2-T2] Author `UtilitiesCS.Test/OutlookObjects/Folder/FolderNodeViewModelTests.cs` covering glyph bijection (INV4: `'+'` when `HasChildren && !Expanded`, `'-'` when `HasChildren && Expanded`, none when leaf), empty formatted percentage when `Probability` is null, and non-empty formatted percentage when set from a `FolderScore.Probability` value; register the `<Compile Include>`; run and confirm failure. `[expect-fail]`
  - Acceptance: `evidence/regression-testing/red-folder-node-viewmodel.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` showing the tests fail.
- [x] [P2-T3] Implement `FolderNodeViewModel` behavior (glyph derivation, null-probability empty percentage) and run `FolderNodeViewModelTests` to green.
  - Acceptance: `evidence/regression-testing/green-folder-node-viewmodel.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing all `FolderNodeViewModelTests` pass.

### Phase 3 — Host-Neutral Seam: FolderHierarchyBuilder (Red-Then-Green)

- [x] [P3-T1] Create `UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs` declaring a pure `public IReadOnlyList<TreeNode<FolderNodeViewModel>> Build(IReadOnlyList<FolderRow> rows)` that reuses `UtilitiesCS/ReusableTypeClasses/Other/TreeNodeOfT.cs` `TreeNode<T>`, and register `<Compile Include="OutlookObjects\Folder\FolderHierarchyBuilder.cs" />` in `UtilitiesCS/UtilitiesCS.csproj`.
  - Acceptance: `UtilitiesCS` compiles with the new file present and referenced in the `.csproj`.
- [x] [P3-T2] Author `UtilitiesCS.Test/OutlookObjects/Folder/FolderHierarchyBuilderTests.cs` covering: `Kind == Suggestion` rows (non-null `Score`) split `FolderScore.FolderPath` on `\` with find-or-add ancestor synthesis, probability attached only at the full-folder leaf, synthesized ancestors carrying no probability but `HasChildren = true`; `Separator`/`SearchResult`/`Recent` rows (null `Score`) emitted as depth-0 leaf rows with no probability and `Text` preserved verbatim and in input order; `DisplayName` = last path segment; full path retained as node key/selection value; register the `<Compile Include>`; run and confirm failure. `[expect-fail]`
  - Acceptance: `evidence/regression-testing/red-folder-hierarchy-builder.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` showing the tests fail.
- [x] [P3-T3] Implement `FolderHierarchyBuilder.Build(IReadOnlyList<FolderRow>)` (Suggestion-row segment splitting, find-or-add insertion, leaf probability placement from `FolderScore.Probability`, ancestor synthesis; non-Suggestion rows as depth-0 leaves preserving `Text` and order) and run `FolderHierarchyBuilderTests` to green.
  - Acceptance: `evidence/regression-testing/green-folder-hierarchy-builder.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing all `FolderHierarchyBuilderTests` pass.

### Phase 4 — Host-Neutral Seam: FolderTreeStateModel INV1-INV8 (Red-Then-Green)

- [x] [P4-T1] Create `UtilitiesCS/OutlookObjects/Folder/FolderTreeStateModel.cs` declaring the pure transitions `Expand`, `Collapse`, `Toggle`, `Highlight`, `RightArrow`, `LeftArrow` and a `GetVisibleRows()` pre-order-DFS projection over a `TreeNode<FolderNodeViewModel>` forest, and register `<Compile Include="OutlookObjects\Folder\FolderTreeStateModel.cs" />` in `UtilitiesCS/UtilitiesCS.csproj`.
  - Acceptance: `UtilitiesCS` compiles with the new file present and referenced in the `.csproj`.
- [x] [P4-T2] Author `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeStateModelTests.cs` exhaustively covering INV1 (no leaf expansion), INV2 (visibility iff all ancestors expanded; roots always visible), INV3 (single highlight), INV4 (glyph bijection), INV5 (descendant-state preservation across collapse/re-expand), INV6 (toggle involution), INV7 (indent equals `Depth`), INV8 (stable pre-order DFS order with descending `FolderScore.Score` then ordinal tie-break), plus arrow-key no-ops at root and leaf and the collapse/re-expand round-trip; register the `<Compile Include>`; run and confirm failure. `[expect-fail]`
  - Acceptance: `evidence/regression-testing/red-folder-tree-state-model.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), `Output Summary:` showing the INV1-INV8 tests fail.
- [x] [P4-T3] Implement `FolderTreeStateModel` transitions and visible-row projection to satisfy INV1-INV8, then run `FolderTreeStateModelTests` to green.
  - Acceptance: `evidence/regression-testing/green-folder-tree-state-model.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing all INV1-INV8 tests pass.

### Phase 5 — ItemViewer Owner-Draw, Hit-Test, Keyboard, and Controller Injection (Consume the Contract)

- [x] [P5-T1] Add the additive intent member `void SetFolderSuggestions(IReadOnlyList<FolderRow> rows)` to `QuickFiler/Viewers/IItemViewer.cs`, leaving `SetFolderItems(string[])`, `ClearFolderItems()`, `FolderContains(string)`, and `GetFolderItems()` unchanged, and altering no `NavigateToString`/WebView2 member.
  - Acceptance: `IItemViewer.cs` declares `SetFolderSuggestions(IReadOnlyList<FolderRow>)` alongside the retained members; `QuickFiler` compiles; no WebView2/`NavigateToString` member is altered.
- [x] [P5-T2] Implement `SetFolderSuggestions(IReadOnlyList<FolderRow>)` on `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` to build the forest via `FolderHierarchyBuilder.Build(rows)`, drive `FolderTreeStateModel`, project visible rows, and rebind `CboFolders.Items`; add the `CboFolders` owner-draw `DrawItem` paint (indent by `Depth`, `+`/`-` glyph, display name, right-aligned percentage from `FolderNodeViewModel`'s formatted-percentage accessor into an `e.Bounds.Right`-anchored rectangle) and the glyph mouse hit-test that toggles the node and re-projects.
  - Acceptance: `ItemViewer.FolderSearch.cs` implements `SetFolderSuggestions(IReadOnlyList<FolderRow>)`, `DrawItem`, and glyph hit-test; selection still returns the full folder-path string; `QuickFiler` compiles.
- [x] [P5-T3] Configure `CboFolders.DrawMode = DrawMode.OwnerDrawFixed` and wire the `DrawItem` handler on the runtime-live `ItemViewer` in `QuickFiler/Viewers/ItemViewer.Designer.cs` (or `ItemViewer.cs`), touching no other viewer variant.
  - Acceptance: `CboFolders` on `ItemViewer` is configured for owner-draw and its `DrawItem` event is bound; none of the nine dead design-time variants is modified.
- [x] [P5-T4] Route the Right/Left arrow keys through `QuickFiler/Controllers/KeyboardHandler.cs` to `FolderTreeStateModel.RightArrow`/`LeftArrow` on the highlighted-row node, then re-project `CboFolders.Items`.
  - Acceptance: `KeyboardHandler.cs` invokes the state-model arrow transitions on the highlighted node; the transition logic itself remains in the unit-tested `FolderTreeStateModel`.
- [x] [P5-T5] Inject rows from `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` by sourcing `FolderRow[]` from `FolderPredictor.FolderRowArray` / `FolderPredictor.FindFolderRows(...)` and calling `_itemViewer.SetFolderSuggestions(rows)`, while retaining the existing `SetFolderItems(string[])` call sites (the "Trash to Delete" append in `QfcItemController.MailActions.cs`, the predetermined/index selection path, and `GetFolderItems`/`FolderContains` reads) unchanged.
  - Acceptance: `QfcItemController.FolderHandling.cs` calls `SetFolderSuggestions` with the `FolderRow[]` from the predictor; existing `SetFolderItems(string[])` call sites are preserved; `QuickFiler` compiles.
- [x] [P5-T6] Author `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs` (MSTest + Moq + FluentAssertions) verifying `QfcItemController` hands the `FolderRow[]` rows (Suggestion rows carrying `FolderScore`, separators/recents carrying null `Score`) to `SetFolderSuggestions` via `Mock<IItemViewer>`, and that existing `SetFolderItems(string[])` expectations ("Trash to Delete", index-1/predetermined selection) remain satisfied; register `<Compile Include="Controllers\QfcItemController.FolderSuggestionsTests.cs" />` in `QuickFiler.Test/QuickFiler.Test.csproj`; run to green.
  - Acceptance: `evidence/regression-testing/green-controller-injection.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing the new controller-injection tests pass and existing `SetFolderItems` expectations remain green.

### Phase 6 — Final QC Loop, Coverage Delta, and Non-Interference Verification

- [x] [P6-T1] Run `dotnet tool run csharpier .` at repo root; if it reformats any file, restart the toolchain loop from this task.
  - Acceptance: `evidence/qa-gates/final-csharpier.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming no files required reformatting on the final pass.
- [x] [P6-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  - Acceptance: `evidence/qa-gates/final-analyzers.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing build succeeded with zero analyzer errors.
- [x] [P6-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  - Acceptance: `evidence/qa-gates/final-nullable.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` showing build succeeded with warnings treated as errors and zero nullable warnings.
- [x] [P6-T4] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` and record numeric post-change coverage.
  - Acceptance: `evidence/qa-gates/final-tests.2026-07-15T16-43.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` including all-tests-pass and numeric post-change line and branch coverage percentages.
- [x] [P6-T5] Compare baseline coverage (P0-T5) against post-change coverage (P6-T4) and report the four new-seam module coverages (`PercentageFormatter`, `FolderNodeViewModel`, `FolderHierarchyBuilder`, `FolderTreeStateModel`) against the thresholds.
  - Acceptance: `evidence/qa-gates/coverage-delta.2026-07-15T16-43.md` exists reporting baseline line/branch coverage, post-change line/branch coverage, and per-seam new-code coverage, with an explicit pass/fail against line `>= 85%`, branch `>= 75%`, and new-module `>= 90%`. If any threshold is unmet the outcome is remediation-required, not PASS.
- [x] [P6-T6] Verify non-interference with 9004 by confirming the diff for this feature modifies none of `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`, `QuickFiler/Viewers/ItemViewer.WebViewThread.cs`, `QuickFiler/Viewers/WebView2CoreInitializer.cs`, `QuickFiler/Viewers/IWebViewCoreInitializer.cs`, or the WebView2/`NavigateToString` members, and modifies none of the nine dead design-time viewer variants.
  - Acceptance: `evidence/qa-gates/non-interference-9004.2026-07-15T16-43.md` exists listing the `git diff --name-only` file set and confirming zero overlap with the 9004 body-render files and zero changes to the nine dead viewer variants.
