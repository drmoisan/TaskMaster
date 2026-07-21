# Analyzer / Codestyle Build — Baseline (Issue #364)

- Timestamp: 2026-07-19T08-51
- Task: [P0-T3]
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0

## Environment Reconciliation (pre-existing branch-state drift, not a feature edit)

The first baseline attempt failed with `CS0006: Metadata file could not be found` for three analyzer DLLs. Root cause is a pre-existing drift on the epic-integration base branch: `UtilitiesCS/UtilitiesCS.csproj` (and `VBFunctions.csproj`) reference analyzer versions `Meziantou.Analyzer.3.0.101`, `SonarAnalyzer.CSharp.10.27.0.140913`, and `Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4` in their `<Analyzer Include>` items, while `packages.config` pins the newer `3.0.123`, `10.29.0.143774`, and `5.6.0`. The csproj-to-packages reconciliation exists on `main` (commit `097f0ba2` "Bump the analyzers-dev-deps group") but is not present in this epic-integration base. `Sync-PackageReferences.ps1` reconciles only `<HintPath>` reference-assembly items, not `<Analyzer Include>` items, so it does not close this gap.

Resolution (zero tracked-file edits): the three csproj-referenced analyzer versions were installed into the gitignored `packages/` folder via `nuget.exe install`, so the committed csproj's analyzer paths resolve. No `.csproj`, `.sln`, or `packages.config` file was modified. This is environment setup only; it is unrelated to issue #364 and reflects the branch's own declared analyzer configuration.

## Output Summary

- Result: PASS (Build succeeded).
- Errors: 0.
- Warnings: 75 (build-summary count). Warnings are NOT promoted to errors in this analyzer/codestyle gate (no `TreatWarningsAsErrors`).
- Top warning IDs (repo-wide, pre-existing): CS8632 (66, nullable annotation outside a `#nullable` context — largely in test projects), CS0618 (56, obsolete API), CS0108 (8), CS0169 (6), CS0067 (6), MSTEST0032 (2), CS4014 (2), CS2002 (2), CS0168 (2).
- These warnings are the pre-change baseline for the P9-T2 no-new-diagnostics comparison.
