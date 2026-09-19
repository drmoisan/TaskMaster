# dependabot-fanout-and-ci-failing-nuget-upgrades (Spec)

- **Issue:** #911
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-19T09-44
- **Status:** Draft
- **Version:** 0.1

## Context
Dependabot opens several pull requests per upgrade cycle and the resulting branches fail the
required CI checks, so dependency upgrades are effectively unmergeable without manual repair.
Dependabot edits `packages.config` but cannot maintain the coupled `.csproj` state that
`packages.config`-style (non-SDK) projects require, so every bot branch is internally inconsistent
from the moment it is created.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (.NET Framework 4.8.1 VSTO solution, 17 non-SDK projects)
- Command/flags used: `.github/dependabot.yml` weekly NuGet schedule; required checks `actionlint`,
  `format-check`, `build-analyzers`, `build-nullable`, `mstest-coverage`, `pester`
- Data source or fixture: 17 `packages.config` manifests and their sibling `.csproj` files

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Dependency upgrades — including security-relevant ones — cannot land. Separately, `main` is one
cache eviction away from an unbuildable state and analyzers are silently disabled in the 15
projects carrying the stale path.


## Repro & Evidence
Steps to Reproduce:
1. Allow the weekly Dependabot NuGet schedule to run against `main`.
2. Observe the number of pull requests opened (three are open as of 2026-09-19: #907, #908, #909).
3. Open any one of them and run the required checks.
4. Inspect the branch diff: `packages.config` version attributes changed, `.csproj` unchanged.

Expected:
One consolidated pull request per upgrade cycle, containing a dependency upgrade that is
internally consistent — manifest versions, `<HintPath>`, `<Analyzer Include>`, package-import
guards, and binding redirects all moved together — and that passes all six required checks
without human edits.

Actual:
Multiple pull requests per cycle (one per configured group, multiplied by the `directories` glob
fan-out), each failing CI. Measured history: **14 of 59 Dependabot pull requests have ever been
merged, and none since 2026-08-21**; every merge that did land carried human commits performing
the `.csproj` maintenance NuGet would normally perform.

Four distinct defects contribute:

1. `packages.config` is absent from `.csharpierignore`, so CSharpier reflows manifest entries and
   `format-check` fails on the bot's unformatted edit.
2. `.csproj` analyzer paths do not move with the manifests. **15 of 17 projects reference
   `Meziantou.Analyzer.3.0.203` while every manifest pins `3.0.235`** (issue #898). CI is currently
   green only because the workflow cache prefix fallback carries the old package forward; a cache
   eviction turns `main` red with no code change (`CS0006`).
3. Deedle no longer supports this target framework, so it cannot be upgraded at all.
4. Four groups combined with the directory glob produce duplicate pull requests for the same
   package across projects.

Logs / Screenshots:
- [x] Attached minimal logs or snippet
- Snippet (measured on `origin/main` at 734112ed2, 2026-09-19):

```
grep -rho "Meziantou.Analyzer.[0-9.]*" --include=*.csproj . | sort | uniq -c
     15 Meziantou.Analyzer.3.0.203      <- stale csproj references
     65 Meziantou.Analyzer.3.0.235

gh pr list --state open --author app/dependabot
909  Bump the graph-identity-telemetry group with 2 updates
908  Bump the test-frameworks group with 11 updates
907  Bump the analyzers-dev-deps group with 1 update
```


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
The root cause is not package incompatibility (Deedle excepted). It is that **Dependabot cannot
correctly upgrade `packages.config` projects**: it edits manifests and leaves the coupled `.csproj`
state behind. Local Visual Studio / NuGet upgrades succeed because
`scripts/vscode/Sync-PackageReferences.ps1` runs before every local build (invoked from
`scripts/vscode/Invoke-VSBuild.ps1` lines 247-253) and repairs `<HintPath>` values. **That script
never runs in CI**, so CI builds exactly what was committed. That asymmetry explains the whole
failure pattern.

Three adjacent defects make a repaired pipeline fail on its first run and are in scope:

- **#898** — 15 `<Analyzer Include>` sites pinned to `Meziantou.Analyzer.3.0.203`.
- **#902** — `Sync-PackageReferences.ps1` ranks `netstandard2.1` above `netstandard2.0`, which would
  reintroduce #895 on the next local build. `net481` cannot consume `netstandard2.1` at all.
- **#903** — `ToDoModel.Test/packages.config` omits packages for which the `.csproj` carries
  `<HintPath>` entries (confirmed: `FSharp.Core`, `Deedle`).

Verified constraints on the NuGet CLI update command (fact-find, 2026-09-17): it is
non-interactive-capable (requires the non-interactive and overwrite-conflict switches, and requires
MSBuild, which CI has). It writes `packages.config`, existing `<Reference>`/`<HintPath>`, the
conditional package `<Import>`, and the package-imports `<Error>` target. It **does not** write
`<Analyzer Include>` (162 occurrences across all 17 projects; those are added by `install.ps1` via
EnvDTE, and the update command never runs `install.ps1`) and it does **not** write binding
redirects (the add-binding-redirects routine is a documented no-op, closed *By Design*). The
version switch applies only when exactly one package id is supplied.

One claim remains contested and must be settled empirically rather than assumed: the documentation
states the update command adds no `<Reference>` element for a newly-added assembly, while the
source suggests a full uninstall/install cycle that would. If the pessimistic reading holds, a
third post-pass is required.


## Proposed Fix

### Design summary (what changes where):

### Boundaries and invariants to preserve:

### Dependencies or blocked work:

### Implementation strategy (what changes, not sequencing):
	
#### Files/modules to change:

#### Functions/classes/CLI commands impacted:

#### Data flow and validation changes:

#### Error handling and logging updates:

#### Rollback/feature-flag considerations (if applicable):

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

#### Required configuration keys and defaults:

#### Backward-compatibility expectations:

#### Performance constraints (latency/throughput/memory):

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
- Constraints (budget, performance, compatibility):
- External dependencies (services, libraries, releases):

## Data / API / Config Impact
- User-facing or API changes:
- Data or migration considerations:
- Logging/telemetry updates (if any):
- Compatibility notes (CLI flags, config schemas, versioning):

## Test Strategy
Seeded from issue:

Settled design (22 decisions, design-tree session 2026-09-19):

- [x] **Dependabot detects; NuGet performs the upgrade.** One consolidated pull request,
      `open-pull-requests-limit: 1`, directory fan-out collapsed, Deedle ignored entirely, the eight
      existing major-version ignores retained.
- [x] **A new reusable workflow performs the upgrade** automatically on its own fresh branch and
      pull request, gated by a framework-compatibility check.
- [x] **Compatibility is asset-level**: a candidate passes only if it ships an asset `net481` can
      consume. `netstandard2.1` is excluded outright, not merely ranked last. An incompatible
      package is skipped with a recorded reason and the remaining upgrades proceed.
- [x] **One update invocation per package**, each version-pinned to what Dependabot identified,
      followed by two post-passes for what NuGet provably does not write (`<Analyzer Include>` and
      binding redirects).
- [x] **A verifier repairs freely and fails only if the post-fix tree is still inconsistent**,
      labelling the pull request `deps:autofixed` when a repair outside the two known-weak classes
      was applied, and recording a "Repairs applied" block in the pull request body.
- [x] Prerequisites folded into the same change so the verifier's first run is a clean pass:
      #898, #902, #903, `packages.config` added to `.csharpierignore`, and all 17 manifests
      normalised to inline form once.
- [x] Pin the NuGet CLI version (currently floating in all three CI workflows).
- [x] Unit coverage areas: the compatibility evaluator and the verifier are pure functions over
      parsed manifest and project state and are unit-testable with Pester without touching the
      network.
- [x] Integration scenario to retest: run the upgrade workflow against a deliberately stale
      manifest and confirm all six required checks pass on the produced branch.
- [x] Manual verification notes: confirm `main` builds from a cold cache after the #898 correction.

- Regression tests to add or update:
- Unit tests (pytest) for the fixed behavior and boundaries:
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):


## Acceptance Criteria
- [ ] Repro steps now produce the expected behavior in all documented environments.
- [ ] Regression test(s) added and passing (list file path and test name).
- [ ] Edge cases and invalid inputs are handled with correct errors or fallbacks.
- [ ] No unintended behavior changes outside the defined scope.
- [ ] Required logs/telemetry updated and validated (if applicable).
- [ ] Performance constraints met or explicitly waived with rationale.
- [ ] Full toolchain pass completed (format → lint → type-check → test).
- [ ] Docs/config references updated to match the new behavior.

## Risks & Mitigations
- Technical or operational risks:
- Mitigations and rollbacks:

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
