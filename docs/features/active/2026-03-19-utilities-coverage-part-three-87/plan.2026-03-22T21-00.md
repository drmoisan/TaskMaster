# 2026-03-19-utilities-coverage-part-three - Plan

- **Issue:** #87
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-23
- **Status:** In Progress
- **Version:** 1.3

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

- [x] [P0-T1] Read all repo policy files in required order: `.github/copilot-instructions.md`, `general-code-change.instructions.md`, `general-unit-test.instructions.md`, `csharp-code-change.instructions.md`, `csharp-unit-test.instructions.md`
  - Acceptance: Evidence artifact at `evidence/baseline/phase0-instructions-read.md` contains `Timestamp:`, `Policy Order:`, and explicit list of all five files read

- [x] [P0-T2] Capture baseline build state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-build.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [x] [P0-T3] Capture baseline test results with coverage by running `vstest.console.exe` with `/EnableCodeCoverage` over all `*.Test.dll` assemblies
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-test-coverage.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` including total test count, pass count, and repo-wide UtilitiesCS line coverage percentage

- [x] [P0-T4] Record per-file baseline coverage for all UtilitiesCS production files below 80% line rate from the current `coverage/coverage.cobertura.xml`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-per-file-coverage.md` lists each file with its current line-rate percentage, categorized by difficulty (Easy/Medium/Hard/Skip)

- [x] [P0-T5] Reconcile every currently sub-80 non-skip UtilitiesCS file from `evidence/qa-gates/final-coverage-verification.md` against the remaining plan and `evidence/other/skip-candidates.md`
  - Acceptance: Evidence artifact at `evidence/baseline/remaining-sub80-reconciliation.md` contains one row for every file listed under "Non-Skip UtilitiesCS Files Below 80%" in `evidence/qa-gates/final-coverage-verification.md`, and each row maps the file to exactly one remaining task path: `Implementation Task` or `Phase 4 Skip Task`

- [x] [P0-T6] Verify the revised checklist state matches the reconciliation matrix before additional implementation resumes
  - Preconditions: P0-T5 complete
  - Acceptance: Every file mapped to `Implementation Task` in `evidence/baseline/remaining-sub80-reconciliation.md` references an unchecked P1/P2/P3 task ID, every file mapped to `Phase 4 Skip Task` references an unchecked P4 task ID, and no checked task still depends on a file that remains below 80% in `evidence/qa-gates/final-coverage-verification.md`

- [x] [P0-T7] Capture baseline formatter state by running `dotnet tool run csharpier .`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-csharpier.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` indicating whether files were reformatted

- [x] [P0-T8] Capture baseline analyzer-build state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-analyzer-build.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` listing whether analyzer diagnostics were emitted

- [x] [P0-T9] Capture baseline nullable/type-safety build state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-nullable-build.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` listing whether nullable or warning-as-error diagnostics were emitted

### Phase 1 — FolderNotFoundViewer Coverage (`UtilitiesCS\Dialogs\FolderNotFoundViewer.cs`)

- [x] [P1-T1] Add test to `UtilitiesCS.Test\Dialogs\FolderNotFoundViewer_Tests.cs` verifying that clicking the save-style action button sets `FolderAction` to the expected keep/save enum value
  - Acceptance: `[TestMethod]` exists in `FolderNotFoundViewer_Tests.cs`, creates a `FolderNotFoundViewer` instance on an STA thread, invokes the save button click handler, and asserts `FolderAction` equals the expected save enum value

- [x] [P1-T2] Add test to `UtilitiesCS.Test\Dialogs\FolderNotFoundViewer_Tests.cs` verifying that clicking the discard-style action button sets `FolderAction` to the expected discard/remove enum value
  - Acceptance: `[TestMethod]` exists, invokes the discard button click handler, and asserts `FolderAction` equals the expected discard enum value

- [x] [P1-T3] Add test to `UtilitiesCS.Test\Dialogs\FolderNotFoundViewer_Tests.cs` verifying that `FolderName` property returns the backing folder-name text correctly
  - Acceptance: `[TestMethod]` exists, assigns a known string to the backing field or constructor, and asserts `FolderName` returns that exact string

- [x] [P1-T4] Add test to `UtilitiesCS.Test\Dialogs\FolderNotFoundViewer_Tests.cs` verifying that the viewer calls `Hide` rather than `Dispose` when an action button is activated
  - Acceptance: `[TestMethod]` exists, invokes the action click handler, and asserts the viewer instance is not disposed after the call

- [x] [P1-T5] Register `UtilitiesCS.Test\Dialogs\FolderNotFoundViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Dialogs\FolderNotFoundViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 2 — InputBox Coverage (`UtilitiesCS\Dialogs\InputBox.cs`)

- [x] [P2-T1] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that the default response value populates the viewer's textbox state when the dialog is initialized
  - Acceptance: `[TestMethod]` exists in `InputBox_Test.cs`, creates an `InputBoxViewer` with a known default response string, and asserts the textbox text equals that default string

- [x] [P2-T2] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that accepting the dialog (OK path) returns the text entered in the textbox
  - Acceptance: `[TestMethod]` exists, sets the viewer textbox to a known string, triggers the OK path, and asserts the returned value equals the entered text

- [x] [P2-T3] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that cancelling the dialog returns `null`
  - Acceptance: `[TestMethod]` exists, triggers the cancel path on the viewer, and asserts the return value is `null`

### Phase 3 — InputBoxViewer Coverage (`UtilitiesCS\Dialogs\InputBoxViewer.cs`)

- [x] [P3-T1] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that `Ok_Click` copies the textbox text to the response field and closes the viewer
  - Acceptance: `[TestMethod]` exists, sets the textbox text on a direct `InputBoxViewer` instance, calls `Ok_Click`, and asserts the response field equals the textbox text and the viewer is no longer visible

- [x] [P3-T2] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that `Cancel_Click` clears the response field
  - Acceptance: `[TestMethod]` exists, calls `Cancel_Click` on a direct `InputBoxViewer` instance, and asserts the response field is `null` or empty

- [x] [P3-T3] Add test to `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that `DpiAware` property and `DpiCalled` static flag toggle their expected state
  - Acceptance: `[TestMethod]` resets `DpiCalled` to its default, sets `DpiAware`, and asserts `DpiCalled` reflects the expected toggled value; static state is reset in `TestCleanup`

### Phase 4 — MyBox Coverage (`UtilitiesCS\Dialogs\MyBox.cs`)

- [x] [P4-T1] Add test to `UtilitiesCS.Test\Dialogs\MyBox_Tests.cs` verifying that button conversion preserves dialog result ordering when standard buttons are mapped to custom equivalents
  - Acceptance: `[TestMethod]` exists in `MyBox_Tests.cs`, calls the button-conversion helper with a known set of standard buttons, and asserts the output sequence preserves expected `DialogResult` order

- [x] [P4-T2] Add test to `UtilitiesCS.Test\Dialogs\MyBox_Tests.cs` verifying that the button replacement helper swaps custom buttons into the viewer correctly
  - Acceptance: `[TestMethod]` exists, supplies a custom button list to the replacement helper, and asserts the viewer's button collection contains the custom buttons

- [x] [P4-T3] Add test to `UtilitiesCS.Test\Dialogs\MyBox_Tests.cs` verifying that `FunctionButtonGroup<T>` routing returns the mapped value for each button entry
  - Acceptance: `[TestMethod]` exists, creates a `FunctionButtonGroup<T>` binding with a known mapping, triggers the delegate, and asserts the returned value equals the expected mapped result

- [x] [P4-T4] Register `UtilitiesCS.Test\Dialogs\MyBox_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Dialogs\MyBox_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 5 — NotImplementedDialog Coverage (`UtilitiesCS\Dialogs\NotImplementedDialog.cs`)

- [x] [P5-T1] Add test to `UtilitiesCS.Test\Dialogs\NotImplementedDialog_Tests.cs` verifying that when `StopAtNotImplemented` is `true` the not-implemented trigger path throws the expected exception
  - Acceptance: `[TestMethod]` exists, sets `StopAtNotImplemented = true` via reflection or public API, invokes the trigger path, and asserts the expected exception type is thrown using FluentAssertions `.Should().Throw<>()`

- [x] [P5-T2] Add test to `UtilitiesCS.Test\Dialogs\NotImplementedDialog_Tests.cs` verifying that when `StopAtNotImplemented` is `false` the trigger path completes without throwing
  - Acceptance: `[TestMethod]` exists, sets `StopAtNotImplemented = false`, invokes the trigger path, and asserts no exception is thrown (method returns normally)

- [x] [P5-T3] Add `[TestCleanup]` method to `NotImplementedDialog_Tests.cs` that resets `StopAtNotImplemented` to its original value after each test to prevent static state pollution
  - Acceptance: `[TestInitialize]`-annotated method captures the original flag, `[TestCleanup]`-annotated method restores it, and both methods exist in the test class

- [x] [P5-T4] Register `UtilitiesCS.Test\Dialogs\NotImplementedDialog_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Dialogs\NotImplementedDialog_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 6 — SKIP_EVALUATION: ConfusionViewer (`UtilitiesCS\EmailIntelligence\Bayesian\Performance\ConfusionViewer.cs`)

- [x] [P6-T1] Record skip-evaluation decision for `ConfusionViewer.cs` in plan notes
  - Acceptance: This task is marked complete to document that `ConfusionViewer.cs` is a constructor-only WinForms designer shell with no meaningful non-designer logic; no test file will be created for this file

### Phase 7 — SKIP_EVALUATION: MetricChartViewer (`UtilitiesCS\EmailIntelligence\Bayesian\Performance\MetricChartViewer.cs`)

- [x] [P7-T1] Record skip-evaluation decision for `MetricChartViewer.cs` in plan notes
  - Acceptance: This task is marked complete to document that `MetricChartViewer.cs` is a constructor-only WinForms designer shell with no meaningful non-designer logic; no test file will be created for this file

### Phase 8 — AutoFile Coverage (`UtilitiesCS\EmailIntelligence\EmailParsingSorting\AutoFile.cs`)

- [x] [P8-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\AutoFile_Tests.cs` verifying that `AreConversationsGrouped` returns `true` when category and state inputs indicate grouped conversations
  - Acceptance: `[TestMethod]` exists, constructs synthetic category/state inputs using mocked Outlook objects, calls `AreConversationsGrouped`, and asserts the return value is `true`

