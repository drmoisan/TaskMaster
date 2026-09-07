# Bootstrap — Analyzer Version Back-Fill ([P0-T5])

Timestamp: 2026-08-27T19-58

Command:
```
ls -d packages/Meziantou.Analyzer.3.0.156 packages/Roslynator.Analyzers.4.16.0
ls -1 packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/
ls -1 packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/
grep -rl '<Analyzer Include' --include=*.csproj .
```

EXIT_CODE: 0

## Output Summary

Both back-filled package folders exist under `packages/`, so no `nuget install` was executed by this
task; the back-fill was already in place in this worktree before execution began. Nothing was
downloaded or installed.

- `packages/Meziantou.Analyzer.3.0.156/` — present.
- `packages/Roslynator.Analyzers.4.16.0/` — present.

### Resolved analyzer DLL paths (as named by the `<Analyzer Include>` items)

| Package | Resolved path (repository-relative) | Present |
| --- | --- | --- |
| Meziantou.Analyzer 3.0.156 | `packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll` | yes |
| Meziantou.Analyzer 3.0.156 | `packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.CodeFixers.dll` | yes |
| Roslynator.Analyzers 4.16.0 | `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator.CSharp.Analyzers.dll` | yes |
| Roslynator.Analyzers 4.16.0 | `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator_Analyzers_Roslynator.Common.dll` | yes |
| Roslynator.Analyzers 4.16.0 | `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator_Analyzers_Roslynator.Core.dll` | yes |
| Roslynator.Analyzers 4.16.0 | `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator_Analyzers_Roslynator.CSharp.dll` | yes |

### The version skew this back-fill compensates for

- Seventeen `.csproj` files in this solution carry `<Analyzer Include>` items, matching the plan's
  corrected count of seventeen.
- `QuickFiler.Test/QuickFiler.Test.csproj:480-484` names the two back-filled versions
  unconditionally (`..\packages\Meziantou.Analyzer.3.0.156\...` and
  `..\packages\Roslynator.Analyzers.4.16.0\...`).
- `QuickFiler.Test/packages.config` pins newer versions: `Meziantou.Analyzer` `3.0.174` (line 11
  block) and `Roslynator.Analyzers` `4.16.1` (line 140 block). The restore therefore produces
  `3.0.174` / `4.16.1` folders, not the `3.0.156` / `4.16.0` folders the `<Analyzer Include>` items
  name.
- A missing `<Analyzer>` path is compiler error CS0006, not a warning, so without this back-fill the
  compile fails outright. This is a pre-existing repository-wide latent defect. No `.csproj` was
  edited to work around it.
