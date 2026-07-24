---
name: discovery-repo-inventory
description: 'Drive the language-neutral repository and project inventory analyzer for the discovery workflow. Use when running the inventory stage against the domain profile source and target roots, recording analyzer outputs under the profile artifacts root, and running stack-specific analyzers generically per the profile technology stack. Second stage after profile load.'
allowed-tools: Bash Read Glob Grep
---

# Discovery Repo Inventory

Runs the inventory stage of the discovery and parity-definition workflow. It
drives the language-neutral repository/project inventory analyzer against the
roots declared in the domain profile, records outputs under the profile's
artifact root, and then runs any stack-specific analyzers generically, without
naming any concrete technology. All domain specificity is read from the domain
profile at runtime.

## When to Use This Skill

- You have loaded the domain profile and are ready to inventory the legacy
  source and the modern target.
- You need analyzer outputs recorded as evidence for the coverage stage.

## Prerequisites

- The domain profile loads via `dev.discovery.profile`.
- The analyzer framework CLI is available (referenced by name only; the full
  registry lives in `discovery-workflow`).

## Workflow

1. **Load the domain profile.** Run `dev.discovery.profile` to load and echo the
   consumer repository's `discovery-profile.yaml`. Read the following keys and
   use them for every subsequent step; do not hard-code any of them:
   - `legacy_source.root` — the legacy application source root.
   - `target.root` — the modern target root.
   - `artifacts.root` — where analyzer outputs and discovery artifacts are
     recorded.
   - `technology_stack` — the declared stack, used to select stack-specific
     analyzers generically.

2. **Run the language-neutral inventory analyzer.** Drive the inventory command
   (assumed `dev.discovery.inventory`; see the fan-in note below) against
   `legacy_source.root` and `target.root`. Record all outputs under
   `artifacts.root` following the profile's artifact conventions.

   **Fan-in reconciliation assumption:** the inventory command name
   `dev.discovery.inventory` is assumed and may be renamed when the analyzer
   framework (epic #9006) lands. The canonical registry entry and this fragment
   are the only two locations to reconcile; the canonical entry lives in
   `discovery-workflow`.

3. **Run stack-specific analyzers generically.** For the declared
   `technology_stack`, run any stack-specific analyzer commands documented by the
   analyzer framework. Do not name a concrete analyzer or technology here; the
   profile's `technology_stack` value selects the applicable analyzers at
   runtime. Record their outputs under `artifacts.root` as well.

4. **Record evidence references.** Capture each analyzer output as an
   evidence-reference instance so downstream stages can cite provenance.

## Validation

- Before the inventory stage begins, confirm the profile is valid with
  `dev.discovery.validate-profile`.
- After recording evidence references, validate them with
  `dev.discovery.validate-evidence-reference`.
- An empty error list is a pass. On any error, follow the direction in
  `discovery-validate-artifacts`, which owns the canonical validation-gate
  mechanics.

## Referenced Skills

- `discovery-workflow` — stage order and the canonical Referenced Contracts
  registry (analyzer command, agent slugs, schema paths, validators).
- `discovery-validate-artifacts` — pass/fail semantics and error routing.

## Notes

- This skill introduces no domain-specific identifier. Source root, target root,
  artifact root, and stack are all read from the domain profile at runtime.
- Outputs from this stage feed `discovery-coverage-ledger`.
