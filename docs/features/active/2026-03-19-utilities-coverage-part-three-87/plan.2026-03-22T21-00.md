# 2026-03-19-utilities-coverage-part-three - Plan

- **Issue:** #87
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-22
- **Status:** In Progress
- **Version:** 1.2

## Required References

- General Coding Standards: [`.github/instructions/general-code-change.instructions.md`](../../../../.github/instructions/general-code-change.instructions.md)
- General Unit Test Policy: [`.github/instructions/general-unit-test.instructions.md`](../../../../.github/instructions/general-unit-test.instructions.md)
- C# Code Change Policy: [`.github/instructions/csharp-code-change.instructions.md`](../../../../.github/instructions/csharp-code-change.instructions.md)
- C# Unit Test Policy: [`.github/instructions/csharp-unit-test.instructions.md`](../../../../.github/instructions/csharp-unit-test.instructions.md)
- Spec: [`spec.md`](spec.md)
- User Story: [`user-story.md`](user-story.md)
- Research: [`../../../../artifacts/research/20260319-utilities-coverage-part-three-87-research.md`](../../../../artifacts/research/20260319-utilities-coverage-part-three-87-research.md)

**All work must comply with these policies; do not duplicate their content here.**

## Overview

Raise every production `.cs` file compiled by `UtilitiesCS.csproj` to >= 80% line coverage by adding or extending MSTest unit tests in `UtilitiesCS.Test`, with evidence-backed skip evaluation only where repo policy and deterministic testability constraints make the 80% target unattainable. Work is phased by testability difficulty (Easy → Medium → Hard), now preceded by an explicit reconciliation gate that maps every currently sub-80 non-skip file to either a remaining implementation task or a Phase 4 skip task before further execution resumes. After the latest reconciliation pass, every remaining unchecked implementation task lists only implementation-routed files, and every Phase 4 constrained skip batch mirrors the reconciliation ledger exactly. Approximately 155 files have explicit line-rate below 80% in the Cobertura report, plus ~16 `Designer.cs` files, ~4 commented stubs, and ~40+ pure interface files with no executable code.

## Acceptance Criteria Traceability

| AC | Source (issue.md) | Plan Coverage |
|---|---|---|
| AC1 | Every .cs file compiled by UtilitiesCS.csproj >= 80% line coverage | P0-T5 through P0-T6 reconciliation + P1–P3 implementation tasks + P4-T1 through P4-T39 skip evaluation + P5-T5 verification |
| AC2 | No pre-existing tests broken or removed | P0-T3 baseline + P5-T6 verification |
| AC3 | All new tests follow MSTest + Moq + FluentAssertions conventions | P0-T1 policy read + all P1–P3 implementation tasks |
| AC4 | All new tests deterministic, isolated, no external deps | P0-T1 policy read + all P1–P3 implementation tasks |
| AC5 | All new test files registered in UtilitiesCS.Test.csproj | P1-T13, P2-T24, P3-T68 registration tasks |
| AC6 | C# toolchain loop passes clean | P5-T1 through P5-T4 |
| AC7 | Repo-wide coverage does not regress below baseline | P0-T3 baseline + P5-T5 comparison |

## Implementation Plan (Atomic Tasks)

### Phase 0 — Compliance & Baseline Capture

- [ ] [P0-T1] Read all repo policy files in required order: `.github/copilot-instructions.md`, `general-code-change.instructions.md`, `general-unit-test.instructions.md`, `csharp-code-change.instructions.md`, `csharp-unit-test.instructions.md`
  - Acceptance: Evidence artifact at `evidence/baseline/phase0-instructions-read.md` contains `Timestamp:`, `Policy Order:`, and explicit list of all five files read

