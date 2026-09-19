# dependabot-fanout-and-ci-failing-nuget-upgrades (Spec)

- **Issue:** #911
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-19
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug

> This document is the **sole authoritative acceptance-criteria source** for issue #911. Work mode
> is `full-bug`, so no user-story document exists and none may be created; per the
> acceptance-criteria-tracking skill (.claude/skills/acceptance-criteria-tracking/SKILL.md) a second
> checkbox-bearing file would split the criteria and break the check-off protocol.

> Formatting note for later editors: inline code spans around repository paths are load-bearing.
> A downstream tool derives the change footprint from backticked paths. Every file this change
> writes is backticked at least once (see `## Write Set`); every file named only for comparison is
> deliberately left unbackticked. Do not "fix" that inconsistency.

---

## Context

Dependabot opens one pull request per configured group each cycle, and those pull requests fail the
required CI checks, so dependency upgrades are effectively unmergeable without manual repair.
Measured history recorded in the issue document: 14 of 59 Dependabot pull requests have ever merged, and
none since 2026-08-21; every merge that landed carried human repair commits.

The cause is **not** that Dependabot fails to maintain `.csproj` state. It maintains it — Dependabot
invokes the NuGet CLI update command in-process, so it rewrites `<Import>`, `<Error>`, `<Reference>`,
`<HintPath>` and the `app.config` binding redirects. The cause is that while updating one group it
**also** rewrites the package-import guards of packages **outside** that group to a version that no
`packages.config` in the repository declares. Restore honours the manifest, the build honours the
project file, and the package-imports `<Error>` target fails closed.

Environment:

- OS/version: Windows 11 Pro 10.0.26200.
- .NET Framework 4.8.1 VSTO solution; 18 non-SDK projects, each with a `packages.config`.
- 162 `<Analyzer Include>` items across 17 of those projects (SVGControl carries none).
- Required checks are the set declared by repository ruleset 18572843, with
  `strict_required_status_checks_policy: true`.
- `.github/workflows/_pester.yml` **does exist** on `origin/main` and the
  `pester / Run Pester suite with coverage` check ran and passed on pull request #908. It is not one
  of the checks the ruleset marks required, but it does execute on every pull request. (An earlier
  statement that no Pester workflow existed was read from a session worktree 243 commits behind
  `origin/main` and was incorrect.)
- **However, that job is scoped to one directory and would not execute this change's tests.** It
  hard-codes `Run.Path = 'tests/scripts/vscode'` (line 41) and
  `CodeCoverage.Path = 'scripts/vscode'` (line 45). Tests added under `tests/scripts/dependencies/`
  would never run, and the job would report green while measuring none of the new code — an instance
  of the exact vacuous-gate failure this specification is written to avoid. Widening both paths is
  therefore in scope and is a precondition for treating any Pester-evidenced criterion below as
  CI-gated. Until that widening lands, Pester evidence is local-only.
- The `pester` job enforces a line-coverage floor of 80 (line 71), while `.claude/rules/` state 85.
  This specification asserts nothing about that discrepancy; it is recorded so the gap is not
  mistaken for a property this change establishes.

Impact / Severity: **High**. Dependency upgrades, including security-relevant ones, cannot land.
Separately, `main` is one cache eviction away from an unbuildable state: the stale
`<Analyzer Include>` paths resolve today only through the build workflows' cache `restore-keys:`
prefix fallback, and analyzers are silently disabled in the 15 affected projects.

---

## Repro & Evidence

Steps to reproduce:

