# Phase 0 — Baseline type-check (nullable)

Timestamp: 2026-09-09T12-38
Task: [P0-T9]

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

`/p:Nullable=enable` was **not** added. No project in this repository carries a `<Nullable>` element,
so that property would be a solution-wide opt-in conscripting every file which never adopted the
`#nullable enable` pragma, producing hundreds of errors. CI omits it deliberately. The command above
is character-for-character the repository's approved nullable-gate command.

Output tail, verbatim:

```text
     1>Done Building Project "<repo-root>\TaskMaster.sln" (Rebuild target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:11.54
```

## Baseline integers

| Metric | Value |
|---|---|
| Summary line ending `Warning(s)` | **0** |
| Summary line ending `Error(s)` | **0** |
| Literal `Build succeeded.` present | **yes** |

## Proof the Rebuild was not vacuous

| Observation over the captured build log | Count |
|---|---|
| Lines containing `CoreCompile:` | 67 |
| Lines containing `csc.exe` | 36 |

Compilation ran on every project, so the nullable-flow diagnostics genuinely ran. A warm `/t:Build`
would have returned exit 0 with `CoreCompile` skipped and could not have failed; that failure mode is
excluded here by observation rather than by assumption.

Output Summary: baseline type-check state is **0 errors** and 0 warnings, with `Build succeeded.`
present and exit code 0. The two `#nullable enable` files this plan will edit —
`UtilitiesCS/Threading/ProgressViewer.cs` and `UtilitiesCS/Threading/ProgressPane.cs` — currently
compile clean only because each suppresses its dereference with the `!` operator. Removing that
operator without supplying a guard the compiler can see would raise CS8602, which this gate promotes
to an error; `[P6-T4]` is therefore the gate that proves the delivered guard is real.