- [ ] [P0-T2] Capture baseline build state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-build.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [ ] [P0-T3] Capture baseline test results with coverage by running `vstest.console.exe` with `/EnableCodeCoverage` over all `*.Test.dll` assemblies
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-test-coverage.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` including total test count, pass count, and repo-wide UtilitiesCS line coverage percentage

- [ ] [P0-T4] Record per-file baseline coverage for all UtilitiesCS production files below 80% line rate from the current `coverage/coverage.cobertura.xml`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-per-file-coverage.md` lists each file with its current line-rate percentage, categorized by difficulty (Easy/Medium/Hard/Skip)

- [ ] [P0-T5] Reconcile every currently sub-80 non-skip UtilitiesCS file from `evidence/qa-gates/final-coverage-verification.md` against the remaining plan and `evidence/other/skip-candidates.md`
  - Acceptance: Evidence artifact at `evidence/baseline/remaining-sub80-reconciliation.md` contains one row for every file listed under "Non-Skip UtilitiesCS Files Below 80%" in `evidence/qa-gates/final-coverage-verification.md`, and each row maps the file to exactly one remaining task path: `Implementation Task` or `Phase 4 Skip Task`

- [ ] [P0-T6] Verify the revised checklist state matches the reconciliation matrix before additional implementation resumes
  - Preconditions: P0-T5 complete
  - Acceptance: Every file mapped to `Implementation Task` in `evidence/baseline/remaining-sub80-reconciliation.md` references an unchecked P1/P2/P3 task ID, every file mapped to `Phase 4 Skip Task` references an unchecked P4 task ID, and no checked task still depends on a file that remains below 80% in `evidence/qa-gates/final-coverage-verification.md`

### Phase 1 — FolderNotFoundViewer Coverage (`UtilitiesCS\Dialogs\FolderNotFoundViewer.cs`)

- [ ] [P1-T1] Add test to `UtilitiesCS.Test\Dialogs\FolderNotFoundViewer_Tests.cs` verifying that clicking the save-style action button sets `FolderAction` to the expected keep/save enum value
  - Acceptance: `[TestMethod]` exists in `FolderNotFoundViewer_Tests.cs`, creates a `FolderNotFoundViewer` instance on an STA thread, invokes the save button click handler, and asserts `FolderAction` equals the expected save enum value

- [ ] [P1-T2] Add test to `UtilitiesCS.Test\Dialogs\FolderNotFoundViewer_Tests.cs` verifying that clicking the discard-style action button sets `FolderAction` to the expected discard/remove enum value
  - Acceptance: `[TestMethod]` exists, invokes the discard button click handler, and asserts `FolderAction` equals the expected discard enum value

- [ ] [P1-T3] Add test to `UtilitiesCS.Test\Dialogs\FolderNotFoundViewer_Tests.cs` verifying that `FolderName` property returns the backing folder-name text correctly
  - Acceptance: `[TestMethod]` exists, assigns a known string to the backing field or constructor, and asserts `FolderName` returns that exact string

- [ ] [P1-T4] Add test to `UtilitiesCS.Test\Dialogs\FolderNotFoundViewer_Tests.cs` verifying that the viewer calls `Hide` rather than `Dispose` when an action button is activated
  - Acceptance: `[TestMethod]` exists, invokes the action click handler, and asserts the viewer instance is not disposed after the call

