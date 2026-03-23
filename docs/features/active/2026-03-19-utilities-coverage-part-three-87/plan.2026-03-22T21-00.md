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


