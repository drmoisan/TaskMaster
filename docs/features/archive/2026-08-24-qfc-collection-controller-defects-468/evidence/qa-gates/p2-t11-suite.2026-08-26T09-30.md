# [P2-T11] Full `QuickFiler.Test` suite after the issue #474 defect 1 retype

Timestamp: 2026-08-26T09-30

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p2-t11.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p2-t11
```

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 939
     Passed: 939
 Total time: 8.7256 Seconds
```

This is the third and final execution. Between the second (passing, 10.9434 seconds) and this one,
the only change to the tree was a line-ending restoration: an editing round-trip had rewritten
`QuickFiler/Controllers/QfcCollectionController.cs` and
`QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` from CRLF to LF, which
`dotnet tool run csharpier check .` rejected with "The file contained different line endings than
formatting it would result in". Both files were converted back to CRLF with their BOM state
preserved (`QfcCollectionController.cs` keeps its UTF-8 BOM; `QfcCollectionControllerDarkModeTests.cs`
has none, as it did at the base commit). `dotnet tool run csharpier check .` then reported
`Checked 1522 files` with `EXIT_CODE 0` repository-wide. The suite was rebuilt and re-run after that
restoration so this artifact records a run against the exact bytes that are committed.

TRX `<Counters>` (`p2-t11.trx`):

```
total="939" executed="939" passed="939" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | **0** |
| Failed count | exactly 0 | **0** |
| Total count vs P1-T8 baseline (938) | +1, the single test added by P2-T4 | **939** |

The `+1` is `ParentFieldAndConstructorParameterAreTypedIQfcFormController`, the only test method
added in Phase 2. No pre-existing test was removed or renamed.

### First attempt: nine environment-induced timeouts, retained for audit

The first execution of this command returned `EXIT_CODE 1` with `939 total / 930 passed / 9 failed`
in 7.3364 minutes. Its TRX is retained unmodified alongside the passing one as
`p2-t11-attempt1-flaky.trx` rather than being discarded, so the run is auditable.

All nine failures carried the identical message form `Test '<name>' timed out after 60000ms` — none
carried an assertion failure — and all nine belong to the same WinForms pump-host initialization
class:

| Failing test (first attempt) |
|---|
| `CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing` |
| `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` |
| `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` |
| `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` |
| `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` |
| `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` |
| `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` |
| `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` |
| `CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController` |

Three facts establish these as environment-induced and not a regression from this change:

1. **They exercise a different type.** Every one drives `QfcItemController` through the WinForms
   pump host. This phase changed `QfcCollectionController`'s `_parent` field type, its constructor
   parameter 5, and one call site; it touched no `QfcItemController` code path and no pump host.
2. **The failure mode is a wall-clock timeout, not an assertion.** `9 x 60000ms` accounts for
   almost exactly the difference between the first attempt's 7.3364 minutes and the passing run's
   10.9434 seconds. A behavioural regression would surface as an assertion failure with a stable
   duration, not as nine simultaneous 60-second stalls.
3. **They pass on re-run with the source tree unchanged.** No file was edited between the two runs.
   The machine also hosts unrelated concurrent work, and seventeen orphaned MSBuild worker processes
   from an earlier crashed run remain resident; per the environment constraint in force they were
   not terminated. Instantaneous processor utilisation sampled between the two runs was 35.3%,
   31.9%, and 10.6%.

The build precondition returned `EXIT_CODE 0` with `0 Error(s)` before both attempts, so neither
attempt was blocked by a locked `obj/` or `bin/` output.

Host-identifier sanitisation was applied to both TRX files exactly as recorded in the P2-T6
artifact. A post-substitution scan of each for the bare account name, the machine name in either
casing, the workspace absolute path, and the user-profile path returns zero hits. `/InIsolation`
also created an empty deployment scratch directory whose name embedded the account and machine
name; it contained no files and was removed.

Result: PASS.
