# P5-T184 — Anti-masking closure for the Branch B UI-dispatch correction

Timestamp: 2026-07-22T16-22Z

Command: `cd "C:/Users/DanMoisan/repos/TaskMaster-wt/2026-07-21T10-25" && sha256sum QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs coverage.config scripts/vscode/TaskMaster.cli.runsettings && wc -l QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs && git status --porcelain -- QuickFiler QuickFiler.Test && grep -nE "Thread\.Sleep|Task\.Delay|SpinWait|Stopwatch|Timeout|WaitOne|DoNotParallelize|\[Ignore|TestCategory" QuickFiler/Viewers/BreadcrumbUiDispatcher.cs QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs`

EXIT_CODE: 0

Sources used: P5-T174 (before-side inventory) and P5-T181 through P5-T183 only.

## 1. The previously failing case now passes at all three levels

| Level | Evidence | Result |
|---|---|---|
| Uninstrumented, focused | P5-T181 (`p5-uidispatch-correction-uninstrumented-pass-after.2026-07-22T15-07.md`) | 9/9 passed, 0 failed, 0 skipped, exit 0; `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` listed **Passed** |
| Instrumented, focused, two consecutive runs | P5-T182 (`p5-uidispatch-correction-instrumented-pass-after.2026-07-22T15-07.md`) | run 1: 9/9 exit 0; run 2: 9/9 exit 0; the case passed in both (296 ms, 303 ms) |
| Instrumented, full 17-class composition | P5-T183 (`p5-numeric-coverage-composition.2026-07-22T16-22.md`) | 160/160 passed, 0 failed, 0 skipped, natural exit 0 |

## 2. Before/after assertion inventory — zero assertions removed, weakened, relaxed, or made conditional

`QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` was **not edited**. Its SHA-256 is byte-identical before and
after the correction:

| Side | SHA-256 | Lines |
|---|---|---:|
| Before (P5-T174) | `e4bd60150636a83ce977681249e03c63a2fc7ca96c32c5f8ef5bbb760926e62e` | 480 |
| After (this task) | `e4bd60150636a83ce977681249e03c63a2fc7ca96c32c5f8ef5bbb760926e62e` | 480 |

Because the file is bit-identical, all nine `[TestMethod]` names and all 33 assertion expressions inventoried by
P5-T174 are still present with unchanged meaning, and there is no changed line requiring explanation:

1. `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` — 5 assertions, including
   `context.PostCount.Should().BeGreaterThan(0, ...)` at line 55, `messenger.Posted.Should().BeEmpty(...)` at line 58,
   the trailing drain at line 59, `messenger.Posted.Should().NotBeEmpty()` at line 60,
   `messenger.PostContexts.Should().OnlyContain(...)` at line 61, and `provider.VerifyAll()` at line 62 — all intact.
2. `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext` — 6 assertions intact.
3. `DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink` — 5 assertions intact.
4. `DispatcherActionFailure_IsReportedExactlyOnce` — 1 assertion intact.
5. `DispatchValue_AmbientOwningContext_StillSchedulesBeforeControlAccess` — 5 assertions intact.
6. `DispatchValue_NestedSynchronousDispatch_ExecutesInlineWithoutAnotherPost` — 6 assertions intact.
7. `DispatchValue_SchedulingFailure_ReportsOnceAndFaultsReturnedTask` — 3 assertions intact.
8. `ProductionCaptureWithoutUiContext_FailsFast` — 2 assertions intact.
9. `InboundCurrentDispatchFailure_IsObservedWithoutEscapingEventBoundary` — 4 assertions intact.

Assertion count before 33, after 33. Test-method count before 9, after 9.

## 3. No prohibited construct was added to the touched files

