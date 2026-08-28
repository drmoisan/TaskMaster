# [P4-T5] Type-checking step

Timestamp: 2026-08-27T19-51
Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: `5 Warning(s)` / `0 Error(s)`. Error count 0 is not greater than
`BaselineNullableErrors = 0`. The recorded `Command:` string above contains zero occurrences of
`Nullable=enable`.

The command is recorded verbatim as executed. `/p:Nullable=enable` is **not** present, deliberately:
that property is a solution-wide opt-in that conscripts every file which has never adopted the
`#nullable enable` pragma, and `.github/workflows/ci.yml` omits it. Nullable enforcement in this
repository is per-file `#nullable enable` promoted to errors by `/p:TreatWarningsAsErrors=true`.

`/t:Rebuild` is used rather than `/t:Build`, because MSBuild's up-to-date check does not invalidate
on a command-line `/p:` change and a warm `/t:Build` would return exit 0 with `CoreCompile` skipped
on every project.

## Counts

| Figure | Value |
| --- | --- |
| `EXIT_CODE` | 0 |
| Error count (summary line) | 0 |
| Warning count (summary line) | 5 |
| `BaselineNullableErrors` (`[P0-T18]`) | 0 |
| Lines matching `: error ` in the log | 0 |
| Occurrences of `Nullable=enable` in the recorded `Command:` | 0 |

## Non-vacuity proof

| Evidence | Value |
| --- | --- |
| Occurrences of `Skipping target "CoreCompile"` in the log | **0** |
| `CoreCompile` references in the log | 85 |
| Assembly output lines (` -> <path>.dll`) | 18 |
| Log lines captured | 11757 |

## Warning characterisation

The 5 warnings are the same pre-existing `System.Reactive 7.0.0` `packages.config` diagnostic
recorded under `[P0-T17]` and `[P0-T18]`. They are emitted by a NuGet `.targets` file rather than by
the compiler, which is why `/p:TreatWarningsAsErrors=true` does not promote them to errors.

## Acceptance

- `EXIT_CODE: 0` — met.
- Error count is not greater than the recorded baseline — met (0 vs baseline 0).
- The recorded `Command:` string contains zero occurrences of `Nullable=enable` — met.
