# Consolidated msbuild Command — Empirical Verification (Issue #267)

- Timestamp: 2026-07-07T21-25
- Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- Execution note: Invoked in git-bash as `msbuild.exe TaskMaster.sln -t:Build -m -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true -p:Nullable=enable -p:TreatWarningsAsErrors=true` (dash switches; same properties/flags as the plan's stated command).

## Result 1 — Literal same-session run (stale incremental state)

- EXIT_CODE: 0
- Output Summary: Build succeeded in 1.70s, 0 Warning(s), 0 Error(s). This run immediately followed the Phase 0 baseline msbuild passes in the same session; 68 `CoreCompile` targets were reported "Skipping ... because all output files are up-to-date with respect to the input files." This result reflects an MSBuild incremental up-to-date short-circuit (legacy/non-SDK `.csproj` projects key their `CoreCompile` up-to-date check on file timestamps, not on the command-line property set), not a genuine exercise of the merged property set's compiler behavior.

## Result 2 — Genuine fresh-build run (empirical verification, per plan's explicit "must be verified empirically, not assumed" instruction)

To confirm Result 1 was not a false-positive pass, the solution was cleaned (`msbuild TaskMaster.sln -t:Clean -p:Configuration=Debug -p:Platform="Any CPU"`) and the exact consolidated command was re-run against the fully clean state (no prior `obj`/`bin`), reproducing the conditions of a fresh CI checkout for this single build step.

- EXIT_CODE: 1
- Output Summary: **Build FAILED** in 0.35s (compilation aborted quickly due to failing `CoreCompile`). 0 Warning(s), **84 Error(s)** — 34 in `SVGControl\SVGControl.csproj`, 50 in `UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj`. All 84 errors are base-compiler nullable-flow diagnostics promoted to errors by `/p:TreatWarningsAsErrors=true` under `/p:Nullable=enable`: `CS8625` (null literal to non-nullable type), `CS8618` (non-nullable field/event not initialized in constructor), `CS8602`/`CS8603`/`CS8604` (possible null dereference/return/argument), `CS8600`/`CS8601` (possible null conversion/assignment), `CS8619` (nullable-annotation mismatch in generic type). Reproduced twice with identical 84-error result. Full logs retained in the session scratchpad during investigation (not committed; not required by this artifact's schema).

## Root-Cause Analysis — Why Result 1 and Result 2 disagree, and why this is not a local-environment artifact

A controlled comparison was run to determine whether the **original, unmodified two-pass workflow** (the pre-edit baseline) would also fail under the same genuinely-fresh conditions:

1. Solution cleaned.
2. Original pass 1 (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, no `Nullable`) run fresh: **Build succeeded, 72 Warning(s), 0 Error(s), 11.45s** — a genuine full compile (matches a fresh CI checkout for this step).
3. Original pass 2 (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`, no analyzer properties) run immediately after, with no intervening clean (matching how the two steps actually run back-to-back in CI): **Build succeeded, 0 Warning(s), 0 Error(s), 1.29s**, with 18 `CoreCompile` targets reported "Skipping ... up-to-date."

This demonstrates the underlying mechanism: because `SVGControl` and `UtilitiesSwordfish` are legacy (non-SDK) `.csproj` projects, MSBuild's `CoreCompile` up-to-date check is based on **source-file timestamps versus output-file timestamps only** — it does not factor in the command-line property set (`Nullable`, `TreatWarningsAsErrors`). Pass 1 compiles these projects successfully (without nullable checking, since `Nullable` is not set on that pass). Pass 2, run immediately after with no source changes, finds the same outputs already up-to-date and **skips `CoreCompile` entirely for these projects**, so the nullable-flow analysis governed by `/p:Nullable=enable` is **never actually applied** to `SVGControl` or `UtilitiesSwordfish` by the current, unmodified two-pass workflow. The second build step's `TreatWarningsAsErrors` enforcement is effectively inert for these two vendored projects on every CI run today.

The consolidated single-pass step (AC3/AC4) removes this masking: because there is exactly one `/t:Build` invocation, running from a fresh CI checkout, it is the **only** compile pass, and it sets `/p:Nullable=enable /p:TreatWarningsAsErrors=true` from the start. `SVGControl` and `UtilitiesSwordfish` are therefore compiled with nullable-flow analysis genuinely active for the first time, surfacing 84 pre-existing nullable-annotation defects in that vendored code as build-breaking errors.

## Conclusion

The consolidated command does **not** reliably exit 0 under genuine (non-incremental) conditions equivalent to a fresh CI checkout. This is a real, reproducible build failure caused by the consolidation itself removing an incremental-build masking effect that the current two-pass workflow depends on (likely unintentionally) to avoid ever nullable-checking `SVGControl` and `UtilitiesSwordfish`. Per the explicit verification constraint, no enforced property was weakened and no diagnostic was suppressed to force a pass. This is reported as a genuine, blocking finding for AC3/AC4 rather than a passing result.
