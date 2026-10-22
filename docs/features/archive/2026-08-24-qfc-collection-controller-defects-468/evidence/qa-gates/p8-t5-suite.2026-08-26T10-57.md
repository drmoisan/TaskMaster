# [P8-T5] Full `QuickFiler.Test` suite after the issue #470 defect 1 fix

Timestamp: 2026-08-26T10-57

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p8-t5.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p8-t5
```

EXIT_CODE: 0

## Output Summary

`Test Run Successful. Total tests: 957  Passed: 957`.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p8-t5/p8-t5.trx`:

```
total="957" executed="957" passed="957" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `0`, as the task's acceptance requires.

## Suite-size accounting

| Run | Total | Passed | Failed |
|---|---|---|---|
| P5-T6 (end of Phase 5) | 946 | 946 | 0 |
| P6-T6 (end of Phase 6) | 949 | 949 | 0 |
| P7-T13 (end of Phase 7) | 955 | 955 | 0 |
| P8-T5 (this run) | 957 | 957 | 0 |

The delta of `+2` is exactly the two tests added by P8-T1 and P8-T2. No test was removed.

This run also re-verifies the seven Phase 6 and Phase 7 tests after the conversation test file was
rewritten to a more compact documentation style (see `p8-t1-fail-before.2026-08-26T10-45.md` for
why). All seven pass, confirming the rewrite changed documentation only.

## Flaky first attempt, retained and analysed

The first attempt failed with `Total tests: 957  Passed: 947  Failed: 10`. That TRX is retained at
`evidence/qa-gates/p8-t5/p8-t5-attempt1-flaky.trx`. The re-run on a byte-identical tree passed
957/957.

The ten failures split into two known load-sensitive families, neither of which this feature
touches:

**Nine `QfcItemController` pump-host tests, all with the identical message shape
`Test '<name>' timed out after 60000ms`:**

- `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults`
- `CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController`
- `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`
- `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`
- `CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing`
- `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState`
- `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`
- `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme`
- `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread`

This is exactly the documented nine-test pump-host signature: these tests stand up a WinForms
message pump on a dedicated thread and wait on it with a fixed 60-second budget. Under CPU
contention the pump does not reach the awaited state inside the budget and every test sharing that
harness times out together, which is why they fail as a block of nine rather than individually.

**One `QfcDatamodel` background-worker test:**

- `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`, with
  `Expected entered.Task.Wait(TimeSpan.FromSeconds(5)) to be True because the started worker must
  reach the injected loader, but found False.`

This is the same bounded-wait pattern that produced the single flake analysed in
`p6-t6-suite.2026-08-26T10-26.md`, in a sibling test of the same file.

Classified as load-induced flakiness, not a regression, on these grounds:

1. **Every failure is a timeout, not an assertion about behaviour.** Nine carry the framework's own
   60-second timeout message; the tenth is a five-second `Task.Wait` returning `False`. None
   reports a wrong value.
2. **No call path connects them to this change.** P8-T3 edits `PromoteFirstChild`,
   `ToggleGroupConv(string)` and `ChangeConversationSilently(int, bool)` in
   `QfcCollectionController`. None of the ten tests constructs a `QfcCollectionController`; they
   exercise `QfcItemController` initialization and the `QfcDatamodel` email queue.
3. **This feature touches no pump host.** Every test added by this plan runs on the calling thread
   with no message pump and no WinForms control.
4. **They passed on re-run with a byte-identical tree.** No source file, build output, or test file
   changed between the two attempts.

Recorded rather than chased, per the plan's Conventions.

## Toolchain state at this run

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,524 files checked |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors |
| Test | this run | `EXIT_CODE 0`, 957 passed, 0 failed |

## Host-identifier sanitisation

Both TRX files were sanitised case-insensitively before commit: 2,878 substitutions in `p8-t5.trx`
and 2,880 in `p8-t5-attempt1-flaky.trx`. Post-sanitisation both contain zero occurrences of any of
the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`. The empty
`Deploy_<user> <timestamp>_<pid>` scaffolding directories vstest creates were removed.
