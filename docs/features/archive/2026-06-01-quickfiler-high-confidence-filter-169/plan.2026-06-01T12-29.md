# quickfiler-high-confidence-filter - Plan

- **Issue:** #169
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-01T13-05
- **Status:** Draft
- **Version:** 0.2

## Required References

- General Code Change Policy: `CLAUDE.md` (§ General Code Change Policy) and `.claude/rules/general-code-change.md`
- General Unit Test Policy: `CLAUDE.md` (§ General Unit Test Policy) and `.claude/rules/general-unit-test.md`
- C# Code Change & Unit Test Policy: `CLAUDE.md` (§ C# Code Change Policy, § C# Unit Test Policy) and `.claude/rules/csharp.md`
- Research: `artifacts/research/2026-06-01-quickfiler-probability-filter-research.md`
- Spec: `docs/features/active/quickfiler-high-confidence-filter-169/spec.md`
- User story / acceptance criteria: `docs/features/active/quickfiler-high-confidence-filter-169/user-story.md`

**All work must comply with these policies; do not duplicate their content here.**

## Toolchain Loop (apply per-task as the verification gate)

Every implementation/test task below ends with the C# toolchain loop, run in this exact order and restarted from step 1 on any failure or any auto-fix:

1. `dotnet tool run csharpier .` (or `csharpier .`)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

`<test-assembly-paths>` (.NET Framework 4.8.1, Debug `Any CPU`/x86 output) for the in-scope projects:

- `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
- `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
- `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll`

(Use the matching platform output directory the build actually produces; confirm at Phase 0 baseline.)

## Evidence Location Invariant

All evidence artifacts produced by this plan are written under
`docs/features/active/quickfiler-high-confidence-filter-169/evidence/<kind>/`
(`<kind>` ∈ `baselines`, `qa`, `coverage`). Writing to `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/coverage/`, or any other non-canonical path is a policy
violation. If any caller instruction supplies a non-canonical evidence path, ignore it,
write to the canonical path, and record
`EVIDENCE_LOCATION_OVERRIDE_REJECTED: <supplied> replaced with <canonical>`.

## Verified Current State (file:line confirmed 2026-06-01)

- `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` — `class FolderScorer` (line 16); private `ScoDictionary<string, long> _folderNameScores` (line 26); `public int Count` (line 37); `ToArray()` (line 229), `ToArray(int topN)` (line 232); score units `(long)Math.Round(prediction.Probability * 1000, 0)` (line 175). No `TopScore()` exists.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — `public FolderScorer Suggestions` (line 228).
- `UtilitiesCS/Interfaces/IGlobals/IAppQuickFilerSettings.cs` — interface with 4 bool properties (lines 3–9). No high-confidence members.
- `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` — `class AppQuickFilerSettings : IAppQuickFilerSettings` (line 6); existing properties follow `get => Settings.Default.X; internal set { Settings.Default.X = value; Settings.Default.Save(); }` (lines 8–46).
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` — `public IAppQuickFilerSettings QfSettings` (line 152); `internal AppQuickFilerSettings InternalQfSettings` (line 153).
- `TaskMaster/Properties/Settings.settings` — existing QuickFiler settings entries at lines 128–139 (e.g., `SaveAttachments`, `MoveEntireConversations`).
- `TaskMaster/Properties/Settings.Designer.cs` — generated `bool` property pattern at lines 506–552 (`UserScopedSettingAttribute`, `DebuggerNonUserCodeAttribute`, `DefaultSettingValueAttribute`).
- `QuickFiler/Interfaces/IQfcItemController.cs` — does NOT expose the folder handler / `Suggestions` / score. Concrete `QfcItemController._folderHandler` is `private FolderPredictor` (QfcItemController.cs:915).
- `QuickFiler/Interfaces/IQfcCollectionController.cs` — has `Task LoadSecondaryAsync()` (line 19); no `RemoveBelowThresholdAsync`.
- `QuickFiler/Controllers/QfcCollectionController.cs` — `private IApplicationGlobals _globals` (line 59); `private List<QfcItemGroup> _itemGroups` (line 237); `public async Task LoadSecondaryAsync()` (line 430); `private EmailMoveMonitor _moveMonitor` (line 75); existing removal path `public async Task RemoveSpecificControlGroupAsync(int selection)` (line 1012) and `internal void RemoveSpecificControlGroup(string entryID)` (line 945), both of which already call `_moveMonitor.UnhookItem(...)` and `_itemGroups.RemoveAt(...)`.
- `QuickFiler/Controllers/QfcItemGroup.cs` — `internal IQfcItemController ItemController` (line 39); `MailItem` accessible via group.
- `QuickFiler/Controllers/QfcFormController.cs` — `private IApplicationGlobals _globals` (line 70); `LoadItemsAsync(IList<MailItem>, ProgressTracker)` awaits `_groups.LoadSecondaryAsync()` at line 935 (insertion point).
- `TaskMaster/Ribbon/RibbonController.cs` — `internal async Task LoadQuickFilerAsync()` (lines 106–118) calls `QfcHomeController.LaunchAsync`; `#region SettingsMenu` toggle/accessor pattern at lines 204–230.
- `TaskMaster/Ribbon/RibbonViewer.cs` — `QuickFiler_Click` (line 125); `#region SettingsMenu` callbacks (lines 147–176).
- `TaskMaster/Ribbon/RibbonExplorer.xml` — `<button id="QuickFiler" onAction="QuickFiler_Click" .../>` (lines 206–213); `ItemSortSettings` menu with `<checkBox>` entries (lines 233–263).
- Test projects target .NET Framework 4.8.1 (`UtilitiesCS.Test.csproj` line 35).

