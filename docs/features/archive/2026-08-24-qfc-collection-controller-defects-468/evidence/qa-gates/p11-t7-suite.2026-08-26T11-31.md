# [P11-T7] Full `QuickFiler.Test` suite after the issue #473 defect 1 fix

Timestamp: 2026-08-26T11-31

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p11-t7.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p11-t7
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 963  Passed: 963`. Total time 12.80 s, first attempt, no flaky
retry needed.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p11-t7/p11-t7.trx`:

```
total="963" executed="963" passed="963" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `0`, as the task's acceptance requires.

## Suite-size accounting for Phase 11

| Run | Total | Passed | Failed | Delta |
|---|---|---|---|---|
| P10-T11 (end of Phase 10) | 962 | 962 | 0 | — |
| P11-T2 (at the drain seam) | 962 | 962 | 0 | 0 |
| P11-T7 (this run) | 963 | 963 | 0 | +1 |

The one added test is `DrainBackgroundLoadingTasksAsync_AwaitsATaskAddedDuringTheDrainWindow` in
`QfcCollectionControllerDefects468Tests.cs`, named by P11-T4. No test was removed and no previously
passing test regressed.

## Callers of the changed member

The drain has two call sites, one on each of the two load paths in `<CTRL>`. Both were byte-identical
before the seam and both now call the same member. Both continue to pass through the full suite.
The fix strictly widens what the drain awaits — a caller that previously saw the drain return can now
only see it return later, never earlier — so no caller can observe a narrowing of behaviour.

## Toolchain state at this run

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,525 files checked, 0 needing formatting |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors, 5 pre-existing warnings |
| Test | this run | `EXIT_CODE 0`, 963 passed, 0 failed |

A transient sixth warning (`CS4014`, an unawaited `ContinueWith` in the new test) appeared while
P11-T4's test was being written and was closed by discarding the returned task before the
fail-before run. The warning count is back at the pre-existing 5, all of which are the
`System.Reactive` `packages.config` advisory emitted once per project.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit. Any `Deploy_*` scaffolding
directory was removed. A post-sanitisation sweep returns zero hits for every token class recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
