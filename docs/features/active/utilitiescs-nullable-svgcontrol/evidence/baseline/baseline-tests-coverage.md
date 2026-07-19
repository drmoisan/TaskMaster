# Baseline Test Run With Coverage

Timestamp: 2026-07-19T01-00

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-svgcontrol/evidence/baseline/baseline-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:

- Discovered 1 test assembly: `SVGControl.Test\bin\Debug\SVGControl.Test.dll`.
- Total tests: 37. Passed: 37. Failed: 0. Total time: 1.5659s.
- Coverage XML written to `docs/features/active/utilitiescs-nullable-svgcontrol/evidence/baseline/baseline-coverage.cobertura.xml` (post-processed for Koverage compatibility).
- Repository (SVGControl package) headline: line-rate `0.2651162790697674` (26.51%), branch-rate
  `0.3202054794520548` (32.02%), 870/3264 lines covered, 368/1140 branches covered.
- `RelativePath.cs` class-level (the one file in scope with a real automated baseline):
  line-rate `0.567529` (56.75%), branch-rate `0.543544` (54.35%).
- All other 11 hand-authored remediation-target files show `line-rate="0"` at the class level
  (confirmed by inspection of the Cobertura XML), consistent with the plan's documented 0%
  baseline for those files (they are exercised only incidentally, never invoked by
  `SVGControl.Test`'s `GetRelativePath_Test.cs`/`RelativePathCoverageTests.cs`).

Environment/tooling notes (both required to make this exact command executable; neither is a
change to any of the 12 remediation-target `.cs` files or to `SVGControl.csproj`):

1. `SVGControl.Test.csproj` is **not** a member of `TaskMaster.sln` (confirmed:
   `grep -n "SVGControl" TaskMaster.sln` returns only the `SVGControl` project entry, not
   `SVGControl.Test`). Consequently `scripts/vscode/Invoke-Restore.ps1` (which restores against
   `TaskMaster.sln`) never restores `SVGControl.Test`'s own `packages.config`-pinned package
   versions (`MSTest.TestAdapter 3.1.1`, `MSTest.TestFramework 3.1.1`, `FluentAssertions 6.12.0`,
   `Moq 4.20.69`, `Castle.Core 5.1.1`, `System.Runtime.CompilerServices.Unsafe 6.0.0`,
   `System.Threading.Tasks.Extensions 4.5.4`). A direct restore was required:
   `msbuild SVGControl.Test/SVGControl.Test.csproj /t:Restore /p:Configuration=Debug
   /p:Platform=AnyCPU /p:RestorePackagesConfig=true /p:SolutionDir=<repo-root>\` (the
   `/p:SolutionDir` override is required because `SVGControl.Test.csproj` has no owning
   `.sln` for NuGet's `packages.config` restore target to resolve). This is standard NuGet
   package restore, not a source-code or scope change.
2. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (line 139, before this feature's fix) assigned
   `$testAssemblies` from `Select-Object -ExpandProperty FullName` without forcing array
   semantics. Under this script's `Set-StrictMode -Version Latest`, when exactly one test
   assembly matches the filter (the case here, since only `SVGControl.Test.dll` currently
   builds in this fresh worktree — all other `*.Test.dll` projects depend on `UtilitiesCS.csproj`
   or `VBFunctions.csproj`, which fail to build due to a pre-existing, out-of-scope
   analyzer-package-version-pin mismatch; see `baseline-analyzers.md`), `$testAssemblies` is a
   scalar `[string]`, and `$testAssemblies.Count` throws
   `The property 'Count' cannot be found on this object.` This is a latent script defect,
   unrelated to nullable content, that would otherwise block every coverage-capture task in this
   plan (baseline, all 5 per-batch, and final). The minimal fix — wrapping the assignment in
   `@( ... )` to guarantee array semantics regardless of match count — was applied to
   `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (one line changed; no behavior change for the
   pre-existing multi-assembly case). This script is not one of the 12 `SVGControl/`
   remediation-target files and carries no `#nullable enable` pragma implications; it is
   PowerShell coverage tooling shared by the whole repository. This deviation is reported at
   plan completion per the atomic-executor escalation protocol.
