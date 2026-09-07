# [P3-T2] [expect-fail] Pre-fix red state for issue #286 — throw at the first statement

Timestamp: 2026-08-26T09-42

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter" `
    /Logger:"trx;LogFileName=p3-t2.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p3-t2
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

```
Failed RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter [157 ms]

Test Run Failed.
Total tests: 1
     Failed: 1
 Total time: 1.2541 Seconds
```

TRX `<Counters>`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Recorded failure message:

```
Expected ReadReentrancyCounter() to be 0 because issue #286 requires the Interlocked.Decrement to
run on the exceptional exit path, so the counter must return to its pre-call value, but found 1
(difference of 1).
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| Failed count in the `p3-t2` TRX | exactly 1 | **1** |
| Failure message shows the counter one higher than its pre-call value | pre-call `0`, observed `1` | message reads `to be 0 ... but found 1 (difference of 1)` |
| Exit code | non-zero, declared `ExpectedExitCode: 1` | **1** |

The `ThrowAsync<NullReferenceException>` assertion that precedes the counter assertion **passed** —
the recorded failure is the counter assertion alone. That ordering matters: it proves the arrangement
reached the intended throw site rather than failing for an unrelated reason. `UnregisterNavigation()`
at `QfcCollectionController.cs:1109-1122` opens with `for (int i = 0; i < _itemGroups.Count; i++)`,
and `_itemGroups` is `null` on a controller allocated through
`FormatterServices.GetUninitializedObject`, so the dereference raises `NullReferenceException` at the
statement immediately after `Interlocked.Increment(ref removespecificcontrolgroupcounter)` at `:954`.
The matching `Interlocked.Decrement` at `:1040` is the method's last statement and is unreachable
after that throw, which is the defect.

The counter is reset to `0` in `[TestInitialize]` and again in `[TestCleanup]` (P3-T1), so the
pre-call value is deterministic regardless of test order or of which other tests ran first in the
same process.

Host-identifier sanitisation was applied to the committed TRX exactly as recorded in the P2-T6
artifact. A post-substitution scan for the bare account name, the machine name in either casing, the
workspace absolute path, and the user-profile path returns zero hits.

Result: PASS (expected failure observed).
