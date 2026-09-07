# P0-T13 — Nullable / Type-Check Baseline (Issue #751)

Timestamp: 2026-09-03T14-29

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

Sanitization: applied. Placeholder tokens used: `<WORKTREE>` and `<USER>`.

## Output Summary

Final summary lines, sanitized and transcribed:

```
    19>Done Building Project "<WORKTREE>\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (Rebuild target(s)).
     1>Done Building Project "<WORKTREE>\TaskMaster.sln" (Rebuild target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.78
```

- Warning count: **0**
- Error count: **0**
- Build result: succeeded

## Acceptance

| Required | Observed | Result |
|---|---|---|
| `EXIT_CODE: 0` | 0 | PASS |

## Notes on the command shape

- `/p:Nullable=enable` was **not** added. No project in this repository carries a `<Nullable>` element, and
  the root `Directory.Build.props` sets only `RxUseUnsupportedPackagesConfig`, so no solution-wide nullable
  opt-in exists. Adding the property would conscript every file that has never adopted the
  `#nullable enable` pragma. CI omits it deliberately.
- `/t:Rebuild` was used and `/t:Build` was not, for the reason recorded in P0-T12: a warm `/t:Build` skips
  `CoreCompile` and the gate cannot fail.
- Nullable enforcement in this repository is per-file opt-in: a file participates when it carries
  `#nullable enable`, and `/p:TreatWarningsAsErrors=true` then promotes its `CS86xx` diagnostics to build
  errors. Zero such errors were produced.
