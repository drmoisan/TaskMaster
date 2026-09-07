# Phase 5 — QFC Test File Size Audit

Timestamp: 2026-08-26T11-26
Task: [P5-T15]
Command: `wc -l QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
EXIT_CODE: 0

## Output Summary

Measured line count of `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` after
compaction and after CSharpier formatting: **453**.

The acceptance condition is a count of at most 499. 453 satisfies it with 46 lines of margin. No
residual overage remains, so the fallback blocker branch of this task is not taken and Phase 6 may
start.

## Trajectory

| Point | Lines |
| --- | --- |
| Pre-change baseline ([P0-T10]) | 421 |
| After the Phase 4 additions ([P4-T1] through [P4-T3]) | 494 |
| After the Phase 5 additions ([P5-T2] through [P5-T6]) | 641 |
| After [P5-T11] deleted the obsolete delay-seam test | 634 |
| After the compaction described below | 426 |
| After restoring the two issue #97 tests in collapsed form | 456 |
| After CSharpier formatting | **453** |

## Compaction performed

The file exceeded the 500-line cap by 134 lines after the Phase 5 additions. The plan's stated
remedy is to shorten the new test methods by reusing `BuildLooseMetricsController()` rather than
relocating any test into the EFC file, and, if that is insufficient, to consolidate the duplicated
arrange code of the existing tests into the same helper. Both were applied. No test was relocated
and no new file was created; both remain forbidden.

### 1. Removed dead test infrastructure (about 42 lines)

The class carried a strict-`MockRepository` fixture: six private fields (`_mockRepository`,
`_mockApplicationGlobals`, `_mockParentCleanup`, `_controller`, `_mockOlApp`, `_mockExplorer`), a
`[TestInitialize] Setup()` method, and a `SetUpMockIntelRes` helper.

A reference search showed that after [P5-T11] deleted
`NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay`, **no test referenced
`_controller` or any of those fields**. The fixture's only remaining consumer was `Setup()` itself,
which existed solely to populate it. It was therefore dead code and was removed. Every surviving
test builds its own controller through `BuildLooseMetricsController()`.

### 2. Consolidated the two issue #97 tests into the shared helper (about 160 lines)

`QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow` and
`GetMoveDiagnostics_NullAppointment_DoesNotThrow` each carried roughly 70 lines of hand-rolled
arrange code that `BuildLooseMetricsController()` already produces identically: loose globals, a
`SpecialFolders` dictionary, a calendar root whose `Folders` enumerates empty so `GetCalendar`
returns null, a `GetMoveDiagnostics` stub returning an empty array, an injected `_formController`,
and an injected `_stopWatchMoved`.

Both were rewritten to call the helper. The arrange semantics are preserved exactly, including the
one difference between them: the second test's original fixture had an **empty** `SpecialFolders`
dictionary, which is now expressed as `BuildLooseMetricsController(withMyDocuments: false)`.

Both tests keep their original names, their original act step (including the original filename
arguments `test-metrics.txt` and `test-metrics-2.txt`), and their original assertion
(`act.Should().NotThrow()`). Neither assertion was weakened and neither test was deleted; the issue
#97 regression coverage is intact.

### 3. Removed usings left unused by the removals

`System.ComponentModel`, `System.Windows.Forms`, `UtilitiesCS.EmailIntelligence`, and
`UtilitiesCS.ReusableTypeClasses` were consumed only by the deleted fixture. They were removed.
`System.Collections.Generic` was removed in the same pass and then restored, because the compiler
reported `CS0246: The type or namespace name 'List<>' could not be found` at three sites in the
surviving flush tests. The compiler was treated as the authority and the removal was reverted within
this task rather than deferred.

## Verification after compaction

The scoped suite was re-run to confirm the compaction changed no outcome:

```
Test Run Successful.
Total tests: 11
     Passed: 11
 Total time: 2.0665 Seconds
```

All eleven tests pass, the same set and the same count as the [P5-T13] green run.