### Drift / decisions versus the research artifact

- The research recommends adding `TopScore()` and reading it through `_folderHandler.Suggestions`. Because `IQfcItemController` does NOT expose `_folderHandler`/`Suggestions`, a narrow read-only seam member (`long TopFolderScore { get; }`) must be added to `IQfcItemController` and implemented on `QfcItemController` (delegating to `_folderHandler?.Suggestions?.TopScore() ?? 0`). This is required to make `RemoveBelowThresholdAsync` unit-testable per research §6.2 with `Mock<IQfcItemController>`. Tasks below add this seam.
- `RemoveBelowThresholdAsync` reuses the existing removal path (`RemoveSpecificControlGroup(string entryID)` / `RemoveSpecificControlGroupAsync`), which already performs UI-thread removal, `_moveMonitor.UnhookItem`, and renumbering, rather than duplicating that logic.
- Existing ItemSortSettings `<checkBox>` entries use `onAction="*_Clicked"` while `RibbonViewer` defines `*_Click`; new ribbon callbacks below mirror the working `QuickFiler_Click` button to avoid the naming inconsistency.

---

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture & Policy Read

- [x] [P0-T1] Read and record the policy reading order before any code change: `CLAUDE.md` (General Code Change Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy), `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`.
  - Acceptance: `docs/features/active/quickfiler-high-confidence-filter-169/evidence/baselines/policy-read.<ISO-8601>.md` exists, lists each policy file read with an ISO-8601 timestamp, recorded before Phase 1.
- [x] [P0-T2] Capture the formatter baseline by running `dotnet tool run csharpier . --check` (no changes applied) and recording the result.
  - Acceptance: `docs/features/active/quickfiler-high-confidence-filter-169/evidence/baselines/csharpier.<ISO-8601>.txt` records the command and its full output (pass/fail and any flagged files).
- [x] [P0-T3] Capture the analyzer build baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  - Acceptance: `docs/features/active/quickfiler-high-confidence-filter-169/evidence/baselines/analyzer-build.<ISO-8601>.txt` records the command, exit status, warning/error counts, and the resolved Debug output directory for the three in-scope test assemblies.
- [x] [P0-T4] Capture the nullable/type-check build baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  - Acceptance: `docs/features/active/quickfiler-high-confidence-filter-169/evidence/baselines/nullable-build.<ISO-8601>.txt` records the command and its full result (pass/fail and any nullable warnings on touched paths).
- [x] [P0-T5] Capture the test + coverage baseline by running `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`.
  - Acceptance: `docs/features/active/quickfiler-high-confidence-filter-169/evidence/baselines/tests-coverage.<ISO-8601>.txt` records total/passed/failed counts and the repository-wide and per-project line-coverage percentages used as the pre-change comparison point.

