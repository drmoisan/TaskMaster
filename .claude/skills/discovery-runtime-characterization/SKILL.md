---
name: discovery-runtime-characterization
description: 'Produce runtime characterization scenarios and evidence references in the discovery workflow. Use when characterizing observed runtime behavior of the legacy application into scenarios with supporting evidence and routing analysis to the runtime role. Fourth stage, after coverage and before parity.'
---

# Discovery Runtime Characterization

Runs the runtime characterization stage of the discovery and parity-definition
workflow. It captures observed runtime behavior of the legacy application as
characterization scenarios, each backed by evidence references, so that parity
analysis can compare intended contracts against observed behavior. All domain
specificity is read from the domain profile at runtime.

## When to Use This Skill

- Feature contracts and the coverage ledger exist from the coverage stage.
- You need documented runtime behavior scenarios before building the parity
  matrix.

## Prerequisites

- `discovery-coverage-ledger` has completed.
- The domain profile is loaded and valid.

## Workflow

1. **Produce characterization scenarios.** For each behavior requiring runtime
   observation, produce a scenario conforming to the schema
   `schemas/discovery/v1/runtime-characterization-scenario.schema.json`. Each
   scenario records the observed runtime behavior in a domain-neutral form.

2. **Record evidence references.** Attach evidence to each scenario as
   evidence-reference instances conforming to the schema
   `schemas/discovery/v1/evidence-reference.schema.json`, so the provenance of
   each observation is traceable.

3. **Route analysis.** Hand the scenarios to the runtime role for analysis (see
   `## Worker Routing`).

## Worker Routing

- Worker: `runtime-characterization-analyst`

The runtime role analyzes and confirms each characterization scenario and its
evidence before the workflow proceeds to parity.

## Validation

- Validate scenarios with `dev.discovery.validate-runtime-scenario`.
- Validate evidence references with `dev.discovery.validate-evidence-reference`.
- An empty error list is a pass. On any error, follow the direction in
  `discovery-validate-artifacts`.

## Referenced Skills

- `discovery-workflow` — stage order and the canonical Referenced Contracts
  registry.
- `discovery-validate-artifacts` — pass/fail semantics and error routing.

## Notes

- Scenarios and evidence references from this stage are consumed by
  `discovery-parity-matrix`.
