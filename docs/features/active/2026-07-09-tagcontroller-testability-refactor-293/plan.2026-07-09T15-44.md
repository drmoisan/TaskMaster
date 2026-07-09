# tagcontroller-testability-refactor — Atomic Implementation Plan (#293)

- **Issue:** #293
- **Parent epic:** winforms-testability-refactor (#295), wave 0 — `docs/features/epics/winforms-testability-refactor/epic.md`
- **Owner:** drmoisan
- **Last Updated:** 2026-07-09T15-44
- **Status:** Ready for Preflight
- **Version:** 1.0
- **Work Mode:** full-feature
- **plan-path:** `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/plan.2026-07-09T15-44.md`

## Sources (read, do not restate)

- Epic manifest and Shared Design Pattern: `docs/features/epics/winforms-testability-refactor/epic.md`
- Acceptance criteria: `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/issue.md`
- Authoritative spec: `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/spec.md`
- Implementation-ready design: `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/research/research-findings.2026-07-09T21-30-00Z.md`
- Policy order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`

## Execution-Environment Note

Execution occurs later in a different git worktree branched from `epic/winforms-testability-refactor-integration`. Every path in this plan is repo-root-relative (for example `Tags/TagController.cs`). No absolute path from the planning workspace is used, because such paths would be stale in the execution worktree.

## Evidence Locations

- Numeric coverage EVIDENCE artifacts and toolchain-step evidence: canonical `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/<kind>/` where `<kind>` is `baseline` (Phase 0) or `qa-gates` (Phase 7). Fail-before/regression dossiers go to `evidence/regression-testing/`.
- Raw vstest coverage tool output (the coverage XML consumed by feature-review): `artifacts/csharp/coverage.xml`. This is a tool-output location, not an evidence `<kind>` sub-path; the numeric coverage headline derived from it is recorded inside the canonical evidence artifacts above.
- The caller supplied `artifacts/csharp/` for coverage. That is retained ONLY for the raw coverage XML tool output. It is NOT used for evidence artifacts. No forbidden `artifacts/` evidence sub-path (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, `artifacts/evidence/`, `artifacts/regression-testing/`) is used anywhere in this plan.

## Standard Toolchain Gate (STG)

Every implementation task in Phases 1–6 is complete only when the full C# toolchain passes, in this exact order, with no failing step and no residual auto-format change:

1. `csharpier .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe Tags.Test/bin/Debug/Tags.Test.dll /EnableCodeCoverage` (executor resolves the actual built assembly path under `Tags.Test/bin/`)

If any step fails or any step rewrites files, restart from step 1. "STG passes" in a task means one clean full pass with no file change on the final iteration.

## Legacy csproj Wiring Invariant (scope-lock)

`Tags/Tags.csproj` and `Tags.Test/Tags.Test.csproj` are legacy non-SDK, `packages.config` projects that enumerate every source via explicit `<Compile Include="..." />` items with NO wildcard glob. Any NEW `.cs` file added to either project MUST be wired with a matching `<Compile Include>` item in the same task that creates the file, or it will not compile into the assembly and will not appear in the coverage denominator. Scope-lock csproj modifications:

- MODIFY `Tags/Tags.csproj`: add `<Compile Include>` items for `ITagViewer.cs`, `IUserPrompt.cs`, `WinFormsUserPrompt.cs`, `TagSelectionModel.cs`, `TagController.Rendering.cs`, `LauncherAutoAssign.cs`.
- MODIFY `Tags.Test/Tags.Test.csproj`: add `<Compile Include>` items for each new test/fake file created in Phases 5–6.

## Strategy and Green-Build Ordering

The refactor is sequenced so each atomic task leaves the solution compiling and the existing test suite passing:

1. Phase 1 adds the new seam interfaces and the host-neutral `TagSelectionModel` without touching `TagController`, so the build stays green.
2. Phase 2 makes `TagViewer` implement `ITagViewer` before any controller signature change, so the later `TagViewer -> ITagViewer` upcast at `TagLauncher` and existing test call sites keeps compiling.
3. Phase 3 rewires `TagController` to consume `ITagViewer`/`IUserPrompt`/`TagSelectionModel`, splits the file, and extracts the async auto-assign action while retaining the `async void` handler so existing tests still pass.
4. Phase 4 extracts `LauncherAutoAssign`.
5. Phase 5 migrates existing anti-deterministic tests to seams (removing `[STAThread]` and `Task.Delay`) with assertions preserved or strengthened.
6. Phase 6 adds new coverage tests for the extracted, host-neutral units.
7. Phase 7 runs the final QA loop and verifies the coverage thresholds.

Fail-closed evidence rule: each baseline, QA-gate, and coverage-comparison task names the artifact it must produce. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing or lacks numeric coverage values, the outcome is remediation-required, never PASS.

## Coverage Exemption Register (maintainer ratification required)

`[ExcludeFromCodeCoverage]` is applied ONLY to irreducible WinForms/COM wiring. Each site below is individually justified. Testable seams (`TagSelectionModel`, `TagController` orchestration, `TagController.Rendering` arithmetic against the panel abstraction, `LauncherAutoAssign`, and the extracted `CheckBoxController` decision helper) are NEVER exempt and must meet the coverage floor.

| # | Site (repo-relative) | Justification | Ratification |
|---|---|---|---|
| E1 | `Tags/WinFormsUserPrompt.cs` (class body) | Thin production adapter over `MessageBox.Show` / `InputBox.ShowDialog`; realizes live UI, no decision logic to test. | pending maintainer sign-off |
| E2 | `Tags/TagController.Rendering.cs` — `DrawFocus` seam DEFAULT body only (`Graphics.FromHwnd(cbx.Handle)` + `ControlPaint.DrawFocusRectangle`) | Forces an HWND; the surrounding index/navigation arithmetic is tested via the injected no-op delegate and is NOT exempt. | pending maintainer sign-off |
| E3 | `Tags/TagViewer.cs` (form-derived intent-member bodies) | WinForms `Form`-derived class (exempt category b); intent members are 1:1 thin mappings onto designer controls. | pending maintainer sign-off |
| E4 | `Tags/TagViewer.Designer.cs` | Designer-generated code (exempt category b). Unchanged by this feature. | pre-existing |
| E5 | `Tags/TagLauncher.cs` remaining live-form launcher + globals/COM wiring (`LaunchAndSelect`, `LaunchAndFindMatch`/`ShowDialog`, `GetAutoAssign()` instance overload, `GetHelper`, `CreateCategoryModule`) | Irreducibly host-bound; no injectable seam without a live Outlook/Form. | pending maintainer sign-off |
| E6 | `Tags/Helper Classes/CheckBoxController.cs` — event-wiring members ONLY (`CtrlCB` setter subscribe/unsubscribe, GotFocus/LostFocus color swap) | Irreducible WinForms event plumbing. The click state-machine decision logic is extracted to a testable helper in P6-T4 and is NOT exempt. | pending maintainer sign-off (narrowed from prior blanket exemption) |

If P6-T4 extraction of the `CheckBoxController` decision logic is descoped, E6 must be re-ratified explicitly as a blanket exemption rather than inherited silently.

## Reported-Only Defects (OUT OF SCOPE, do not fix)

Per spec `## Non-Goals`: the `RemoveControls` `_colColorbox.Remove(i)` index/element confusion (latent), the orphaned uncompiled files `Tags/CheckBoxController.cs` (root) and `Tags/AutoAssignInterface.cs`, and the `PrefixItem.PrefixType`/`OlUserFieldName` `NotImplementedException` members are report-only and are NOT fixed here. Exception: banned APIs (`DateTime.Now`/`UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`) found in any file this plan touches MUST be remediated in the touching task (the known instance is `Task.Delay(50)` in `Tags.Test/TagControllerCoverageExpansionTests.cs`, remediated in P5-T3).

---

### Phase 0 — Baseline Capture and Precondition Verification

- [ ] [P0-T1] Read the four policy files in required order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and write `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [ ] [P0-T2] Verify `Tags/TagController.cs` exists and its line count is in the 870–885 range (spec states 877). Record the actual count in `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/baseline/precondition-file-shape.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P0-T3] Verify the interface shape of `UtilitiesCS/Interfaces/IWinForm/IForm.cs`: it exposes `Close()`, `KeyPreview`, `ShowDialog()`, `ShowDialog(IWin32Window)` and does NOT expose `Text`, a `KeyDown` event, or `Controls`. Append the finding to `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/baseline/precondition-file-shape.md`.
- [ ] [P0-T4] Verify `Tags.Test/Tags.Test.csproj` references Moq 4.20, FluentAssertions 8.9, and MSTest 4.2, and enumerate its current `<Compile Include>` set. Record to `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/baseline/precondition-testproject.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P0-T5] Verify NO `ITagViewer` type and NO `IUserPrompt` type exist yet in the repository (grep across `Tags/` and `UtilitiesCS/`), and confirm the `Tags/Tags.csproj` `<Compile>` set matches the research inventory (`Helper Classes/CheckBoxController.cs`, `Helper Classes/PrefixItem.cs`, `Properties/AssemblyInfo.cs`, `Resources.Designer.cs`, `TagController.cs`, `TagLauncher.cs`, `TagViewer.cs`, `TagViewer.Designer.cs`). Record to `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/baseline/precondition-no-seams.md`.
- [ ] [P0-T6] Grep the files this plan will touch (`Tags/TagController.cs`, `Tags/TagViewer.cs`, `Tags/TagLauncher.cs`, `Tags/Helper Classes/CheckBoxController.cs`, `Tags.Test/TagControllerTests.cs`, `Tags.Test/TagControllerCoverageExpansionTests.cs`) for banned APIs (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`) and record every hit (expected: `Task.Delay` in `Tags.Test/TagControllerCoverageExpansionTests.cs`) to `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/baseline/precondition-banned-apis.md`.
- [ ] [P0-T7] Run baseline `csharpier .` and write `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/baseline/baseline-csharpier.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P0-T8] Run baseline analyzer build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/baseline/baseline-analyzer.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P0-T9] Run baseline nullable/type-check build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/baseline/baseline-nullable.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P0-T10] Run baseline `vstest.console.exe Tags.Test/bin/Debug/Tags.Test.dll /EnableCodeCoverage`, copy the raw coverage output to `artifacts/csharp/coverage.xml`, and write `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/baseline/baseline-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` that records the numeric baseline `Tags` project line-coverage percentage and total tests passed.

### Phase 1 — Seam Interfaces and Host-Neutral Model

- [ ] [P1-T1] Create `Tags/ITagViewer.cs` declaring `public interface ITagViewer : UtilitiesCS.Interfaces.IWinForm.IForm` with the members from spec/research section B (command intent events `OkClicked`, `CancelClicked`, `NewClicked`, `AutoAssignClicked`, `SearchTextChanged`, `SearchKeyDown`, `SearchKeyUp`, `HideArchiveChanged`, `ViewKeyDown`, `OptionsPreviewKeyDown`, `OptionsKeyDown`; state properties `HideArchiveChecked`, `AutoAssignVisible`, `AutoAssignEnabled`, `ButtonNewVisible`, `SearchTextValue`, `SearchSelectionStart`, `Caption`; intent methods `CaptureAndRemoveTemplate`, `FocusSearch`, `AddOptionControl`, `RemoveOptionControl`, `OptionControls`, `OptionsPanelHeight`, `OptionsScrollMaximum`, `OptionsAutoScrollPosition`; and `void SetController(TagController controller)`) and add its `<Compile Include="ITagViewer.cs" />` to `Tags/Tags.csproj`. Acceptance: file exists, is wired into `Tags/Tags.csproj`, and STG passes.
- [ ] [P1-T2] Create `Tags/IUserPrompt.cs` declaring the seam `DialogResult ShowYesNo(string message, string title)`, `void ShowMessage(string message)`, and `string GetCategoryInput(string prompt, string title, string defaultResponse)`, and add `<Compile Include="IUserPrompt.cs" />` to `Tags/Tags.csproj`. Acceptance: file exists, wired, STG passes.
- [ ] [P1-T3] Create `Tags/WinFormsUserPrompt.cs` implementing `IUserPrompt` over `MessageBox.Show` and `InputBox.ShowDialog`, annotated `[ExcludeFromCodeCoverage]` (register E1) with an in-code justification comment, and add `<Compile Include="WinFormsUserPrompt.cs" />` to `Tags/Tags.csproj`. Acceptance: file exists, wired, STG passes.
- [ ] [P1-T4] Create `Tags/TagSelectionModel.cs` as a host-neutral class (zero WinForms references) owning `_dictOriginal`, `_dictOptions`, `_filteredOptions`, `_selections`, `_filteredSelections`, `_prefix` and the pure logic `Search`, `ParseSearchStrings`, `FilterArchive`, `IsPrefixMissing`, `SelectionAsList`, `SelectionAsString`, `GetSelections`, `ToggleChoice`/`On`/`Off`, the dictionary part of `AddOption`, `UpdateSelections`, `ResolvePrefix`, `GetDefaultPrefix`, the dictionary toggle of `LoadSelections`, and the filtered-set computation for `FilterToSelected`/`SearchAndReload`; add `<Compile Include="TagSelectionModel.cs" />` to `Tags/Tags.csproj`. Acceptance: file exists (<= 500 lines), wired, contains no `using System.Windows.Forms` and no Outlook Interop reference beyond the `IAutoAssign`/`IPrefix` interface parameters, and STG passes.

### Phase 2 — TagViewer Implements ITagViewer

- [ ] [P2-T1] Modify `Tags/TagViewer.cs` to declare `public partial class TagViewer : Form, ITagViewer`, implementing every `ITagViewer` intent member as a thin 1:1 mapping onto the existing designer controls (events forward the corresponding control events; `AddOptionControl`/`RemoveOptionControl`/`OptionControls` map to `L1v2L2_OptionsPanel.Controls`; `Caption` maps to `Text`; `CaptureAndRemoveTemplate` performs `ControlPosition.CreateTemplate(TemplateCheckBox)` and removes the template), keeping `SetController(TagController)` unchanged. Acceptance: `Tags/TagViewer.cs` <= 500 lines, all `ITagViewer` members implemented, STG passes (existing tests still pass because `TagController` is unchanged).

### Phase 3 — TagController Rewire and File Split

- [ ] [P3-T1] Rewire `Tags/TagController.cs` so the viewer field is typed `ITagViewer`: change the constructor parameter from `TagViewer` to `ITagViewer` (add an optional `IUserPrompt prompt = null` parameter defaulting via `prompt ??= new WinFormsUserPrompt()`), rewrite `WireEvents` to subscribe to the `ITagViewer` intent events, and replace every raw control access (`HideArchive.Checked`, `ButtonAutoAssign.Visible/Enabled`, `ButtonNew.Visible`, `SearchText.Text/SelectionStart/Focus/Select`, `L1v2L2_OptionsPanel.*`, `Text` caption, `Close()`) with the corresponding intent member. Acceptance: `Tags/TagController.cs` no longer references the concrete `TagViewer` type or any WinForms control property directly (except `CheckBox` row instances it creates), and STG passes.
- [ ] [P3-T2] Route all dialogs in `Tags/TagController.cs` through the injected `IUserPrompt`: replace `MessageBox.Show` in `LoadSelections`, `TryGetAutoAssignment`, and the three `LoadControls` error popups, replace `InputBox.ShowDialog` in `GetUserInputCategory`, and change `GetUserInputCategory` from `internal static` to an instance method routing through `_prompt` (its only caller `AddColorCategory` updated accordingly). Acceptance: `Tags/TagController.cs` contains no direct `MessageBox`/`InputBox` reference, and STG passes.
- [ ] [P3-T3] Replace `TagController`'s in-line selection/search/filter/prefix state in `Tags/TagController.cs` with delegation to a `TagSelectionModel` instance: remove the duplicated dictionaries/fields (including `_selections` at line 201 and `_filteredSelections` at line 202) and route `Search`, `ParseSearchStrings`, `FilterArchive`, `IsPrefixMissing`, `SelectionAsList`/`AsString`, `GetSelections`, `ToggleChoice`/`On`/`Off`, `AddOption`, `UpdateSelections`, `ResolvePrefix`/`GetDefaultPrefix`, and the dictionary toggle of `LoadSelections` through the model. Because this task removes the `_selections` and `_filteredSelections` fields, the existing un-migrated test `Tags.Test/TagControllerCoverageExpansionTests.cs::UpdateSelections_AfterFiltering_SynchronizesPrivateSelectionLists` (lines 233-238), which reflects into those exact fields via `GetPrivateField<IList<string>>(fixture.Controller, "_selections")` and `GetPrivateField<IList<string>>(fixture.Controller, "_filteredSelections")`, would fail its reflection lookups the moment this task's vstest gate runs. As part of this same task, update those two reflection assertions to read the selection lists from the controller's `TagSelectionModel` instance instead of the removed fields, so no reflection into a removed field survives when the P3-T3 STG test gate runs. (Verified unaffected: the `intFocus` and `_isMail` reflections stay on `TagController`; `Tags.Test/TagControllerTests.cs` reflects only `Control.InvokeOnClick`.) Acceptance: `Tags/TagController.cs` holds no selection/filter dictionaries of its own and delegates to `TagSelectionModel`; the two reflection assertions in `UpdateSelections_AfterFiltering_SynchronizesPrivateSelectionLists` read from the controller's `TagSelectionModel` instance rather than the removed `_selections`/`_filteredSelections` fields; and STG passes. Note: this specific test-touch is a mechanically-required part of the field relocation to keep the P3-T3 green-build invariant, and is distinct from the fuller migration of `Tags.Test/TagControllerCoverageExpansionTests.cs` to deterministic seams performed in P5-T3.
- [ ] [P3-T4] Create partial `Tags/TagController.Rendering.cs` and move `LoadControls`, `RemoveControls`, `FilterToSelected`, `Select_Ctrl_By_Offset`/`_By_Position`/`_First`/`_Last`/`_PageDown`/`_PageUp`, and `FocusCheckbox` into it; introduce a private `DrawFocus(CheckBox)` routed through an injectable `Action<CheckBox> _drawFocus` that defaults to the real `ControlPaint.DrawFocusRectangle(Graphics.FromHwnd(cbx.Handle), cbx.ClientRectangle)` call, with `[ExcludeFromCodeCoverage]` on the default-body method only (register E2); add `<Compile Include="TagController.Rendering.cs" />` to `Tags/Tags.csproj`. `Tags/TagController.cs` currently contains TWO `ControlPaint.DrawFocusRectangle(Graphics.FromHwnd(cbx.Handle), ...)` call sites (line 759 and line 862); BOTH HWND-forcing call sites MUST be consolidated into the single `DrawFocus(CheckBox)` seam routed through `_drawFocus`, so no direct `Graphics.FromHwnd`/`ControlPaint.DrawFocusRectangle` focus-draw call remains in the testable navigation arithmetic. Acceptance: `Tags/TagController.Rendering.cs` exists (<= 500 lines) as a partial of the same `TagController` class, wired into the csproj, the navigation arithmetic routes focus through `_drawFocus`, both former HWND-forcing call sites now invoke the single `DrawFocus(CheckBox)` seam with no residual direct HWND-forcing focus-draw call remaining, and STG passes.
- [ ] [P3-T5] Extract `internal async Task ButtonAutoAssign_Action()` in `Tags/TagController.cs` containing the awaited auto-assign body, and make the existing `async void ButtonAutoAssign_Click` handler call it. Acceptance: `Tags/TagController.cs` exposes `ButtonAutoAssign_Action` returning `Task`, the `async void` handler delegates to it, and STG passes.
- [ ] [P3-T6] Verify every production file in `Tags/` is <= 500 lines by counting lines of `Tags/TagController.cs`, `Tags/TagController.Rendering.cs`, `Tags/TagViewer.cs`, `Tags/TagSelectionModel.cs`, `Tags/ITagViewer.cs`, `Tags/IUserPrompt.cs`, `Tags/WinFormsUserPrompt.cs`, and record the counts to `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/qa-gates/file-size-compliance.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: no listed file exceeds 500 lines; if any does, split further before proceeding.

