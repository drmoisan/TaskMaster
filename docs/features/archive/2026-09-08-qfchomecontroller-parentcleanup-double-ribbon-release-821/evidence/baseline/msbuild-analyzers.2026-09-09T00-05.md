# Phase 0 — Baseline analyzers

Timestamp: 2026-09-09T12-35
Task: [P0-T8]

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0

Output tail, verbatim:

```text
     1>Done Building Project "<repo-root>\TaskMaster.sln" (Rebuild target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.08
```

## Baseline integers

| Metric | Value |
|---|---|
| Summary line ending `Warning(s)` | **0** |
| Summary line ending `Error(s)` | **0** |
| Literal `Build succeeded.` present | **yes** |

## Proof the Rebuild was not vacuous

The Rebuild target is mandatory because MSBuild's up-to-date check does not invalidate on a
command-line property change, so a warm `/t:Build` can return exit 0 having skipped `CoreCompile` on
every project and run no analyzers at all. That failure mode is ruled out here by counting
compilation activity in the captured log rather than by trusting the exit code:

| Observation over the captured build log | Count |
|---|---|
| Total log lines | 4780 |
| Lines containing `CoreCompile:` | 47 |
| Lines containing `csc.exe` | 36 |

Compilation genuinely ran on every project, so the analyzers genuinely ran and the recorded
zero-warning, zero-error result is a real observation rather than a skipped-target artefact.

Output Summary: baseline analyzer state is **0 warnings and 0 errors**, with `Build succeeded.`
present and exit code 0. The build was verified non-vacuous by 47 `CoreCompile:` occurrences and 36
`csc.exe` invocations in the log. The zero baseline warning count makes `[P6-T3]`'s condition — the
post-change warning integer must not exceed the baseline — a strict zero-warning requirement on the
final tree.
