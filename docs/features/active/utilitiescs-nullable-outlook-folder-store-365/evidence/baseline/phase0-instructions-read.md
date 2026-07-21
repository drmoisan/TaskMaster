# Phase 0 — Policy Read Receipt (P0-T1)

Timestamp: 2026-07-19T10-53

Policy Order: The required policy reading order for this repository (per `policy-compliance-order` and CLAUDE.md) is:
1. `CLAUDE.md` (standing instructions, C# toolchain section)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C#-specific toolchain and standards)

Files Read (from the current worktree root
`C:\Users\DanMoisan\repos\TaskMaster-wt\utilitiescs-nullable-outlook-folder-store-365`):
- `CLAUDE.md` — read in full.
- `.claude/rules/general-code-change.md` — read in full.
- `.claude/rules/general-unit-test.md` — read in full.
- `.claude/rules/csharp.md` — read in full.
- Feature requirements sources also read: `spec.md`, `user-story.md`, `issue.md` (via plan references), and the
  approved plan `plan.2026-07-18T22-03.md`.

Key operative constraints acknowledged for this feature:
- Per-file `#nullable enable` opt-in ONLY; do NOT add a `<Nullable>` element to `UtilitiesCS/UtilitiesCS.csproj` (AC2).
- Nullable verification gate is `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
  /p:TreatWarningsAsErrors=true` WITHOUT `/p:Nullable=enable` (deliberate, plan-mandated deviation from the general
  csharp.md type-check command for nullable verification specifically).
- The other three toolchain stages run as documented: `dotnet tool run csharpier .` (format),
  `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers), and
  `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1` (MSTest with coverage).
- No nullable post-condition attributes (net481 lacks them; not polyfilled).
- No `record`/`record struct`/`init` accessors (net481 lacks `IsExternalInit`).
- Do NOT split `FolderPredictor.cs` (974), `FolderScorer.cs` (663), `"FolderWrapper .cs"` (531).
- Do NOT pragma-annotate the two Designer-generated files.
