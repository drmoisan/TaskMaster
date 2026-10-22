# Phase 0 — baseline nullable / type-check build (P0-T7)

Timestamp: 2026-09-01T21-35

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

Output Summary:

The MSBuild summary lines, reproduced verbatim:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:13.35
```

## CS86 diagnostic enumeration

A scan of the full build log for the pattern `CS86[0-9][0-9]` returned **no match**. **No `CS86`
diagnostic was reported.** The baseline CS86 set is therefore empty, and any `CS86` diagnostic
appearing in the P2-T4 post-change run is newly introduced by this change.

The five warnings reported are the same uncoded System.Reactive `packages.config` warnings
enumerated in `analyzer-build.md`; none is a compiler or nullable-flow diagnostic. `0 Error(s)`
confirms that `/p:TreatWarningsAsErrors=true` promoted nothing to an error.

## Non-vacuity control

`/t:Rebuild` was used rather than `/t:Build`, verified directly: the build log contains **62**
`CoreCompile:` target executions, so compilation, and therefore nullable-flow analysis, actually ran
on this invocation. MSBuild's up-to-date check does not invalidate on a command-line `/p:` change,
so a warm `/t:Build` would have exited 0 with `CoreCompile` skipped on every project and the gate
could not have failed.

`/p:Nullable=enable` was deliberately **not** added. This command is character-for-character the one
in `.github/workflows/ci.yml`. No project in this repository carries a `<Nullable>` element and
there is no `Directory.Build.props`, so adding that property would conscript every file that has
never adopted the `#nullable enable` pragma. Nullable enforcement here is per-file opt-in; omitting
the property loses no enforcement over any file that has opted in.
