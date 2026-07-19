# Pragma-Only Nullable Build Baseline

- Timestamp: 2026-07-19T08-51
- Task: [P0-T4]
- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`  (NO `/p:Nullable=enable`)
- EXIT_CODE: 1

## /p:Nullable=enable Confirmation

`/p:Nullable=enable` was NOT passed. The command is pragma-only, per the plan's Critical Toolchain Deviation. Enforcement relies solely on each file's own `#nullable enable` pragma; at baseline no `UtilitiesCS/Threading/` file carries a pragma.

## Output Summary

- Threading CS86xx count attributable to `UtilitiesCS/Threading/`: **0** (no Threading file carries a `#nullable enable` pragma yet; this documents the pre-opt-in state).
- Total CS86xx across the solution from this command: **0**.
- The non-zero EXIT_CODE is caused entirely by **2 pre-existing vendored `SVGControl` CS0649 errors** (`SvgImageSelector.cs(55,24)` `_relativeImagePath` and `SvgImageSelector.cs(56,24)` `_absoluteImagePath` are never assigned). CS0649 is a standard compiler warning promoted to an error by `/p:TreatWarningsAsErrors=true`. It is not a nullable diagnostic and is unrelated to Threading.
- Under `/m` parallel scheduling, the vendored `SVGControl` project (a leaf dependency) compiles early and fails fast (~0.5s), which cancels scheduling of the remaining projects. This means the literal command never reaches first-party `UtilitiesCS` compilation, so it cannot by itself surface Threading CS86xx.

## Supplementary Scoped Nullable Read (reliable Threading CS86xx signal)

Because the literal command aborts on the vendored project before `UtilitiesCS` compiles, a scoped read is used to obtain the authoritative per-file Threading CS86xx count. Method: force-recompile only `UtilitiesCS.csproj` under pragma-only TWAE (delete `UtilitiesCS/obj/Debug/UtilitiesCS.csproj.CoreCompileInputs.cache`, then `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`).

- Result: EXIT_CODE 1, but **0 CS86xx** (zero nullable diagnostics), confirming no Threading file emits CS86xx at baseline.
- The 15 errors surfaced are pre-existing first-party warnings promoted by TWAE and unrelated to nullable: CS0618 x14 (obsolete `IAsyncEnumerable` `SelectAwait`/`WhereAwait`/`ForEachAwaitAsync` overloads) and CS0168 x2 (unused local). These are constant noise from a standalone-project TWAE build; they are not emitted as errors by the solution analyzer build (P0-T3, warnings only). The per-batch verification tracks the CS86xx subset only (baseline 0), which is unaffected by this noise.

## Pre-Existing Condition Note (for maintainer)

The literal plan command (and the CI nullable gate `/t:Rebuild /m /p:Nullable=enable /p:TreatWarningsAsErrors=true` in `.github/workflows/ci.yml`) fails on vendored `SVGControl` CS0649 under TWAE. This vendored breach is pre-existing, is outside this feature's `Threading/`-only scope, and is not introduced or altered by this feature. Per-batch and final-gate verification therefore assert: (a) the literal command's failure set remains vendored-only (no new first-party/Threading errors), and (b) the scoped read shows zero Threading CS86xx.
