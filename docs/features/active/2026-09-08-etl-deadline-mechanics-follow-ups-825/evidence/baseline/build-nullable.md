# Baseline — MSBuild Nullable Gate

Timestamp: 2026-09-09T16-37

Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /flp:LogFile=docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/build-nullable.txt;Verbosity=detailed

EXIT_CODE: 0

WarningCount: 0
ErrorCount: 0

Output Summary: The solution rebuild with warnings treated as errors reported "Build succeeded",
0 Warning(s) and 0 Error(s) in 00:00:11.55. The sibling detailed file log build-nullable.txt exists
and carries 70622 lines. /p:Nullable=enable is deliberately not passed: no project in this
repository carries a Nullable element and there is no Directory.Build.props, so the property would
conscript every file that has never adopted the `#nullable enable` pragma and the gate could not
pass. Nullable enforcement here is per-file opt-in, and /p:TreatWarningsAsErrors=true promotes the
CS86xx diagnostics of the files that have opted in to build errors. /t:Rebuild is mandatory for the
same reason recorded at P0-T6.

## D7 sanitisation

The sibling build-nullable.txt was sanitised as the last action of this task, after the counts above
were taken. Three rewrites were applied in order: the worktree root to `<repo-root>`, the main
checkout root to `<main-checkout-root>`, and, as the deviation recorded in full at
evidence/baseline/build-analyzers.md, the user profile root to `<user-profile-root>` to close the
two residual leak classes D7 does not name. The count of lines containing `C:\Users\` in this log is
now 0.
