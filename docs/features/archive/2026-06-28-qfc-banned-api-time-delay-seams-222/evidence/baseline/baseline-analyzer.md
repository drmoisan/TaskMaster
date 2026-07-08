# Baseline — Analyzer Build (P0-T7)

Timestamp: 2026-06-28T19-09
Command: MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary:
- Build succeeded. 0 Error(s), 68 Warning(s). Time elapsed ~13s.
- Warnings are all pre-existing and out of scope: CS8632 (nullable annotation outside #nullable context, mostly test projects), CS0618 (obsolete AsyncEnumerable LINQ overloads), CS0067 (unused events in tests), MSTEST0032 (one always-true assertion in QfcFormControllerTests.cs).
- RS0030 (BannedApiAnalyzers) occurrences for the eight sites: NOT surfaced as build warnings. RS0030 is configured at `severity = suggestion` per .claude/rules/csharp.md (initial rollout; existing call sites are not build-broken). Suggestion-level diagnostics are not emitted by `-v:m` and do not fail the analyzer build. The eight banned sites remain present at baseline (confirmed in site-reconfirmation.md) but do not break the build.
