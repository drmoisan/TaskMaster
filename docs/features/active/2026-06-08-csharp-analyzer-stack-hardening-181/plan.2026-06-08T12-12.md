# csharp-analyzer-stack-hardening — Plan

- **Issue:** #181
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-08T12-12
- **Status:** Draft
- **Version:** 2.0 (revised to resolve P4-T16 blocking finding: SecurityCodeScan.VS2019 dropped)
- **Work Mode:** full-feature

## Required References

- Policy compliance order: `.claude/skills/policy-compliance-order/SKILL.md`
- CLAUDE.md toolchain sections (C# Toolchain; C# Code Change Policy; C# Unit Test Policy)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `.claude/rules/ci-workflows.md`
- Feasibility research (authoritative mechanism): `artifacts/research/2026-06-08-csharp-analyzer-stack-feasibility-181.md`
- Authoritative CI gate: `.github/workflows/ci.yml`

**All work must comply with these policies; do not duplicate their content here.**

## Scope Definitions (fixed inputs for this plan)

- **First-party projects (15) — analyzers APPLY:** QuickFiler, QuickFiler.Test, Tags, Tags.Test, TaskMaster, TaskMaster.Test, TaskTree, TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS, UtilitiesCS.Test, VBFunctions, VBFunctions.Test.
- **Vendored projects (4) — EXCLUDED:** SVGControl (`SVGControl\SVGControl.csproj`), SVGControl.Test (`SVGControl.Test\SVGControl.Test.csproj`), UtilitiesSwordfish (`UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj`), UtilitiesSwordfish.Test (`UtilitiesSwordfish.Test\UtilitiesSwordfish.NET.Test.csproj`).
- **Analyzer packages (5):** Meziantou.Analyzer, SonarAnalyzer.CSharp, Roslynator.Analyzers, AsyncFixer, Microsoft.CodeAnalysis.BannedApiAnalyzers.
- **Banned symbols (5 targets):** System.DateTime.Now, System.DateTime.UtcNow, System.Random.Shared, System.Threading.Thread.Sleep, System.Threading.Tasks.Task.Delay.
- **DEFERRED analyzer (NOT in this rollout):** SecurityCodeScan.VS2019. During execution at P4-T16 it was found incompatible with this repo's Roslyn 5.6 (VS18) analyzer loader: its types fail to initialize (`TypeInitializationException` -> `FileNotFoundException` for `YamlDotNet, Version=11.0.0.0`), emitting compiler warning **CS8032**. CS8032 is a compiler warning, not an analyzer rule, so it cannot be set to `suggestion` via `.editorconfig`; under the CI `/p:Nullable=enable /p:TreatWarningsAsErrors=true` build it is promoted to an error (+16 errors: 84 baseline -> 100). Evidence: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/p4-build-no-regression.2026-06-08T12-12.md`. It is dropped entirely (no CS8032 suppression, no substitute security analyzer) and recorded as a documented deferral.

## Critical Ordering Invariant (protects the nullable CI step)

The nullable build step runs `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`. `TreatWarningsAsErrors=true` promotes any analyzer diagnostic at `warning` severity to a build error. Therefore the plan is sequenced so that:

1. All new analyzer rule IDs are added to `.editorconfig` at `severity = suggestion` (RS0030 included, never `warning`/`error`) in Phase 2, BEFORE any `<Analyzer Include>` items are added to any `.csproj` (Phase 4).
2. `nuget restore` package downloads and analyzer wiring (Phase 3 and Phase 4) only proceed after severities are committed and verified.

This ordering is structurally enforced by the phase numbering. No `<Analyzer Include>` item may be added in or before Phase 3.

## Revision-2 State and Cleanup Note (SecurityCodeScan removal)

The prior execution (plan v1.0) reached the P4-T16 protected gate and stopped. As a result, the following SecurityCodeScan.VS2019 artifacts are ALREADY on disk and MUST be removed by this revised plan before the protected nullable gate can hold at the 84-error baseline:

- A SecurityCodeScan.VS2019 `<package>` entry in each of the 15 first-party `packages.config` files (added under Phase 3 v1.0).
- A SecurityCodeScan `<Analyzer Include>` item AND a co-located sibling `YamlDotNet.dll` `<Analyzer Include>` item in each of the 15 first-party `.csproj` files (added under Phase 4 v1.0). Both must be removed.
- SecurityCodeScan rule-severity lines (`dotnet_diagnostic.SCS*.severity = suggestion`) added to `.editorconfig` under Phase 2 v1.0.

Because the `.editorconfig` severities for the remaining 5 analyzers are already committed and were verified non-regressing at P2-T9 (v1.0), the ordering invariant remains satisfied: the only action that returns the protected gate to baseline is removing the SecurityCodeScan wiring. The revised plan therefore frames Phase 2/3/4 cleanup tasks so that the protected nullable gate is verified clean (84-error baseline, no regression) AFTER SecurityCodeScan removal. No `<Analyzer Include>` for any analyzer is added ahead of its `.editorconfig` severity. No CS8032 suppression is introduced under any circumstances.

## Toolchain Loop (run for each implementation task; restart from step 1 on any failure or file change)

1. `dotnet tool restore`
2. `dotnet tool run csharpier .` (local) / `dotnet csharpier check .` (CI verification form)
3. `nuget restore TaskMaster.sln`
4. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
5. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`  (MUST NOT REGRESS — central risk)
6. `vstest.console.exe <built *.Test.dll paths> /EnableCodeCoverage /InIsolation /Logger:trx`

## Evidence Location Invariant

All evidence artifacts produced by this plan MUST be written under:
`docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/<kind>/`
using canonical sub-paths (`baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/`, `other/`). Writing to any `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or other non-canonical evidence path is a policy violation. Timestamps use `yyyy-MM-ddTHH-mm`.

---

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Reading

- [x] [P0-T1] Read policy files in required order and record evidence
  - Files: read `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/ci-workflows.md`, `.claude/skills/policy-compliance-order/SKILL.md`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact contains `Timestamp:`, `Policy Order:`, and an explicit list of files read

- [x] [P0-T2] Capture baseline CSharpier formatting state
  - Run: `dotnet tool restore` then `dotnet csharpier check .`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-format.2026-06-08T12-12.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (clean/dirty state)

- [x] [P0-T3] Capture baseline solution restore state
  - Run: `nuget restore TaskMaster.sln`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-restore.2026-06-08T12-12.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (restore succeeded/failed)

- [x] [P0-T4] Capture baseline analyzer/code-style build state
  - Run: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-analyzer-build.2026-06-08T12-12.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (warning/error counts)

- [x] [P0-T5] Capture baseline nullable TreatWarningsAsErrors build state (the protected step)
  - Run: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-nullable-build.2026-06-08T12-12.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; records baseline EXIT_CODE as the no-regression reference

- [x] [P0-T6] Capture baseline MSTest coverage state
  - Run: `vstest.console.exe <built *.Test.dll paths> /EnableCodeCoverage /InIsolation /Logger:trx`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric repo-wide line coverage percent (baseline reference for the 80%/90% gates)

### Phase 1 — Package Version and Analyzer DLL Path Discovery (resolve open unknowns before any edits)

- [x] [P1-T1] Determine the exact compatible package version for Meziantou.Analyzer
  - Run: `nuget install Meziantou.Analyzer -OutputDirectory packages -ExcludeVersion:false` (or inspect the restored `packages/` folder) against this repo's VS2022/Roslyn toolchain
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/other/package-versions.2026-06-08T12-12.md` (append section: Meziantou.Analyzer)
  - Acceptance: artifact records resolved version string and `Timestamp:`, `Command:`, `EXIT_CODE:`

- [x] [P1-T2] Determine the exact compatible package version for SonarAnalyzer.CSharp
  - Run: install/inspect SonarAnalyzer.CSharp
  - Write: append section "SonarAnalyzer.CSharp" to `evidence/other/package-versions.2026-06-08T12-12.md`
  - Acceptance: resolved version recorded with command and exit code

- [x] [P1-T3] Determine the exact compatible package version for Roslynator.Analyzers
  - Run: install/inspect Roslynator.Analyzers
  - Write: append section "Roslynator.Analyzers" to `evidence/other/package-versions.2026-06-08T12-12.md`
  - Acceptance: resolved version recorded with command and exit code

- [x] [P1-T4] Determine the exact compatible package version for AsyncFixer (verify 1.6.0 loads under Roslyn 4.x)
  - Run: install/inspect AsyncFixer; confirm the analyzer DLL targets a Roslyn-4.x-compatible loader
  - Write: append section "AsyncFixer" to `evidence/other/package-versions.2026-06-08T12-12.md`
  - Acceptance: resolved version recorded; note whether the DLL loads under VS2022 build tools (yes/no), with command and exit code

- [x] [P1-T5] Determine the exact compatible package version for Microsoft.CodeAnalysis.BannedApiAnalyzers
  - Run: install/inspect Microsoft.CodeAnalysis.BannedApiAnalyzers
  - Write: append section "BannedApiAnalyzers" to `evidence/other/package-versions.2026-06-08T12-12.md`
  - Acceptance: resolved version recorded with command and exit code

- [x] [P1-T6] Enumerate the exact analyzer DLL relative paths inside each package's `analyzers/dotnet/cs/` folder (5 packages only)
  - Run: directory listing of each restored package's `analyzers/dotnet/cs/` folder for the 5 in-scope analyzers (capture all DLLs, including SonarAnalyzer and Roslynator multi-DLL sets); use Meziantou `roslyn5.0` and Roslynator `roslyn4.7` subfolders for this repo's Roslyn 5.6
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/other/analyzer-dll-paths.2026-06-08T12-12.md`
  - Acceptance: artifact lists, per package, the exact `..\packages\<id>.<version>\analyzers\dotnet\cs\<dll>` relative path(s) to be used in `<Analyzer Include>` items; no SecurityCodeScan/YamlDotNet path is listed; contains `Timestamp:` and `Command:`

- [x] [P1-T7] Measure the volume of existing banned-symbol violations (DateTime.Now/UtcNow, Random.Shared, Thread.Sleep, Task.Delay) across first-party source
  - Run: a read-only source search (e.g., Grep) across the 15 first-party project directories for each banned symbol
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/other/banned-symbol-volume.2026-06-08T12-12.md`
  - Acceptance: artifact records the per-symbol count; states that the count confirms `RS0030 = suggestion` is required at initial rollout and that legacy cleanup is documented as out-of-scope/follow-up; contains `Timestamp:`

### Phase 2 — `.editorconfig` Severities First (lands before any analyzer wiring)

- [x] [P2-T1] Enumerate the new analyzer rule IDs to be configured and record the severity map (5 packages)
  - Source: the analyzer rule IDs emitted by the five in-scope packages plus RS0030 (BannedApiAnalyzers)
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/other/editorconfig-severity-map.2026-06-08T12-12.md`
  - Acceptance: artifact lists every rule ID prefix to be added (MA*, S*, RCS*, AF*, RS0030) with target severity `suggestion`; SCS* is NOT included; contains `Timestamp:`

- [x] [P2-T2] Add Meziantou.Analyzer rule severities to `.editorconfig` at `severity = suggestion`
  - Edit: `.editorconfig` (`[*.cs]` section), add `dotnet_diagnostic.<MA-id>.severity = suggestion` entries per the severity map
  - Acceptance: each added Meziantou ID is set to `suggestion`; no ID set to `warning` or `error`; file remains a valid `.editorconfig`

- [x] [P2-T3] Add SonarAnalyzer.CSharp rule severities to `.editorconfig` at `severity = suggestion`
  - Edit: `.editorconfig`, add `dotnet_diagnostic.<S-id>.severity = suggestion` entries
  - Acceptance: each added Sonar ID is set to `suggestion`; none at `warning`/`error`

- [x] [P2-T4] Add Roslynator.Analyzers rule severities to `.editorconfig` at `severity = suggestion`
  - Edit: `.editorconfig`, add `dotnet_diagnostic.<RCS-id>.severity = suggestion` entries
  - Acceptance: each added Roslynator ID is set to `suggestion`; none at `warning`/`error`

- [x] [P2-T5] Add AsyncFixer rule severities to `.editorconfig` at `severity = suggestion`
  - Edit: `.editorconfig`, add `dotnet_diagnostic.<AF-id>.severity = suggestion` entries
  - Acceptance: each added AsyncFixer ID is set to `suggestion`; none at `warning`/`error`

- [x] [P2-T6] Add `dotnet_diagnostic.RS0030.severity = suggestion` (BannedApiAnalyzers) to `.editorconfig`
  - Edit: `.editorconfig`, add the RS0030 entry at `suggestion` with an in-file comment that promotion to `warning` is a post-cleanup follow-up
  - Acceptance: RS0030 is present at `suggestion` exactly; never `warning`/`error`

- [x] [P2-T7] Add naming preference rules to `.editorconfig` at non-error severity
  - Edit: `.editorconfig`, add `_camelCase` private-field, `I`-prefixed interface, and `Async`-suffix naming preferences at `suggestion`/`silent` severity
  - Acceptance: each naming rule added at `suggestion` or `silent`; no naming rule at `warning`/`error`; existing `csharp_style_namespace_declarations = block_scoped:silent` is unchanged (not flipped to file-scoped enforcement)

- [x] [P2-T8] Remove the SecurityCodeScan rule-severity lines from `.editorconfig` (revision-2 cleanup)
  - Edit: `.editorconfig` (`[*.cs]` section), delete every `dotnet_diagnostic.SCS*.severity = suggestion` line added under plan v1.0; leave the 5 in-scope analyzers' severities (MA*, S*, RCS*, AF*, RS0030) intact
  - Acceptance: no `SCS*` diagnostic-severity line remains in `.editorconfig`; the MA*/S*/RCS*/AF*/RS0030 severities and the naming/namespace preferences are unchanged; file remains a valid `.editorconfig`

- [x] [P2-T9] Verify the `.editorconfig` change (SCS severities removed, 5-analyzer severities retained) does not regress the build via the full toolchain loop
  - Run: toolchain steps 1-6 against current on-disk state. NOTE: the 15 first-party `.csproj` files still contain SecurityCodeScan/YamlDotNet `<Analyzer Include>` items at this point, so the nullable step (step 5) is expected to still show the +16 CS8032 regression until Phase 4 cleanup completes; record this explicitly. The analyzer/code-style step (step 4) must show no new errors and the 5 in-scope analyzers active.
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/p2-severities-toolchain.2026-06-08T12-12.md`
  - Acceptance: artifact records each command, `EXIT_CODE:`, and `Output Summary:`; states that CS8032 cannot be controlled via `.editorconfig` and will be eliminated by removing SecurityCodeScan `<Analyzer Include>` items in Phase 4 (P4-T16 cleanup); the protected-gate no-regression assertion is deferred to the post-cleanup verification in P4-T16; contains `Timestamp:`

### Phase 3 — BannedSymbols.txt and packages.config Analyzer Entries (5 packages; remove SecurityCodeScan)

> Plan v1.0 added 6 analyzer `<package>` entries (including SecurityCodeScan.VS2019) to each of the 15 first-party `packages.config` files. Revision-2 reduces each file to the 5 in-scope analyzers by REMOVING the SecurityCodeScan.VS2019 `<package>` entry. BannedSymbols.txt (P3-T1) is already correct and is retained unchanged.

- [x] [P3-T1] Create `BannedSymbols.txt` at the repo root with the 5 banned-symbol target entries
  - Write: `BannedSymbols.txt` (repo root) using BannedApiAnalyzers doc-ID syntax for DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep (Int32 and TimeSpan overloads), Task.Delay (Int32 and TimeSpan overloads)
  - Acceptance: file exists at repo root; each entry uses correct `P:`/`M:` doc-ID syntax with a remediation message; covered symbols match the 5 targets

- [x] [P3-T2] Remove the SecurityCodeScan.VS2019 `<package>` entry from QuickFiler `packages.config` (leave 5 analyzer entries)
  - Edit: `QuickFiler\packages.config`, delete the `<package id="SecurityCodeScan.VS2019" ... />` line
  - Acceptance: no SecurityCodeScan entry remains; the 5 in-scope analyzer entries (Meziantou.Analyzer, SonarAnalyzer.CSharp, Roslynator.Analyzers, AsyncFixer, Microsoft.CodeAnalysis.BannedApiAnalyzers) remain with `developmentDependency="true"`; file is valid XML

- [x] [P3-T3] Remove the SecurityCodeScan.VS2019 `<package>` entry from QuickFiler.Test `packages.config` (leave 5)
  - Edit: `QuickFiler.Test\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T4] Remove the SecurityCodeScan.VS2019 `<package>` entry from Tags `packages.config` (leave 5)
  - Edit: `Tags\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T5] Remove the SecurityCodeScan.VS2019 `<package>` entry from Tags.Test `packages.config` (leave 5)
  - Edit: `Tags.Test\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T6] Remove the SecurityCodeScan.VS2019 `<package>` entry from TaskMaster `packages.config` (leave 5)
  - Edit: `TaskMaster\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T7] Remove the SecurityCodeScan.VS2019 `<package>` entry from TaskMaster.Test `packages.config` (leave 5)
  - Edit: `TaskMaster.Test\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T8] Remove the SecurityCodeScan.VS2019 `<package>` entry from TaskTree `packages.config` (leave 5)
  - Edit: `TaskTree\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T9] Remove the SecurityCodeScan.VS2019 `<package>` entry from TaskVisualization `packages.config` (leave 5)
  - Edit: `TaskVisualization\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T10] Remove the SecurityCodeScan.VS2019 `<package>` entry from TaskVisualization.Test `packages.config` (leave 5)
  - Edit: `TaskVisualization.Test\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T11] Remove the SecurityCodeScan.VS2019 `<package>` entry from ToDoModel `packages.config` (leave 5)
  - Edit: `ToDoModel\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T12] Remove the SecurityCodeScan.VS2019 `<package>` entry from ToDoModel.Test `packages.config` (leave 5)
  - Edit: `ToDoModel.Test\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T13] Remove the SecurityCodeScan.VS2019 `<package>` entry from UtilitiesCS `packages.config` (leave 5)
  - Edit: `UtilitiesCS\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T14] Remove the SecurityCodeScan.VS2019 `<package>` entry from UtilitiesCS.Test `packages.config` (leave 5)
  - Edit: `UtilitiesCS.Test\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T15] Remove the SecurityCodeScan.VS2019 `<package>` entry from VBFunctions `packages.config` (leave 5)
  - Edit: `VBFunctions\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T16] Remove the SecurityCodeScan.VS2019 `<package>` entry from VBFunctions.Test `packages.config` (leave 5)
  - Edit: `VBFunctions.Test\packages.config`
  - Acceptance: as P3-T2 for this project

