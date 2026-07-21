# dependabot-net481-support (Issue #340)

- Date captured: 2026-07-16
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/dependabot-net481-support/ (Issue #340)

- Issue: #340
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/340
- Last Updated: 2026-07-16
- Work Mode: full-feature

## Problem / Why

The repository has no automated dependency-update tooling. NuGet package versions across all eight `packages.config`-based projects (and their test counterparts) are updated manually, so known-vulnerable or stale dependency versions can persist unnoticed.

## Proposed Behavior

Add a `.github/dependabot.yml` configuration that enables the `nuget` package ecosystem for this repository. Because every production project targets `.NET Framework 4.8.1` (a framework line, not a package, so it is not itself a Dependabot-managed dependency), the configuration must not propose NuGet package versions that drop support for `.NET Framework` / `net48`/`net481` compatibility. Because Dependabot's NuGet updater only opens PRs for dependencies declared directly in a project's `packages.config`/`PackageReference` list, and it resolves any correlated transitive-dependency bumps required by a direct upgrade using NuGet's own dependency graph, secondary (transitive) dependencies must never be bumped independently of, or beyond what, their referencing primary dependency actually supports.

## Acceptance Criteria (early draft)

- [ ] `.github/dependabot.yml` exists, is schema-valid, and declares a `nuget` ecosystem update job.
- [ ] The configuration covers every project directory containing a `packages.config` (all eight production + eight test projects, or their common root as supported by the ecosystem).
- [ ] The configuration includes explicit `ignore` rules (or equivalent version-range constraints) preventing upgrades to package major/minor versions known to have dropped `.NET Framework 4.8`/`4.8.1` support.
- [ ] Documentation records the rationale: Dependabot does not independently upgrade transitive dependencies beyond what the direct/primary dependency's own manifest supports, and how this repo's config reinforces that for framework compatibility.
- [ ] No production or test project's target framework moniker (TFM) is changed.

## Constraints & Risks

- All projects use `packages.config`, not `PackageReference` — must confirm Dependabot's `nuget` ecosystem support for that manifest format.
- `.NET Framework 4.8.1` is fixed; the repository must not receive PRs proposing dependency versions that require `.NET`/`.NET Core`/newer `net4x` TFMs only.
- Risk: Dependabot has no native "TFM-aware ignore" primitive — compatibility constraints must be expressed via explicit per-package `ignore` version-range rules discovered through research into affected packages' release notes/support matrices.
- Risk: overly broad `ignore` rules could suppress legitimate compatible security patches; rules should be as narrow as version ranges allow.

## Test Conditions to Consider

- [ ] YAML validates against the Dependabot v2 config schema.
- [ ] Config correctly scopes `directory`/`directories` to every `packages.config` project root.
- [ ] `ignore` rules are anchored to specific packages and version ranges, not blanket ecosystem suppression.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/dependabot-net481-support/` folder from the template

