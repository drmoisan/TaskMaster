# P0-T9 — Nullable / type-check baseline build

Timestamp: 2026-09-03T08-23

Command (quoted command line, verbatim):
```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

EXIT_CODE: 0

## Output Summary

Trailing MSBuild summary, verbatim:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.92
```

- Errors: **0**

## Acceptance

- `EXIT_CODE: 0` — satisfied.
- The artifact records `0 Error(s)` — satisfied.
- The quoted command line **contains** `msbuild.exe TaskMaster.sln` — satisfied. The clause is
  worded as `contains` rather than `begins with` because the recorded line begins with the
  `env -C <worktree-root> ` working-directory prefix and the `MSYS_NO_PATHCONV=1 ` assignment
  required by constraint 4 of "Shell constraints measured in this worktree".
- The quoted command line contains no `Nullable=enable` substring — satisfied. `/p:Nullable=enable`
  is deliberately absent, matching `.github/workflows/_build-nullable.yml` and CLAUDE.md's explicit
  instruction not to add it.
- The quoted command line uses `/t:Rebuild` rather than `/t:Build` — satisfied, so the gate is not
  vacuous through MSBuild incrementality.
