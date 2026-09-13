# P4-T8 — enqueue success path

Timestamp: 2026-09-13T16-22

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `2 Warning(s)`, unchanged.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t8b

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t8b\vstest-class-run.trx:
  `total=11 executed=11 passed=11 failed=0`
- New case: `EnqueueAsync_WithOnePage_QueuesTheTemplatePanelAndGroupsInInputOrder`. It substitutes
  the dispatcher fake, the background-template factory and the item-group factory, enqueues one
  page of three mail-item mocks, and asserts the queue count becomes 1, that the dequeued tuple
  carries the exact panel reference the substituted background-template factory returned, and that
  the item groups carry the mail items in input order.

Defect found and fixed while completing this task:

- The first class-scoped run of this task hung indefinitely. A repeat run scoped to the single new
  case, with `/Blame:CollectHangDump;TestTimeout=90000`, named
  `QfcQueueEnqueueTests.EnqueueAsync_WithOnePage_QueuesTheTemplatePanelAndGroupsInInputOrder` as the
  test running when the host was dumped, which established the hang was inside the new case rather
  than an interaction with an existing one.
- Cause: the harness constructs a sentinel `TableLayoutPanel` in a field initializer. Constructing
  any WinForms control installs a `WindowsFormsSynchronizationContext` on the current thread. The
  enqueue path's first genuinely asynchronous await — the `Task.Run` that runs the move-monitor hook
  loop — then captured that context and posted its continuation back to the test thread, which no
  unit-test host pumps, so the await never resumed. The three tasks before this one never reached a
  genuine await: the two guard cases throw before the first one, and the remaining cases are
  synchronous.
- Fix: a `[TestInitialize]` in the harness part detaches the synchronization context from the test
  thread. Field initializers run before it, so the context that the panel installed is removed
  before any test body runs, and every continuation completes on the thread pool. The fix involves
  no sleep, no wall-clock wait and no message pump.
- The recorded run above is the plain catalogue command with no blame collector, so the artifact
  records an unmodified CMD-VSTEST-CLASS invocation.