- [x] [P3-T17] Verify the solution restores cleanly with the 5-analyzer packages.config entries via `nuget restore`
  - Run: `nuget restore TaskMaster.sln`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/p3-restore.2026-06-08T12-12.md`
  - Acceptance: `EXIT_CODE: 0`; artifact confirms the 5 in-scope analyzer packages are present in `packages/` and that no `packages.config` references SecurityCodeScan.VS2019; contains `Timestamp:`, `Command:`, `Output Summary:`

- [x] [P3-T18] Confirm the 4 vendored projects' packages.config files were NOT modified
  - Verify: `SVGControl\packages.config`, `SVGControl.Test\packages.config`, `UtilitiesSwordfish\packages.config` (if present), `UtilitiesSwordfish.Test\packages.config` contain no analyzer package entries
  - Write: append "Vendored exclusion check" to `evidence/qa-gates/p3-restore.2026-06-08T12-12.md`
  - Acceptance: artifact records that no vendored packages.config was changed

### Phase 4 — Remove SecurityCodeScan/YamlDotNet `<Analyzer Include>` Items; Retain the 5-Analyzer Wiring in First-Party .csproj Files

> Plan v1.0 wired an `<ItemGroup>` into each of the 15 first-party `.csproj` files containing 6 analyzer `<Analyzer Include="..\packages\<id>.<version>\analyzers\dotnet\cs\<dll>" />` item sets PLUS a co-located `YamlDotNet.dll` `<Analyzer Include>` (the attempted SecurityCodeScan dependency reference) PLUS `<AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />`. Revision-2 work in each task below is to REMOVE the SecurityCodeScan `<Analyzer Include>` item AND the co-located `YamlDotNet.dll` `<Analyzer Include>` item, while RETAINING the 5 in-scope analyzer DLL sets (Meziantou, SonarAnalyzer, Roslynator, AsyncFixer, BannedApiAnalyzers, including SonarAnalyzer/Roslynator multi-DLL sets) and the `BannedSymbols.txt` AdditionalFiles entry. The retained 5-analyzer severities were committed in Phase 2 and verified non-regressing for the 5-analyzer set, so the ordering invariant holds. Removing the SecurityCodeScan/YamlDotNet items is the action that eliminates the CS8032 load failures and returns the protected nullable gate to its 84-error baseline.

- [x] [P4-T1] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `QuickFiler\QuickFiler.csproj` (retain the 5-analyzer set)
  - Edit: `QuickFiler\QuickFiler.csproj`
  - Acceptance: no `SecurityCodeScan` and no `YamlDotNet.dll` `<Analyzer Include>` item remains; the 5 in-scope analyzer DLL paths and the `BannedSymbols.txt` AdditionalFiles entry remain and match the discovery artifact; file is valid MSBuild XML

- [x] [P4-T2] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `QuickFiler.Test\QuickFiler.Test.csproj` (retain the 5-analyzer set)
  - Edit: `QuickFiler.Test\QuickFiler.Test.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T3] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `Tags\Tags.csproj` (retain the 5-analyzer set)
  - Edit: `Tags\Tags.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T4] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `Tags.Test\Tags.Test.csproj` (retain the 5-analyzer set)
  - Edit: `Tags.Test\Tags.Test.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T5] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `TaskMaster\TaskMaster.csproj` (retain the 5-analyzer set)
  - Edit: `TaskMaster\TaskMaster.csproj`
  - Acceptance: as P4-T1 for this project; this is the VSTO/COM interop project, verified separately in P4-T17

