# Phase 0 — Nullable build baseline

Timestamp: 2026-09-07T00-53
Task: [P0-T7]
Issue: #798

Host-specific absolute paths are redacted to a `<worktree>`, `<vs-install>` or `<scratch>` token.

## Command

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true

Executed with the working directory set to `<worktree>`, using the vswhere-resolved MSBuild at
`<vs-install>\MSBuild\Current\Bin\MSBuild.exe`. Two logging switches were added so the build summary
could be observed: `/nologo /v:q` on the console and a file logger writing to `<scratch>` at normal
verbosity. Neither switch changes what is compiled or which diagnostics are produced.

EXIT_CODE: 0

## Summary lines quoted verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Time Elapsed 00:00:18.56 for the logged run.

## Baseline warning count

Baseline warning count: 0

P8-T2 compares the final analyzer build's warning count against the P0-T6 baseline. This nullable
baseline records its own warning count of 0 for completeness.

## `/p:Nullable=enable` was not supplied

`/p:Nullable=enable` was deliberately omitted from the command above. The reasons are:

- No project in this repository carries a `<Nullable>` element and there is no repository-root build
  property file, so the property is a solution-wide opt-in that would conscript every file that has
  never adopted the `#nullable enable` pragma. Forcing it produced 195 errors in the UtilitiesCS
  project on a prior occasion against zero errors without it.
- The command executed here is character-for-character the command CI runs in its nullable build
  workflow step. CI omits the property deliberately, and adding it locally would make the local gate
  diverge from the gate that actually protects the branch.
- Omitting the property loses no enforcement over any file that has opted in: nullable enforcement
  in this repository is per-file opt-in through the pragma, and `/p:TreatWarningsAsErrors=true` then
  promotes those files' `CS86xx` diagnostics to build errors.

This matters directly for this change. `UtilitiesCS/Extensions/DfDeedle.cs` carries
`#nullable enable`, and P1-T1 requires the new partial `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`
to carry the same pragma, so both files participate in nullable analysis under this command and a
null-state defect introduced by this change would fail this gate.

## Notes on gate validity

- `/t:Rebuild` was used, not `/t:Build`. A warm `/t:Build` returns exit 0 having skipped
  `CoreCompile` on every project, so the gate could not fail. The logged run rebuilt every project
  in the solution.
- The artifact asserts the exit code and the summary line `0 Error(s)`, never the absence of the
  substring `error`, which a successful msbuild run prints many times in switch names and summary
  text.

Output Summary: The solution-wide nullable rebuild exited 0 with the summary line `0 Error(s)` and a
reported warning count of 0. `/p:Nullable=enable` was not supplied, matching CI and preserving the
repository's per-file opt-in nullable model.
