# Baseline — Nullable / type-check build

- Timestamp: 2026-09-02T01-05
- Issue: #678
- Task: [P0-T7]

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

EXIT_CODE: 0

This is character-for-character the command `.github/workflows/ci.yml` runs for its
"Build with nullable warnings treated as errors" step, except that `/t:Rebuild` replaces
CI's `/t:Build` because a warm local worktree can otherwise skip `CoreCompile` and exit 0
without compiling. `/p:Nullable=enable` is deliberately absent: no project carries a
`<Nullable>` element and there is no `Directory.Build.props`, so adding it would conscript
every file that never adopted the per-file `#nullable enable` pragma.

## `CS86` enumeration

`CS86` diagnostics reported: **0**. **No `CS86` diagnostic was reported.** The literal
`CS86` occurs zero times in the 11846-line build log.

## Non-vacuity control

`CoreCompile:` occurrences in the build log: **77**.

Greater than zero, so compilation ran and the nullable-flow analysis ran with it.

## MSBuild summary lines

```
    5 Warning(s)
    0 Error(s)
```

The five warnings are the same pre-existing System.Reactive `packages.config` migration
notice enumerated in `analyzer-build.md` (P0-T6): one each from `QuickFiler.csproj`,
`TaskMaster.csproj`, `ToDoModel.csproj`, `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj`.
They are emitted by an MSBuild target rather than by the C# compiler, so
`/p:TreatWarningsAsErrors=true` does not promote them and the build exits 0.

## Output Summary

EXIT_CODE 0. Zero `CS86` diagnostics reported. `CoreCompile:` occurred 77 times, so the
gate is demonstrably non-vacuous. `5 Warning(s)` / `0 Error(s)`, the five being the same
pre-existing System.Reactive notice recorded at P0-T6.