- [x] [P4-T6] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `TaskMaster.Test\TaskMaster.Test.csproj` (retain the 5-analyzer set)
  - Edit: `TaskMaster.Test\TaskMaster.Test.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T7] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `TaskTree\TaskTree.csproj` (retain the 5-analyzer set)
  - Edit: `TaskTree\TaskTree.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T8] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `TaskVisualization\TaskVisualization.csproj` (retain the 5-analyzer set)
  - Edit: `TaskVisualization\TaskVisualization.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T9] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `TaskVisualization.Test\TaskVisualization.Test.csproj` (retain the 5-analyzer set)
  - Edit: `TaskVisualization.Test\TaskVisualization.Test.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T10] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `ToDoModel\ToDoModel.csproj` (retain the 5-analyzer set)
  - Edit: `ToDoModel\ToDoModel.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T11] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `ToDoModel.Test\ToDoModel.Test.csproj` (retain the 5-analyzer set)
  - Edit: `ToDoModel.Test\ToDoModel.Test.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T12] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `UtilitiesCS\UtilitiesCS.csproj` (retain the 5-analyzer set)
  - Edit: `UtilitiesCS\UtilitiesCS.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T13] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `UtilitiesCS.Test\UtilitiesCS.Test.csproj` (retain the 5-analyzer set)
  - Edit: `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T14] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `VBFunctions\VBFunctions.csproj` (retain the 5-analyzer set)
  - Edit: `VBFunctions\VBFunctions.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T15] Remove SecurityCodeScan + YamlDotNet `<Analyzer Include>` items from `VBFunctions.Test\VBFunctions.Test.csproj` (retain the 5-analyzer set)
  - Edit: `VBFunctions.Test\VBFunctions.Test.csproj`
  - Acceptance: as P4-T1 for this project

- [x] [P4-T16] Verify the analyzer/code-style build passes and the nullable TreatWarningsAsErrors build returns to the 84-error baseline (no regression) after SecurityCodeScan removal
  - Run: toolchain steps 1-5 (format, restore, analyzer build, nullable build) on a rebuild that recompiles the first-party projects
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/p4-build-no-regression.2026-06-08T12-12.md` (append a revision-2 post-cleanup section; do not overwrite the v1.0 blocking-finding record)
  - Acceptance: analyzer/code-style build (step 4) succeeds with 0 errors and shows NO CS8032 instances (SecurityCodeScan no longer loaded); nullable step (step 5) `EXIT_CODE:` and error count equal the Phase 0 baseline (P0-T5 = 84 errors, all in vendored projects) — the +16 CS8032 regression is eliminated; artifact records each command, `EXIT_CODE:`, `Output Summary:`, and `Timestamp:`

