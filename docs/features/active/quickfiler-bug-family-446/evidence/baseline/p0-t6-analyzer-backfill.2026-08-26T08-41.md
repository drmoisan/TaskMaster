# [P0-T6] Analyzer Package Back-Fill

Timestamp: 2026-08-26T08-41

Task: [P0-T6]
Feature: docs/features/active/quickfiler-bug-family-446

## Precondition Observed

After `[P0-T5]` restore, the version skew described by the task was confirmed:

- `packages/Meziantou.Analyzer.3.0.156` — absent
- `packages/Roslynator.Analyzers.4.16.0` — absent
- `packages/Meziantou.Analyzer.3.0.174` — present (restored from `packages.config`)
- `packages/Roslynator.Analyzers.4.16.1` — present (restored from `packages.config`)

All 16 first-party `.csproj` files carry unconditional `<Analyzer Include>` items naming the
`3.0.156` and `4.16.0` directories, and a missing `Analyzer` path is `error CS0006`, which fails
the compile rather than emitting a warning.

## Route Taken

`nuget.exe` was available on this machine, so the first of the two routes named by the task was
used. The parent-checkout copy route was not needed.

Command: `pwsh -NoProfile -Command 'Get-Command nuget -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source'`
EXIT_CODE: 0
Output Summary: `nuget.exe` resolved from the local WinGet package store.

Command: `pwsh -NoProfile -Command 'nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages; exit $LASTEXITCODE'`
EXIT_CODE: 0
Output Summary: "Successfully installed 'Meziantou.Analyzer 3.0.156'" into the worktree `packages`
folder, retrieved from the local NuGet global-packages feed.

Command: `pwsh -NoProfile -Command 'nuget install Roslynator.Analyzers -Version 4.16.0 -OutputDirectory packages; exit $LASTEXITCODE'`
EXIT_CODE: 0
Output Summary: "Successfully installed 'Roslynator.Analyzers 4.16.0'" into the worktree `packages`
folder, retrieved from the local NuGet global-packages feed.

## Post-Condition Verification

`packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/` contains analyzer DLLs under
`roslyn4.14/cs`, `roslyn4.8/cs`, `roslyn5.0/cs` and `roslyn5.6/cs`, including
`Meziantou.Analyzer.dll` and `Meziantou.Analyzer.CodeFixers.dll`.

`packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/` contains analyzer DLLs under
`roslyn3.8/cs` and `roslyn4.7/cs`, including `Roslynator.CSharp.Analyzers.dll`.

Both required directories exist and each contains at least one `.dll` under `analyzers/dotnet/`.

## Output Summary

Both skewed analyzer package versions back-filled via `nuget install` (exit 0 for each).
Acceptance condition satisfied: `packages/Meziantou.Analyzer.3.0.156` and
`packages/Roslynator.Analyzers.4.16.0` both exist and each carries analyzer DLLs under
`analyzers/dotnet/`.
