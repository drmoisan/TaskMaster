# Baseline — Analyzer Gate

Timestamp: 2026-08-22T09-24

Command:

```
pwsh -NoProfile -Command 'msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true'
```

Run from the worktree root
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad37a256a0fb60243`. `msbuild` was
invoked through its absolute resolved path
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
(resolved with `vswhere -latest -products * -find 'MSBuild\**\Bin\MSBuild.exe'`), and the full build
log was captured to `coverage\analyzer-baseline.log` (4,724 lines). The invocation went through
`pwsh -NoProfile` rather than the Bash tool because the Bash tool mangles MSBuild switches (`/m`
becomes `M:/`, producing MSB1008).

EXIT_CODE: 0

Output Summary:

| Measure | Value |
| --- | --- |
| Exit code | **0** |
| Warning count | **5** |
| Error count | **0** |
| Log lines matching `Skipping target "CoreCompile"` | **0** |
| Log lines matching `CoreCompile:` (target actually executed) | 34 |
| `Done Building Project` lines | 20 |
| Occurrences of `CS0006` | **0** |
| Log file | `coverage\analyzer-baseline.log` (4,724 lines) |
| Wall time | 00:00:21.11 |

## Acceptance conditions

1. **`EXIT_CODE: 0`** — met.
2. **`Skipping target "CoreCompile"` count is exactly 0** — met. This is the load-bearing proof that
   the analyzers actually ran rather than being skipped by MSBuild incrementality. It is corroborated
   positively by 34 `CoreCompile:` target executions in the same log; the plan's guidance not to
   assert a `csc.exe` count was followed, because that count is zero even on a real compile and would
   gate nothing.

## Warning inventory

The plan makes no prediction about the warning count, so the observed count is recorded as-is.

All **5** warnings are the same pre-existing diagnostic, emitted once per affected project by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`:

> warning : The project contains a packages.config file, which is not supported by System.Reactive
> v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the
> RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.)

The five emitting projects are:

- `QuickFiler.csproj`
- `TaskMaster.csproj`
- `ToDoModel.csproj`
- `UtilitiesCS.csproj`
- `UtilitiesCS.Test.csproj`

Filtering the log for any warning line **not** originating from System.Reactive returned zero
results, so there are no analyzer-rule warnings at baseline. This is a **pre-existing condition**
attributable to the System.Reactive 7.0 packages.config incompatibility, entirely unrelated to this
child, and it is recorded rather than repaired here. It does not break the gate because these are
warnings and this command does not pass `/p:TreatWarningsAsErrors=true`.

## Analyzer version skew — back-fill confirmed effective

The `CS0006` occurrence count in the log is **0**. Before the P0-T10 back-fill, the five skewed
`<Analyzer Include>` paths naming `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` did
not resolve, and csc would have reported `error CS0006` with a non-zero exit in all 16 first-party
projects. The zero `CS0006` count together with `0 Error(s)` and `EXIT_CODE: 0` confirms the
back-fill took effect. No return to P0-T10 was required, and **no project file was edited**.