- [x] [P4-T17] Verify Meziantou and the other 4 in-scope analyzers do not produce build-breaking diagnostics against the VSTO/COM interop in `TaskMaster.csproj`
  - Run: inspect the analyzer build output (from P4-T16 post-cleanup) filtered to TaskMaster project diagnostics; confirm all are at `suggestion` (message) level, not error
  - Write: append "TaskMaster VSTO/COM diagnostic check" to `evidence/qa-gates/p4-build-no-regression.2026-06-08T12-12.md`
  - Acceptance: artifact records that no analyzer diagnostic from the 5 in-scope packages is promoted to error in TaskMaster; confirms suggestion-severity mitigation held

- [x] [P4-T18] Verify BannedApiAnalyzers RS0030 fires at suggestion level against at least one known banned-symbol call site
  - Run: inspect the analyzer build output for an RS0030 message on an existing DateTime.Now/UtcNow/Thread.Sleep/Task.Delay usage identified in P1-T7
  - Write: append "RS0030 activation check" to `evidence/qa-gates/p4-build-no-regression.2026-06-08T12-12.md`
  - Acceptance: artifact shows at least one RS0030 message emitted at suggestion severity, confirming BannedSymbols.txt doc-IDs are correct and the rule is active (not a build break)

### Phase 5 — Documentation: TimeProvider Guidance and Mechanism in rules/csharp.md

