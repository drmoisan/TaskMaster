# Fail-Before Exception Dossier — Defect B (progress package source ownership)

Timestamp: 2026-09-13T15-22
Task: [P1-T17]

WhyFailingRunImpossible: The three AC4 tests call a `Dispose` method that does not exist on
`ProgressPackage` before P1-T4 adds it, so the UtilitiesCS test assembly does not compile while the
tests are present and the fix is not. A test that cannot be compiled cannot be observed to fail at run
time, so there is no run in which these three tests execute and report a failing outcome.

## Absence-Of-Test Proof

Before this delivery, `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs` contained exactly four
test methods and none of them made any disposal assertion:

- `InitializeAsync_ShouldUseProvidedTrackerTokenAndStopwatch` — asserts that the injected source,
  token, tracker and stop watch are the ones the package holds.
- `CreateAsTupleAsync_ShouldReturnProvidedDependencies` — asserts the tuple factory returns the
  injected dependencies.
- `SpawnChild_ShouldReuseSharedState_AndCreateChildProgressTracker` — asserts the child reuses the
  parent's source, token and stop watch and gets its own tracker.
- `ToTupleAndToTuplePane_ShouldExposeCurrentPropertyValues` — asserts both tuple projections expose the
  current property values.

None of the four reads a cancellation token after a disposal, none calls any release path, and the type
exposed no `Dispose` method to call. The behaviour AC4 governs was therefore untested rather than
tested and failing, which is what this dossier records in place of a failing run.

## Search Record

SearchScope: `docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/regression-testing`
SearchPatterns: `fail-before-exception.*.md`
SearchResult: none — the enumeration returned a count of 0 before this artifact was written, so this
is the first and only fail-before dossier in that folder.

## The Other Two Defects Carry No Fail-Before Obligation

- **Defect A (AC1, AC2).** The dequeue gate's production source already emits every field the two new
  tests assert, and is outside the Write Set. The defect is a missing assertion rather than a
  behavioural fault, so there is no pre-change behaviour for a failing test to expose. Both tests were
  expected to pass immediately and P1-T16 records that they did, 2 of 2 passed at exit 0.
- **Defect C (AC6, AC7).** It is a deletion of dormant code and of the two Compile items that named it.
  A deletion has no behaviour to reproduce, so no regression test can be written to fail before it.

## Pass-After Evidence

The pass-after half of the pair is recorded by
`docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/regression-testing/p1-t15-utilitiescs-scoped.md`,
which records `TotalTests: 3` and `Passed: 3` at `EXIT_CODE: 0` for the three AC4 tests once P1-T4 had
landed.
