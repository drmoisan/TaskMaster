# Research: Fizzler redirect remedy and bindingRedirect gate (issue #953)

- Date: 2026-10-02
- Scope: research only; no source changes. All paths are repository-relative to the worktree unless marked "main checkout".
- Evidence tags: [V] verified by Grep/Read/Glob in this session; [U] not verified.

## 1. Current state (re-verified)

### 1.1 Fizzler redirects: 13 configs, 2 correct, 11 stale [V]

Every `name="Fizzler"` block in `**/app.config` (Grep with -A1):

| Config | newVersion | Line |
|---|---|---|
| SVGControl/app.config | 1.3.1.0 | 14-15 |
| UtilitiesCS/app.config | 1.3.1.0 | 51-52 |
| QuickFiler/app.config | 1.3.0.0 | 50-51 |
| QuickFiler.Test/app.config | 1.3.0.0 | 46-47 |
| TaskMaster/app.config | 1.3.0.0 | 50-51 |
| Tags/app.config | 1.3.0.0 | 46-47 |
| TaskTree/app.config | 1.3.0.0 | 46-47 |
| TaskVisualization/app.config | 1.3.0.0 | 46-47 |
| TaskVisualization.Test/app.config | 1.3.0.0 | 46-47 |
| ToDoModel/app.config | 1.3.0.0 | 51-52 |
| ToDoModel.Test/app.config | 1.3.0.0 | 46-47 |
| UtilitiesCS.Test/app.config | 1.3.0.0 | 46-47 |
| SVGControl.Test/app.config | 1.3.0.0 | 18-19 |

