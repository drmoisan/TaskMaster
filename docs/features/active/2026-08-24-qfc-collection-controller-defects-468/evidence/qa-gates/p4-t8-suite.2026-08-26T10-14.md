# [P4-T8] Full `QuickFiler.Test` suite after the issue #469 defect 3 fix

Timestamp: 2026-08-26T10-14

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p4-t8.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p4-t8
```

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 943
     Passed: 943
 Total time: 12.9713 Seconds
```

TRX `<Counters>`:

```
total="943" executed="943" passed="943" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
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
| P1-T8 | 938 | 938 | 0 |
| P2-T11 | 939 | 939 | 0 |
| P3-T6 | 941 | 941 | 0 |
| P4-T8 (this run) | **943** | **943** | **0** |

The `+2` over P3-T6 is exactly the two issue #469 defect 3 tests added by P4-T1 and P4-T6.

### Non-regression of the retyped field

`QfcCollectionControllerTests.cs` injects `_itemGroupsToMove` by reflection. Because P4-T4 changed
the field's declared type, that injection had to change with it (P4-T5), otherwise `SetValue` would
have thrown at run time even though the assembly compiled. All thirteen of that file's pre-existing
tests pass in this run, and its `[TestMethod]` count is unchanged at **13**, equal to the P0-T15
baseline; its line count is **500**, at the repo cap and not above it.

Host-identifier sanitisation was applied to the committed TRX exactly as recorded in the P2-T6
artifact. A post-substitution scan for the bare account name, the machine name in either casing, the
workspace absolute path, and the user-profile path returns zero hits.

Result: PASS.