### Phase 4 — LauncherAutoAssign Extraction and Caller Updates

- [ ] [P4-T1] Create `Tags/LauncherAutoAssign.cs`, moving the inner `LauncherAutoAssign` class and the pure `GetAutoAssign(filterList, delegates...)` factory out of `Tags/TagLauncher.cs` WITHOUT `[ExcludeFromCodeCoverage]` (pure delegate pass-through: `AddChoicesToDict`/`AddColorCategory`/`AutoFind`/`AutoFindAsync`/`FilterList`); add `<Compile Include="LauncherAutoAssign.cs" />` to `Tags/Tags.csproj`. Acceptance: `Tags/LauncherAutoAssign.cs` exists (<= 500 lines), carries no coverage exemption, is wired into the csproj, and STG passes.
- [ ] [P4-T2] Update `Tags/TagLauncher.cs` to remove the moved members, retaining only the live-form launcher and globals/COM wiring under a maintainer-ratified `[ExcludeFromCodeCoverage]` (register E5); confirm its `new TagController(...)` call sites compile unchanged via the `TagViewer -> ITagViewer` implicit upcast and that `TagLauncher` does not need to pass `IUserPrompt`. Acceptance: `Tags/TagLauncher.cs` <= 500 lines, no longer declares `LauncherAutoAssign`, and STG passes.

