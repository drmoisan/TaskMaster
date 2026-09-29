# 2026-09-28-tests-depend-on-uncontrolled-environment (Spec)

- **Issue:** #931
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-28T20-01
- **Status:** Draft
- **Version:** 0.1

## Context
Consolidates #905 and #906. The shared root cause: a unit test depends on environment state it does not control, so its result depends on scheduling or on other processes rather than on the code under test.

1. **#905:** tests use `Task.Run` as the "other thread". `Task.Run` guarantees only a thread-pool thread, never a different one, so under parallel execution the guard under test can go unexercised. PR #904 (#900) fixed two instances. Remaining:
   - `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs:332`. The file is 490 lines, so the fix requires a split to stay under 500.
   - Candidates to triage:
     - `BreadcrumbSelectorToggleUiBoundaryTests.cs:75`
     - `BreadcrumbPopupControlDispatchTests.cs:29,111`
     - `BreadcrumbPopupBoundaryCoverageTests.cs:58`
     - `BreadcrumbPopupBoundaryCoverageTests.Part2.cs:192`
     - `BreadcrumbUiThreadDispatchTests.cs:90,301`
   - `Task.Run(() => tcs.SetResult(...))` calls that only complete a task are not affected.
2. **#906:** `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs:56-62` opens the repository's own `TaskMaster.sln`, found by `GetSolutionFile()` at lines 340-352, as a fixture. Resident MSBuild node-reuse workers can hold that file open, so the outcome depends on build history.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (C#, MSTest, net48)
- Command/flags used: parallel regime `/Settings:TaskMaster.runsettings` (Workers 0, Scope ClassLevel)
- Data source or fixture: files listed above, `main` at `177b6d78e`

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Inspect the cited `Task.Run` sites and confirm that each asserts a thread-identity property against the thread it obtained.
2. Inspect `FileInfoWrapper_Tests.GetSolutionFile()` and confirm it resolves the repository's own solution file.

Expected:
- A test that needs a distinct thread uses a dedicated `Thread` that is joined, and asserts inside that thread that it is distinct (for example `CheckAccess() == false`) before exercising the guard. This is the #900 pattern.
- A file-handle test uses a stream the test owns, supplied through the wrapper's seam or an in-memory stream. It never uses a repository file.
- **Temporary files are prohibited by the unit-test policy and must not be used.**

Actual:
The guard under test can pass without being exercised, and the file-open test can fail or pass depending on MSBuild worker residency.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: none (static findings, verified present on 2026-09-28)


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
Both patterns were copied from earlier tests and survived because they usually pass. Tests must run in parallel. Do not fix either defect with `Workers=1`, `[DoNotParallelize]` or retries.


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

- [ ] Apply the #900 dedicated-thread pattern at every triaged site. Split `ItemViewerBreadcrumbThreadAffinityTests.cs` so it stays at or under 500 lines, and register any new file in `QuickFiler.Test.csproj`.
- [ ] Replace the `TaskMaster.sln` fixture with a test-owned stream or an injected seam. Add a seam to the wrapper only if one does not already exist.
- [ ] Each rewritten test must be shown to fail against a deliberately broken guard, so the test demonstrably exercises the guard.
- [ ] Run the full `QuickFiler.Test` and `UtilitiesCS.Test` suites in the parallel regime.

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