- [x] [P8-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\AutoFile_Tests.cs` verifying that category-selection guard does not duplicate an already-selected category
  - Acceptance: `[TestMethod]` exists, builds a collection that already contains the target category, invokes category selection, and asserts the collection size is unchanged and the category appears exactly once

- [x] [P8-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\AutoFile_Tests.cs` verifying that `AutoFindPeople` selects the expected person candidate from a synthetic collection
  - Acceptance: `[TestMethod]` exists, passes a synthetic person collection with a single unambiguous match, calls `AutoFindPeople`, and asserts the returned candidate equals the expected value

- [x] [P8-T4] Register `UtilitiesCS.Test\EmailIntelligence\AutoFile_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\AutoFile_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 9 — SortEmail Coverage (`UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs`)

- [x] [P9-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs` verifying that `InitializeSortToExisting` throws `NotImplementedException`
  - Acceptance: `[TestMethod]` exists, invokes `InitializeSortToExisting`, and asserts a `NotImplementedException` is thrown using FluentAssertions `.Should().Throw<NotImplementedException>()`

- [x] [P9-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs` verifying that `ProcessMailItemAsync` short-circuits without proceeding to filing logic when the mail item input is null
  - Acceptance: `[TestMethod]` exists, passes `null` as the mail item, awaits `ProcessMailItemAsync`, and asserts no filing side-effects were triggered (mocked engine manager receives no file calls)

- [x] [P9-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs` verifying that both `SortAsync` overloads delegate to the same core processing path via the engine manager
  - Acceptance: `[TestMethod]` exists, invokes each overload with mocked engine manager, and asserts the expected core processing method was called exactly once per overload

- [x] [P9-T4] Register `UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\SortEmail_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 10 — FilterOlFoldersController Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersController.cs`)

- [x] [P10-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` verifying that `Save` forwards the save action to the backing model
  - Acceptance: `[TestMethod]` exists, calls `Save` on the controller with a Moq-mocked backing model, and asserts the model's save method was invoked exactly once

- [x] [P10-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` verifying that `Discard` forwards the discard action to the backing model
  - Acceptance: `[TestMethod]` exists, calls `Discard` on the controller with a Moq-mocked backing model, and asserts the model's discard method was invoked exactly once

- [x] [P10-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` verifying that a tree property change propagates to the viewer-facing state
  - Acceptance: `[TestMethod]` exists, triggers a property-changed event on the mocked tree, and asserts the controller's viewer-facing state reflects the updated value

- [x] [P10-T4] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` verifying that the check-state helpers round-trip the expected value
  - Acceptance: `[TestMethod]` exists, sets a check-state value via the setter, reads it back via the getter, and asserts the retrieved value equals the value originally set

- [x] [P10-T5] Register `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FilterOlFoldersController_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 11 — FilterOlFoldersViewer Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersViewer.cs`)

- [x] [P11-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs` verifying that `SetController` registers the expected delegates on a Moq-mocked controller
  - Acceptance: `[TestMethod]` exists, calls `SetController` with a mocked controller, and asserts the expected event/delegate registrations were performed on the mock

- [x] [P11-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs` verifying that `FormatFileSize` returns the expected string for a byte-range input (less than 1 KB)
  - Acceptance: `[TestMethod]` exists, calls `FormatFileSize` with a value less than 1,024, and asserts the return value matches the expected byte-formatted string

- [x] [P11-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs` verifying that `FormatFileSize` returns the expected string for a KB-or-larger input
  - Acceptance: `[TestMethod]` exists, calls `FormatFileSize` with a value of 1,024 or more, and asserts the return value matches the expected KB/MB-formatted string

- [x] [P11-T4] Add test to `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs` verifying that the viewer's save and discard buttons forward their events to the corresponding controller methods
  - Acceptance: `[TestMethod]` exists, triggers save and discard button clicks or event handlers, and asserts the mocked controller's `Save` and `Discard` methods were each invoked exactly once

- [x] [P11-T5] Register `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FilterOlFoldersViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 12 — FolderInfoViewer Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FolderInfoViewer.cs`)

- [x] [P12-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderInfoViewer_Tests.cs` verifying that `SetFolderTree` updates the `FolderTree` property to the assigned reference
  - Acceptance: `[TestMethod]` exists, calls `SetFolderTree` with a non-null argument, and asserts `FolderTree` returns the same reference that was assigned

- [x] [P12-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderInfoViewer_Tests.cs` verifying that assigning a new tree reference via `SetFolderTree` replaces the prior reference
  - Acceptance: `[TestMethod]` exists, assigns an initial tree reference, then assigns a second distinct reference, and asserts `FolderTree` returns the most recent assignment

- [x] [P12-T3] Register `UtilitiesCS.Test\EmailIntelligence\FolderInfoViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FolderInfoViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 13 — OSBrowser Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\OSBrowser.cs`)

- [x] [P13-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs` verifying that the column setup method initializes the expected number and names of columns
  - Acceptance: `[TestMethod]` exists, invokes the column-setup method, and asserts the column collection contains the expected count and identifiers

- [x] [P13-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs` verifying that the tree setup method configures the expected tree options
  - Acceptance: `[TestMethod]` exists, invokes the tree-setup method on a direct form instance, and asserts the expected tree option flags are set

- [x] [P13-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs` verifying that `FormatFileSize` returns the expected string for a bytes-range input (less than 1 KB)
  - Acceptance: `[TestMethod]` exists, calls `FormatFileSize` with a value below 1,024, and asserts the return value ends with the expected byte-unit label

- [x] [P13-T4] Add test to `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs` verifying that `FormatFileSize` returns the expected string for a KB-range input and for an MB-range input
  - Acceptance: `[TestMethod]` exists, calls `FormatFileSize` with a value of 1,024 and a value of 1,048,576, and asserts each return value ends with the correct unit label (KB or MB respectively)

- [x] [P13-T5] Register `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\OSBrowser_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 14 — FolderRemapController Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapController.cs`)

- [x] [P14-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` verifying that a simulated drag/drop operation updates the mapping entry in the mocked remap tree
  - Acceptance: `[TestMethod]` exists, triggers the drag/drop handler with synthetic folder-node arguments, and asserts the expected mapping change is applied to the mocked tree/model

- [x] [P14-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` verifying that `Save` forwards the save action to the backing model
  - Acceptance: `[TestMethod]` exists, calls `Save`, and asserts the mocked backing model's save method was invoked once

- [x] [P14-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` verifying that `Discard` forwards the discard action to the backing model
  - Acceptance: `[TestMethod]` exists, calls `Discard`, and asserts the mocked backing model's discard method was invoked once

- [x] [P14-T4] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` verifying that `ExpandTo` selects the correct folder node path in the mocked tree
  - Acceptance: `[TestMethod]` exists, calls `ExpandTo` with a synthetic node identifier, and asserts the mocked tree's selection matches the expected node path

- [x] [P14-T5] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` verifying that `SyncGlobalMap` propagates expected mapping changes to the global state
  - Acceptance: `[TestMethod]` exists, sets up a local mapping, calls `SyncGlobalMap`, and asserts the global mapping reflects the locally applied changes

- [x] [P14-T6] Register `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FolderRemapController_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 15 — FolderRemapViewer Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapViewer.cs`)

- [x] [P15-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapViewer_Tests.cs` verifying that the viewer forwards a drag/drop event to the mocked controller
  - Acceptance: `[TestMethod]` exists, triggers the drag/drop event on the viewer, and asserts the mocked controller's corresponding handler was invoked exactly once

- [x] [P15-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapViewer_Tests.cs` verifying that the viewer's setup methods establish the expected initial renderer and tree state
  - Acceptance: `[TestMethod]` exists, calls the setup method, and asserts the expected renderer type is applied and the tree's initial configuration matches the expected values

- [x] [P15-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapViewer_Tests.cs` verifying that the file-size formatting helper returns the expected string for a sample input
  - Acceptance: `[TestMethod]` exists, calls the file-size formatting helper with a known value, and asserts the return string matches the expected formatted representation

- [x] [P15-T4] Register `UtilitiesCS.Test\EmailIntelligence\FolderRemapViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FolderRemapViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 16 — FolderSelector Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderSelector.cs`)

- [x] [P16-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderSelector_Tests.cs` verifying that initialization sets the expected selection source reference
  - Acceptance: `[TestMethod]` exists, instantiates `FolderSelector` with a fake folder-tree source, and asserts the stored source reference equals the provided input

- [x] [P16-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderSelector_Tests.cs` verifying that confirming a selection sets `Selection` to the chosen folder node
  - Acceptance: `[TestMethod]` exists, simulates a completed selection by setting the expected node state, and asserts the `Selection` property returns the expected node/folder reference

- [x] [P16-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderSelector_Tests.cs` verifying that passing a null/empty input leaves `Selection` as null
  - Acceptance: `[TestMethod]` exists, calls the relevant path with null or empty source, and asserts `Selection` is null after the call

- [x] [P16-T4] Register `UtilitiesCS.Test\EmailIntelligence\FolderSelector_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FolderSelector_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 17 — SubjectMapEncoder Coverage (`UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`)

- [x] [P17-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapEncoder_Tests.cs` verifying that `RebuildEncoding` builds symmetric encode/decode maps
  - Acceptance: `[TestMethod]` exists, calls `RebuildEncoding` with a known token list, and asserts each token maps forward and backward correctly (encode[token] → id, decode[id] → token)

- [x] [P17-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapEncoder_Tests.cs` verifying that `AugmentTokenDict` appends only unseen tokens
  - Acceptance: `[TestMethod]` exists, calls `AugmentTokenDict` with a mix of existing and new tokens, and asserts only the new tokens are added while existing entries are unchanged

- [x] [P17-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapEncoder_Tests.cs` verifying that `Encode` followed by `Decode` round-trips the original terms
  - Acceptance: `[TestMethod]` exists, encodes a known term sequence and then decodes the result, and asserts the decoded output matches the original input

- [x] [P17-T4] Register `UtilitiesCS.Test\EmailIntelligence\SubjectMapEncoder_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\SubjectMapEncoder_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 18 — SubjectMapMetrics Coverage (`UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapMetrics.cs`)

- [x] [P18-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapMetrics_Tests.cs` verifying that the primary constructor copies expected counts and rates into `DlvMetrics`
  - Acceptance: `[TestMethod]` exists, constructs `SubjectMapMetrics` with known numeric inputs, and asserts the corresponding `DlvMetrics` properties hold the expected values

- [x] [P18-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapMetrics_Tests.cs` verifying that alternate constructor overloads produce equivalent state to the primary constructor
  - Acceptance: `[TestMethod]` exists, constructs instances via two different overloads with equivalent inputs, and asserts the resulting `DlvMetrics` properties are equal across both instances

- [x] [P18-T3] Register `UtilitiesCS.Test\EmailIntelligence\SubjectMapMetrics_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\SubjectMapMetrics_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 19 — DfDeedle Coverage (`UtilitiesCS\Extensions\DfDeedle.cs`)

- [x] [P19-T1] Add test to `UtilitiesCS.Test\Extensions\DfDeedle_Tests.cs` verifying that a 2D email array converts to a DataFrame with the expected row count and column layout
  - Acceptance: `[TestMethod]` exists, passes a small in-memory 2D array to the conversion method, and asserts the returned frame has the expected number of rows and correctly named columns

- [x] [P19-T2] Add test to `UtilitiesCS.Test\Extensions\DfDeedle_Tests.cs` verifying that invalid triage values are filtered out from the DataFrame
  - Acceptance: `[TestMethod]` exists, constructs a frame containing invalid triage entries, calls the filter method, and asserts the result excludes rows with invalid triage values

- [x] [P19-T3] Add test to `UtilitiesCS.Test\Extensions\DfDeedle_Tests.cs` verifying that date extraction handles null and invalid date slots without throwing
  - Acceptance: `[TestMethod]` exists, calls the date extraction path with null and unparseable date values, and asserts the method returns null/default rather than throwing

- [x] [P19-T4] Register `UtilitiesCS.Test\Extensions\DfDeedle_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Extensions\DfDeedle_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 20 — DvgForm Coverage (`UtilitiesCS\HelperClasses\DvgForm.cs`)

- [x] [P20-T1] Add test to `UtilitiesCS.Test\HelperClasses\DvgForm_Tests.cs` verifying that triggering resize-end invokes expected layout behavior without throwing
  - Acceptance: `[TestMethod]` exists, instantiates `DvgForm` and triggers the resize-end event path, and asserts no exception is thrown and the expected layout side effect occurs

- [x] [P20-T2] Register `UtilitiesCS.Test\HelperClasses\DvgForm_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\DvgForm_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 21 — QfcTipsDetails Coverage (`UtilitiesCS\HelperClasses\ToolTips\QfcTipsDetails.cs`)

- [x] [P21-T1] Add test to `UtilitiesCS.Test\HelperClasses\QfcTipsDetails_Tests.cs` verifying that parent-type resolution returns the expected enum/type value
  - Acceptance: `[TestMethod]` exists, invokes the parent-type resolution path with a known parent stub, and asserts the returned type/enum value matches the expected case

- [x] [P21-T2] Add test to `UtilitiesCS.Test\HelperClasses\QfcTipsDetails_Tests.cs` verifying that `InitializeAsync` populates expected labels and toggle state
  - Acceptance: `[TestMethod]` exists, calls the initialization path on a direct instance, and asserts the detail labels and toggle properties hold the expected post-initialization values

- [x] [P21-T3] Add test to `UtilitiesCS.Test\HelperClasses\QfcTipsDetails_Tests.cs` verifying that visibility toggle methods update internal state consistently
  - Acceptance: `[TestMethod]` exists, calls a visibility toggle method and asserts the relevant internal state property reflects the toggled value; calling the same toggle again restores the previous state

- [x] [P21-T4] Register `UtilitiesCS.Test\HelperClasses\QfcTipsDetails_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\QfcTipsDetails_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 22 — TipsController Coverage (`UtilitiesCS\HelperClasses\ToolTips\TipsController.cs`)

- [x] [P22-T1] Add test to `UtilitiesCS.Test\HelperClasses\TipsController_Tests.cs` verifying that label setup reflects the details state after initialization
  - Acceptance: `[TestMethod]` exists, constructs a `TipsController` with a fake details object and calls the label setup path, and asserts the resulting label values match the details' expected content

- [x] [P22-T2] Add test to `UtilitiesCS.Test\HelperClasses\TipsController_Tests.cs` verifying that toggle methods switch only the intended columns/sections
  - Acceptance: `[TestMethod]` exists, calls a toggle method and asserts only the targeted column/section changes state while others remain unchanged

- [x] [P22-T3] Add test to `UtilitiesCS.Test\HelperClasses\TipsController_Tests.cs` verifying that repeated toggles are idempotent (calling toggle twice returns to the original state)
  - Acceptance: `[TestMethod]` exists, calls a toggle method twice in succession and asserts the relevant state is identical to its value before either call

- [x] [P22-T4] Register `UtilitiesCS.Test\HelperClasses\TipsController_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\TipsController_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 23 — OlvExtension Coverage (`UtilitiesCS\HelperClasses\Windows Forms\OlvExtension.cs`)

- [x] [P23-T1] Add test to `UtilitiesCS.Test\HelperClasses\OlvExtension_Tests.cs` verifying that `AutoScaleColumnsToContainer` expands columns proportionally to the container width
  - Acceptance: `[TestMethod]` exists, constructs an `ObjectListView` with known columns and a fixed container width, calls `AutoScaleColumnsToContainer`, and asserts each column's width is proportional to its share of the total width

- [x] [P23-T2] Add test to `UtilitiesCS.Test\HelperClasses\OlvExtension_Tests.cs` verifying that calling `AutoScaleColumnsToContainer` with an empty column list is a no-op and does not throw
  - Acceptance: `[TestMethod]` exists, calls `AutoScaleColumnsToContainer` on an `ObjectListView` with no columns, and asserts no exception is thrown and the result is a no-op

- [x] [P23-T3] Register `UtilitiesCS.Test\HelperClasses\OlvExtension_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\OlvExtension_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 24 — ConfigGroupBox Coverage (`UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigGroupBox.cs`)

- [x] [P24-T1] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigGroupBox_Tests.cs` verifying that wrapper getter properties stay synchronized with child control values
  - Acceptance: `[TestMethod]` exists, sets child control values directly and reads back via the wrapper getter, and asserts the returned value equals the value set on the child control

- [x] [P24-T2] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigGroupBox_Tests.cs` verifying that the active-disk selection property maps correctly to the expected disk index
  - Acceptance: `[TestMethod]` exists, sets the disk selection state on the control, and asserts the active-disk property returns the expected index/enum value

- [x] [P24-T3] Register `UtilitiesCS.Test\ReusableTypeClasses\ConfigGroupBox_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="ReusableTypeClasses\ConfigGroupBox_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 25 — ConfigViewer Coverage (`UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigViewer.cs`)

- [x] [P25-T1] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigViewer_Tests.cs` verifying that the save handler routes to the mocked controller's save method
  - Acceptance: `[TestMethod]` exists, binds a mocked `ConfigController` to the viewer, invokes the save handler, and asserts the controller's save method was called exactly once

- [x] [P25-T2] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigViewer_Tests.cs` verifying that the cancel handler routes to the mocked controller's cancel method
  - Acceptance: `[TestMethod]` exists, binds a mocked `ConfigController` to the viewer, invokes the cancel handler, and asserts the controller's cancel method was called exactly once

- [x] [P25-T3] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigViewer_Tests.cs` verifying that disk group activation toggles the correct controls
  - Acceptance: `[TestMethod]` exists, activates a specific disk group and asserts the corresponding group box controls enter the enabled/visible state while others remain unchanged

- [x] [P25-T4] Register `UtilitiesCS.Test\ReusableTypeClasses\ConfigViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="ReusableTypeClasses\ConfigViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 26 — IdleActionQueue Coverage (`UtilitiesCS\Threading\IdleActionQueue.cs`)

- [ ] [P26-T1] Add test to `UtilitiesCS.Test\Threading\IdleActionQueue_Tests.cs` verifying that the first `AddEntry` call initializes the internal queue
  - Acceptance: `[TestMethod]` exists, resets static state via reflection, calls `AddEntry` once, and asserts the queue's internal entry count is 1

- [ ] [P26-T2] Add test to `UtilitiesCS.Test\Threading\IdleActionQueue_Tests.cs` verifying that the idle callback drains queued entries in enqueue order
  - Acceptance: `[TestMethod]` exists, enqueues multiple actions via `AddEntry`, fires the idle callback via reflection, and asserts the actions were invoked in the order they were added

- [ ] [P26-T3] Add test to `UtilitiesCS.Test\Threading\IdleActionQueue_Tests.cs` verifying that the unsubscribe path clears the idle callback after inactivity
  - Acceptance: `[TestMethod]` exists, enqueues work, drains the queue, triggers the unsubscribe timer, and asserts the idle handler is no longer registered

- [ ] [P26-T4] Register `UtilitiesCS.Test\Threading\IdleActionQueue_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Threading\IdleActionQueue_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 27 — IdleAsyncQueue Coverage (`UtilitiesCS\Threading\IdleAsyncQueue.cs`)

- [ ] [P27-T1] Add test to `UtilitiesCS.Test\Threading\IdleAsyncQueue_Tests.cs` verifying that a queued async task runs exactly once when the idle callback fires
  - Acceptance: `[TestMethod]` exists, enqueues a fake async task, fires the idle callback via reflection, and asserts the task was invoked exactly once

- [ ] [P27-T2] Add test to `UtilitiesCS.Test\Threading\IdleAsyncQueue_Tests.cs` verifying that the UI-thread flag routes work through the expected scheduling path
  - Acceptance: `[TestMethod]` exists, enqueues work with the UI-thread flag set, fires the callback, and asserts the scheduling path taken matches the expected dispatcher/sync-context route

- [ ] [P27-T3] Add test to `UtilitiesCS.Test\Threading\IdleAsyncQueue_Tests.cs` verifying that an exception thrown by one queued item does not prevent subsequent items from executing
  - Acceptance: `[TestMethod]` exists, enqueues a faulting task followed by a normal task, fires the idle callback, and asserts the normal task still executes and no unhandled exception escapes the queue

- [ ] [P27-T4] Register `UtilitiesCS.Test\Threading\IdleAsyncQueue_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Threading\IdleAsyncQueue_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 28 — ProgressMultiStepViewer Coverage (`UtilitiesCS\Threading\ProgressMultiStepViewer.cs`) — SKIP_EVALUATION

- [ ] [P28-T1] Record skip-evaluation decision for `ProgressMultiStepViewer.cs`: constructor-only designer shell with no meaningful non-designer logic; no unit tests added
  - Acceptance: This task is checked off and the decision is noted inline; no test file is created for this file

### Phase 29 — ProgressPane Coverage (`UtilitiesCS\Threading\ProgressPane.cs`)

- [ ] [P29-T1] Add test to `UtilitiesCS.Test\Threading\ProgressPane_Tests.cs` verifying that initialization captures the UI synchronization context/scheduler
  - Acceptance: `[TestMethod]` exists, constructs `ProgressPane` under a controlled `SynchronizationContext`, and asserts the exposed scheduler/context property holds the expected value

- [ ] [P29-T2] Add test to `UtilitiesCS.Test\Threading\ProgressPane_Tests.cs` verifying that the cancellation token source is honored when cancellation is requested
  - Acceptance: `[TestMethod]` exists, calls the cancellation path on the pane, and asserts the exposed `CancellationToken` enters the cancelled state

- [ ] [P29-T3] Add test to `UtilitiesCS.Test\Threading\ProgressPane_Tests.cs` verifying that visibility and progress-report state change as expected when updated
  - Acceptance: `[TestMethod]` exists, sets the visible/report state and asserts the corresponding properties reflect the new values

- [ ] [P29-T4] Register `UtilitiesCS.Test\Threading\ProgressPane_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Threading\ProgressPane_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 30 — ProgressViewer Coverage (`UtilitiesCS\Threading\ProgressViewer.cs`)

- [ ] [P30-T1] Add test to `UtilitiesCS.Test\Threading\ProgressViewer_Tests.cs` verifying that activating the cancel path sets the cancellation token source to cancelled
  - Acceptance: `[TestMethod]` exists, constructs `ProgressViewer`, programmatically invokes the cancel path, and asserts the exposed `CancellationToken` is in the cancelled state

- [ ] [P30-T2] Add test to `UtilitiesCS.Test\Threading\ProgressViewer_Tests.cs` verifying that the exposed sync context and dispatcher properties are populated after initialization
  - Acceptance: `[TestMethod]` exists, initializes `ProgressViewer` under a controlled context, and asserts the sync-context and dispatcher properties are non-null

- [ ] [P30-T3] Register `UtilitiesCS.Test\Threading\ProgressViewer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Threading\ProgressViewer_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 31 — ThreadMonitor Coverage (`UtilitiesCS\Threading\ThreadMonitor.cs`) — SKIP_EVALUATION

- [ ] [P31-T1] Record skip-evaluation decision for `ThreadMonitor.cs`: relies on obsolete `Thread.Suspend`/`Thread.Resume` APIs and timing-sensitive diagnostics; deterministic unit tests are not feasible
  - Acceptance: This task is checked off and the decision is noted inline; no test file is created for this file

### Phase 32 — CSVDictUtilities Coverage (`UtilitiesCS\To Depricate\CSVDictUtilities.cs`) — SKIP_EVALUATION

- [ ] [P32-T1] Record skip-evaluation decision for `CSVDictUtilities.cs`: deprecated utility with no injection seam and direct file-system dependence; tests would require real disk I/O
  - Acceptance: This task is checked off and the decision is noted inline; no test file is created for this file

### Phase 33 — FileIO2 Coverage (`UtilitiesCS\To Depricate\FileIO2.cs`) — SKIP_EVALUATION

- [ ] [P33-T1] Record skip-evaluation decision for `FileIO2.cs`: deprecated file helper with no seam; main public paths are direct static file I/O making deterministic unit tests low-value without prior abstraction work
  - Acceptance: This task is checked off and the decision is noted inline; no test file is created for this file

### Phase 34 — EmailDataMiner Coverage (`UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailDataMiner.cs`)

- [ ] [P34-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_Tests.cs` verifying that an empty source returns no mined rows
  - Acceptance: `[TestMethod]` exists, passes an empty/null folder tree to the mining orchestration path, and asserts the returned result set is empty

- [ ] [P34-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_Tests.cs` verifying that the chunking path groups inputs into the expected count/size
  - Acceptance: `[TestMethod]` exists, passes a known-size input to the chunking method with a fixed chunk size, and asserts the number of chunks and item counts per chunk are correct

- [ ] [P34-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_Tests.cs` verifying that the staging-delete routine short-circuits when the target path is missing
  - Acceptance: `[TestMethod]` exists, calls the staging-delete path with a non-existent path, and asserts the method returns early without error rather than proceeding

- [ ] [P34-T4] Register `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\EmailDataMiner_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 35 — ScreenHelper Coverage (`UtilitiesCS\HelperClasses\Windows Forms\ScreenHelper.cs`) — SKIP_EVALUATION

- [ ] [P35-T1] Record skip-evaluation decision for `ScreenHelper.cs`: behavior depends on actual machine monitor topology and active forms; static `Screen.AllScreens` has no injection seam
  - Acceptance: This task is checked off and the decision is noted inline; no test file is created for this file

### Phase 36 — SubjectMapSco Coverage (`UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.cs`)

- [ ] [P36-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Tests.cs` verifying that adding a token updates the lookup counts
  - Acceptance: `[TestMethod]` exists, adds a known token to the subject map, and asserts the lookup count for that token increments as expected

- [ ] [P36-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Tests.cs` verifying that `TryRepair` fixes a recoverable missing encoding
  - Acceptance: `[TestMethod]` exists, introduces a missing-encoding condition via a fake map state, calls `TryRepair`, and asserts the encoding is restored to the expected value

- [ ] [P36-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Tests.cs` verifying that query helpers return deterministic matches for known inputs
  - Acceptance: `[TestMethod]` exists, sets up a known in-memory subject map, calls the query helper with a fixed input, and asserts the returned matches equal the expected set

- [ ] [P36-T4] Register `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\SubjectMapSco_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 37 — Theme Coverage (`UtilitiesCS\HelperClasses\ThemeHelpers\Theme.cs`) — SKIP_EVALUATION

- [ ] [P37-T1] Record skip-evaluation decision for `Theme.cs`: broad UI/control graph and large mutable surface make meaningful unit coverage low-value; narrower `ThemeControlGroup` behavior is the preferred coverage target
  - Acceptance: This task is checked off and the decision is noted inline; no test file is created for this file

### Phase 38 — IntelligenceConfig Coverage (`UtilitiesCS\EmailIntelligence\IntelligenceConfig.cs`)

- [ ] [P38-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\IntelligenceConfig_Tests.cs` verifying that derived-type detection matches expected classifier types
  - Acceptance: `[TestMethod]` exists, constructs a config with a known type discriminator value, and asserts the type-detection path returns the expected classifier enum/type

- [ ] [P38-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\IntelligenceConfig_Tests.cs` verifying that property changes trigger the write path via the mocked loader
  - Acceptance: `[TestMethod]` exists, sets a config property and asserts the mocked loader's write/save method was invoked

- [ ] [P38-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\IntelligenceConfig_Tests.cs` verifying that missing config data initializes defaults
  - Acceptance: `[TestMethod]` exists, loads a config with a synthetic empty/null payload, and asserts the resulting config properties equal expected default values

- [ ] [P38-T4] Register `UtilitiesCS.Test\EmailIntelligence\IntelligenceConfig_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\IntelligenceConfig_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 39 — EmailFiler Coverage (`UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFiler.cs`)

- [ ] [P39-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\EmailFiler_Tests.cs` verifying that the open-folder helper short-circuits on an invalid path
  - Acceptance: `[TestMethod]` exists, calls the open-folder path with a null or empty path, and asserts the method returns early without calling the folder-open side effect

- [ ] [P39-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\EmailFiler_Tests.cs` verifying that tab/CRLF stripping produces deterministic clean output
  - Acceptance: `[TestMethod]` exists, passes a string with embedded tabs and CRLF sequences to the stripping helper, and asserts the result equals the expected clean string

- [ ] [P39-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\EmailFiler_Tests.cs` verifying that the undo-stack capture records move details correctly
  - Acceptance: `[TestMethod]` exists, invokes the undo-capture path with synthetic source and destination values, and asserts the captured undo entry contains the expected source and destination

- [ ] [P39-T4] Register `UtilitiesCS.Test\EmailIntelligence\EmailFiler_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\EmailFiler_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 40 — ConfigController Coverage (`UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigController.cs`)

- [ ] [P40-T1] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigController_Tests.cs` verifying that activating the local disk group toggles the correct target group
  - Acceptance: `[TestMethod]` exists, calls `ActivateDiskGroup` for the local disk option, and asserts the expected group properties enter the active/enabled state

- [ ] [P40-T2] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigController_Tests.cs` verifying that `Cancel` restores the prior config state
  - Acceptance: `[TestMethod]` exists, modifies config state, calls `Cancel`, and asserts the original state is restored

- [ ] [P40-T3] Add test to `UtilitiesCS.Test\ReusableTypeClasses\ConfigController_Tests.cs` verifying that the unimplemented file-chooser path does not throw or performs a no-op as coded
  - Acceptance: `[TestMethod]` exists, invokes the not-implemented file-chooser handler, and asserts no exception is thrown

- [ ] [P40-T4] Register `UtilitiesCS.Test\ReusableTypeClasses\ConfigController_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="ReusableTypeClasses\ConfigController_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 41 — AsyncMultiTasker Coverage (`UtilitiesCS\Threading\AsyncMultiTasker.cs`)

- [ ] [P41-T1] Add test to `UtilitiesCS.Test\Threading\AsyncMultiTasker_Tests.cs` verifying that the chunk-size helper partitions inputs into batches of the expected size
  - Acceptance: `[TestMethod]` exists, passes a known-length input list with a fixed chunk size, and asserts the number of batches and per-batch counts are correct

- [ ] [P41-T2] Add test to `UtilitiesCS.Test\Threading\AsyncMultiTasker_Tests.cs` verifying that an async overload preserves result order and count
  - Acceptance: `[TestMethod]` exists, passes ordered inputs to the async overload and awaits completion, and asserts the returned result sequence matches the expected order and length

- [ ] [P41-T3] Add test to `UtilitiesCS.Test\Threading\AsyncMultiTasker_Tests.cs` verifying that the progress callback receives a terminal completion notification
  - Acceptance: `[TestMethod]` exists, supplies a progress callback, runs the async task to completion, and asserts the callback was invoked with a completion/100% signal

- [ ] [P41-T4] Register `UtilitiesCS.Test\Threading\AsyncMultiTasker_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Threading\AsyncMultiTasker_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 42 — FolderRemapTree Coverage (`UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapTree.cs`)

- [ ] [P42-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapTree_Tests.cs` verifying that building a tree from a mapping source yields the expected nodes
  - Acceptance: `[TestMethod]` exists, passes a synthetic folder mapping to the build method, and asserts the resulting tree contains the expected node paths/labels

- [ ] [P42-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapTree_Tests.cs` verifying that the filter path removes excluded nodes
  - Acceptance: `[TestMethod]` exists, applies a filter to the built tree and asserts excluded node paths are absent from the filtered result

- [ ] [P42-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\FolderRemapTree_Tests.cs` verifying that notification fires on a map update
  - Acceptance: `[TestMethod]` exists, subscribes to the map-update notification, modifies the map, and asserts the notification was raised exactly once

- [ ] [P42-T4] Register `UtilitiesCS.Test\EmailIntelligence\FolderRemapTree_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\FolderRemapTree_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 43 — ClassifierGroupUtilities Coverage (`UtilitiesCS\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities.cs`)

- [ ] [P43-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities_Tests.cs` verifying that an existing loader path resolves to the expected classifier group
  - Acceptance: `[TestMethod]` exists, provides a mocked loader returning a known config, and asserts the resolved group identity matches the expected key

- [ ] [P43-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities_Tests.cs` verifying that a missing config returns a fallback or new classifier
  - Acceptance: `[TestMethod]` exists, provides a mocked loader returning null/missing config, and asserts the returned classifier is a valid fallback or newly initialized instance

- [ ] [P43-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities_Tests.cs` verifying that serialize/deserialize preserves expected config fields
  - Acceptance: `[TestMethod]` exists, serializes a known config object and deserializes back, and asserts the round-tripped fields equal the originals

- [ ] [P43-T4] Register `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 44 — PeopleScoDictionaryNew Coverage (`UtilitiesCS\EmailIntelligence\People\PeopleScoDictionaryNew.cs`)

- [ ] [P44-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\PeopleScoDictionaryNew_Tests.cs` verifying that matching prefers exact names/categories over partial matches
  - Acceptance: `[TestMethod]` exists, sets up a dictionary with both exact and partial matches, calls the matching method, and asserts the exact-match result is returned

- [ ] [P44-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\PeopleScoDictionaryNew_Tests.cs` verifying that the add flow applies the expected category prefix rules
  - Acceptance: `[TestMethod]` exists, adds an entry with a known prefix rule active, and asserts the stored entry's category bears the expected prefix

- [ ] [P44-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\PeopleScoDictionaryNew_Tests.cs` verifying that duplicate additions are ignored or merged as coded
  - Acceptance: `[TestMethod]` exists, adds the same entry twice, and asserts the dictionary count reflects the expected duplicate-handling behavior

- [ ] [P44-T4] Register `UtilitiesCS.Test\EmailIntelligence\PeopleScoDictionaryNew_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\PeopleScoDictionaryNew_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 45 — SCODictionary Coverage (`UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`)

- [ ] [P45-T1] Add test to `UtilitiesCS.Test\ReusableTypeClasses\SCODictionary_Tests.cs` verifying that deserializing a missing path returns an empty or new object
  - Acceptance: `[TestMethod]` exists, calls the deserialize path with a synthetic null/missing-path config, and asserts the resulting dictionary is empty rather than throwing

- [ ] [P45-T2] Add test to `UtilitiesCS.Test\ReusableTypeClasses\SCODictionary_Tests.cs` verifying that the backup loader selection prefers the expected source
  - Acceptance: `[TestMethod]` exists, sets up two candidate sources and calls the backup-select path, and asserts the returned source matches the expected priority order

- [ ] [P45-T3] Register `UtilitiesCS.Test\ReusableTypeClasses\SCODictionary_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="ReusableTypeClasses\SCODictionary_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 46 — FileInfoWrapper Coverage (`UtilitiesCS\HelperClasses\FileSystem\FileInfoWrapper.cs`)

- [ ] [P46-T1] Add test to `UtilitiesCS.Test\HelperClasses\FileInfoWrapper_Tests.cs` verifying that the wrapper forwards `Exists`, name, and path properties from the inner `FileInfo`
  - Acceptance: `[TestMethod]` exists, constructs a `FileInfoWrapper` with a known backing path, and asserts the `Exists`, `Name`, and `FullName` properties equal the underlying `FileInfo` values

- [ ] [P46-T2] Add test to `UtilitiesCS.Test\HelperClasses\FileInfoWrapper_Tests.cs` verifying that a null inner `FileInfo` is handled gracefully as coded
  - Acceptance: `[TestMethod]` exists, constructs a `FileInfoWrapper` with a null inner value and calls the relevant property or method, and asserts no unhandled exception is thrown and the result matches the expected null/default behavior

- [ ] [P46-T3] Register `UtilitiesCS.Test\HelperClasses\FileInfoWrapper_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\FileInfoWrapper_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 47 — DirectoryInfoWrapper Coverage (`UtilitiesCS\HelperClasses\FileSystem\DirectoryInfoWrapper.cs`)

- [ ] [P47-T1] Add test to `UtilitiesCS.Test\HelperClasses\DirectoryInfoWrapper_Tests.cs` verifying that the wrapper forwards directory `Name`, `FullName`, and `Exists` from the inner `DirectoryInfo`
  - Acceptance: `[TestMethod]` exists, constructs a `DirectoryInfoWrapper` with a known path, and asserts the wrapper properties equal the underlying `DirectoryInfo` values

- [ ] [P47-T2] Register `UtilitiesCS.Test\HelperClasses\DirectoryInfoWrapper_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\DirectoryInfoWrapper_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 48 — DfMLNet Coverage (`UtilitiesCS\Extensions\DfMLNet.cs`)

- [ ] [P48-T1] Add test to `UtilitiesCS.Test\Extensions\DfMLNet_Tests.cs` verifying that `ToDataFrame` converts an object sequence to a DataFrame with the expected columns and types
  - Acceptance: `[TestMethod]` exists, passes a small known-type list to `ToDataFrame`, and asserts the resulting DataFrame has the correct column names and types

- [ ] [P48-T2] Add test to `UtilitiesCS.Test\Extensions\DfMLNet_Tests.cs` verifying that the first-non-null column selector returns the correct column from mixed-null inputs
  - Acceptance: `[TestMethod]` exists, provides columns where some rows are null, calls the first-non-null selector, and asserts the selected column is the expected one

- [ ] [P48-T3] Add test to `UtilitiesCS.Test\Extensions\DfMLNet_Tests.cs` verifying that `ToDataTable` conversion preserves the row count
  - Acceptance: `[TestMethod]` exists, converts a known DataFrame to a `DataTable`, and asserts the returned table row count equals the source frame row count

- [ ] [P48-T4] Register `UtilitiesCS.Test\Extensions\DfMLNet_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Extensions\DfMLNet_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 49 — TableLayoutHelper Coverage (`UtilitiesCS\HelperClasses\Windows Forms\TableLayoutHelper.cs`)

- [ ] [P49-T1] Add test to `UtilitiesCS.Test\HelperClasses\WindowsForms\ScreenAndTableLayoutTests.cs` verifying that adding a row to a `TableLayoutPanel` increments the row count and repositions existing controls
  - Acceptance: `[TestMethod]` exists, calls the add-row helper on a direct `TableLayoutPanel` instance with known row count, and asserts the row count is incremented by one

- [ ] [P49-T2] Add test to `UtilitiesCS.Test\HelperClasses\WindowsForms\ScreenAndTableLayoutTests.cs` verifying that the invoke branch executes without error when called from the owning thread
  - Acceptance: `[TestMethod]` exists, calls the helper while on the control's thread of origin, and asserts no exception is thrown and the expected mutation applied

- [ ] [P49-T3] Register `UtilitiesCS.Test\HelperClasses\WindowsForms\ScreenAndTableLayoutTests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 50 — SpamBayes Coverage (`UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.cs`)

- [ ] [P50-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that the create-new path returns a configured classifier group
  - Acceptance: `[TestMethod]` exists, calls the create path with mocked globals and manager, and asserts the returned group is non-null and has the expected configuration

- [ ] [P50-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that a missing configuration invokes the fallback handling path
  - Acceptance: `[TestMethod]` exists, supplies a mocked loader returning null config, invokes the create path, and asserts the fallback handling branch executes without exception

- [ ] [P50-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that validation rejects an incomplete setup
  - Acceptance: `[TestMethod]` exists, provides an incomplete/invalid config, calls the validation method, and asserts the validation returns false or throws the expected exception

- [ ] [P50-T4] Register `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 51 — ScBag Coverage (`UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\ScBag.cs`)

- [ ] [P51-T1] Add test to nearest `UtilitiesCS.Test\ReusableTypeClasses` test file verifying that deserializing a missing/null path creates an empty bag
  - Acceptance: `[TestMethod]` exists, calls the deserialize path with a synthetic null/missing path config, and asserts the result is an empty bag rather than throwing

- [ ] [P51-T2] Add test to nearest `UtilitiesCS.Test\ReusableTypeClasses` test file verifying that request-serialization routes only when the config directs it
  - Acceptance: `[TestMethod]` exists, sets the serialization config flag to disabled, calls request-serialize, and asserts the underlying writer was not invoked

- [ ] [P51-T3] Add test to nearest `UtilitiesCS.Test\ReusableTypeClasses` test file verifying that the ask-user branch handles a cancellation response gracefully
  - Acceptance: `[TestMethod]` exists, supplies a mock responder returning Cancel, invokes the ask-user path, and asserts the bag retains its prior state without error

- [ ] [P51-T4] Register the chosen `UtilitiesCS.Test\ReusableTypeClasses` test file in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 52 — CorpusInherit Coverage (`UtilitiesCS\EmailIntelligence\Bayesian\CorpusInherit.cs`)

- [ ] [P52-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\Bayesian\CorpusInherit_Tests.cs` verifying that increment and decrement adjust token counts correctly
  - Acceptance: `[TestMethod]` exists, increments a known token twice and decrements once, and asserts the stored count equals 1

- [ ] [P52-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\Bayesian\CorpusInherit_Tests.cs` verifying that deserializing an empty payload returns an initialized (non-null, empty) corpus
  - Acceptance: `[TestMethod]` exists, calls deserialize with an empty or minimal payload, and asserts the result is a valid empty corpus instance

- [ ] [P52-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\Bayesian\CorpusInherit_Tests.cs` verifying that serialization preserves the token frequency map round-trip
  - Acceptance: `[TestMethod]` exists, populates a corpus with known token frequencies, serializes and deserializes it, and asserts the retrieved frequency map matches the original

- [ ] [P52-T4] Register `UtilitiesCS.Test\EmailIntelligence\Bayesian\CorpusInherit_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\Bayesian\CorpusInherit_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 53 — FunctionButton Coverage (`UtilitiesCS\Dialogs\FunctionButton.cs`)

- [ ] [P53-T1] Add test to `UtilitiesCS.Test\Dialogs\FunctionButton_Tests.cs` verifying that each constructor overload preserves the supplied metadata and delegate
  - Acceptance: `[TestMethod]` exists, constructs a `FunctionButton` via a specific overload, and asserts the resulting `Text`/metadata and delegate reference match the supplied values

- [ ] [P53-T2] Add test to `UtilitiesCS.Test\Dialogs\FunctionButton_Tests.cs` verifying that reassigning the underlying `Button` unwires the old click handler
  - Acceptance: `[TestMethod]` exists, wires a click handler to the original button, reassigns to a new `Button`, clicks the old button, and asserts the delegate was not invoked

- [ ] [P53-T3] Add test to `UtilitiesCS.Test\Dialogs\FunctionButton_Tests.cs` verifying that an async callback executes exactly once when the button is clicked
  - Acceptance: `[TestMethod]` exists, uses a `TaskCompletionSource`-based async delegate, simulates a click, awaits the task, and asserts the delegate was invoked exactly one time

- [ ] [P53-T4] Register `UtilitiesCS.Test\Dialogs\FunctionButton_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Dialogs\FunctionButton_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 54 — MyBoxViewer Coverage (`UtilitiesCS\Dialogs\MyBoxViewer.cs`)

- [ ] [P54-T1] Add test to nearest `UtilitiesCS.Test\Dialogs` test file verifying that custom buttons invoke their mapped delegate when clicked
  - Acceptance: `[TestMethod]` exists, adds a custom button with a known action, simulates a click, and asserts the action was invoked

- [ ] [P54-T2] Add test to nearest `UtilitiesCS.Test\Dialogs` test file verifying that removing standard buttons leaves only the custom controls
  - Acceptance: `[TestMethod]` exists, removals standard buttons via the viewer API, and asserts the button panel no longer contains standard button controls

- [ ] [P54-T3] Add test to nearest `UtilitiesCS.Test\Dialogs` test file verifying that text changes trigger a growth/min-size recalculation
  - Acceptance: `[TestMethod]` exists, sets text that should require growth, and asserts the viewer's minimum size or height reflects the recalculated value

- [ ] [P54-T4] Register the chosen `UtilitiesCS.Test\Dialogs` test file in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 55 — YesNoToAll Coverage (`UtilitiesCS\Dialogs\YesNoToAll.cs`)

- [ ] [P55-T1] Add test to `UtilitiesCS.Test\Dialogs\YesNoToAll_Tests.cs` verifying that each response setter stores the expected enum value
  - Acceptance: `[TestMethod]` exists, calls each response setter in turn, and asserts the `Response` property equals the corresponding expected enum member

- [ ] [P55-T2] Add test to `UtilitiesCS.Test\Dialogs\YesNoToAll_Tests.cs` verifying that the dialog result property reflects the current state after a setter is called
  - Acceptance: `[TestMethod]` exists, sets a response, and asserts the associated `DialogResult` or display state matches the expected value

- [ ] [P55-T3] Register `UtilitiesCS.Test\Dialogs\YesNoToAll_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Dialogs\YesNoToAll_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 56 — CategoryClassifierGroup Coverage (`UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs`)

- [ ] [P56-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that category expansion creates the expected classifier keys
  - Acceptance: `[TestMethod]` exists, provides synthetic categories, calls expand/build, and asserts the resulting classifier keys match each category name

- [ ] [P56-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that the build path skips empty categories
  - Acceptance: `[TestMethod]` exists, includes an empty category in the input, calls build, and asserts no classifier key was created for that category

- [ ] [P56-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that the load path reuses existing manager entries rather than creating duplicates
  - Acceptance: `[TestMethod]` exists, pre-populates the mocked manager with a known entry, calls load, and asserts the pre-existing entry is returned without creating a new one

- [ ] [P56-T4] Register `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 57 — MouseDownFilter Coverage (`UtilitiesCS\HelperClasses\Windows Forms\MouseDownFilter.cs`)

- [ ] [P57-T1] Add test to nearest `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs` verifying that a left-button WM_LBUTTONDOWN message triggers the `FormClicked` event
  - Acceptance: `[TestMethod]` exists, subscribes to `FormClicked`, constructs a WM_LBUTTONDOWN `Message`, calls `PreFilterMessage`, and asserts the event was raised

- [ ] [P57-T2] Add test to nearest `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs` verifying that an unrelated message returns false without raising the event
  - Acceptance: `[TestMethod]` exists, constructs a non-mouse `Message`, calls `PreFilterMessage`, and asserts the return value is false and no event was raised

- [ ] [P57-T3] Add test to nearest `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs` verifying that calling `PreFilterMessage` with no subscribers does not throw
  - Acceptance: `[TestMethod]` exists, constructs a `MouseDownFilter` with no event subscribers, calls `PreFilterMessage`, and asserts no exception is thrown

- [ ] [P57-T4] Register `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 58 — ShellUtilities SKIP (`UtilitiesCS\HelperClasses\FileSystem\ShellUtilities.cs`)

- [ ] [P58-T1] Record skip-evaluation decision for `ShellUtilities.cs`: static Win32 shell interop and PInvoke icon extraction have no DI seam and are environment-dependent; unit-test ROI is negligible without OS/shell coupling
  - Acceptance: This task is checked off and the decision is noted inline; no test file is created for this file

### Phase 59 — ShellUtilitiesStatic SKIP (`UtilitiesCS\HelperClasses\FileSystem\ShellUtilitiesStatic.cs`)

- [ ] [P59-T1] Record skip-evaluation decision for `ShellUtilitiesStatic.cs`: same static Win32 shell dependence as `ShellUtilities.cs`; no viable seam for meaningful deterministic unit tests
  - Acceptance: This task is checked off and the decision is noted inline; no test file is created for this file

### Phase 60 — ThemeControlGroup Coverage (`UtilitiesCS\HelperClasses\ThemeHelpers\ThemeControlGroup.cs`)

- [ ] [P60-T1] Add test to nearest `UtilitiesCS.Test\HelperClasses` test file verifying that `ApplyTheme` updates supported control properties to theme values
  - Acceptance: `[TestMethod]` exists, creates a `ThemeControlGroup` with simple WinForms controls and a known theme, calls `ApplyTheme`, and asserts the backed controls have the expected foreground/background values

- [ ] [P60-T2] Add test to nearest `UtilitiesCS.Test\HelperClasses` test file verifying that the alternate/hover setters target the intended control subset
  - Acceptance: `[TestMethod]` exists, calls the alternate setter with a subset of controls, and asserts only the targeted controls received the alternate styling values

- [ ] [P60-T3] Add test to nearest `UtilitiesCS.Test\HelperClasses` test file verifying that unsupported control types are ignored safely without throwing
  - Acceptance: `[TestMethod]` exists, includes an unsupported control type in the group, calls `ApplyTheme`, and asserts no exception is thrown

- [ ] [P60-T4] Register the chosen `UtilitiesCS.Test\HelperClasses` test file in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 61 — OlTableExtensions Coverage (`UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs`)

- [ ] [P61-T1] Add test to `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs` verifying that add-column and remove-column call the expected Outlook table members in order
  - Acceptance: `[TestMethod]` exists, mocks the COM table/columns interface, calls the helper, and asserts the expected COM Add/Remove calls were made in the expected order via Moq verification

- [ ] [P61-T2] Add test to `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs` verifying that the retry wrapper retries the specified number of times on transient failure
  - Acceptance: `[TestMethod]` exists, supplies a mock action that throws for the first N-1 calls and succeeds on the Nth, calls the retry wrapper, and asserts the action was invoked exactly N times

- [ ] [P61-T3] Add test to `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs` verifying that the extract helper maps mocked rows to the expected strongly-typed records
  - Acceptance: `[TestMethod]` exists, supplies mock rows with known column values, calls the extract helper, and asserts the resulting records have the expected field values

- [ ] [P61-T4] Register `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Table\OlTableExtensions_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 62 — ProgressTrackerAsync Coverage (`UtilitiesCS\Threading\ProgressTrackerAsync.cs`)

- [ ] [P62-T1] Add test to `UtilitiesCS.Test\Threading\ProgressTrackerAsync_Tests.cs` verifying that `Initialize` populates root-tracker state
  - Acceptance: `[TestMethod]` exists, calls `Initialize`, and asserts the root tracker's percent and message properties are in their initialized (non-null/zero) state

- [ ] [P62-T2] Add test to `UtilitiesCS.Test\Threading\ProgressTrackerAsync_Tests.cs` verifying that `Report` updates the percentage and message fields
  - Acceptance: `[TestMethod]` exists, calls `Report` with a known percent and message, and asserts the tracker properties reflect those values

- [ ] [P62-T3] Add test to `UtilitiesCS.Test\Threading\ProgressTrackerAsync_Tests.cs` verifying that child allocation inherits the expected scheduler and token state
  - Acceptance: `[TestMethod]` exists, allocates a child tracker, and asserts the child references the parent scheduler/cancellation token source

- [ ] [P62-T4] Register `UtilitiesCS.Test\Threading\ProgressTrackerAsync_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Threading\ProgressTrackerAsync_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 63 — WinFormsExtensions Coverage (`UtilitiesCS\Extensions\WinFormsExtensions.cs`)

- [ ] [P63-T1] Add test to `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs` verifying that control-descendant traversal returns all nested descendants in expected order
  - Acceptance: `[TestMethod]` exists, builds a two-level control tree, calls the traversal helper, and asserts the result collection contains all expected control references in the expected sequence

- [ ] [P63-T2] Add test to `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs` verifying that ancestor lookup handles a control with no parent without throwing
  - Acceptance: `[TestMethod]` exists, calls the ancestor-lookup helper on a control with no parent, and asserts the result is null/empty and no exception is thrown

- [ ] [P63-T3] Add test to `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs` verifying that `RemoveEventHandlers` prevents subsequent invocation of removed handlers
  - Acceptance: `[TestMethod]` exists, wires a delegate to an event, calls `RemoveEventHandlers`, fires the event, and asserts the delegate was not invoked

- [ ] [P63-T4] Register `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 64 — MulticlassEngine Coverage (`UtilitiesCS\EmailIntelligence\ClassifierGroups\MulticlassEngine.cs`)

- [ ] [P64-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\MulticlassEngine_Tests.cs` verifying that `Init` wires the manager and globals correctly
  - Acceptance: `[TestMethod]` exists, calls `Init` with mocked globals and manager, and asserts the engine properties reference the provided instances

- [ ] [P64-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\MulticlassEngine_Tests.cs` verifying that the build path creates the expected number of classifiers
  - Acceptance: `[TestMethod]` exists, provides synthetic classifier input data of known cardinality, calls build, and asserts the classifier count in the engine equals the expected value

- [ ] [P64-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\MulticlassEngine_Tests.cs` verifying that the load path short-circuits when a manager entry is missing
  - Acceptance: `[TestMethod]` exists, supplies a mocked manager returning null for the requested entry, calls load, and asserts the method returns early without creating a classifier

- [ ] [P64-T4] Register `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\MulticlassEngine_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\ClassifierGroups\MulticlassEngine_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 65 — Triage Coverage (`UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage.cs`)

- [ ] [P65-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs` verifying that the create-new path sets the expected config file name
  - Acceptance: `[TestMethod]` exists, calls the create path with mocked globals, and asserts the resulting classifier group config file name equals the expected value

- [ ] [P65-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs` verifying that validation rejects a config with a missing classifier group
  - Acceptance: `[TestMethod]` exists, supplies an incomplete config with no classifier group, calls validation, and asserts the validation returns false or raises the expected error

- [ ] [P65-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs` verifying that the training path routes through the mocked manager as expected
  - Acceptance: `[TestMethod]` exists, supplies a mock manager and training input, calls the training method, and asserts the mock manager's training method was invoked with the expected arguments

- [ ] [P65-T4] Register `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 66 — ProgressTrackerPane Coverage (`UtilitiesCS\Threading\ProgressTrackerPane.cs`)

- [ ] [P66-T1] Add test to nearest `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs` verifying that the root tracker reports progress to the pane
  - Acceptance: `[TestMethod]` exists, creates a `ProgressTrackerPane` with a stub pane, calls `Report`, and asserts the stub pane's update method was invoked with the expected percent/message

- [ ] [P66-T2] Add test to nearest `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs` verifying that a spawned child tracker inherits a scaled range from the parent
  - Acceptance: `[TestMethod]` exists, spawns a child from an initialized parent tracker, reports 50% on the child, and asserts the parent's reported progress is in the expected mapped range

- [ ] [P66-T3] Add test to nearest `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs` verifying that completing the child at 100% properly closes or finalizes the pane state
  - Acceptance: `[TestMethod]` exists, reports 100% on the tracker, and asserts the pane's close or finalize method was called

- [ ] [P66-T4] Register `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 67 — OlFolderClassifierGroup Coverage (`UtilitiesCS\EmailIntelligence\ClassifierGroups\OlFolder\OlFolderClassifierGroup.cs`)

- [ ] [P67-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that the build path creates one classifier per eligible folder in the staging source
  - Acceptance: `[TestMethod]` exists, provides N synthetic folder metadata entries, calls build, and asserts the classifier collection count equals N

- [ ] [P67-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that an empty staging source yields no classifiers
  - Acceptance: `[TestMethod]` exists, provides an empty staging source, calls build, and asserts the classifier collection is empty

- [ ] [P67-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that the load path rehydrates an existing group from the manager
  - Acceptance: `[TestMethod]` exists, pre-populates the mocked manager with a known group, calls load, and asserts the returned group matches the pre-populated entry

- [ ] [P67-T4] Register `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 68 — ApplicationIdleTimer Coverage (`UtilitiesCS\Threading\ApplicationIdleTimer.cs`)

- [ ] [P68-T1] Add test to `UtilitiesCS.Test\Threading\ApplicationIdleTimer_Tests.cs` verifying that subscribing and then unsubscribing reduces the listener count correctly
  - Acceptance: `[TestMethod]` exists, subscribes two listeners and unsubscribes one, and asserts the listener count equals 1

- [ ] [P68-T2] Add test to `UtilitiesCS.Test\Threading\ApplicationIdleTimer_Tests.cs` verifying that the heartbeat raises event args matching the expected elapsed/state fields
  - Acceptance: `[TestMethod]` exists, triggers a heartbeat, captures the event args, and asserts the elapsed time and/or activity state fields equal the expected values

- [ ] [P68-T3] Add test to `UtilitiesCS.Test\Threading\ApplicationIdleTimer_Tests.cs` verifying that the singleton instance property returns the same reference on repeated access
  - Acceptance: `[TestMethod]` exists, reads the singleton property twice and asserts both references are equal

- [ ] [P68-T4] Register `UtilitiesCS.Test\Threading\ApplicationIdleTimer_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 69 — RecentsList Coverage (`UtilitiesCS\EmailIntelligence\Recents\RecentsList.cs`)

- [ ] [P69-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\RecentsList_Tests.cs` verifying that adding a duplicate item moves the existing entry to the front of the list
  - Acceptance: `[TestMethod]` exists, adds item A and item B, re-adds item A, and asserts A is now the first element

- [ ] [P69-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\RecentsList_Tests.cs` verifying that exceeding the max count trims the oldest entry
  - Acceptance: `[TestMethod]` exists, fills the list to max capacity, adds one more item, and asserts the count remains at max and the oldest item is absent

- [ ] [P69-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\RecentsList_Tests.cs` verifying that serialization and deserialization preserve insertion order
  - Acceptance: `[TestMethod]` exists, populates a known-order list, serializes and deserializes it, and asserts the resulting list order matches the original

- [ ] [P69-T4] Register `UtilitiesCS.Test\EmailIntelligence\RecentsList_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\RecentsList_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 70 — OneDriveDownloader Coverage (`UtilitiesCS\OneDriveHelpers\OneDriveDownloader.cs`)

- [ ] [P70-T1] Add test to `UtilitiesCS.Test\OneDriveHelpers\OneDriveDownloader_Tests.cs` verifying that a successful download writes the stream contents via the injected writer
  - Acceptance: `[TestMethod]` exists, supplies an in-memory HTTP response stream and a mock writer delegate, calls `DownloadFileAsync`, and asserts the writer received the expected bytes

- [ ] [P70-T2] Add test to `UtilitiesCS.Test\OneDriveHelpers\OneDriveDownloader_Tests.cs` verifying that a missing writer returns false without producing file output
  - Acceptance: `[TestMethod]` exists, supplies a null/missing writer factory, calls the download method, and asserts the return value is false and no file data was written

- [ ] [P70-T3] Add test to `UtilitiesCS.Test\OneDriveHelpers\OneDriveDownloader_Tests.cs` verifying that a failed HTTP client call returns false without invoking the writer
  - Acceptance: `[TestMethod]` exists, supplies a mock client delegate that throws or returns an error, calls the download method, and asserts the return is false and the writer was not invoked

- [ ] [P70-T4] Register `UtilitiesCS.Test\OneDriveHelpers\OneDriveDownloader_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="OneDriveHelpers\OneDriveDownloader_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 71 — ManagerAsyncLazy Coverage (`UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs`)

- [ ] [P71-T1] Add test to nearest `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs` verifying that `ResetConfigAsyncLazy` replaces the prior configuration task with a new one
  - Acceptance: `[TestMethod]` exists, captures the initial lazy task reference, calls `ResetConfigAsyncLazy`, and asserts the new task reference is different from the original

- [ ] [P71-T2] Add test to nearest `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs` verifying that removing an inactive loader drops the corresponding engine entry
  - Acceptance: `[TestMethod]` exists, adds a loader entry, marks it inactive, calls the removal/cleanup path, and asserts the engine dictionary no longer contains the entry

- [ ] [P71-T3] Add test to nearest `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs` verifying that `GetAsyncLazyClassifierLoader` attaches a config-change handler and uses the alternate loader when available
  - Acceptance: `[TestMethod]` exists, supplies an alternate loader mock, calls `GetAsyncLazyClassifierLoader`, and asserts the returned loader invokes the alternate mock rather than the default path

- [ ] [P71-T4] Register `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 72 — FileSystemInfoWrapper Coverage (`UtilitiesCS\HelperClasses\FileSystem\FileSystemInfoWrapper.cs`)

- [ ] [P72-T1] Add test to nearest `UtilitiesCS.Test\HelperClasses\FileInfoWrapper_Tests.cs` verifying that the wrapper forwards common `FileSystemInfo` properties such as `Name`, `FullName`, and `Exists`
  - Acceptance: `[TestMethod]` exists, constructs a `FileSystemInfoWrapper` with a known path, and asserts the wrapper's property values equal the underlying `FileSystemInfo` values

- [ ] [P72-T2] Add test to nearest `UtilitiesCS.Test\HelperClasses\FileInfoWrapper_Tests.cs` verifying that null or invalid state is handled consistently with the rest of the wrapper family
  - Acceptance: `[TestMethod]` exists, constructs the wrapper with a null/invalid inner value and accesses key properties, and asserts the behavior matches the expected null/default pattern without throwing

- [ ] [P72-T3] Register `UtilitiesCS.Test\HelperClasses\FileInfoWrapper_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 73 — DispatchUtility Coverage (`UtilitiesCS\HelperClasses\CloningFunctions\DispatchUtility.cs`)

- [ ] [P73-T1] Add test to `UtilitiesCS.Test\HelperClasses\DispatchUtility_Tests.cs` verifying that `ImplementsIDispatch` returns false for non-dispatch objects
  - Acceptance: `[TestMethod]` exists, passes a plain managed object (not COM-visible) to the helper, and asserts the return value is false

- [ ] [P73-T2] Add test to `UtilitiesCS.Test\HelperClasses\DispatchUtility_Tests.cs` verifying that a dispatch-id lookup failure returns false without throwing
  - Acceptance: `[TestMethod]` exists, passes a member name that does not exist on the dispatch target, calls `TryGetDispId`, and asserts the return is false and no exception is thrown

- [ ] [P73-T3] Add test to `UtilitiesCS.Test\HelperClasses\DispatchUtility_Tests.cs` verifying that invalid invoke arguments surface the expected exception
  - Acceptance: `[TestMethod]` exists, calls `Invoke` with an invalid argument combination, and asserts the expected exception type is thrown

- [ ] [P73-T4] Register `UtilitiesCS.Test\HelperClasses\DispatchUtility_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\DispatchUtility_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 74 — ProgressTracker Coverage (`UtilitiesCS\Threading\ProgressTracker.cs`)

- [ ] [P74-T1] Add test to `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs` verifying that `Report` updates the percent and message properties on the tracker
  - Acceptance: `[TestMethod]` exists, calls `Report` with a known percent and message string, and asserts the tracker's `Percent` and `Message` properties equal those values

- [ ] [P74-T2] Add test to `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs` verifying that a child tracker maps its completion percentage into the parent's allocated range
  - Acceptance: `[TestMethod]` exists, allocates a child for a known parent sub-range, reports 100% on the child, and asserts the parent's percent shifted by the expected range amount

- [ ] [P74-T3] Add test to `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs` verifying that reaching 100% on the tracker closes or finalizes the viewer state
  - Acceptance: `[TestMethod]` exists, supplies a mock viewer, reports 100% on the tracker, and asserts the mock viewer's close/finalize method was invoked

- [ ] [P74-T4] Register `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 75 — ComStreamWrapper Coverage (`UtilitiesCS\HelperClasses\WipUnfinished\ComStreamWrapper.cs`)

- [ ] [P75-T1] Add test to `UtilitiesCS.Test\HelperClasses\ComStreamWrapper_Tests.cs` verifying that a read with zero offset forwards the call correctly to the mocked `IStream`
  - Acceptance: `[TestMethod]` exists, supplies a mocked `IStream`, calls `Read` with offset 0, and asserts the mock's `Read` equivalent received the expected buffer and count

- [ ] [P75-T2] Add test to `UtilitiesCS.Test\HelperClasses\ComStreamWrapper_Tests.cs` verifying that a read or write with a nonzero offset throws the expected exception
  - Acceptance: `[TestMethod]` exists, calls `Read` or `Write` with a nonzero offset, and asserts the expected exception type is thrown

- [ ] [P75-T3] Add test to `UtilitiesCS.Test\HelperClasses\ComStreamWrapper_Tests.cs` verifying that `Seek`, `Length`, and `Position` round-trip correctly through the COM stream
  - Acceptance: `[TestMethod]` exists, sets a known seek position or length via the mock, reads it back through the wrapper, and asserts the returned value matches

- [ ] [P75-T4] Register `UtilitiesCS.Test\HelperClasses\ComStreamWrapper_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\ComStreamWrapper_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 76 — ActionableClassifierGroup Coverage (`UtilitiesCS\EmailIntelligence\ClassifierGroups\Actionable\ActionableClassifierGroup.cs`)

- [ ] [P76-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that the actionable category filter returns the expected subset of categories
  - Acceptance: `[TestMethod]` exists, provides a mix of actionable and non-actionable categories via the mocked globals, calls the filter method, and asserts only actionable categories are returned

- [ ] [P76-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that the build path creates the engine when all prerequisites are met
  - Acceptance: `[TestMethod]` exists, provides a fully configured mocked globals and manager, calls `CreateEngineAsync`, and asserts the resulting engine is non-null

- [ ] [P76-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` verifying that the test path short-circuits on empty data without throwing
  - Acceptance: `[TestMethod]` exists, supplies an empty input to `TestAsync`, and asserts the method returns or completes without throwing an exception

- [ ] [P76-T4] Register `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 77 — StoreWrapperController Coverage (`UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`)

- [ ] [P77-T1] Add test to `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperController_Tests.cs` verifying that `PopulateWithCurrent` mirrors the backing store wrapper's field values
  - Acceptance: `[TestMethod]` exists, supplies a mocked store wrapper with known field values, calls `PopulateWithCurrent`, and asserts the controller properties match the mock values

- [ ] [P77-T2] Add test to `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperController_Tests.cs` verifying that `AnyChanges` returns true when a field differs from the backing wrapper
  - Acceptance: `[TestMethod]` exists, populates the controller, modifies one field, calls `AnyChanges`, and asserts the return is true

- [ ] [P77-T3] Add test to `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperController_Tests.cs` verifying that selecting a folder updates the target folder properties on the controller
  - Acceptance: `[TestMethod]` exists, calls the select-folder callback with a synthetic folder object, and asserts the controller's target folder properties have been updated

- [ ] [P77-T4] Register `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperController_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Store\StoreWrapperController_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 78 — Triage_OlLogic Coverage (`UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs`)

- [ ] [P78-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs` verifying that the filter builder strips unsupported filter clauses
  - Acceptance: `[TestMethod]` exists, provides a filter string with known unsupported clauses, calls the stripping helper, and asserts the result contains only the supported clauses

- [ ] [P78-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs` verifying that `TrainSelectionAsync` skips an empty selection without throwing
  - Acceptance: `[TestMethod]` exists, supplies an empty selection mock, calls `TrainSelectionAsync`, and asserts the method completes without error and the triage classifier's train method was not invoked

- [ ] [P78-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs` verifying that selected rows are mapped to training examples with the expected label and content
  - Acceptance: `[TestMethod]` exists, provides a mocked selection with known row values, calls `TrainSelectionAsync`, and asserts the mocked triage classifier received training examples matching the expected label/content

- [ ] [P78-T4] Register `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 79 — SystemThemeDetector SKIP (`UtilitiesCS\HelperClasses\ThemeHelpers\SystemThemeDetector.cs`)

- [ ] [P79-T1] Record skip-evaluation decision for `SystemThemeDetector.cs`: static registry reads have no DI seam; positive-path tests would couple to machine/user theme settings and are environment-dependent
  - Acceptance: This task is checked off and the decision is noted inline; no test file is created for this file

### Phase 80 — BayesianPerformanceMeasurement Coverage (`UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs`)

- [ ] [P80-T1] Add test to `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianPerformanceMeasurement_Tests.cs` verifying that the split helper partitions a dataset into the expected train/test proportions
  - Acceptance: `[TestMethod]` exists, passes a known-size corpus to the split helper with a specified ratio, and asserts the train and test partition sizes equal the expected values

- [ ] [P80-T2] Add test to `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianPerformanceMeasurement_Tests.cs` verifying that confusion-driver extraction returns the expected row count and label fields
  - Acceptance: `[TestMethod]` exists, provides synthetic classification output, calls the confusion extraction helper, and asserts the resulting confusion rows are correct in count and label content

- [ ] [P80-T3] Add test to `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianPerformanceMeasurement_Tests.cs` verifying that an empty or invalid corpus short-circuits without writing output
  - Acceptance: `[TestMethod]` exists, passes an empty corpus to the performance measurement path, and asserts the method returns early and the mocked writer was not invoked

- [ ] [P80-T4] Register `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianPerformanceMeasurement_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\Bayesian\BayesianPerformanceMeasurement_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 81 — LockingObservableLinkedListNode Coverage (`UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedListNode.cs`)

- [ ] [P81-T1] Add test to `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedListNode_Tests.cs` verifying that `Next` and `Previous` return the expected adjacent nodes from the inner linked node
  - Acceptance: `[TestMethod]` exists, constructs a node in a list with a known next/previous node, and asserts the wrapper's `Next` and `Previous` properties reference the expected nodes

- [ ] [P81-T2] Add test to `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedListNode_Tests.cs` verifying that movement helpers invoke the expected callback on the owning list
  - Acceptance: `[TestMethod]` exists, attaches a fake owning list, calls a movement helper on the node, and asserts the list's expected move/update method was invoked

- [ ] [P81-T3] Add test to `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedListNode_Tests.cs` verifying that `Invalidate` clears the node's references
  - Acceptance: `[TestMethod]` exists, calls `Invalidate` on a populated node, and asserts the node's `Value`, `Next`, and `Previous` properties are null or cleared as expected

- [ ] [P81-T4] Register `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedListNode_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="ReusableTypeClasses\LockingObservableLinkedListNode_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 82 — AsyncSerialization Coverage (`UtilitiesCS\Extensions\AsyncSerialization.cs`)

- [ ] [P82-T1] Add test to `UtilitiesCS.Test\Extensions\AsyncSerialization_Tests.cs` verifying that `ToMbString` formats a known byte count to the expected megabyte string
  - Acceptance: `[TestMethod]` exists, calls `ToMbString` with a known byte count and asserts the result equals the expected formatted string (e.g., `"1.00 MB"`)

- [ ] [P82-T2] Add test to `UtilitiesCS.Test\Extensions\AsyncSerialization_Tests.cs` verifying that the async copy helper reports monotonically increasing progress
  - Acceptance: `[TestMethod]` exists, copies a known in-memory stream with a progress callback, captures all reported percent values, and asserts each value is greater than or equal to the prior

- [ ] [P82-T3] Add test to `UtilitiesCS.Test\Extensions\AsyncSerialization_Tests.cs` verifying that the progress message formatting handles the zero-complete case without division errors
  - Acceptance: `[TestMethod]` exists, calls the progress-formatting helper with zero bytes complete and a known total, and asserts the result is a valid string without exception

- [ ] [P82-T4] Register `UtilitiesCS.Test\Extensions\AsyncSerialization_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Extensions\AsyncSerialization_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 83 — DelegateButton Coverage (`UtilitiesCS\Dialogs\DelegateButton.cs`)

- [ ] [P83-T1] Add test to `UtilitiesCS.Test\Dialogs\DelegateButton_Tests.cs` verifying that each constructor overload preserves the template metadata and dialog result
  - Acceptance: `[TestMethod]` exists, constructs a `DelegateButton` via a specific overload with known parameters, and asserts `Text`, `DialogResult`, and any delegate reference are preserved

- [ ] [P83-T2] Add test to `UtilitiesCS.Test\Dialogs\DelegateButton_Tests.cs` verifying that replacing the `Button` reference unwires the old click handler
  - Acceptance: `[TestMethod]` exists, wires a click handler to the original `Button`, reassigns the `Button` property, clicks the old button, and asserts the original delegate was not invoked

- [ ] [P83-T3] Add test to `UtilitiesCS.Test\Dialogs\DelegateButton_Tests.cs` verifying that the image helper sets the correct image-relation and replaces any prior image
  - Acceptance: `[TestMethod]` exists, sets an initial image and calls the image helper with a new image and relation, and asserts the `Button.Image` and `TextImageRelation` equals the values provided

- [ ] [P83-T4] Register `UtilitiesCS.Test\Dialogs\DelegateButton_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Dialogs\DelegateButton_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 84 — TimedDiskWriter Coverage (`UtilitiesCS\ReusableTypeClasses\TimedActions\TimedDiskWriter.cs`)

- [ ] [P84-T1] Add test to `UtilitiesCS.Test\HelperClasses\TimedDiskWriterTests.cs` verifying that enqueuing an item starts the timer when the timer is currently inactive
  - Acceptance: `[TestMethod]` exists, calls `Enqueue` on an idle `TimedDiskWriter`, and asserts the mock timer's start method was invoked once

- [ ] [P84-T2] Add test to `UtilitiesCS.Test\HelperClasses\TimedDiskWriterTests.cs` verifying that the timed event drains the queue and invokes the writer with all batched items
  - Acceptance: `[TestMethod]` exists, enqueues N items, triggers the timed event, and asserts the mock writer was called with all N items in the batch

- [ ] [P84-T3] Add test to `UtilitiesCS.Test\HelperClasses\TimedDiskWriterTests.cs` verifying that repeated empty-queue checks stop the timer
  - Acceptance: `[TestMethod]` exists, drains the queue and triggers the timed event with an empty queue, and asserts the mock timer's stop method was invoked

- [ ] [P84-T4] Register `UtilitiesCS.Test\HelperClasses\TimedDiskWriterTests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 85 — UiThread Coverage (`UtilitiesCS\Threading\UiThread.cs`)

- [ ] [P85-T1] Add test to `UtilitiesCS.Test\Threading\UiThread_Tests.cs` verifying that the awaiter rejects a null synchronization context with an expected exception
  - Acceptance: `[TestMethod]` exists, constructs the awaiter with a null context, and asserts the expected exception type is thrown

- [ ] [P85-T2] Add test to `UtilitiesCS.Test\Threading\UiThread_Tests.cs` verifying that `IsCompleted` returns the expected value based on whether the current context matches the captured UI context
  - Acceptance: `[TestMethod]` exists, provides a mocked `SynchronizationContext`, reads `IsCompleted` from a matching and a non-matching context, and asserts true and false respectively

- [ ] [P85-T3] Add test to `UtilitiesCS.Test\Threading\UiThread_Tests.cs` verifying that `OnCompleted` posts the supplied continuation to the target synchronization context
  - Acceptance: `[TestMethod]` exists, supplies a mock `SynchronizationContext`, calls `OnCompleted` with a known action, and asserts the mock context's `Post` method received that action

- [ ] [P85-T4] Register `UtilitiesCS.Test\Threading\UiThread_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="Threading\UiThread_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 86 — ClassifierGroup (Obsolete) Coverage (`UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\ClassifierGroup.cs`)

- [ ] [P86-T1] Add test to nearest `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianClassifierGroup_Tests.cs` verifying that `Add`/`Update` creates or appends to the correct classifier based on the source key
  - Acceptance: `[TestMethod]` exists, calls `Add` or `Update` with a known source key and token sequence, and asserts the classifier for that key was created or appended

- [ ] [P86-T2] Add test to nearest `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianClassifierGroup_Tests.cs` verifying that `Classify` returns ordered predictions for a known token input
  - Acceptance: `[TestMethod]` exists, trains classifiers with distinct token sets, calls `Classify` with a known input, and asserts the returned predictions are sorted by score descending

- [ ] [P86-T3] Add test to nearest `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianClassifierGroup_Tests.cs` verifying that dedicated and shared token counts contribute to the metrics state
  - Acceptance: `[TestMethod]` exists, adds tokens to both dedicated and shared classifiers, and asserts the resulting metrics state reflects counts from both paths

- [ ] [P86-T4] Register `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianClassifierGroup_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if not already present
  - Acceptance: `UtilitiesCS.Test.csproj` contains the relevant `<Compile Include="..." />` entry for this test file and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 87 — LockingObservableLinkedList Coverage (`UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedList.cs`)

- [ ] [P87-T1] Add test to `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedList_Tests.cs` verifying that `Add` and `Remove` raise the expected `CollectionChanged` action with the correct node reference
  - Acceptance: `[TestMethod]` exists, subscribes to `CollectionChanged`, adds and removes a node, and asserts the event args action type and node reference match expected values for each operation

- [ ] [P87-T2] Add test to `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedList_Tests.cs` verifying that `AddOrMoveFirst` moves an existing node to first position rather than duplicating it
  - Acceptance: `[TestMethod]` exists, adds a node, calls `AddOrMoveFirst` for the same value, and asserts the list contains the node exactly once and it is at the first position

- [ ] [P87-T3] Add test to `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedList_Tests.cs` verifying that partial observers receive only changes for their registered nodes
  - Acceptance: `[TestMethod]` exists, registers a partial observer for one node, modifies a different node, and asserts the partial observer was not notified

- [ ] [P87-T4] Register `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedList_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="ReusableTypeClasses\LockingObservableLinkedList_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 88 — OlToDoTable Coverage (`UtilitiesCS\OutlookObjects\Table\OlToDoTable.cs`)

- [ ] [P88-T1] Add test to `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTable_Tests.cs` verifying that a missing To-Do default folder returns `null` from `GetToDoTable`
  - Acceptance: `[TestMethod]` exists, supplies a mocked `Store` returning null for the default To-Do folder, calls `GetToDoTable`, and asserts the result is null

- [ ] [P88-T2] Add test to `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTable_Tests.cs` verifying that the expected MAPI fields are cleared and re-added to the table columns
  - Acceptance: `[TestMethod]` exists, provides a mocked table with a known column set, calls the column-setup helper, and asserts the mock table's `Columns.RemoveAll` was called before the expected `Columns.Add` calls

- [ ] [P88-T3] Add test to `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTable_Tests.cs` verifying that unreadable items are skipped without failing the table build
  - Acceptance: `[TestMethod]` exists, supplies a mocked item that throws on property access, calls the table builder, and asserts the method completes without exception and skips the failing item

- [ ] [P88-T4] Register `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTable_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Table\OlToDoTable_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 89 — FilePathHelper Coverage (`UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs`)

- [ ] [P89-T1] Add test to `UtilitiesCS.Test\HelperClasses\FilePathHelper_Tests.cs` verifying that changing the `Name` property recomputes the dependent path/stem fields
  - Acceptance: `[TestMethod]` exists, constructs a `FilePathHelper` with a known initial path, sets `Name` to a new value, and asserts `FullName`, `Stem`, and related path properties are updated consistently

- [ ] [P89-T2] Add test to `UtilitiesCS.Test\HelperClasses\FilePathHelper_Tests.cs` verifying that `TryParseFileStem` handles empty, prefix-only, and suffix-only combinations correctly
  - Acceptance: `[TestMethod]` exists, calls `TryParseFileStem` with each boundary combination (empty string, prefix only, suffix only), and asserts the returned stem equals the expected value in each case

- [ ] [P89-T3] Add test to `UtilitiesCS.Test\HelperClasses\FilePathHelper_Tests.cs` verifying that `AdjustForMaxPath` truncates only the seed portion of the name while preserving extension and path prefix
  - Acceptance: `[TestMethod]` exists, provides a path that exceeds the max-path limit, calls `AdjustForMaxPath`, and asserts the result fits within the limit and the extension is preserved

- [ ] [P89-T4] Register `UtilitiesCS.Test\HelperClasses\FilePathHelper_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains `<Compile Include="HelperClasses\FilePathHelper_Tests.cs" />` and `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0

### Phase 90 — Final QC Pass

- [ ] [P90-T1] Run `dotnet tool run csharpier .` to format all modified C# files and confirm no formatting changes remain
  - Acceptance: Command exits with code 0 and reports no files were reformatted; evidence artifact saved to `evidence/qa-gates/final-qc-format.md` containing `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [ ] [P90-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` to confirm zero analyzer diagnostics
  - Acceptance: Build exits with code 0 with `0 Error(s)` and `0 Warning(s)`; evidence artifact saved to `evidence/qa-gates/final-qc-analyzers.md` containing `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [ ] [P90-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` to confirm zero nullable/type-safety warnings
  - Acceptance: Build exits with code 0 with no warnings treated as errors; evidence artifact saved to `evidence/qa-gates/final-qc-nullable.md` containing `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [ ] [P90-T4] Run `vstest.console.exe` against the `UtilitiesCS.Test` assembly with `/EnableCodeCoverage` and confirm all pre-existing tests still pass and no new test failures are introduced
  - Acceptance: All previously passing tests continue to pass; zero test failures; evidence artifact saved to `evidence/qa-gates/final-qc-test-coverage.md` containing `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` including total test count, pass count, and numeric post-change UtilitiesCS line coverage percentage

- [ ] [P90-T5] Confirm that every non-skipped phase (P1–P89 excluding P6, P7, P28, P31, P32, P33, P35, P37, P58, P59, P79) has a corresponding `<Compile Include="..." />` entry present in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains a `<Compile Include="..." />` line for each expected test file; the count of new entries equals the count of IMPLEMENT phases

- [ ] [P90-T6] Verify that line coverage for `UtilitiesCS` in the coverage report meets or exceeds the 80% repository-wide threshold
  - Acceptance: The coverage report produced by the `/EnableCodeCoverage` run in P90-T4 shows `UtilitiesCS` line coverage ≥ 80%; if not, identify remaining below-threshold files and record a follow-up note inline