- [ ] [P1-T5] Register `UtilitiesCS.Test\Dialogs\FolderNotFoundViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Dialogs\FolderNotFoundViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 2 — InputBox Coverage (`UtilitiesCS\Dialogs\InputBox.cs`)

- [ ] [P2-T1] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that the default response value populates the viewer's textbox state when the dialog is initialized
  - Acceptance: `[TestMethod]` exists in `InputBox_Test.cs`, creates an `InputBoxViewer` with a known default response string, and asserts the textbox text equals that default string

- [ ] [P2-T2] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that accepting the dialog (OK path) returns the text entered in the textbox
  - Acceptance: `[TestMethod]` exists, sets the viewer textbox to a known string, triggers the OK path, and asserts the returned value equals the entered text

- [ ] [P2-T3] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that cancelling the dialog returns `null`
  - Acceptance: `[TestMethod]` exists, triggers the cancel path on the viewer, and asserts the return value is `null`

### Phase 3 — InputBoxViewer Coverage (`UtilitiesCS\Dialogs\InputBoxViewer.cs`)

- [ ] [P3-T1] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that `Ok_Click` copies the textbox text to the response field and closes the viewer
  - Acceptance: `[TestMethod]` exists, sets the textbox text on a direct `InputBoxViewer` instance, calls `Ok_Click`, and asserts the response field equals the textbox text and the viewer is no longer visible

- [ ] [P3-T2] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that `Cancel_Click` clears the response field
  - Acceptance: `[TestMethod]` exists, calls `Cancel_Click` on a direct `InputBoxViewer` instance, and asserts the response field is `null` or empty

- [ ] [P3-T3] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that `DpiAware` property and `DpiCalled` static flag toggle their expected state
  - Acceptance: `[TestMethod]` resets `DpiCalled` to its default, sets `DpiAware`, and asserts `DpiCalled` reflects the expected toggled value; static state is reset in `TestCleanup`

### Phase 4 — MyBox Coverage (`UtilitiesCS\Dialogs\MyBox.cs`)

- [ ] [P4-T1] Add test to `UtilitiesCS.Test\Dialogs\MyBox_Tests.cs` verifying that button conversion preserves dialog result ordering when standard buttons are mapped to custom equivalents
  - Acceptance: `[TestMethod]` exists in `MyBox_Tests.cs`, calls the button-conversion helper with a known set of standard buttons, and asserts the output sequence preserves expected `DialogResult` order

- [ ] [P4-T2] Add test to `UtilitiesCS.Test\Dialogs\MyBox_Tests.cs` verifying that the button replacement helper swaps custom buttons into the viewer correctly
  - Acceptance: `[TestMethod]` exists, supplies a custom button list to the replacement helper, and asserts the viewer's button collection contains the custom buttons

- [ ] [P4-T3] Add test to `UtilitiesCS.Test\Dialogs\MyBox_Tests.cs` verifying that `FunctionButtonGroup<T>` routing returns the mapped value for each button entry
  - Acceptance: `[TestMethod]` exists, creates a `FunctionButtonGroup<T>` binding with a known mapping, triggers the delegate, and asserts the returned value equals the expected mapped result

- [ ] [P4-T4] Register `UtilitiesCS.Test\Dialogs\MyBox_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Dialogs\MyBox_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 5 — NotImplementedDialog Coverage (`UtilitiesCS\Dialogs\NotImplementedDialog.cs`)

- [ ] [P5-T1] Add test to `UtilitiesCS.Test\Dialogs\NotImplementedDialog_Tests.cs` verifying that when `StopAtNotImplemented` is `true` the not-implemented trigger path throws the expected exception
  - Acceptance: `[TestMethod]` exists, sets `StopAtNotImplemented = true` via reflection or public API, invokes the trigger path, and asserts the expected exception type is thrown using FluentAssertions `.Should().Throw<>()`

- [ ] [P5-T2] Add test to `UtilitiesCS.Test\Dialogs\NotImplementedDialog_Tests.cs` verifying that when `StopAtNotImplemented` is `false` the trigger path completes without throwing
  - Acceptance: `[TestMethod]` exists, sets `StopAtNotImplemented = false`, invokes the trigger path, and asserts no exception is thrown (method returns normally)

- [ ] [P5-T3] Add `[TestCleanup]` method to `NotImplementedDialog_Tests.cs` that resets `StopAtNotImplemented` to its original value after each test to prevent static state pollution
  - Acceptance: `[TestInitialize]`-annotated method captures the original flag, `[TestCleanup]`-annotated method restores it, and both methods exist in the test class

