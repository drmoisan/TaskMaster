# [P3-T5] Post-fix green state for issue #286

Timestamp: 2026-08-26T09-53

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter|FullyQualifiedName~RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter" `
    /Logger:"trx;LogFileName=p3-t5.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p3-t5
```

The two clauses are joined with `|`, not with `OR`: vstest 18.8.0 rejects the `OR` keyword inside
`/TestCaseFilter`.

EXIT_CODE: 0

## Output Summary

```
Passed RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter [64 ms]
Passed RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter [104 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.3693 Seconds
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
| `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` | P3-T2, failed 1, counter observed `1` against a pre-call `0` | this run, passed |
| `RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter` | P3-T3, failed 1, counter observed `1` against a pre-call `0` | this run, passed |

The single change between the red and green runs is P3-T4: the body of
`RemoveSpecificControlGroupAsync` from the statement after `Interlocked.Increment` through the
statement before `Interlocked.Decrement` is now inside a `try`, and the decrement is inside a
`finally`. The increment remains outside the `try`. A whitespace-ignoring diff of the production
file for that task is **10 insertions and 0 deletions** — the `try {`, the `finally {`, the closing
braces, and a four-line explanatory comment. No existing statement was altered, so the
race-condition `logger.Error` message and the unsynchronized plain read of the counter are both
unchanged, as P3-T4 requires.

Host-identifier sanitisation was applied to the committed TRX exactly as recorded in the P2-T6
artifact. A post-substitution scan for the bare account name, the machine name in either casing, the
workspace absolute path, and the user-profile path returns zero hits.

Result: PASS.
