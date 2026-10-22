# [P3-T6] Full `QuickFiler.Test` suite after the issue #286 fix

Timestamp: 2026-08-26T09-55

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p3-t6.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p3-t6
```

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 941
     Passed: 941
 Total time: 7.5922 Seconds
```

TRX `<Counters>`:

```
total="941" executed="941" passed="941" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | **0** |
| Failed count | exactly 0 | **0** |

### Count progression

| Gate | Total | Passed | Failed |
|---|---|---|---|
| P0-T14 baseline (`QuickFiler.Test` only) | 938 | 938 | 0 |
| P1-T8 (after the `#468` dead-code removal) | 938 | 938 | 0 |
| P2-T11 (after the `#474-1` retype) | 939 | 939 | 0 |
| P3-T6 (this run) | **941** | **941** | **0** |

The `+2` over P2-T11 is exactly the two issue #286 tests added by P3-T2 and P3-T3. No pre-existing
test was removed, renamed, or newly failed.

The load-induced `QfcItemController_InitializationTests` pump-host timeouts recorded against the
first P2-T11 attempt did not recur in this run; all nine of those tests passed here.

Host-identifier sanitisation was applied to the committed TRX exactly as recorded in the P2-T6
artifact. A post-substitution scan for the bare account name, the machine name in either casing, the
workspace absolute path, and the user-profile path returns zero hits.

Result: PASS.
