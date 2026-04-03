---
name: Remediation Plan: 2026-03-19-utilities-coverage-part-three-87 (2026-03-27T08-20)
status: Planned
work-mode: full-feature
source-issue: docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/issue.md
source-spec: docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/spec.md
source-user-story: docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/user-story.md
last-updated: 2026-04-03
---

# Remediation Plan — utilities-coverage-part-three-87

## Objective

Remediate the clean-branch blockers identified in `remediation-inputs.2026-03-27T08-20.md` so the branch diff is isolated to issue `#87`, every remaining non-skipped `UtilitiesCS` production file is either raised to `>= 80%` line coverage or re-validated as an explicit constrained skip, and the final C# QA loop passes in a single clean pass.

## Constraints

- Follow the repository C# toolchain in this exact order: format, analyzer build, nullable build, coverage-enabled MSTest.
- Do not weaken policy requirements.
- Do not introduce scope beyond issue `#87` remediation.
- Do not check off the main coverage acceptance criterion until verification passes.
- Treat `coverage/coverage.cobertura.xml` as the authoritative line-rate source.
- Reuse the existing `v2/plan.2026-03-22T21-00.md` phase map whenever a remaining below-threshold file already has a corresponding v2 phase.

## Remaining Scope Snapshot

### Implementation-routed files still below 80%

- `UtilitiesCS\Dialogs\InputBox.cs`
- `UtilitiesCS\Dialogs\MyBox.cs`
- `UtilitiesCS\Dialogs\NotImplementedDialog.cs`
- `UtilitiesCS\Dialogs\FunctionButton.cs`
- `UtilitiesCS\Dialogs\MyBoxViewer.cs`
- `UtilitiesCS\Dialogs\YesNoToAll.cs`
- `UtilitiesCS\Dialogs\DelegateButton.cs`
- `UtilitiesCS\EmailIntelligence\EmailParsingSorting\AutoFile.cs`
- `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs`
- `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailDataMiner.cs`
- `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFiler.cs`
- `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersController.cs`
- `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersViewer.cs`
- `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\OSBrowser.cs`
- `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapController.cs`
- `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapTree.cs`
- `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderSelector.cs`
- `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`
- `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.cs`
- `UtilitiesCS\EmailIntelligence\People\PeopleScoDictionaryNew.cs`
- `UtilitiesCS\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities.cs`
- `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs`
- `UtilitiesCS\EmailIntelligence\ClassifierGroups\MulticlassEngine.cs`
- `UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs`
- `UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.cs`
- `UtilitiesCS\EmailIntelligence\ClassifierGroups\OlFolder\OlFolderClassifierGroup.cs`
- `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage.cs`
- `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs`
- `UtilitiesCS\EmailIntelligence\ClassifierGroups\Actionable\ActionableClassifierGroup.cs`
- `UtilitiesCS\EmailIntelligence\Bayesian\CorpusInherit.cs`
- `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs`
- `UtilitiesCS\EmailIntelligence\Bayesian\BayesianSerializationHelper.cs`
- `UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\ClassifierGroup.cs`
- `UtilitiesCS\EmailIntelligence\IntelligenceConfig.cs`
- `UtilitiesCS\Extensions\DfDeedle.cs`
- `UtilitiesCS\Extensions\DfMLNet.cs`
- `UtilitiesCS\Extensions\AsyncSerialization.cs`
- `UtilitiesCS\Extensions\WinFormsExtensions.cs`
- `UtilitiesCS\HelperClasses\ToolTips\QfcTipsDetails.cs`
- `UtilitiesCS\HelperClasses\ToolTips\TipsController.cs`
- `UtilitiesCS\HelperClasses\Windows Forms\TableLayoutHelper.cs`
- `UtilitiesCS\HelperClasses\ThemeHelpers\ThemeControlGroup.cs`
- `UtilitiesCS\HelperClasses\FileSystem\FileInfoWrapper.cs`
- `UtilitiesCS\HelperClasses\FileSystem\DirectoryInfoWrapper.cs`
- `UtilitiesCS\HelperClasses\FileSystem\FileSystemInfoWrapper.cs`
- `UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs`
- `UtilitiesCS\HelperClasses\CloningFunctions\DispatchUtility.cs`
- `UtilitiesCS\OneDriveHelpers\OneDriveDownloader.cs`
- `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs`
- `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`
- `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigGroupBox.cs`
- `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigController.cs`
- `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigViewer.cs`
- `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`
- `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\ScBag.cs`
- `UtilitiesCS\ReusableTypeClasses\TimedActions\TimedDiskWriter.cs`
- `UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedList.cs`
- `UtilitiesCS\Threading\AsyncMultiTasker.cs`
- `UtilitiesCS\Threading\ProgressViewer.cs`
- `UtilitiesCS\Threading\ProgressTrackerAsync.cs`
- `UtilitiesCS\Threading\ProgressTrackerPane.cs`
- `UtilitiesCS\Threading\ProgressTracker.cs`
- `UtilitiesCS\Threading\ApplicationIdleTimer.cs`

### Skip candidates that still require explicit re-validation

- `UtilitiesCS\EmailIntelligence\Bayesian\Performance\ConfusionViewer.cs`
- `UtilitiesCS\EmailIntelligence\Bayesian\Performance\MetricChartViewer.cs`
- `UtilitiesCS\Threading\ProgressMultiStepViewer.cs`
- `UtilitiesCS\Threading\ThreadMonitor.cs`
- `UtilitiesCS\To Depricate\FileIO2.cs`
- `UtilitiesCS\HelperClasses\Windows Forms\ScreenHelper.cs`
- `UtilitiesCS\HelperClasses\ThemeHelpers\Theme.cs`
- `UtilitiesCS\HelperClasses\FileSystem\ShellUtilities.cs`
- `UtilitiesCS\HelperClasses\FileSystem\ShellUtilitiesStatic.cs`
- `UtilitiesCS\HelperClasses\ThemeHelpers\SystemThemeDetector.cs`