### Phase 5 — Existing Test Migration to Deterministic Seams

- [ ] [P5-T1] Create `Tags.Test/Fakes/FakeTagViewer.cs`, an in-memory `ITagViewer` fake with a backing `List<CheckBox>` for `OptionControls`/`AddOptionControl`/`RemoveOptionControl`, settable `SearchTextValue`/`SearchSelectionStart`/`Caption`/`AutoAssign*`/`ButtonNewVisible`, and raisable intent events; add its `<Compile Include>` to `Tags.Test/Tags.Test.csproj`. Acceptance: file exists, wired, constructs no live `Form`/`Control` beyond headless `CheckBox` property bags, and STG passes.
- [ ] [P5-T2] Migrate `Tags.Test/TagControllerTests.cs` to construct `TagController` with `FakeTagViewer` (or a Moq `ITagViewer`), a Moq `IUserPrompt`, and a no-op `DrawFocus`; remove `[STAThread]` and `new TagViewer()` live-form construction and replace live control-tree walking with assertions against `TagSelectionModel` state or `FakeTagViewer.OptionControls`. Acceptance: `Tags.Test/TagControllerTests.cs` constructs no live `TagViewer`/`Form`, preserves or strengthens every prior behavioral assertion, and STG passes.
- [ ] [P5-T3] Migrate `Tags.Test/TagControllerCoverageExpansionTests.cs`: replace `CreateFixture`/`CreateAutoAssignFixture` live `TagViewer` with `FakeTagViewer`, inject Moq `IUserPrompt` and no-op `DrawFocus`, replace `GetVisibleOptionCheckBoxes`/`FindNamedControl` with `FakeTagViewer.OptionControls`, remove `[STAThread]`, and remediate the banned `Task.Delay(50)` by awaiting the extracted `ButtonAutoAssign_Action()` directly. Acceptance: `Tags.Test/TagControllerCoverageExpansionTests.cs` contains no `Task.Delay`, no `[STAThread]`, and no live-form construction; behavioral assertions are preserved or strengthened; and STG passes.

