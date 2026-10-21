# P4-T6 — GREEN for #487 D1, both ParentChanged contract tests now pass

Timestamp: 2026-08-28T00-45
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p4-t6.trx" "/TestCaseFilter:FullyQualifiedName~ItemViewer_DeclaresNoParentChangedHandler|FullyQualifiedName~ItemViewerExpanded_DeclaresNoParentChangedHandler" /ResultsDirectory:<temp>
EXIT_CODE: 0
ExpectedExitCode: 0

Ran: 2
Passed: 2
Failed: 0
Skipped: 0

## Acceptance

`Test Run Successful. / Total tests: 2 / Passed: 2` in 1.20 seconds.

```
Passed ItemViewer_DeclaresNoParentChangedHandler [32 ms]
Passed ItemViewerExpanded_DeclaresNoParentChangedHandler [< 1 ms]
```

The same two tests, run under the same filter as P3-T3 where both failed, now pass. The transition
is caused solely by the Phase 4 deletions: `GetMember("L0v2h2_WebView2_ParentChanged", Flags)` now
returns an empty array on both `QuickFiler.ItemViewer` and `QuickFiler.ItemViewerExpanded` because
P4-T1 and P4-T3 removed the member declarations. No test was modified between the RED and this run.

RedRunReference: evidence/regression-testing/p3-t3-red-487-d1.2026-08-28T00-42.md
RedRunResult: 2 ran, 2 failed, 0 passed

## TRX artifact

`evidence/regression-testing/p4-t6.trx`, sanitised with the same case-insensitive, XML-entity
substitution scheme used at P3-T3. After redaction the file parses as XML, its `<UnitTestResult>`
count is **2** — matching the `Ran: 2` recorded above — its `ResultSummary` counters read
`total=2 passed=2 failed=0`, and a case-insensitive search for the account name, the short 8.3
account name and the machine name returns **0** residual occurrences.

Output Summary: Both #487 D1 regression tests pass. `Total tests: 2 / Passed: 2`, 0 failed,
0 skipped, `EXIT_CODE: 0`. The tests are byte-identical to the ones that failed at P3-T3; the only
change between the two runs is the deletion of the two `L0v2h2_WebView2_ParentChanged` members and
their two designer `+=` wirings, so the RED-to-GREEN transition is attributable to the fix alone.
The sanitised TRX parses and its `<UnitTestResult>` count of 2 matches the recorded totals.
