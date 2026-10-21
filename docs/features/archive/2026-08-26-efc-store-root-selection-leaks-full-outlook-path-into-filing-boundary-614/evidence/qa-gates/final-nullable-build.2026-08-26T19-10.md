# P9-T3 — Final QC step 3: nullable / type-check gate (#614; AC24 step 3)

Timestamp: 2026-08-26T19-10

Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

Run from `<repo-root>` under `pwsh -NoProfile`. This is character-for-character the command in
`.github/workflows/ci.yml`. `/p:Nullable=enable` was **NOT** added (it diverges from CI and is red
on main by construction), and `/t:Build` was **NOT** substituted for `/t:Rebuild`.

EXIT_CODE: 0

## Output Summary

- MSBuild final tally: `5 Warning(s)`, `0 Error(s)`. Time elapsed 00:00:10.63.
- Error lines matching `: error ` in the full log: **0**.
- Nullable-flow diagnostics matching `CS86[0-9][0-9]` in the full log: **0**.
- The 5 warnings are the same pre-existing System.Reactive packages.config advisory recorded in
  the P9-T2 artifact and on the Phase 0 baseline. Under `/p:TreatWarningsAsErrors=true` they are
  not promoted to errors because they are emitted by an MSBuild target rather than by the C#
  compiler, which is the same behaviour as the baseline run.
- Nullable-context note: the three files this change edits that carry `#nullable enable`
  (`BreadcrumbBridgeRouter.cs`, `EmailFilerConfig.cs`, `FolderConverter.cs`) plus the two new
  `#nullable enable` files (`ArchiveStemContract.cs`, `EfcSelectionGuard.cs`) introduce zero
  `CS86xx` diagnostics.

## Non-vacuity demonstration (AC24)

- Projects producing a `-> ....dll` output line: **18** (the whole solution).
- `CoreCompile:` occurrences: **61**.
- `csc.exe` invocations recorded: **36**.

`/t:Rebuild` forces Clean followed by Build, so the incremental up-to-date check cannot skip
compilation. A vacuous run would show zero `csc.exe` invocations; this run shows 36.

Raw MSBuild log (contains absolute host paths, including the machine account name) was written to
the session scratchpad outside the repository and is not copied under `evidence/`.
