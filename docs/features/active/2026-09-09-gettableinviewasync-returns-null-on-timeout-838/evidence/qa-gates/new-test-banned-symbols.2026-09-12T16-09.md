# P4-T15 — The new test file introduces no banned symbol and no non-deterministic timing

Timestamp: 2026-09-13T03-21

Command: the plan's fixed search-gate form applied to `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncFailureContractTests.cs` for four absence literals and one presence literal, plus one case-sensitive regular expression. The pattern was echoed before use and delivered intact as `new CancellationTokenSource\([^)]`; it was constructed from `[char]92` because the argv conversion layer de-doubles a doubled backslash written directly in the payload.

EXIT_CODE: 0

```
CTOR_PATTERN=new CancellationTokenSource\([^)]
CANCELAFTER_COUNT=0
SLEEP_COUNT=0
DELAY_COUNT=0
TIMED_CTOR_COUNT=0
PLAIN_CTOR_COUNT=7
```

Output Summary: all six acceptance clauses hold. The four absence counts are exactly 0 and `PLAIN_CTOR_COUNT` is 7, which is at least 1.

P0-T21 recorded a non-zero positive control for each of the four absence searches, read from a named file in the tree through this same file-reading search form: 1 for the timer call, 2 for the thread sleep, 3 for the task delay and 10 for the timed constructor pattern. Each of those four gates therefore has a demonstrated ability to find its literal, so a zero here is a real absence rather than a search that cannot match.

The presence count is what shows the absences are achieved by construction rather than by omitting cancellation sources altogether: the file builds all seven of its `CancellationTokenSource` instances through the parameterless constructor and drives cancellation explicitly with a `Cancel()` call where a test needs it. That is why no test in the file depends on elapsed wall-clock time: a source that is already cancelled or never cancelled has a determinate state at every point in the test, whereas a timed constructor or a timer call would make the outcome a race against the scheduler. This decides acceptance criterion 13.
