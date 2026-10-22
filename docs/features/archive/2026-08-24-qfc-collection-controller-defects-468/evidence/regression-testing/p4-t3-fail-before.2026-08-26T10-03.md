# [P4-T3] [expect-fail] Pre-fix red state for issue #469 defect 3

Timestamp: 2026-08-26T10-03

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~ItemGroupsToMoveFieldDeclaresAnOrderedContract" `
    /Logger:"trx;LogFileName=p4-t3.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p4-t3
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

```
Failed ItemGroupsToMoveFieldDeclaresAnOrderedContract

Test Run Failed.
Total tests: 1
     Failed: 1
 Total time: 1.2926 Seconds
```

TRX `<Counters>`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Recorded failure message:

```
Expected declared System.Collections.Concurrent.ConcurrentDictionary`2[QuickFiler.Controllers.QfcItemGroup,System.Int32]
to be assignable to System.Collections.Generic.IReadOnlyList`1[QuickFiler.Controllers.QfcItemGroup]
because issue #469 defect 3 requires the move collection to guarantee the insertion order that
TryGetItemGroupByIndex, MoveEmailsAsync and GetMoveDiagnostics all depend on when they resolve a
group by position, but it is not.
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| Failed count in the `p4-t3` TRX | exactly 1 | **1** |
| Failure message names the concurrent dictionary type | yes | `System.Collections.Concurrent.ConcurrentDictionary`2[QuickFiler.Controllers.QfcItemGroup,System.Int32]` appears verbatim |
| Exit code | non-zero, declared `ExpectedExitCode: 1` | **1** |

At the time of this run `QfcCollectionController.cs:70` declares
`private ConcurrentDictionary<QfcItemGroup, int> _itemGroupsToMove;`, and `TryGetItemGroupByIndex`
at `:2029-2039` resolves a group positionally with `_itemGroupsToMove.ElementAt(index).Key` inside a
`try`/`catch (System.Exception)` that converts any failure to `null`. `MoveEmailsAsync` at `:1975`
and `GetMoveDiagnostics` at `:2041` both drive that positional lookup.

Host-identifier sanitisation was applied to the committed TRX exactly as recorded in the P2-T6
artifact. A post-substitution scan for the bare account name, the machine name in either casing, the
workspace absolute path, and the user-profile path returns zero hits.

Result: PASS (expected failure observed).
