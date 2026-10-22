# [P1-T15] `Keys.Down` decision-pin test

Timestamp: 2026-08-27T09-45
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerNavigationDigitsTests.RegisterAsyncKeyActions_RegistersExactlyOneDownBoundToSelectNextItemAsync"`
EXIT_CODE: 0

## Result (verbatim)

```
Passed RegisterAsyncKeyActions_RegistersExactlyOneDownBoundToSelectNextItemAsync [134 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
```

| Measure | Value |
| --- | --- |
| Total | 1 |
| Passed | **1** |
| Failed | **0** |

The test calls the `internal RegisterAsyncKeyActions()` on an uninitialized controller with a Loose
`Mock<IQfcKeyboardHandler>` whose `KeyActionsAsync` property is tracked with `SetupProperty`, then
asserts the resulting registry holds exactly one `("Collection", Keys.Down)` entry and exactly one
`("Collection", Keys.Up)` entry.

This test has **no pre-fix red state**: it pins behaviour upstream #468 already established by
deleting the ambiguous `WireUpKeyboardHandler` seed. `[P1-T18]` records the pass-after-only exception
dossier so the fail-before dossier is not read as carrying a missing red.

## Acceptance evaluation

- The run reports `Passed: 1` and `Failed: 0`. PASS.

Output Summary: 1 test run, 1 passed, 0 failed; the surviving live `Keys.Down` binding cardinality is
pinned.
