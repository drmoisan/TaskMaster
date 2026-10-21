# [P5-T1] [expect-fail] Pre-fix red state for issue #473 defect 2 — cancellation is swallowed

Timestamp: 2026-08-26T10-24

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException" `
    /Logger:"trx;LogFileName=p5-t1.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p5-t1
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

```
Failed MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException

Test Run Failed.
Total tests: 1
     Failed: 1
 Total time: 1.8194 Seconds
```

TRX `<Counters>`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Recorded failure message:

```
Expected a <System.OperationCanceledException> to be thrown because issue #473 defect 2 requires
cancellation to reach the caller so an aborted batch stops, instead of being swallowed by the broad
catch and logged as a move error, but no exception was thrown.
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| Failed count in the `p5-t1` TRX | exactly 1 | **1** |
| Exit code | non-zero, declared `ExpectedExitCode: 1` | **1** |

"but no exception was thrown" is the exact symptom of the defect: `TryMoveEmailByGroupAsync` catches
`System.Exception`, and `OperationCanceledException` derives from it, so the cancellation is
absorbed, recorded via `logger.Error` as a move failure, and the batch continues iterating the
remaining cached groups.

Host-identifier sanitisation was applied to the committed TRX exactly as recorded in the P2-T6
artifact. A post-substitution scan for the bare account name, the machine name in either casing, the
workspace absolute path, and the user-profile path returns zero hits.

Result: PASS (expected failure observed).
