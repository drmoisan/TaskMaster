# P0-T5 — Guarded NuGet Restore and Analyzer Back-fill Check (Issue #680)

Timestamp: 2026-08-28T14-56

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1` (defaults: `TaskMaster.sln`,
`Debug`, `Any CPU`), followed by
`Test-Path 'packages\Meziantou.Analyzer.3.0.156\analyzers'` and
`Test-Path 'packages\Roslynator.Analyzers.4.16.0\analyzers'`

EXIT_CODE: 0

Output Summary:

- Restore: `MSBuild version 18.9.1+a81b43525 for .NET Framework`, solution configuration
  `Debug|Any CPU`, `Build succeeded. 0 Warning(s) 0 Error(s)`, elapsed 00:00:01.43. Exit code 0.
- Analyzer back-fill check (a missing `<Analyzer Include>` path is compile error CS0006, not a warning):
  - `packages\Meziantou.Analyzer.3.0.156\analyzers` -> `True`
  - `packages\Roslynator.Analyzers.4.16.0\analyzers` -> `True`
- No back-fill was required; neither `nuget install` nor a copy from the main checkout was performed.

Acceptance: satisfied — restore exited 0 and both `Test-Path` calls returned `True`.
