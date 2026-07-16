# efcviewer-folder-tree-percentage — Plan

- **Issue:** #327
- **Parent:** Epic `folder-tree-percentage-ui` (child feature, wave 1)
- **Owner:** drmoisan
- **Branch:** feature/efcviewer-folder-tree-percentage-327 (cut from epic/folder-tree-percentage-ui-integration)
- **Last Updated:** 2026-07-15T18-00
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Requirements Sources

- `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/spec.md` (authoritative AC)
- `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/user-story.md` (authoritative AC)
- `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/issue.md`
- `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/research/2026-07-15T17-15-efcviewer-tree-percentage-research.md`

**All work must comply with the repository policies (CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`). Do not duplicate their content here.**

## Upstream Dependency Note (folder-probability-plumbing, epic placeholder issue 9001)

This feature consumes an upstream contract that maps full folder-path string -> prediction probability
(assumed `double` in `[0, 1]`). This plan does NOT implement 9001.

- The probability value is CONSUMED for the percentage display; scores are never recomputed here (spec.md §Out of Scope; research §5.2).
- Consumption is isolated behind a narrow seam (`IFolderProbabilitySource`) plus a `FolderProbabilityAdapter`, so that if the finalized 9001 contract differs, only the adapter/seam changes.
- Sequencing: at epic execution time, 9001 merges into `epic/folder-tree-percentage-ui-integration` BEFORE #327 runs. Task `P3-T4` re-confirms the actual merged contract shape before wiring the real value. Until the merged shape is confirmed, the adapter is wired to the seam interface only (assumed shape from research §5.2).

## Scope Lock (files created / modified)

New host-neutral source (non-exempt, target >= 90% coverage) — all under `UtilitiesCS/OutlookObjects/Folder/`:
- CREATE `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionNode.cs` (+ `<Compile Include>` in `UtilitiesCS/UtilitiesCS.csproj`)
- CREATE `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` (+ `<Compile Include>`)
- CREATE `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` (+ `<Compile Include>`)
- CREATE `UtilitiesCS/OutlookObjects/Folder/IFolderProbabilitySource.cs` (+ `<Compile Include>`)
- CREATE `UtilitiesCS/OutlookObjects/Folder/FolderProbabilityAdapter.cs` (+ `<Compile Include>`)

New tests — all under `UtilitiesCS.Test/OutlookObjects/Folder/`:
- CREATE `UtilitiesCS.Test/OutlookObjects/Folder/FolderSuggestionTreeHierarchyTests.cs` (+ `<Compile Include>` in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`)
- CREATE `UtilitiesCS.Test/OutlookObjects/Folder/FolderSuggestionTreeStateTests.cs` (+ `<Compile Include>`)
- CREATE `UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs` (+ `<Compile Include>`)
- CREATE `UtilitiesCS.Test/OutlookObjects/Folder/FolderProbabilityAdapterTests.cs` (+ `<Compile Include>`)

Modified WinForms / controller (coverage-exempt, build + manual QA only):
- MODIFY `QuickFiler/Viewers/EfcViewer3.cs` (add `[ExcludeFromCodeCoverage]`)
- MODIFY `QuickFiler/Viewers/EfcViewer.Designer.cs` (TreeListView + right-aligned percentage column)
- MODIFY `QuickFiler/Viewers/EfcViewer3.Designer.cs` (TreeListView + right-aligned percentage column)
- MODIFY `QuickFiler/Controllers/EfcFormController.cs` (bindings 551/799/961, delete-rebind 737-744, `SelectedFolder` 278-281, `IsValidSelection` 968-980, banner guard 703, KeyDown 398-406)

