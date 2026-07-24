---
name: discovery-behavior-reconciliation
description: 'Capture unspecified or contradictory behavior and reconcile it into product decisions in the discovery workflow. Use when recording unspecified-behavior findings from the parity matrix and reconciling them into product-decision records via the reconciler role. Sixth stage, after parity and before the validation gate.'
---

# Discovery Behavior Reconciliation

Runs the reconciliation stage of the discovery and parity-definition workflow.
It captures behavior that is unspecified or contradictory across the legacy
source, the runtime observations, and the parity matrix, then reconciles each
finding into a product decision. All domain specificity is read from the domain
profile at runtime.

## When to Use This Skill

- The parity matrix has flagged gaps or contradictions that need a documented
  resolution.
- You need product-decision records before the final validation gate.

## Prerequisites

- `discovery-parity-matrix` has completed.
- The domain profile is loaded and valid.

## Workflow

1. **Capture unspecified behavior.** Record each unspecified or contradictory
   behavior as a record conforming to the schema
   `schemas/discovery/v1/unspecified-behavior-record.schema.json`, citing the
   parity-matrix entry and evidence that surfaced it.

2. **Reconcile into product decisions.** For each captured record, produce a
   product-decision record conforming to the schema
   `schemas/discovery/v1/product-decision-record.schema.json`, stating the
   decision and its rationale.

3. **Route reconciliation.** Hand the records to the reconciler role for the
   reconciliation decision (see `## Worker Routing`).

## Worker Routing

- Worker: `requirements-reconciler`

The reconciler role decides how each unspecified or contradictory behavior is
resolved and records the outcome as a product decision.

## Validation

- Validate unspecified-behavior records with
  `dev.discovery.validate-unspecified-behavior`.
- Validate product-decision records with
  `dev.discovery.validate-product-decision`.
- An empty error list is a pass. On any error, follow the direction in
  `discovery-validate-artifacts`.

## Referenced Skills

- `discovery-workflow` — stage order and the canonical Referenced Contracts
  registry.
- `discovery-validate-artifacts` — pass/fail semantics and error routing.

## Notes

- Product-decision records complete the artifact set consumed by the final
  validation gate in `discovery-validate-artifacts`.
