# Baseline Analyzer State — MSBuild (issue #781)

Timestamp: 2026-09-05T16-22

Task: [P0-T6]

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Issued from the repository root inside a `pwsh -NoProfile -Command` process, invoking the
MSBuild executable resolved by [P0-T4]. Console output was redirected to a log file under the
user temporary directory, outside the repository, and the counts below were read from that log.

EXIT_CODE: 0

Output Summary: `Build succeeded.` — **3 Warning(s)**, **0 Error(s)**, Time Elapsed
`00:00:14.62`.

- Warning count: **3**
- Error count: **0**

All three warnings are the same diagnostic, `MSB3061`, raised by the `CoreClean` target of
`TaskMaster.csproj` when it could not delete three native output binaries under
`TaskMaster\bin\Debug\` because a running Microsoft Outlook process holds them open. The
diagnostic concerns deletion of build output and is unrelated to source code or to analyzer
rules; no analyzer rule identifier appears in the summary. This is an environmental condition of
the workstation rather than a repository defect, and it can vary between runs depending on
whether Outlook is running. The [P2-T3] acceptance compares the final warning count against this
figure with an at-or-below test, so a later run with Outlook closed and zero such warnings still
satisfies it.

The run is not a vacuous incremental pass. The log records 67 `CoreCompile:` target entries and
36 `csc.exe` command lines, 34 of which carry `/analyzer:` arguments, across 20
`Done Building Project` entries, so compilation and analyzer execution did occur under
`/t:Rebuild`.