Stale ones carry `oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0"`; the two correct ones carry `oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0"`. The issue text says "twelve" stale and one at 1.3.1.0; the current tree has 11 stale and 2 at 1.3.1.0 (SVGControl was fixed by #929 task P1-T5, `docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t5-redirects-fixed.2026-09-28T20-01.md:13`). The "single config already at 1.3.1.0" the issue asks to identify is therefore UtilitiesCS (original) plus SVGControl (fixed by #929); not a partial attempt aimed at this issue.

### 1.2 Unsafe: all 17 configs at 6.0.3.0 [V]

Grep of `System.Runtime.CompilerServices.Unsafe` -A1 over `**/app.config` returns 17 blocks, every one `oldVersion="0.0.0.0-6.0.3.0" newVersion="6.0.3.0"` (SVGControl/app.config:18-19 included). 17 equals the config-file count from `assemblyIdentity name=` count-mode (17 files listed). The Unsafe half of the issue is fixed.

### 1.3 Fizzler in the project graph [V]

- `SVGControl/SVGControl.csproj:58-59` and `UtilitiesCS/UtilitiesCS.csproj:65-66`: `<Reference Include="Fizzler, Version=1.3.1.0, ...>` with HintPath `..\packages\Fizzler.1.3.1\lib\netstandard2.0\Fizzler.dll`.
- `SVGControl/packages.config:4` and `UtilitiesCS/packages.config:11`: `Fizzler` version `1.3.1`.
- No other csproj or packages.config mentions Fizzler. The other 11 configs name Fizzler only through the redirect; those projects receive Fizzler.dll transitively by project reference (copy-local) [inference from UtilitiesCS being the common dependency; [U] not built here].
- `using Fizzler;` appears in `SVGControl/PictureBoxSVG.cs:15`, `UtilitiesCS/.../Triage_OlLogic.cs:9`, `.../BayesianClassifier.cs:14`, `.../MailItemHelper.cs:11`. The #418 research (`docs/features/archive/2026-08-04-svg-renderer-null-document-nre-418/research/2026-08-04T15-05-svg-renderer-null-document-research.md:94,270-276`) records that Svg/ExCSS carry no Fizzler AssemblyRef and the usings are unused, so the redirects are inert today. Not re-verified here (no binaries in the worktree).
- Packages tree: absent in the worktree (Glob `packages/*` returned nothing). Main checkout (read-only): `<main-checkout-root>\packages\Fizzler.1.3.1\lib\netstandard2.0\Fizzler.dll` and `netstandard1.0` exist; also `Svg.3.4.8`, `ExCSS.4.3.2`, `System.Runtime.CompilerServices.Unsafe.6.1.2`.

### 1.4 Binding-redirect autogeneration [V]

Grep for `AutoGenerateBindingRedirects|GenerateBindingRedirectsOutputType` over `**/*.csproj` returns no matches. The projects are legacy packages.config projects with hand-carried `app.config` redirects; no build step regenerates them. NuGet/VS regenerates redirects only at package install/update time (VS tooling behavior; [U] not verifiable offline), so a config can drift silently otherwise, which is the cause the issue records.

## 2. Remedy decision (Q3)

| Option | Effect | Assessment |
|---|---|---|
| (a) Sweep 11 to 1.3.1.0 | Same shape as UtilitiesCS/SVGControl; `oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0"` | Correct for an assembly that is actually deployed (Fizzler.dll 1.3.1.0 is copy-local via UtilitiesCS/SVGControl). Minimal, mechanical, uniform across the 13 configs. A host that ever requests Fizzler binds successfully. Harmless if never requested. Gate-compatible. |
| (b) Remove Fizzler blocks | Config without a redirect for a never-requested assembly is harmless | Diverges from the two projects that carry the Reference; any VS package update would re-add a redirect (so removal is not stable); `Invoke-BindingRedirectReconciliation` (`scripts/dependencies/ProjectConsistency.psm1:271-375`) deliberately returns text unchanged when no redirect exists, so tooling would not maintain absence. Larger semantic change than the defect warrants; if a future dependency requests Fizzler 1.3.0.x, an absent redirect would still bind to 1.3.1.0 only via the exact-version rule, which fails (assembly 1.3.0.0 requested, 1.3.1.0 on disk, no redirect). |
| (c) Mix | Keep in projects that reference it, remove elsewhere | Inconsistent; no benefit over (a). |

Recommendation: (a). WinForms/VSTO hosts read the host `app.config` / `.vsto` config, and every test/host config would then name the one deployed version. Config edits do not alter compilation.

## 3. Mechanical gate (Q4)

### 3.1 Existing tooling inspected [V]

- `scripts/dependencies/PackageGraph.psm1` (465 lines): `ConvertFrom-AppConfigText` (line 285) already parses each `dependentAssembly` into records with `Name, PublicKeyToken, Culture, OldVersion, NewVersion`; `ConvertFrom-ProjectFileText` (line 215) parses `Reference`, `HintPath` elements; `Get-PackageManifestPath -Kind AppConfig` (line 87) takes an injected lister and excludes packages/bin/obj/node_modules.
- `scripts/dependencies/ProjectConsistency.psm1` (381): `Invoke-BindingRedirectReconciliation` (271) rewrites a redirect to a supplied assembly version; it is a repair, not a verifier, and only for packages the repair run upgraded (`Repair-PackageManifestConsistency.ps1:22-24`, `:433`).
- `scripts/dependencies/ConsistencyVerifier.psm1` (499): detection functions `Find-VersionDisagreement`, `Find-OrphanedHintPath`, `Test-ReferenceCompleteness`, `Find-PackageAbsentFromManifest`. None inspects `bindingRedirect` (Grep for `bindingRedirect` in this file: no match).
- Other modules: `AnalyzerItemRepair.psm1` (402), `PackageCompatibility.psm1` (172), `Repair-PackageManifestConsistency.ps1` (475). No existing verifier checks redirects against deployed versions.
- Existing redirect test precedent: `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1:92-110` asserts only SVGControl's Fizzler and Unsafe redirects equal the SVGControl csproj Reference version. It reads tracked files with `[System.IO.File]::ReadAllText` from `$PSScriptRoot`-derived paths (lines 4, 94-95); no temp files, no `$TestDrive`. It has a local `Get-DependentAssemblyBlock` helper (lines 45-60).
- Line counts (Grep `.*` count; approximate to the extent of trailing-line handling): see the module list above; test files: ProjectConsistency.Tests 494, PackageGraph.Tests 487, Repair-PackageManifestConsistency.Tests 429, RepositoryTreeConsistency.Tests 152, ConsistencyVerifier.Tests 337, AnalyzerItemRepair.Tests 311, DependabotConfig.Tests 470, PackageCompatibility.Tests 124.
- Module pattern: `Set-StrictMode -Version Latest`, advanced functions with `[CmdletBinding()]`/`[OutputType]`, comment-based help, pure-over-text functions, I/O only through injected scriptblock delegates (`PackageGraph.psm1:8-11`; `Repair-PackageManifestConsistency.ps1:7-11,33-42`), `Export-ModuleMember -Function @(...)` at the bottom (e.g. `ProjectConsistency.psm1:377-381`). Modules import siblings via `Import-Module (Join-Path $PSScriptRoot 'X.psm1')` without `-Force` (`ProjectConsistency.psm1:33-44`). Tests import with `-Force` (`RepositoryTreeConsistency.Tests.ps1:5`).
- No temp files: fixtures are inline strings; the one repo-level test reads tracked files read-only. Matches `.claude/rules/powershell.md` "No external dependencies in unit tests" and the repo ban on temp files.

### 3.2 Home for the new rule

- `ConsistencyVerifier.psm1` is at 499 of 500 lines: cannot host it.
- `ProjectConsistency.psm1` (381) has room (~119 lines) but its header (lines 1-29) scopes it to reconciliation, with detection owned by ConsistencyVerifier; adding detection there breaks the one-owner split the header documents.
- `PackageGraph.psm1` (465) is the text parser; leave as is, consume it.
- Preferred: a new small module `scripts/dependencies/BindingRedirectVerification.psm1` (target ~100-150 lines) importing `PackageGraph.psm1` and exporting one detector. Fallback with no new file: append the detector to `ProjectConsistency.psm1` and update its header; unit tests then go into `RepositoryTreeConsistency.Tests.ps1` because `ProjectConsistency.Tests.ps1` (494) has no room.

### 3.3 Detector shape (in-memory seam)

`Find-StaleBindingRedirect -AppConfigText <string> -DeployedVersionProvider <scriptblock>` returning findings `{AssemblyName, NewVersion, DeployedVersions[]}` plus an examined count (the repo pattern guards a zero finding count with an examined count: `RepositoryTreeConsistency.Tests.ps1:87-89`). The provider is a delegate taking an assembly name and returning the set of deployed assembly versions, mirroring `AssemblyIdentityProvider` (`Repair-PackageManifestConsistency.ps1:39-41`, default at `:117-133`). Policy for a name the provider returns nothing for: report as "unverifiable" separately, not as a failure, so transitive-only redirects do not need an allow list; the real-repo test asserts the unverifiable count is bounded and that the four named families (Fizzler, ExCSS, Svg, Unsafe) are all verifiable.

Tests (negative control required by the issue): (1) fixture config with Fizzler redirect `1.3.0.0` and provider reporting `1.3.1.0` yields one finding; (2) the same config with `1.3.1.0` yields none; (3) a name with no provider data is unverifiable, not a failure; (4) range `oldVersion` ignored, only `newVersion` compared; (5) a block without bindingRedirect is skipped (parser already yields empty `NewVersion`). The repo-level test then builds the provider from every `*.csproj` `Reference Include="Name, Version=..."` text, runs the detector over all 17 configs, and asserts zero findings. The negative control must flip the provider or fixture, not the production code (memory note: negative control must isolate the code fix).

## 4. Source of truth for the deployed version (Q5)

| Source | CI feasible in the Pester job | Reproduces #418 ExCSS / Fizzler classes | Notes |
|---|---|---|---|
| (i) assembly metadata under `packages/` | No. `.github/workflows/_pester.yml` has no checkout of packages, no `nuget restore`; it runs only `tests/scripts/dependencies`, `hygiene`, `vscode` (lines 16-47). Restore + cache exists only in `_mstest-coverage.yml:46-66`, `_build-*.yml`, `dependabot-repair.yml:64-73`. Worktree has no `packages/` either. | Yes, ground truth | Usable locally and in `Repair-PackageManifestConsistency.ps1` default delegate only. |
| (ii) csproj `Reference Include="Name, Version=..."` | Yes (tracked text) | Yes, provided the csproj Reference reflects the deployed assembly, which `Resolve-ReferenceAssemblyVersion` (`ProjectConsistency.psm1:125-167`) and the repair tool keep in agreement with the restored package | Fizzler: both Reference entries at 1.3.1.0 vs 11 redirects at 1.3.0.0 => 11 findings. ExCSS: Reference 4.3.2.0 across csproj lines (e.g. `UtilitiesCS.csproj:62`). |
| (iii) packages.config version mapped to assembly version | Yes | No: not equal quantities | Unsafe package 6.1.2 vs assembly 6.0.3.0 (issue.md:29; csproj Reference `Version=6.0.3.0`, e.g. `SVGControl.csproj:82`); Svg package 3.4.8 vs assembly 3.4.0.0 (`UtilitiesCS.csproj:293`, redirect `QuickFiler/app.config:234-235` is 3.4.0.0). Comparing against package version would produce false findings. |

Chosen: (ii), with (i) kept as the repair-time truth. The chain is then packages.config <-> HintPath folder (existing verifier) <-> Reference version (existing repair/resolve) <-> redirect newVersion (new gate). Limitation: a redirect whose assembly appears in no csproj Reference is unverifiable under (ii); the number of such names across the 1176 `assemblyIdentity` entries was not enumerated here [U] and must be measured by the planner before fixing the unverifiable policy. All 13 Fizzler, all 17 ExCSS-and-Unsafe-relevant, and the one Svg redirect do have csproj Reference entries (ExCSS refs: SVGControl.Test, UtilitiesCS, SVGControl, QuickFiler csproj lines found; Unsafe in 17 projects' references per the grep).

CI coverage: Pester CI gate is `$linePercent -lt 80` (`_pester.yml:71`) over `scripts/dependencies` (line 45), whereas `.claude/rules/powershell.md` and `general-unit-test.md` state >= 85%. A new module under `scripts/dependencies` joins the denominator; plan to the stricter 85%.

## 5. Numeric Derivation Evidence

Claim A: 13 configs carry a Fizzler redirect; 11 name 1.3.0.0.
- Complete Family: all `app.config` files in the repository (17).
- Exhaustive Search Scope: Glob `**/app.config` via Grep over worktree; `bin/obj/packages` pruned by glob match of the file name only (no app.config under packages in the worktree).
- Inclusion Rules: a `dependentAssembly` with `assemblyIdentity name="Fizzler"`.
- Exclusion Rules: none.
- Primary Search Strategy: Grep `name="Fizzler"` with -A1 over `**/app.config`.
- Primary Member Set: UtilitiesCS.Test, Tags, TaskVisualization.Test, SVGControl.Test, TaskVisualization, TaskTree, TaskMaster, SVGControl, UtilitiesCS, ToDoModel.Test, QuickFiler.Test, ToDoModel, QuickFiler.
- Primary Count: 13 (11 at 1.3.0.0, 2 at 1.3.1.0).
- Cross-check Strategy: Grep `Fizzler` over `**/*.{config,csproj,cs,props,targets}` filtered to config hits.
- Cross-check Member Set: QuickFiler.Test, QuickFiler, TaskMaster, UtilitiesCS.Test, Tags, SVGControl.Test, SVGControl, UtilitiesCS, ToDoModel.Test, ToDoModel, TaskVisualization.Test, TaskVisualization, TaskTree.
- Cross-check Count: 13.
- Member-set Comparison: identical sets.

Claim B: 17 configs, all Unsafe at 6.0.3.0.
- Primary: Grep `System.Runtime.CompilerServices.Unsafe` -A1 over `**/app.config`: 17 blocks, all `6.0.3.0`. Cross-check: Grep count of `assemblyIdentity name="` per app.config: 17 files. Member sets agree (VBFunctions.Test, TaskVisualization.Test, SVGControl.Test, TaskVisualization, SVGControl, TaskTree.Test, TaskMaster.Test, ToDoModel, QuickFiler, TaskTree, TaskMaster, QuickFiler.Test, Tags.Test, UtilitiesCS.Test, Tags, UtilitiesCS, ToDoModel.Test = 17).

## 6. File footprint and budget (Q6)

Sweep (11 files, exact paths): `QuickFiler/app.config`, `QuickFiler.Test/app.config`, `TaskMaster/app.config`, `Tags/app.config`, `TaskTree/app.config`, `TaskVisualization/app.config`, `TaskVisualization.Test/app.config`, `ToDoModel/app.config`, `ToDoModel.Test/app.config`, `UtilitiesCS.Test/app.config`, `SVGControl.Test/app.config`. Edit only the two attributes on the Fizzler line pair (`oldVersion` upper bound and `newVersion`).

PowerShell:
- New: `scripts/dependencies/BindingRedirectVerification.psm1` (production, 1 slot).
- New: `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` (1 test slot).
- Modified: `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` (152 lines; adds the all-17-configs real-file test; second test slot). Optionally replace the SVGControl-only test at lines 92-110 or leave it.
- Total: 1 production file, 2 test files: inside the direct-mode cap (2 production) and per-batch cap (3/3) in `.claude/rules/powershell.md`. If the session-wide budget counter has no free production slot, use the fallback (modify `ProjectConsistency.psm1` + `RepositoryTreeConsistency.Tests.ps1`: 1 production modified, 1 test modified, zero new files), at the cost of the header ownership split. The budget state itself is [U]; this run cannot read it.

No C# change; no `.csproj` change.

## 7. Risks (Q7)

- Line endings: app.configs are CRLF (`\r$` match counts equal roughly line counts per file, e.g. SVGControl/app.config 23, TaskMaster/app.config 430) [V]; edits must preserve CRLF. Whether a BOM exists was not conclusively checked [U]. Use byte-preserving edits (Edit tool on exact attribute text), not a rewrite via a text writer that normalizes endings. `Invoke-BindingRedirectReconciliation` itself preserves terminators by construction (`ProjectConsistency.psm1:18-23`), and is a usable mechanical alternative to hand edits (calling it with `Fizzler`/`1.3.1.0`).
- Formatter: `.csharpierignore:17-18` excludes `**/app.config` and `**/packages.config`, so `csharpier check` neither requires nor reformats these edits. `.csproj/.props/.targets` also excluded (lines 12-14).
- Build impact: redirects are runtime-only; a wide edit does not change compilation. `dependabot-repair.yml:135` stages `*/app.config`, so later bot runs will touch the same files; no conflict unless a bot branch is open.
- Gate false positives: comparing to package version (source iii) would fail on Unsafe (6.1.2 vs 6.0.3.0) and Svg (3.4.8 vs 3.4.0.0). Redirects for assemblies with multiple Reference versions across projects (if any) need set-membership, not equality; not enumerated [U].
- Gate vacuity: a detector reading zero redirects passes trivially; the examined-count guard and the negative control are required.
- Pester CI line floor is 80 in the workflow versus 85 in the rules: target 85.

## Recommendation

1. Remedy: option (a). Change the 11 stale Fizzler blocks to `oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0"`. Do not remove blocks.
2. Gate: a new Pester detector over text, with an injected `DeployedVersionProvider` delegate fed in the real-repo test from csproj `Reference Include="Name, Version="` values (source ii). Assert zero findings across all 17 `app.config` files with an examined-count guard; include a negative control (fixture with a redirect to a version no Reference provides, mirroring #418 ExCSS/#953 Fizzler) and a positive control. Report unverifiable names separately after the planner measures how many exist.
3. Footprint: 11 `app.config` files; new `scripts/dependencies/BindingRedirectVerification.psm1`; new `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`; modified `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`. Fallback if no production slot is available: modify `scripts/dependencies/ProjectConsistency.psm1` instead of adding the module.
