# Baseline Analyzer/Code-Style Build

Timestamp: 2026-07-19T00-40

Command: `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary: Build succeeded. 76 Warning(s), 0 Error(s). Warning categories observed:
CS8632 (66 — nullable annotation context, pre-existing, unrelated to this feature's
not-yet-opted-in cluster files), CS0618 (56 — obsolete-API usage, pre-existing),
CS0108/CS0169/CS0067/CS4014/CS2002/CS0168/MSTEST0032/CS8625 (remaining, all pre-existing).
None of these warnings originate from the 24 remediation-target files in
`UtilitiesCS/EmailIntelligence/{EmailParsingSorting,SubjectMap,Ctf}/` (none of those files
carry `#nullable enable` yet, so they emit no nullable-context warnings).

Environment note: this run required a one-time fix for a pre-existing analyzer-package-version
skew on this fresh worktree (documented in agent memory
`project_analyzer_version_skew_fresh_worktree.md`): the tracked `packages.config` pins newer
analyzer versions than the hand-written `<Analyzer Include>` paths in several first-party
`.csproj` files reference. Fixed by installing the older analyzer package versions into the
gitignored `packages/` folder (no tracked file changed):
`nuget install Meziantou.Analyzer -Version 3.0.101 -OutputDirectory packages`,
`nuget install Microsoft.CodeAnalysis.BannedApiAnalyzers -Version 3.3.4 -OutputDirectory packages`,
`nuget install SonarAnalyzer.CSharp -Version 10.27.0.140913 -OutputDirectory packages`.
