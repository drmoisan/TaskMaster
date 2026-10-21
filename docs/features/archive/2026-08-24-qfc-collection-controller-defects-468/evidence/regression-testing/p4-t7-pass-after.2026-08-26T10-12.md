# [P4-T7] Post-fix green state for issue #469 defect 3

Timestamp: 2026-08-26T10-12

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~ItemGroupsToMoveFieldDeclaresAnOrderedContract|FullyQualifiedName~TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation" `
    /Logger:"trx;LogFileName=p4-t7.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p4-t7
```

Clauses joined with `|`; vstest 18.8.0 rejects `OR`.

EXIT_CODE: 0

## Output Summary

```
Passed ItemGroupsToMoveFieldDeclaresAnOrderedContract [47 ms]
Passed TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation [25 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.3855 Seconds
```

TRX `<Counters>`:

```
total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | **0** |
| Passed count | exactly 2 | **2** |
| Failed count | exactly 0 | **0** |

### Fail-before / pass-after pairing

| Test | Red evidence | Green evidence |
|---|---|---|
| `ItemGroupsToMoveFieldDeclaresAnOrderedContract` | P4-T3, failed 1, message naming `ConcurrentDictionary`2[QuickFiler.Controllers.QfcItemGroup,System.Int32]` | this run, passed |
| `TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation` | none — deterministic red state impossible, see below | this run, passed |

`TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation` deliberately carries no fail-before
artifact. A `ConcurrentDictionary`'s enumeration order is *unspecified*, not guaranteed-wrong, so a
pre-fix run could return the expected order by chance and the assertion would be flaky by
construction. The deterministic pre-fix proof for issue #469 defect 3 is therefore carried by the
structural assertion (P4-T3), and this test carries the permanent post-fix behavioural contract. The
exception is recorded in the plan's fail-before dossier task (P14-T1), whose input list names "the
`#469-3` behavioural ordering test".

Host-identifier sanitisation was applied to the committed TRX exactly as recorded in the P2-T6
artifact. A post-substitution scan for the bare account name, the machine name in either casing, the
workspace absolute path, and the user-profile path returns zero hits.

Result: PASS.
