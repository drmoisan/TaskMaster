# Phase 2 — Analyzer Build (P2-T2)

Timestamp: 2026-07-20T23-03

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(VS18 MSBuild.exe, dash-switch syntax, MSYS_NO_PATHCONV=1.)

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Error(s), 6 Warning(s). Zero analyzer errors. The six warnings are:
- 4 pre-existing System.Reactive 7.0 packages.config advisory warnings (ToDoModel, QuickFiler,
  TaskMaster, UtilitiesCS.Test) — unchanged from the P0-T3 baseline.
- 2 CS2002 "Source file 'OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times"
  warnings. This is a PRE-EXISTING duplicate `<Compile Include>` for PercentageFormatterTests.cs present
  in UtilitiesCS.Test.csproj at HEAD (lines 290 and 340; `git show HEAD:...` confirms both entries).
  It surfaces now only because the R1 test-file edits force a recompile of UtilitiesCS.Test. It is a
  compiler warning (not an analyzer diagnostic) and does not fail this gate (0 analyzer errors). It is
  outside the R1 scope lock (which limits csproj changes to the two new `<Compile Include>` additions),
  so it is not remediated here and is recorded as an escalation observation.

No analyzer diagnostics are attributable to the four split test files. Gate PASS.
