# Phase 6 — Test Determinism Audit

Timestamp: 2026-08-26T11-30
Task: [P6-T8]
Command: `git grep -nE "Thread\.Sleep|Task\.Delay|DateTime\.Now|Path\.GetTempPath|GetTempFileName" -- QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`
EXIT_CODE: 1 (no match)

## Output Summary

The search returned **zero hits**. `git grep` produced no output and exited 1, its no-match status.

This artifact carries acceptance criterion AC-17.

| Banned pattern | Hits in the two owned test files |
| --- | --- |
| `Thread\.Sleep` | **0** |
| `Task\.Delay` | **0** |
| `DateTime\.Now` | **0** |
| `Path\.GetTempPath` | **0** |
| `GetTempFileName` | **0** |

## How determinism is achieved instead

No new time seam was introduced. Determinism in the tests this feature added comes from four
existing or structural mechanisms:

1. **The EFC parameter seam.** `BuildQuickFileMetricLines` takes `elapsedSeconds` as a plain
   parameter, so every EFC duration assertion supplies an explicit value (8, 90, 120) rather than
   measuring anything. The `MetricsNowFactory` dependency supplies a fixed `DateTime` for the date
   and time fields.
2. **Reflection-injected stopwatches on the QFC side.** `StoppedStopwatchWithElapsed(int seconds)`
   assigns the stopwatch's internal elapsed-tick field directly to `Stopwatch.Frequency * seconds`,
   producing an exact interval on any host. A start/stop pair was deliberately not used: it does not
   guarantee a non-zero elapsed value and would make the assertion time-dependent.
3. **`Stopwatch.IsRunning` rather than an elapsed comparison** for the EFC construction-site test,
   so the assertion is a state check, not a timing measurement.
4. **`Task.Yield` rather than a delay** for the flush happens-before assertion in
   `WriteMetricsAsync_CompletesWriterTaskBeforeReturning`. The delegate genuinely suspends and
   resumes without any wall-clock wait, so the invariant is asserted structurally.

`FakeTimeProvider` continues to supply the injected clock for the pre-existing issue #222 tests.

## Filesystem isolation

No test in either file touches the filesystem. This was actively corrected during [P5-T13]: after
[P5-T8] routed the flush through the `MetricsFileWriter` seam, three tests that did not assign the
seam fell through to its production default `FileIO2.WriteTextFileAsync`, which probed a real path
under the fixture's `C:\FakeDocs` root and retried 100 times with `await Task.Delay(100)` between
attempts, costing exactly ten seconds of wall-clock wait per test.

`BuildLooseMetricsController()` now assigns a no-op writer returning `Task.CompletedTask`, and every
test that asserts on the flush overrides it with its own capturing delegate. The `Task.Delay` in
question was in production code reached from a test, not in test code, so it never appeared in the
search above; it is recorded here because the determinism obligation is about test behaviour, not
only about test source text.

`C:\FakeDocs` was confirmed absent from the filesystem, so no file was created by any run.

## Scope note

This search is scoped to the two owned test files, which is what AC-17 specifies. It is not a
repository-wide determinism claim.
