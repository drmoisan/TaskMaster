# P4-T6 — default viewer factory identity, asserted without invoking the delegate

Timestamp: 2026-09-13T16-03

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `2 Warning(s)`, unchanged.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t6

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t6\vstest-class-run.trx: `total=8 executed=8 passed=8 failed=0`
- New case: `ItemViewerFactory_Default_IsTheViewerQueueDequeueMethodGroup`. It reads the `Method`
  property of the default delegate and asserts the method name is `Dequeue` and the declaring type
  is `ItemViewerQueue`.
- The delegate is inspected only. It is never invoked, because invoking it would read the
  process-wide dispatcher through the viewer queue core, which no headless test host provides.
