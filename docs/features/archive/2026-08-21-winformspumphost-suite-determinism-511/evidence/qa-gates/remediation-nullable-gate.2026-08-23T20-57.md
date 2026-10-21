# Remediation QA Gate — Nullable / Warnings-As-Errors Gate

Timestamp: 2026-08-23T19-18

Command:
```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

Run from the worktree root. Launched per the Phase 3 long-running command mechanic: a detached
`pwsh -NoProfile` runner invoked `Start-Process -PassThru` with `-RedirectStandardOutput
coverage\nullable-remediation.log` and `-RedirectStandardError coverage\nullable-remediation.err.log`,
recorded the child PID, then polled to completion. The recorded exit code is taken from the returned
process object's `ExitCode` property, not from `$LASTEXITCODE` of the polling shell.

EXIT_CODE: 0

Output Summary:

| Measure | Value | Required |
| --- | --- | --- |
| Resolved msbuild path | `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` | recorded |
| Launched PID (`MSBuild.exe` child) | **90916** | recorded |
| Exit code (from the process object's `ExitCode`) | **0** | 0 |
| Error count | **0** | 0 |
| Warning count | 5 | not gated |
| Log lines matching `Skipping target "CoreCompile"` | **0** | exactly 0 |
| Log lines matching `CoreCompile:` (target actually executed) | 78 | corroboration only |
| Occurrences of `p:Nullable=enable` anywhere in the log | **0** | must be 0 |
| Log file | `coverage\nullable-remediation.log` (11,939 lines) | — |
| Wall time | 00:00:17.32 | — |

MSBuild summary block, verbatim:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.32
```

## Acceptance conditions

1. **`EXIT_CODE: 0`, taken from the process object's `ExitCode`** — met.
2. **The artifact records the resolved msbuild path** — met.
3. **Error count is 0** — met. No `CS86xx` nullable-flow diagnostic was promoted to an error in any
   file that carries a `#nullable enable` directive.
4. **`Skipping target "CoreCompile"` count is exactly 0** — met, corroborated by 78 `CoreCompile:`
   target executions. `/t:Rebuild` is used, never `/t:Build`, because MSBuild's up-to-date check
   does not invalidate on a command-line `/p:` change and a warm `/t:Build` would return exit 0
   having compiled nothing.
5. **The command carried no `/p:Nullable=enable`** — confirmed. The property appears nowhere in the
   command line above and matches zero lines of the build log. This is deliberate: no project in
   this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the
   property is a solution-wide opt-in that conscripts every file which never adopted the pragma, and
   `.github/workflows/ci.yml` omits it. The command above is character-for-character the CI step
   "Build with nullable warnings treated as errors".

The 5 warnings are the same pre-existing `System.Reactive.PackagesConfigCheck.targets` diagnostic
recorded in the analyzer gate. `/p:TreatWarningsAsErrors=true` did not promote them to errors, which
matches the baseline recorded in
`docs/features/active/winformspumphost-suite-determinism-511/evidence/baseline/nullable-gate.2026-08-21T18-10.md`.
