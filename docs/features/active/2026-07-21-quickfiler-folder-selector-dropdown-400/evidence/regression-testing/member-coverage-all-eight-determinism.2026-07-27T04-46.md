# All-eight test-assembly determinism gate

- Timestamp (UTC): 2026-07-27T04:46Z
- Task: P8-T67
- VSTest resolution: `vswhere.exe -latest -products * -property installationPath`, then `Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\vstest.console.exe`.

## Ordered assembly paths

1. `QuickFiler.Test\\bin\\Debug\\QuickFiler.Test.dll`
2. `Tags.Test\\bin\\Debug\\Tags.Test.dll`
3. `TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll`
4. `TaskTree.Test\\bin\\Debug\\TaskTree.Test.dll`
5. `TaskVisualization.Test\\bin\\Debug\\TaskVisualization.Test.dll`
6. `ToDoModel.Test\\bin\\Debug\\ToDoModel.Test.dll`
7. `UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll`
8. `VBFunctions.Test\\bin\\Debug\\VBFunctions.Test.dll`

## Direct foreground commands and results

Both runs used the ordered assembly paths above, `scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`, detailed console logging, and the stated canonical results directory. No PowerShell output buffer, capture, pipeline, redirection, or background execution was used.

1. `& $vstestPath @assemblyPaths '/Settings:scripts/vscode/TaskMaster.cli.runsettings' '/InIsolation' '/TestCaseFilter:TestCategory!=LiveOutlook' '/Logger:console;verbosity=detailed' "/ResultsDirectory:$resultsDirectory" '/Logger:trx;LogFileName=member-coverage-all-eight-determinism-run-1.2026-07-27T04-43.trx'`
   - `EXIT_CODE=0`; 6,056 total, 6,056 passed, 0 failed, 0 skipped.
   - TRX: `member-coverage-all-eight-determinism-run-1.2026-07-27T04-43.trx`
   - SHA-256: `4818FB2D3CD4F7C8B6AFB6D901894A10229D900319030B0A79B0CD19D2DDAE60`
2. `& $vstestPath @assemblyPaths '/Settings:scripts/vscode/TaskMaster.cli.runsettings' '/InIsolation' '/TestCaseFilter:TestCategory!=LiveOutlook' '/Logger:console;verbosity=detailed' "/ResultsDirectory:$resultsDirectory" '/Logger:trx;LogFileName=member-coverage-all-eight-determinism-run-2.2026-07-27T04-44.trx'`
   - `EXIT_CODE=0`; 6,056 total, 6,056 passed, 0 failed, 0 skipped.
   - TRX: `member-coverage-all-eight-determinism-run-2.2026-07-27T04-44.trx`
   - SHA-256: `E92CE749818F451F8689B680F61D5CE78F987CA75870B099A2D5C381133D36F3`

## Required result verification

Each TRX contains exactly one `Passed` result for each of the following methods:

- `QuickFiler.Test.Viewers.BreadcrumbCoordinatorUpgradeLifetimeTests.ArgumentGuards_NullInputsThrowArgumentNullException`
- `QuickFiler.Test.Viewers.BreadcrumbCoordinatorUpgradeLifetimeTests.RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure`
- `QuickFiler.Test.Viewers.BreadcrumbCoordinatorUpgradeLifetimeTests.RunAsync_SupersededCancellationIsSwallowedAndSettled`
- `QuickFiler.Test.Viewers.BreadcrumbCoordinatorUpgradeLifetimeTests.Disposal_RepeatedLifetimeDisposeIsSafeAndLeaseDisposeFailureIsReported`
- `QuickFiler.Test.Viewers.BreadcrumbDropDownOpenCoordinatorTests.Reset_HostAlreadyClosedWithOpenSelector_CancelsExactlyOnce`
- `QuickFiler.Test.Viewers.BreadcrumbDropDownOpenCoordinatorTests.SetDroppedDown_CloseThrows_ReportsOnceAndAllowsRetry`
- `QuickFiler.Test.Viewers.BreadcrumbCoordinatorLifecycleTests.PostRenderAndSelectorAsync_StaleLeaseReturnsCompletedWithoutPublishing`
- `QuickFiler.Test.Viewers.BreadcrumbDropDownLifecycleCoverageTests.Host_DisposeAndUseAfterDispose_FollowDeterministicContract`

Both determinism runs passed. No blame or hang diagnostic was required. No source, coverage policy, scope, settings, filter, exclusion, threshold, or postprocessor was changed.
