---
name: nullable-pragma-gate-net481-mechanics
description: How to actually run the per-file #nullable-enable pragma gate on UtilitiesCS (net481) and measure it, given pre-existing out-of-scope TWAE noise
metadata:
  type: project
---

The epic `utilitiescs-nullable-remediation` per-file pragma gate is
`msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:TreatWarningsAsErrors=true` WITHOUT
`/p:Nullable=enable`. On a cold worktree this literal command cannot reach EXIT_CODE 0, for reasons
entirely unrelated to nullable annotations. To make it runnable and measurable:

**Why:** three pre-existing, out-of-scope blockers surface under this command (all inherited from
`main`, all confirmed via `git show main:`):
1. Analyzer version drift: `UtilitiesCS/UtilitiesCS.csproj`'s hardcoded `<Analyzer Include>` paths
   (Meziantou 3.0.101, SonarAnalyzer 10.27.0.140913, BannedApiAnalyzers 3.3.4) are STALE vs
   `packages.config` (3.0.123 / 10.29.0.143774 / 5.6.0) — a dependabot bump (commit 7de9f11f) bumped
   packages.config but not the csproj paths → `CS0006 metadata file not found`.
2. Vendored `SVGControl` (a ProjectReference) has 2 pre-existing `CS0649` (unassigned field) that
   only error under TWAE. `/t:Rebuild` cascades Clean+Build to it, so it fails fast BEFORE UtilitiesCS
   compiles → the literal command yields NO cluster CS86xx signal at all.
3. UtilitiesCS itself has ~14 pre-existing non-nullable TWAE warnings-as-errors (CS0168, CS0618) in
   non-cluster files (BayesianClassifierGroup, EmailFiler, SortEmail, Triage, etc.) — all OUTSIDE
   ReusableTypeClasses.

**How to apply:**
- Bootstrap (no tracked-file edits): `nuget install` the exact drifted analyzer versions the csproj
  references into the gitignored `packages/`; the analyzers solution build (no TWAE) then passes.
- Standalone csproj build needs `/p:Platform=AnyCPU` (no space) — the csproj condition is
  `Debug|AnyCPU`; the `.sln` maps "Any CPU"→"AnyCPU" but a direct csproj build does not.
- Isolated cluster measurement: pre-build `SVGControl.csproj` clean (no TWAE), then
  `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`
  so UtilitiesCS actually compiles; then grep the log for diagnostics whose path is under
  `ReusableTypeClasses/`. Success = zero CS86xx attributed to the cluster (the plan's real acceptance
  criterion), NOT whole-build EXIT_CODE 0.
- Toolchain: repo-local `.dotnet-sdk` (Install-RepoDotNetSdk.ps1, pwsh7) + `Invoke-Restore.ps1` first;
  csharpier is pinned v1.2.6 needing `dotnet tool run csharpier check|format .` (the legacy `csharpier .`
  form errors "Required command was not provided"). See [[nullable-cs8714-not-on-net481]].
