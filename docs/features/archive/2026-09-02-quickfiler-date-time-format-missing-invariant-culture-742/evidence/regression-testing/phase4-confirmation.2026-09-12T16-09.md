# Phase 4 Confirmation — Seven Named Tests Pass (issue #742, [P4-T4])

Timestamp: 2026-09-14T02-19

Command: `pwsh -NoProfile -Command '$vswherePath = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstestPath = (& $vswherePath -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe") | Select-Object -First 1; & $vstestPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /TestCaseFilter:"FullyQualifiedName~QuickFilerInvariantCultureIssue742Tests|FullyQualifiedName~WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps|FullyQualifiedName~QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine" /InIsolation /Logger:"console;verbosity=normal"; Write-Output "EXITCODE=$LASTEXITCODE"'`

EXIT_CODE: 0

Output Summary: `Test Run Successful. Total tests: 7.` All seven named tests passed:

```
Passed WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps [226 ms]
Passed QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine [3 ms]
Passed QuickFileMetricsWrite_UnderSentinelSeparatorCulture_RendersInvariantDataLineBeginning [28 ms]
Passed BuildQuickFileMetricLines_UnderSentinelSeparatorCulture_RendersInvariantDateAndTime [29 ms]
Passed GetItemSummary_UnderSentinelSeparatorCulture_RendersInvariantDateAndTime [< 1 ms]
Passed QfcCollectionControllerRenderingSites_UnderSentinelSeparatorCulture_RenderInvariantDateAndTime [46 ms]
Passed EfcItemControllerSentDateAndSentTime_UnderSentinelSeparatorCulture_RenderInvariantSeparators [< 1 ms]
```

Acceptance: `EXITCODE` is 0, covering all seven named tests — the five new tests from [P1-T1] plus
the two rewritten oracle tests from [P4-T1] and [P4-T2] — satisfied.

## Fail-before / pass-after pairing

The same five new tests failed 5 of 5 against the unfixed production tree in [P1-T4], recorded in
`expect-fail-new-tests.2026-09-12T16-09.md`, each on the date or time separator. The only change
between the two runs is the Phase 2 and Phase 3 production edits plus the Phase 4 oracle repair, so
the transition from 5 failed to 7 passed is attributable to the fix.

The two rewritten oracle tests were passing before this change as well. That is expected and is the
reason they needed repair: their expected values were built with the same uncultured
`ToString(format)` call the production code used, so the oracle tracked the defect instead of
detecting it. After [P4-T1] and [P4-T2] they name `CultureInfo.InvariantCulture` explicitly and can
no longer agree with a culture-dependent production rendering.

## Environment notes

- The run used `/InIsolation` and was scoped by `/TestCaseFilter` to the seven named tests, so it
  did not execute the `UtilitiesCS.Test` shell-icon classes known to stall vstest on this machine.
- The console logger was used; no `.trx` and no Cobertura XML was produced or committed by this
  task. The figures above are transcribed from the console output.