- [ ] [P5-T4] Register `UtilitiesCS.Test\Dialogs\NotImplementedDialog_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Dialogs\NotImplementedDialog_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 6 — SKIP_EVALUATION: ConfusionViewer (`UtilitiesCS\EmailIntelligence\Bayesian\Performance\ConfusionViewer.cs`)

- [ ] [P6-T1] Record skip-evaluation decision for `ConfusionViewer.cs` in plan notes
  - Acceptance: This task is marked complete to document that `ConfusionViewer.cs` is a constructor-only WinForms designer shell with no meaningful non-designer logic; no test file will be created for this file

### Phase 7 — SKIP_EVALUATION: MetricChartViewer (`UtilitiesCS\EmailIntelligence\Bayesian\Performance\MetricChartViewer.cs`)

- [ ] [P7-T1] Record skip-evaluation decision for `MetricChartViewer.cs` in plan notes
  - Acceptance: This task is marked complete to document that `MetricChartViewer.cs` is a constructor-only WinForms designer shell with no meaningful non-designer logic; no test file will be created for this file

### Phase 8 — AutoFile Coverage (`UtilitiesCS\EmailIntelligence\EmailParsingSorting\AutoFile.cs`)

- [ ] [P8-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\AutoFile_Tests.cs` verifying that `AreConversationsGrouped` returns `true` when category and state inputs indicate grouped conversations
  - Acceptance: `[TestMethod]` exists, constructs synthetic category/state inputs using mocked Outlook objects, calls `AreConversationsGrouped`, and asserts the return value is `true`