### Phase 6 — New Coverage Tests

- [ ] [P6-T1] Create `Tags.Test/TagSelectionModelTests.cs` covering `ParseSearchStrings`, `Search`, `FilterArchive` (Moq `IAutoAssign`), `IsPrefixMissing`, `SelectionAsList`/`AsString`/`GetSelections`, `ToggleChoice`/`AddOption`/`UpdateSelections`, and `ResolvePrefix`/`GetDefaultPrefix` (including the unknown-key `ArgumentException` edge) with positive, negative, and edge scenarios; add its `<Compile Include>` to `Tags.Test/Tags.Test.csproj`. Acceptance: file exists, wired, `TagSelectionModel` reaches >= 90% line coverage, and STG passes.
- [ ] [P6-T2] Create `Tags.Test/LauncherAutoAssignTests.cs` covering each `LauncherAutoAssign` pass-through method with Moq delegates (including `AutoFindAsync` awaiting the sync delegate); add its `<Compile Include>` to `Tags.Test/Tags.Test.csproj`. Acceptance: file exists, wired, `LauncherAutoAssign` reaches >= 90% line coverage, and STG passes.
- [ ] [P6-T3] Create `Tags.Test/TagControllerSeamTests.cs` covering the controller methods listed in research section H that are not already exercised: dialog-driven `LoadSelections`/`TryGetAutoAssignment`/`AddColorCategory`/`GetUserInputCategory` via Moq `IUserPrompt`; keyboard handlers `OptionsPanel_PreviewKeyDown`/`OptionsPanel_KeyDown`/`TagViewer_KeyDown`/`SearchText_KeyDown`/`KeyUp`; navigation `Select_Ctrl_By_Offset`/`_By_Position`/`_First`/`_Last`/`_PageDown`/`_PageUp` with `FakeTagViewer` rows and no-op `DrawFocus` (including the out-of-range `ArgumentOutOfRangeException` edge); rendering `LoadControls`/`RemoveControls`/`FilterToSelected`; property forwarders `SetAutoAssignState`/`SetCaption`/`SetSearchText`; and `ButtonAutoAssign_Action`; add its `<Compile Include>` to `Tags.Test/Tags.Test.csproj`. Acceptance: file exists, wired, no live form/popup, `TagController` reaches >= 80% line coverage, and STG passes.
- [ ] [P6-T4] Extract the `CheckBoxController` click state-machine decision logic (`TrigByKeyChg`/`TrigByValChg` + `Tag`/`Text`/prefix -> `strTemp`) in `Tags/Helper Classes/CheckBoxController.cs` into a host-neutral testable helper method, narrow the `[ExcludeFromCodeCoverage]` to the event-wiring members only (register E6), and add `Tags.Test/CheckBoxControllerDecisionTests.cs` covering the extracted helper; add its `<Compile Include>` to `Tags.Test/Tags.Test.csproj`. Acceptance: the extracted decision helper is not exempt and is covered by tests, only event-wiring members remain exempt in `Tags/Helper Classes/CheckBoxController.cs`, and STG passes. (If the maintainer descopes this extraction, E6 must be re-ratified as a blanket exemption before this task may be marked complete.)

