# Baseline — Analyzer Build (.NET Analyzers / Roslyn)

Timestamp: 2026-07-10T20-52
Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug "-p:Platform=Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: Build succeeded. 76 Warning(s), 0 Error(s). All warnings are pre-existing
(CS0618 obsolete AsyncEnumerable APIs, CS8632 nullable-annotation-context, CS0169/CS0067
unused-field/event, CS0108 member-hiding, CS4014 unawaited-call, MSTEST0032) in files unrelated
to the F4 scope (KbdActions.cs, KeyboardHandler.cs, FlagDetails.cs, FolderRemapController.cs,
TraceUtility.cs). None of the five in-scope files appear in the warning list. Establishes the
pre-change baseline warning count for later no-regression comparison.
