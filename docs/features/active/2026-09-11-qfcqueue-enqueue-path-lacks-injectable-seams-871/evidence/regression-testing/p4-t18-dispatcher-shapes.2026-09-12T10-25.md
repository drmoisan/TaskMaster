# P4-T18 — all three dispatcher shapes through one enqueue call

Timestamp: 2026-09-13T16-33

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `0 Warning(s)`.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t18

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t18\vstest-class-run.trx:
  `total=27 executed=27 passed=27 failed=0`
- New case: `EnqueueAsync_WithDefaultItemGroupFactory_UsesEachDispatcherShapeOnce`. It leaves the
  item-group seam at its production default, substitutes the dispatcher fake plus the viewer
  factory and row placer that keep the flow headless, drives one enqueue call, and asserts the
  substituted dispatcher recorded exactly one invocation of each of the three shapes — the action
  shape from the row placement, the function shape from the background-template read, and the
  asynchronous-function shape from the loader — and that the queue count became 1, so the
  observable behaviour is unchanged.
- The background-template factory is also substituted. Its production default clones the template
  panel, and the template is null on a queue that no form controller has populated, so leaving it
  at its default would raise from a statement that sits before the enqueue member's try block.
  That is the counter-leak region this item is forbidden to exercise, so the seam is substituted
  rather than driven.
- Every call the tests make to a generic dispatcher member passes an explicit type argument. The
  interface declares both a function shape and an asynchronous-function shape, and an argument of
  the asynchronous shape is applicable to both, so an inferred call is ambiguous and does not
  compile.
