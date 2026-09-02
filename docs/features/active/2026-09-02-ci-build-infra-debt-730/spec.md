# 2026-09-02-ci-build-infra-debt (Spec)

- **Issue:** #730
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T19:31
- **Status:** Ready for Planning
- **Version:** 0.3

## Write Set

- `.github/workflows/_build-analyzers.yml`
- `.github/workflows/_build-nullable.yml`
- `.github/workflows/_mstest-coverage.yml`
- `Directory.Build.props`

## Context
Two consolidated CI/build-infrastructure findings: a NuGet cache fallback with no restore-verification step, and an unsuppressed unsupported-package warning. Consolidated into one issue rather than two since both are build-pipeline configuration debt in the same category (silent tolerance of a degraded/unverified state) rather than application code defects.

Environment:
- OS/version: Windows 11 Pro (repo default) / GitHub Actions `windows-latest` runners
- Python version: n/a — GitHub Actions workflow YAML and packages.config
- Command/flags used: n/a — findings are from direct workflow/config inspection
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: neither finding causes a build failure today, but both represent a build pipeline silently tolerating a degraded-confidence state (unverified stale-cache restore; an unacknowledged unsupported-package warning) rather than failing loudly or being deliberately suppressed with a documented rationale.


## Repro & Evidence
Steps to Reproduce:
Not applicable — both findings are static configuration inspections. See "Actual Behavior."

Expected:
CI's NuGet restore either hits a valid cache or fails loudly rather than silently resolving stale packages; a deliberately-accepted unsupported-package warning is suppressed with a documented rationale rather than firing unacknowledged on every build.

