# dependabot-net481-support — Spec

- **Issue:** #340
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-16
- **Status:** Draft
- **Version:** 0.2

## Overview

The repository has no automated dependency-update tooling. NuGet package versions across all 16 `packages.config`-based project directories are updated manually, so known-vulnerable or stale dependency versions can persist unnoticed. This spec authorizes a `.github/dependabot.yml` configuration that enables the `nuget` package ecosystem for the whole repository, incorporates a documented, non-fabricated safety net against `.NET Framework 4.8.1` (`net481`) incompatibility, and records the rationale in a repository-level documentation section. Findings are taken from `research/2026-07-16T16-10-dependabot-net481-support-research.md` and are treated as the authoritative technical basis; where research left a residual ambiguity, this spec makes the final decision (see "Resolved Ambiguities" below).

## Behavior

Add a `.github/dependabot.yml` configuration that enables the `nuget` package ecosystem for this repository. Because every production project targets `.NET Framework 4.8.1` (a framework line, not a package, so it is not itself a Dependabot-managed dependency), the configuration must not propose NuGet package versions that drop support for `.NET Framework`/`net48`/`net481` compatibility. Because Dependabot's NuGet updater is documented to be unable to update an indirect/transitive dependency independently of its parent (GitHub Docs, "About Dependabot security updates" — this is stated as ecosystem-level behavior, not merely a security-update-path detail, and NuGet is one of the "other ecosystems" contrasted against npm's lockfile-rewrite capability), and `packages.config` carries no lockfile of its own, this constraint is **already satisfied by Dependabot's documented default behavior** for this repo. The feature's job is therefore two-fold: (1) author a schema-correct config that covers all 16 project directories with sensible grouping/scheduling, and (2) add a `semver-major`-scoped `ignore` safety net plus documentation, rather than inventing a config primitive that does not exist in the Dependabot schema.

### Directory inventory (from research §1.1)

All 16 `packages.config`-bearing project directories are immediate children of the repository root:

`QuickFiler`, `QuickFiler.Test`, `SVGControl`, `SVGControl.Test`, `Tags`, `Tags.Test`, `TaskMaster`, `TaskMaster.Test`, `TaskTree`, `TaskTree.Test`, `TaskVisualization`, `TaskVisualization.Test`, `ToDoModel`, `ToDoModel.Test`, `UtilitiesCS`, `UtilitiesCS.Test`.

`VBFunctions` and `VBFunctions.Test` also exist as sibling directories and each carry a `packages.config` containing only the shared analyzer/dev-dependency set (no product package pins). They are swept up by the same root-level glob at no additional config cost and require no dedicated `groups`/`ignore` entries beyond the shared `analyzers-dev-deps` group already covering the analyzer set present in every directory.

No project has a `packages.lock.json`; `packages.config` has no lockfile concept — NuGet resolves the dependency graph at restore time from each package's own `.nuspec` dependency group for the current TFM.

## Resolved Ambiguities

Research flagged two residual ambiguities. Both are resolved here as final decisions, not left open for the atomic-planner:

1. **`packages.config` manifest-format support is corroborating, not primary-source verbatim.** Decision: proceed on the basis that `packages.config` is a supported NuGet manifest format for Dependabot version updates (research §2), and require the post-merge manual verification in AC-11 as the closing, authoritative confirmation. This is not a blocking condition on the initial merge (the config is still committed and enabled), but the feature is not considered fully verified until AC-11 passes.
2. **`/*` glob-depth semantics for `directories`.** Decision: ship `directories: ["/*"]` as the final, primary mechanism (not a placeholder) because every packages.config-bearing directory is an immediate child of the repository root — a single-segment wildcard is the documented use case (GitHub Docs: "The `directories` key supports globbing and the wildcard character `*`"). A concrete, pre-decided fallback is defined in AC-5: if AC-11's post-merge check shows fewer than the expected directory count scanned, the config is corrected in a follow-up commit to the literal enumerated `directories:` list in Appendix A, with no other resolution accepted.

## Inputs / Outputs