Note: `UtilitiesCS/UtilitiesCS.csproj` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` are legacy `packages.config` projects that enumerate every source via explicit `<Compile Include>` with no wildcard glob. Each new `.cs` file MUST be wired with a matching `<Compile Include>` item in the same task that creates it, or it will not compile into the assembly.

## Evidence Location Invariant

All evidence artifacts are written ONLY under
`docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/<kind>/`
using kinds `baseline/`, `qa-gates/`, `regression-testing/`, and `other/`.
Writing to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any other non-canonical
path is a policy violation. Timestamps use `yyyy-MM-ddTHH-mm`.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Review

- [x] [P0-T1] Read the policy files in policy-compliance order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact exists containing `Timestamp:`, `Policy Order:`, and the explicit list of the four files read.
- [x] [P0-T2] Run `csharpier . --check` (or `dotnet tool run csharpier . --check`) at repo root and record the result in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/baseline/phase0-baseline-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T3] Build analyzers via `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/baseline/phase0-baseline-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (warning/error counts).
- [x] [P0-T4] Build nullable gate via `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record the result in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/baseline/phase0-baseline-nullable.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T5] Run baseline tests with coverage via `vstest.console.exe` against the `UtilitiesCS.Test` and `QuickFiler.Test` assemblies with `/EnableCodeCoverage` and record the numeric baseline coverage headline in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/baseline/phase0-baseline-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric baseline line-coverage and branch-coverage percentages.

### Phase 1 — Host-Neutral Node Model and Hierarchy Builder

- [x] [P1-T1] Create `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionNode.cs` defining the pure data node (`FullPath`, `DisplayName`, `Depth`, `Children`, `HasChildren`, `IsExpanded`, `Probability` as `double?`, `Kind` enum `{ Folder, Banner }`; no WinForms/COM types) and add a matching `<Compile Include="OutlookObjects\Folder\FolderSuggestionNode.cs" />` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists, csproj item present, `msbuild TaskMaster.sln` compiles the type into `UtilitiesCS.dll`.
- [x] [P1-T2] Create `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` with `BuildFromRows(IReadOnlyList<string>)` that partitions on banner rows (`StartsWith("====")` -> `Kind.Banner`, non-expandable), establishes parent/child edges by longest-present-prefix `X + "\\"` within each section (no ancestor synthesis; unmatched deep paths become section roots), sets `HasChildren`, and preserves per-section input order; add the matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists, csproj item present, compiles; `BuildFromRows` returns ordered roots with banner classification.
- [x] [P1-T3] Create `UtilitiesCS.Test/OutlookObjects/Folder/FolderSuggestionTreeHierarchyTests.cs` (MSTest + FluentAssertions) covering roots, nested children, a deep path without its parent present, banner rows, empty input, and a single node; add the matching `<Compile Include>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: file exists, csproj item present, all hierarchy tests pass under `vstest.console.exe`.
- [x] [P1-T4] Run the full C# toolchain loop in order (`csharpier .` -> `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` -> `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` -> `vstest.console.exe ... /EnableCodeCoverage`) restarting on any failure or file change, and record the green pass in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/qa-gates/phase1-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:` (all four), `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass.

### Phase 2 — Expand/Collapse State Machine and Visible-Row Projection

- [x] [P2-T1] Add `VisibleRows()` to `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` performing a pre-order flatten that emits a node's children only when `IsExpanded == true`, always emits banner rows in section order, and never expands banner nodes
  - Acceptance: `VisibleRows()` returns the correct ordered projection for collapsed and expanded states; compiles.
- [x] [P2-T2] Add pure state transitions to `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`: `Expand`, `Collapse`, `Toggle`, `RightArrow`, `LeftArrow` with the documented no-op rules (leaf, already-expanded on Right, already-collapsed/leaf on Left, banner row) and no side effects beyond `IsExpanded`
  - Acceptance: transitions honor the no-op rules and mutate only `IsExpanded`; compiles.
- [x] [P2-T3] Create `UtilitiesCS.Test/OutlookObjects/Folder/FolderSuggestionTreeStateTests.cs` covering leaf Right/Left no-op, already-expanded Right no-op, already-collapsed Left no-op, root nodes, highlighted banner no-op, empty list, single node, and `VisibleRows()` projection after expand and after collapse; add the matching `<Compile Include>` item to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: file exists, csproj item present, all state/projection tests pass.
- [x] [P2-T4] Run the full C# toolchain loop in order (`csharpier .` -> analyzers `msbuild` -> nullable `msbuild` -> `vstest.console.exe /EnableCodeCoverage`) and record the green pass in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/qa-gates/phase2-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass.

### Phase 3 — Percentage Formatter and Upstream Probability Adapter

