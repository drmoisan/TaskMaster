# Analyzer / Codestyle Build Baseline

- Timestamp: 2026-07-19T08-51
- Task: [P0-T3]
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0

## Output Summary

Analyzer/codestyle build succeeded: 0 Errors, 75 Warnings (MSBuild-deduplicated summary count). Warnings are pre-existing (e.g. CS0067 unused test events, CS0649 vendored SVGControl fields, CS0618 obsolete IAsyncEnumerable overloads) and unrelated to this feature. This is the authoritative analyzer baseline for the final-pass no-new-diagnostics comparison in P9-T2.

## Environment Setup Note (pre-existing HEAD drift, not a feature change)

The initial run of this command failed with 8x CS0006 "Metadata file could not be found" for old analyzer DLLs. Root cause: pre-existing dependabot drift at HEAD (commit 7de9f11f "bump microsoft-extensions-and-bcl group") updated `packages.config` and each csproj's analyzer `<Import>`/`<Error>` props to new analyzer versions (Meziantou.Analyzer 3.0.123, SonarAnalyzer.CSharp 10.29.0.143774, Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0) but left the hand-maintained `<Analyzer Include>` DLL paths pinned to the prior versions (Meziantou.Analyzer 3.0.101, SonarAnalyzer.CSharp 10.27.0.140913, Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4) in all first-party csproj files. NuGet restore (driven by packages.config) fetched only the new versions.

Resolution was a restore-style environment action only: the three pinned old-version analyzer packages were installed into the gitignored local `packages/` folder via `nuget.exe install`. No tracked source or project file was modified; `packages/` is gitignored. This drift is pre-existing, affects the whole solution (not just Threading), and is outside this feature's `.cs`-only scope; it is recorded here for the maintainer.
