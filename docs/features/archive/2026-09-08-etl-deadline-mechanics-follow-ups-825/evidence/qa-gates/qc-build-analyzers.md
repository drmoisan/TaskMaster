# QC Step 3 — MSBuild Analyzer Gate

Timestamp: 2026-09-09T17-16

Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /flp:LogFile=docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/qc-build-analyzers.txt;Verbosity=detailed

EXIT_CODE: 0

WarningCount: 0
ErrorCount: 0

Output Summary: The solution rebuild with .NET analyzers and code-style enforcement reported
"Build succeeded", 0 Warning(s) and 0 Error(s) in 00:00:12.02. The counts match the P0-T6 baseline
exactly, so this feature introduces no analyzer diagnostic. The sibling detailed file log
qc-build-analyzers.txt exists and carries 70228 lines.

/t:Rebuild is mandatory. MSBuild's up-to-date check does not invalidate on a command-line property
change, so a warm /t:Build would return exit 0 having skipped CoreCompile on every project and would
run no analyzers, making this gate vacuous. Non-vacuity is demonstrated from this log at P8-T5.

## D7 sanitisation

The sibling qc-build-analyzers.txt was sanitised as the last action of this task, after P8-T5 read
every count from it. The counted tokens carry no absolute path and are unaffected by the rewrite.
Three rewrites were applied in order: the worktree root to `<repo-root>`, the main checkout root to
`<main-checkout-root>`, and, as the deviation recorded in full at evidence/other/plan-deviations.md,
the user profile root to `<user-profile-root>`. The count of lines containing `C:\Users\` in this
log is now 0.
