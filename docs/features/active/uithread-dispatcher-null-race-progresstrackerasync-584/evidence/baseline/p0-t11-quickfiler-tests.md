# P0-T11 — QuickFiler.Test baseline run

Timestamp: 2026-09-03T08-28

Command:
```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 PATH="<resolved-vstest-dir>:$PATH" vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults/p0-t11 /TestCaseFilter:TestCategory!=LiveOutlook
```

EXIT_CODE: 0

## Output Summary

Console summary block, verbatim:

```text
Test Run Successful.
Total tests: 1312
     Passed: 1312
 Total time: 14.2187 Seconds
```

- **Total tests: 1312** (console summary block)
- **Passed: 1312** (console summary block)
- **Failed: 0** (read from the `failed` attribute of the single `<Counters .../>` element in the TRX
  written under `TestResults/p0-t11/`)
- **Skipped: 0** (derived as `total` minus `executed`)

TRX `<Counters .../>` values used for the derivation:

- `total` = **1312**
- `executed` = **1312**
- `failed` = **0**
- derived `Skipped` = 1312 - 1312 = **0**

The `notExecuted` attribute was NOT used, per constraint 5 of "Shell constraints measured in this
worktree".

TRX SELECTED: most recently modified .trx in TestResults/p0-t11/
Last-modified timestamp of the selected file: `2026-09-03 08:28:04.618542900 -0400`.
That directory held two `.trx` files (an earlier one dated 2026-09-02 from a prior preparation-cycle
run, and the one this task produced), so the TRX selection rule applies. The selected file's own name
is deliberately not recorded, and the run's `Results File:` console line is deliberately not quoted.

BASELINE_FAILURE_SET: empty. `Failed` is 0.

## Acceptance

All four counts are recorded as concrete numbers, the `total` and `executed` values from which
`Skipped` was derived are recorded, and `TestResults/p0-t11/` is identified as the results directory
`Failed` and `Skipped` were read from, without a TRX filename and without a quoted `Results File:`
line.

This assembly is baselined because the sibling audit in P3-T6 found that
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` constructs the parameterless `WpfUiDispatcher`,
whose provider closes over `UiThread.Dispatcher`. `Total tests: 1312` is the figure P4-T6's
acceptance compares against.