The scan
`grep -nE "Thread\.Sleep|Task\.Delay|SpinWait|Stopwatch|Timeout|WaitOne|DoNotParallelize|\[Ignore|TestCategory"` over
both touched-file candidates (`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, the only edited file, and
`QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs`, unedited) returned **no match** (grep exit status 1). No
`Thread.Sleep`, `Task.Delay`, wall-clock wait, retry loop, timing threshold, `[DoNotParallelize]`, `[Ignore]`, or
category-based skip was added.

## 4. No filter narrowed, no coverage or test exclusion added

| Item | Before (P5-T174) | After | Identical |
|---|---|---|---|
| `coverage.config` SHA-256 | `b9cd80356c6bdbe03807a0b8cb106ae03d24efbdbb2515097fbf003099050943` | `b9cd80356c6bdbe03807a0b8cb106ae03d24efbdbb2515097fbf003099050943` | yes |
| `scripts/vscode/TaskMaster.cli.runsettings` SHA-256 | `98ef03a8d3b0ebb2ed7a765e3b5e1b58e774d20202df2f294c03a7260b9cef57` | `98ef03a8d3b0ebb2ed7a765e3b5e1b58e774d20202df2f294c03a7260b9cef57` | yes |
| P5-T171 / P5-T183 17-class `TestCaseFilter` string | 17 inclusive `FullyQualifiedName~` terms | same 17 terms, byte-identical | yes |
| P5-T181 focused filter | `FullyQualifiedName~BreadcrumbUiThreadDispatchTests` | unchanged, class-level, not narrowed | yes |

`git status --porcelain -- QuickFiler QuickFiler.Test` reports exactly one modified path,
`M QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`. No project file, runsettings, coverage config, threshold, exclusion,
or designer file changed.

## 5. Class inventory and case total preserved

P5-T183 recorded 17 classes and 160 cases with per-class counts identical to the P5-T171 inventory
(9, 4, 13, 8, 12, 10, 7, 4, 3, 13, 12, 5, 10, 18, 12, 10, 10). No case was added, removed, or skipped.

## 6. Branch B: the production change is present and the fix is not test-only

`BRANCH: B` was selected by P5-T175. The correction is entirely in production code:

- Edited production file: `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` (285 lines post-format, SHA-256
  `0764d49c8747276722853bf30fe32aca133cb19a3d634a9cda351217fd49017e`).
- Edited test files: **none**.
- Change: `IsCurrentBoundary()` no longer accepts bare owner-thread identity as boundary proof when a synchronization
  context was captured. It now returns true for a captured-context dispatcher only when the dispatcher is already
  executing a callback on the current thread or when `SynchronizationContext.Current` is reference-equal to the captured
  context; thread identity remains the boundary proof only for the context-less owner-thread test dispatcher created by
  `CreateForCurrentThreadTests()`. This removes the P5-T172 deciding branch (lines 259-262 of the pre-correction file)
  that allowed `Dispatch` to run inline at line 84 and return `Task.CompletedTask` at line 94 without reaching
  `_context.Post`.

There is therefore no contradiction: a production defect explained the failure, the production defect was fixed, and no
test was corrected.

Output Summary: Anti-masking closure verified with zero contradictions. The previously failing case passes
uninstrumented (9/9), instrumented twice (9/9 each), and inside the full instrumented 17-class composition (160/160,
exit 0). `BreadcrumbUiThreadDispatchTests.cs` is byte-identical before and after (SHA-256 `e4bd6015...26e62e`, 480
lines), so all 9 test methods and all 33 assertion expressions are unchanged and zero assertions were removed, weakened,
relaxed, or made conditional. No `Thread.Sleep`, `Task.Delay`, wall-clock wait, retry loop, timing threshold,
`[DoNotParallelize]`, `[Ignore]`, or category skip was added; no filter was narrowed; `coverage.config` and the
runsettings are hash-identical and the 17-class filter string is byte-identical. The 17-class / 160-case inventory and
per-class counts are preserved. Under `BRANCH: B` the production change is present in
`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` and no test file was edited, so the fix is demonstrably not test-only.
EXIT_CODE: 0.
