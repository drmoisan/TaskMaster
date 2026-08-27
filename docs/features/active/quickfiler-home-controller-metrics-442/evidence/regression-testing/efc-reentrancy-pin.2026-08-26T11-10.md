# Phase 3 — EFC Re-Entrancy Contract Pinned Against the Pre-Change Primitive

Timestamp: 2026-08-26T11-10
Task: [P3-T3]
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"; $vt = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vt "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~TryBeginExecuteMoves" /Logger:trx "/ResultsDirectory:TestResults\p3-t3"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

TRX file: `TestResults\p3-t3\<account>_<HOST>_2026-08-26_11_10_56_net481.trx` (account and host
components redacted).

## Output Summary

```
Test Run Successful.
Total tests: 3
     Passed: 3
```

| Test | Source | Result |
| --- | --- | --- |
| `TryBeginExecuteMoves_SecondCallBeforeReset_ReturnsFalse` | [P3-T1] | Passed (4 ms) |
| `TryBeginExecuteMoves_AfterResetExecuteMovesState_ReturnsTrue` | [P3-T2] | Passed (< 1 ms) |
| `TryBeginExecuteMoves_ReturnsFalseUntilExecutionStateIsReset` | pre-existing, matched by the same filter | Passed (56 ms) |

Both new tests pass **before** the primitive change, by design. They are not regression tests for
root cause RC-6; they exist to pin the observable single-threaded contract of
`TryBeginExecuteMoves` and `ResetExecuteMovesState` across the swap from
`private volatile bool _isExecuting` to `private int _isExecuting` with
`Interlocked.CompareExchange`. Re-running them after the swap ([P3-T6]) demonstrates that the
change to the synchronisation primitive did not alter the behaviour any caller can observe
sequentially.

The reason no failing-first run exists for RC-6 is recorded in the fail-before exception dossier
`evidence/regression-testing/fail-before-exception.2026-08-26T11-10.md`, written by [P3-T4].

At the time of this run `QuickFiler/Controllers/EfcHomeController.cs:389` still reads
`private volatile bool _isExecuting;` and
`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:48-57` still performs the non-atomic
read-then-write. That is the state this pin was taken against.
