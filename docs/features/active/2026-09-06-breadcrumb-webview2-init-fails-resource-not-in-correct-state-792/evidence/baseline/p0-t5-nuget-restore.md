# [P0-T5] NuGet package restore

- Issue: #792
- Timestamp: 2026-09-17T18-37
- Command: `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1` (msbuild `/t:Restore /p:RestorePackagesConfig=true`, run with the item worktree as the working directory)
- EXIT_CODE: 0
- Output Summary: Restore target succeeded; `Installed: 172 package(s) to packages.config projects`; `0 Warning(s)`, `0 Error(s)`; `Time Elapsed 00:00:02.47`.

## Acceptance observations

- MOQ-EXACT-FOLDER-PRESENT: true — `Test-Path packages/Moq.4.20.72` held, so the exact-folder form was used (the wildcard fallback was not needed).
- PROJECTS-SCANNED: 2 (`QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`)
- ANALYZER-PATHS-RESOLVED: 20 of 20 (final state, after the restore supplement below)
- PORCELAIN-SOURCE-LINES: 0 (`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'` printed nothing)

## Restore supplement (recorded, not hidden)

The first pass of the analyzer-path check after the restore reported `ANALYZER-PATHS-RESOLVED: 18 of 20`. The two unresolved items were the `<Analyzer Include="..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />` items at `QuickFiler/QuickFiler.csproj:598` and `QuickFiler.Test/QuickFiler.Test.csproj:525`; the other 18 (Roslynator 5.0.0, AsyncFixer 2.1.0, BannedApiAnalyzers 5.6.0, SonarAnalyzer.CSharp 10.34.0.3385, MSTest.Analyzers 4.4.0) resolved on the first pass, which is the positive control that the probe reached the intended files.

Cause, verified before acting:

- Every first-party `packages.config` and every csproj `<Import>`/`<Error Condition>` line names `Meziantou.Analyzer.3.0.235`, so the restore installed only `packages/Meziantou.Analyzer.3.0.235`; the hand-written `<Analyzer Include>` items in 15 of 16 first-party projects still name `3.0.203` (only `TaskMaster/TaskMaster.csproj:575` was updated to `3.0.235`).
- The skew is inherited: `git show origin/main:QuickFiler/QuickFiler.csproj` carries the identical `3.0.203` item under the `3.0.235` import, and `git diff --stat origin/main HEAD -- '*.csproj' 'packages.config'` is empty, so this branch never touched those files. It surfaces only on a cold worktree, where the superseded package folder is absent.

Action taken (environment only; no tracked file changed): `nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -DependencyVersion Ignore` (exit 0). `git check-ignore -v` reports the provisioned DLL is ignored by `.gitignore:191` (`**/[Pp]ackages/*`). The check was then re-run and reported `ANALYZER-PATHS-RESOLVED: 20 of 20`. The plan text labels a mismatch as an environment defect; the remedy repairs the environment and leaves the source tree byte-identical, which is why it is recorded here as a restore supplement rather than a halt. The durable fix (aligning the 15 stale `<Analyzer Include>` version strings with `packages.config`) is outside this item's write set and is reported to the maintainer in the Phase 0 summary as a pre-existing repository defect.
