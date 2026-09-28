# P4-T5 — headless construction and the production dispatcher default

Timestamp: 2026-09-13T16-02

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `2 Warning(s)`, unchanged from P4-T4: the two `CS0649` fields the later tasks assign.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t5

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t5\vstest-class-run.trx: `total=7 executed=7 passed=7 failed=0`
- New case: `Construction_InHeadlessHost_SucceedsAndDefaultsToProductionAdapter`. It constructs a
  queue through the real primary constructor in the headless test host, asserts no exception is
  thrown, and asserts the value the `UiIdleDispatcher` getter returns is of type
  `UiThreadIdleDispatcher`, the adapter declared in `QuickFiler/Controllers/QfcQueue.UiIdle.cs`.
- The second assertion is the positive reference tying a test to a type declared in that new
  production part, and it also demonstrates the lazy getter yields the production default rather
  than null.
