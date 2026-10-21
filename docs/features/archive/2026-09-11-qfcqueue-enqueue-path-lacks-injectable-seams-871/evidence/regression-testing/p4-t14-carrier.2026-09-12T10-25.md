# P4-T14 — carried-handler resolution, both outcomes

Timestamp: 2026-09-13T16-29

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `0 Warning(s)`.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t14

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t14\vstest-class-run.trx:
  `total=22 executed=22 passed=22 failed=0`
- New cases: `EnqueueAsync_WithMatchingCarrier_PassesTheCarriedHandler`, which supplies a
  pre-scored carrier list holding one entry built for the enqueued mail item and asserts the
  captured carried-handler argument is that entry's handler by reference, and
  `EnqueueAsync_WithNoCarrierList_PassesANullCarriedHandler`, which supplies a null carrier list
  and asserts the captured argument is null.
- The carrier-found case matches by reference identity, which is the first of the two matching
  rules the resolver applies and the one the happy path uses.
