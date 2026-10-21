# Phase 0 — NuGet restore

Timestamp: 2026-08-27T23-19
Task: [P0-T6]
Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1` from the worktree root
EXIT_CODE: 0

## Result

- `Build succeeded.` with `0 Warning(s)` and `0 Error(s)`; elapsed 00:00:02.51.
- `Installed: 172 package(s) to packages.config projects`.
- Required probe path exists: `packages\Meziantou.Analyzer.3.0.174\build\Meziantou.Analyzer.props`.

## Analyzer package folders present after restore

- `packages/Meziantou.Analyzer.3.0.174/`
- `packages/Roslynator.Analyzers.4.16.1/`

The two versions `[P0-T7]` back-fills — `Meziantou.Analyzer` 3.0.156 and `Roslynator.Analyzers` 4.16.0 —
are **not** present after restore, which is the version skew `[P0-T7]` exists to close.

Output Summary: Restore exited 0, installed 172 packages, and the required
Meziantou.Analyzer.3.0.174 props file exists. The two skewed analyzer versions named by [P0-T7] remain
absent and are handled by that task.
