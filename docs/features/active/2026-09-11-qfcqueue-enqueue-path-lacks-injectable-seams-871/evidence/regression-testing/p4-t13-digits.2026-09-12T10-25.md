# P4-T13 — data-driven item-number digit width

Timestamp: 2026-09-13T16-28

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `0 Warning(s)`.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t13

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t13\vstest-class-run.trx:
  `total=20 executed=20 passed=20 failed=0`
- New case: `EnqueueAsync_WithItemTotal_PassesExpectedDigitsToEachController`, a `[DataTestMethod]`
  with three `[DataRow]` rows covering item totals of 9, 10 and 11 and asserting the digits value
  captured by the recording item-controller factory is 1, 2 and 2 respectively.

Data-row counting observation (the observation this task exists to record):

- ThisRunTotal: 20
- PrecedingTaskTotal: 17 (recorded by P4-T12)
- Difference: 3
- CountingRule: THREE RESULTS, NO AGGREGATE PARENT. The trx carries exactly three
  `UnitTestResult` elements whose test name begins with the method name, named
  `... (9,1)`, `... (10,2)` and `... (11,2)`, each with `outcome="Passed"` and none carrying an
  `InnerResults` element. The runner therefore counts each data row as one result and adds no
  aggregate parent row.
- Gate: the difference must be 3 or 4. Observed 3, so the gate is satisfied.
- All three data rows appear as passed results in the trx, as enumerated above.
- The same counting rule applies to the class-scoped run P4-T19 records and to the whole-assembly
  run P4-T24 records, which is what makes P4-T24's arithmetic identity hold. With this runner
  counting three rather than four, the class-scoped total P4-T19 records already includes all three
  data rows exactly once, and the whole-assembly run counts them the same way.