## Overview

This remediation pass starts by reconciling the live Cobertura inventory, the clean-branch diff, and the authoritative v2 phase map. It then removes residual out-of-scope diff entries, executes the high-risk dialog and async/helper follow-up work that the research identified as seam-sensitive, re-validates the documented skip set, reopens one explicit remediation task for every other remaining below-threshold file, and finishes with the required full QA and documentation loop.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Policy Read & Baseline Capture

- [x] [P0-T1] Read `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, and `.github/instructions/csharp-unit-test.instructions.md` in repository policy order
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-instructions-read.md` exists and contains `Timestamp:`, `Policy Order:`, and the five exact policy file paths in the required order.

- [x] [P0-T2] Read `v2/issue.md`, `v2/spec.md`, `v2/user-story.md`, `remediation-inputs.2026-03-27T08-20.md`, and `v2/plan.2026-03-22T21-00.md` before implementation begins
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-instructions-read.md` contains `Requirements Read:` followed by the five exact source paths, including `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/issue.md`, `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/spec.md`, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/user-story.md`.

- [x] [P0-T3] Capture the live branch-diff baseline with `git diff --name-status development...HEAD`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-branch-diff.md` exists and contains `Timestamp:`, `Command: git diff --name-status development...HEAD`, `EXIT_CODE: 0`, `Output Summary:`, and the full diff listing.

- [x] [P0-T4] Capture the baseline C# formatter result with `dotnet tool run csharpier .`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-csharpier.md` exists and contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T5] Capture the baseline analyzer-build result with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-analyzers.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T6] Capture the baseline nullable-build result with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-nullable.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T7] Capture the live coverage baseline with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-tests-with-coverage.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, and a numeric `UtilitiesCS Line Rate:` baseline value.

- [x] [P0-T8] Regenerate the remaining-file reconciliation ledger from `coverage/coverage.cobertura.xml` and classify every still-below-threshold file as either `Implementation` or `Skip Re-Validation`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-remaining-ledger.md` exists, lists every file named in the two scope sections above exactly once, and records a numeric `Baseline Line Rate:` plus a `Route:` of `Implementation` or `Skip Re-Validation` for each row.

- [x] [P0-T9] Map each `Implementation` row in `phase0-remaining-ledger.md` to its authoritative v2 phase or to a new remediation-only phase when no v2 phase exists
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-remaining-ledger.md` contains a non-empty `Phase:` column for every `Implementation` row, and `UtilitiesCS\EmailIntelligence\Bayesian\BayesianSerializationHelper.cs` is explicitly mapped to `Remediation-Only`.

### Phase 1 — Branch Isolation Cleanup

- [x] [P1-T1] Remove the out-of-scope file diff entry `VBFunctions.Test/ComputerInfo_Test.cs` from the branch
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p1-vbfunctions-computerinfo-cleanup.md` exists and contains `Timestamp:`, `Target Path: VBFunctions.Test/ComputerInfo_Test.cs`, and `Resolution:` describing the exact cleanup action.

- [x] [P1-T2] Remove the out-of-scope project diff entry `VBFunctions.Test/VBFunctions.Test.csproj` from the branch
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p1-vbfunctions-csproj-cleanup.md` exists and contains `Timestamp:`, `Target Path: VBFunctions.Test/VBFunctions.Test.csproj`, and `Resolution:` describing the exact cleanup action.