- [x] [P3-T1] Create `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs` with `FormatPercent(double? probability)` returning a whole-number percent string (`Math.Round(probability.Value * 100, MidpointRounding.AwayFromZero)` + `"%"`, no decimals) and an empty string when `probability` is null; add the matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists, csproj item present, compiles; `0.732 -> "73%"`, `null -> ""`.
- [x] [P3-T2] Create `UtilitiesCS/OutlookObjects/Folder/IFolderProbabilitySource.cs` defining the narrow 9001 seam (`bool TryGetProbability(string fullFolderPath, out double probability)`, assumed shape from research §5.2) and add the matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists, csproj item present, compiles; interface is the only coupling point to 9001.
- [x] [P3-T3] Create `UtilitiesCS/OutlookObjects/Folder/FolderProbabilityAdapter.cs` that joins an `IFolderProbabilitySource` to presented rows by full-path string equality, assigns `Node.Probability` for matched folder rows, and leaves `Probability == null` for banners/recents/unmatched rows; add the matching `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`
  - Acceptance: file exists, csproj item present, compiles; matched rows carry probability, unmatched rows stay null.
- [x] [P3-T4] Re-confirm the merged `folder-probability-plumbing` (9001) contract shape against `epic/folder-tree-percentage-ui-integration`; if 9001 is merged, verify `IFolderProbabilitySource` matches the real surface (adjust the adapter only if it deviates); if 9001 is not yet merged, record that the adapter remains wired to the assumed seam; write findings to `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/other/upstream-9001-contract-reconfirm.md`
  - Acceptance: artifact records `Timestamp:`, merged-vs-unmerged status, the observed/assumed contract shape, and any adapter delta (or "no change required").
- [x] [P3-T5] Create `UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs` and `UtilitiesCS.Test/OutlookObjects/Folder/FolderProbabilityAdapterTests.cs` covering percent formatting (0, 1, `.5` boundary away-from-zero, null -> blank) and the path->probability join (matched, unmatched, banner) using a Moq `IFolderProbabilitySource`; add both matching `<Compile Include>` items to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: both files exist, csproj items present, all formatter and adapter tests pass.
- [x] [P3-T6] Run the full C# toolchain loop in order (`csharpier .` -> analyzers `msbuild` -> nullable `msbuild` -> `vstest.console.exe /EnableCodeCoverage`) and record the green pass in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/qa-gates/phase3-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass.

### Phase 4 — WinForms and Controller Wiring (coverage-exempt)

- [ ] [P4-T1] Add `[ExcludeFromCodeCoverage]` (with `using System.Diagnostics.CodeAnalysis;`) to the `EfcViewer3` Form class in `QuickFiler/Viewers/EfcViewer3.cs` so the Form-derived class stays out of the testable denominator before UI code is added
  - Acceptance: attribute present on the class; `EfcViewer3` compiles.
- [ ] [P4-T2] Replace the flat `FolderListBox` `ListBox` with a `BrightIdeasSoftware.TreeListView` (tree/name column plus a right-aligned percentage `OLVColumn` with `TextAlign = HorizontalAlignment.Right`) in `QuickFiler/Viewers/EfcViewer.Designer.cs`, reusing the referenced `ObjectListView 2.9.1` library
  - Acceptance: `EfcViewer.Designer.cs` declares the TreeListView with two columns; solution compiles.
- [ ] [P4-T3] Replace the flat `FolderListBox` `ListBox` with a `BrightIdeasSoftware.TreeListView` (tree/name column plus a right-aligned percentage `OLVColumn`) in `QuickFiler/Viewers/EfcViewer3.Designer.cs`
  - Acceptance: `EfcViewer3.Designer.cs` declares the TreeListView with two columns; solution compiles.
- [ ] [P4-T4] Update the three DataSource binding sites (lines ~551, ~799, ~961) and the delete-rebind (lines ~737-744) in `QuickFiler/Controllers/EfcFormController.cs` to build a `FolderSuggestionTree` from the existing `string[]`, join probabilities via `FolderProbabilityAdapter`, and feed the TreeListView via `CanExpandGetter`/`ChildrenGetter`/`AspectGetter` + `SetObjects` (replacing raw `string[]` binding)
  - Acceptance: all four sites compile against the new model; the "Trash to Delete" delete path still functions; solution builds.
- [ ] [P4-T5] Update `SelectedFolder` (lines ~278-281) and `IsValidSelection` (lines ~968-980) in `QuickFiler/Controllers/EfcFormController.cs` to derive the full folder path from the selected tree node and preserve rejection of null/empty/`len<3`/banner (`===`) rows as invalid filing targets
  - Acceptance: `SelectedFolder` returns the node's full path for real folders; banner/short rows remain invalid; compiles.
