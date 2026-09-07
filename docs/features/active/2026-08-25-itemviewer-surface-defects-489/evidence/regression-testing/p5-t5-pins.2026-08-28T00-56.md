# P5-T5 — The three #489 pins pass before the fix

Timestamp: 2026-08-28T00-56
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p5-t5.trx" "/TestCaseFilter:FullyQualifiedName~HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly|FullyQualifiedName~IItemViewer_StillDeclaresUiDispatcher|FullyQualifiedName~IItemViewer_StillDeclaresUiSyncContext" /ResultsDirectory:<temp>
EXIT_CODE: 0
ExpectedExitCode: 0

Ran: 3
Passed: 3
Failed: 0
Skipped: 0

## Acceptance

`Test Run Successful. / Total tests: 3 / Passed: 3`.

```
Passed HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly [283 ms]
Passed IItemViewer_StillDeclaresUiDispatcher [2 ms]
Passed IItemViewer_StillDeclaresUiSyncContext [< 1 ms]
```

These three are pins, not REDs: they describe behaviour that is already correct and must survive
Phase 6 unchanged. `HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly` pins that the
already-on-the-UI-thread path still writes through directly, so the P6-T1 guard cannot be
implemented by marshalling unconditionally. `IItemViewer_StillDeclaresUiDispatcher` and
`IItemViewer_StillDeclaresUiSyncContext` guard against over-deletion when P6-T2 removes
`UiScheduler` from the same three-line block of `IItemViewer.cs`: `UiDispatcher` sits at `:36` and
`UiSyncContext` at `:38`, immediately either side of the `UiScheduler` declaration at `:37`, and both
still have production consumers.

## One correction was required to the P5-T1 arrange, recorded in full

On its first run `HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly` **failed**, and the
failure was in the test's own setup rather than in the production behaviour it pins:

```
System.ArgumentNullException: Value cannot be null. Parameter name: input
  at System.Text.RegularExpressions.Regex.Replace(String input, String replacement)
  at UtilitiesCS.MailItemHelper.GetHtml(String htmlBody)   MailItemHelper.Html.cs:206
  at UtilitiesCS.MailItemHelper.get_Html()                 MailItemHelper.Properties.cs:226
  at UtilitiesCS.MailItemHelper.ToggleDark(ToggleState)    MailItemHelper.Html.cs:177
  at QuickFiler.Controllers.QfcItemController.HtmlDarkConverter(ToggleState)
```

The arrange originally built the helper with `new MailItemHelper(mailItem.Object, globals.Object)`,
copying the pattern from the existing
`ApplyReadEmailFormat_MarksMailReadFalseAndRoutesThemeThroughInjectedDispatcherBeginInvoke` test in
`QfcItemController.SeamDispatcherTests.cs`. That overload calls `InitLazyFields`, which wires
`_html` to `new(() => GetHtml(HTMLBody), true)` at `MailItemHelper.cs:113`. `GetHtml` ignores its
parameter and reads `_item.HTMLBody` directly at `MailItemHelper.Html.cs:204`; a mocked COM
`MailItem` returns `null` for it, and `Regex.Replace(null, …)` throws.

The arrange now uses the parameterless `MailItemHelper()` constructor at `MailItemHelper.cs:80`,
which calls `InitializeSafeDefaults()` and seeds `_html` with `string.Empty.ToLazy()` and
`_attachmentsHelper`/`_attachmentsInfo` with empty arrays. `ToggleDark` then reduces to a pure string
transform over `string.Empty` and touches no COM member at all. The earlier failure was an incidental
`ArgumentNullException` in the fixture, not a signal about the marshalling guard, and correcting it
is what allows a failure in the two RED tests to be attributable to the missing guard alone.

No production file was changed to make this pin pass, and the two RED tests were not weakened: the
same corrected fixture is what P5-T6 uses to record the RED.

## TRX artifact

`evidence/regression-testing/p5-t5.trx`, sanitised with the same case-insensitive, XML-entity
substitution scheme used at P3-T3 and P4-T6. After redaction the file parses as XML, its
`<UnitTestResult>` count is **3** — matching the `Ran: 3` recorded above — its `ResultSummary`
counters read `total=3 passed=3 failed=0`, and a case-insensitive search for the account name, the
short 8.3 account name and the machine name returns **0** residual occurrences.

Output Summary: All three #489 pins pass before the Phase 6 fix. `Total tests: 3 / Passed: 3`,
0 failed, 0 skipped, `EXIT_CODE: 0`. One correction was needed in the P5-T1 test fixture, recorded
above in full: the MailItem-backed `MailItemHelper` constructor wires `Html` to a lazy `GetHtml()`
that dereferences a null `MailItem.HTMLBody` and throws `ArgumentNullException`, so the arrange was
switched to the parameterless constructor whose `InitializeSafeDefaults()` makes `ToggleDark` a pure
string transform. No production code was altered and no assertion was weakened. The sanitised TRX
parses and its `<UnitTestResult>` count of 3 matches the recorded totals.
