# [P9-T4] Full `QuickFiler.Test` suite after the issue #470 defect 3 fix

Timestamp: 2026-08-26T11-03

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p9-t4.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p9-t4
```

EXIT_CODE: 0

## Output Summary

`Test Run Successful. Total tests: 958  Passed: 958`.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p9-t4/p9-t4.trx`:

```
total="958" executed="958" passed="958" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `0`, as the task's acceptance requires. The run completed on the first
attempt; no flaky retry was needed.

## Suite-size accounting across the delegated window

| Run | Total | Passed | Failed | Delta |
|---|---|---|---|---|
| P5-T6 (end of Phase 5, inherited) | 946 | 946 | 0 | — |
| P6-T6 (end of Phase 6) | 949 | 949 | 0 | +3 |
| P7-T13 (end of Phase 7) | 955 | 955 | 0 | +6 |
| P8-T5 (end of Phase 8) | 957 | 957 | 0 | +2 |
| P9-T4 (this run) | 958 | 958 | 0 | +1 |

Total added across Phases 6 through 9: **12 tests**, every one of them accounted for by a plan task
that names it. No test was removed at any point and no previously passing test regressed.

## Callers of `SetVisualDigits` unaffected

`SetVisualDigits` has three call sites inside `QfcCollectionController`
(`:1009`, `:1125` at the pre-change numbering, and the `ToggleUnGroupConv` digit-refresh path). All
three continue to pass through the full suite. The change is additive: a group that was previously
processed is still processed identically, because the guard only diverts groups that would have
thrown.

## Toolchain state at this run

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,524 files checked |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors |
| Test | this run | `EXIT_CODE 0`, 958 passed, 0 failed |

The analyzer gate (`/t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) and
the nullable gate (`/t:Rebuild ... /p:TreatWarningsAsErrors=true`) are Phase 15 tasks and are not
run here; the per-phase precondition the plan defines is `-Target Build`.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 2,881 substitutions. Post-sanitisation the
file contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`. The empty
`Deploy_<user> <timestamp>_<pid>` scaffolding directory vstest creates was removed.