### Phase 1 — FolderScorer.TopScore() (AC2, AC3)

- [x] [P1-T1] Add `public long TopScore()` to `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` (in the `#region public methods`) returning `_folderNameScores.Count == 0 ? 0 : _folderNameScores.Max(x => x.Value)`, with an XML doc comment stating it returns the highest score in 0–1000 units or 0 when empty, and is pure in-memory (callable on any thread once populated).
  - Acceptance: method compiles; `FolderScorer.cs` remains under 500 lines; behavior matches the contract; the toolchain loop passes.
- [x] [P1-T2] Add MSTest tests for `TopScore()` to `UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs` using FluentAssertions: (a) empty scorer returns 0; (b) single suggestion returns its score; (c) multiple suggestions returns the highest; (d) tied highest returns that tied value. Build scorers via `AddSuggestion(string, long)`; no Outlook COM, no temp files.
  - Acceptance: all four tests are present, independent, deterministic, pass; `TopScore()` line coverage is 100%; the toolchain loop passes.

### Phase 2 — Settings plumbing: HighConfidenceModeEnabled, HighConfidenceThreshold (AC4, AC5, AC6)

- [x] [P2-T1] Add two `<Setting>` entries to `TaskMaster/Properties/Settings.settings` mirroring lines 128–139: `HighConfidenceModeEnabled` (`System.Boolean`, `Scope="User"`, default `False`) and `HighConfidenceThreshold` (`System.Double`, `Scope="User"`, default `0.9`).
  - Acceptance: both entries exist with correct types, scope, and defaults; XML is well-formed; the toolchain loop passes.
- [x] [P2-T2] Add the matching generated properties to `TaskMaster/Properties/Settings.Designer.cs` mirroring the pattern at lines 506–552: `public bool HighConfidenceModeEnabled` (`DefaultSettingValueAttribute("False")`) and `public double HighConfidenceThreshold` (`DefaultSettingValueAttribute("0.9")`), each with `UserScopedSettingAttribute` and `DebuggerNonUserCodeAttribute`, getter casting `this["..."]`, setter assigning `this["..."]`.
  - Acceptance: both generated properties exist and compile; defaults match `.settings`; the toolchain loop passes.
- [x] [P2-T3] Add read-only members to `UtilitiesCS/Interfaces/IGlobals/IAppQuickFilerSettings.cs`: `bool HighConfidenceModeEnabled { get; }` and `double HighConfidenceThreshold { get; }`.
  - Acceptance: interface compiles; the toolchain loop passes (note: existing test doubles in `TaskMaster.Test` that implement `IAppQuickFilerSettings` indirectly via mocks are unaffected; any concrete implementor is updated in P2-T4).
- [x] [P2-T4] Implement both properties in `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` mirroring lines 8–46: `get => Settings.Default.<Name>;` with `internal set { Settings.Default.<Name> = value; Settings.Default.Save(); }` for each.
  - Acceptance: both properties compile and satisfy `IAppQuickFilerSettings`; `AppQuickFilerSettings.cs` stays under 500 lines; the toolchain loop passes.
- [x] [P2-T5] Create `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs` (MSTest + FluentAssertions) with `[TestInitialize]`/`[TestCleanup]` that snapshots and restores `Settings.Default.HighConfidenceModeEnabled` and `Settings.Default.HighConfidenceThreshold` (no temp files; no reliance on persisted machine state across tests). Tests: (a) default `HighConfidenceModeEnabled == false`; (b) default `HighConfidenceThreshold == 0.9`; (c) setting `HighConfidenceModeEnabled = true` reads back `true`; (d) setting `HighConfidenceThreshold = 0.75` reads back `0.75`.
  - Acceptance: file exists, all four tests independent/deterministic and pass; new property lines reach >= 90% coverage; `Settings.Default` is restored in `[TestCleanup]`; the toolchain loop passes.

### Phase 3 — Item-controller score seam (testability for Phase 4)

