# 2026-09-29-engine-toggle-prime-fault-logging-test-races (Spec)

- **Issue:** #942
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-29T23-07
- **Status:** Draft
- **Version:** 0.1

## Context
`EngineToggleStateCoordinatorTests.GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` failed once in CI and passed on rerun. The fault logging it asserts appears to race the task the test awaits.

Environment:
- OS/version: windows-latest (GitHub Actions)
- Python version: n/a (C# / MSTest)
- Command/flags used: required check MSTest with coverage
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Run `TaskMaster.Test` under the parallel regime (Workers=0, Scope=ClassLevel).
2. Observe `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` (`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs:213`). It fails intermittently.

Expected:
The test is deterministic: the error log it asserts is written before the awaited task completes, or the test awaits the logging continuation itself.

Actual:
It failed once on PR #939 head `9624376dc`, passed on a single rerun, and passed in two local runs. PR #939 does not touch this code.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: CI run for PR #939 at head `9624376dc` (first attempt).


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
The prime fault is probably observed and logged in a continuation that is not part of the awaited task, so the assertion can run before the log call. This is a determinism defect that hits a required check. Fix it by awaiting or injecting the continuation, not by retries, sleeps, `[DoNotParallelize]`, or Workers=1.


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

- [ ] Write a regression test that forces the ordering deterministically (for example a controllable scheduler or a `TaskCompletionSource` gate), then fix the coordinator or the test seam.
- [ ] Negative control: show that the test fails when the ordering is inverted.

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
