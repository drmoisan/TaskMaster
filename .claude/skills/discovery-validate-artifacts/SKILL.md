---
name: discovery-validate-artifacts
description: 'Canonical validation-gate mechanics for the discovery workflow. Use when running the per-artifact discovery validators after each stage and the completion gate at the end, interpreting the empty-error-list pass semantics, and routing any validator error back to the owning stage skill. Seventh and final stage.'
allowed-tools: Bash Read
---

# Discovery Validate Artifacts

Owns the canonical validation-gate mechanics for the discovery and
parity-definition workflow. Each discovery artifact is validated against its
schema by a `dev.discovery.validate-*` console script after its owning stage,
and `dev.discovery.validate-all` runs as the workflow completion gate. This skill
is the single canonical location for the pass/fail semantics; stage skills defer
to it by name.

## When to Use This Skill

- A stage has produced an artifact and you need to validate it before the next
  stage proceeds.
- You have completed all stages and need to run the workflow completion gate.
- A validator reported errors and you need the routing rule back to the owning
  stage skill.

## Prerequisites

- The relevant discovery artifacts exist under the profile's artifact root.
- The validator console scripts are available (registry in `discovery-workflow`).

## Pass/Fail Semantics

Every `dev.discovery.validate-*` invocation yields a `list[str]` of error
messages. An empty list is a pass; a non-empty list is a failure whose entries
name the exact problems. The validators never mutate the artifacts they check.

## Per-Stage Validators

Run each validator after its owning stage produces its artifact. On a non-empty
error list, return to the named owning stage skill, correct the artifact, and
re-run the validator until the list is empty.

| Validator | Validates schema | Owning stage skill |
|---|---|---|
| `dev.discovery.validate-profile` | (domain profile) | `discovery-repo-inventory` |
| `dev.discovery.validate-evidence-reference` | `schemas/discovery/v1/evidence-reference.schema.json` | `discovery-repo-inventory`, `discovery-runtime-characterization` |
| `dev.discovery.validate-feature-contract` | `schemas/discovery/v1/feature-contract.schema.json` | `discovery-coverage-ledger` |
| `dev.discovery.validate-coverage-ledger` | `schemas/discovery/v1/coverage-ledger.schema.json` | `discovery-coverage-ledger` |
| `dev.discovery.validate-runtime-scenario` | `schemas/discovery/v1/runtime-characterization-scenario.schema.json` | `discovery-runtime-characterization` |
| `dev.discovery.validate-parity-matrix` | `schemas/discovery/v1/parity-matrix.schema.json` | `discovery-parity-matrix` |
| `dev.discovery.validate-unspecified-behavior` | `schemas/discovery/v1/unspecified-behavior-record.schema.json` | `discovery-behavior-reconciliation` |
| `dev.discovery.validate-product-decision` | `schemas/discovery/v1/product-decision-record.schema.json` | `discovery-behavior-reconciliation` |

## Completion Gate

After all stages pass their per-stage validators, run
`dev.discovery.validate-all` as the workflow completion gate. It validates the
full artifact set across all seven schemas under `schemas/discovery/v1/`:

- `schemas/discovery/v1/feature-contract.schema.json`
- `schemas/discovery/v1/coverage-ledger.schema.json`
- `schemas/discovery/v1/runtime-characterization-scenario.schema.json`
- `schemas/discovery/v1/parity-matrix.schema.json`
- `schemas/discovery/v1/unspecified-behavior-record.schema.json`
- `schemas/discovery/v1/product-decision-record.schema.json`
- `schemas/discovery/v1/evidence-reference.schema.json`

An empty error list from `dev.discovery.validate-all` means the workflow is
complete. A non-empty list names the exact artifact and problem; route each entry
back to its owning stage skill using the per-stage table above, correct the
artifact, and re-run the gate.

## Referenced Skills

- `discovery-workflow` — stage order and the canonical Referenced Contracts
  registry (analyzer command, agent slugs, schema paths, validators).

## Notes

- This skill names no domain-specific identifier; artifact locations are read
  from the domain profile at runtime.
