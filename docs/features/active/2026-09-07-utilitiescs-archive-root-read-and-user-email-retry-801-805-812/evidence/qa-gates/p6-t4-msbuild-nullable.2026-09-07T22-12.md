# Phase 6 — Nullable Gate (P6-T4)

Timestamp: 2026-09-08T08-34

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:LogFile=coverage/plan812/p6-t4/nullable.log;Verbosity=detailed"`

EXIT_CODE: 0

`/p:Nullable=enable` is deliberately not present. Nullable enforcement in this repository is per-file opt-in through the `#nullable enable` directive, and the solution-wide property would conscript files that never adopted the pragma. This command is character-for-character the one in `.github/workflows/_build-nullable.yml`.

Output Summary:

- Error count: **0**, transcribed from the build summary line `    0 Error(s)`.
- Warning count: **0**, transcribed from the build summary line `    0 Warning(s)`.
- Count of lines in `coverage/plan812/p6-t4/nullable.log` containing the literal `Skipping target "CoreCompile"`: **0**.

Non-vacuity evidence:

- The log file `coverage/plan812/p6-t4/nullable.log` exists.
- Its total line count is **69300**, which is greater than zero, so the `Skipping target "CoreCompile"` count is a count taken over a real log.
- `/t:Rebuild` is what makes the zero count achievable, for the reason recorded in D4 and in the P6-T3 artifact.

Both files this plan adds to the compile set participate in nullable analysis: `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs` carries `#nullable enable` on line 1, and `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` already did. The guarded accessor's `string?` return type, the null-conditional `_globals?.Ol.ArchiveRootPath` read, and the relocated helper's `string? archiveRoot` parameter are therefore all checked, and none produced a `CS86xx` diagnostic. Because `/p:TreatWarningsAsErrors=true` promotes compiler warnings to errors, a nullable-flow warning in either file would have failed this build rather than appearing as a warning.

Note on the warning count: `/p:TreatWarningsAsErrors=true` promotes compiler warnings only, so an MSBuild task warning such as MSB3277 would survive into the summary with the build still exiting 0. The recorded warning count of 0 is therefore a real observation and not an artefact of the promotion.

Comparison against the P0-T10 baseline, which recorded the same command at `EXIT_CODE: 0` with a warning count of 0, an error count of 0, and a zero `Skipping target "CoreCompile"` count: the figures are unchanged.
