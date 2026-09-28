# P4-T16 — loader index mapping for a non-zero start

Timestamp: 2026-09-13T16-31

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `0 Warning(s)`.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t16

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t16\vstest-class-run.trx:
  `total=25 executed=25 passed=25 failed=0`
- New case: `LoadControllersViewersAsync_WithNonZeroStart_MapsIndexAndWidensDigits`. The loader
  member is private, so the test obtains it by reflection on the queue type, invokes it with a
  start value of 9 and a single-item list, awaits the returned value-task, and asserts the
  recording item-group factory received that single item at index 9 while the item-controller
  factory received a digits value of 2.
- The digits value is 2 rather than 1 because the loader computes the width from the start offset
  plus the item count, so a single item starting at 9 reaches the ten-item boundary. This is the
  behaviour the index mapping pins.
