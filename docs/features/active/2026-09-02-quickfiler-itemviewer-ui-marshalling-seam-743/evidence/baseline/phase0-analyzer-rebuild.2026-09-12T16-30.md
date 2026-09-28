# Phase 0 — Analyzer baseline (P0-T6)

Task: [P0-T6]
Timestamp: 2026-09-13T02-18
Command: `pwsh -Command '& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true'` Run from the item worktree root via Set-Location inside one pwsh invocation, with the Command Reference tool resolution prepended; console output redirected to the ignored path `coverage\p0-t6-analyzer.log`. Run while holding the shared machine build lock for item 743.
EXIT_CODE: 0
Output Summary:
- `Build succeeded.`
- `    0 Warning(s)`
- `    0 Error(s)`
- `Time Elapsed 00:00:20.48`
- All nineteen projects in the solution reached `Done Building Project ... (Rebuild target(s))`, including `UtilitiesCS`, `QuickFiler` and `QuickFiler.Test`; `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` exists after the run.
- No line of the form `error XXnnnn` or `warning XXnnnn` appears anywhere in the log.

## Bootstrap observation recorded before this run (environment provisioning, zero tracked files changed)

A first attempt at this exact command, at 2026-09-13T02-13, exited 1 in 1.42 s with `0 Warning(s)` / `2 Error(s)`:

- `CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [VBFunctions\VBFunctions.csproj]`
- `CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [UtilitiesCS\UtilitiesCS.csproj]`

Cause (pre-existing, not introduced by this branch): the `<Analyzer Include>` items in `UtilitiesCS\UtilitiesCS.csproj` (line 1308) and `VBFunctions\VBFunctions.csproj` (line 58) name `Meziantou.Analyzer.3.0.203`, while `packages.config` in both projects, and the `<Import>`/`<Error Condition>` items at their lines 3 and 1300 / 3 and 73, name `Meziantou.Analyzer.3.0.235`. The P0-T4 packages.config restore installs only `3.0.235`, so the HintPath-named `3.0.203` folder was absent in this cold worktree. `git diff --name-only origin/main...HEAD -- "*.csproj" "*/packages.config"` printed nothing, so this branch did not touch any project file.

Remedy applied, which changes no tracked file: `nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -DependencyVersion Ignore` (exit 0, `Successfully installed 'Meziantou.Analyzer 3.0.203'`). `packages/` is gitignored restore output. A check over every `<Analyzer Include>` path in every `.csproj` in the tree then found no unresolvable path. The figures above are from the re-run after that provisioning. The csproj-side HintPath skew itself is an out-of-scope defect and is reported to the caller rather than edited here.