- [ ] [P4-T6] Update the `ActionOkAsync` banner guard (line ~703) and fill the empty `Left`/`Right` branches of `FolderListBox_KeyDown` (lines ~398-406) in `QuickFiler/Controllers/EfcFormController.cs` so left arrow collapses and right arrow expands the highlighted node (delegating to `FolderSuggestionTree` transitions or native TreeListView behavior without double-handling), keeping the `Up`-at-first-row -> `SearchText.Select()` behavior
  - Acceptance: banner rows remain non-selectable; Left/Right expand/collapse the highlighted node; Up at first row returns to search; compiles.
- [ ] [P4-T7] Run the full C# toolchain loop in order (`csharpier .` -> analyzers `msbuild` -> nullable `msbuild` -> `vstest.console.exe /EnableCodeCoverage`) and record the green pass in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/qa-gates/phase4-toolchain.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:` per step, `Output Summary:`; all four steps green in a single pass (exempt wiring compiles and no host-neutral test regresses).

### Phase 5 — Final QA and Coverage Verification

- [ ] [P5-T1] Run `csharpier .` (or `dotnet tool run csharpier .`) at repo root and record the result in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/qa-gates/phase5-final-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; no formatting changes remain (rerun loop from step 1 if any file changed).
- [ ] [P5-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/qa-gates/phase5-final-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; zero analyzer errors.
- [ ] [P5-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record the result in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/qa-gates/phase5-final-nullable.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; zero nullable/warning-as-error failures.
- [ ] [P5-T4] Run `vstest.console.exe` against `UtilitiesCS.Test` and `QuickFiler.Test` assemblies with `/EnableCodeCoverage` and record numeric post-change coverage in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/qa-gates/phase5-final-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric post-change line/branch coverage and per-file coverage for the five new `UtilitiesCS/OutlookObjects/Folder/` modules.
- [ ] [P5-T5] Compute and record the coverage delta/threshold verification in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/qa-gates/phase5-coverage-delta.md`, comparing baseline (P0-T5) vs post-change (P5-T4) and reporting new/changed-code coverage
  - Acceptance: artifact records baseline coverage, post-change coverage, and new-code coverage; new host-neutral modules meet `>= 90%`, repository floor is not regressed, and no changed line loses coverage; if any threshold is unmet the outcome is remediation-required (not PASS).
- [ ] [P5-T6] Map every acceptance criterion in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/spec.md` and `user-story.md` to its verifying test or build/manual-QA evidence in `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/evidence/other/ac-verification-map.md`
  - Acceptance: artifact lists each AC with a PASS/PARTIAL/BLOCKED verdict and a concrete evidence pointer (test name or exempt build/manual-QA note for the two Forms and controller).

## Test Plan

- Unit (MSTest + Moq + FluentAssertions, host-neutral, deterministic, no temp files/COM/network):
  - Hierarchy building from sectioned `string[]` (roots, nested children, deep-path-without-parent, banners, empty, single node).
  - Expand/collapse transitions and `VisibleRows()` projection for all edge cases (leaf, already-expanded, already-collapsed, root, highlighted banner, empty, single node).
  - Percentage formatting (0, 1, `.5` boundary away-from-zero, null -> blank).
  - Path->probability join by full-path equality (matched, unmatched, banner) via a Moq `IFolderProbabilitySource`.
- Build + manual QA (coverage-exempt): TreeListView wiring, both Designer files, and controller binding/keyboard/selection paths in `EfcViewer`, `EfcViewer3`, and `EfcFormController`.
- Coverage evidence:
  - Baseline: `evidence/baseline/phase0-baseline-tests-coverage.md`
  - Post-change: `evidence/qa-gates/phase5-final-tests-coverage.md`
  - Delta/threshold: `evidence/qa-gates/phase5-coverage-delta.md`

## Open Questions / Notes

- Home of the host-neutral model is `UtilitiesCS/OutlookObjects/Folder/` (maximizes reuse; sibling to `FolderPredictor`; the upstream contract also lives in UtilitiesCS).
- No intermediate ancestor node synthesis for deep-path-without-parent (research §4 decision); such nodes render at their section root with their full/relative path.
- If merged 9001 exposes an already-scaled percentage rather than `[0, 1]`, only `PercentageFormatter` changes (drop the `* 100`); the adapter/seam absorb any shape difference (research §5.2).
- `EfcFormController` is shared by both viewers and is already `[ExcludeFromCodeCoverage]`; TreeListView wiring lives there and stays exempt.
