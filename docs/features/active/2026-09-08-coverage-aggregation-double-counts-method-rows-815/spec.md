# 2026-09-08-coverage-aggregation-double-counts-method-rows (Spec)

- **Issue:** #815
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-08T23-49
- **Status:** Draft
- **Version:** 0.1

## Context
The coverage aggregation method pinned in atomic plans double-counts method rows when it sums
Cobertura output, so reported first-party coverage is inflated. On issue 809's delivery the plan's
artifacts reported 79.38% first-party branch coverage; recomputing the same run de-duplicated gives
77.03%. The method is reused by other plans, so every gate that depends on it reads high.

Environment:
- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8
- Python version: not applicable
- Command/flags used: the repository coverage route (`dotnet-coverage`), Cobertura output consumed by
  the aggregation step written into atomic plans
- Data source or fixture: the raw Cobertura report from issue 809's final QA gate run

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High because it silently corrupts a quality gate across many future plans rather than affecting one
item. A gate that reports a number nobody can reproduce provides no assurance, and the error is in
the optimistic direction, so it fails to stop the cases it exists to stop.


## Repro & Evidence
Steps to Reproduce:
1. Run the repository coverage route to produce a Cobertura report.
2. Aggregate first-party branch coverage using the method pinned in a current atomic plan.
3. Recompute the same figure de-duplicating method rows before summing.
4. Compare: the two disagree, with the pinned method reporting the higher value.

Expected:
A coverage figure quoted in a QA gate equals the figure a de-duplicated recomputation from the same
raw Cobertura report produces. Two methods over one report do not give two answers.

Actual:
The pinned aggregation counts method rows more than once. Measured on issue 809: 79.38% reported
versus 77.03% recomputed, a 2.35 point overstatement on first-party branch coverage.

Both figures clear the 75% branch threshold, so no verdict changed on 809 and the merge was not
affected. The defect is that the counting method is wrong and is copied forward into other plans,
where a smaller true margin would not survive the same overstatement.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: the recomputation was performed by the feature reviewer for issue 809 from the raw
  Cobertura report committed under that item's `evidence/qa-gates/` tree.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
Surfaced by the feature review for issue 809 on 2026-09-08 and ranked by the run's orchestrator as
the highest-value of that item's eight follow-ups, on the grounds that it is the only one whose
effect propagates beyond the item that found it.

Related wording defect worth settling in the same change: `CLAUDE.md` CUT3 step 4 names
`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` while the actual coverage route in
use is `dotnet-coverage`. That mismatch is what made issue 809's AC6 read as PARTIAL on wording
alone; the reviewer recomputed AC6's measurable clauses from raw Cobertura and they pass.


## Proposed Fix

### Design summary (what changes where):

### Boundaries and invariants to preserve:

### Dependencies or blocked work:

### Implementation strategy (what changes, not sequencing):
	
#### Files/modules to change:

#### Functions/classes/CLI commands impacted:

#### Data flow and validation changes:

#### Error handling and logging updates:

#### Rollback/feature-flag considerations (if applicable):

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

#### Required configuration keys and defaults:

#### Backward-compatibility expectations:

#### Performance constraints (latency/throughput/memory):

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
- Constraints (budget, performance, compatibility):
- External dependencies (services, libraries, releases):

## Data / API / Config Impact
- User-facing or API changes:
- Data or migration considerations:
- Logging/telemetry updates (if any):
- Compatibility notes (CLI flags, config schemas, versioning):

## Test Strategy
Seeded from issue:

- [ ] Unit coverage areas: a test over a small fixed Cobertura fixture containing duplicate method
      rows, asserting the aggregation returns the de-duplicated rate.
- [ ] Integration scenario to retest: re-derive the coverage figures for a recently merged item and
      confirm the corrected method reproduces the reviewer's recomputation.
- [ ] Manual verification notes: identify every atomic plan template or skill that pins the current
      aggregation text, so the correction propagates rather than being fixed in one plan.

- Regression tests to add or update:
- Unit tests (pytest) for the fixed behavior and boundaries:
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):


## Acceptance Criteria
- [ ] Repro steps now produce the expected behavior in all documented environments.
- [ ] Regression test(s) added and passing (list file path and test name).
- [ ] Edge cases and invalid inputs are handled with correct errors or fallbacks.
- [ ] No unintended behavior changes outside the defined scope.
- [ ] Required logs/telemetry updated and validated (if applicable).
- [ ] Performance constraints met or explicitly waived with rationale.
- [ ] Full toolchain pass completed (format → lint → type-check → test).
- [ ] Docs/config references updated to match the new behavior.

## Risks & Mitigations
- Technical or operational risks:
- Mitigations and rollbacks:

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
