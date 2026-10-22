# Phase 6 — Analyzer Gate (P6-T3)

Timestamp: 2026-09-08T08-32

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:LogFile=coverage/plan812/p6-t3/analyzers.log;Verbosity=detailed"`

EXIT_CODE: 0

Output Summary:

- Error count: **0**, transcribed from the build summary line `    0 Error(s)`.
- Warning count: **0**, transcribed from the build summary line `    0 Warning(s)`.
- Count of lines in `coverage/plan812/p6-t3/analyzers.log` containing the literal `Skipping target "CoreCompile"`: **0**.

Non-vacuity evidence, which AC7 requires:

- The log file `coverage/plan812/p6-t3/analyzers.log` exists.
- Its total line count is **69105**, which is greater than zero. The `Skipping target "CoreCompile"` count above is therefore a count taken over a real log rather than over a missing file, where a count of zero would be indistinguishable from a pass.
- The zero `Skipping target "CoreCompile"` count is achievable only because the command uses `/t:Rebuild`. MSBuild's incremental up-to-date check compares timestamps and does not invalidate on a command-line `/p:` change, so a warm `/t:Build` would return exit 0 having skipped `CoreCompile` on every project and having run no analyzers at all; that gate could not fail. Every project therefore genuinely recompiled under the analyzer properties.

The log file itself is not committed. It is written under `coverage/plan812/`, which is git-ignored by `coverage/*`, because an MSBuild log at detailed verbosity carries absolute host paths.

Comparison against the P0-T9 baseline, which recorded the same command at `EXIT_CODE: 0` with a warning count of 0, an error count of 0, and a zero `Skipping target "CoreCompile"` count: the figures are unchanged, so this change introduces no analyzer diagnostic.
