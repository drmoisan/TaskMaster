---
name: discovery-workflow
description: 'Umbrella sequencing skill for the domain-neutral legacy discovery and parity-definition workflow. Use when onboarding or running the end-to-end discovery workflow across inventory, coverage, runtime characterization, parity, behavior reconciliation, and the validation gate, and when reconciling upstream analyzer and agent contract names. Holds the canonical Referenced Contracts registry.'
---

# Discovery Workflow

Sequences the reusable, domain-neutral discovery and parity-definition workflow
end to end. This skill is the entry point: it establishes the stage order,
directs each stage to its owning skill, and holds the single canonical registry
of upstream contract names (analyzer CLI commands, agent slugs, schema paths,
and validators).

All domain specificity is read at runtime from the consumer repository's domain
profile (`discovery-profile.yaml`) via `dev.discovery.profile`. This skill names
no concrete repository, path, or technology stack.

## When to Use This Skill

- You are onboarding the discovery and parity-definition workflow in a consumer
  repository and need the documented stage order.
- You need the canonical list of upstream contract names to reconcile a renamed
  analyzer command or agent slug at epic fan-in.
- You need to know which stage skill owns which artifact and validator.

## Prerequisites

- A `discovery-profile.yaml` domain profile exists in the consumer repository and
  loads cleanly via `dev.discovery.profile`.
- The analyzer framework CLI and the four generic agent roles are available in
  the consumer environment (referenced here by name only).

## Stage Order

Run the workflow in this fixed order. Each stage validates its artifacts before
the next stage proceeds:

1. **profile** — load and echo the domain profile via `dev.discovery.profile`.
2. **inventory** — drive the language-neutral repository/project inventory
   analyzer (`discovery-repo-inventory`).
3. **coverage** — produce feature contracts and the coverage ledger
   (`discovery-coverage-ledger`).
4. **runtime** — produce runtime characterization scenarios and evidence
   references (`discovery-runtime-characterization`).
5. **parity** — produce or refresh the parity matrix (`discovery-parity-matrix`).
6. **reconciliation** — capture unspecified or contradictory behavior and
   reconcile it into product decisions (`discovery-behavior-reconciliation`).
7. **validation gate** — run the per-stage validators and the completion gate
   (`discovery-validate-artifacts`).

Compact form: profile -> inventory -> coverage -> runtime -> parity ->
reconciliation -> validation gate.

## Worker Routing

The four generic agent roles are routed by slug from their owning stage skills.
The routing table below is the canonical mapping; each stage skill repeats only
the single slug it routes to.

| Stage skill | Agent slug |
|---|---|
| `discovery-coverage-ledger` | `migration-coverage-reviewer` |
| `discovery-runtime-characterization` | `runtime-characterization-analyst` |
| `discovery-parity-matrix` | `legacy-parity-analyst` |
| `discovery-behavior-reconciliation` | `requirements-reconciler` |

## Referenced Contracts

This registry is the single canonical location for upstream contract names.
Stage skills name only the specific contract(s) they use and defer to this
registry for the full set. Indirect reference by skill name is not duplication.

### Domain profile (config contract, issue #360 / epic #9001)

- Config artifact: `discovery-profile.yaml`.
- CLI: `dev.discovery.profile`.

### Schemas (issue #359 / epic #9002)

Seven schemas under `schemas/discovery/v1/`:

- `schemas/discovery/v1/feature-contract.schema.json`
- `schemas/discovery/v1/coverage-ledger.schema.json`
- `schemas/discovery/v1/runtime-characterization-scenario.schema.json`
- `schemas/discovery/v1/parity-matrix.schema.json`
- `schemas/discovery/v1/unspecified-behavior-record.schema.json`
- `schemas/discovery/v1/product-decision-record.schema.json`
- `schemas/discovery/v1/evidence-reference.schema.json`

### Validators (issue #361 / epic #9003)

Nine `dev.discovery.validate-*` console scripts. Each yields a `list[str]` of
errors; an empty list is a pass.

- `dev.discovery.validate-profile`
- `dev.discovery.validate-feature-contract`
- `dev.discovery.validate-coverage-ledger`
- `dev.discovery.validate-runtime-scenario`
- `dev.discovery.validate-parity-matrix`
- `dev.discovery.validate-unspecified-behavior`
- `dev.discovery.validate-product-decision`
- `dev.discovery.validate-evidence-reference`
- `dev.discovery.validate-all`

### Analyzer framework (epic #9006, in preparation)

- Language-neutral repository/project inventory command: `dev.discovery.inventory`.

  **Fan-in reconciliation assumption:** the inventory command name
  `dev.discovery.inventory` is assumed. It is not fixed by the prepared upstream
  summaries and is the epic #9006 owner's decision. This assumption is isolated
  to this single registry line plus one fragment in `discovery-repo-inventory`.
  If #9006 lands a different command name, update those two locations only.

### Agent roles (epic #9007, in preparation)

Four generic agent personas, referenced by slug in the Worker Routing table
above:

- `legacy-parity-analyst`
- `runtime-characterization-analyst`
- `requirements-reconciler`
- `migration-coverage-reviewer`

  **Fan-in reconciliation assumption:** the four agent slugs are assumed to be
  the kebab-case of the persona titles. Final slugs are the epic #9007 owner's
  decision. The assumption is isolated to this registry and the per-stage
  `## Worker Routing` fragments; reconciling a renamed slug at fan-in is a
  bounded edit.

## Referenced Skills

- `discovery-repo-inventory` — inventory stage mechanics.
- `discovery-coverage-ledger` — coverage stage mechanics.
- `discovery-runtime-characterization` — runtime stage mechanics.
- `discovery-parity-matrix` — parity stage mechanics.
- `discovery-behavior-reconciliation` — reconciliation stage mechanics.
- `discovery-validate-artifacts` — canonical validation-gate mechanics.

## Notes

- No skill in this workflow asserts the existence of upstream analyzer or agent
  files; upstream contracts are referenced by name only, so this workflow stays
  correct regardless of upstream merge order.
- Domain specificity (source and target roots, technology stack, artifact
  conventions) is read from the domain profile at runtime, never hard-coded.
