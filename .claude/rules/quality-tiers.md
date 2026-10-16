---
paths:
  - "**"
description: Module rigor tier system and uniform coverage thresholds.
---

# Module Rigor Tiers

Applicability: this rule applies only when `quality-tiers.yml` exists at the repository root, which indicates that the repository has adopted module rigor tiers. When that file is absent, there is no tier classification requirement, no `tier-classification` CI stage, and no tier-dependent gate (the escape-hatch limits, property-test density, mutation score, contract-bump rule, determinism retry rate, golden tests, and E2E suite scope in the Tier-dependent table below), and the absence of `quality-tiers.yml` is not a defect. The coverage defaults listed under Uniform across all tiers do not depend on tier adoption; their precedence is stated in that section.

When `quality-tiers.yml` exists, this rule defines the T1–T4 module rigor tier system used by the repository's CI gates, and the tier definitions and gate matrix in this document are the tier system's source of truth.

## Tiers

- **T1 — Critical.** Behavior bugs cause silent data loss, model drift, or security holes. Examples: classifier and scoring engines, identifier allocators and hierarchy operations, adapters that write to an external system of record, authentication and token handling, command dispatch.
- **T2 — Core.** Bugs cause feature regressions but not data loss. Examples: domain and application layers, data-transfer objects, settings store abstractions, schema definitions.
- **T3 — Adapters & UI.** Glue around APIs the team does not own. Examples: user-interface surfaces, host-platform API wrappers, third-party API and SDK wrappers, persistence I/O.
- **T4 — Scaffolding.** Examples: DI wiring, bootstrap, build scripts, dev tooling, generated code, manifests.

## Source of Truth

- When `quality-tiers.yml` exists at the repository root, it maps every project to one tier.
- In a repository that runs a `tier-classification` CI stage, that stage validates that every project entry has a tier and that no unclassified project exists, and it fails CI for an unclassified project.

## Uniform-vs-Tier-Dependent Gate Matrix

Per Authoritative Decision #2, line and branch coverage thresholds are uniform across all tiers. The line threshold applies to every coverage language; the branch threshold applies to languages whose coverage tooling measures branch coverage. Other gates remain tier-dependent.

### Uniform across all tiers (T1–T4)

Threshold precedence: when the repository's root `CLAUDE.md` states line or branch coverage thresholds, those thresholds govern. The 85% line and 75% branch figures are defaults that apply only when the root `CLAUDE.md` states none. This precedence applies to every restatement of these figures in other rule files, agents, and skills. Each metric falls back independently: when the root `CLAUDE.md` states only a line threshold or only a branch threshold, the stated threshold governs that metric and the default governs the other.

- Format check: 100% pass.
- Lint errors: 0.
- Type errors: 0.
- Architecture violations: 0.
- Line coverage: >= 85%.
- Branch coverage: >= 75% for languages whose coverage tooling measures branch coverage. PowerShell (Pester) and bash (kcov) are exempt from this threshold because neither tool measures branch coverage; no branch-coverage gate applies to them.
- No regression on changed lines.

### Tier-dependent

| Gate | T1 | T2 | T3 | T4 |
|---|---|---|---|---|
| Untyped escape hatches (`any`/`dynamic`) | 0 | 0 | <= 5 per file, justified | unlimited |
| Property test density | >= 1 per pure function | >= 1 per pure function | none | none |
| Mutation score | >= 75% | trend-only | none | none |
| Contract breaking changes | major bump required | major bump required | n/a | n/a |
| Determinism (retry rate) | < 0.5% | < 1% | < 2% | n/a |
| Golden tests | required for classifier-output modules | optional | none | none |
| Full E2E suite scope | all critical paths | core paths | adapter smoke | none |

## Rationale (uniform coverage thresholds)

High test coverage is a fundamental quality-control design choice that enables autonomous agentic development and trust in the work product. For that reason, line coverage >= 85% applies uniformly across T1–T4 to every coverage language, and branch coverage >= 75% applies uniformly across T1–T4 to every language whose coverage tooling measures branch coverage; tier-specific lower coverage floors are not used. These figures are defaults under the threshold precedence stated above: a root `CLAUDE.md` that states coverage thresholds overrides them. Each metric falls back independently, so a root `CLAUDE.md` that states only one of the two thresholds overrides only that one. The branch threshold is not applied to PowerShell or bash because Pester and kcov do not measure branch coverage. That exemption is a capability limit on an unevaluable threshold, not a licence to exclude files from measurement: PowerShell and bash production files remain in the coverage denominator under the Coverage Exclusion Policy in `.claude/rules/general-unit-test.md`.