- [x] [P3-T1] Add `long TopFolderScore { get; }` to `QuickFiler/Interfaces/IQfcItemController.cs`.
  - Acceptance: interface member added with an XML doc comment stating it returns the top folder suggestion score (0–1000 units) or 0 when no suggestion exists; the toolchain loop passes.
- [x] [P3-T2] Implement `public long TopFolderScore => _folderHandler?.Suggestions?.TopScore() ?? 0;` in `QuickFiler/Controllers/QfcItemController.cs` (near the `_folderHandler` field at line 915).
  - Acceptance: property compiles, returns 0 when `_folderHandler` is null/unpopulated; `QfcItemController.cs` stays under 500 lines (if the file is already near the limit, place the property without exceeding 500 lines, otherwise split is out of scope and must be flagged); the toolchain loop passes.

### Phase 4 — RemoveBelowThresholdAsync on the collection controller (AC2, AC3)

- [x] [P4-T1] Add `Task RemoveBelowThresholdAsync(double threshold)` to `QuickFiler/Interfaces/IQfcCollectionController.cs` (in the "UI Add and Remove QfcItems" group), with an XML doc comment defining: removes item groups whose `ItemController.TopFolderScore` is below `(long)Math.Round(threshold * 1000, 0)`; comparison is inclusive of the boundary (score == threshold retained); `threshold` is a `double` in [0.0, 1.0].
  - Acceptance: interface member added; the toolchain loop passes.
- [x] [P4-T2] Implement `public async Task RemoveBelowThresholdAsync(double threshold)` in `QuickFiler/Controllers/QfcCollectionController.cs`: compute `long cutoff = (long)Math.Round(threshold * 1000, 0)`; snapshot the `EntryID` of each group in `_itemGroups` whose `ItemController.TopFolderScore < cutoff`; for each captured EntryID, remove via the existing `RemoveSpecificControlGroup(string entryID)` path (which already performs UI-thread removal, `_moveMonitor.UnhookItem`, and renumbering). Capture EntryIDs before removing to avoid index/renumber drift during iteration. Guard against null `_itemGroups`.
  - Acceptance: method compiles; groups at or above `cutoff` are retained, groups below are removed; groups with `TopFolderScore == 0` (no suggestion) are removed when `threshold > 0`; the move monitor is unhooked for each removed item via the reused removal path; `QfcCollectionController.cs` stays under 500 lines or the addition does not push it over (flag if it would); the toolchain loop passes.
