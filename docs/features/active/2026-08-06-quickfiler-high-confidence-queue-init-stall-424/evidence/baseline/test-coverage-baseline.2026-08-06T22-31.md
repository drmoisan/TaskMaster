# [P0-T7] Test + Coverage Baseline — Baseline Evidence

- **Issue:** #424
- **Task:** [P0-T7]
- **Toolchain step:** 4 of 4 (test, coverage-enabled)

Timestamp: 2026-08-06T22-31

Command: `pwsh -NoProfile -Command "& ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput 'docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml'"`

EXIT_CODE: 1

Output Summary:

```
Test Run Failed.
Total tests: 6241
     Passed: 6240
     Failed: 1
 Total time: 34.8300 Seconds
Code coverage results: ...\evidence\baseline\coverage-baseline.cobertura.xml
```

The runner drives `vstest.console.exe <9 test-assembly-paths>` under `dotnet-coverage`, satisfying the coverage-enabled test requirement (the CLI equivalent of `/EnableCodeCoverage`).

## Test-assembly discovery (`\.claude\` check — Decisions Record item 9)

**9 assemblies discovered; `CLAUDE_PATH_COUNT = 0`.** No discovered assembly contains a `\.claude\` path segment, so no stale agent-worktree build entered this run and no exclusion action was required.

```
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
SVGControl.Test\bin\Debug\SVGControl.Test.dll
Tags.Test\bin\Debug\Tags.Test.dll
TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
TaskTree.Test\bin\Debug\TaskTree.Test.dll
TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

## Pre-existing test failure (1) — characterized, unrelated to #424

`UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`

- Failure: `Expected threadException to be <null> because the STA thread must not throw, but it threw: System.Threading.Tasks.TaskCanceledException: A task was canceled.` originating at `UtilitiesCS/Threading/ProgressTrackerAsync.cs:35`, asserted at `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:164`.
- **Classified as a pre-existing flake under coverage instrumentation, not a real defect.** Verified by re-running the same test in isolation without `dotnet-coverage`:

  Command: `vstest.console.exe "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker"`
  EXIT_CODE: 0
  Output: `Passed InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker [161 ms]` / `Test Run Successful. Total tests: 1  Passed: 1`

- Scope: `UtilitiesCS` STA/dispatcher timing. **Zero relationship to QuickFiler, the confidence gate, the datamodel, or issue #424.** It is a dispatcher-timeout sensitivity aggravated by instrumentation overhead. Recorded as a baseline condition; not fixed (out of scope per the execution directive).

## Coverage figures (Cobertura root `<coverage>` element)

| Metric | Baseline value |
|---|---|
| `line-rate` | **0.7019272859161799** (70.19%) |
| `branch-rate` | **0.5829763295685664** (58.30%) |
| `lines-covered` / `lines-valid` | 56124 / 79957 |
| `branches-covered` / `branches-valid` | 13472 / 23109 |

### Per-file baseline line coverage (deduplicated by line number)

Cobertura repeats each line under both `<method><lines>` and the class-level `<lines>`; figures below deduplicate by `(filename, line number)` so the denominator is the true source-line count.

| File | Lines covered / valid | Line rate |
|---|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 57 / 60 | **95.00%** |
| `QuickFiler/Controllers/QfcHomeController.cs` | 165 / 244 | **67.62%** |
| `QuickFiler/Controllers/QfcDatamodel.cs` | not present | `[ExcludeFromCodeCoverage]` (`QfcDatamodel.cs:25`) — outside the denominator, as expected |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | not present | same partial class, therefore also excluded |
| `QuickFiler/Controllers/QfcScanProgressBandMapper.cs` | not present | file does not exist yet (created in [P4-T2]) |

## Observation carried forward to [P6-T5] — pre-existing repo-wide shortfall

The whole-report repository line rate is **70.19%**, which is **already below the plan's >= 80% repository gate at baseline, before any change made by this plan.** This is a pre-existing condition of the measurement, driven by uninstrumented and vendored assemblies counted in the denominator (for example `SVGControl`, Swordfish collections) and by the COM/VSTO/WinForms classes that `CLAUDE.md` formally exempts from the 80% floor via the "testable denominator" carve-out.

Consequences recorded now so `[P6-T5]` is judged honestly:
- This plan cannot be held responsible for raising a pre-existing repo-wide shortfall.
- The gates this plan **can** and **must** satisfy are: **no regression** against these baseline numbers, **>= 90%** on the new module (`QfcScanProgressBandMapper.cs`) and the changed module (`QfcStreamingDequeueConfidenceGate.cs`, baseline 95.00%), and no coverage regression on changed lines.
- `[P6-T5]` will restate the baseline-vs-post-change repository figures numerically alongside this pre-existing-shortfall note, rather than reporting a spurious pass or a regression caused elsewhere.
