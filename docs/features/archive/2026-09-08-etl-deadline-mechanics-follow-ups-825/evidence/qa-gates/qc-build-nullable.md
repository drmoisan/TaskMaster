# QC Step 4 — MSBuild Nullable Gate

Timestamp: 2026-09-09T17-17

Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /flp:LogFile=docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/qc-build-nullable.txt;Verbosity=detailed

EXIT_CODE: 0

WarningCount: 0
ErrorCount: 0

Output Summary: The solution rebuild with warnings treated as errors reported "Build succeeded",
0 Warning(s) and 0 Error(s) in 00:00:13.11. The counts match the P0-T7 baseline exactly. This is the
gate that matters most for item 3: widening EtlAsync's first tuple element to `object[,]?` changes
null-state flow in two files that carry `#nullable enable`,
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs and UtilitiesCS/Extensions/DfDeedle.cs,
and a clean pass under /p:TreatWarningsAsErrors=true is what shows no CS86xx diagnostic was
introduced. The three tableSnapshot.Item1 reads renamed at P5-T4 are the reason it stays clean: the
guard at DfDeedle.cs establishes flow state for the tuple's `data` name, and a read through the
positional Item1 name would not have carried that state.

/p:Nullable=enable is deliberately not added. No project in this repository carries a Nullable
element and there is no Directory.Build.props, so the property is a solution-wide opt-in that
conscripts every file which has never adopted the pragma; the gate could not pass with it and CI
omits it deliberately. Nullable enforcement here is per-file opt-in through the `#nullable enable`
directive, and /p:TreatWarningsAsErrors=true promotes the CS86xx diagnostics of the participating
files to build errors.

/t:Rebuild is mandatory for the same reason recorded at P8-T3. The sibling detailed file log
qc-build-nullable.txt exists and carries 70653 lines.

## D7 sanitisation

The sibling qc-build-nullable.txt was sanitised as the last action of this task, after P8-T5 read
every count from it. Three rewrites were applied in order: the worktree root to `<repo-root>`, the
main checkout root to `<main-checkout-root>`, and, as the deviation recorded in full at
evidence/other/plan-deviations.md, the user profile root to `<user-profile-root>`. The count of
lines containing `C:\Users\` in this log is now 0.