- [ ] [P8-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\AutoFile_Tests.cs` verifying that category-selection guard does not duplicate an already-selected category
  - Acceptance: `[TestMethod]` exists, builds a collection that already contains the target category, invokes category selection, and asserts the collection size is unchanged and the category appears exactly once

- [ ] [P8-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\AutoFile_Tests.cs` verifying that `AutoFindPeople` selects the expected person candidate from a synthetic collection
  - Acceptance: `[TestMethod]` exists, passes a synthetic person collection with a single unambiguous match, calls `AutoFindPeople`, and asserts the returned candidate equals the expected value

- [ ] [P8-T4] Register `UtilitiesCS.Test\EmailIntelligence\AutoFile_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\AutoFile_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 9 — SortEmail Coverage (`UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs`)

- [ ] [P9-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs` verifying that `InitializeSortToExisting` throws `NotImplementedException`
  - Acceptance: `[TestMethod]` exists, invokes `InitializeSortToExisting`, and asserts a `NotImplementedException` is thrown using FluentAssertions `.Should().Throw<NotImplementedException>()`

- [ ] [P9-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs` verifying that `ProcessMailItemAsync` short-circuits without proceeding to filing logic when the mail item input is null
  - Acceptance: `[TestMethod]` exists, passes `null` as the mail item, awaits `ProcessMailItemAsync`, and asserts no filing side-effects were triggered (mocked engine manager receives no file calls)

- [ ] [P9-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs` verifying that both `SortAsync` overloads delegate to the same core processing path via the engine manager
  - Acceptance: `[TestMethod]` exists, invokes each overload with mocked engine manager, and asserts the expected core processing method was called exactly once per overload

- [ ] [P9-T4] Register `UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\SortEmail_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 10 — FilterOlFoldersController Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersController.cs`)

- [ ] [P10-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` verifying that `Save` forwards the save action to the backing model
  - Acceptance: `[TestMethod]` exists, calls `Save` on the controller with a Moq-mocked backing model, and asserts the model's save method was invoked exactly once

- [ ] [P10-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` verifying that `Discard` forwards the discard action to the backing model
  - Acceptance: `[TestMethod]` exists, calls `Discard` on the controller with a Moq-mocked backing model, and asserts the model's discard method was invoked exactly once

- [ ] [P10-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` verifying that a tree property change propagates to the viewer-facing state
  - Acceptance: `[TestMethod]` exists, triggers a property-changed event on the mocked tree, and asserts the controller's viewer-facing state reflects the updated value

- [ ] [P10-T4] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` verifying that the check-state helpers round-trip the expected value
  - Acceptance: `[TestMethod]` exists, sets a check-state value via the setter, reads it back via the getter, and asserts the retrieved value equals the value originally set

- [ ] [P10-T5] Register `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FilterOlFoldersController_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 11 — FilterOlFoldersViewer Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersViewer.cs`)

- [ ] [P11-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs` verifying that `SetController` registers the expected delegates on a Moq-mocked controller
  - Acceptance: `[TestMethod]` exists, calls `SetController` with a mocked controller, and asserts the expected event/delegate registrations were performed on the mock

- [ ] [P11-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs` verifying that `FormatFileSize` returns the expected string for a byte-range input (less than 1 KB)
  - Acceptance: `[TestMethod]` exists, calls `FormatFileSize` with a value less than 1,024, and asserts the return value matches the expected byte-formatted string

- [ ] [P11-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs` verifying that `FormatFileSize` returns the expected string for a KB-or-larger input
  - Acceptance: `[TestMethod]` exists, calls `FormatFileSize` with a value of 1,024 or more, and asserts the return value matches the expected KB/MB-formatted string

- [ ] [P11-T4] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs` verifying that the viewer's save and discard buttons forward their events to the corresponding controller methods
  - Acceptance: `[TestMethod]` exists, triggers save and discard button clicks or event handlers, and asserts the mocked controller's `Save` and `Discard` methods were each invoked exactly once

- [ ] [P11-T5] Register `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FilterOlFoldersViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 12 — FolderInfoViewer Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FolderInfoViewer.cs`)

- [ ] [P12-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderInfoViewer_Tests.cs` verifying that `SetFolderTree` updates the `FolderTree` property to the assigned reference
  - Acceptance: `[TestMethod]` exists, calls `SetFolderTree` with a non-null argument, and asserts `FolderTree` returns the same reference that was assigned

- [ ] [P12-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderInfoViewer_Tests.cs` verifying that assigning a new tree reference via `SetFolderTree` replaces the prior reference
  - Acceptance: `[TestMethod]` exists, assigns an initial tree reference, then assigns a second distinct reference, and asserts `FolderTree` returns the most recent assignment

- [ ] [P12-T3] Register `UtilitiesCS.Test\EmailIntelligence\FolderInfoViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FolderInfoViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 13 — OSBrowser Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\OSBrowser.cs`)

- [ ] [P13-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs` verifying that the column setup method initializes the expected number and names of columns
  - Acceptance: `[TestMethod]` exists, invokes the column-setup method, and asserts the column collection contains the expected count and identifiers

- [ ] [P13-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs` verifying that the tree setup method configures the expected tree options
  - Acceptance: `[TestMethod]` exists, invokes the tree-setup method on a direct form instance, and asserts the expected tree option flags are set

- [ ] [P13-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs` verifying that `FormatFileSize` returns the expected string for a bytes-range input (less than 1 KB)
  - Acceptance: `[TestMethod]` exists, calls `FormatFileSize` with a value below 1,024, and asserts the return value ends with the expected byte-unit label

- [ ] [P13-T4] Add test to `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs` verifying that `FormatFileSize` returns the expected string for a KB-range input and for an MB-range input
  - Acceptance: `[TestMethod]` exists, calls `FormatFileSize` with a value of 1,024 and a value of 1,048,576, and asserts each return value ends with the correct unit label (KB or MB respectively)

- [ ] [P13-T5] Register `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\OSBrowser_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 14 — FolderRemapController Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapController.cs`)

- [ ] [P14-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` verifying that a simulated drag/drop operation updates the mapping entry in the mocked remap tree
  - Acceptance: `[TestMethod]` exists, triggers the drag/drop handler with synthetic folder-node arguments, and asserts the expected mapping change is applied to the mocked tree/model

- [ ] [P14-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` verifying that `Save` forwards the save action to the backing model
  - Acceptance: `[TestMethod]` exists, calls `Save`, and asserts the mocked backing model's save method was invoked once

- [ ] [P14-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` verifying that `Discard` forwards the discard action to the backing model
  - Acceptance: `[TestMethod]` exists, calls `Discard`, and asserts the mocked backing model's discard method was invoked once

- [ ] [P14-T4] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` verifying that `ExpandTo` selects the correct folder node path in the mocked tree
  - Acceptance: `[TestMethod]` exists, calls `ExpandTo` with a synthetic node identifier, and asserts the mocked tree's selection matches the expected node path

- [ ] [P14-T5] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` verifying that `SyncGlobalMap` propagates expected mapping changes to the global state
  - Acceptance: `[TestMethod]` exists, sets up a local mapping, calls `SyncGlobalMap`, and asserts the global mapping reflects the locally applied changes

- [ ] [P14-T6] Register `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FolderRemapController_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 15 — FolderRemapViewer Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapViewer.cs`)

- [ ] [P15-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapViewer_Tests.cs` verifying that the viewer forwards a drag/drop event to the mocked controller
  - Acceptance: `[TestMethod]` exists, triggers the drag/drop event on the viewer, and asserts the mocked controller's corresponding handler was invoked exactly once

- [ ] [P15-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapViewer_Tests.cs` verifying that the viewer's setup methods establish the expected initial renderer and tree state
  - Acceptance: `[TestMethod]` exists, calls the setup method, and asserts the expected renderer type is applied and the tree's initial configuration matches the expected values

- [ ] [P15-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapViewer_Tests.cs` verifying that the file-size formatting helper returns the expected string for a sample input
  - Acceptance: `[TestMethod]` exists, calls the file-size formatting helper with a known value, and asserts the return string matches the expected formatted representation

- [ ] [P15-T4] Register `UtilitiesCS.Test\EmailIntelligence\FolderRemapViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FolderRemapViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 16 — FolderSelector Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderSelector.cs`)

- [ ] [P16-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderSelector_Tests.cs` verifying that initialization sets the expected selection source reference
  - Acceptance: `[TestMethod]` exists, instantiates `FolderSelector` with a fake folder-tree source, and asserts the stored source reference equals the provided input

- [ ] [P16-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderSelector_Tests.cs` verifying that confirming a selection sets `Selection` to the chosen folder node
  - Acceptance: `[TestMethod]` exists, simulates a completed selection by setting the expected node state, and asserts the `Selection` property returns the expected node/folder reference

- [ ] [P16-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderSelector_Tests.cs` verifying that passing a null/empty input leaves `Selection` as null
  - Acceptance: `[TestMethod]` exists, calls the relevant path with null or empty source, and asserts `Selection` is null after the call

- [ ] [P16-T4] Register `UtilitiesCS.Test\EmailIntelligence\FolderSelector_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FolderSelector_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 17 — SubjectMapEncoder Coverage (`UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`)

- [ ] [P17-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapEncoder_Tests.cs` verifying that `RebuildEncoding` builds symmetric encode/decode maps
  - Acceptance: `[TestMethod]` exists, calls `RebuildEncoding` with a known token list, and asserts each token maps forward and backward correctly (encode[token] → id, decode[id] → token)

- [ ] [P17-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapEncoder_Tests.cs` verifying that `AugmentTokenDict` appends only unseen tokens
  - Acceptance: `[TestMethod]` exists, calls `AugmentTokenDict` with a mix of existing and new tokens, and asserts only the new tokens are added while existing entries are unchanged

- [ ] [P17-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapEncoder_Tests.cs` verifying that `Encode` followed by `Decode` round-trips the original terms
  - Acceptance: `[TestMethod]` exists, encodes a known term sequence and then decodes the result, and asserts the decoded output matches the original input

- [ ] [P17-T4] Register `UtilitiesCS.Test\EmailIntelligence\SubjectMapEncoder_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\SubjectMapEncoder_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 18 — SubjectMapMetrics Coverage (`UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapMetrics.cs`)

- [ ] [P18-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapMetrics_Tests.cs` verifying that the primary constructor copies expected counts and rates into `DlvMetrics`
  - Acceptance: `[TestMethod]` exists, constructs `SubjectMapMetrics` with known numeric inputs, and asserts the corresponding `DlvMetrics` properties hold the expected values

- [ ] [P18-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapMetrics_Tests.cs` verifying that alternate constructor overloads produce equivalent state to the primary constructor
  - Acceptance: `[TestMethod]` exists, constructs instances via two different overloads with equivalent inputs, and asserts the resulting `DlvMetrics` properties are equal across both instances

- [ ] [P18-T3] Register `UtilitiesCS.Test\EmailIntelligence\SubjectMapMetrics_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\SubjectMapMetrics_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 19 — DfDeedle Coverage (`UtilitiesCS\Extensions\DfDeedle.cs`)

- [ ] [P19-T1] Add test to `UtilitiesCS.Test\Extensions\DfDeedle_Tests.cs` verifying that a 2D email array converts to a DataFrame with the expected row count and column layout
  - Acceptance: `[TestMethod]` exists, passes a small in-memory 2D array to the conversion method, and asserts the returned frame has the expected number of rows and correctly named columns

- [ ] [P19-T2] Add test to `UtilitiesCS.Test\Extensions\DfDeedle_Tests.cs` verifying that invalid triage values are filtered out from the DataFrame
  - Acceptance: `[TestMethod]` exists, constructs a frame containing invalid triage entries, calls the filter method, and asserts the result excludes rows with invalid triage values

- [ ] [P19-T3] Add test to `UtilitiesCS.Test\Extensions\DfDeedle_Tests.cs` verifying that date extraction handles null and invalid date slots without throwing
  - Acceptance: `[TestMethod]` exists, calls the date extraction path with null and unparseable date values, and asserts the method returns null/default rather than throwing

- [ ] [P19-T4] Register `UtilitiesCS.Test\Extensions\DfDeedle_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Extensions\DfDeedle_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 20 — DvgForm Coverage (`UtilitiesCS\HelperClasses\DvgForm.cs`)

- [ ] [P20-T1] Add test to `UtilitiesCS.Test\HelperClasses\DvgForm_Tests.cs` verifying that triggering resize-end invokes expected layout behavior without throwing
  - Acceptance: `[TestMethod]` exists, instantiates `DvgForm` and triggers the resize-end event path, and asserts no exception is thrown and the expected layout side effect occurs

- [ ] [P20-T2] Register `UtilitiesCS.Test\HelperClasses\DvgForm_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\DvgForm_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 21 — QfcTipsDetails Coverage (`UtilitiesCS\HelperClasses\ToolTips\QfcTipsDetails.cs`)

- [ ] [P21-T1] Add test to `UtilitiesCS.Test\HelperClasses\QfcTipsDetails_Tests.cs` verifying that parent-type resolution returns the expected enum/type value
  - Acceptance: `[TestMethod]` exists, invokes the parent-type resolution path with a known parent stub, and asserts the returned type/enum value matches the expected case

- [ ] [P21-T2] Add test to `UtilitiesCS.Test\HelperClasses\QfcTipsDetails_Tests.cs` verifying that `InitializeAsync` populates expected labels and toggle state
  - Acceptance: `[TestMethod]` exists, calls the initialization path on a direct instance, and asserts the detail labels and toggle properties hold the expected post-initialization values

- [ ] [P21-T3] Add test to `UtilitiesCS.Test\HelperClasses\QfcTipsDetails_Tests.cs` verifying that visibility toggle methods update internal state consistently
  - Acceptance: `[TestMethod]` exists, calls a visibility toggle method and asserts the relevant internal state property reflects the toggled value; calling the same toggle again restores the previous state

- [ ] [P21-T4] Register `UtilitiesCS.Test\HelperClasses\QfcTipsDetails_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\QfcTipsDetails_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 22 — TipsController Coverage (`UtilitiesCS\HelperClasses\ToolTips\TipsController.cs`)

- [ ] [P22-T1] Add test to `UtilitiesCS.Test\HelperClasses\TipsController_Tests.cs` verifying that label setup reflects the details state after initialization
  - Acceptance: `[TestMethod]` exists, constructs a `TipsController` with a fake details object and calls the label setup path, and asserts the resulting label values match the details' expected content

- [ ] [P22-T2] Add test to `UtilitiesCS.Test\HelperClasses\TipsController_Tests.cs` verifying that toggle methods switch only the intended columns/sections
  - Acceptance: `[TestMethod]` exists, calls a toggle method and asserts only the targeted column/section changes state while others remain unchanged

- [ ] [P22-T3] Add test to `UtilitiesCS.Test\HelperClasses\TipsController_Tests.cs` verifying that repeated toggles are idempotent (calling toggle twice returns to the original state)
  - Acceptance: `[TestMethod]` exists, calls a toggle method twice in succession and asserts the relevant state is identical to its value before either call

- [ ] [P22-T4] Register `UtilitiesCS.Test\HelperClasses\TipsController_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\TipsController_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 23 — OlvExtension Coverage (`UtilitiesCS\HelperClasses\Windows Forms\OlvExtension.cs`)

- [ ] [P23-T1] Add test to `UtilitiesCS.Test\HelperClasses\OlvExtension_Tests.cs` verifying that `AutoScaleColumnsToContainer` expands columns proportionally to the container width
  - Acceptance: `[TestMethod]` exists, constructs an `ObjectListView` with known columns and a fixed container width, calls `AutoScaleColumnsToContainer`, and asserts each column's width is proportional to its share of the total width

- [ ] [P23-T2] Add test to `UtilitiesCS.Test\HelperClasses\OlvExtension_Tests.cs` verifying that calling `AutoScaleColumnsToContainer` with an empty column list is a no-op and does not throw
  - Acceptance: `[TestMethod]` exists, calls `AutoScaleColumnsToContainer` on an `ObjectListView` with no columns, and asserts no exception is thrown and the result is a no-op

- [ ] [P23-T3] Register `UtilitiesCS.Test\HelperClasses\OlvExtension_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\OlvExtension_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 24 — ConfigGroupBox Coverage (`UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigGroupBox.cs`)

- [ ] [P24-T1] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigGroupBox_Tests.cs` verifying that wrapper getter properties stay synchronized with child control values
  - Acceptance: `[TestMethod]` exists, sets child control values directly and reads back via the wrapper getter, and asserts the returned value equals the value set on the child control

- [ ] [P24-T2] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigGroupBox_Tests.cs` verifying that the active-disk selection property maps correctly to the expected disk index
  - Acceptance: `[TestMethod]` exists, sets the disk selection state on the control, and asserts the active-disk property returns the expected index/enum value

- [ ] [P24-T3] Register `UtilitiesCS.Test\ReusableTypeClasses\ConfigGroupBox_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="ReusableTypeClasses\ConfigGroupBox_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 25 — ConfigViewer Coverage (`UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigViewer.cs`)

- [ ] [P25-T1] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigViewer_Tests.cs` verifying that the save handler routes to the mocked controller's save method
  - Acceptance: `[TestMethod]` exists, binds a mocked `ConfigController` to the viewer, invokes the save handler, and asserts the controller's save method was called exactly once

- [ ] [P25-T2] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigViewer_Tests.cs` verifying that the cancel handler routes to the mocked controller's cancel method
  - Acceptance: `[TestMethod]` exists, binds a mocked `ConfigController` to the viewer, invokes the cancel handler, and asserts the controller's cancel method was called exactly once

- [ ] [P25-T3] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigViewer_Tests.cs` verifying that disk group activation toggles the correct controls
  - Acceptance: `[TestMethod]` exists, activates a specific disk group and asserts the corresponding group box controls enter the enabled/visible state while others remain unchanged

- [ ] [P25-T4] Register `UtilitiesCS.Test\ReusableTypeClasses\ConfigViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="ReusableTypeClasses\ConfigViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0


