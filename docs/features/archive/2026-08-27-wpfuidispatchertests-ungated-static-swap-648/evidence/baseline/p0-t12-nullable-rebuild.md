# P0-T12 — Nullable-Gate Baseline

Timestamp: 2026-09-01T13-41

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```
MSBuild was re-resolved through `vswhere.exe` and the command was issued through `pwsh`, from the
checkout root. `/p:Nullable=enable` was not added, per `CLAUDE.md:211` and `.claude/rules/csharp.md:16`.

EXIT_CODE: 0

Output Summary:

Summary error count and warning count, taken verbatim from the MSBuild summary block
(log lines 11902, 11929 and 11930):

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

The error count is zero and the observed exit code is zero, so no `BASELINE_GATE_RED:` line is
recorded and execution continues to P0-T13.

Supporting observation confirming this was a genuine compile: the log contains 67 `CoreCompile:`
target entries, so `CoreCompile` was not skipped by MSBuild incrementality.

All five warnings are again the System.Reactive `packages.config` diagnostic emitted once per owning
project, the same population P0-T11 recorded. None is a compiler or nullable-flow diagnostic and none
names `Controllers\WpfUiDispatcherTests.cs`. The baseline warning count that P2-T4 compares against
is therefore 5.

Note on prior recorded state: earlier sessions recorded a large pre-existing nullable-error
population on this solution under a forced Rebuild, including a vendored-project population. That
population is not present on this tree; this run produced zero errors across every project in
`TaskMaster.sln`. The observation is recorded because it supersedes those earlier readings for this
branch.
