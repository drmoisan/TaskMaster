# P6-T5 — Full Discovered Test Set Through vstest.console.exe

Timestamp: 2026-08-31T20-35
Command: vstest.console.exe <9 assemblies> /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /Logger:trx /ResultsDirectory:coverage\testresults\p6-t5-rerun /EnableCodeCoverage /Settings:TaskMaster.runsettings
EXIT_CODE: 0
ExpectedExitCode: 0
Iteration: 1

RUNSETTINGS_PATH: `TaskMaster.runsettings` at the repository root.

The `/Settings:` argument is load-bearing and was not dropped. `vstest.console.exe` does not auto-detect the repository-root runsettings, and that file is the only source of the Code Coverage `ModulePaths/Exclude` list for Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing and MSTest. Without it the collector instruments those modules, which is the documented cause of instrumentation-induced failures recorded at `scripts/vscode/Invoke-MSTestWithCoverage.ps1` lines 318 through 320.

## Assembly discovery

ASSEMBLY_COUNT: 9. Required: at least 3. Holds, and it equals the assembly count the P0-T15 baseline run discovered.

The list was built by taking every file matching `*.Test.dll` under the workspace root whose path contains a `bin\Debug` output segment, then dropping any whose path **relative to the workspace root** contains a `.claude` segment. Applying the filter to the relative path rather than the full path is load-bearing here: this working tree is itself rooted under a path segment named `.claude`, so a full-path filter would match every candidate and drop all 9, silently producing an empty run. Measured: 9 discovered before the filter, 0 dropped by it, 9 kept.

The 9 assemblies: `QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`, each at `<project>\bin\Debug\<project>.dll`.

## Counts

- Total: 6899
- Passed: 6899
- Failed: 0
- Skipped: 0

Failed test names: none.

## Acceptance evaluation against the recorded baseline

The set of Failed test names is empty. `BASELINE_FAILURE_SET:` recorded in `evidence/baseline/p0-t19-baseline-failure-set.md` is the literal word `none`, so the subset relation holds and the clause that then applies requires `EXIT_CODE:` to be 0. It is 0.

CARRIED_BASELINE_FAILURES: not applicable. The recorded baseline is `none` rather than a name list, so no carried-failure branch is available and no non-zero exit code was authorized. None was needed.

## Individual result of each of the six named FileIO2_Tests methods

| Test method | Result | Duration |
|---|---|---|
| `WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying` | Passed | 2 ms |
| `WriteTextFileAsync_WhenEveryOpenAttemptFails_ShouldReturnFalseAfterBudget` | Passed | 1 ms |
| `WriteTextFileAsync_WhenTransientOpenFailureThenSucceeds_ShouldReturnTrueAndWriteAllLines` | Passed | 1 ms |
| `WriteTextFileAsync_WhenTokenAlreadyCancelled_ShouldThrowBeforeOpening` | Passed | 1 ms |
| `WriteTextFileAsync_WhenCancelledDuringRetryWindow_ShouldThrowPromptly` | Passed | 2 ms |
| `WriteTextFileAsync_WhenRetrying_ShouldPassCallerTokenToDelay` | Passed | 8 ms |

All six are recorded **Passed**.

## First invocation of this task, recorded for completeness

An earlier invocation of the byte-identical command, with TRX in `coverage\testresults\p6-t5`, reported Total 6899, Passed 6885, Failed 14, exit code 1. All 14 Failed tests reported a duration of approximately 1 minute, which is a timeout rather than an assertion failure, and all 14 belong to `QuickFiler.Test`:

`InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState`, `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose`, `CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController`, `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme`, `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent`, `CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing`, `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`, `Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`, `Transaction_DisposedTwice_DoesNotOverReleaseTheGate`, `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults`, `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException`, `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread`, `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`.

They were characterized rather than assumed:

1. **Re-run of the whole `QuickFiler.Test` assembly without `/EnableCodeCoverage`** reported Total 1272, Passed 1272, exit code 0. All 14 passed.
2. **Re-run of the byte-identical full command, with `/EnableCodeCoverage` and the same `/Settings:` path**, reported Total 6899, Passed 6899, exit code 0. All 14 passed. That is the accepted run recorded above.

Attribution: the 14 tests are the WinFormsPumpHost and `UiThread.Dispatcher` fixture tests in `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` and `QfcItemController.UiThreadDispatcherFixtureTests.cs`. They drive a real message pump on a dedicated thread under a fixed one-minute timeout, and they contend for a process-wide static dispatcher field. Under the additional overhead the Code Coverage collector imposes, and under concurrent machine load, that timeout is reachable. None of the 14 has any dependency on `FileIO2`, on the writer seam, or on any file in this change's footprint; the second re-run passing against a byte-identical command and an unchanged tree is the evidence that they are load-sensitive rather than a regression.

Output Summary: 6899 of 6899 tests passed across 9 assemblies with exit code 0, and all six named `FileIO2_Tests` methods are recorded Passed.