- [x] [P4-T3] Add MSTest tests to `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (MSTest + Moq + FluentAssertions) verifying `RemoveBelowThresholdAsync` removal selection using `Mock<IQfcItemController>` per group with `SetupGet(c => c.TopFolderScore)` returning known values: (a) all groups above threshold — none removed; (b) all groups below — all removed; (c) mixed — only below-threshold removed; (d) inclusive boundary — group with score exactly equal to `(long)Math.Round(threshold*1000,0)` retained; (e) group with `TopFolderScore == 0` removed when threshold > 0. Verify the removal path is invoked for exactly the expected EntryIDs and not the retained ones; no Outlook COM, no temp files.
  - Acceptance: tests are present, independent, deterministic, and pass; new `RemoveBelowThresholdAsync` lines reach >= 90% coverage; changed-line coverage does not regress; the toolchain loop passes.

> Note for [P4-T3]: if invoking the real `RemoveSpecificControlGroup` requires live UI state that cannot be exercised under unit test, introduce the smallest seam (extract the per-group removal into an injectable `Func<string, Task>`/internal virtual method, defaulting to the existing path) so the selection logic is verifiable without WinForms/COM. Record the seam choice in the task notes.
>
> SEAM INTRODUCED ([P4-T2]/[P4-T3]): Added a private injectable delegate seam in
> `QfcCollectionController`: `private Func<string, Task> _removeGroupByEntryId;` with a lazy
> property `RemoveGroupByEntryId` that defaults to wrapping the existing UI-thread path
> `RemoveSpecificControlGroup(string entryID)` (which unhooks the move monitor and renumbers).
> `RemoveBelowThresholdAsync` awaits this delegate per captured EntryID. The seam is the
> injectable-delegate option (preferred order per `.claude/rules/csharp.md` DI Seams: interface ->
> delegate -> adapter). The P4-T3 tests inject a recording delegate via reflection
> (`_removeGroupByEntryId`) so the below-threshold selection logic is verified without WinForms/COM.
> Production threading is unchanged: the default delegate runs the existing synchronous UI-thread
> removal path.

### Phase 5 — Conditional removal call in the form controller (AC2, AC6)

- [x] [P5-T1] In `QuickFiler/Controllers/QfcFormController.cs`, immediately after `await _groups.LoadSecondaryAsync();` (line 935) in `LoadItemsAsync(IList<MailItem>, ProgressTracker)`, add: `if (_globals.QfSettings.HighConfidenceModeEnabled) { await _groups.RemoveBelowThresholdAsync(_globals.QfSettings.HighConfidenceThreshold); }`.
  - SEAM INTRODUCED ([P5-T1]/[P5-T2]): The conditional was extracted into `internal async Task ApplyHighConfidenceFilterAsync(IQfcCollectionController groups)` on `QfcFormController`, called immediately after `await _groups.LoadSecondaryAsync();`. `LoadItemsAsync` itself constructs a real `QfcCollectionController` and shows the WinForms viewer, so it is not unit-testable; extracting the conditional (interface-parameter seam) lets P5-T2 verify the enabled/disabled branch with `Mock<IQfcCollectionController>` and `Mock<IApplicationGlobals>`/`Mock<IAppQuickFilerSettings>` without WinForms/COM. Threading is preserved: the call remains after `LoadSecondaryAsync` completes. `QuickFiler` already exposes `InternalsVisibleTo("QuickFiler.Test")`.
  - Acceptance: code compiles; removal runs only after `LoadSecondaryAsync` has awaited to completion and only when the mode is enabled; `QfcFormController.cs` stays under 500 lines or the addition does not push it over (flag if it would); the toolchain loop passes.
- [x] [P5-T2] Add MSTest tests to `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (MSTest + Moq + FluentAssertions) using `Mock<IQfcCollectionController>` and `Mock<IApplicationGlobals>` whose `QfSettings` returns a stub `IAppQuickFilerSettings`: (a) when `HighConfidenceModeEnabled == true`, after `LoadItemsAsync`, `RemoveBelowThresholdAsync` is called exactly once with the configured `HighConfidenceThreshold`; (b) when `HighConfidenceModeEnabled == false`, `RemoveBelowThresholdAsync` is never called. Reuse the existing test setup pattern for `QfcFormController`; no Outlook COM, no temp files.
  - Acceptance: both tests present, independent, deterministic, pass; the conditional branch lines are covered (>= 90% on changed lines); the toolchain loop passes.

### Phase 6 — Ribbon entry point + threshold input control (AC1, AC5)

- [x] [P6-T1] Add `internal async Task LoadQuickFilerHighConfidenceAsync()` to `TaskMaster/Ribbon/RibbonController.cs` mirroring `LoadQuickFilerAsync` (lines 106–118): ensure high-confidence mode is active for this launch by setting `Globals.InternalQfSettings.HighConfidenceModeEnabled = true` before awaiting `QfcHomeController.LaunchAsync(Globals, ReleaseQuickFiler)`, using the existing `_quickFilerLoaded` guard.
  - Acceptance: method compiles; the standard `LoadQuickFilerAsync` path is unchanged; the toolchain loop passes.
- [x] [P6-T2] Add to the `#region SettingsMenu` of `TaskMaster/Ribbon/RibbonController.cs` (mirroring lines 204–230): `internal bool IsHighConfidenceModeActive() => Globals.QfSettings.HighConfidenceModeEnabled;`, `internal void ToggleHighConfidenceMode() => Globals.InternalQfSettings.HighConfidenceModeEnabled = !Globals.InternalQfSettings.HighConfidenceModeEnabled;`, `internal string GetHighConfidenceThresholdText() => Math.Round(Globals.QfSettings.HighConfidenceThreshold * 100, 0).ToString(CultureInfo.InvariantCulture);`, and `internal void SetHighConfidenceThresholdText(string text)` that parses a percentage in [0,100], converts to a `double` in [0.0,1.0], and on valid input writes `Globals.InternalQfSettings.HighConfidenceThreshold`; on invalid/out-of-range input leaves the persisted value unchanged.
  - Acceptance: methods compile; valid percentage input persists the converted value; invalid/out-of-range input leaves the setting unchanged; the toolchain loop passes.
