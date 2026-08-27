# Phase 6 — .NET Analyzer Gate (final pass)

Timestamp: 2026-08-27T14-18
Task: [P6-T3]
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0

Started 2026-08-27T14:17:28Z, ended 2026-08-27T14:17:50Z. `Time Elapsed 00:00:20.90`.

## Output Summary

- **Errors: 0** — the acceptance condition.
- Warnings: 5.

### Non-vacuity proof

`/t:Rebuild` is used rather than `/t:Build`. MSBuild's incremental up-to-date check compares
timestamps and does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit
0 with `CoreCompile` skipped on every project and runs no analyzer at all. Two counts taken from the
build log establish that this run actually compiled:

| Measurement | Value | Meaning |
| --- | --- | --- |
| `Skipping target "CoreCompile"` occurrences | **0** | no project skipped compilation |
| `CoreCompile:` target-execution headers | **51** | compilation ran 51 times |

A `csc.exe` process count is deliberately not used as the non-vacuity signal: this build compiles
through the in-process compiler server, so that count reads zero even on a genuine full compile.

### The five warnings

All five are the same pre-existing warning replicated once per affected project, and none originates
in a file this feature touches:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference.
```

Emitted for `QuickFiler.csproj`, `TaskMaster.csproj`, `ToDoModel.csproj`, `UtilitiesCS.csproj` and
`UtilitiesCS.Test.csproj`. This is a repository-wide packaging condition that predates this feature;
this change edits no project file (see `evidence/qa-gates/project-file-gate.2026-08-27T14-03.md`), so
it neither introduced nor can it remove these warnings.

### Earlier aborted attempt on the same tree

An attempt at 2026-08-27T13:55:33Z recorded `EXIT_CODE: 1` with 28 errors. Every one of those 28 was
`MSB3021` or `MSB3027` file-copy contention — `The file is locked by: "testhost (84376)"` — caused by
a coverage-enabled vstest run that was live in this worktree at the time. **Zero compiler
diagnostics were emitted in that attempt.** The failure was environmental. The contending run was
subsequently confirmed hung (28.7 seconds of CPU across 30 minutes of wall time, no test result file
written, no coverage output written) and was terminated, and the toolchain was restarted from
[P6-T1] as the Phase 6 restart rule requires. Full sequence:
`evidence/qa-gates/toolchain-loop.2026-08-27T14-18.md`.
