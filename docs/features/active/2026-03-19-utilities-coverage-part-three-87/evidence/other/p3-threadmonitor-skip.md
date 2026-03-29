# P3-T4: Skip Re-Validation — ThreadMonitor.cs

## File
`UtilitiesCS\Threading\ThreadMonitor.cs`

## Current Coverage
`line-rate="0"` (0%) — no corresponding test file exists.

## Source Analysis
`ThreadMonitor` contains a background `Task.Run` loop that polls a WPF `Dispatcher` tied to a live `Thread`, measures UI delays using sleeps, emits debug/log output, and captures stack traces with `Thread.Suspend()`/`Thread.Resume()`.

## Skip Rationale
The class depends on non-deterministic timing, a live dispatcher-bound UI thread, and obsolete thread-suspension APIs. Reliable unit tests would require complex threading orchestration and are likely to be flaky or hang-prone. The code is infrastructure/diagnostic logic rather than stable domain behaviour.

## Decision: Skip Confirmed
