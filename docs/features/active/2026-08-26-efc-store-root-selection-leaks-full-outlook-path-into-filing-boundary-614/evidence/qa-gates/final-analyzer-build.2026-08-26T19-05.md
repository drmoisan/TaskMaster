# P9-T2 — Final QC step 2: analyzer gate (#614; AC24 step 2)

Timestamp: 2026-08-26T19-05

Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Run from `<repo-root>` under `pwsh -NoProfile`. `/t:Build` was NOT substituted for `/t:Rebuild`.

EXIT_CODE: 0

## Output Summary

- MSBuild final tally: `5 Warning(s)`, `0 Error(s)`. Time elapsed 00:00:11.35.
- Error lines matching `: error ` in the full log: **0**.
- Warning lines matching `: warning ` in the full log: 10 log occurrences resolving to the 5
  distinct warnings MSBuild tallied. All 5 are the same pre-existing advisory, one per
  packages.config project that references System.Reactive 7.0.0:
  `System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a
  packages.config file, which is not supported by System.Reactive v7.0 or later.` This warning is
  present on the Phase 0 baseline (`evidence/baseline/analyzer-build.2026-08-26T11-32.md`) and is
  unchanged by this change. Zero analyzer diagnostics were introduced.

## Non-vacuity demonstration (AC24)

The build did NOT skip `CoreCompile`. Measured from the full log:

- Projects producing a `-> ....dll` output line: **18** (the whole solution).
- `CoreCompile:` occurrences: **54**.
- `csc.exe` invocations recorded: **36**.

`/t:Rebuild` forces Clean followed by Build, so MSBuild's incremental up-to-date check cannot skip
compilation on a warm tree. A vacuous run would show zero `csc.exe` invocations and zero
`CoreCompile:` lines; this run shows 36 and 54 respectively, so every project was genuinely
recompiled with `EnableNETAnalyzers` and `EnforceCodeStyleInBuild` in effect.

Raw MSBuild log (contains absolute host paths, including the machine account name) was written to
the session scratchpad outside the repository and is not copied under `evidence/`.