- [x] [P6-T3] Add ribbon callbacks to `TaskMaster/Ribbon/RibbonViewer.cs`: a Click handler for the high-confidence button mirroring `QuickFiler_Click` (line 125) calling `_controller.LoadQuickFilerHighConfidenceAsync()`, plus editBox callbacks `getText` → `_controller.GetHighConfidenceThresholdText()` and `onChange` → `_controller.SetHighConfidenceThresholdText(string text)` with the Office editBox `onChange(IRibbonControl, string)` signature.
  - Acceptance: callbacks compile with the correct Office callback signatures; the toolchain loop passes.
- [x] [P6-T4] Add ribbon UI to `TaskMaster/Ribbon/RibbonExplorer.xml`: a `<button id="QuickFilerHighConfidence" onAction="QuickFilerHighConfidence_Click" label="Quick Filer — High Confidence" .../>` adjacent to the existing `QuickFiler` button (lines 206–213), and an `<editBox id="HighConfidenceThreshold" label="High Confidence %" getText="HighConfidenceThreshold_GetText" onChange="HighConfidenceThreshold_OnChange" .../>` under the `QuickFilerSettings`/`ItemSortSettings` menu (lines 233–263). Callback names must match the methods added in P6-T3.
  - Acceptance: XML is well-formed; control ids and callback names match the C# callbacks; the standard `QuickFiler` button entry is unchanged; the toolchain loop passes.
