---
name: 364-nullable-gate-preexisting-blockers
description: The full-solution pragma-only nullable gate fails at baseline on the epic-integration branch (vendored SVGControl CS0649 + non-HelperClasses UtilitiesCS CS0618/CS0168); verify CS86xx via isolated UtilitiesCS build. Plus analyzer-version drift and coverage-script single-assembly bug.
metadata:
  type: project
---

Executing the `utilitiescs-nullable-remediation` epic children (e.g. #364 HelperClasses) on branches off `epic/utilitiescs-nullable-remediation-integration`.

**Why:** The recent HEAD commit changed the CI nullable gate from `/t:Build` (a silent no-op) to `/t:Rebuild`, so a genuine recompile now surfaces pre-existing warnings that `/p:TreatWarningsAsErrors=true` promotes to errors — across projects that are OUT of a given child's scope.

**How to apply:**

1. The plan-literal gate `msbuild TaskMaster.sln /t:Rebuild /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`) FAILS at baseline, before any edit: vendored `SVGControl/SvgImageSelector.cs` has 2 pre-existing CS0649 (fields never assigned, dating to 2023) that halt the solution build early; and non-HelperClasses `UtilitiesCS` files (EmailIntelligence/, Extensions/) have ~28 CS0618 + 2 CS0168. None are fixable within a HelperClasses-scoped child. Flag them; do not treat as regressions.
2. Authoritative CS86xx verification for a UtilitiesCS child: first build the solution once WITHOUT TWAE so SVGControl.dll exists, then `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false` (no TWAE) and grep the output for `warning CS86`. CS86xx are severity-independent and arise only from `#nullable enable` files (project default is oblivious — `UtilitiesCS.csproj` has no `<Nullable>`), so zero CS86xx warnings == zero CS86xx errors under TWAE. `BuildProjectReferences=false` avoids re-triggering the SVGControl halt (UtilitiesCS has a project ref to SVGControl).
3. Analyzer version drift: `UtilitiesCS.csproj`/`VBFunctions.csproj` `<Analyzer Include>` paths pin OLDER analyzer versions (Meziantou 3.0.101, SonarAnalyzer 10.27.0.140913, BannedApiAnalyzers 3.3.4) than `packages.config` (3.0.123 / 10.29.0.143774 / 5.6.0). `main` has the reconciling commit (097f0ba2) but the epic-integration base does not. `Sync-PackageReferences.ps1` only fixes `<HintPath>`, NOT `<Analyzer Include>`, so the analyzer build gets CS0006. Fix without touching tracked files: `nuget.exe install <id> -Version <old> -OutputDirectory packages` for the three csproj-referenced versions (packages/ is gitignored).
4. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws under StrictMode when discovery returns a SINGLE test assembly (`$testAssemblies.Count` on a scalar). To scope coverage to one assembly (e.g. UtilitiesCS.Test), invoke the underlying command directly: `dotnet-coverage collect --output <cobertura> --output-format cobertura --settings coverage.config -- <vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`.
5. Env bootstrap on a fresh worktree: `Invoke-Restore.ps1` (nuget restore, 169 pkgs) then the analyzer install in (3). Global `csharpier` is v1.3.0 (subcommand syntax `csharpier check .` / `csharpier format .`, NOT `--check`); `dotnet tool run csharpier` fails because the repo-local .NET SDK is not installed.
