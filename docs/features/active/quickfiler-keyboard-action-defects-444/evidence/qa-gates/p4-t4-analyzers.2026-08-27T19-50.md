# [P4-T4] Linting / static-analysis step

Timestamp: 2026-08-27T19-50
Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: `5 Warning(s)` / `0 Error(s)`. Error count 0 equals `BaselineAnalyzerErrors`;
warning count 5 equals `BaselineAnalyzerWarnings`, so it is not greater than the baseline. All five
warnings are the same pre-existing `System.Reactive 7.0.0` `packages.config` diagnostic recorded at
`[P0-T17]`, emitted by a NuGet `.targets` file rather than by the compiler.

MSBuild resolved to `<program-files>\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`.
The command was run from `WS` under `pwsh -NoProfile`, not through a POSIX shell, so the bare `/m`
switch was not rewritten into a drive-style path.

## Counts

| Figure | Value |
| --- | --- |
| `EXIT_CODE` | 0 |
| Error count (summary line) | 0 |
| Warning count (summary line) | 5 |
| `BaselineAnalyzerErrors` (`[P0-T17]`) | 0 |
| `BaselineAnalyzerWarnings` (`[P0-T17]`) | 5 |
| Lines matching `: error ` in the log | 0 |

## Non-vacuity proof

`/t:Rebuild` was used, never `/t:Build`. A warm `/t:Build` returns exit 0 having skipped
`CoreCompile` on every project, because MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so the gate could not fail.

| Evidence | Value |
| --- | --- |
| Occurrences of `Skipping target "CoreCompile"` in the log | **0** |
| `CoreCompile` references in the log | 81 |
| Assembly output lines (` -> <path>.dll`) | 18 |
| Log lines captured | 12200 |

A zero count of `Skipping target "CoreCompile"` is the assertion of record. `csc.exe` invocation
counts are not used as the non-vacuity proof, because this MSBuild reports them as 0 even on a
genuine compile.

## Warning characterisation

All five occurrences are:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.
```

one per project consuming `System.Reactive 7.0.0` through `packages.config`: `UtilitiesCS`,
`ToDoModel`, `QuickFiler`, `TaskMaster`, `UtilitiesCS.Test`. None originates in a file this feature
touches, and the set is identical to the `[P0-T17]` baseline set.

## Acceptance

- `EXIT_CODE: 0` — met.
- Error count is `0` — met.
- Warning count is not greater than the recorded baseline — met (5 vs baseline 5, equal).
