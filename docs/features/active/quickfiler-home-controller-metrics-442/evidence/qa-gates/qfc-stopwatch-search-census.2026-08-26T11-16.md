# Phase 4 — QFC #443 Post-Fix Search Census

Timestamp: 2026-08-26T11-16
Task: [P4-T11]
Command: `git grep -n "Elapsed.Seconds" -- QuickFiler/Controllers/` and `git grep -n "OlEndTime.Subtract" -- QuickFiler/Controllers/QfcHomeController.Metrics.cs`
EXIT_CODE: 0

This artifact carries the search half of acceptance criteria AC-7 and AC-8. Its pre-fix counterpart
is `evidence/baseline/defect-site-census.2026-08-26T10-42.md`.

## Output Summary

| # | Search | Scope | Pre-fix hits | Post-fix hits | Required |
| --- | --- | --- | --- | --- | --- |
| 1 | `Elapsed.Seconds` | `QuickFiler/Controllers/` | 4 | **0** | zero |
| 2 | `OlEndTime.Subtract` | `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 1 | **1** | exactly one, containing `_stopWatchMoved.Elapsed` |

### 1. `git grep -n "Elapsed.Seconds" -- QuickFiler/Controllers/`

No output; `git grep` exited 1 (no match). All four pre-fix occurrences are gone:

| Pre-fix site | Disposition |
| --- | --- |
| `EfcHomeController.Metrics.cs:23` | redirected to `TotalSeconds` by [P2-T3] |
| `QfcHomeController.Metrics.cs:42` | redirected to `_stopWatchMoved.Elapsed.TotalSeconds` by [P4-T6] |
| `QfcHomeController.Metrics.cs:120` | commented-out line **deleted** by [P4-T7] |
| `QfcHomeController.Metrics.cs:121` | redirected to `_stopWatchMoved.Elapsed.TotalSeconds` by [P4-T7] |

Deleting the commented-out occurrence was mandatory rather than cosmetic. AC-7 asserts this search
returns no match, and a commented occurrence is still a match, so leaving it would have made the
criterion unsatisfiable however correct the live code became. The plan records this in its
"Planner-identified gate correction" section.

### 2. `git grep -n "OlEndTime.Subtract" -- QuickFiler/Controllers/QfcHomeController.Metrics.cs`

```
QuickFiler/Controllers/QfcHomeController.Metrics.cs:125:            OlStartTime = OlEndTime.Subtract(_stopWatchMoved.Elapsed);
```

Exactly one hit, and its text contains `_stopWatchMoved.Elapsed`, satisfying AC-8.

### Supporting search for AC-8: the truncating cast is gone

`git grep -nF "(int)Duration" -- QuickFiler/Controllers/QfcHomeController.Metrics.cs` returns no
output and exits 1. The pre-fix line reconstructed the span as
`OlEndTime.Subtract(new TimeSpan(0, 0, 0, (int)Duration))`, which discarded the sub-second part and,
because `Duration` had already been divided by `emailsLoaded` in some paths, could disagree with the
reported duration. The read is positioned before the `Duration /= emailsLoaded` division at line
131, which the current line position (125) already satisfies.

AC-8 is verified by inspection and by this census rather than by a test, because
`UtilitiesCS.Calendar.GetCalendar` returns null in every unit fixture, so `OlStartTime` never
reaches an assertable observer. That boundary is recorded by [P7-T3].
