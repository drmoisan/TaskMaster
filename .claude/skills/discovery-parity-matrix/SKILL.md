---
name: discovery-parity-matrix
description: 'Produce or refresh the parity matrix in the discovery workflow. Use when building the parity matrix from feature contracts and runtime characterization evidence and routing parity reasoning to the parity role. Fifth stage, after runtime characterization and before behavior reconciliation.'
---

# Discovery Parity Matrix

Runs the parity stage of the discovery and parity-definition workflow. It
produces or refreshes the parity matrix by comparing the feature contracts
against the runtime characterization scenarios, so the workflow can measure how
closely the modern target reproduces the legacy behavior. All domain specificity
is read from the domain profile at runtime.

## When to Use This Skill

- Feature contracts and runtime characterization scenarios both exist.
- You need a current parity matrix before reconciling unspecified behavior.

## Prerequisites

- `discovery-coverage-ledger` and `discovery-runtime-characterization` have
  completed.
- The domain profile is loaded and valid.

## Workflow

1. **Consume upstream artifacts.** Read the feature contracts (schema
   `schemas/discovery/v1/feature-contract.schema.json`) and the runtime
   characterization scenarios (schema
   `schemas/discovery/v1/runtime-characterization-scenario.schema.json`).

2. **Produce or refresh the parity matrix.** Build the parity matrix conforming
   to the schema `schemas/discovery/v1/parity-matrix.schema.json`, mapping each
   feature contract to its characterized behavior and its current parity state.
   When the matrix already exists, refresh it in place from the latest upstream
   artifacts.

3. **Route parity reasoning.** Hand the matrix to the parity role for reasoning
   about gaps and states (see `## Worker Routing`).

## Worker Routing

- Worker: `legacy-parity-analyst`

The parity role reasons about each parity-matrix entry, classifying parity state
and flagging gaps for behavior reconciliation.

## Validation

- Validate the parity matrix with `dev.discovery.validate-parity-matrix`.
- An empty error list is a pass. On any error, follow the direction in
  `discovery-validate-artifacts`.

## Referenced Skills

- `discovery-workflow` — stage order and the canonical Referenced Contracts
  registry.
- `discovery-validate-artifacts` — pass/fail semantics and error routing.

## Notes

- Parity gaps flagged here are the primary input to
  `discovery-behavior-reconciliation`.
