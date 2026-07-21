# `dependabot-net481-support` — User Story

- Issue: #340
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-16

## Story Statement

- As a maintainer of this repository, I want automated NuGet dependency-update pull requests across all 16 `packages.config` project directories, so that stale or known-vulnerable package versions no longer persist unnoticed due to manual-only tracking.
- As a maintainer of this repository, I want the dependency-update automation to never propose a package version that has dropped `.NET Framework 4.8.1` (`net481`) support, so that an automated PR cannot introduce a build break the project cannot recover from without a framework migration it is not undertaking.

## Problem / Why

The repository has no automated dependency-update tooling. NuGet package versions across all 16 `packages.config`-based project directories are updated manually, so known-vulnerable or stale dependency versions can persist unnoticed. Every production project is fixed to `.NET Framework 4.8.1`; any automation introduced to solve the staleness problem must not itself become a source of framework-incompatible breakage, since the maintainer has no path to move these projects off `net481` in the near term.

## Personas & Scenarios

- Persona: Dan, the sole maintainer of this VSTO Outlook add-in repository.
  - Who he is: reviews and merges all PRs personally; has limited time to manually audit 16 separate `packages.config` files for outdated or vulnerable package versions.
  - What he cares about: dependency currency (especially security-relevant patches) without spending recurring manual effort checking each project directory.
  - His constraints: every project is permanently pinned to `.NET Framework 4.8.1` (a VSTO/Outlook interop requirement) — he cannot accept an automated dependency bump that requires a newer-only TFM, because that would silently break the build the next time CI restores packages.
  - His goals and frustrations: wants "set it up once and get PRs" dependency hygiene, but is wary of blindly trusting an automation tool that could, in principle, propose an incompatible major-version bump for a package family that later drops legacy-framework support.
  - His context and motivations: this is a long-lived personal/small-team project without CI infrastructure dedicated to dependency scanning; GitHub's native Dependabot tooling is the lowest-friction option since it requires only a committed config file, not a new service to operate.

- Scenario: Enabling dependency automation without breaking the fixed framework target.
  - Who is acting: Dan, merging the `.github/dependabot.yml` PR into the default branch.
  - What triggered the action: recurring awareness that dependency versions across `QuickFiler`, `TaskMaster`, `UtilitiesCS`, and the other 13 project directories are checked manually and inconsistently.
  - What steps he takes: reviews the proposed `.github/dependabot.yml`, confirms it scopes all 16 directories via `directories: ["/*"]`, confirms the `ignore` rules only gate `semver-major` bumps for the Microsoft `.NET`-runtime-aligned package families (not blanket suppression), confirms no `.csproj` TFM element changed, and merges.
  - What obstacles or decisions occur: Dan has to decide whether to trust that Dependabot's NuGet updater genuinely cannot bump a transitive dependency independently of its parent (it cannot, per GitHub's own documentation — a property of the ecosystem, not something this config has to separately enforce), and whether the `/*` glob will actually match all 16 directories, given the documentation available did not include a fully worked example of the glob's matching depth.
  - What outcome he expects: within a week of merge, Dependabot opens its first grouped PR(s) proposing minor/patch bumps for shared packages (e.g., `log4net`, an `Microsoft.Extensions.*` family member) collapsed into one PR per shared package rather than 16 near-duplicate PRs; any future major version of a gated package family that would drop `net481` support is withheld until Dan manually re-verifies compatibility, rather than arriving as an automatically-mergeable PR.
  - Follow-up check: Dan visits the repository's Insights → Dependency graph → Dependabot tab after the first scheduled run to confirm at least one of the 16 directories was actually scanned, closing the small residual uncertainty about whether Dependabot's NuGet updater fully supports the `packages.config` manifest format in this repository's specific layout.

## Acceptance Criteria

Full acceptance-criteria detail, including exact `ignore`/`groups`/`directories` values and the finalized fallback/verification decisions, is defined in `spec.md` (AC-1 through AC-12). Restated here from the maintainer's perspective:

- [x] A single `.github/dependabot.yml` covers all 16 `packages.config` project directories, so no directory is left unmonitored (spec AC-1 through AC-5).
- [x] The configuration never proposes a package version whose NuGet Frameworks panel has dropped `.NET Framework`/`net48`/`net481` support, achieved through a documented `semver-major`-scoped `ignore` safety net on the Microsoft `.NET`-runtime-aligned package families — not through fabricated version-ceiling guesses (spec AC-6, AC-7).
- [x] Updates for shared packages across the 16 directories arrive as one grouped PR, not one PR per directory, so review load stays manageable (spec AC-8).
- [x] The update cadence is weekly with at most 10 open PRs at a time, so PR volume does not overwhelm a single-reviewer workflow (spec AC-9).
- [x] No project's target framework moniker changes as a side effect of this feature (spec AC-10).
- [x] `README.md` explains, in plain language a future contributor can read without re-deriving it, why transitive dependencies are never bumped independently of their parent (Dependabot's documented default NuGet behavior) and why the `ignore` safety net exists (spec AC-12).
- [ ] After merge, Dan (or another maintainer) confirms via GitHub's Dependabot Insights tab that scanning actually occurred for at least one directory — an explicitly manual, post-merge check, not something the automated toolchain verifies (spec AC-11).

## Non-Goals

- This feature does not migrate any project off `.NET Framework 4.8.1`, and does not change any `.csproj` TFM.
- This feature does not convert any project from `packages.config` to `PackageReference`.
- This feature does not enable or configure Dependabot **security alerts** (a separate GitHub capability from version updates); that remains a repository-settings concern outside this file-based change.
- This feature does not add a CI workflow, test suite, or local YAML-schema-validation script — GitHub's own push-time validation and the manual post-merge Insights check (AC-11) are the accepted verification mechanisms for this config-only artifact.
- This feature does not attempt to invent a Dependabot config primitive for "never bump a transitive dependency" — that behavior is already the ecosystem's documented default and is recorded, not re-implemented.
