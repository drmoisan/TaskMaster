---
name: discovery-coverage-ledger
description: 'Produce feature contracts and the coverage ledger from inventory output in the discovery workflow. Use when deriving the feature contract set and the migration coverage ledger from the repository inventory and routing coverage review to the coverage role. Third stage, after inventory and before runtime characterization.'
---

# Discovery Coverage Ledger

Runs the coverage stage of the discovery and parity-definition workflow. It
derives feature contracts and the coverage ledger from the inventory output, so
that every inventoried unit of behavior has a contract and a tracked coverage
state. All domain specificity is read from the domain profile at runtime.

## When to Use This Skill

- The inventory stage has produced analyzer outputs under the profile's artifact
  root.
- You need the feature contract set and the coverage ledger before runtime
  characterization and parity analysis.

## Prerequisites

- `discovery-repo-inventory` has completed and recorded its outputs.
- The domain profile is loaded and valid.

## Workflow

1. **Derive feature contracts.** From the inventory output, produce one feature
   contract per inventoried unit of behavior, conforming to the schema
   `schemas/discovery/v1/feature-contract.schema.json`.

2. **Build the coverage ledger.** Aggregate the feature contracts into the
   coverage ledger, conforming to the schema
   `schemas/discovery/v1/coverage-ledger.schema.json`. The ledger records the
   coverage state of each contract so later stages can measure parity progress.

3. **Route coverage review.** Hand the ledger to the coverage role for review
   (see `## Worker Routing`).

## Worker Routing

- Worker: `migration-coverage-reviewer`

The coverage role reviews the derived feature contracts and the coverage ledger
for completeness and correctness before the workflow proceeds to runtime
characterization.

## Validation

- Validate feature contracts with `dev.discovery.validate-feature-contract`.
- Validate the coverage ledger with `dev.discovery.validate-coverage-ledger`.
- An empty error list is a pass. On any error, follow the direction in
  `discovery-validate-artifacts`.

## Referenced Skills

- `discovery-workflow` — stage order and the canonical Referenced Contracts
  registry.
- `discovery-validate-artifacts` — pass/fail semantics and error routing.

## Notes

- Feature contracts and the coverage ledger are jointly derived from the
  inventory, so contract authorship is assigned to this stage and reviewed by
  the coverage role.
- Outputs from this stage feed `discovery-runtime-characterization` and
  `discovery-parity-matrix`.
