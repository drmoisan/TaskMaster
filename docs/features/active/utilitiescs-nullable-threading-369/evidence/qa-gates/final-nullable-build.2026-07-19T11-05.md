# Final QC — Pragma-Only Nullable / TreatWarningsAsErrors Type-Check Gate

- Timestamp: 2026-07-19T11-05
- Task: [P9-T3]
- Literal plan command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
- EXIT_CODE (literal command): 1

## /p:Nullable=enable Confirmation

`/p:Nullable=enable` was NOT passed. Enforcement is per-file pragma only, per the plan's Critical Toolchain Deviation.

## Output Summary

- **Zero CS86xx across all 25 opted-in `UtilitiesCS/Threading/` files.** Confirmed by two complementary runs:
  1. **Literal solution `/t:Rebuild ... /p:TreatWarningsAsErrors=true`**: EXIT 1, failure confined to the pre-existing vendored `SVGControl` CS0649 (unassigned fields promoted by TWAE) — identical to the P0-T4 baseline; zero CS86xx; the parallel Rebuild aborts on the vendored project before first-party compilation, so it cannot itself surface Threading diagnostics.
  2. **Scoped genuine recompile of `UtilitiesCS.csproj` under the same pragma-only TWAE** (invalidate `UtilitiesCS` CoreCompileInputs cache, solution `/t:Build`, vendored projects up-to-date/skipped): **0 CS86xx across all 25 opted-in Threading files** (grep of `Threading[/\\]*: (error|warning) CS86` = 0). The only errors are the pre-existing first-party TWAE noise CS0618 x14 (obsolete `IAsyncEnumerable` overloads) + CS0168 x2 (unused local), unchanged from baseline and unrelated to nullable.
- **25 files carry `#nullable enable`** (verified): ApplicationIdleTimer, AsyncIdleQueue1, AsyncMultiTasker, CurrentStoreContext, IdleActionQueue, IdleAsyncQueue, IProgressViewer, IUiDispatcher, LockupStallDecider, ProgressMultiStepViewer, ProgressPackage, ProgressPane, ProgressTracker, ProgressTrackerAsync, ProgressTrackerPane, ProgressViewer, StoreLockupResponder, SyncContextForm, TaskPriority, ThreadMonitor, ThreadSafeFunctions, ThreadSafeSingleShotGuard, TimeOutTask, UiThread, WpfUiDispatcher.
- **No `*.Designer.cs` file carries a pragma; no `*.Designer.cs` or `.resx` file was modified** (verified via grep + `git status`). No new diagnostics elsewhere. No files changed by this gate; no toolchain restart required.