1. Allow the weekly Dependabot NuGet schedule to run against `main`.
2. Observe one pull request per configured group (four groups produced #907, #908, #909).
3. Open any one of them and inspect the required checks.
4. For a package that is **not** in that pull request's group, compare the version in
   `packages.config` against the version in the sibling `.csproj` `<Import>` element.

Verified on pull request #908 (the `test-frameworks` group), run 35264873270. It changed 30 files
across ten project directories: 10 project files, 10 `app.config`, 10 `packages.config`. In nine of
those project files it moved Meziantou.Analyzer — which belongs to the `analyzers-dev-deps` group,
not that one — from 3.0.235 to 3.0.259 in `<Import>` and `<Error>`, while leaving `packages.config`
at 3.0.235. The resulting three-way divergence inside a single project:

| Location | Version |
|---|---|
| `packages.config` | 3.0.235 (unchanged) |
| project file `<Import>` / `<Error>` | 3.0.259 (rewritten, out of scope) |
| project file `<Analyzer Include>` | 3.0.203 (never rewritten by anything) |

Restore honoured the manifest and fetched 3.0.235; Meziantou.Analyzer.3.0.259 is absent from the
restore log. MSBuild then failed in nine projects:

```
error : This project references NuGet package(s) that are missing on this computer.
The missing file is ..\packages\Meziantou.Analyzer.3.0.259\build\Meziantou.Analyzer.props.
```

The compiler command line in the same log carries an analyzer switch naming
Meziantou.Analyzer.3.0.203 under the roslyn5.0 C-sharp analyzer folder, confirming the third
version is live in the compile. Separately, the formatting check rejected all
20 touched config files, because Dependabot writes them inline while the committed form is reflowed.

---

## Scope & Non-Goals

In scope:

- Dependabot configuration consolidation.
- A repair pass that runs on Dependabot pull requests and makes their trees internally consistent.
- The four prerequisite corrections (#898, #902, #903, formatting-scope) plus a one-time
  normalisation of the config manifests and a pinned NuGet CLI version.

Out of scope, deliberately unbackticked so the footprint harvester does not claim them:

- .github/workflows/ci.yml. Its existing pull_request trigger on base branches main and development
  already produces the required checks on a Dependabot pull request; no change is needed, and
  widening it or adopting pull_request_target is rejected below.
- scripts/vscode/Invoke-VSBuild.ps1. It continues to invoke the reference-sync script unchanged;
  only the script's framework-ranking data moves.
- Replacing Dependabot with direct NuGet V3 API discovery. That would require reimplementing the
  semver-major ignore rules and security-advisory awareness; revisit only if the repair pass proves
  unreliable.
- Upgrading the setup-nuget action from v2 to v3.
- Any package upgrade itself. This change makes upgrades landable; it does not land one.

---

## Root Cause Analysis

Four defects, in descending order of consequence.

- **D1 — out-of-scope project-file rewrites.** Dependabot writes `<Import>`/`<Error>` versions for
  packages outside the pull request's declared group, to a version no manifest declares. Direct
  cause of the build failure. Why it selects that version is an inference; the divergence and its
  consequence are verified.
- **D2 — `<Analyzer Include>` is never rewritten by anything.** The NuGet CLI's project system
  contains no analyzer-item logic, and pull request #908 contains zero `<Analyzer Include>` lines.
  This is issue #898. The naive mapping `analyzers\dotnet\cs\<Id>.dll` is wrong for three of the
  five analyzer families in use: Meziantou uses `dotnet\roslyn5.0\cs`, Roslynator uses
  `dotnet\roslyn4.7\cs` with four mangled assembly names, and SonarAnalyzer uses a bare `analyzers`
  directory. A repair must therefore enumerate the restored package on disk rather than compute the
  path, and must preserve the sibling `<AdditionalFiles>` element that supplies the banned-symbols
  list, since dropping it silently disables that analyzer.
- **D3 — formatting.** The committed form of `packages.config` and `app.config` is reflowed;
  Dependabot writes both inline. `.csharpierignore` currently excludes neither.
- **D4 — fan-out.** Four groups produce four pull requests. Grouping already consolidates across
  directories — #908 spans ten — so a single group yields a single pull request. The `directories`
  glob is not the multiplier.

Adjacent defects folded in so the repaired pipeline passes on its first run: #898 (15 stranded
analyzer sites), #902 (`netstandard2.1` ranked above `netstandard2.0` in the local reference-sync
script, which would reintroduce #895 on the next local build; net481 cannot consume
`netstandard2.1` at all), and #903 (`ToDoModel.Test/packages.config` omits packages for which the
project file carries `<HintPath>` entries).

Execution constraint: a push made with the default Actions token does not re-trigger workflows, and
checks produced by `workflow_dispatch` or `workflow_run` runs are not evaluated against a pull
request's required-status-check policy. A self-repairing pull request therefore requires a GitHub
App installation token for the push, so that the resulting `synchronize` event re-runs the required
checks under the `pull_request` event.

---

## Proposed Fix

### The central invariant

> For every project, the package version recorded in `packages.config` is the single source of
> truth, and every dependent element in the sibling project file (`<Import>`, `<Error>`,
> `<Reference>`, `<HintPath>`, `<Analyzer Include>`) and in `app.config` (binding redirects) must
> agree with it.

Every gate below is verified against that sentence. A repair is any edit that moves a dependent
element into agreement with its manifest; a failure is a dependent element that still disagrees
after all repairs have run, or a manifest entry that no dependent element can be reconciled to.

