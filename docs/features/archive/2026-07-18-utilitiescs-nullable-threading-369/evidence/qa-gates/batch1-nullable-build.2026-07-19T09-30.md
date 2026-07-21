# Batch 1 — Pragma-Only Nullable Build Verification

- Timestamp: 2026-07-19T09-30
- Task: [P1-T7]
- Literal plan command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
- Executed equivalent (genuine recompile of the changed project): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true /m`
- EXIT_CODE: 1 (pre-existing first-party TWAE noise only; zero CS86xx — see below)

## Opted-in Batch 1 files (5)

- UtilitiesCS/Threading/TaskPriority.cs
- UtilitiesCS/Threading/AsyncIdleQueue1.cs
- UtilitiesCS/Threading/ThreadSafeSingleShotGuard.cs
- UtilitiesCS/Threading/ThreadSafeFunctions.cs
- UtilitiesCS/Threading/ProgressMultiStepViewer.cs

## Output Summary

- **CS86xx for the 5 opted-in Batch 1 files: 0.** CS86xx count anywhere in the build: 0.
- The changed project `UtilitiesCS.csproj` was genuinely recompiled (the 5 Threading source files were edited + reformatted, so their mtimes invalidated the up-to-date check); the vendored `SVGControl` project stayed up-to-date and was skipped, so the pre-existing vendored CS0649 abort did not occur under this `/t:Build` form.
- The non-zero EXIT_CODE is due solely to pre-existing first-party warnings promoted by `/p:TreatWarningsAsErrors=true`: CS0618 x14 (obsolete `IAsyncEnumerable` `SelectAwait`/`WhereAwait`/`ForEachAwaitAsync` overloads) and CS0168 x2 (unused local), reported as 28+2 lines (inline). These are constant, exist at baseline (P0-T4 scoped read), and are unrelated to nullable annotation. No new diagnostics elsewhere.
- Result matches the P0-T4 baseline expectation: zero Threading CS86xx, no new first-party errors; the only failures are the documented pre-existing noise.

## Command-form note

The literal solution `/t:Rebuild ... /p:TreatWarningsAsErrors=true` aborts in ~0.5s on the pre-existing vendored `SVGControl` CS0649 (Clean+Build re-attempts the vendored project) before any first-party project compiles, so it cannot surface Threading CS86xx (established in the P0-T4 baseline). The `/t:Build` form above performs the same genuine recompile of the edited first-party project while leaving the up-to-date vendored project skipped, which is the reliable way to read Threading CS86xx. The literal solution `/t:Rebuild` command is exercised once at the final gate (P9-T3). `/p:Nullable=enable` was NOT passed.
