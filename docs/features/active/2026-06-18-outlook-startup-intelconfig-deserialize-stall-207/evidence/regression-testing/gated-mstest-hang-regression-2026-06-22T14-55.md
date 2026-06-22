# Gated MSTest Run — Regression: Existing Unit Test Hangs on DispatcherDelay (Issue #207)

Timestamp: 2026-06-22T14-55

Command:
- Built-in collector attempt: `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage /Settings:TaskMaster.runsettings /TestCaseFilter:"TestCategory!=LiveOutlook"`
- dotnet-coverage attempt: `dotnet-coverage collect --output <postchange cobertura> --output-format cobertura -- vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`

EXIT_CODE: (run never terminates; manually killed after the test host stalls indefinitely)

Output Summary:
- The gated MSTest run STALLS indefinitely and never produces a final pass/fail summary or a `.coverage` artifact. Test processes (vstest.console / testhost / datacollector) remain alive with near-flat CPU; no new test output after a fixed point.
- The stall reproduces deterministically across two independent collection methods (built-in `/EnableCodeCoverage` and external `dotnet-coverage collect`), so it is not a coverage-collector artifact — it is a hanging test.
- The new pure-seam tests PASS before the stall: all `HookReadinessCoordinatorTests` methods (full scripted not-ready×N → transient COMException → ready → exactly-once timeline; never-give-up; run-once-after-complete; Unhook interaction; non-transient propagation; null-gate / null-hookup constructor guards) are observed `Passed` in the live log.
- Last test logged before the stall: `AppEventsTests.LoadAsync_EmitsStartupInboxTimingEnvelopeBeforeDeferredProcessingWindow`. The hanging method is in the same class (`TaskMaster.Test/AppGlobals/AppEventsTests.cs`), which runs class-level-parallel per `TaskMaster.runsettings`.

Root cause (definitive):
- P3-T2 replaced the pre-existing `await Task.Delay(100)` in `AppEvents.ProcessNewInboxItemsAsync` (unprocessed-queue error-retry `else` branch, current `AppEvents.cs` line 460) with `await DispatcherDelay.WaitAsync(TimeSpan.FromMilliseconds(100))`.
- `DispatcherDelay.WaitAsync` (`TaskMaster/AppGlobals/DispatcherDelay.cs`) completes a `TaskCompletionSource` only on the first `Tick` of a one-shot `System.Windows.Threading.DispatcherTimer`. A `DispatcherTimer` only fires when a `Dispatcher` message loop is pumping on the current thread.
- The MSTest test host runs `AppEventsTests` on a thread with NO running `Dispatcher` (no STA pump; `[TestInitialize]` sets up no Dispatcher). The timer never ticks, so the returned `Task` never completes and the awaiting test hangs forever, stalling the whole assembly.
- Two existing in-scope tests drive this exact `else` (errors < 3) branch:
  - `ProcessNewInboxItemsAsync_BatchesMailboxProcessingAfterInteractiveCheckpoint` (2 unprocessed items, `CreateGlobalsWithNoEngines` → `ProcessMailItemAsync` returns false → error-retry branch → `DispatcherDelay.WaitAsync` → hang).
  - `ProcessNewInboxItemsAsync_WhenEngineProcessesStartupBatch_LogsSuccessAndDeferredContinuation` (drives the same retry path).

Why this is a regression introduced by the fix (not pre-existing):
- The Phase-0 baseline (`evidence/baseline/mstest-coverage-2026-06-21T16-30.md`) ran the IDENTICAL gated command on the pre-implementation tree and recorded 111 passed / 0 failed, exit 0. At baseline the retry branch still used `Task.Delay(100)`, which completes on the thread pool with no Dispatcher dependency. The DispatcherDelay substitution is what introduced the hang.

Toolchain steps 1–3 (this QC pass, current tree) — all green:
- CSharpier `check .`: "Checked 1087 files in 3190ms.", exit 0 (0 reformatted; idempotent).
- Analyzer build (`MSBuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true -m`): Build succeeded, 0 Warning(s), 0 Error(s).
- Nullable build (`MSBuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true -m`): Build succeeded, 0 Warning(s), 0 Error(s).

Verdict: BLOCKED. The gated MSTest run (toolchain step 4) cannot complete; an existing in-scope unit test hangs. Per the corrective-fix directive and `.claude/rules/csharp.md` ("Weakening assertions or relaxing test expectations to make tests pass" and "Adding sleeps, retries, or timing hacks to mask flaky behavior" are prohibited), the QC loop is STOPPED and reported rather than worked around. No local commit is made. The remediation (make `DispatcherDelay.WaitAsync` deterministic without a live Dispatcher pump, or otherwise restore unit-testability of the retry branch) requires a plan revision / engineering change and is outside the mechanical scope of this final-QC task.

Processes terminated after diagnosis: vstest.console.exe, testhost.exe, datacollector.exe, dotnet.exe (forced stop; 0 remaining).
