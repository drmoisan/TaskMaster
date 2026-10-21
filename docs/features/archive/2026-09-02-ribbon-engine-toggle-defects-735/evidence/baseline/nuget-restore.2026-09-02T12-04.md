# Phase 0 — NuGet Package Restore (P0-T4)

Timestamp: 2026-09-03T01-17
Task: [P0-T4]
Command: `pwsh -NoProfile -File <worktree>/scripts/vscode/Invoke-Restore.ps1` (working directory set to the worktree root)
EXIT_CODE: 0

Rationale for this task: without a completed package restore, the analyzer and nullable baselines
fail with CS0006 (analyzer assembly not found) rather than measuring anything.

## Result

```
Using MSBuild: <program-files>\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
MSBuild version 18.9.1+a81b43525 for .NET Framework
...
Installed:
    172 package(s) to packages.config projects
1>Done Building Project "<worktree>\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.21
```

## Post-run state check

- `PACKAGES_DIR_EXISTS`: True — the repository-root `packages` directory exists after the run.
- `PACKAGES_CHILD_COUNT`: 172 package directories present under `packages`.

This is the state the two msbuild baselines (P0-T6 analyzer, P0-T7 nullable) require. The five
analyzer packages named by `.claude/rules/csharp.md` (Meziantou.Analyzer, SonarAnalyzer.CSharp,
Roslynator.Analyzers, AsyncFixer, Microsoft.CodeAnalysis.BannedApiAnalyzers) are among the restored
set, so the `<Analyzer Include>` HintPaths in the first-party projects resolve.

Output Summary: Restore succeeded with EXIT_CODE 0, 0 warnings and 0 errors. 172 packages were
installed to the packages.config projects and the repository-root `packages` directory exists after
the run.