- [x] [P5-T1] Add TimeProvider/FakeTimeProvider seam guidance to `.claude/rules/csharp.md`
  - Edit: `.claude/rules/csharp.md` (DI Seams or Deterministic Test Rules section)
  - Content: new/touched time-dependent code injects `System.TimeProvider` via constructor; production supplies `TimeProvider.System`; tests supply `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`; do not call DateTime.Now/UtcNow/DateTimeOffset.Now directly; existing call sites are NOT rewritten by this change
  - Acceptance: guidance present and explicitly states it is guidance-only with no runtime behavior change; references that `Microsoft.Bcl.TimeProvider` is already present in UtilitiesCS

- [x] [P5-T2] Document the adopted 5-analyzer-stack mechanism in `.claude/rules/csharp.md` while retaining existing policy
  - Edit: `.claude/rules/csharp.md`
  - Content: document the packages.config + `<Analyzer Include>` mechanism for the adopted FIVE analyzers (Meziantou.Analyzer, SonarAnalyzer.CSharp, Roslynator.Analyzers, AsyncFixer, Microsoft.CodeAnalysis.BannedApiAnalyzers), first-party-only scope, BannedSymbols.txt, and severity-first ordering; retain MSTest/Moq/FluentAssertions, 80/90 line coverage, msbuild + vstest commands unchanged
  - Acceptance: rules/csharp.md documents the 5 in-scope analyzers; still states MSTest/Moq, 80% repo / 90% new line coverage (no 85/75, no branch coverage), msbuild + vstest.console.exe; no CPM, no quality-tiers, no 7-stage toolchain, no COM/VSTO bans introduced

