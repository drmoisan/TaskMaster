# Final QC Step 4 — MSBuild nullable and type check (issue #781)

Timestamp: 2026-09-05T17-01

Task: [P2-T4]

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

Issued from the repository root inside a `pwsh -NoProfile -Command` process, using the MSBuild
executable resolved by [P0-T4]. `/p:Nullable=enable` was not added and `/t:Build` was not
substituted.

EXIT_CODE: 0

## Output Summary

`Build succeeded.` — **3 Warning(s)**, **0 Error(s)**, Time Elapsed `00:00:13.03`.

- Warning count: **3**
- Error count: **0**

Both acceptance conditions hold: `EXIT_CODE:` is 0 and the error count is 0.

The only diagnostic identifier present anywhere in the log is `MSB3061`, the `CoreClean`
file-deletion warning caused by a running Microsoft Outlook process holding three native output
binaries open. No `CS86xx` nullable-flow diagnostic and no other compiler diagnostic appears, so
the guard rewrite in `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` and the new test file
introduced no nullable or type-safety warning that `/p:TreatWarningsAsErrors=true` would promote
to an error. The counts are identical to the baseline recorded in
`FEATURE/evidence/baseline/msbuild-nullable.2026-09-05T10-49.md`.

The run is not a vacuous incremental pass: the log records 36 `csc.exe` command lines under
`/t:Rebuild`. No file in the working tree was changed by this step, so the loop proceeds to
[P2-T5] rather than restarting at [P2-T1].