- Inputs: none (no CLI flags, no environment variables; this is a declarative configuration file consumed by GitHub's Dependabot service after merge to the default branch).
- Outputs:
  - `.github/dependabot.yml` (new file).
  - A new `## Dependency updates (Dependabot)` section in `README.md` (documentation deliverable — see "Documentation Deliverable" below).
- Config keys and defaults: `package-ecosystem: "nuget"`, `directories: ["/*"]`, `schedule.interval: "weekly"`, `open-pull-requests-limit: 10`, `groups` (four buckets, see below), `ignore` (semver-major-scoped entries, see below).
- Versioning or backward-compatibility constraints: none — this is an additive, config-only change with no runtime or public-API surface. Reverting the file re-disables scheduled Dependabot runs for this ecosystem with no other side effects.

## API / CLI Surface

Not applicable. `.github/dependabot.yml` has no CLI/API of its own; its "contract" is the Dependabot v2 configuration schema documented at GitHub Docs' dependabot.yml options reference. The relevant keys and their documented semantics (verified in research §2, §4, §5) are:

- `package-ecosystem: "nuget"` — enables the NuGet ecosystem (GitHub Docs table confirms Version updates: supported).
- `directories: ["/*"]` — scopes the ecosystem entry to every immediate-child directory (see Resolved Ambiguities #2).
- `ignore` → `dependency-name` (wildcard-capable) + `update-types` — suppresses specific semantic-version-level updates for named package patterns.
- `groups` → `patterns` + `group-by: "dependency-name"` — collapses a shared package's update across multiple `directories` entries into one PR instead of 16.
- `schedule.interval` — run cadence.
- `open-pull-requests-limit` — concurrent open-PR cap.

## Data & State

- Data transformations and invariants: none in the traditional sense — no data is read, transformed, or persisted by this repository's own code. The only "state change" is that GitHub's Dependabot service begins reading `.github/dependabot.yml` from the default branch and scheduling scans/PRs against it.
- Caching or persistence details: not applicable.
- Migration or backfill requirements: none.

## Constraints & Risks

- All projects use `packages.config`, not `PackageReference`. Dependabot's `nuget` ecosystem support for this manifest format is corroborated (not verbatim-primary-sourced) per research §2; the residual gap is closed by the post-merge AC-11 verification, not by blocking the merge.
- `.NET Framework 4.8.1` is fixed; the repository must not receive PRs proposing dependency versions that require `.NET`/`.NET Core`/newer-`net4x`-only TFMs.
- Dependabot has no native "TFM-aware ignore" primitive. Per research §4, no currently-published version of any package referenced in this repo's 16 `packages.config` files has dropped net481 support — this is a verified negative finding, not an assumption. Consequently, the compatibility safety net is a `semver-major`-scoped `ignore` rule set on the Microsoft `.NET`-runtime-aligned package families (the packages whose upstream release cadence is coupled to `.NET`'s own yearly major-version train), **not** fabricated version-ceiling numbers. This is the finalized mechanism; the atomic-planner must not substitute invented version ranges.
- Risk: overly broad `ignore` rules could suppress legitimate compatible security patches. Mitigated by scoping every `ignore` entry to `update-types: ["version-update:semver-major"]` only — minor/patch updates (including security patches) remain unaffected (AC-7).
- Risk: without `groups`, a package shared across all 16 directories (e.g. `log4net`) would open up to 16 near-duplicate PRs per release. Mitigated by `group-by: "dependency-name"` (AC-8).

## Implementation Strategy

- Implementation scope: add one new file (`.github/dependabot.yml`) and one new documentation section (`README.md`). No source code, test code, or `.csproj`/TFM changes are in scope.
- New classes/functions/commands: none — this is a declarative YAML artifact, not executable code.
- Dependency changes: none introduced by this feature itself; the feature *enables* future automated dependency-version PRs but does not itself bump any package.
- Logging/telemetry additions: none applicable — GitHub's Dependabot service does its own internal logging (visible via Insights → Dependency graph → Dependabot), which this repo does not control or extend.
- Rollout plan: single-commit, config-only change with no flag needed. Rollback path is a plain revert of the `.github/dependabot.yml` addition, which stops scheduled runs with no other side effects on the codebase.

## Documentation Deliverable (Decision)

The rationale documentation required by AC-12 is delivered as a new `## Dependency updates (Dependabot)` section in `README.md`, inserted after the existing `## Configuration & storage` section and before `## Common issues`, with a corresponding entry added to the `## Contents` list. This is the single, final target path — not a choice left to the atomic-planner. The section must state, in prose:

1. Dependabot's NuGet updater cannot independently bump a transitive/indirect dependency beyond what its referencing primary dependency's own manifest supports — this is documented default ecosystem behavior (GitHub Docs, "About Dependabot security updates"), not a mechanism this repo's config invents.
2. The `semver-major`-scoped `ignore` rule set (Microsoft `.NET`-runtime-aligned package families) is this repo's defense against a future framework-support drop, adopted because no currently-published version of any package referenced in this repo has dropped net481 support (verified 2026-07-16; see research §4). Minor/patch updates for the same families remain unaffected.
3. The `directories: ["/*"]` decision and its pre-decided fallback (the literal 16-entry list in Appendix A), to be adopted if post-merge verification shows under-coverage.
4. A pointer to the manual, out-of-toolchain post-merge verification step (AC-11) as a standing runbook note for maintainers.

## Acceptance Criteria

Supersedes the early draft in `issue.md`. Each item below is independently verifiable and decidable; none are left open for the atomic-planner to resolve.

### Config existence & schema

- [x] AC-1: `.github/dependabot.yml` exists at the repository root, begins with `version: 2`, and contains exactly one `updates:` entry with `package-ecosystem: "nuget"`.
- [x] AC-2: The `nuget` update entry's keys (`package-ecosystem`, `directories`, `schedule`, `open-pull-requests-limit`, `groups`, `ignore`) are present, correctly typed, and match the Dependabot v2 options reference structure cited in research §2/§4/§5; the file parses as valid YAML.

### Directory coverage (16 packages.config directories)

- [x] AC-3: The delivered config's directory-scoping mechanism is `directories: ["/*"]`, shipped as final per "Resolved Ambiguities" #2 above (not a placeholder pending later decision).
- [x] AC-4: Pre-merge, an enumeration check (e.g., `Get-ChildItem -Path . -Filter packages.config -Recurse | Select-Object FullName`, or equivalent) confirms `packages.config` files exist only at depth 1 from the repository root, across exactly the 16 directories listed in Appendix A (plus `VBFunctions`/`VBFunctions.Test`, which carry only the analyzer-only `packages.config` noted above). This enumeration is recorded as evidence supporting AC-3.
- [ ] AC-5: Pre-decided fallback — if the post-merge check in AC-11 shows Dependabot scanning fewer directories than expected (the `/*` glob under-matches), the config is corrected in a follow-up commit to the literal 16-entry `directories:` list in Appendix A, removing the glob. No other resolution is acceptable under this spec.

### TFM-compatibility safety net

- [x] AC-6: The config's `ignore` list contains one entry per Microsoft `.NET`-runtime-aligned package family — `Microsoft.Extensions.*`, `Microsoft.Bcl.*`, `System.Text.Json`, `System.Drawing.Common`, `Microsoft.Graph*`, `Apache.Arrow*`, `Microsoft.Data.Analysis`, `Microsoft.ML*` — each scoped to `update-types: ["version-update:semver-major"]`. No `ignore` entry specifies a fabricated version-ceiling number under a `versions:` key.
- [x] AC-7: No `ignore` entry includes `version-update:semver-minor` or `version-update:semver-patch` in its `update-types` list for any package — minor/patch updates, including security patches, remain unaffected by the safety net.

### Grouping & scheduling

- [x] AC-8: `groups` defines four buckets — `analyzers-dev-deps`, `test-frameworks`, `microsoft-extensions-and-bcl`, `graph-identity-telemetry` — each using `patterns` and `group-by: "dependency-name"`, matching the package-family inventory in research §1.1/§6.
- [x] AC-9: `schedule.interval` is `"weekly"` and `open-pull-requests-limit` is `10`.

### TFM non-modification

- [x] AC-10: A diff review of the merged change shows zero modifications to any `.csproj` file's `<TargetFrameworkVersion>` element (or any other TFM-declaring element). The only files added/changed are `.github/dependabot.yml` and the `README.md` documentation section (AC-12).

### Documentation

- [x] AC-12: `README.md` gains the `## Dependency updates (Dependabot)` section described in "Documentation Deliverable" above, including all four required content points, with a corresponding `## Contents` entry added.

### Post-merge verification (manual, outside the automated toolchain)

- [ ] AC-11: After merge, a maintainer manually confirms via the repository's **Insights → Dependency graph → Dependabot** tab (or the "Recent update jobs" log) that at least one of the 16 directories is scanned and produces a dependency list. This check is explicitly out of scope for the atomic-executor's automated toolchain — no format/lint/type-check/test stage applies to a static YAML file. It is recorded as a runbook note in the AC-12 documentation section and tracked as a deferred, manual verification item: it does not block the initial merge, but the feature is not considered fully verified (feature-audit) until it passes or AC-5's fallback has been applied and re-checked.

## Appendix A — Literal 16-directory fallback list

For use only if AC-5's fallback condition is triggered:

```yaml
directories:
  - "/QuickFiler"
  - "/QuickFiler.Test"
  - "/SVGControl"
  - "/SVGControl.Test"
  - "/Tags"
  - "/Tags.Test"
  - "/TaskMaster"
  - "/TaskMaster.Test"
  - "/TaskTree"
  - "/TaskTree.Test"
  - "/TaskVisualization"
  - "/TaskVisualization.Test"
  - "/ToDoModel"
  - "/ToDoModel.Test"
  - "/UtilitiesCS"
  - "/UtilitiesCS.Test"
```

## Definition of Done

- [x] Acceptance criteria (AC-1 through AC-12 above) documented and mapped to verification steps
- [x] `.github/dependabot.yml` matches every AC in this spec
- [x] `README.md` documentation section added per AC-12
- [x] Pre-merge enumeration evidence (AC-4) recorded
- [x] Diff review confirms no TFM changes (AC-10)
- [x] Post-merge Dependabot scan verification (AC-11) scheduled/tracked as a runbook item
- [x] No toolchain pass required beyond YAML validity — this is a config-only change with no C# source, so the CSharpier/analyzer/nullable/vstest loop does not apply (research §1.2)

## Seeded Test Conditions (from research §9)

- [ ] YAML validates against the Dependabot v2 config schema (GitHub validates on push; confirmed via AC-2/AC-11).
- [ ] Config correctly scopes `directories` to every `packages.config` project root (AC-3/AC-4/AC-5).
- [ ] `ignore` rules are anchored to specific packages and `update-types`, not blanket ecosystem suppression (AC-6/AC-7).
- [ ] `groups` collapse duplicate cross-directory bumps for the same package rather than producing per-directory PRs (AC-8).
