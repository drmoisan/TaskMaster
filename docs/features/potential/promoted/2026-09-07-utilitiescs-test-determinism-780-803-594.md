# utilitiescs-test-determinism-780-803-594 (Issue #811)

- Date captured: 2026-09-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilitiescs-test-determinism-780-803-594/ (Issue #811)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #811
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/811
- Last Updated: 2026-09-08
## Summary

Consolidates the `UtilitiesCS.Test` nondeterminism filed as #780, #803, and #594 into one item so the required `mstest-coverage` check stops failing on unrelated pull requests. #803 is a duplicate of the `DfDeedle_COM_Tests` `NullReferenceException` already in #594: `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` fails under parallel load because the class mutates static seams and production code at `DfDeedle.cs:186` dereferences `tableSnapshot.Item1` without a null check. #780 is `DictionaryExtensions.TryAddValuesAsync` cancelling its inner `Task.Run` after a hard-coded 500 ms wall-clock window, which the thread pool exceeds under coverage instrumentation with 24 class workers. #594 also carries two `Console.Out` races between concurrently executing tests. #592 (QuickFiler pump-host 60 s expiry) is deliberately not included: it is a different assembly and needs its own investigation.

## Environment

- OS/version: Windows 11 Pro 10.0.26200 locally; GitHub Actions windows runner in CI
- Runtime: .NET Framework 4.8.1 test host, VSTest 18.9.0, MSTest 4.4.0, assembly-level parallelization with 24 class-scope workers
- Command/flags used: `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation /Logger:trx "/TestCaseFilter:TestCategory!=LiveOutlook"`; the repository `mstest-coverage` job
- Data source or fixture: in-memory dictionaries; mocked `Table` and `MAPIFolder` in `DfDeedle_COM_Tests`

## Steps to Reproduce

1. Run the full suite under coverage with parallel class workers. Observe, intermittently: `TryAddValuesAsync_UpdatesExistingValue` failing with `TaskCanceledException` after ~20 s (passes in ~2 ms alone); `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` failing with `NullReferenceException` at `DfDeedle.cs:186`; two `Console.Out` races.
2. Re-run the identical head commit with no change and observe all checks passing. Observed on PR #802 at `b2132349` (7107 of 7108 passed, then green on re-run) and during PR #779 verification on 2026-09-04.

## Expected Behavior

- Every test in `UtilitiesCS.Test` passes deterministically regardless of class ordering and parallel load.
- `TryAddValuesAsync` does not fail production callers on thread-pool scheduling latency.
- Production code at `DfDeedle.cs:186` does not dereference a possibly-null snapshot element.
- A full nine-assembly `/InIsolation` run reports zero failures on ten consecutive runs.

## Actual Behavior

Intermittent failures as above, each blocking a required check on an unrelated pull request and inviting re-run-until-green.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet (#780, 2026-09-04): `System.Threading.Tasks.TaskCanceledException: A task was canceled. at UtilitiesCS.DictionaryExtensions.<TryAddValuesAsync>d__10\`2.MoveNext() in UtilitiesCS\Extensions\DictionaryExtensions.cs:line 179`. Local run 4767 tests, 4766 passed, the failing test alone took 21 s.
- Snippet (#803, PR #802 run 1): `System.NullReferenceException at UtilitiesCS.DfDeedle.GetEmailDataInViewAsync, UtilitiesCS/Extensions/DfDeedle.cs line 186`, Total 7108, Passed 7107, Failed 1.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: a nondeterministic failure in a required check blocks unrelated pull requests and erodes the signal the gate exists to provide. Production callers of `TryAddValuesAsync` may also receive a spurious cancellation under starvation.

## Suspected Cause / Notes

- #780: wall-clock timeout in production code measures scheduling latency, not the operation; the fix is to remove the hard-coded window or inject a `TimeProvider` so tests use `FakeTimeProvider`, per `.claude/rules/csharp.md` time-seam guidance.
- #803 / #594 item 1: static seam mutation in `DfDeedle_COM_Tests` races other classes; the production null dereference at `DfDeedle.cs:186` turns a race into an NRE. Fix both: make the seam instance-scoped or serialize the class with `[DoNotParallelize]` on a documented basis, and guard the snapshot dereference.
- #594 items 2 and 3: eliminate the shared-console dependency rather than serializing with sleeps or retries.
- Superseded issues: #780, #803, #594 (close with a pointer to this issue).

## Proposed Fix / Validation Ideas

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

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
