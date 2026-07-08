# P5-T4 — Hard Invariant Check (Issue #181)

Timestamp: 2026-06-08T13-34

Each hard invariant is enumerated with its verification result and evidence.

## Invariant 1 — Only `.claude/rules/csharp.md` changed among `.claude/rules/`
PASS. `git status --porcelain -- .claude/rules/` returns only:
```
 M .claude/rules/csharp.md
```
No other `.claude/rules/*` file was modified or created.

## Invariant 2 — No Central Package Management / quality-tiers / globalconfig artifacts created
PASS. `Directory.Packages.props`, `quality-tiers.yml`, and `.globalconfig` are absent and were not created (checked both filesystem and `git status`). The csharp.md analyzer-stack section explicitly states no PackageReference, no Central Package Management, and no `dotnet restore` are introduced.

## Invariant 3 — Test framework, coverage thresholds, build/test commands unchanged
PASS. `.claude/rules/csharp.md` still specifies:
- MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`), Moq, FluentAssertions.
- Repository-wide line coverage `>= 80%`; new module/class/method `>= 90%`. No 85/75 thresholds, no branch-coverage requirement introduced.
- `msbuild TaskMaster.sln ...` for lint/type-check and `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` for testing. No 7-stage toolchain introduced; the documented order remains format → lint → type-check → test.
- No COM/VSTO ban introduced.

## Invariant 4 — No remaining SecurityCodeScan reference in first-party packages.config / .csproj / .editorconfig
PASS. Repo-wide search for `SecurityCodeScan` across `*.csproj`, `packages.config`, and `.editorconfig` (excluding vendored SVGControl/UtilitiesSwordfish and the `docs/features` evidence tree) returns no matches in first-party build config.

## Invariant 5 — No remaining sibling `YamlDotNet.dll` `<Analyzer>` entry
PASS. Repo-wide search for `YamlDotNet.dll` across `*.csproj` returns no matches.

## Invariant 6 — No CS8032 suppression anywhere
PASS.
- No `dotnet_diagnostic.CS8032` line exists in `.editorconfig` (or any config file).
- No `<WarningsNotAsErrors>` entry containing `CS8032` exists in any `*.csproj` / `*.props`.
- Repo-wide search for `CS8032` across `*.csproj`, `*.props`, `*.targets`, `.editorconfig` (excluding the `docs/features` evidence tree which documents the CS8032 root cause) returns no matches.

## Invariant 7 — Change scope confined to build-config + rules/csharp.md + .editorconfig + per-project analyzer refs + BannedSymbols.txt
PASS. `git status` (excluding `docs/features` plan/evidence and `.claude/agent-memory`) shows:
- `.claude/rules/csharp.md` (Phase 5 documentation).
- `.editorconfig` (SCS severity removal).
- 15 first-party `.csproj` (SecurityCodeScan/YamlDotNet `<Analyzer Include>` removal; 5-analyzer set retained).
- 15 first-party `packages.config` (SecurityCodeScan `<package>` removal; 5-analyzer set retained). `VBFunctions/packages.config` is newly tracked (created in plan v1.0 P3-T15 for a first-party project that previously had no packages.config).
- `BannedSymbols.txt` (repo root, Phase 3).
- No application source `.cs` files were modified.

## Cleanup note — stray Phase 1 scratch files removed
Five non-canonical scratch files (`evidence_ids_BannedApi.txt`, `evidence_ids_Meziantou.txt`, `evidence_ids_Roslynator.txt`, `evidence_ids_Sonar.txt`, `evidence_ids_Sonar_strings.txt`) were left at the repo root by a prior session's Phase 1 rule-ID discovery. They are throwaway scratch, not referenced by any committed evidence artifact, and not deliverables. They were removed to keep the change scoped (build-config + rules + editorconfig + analyzer refs + BannedSymbols.txt only). The canonical Phase 1 discovery evidence lives under `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/other/`.

## Verdict
All hard invariants PASS, including the SecurityCodeScan-removal and no-CS8032-suppression checks. The change is scoped to build configuration, `.editorconfig`, per-project analyzer references, `BannedSymbols.txt`, and `.claude/rules/csharp.md`.
