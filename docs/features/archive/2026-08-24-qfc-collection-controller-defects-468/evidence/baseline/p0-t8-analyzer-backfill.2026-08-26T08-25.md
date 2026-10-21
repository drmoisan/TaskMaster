# [P0-T8] Analyzer package back-fill (version skew)

Timestamp: 2026-08-26T08-25

Command: `nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages`
Command: `nuget install Roslynator.Analyzers -Version 4.16.0 -OutputDirectory packages`

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

### The skew, measured

P0-T7's restore installs the versions pinned in `packages.config`, not the versions named by the
`<Analyzer Include>` items. Measured across the sixteen first-party `.csproj` files:

| Reference kind | Meziantou.Analyzer | Roslynator.Analyzers |
|---|---|---|
| `packages.config` `<package version=...>` (what restore installs) | `3.0.174` (16 files) | `4.16.1` (16 files) |
| `.csproj` `<Analyzer Include>` path (what the compiler loads) | `3.0.156` (16 hits) | `4.16.0` (64 hits, 4 DLLs x 16 files) |
| `.csproj` `<Import>` / `EnsureNuGetPackageBuildImports` `<Error>` path | `3.0.174` (32 hits, 2 per file) | n/a |

After P0-T7, `packages/` contained `Meziantou.Analyzer.3.0.174` and `Roslynator.Analyzers.4.16.1`
only. A missing `<Analyzer Include>` path is `error CS0006`, not a warning, so the compile would
have failed outright on all sixteen first-party projects.

Representative wiring, `QuickFiler/QuickFiler.csproj`:

```
  3:  <Import Project="..\packages\Meziantou.Analyzer.3.0.174\build\Meziantou.Analyzer.props" ... />
576:    <Error Condition="!Exists('..\packages\Meziantou.Analyzer.3.0.174\build\Meziantou.Analyzer.props')" ... />
582:    <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
583:    <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll" />
584:    <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll" />
585:    <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll" />
586:    <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll" />
```

Both versions are therefore genuinely required simultaneously: `3.0.174` for the props import and
the `EnsureNuGetPackageBuildImports` guard, `3.0.156` for the analyzer assembly the compiler loads.
This is a pre-existing repository condition, not something this feature introduced, and it is out of
scope to reconcile (AC-25 forbids scope creep).

### Back-fill performed

Both packages resolved from the local NuGet fallback folder (`<user-profile>\.nuget\packages\`); no
network fetch was required. Installer output, verbatim (host paths replaced with `<WS>`):

```
Retrieving package 'Meziantou.Analyzer 3.0.156' from '<user-profile>\.nuget\packages\'.
Successfully installed 'Meziantou.Analyzer 3.0.156' to <WS>\packages

Retrieving package 'Roslynator.Analyzers 4.16.0' from '<user-profile>\.nuget\packages\'.
Successfully installed 'Roslynator.Analyzers 4.16.0' to <WS>\packages
```

### Acceptance verification

`ls -d packages/Meziantou.Analyzer.3.0.156 packages/Roslynator.Analyzers.4.16.0`:

```
packages/Meziantou.Analyzer.3.0.156/
packages/Roslynator.Analyzers.4.16.0/
```

Both directories exist. The five analyzer DLLs the `.csproj` files actually reference are present at
the exact sub-paths named:

- `packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll`
- `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator.CSharp.Analyzers.dll`
- `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator_Analyzers_Roslynator.Common.dll`
- `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator_Analyzers_Roslynator.Core.dll`
- `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator_Analyzers_Roslynator.CSharp.dll`

Result: PASS. Both acceptance conditions are met. `packages/` is not tracked by git (see the
P0-T10 porcelain output), so this back-fill produces no working-tree change.