- [x] [P1-T3] Remove the out-of-scope documentation diff group `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/**` from the branch
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p1-issue96-docs-cleanup.md` exists and contains `Timestamp:`, `Target Path Group: docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/**`, and `Resolution:` describing the exact cleanup action.

- [x] [P1-T4] Remove the stale audit diff group `docs/features/active/2026-03-19-utilities-coverage-part-three-87/audit-2026-03-26T09-40/**` from the branch
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p1-stale-audit-cleanup.md` exists and contains `Timestamp:`, `Target Path Group: docs/features/active/2026-03-19-utilities-coverage-part-three-87/audit-2026-03-26T09-40/**`, and `Resolution:` describing the exact cleanup action.

- [x] [P1-T5] Re-capture the isolated branch diff after `P1-T1` through `P1-T4` complete
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/phase1-branch-diff-clean.md` exists and contains `Timestamp:`, `Command: git diff --name-status development...HEAD`, `EXIT_CODE: 0`, `Output Summary:`, and no rows whose path equals `VBFunctions.Test/ComputerInfo_Test.cs` or `VBFunctions.Test/VBFunctions.Test.csproj`, and no rows whose path starts with `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/` or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/audit-2026-03-26T09-40/`.

### Phase 2 — High-Risk Dialog and Async Follow-up Work

- [x] [P2-T1] Introduce a deterministic dialog-invoker seam for `UtilitiesCS\Dialogs\InputBox.cs` so the wrapper can be covered without opening a real modal dialog
	- Acceptance: `UtilitiesCS\Dialogs\InputBox.cs` exposes a replaceable dialog-invoker seam, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-inputbox-seam.md` records the new seam member name.

- [x] [P2-T2] Add an MSTest scenario in `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that `InputBox.cs` returns the accepted value produced by the injected dialog seam
	- Acceptance: the updated coverage report records `UtilitiesCS\Dialogs\InputBox.cs` at `>= 0.80`, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-inputbox-accepted.md` records the exact test method name added to `InputBox_Test.cs`.

- [x] [P2-T3] Add an MSTest scenario in `UtilitiesCS.Test\Dialogs\InputBox_Test.cs` verifying that `InputBox.cs` returns `null` when the injected dialog seam reports cancel
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-inputbox-cancel.md` records the exact test method name added to `InputBox_Test.cs`, and `coverage/coverage.cobertura.xml` still records `UtilitiesCS\Dialogs\InputBox.cs` at `>= 0.80`.

- [x] [P2-T4] Introduce a deterministic dialog-invoker seam for `UtilitiesCS\Dialogs\MyBox.cs` where the wrapper still depends on real modal display logic
	- Acceptance: `UtilitiesCS\Dialogs\MyBox.cs` exposes a replaceable dialog-invoker seam, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-mybox-seam.md` records the seam member name.

- [x] [P2-T5] Add an MSTest scenario in `UtilitiesCS.Test\Dialogs\MyBox_Tests.cs` verifying that `MyBox.cs` returns the expected button mapping for a simulated affirmative result
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-mybox-affirmative.md` records the exact test method name added to `MyBox_Tests.cs`, and the updated coverage report records `UtilitiesCS\Dialogs\MyBox.cs` at `>= 0.80`.

- [x] [P2-T6] Add an MSTest scenario in `UtilitiesCS.Test\Dialogs\MyBox_Tests.cs` verifying that `MyBox.cs` preserves the caller-supplied default result when the injected dialog seam returns that default path
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-mybox-default.md` records the exact test method name added to `MyBox_Tests.cs`, and `coverage/coverage.cobertura.xml` still records `UtilitiesCS\Dialogs\MyBox.cs` at `>= 0.80`.

- [x] [P2-T7] Introduce a deterministic notification seam for `UtilitiesCS\Dialogs\NotImplementedDialog.cs` if the current tests still leave wrapper-only branches uncovered
	- Acceptance: either `NotImplementedDialog.cs` exposes a replaceable notification seam and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-notimplemented-seam.md` records it, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-notimplemented-seam.md` records `Seam Not Required` with the exact uncovered members closed by test-only changes.

- [x] [P2-T8] Add an MSTest scenario in `UtilitiesCS.Test\Dialogs\NotImplementedDialog_Tests.cs` verifying the wrapper overload that supplies a custom message path
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-notimplemented-message.md` records the exact test method name added to `NotImplementedDialog_Tests.cs`, and the updated coverage report records `UtilitiesCS\Dialogs\NotImplementedDialog.cs` at `>= 0.80`.

- [x] [P2-T9] Add an MSTest scenario in `UtilitiesCS.Test\Dialogs\NotImplementedDialog_Tests.cs` verifying the wrapper overload that resolves the default not-implemented message path
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-notimplemented-default.md` records the exact test method name added to `NotImplementedDialog_Tests.cs`, and `coverage/coverage.cobertura.xml` still records `UtilitiesCS\Dialogs\NotImplementedDialog.cs` at `>= 0.80`.

- [x] [P2-T10] Introduce a deterministic dialog-invoker seam for `UtilitiesCS\Dialogs\YesNoToAll.cs` if response-selection still depends on modal display state
	- Acceptance: either `YesNoToAll.cs` exposes a replaceable dialog-invoker seam and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-yesnotoall-seam.md` records it, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-yesnotoall-seam.md` records `Seam Not Required` with the exact uncovered members closed by test-only changes.

- [x] [P2-T11] Add an MSTest scenario in `UtilitiesCS.Test\Dialogs\YesNoToAll_Tests.cs` verifying that the wrapper returns the `Yes` path when the dialog seam reports yes
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-yesnotoall-yes.md` records the exact test method name added to `YesNoToAll_Tests.cs`, and the updated coverage report records `UtilitiesCS\Dialogs\YesNoToAll.cs` at `>= 0.80`.

- [x] [P2-T12] Add an MSTest scenario in `UtilitiesCS.Test\Dialogs\YesNoToAll_Tests.cs` verifying that the wrapper returns the `No` path when the dialog seam reports no
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-yesnotoall-no.md` records the exact test method name added to `YesNoToAll_Tests.cs`, and `coverage/coverage.cobertura.xml` still records `UtilitiesCS\Dialogs\YesNoToAll.cs` at `>= 0.80`.

- [x] [P2-T13] Add an MSTest scenario in `UtilitiesCS.Test\Dialogs\YesNoToAll_Tests.cs` verifying that the wrapper returns the `All` path when the dialog seam reports all
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-yesnotoall-all.md` records the exact test method name added to `YesNoToAll_Tests.cs`, and `coverage/coverage.cobertura.xml` still records `UtilitiesCS\Dialogs\YesNoToAll.cs` at `>= 0.80`.

- [x] [P2-T14] Add an MSTest scenario in `UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs` verifying the next uncovered non-null mail-processing branch identified in `SortEmail.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-sortemail-followup.md` records the exact test method name added to `SortEmail_Tests.cs`, and the updated coverage report records `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs` at `>= 0.80`.

- [x] [P2-T15] Add an MSTest scenario in `UtilitiesCS.Test\EmailIntelligence\PeopleScoDictionaryNew_Tests.cs` verifying the next uncovered branch in `PeopleScoDictionaryNew.cs` after duplicate-add coverage
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-peoplesco-followup.md` records the exact test method name added to `PeopleScoDictionaryNew_Tests.cs`, and the updated coverage report records `UtilitiesCS\EmailIntelligence\People\PeopleScoDictionaryNew.cs` at `>= 0.80`.

- [x] [P2-T16] Create `UtilitiesCS.Test\EmailIntelligence\ManagerAsyncLazy_Tests.cs` for `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs`
	- Acceptance: `UtilitiesCS.Test\EmailIntelligence\ManagerAsyncLazy_Tests.cs` exists and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-managerasynclazy-testhome.md` records the file path.

- [x] [P2-T17] Register `UtilitiesCS.Test\EmailIntelligence\ManagerAsyncLazy_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
	- Acceptance: `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\ManagerAsyncLazy_Tests.cs" />`.

- [x] [P2-T18] Add an MSTest scenario in `UtilitiesCS.Test\EmailIntelligence\ManagerAsyncLazy_Tests.cs` verifying the lazy-success path for `ManagerAsyncLazy.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-managerasynclazy-success.md` records the exact test method name added to `ManagerAsyncLazy_Tests.cs`, and the updated coverage report records `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs` at `>= 0.80`.

- [x] [P2-T19] Add an MSTest scenario in `UtilitiesCS.Test\EmailIntelligence\ManagerAsyncLazy_Tests.cs` verifying the cached-or-faulted follow-up path for `ManagerAsyncLazy.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-managerasynclazy-followup.md` records the exact test method name added to `ManagerAsyncLazy_Tests.cs`, and `coverage/coverage.cobertura.xml` still records `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs` at `>= 0.80`.

- [x] [P2-T20] Add an MSTest scenario in `UtilitiesCS.Test\Extensions\AsyncSerialization_Tests.cs` verifying the next uncovered branch in `AsyncSerialization.cs` after the existing progress-formatting tests
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-asyncserialization-followup.md` records the exact test method name added to `AsyncSerialization_Tests.cs`, and the updated coverage report records `UtilitiesCS\Extensions\AsyncSerialization.cs` at `>= 0.80`.

- [x] [P2-T21] Add an MSTest scenario in `UtilitiesCS.Test\HelperClasses\TipsController_Tests.cs` verifying the next uncovered branch in `TipsController.cs` after the existing toggle tests
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p2-tipscontroller-followup.md` records the exact test method name added to `TipsController_Tests.cs`, and the updated coverage report records `UtilitiesCS\HelperClasses\ToolTips\TipsController.cs` at `>= 0.80`.

### Phase 3 — Skip Re-Validation

- [x] [P3-T1] Re-validate the skip rationale for `UtilitiesCS\EmailIntelligence\Bayesian\Performance\ConfusionViewer.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p3-confusionviewer-skip.md` exists and records `File: UtilitiesCS\EmailIntelligence\Bayesian\Performance\ConfusionViewer.cs` plus `Decision: Skip Confirmed` or `Decision: Return To Implementation`.

- [x] [P3-T2] Re-validate the skip rationale for `UtilitiesCS\EmailIntelligence\Bayesian\Performance\MetricChartViewer.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p3-metricchartviewer-skip.md` exists and records `Decision: Skip Confirmed` or `Decision: Return To Implementation`.

- [x] [P3-T3] Re-validate the skip rationale for `UtilitiesCS\Threading\ProgressMultiStepViewer.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p3-progressmultistepviewer-skip.md` exists and records `Decision: Skip Confirmed` or `Decision: Return To Implementation`.

- [x] [P3-T4] Re-validate the skip rationale for `UtilitiesCS\Threading\ThreadMonitor.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p3-threadmonitor-skip.md` exists and records `Decision: Skip Confirmed` or `Decision: Return To Implementation`.

- [x] [P3-T5] Re-validate the skip rationale for `UtilitiesCS\To Depricate\FileIO2.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p3-fileio2-skip.md` exists and records `Decision: Skip Confirmed` or `Decision: Return To Implementation`.

- [x] [P3-T6] Re-validate the skip rationale for `UtilitiesCS\HelperClasses\Windows Forms\ScreenHelper.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p3-screenhelper-skip.md` exists and records `Decision: Skip Confirmed` or `Decision: Return To Implementation`.

- [x] [P3-T7] Re-validate the skip rationale for `UtilitiesCS\HelperClasses\ThemeHelpers\Theme.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p3-theme-skip.md` exists and records `Decision: Skip Confirmed` or `Decision: Return To Implementation`.

- [x] [P3-T8] Re-validate the skip rationale for `UtilitiesCS\HelperClasses\FileSystem\ShellUtilities.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p3-shellutilities-skip.md` exists and records `Decision: Skip Confirmed` or `Decision: Return To Implementation`.

- [x] [P3-T9] Re-validate the skip rationale for `UtilitiesCS\HelperClasses\FileSystem\ShellUtilitiesStatic.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p3-shellutilitiesstatic-skip.md` exists and records `Decision: Skip Confirmed` or `Decision: Return To Implementation`.

- [x] [P3-T10] Re-validate the skip rationale for `UtilitiesCS\HelperClasses\ThemeHelpers\SystemThemeDetector.cs`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p3-systemthemedetector-skip.md` exists and records `Decision: Skip Confirmed` or `Decision: Return To Implementation`.

### Phase 4 — Remaining Reopened Coverage Phases

- [x] [P4-T1] Reopen v2 Phase 8 for `UtilitiesCS\EmailIntelligence\EmailParsingSorting\AutoFile.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\AutoFile_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\EmailParsingSorting\AutoFile.cs` at `>= 0.80`.

- [x] [P4-T2] Reopen v2 Phase 10 for `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersController.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersController.cs` at `>= 0.80`.

- [x] [P4-T3] Reopen v2 Phase 11 for `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersViewer.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersViewer.cs` at `>= 0.80`.

- [x] [P4-T4] Reopen v2 Phase 13 for `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\OSBrowser.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\OSBrowser_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\OSBrowser.cs` at `>= 0.80`.

- [x] [P4-T5] Reopen v2 Phase 14 for `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapController.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapController.cs` at `>= 0.80`.

- [x] [P4-T6] Reopen v2 Phase 16 for `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderSelector.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\FolderSelector_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderSelector.cs` at `>= 0.80`.

- [x] [P4-T7] Reopen v2 Phase 17 for `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\SubjectMapEncoder_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs` at `>= 0.80`.

- [x] [P4-T8] Reopen v2 Phase 19 for `UtilitiesCS\Extensions\DfDeedle.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Extensions\DfDeedle_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Extensions\DfDeedle.cs` at `>= 0.80`.

- [x] [P4-T9] Reopen v2 Phase 21 for `UtilitiesCS\HelperClasses\ToolTips\QfcTipsDetails.cs` and add the next deterministic scenario in `UtilitiesCS.Test\HelperClasses\QfcTipsDetails_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\HelperClasses\ToolTips\QfcTipsDetails.cs` at `>= 0.80`.

- [x] [P4-T10] Reopen v2 Phase 24 for `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigGroupBox.cs` and add the next deterministic scenario in `UtilitiesCS.Test\ReusableTypeClasses\ConfigGroupBox_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigGroupBox.cs` at `>= 0.80`.

- [x] [P4-T11] Reopen v2 Phase 25 for `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigViewer.cs` and add the next deterministic scenario in `UtilitiesCS.Test\ReusableTypeClasses\ConfigViewer_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigViewer.cs` at `>= 0.80`.
	- 2026-04-03 investigation note: staged handler-only tests were hardened to use headless viewer instances so they no longer construct a real `ConfigViewer` form during execution.

- [x] [P4-T12] Reopen v2 Phase 30 for `UtilitiesCS\Threading\ProgressViewer.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Threading\ProgressViewer_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Threading\ProgressViewer.cs` at `>= 0.80`.
	- 2026-04-03 investigation note: staged property-only tests were hardened to use headless viewer instances so they no longer construct a real `ProgressViewer` form during execution.

- [ ] [P4-T13] Reopen v2 Phase 34 for `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailDataMiner.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailDataMiner.cs` at `>= 0.80`.

- [ ] [P4-T14] Reopen v2 Phase 36 for `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.cs` at `>= 0.80`.

- [ ] [P4-T15] Reopen v2 Phase 38 for `UtilitiesCS\EmailIntelligence\IntelligenceConfig.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\IntelligenceConfig_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\IntelligenceConfig.cs` at `>= 0.80`.

- [ ] [P4-T16] Reopen v2 Phase 39 for `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFiler.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\EmailFiler_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFiler.cs` at `>= 0.80`.

- [ ] [P4-T17] Reopen v2 Phase 40 for `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigController.cs` and add the next deterministic scenario in `UtilitiesCS.Test\ReusableTypeClasses\ConfigController_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigController.cs` at `>= 0.80`.

- [ ] [P4-T18] Reopen v2 Phase 41 for `UtilitiesCS\Threading\AsyncMultiTasker.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Threading\AsyncMultiTasker_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Threading\AsyncMultiTasker.cs` at `>= 0.80`.

- [ ] [P4-T19] Reopen v2 Phase 42 for `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapTree.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\FolderRemapTree_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapTree.cs` at `>= 0.80`.

- [ ] [P4-T20] Reopen v2 Phase 43 for `UtilitiesCS\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\ClassifierGroupUtilities_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities.cs` at `>= 0.80`.

- [ ] [P4-T21] Reopen v2 Phase 45 for `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs` and add the next deterministic scenario in `UtilitiesCS.Test\ReusableTypeClasses\SCODictionary_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs` at `>= 0.80`.

- [ ] [P4-T22] Reopen v2 Phase 46 for `UtilitiesCS\HelperClasses\FileSystem\FileInfoWrapper.cs` and add the next deterministic scenario in `UtilitiesCS.Test\HelperClasses\FileInfoWrapper_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\HelperClasses\FileSystem\FileInfoWrapper.cs` at `>= 0.80`.

- [ ] [P4-T23] Reopen v2 Phase 47 for `UtilitiesCS\HelperClasses\FileSystem\DirectoryInfoWrapper.cs` and add the next deterministic scenario in `UtilitiesCS.Test\HelperClasses\DirectoryInfoWrapper_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\HelperClasses\FileSystem\DirectoryInfoWrapper.cs` at `>= 0.80`.

- [ ] [P4-T24] Reopen v2 Phase 48 for `UtilitiesCS\Extensions\DfMLNet.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Extensions\DfMLNet_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Extensions\DfMLNet.cs` at `>= 0.80`.

- [ ] [P4-T25] Reopen v2 Phase 49 for `UtilitiesCS\HelperClasses\Windows Forms\TableLayoutHelper.cs` and add the next deterministic scenario in `UtilitiesCS.Test\HelperClasses\TableLayoutHelper_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\HelperClasses\Windows Forms\TableLayoutHelper.cs` at `>= 0.80`.

- [ ] [P4-T26] Reopen v2 Phase 50 for `UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\SpamBayes_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.cs` at `>= 0.80`.

- [ ] [P4-T27] Reopen v2 Phase 51 for `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\ScBag.cs` and add the next deterministic scenario in `UtilitiesCS.Test\ReusableTypeClasses\ScBag_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\ScBag.cs` at `>= 0.80`.

- [ ] [P4-T28] Reopen v2 Phase 52 for `UtilitiesCS\EmailIntelligence\Bayesian\CorpusInherit.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\CorpusInherit_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\Bayesian\CorpusInherit.cs` at `>= 0.80`.

- [ ] [P4-T29] Reopen v2 Phase 53 for `UtilitiesCS\Dialogs\FunctionButton.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Dialogs\FunctionButton_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Dialogs\FunctionButton.cs` at `>= 0.80`.

- [x] [P4-T30] Reopen v2 Phase 54 for `UtilitiesCS\Dialogs\MyBoxViewer.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Dialogs\MyBoxViewer_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Dialogs\MyBoxViewer.cs` at `>= 0.80`.

- [ ] [P4-T31] Reopen v2 Phase 56 for `UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\CategoryClassifierGroup_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs` at `>= 0.80`.

- [ ] [P4-T32] Reopen v2 Phase 60 for `UtilitiesCS\HelperClasses\ThemeHelpers\ThemeControlGroup.cs` and add the next deterministic scenario in `UtilitiesCS.Test\HelperClasses\ThemeControlGroup_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\HelperClasses\ThemeHelpers\ThemeControlGroup.cs` at `>= 0.80`.

- [ ] [P4-T33] Reopen v2 Phase 61 for `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs` and add the next deterministic scenario in `UtilitiesCS.Test\OutlookObjects\OlTableExtensions_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs` at `>= 0.80`.

- [ ] [P4-T34] Reopen v2 Phase 62 for `UtilitiesCS\Threading\ProgressTrackerAsync.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Threading\ProgressTrackerAsync_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Threading\ProgressTrackerAsync.cs` at `>= 0.80`.

- [ ] [P4-T35] Reopen v2 Phase 63 for `UtilitiesCS\Extensions\WinFormsExtensions.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Extensions\WinFormsExtensions.cs` at `>= 0.80`.

- [ ] [P4-T36] Reopen v2 Phase 64 for `UtilitiesCS\EmailIntelligence\ClassifierGroups\MulticlassEngine.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\MulticlassEngine_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\ClassifierGroups\MulticlassEngine.cs` at `>= 0.80`.

- [ ] [P4-T37] Reopen v2 Phase 65 for `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\Triage_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage.cs` at `>= 0.80`.

- [ ] [P4-T38] Reopen v2 Phase 66 for `UtilitiesCS\Threading\ProgressTrackerPane.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Threading\ProgressTrackerPane_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Threading\ProgressTrackerPane.cs` at `>= 0.80`.

- [ ] [P4-T39] Reopen v2 Phase 67 for `UtilitiesCS\EmailIntelligence\ClassifierGroups\OlFolder\OlFolderClassifierGroup.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\OlFolderClassifierGroup_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\ClassifierGroups\OlFolder\OlFolderClassifierGroup.cs` at `>= 0.80`.

- [ ] [P4-T40] Reopen v2 Phase 68 for `UtilitiesCS\Threading\ApplicationIdleTimer.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Threading\ApplicationIdleTimer_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Threading\ApplicationIdleTimer.cs` at `>= 0.80`.

- [ ] [P4-T41] Reopen v2 Phase 70 for `UtilitiesCS\OneDriveHelpers\OneDriveDownloader.cs` and add the next deterministic scenario in `UtilitiesCS.Test\OneDriveHelpers\OneDriveDownloader_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\OneDriveHelpers\OneDriveDownloader.cs` at `>= 0.80`.

- [ ] [P4-T42] Reopen v2 Phase 72 for `UtilitiesCS\HelperClasses\FileSystem\FileSystemInfoWrapper.cs` and add the next deterministic scenario in `UtilitiesCS.Test\HelperClasses\FileSystemInfoWrapper_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\HelperClasses\FileSystem\FileSystemInfoWrapper.cs` at `>= 0.80`.

- [ ] [P4-T43] Reopen v2 Phase 73 for `UtilitiesCS\HelperClasses\CloningFunctions\DispatchUtility.cs` and add the next deterministic scenario in `UtilitiesCS.Test\HelperClasses\DispatchUtility_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\HelperClasses\CloningFunctions\DispatchUtility.cs` at `>= 0.80`.

- [ ] [P4-T44] Reopen v2 Phase 74 for `UtilitiesCS\Threading\ProgressTracker.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Threading\ProgressTracker.cs` at `>= 0.80`.

- [ ] [P4-T45] Reopen v2 Phase 76 for `UtilitiesCS\EmailIntelligence\ClassifierGroups\Actionable\ActionableClassifierGroup.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\ActionableClassifierGroup_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\ClassifierGroups\Actionable\ActionableClassifierGroup.cs` at `>= 0.80`.

- [ ] [P4-T46] Reopen v2 Phase 77 for `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs` and add the next deterministic scenario in `UtilitiesCS.Test\OutlookObjects\StoreWrapperController_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs` at `>= 0.80`.

- [ ] [P4-T47] Reopen v2 Phase 78 for `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\Triage_OlLogic_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs` at `>= 0.80`.

- [ ] [P4-T48] Reopen v2 Phase 80 for `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\BayesianPerformanceMeasurement_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs` at `>= 0.80`.

- [ ] [P4-T49] Reopen v2 Phase 83 for `UtilitiesCS\Dialogs\DelegateButton.cs` and add the next deterministic scenario in `UtilitiesCS.Test\Dialogs\DelegateButton_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\Dialogs\DelegateButton.cs` at `>= 0.80`.

- [ ] [P4-T50] Reopen v2 Phase 84 for `UtilitiesCS\ReusableTypeClasses\TimedActions\TimedDiskWriter.cs` and add the next deterministic scenario in `UtilitiesCS.Test\ReusableTypeClasses\TimedDiskWriter_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\ReusableTypeClasses\TimedActions\TimedDiskWriter.cs` at `>= 0.80`.

- [ ] [P4-T51] Add a remediation-only deterministic scenario for `UtilitiesCS\EmailIntelligence\Bayesian\BayesianSerializationHelper.cs` in `UtilitiesCS.Test\EmailIntelligence\BayesianSerializationHelper_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\Bayesian\BayesianSerializationHelper.cs` at `>= 0.80`, and `UtilitiesCS.Test\EmailIntelligence\BayesianSerializationHelper_Tests.cs` exists.

- [ ] [P4-T52] Register `UtilitiesCS.Test\EmailIntelligence\BayesianSerializationHelper_Tests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` if `P4-T51` created a new file
	- Acceptance: either `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="EmailIntelligence\BayesianSerializationHelper_Tests.cs" />`, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-bayesianserializationhelper-registration.md` records `Existing Test Home Reused`.

- [ ] [P4-T53] Reopen v2 Phase 86 for `UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\ClassifierGroup.cs` and add the next deterministic scenario in `UtilitiesCS.Test\EmailIntelligence\ClassifierGroup_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\ClassifierGroup.cs` at `>= 0.80`.

- [ ] [P4-T54] Reopen v2 Phase 87 for `UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedList.cs` and add the next deterministic scenario in `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedList_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedList.cs` at `>= 0.80`.

- [ ] [P4-T55] Reopen v2 Phase 89 for `UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs` and add the next deterministic scenario in `UtilitiesCS.Test\HelperClasses\FilePathHelper_Tests.cs`
	- Acceptance: the updated coverage report records `UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs` at `>= 0.80`.

- [ ] [P4-T56] If `P3-T1` recorded `Decision: Return To Implementation`, add the deterministic coverage scenario for `UtilitiesCS\EmailIntelligence\Bayesian\Performance\ConfusionViewer.cs` in a registered MSTest home; otherwise record `Not Reopened` in `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-confusionviewer-return.md`
	- Acceptance: either the updated coverage report records `UtilitiesCS\EmailIntelligence\Bayesian\Performance\ConfusionViewer.cs` at `>= 0.80` and the evidence file records the exact test method name, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-confusionviewer-return.md` records `Not Reopened` with `Source Decision: Skip Confirmed`.

- [ ] [P4-T57] If `P3-T2` recorded `Decision: Return To Implementation`, add the deterministic coverage scenario for `UtilitiesCS\EmailIntelligence\Bayesian\Performance\MetricChartViewer.cs` in a registered MSTest home; otherwise record `Not Reopened` in `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-metricchartviewer-return.md`
	- Acceptance: either the updated coverage report records `UtilitiesCS\EmailIntelligence\Bayesian\Performance\MetricChartViewer.cs` at `>= 0.80` and the evidence file records the exact test method name, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-metricchartviewer-return.md` records `Not Reopened` with `Source Decision: Skip Confirmed`.

- [ ] [P4-T58] If `P3-T3` recorded `Decision: Return To Implementation`, add the deterministic coverage scenario for `UtilitiesCS\Threading\ProgressMultiStepViewer.cs` in a registered MSTest home; otherwise record `Not Reopened` in `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-progressmultistepviewer-return.md`
	- Acceptance: either the updated coverage report records `UtilitiesCS\Threading\ProgressMultiStepViewer.cs` at `>= 0.80` and the evidence file records the exact test method name, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-progressmultistepviewer-return.md` records `Not Reopened` with `Source Decision: Skip Confirmed`.

- [ ] [P4-T59] If `P3-T4` recorded `Decision: Return To Implementation`, add the deterministic coverage scenario for `UtilitiesCS\Threading\ThreadMonitor.cs` in a registered MSTest home; otherwise record `Not Reopened` in `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-threadmonitor-return.md`
	- Acceptance: either the updated coverage report records `UtilitiesCS\Threading\ThreadMonitor.cs` at `>= 0.80` and the evidence file records the exact test method name, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-threadmonitor-return.md` records `Not Reopened` with `Source Decision: Skip Confirmed`.

- [ ] [P4-T60] If `P3-T5` recorded `Decision: Return To Implementation`, add the deterministic coverage scenario for `UtilitiesCS\To Depricate\FileIO2.cs` in a registered MSTest home; otherwise record `Not Reopened` in `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-fileio2-return.md`
	- Acceptance: either the updated coverage report records `UtilitiesCS\To Depricate\FileIO2.cs` at `>= 0.80` and the evidence file records the exact test method name, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-fileio2-return.md` records `Not Reopened` with `Source Decision: Skip Confirmed`.

- [ ] [P4-T61] If `P3-T6` recorded `Decision: Return To Implementation`, add the deterministic coverage scenario for `UtilitiesCS\HelperClasses\Windows Forms\ScreenHelper.cs` in a registered MSTest home; otherwise record `Not Reopened` in `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-screenhelper-return.md`
	- Acceptance: either the updated coverage report records `UtilitiesCS\HelperClasses\Windows Forms\ScreenHelper.cs` at `>= 0.80` and the evidence file records the exact test method name, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-screenhelper-return.md` records `Not Reopened` with `Source Decision: Skip Confirmed`.

- [ ] [P4-T62] If `P3-T7` recorded `Decision: Return To Implementation`, add the deterministic coverage scenario for `UtilitiesCS\HelperClasses\ThemeHelpers\Theme.cs` in a registered MSTest home; otherwise record `Not Reopened` in `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-theme-return.md`
	- Acceptance: either the updated coverage report records `UtilitiesCS\HelperClasses\ThemeHelpers\Theme.cs` at `>= 0.80` and the evidence file records the exact test method name, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-theme-return.md` records `Not Reopened` with `Source Decision: Skip Confirmed`.

- [ ] [P4-T63] If `P3-T8` recorded `Decision: Return To Implementation`, add the deterministic coverage scenario for `UtilitiesCS\HelperClasses\FileSystem\ShellUtilities.cs` in a registered MSTest home; otherwise record `Not Reopened` in `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-shellutilities-return.md`
	- Acceptance: either the updated coverage report records `UtilitiesCS\HelperClasses\FileSystem\ShellUtilities.cs` at `>= 0.80` and the evidence file records the exact test method name, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-shellutilities-return.md` records `Not Reopened` with `Source Decision: Skip Confirmed`.

- [ ] [P4-T64] If `P3-T9` recorded `Decision: Return To Implementation`, add the deterministic coverage scenario for `UtilitiesCS\HelperClasses\FileSystem\ShellUtilitiesStatic.cs` in a registered MSTest home; otherwise record `Not Reopened` in `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-shellutilitiesstatic-return.md`
	- Acceptance: either the updated coverage report records `UtilitiesCS\HelperClasses\FileSystem\ShellUtilitiesStatic.cs` at `>= 0.80` and the evidence file records the exact test method name, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-shellutilitiesstatic-return.md` records `Not Reopened` with `Source Decision: Skip Confirmed`.

- [ ] [P4-T65] If `P3-T10` recorded `Decision: Return To Implementation`, add the deterministic coverage scenario for `UtilitiesCS\HelperClasses\ThemeHelpers\SystemThemeDetector.cs` in a registered MSTest home; otherwise record `Not Reopened` in `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-systemthemedetector-return.md`
	- Acceptance: either the updated coverage report records `UtilitiesCS\HelperClasses\ThemeHelpers\SystemThemeDetector.cs` at `>= 0.80` and the evidence file records the exact test method name, or `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p4-systemthemedetector-return.md` records `Not Reopened` with `Source Decision: Skip Confirmed`.

### Phase 5 — Final QA and Documentation Loop

- [ ] [P5-T1] Run `dotnet tool run csharpier .`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/phase5-csharpier.md` exists and contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE: 0`, and `Output Summary:`.

- [ ] [P5-T2] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/phase5-analyzers.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and `Analyzer Diagnostics: 0`.

- [ ] [P5-T3] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/phase5-nullable.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and `Warnings As Errors: 0`.

- [ ] [P5-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/phase5-tests-with-coverage.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, `Failed Tests: 0`, and a numeric post-remediation `UtilitiesCS Line Rate:` value.

- [ ] [P5-T5] Verify the refreshed coverage report closes AC1 for `UtilitiesCS` and records changed/new-code coverage compliance
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/phase5-coverage-verification.md` exists and records `Baseline UtilitiesCS Line Rate:`, `Post-Remediation UtilitiesCS Line Rate:`, `Post-Remediation UtilitiesCS Line Rate: >= 0.80`, `Touched Production Files:`, `Per-File Baseline/Post Line Rates:` for every touched production file from `phase0-remaining-ledger.md`, `Coverage Regression Check: none`, `Changed/New-Code Coverage: <numeric value>`, `New Production Members Introduced: <count>`, and for any newly introduced production members `New Production Member Coverage: >= 0.90`; the artifact must also confirm that no `Implementation` row from `phase0-remaining-ledger.md` remains below threshold, each `Skip Re-Validation` row has a corresponding Phase 3 evidence file, and every Phase 3 row that recorded `Decision: Return To Implementation` has a corresponding completed task in `P4-T56` through `P4-T65` with matching evidence.

- [ ] [P5-T6] Update `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/plan.2026-03-22T21-00.md` to reflect the remediation outcomes for every reopened phase
	- Acceptance: each reopened phase referenced in this remediation plan is either checked off with linked evidence or annotated with a follow-up note that references the blocking evidence artifact.

- [ ] [P5-T7] Update `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/spec.md` and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/user-story.md` only after `P5-T5` passes
	- Acceptance: the acceptance-criteria checkboxes in `v2/user-story.md` and the matching DoD statements in `v2/spec.md` reflect the verified post-remediation state and do not mark the `Every .cs file compiled by UtilitiesCS.csproj has >=80% line coverage as reported by Cobertura` acceptance criterion complete unless `P5-T5` verified that no `Implementation` row from `phase0-remaining-ledger.md` remains below `0.80`, every retained skip row is backed by its required evidence artifact, and every `Return To Implementation` decision is closed by `P4-T56` through `P4-T65`.

- [ ] [P5-T8] Record the post-remediation review-artifact disposition only after `P5-T1` through `P5-T7` complete
	- Acceptance: `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/phase5-review-refresh.md` exists and contains `Timestamp:` plus either (a) `Review Refresh Requested: No`, or (b) `Review Refresh Requested: Yes` and references to `phase1-branch-diff-clean.md`, `phase5-tests-with-coverage.md`, and `phase5-coverage-verification.md` as the evidence baseline.

## Validation Target

- The executor should treat Phase 0 as a mandatory preflight gate.
- The executor must not start any Phase 2, Phase 3, or Phase 4 task until `P0-T1` through `P1-T5` are complete.
- The executor must rerun `P5-T1` through `P5-T4` from the top whenever any of `P5-T1` through `P5-T4` changes files or exits non-zero, and may treat Phase 5 as complete only after one clean pass across all four tasks.
