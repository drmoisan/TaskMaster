# Final QC Step 3 — MSBuild analyzers (issue #781)

Timestamp: 2026-09-05T16-59

Task: [P2-T3]

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Issued from the repository root inside a `pwsh -NoProfile -Command` process, using the MSBuild
executable resolved by [P0-T4]. Console output was redirected to a log file under the user
temporary directory, outside the repository.

EXIT_CODE: 0

## Output Summary

`Build succeeded.` — **3 Warning(s)**, **0 Error(s)**, Time Elapsed `00:00:14.43`.

- Warning count: **3**
- Error count: **0**

All three acceptance conditions hold:

1. `EXIT_CODE:` is 0.
2. The error count is 0.
3. The warning count of 3 is at or below the baseline count of 3 recorded in
   `FEATURE/evidence/baseline/msbuild-analyzers.2026-09-05T10-49.md`.

The only warning identifier present anywhere in the log is `MSB3061`, the same `CoreClean`
file-deletion diagnostic recorded at baseline: three native output binaries under
`TaskMaster\bin\Debug\` are held open by a running Microsoft Outlook process. It concerns
deletion of build output rather than source, and no analyzer rule identifier appears in the
summary. The warning set is therefore unchanged by this plan's edits, neither added to nor
reduced.

The run is not a vacuous incremental pass: the log records 36 `csc.exe` command lines under
`/t:Rebuild`, so compilation and analyzer execution genuinely occurred and the gate was able to
fail. No file in the working tree was changed by this step, so the toolchain loop proceeds to
[P2-T4] rather than restarting at [P2-T1].
