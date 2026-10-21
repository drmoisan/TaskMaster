# Baseline Nullable and Type-Check State — MSBuild (issue #781)

Timestamp: 2026-09-05T16-25

Task: [P0-T7]

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

Issued from the repository root inside a `pwsh -NoProfile -Command` process, invoking the
MSBuild executable resolved by [P0-T4]. `/p:Nullable=enable` was not added and `/t:Build` was
not substituted, as the task and `CLAUDE.md` both require. Console output was redirected to a
log file under the user temporary directory, outside the repository.

EXIT_CODE: 0

Output Summary: `Build succeeded.` — **3 Warning(s)**, **0 Error(s)**, Time Elapsed
`00:00:13.55`.

- Warning count: **3**
- Error count: **0**

The only warning identifier present anywhere in the log is `MSB3061`, the `CoreClean`
file-deletion diagnostic already recorded by [P0-T6]: three native output binaries under
`TaskMaster\bin\Debug\` are held open by a running Microsoft Outlook process. No `CS86xx`
nullable-flow diagnostic and no other compiler diagnostic appears, so no file that has opted
into nullable analysis with a `#nullable enable` directive currently produces a warning that
`/p:TreatWarningsAsErrors=true` would promote to an error.

The run is not a vacuous incremental pass: the log records 36 `csc.exe` command lines under
`/t:Rebuild`, so compilation genuinely occurred and the gate was able to fail.
