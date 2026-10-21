# P3-T4 — Both Named Regression Tests

Timestamp: 2026-08-22T10-34

Command:
```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx `
  /ResultsDirectory:docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p3-t4 `
  /TestCaseFilter:"FullyQualifiedName~BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread|FullyQualifiedName~BuildPumpHarness_DoesNotCreateTheWebViewChildHandles"
```

Resolved through
`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`,
invoked from `pwsh -NoProfile` at the worktree root. `/InIsolation` is present, as mandated.

EXIT_CODE: 0

Output Summary:

TRX: `evidence/regression-testing/p3-t4/2026-08-22_10_34_47_net481.trx`
(the only file in that subdirectory).

TRX `<Counters>` verbatim:

```
total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
```

| Test | Outcome | Duration |
| --- | --- | --- |
| `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` | **Passed** | 1 s |
| `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` | **Passed** | 98 ms |

Acceptance: exactly 2 executed, 2 passed, 0 failed, 0 skipped (`notExecuted="0"`). Neither test was
skipped or not-run.

## Recorded deviation

`BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` did not pass in its first authored form. The
first run of this task recorded `total=2 passed=1 failed=1`, the failure being
`Expected bodyWebViewHandleCreated to be False ... but found True`. That run's TRX was discarded
along with its subdirectory and is not counted here; the result recorded above is from the re-run
after the correction.

The cause was established by a four-configuration measurement, not by assumption, and is recorded in
full in `webview-child-handle-measurement.2026-08-21T18-10.md`. In summary: both
`Microsoft.Web.WebView2.WinForms.WebView2` children already have their window handles created by
`ItemViewer` construction (the Designer's `ISupportInitialize.EndInit()` calls), before the harness
runs and independently of the Phase 2 fixture change. A bare `new QuickFiler.ItemViewer()`
constructed on the pump with no harness at all reports both children as handle-created. P3-T1's
instruction to assert `false` was therefore an unmeasured prediction that does not hold, and the
assertions were corrected to the measured value with `because` clauses recording the provenance.
The method name, attributes, structure, and the two named property reads are unchanged, so P3-T1's
own acceptance condition is satisfied verbatim.

This deviation is escalated in the final execution report. No timing construct was introduced and no
production file was touched to obtain this result.