### Phase 7 — Final QA Loop and Coverage Verification

- [ ] [P7-T1] Run `csharpier .` and write `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/qa-gates/final-csharpier.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If the step rewrites any file, restart the QA loop from this task.
- [ ] [P7-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/qa-gates/final-analyzer.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P7-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/qa-gates/final-nullable.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P7-T4] Run `vstest.console.exe Tags.Test/bin/Debug/Tags.Test.dll /EnableCodeCoverage`, copy the raw coverage output to `artifacts/csharp/coverage.xml`, and write `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/qa-gates/final-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` recording the numeric post-change `Tags` project line-coverage percentage and total tests passed.
- [ ] [P7-T5] Compute and record the coverage comparison in `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/qa-gates/coverage-delta.md`: baseline `Tags` line coverage (from P0-T10), post-change `Tags` line coverage (from P7-T4), the new-module line coverage for `TagSelectionModel` and `LauncherAutoAssign`, and a no-regression-on-changed-lines statement. Acceptance: `Tags` project >= 80%, `TagSelectionModel` >= 90%, `LauncherAutoAssign` >= 90%, no changed-line regression; if any threshold is unmet the outcome is remediation-required (add tests and restart the QA loop), never PASS.
- [ ] [P7-T6] Grep `Tags.Test/` for `new TagViewer(`, `new Form(`, `.ShowDialog(`, `MessageBox.`, `InputBox.`, `[STAThread]`, `Thread.Sleep`, `Task.Delay`, and temp-file APIs, and write `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/qa-gates/determinism-scan.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: no live-form construction, no popup, no `Thread.Sleep`/`Task.Delay`, and no temp-file usage remain in `Tags.Test/`.
- [ ] [P7-T7] Confirm one clean full QA-loop pass (P7-T1 through P7-T4 with no residual file change on the final iteration) and write the consolidated final-QA summary to `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/qa-gates/final-qa-summary.md` with `Timestamp:`, the exact commands run in order, each `EXIT_CODE:`, and an `Output Summary:` mapping each satisfied acceptance criterion from `spec.md` `### Acceptance Criteria` to its evidence artifact.

## Work-Mode AC Source Exception

This feature declares `Work Mode: full-feature`, whose acceptance-criteria resolution normally requires both `spec.md` AND `user-story.md`. For this refactor child of epic winforms-testability-refactor (#295), `user-story.md` is intentionally absent and is WAIVED per the epic manifest (`docs/features/epics/winforms-testability-refactor/epic.md`, Design-Phase Deliverables). Do NOT create `user-story.md`.

- AC sources for this feature are `spec.md` `### Acceptance Criteria` + `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/issue.md` `## Acceptance Criteria` ONLY.
- `user-story.md` is waived per epic #295 manifest; its absence MUST NOT be treated as a blocker by downstream AC tracking or feature-review.
- This exception is mirrored as a one-line marker in `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/issue.md` metadata for machine visibility.

## Acceptance Criteria Traceability

| Spec/issue AC (#293) | Satisfying tasks |
|---|---|
| `ITagViewer : IForm` exists with required members; `TagViewer` implements it | P1-T1, P2-T1 |
| `TagController` depends on `ITagViewer`, not concrete `TagViewer` | P3-T1 |
| Host-neutral logic separated from COM/WinForms | P1-T4, P3-T2, P3-T3, P3-T4 |
| No production file exceeds 500 lines | P3-T4, P3-T6, P4-T1, P4-T2 |
| Unit tests cover named methods without real WinForms objects; seams introduced | P1-T2, P1-T3, P3-T4, P5-*, P6-* |
| `TagController` (and extracted logic) >= 80% line coverage | P6-T1, P6-T3, P7-T5 |
| `Tags` project >= 80% line coverage (incl. `TagLauncher`/`CheckBoxController` as needed) | P4-T1, P6-T2, P6-T4, P7-T5 |
| No test constructs a live form/window or triggers a popup | P5-*, P6-*, P7-T6 |
| Full C# toolchain passes with no regression | STG on every Phase 1–6 task; P7-T1..T7 |

## Preflight

DIRECTIVE: PREFLIGHT VALIDATION ONLY — this plan is submitted for validation-only preflight through `atomic-executor`, reusing this exact `plan-path` for any revision iteration. The planner does not execute implementation and does not spawn nested workers.
