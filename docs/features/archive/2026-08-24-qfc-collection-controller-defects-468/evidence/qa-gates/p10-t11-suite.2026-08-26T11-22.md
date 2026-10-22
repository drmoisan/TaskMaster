# [P10-T11] Full `QuickFiler.Test` suite after the issue #471 fix

Timestamp: 2026-08-26T11-22

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p10-t11.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p10-t11
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 962  Passed: 962`. Total time 10.22 s, first attempt, no flaky
retry needed.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p10-t11/p10-t11.trx`:

```
total="962" executed="962" passed="962" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `0`, as the task's acceptance requires.

## Suite-size accounting for Phase 10

| Run | Total | Passed | Failed | Delta |
|---|---|---|---|---|
| P9-T4 (end of Phase 9) | 958 | 958 | 0 | — |
| P10-T2 (at the `ShrinkByRows` seam) | 958 | 958 | 0 | 0 |
| P10-T11 (this run) | 962 | 962 | 0 | +4 |

The four added tests, each named by a Phase 10 plan task:

| Test | File | Task |
|---|---|---|
| `EliminateSpaceForItems_ReducesMinimumHeightByTemplateHeightTimesRemovalCount` | `QfcCollectionControllerLayout.StaTests.cs` | P10-T4 |
| `MakeSpaceThenEliminateSpace_IsMinimumHeightNeutral` | `QfcCollectionControllerLayout.StaTests.cs` | P10-T10 |
| `ShrinkByRows_WithPositiveRemovalCount_ReducesHeight` | `QfcCollectionControllerDefects468Tests.cs` | P10-T7 |
| `ShrinkByRows_WithNegativeRemovalCount_IncreasesHeight` | `QfcCollectionControllerDefects468Tests.cs` | P10-T7 |

No test was removed and no previously passing test regressed. The seam run at 958 is the evidence
that the extraction was behaviour-neutral; the +4 here is entirely accounted for by named tasks.

## Callers of the corrected method

`EliminateSpaceForItems` has one call site inside the controller, on the conversation-collapse
path. It continues to pass through the full suite. The change reverses the direction of a size
adjustment that was previously applied backwards; no caller passes a negative removal count, so no
caller relied on the inverted behaviour.

## Toolchain state at this run

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,525 files checked, 0 needing formatting |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors, 5 pre-existing warnings |
| Test | this run | `EXIT_CODE 0`, 962 passed, 0 failed |

The file count checked by CSharpier rose from 1,524 to 1,525: the one new file is
`QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs`. The analyzer and nullable
gates are Phase 15 tasks; the per-phase precondition the plan defines is `-Target Build`.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit: 2,893 substitutions. No
`Deploy_*` scaffolding directory was left behind. A post-sanitisation sweep returns zero hits for
every token class recorded in `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
