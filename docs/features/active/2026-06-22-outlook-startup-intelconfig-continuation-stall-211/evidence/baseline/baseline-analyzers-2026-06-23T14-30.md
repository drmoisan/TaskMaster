# Baseline — Analyzers (#211 Phase 3)

Timestamp: 2026-06-23T14-30
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(MSBuild path: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`)
EXIT_CODE: 0

Output Summary:
- `Build succeeded.` — `34 Warning(s)`, `0 Error(s)`.
- All 34 warnings are pre-existing and unrelated to this change. Categories:
  - CS8632 (nullable annotation outside `#nullable` context) — predominantly in `UtilitiesCS.Test` and `TaskMaster.Test` test files.
  - CS0618 (obsolete `AsyncEnumerable.SelectAwait`/`WhereAwait`/`ForEachAwaitAsync`) — including a pre-existing instance at `TaskMaster/AppGlobals/AppItemEngines.cs(44,34)` on the existing `.SelectAwait` call. This is the baseline state of the file being instrumented in Phase 2.
  - CS0067 (event never used) — in `UtilitiesCS.Test` test doubles.
- No analyzer-rule errors. Analyzer baseline is green (EXIT_CODE 0).
