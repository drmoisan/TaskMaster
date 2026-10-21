# Phase 0 — analyzer package back-fill

Timestamp: 2026-08-27T23-20
Task: [P0-T7]
Command: `nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages -NonInteractive` and `nuget install Roslynator.Analyzers -Version 4.16.0 -OutputDirectory packages -NonInteractive`, both under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 0

## The skew this task closes

`QuickFiler.Test/QuickFiler.Test.csproj` names two analyzer versions by unconditional `Analyzer Include`
items that `[P0-T6]`'s restore does not produce:

| csproj line | HintPath version | Present after restore |
|---|---|---|
| `:483` | `Meziantou.Analyzer.3.0.156` | no — restore produced `3.0.174` |
| `:484` to `:487` | `Roslynator.Analyzers.4.16.0` (four DLLs) | no — restore produced `4.16.1` |

A missing `Analyzer Include` HintPath is compile error **CS0006**, not a warning, so a baseline build
without this back-fill would fail for an environmental reason unrelated to this feature.

## Route taken — `nuget install` (route 1)

`nuget` resolved on `PATH` from the WinGet package location. Both installs exited 0:

- `Meziantou.Analyzer 3.0.156` — installed to `packages`, EXIT_CODE 0
- `Roslynator.Analyzers 4.16.0` — installed to `packages`, EXIT_CODE 0

The second-checkout copy route was not needed and was not used. `ANALYZER_PACKAGES_ABSENT` is **not**
recorded: both packages are present.

## Acceptance probe

Both required paths exist:

- `packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`
- `packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll`

`packages/` is a restore output directory and is not tracked, so this back-fill produces no diff.

Output Summary: Both skewed analyzer packages back-filled through `nuget install` (exit 0 each); both
acceptance probe paths exist. The CS0006 hazard from the csproj/packages version skew is closed.
