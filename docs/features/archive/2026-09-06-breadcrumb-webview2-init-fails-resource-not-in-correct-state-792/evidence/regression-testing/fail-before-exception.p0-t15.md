# [P0-T15] AC-U4 pass-before observation — `PopulateFolderCombobox` half (fail-before exception dossier)

- Issue: #792
- Timestamp: 2026-09-17T18-49
- Command: CMD-VSTEST, then CMD-OUTLOOK and CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, exit 0, `0 Error(s)`, incremental: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` unchanged from the [P0-T11] Rebuild at 18:42:46), then `& $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcFormControllerTests.PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault" "/ResultsDirectory:coverage/test-results/p0-t15" "/Logger:trx;LogFileName=p0-t15.trx"` (CMD-SCOPED-RUN with `<task>` = `p0-t15`; run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; console captured to the gitignored `coverage/p0-t15-scoped.log`; TRX under the gitignored `coverage/test-results/p0-t15/`)
- EXIT_CODE: 0
- Output Summary: `Passed PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault [50 ms]`; `Test Run Successful.`; `Total tests: 1`; `Passed: 1`; `Failed: 0 (omitted category)`; `Skipped: 0 (omitted category)`; the filter discovered exactly one test, so the run is not vacuous.

## Why a failing run is impossible for this half of AC-U4

WhyFailingRunImpossible: `TryReportBoundaryFault(ex.Message, ex)` already exists at `QuickFiler/Controllers/EfcFormController.cs:1270` inside the catch block of `PopulateFolderCombobox` (declared at `EfcFormController.cs:1251`), so the `PopulateFolderCombobox` half of AC-U4 is already satisfied on the unfixed tree and cannot be observed failing. The work for this half is the strengthened user-surface test `PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink` (created in [P1-T2] in `QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs`), whose non-vacuity is proven by mutation in [P5-T1] (dropping the notifier call inside `DefaultBoundaryErrorSink`, which the existing test at `EfcFormControllerTests.cs:300` cannot detect because it asserts only the sink call count).

The other half of AC-U4 (`InitializeBreadcrumbHostAsync`) is a real change and is observed failing at [P1-T4]; it is not covered by this dossier.

## Alternative proof (pass-before observation)

The existing test `QuickFiler.Controllers.Tests.EfcFormControllerTests.PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault` (`QuickFiler.Test/Controllers/EfcFormControllerTests.cs:300`) passed on the unfixed tree as transcribed above, which pins the current behaviour that the strengthened test extends.

## Negative-evidence search (no prior dossier or failing run exists)

- SearchScope: `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/regression-testing/` and the whole feature folder `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/` (the feature is single-version; no `v1/` scope exists)
- SearchPatterns: `**/fail-before-exception.*.md`
- SearchResult: none (this file is the first dossier written for the feature)
