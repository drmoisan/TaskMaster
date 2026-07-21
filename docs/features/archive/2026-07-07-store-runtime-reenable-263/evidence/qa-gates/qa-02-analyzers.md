# QA Gate 02 — Analyzers (P6-T2)

Timestamp: 2026-07-08T01-27

Command: msbuild TaskMaster.sln -t:Build -m -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true

(dash-switch form; equivalent to the CLAUDE.md `/t:Build ...` form. git-bash MSYS mangles leading-slash switches.)

EXIT_CODE: 0

Output Summary:
- Build succeeded. 73 Warning(s), 0 Error(s). Baseline (P0-T11) was 72 warnings, 0 errors.
- No NEW analyzer/compiler warning originates in any F3 file. A targeted rebuild scan of the F3-touched files found only two pre-existing CS0618 (obsolete AsyncEnumerable) warnings in `AppEvents.cs` lines 269/301 — located in `ProcessMailItemAsync`, code F3 did not modify (F3's AppEvents.cs edit is only the PerformReadinessHookup loop-body delegation at line ~244).
- All other warnings are pre-existing and unrelated (QuickFiler CS0108, ToDoModel.Test CS0169, QuickFiler.Test MSTEST0032, UtilitiesCS Bayesian/EmailIntelligence CS0618 obsolete-API, AutoFile CS0168, UtilitiesCS.Test CS8632/CS0067).
- No increase attributable to F3 code. Analyzer gate PASS.