- [x] [P5-T3] Record the SecurityCodeScan.VS2019 deferral as a documented decision in `.claude/rules/csharp.md`
  - Edit: `.claude/rules/csharp.md` (analyzer-stack section), add a short "Deferred analyzer" note
  - Content: state that SecurityCodeScan.VS2019 was evaluated and deferred (not silently omitted) because version 5.6.7 is incompatible with this repo's Roslyn 5.6 analyzer loader — it emits compiler warning CS8032 (analyzer instance cannot be created; `FileNotFoundException` for `YamlDotNet, Version=11.0.0.0`), and CS8032 is a compiler warning that cannot be set to `suggestion` via `.editorconfig`, so under `/p:TreatWarningsAsErrors=true` it breaks the protected nullable gate; re-evaluation is a follow-up pending a Roslyn-5.x-compatible security analyzer; no CS8032 suppression was introduced
  - Acceptance: the deferral note is present, identifies the package, the CS8032 root cause, and that it is a documented follow-up; the note does not introduce any suppression directive

- [x] [P5-T4] Verify no other `.claude/rules/` file was modified, SecurityCodeScan is fully removed, and all hard invariants are intact
  - Verify: `git status` shows only `.claude/rules/csharp.md` changed among `.claude/rules/`; no `Directory.Packages.props`, no `quality-tiers.yml`, no `.globalconfig` created; test framework, coverage thresholds, and build/test commands unchanged; a repo-wide search confirms no remaining reference to `SecurityCodeScan` or the sibling `YamlDotNet.dll` `<Analyzer>` entry in any first-party `packages.config`, `.csproj`, or `.editorconfig`; no CS8032 suppression (no `dotnet_diagnostic.CS8032` line, no `<WarningsNotAsErrors>` containing CS8032) exists anywhere
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/other/invariant-check.2026-06-08T12-12.md`
  - Acceptance: artifact enumerates each hard invariant with PASS and the verification evidence, including the SecurityCodeScan-removal and no-CS8032-suppression checks; contains `Timestamp:`

### Phase 6 — Final QA Loop and Acceptance Criteria Verification

- [x] [P6-T1] Run CSharpier formatting as the first final-QA step
  - Run: `dotnet tool restore` then `dotnet csharpier check .`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-format.2026-06-08T12-12.md`
  - Acceptance: `EXIT_CODE: 0`; artifact contains `Timestamp:`, `Command:`, `Output Summary:`; if files changed, restart from this step

