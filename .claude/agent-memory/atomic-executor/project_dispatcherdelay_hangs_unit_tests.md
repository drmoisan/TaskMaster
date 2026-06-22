---
name: dispatcherdelay-hangs-unit-tests
description: DispatcherDelay.WaitAsync (one-shot DispatcherTimer) never completes in the MSTest host (no Dispatcher pump), so any unit test that reaches a call site awaiting it hangs the whole TaskMaster.Test assembly
metadata:
  type: project
---

`TaskMaster/AppGlobals/DispatcherDelay.cs` `WaitAsync(TimeSpan)` completes its `TaskCompletionSource` only on the first `Tick` of a one-shot `System.Windows.Threading.DispatcherTimer`. A `DispatcherTimer` only fires when a `Dispatcher` message loop is pumping on the current thread.

The vstest MSTest host runs tests on a thread with NO running Dispatcher (no STA pump; `AppEventsTests` `[TestInitialize]` sets up none). So `WaitAsync` never completes and any awaiting test hangs forever, stalling the entire assembly (no final summary, no `.coverage`).

**Why:** Issue #207 corrective fix (P3-T2) replaced the pre-existing `await Task.Delay(100)` in `AppEvents.ProcessNewInboxItemsAsync` (unprocessed-queue error-retry `else` branch) with `await DispatcherDelay.WaitAsync(...)`. `Task.Delay` completed fine on the thread pool; DispatcherDelay does not. Two existing in-scope tests drive that retry branch and now hang: `ProcessNewInboxItemsAsync_BatchesMailboxProcessingAfterInteractiveCheckpoint` and `ProcessNewInboxItemsAsync_WhenEngineProcessesStartupBatch_LogsSuccessAndDeferredContinuation` (both in `TaskMaster.Test/AppGlobals/AppEventsTests.cs`). The plan's "DispatcherDelay is unit-test-exempt by inspection" guardrail did not account for an existing test that drives the call site.

**How to apply:** When a fix routes a production code path through a Dispatcher-pump-dependent helper, check whether any EXISTING unit test exercises that path on a pump-less thread before assuming "exempt by inspection." If running the gated TaskMaster.Test suite stalls at `AppEventsTests` with flat-CPU vstest/testhost/datacollector and no summary, suspect DispatcherDelay. Remediation (deterministic completion without a live Dispatcher, e.g. a TimeProvider/timer seam, or fall back to a pump-less completion path when no Dispatcher is running) is an engineering/plan-revision change, not a QC micro-action — do NOT weaken the tests to pass.

The gated coverage run in this repo is best driven by `dotnet-coverage collect -- vstest.console.exe ...` (Windows-style path for the wrapped exe; `MSYS_NO_PATHCONV=1` so `/InIsolation` and `/TestCaseFilter` args are not path-mangled). The built-in `vstest /EnableCodeCoverage` finalization is very slow / appears to stall on this solution. See [[project_runsettings_datacollector_default_enabled]] and [[project_vstest_isolation_and_filepathhelper_serialization]].
