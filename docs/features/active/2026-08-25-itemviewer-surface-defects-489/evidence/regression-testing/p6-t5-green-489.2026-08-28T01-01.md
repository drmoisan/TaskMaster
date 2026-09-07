# P6-T5 — GREEN for #489 D2 and D4, all six tests pass

Timestamp: 2026-08-28T01-01
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p6-t5.trx" "/TestCaseFilter:FullyQualifiedName~HtmlDarkConverter_WhenInvokeRequired_MarshalsThroughInvoke|FullyQualifiedName~HtmlDarkConverter_WhenInvokeRequired_DoesNotNavigateWithoutMarshalling|FullyQualifiedName~IItemViewer_DeclaresNoUiSchedulerMember|FullyQualifiedName~HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly|FullyQualifiedName~IItemViewer_StillDeclaresUiDispatcher|FullyQualifiedName~IItemViewer_StillDeclaresUiSyncContext" /ResultsDirectory:<temp>
EXIT_CODE: 0
ExpectedExitCode: 0

Ran: 6
Passed: 6
Failed: 0
Skipped: 0

## Acceptance

`Test Run Successful. / Total tests: 6 / Passed: 6`.

```
Passed HtmlDarkConverter_WhenInvokeRequired_MarshalsThroughInvoke [255 ms]
Passed HtmlDarkConverter_WhenInvokeRequired_DoesNotNavigateWithoutMarshalling [4 ms]
Passed HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly [27 ms]
Passed IItemViewer_DeclaresNoUiSchedulerMember [3 ms]
Passed IItemViewer_StillDeclaresUiDispatcher [1 ms]
Passed IItemViewer_StillDeclaresUiSyncContext [< 1 ms]
```

## Red-to-green transition, per test

| Test | Role | Before fix | After fix |
|---|---|---|---|
| `HtmlDarkConverter_WhenInvokeRequired_MarshalsThroughInvoke` | RED (D2) | failed, P5-T6 | passed |
| `HtmlDarkConverter_WhenInvokeRequired_DoesNotNavigateWithoutMarshalling` | RED (D2) | failed, P5-T6 | passed |
| `IItemViewer_DeclaresNoUiSchedulerMember` | RED (D4) | failed, P5-T7 | passed |
| `HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly` | pin | passed, P5-T5 | passed |
| `IItemViewer_StillDeclaresUiDispatcher` | pin | passed, P5-T5 | passed |
| `IItemViewer_StillDeclaresUiSyncContext` | pin | passed, P5-T5 | passed |

All three REDs flipped to green and all three pins held. No test was modified between the RED runs
and this one; the only changes are the three Phase 6 production edits:

- P6-T1 added the `InvokeRequired` guard to `HtmlDarkConverter`, so a call arriving with
  `InvokeRequired` true now marshals through `_itemViewer.Invoke` exactly once and performs no direct
  `NavigateToString`. That flips both D2 REDs.
- P6-T2 removed the `UiScheduler` declaration from `IItemViewer.cs:37`, flipping the D4 RED.
- P6-T3 removed the `_uiScheduler` capture and the `UiScheduler` property from `ItemViewer.cs`.

The two surviving pins are the meaningful ones for over-deletion. `UiScheduler` was removed from a
three-line block whose neighbours `UiDispatcher` at `:36` and `UiSyncContext` at `:38` still have
production consumers; both pins passing here confirms the deletion took exactly one line from that
block and not the block.

The `WhenNotInvokeRequired` pin passing confirms the guard was implemented as a branch and not as an
unconditional marshal: had P6-T1 routed every call through `Invoke`, that pin's
`Verify(v => v.Invoke(…), Times.Never())` clause would now fail.

## TRX artifact

`evidence/regression-testing/p6-t5.trx`, sanitised with the same case-insensitive, XML-entity
substitution scheme used throughout this batch. After redaction the file parses as XML, its
`<UnitTestResult>` count is **6** — matching the `Ran: 6` recorded above — its `ResultSummary`
counters read `total=6 passed=6 failed=0`, and a case-insensitive search for the account name, the
short 8.3 account name and the machine name returns **0** residual occurrences.

Output Summary: All six #489 tests pass. `Total tests: 6 / Passed: 6`, 0 failed, 0 skipped,
`EXIT_CODE: 0`. The three REDs recorded failing at P5-T6 and P5-T7 are now green, and the three pins
recorded passing at P5-T5 still pass. No test source changed between the RED and GREEN runs, so the
transition is attributable to the three Phase 6 production edits alone. The `WhenNotInvokeRequired`
pin and the two interface pins together demonstrate the fix is a branch rather than an unconditional
marshal, and that the `UiScheduler` deletion did not over-reach into its two neighbouring members.