- [x] [P6-T2] Run the solution restore as the final-QA restore step
  - Run: `nuget restore TaskMaster.sln`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-restore.2026-06-08T12-12.md`
  - Acceptance: `EXIT_CODE: 0`; artifact contains `Timestamp:`, `Command:`, `Output Summary:`

- [x] [P6-T3] Run the analyzer/code-style build as the final-QA lint step
  - Run: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-analyzer-build.2026-06-08T12-12.md`
  - Acceptance: `EXIT_CODE: 0`; artifact contains `Timestamp:`, `Command:`, `Output Summary:` (new analyzer diagnostics present as messages, none as errors)

- [x] [P6-T4] Run the nullable TreatWarningsAsErrors build as the final-QA type-check step (no-regression gate)
  - Run: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-nullable-build.2026-06-08T12-12.md`
  - Acceptance: `EXIT_CODE:` equals the Phase 0 baseline (P0-T5) — no regression; artifact contains `Timestamp:`, `Command:`, `Output Summary:`

- [x] [P6-T5] Run the MSTest suite with coverage as the final-QA test step
  - Run: `vstest.console.exe <built *.Test.dll paths> /EnableCodeCoverage /InIsolation /Logger:trx`
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-test-coverage.2026-06-08T12-12.md`
  - Acceptance: `EXIT_CODE: 0`; artifact records numeric repo-wide line coverage percent in `Output Summary:`; contains `Timestamp:`, `Command:`

