# Final Test + Coverage Gate (Issue #270) — REMEDIATION REQUIRED

Timestamp: 2026-07-07T22-29

Command: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /EnableCodeCoverage`

EXIT_CODE: 1

Output Summary:
- Total tests: 202. Passed: 201. Failed: 1.
- The two new issue #270 regression tests PASS (`HandleInboxItemAddAsync_...`, `HandleToDoItemChangeAsync_...`).
- One PRE-EXISTING test FAILS: `OlInboxItemsItemAdd_WhenProcessingThrows_RethrowsThroughSynchronizationContext` in `TaskMaster.Test/AppGlobals/AppEventsCoverageExpansionTests.cs:80`.

## Root cause of the single failure (expected consequence of the fix, not a defect)

The failing test asserts (line 99) `context.CapturedException.Should().BeSameAs(expected)` — i.e., it asserts that a fault raised inside `OlInboxItems_ItemAdd` is RETHROWN through the captured `SynchronizationContext`. That rethrow is exactly the process-termination mechanism issue #270 fixes. AC1/AC2 require the handler to contain and log the fault (no rethrow). After the Phase 2 fix the exception is contained, so `context.CapturedException` is null and the test correctly fails. The failure is deterministic (reproduced isolated).

## Scope conflict

`AppEventsCoverageExpansionTests.cs` is NOT in the authorized file set for this plan (production `AppEvents.ReadinessHookup.cs`; tests `AppEventsTests.cs` and new `AppEventsTests.Helpers.cs`). Per the executor protocol (post-preflight, no blocking; explicit "No other files" directive), this file was not modified. The final toolchain therefore cannot be reported as fully green. Outcome: REMEDIATION REQUIRED.

## Required remediation (precise, one test)

Update `OlInboxItemsItemAdd_WhenProcessingThrows_RethrowsThroughSynchronizationContext` (rename to reflect containment, e.g. `..._ContainsAndDoesNotRethrowThroughSynchronizationContext`) so the assertion becomes `context.CapturedException.Should().BeNull();` (fault contained, not posted to the SynchronizationContext), consistent with AC1/AC2. This is a single-file, single-test change requiring a plan revision or explicit scope authorization.

## Coverage (post-change, informational; from the same run)

- `TaskMaster` package: 64.07% line (baseline 63.64%).
- New core methods: `HandleInboxItemAddAsync` 100.00% line; `HandleToDoItemChangeAsync` 92.86% line (the single uncovered line is the production default-collaborator lambda — the COM path not driven by the unit test).
- File-level `AppEvents.ReadinessHookup.cs` (partial class `TaskMaster.AppEvents`): 65.52% line (baseline 66.67%); the small dip reflects newly added lines, not a regression on previously covered lines.