Actual:
**1. Three CI workflows carry a bare-prefix NuGet cache `restore-keys` fallback with no restore-verification step.** Confirmed at `.github/workflows/_build-analyzers.yml:40`, `.github/workflows/_build-nullable.yml:40`, and `.github/workflows/_mstest-coverage.yml:40`, each with `restore-keys: nuget-${{ runner.os }}-` (bare prefix, no lock-file hash component). A cache-key miss (guaranteed by any packages.config/.csproj change) falls back to restoring a stale, pre-change package tree, and nothing in these workflows verifies the restored packages actually match the current lock state before proceeding — risking builds silently running against stale package versions. *(Source: #569.)*

**2. packages.config pins `System.Reactive 7.0.0`, which is unsupported for packages.config-style references, and no `RxUseUnsupportedPackagesConfig` suppression exists anywhere in the repo.** Confirmed: the property does not appear in any .csproj/`Directory.Build.props`/config file — the only occurrence repo-wide is inside a committed evidence log quoting the warning text itself. The guard-target warning fires on every build referencing this package and has been observed as recently as 2026-08-26 evidence. *(Source: #570.)*

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citations above, each confirmed directly against `origin/main` on 2026-09-02.


## Scope & Non-Goals
- In scope:
  - Finding 1: add a documentation-only explanatory YAML comment block immediately above `restore-keys:` in each of `.github/workflows/_build-analyzers.yml`, `.github/workflows/_build-nullable.yml`, and `.github/workflows/_mstest-coverage.yml`. No functional change to the cache/restore step behavior in any of the three files.
  - Finding 2: add a new repository-root `Directory.Build.props` file that sets `RxUseUnsupportedPackagesConfig` to `true` with an inline rationale comment, suppressing the System.Reactive.PackagesConfigCheck warning across the five projects that reference `System.Reactive` via packages.config.
- Out of scope / non-goals:
  - Migrating `System.Reactive` from packages.config to `PackageReference` in any of the five affected projects (QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, and UtilitiesCS.Test) — research §2.4(c) confirms this is a materially larger, conflicting change against the repo's documented legacy-VSTO/packages.config convention.
  - Rolling back the `System.Reactive` package version to a 6.x release — research §2.4(d) confirms this re-litigates the original #395/#570 decision and is not a build-infrastructure configuration fix.
  - Any functional edit to the workflow file at path .github/workflows/ci.yml, unless a later planning/execution step finds it strictly unavoidable to reference the new documentation (not anticipated by research).
  - Adding, removing, or changing any coverage threshold, Pester job, or coverage gate — these are covered by issues #561/#562/#563 and are explicitly excluded from this issue's scope.
  - Adding a new restore-verification script step or removing the `restore-keys` bare-prefix fallback (research §1.3 options (a) and (b)) — research confirms the risk those options would address is not real for this repository's packages.config + `nuget.exe restore` combination.
  - Editing the structurally similar `restore-keys` fallback (dotnet-tools cache) in the workflow file at path .github/workflows/_format-check.yml — out of scope per research §1.4, not one of the three files named in the issue, and not independently analyzed for correctness.
  - Any edit under the Claude runtime tree at path .claude (and everything beneath it), the Codex mirror tree at path .codex (and everything beneath it), the dot-agents tree at path .agents (and everything beneath it), or the two published configuration files at paths config/blast-radius.json and config/orchestration-routing.json — these paths are published from an upstream repo and are overwritten on the next push-down.
- Explicitly excluded systems, integrations, or datasets: none — both fixes are confined to build/CI configuration files; no application runtime, data, or integration surface is touched.

## Root Cause Analysis
Each finding traces to a specific prior issue, cited inline above (#569 for Finding 1, #570 for Finding 2). Both are configuration-only fixes with no application-code footprint — a workflow YAML edit and a new project-file/MSBuild property file, respectively.

Research confirms and extends this root-cause framing for each finding:

- **Finding 1**: the underlying premise — that a `restore-keys` bare-prefix fallback hit could let a build "silently run against stale package versions" — was investigated and refuted (research §1.2). `nuget.exe restore` for packages.config-style projects is per-package, existence-checked, and idempotent: every package is materialized under a version-qualified directory (`packages/{id}.{version}/`) that exactly matches every `HintPath` in the consuming .csproj files, and `nuget restore` runs unconditionally after the cache step in all three workflows (no `cache-hit` gate exists on any step). A fallback-restored `packages/` tree can therefore only ever contain version-folders that still match the current packages.config (legitimate reuse) or inert orphaned version-folders for packages no longer referenced (harmless). Any version bump in packages.config is fetched fresh from the network regardless of cache tier. The true "root cause" is therefore not a functional defect but an undocumented invariant — the fallback was already safe, but nothing recorded why.
- **Finding 2**: root cause is confirmed as a missing suppression, not a missing capability. The `RxUseUnsupportedPackagesConfig` property is the package vendor's own documented escape hatch (confirmed via the verbatim warning text captured in a prior evidence log and an independent prior research note, research §2.2) and no competing import-disabling property or NuGet.Config file exists anywhere in the repo (research §2.3) that would prevent a root-level `Directory.Build.props` from reaching all five affected projects through the same auto-import mechanism (Microsoft.Common.props) already proven live in this repo via the existing Directory.Build.targets file.

## Proposed Fix

### Design summary (what changes where):
Two independent, additive, configuration-only changes with no application source code footprint:
1. **Finding 1** — add an explanatory (non-functional) YAML comment block immediately above the `restore-keys:` key in each of the three CI workflow files, documenting why the bare-prefix cache fallback is already safe (version-folder-scoped `nuget restore` idempotency). No YAML key, value, step, or job is added, removed, or reordered.
2. **Finding 2** — add one new file, `Directory.Build.props`, at the repository root, setting `RxUseUnsupportedPackagesConfig` to `true` with an inline rationale comment. This is picked up automatically by all five `System.Reactive`-consuming projects (and harmlessly by the other thirteen non-`System.Reactive` projects in the solution) via MSBuild's standard Microsoft.Common.props auto-import of `Directory.Build.props`, which is already proven live in this repository through the existing Directory.Build.targets file.

### Boundaries and invariants to preserve:
- The cache/restore step behavior in the three workflow files (cache key, restore-keys value, step names, unconditional `nuget restore` invocation) must remain byte-identical except for the inserted comment lines.
- `Directory.Build.props` must not alter any existing property already set explicitly inside any of the eighteen .csproj files; it only supplies a new property (`RxUseUnsupportedPackagesConfig`) that no .csproj currently sets, so there is no override/collision risk.
- No .csproj, packages.config, or Directory.Build.targets file is edited by either fix (research §1.4, §2.5).
- No application (non-build-config) source file is touched by either fix.

### Dependencies or blocked work:
- None. Both fixes are self-contained and do not depend on or block any other open issue. Coverage-threshold/Pester/gate work tracked under #561/#562/#563 is explicitly independent of this issue's scope.

### Implementation strategy (what changes, not sequencing):
This is a build/CI-configuration-only change. No application source code (`.cs`, application XAML/WinForms, VSTO ribbon/ThisAddIn code, etc.) is touched by either fix.

- Finding 1: insert the literal comment block given verbatim in research §1.4 immediately above `restore-keys:` in each of the three named workflow files. The comment text is identical across all three files.
- Finding 2: create the new `Directory.Build.props` file at the repository root with the literal content given verbatim in research §2.5.

#### Files/modules to change:
- `.github/workflows/_build-analyzers.yml`
- `.github/workflows/_build-nullable.yml`
- `.github/workflows/_mstest-coverage.yml`
- `Directory.Build.props`

#### Functions/classes/CLI commands impacted:
None. No C# class, method, or CLI command is impacted. The `Restore solution` step's `nuget restore $env:SOLUTION_PATH` invocation is unchanged in all three workflow files (comment-only edit occurs on the preceding `Cache NuGet packages` step). MSBuild's System.Reactive.PackagesConfigCheck.targets guard target continues to run in all five affected projects' builds; only its warning-emission condition (`RxUseUnsupportedPackagesConfig`) is now satisfied.

#### Data flow and validation changes:
None. Neither fix changes any data flow, input, or output. Finding 1 is a comment insertion with zero effect on YAML evaluation. Finding 2 adds an MSBuild property that is consumed only by the vendor's PackagesConfigCheck guard-target condition; it does not change which files are restored, compiled, or emitted.

#### Error handling and logging updates:
None required by either fix. Finding 2 suppresses a build-time `warning` (not an `error`); no error-handling code path exists to update. No workflow failure-handling logic is changed by Finding 1.

#### Rollback/feature-flag considerations (if applicable):
Both fixes are trivially revertible via a single file-level git revert (delete `Directory.Build.props`; revert the three comment-only workflow diffs) with no downstream migration or data cleanup required, since neither fix changes runtime behavior, only comment text and a warning-suppression property.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- Finding 1: pure YAML comment (`#`-prefixed lines) inserted above an existing `restore-keys:` mapping key; no change to the workflow's declared inputs (`SOLUTION_PATH` env var) or outputs (restored `packages/` directory contents).
- Finding 2: new MSBuild `.props` XML file conforming to the standard `<Project><PropertyGroup>...</PropertyGroup></Project>` shape, defining exactly one property, `RxUseUnsupportedPackagesConfig`, with value `true`.

#### Required configuration keys and defaults:
- `RxUseUnsupportedPackagesConfig` = `true` (new; no prior default existed — the vendor guard target's default behavior is to emit the warning when this property is unset or `false`).

#### Backward-compatibility expectations:
Fully backward compatible. Finding 1 changes no evaluated YAML semantics. Finding 2 introduces a new property that is additive and only recognized by System.Reactive.PackagesConfigCheck.targets; it has no effect on the twelve non-`System.Reactive` projects beyond being silently ignored (harmless, per research §2.3's closing note).

#### Performance constraints (latency/throughput/memory):
None applicable. Neither fix changes restore time, cache size, network call count, or compile time in any measurable way beyond the removal of five warning-log lines per rebuild.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - The repository's packages.config + classic `nuget.exe restore` model (not `PackageReference`/lock-file restore) remains the restore mechanism for all five affected projects for the duration of this fix; research's non-realizability conclusion for Finding 1 (§1.2) is scoped specifically to this restore model and does not automatically transfer if the projects later migrate to `PackageReference`.
  - No NuGet.Config file or competing `Directory.Build.props` is introduced elsewhere in the repository between research capture (2026-09-02) and implementation that would change the auto-import chain confirmed in research §2.3.
- Constraints (budget, performance, compatibility):
  - Must not modify any .csproj, packages.config, or Directory.Build.targets file (research §1.4, §2.5 confirm neither fix requires it).
  - Must not exceed the 500-line file-size limit for any touched or newly created file (`Directory.Build.props` is 19 lines; each workflow comment insertion adds fewer than 25 lines to files already well under the limit).
- External dependencies (services, libraries, releases):
  - System.Reactive.PackagesConfigCheck.targets, shipped inside the System.Reactive 7.0.0 NuGet package, is the vendor-owned guard target whose warning-emission condition this fix suppresses; its exact XML was not directly readable in this session because `packages/` is not restored in this worktree, but its behavior is corroborated by two independent repo-local sources (research §2.2).
  - GitHub Actions `actions/cache@v4`'s `restore-keys` prefix-match/tie-break behavior (documented upstream, not repo-local) underlies Finding 1's comment rationale but does not change the conclusion regardless of which specific prior cache entry is selected (research §1.2 item 6).

## Data / API / Config Impact
- User-facing or API changes: none. Neither fix is visible to any TaskMaster application user; both affect only the CI pipeline and the local/CI build-warning surface.
- Data or migration considerations: none. No data schema, database, or migration is touched.
- Logging/telemetry updates (if any): the only observable "log" change is a reduction of five System.Reactive.PackagesConfigCheck "unsupported scenario" warning lines from each rebuild's MSBuild output (Finding 2). No application logging or telemetry is modified.
- Compatibility notes (CLI flags, config schemas, versioning): none. No CLI flag, config schema, or package/assembly version changes.

## Test Strategy

Both fixes are build/CI-configuration-only changes with no application source code touched, so MSTest/Moq/FluentAssertions unit-test authorship is not applicable to either fix directly (per research "Testing implications"). Verification strategy:

- **No new unit tests** are added for either finding — there is no new application logic to unit-test.
- **Finding 1 (comment-only change) verification:**
  - Confirm the diff for each of the three workflow files is comment-only (no key, value, step, or job added/removed/reordered) by reviewing the diff directly against the literal replacement text in research §1.4.
  - Confirm the existing CI workflow YAML remains syntactically valid after the change (e.g., a workflow-syntax check, or observing that `_build-analyzers.yml`, `_build-nullable.yml`, and `_mstest-coverage.yml` continue to trigger and run their existing steps without a YAML parse failure).
- **Finding 2 (`Directory.Build.props` addition) verification:**
  - Capture a full local rebuild transcript **before** the change using both of CLAUDE.md's two C# toolchain build commands:
    - `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    - `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  - Add `Directory.Build.props`, then capture a full local rebuild transcript **after** the change using the same two commands.
  - Compare the before/after transcripts: confirm all five System.Reactive.PackagesConfigCheck "unsupported scenario" warnings (one per affected project: QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, and UtilitiesCS.Test) are absent from the after-transcript, and confirm no new warning or error is introduced anywhere in the after-transcript relative to the before-transcript.
  - Re-run the existing MSTest suites in the Rx-dependent test assemblies (UtilitiesCS.Test, and QuickFiler.Test if present) once after the `Directory.Build.props` addition via `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`, to confirm no behavior change. This is regression re-verification of existing tests, not new test authorship.
- Coverage impact and targets for changed lines/modules: not applicable — no application source line is added, removed, or modified by either fix, so there is no coverage-denominator impact.
- Toolchain commands to run (format → lint → type-check → test): `dotnet tool run csharpier check .` (verify no formatting drift is introduced by the new `Directory.Build.props` file — CSharpier's `.csharpierignore` excludes `.csproj`/`.props`/`.targets`, so this is expected to be a no-op check for the new file, but is run to confirm), then the two `msbuild TaskMaster.sln /t:Rebuild ...` commands above (analyzer pass and nullable/TreatWarningsAsErrors pass), then `vstest.console.exe` re-run of the Rx-dependent test assemblies. No YAML linter is mandated by CLAUDE.md; workflow-syntax validity is confirmed by observation of the three workflows continuing to run.
- Manual validation steps (if required): visually confirm the inserted comment blocks in the three workflow files read correctly as YAML comments (each line prefixed with `#` at the correct indentation level matching the surrounding step) and do not accidentally comment out or alter the `restore-keys:` key itself.
- Evidence capture location: before/after rebuild transcripts and any workflow-syntax-check output must be written under `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/` per the evidence-and-timestamp-conventions skill, not as ad hoc files elsewhere.


## Acceptance Criteria
- [ ] Explanatory rationale comment (identical in content, per research §1.4) is present immediately above `restore-keys:` in each of `.github/workflows/_build-analyzers.yml`, `.github/workflows/_build-nullable.yml`, and `.github/workflows/_mstest-coverage.yml` (3 files, per research's confirmed exhaustive derivation in §1.1's Numeric Derivation Evidence), with no functional change to the cache/restore step behavior in any of the three files.
- [ ] A new `Directory.Build.props` file exists at the repository root, setting `RxUseUnsupportedPackagesConfig` to `true` inside a `<PropertyGroup>`, with an inline XML comment stating the rationale for accepting the vendor's unsupported-scenario trade-off.
- [ ] A full local rebuild (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`, per CLAUDE.md's C# toolchain commands) shows zero System.Reactive.PackagesConfigCheck "unsupported scenario" warnings across all five previously-affected projects (QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, and UtilitiesCS.Test — per research's confirmed exhaustive derivation in §2.1's Numeric Derivation Evidence), with no new warnings or errors introduced relative to a pre-change baseline rebuild.
- [ ] No .csproj, packages.config, Directory.Build.targets, or application source file is modified by either fix.
- [ ] No coverage threshold, Pester job, or coverage gate is added or changed by this work (that scope belongs to #561/#562/#563).
- [ ] Existing MSTest suites in the Rx-dependent test assemblies (UtilitiesCS.Test, and QuickFiler.Test if present) pass on re-run after the `Directory.Build.props` addition, confirming no behavior change.
- [ ] Full toolchain pass completed for the touched languages: `dotnet tool run csharpier check .` (no formatting drift), both `msbuild TaskMaster.sln /t:Rebuild ...` commands above pass clean, and the `vstest.console.exe` regression re-run passes — no C# source changed, so this is regression re-verification, not new-test authorship.
- [ ] Before/after rebuild transcripts (and any workflow-syntax-check output) are captured as evidence under `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/` per the evidence-and-timestamp-conventions skill.

## Risks & Mitigations
- Technical or operational risks:
  - A root-level `Directory.Build.props` is auto-imported by all eighteen .csproj files in the solution (not just the five `System.Reactive`-consuming ones), so any future property added to this file affects every project. Mitigation: this fix adds only one property (`RxUseUnsupportedPackagesConfig`), which is confirmed (research §2.3) to be inert for the thirteen non-`System.Reactive` projects; any future addition to this file must be re-evaluated for solution-wide impact before merging.
  - The vendor guard target's exact `.targets` XML could not be read directly in this session because `packages/` is not restored in this worktree (research §2.2); the property name/effect rests on two independent repo-local corroborating sources rather than a live read of the vendor file. Mitigation: the pre/post rebuild-transcript verification in Test Strategy directly observes the real vendor guard target's behavior at build time, closing this gap empirically before the fix is accepted as verified.
  - A future change to GitHub Actions' `actions/cache@v4` `restore-keys` tie-break behavior is not repo-verifiable (research §1.2 item 6). Mitigation: the safety argument documented in the Finding 1 comment block holds regardless of which prior cache entry is selected, because it depends only on `nuget restore`'s per-package idempotency, not on cache tie-break semantics.
- Mitigations and rollbacks:
  - Both fixes are single-purpose, additive, and independently revertible (see Rollback/feature-flag considerations above): reverting either fix restores prior behavior exactly (three workflow files lose their comment block; `Directory.Build.props` is deleted and the five warnings return), with no other cleanup required.

## Rollout & Follow-up
- Release/rollout steps:
  - Land both fixes together in a single PR against `bug/ci-build-infra-debt-730` (based on `origin/main`), since they are independent, low-risk, configuration-only changes already scoped to one consolidated issue.
  - No staged rollout, feature flag, or environment-specific sequencing is required — both fixes take effect on the very next CI run and the very next local rebuild after merge.
- Post-fix monitoring or clean-up tasks:
  - Confirm on the first post-merge CI run of each of the three named workflows that the cache/restore step still behaves identically (cache hit/miss/fallback tiers unaffected).
  - Confirm on the first post-merge full solution rebuild (local or CI) that the five System.Reactive.PackagesConfigCheck warnings no longer appear in build output.
  - No further clean-up task is anticipated; this issue does not open or require any follow-up issue.
- Links: issue #730 (`https://github.com/drmoisan/TaskMaster/issues/730`); research at path docs/features/active/2026-09-02-ci-build-infra-debt-730/research/research.2026-09-02T09-15.md; prior related issues #569, #570, #395, #561, #562, #563.
