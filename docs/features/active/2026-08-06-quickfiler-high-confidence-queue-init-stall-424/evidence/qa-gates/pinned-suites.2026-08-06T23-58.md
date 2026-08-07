# [P5-T1] Pinned Suites — BLOCKED (fail-closed)

- **Issue:** #424
- **Task:** [P5-T1]
- **Outcome:** **REMEDIATION REQUIRED.** The task cannot be satisfied as written. `[P5-T1]` is left unchecked.

Timestamp: 2026-08-06T23-58

## Diff verification — passes

Command: `git status --porcelain` (filtered against the `[P0-T3]` baseline allowance for `.claude/agent-memory/` and the feature folder)
EXIT_CODE: 0

Output Summary: none of the five pinned test files appears in the changed-file list. All five are byte-unmodified:

```
unmodified: QfcHomeControllerIterationTests.cs
unmodified: QfcInitEmailQueueZeroBatchTests.cs
unmodified: QfcHighConfidencePreFilterTests.cs
unmodified: QfcHomeControllerIssue218Tests.cs
unmodified: QfcFormControllerTests.cs
```

## Suite run — 2 failures

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcHomeControllerIterationTests|FullyQualifiedName~QfcInitEmailQueueZeroBatchTests|FullyQualifiedName~QfcHighConfidencePreFilterTests|FullyQualifiedName~QfcHomeControllerIssue218Tests|FullyQualifiedName~QfcFormControllerTests"`

EXIT_CODE: 1

Output Summary:

```
Test Run Failed.
Total tests: 64
     Passed: 62
     Failed: 2
```

Per-suite breakdown:

| Pinned suite | Tests | Result |
|---|---|---|
| `QfcHomeControllerIterationTests` | 8 | **8 passed** |
| `QfcInitEmailQueueZeroBatchTests` | 3 | **3 passed** |
| `QfcHighConfidencePreFilterTests` | 10 | **10 passed** |
| `QfcHomeControllerIssue218Tests` | 2 | **2 FAILED** |
| `QfcFormControllerTests` | 41 | **41 passed** |

### Exact-argument iteration pin — passes

Command: `... /TestCaseFilter:"FullyQualifiedName~IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems"`
EXIT_CODE: 0
Output Summary: `Passed IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems [285 ms]` — the `DequeueNextItemGroupAsync(8, 2000)` pin at `QfcHomeControllerIterationTests.cs:268` holds, confirming the post-UI iteration call site is untouched.

## The two failures

```
QfcHomeControllerIssue218Tests.RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch
  Moq.MockException: the first displayed page must come from the dequeue-layer gate
  Expected invocation on the mock once, but was 0 times:
    m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())

QfcHomeControllerIssue218Tests.RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter
  Moq.MockException:
  Expected invocation on the mock once, but was 0 times:
    m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())
```

Sites: setups at `QfcHomeControllerIssue218Tests.cs:101` and `:192`; verifications at `:160` and `:226`.

## Root cause — a plan classification error, not a code defect

Both failing tests are `RunAsync` **high-confidence** tests. They call `_controller.RunAsync(progress)` and then verify that the **two-argument** `DequeueNextItemGroupAsync(int, int)` overload was invoked exactly once.

`[P4-T5]` deliberately moved that pre-UI call site to the new four-argument overload:

```csharp
listEmail = await _datamodel.DequeueNextItemGroupAsync(
    itemsPerIteration, 200,
    QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline,
    scanProgress.Report);
```

so the two-argument overload is now invoked **zero** times from `RunAsync`. The mock is loose, so this is a verification failure at run time, not a compile break — which is why `[P4-T4]`'s "grep for other implementers and strict mocks and update any compile breaks" did not surface it.

**`QfcHomeControllerIssue218Tests.cs` is not a dormant-path suite.** Both `spec.md` (Test Strategy table: "`QfcHighConfidencePreFilterTests.cs`, `QfcHomeControllerIssue218Tests.cs`, `QfcFormControllerTests.cs` (dormant #171/#169 paths) ... **Unchanged** (paths remain dormant)") and plan `[P5-T1]` classify it as dormant and require it to pass unmodified. That classification is incorrect: its two tests assert directly on the in-scope `RunAsync` high-confidence dequeue call that this plan was authorized to change. The other two files in that group are genuinely dormant and do pass unmodified (10/10 and 41/41).

## Why this is not resolved here

`[P5-T1]` requires `QfcHomeControllerIssue218Tests.cs` to be simultaneously (a) absent from `git diff --name-only` and (b) passing. After `[P4-T5]` those two conditions are mutually exclusive. No task in this plan authorizes editing the file, and the plan forbids improvised substitutes, so execution stops here fail-closed rather than silently editing a pinned file or weakening an assertion.

The production behavior is correct and is covered: `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue` in `QfcHomeControllerRunAsyncHighConfidenceTests.cs` verifies the new four-argument call with the exact expected arguments, and passes.

## Required plan delta

Add a task to Phase 4 (after `[P4-T6]`, renumbering `[P4-T7]`/`[P4-T8]` downstream) authorizing the minimal retarget:

> **[P4-T7]** Update `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs`: in `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch` and `RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter`, change the `Setup` (lines 101, 192) and `Verify` (lines 160, 226) of `DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())` to the four-argument overload `DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<System.Action<int, int, int>>())`. Change nothing else: the `preFilterInvoked` assertion, both `LoadItemsAsync` overload-discipline assertions, and the `Times.Once` counts are unmodified. Rationale: these two tests exercise the in-scope `RunAsync` high-confidence path that `[P4-T5]` retargeted; they are not dormant-path tests.
>   - Acceptance: both tests pass; `git diff` for this file shows only the four overload-shape hunks; the `#218` intent (no pre-filter, plain `MailItem` load path, first page from the dequeue-layer gate) is preserved.

Corresponding corrections:
- `[P5-T1]`: remove `QfcHomeControllerIssue218Tests.cs` from the byte-unmodified list; keep it in the must-pass list.
- `spec.md` Test Strategy table: reclassify `QfcHomeControllerIssue218Tests.cs` from "Unchanged (dormant)" to "Update — overload shape only".
- AC 12 in `spec.md` names "the dormant-path suites (`QfcHighConfidencePreFilterTests.cs`, `QfcHomeControllerIssue218Tests.cs`, `QfcFormControllerTests.cs`)" as remaining passing as-is; it needs the same reclassification, since AC 12 as written cannot be satisfied.

## Execution state at block

Phases 0-4 are complete (37 tasks checked). Phase 5 stops at `[P5-T1]`; `[P5-T2]`, `[P5-T3]`, and all of Phase 6 are not started. Toolchain at this point: format 0, analyzers 0, nullable 0; all in-scope suites pass (54/54 across the gate, datamodel, mapper, and home-controller suites).
