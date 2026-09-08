# 2026-09-07-utilitiescs-test-determinism-780-803-594 (Spec)

- **Issue:** #811
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-07T22-03
- **Status:** Draft
- **Version:** 0.1

## Context
Consolidates the `UtilitiesCS.Test` nondeterminism filed as #780, #803, and #594 into one item so the required `mstest-coverage` check stops failing on unrelated pull requests. #803 is a duplicate of the `DfDeedle_COM_Tests` `NullReferenceException` already in #594: `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` fails under parallel load because the class mutates static seams and production code at `DfDeedle.cs:186` dereferences `tableSnapshot.Item1` without a null check. #780 is `DictionaryExtensions.TryAddValuesAsync` cancelling its inner `Task.Run` after a hard-coded 500 ms wall-clock window, which the thread pool exceeds under coverage instrumentation with 24 class workers. #594 also carries two `Console.Out` races between concurrently executing tests. #592 (QuickFiler pump-host 60 s expiry) is deliberately not included: it is a different assembly and needs its own investigation.

Environment:
- OS/version: Windows 11 Pro 10.0.26200 locally; GitHub Actions windows runner in CI
- Runtime: .NET Framework 4.8.1 test host, VSTest 18.9.0, MSTest 4.4.0, assembly-level parallelization with 24 class-scope workers
- Command/flags used: `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation /Logger:trx "/TestCaseFilter:TestCategory!=LiveOutlook"`; the repository `mstest-coverage` job
- Data source or fixture: in-memory dictionaries; mocked `Table` and `MAPIFolder` in `DfDeedle_COM_Tests`

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: a nondeterministic failure in a required check blocks unrelated pull requests and erodes the signal the gate exists to provide. Production callers of `TryAddValuesAsync` may also receive a spurious cancellation under starvation.


## Repro & Evidence
Steps to Reproduce:
1. Run the full suite under coverage with parallel class workers. Observe, intermittently: `TryAddValuesAsync_UpdatesExistingValue` failing with `TaskCanceledException` after ~20 s (passes in ~2 ms alone); `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` failing with `NullReferenceException` at `DfDeedle.cs:186`; two `Console.Out` races.
2. Re-run the identical head commit with no change and observe all checks passing. Observed on PR #802 at `b2132349` (7107 of 7108 passed, then green on re-run) and during PR #779 verification on 2026-09-04.

Expected:
- Every test in `UtilitiesCS.Test` passes deterministically regardless of class ordering and parallel load.
- `TryAddValuesAsync` does not fail production callers on thread-pool scheduling latency.
- Production code at `DfDeedle.cs:186` does not dereference a possibly-null snapshot element.
- A full nine-assembly `/InIsolation` run reports zero failures on ten consecutive runs.

Actual:
Intermittent failures as above, each blocking a required check on an unrelated pull request and inviting re-run-until-green.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet (#780, 2026-09-04): `System.Threading.Tasks.TaskCanceledException: A task was canceled. at UtilitiesCS.DictionaryExtensions.<TryAddValuesAsync>d__10\`2.MoveNext() in UtilitiesCS\Extensions\DictionaryExtensions.cs:line 179`. Local run 4767 tests, 4766 passed, the failing test alone took 21 s.
- Snippet (#803, PR #802 run 1): `System.NullReferenceException at UtilitiesCS.DfDeedle.GetEmailDataInViewAsync, UtilitiesCS/Extensions/DfDeedle.cs line 186`, Total 7108, Passed 7107, Failed 1.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
- #780: wall-clock timeout in production code measures scheduling latency, not the operation; the fix is to remove the hard-coded window or inject a `TimeProvider` so tests use `FakeTimeProvider`, per `.claude/rules/csharp.md` time-seam guidance.
- #803 / #594 item 1: static seam mutation in `DfDeedle_COM_Tests` races other classes; the production null dereference at `DfDeedle.cs:186` turns a race into an NRE. Fix both: make the seam instance-scoped or serialize the class with `[DoNotParallelize]` on a documented basis, and guard the snapshot dereference.
- #594 items 2 and 3: eliminate the shared-console dependency rather than serializing with sleeps or retries.
- Superseded issues: #780, #803, #594 (close with a pointer to this issue).


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

Acceptance criteria:

- [ ] AC1: `TryAddValuesAsync` no longer cancels on a fixed wall-clock window, or the window is driven by an injected `TimeProvider`; the test passes deterministically under 24-worker parallel coverage runs.
- [ ] AC2: `DfDeedle_COM_Tests` no longer mutates process-wide static seams in a way another class can observe, and `DfDeedle.cs:186` guards the null snapshot element with a descriptive failure.
- [ ] AC3: The two `Console.Out` races are removed by eliminating the shared-console dependency.
- [ ] AC4: A full nine-assembly `/InIsolation` run with `TestCategory!=LiveOutlook` reports zero failures on ten consecutive runs, recorded as evidence.
- [ ] AC5: No test is stabilized by a sleep, a retry, or a timing tolerance.

Validation:

- [ ] Unit coverage areas: `DictionaryExtensions.TryAddValuesAsync` with `FakeTimeProvider`; `DfDeedle.GetEmailDataInViewAsync` null-snapshot path.
- [ ] Integration scenario to retest: ten consecutive full-suite runs locally and one CI run.
- [ ] Manual verification notes: none.

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
