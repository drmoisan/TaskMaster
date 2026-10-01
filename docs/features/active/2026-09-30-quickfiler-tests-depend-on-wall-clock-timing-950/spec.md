# 2026-09-30-quickfiler-tests-depend-on-wall-clock-timing (Spec)

- **Issue:** #950
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-01T07-11
- **Status:** Draft
- **Version:** 0.1

## Context
Several QuickFiler.Test tests fail intermittently under load because they wait on real wall-clock time. Seen during parallel run bugs-2026-09-28:
- `QfcDatamodelLivenessTests`, including `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`. Two failures stopped #944's P3-T8 gate, and one failure occurred during #929's local run.
- `QfcItemController.UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, which failed once in CI on an earlier #929 head.

Environment:
- OS/version: Windows 11 (local, with concurrent coverage runs) and windows-latest (CI)
- Python version: n/a (C# / MSTest, parallel Workers=0, Scope=ClassLevel)
- Command/flags used: `Invoke-MSTestWithCoverage.ps1`, and the CI MSTest-with-coverage required check
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Run QuickFiler.Test under the parallel regime while the machine is loaded, for example with a second coverage run.
2. Observe intermittent failures in the tests named above.

Expected:
The tests are deterministic regardless of machine load.

Actual:
- `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` uses `SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5))` (line ~56) and `.Task.Wait(TimeSpan.FromSeconds(5))` (lines ~103 and ~173). A slow scheduler turns these into assertion failures.
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:206` fails intermittently. Its cause is not yet established.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: #944 evidence for P3-T8, first run; #929 PR #949 CI history.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
Real-time bounded waits violate the determinism rules: no wall-clock waits in tests, and use of `FakeTimeProvider` or a controllable scheduler. They hit a required check. The transaction test may be a different root cause. Triage it first, and split it into its own issue if its cause is not timing.


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

- [ ] Replace the bounded waits with deterministic completion signals (`TaskCompletionSource` or awaited handles) or an injected `TimeProvider`.
- [ ] Use no retries, `[DoNotParallelize]`, Workers=1 or longer timeouts, and find any raced static state.
- [ ] Negative control: show that each rewritten test fails when the awaited signal is never set.

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