Trace of one accepted value through the invariant, using the #908 state for Meziantou.Analyzer in
one project: the manifest declares 3.0.235, so 3.0.235 is accepted as truth. The `<Import>` and
`<Error>` guards read 3.0.259 — rewritten to 3.0.235 (repair, class D1). The `<Analyzer Include>`
item reads 3.0.203 — regenerated from the restored directory `packages\Meziantou.Analyzer.3.0.235\`
so it reads 3.0.235 with the `dotnet\roslyn5.0\cs` segment re-derived from disk (repair, class D2).
The manifest itself is never edited by the repair. If the restored directory for 3.0.235 does not
exist, no path can be derived and the verifier throws rather than emitting a guessed path.

### Design summary

Dependabot remains the upgrade engine. The change is a **repair pass over Dependabot's own pull
request**, not a replacement pipeline.

1. **Consolidation.** `.github/dependabot.yml` collapses to one catch-all group with
   `applies-to: version-updates`, `open-pull-requests-limit: 1`, Deedle ignored entirely, the
   pre-existing semver-major ignore entries retained unchanged, and the inert `group-by` keys
   removed.
2. **Repair workflow.** `.github/workflows/dependabot-repair.yml` runs automatically for Dependabot
   pull requests, restores packages, applies the repair passes, verifies, commits, and pushes onto
   Dependabot's existing branch with a GitHub App installation token.
3. **Repair passes**, in this order, because each consumes the previous one's output: asset-level
   compatibility gate; version reconciliation (D1); analyzer-item regeneration (D2); binding-redirect
   reconciliation; config normalisation (D3); verification.
4. **Prerequisites** land in the same change so the verifier's first run is a clean pass.

### Trigger mechanism and the credential

The repair workflow must hold a credential that can push. Workflow runs triggered directly by a
Dependabot `pull_request` event receive a read-only Actions token and no access to repository
secrets, so a bare `pull_request` trigger cannot satisfy the design. The repair workflow therefore
triggers on completion of the CI workflow run for a head branch under the Dependabot branch prefix,
which executes in the base-branch context with access to secrets and write permissions.
`pull_request_target` is rejected: it is a security regression for a convenience gain, and GitHub is
restricting it by default from 2026-11-02.

This mechanism claim is an **assumption of record**, not a measured fact in this repository: no
workflow here consumes a secret today. Acceptance criterion AC19 is written as an outcome assertion
against the fixture pull request precisely so that a wrong mechanism choice fails visibly rather
than silently producing a parked check.

### Resolved tension: `.csharpierignore` versus a formatter post-pass

The issue document requires both `.csharpierignore` coverage for `packages.config` and `app.config`
**and** formatting of those files in the repair pass. Those two are in tension: once a path is
ignored, the formatter will not rewrite it, so "CSharpier formatting" cannot be the mechanism that
normalises it. The research artifact recommended dropping the `.csharpierignore` change and keeping
the formatter post-pass; the issue document is authoritative and requires the ignore entries.

Resolution adopted here, and stated explicitly so the planner does not choose silently:

- `.csharpierignore` gains patterns for `packages.config` and `app.config`. This removes the class
  of formatting failure entirely, including on cycles where the repair pass does not run.
- Because the formatter no longer defines a canonical form for those files, the repository adopts
  the **inline** form — the form the NuGet CLI and Dependabot emit — as canonical, and a
  deterministic, idempotent normaliser in `scripts/dependencies/PackageGraph.psm1` enforces it. Bot
  edits then produce minimal diffs and the normaliser is a no-op on bot output.
- The one-time normalisation converts all 18 `packages.config` files and the sibling `app.config`
  files to that canonical form in this change.

This is a scope decision made by this specification, not by the issue document. It is called out in
the report accompanying this document.

### Files and modules to change

New PowerShell, decomposed so each file stays cohesive and under the repository line ceiling, with
every filesystem and process dependency behind an injectable seam per the PowerShell rule file
(.claude/rules/powershell.md):

- `scripts/dependencies/PackageGraph.psm1` — parses `packages.config`, project files and
  `app.config` into structures; renders the canonical inline form; pure over text.
- `scripts/dependencies/PackageCompatibility.psm1` — asset-level framework selection for net481.
  Single implementation, shared with the local reference-sync script (this is the #902 fix).
- `scripts/dependencies/AnalyzerItemRepair.psm1` — derives the `<Analyzer Include>` set from an
  injected directory listing; rewrites the owning item group.
- `scripts/dependencies/ProjectConsistency.psm1` — version reconciliation across `<Import>`,
  `<Error>`, `<Reference>` and `<HintPath>`; binding-redirect reconciliation; the verifier and its
  inconsistency report.
- `scripts/dependencies/Repair-PackageManifestConsistency.ps1` — composition root and command-line
  entry point; emits the repairs report consumed by the pull-request body.
- `scripts/vscode/Sync-PackageReferences.ps1` — consumes the shared compatibility module instead of
  its own ranking array.

Configuration and workflow changes: `.github/dependabot.yml`, the new
`.github/workflows/dependabot-repair.yml`, the NuGet version pin in
`.github/workflows/_build-analyzers.yml`, `.github/workflows/_build-nullable.yml` and
`.github/workflows/_mstest-coverage.yml`, documentation in `.github/workflows/README.md`, and
`.csharpierignore`.

### Error handling and disclosure

- A repair that cannot be derived (for example, an analyzer package whose restored directory is
  absent) throws rather than emitting a guessed path.
- An incompatible package is **skipped with a recorded reason** and the remaining upgrades proceed;
  a skip is never a run failure.
- The verifier repairs freely and fails only when the post-repair tree is still inconsistent,
  naming the specific condition and project.
- The pull-request body gains a "Repairs applied" block and, when any package was skipped, a
  "Packages skipped" block. The `deps:autofixed` label is applied when a repair outside the two
  known-weak classes (analyzer items, binding redirects) was applied.

---

## Assumptions, Constraints, Dependencies

Assumptions:

1. Workflow runs triggered by a Dependabot `pull_request` event receive a read-only token and no
   repository secrets. Documented GitHub behaviour, not measured here; AC19 falsifies a wrong choice.
2. The Roslyn version of MSBuild on the CI runner is at least the highest Roslyn-qualified folder
   any analyzer package in use ships, so selecting the highest available folder is correct. If
   false, the analyzer build gate surfaces the error.
3. The ruleset's required-check entries are not scoped to a specific GitHub App id. Supplied as
   established; unaffected either way, since all required checks are produced by GitHub Actions.
4. The repository will accept its first repository secret and its first workflow with write
   permissions. This is a governance posture change, not only a technical one.
5. Dependabot will not split the consolidated pull request. The repository is currently version-
   consistent, and the repair pass tolerates more than one open Dependabot pull request.

Constraints:

- net481 consumes assets up to `netstandard2.0` and cannot consume `netstandard2.1`. The
  incompatible framework must be excluded outright, not ranked last.
- A push made with the default Actions token does not re-trigger workflows; `workflow_dispatch` and
  `workflow_run` runs do not satisfy required-status-check policy.
- Repository-wide: temporary files in tests are prohibited. All fixtures are in-memory strings and
  hashtables, with the directory listing supplied through an injected delegate.
- The PowerShell change budget caps a batch at three production and three test files; this change
  exceeds that and must be split into batches by the planner.

External dependencies: the GitHub App installation-token action, and a repository-admin action to
create the App and store its credentials. That admin action is one-time and cannot be automated.

---

## Test Strategy

Three pure cores carry the unit coverage, all testable without network or filesystem access:

1. Framework/asset selection (`PackageCompatibility.psm1`).
2. Analyzer path derivation (`AnalyzerItemRepair.psm1`), over an injected directory listing.
3. Project consistency evaluation and reconciliation (`ProjectConsistency.psm1`).

Integration coverage is the #908 fixture replay plus one live fixture Dependabot pull request.

Gate credibility rule, applied to every criterion below: each gate must be **observed failing** on a
deliberately broken input before it is trusted, and the failing input must be reachable from where
the check runs. Where a criterion asserts that a bad state is absent, it is paired with a positive
assertion that names what was examined, so that a detector which never fires is distinguishable from
a clean tree.

Cold-cache verification is a **local** step. It cannot be a CI criterion: the build workflows' cache
key falls back through a bare `restore-keys:` prefix, so CI structurally cannot observe a cold-cache
failure and would report green either way.

---

## Acceptance Criteria

Evidence for every criterion is written under
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/, in
the baseline, qa or regression subdirectory as indicated, per the evidence and timestamp
conventions skill (.claude/skills/evidence-and-timestamp-conventions/SKILL.md).

- [ ] **AC1 — Dependabot configuration is consolidated.** `tests/scripts/dependencies/DependabotConfig.Tests.ps1`
      parses `.github/dependabot.yml` and asserts, as separate positive assertions: exactly one
      entry under `groups`; that entry declares `applies-to: version-updates` and a catch-all
      pattern; `open-pull-requests-limit` equals 1; an `ignore` entry for Deedle exists with neither
      a `versions` nor an `update-types` qualifier; and the set of (dependency-name, update-types)
      pairs carrying `version-update:semver-major` is exactly the set present at the merge-base
      commit, compared element-by-element against a literal expected set declared in the test.
      Evidence: Pester output under evidence/qa. Fails if any ignore entry is dropped, renamed or
      re-qualified, if a second group is added, or if a `group-by` key is reintroduced anywhere in
      the file.

- [ ] **AC2 — Config manifests are outside the formatting gate, proven positively.** `.csharpierignore`
      contains patterns matching `packages.config` and `app.config`. Verification runs
      `dotnet tool run csharpier check .` against a worktree in which one named `packages.config`
      and one named `app.config` have been transiently rewritten in single-line inline form **and** a
      named C# source file has been transiently perturbed; the captured output must report the
      perturbed C# file and must not report the two config files. Both perturbations are reverted
      after capture. Evidence: the captured command output under evidence/qa. The control proves the
      check was live; without it, a silent no-op run would read as a pass.

- [ ] **AC3 — All 18 manifests are normalised, and normalisation is idempotent.** After the one-time
      normalisation, running the normaliser in `scripts/dependencies/PackageGraph.psm1` over the
      working tree produces an empty `git diff`, and the normaliser reports having examined 18
      `packages.config` files. Evidence: the reported examined-file count and the empty diff, under
      evidence/qa. Fails if the normaliser is non-idempotent, or if it examines fewer than 18 files
      (which would mean the discovery glob, not the tree, is clean).

- [ ] **AC4 — The NuGet CLI version is pinned everywhere it is selected.** A Pester assertion in
      `tests/scripts/dependencies/DependabotConfig.Tests.ps1` enumerates every step in
      `.github/workflows/` that uses the setup-nuget action, asserts the enumerated count is
      greater than zero, and asserts that each such step declares a `nuget-version` that is an exact
      three-part version literal. `run-actionlint` passes on the changed workflow files. Evidence:
      Pester output and actionlint output under evidence/qa. Fails if any step reverts to a floating
      selector, and the greater-than-zero assertion prevents a broken enumerator from passing
      vacuously.

- [ ] **AC5 — Every analyzer item agrees with its manifest (#898).** A verifier invocation over the
      working tree reports zero analyzer-item version disagreements **and** reports having examined
      162 `<Analyzer Include>` items across 17 project files. Evidence: the verifier report under
      evidence/qa. The examined-count assertion is the non-vacuity guard: a detector that matched
      nothing would report zero disagreements and zero examined, and would fail this criterion.

- [ ] **AC6 — The cold-cache failure is observed before the fix and absent after.** Locally, on the
      merge-base tree with the solution `packages` directory deleted and restore re-run, the
      analyzer build command from CLAUDE.md fails with an error naming the
      Meziantou.Analyzer.3.0.203 analyzer assembly. On the fixed tree, the same procedure from the
      same cold state succeeds. Both logs are captured, the failing one under evidence/baseline and
      the passing one under evidence/qa. This criterion is deliberately local: the CI cache
      `restore-keys:` prefix fallback prevents CI from reaching the failing state.

- [ ] **AC7 — The incompatible framework is excluded, not ranked (#902).**
      `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` asserts positively that the
      selector returns `net481` when it is present; returns `net48` when `net481` is absent; returns
      `netstandard2.0` when offered `netstandard2.1` and `netstandard2.0` together; and returns no
      selection when offered only `netstandard2.1`, or only a .NET-Core-era framework, or an empty
      set. `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` asserts that
      `scripts/vscode/Sync-PackageReferences.ps1` resolves the same selection through the shared
      module and declares no framework ordering of its own. Evidence: Pester output under
      evidence/qa. Fails if the framework is merely demoted, because the "offered only
      netstandard2.1" case then returns a selection.

- [ ] **AC8 — Orphaned hint paths are eliminated and detectable (#903).** The verifier reports zero
      orphaned `<HintPath>` entries for `ToDoModel.Test/ToDoModel.Test.csproj` after
      `ToDoModel.Test/packages.config` gains the missing manifest entries, and a unit test in
      `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` asserts the same detector reports a
      non-empty orphan set for an in-memory fixture reproducing the pre-fix pair. Both directions are
      asserted in the same suite. Evidence: Pester output under evidence/qa.

- [ ] **AC9 — The compatibility gate is asset-level.** `tests/scripts/dependencies/PackageCompatibility.Tests.ps1`
      asserts the gate decides from the asset folders a candidate package actually ships, not from a
      declared framework attribute: given an asset set containing only frameworks net481 cannot
      consume, the gate returns a rejection carrying a reason string; given a set containing a
      consumable asset, it returns an acceptance naming the selected asset folder. Evidence: Pester
      output under evidence/qa.

- [ ] **AC10 — An incompatible package is skipped and the remaining upgrades proceed.** A test in
      `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` drives the entry point
      over an in-memory fixture with two candidate upgrades, one incompatible. It asserts all three
      of: the incompatible package's manifest version is unchanged; the compatible package's manifest
      version is the target version; and the returned report contains a skip record naming the
      incompatible package and a non-empty reason. Evidence: Pester output under evidence/qa. A
      fail-fast implementation fails the second assertion.

- [ ] **AC11 — Version reconciliation covers all four dependent element kinds (D1).**
      `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` asserts, with one assertion per
      element kind, that given a manifest version and project text in which `<Import>`, `<Error>`,
      `<Reference>` and `<HintPath>` each name a different version, the reconciled text has each of
      those four elements naming the manifest version. Evidence: Pester output under evidence/qa.
      Per-kind assertions make a reconciler that handles only two kinds fail rather than pass on an
      aggregate.

- [ ] **AC12 — Analyzer items are regenerated by enumerating the restored directory (D2).**
      `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1` supplies an injected directory
      listing and asserts the derived path set for each of the four shapes observed in this
      repository: a plain language-folder shape; a Roslyn-qualified shape; a multi-assembly shape
      whose assembly names do not match the package id; and a shape with no intermediate folders. It
      further asserts that a listing offering two Roslyn-qualified folders selects the higher; that
      non-C-sharp language folders and satellite resource assemblies are excluded; and that a package
      whose listing contains no analyzer directory contributes no items. Evidence: Pester output
      under evidence/qa. Fails for any implementation that computes the path from the package id.

- [ ] **AC13 — Sibling elements in the analyzer item group survive regeneration.** The same suite
      asserts that after regeneration the item group still contains the `<AdditionalFiles>` element
      naming the banned-symbols list and the explanatory comment that precedes the items, and that a
      project fixture with no analyzer item group at all — the SVGControl shape — is returned
      byte-identical with no item group synthesised. Evidence: Pester output under evidence/qa.

- [ ] **AC14 — Binding redirects are reconciled to the resolved assembly version.**
      `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` asserts that for an `app.config`
      fixture whose redirect names an older assembly version than the one resolved from the manifest,
      the reconciled text names the resolved version in both the upper bound of `oldVersion` and in
      `newVersion`; and that an `app.config` with no redirect for that assembly is returned
      unchanged. Evidence: Pester output under evidence/qa.

- [ ] **AC15 — The repair pass leaves a formatting-stable tree.** Running the repair entry point a
      second time over its own output produces an empty `git diff`, and
      `dotnet tool run csharpier check .` reports no findings on the post-repair tree. Evidence: the
      empty diff and the formatter output under evidence/qa. Fails on a non-idempotent normaliser or
      renderer.

- [ ] **AC16 — The verifier repairs freely and fails only on residual inconsistency.**
      `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` asserts both directions against the
      entry point: a fixture whose every divergence is repairable returns a success result whose
      report enumerates the repairs performed; a fixture carrying a divergence no repair can resolve
      returns a failure result naming that condition and the project. Evidence: Pester output under
      evidence/qa. The failing direction is the criterion that proves the verifier is not a
      pass-through.

- [ ] **AC17 — The repair workflow exists and is statically valid.**
      `.github/workflows/dependabot-repair.yml` is present, passes `run-actionlint`, declares the
      write permissions it needs, and restricts its work to head branches under the Dependabot
      branch prefix. A Pester assertion in `tests/scripts/dependencies/DependabotConfig.Tests.ps1`
      asserts the branch restriction is present and that the workflow does not use
      `pull_request_target`. Evidence: actionlint and Pester output under evidence/qa.

- [ ] **AC18 — The repair commit is pushed under the GitHub App identity.** On the fixture Dependabot
      pull request, the head commit after the repair run is authored by the App installation identity
      and not by the default Actions identity, read from the commits API for that pull request.
      Evidence: the captured API response under evidence/qa. Fails if the workflow falls back to the
      default token, which is the failure mode AC19 would otherwise diagnose only indirectly.

- [ ] **AC19 — The required checks re-run and pass on the post-repair head SHA.** On the fixture
      Dependabot pull request, for every check named as required by the repository ruleset, a check
      run exists on the post-repair head SHA, its originating workflow run has event `pull_request`,
      its conclusion is success, and no run is in an approval-required state. Evidence: the captured
      check-runs API response, including the originating run event for each, under evidence/qa. This
      is the outcome assertion that falsifies a wrong trigger or credential choice; a parked or
      `workflow_run`-sourced check fails it.

- [ ] **AC20 — Disclosure is present and conditional.** On the fixture Dependabot pull request, the
      body contains a "Repairs applied" block enumerating the repairs by project, and a "Packages
      skipped" block whenever the run recorded a skip. The `deps:autofixed` label is present on a run
      that applied a repair outside the analyzer-item and binding-redirect classes, and absent on a
      run that applied only those two classes. Both label states are exercised and captured.
      Evidence: the captured pull-request body and label state for both runs, under evidence/qa. The
      absent-label case prevents an implementation that always labels from passing.

- [ ] **AC21 — The #908 three-way divergence is reproduced as a fixture and resolved.**
      `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` carries an in-memory fixture in which
      one project's manifest declares 3.0.235, its `<Import>` and `<Error>` name 3.0.259, and its
      `<Analyzer Include>` names 3.0.203. The test asserts the verifier reports a disagreement for
      the guard elements and a disagreement for the analyzer item before repair, and that after
      repair all three locations name 3.0.235. Evidence: Pester output under evidence/qa.

- [ ] **AC22 — The AC21 regression test is observed failing before the fix.** The AC21 test is
      executed against the tree before the reconciliation and analyzer-repair modules are wired in,
      and the captured run shows it failing; the same test is then captured passing on the delivered
      tree. Evidence: the failing run under evidence/baseline and the passing run under
      evidence/regression. A test that cannot be shown failing is not admitted.

- [ ] **AC23 — Reference completeness is asserted and demonstrably detectable.** The verifier asserts
      that for each package in a manifest, a `<Reference>` with a matching `<HintPath>` exists for
      each consumable library asset resolved for that package, and
      `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` asserts the detector reports a
      missing reference for a fixture from which one such element has been removed. Evidence: Pester
      output under evidence/qa. This check exists to falsify the assumption that the NuGet CLI adds
      references for newly introduced assemblies; if it cannot be made to fail, it tests nothing.

- [ ] **AC24 — PowerShell toolchain and coverage.** The PowerShell toolchain passes in order
      (format, analyze, test) with no findings, and line coverage for each new module under
      `scripts/dependencies/` is at least 90 percent, with no coverage regression on changed lines in
      `scripts/vscode/Sync-PackageReferences.ps1`. Evidence: the formatter, analyzer and Pester
      coverage reports under evidence/qa.

- [ ] **AC25 — C# toolchain passes on the delivered tree.** The four CLAUDE.md commands run in order
      — formatter check, analyzer build, nullable build, test run with coverage — and all pass in a
      single final pass, with the analyzer and nullable builds proven non-vacuous by the absence of a
      skipped compile target in their logs. Evidence: the captured logs under evidence/qa.

- [ ] **AC26 — Documentation matches the delivered behaviour.** `.github/workflows/README.md`
      documents the repair workflow, its trigger, its credential requirement, and the pinned NuGet
      CLI version, and a Pester assertion in `tests/scripts/dependencies/DependabotConfig.Tests.ps1`
      asserts the pinned version literal recorded in the README equals the literal declared in the
      workflow files. Evidence: Pester output under evidence/qa. Fails when the pin is bumped in one
      place only.

---

## Write Set

Files this change creates, modifies or deletes. Every path here is backticked; paths elsewhere in
this document that are not in this list are comparison references and are deliberately not.

Configuration and workflows:

- `.github/dependabot.yml`
- `.github/workflows/dependabot-repair.yml`
- `.github/workflows/_build-analyzers.yml`
- `.github/workflows/_build-nullable.yml`
- `.github/workflows/_mstest-coverage.yml`
- `.github/workflows/README.md`
- `.csharpierignore`

Production PowerShell:

- `scripts/dependencies/PackageGraph.psm1`
- `scripts/dependencies/PackageCompatibility.psm1`
- `scripts/dependencies/AnalyzerItemRepair.psm1`
- `scripts/dependencies/ProjectConsistency.psm1`
- `scripts/dependencies/Repair-PackageManifestConsistency.ps1`
- `scripts/vscode/Sync-PackageReferences.ps1`

Tests:

- `tests/scripts/dependencies/PackageGraph.Tests.ps1`
- `tests/scripts/dependencies/PackageCompatibility.Tests.ps1`
- `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1`
- `tests/scripts/dependencies/ProjectConsistency.Tests.ps1`
- `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`
- `tests/scripts/dependencies/DependabotConfig.Tests.ps1`
- `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`

Project files carrying a stranded analyzer item (#898):

- `QuickFiler/QuickFiler.csproj`
- `QuickFiler.Test/QuickFiler.Test.csproj`
- `Tags/Tags.csproj`
- `Tags.Test/Tags.Test.csproj`
- `TaskMaster.Test/TaskMaster.Test.csproj`
- `TaskTree/TaskTree.csproj`
- `TaskTree.Test/TaskTree.Test.csproj`
- `TaskVisualization/TaskVisualization.csproj`
- `TaskVisualization.Test/TaskVisualization.Test.csproj`
- `ToDoModel/ToDoModel.csproj`
- `ToDoModel.Test/ToDoModel.Test.csproj`
- `UtilitiesCS/UtilitiesCS.csproj`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `VBFunctions/VBFunctions.csproj`
- `VBFunctions.Test/VBFunctions.Test.csproj`

Manifests normalised once, and `ToDoModel.Test/packages.config` additionally gains the entries
required by #903:

- `QuickFiler/packages.config`
- `QuickFiler.Test/packages.config`
- `SVGControl/packages.config`
- `SVGControl.Test/packages.config`
- `Tags/packages.config`
- `Tags.Test/packages.config`
- `TaskMaster/packages.config`
- `TaskMaster.Test/packages.config`
- `TaskTree/packages.config`
- `TaskTree.Test/packages.config`
- `TaskVisualization/packages.config`
- `TaskVisualization.Test/packages.config`
- `ToDoModel/packages.config`
- `ToDoModel.Test/packages.config`
- `UtilitiesCS/packages.config`
- `UtilitiesCS.Test/packages.config`
- `VBFunctions/packages.config`
- `VBFunctions.Test/packages.config`

Application configuration normalised once:

- `QuickFiler/app.config`
- `QuickFiler.Test/app.config`
- `SVGControl/app.config`
- `SVGControl.Test/app.config`
- `Tags/app.config`
- `Tags.Test/app.config`
- `TaskMaster/app.config`
- `TaskMaster.Test/app.config`
- `TaskTree/app.config`
- `TaskTree.Test/app.config`
- `TaskVisualization/app.config`
- `TaskVisualization.Test/app.config`
- `ToDoModel/app.config`
- `ToDoModel.Test/app.config`
- `UtilitiesCS/app.config`
- `UtilitiesCS.Test/app.config`
- `VBFunctions.Test/app.config`

---

## Risks & Mitigations

- **The App credential is refused.** The design degrades to a recurring manual approval click on
  every upgrade pull request rather than to failure. Mitigation: record the degraded mode in
  `.github/workflows/README.md` and keep AC19 as the gate that makes the degradation visible.
- **The trigger mechanism is wrong.** Mitigation: AC19 asserts the outcome, not the mechanism, so a
  wrong choice fails on the fixture pull request rather than in production.
- **The analyzer path derivation picks an unsupported Roslyn folder.** Mitigation: the analyzer build
  gate surfaces the error, and AC12 pins the selection rule against an injected listing.
- **Dependabot force-pushes over the repair commit on a rebase.** The repair pass is idempotent
  (AC15), so a rebase costs one extra run rather than corrupting the branch. Mitigation: the repair
  workflow re-triggers on the resulting CI run.
- **Scope.** This change touches 70 files and exceeds the PowerShell per-batch change budget.
  Mitigation: the planner splits it into batches, with the prerequisite corrections landing before
  the repair pass so the verifier's first run is a clean pass.

---

## Rollout & Follow-up

1. Land the prerequisite corrections and the one-time normalisation first, and verify AC5 and AC6
   locally before anything else merges.
2. Create and install the GitHub App, store its credentials, then land the repair workflow.
3. Exercise the fixture Dependabot pull request and capture the AC18, AC19 and AC20 evidence.
4. Post-merge monitoring, not an acceptance criterion because it cannot be observed at review time:
   confirm over the following two weekly cycles that exactly one Dependabot pull request is opened
   per cycle and that it reaches a mergeable state without human edits. Record the observation in
   the issue.
5. Links: issue #911; pull request #908 as the reproduction artifact; issues #898, #902, #903 as the
   folded-in prerequisites.
