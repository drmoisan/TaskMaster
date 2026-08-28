# Phase 6 — Test Determinism Audit

Timestamp: 2026-08-27T14-19
Task: [P6-T8]
Command: `git grep -nE 'Thread\.Sleep|Task\.Delay|DateTime\.Now|Path\.GetTempPath|GetTempFileName' -- QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`
EXIT_CODE: 1

## Output Summary

**The search returns zero hits.** The acceptance condition holds.

`git grep` exits 1 when it finds no match, which is the expected and required outcome here; exit 0
would mean at least one banned construct is present.

| Banned construct | Hits |
| --- | --- |
| `Thread.Sleep` | 0 |
| `Task.Delay` | 0 |
| `DateTime.Now` | 0 |
| `Path.GetTempPath` | 0 |
| `GetTempFileName` | 0 |

Scope searched: `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` and
`QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`, the two owned test files.

This satisfies the determinism-infrastructure requirements of
`.claude/rules/general-unit-test.md`: no real wall-clock wait, no wall-clock read outside a clock
seam, and no filesystem temporary file. Every clock read in these files goes through
`FakeTimeProvider` or an injected factory, and every write goes through the injected
`MetricsFileWriter` delegate rather than the filesystem, which is what makes AC-2 through AC-5
assertable at all.

The deletion of `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` by [P5-T11]
removed the last test in either file whose subject was a delay seam. Its removal is recorded as a
deliberate disposition in `evidence/other/pr-body-statements.2026-08-26T11-31.md`.

Corroboration from the [P6-T5] run: total suite time was 46.06 seconds for 6701 tests with zero
failures, which is consistent with a suite that contains no wall-clock wait.
