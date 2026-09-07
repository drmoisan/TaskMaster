# [P3-T3] [expect-fail] Pre-fix red state for issue #286 — throw later in the body

Timestamp: 2026-08-26T09-48

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter" `
    /Logger:"trx;LogFileName=p3-t3.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p3-t3
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

```
Failed RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter [252 ms]

Test Run Failed.
Total tests: 1
     Failed: 1
 Total time: 1.3259 Seconds
```

TRX `<Counters>`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Recorded failure message:

```
Expected ReadReentrancyCounter() to be 0 because issue #286 requires the Interlocked.Decrement to
cover the whole body between the increment and the normal exit, not merely the first statement, but
found 1 (difference of 1).
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| Failed count in the `p3-t3` TRX | exactly 1 | **1** |
| Exit code | non-zero, declared `ExpectedExitCode: 1` | **1** |

### Why this arrangement puts the throw later in the body

`_digits = 1` is injected by `CreateUninitializedController`, and a single item group is injected,
so the `Digits` getter computes `digitNeed = 1`, finds `_digits` already equal to it, and does not
set `_digitRefreshNeeded` — `SetVisualDigits` and its WinForms dependencies are never reached.
`_kbdHandler` is a `Mock<IQfcKeyboardHandler>` whose `StringActionsAsync` returns a **real**, empty
`KbdActions<string, KaStringAsync, Func<string, Task>>` rather than a mock, because
`UnregisterNavigation` calls `Remove("Collection", "1")` on it directly; `Remove` on an empty list
returns `false` without throwing. `UnregisterNavigation` therefore runs to completion.

The throw is instead raised by the mocked `IsActiveUI` getter at
`QfcCollectionController.cs:958` — `bool activeUI = _itemGroups[selection - 1].ItemController.IsActiveUI;`
— which is several statements past the increment.

As in P3-T2, the `ThrowAsync<InvalidOperationException>` assertion that precedes the counter
assertion **passed**, and the recorded failure is the counter assertion alone. That confirms the
arrangement reached the intended later throw site. A fix that guarded only the first statement would
make P3-T2 green while leaving this test red, so the pair pins the `finally` to the full span
between the increment and the decrement.

Host-identifier sanitisation was applied to the committed TRX exactly as recorded in the P2-T6
artifact. A post-substitution scan for the bare account name, the machine name in either casing, the
workspace absolute path, and the user-profile path returns zero hits.

Result: PASS (expected failure observed).
