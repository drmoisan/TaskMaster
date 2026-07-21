# Baseline — Analyzer / Code-Style Build

Timestamp: 2026-07-19T00-15

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(invoked via VS18 MSBuild.exe with dash-form switches under git-bash + MSYS_NO_PATHCONV=1 to avoid switch mangling; command semantics identical to the plan text)

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Error(s), 76 Warning(s).
The build initially failed with 8x CS0006 "Metadata file could not be found" for three analyzer DLLs (Meziantou.Analyzer.3.0.101, SonarAnalyzer.CSharp.10.27.0.140913, Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4). Root cause: the committed csproj `<Analyzer Include>` paths hardcode these older versions while `packages.config` references newer versions (3.0.123 / 10.29.0.143774 / 5.6.0), which is what `msbuild /t:Restore` installed. Bootstrap fix (environment-only, no tracked-file edits): the three older analyzer versions were `nuget install`ed into the gitignored `packages/` folder so the stale csproj paths resolve. No csproj and no packages.config was edited; 16 csproj reference these stale paths and editing them would be a prohibited broad refactor.

Baseline warning distribution (pre-existing, not introduced by this feature; analyzer build does not use TreatWarningsAsErrors so these are non-fatal):
- CS8632 x66 (nullable annotation in `#nullable`-disabled context — pre-existing annotations in null-oblivious files)
- CS0618 x56 (obsolete member usage)
- CS0108 x8 (member hides inherited member)
- CS0169 x6, CS0067 x6 (unused field/event)
- CS8625 x2, CS4014 x2, CS2002 x2, CS0168 x2

This is the pre-change analyzer baseline; the batch analyzer gates compare against this to confirm no NEW analyzer errors.
