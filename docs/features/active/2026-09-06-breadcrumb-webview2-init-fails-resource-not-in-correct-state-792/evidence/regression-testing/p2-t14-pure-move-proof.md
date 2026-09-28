# [P2-T14] Pure-move proof by the pre-existing suites

- Issue: #792
- Timestamp: 2026-09-17T19-26
- Command: CMD-OUTLOOK (`Get-Process -Name OUTLOOK`, printed `OUTLOOK-CLOSED: true`), then CMD-VSTEST (vswhere resolved `vstest.console.exe`), then CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`), then `& $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcFormControllerTests|FullyQualifiedName~EfcItemControllerTests|FullyQualifiedName~EfcDataModel|FullyQualifiedName~QfcCollectionControllerTests|FullyQualifiedName~ViewerQueueStaticWrapperTests|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests|FullyQualifiedName~WebView2BreadcrumbHostTests|FullyQualifiedName~EfcHomeController" "/ResultsDirectory:coverage/test-results/p2-t14" "/Logger:trx;LogFileName=p2-t14.trx"` (CMD-SCOPED-RUN with `<task>` = `p2-t14`; all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; build console captured to the gitignored `coverage/p2-t14-build-plain.log`, test console to `coverage/p2-t14-scoped.log`, TRX under the gitignored `coverage/test-results/p2-t14/`)
- EXIT_CODE: 0
- Output Summary: `Test Run Successful.`; `Total tests: 187`; `Passed: 187`; `Failed: 0 (omitted category)`; `Skipped: 0 (omitted category)`; `Total time: 2.4446 Seconds`. `Total tests:` equals `Passed:` and exceeds the required minimum of 60. No test failed, so no HALT.

## Build step

- `BUILD-EXIT_CODE: 0`; `Build succeeded.`; `    0 Warning(s)`; `    0 Error(s)` (exact-line match `^\s+0 Error\(s\)$` = true); 1 s (incremental, nothing to do).
- `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` timestamp 19:24:34, i.e. the assembly produced by the [P2-T13] nullable Rebuild, which ran after the last Phase 2 source edit (the `using UtilitiesCS;` repair recorded in `qa-gates/p2-t13-compile-gate.md`). The assemblies under test therefore contain every Phase 2 change.

## Run observations

- `PASSED-LINES-COUNTED: 187` (`Passed <name>` console lines) and `PASSED-TEST-NAMES-UNIQUE: 187`, consistent with the summary; no `Failed <name>` line and no discovery warning (`No test is available` / `No test matches`) in the 198-line console log.
- The filter discovered 187 tests, so the run is not vacuous.
- `scripts/vscode/TaskMaster.cli.runsettings`: `git diff --numstat origin/main -- scripts/vscode/TaskMaster.cli.runsettings` printed nothing (byte-identical to `origin/main`; `Workers=0`, `ClassLevel` unchanged). The run used `/Settings:` with that file and `/InIsolation`, as the plan's CMD-SCOPED-RUN specifies.

## What this run proves

Phase 2 made no behaviour change: the six-way `EfcFormController` split, the three verbatim member moves (`EfcItemController.WebViewEnvironment.cs`, `QfcCollectionController.PopOut.cs`, `EfcDataModel.Carry.cs`), the new `WebView2EnvironmentContract` type (no consumers yet), the declaration-only seams ([P2-T6] through [P2-T10], [P2-T12]) and the trailing optional constructor parameters plus carry deposit in `EfcHomeController` ([P2-T11]) leave every pre-existing test in the eight named suites passing. The conservation gates for the four moves are recorded in `qa-gates/p2-t13-compile-gate.md` (each `CONSERVATION-DIFF-COUNT: 0` with a non-zero positive control).
