---
name: project_flaky_dfdeedle_etl_250ms_timeout
description: Known intermittent CI failure — DfDeedle GetEmailDataInViewAsync NRE at DfDeedle.cs:186, caused by a 250 ms wall-clock ETL timeout swallowed into a null snapshot; not a branch defect
metadata:
  type: project
---

`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`
fails intermittently in CI with `System.NullReferenceException` at `UtilitiesCS/Extensions/DfDeedle.cs`
line 186. It is NOT caused by whatever branch happens to be red. Verify against current source before
acting — line numbers drift.

Mechanism (derived 2026-09-07, confirmed by a same-SHA re-run passing):
- `OlTableExtensions.EtlAsync` sets `milliseconds = 250 * rowCount`; the test has one row, so 250 ms.
- `ConversationId` is in `MAPIFields.BinaryToStringFields`, so the private `EtlByRowAsync` branch runs;
  it awaits two `Task.Run(...).TimeoutAfter(250, 3)`.
- `TimeOutTask.TimeoutAfter(task, ms, repeatAttempts)` is inert: it wraps `task.TimeoutAfter(ms)` in
  `try/catch(TimeoutException)`, but that overload returns a *faulted proxy Task* rather than throwing
  synchronously, so the catch never fires and the retry count is dead code. Effective budget is a
  single unretried 250 ms window.
- On expiry `EtlAsync`'s `catch (TimeoutException)` swallows it and returns `(data!, columnDictionary)`
  with `data` still null — a null behind a null-forgiving suppression in a non-nullable tuple element.
- `DfDeedle.cs:186` dereferences `tableSnapshot.Item1.GetLength(0)` in a log argument and throws.

Measured margin: 99 ms on green main, 96 ms on a green re-run, 389 ms on the red attempt, against a
250 ms deadline. Roughly 2.5x headroom on a shared runner is why it is intermittent.

**Why:** This is a real production defect, not only a test flake — a slow Outlook ETL silently yields
null and surfaces as an NRE naming neither the folder nor the step. It is the defect class issue #798
fixed on the column-add path, left uncovered on the adjacent ETL path; #798's guard at
`DfDeedle.cs:195` runs *after* the line-186 dereference so it cannot catch this.

**How to apply:** If this test is your only red check, re-run the job before diagnosing
([[ci-rerun-same-sha-discriminates-flake]]). The fix belongs in `OlTableExtensions.EtlAsync` (stop
swallowing the timeout into a null return) and `TimeOutTask.cs` (the inert retry) — both outside a
typical feature blast radius, so report the escape rather than widening
([[footprint-ac-forbids-onbranch-followup-promotion]]). As of 2026-09-07 no issue tracks it.
