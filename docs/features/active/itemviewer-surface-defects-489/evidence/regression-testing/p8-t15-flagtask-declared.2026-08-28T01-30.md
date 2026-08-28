# P8-T15 — FlagTaskDialogResult remains declared and its setter assertions still pass

Timestamp: 2026-08-28T01-30
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~AssignControls_WhenNotInvokeRequired_WritesAllIntentMembersFromSettings|FullyQualifiedName~AssignControls_WhenTaskFlagUnset_SetsCancelDialogResult" "/Logger:trx;LogFileName=p8-t15.trx" /ResultsDirectory:<temp-results-dir>
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

The run: `Test Run Successful.` — `Total tests: 2`, `Passed: 2`, 0 failed, 0 skipped, in 1.32
seconds.

```
Passed AssignControls_WhenNotInvokeRequired_WritesAllIntentMembersFromSettings [242 ms]
Passed AssignControls_WhenTaskFlagUnset_SetsCancelDialogResult [< 1 ms]
```

The declaration grep returns a match in **both** files:

```
QuickFiler/Viewers/IItemViewer.cs:72:        DialogResult FlagTaskDialogResult { get; set; }
QuickFiler/Viewers/ItemViewer.Commands.cs:97:        public DialogResult FlagTaskDialogResult
```

`git diff --numstat <BASELINE_SHA> -- QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`
produces **no output row** at all, so that file is byte-identical to `BASELINE_SHA`: the two tests
pass unchanged, not because they were adjusted.

TRX: `evidence/regression-testing/p8-t15.trx` (sanitised; 2 `UnitTestResult` elements, matching the
total above; parses under a strict XML reader after redaction).

## What P8-T8 removed and what it left

P8-T8 removed the redundant **read-back** only. Both members still write the property exactly once:

```
QuickFiler/Controllers/QfcItemController.MailActions.cs:209:            _itemViewer.FlagTaskDialogResult = flagTaskResult;
QuickFiler/Controllers/QfcItemController.MailActions.cs:229:                _itemViewer.FlagTaskDialogResult = flagTaskResult;
```

and `git grep -F -n "_itemViewer.FlagTaskDialogResult ==" -- QuickFiler/Controllers/QfcItemController.MailActions.cs`
returns zero. The two tests here assert **sets**, not gets:

```
QfcItemController.ViewerSetupTests.cs:258:            viewer.VerifySet(v => v.FlagTaskDialogResult = DialogResult.OK, Times.Once());
QfcItemController.ViewerSetupTests.cs:283:            viewer.VerifySet(v => v.FlagTaskDialogResult = DialogResult.Cancel, Times.Once());
```

They are therefore the direct evidence that the fix removed only the read side. Had P8-T8 gone
further and dropped the write, or routed it through a different member, both `Times.Once()`
assertions would have failed.

The task text locates these assertions at `:258` and `:283`, and they are still exactly there,
because the file is unmodified. `/TestCaseFilter:` requires a `FullyQualifiedName` operand — an
assertion line number is not runnable — so the two tests are named by their declaring methods,
`AssignControls_WhenNotInvokeRequired_WritesAllIntentMembersFromSettings` (`:238`) and
`AssignControls_WhenTaskFlagUnset_SetsCancelDialogResult` (`:266`).

## Baseline comparison

`ExpectedExitCode: 0` is declared because **neither** named test was recorded `failed` in P0-T13's
`BaselineNamedPins:` block, which records all nine pins as `passed`. No test needed attributing to a
sibling child, so the absolute count of 2 passed is asserted directly and the no-regression
comparison is trivially satisfied.

Output Summary: `FlagTaskDialogResult` remains declared on both `IItemViewer` (`:72`) and
`ItemViewer.Commands.cs` (`:97`), and both existing setter assertions pass unchanged —
`EXIT_CODE: 0`, `Total tests: 2`, `Passed: 2`, 0 failed, 0 skipped.
`QfcItemController.ViewerSetupTests.cs` produces no `git diff --numstat` row against
`BASELINE_SHA`, so the tests were not adjusted to accommodate the fix. P8-T8 removed the read-back
only; the property is still written once in each of `FlagAsTask` and `FlagAsTaskAsync`.
