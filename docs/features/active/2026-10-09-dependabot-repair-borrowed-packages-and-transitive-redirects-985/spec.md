# 2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects (Spec)

- **Issue:** #985
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-09T14-30
- **Status:** Draft
- **Version:** 0.2

## Context

Grouped NuGet Dependabot pull requests (PR #984) still fail CI after the `dependabot-repair` workflow (issue #911) runs successfully, because two classes of manifest inconsistency are invisible to both Dependabot and the repair script:

1. Test projects that hint-path a package they do not declare in their own `packages.config` (borrowed from a sibling production project).
2. `app.config` binding redirects for assemblies that a project receives only transitively (no `packages.config` entry and no direct `<Reference>`).

All failures must be remediated automatically, with no manual step on any future Dependabot PR.

Environment:
- OS/version: GitHub Actions `windows-latest` (CI) and Windows 11 (local)
- Toolchain: PowerShell 7 repair scripts, .NET Framework 4.8.1 / packages.config projects
- Workflow: `.github/workflows/dependabot-repair.yml` (`workflow_run` trigger) invokes `scripts/dependencies/Repair-PackageManifestConsistency.ps1` without `-CandidateUpgrade`
- Data source: PR #984 (branch `dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9`); CI run #1059 (run id 37959420911) on repair commit fff92fe83

Impact / Severity:
- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

## Repro & Evidence

Steps to Reproduce:
1. Let Dependabot open a grouped NuGet update that bumps Microsoft.Web.WebView2 and log4net (PR #984).
2. Let `dependabot-repair` run after CI; it repairs `<Analyzer Include>` paths and pushes commit fff92fe83 as `taskmaster-dependabot-repair[bot]`.
3. Observe CI run #1059 on that commit.

Expected:
After the repair workflow runs, every required CI check (build-analyzers, build-nullable, mstest-coverage, pester, format-check, actionlint, hygiene) passes on the Dependabot branch with no human edit. Every project that compiles against a package declares that package in its own `packages.config`, and every `bindingRedirect newVersion` equals an assembly version actually referenced in the solution.

Actual:
- build-analyzers, build-nullable and mstest-coverage fail. `UtilitiesCS.Test` and `QuickFiler.Test` hint-path `packages\Microsoft.Web.WebView2.1.0.4191.47\...` without declaring WebView2 in their own `packages.config`. After the production projects move to 1.0.4258.31 nothing restores 4191.47, producing `CS0012` / `CS0234` / `CS0246` (for example `UtilitiesCS.Test\HelperClasses\ThemeHelpers\ThemeTests.cs(284,29): error CS0012: The type 'WebView2' is defined in an assembly that is not referenced`).
- The same latent pattern exists on main for `ObjectListView.Official 2.9.1` in `QuickFiler.Test` and `TaskTree.Test`.
- pester fails. `BindingRedirectVerification.Tests.ps1` reports `log4net|3.4.0.0` because Tags.Test, TaskTree.Test, TaskVisualization.Test, ToDoModel.Test and VBFunctions.Test keep `newVersion="3.4.0.0"` while every other project moved to 3.5.0.0. Those five projects have no log4net `packages.config` entry or `<Reference>`, so neither Dependabot nor the repair script edits them. The repair's redirect pass does not run from the `workflow_run` trigger because `-CandidateUpgrade` is not supplied (see `dependabot-repair.yml` lines 87-94).

Orchestrator verification (2026-10-09, CI run #1059 and the PR #984 diff):
- The only compiler errors are `CS0234` / `CS0246` / `CS0012` for WebView2 in `QuickFiler.Test` and `UtilitiesCS.Test`. No `MSTEST0032` or `CS0618` diagnostic appears.
- PR #984 dropped no `<Compile>`, `<EmbeddedResource>` or `<ProjectReference>` items.
- Pester fails only on the `log4net|3.4.0.0` transitive redirect.
- The research record's unverified risk rows for analyzer severity, obsolete APIs and dropped `<Compile>` items are therefore closed and out of scope.

Logs / Screenshots:
- Snippet: `Expected 0, because every bindingRedirect newVersion must equal a csproj Reference version (issue 973 emptied the recorded known-debt set; a new stale pair is fixed, not recorded); observed: log4net|3.4.0.0, but got 1.`

## Scope & Non-Goals

- In scope:
  - Declare the borrowed packages in the owning test projects' `packages.config` and remove the duplicate WebView2.Core reference in `QuickFiler.Test.csproj`.
  - Add `scripts/dependencies/BindingRedirectSync.psm1` and wire it into `Repair-PackageManifestConsistency.ps1`.
  - Add a Pester gate that fails when any project hint-paths a package it does not declare.
  - Pester coverage for the new module and the wiring, using in-memory fixtures.
- Out of scope / non-goals:
  - Any change under `.github/workflows/**`, including `.github/workflows/README.md`. A diff there triggers `modified-workflow-needs-green-run`, which cannot be satisfied for a `workflow_run`-only workflow with a `dependabot/` branch filter and no `workflow_dispatch`.
  - Supplying `-CandidateUpgrade` from the workflow, declaring log4net in the transitive-only test manifests, deleting transitive redirects, and auto-synthesising manifest entries for borrowed hint paths (all rejected in research section 3).
  - Edits to `ConsistencyVerifier.psm1`, `ProjectConsistency.psm1` and `BindingRedirectVerification.psm1`.
  - Mitigating a future upstream package release that introduces a new warning-severity compiler diagnostic.
- Explicitly excluded systems, integrations, or datasets: GitHub Actions workflow definitions, Dependabot configuration, and production C# source.

## Root Cause Analysis

Dependabot's packages.config updater and `Repair-PackageManifestConsistency.ps1` both act only on packages a project declares in its own `packages.config`. Borrowed hint paths and transitive binding redirects fall outside that set. The verifier (`ConsistencyVerifier.psm1` `Find-OrphanedHintPath`) already detects borrowed hint paths but is not a failing gate on main.

The existing redirect pass in `Repair-PackageManifestConsistency.ps1` runs only when `-CandidateUpgrade` yields applied upgrades, and is scoped to the app.config beside a manifest that declares the upgraded package. It therefore cannot reach a project that receives an assembly only transitively, even if `-CandidateUpgrade` were supplied.

## Proposed Fix

### Design summary (what changes where)

1. Manifest declarations (borrowed packages), each at the version the production sibling declares:
   - `QuickFiler.Test/packages.config`: `Microsoft.Web.WebView2` and `ObjectListView.Official`.
   - `UtilitiesCS.Test/packages.config`: `Microsoft.Web.WebView2`.
   - `TaskTree.Test/packages.config`: `ObjectListView.Official`.
   - Entries keep the inline NuGet form, sorted by id, with the same `targetFramework` as the sibling entries.
2. `QuickFiler.Test/QuickFiler.Test.csproj`: delete the second `ItemGroup` holding the duplicate `Microsoft.Web.WebView2.Core` `<Reference>`. Existing HintPaths already name the declared folder and version.
3. New module `scripts/dependencies/BindingRedirectSync.psm1` (a solution-wide, unconditional redirect-sync pass), called from `Repair-PackageManifestConsistency.ps1`.
4. New `It` block in `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` that runs `Find-OrphanedHintPath` over every project directory.

### Boundaries and invariants to preserve

- Invariant: after the sync pass, every `bindingRedirect` `newVersion` is a member of the solution-wide csproj `<Reference>` version map, with `oldVersion` equal to a lower bound plus the same upper bound. Fixed-state `app.config` files are rewritten to zero further changes on a second run.
- The pass is stale-only. A redirect whose `newVersion` already belongs to the deployed version set is never modified, which preserves curated ranges (for example the Fizzler redirects).
- Rewrites change only attribute values inside a matched `<dependentAssembly>` block, by reusing `Invoke-BindingRedirectReconciliation`. Line terminators and every other byte are unchanged.
- The sync pass runs after the per-project repair loop and before manifest normalization, so normalization remains the last writer.
- Sync records are not added to `Verification[].Report.Repair`, so the `beyond-known-weak` semantics and the comment in `dependabot-repair.yml` stay accurate and no YAML change is required.
- Unverifiable and unresolvable names are non-fatal. They do not affect `IsSuccess`, so other repairs are still pushed. The CI Pester gate is the backstop.

### Dependencies or blocked work

- None blocking. The end-to-end `workflow_run` path cannot be exercised before merge, so the CI-dependent criterion is verified after merge.
- After merge, `@dependabot recreate` is posted on PR #984. `@dependabot rebase` is refused because a non-Dependabot commit is on the branch.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

1. `QuickFiler.Test/packages.config`, `UtilitiesCS.Test/packages.config`, `TaskTree.Test/packages.config`.
2. `QuickFiler.Test/QuickFiler.Test.csproj`.
3. `scripts/dependencies/BindingRedirectSync.psm1` (new).
4. `scripts/dependencies/Repair-PackageManifestConsistency.ps1` (import, call, result and body wiring only).
5. `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` (new).
6. `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` (new; keeps the existing 429-line test file under the file-size limit).
7. `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` (one new `It`).

#### Functions/classes/CLI commands impacted

New exported functions in `BindingRedirectSync.psm1`:

- `Invoke-BindingRedirectSync -AppConfigText <string> -DeployedVersionProvider <scriptblock> [-PreferredVersionProvider <scriptblock>]`. Pure over text. Returns `Text`, `Repair[]` (Kind `BindingRedirectSync`, AssemblyName, From, To, Rule), `Unverifiable[]`, `Unresolvable[]` and `ExaminedCount`. For each redirect with a non-empty `NewVersion`:
  - Deployed set empty: the name is added to `Unverifiable`; no change.
  - Deployed set contains `NewVersion`: no change (idempotence).
  - Otherwise the target is, in order: (a) the single own-reference version from `PreferredVersionProvider` when exactly one exists and it is in the deployed set; (b) the maximum deployed version by `[System.Version]`; (c) if any candidate fails `[System.Version]::TryParse`, the name is added to `Unresolvable` and nothing changes.
  - The target is applied with `Invoke-BindingRedirectReconciliation`.
- `Invoke-SolutionBindingRedirectSync -DirectoryLister -TextReader -TextWriter [-ProjectTextOverride <hashtable>]` with `SupportsShouldProcess`, mirroring `Invoke-ManifestNormalization`. Builds the map with `ConvertTo-ReferenceVersionMap` over every csproj (preferring post-repair text in the override), processes every app.config, and writes only changed text when `ShouldProcess` approves. Returns `ChangedPath[]`, `Repair[]`, `Unverifiable[]`, `Unresolvable[]` and `ExaminedAppConfig`.
- `Format-BindingRedirectSyncReport -Repair <object[]>`. Returns a `## Binding redirects synchronised` block, or an empty string when there are no repairs.

Composition root changes in `Repair-PackageManifestConsistency.ps1`: import the module without `-Force`; record the repaired text per project path in the main loop; call `Invoke-SolutionBindingRedirectSync` after the loop and before normalization with `-WhatIf:$WhatIfPreference` passed explicitly; add `ChangedPath` to the written set; add a `RedirectSync` result field (Repair, Unverifiable, Unresolvable); append the formatted block to `Body`.

#### Data flow and validation changes

The existing `-CandidateUpgrade` pass keeps its behaviour and writes first when both run. The sync pass then finds `newVersion` already in the map and changes nothing. Written app.config paths reach the workflow's push through `WrittenPath` without YAML edits.

#### Error handling and logging updates

- Pure functions throw only through `ConvertFrom-AppConfigText`, which rejects non-configuration text.
- The I/O wrapper reports skipped names in the result object and emits one `Write-Information` summary line with `-InformationAction Continue`, following the precedent in the repair script.

#### Rollback/feature-flag considerations

None. The pass is idempotent and reverts by reverting the commit. No flag is introduced.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

See the function contracts above. The `RedirectSync` result field is additive to the repair script's result object.

#### Required configuration keys and defaults

None.

#### Backward-compatibility expectations

Existing contexts in `Repair-PackageManifestConsistency.Tests.ps1` stay green without edits, notably the agreeing-tree case (no written paths), the `-WhatIf` case and the SVGControl-scoped `-WhatIf` case. `Verification[].Report.Repair` contents are unchanged.

#### Performance constraints (latency/throughput/memory)

None beyond a single additional read of each csproj and app.config in the tree; the pass is text-only.

## Assumptions, Constraints, Dependencies

- Assumptions: MSBuild reference conflict resolution prefers a primary reference and otherwise the highest version; the design mirrors this and this behaviour was not re-verified in research.
- Constraints:
  - Every PowerShell file stays within the repository file-size limit; `Repair-PackageManifestConsistency.ps1` has little headroom, so it receives wiring only.
  - `.csharpierignore` excludes `packages.config`, `app.config` and csproj files, so manifest edits are not checked by format-check; the canonical manifest form is enforced by `Invoke-ManifestNormalization`.
  - No file under `.github/workflows/**` may change.
- External dependencies: Dependabot's grouped packages.config update (`dependabot.yml` groups every directory into one group), PoshQC MCP tools for the PowerShell toolchain.

## Data / API / Config Impact

- User-facing or API changes: new `RedirectSync` field in the repair script's result; new `## Binding redirects synchronised` section in the repair body.
- Data or migration considerations: none. After merge, the five transitive log4net redirects on the Dependabot branch move to the version already referenced elsewhere when the repair re-runs.
- Logging/telemetry updates: one `Write-Information` summary line from the sync wrapper.
- Compatibility notes: no CLI flag, config schema or workflow change.

## Test Strategy

All fixtures are in-memory strings and hashtable stores; no temporary files. Tests use Pester 5; the toolchain runs through the PoshQC MCP tools (format, analyze, test).

- `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` (new):
  - `Invoke-BindingRedirectSync`: stale transitive redirect rewritten in both positions with the lower bound preserved; single-version `oldVersion` replaced outright; current redirect unchanged and a second pass produces zero repairs; unknown name reported unverifiable and unchanged; several deployed versions with an own reference prefers the own reference; several deployed versions without an own reference selects the highest by `[System.Version]` (use `3.10.0.0` versus `3.9.0.0` to prove numeric rather than string ordering); unparsable version reported unresolvable and unchanged; block without `bindingRedirect` skipped; empty text examines nothing; non-configuration text throws; CRLF bytes outside the substituted span are byte-identical.
  - `Invoke-SolutionBindingRedirectSync`: a two-project store (production csproj referencing the newer version; test directory with only an app.config at the older version); override text preferred over reader text; `-WhatIf` writes nothing; `ChangedPath` is exact.
  - `Format-BindingRedirectSyncReport`: empty input gives an empty string; non-empty input gives a heading plus one line per repair.
- `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` (new), reusing the existing fixture-store pattern: a run with no `-CandidateUpgrade` rewrites the transitive redirect; `WrittenPath` contains the app.config; `Body` carries the new block; `Verification[].Report.Repair` has no `BindingRedirectSync` kind; a second run writes nothing.
- `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`: new `It` asserting that the examined count is positive and `Find-OrphanedHintPath` reports no findings across every project directory, listing any finding as `<project>: line <n> <PackageFolder>`. Fail-first evidence: run before the manifest edits (findings expected) and after (none), recorded under `evidence/regression-testing/`.
- Coverage: target at least 90% line coverage on the new module (new-code policy), no regression on changed composition-root lines.
- Toolchain commands: PoshQC format, then analyze, then test for PowerShell; the repository C# toolchain order from CLAUDE.md (CSharpier, analyzer rebuild, nullable rebuild, MSTest with coverage) because csproj and manifest files change.
- Manual validation: local integration rehearsal described in the last verification criterion below.

## Acceptance Criteria

Evidence classes: criteria 1 to 5 are verified before merge. Criterion 6 is verification evidence produced locally; it is not a CI dependency. Criterion 7 is CI-dependent (pending-CI) and is checked off after merge. Numeric populations are deliberately not asserted in these criteria; the research record's numeric derivation is not used as an acceptance assertion.

- [x] AC1 Borrowed packages declared: `QuickFiler.Test/packages.config` declares `Microsoft.Web.WebView2` and `ObjectListView.Official`; `UtilitiesCS.Test/packages.config` declares `Microsoft.Web.WebView2`; `TaskTree.Test/packages.config` declares `ObjectListView.Official`. Each entry is at the version the production sibling manifest declares and matches the folder named by the project's existing HintPaths. The duplicate `Microsoft.Web.WebView2.Core` `<Reference>` item group is removed from `QuickFiler.Test/QuickFiler.Test.csproj`, and exactly one reference to that assembly remains there.
- [x] AC2 `scripts/dependencies/BindingRedirectSync.psm1` exists and exports `Invoke-BindingRedirectSync`, `Invoke-SolutionBindingRedirectSync` and `Format-BindingRedirectSyncReport`. Pester tests pin each of these behaviours: stale-only rewrite (a redirect whose `newVersion` is already a deployed version is left unchanged); target selection prefers the project's own reference version and otherwise the highest version by `[System.Version]` comparison; assemblies with no deployed version, or with an unparsable version, are left unchanged and reported as `Unverifiable` or `Unresolvable`; a second run produces no repairs and no writes (idempotent). `Repair-PackageManifestConsistency.ps1` invokes the pass unconditionally, including when `-CandidateUpgrade` is not supplied, exposes the outcome in a separate `RedirectSync` result field, does not add sync records to `Verification[].Report.Repair`, and remains within the repository file-size limit for production scripts.
- [x] AC3 Main-branch gate: a Pester `It` in `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` asserts that `Find-OrphanedHintPath` reports no findings across all projects on main, and that the examined count is positive so the assertion cannot pass vacuously. The fail-first run (before the manifest edits, findings reported) and the passing run (after) are recorded under `evidence/regression-testing/`.
- [x] AC4 No file under `.github/workflows/**` is modified by this change, verified by an empty `git diff --name-only` against the merge base filtered to that path.
- [x] AC5 Pester tests use in-memory fixtures only and create no temporary files. Line coverage of the new module meets the repository new-code coverage target. The full PowerShell toolchain (PoshQC format, analyze, test) passes in a single pass, and the C# toolchain from CLAUDE.md passes in a single pass.
- [ ] AC6 Local integration rehearsal (verification evidence, not a CI dependency): with this fix applied on top of the Dependabot branch of PR #984, running `Repair-PackageManifestConsistency.ps1` followed by NuGet restore and the nullable-gate MSBuild `/t:Rebuild` (with `/p:TreatWarningsAsErrors=true`) and the analyzer-gate MSBuild `/t:Rebuild` all succeed, and the `BindingRedirectVerification` Pester suite passes. The captured build logs and Pester summary are stored under `evidence/` per the evidence conventions, as projections rather than raw TRX or coverage documents.
- [ ] AC7 Pending-CI: after this change is merged to main and `@dependabot recreate` is commented on PR #984, every required CI check on that PR (build-analyzers, build-nullable, mstest-coverage, pester, format-check, actionlint, hygiene) passes with no human edit after the repair workflow runs, and the repair workflow's re-run after that green CI writes nothing. This criterion is checked off by the item's orchestrator run only after the CI result is observed.

## Risks & Mitigations

- Technical or operational risks:
  - The end-to-end `workflow_run` path cannot be exercised before merge; AC7 is the only proof of it.
  - Once WebView2 and ObjectListView are declared in the test projects, Dependabot's update rewrites their references and may add a WebView2 `.targets` import and a Wpf reference (unverified). Both are expected to be benign.
  - Highest-version target selection is a heuristic when several versions are referenced and the stale one is not among them; unparsable versions are reported, not guessed.
  - A future upstream release introducing a warning-severity compiler diagnostic would fail the nullable gate, which this fix cannot prevent.
- Mitigations and rollbacks:
  - Idempotent pass and a Pester gate that fails main if a test project borrows a package again.
  - The existing `BindingRedirectVerification` invariant remains the CI backstop for any redirect the pass reports as unverifiable.
  - Rollback is a revert of the commit; no flag or data migration is involved.

## Rollout & Follow-up

- Release/rollout steps: merge to main; post `@dependabot recreate` on PR #984 (not `rebase`); observe CI, the repair run and the idempotent re-run.
- Post-fix monitoring or clean-up tasks: confirm the repair run's body lists the synchronised redirects; confirm no new unverifiable names other than the known `netstandard` case.
- Links: issue #985, PR #984, CI run #1059, issue #911 (repair workflow), issue #973 (redirect curation), research `research/research.2026-10-09T14-10.md`.
