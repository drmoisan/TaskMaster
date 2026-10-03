# 2026-10-02-remaining-stale-binding-redirect-pairs (Spec)

- **Issue:** #973
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-02T22-16
- **Status:** Draft
- **Version:** 0.1

## Context
Preparation for #953 found that, besides the 11 Fizzler redirects #953 fixes, 15 other assembly and version pairs in the repository's `app.config` files redirect to a `newVersion` that no deployed assembly has. #953 adds a Pester test that records these 15 pairs as known exceptions and fails on any new mismatch. This issue covers correcting the 15 pairs and emptying that list.

Environment:
- OS/version: Windows 11 (Outlook VSTO add-in)
- Python version: n/a (.NET Framework 4.8 `app.config` binding redirects)
- Command/flags used: the #953 redirect-consistency Pester test
- Data source or fixture: `*/app.config` against `packages/` assembly versions

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. After #953 merges, read the known-mismatch list in the #953 Pester test. Plan task P2-T16 records the 15 pairs.
2. Compare each pair's `newVersion` with the deployed assembly version.

Expected:
Every `bindingRedirect` `newVersion` names an assembly version that is actually deployed, so the known-mismatch list is empty.

Actual:
15 pairs name versions that are not deployed. They are latent today, but each becomes a load failure as soon as a dependency requests that assembly, which is how #418 happened.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: #953 plan P2-T16 and its evidence.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
Package updates advanced deployed versions without a matching sweep of the redirects. This is the same cause as #953 and #418. Sequence it after #953 merges.


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

- [ ] For each pair, correct the redirect or remove it when nothing references the assembly. Remove the pair from the known list in the same change, so the test proves each fix.
- [ ] Re-test the #418 designer path for `PictureBoxSVG` after the sweep.

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
