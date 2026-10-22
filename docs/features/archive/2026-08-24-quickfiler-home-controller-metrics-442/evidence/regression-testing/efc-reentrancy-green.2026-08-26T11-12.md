# Phase 3 — EFC Re-Entrancy Tests After the Primitive Change

Timestamp: 2026-08-26T11-12
Task: [P3-T6]
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"; $vt = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vt "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~TryBeginExecuteMoves" /Logger:trx "/ResultsDirectory:TestResults\p3-t6"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

TRX file: `TestResults\p3-t6\<account>_<HOST>_2026-08-26_11_12_48_net481.trx` (account and host
components redacted).

## Output Summary

```
Test Run Successful.
Total tests: 3
     Passed: 3
```

| Test | Source | Result before ([P3-T3]) | Result after (this run) |
| --- | --- | --- | --- |
| `TryBeginExecuteMoves_SecondCallBeforeReset_ReturnsFalse` | [P3-T1] | Passed | **Passed** |
| `TryBeginExecuteMoves_AfterResetExecuteMovesState_ReturnsTrue` | [P3-T2] | Passed | **Passed** |
| `TryBeginExecuteMoves_ReturnsFalseUntilExecutionStateIsReset` | pre-existing | Passed | **Passed** |

Identical results on both sides of the primitive swap. That is the behaviour-preservation half of
the alternative proof recorded in
`evidence/regression-testing/fail-before-exception.2026-08-26T11-10.md`: the change from a
non-atomic read-then-write to a single `Interlocked.CompareExchange` altered nothing that a
sequential caller can observe, while removing the interleaving the defect depended on.

## Post-change source form ([P3-T5])

`QuickFiler/Controllers/EfcHomeController.cs:393`

```csharp
private int _isExecuting;
```

`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs`

```csharp
internal bool TryBeginExecuteMoves()
{
    return Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 0;
}

internal void ResetExecuteMovesState()
{
    Interlocked.Exchange(ref _isExecuting, 0);
}
```

`git grep -n "volatile" -- QuickFiler/Controllers/EfcHomeController.cs` returns no match (exit 1),
satisfying the search half of AC-14. The explanatory comment on the field deliberately avoids the
literal token so the gate stays falsifiable: a commented occurrence would still be a match.

`using System.Threading;` was added to `EfcHomeController.ExecuteMoves.cs` for `Interlocked`. The
whole solution compiles clean.
