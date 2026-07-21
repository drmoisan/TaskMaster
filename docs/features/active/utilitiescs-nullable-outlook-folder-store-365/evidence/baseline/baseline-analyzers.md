# Baseline Analyzer / Code-Style Build (P0-T4)

Timestamp: 2026-07-19T10-53

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (via VS18 `amd64/MSBuild.exe`, `/m`)

EXIT_CODE: 0

Output Summary: Build SUCCEEDED. 0 errors, 76 warnings (152 raw lines across the multi-project build).
No `TreatWarningsAsErrors` on this stage, so warnings are informational and do not fail the build.

Top pre-existing warning codes (raw-line counts): CS8632 (66, nullable annotation in non-nullable
context), CS0618 (56, obsolete API use), CS0108 (8, member hides inherited), CS0169/CS0067 (unused
field / unused event), MSTEST0032, CS8625, CS4014, CS2002, CS0168. All are pre-existing and unrelated
to this feature's work.

## Environment bootstrap note

A fresh worktree restore (`msbuild /t:Restore /p:RestorePackagesConfig=true`) installs the analyzer
versions pinned in each `packages.config` (Meziantou.Analyzer 3.0.123, Microsoft.CodeAnalysis.BannedApiAnalyzers
5.6.0, SonarAnalyzer.CSharp 10.29.0.143774). However, three `<Analyzer Include>` paths committed in
`UtilitiesCS/UtilitiesCS.csproj` (and `VBFunctions.csproj`) still reference older versions (Meziantou 3.0.101,
BannedApiAnalyzers 3.3.4, SonarAnalyzer 10.27.0.140913). This is a pre-existing committed inconsistency on
the integration branch tip (`dffadd5a`): the packages.config versions were bumped and the Meziantou
`<Import>`/`<Error>` lines updated to 3.0.123, but the three explicit `<Analyzer Include>` DLL paths were not.
On a developer machine with a historical `packages/` folder both version sets coexist so the build resolves;
a clean worktree only restores the packages.config versions, producing `CS0006` missing-metadata errors.

Bootstrap fix applied (environment-only; no tracked file edited): the three older analyzer package versions
referenced by the csproj were installed into the gitignored `packages/` folder via
`nuget.exe install <id> -Version <old>`. This is a mechanically-necessary environment action to make the
clean worktree match a working dev machine; the csproj analyzer references are out of this feature's scope
(not a Folder/Store file) and were not edited.
