# Baseline — Analyzers Build

Timestamp: 2026-07-19T05-10
Command: `MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s), 229 Warning(s) (per the MSBuild summary line). No
analyzer/code-style error is present on the unmodified branch head. Warnings observed are
pre-existing debt unrelated to this feature (CS0618 obsolete-API usage in QuickFiler/TaskMaster,
CS8632 nullable-annotation-context warnings in test projects, CS8767 nullability-mismatch,
MSTEST0032 analyzer suggestion, CS0067 unused-event warnings) — none block this analyzer/lint
gate since `TreatWarningsAsErrors` is not passed on this command.