- [x] [P6-T5] Create `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (MSTest + Moq + FluentAssertions) using `Mock<IApplicationGlobals>` with `QfSettings`/`InternalQfSettings` stubs. Tests: (a) `IsHighConfidenceModeActive()` returns `Globals.QfSettings.HighConfidenceModeEnabled`; (b) `ToggleHighConfidenceMode()` flips the value via `InternalQfSettings`; (c) `GetHighConfidenceThresholdText()` returns the percentage form of the stored threshold (e.g., 0.9 → "90"); (d) `SetHighConfidenceThresholdText("75")` persists `0.75`; (e) `SetHighConfidenceThresholdText` with non-numeric input leaves the value unchanged; (f) `SetHighConfidenceThresholdText("150")` (out of range) leaves the value unchanged. If `InternalQfSettings` is not mockable directly (concrete type), introduce the smallest seam for the threshold get/set path and record it.
  - Acceptance: file exists; all six tests independent/deterministic and pass; new RibbonController member lines reach >= 90% coverage; the toolchain loop passes.
  - SEAM NOTE ([P6-T5]): No production seam was required. `RibbonController.Globals` is the concrete `ApplicationGlobals`, and `InternalQfSettings` returns the concrete `AppQuickFilerSettings`. The test builds an uninitialized `ApplicationGlobals` (`FormatterServices.GetUninitializedObject`), injects a real `AppQuickFilerSettings` into its private `_quickFilerSettings` field via reflection, and assigns it to `RibbonController.Globals` (reflection). `AppQuickFilerSettings` round-trips through `Settings.Default`, which is snapshotted/restored in `[TestInitialize]`/`[TestCleanup]` (no temp files, deterministic, independent). This exercises the real helper methods end-to-end without constructing the Outlook-backed globals.

### Phase 7 — Final QA Loop & Documentation

- [x] [P7-T1] Run the full C# toolchain loop end-to-end and restart from step 1 on any failure or auto-fix: (1) `dotnet tool run csharpier .`; (2) analyzer build; (3) nullable/`TreatWarningsAsErrors` build; (4) `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`.
  - Acceptance: `docs/features/active/quickfiler-high-confidence-filter-169/evidence/qa/final-toolchain.<ISO-8601>.txt` records all four commands passing without errors in a single final pass.
- [x] [P7-T2] Compare post-change coverage against the Phase 0 baseline.
  - Acceptance: `docs/features/active/quickfiler-high-confidence-filter-169/evidence/coverage/comparison.<ISO-8601>.md` shows repository-wide line coverage >= 80%, new members (`FolderScorer.TopScore`, both settings properties, `IQfcItemController.TopFolderScore`/impl, `RemoveBelowThresholdAsync`, RibbonController helpers) at >= 90%, and no regression on changed lines versus `evidence/baselines/tests-coverage.*`.
- [x] [P7-T3] Update `docs/features/active/quickfiler-high-confidence-filter-169/spec.md` and `user-story.md` Definition-of-Done / acceptance-criteria checkboxes to reflect completed work, and record the v1 batch-1 limitation note in the spec if not already explicit.
  - Acceptance: spec and user-story documents reflect final state; AC1–AC7 mapped to the implementing tasks/tests below; no contradictory open items remain.
- [x] [P7-T4] Verify each acceptance criterion maps to a passing task/test and record the AC status summary.
  - Acceptance: `docs/features/active/quickfiler-high-confidence-filter-169/evidence/qa/ac-status.<ISO-8601>.md` lists AC1–AC7, each marked satisfied with the implementing task IDs and the covering test(s); any unmet AC marks the verdict BLOCKED/INCOMPLETE.

## Acceptance Criteria Mapping

- **AC1** (new ribbon entry point launches high-confidence mode): P6-T1, P6-T3, P6-T4; tested by P6-T5 (a)/(b).
- **AC2** (below-threshold emails not shown): P1-T1, P3-T1/P3-T2, P4-T1/P4-T2, P5-T1; tested by P1-T2, P4-T3 (a)/(b)/(c), P5-T2 (a).
- **AC3** (no qualifying suggestion excluded): P1-T1 (empty → 0), P4-T2; tested by P1-T2 (a), P4-T3 (e).
- **AC4** (default threshold 0.90 persisted): P2-T1, P2-T2, P2-T4; tested by P2-T5 (b).
- **AC5** (runtime threshold input with validation, persisted): P6-T2, P6-T3, P6-T4; tested by P2-T5 (d), P6-T5 (c)/(d)/(e)/(f).
- **AC6** (disabled = unchanged behavior): P5-T1 (conditional guard); tested by P5-T2 (b); standard `LoadQuickFilerAsync` left unchanged (P6-T1).
- **AC7** (MSTest+Moq+FluentAssertions coverage; full toolchain passes, zero regressions): all test tasks (P1-T2, P2-T5, P4-T3, P5-T2, P6-T5) plus P7-T1, P7-T2.

## Test Plan

- Unit:
  - `UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs` — `TopScore()` (P1-T2).
  - `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs` — settings defaults & round-trip (P2-T5).
  - `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` — `RemoveBelowThresholdAsync` selection (P4-T3).
  - `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` — conditional removal call (P5-T2).
  - `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` — high-confidence toggle/threshold get/set/validation (P6-T5).
- Integration: covered at unit level via interface seams (`IQfcCollectionController`, `IQfcItemController`, `IAppQuickFilerSettings`, `IApplicationGlobals`); no live Outlook COM in tests.
- Manual/CLI: manual ribbon validation (launch high-confidence button; confirm below-threshold items are removed; adjust threshold percentage and confirm persistence) is out of automated scope; record observations if performed.
- Coverage evidence:
  - Baseline: `evidence/baselines/tests-coverage.<ISO-8601>.txt` (P0-T5).
  - Post-change: `evidence/qa/final-toolchain.<ISO-8601>.txt` (P7-T1).
  - Comparison: `evidence/coverage/comparison.<ISO-8601>.md` (P7-T2).

## Open Questions / Notes

- v1 filters only the initially loaded batch (consistent with current `InitEmailQueueAsync` batch-1 scope); re-application across later background batches is out of scope.
- File-size watch: `QfcItemController.cs`, `QfcCollectionController.cs`, and `QfcFormController.cs` should be checked against the 500-line limit before adding members; if any addition would exceed the limit, flag for a scoped split rather than silently exceeding policy.
- `RemoveSpecificControlGroup(string entryID)` is the reuse target for removal so the move monitor is unhooked and renumbering occurs through one code path; do not duplicate removal logic.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required — no non-canonical evidence path was supplied; all evidence is written under `docs/features/active/quickfiler-high-confidence-filter-169/evidence/<kind>/`.
