# Remediation QA Gate — Analyzer Gate

Timestamp: 2026-08-23T19-16

Command:
```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

Run from the worktree root. Launched per the Phase 3 long-running command mechanic: a detached
`pwsh -NoProfile` runner invoked `Start-Process -PassThru` with `-RedirectStandardOutput
coverage\analyzer-remediation.log` and `-RedirectStandardError coverage\analyzer-remediation.err.log`,
recorded the child PID, then polled to completion. The recorded exit code is taken from the returned
process object's `ExitCode` property, not from `$LASTEXITCODE` of the polling shell.

EXIT_CODE: 0

Output Summary:

| Measure | Value | Required |
| --- | --- | --- |
| Resolved msbuild path | `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` | recorded |
| Launched PID (`MSBuild.exe` child) | **380896** | recorded |
| Exit code (from the process object's `ExitCode`) | **0** | 0 |
| Warning count | **5** | not gated |
| Error count | **0** | 0 |
| Log lines matching `Skipping target "CoreCompile"` | **0** | exactly 0 |
| Log lines matching `CoreCompile:` (target actually executed) | 66 | corroboration only |
| `Done Building Project` lines | 20 | corroboration only |
| Lines matching `error CS<n>` | 0 | corroboration only |
| Stderr log size | 0 bytes | corroboration only |
| Log file | `coverage\analyzer-remediation.log` (12,208 lines) | — |
| Wall time | 00:00:20.38 | — |

MSBuild summary block, verbatim:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:20.38
```

## Acceptance conditions

1. **`EXIT_CODE: 0`, taken from the process object's `ExitCode`** — met.
2. **The artifact records the resolved msbuild path** — met; bare `msbuild` does not resolve in this
   environment and `pwsh -NoProfile` carries no Visual Studio developer environment, so the verified
   absolute path is invoked through the call operator.
3. **Error count is 0** — met.
4. **`Skipping target "CoreCompile"` count is exactly 0** — met. This is the load-bearing proof that
   the analyzers actually ran rather than being skipped by MSBuild incrementality, which is why
   `/t:Rebuild` is used and `/t:Build` is not. It is corroborated positively by 66 `CoreCompile:`
   target executions and 20 `Done Building Project` lines in the same log. No `csc.exe` count is
   asserted, because that count is zero even on a real compile and would gate nothing.

## Warning inventory

All 5 warnings are the same pre-existing diagnostic, emitted once per affected project by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`:

> warning : The project contains a packages.config file, which is not supported by System.Reactive
> v7.0 or later. Please migrate to PackageReference.

This matches the baseline recorded in
`docs/features/active/winformspumphost-suite-determinism-511/evidence/baseline/analyzer-gate.2026-08-21T18-10.md`
exactly: 5 warnings, 0 errors, 0 `Skipping target "CoreCompile"` lines, 20 `Done Building Project`
lines, 21.11 s baseline versus 20.38 s here. This cycle introduced no analyzer diagnostic.

## Invocation note

The first launch attempt failed with `MSBUILD : error MSB1008: Only one project can be specified.`
because `Start-Process -ArgumentList` was given an array whose `/p:Platform=Any CPU` element lost its
quoting when the arguments were joined. The runner was corrected to pass a single pre-quoted argument
string so the command line reaching `MSBuild.exe` is character-for-character the command above. The
failed attempt compiled nothing and is recorded here for completeness; it is an invocation-mechanics
correction, not a toolchain failure, and does not constitute a loop restart.
