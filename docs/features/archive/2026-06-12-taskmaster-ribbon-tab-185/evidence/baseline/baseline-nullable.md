# Baseline — Nullable / Type-Check Build (Issue #185)

Timestamp: 2026-06-12T10-40

Command: msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true

Note: `-t:Rebuild` (not `-t:Build`) is used for this forced-nullable gate. Incremental
`-t:Build` skips recompilation of unchanged assemblies, which would mean the forced
`Nullable=enable`/`TreatWarningsAsErrors` flags are never exercised against the source. Rebuild
forces a full recompile so the gate actually runs.

EXIT_CODE: 1

Output Summary: Build fails with 84 Error(s), 0 Warning(s). All 84 errors originate
exclusively from two vendored projects:
- SVGControl/SVGControl.csproj — 68 errors (CS8618/CS8603/CS8602/CS8600/CS8601/CS8625/CS0649)
- UtilitiesSwordfish/UtilitiesSwordfish.NET.General.csproj — 16 errors

Per `.claude/rules/csharp.md`, SVGControl and UtilitiesSwordfish.NET.General are vendored
projects explicitly excluded from this repository's analyzer/null-safety standards. The
forced solution-wide `Nullable=enable + TreatWarningsAsErrors` flags promote pre-existing
vendored null-flow warnings to errors. This is the established repository baseline and is
NOT introduced by issue #185.

Scope confirmation: Zero errors originate from the in-scope first-party project
TaskMaster.Test or from any first-party project. The in-scope changes touch
TaskMaster/Ribbon/RibbonExplorer.xml (non-compiled XML resource) and
TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs (first-party). A targeted rebuild of
TaskMaster.Test under the same forced flags produces no errors in TaskMaster.Test itself;
the only errors surfaced are the same pre-existing vendored SVGControl errors pulled in as
a build dependency. The feature's changed code introduces no nullable/type-check regression.
