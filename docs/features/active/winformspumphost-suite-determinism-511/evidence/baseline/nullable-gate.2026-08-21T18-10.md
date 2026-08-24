# Baseline — Nullable Gate

Timestamp: 2026-08-22T09-26

Command:

```
pwsh -NoProfile -Command 'msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true'
```

Run from the worktree root
`<repo-root>\.claude\worktrees\agent-ad37a256a0fb60243`. `msbuild` was
invoked through its absolute resolved path
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, and the full
log was captured to `coverage\nullable-baseline.log` (10,547 lines).

EXIT_CODE: 0

Output Summary:

| Measure | Value |
| --- | --- |
| Exit code | **0** |
| Error count | **0** |
| Warning count | 5 |
| Log lines matching `Skipping target "CoreCompile"` | **0** |
| Log lines matching `CoreCompile:` (target actually executed) | 53 |
| Log file | `coverage\nullable-baseline.log` (10,547 lines) |
| Wall time | 00:00:20.71 |

## Acceptance conditions

1. **`EXIT_CODE: 0`** — met.
2. **`Skipping target "CoreCompile"` count is exactly 0** — met, corroborated positively by 53
   `CoreCompile:` target executions. The compiler and nullable-flow diagnostics genuinely ran; the
   gate was not vacuous.

## Confirmation that no `/p:Nullable=enable` was passed

The exact argument vector handed to `MSBuild.exe` was captured and printed before the invocation:

```
ARGS: TaskMaster.sln | /t:Rebuild | /m | /p:Configuration=Debug | /p:Platform=Any CPU | /p:TreatWarningsAsErrors=true
CONTAINS_NULLABLE_ENABLE_SWITCH: False
```

The vector was additionally matched against the pattern `Nullable`, which returned `False`. **The
command carried no `/p:Nullable=enable`.** This is character-for-character the command in
`.github/workflows/ci.yml` (step "Build with nullable warnings treated as errors"). Adding the switch
is prohibited: no project in this repository carries a `<Nullable>` element and there is no
`Directory.Build.props`, so the property is a solution-wide opt-in that conscripts every file which
never adopted the `#nullable enable` pragma.

## Warning inventory

All **5** warnings are the same pre-existing System.Reactive 7.0 `packages.config` incompatibility
notice recorded in the P0-T13 analyzer-gate artifact, emitted once each by `QuickFiler.csproj`,
`TaskMaster.csproj`, `ToDoModel.csproj`, `UtilitiesCS.csproj`, and `UtilitiesCS.Test.csproj`.

Note on why they did not fail this gate despite `/p:TreatWarningsAsErrors=true`: the diagnostic is
emitted by an MSBuild target (`System.Reactive.PackagesConfigCheck.targets(31,5)`) and carries no
compiler diagnostic code. `TreatWarningsAsErrors` promotes **compiler** warnings, not warnings raised
by arbitrary MSBuild tasks, so the five remain warnings and the error count stays 0. This is a
pre-existing condition, unrelated to this child, recorded rather than repaired.

There were **zero** `CS86xx` nullable-flow diagnostics at baseline. Nullable enforcement in this
repository is per-file opt-in via the `#nullable enable` directive, and no opted-in file currently
carries a violation.
