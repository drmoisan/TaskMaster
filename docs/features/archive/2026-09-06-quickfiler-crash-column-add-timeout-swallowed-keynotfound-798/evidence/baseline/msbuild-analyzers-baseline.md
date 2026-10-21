# Phase 0 — Analyzer build baseline

Timestamp: 2026-09-07T00-53
Task: [P0-T6]
Issue: #798

Host-specific absolute paths are redacted to a `<worktree>`, `<vs-install>` or `<scratch>` token.

## Command

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

Executed with the working directory set to `<worktree>`, using the vswhere-resolved MSBuild at
`<vs-install>\MSBuild\Current\Bin\MSBuild.exe`. Two logging switches were added so the build summary
could be observed: `/nologo /v:q` on the console and a file logger writing to `<scratch>` at normal
verbosity. Neither switch changes what is compiled or which analyzers run.

EXIT_CODE: 0

## Summary lines quoted verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Time Elapsed 00:00:21.26 for the logged run.

## Baseline warning count

Baseline warning count: 0

P8-T2 compares the final analyzer build's warning count against this value. Any warning present at
P8-T2 is therefore an increase over baseline and must be enumerated by diagnostic id in that task's
artifact.

## Notes on gate validity

- `/t:Rebuild` was used, not `/t:Build`. MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
  project and runs no analyzers. The logged run rebuilt all nineteen projects in the solution, which
  the file log confirms, so the analyzers actually executed.
- A successful msbuild run still prints the substring `error` many times in switch names and summary
  text. This artifact therefore asserts the exit code and the summary line `0 Error(s)`, never the
  absence of the substring.
- An earlier invocation of the same command at minimal console verbosity also exited 0. MSBuild does
  not emit the warning and error summary block below normal verbosity, which is why the run recorded
  here carries a normal-verbosity file logger. The compiled inputs and the exit code were identical
  across both invocations.

Output Summary: The solution-wide analyzer rebuild exited 0 with the summary line `0 Error(s)` and a
reported warning count of 0. All nineteen projects rebuilt, so analyzer diagnostics were produced
rather than skipped. Baseline warning count for the P8-T2 comparison is 0.