- [x] [P6-T6] Verify coverage thresholds and no-regression against baseline
  - Compare: P6-T5 coverage vs. P0-T6 baseline coverage
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/coverage-delta.2026-06-08T12-12.md`
  - Acceptance: artifact reports baseline coverage, post-change coverage, and changed-code coverage; repo-wide line coverage remains >= 80%; any new code (compile-required seams, if added) reaches >= 90%; changed lines show no coverage regression; contains `Timestamp:`

- [x] [P6-T7] Verify AC1–AC8 and record the final acceptance summary
  - Verify each acceptance criterion against the evidence artifacts produced above:
    - AC1: the 5 in-scope analyzer packages referenced by all 15 first-party projects and restore clean via `nuget restore`; SecurityCodeScan.VS2019 not referenced (P3-T17, P6-T2)
    - AC2: BannedApiAnalyzers + BannedSymbols.txt active; the 5 banned SYMBOLS (DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay) flagged in new/touched code (P3-T1, P4-T18) — note the 5 banned symbols are independent of the 5-analyzer count
    - AC3: TimeProvider seam guidance in rules/csharp.md, no runtime change (P5-T1)
    - AC4: `.editorconfig` severities (5 analyzers) + naming + namespace preference scoped to avoid build-break; SCS severities removed (P2-T2..P2-T8)
    - AC5: four toolchain stages pass locally to environment extent; nullable step returns to the 84-error baseline with no regression after SecurityCodeScan removal (P6-T1..P6-T5)
    - AC6: PR CI green expectation documented (build/restore/nullable/coverage all green locally; CI parity confirmed by matching commands)
    - AC7: no hard invariant violated; the SecurityCodeScan.VS2019 deferral is recorded as a documented adaptation (per the issue's "adapted so it builds cleanly with zero new build/CI failures" mandate), NOT an invariant violation; rules/csharp.md updated retaining MSTest/Moq, 80/90 coverage, msbuild+vstest (P5-T2, P5-T3, P5-T4)
    - AC8: change scoped to build-config + rules/csharp.md + `.editorconfig` + per-project analyzer refs + BannedSymbols.txt; no app logic changes except compile-required seams (P5-T4 invariant check + `git status` review)
  - Write: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/acceptance-summary.2026-06-08T12-12.md`
  - Acceptance: each AC marked PASS with its supporting artifact path; AC7 explicitly records the SecurityCodeScan deferral as an authorized adaptation with the CS8032 root cause; if any AC lacks supporting evidence, verdict is INCOMPLETE (not PASS); contains `Timestamp:`

- [x] [P6-T8] Mirror the issue-status update and confirm a clean worktree
  - Update: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/issue.md` AC checkboxes to reflect verified status; write mirror `evidence/issue-updates/issue-181.2026-06-08T12-12.md`
  - Verify: `git status` is clean after all evidence is committed
  - Acceptance: issue.md AC section reflects evidence; mirror artifact contains `Timestamp:`, `PostedAs:`, and the exact text; worktree is clean

## Test Plan

- Unit: existing MSTest suites must pass unchanged; no test logic is modified by this feature (analyzer adoption only). Run with `vstest.console.exe ... /EnableCodeCoverage /InIsolation /Logger:trx`.
- Integration: not applicable; this is build-config and documentation only.
- Manual/build verification: the four msbuild/vstest toolchain stages plus CSharpier, matching the CI workflow commands.
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`
  - Post-change: `evidence/qa-gates/final-test-coverage.2026-06-08T12-12.md`
  - Comparison: `evidence/qa-gates/coverage-delta.2026-06-08T12-12.md`

## Open Questions / Notes

- Revision 2.0 resolves the P4-T16 blocking finding by dropping SecurityCodeScan.VS2019 entirely. The analyzer set is now FIVE packages (Meziantou.Analyzer, SonarAnalyzer.CSharp, Roslynator.Analyzers, AsyncFixer, Microsoft.CodeAnalysis.BannedApiAnalyzers). No CS8032 suppression is introduced, and no substitute security analyzer is added (out of scope for this rollout). The deferral is recorded as a documented decision in `.claude/rules/csharp.md` (P5-T3).
- Because plan v1.0 already wrote SecurityCodeScan.VS2019 to all 15 first-party `packages.config` files, all 15 first-party `.csproj` files (including the sibling `YamlDotNet.dll` `<Analyzer>` reference), and the `.editorconfig` SCS severities, revision 2.0 includes explicit cleanup tasks: `.editorconfig` (P2-T8), `packages.config` (P3-T2..P3-T16), and `.csproj` (P4-T1..P4-T15). The protected nullable gate is re-verified clean after cleanup at P4-T16.
- Phase 1 resolves all package-version and DLL-path unknowns before any `.csproj`/`packages.config` edits. No analyzer wiring occurs until versions and paths are recorded.
- RS0030 is intentionally held at `suggestion` for initial rollout; legacy banned-symbol cleanup is out-of-scope follow-up work (documented in P1-T7 and the RS0030 `.editorconfig` comment in P2-T6).
- `TaskTree` has no corresponding `.Test` project; this is a pre-existing coverage gap noted in research and unrelated to analyzer adoption.
- `Microsoft.Extensions.TimeProvider.Testing` is referenced only in documentation (P5-T1). No test-package addition is performed unless a compile-required seam needs it; if so, it would be added to the relevant test project's packages.config + `<Reference>` and covered under the >= 90% new-code rule.
