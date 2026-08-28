# Phase 6 — Coverage Delta, Baseline vs Post-Change

Timestamp: 2026-08-27T14-19
Task: [P6-T6]
Command: Cobertura comparison of the [P0-T9] baseline document against the [P6-T5] post-change document, using one aggregation method applied identically to both
EXIT_CODE: 0

## Comparability

Both documents are the **post-processed** Koverage form: repository-relative `filename` attributes,
a `<sources>` element, third-party packages excluded, one pre-merged `<class>` element per file.
Neither run recorded `COVERAGE_FLOOR_TRIPPED`, so neither document was left un-post-processed and
the repository-wide figures are directly comparable. Both sides of every per-file comparison were
read using the same repository-relative `filename` form.

The comparison method was validated by re-deriving the baseline artifact's published figures from
the baseline document: 84.84 percent repository-wide and 68.40 / 63.31 / 97.81 / 97.73 / 90.41
percent per file, all reproduced exactly.

## Repository-wide line-rate — record-and-report, not a gate

| Metric | Baseline | Post-change | Signed difference |
| --- | --- | --- | --- |
| `line-rate` | 84.8433% | **85.1255%** | **+0.2822** |
| `branch-rate` | 78.8181% | **79.2096%** | **+0.3915** |

Baseline counters: `lines-covered=53912`, `lines-valid=63543`, `branches-covered=12737`,
`branches-valid=16160`.
Post-change counters: `lines-covered=54379`, `lines-valid=63881`, `branches-covered=12927`,
`branches-valid=16320`.

Per the spec, the repository-wide figure is a record-and-report obligation and carries no pass/fail
condition for this feature, because no merge-base baseline existed at spec time. It is recorded
above and it moved **up** on both measures, so AC-22's "does not lower it" clause is satisfied as a
matter of fact rather than by exemption. Note that the post-change denominator also includes the
sibling code that arrived with merge `c1826965`, so the +0.2822 is not attributable to this feature
alone; the change-scoped figures below are the ones this feature owns.

## Per-file line-rate, five owned production files

| Owned production file | Baseline | Post-change | Signed difference |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcHomeController.cs` | 68.40% | **76.23%** | **+7.83** |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 63.31% | **80.00%** | **+16.69** |
| `QuickFiler/Controllers/EfcHomeController.cs` | 97.81% | **98.25%** | **+0.44** |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 97.73% | **100.00%** | **+2.27** |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | 90.41% | **89.86%** | **-0.56** |

### The one file that moved down, explained rather than excused

`EfcHomeController.ExecuteMoves.cs` reads 90.41% (66/73) at baseline and 89.86% (62/69) after.
**No line lost coverage.** The uncovered count is identical on both sides:

- baseline: 73 coverable, 66 covered, **7 uncovered**
- post-change: 69 coverable, 62 covered, **7 uncovered**

The file's coverable-line count fell by 4 because [P3-T5] replaced a nine-line `TryBeginExecuteMoves`
body with a three-line `Interlocked.CompareExchange` form. Four covered lines were deleted along
with it. Holding the uncovered count constant while shrinking the denominator lowers the ratio
arithmetically: 7/73 uncovered is 9.59 percent, 7/69 uncovered is 10.14 percent. The seven residual
uncovered lines are the same seven on both sides — lines 40 to 42 and 44 to 46, the
`try { await ExecuteMovesCoreAsync(); } finally { ResetExecuteMovesState(); }` body, and line 144,
the COM-bound `QuickFileMetrics_WRITE` call in `HandleMoveResult`. All are reachable only with a
live Outlook `MailItem` collection.

## Changed-line coverage — the gating figure

This is the measurement AC-22's "no line changed by this feature loses coverage" clause actually
asks for. Changed lines are the `+` side line numbers of
`git diff <merge-base> HEAD --unified=0`, where the merge base is
`0ddab4107b3b147e706a6c15856888b3b5d6404b`, intersected with the set of coverable lines in the
post-change document.

| Owned production file | Changed lines | Coverable | Covered | Changed-line coverage | Uncovered changed lines |
| --- | --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcHomeController.cs` | 0 | 0 | 0 | not applicable (pure deletion) | none |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 35 | 10 | 10 | **100.00%** | none |
| `QuickFiler/Controllers/EfcHomeController.cs` | 7 | 2 | 2 | **100.00%** | none |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 45 | 25 | 25 | **100.00%** | none |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | 11 | 2 | 2 | **100.00%** | none |
| **Aggregate** | 98 | **39** | **39** | **100.00%** | **none** |

Every executable line this feature changed is covered. `QfcHomeController.cs` contributes no
changed line because its entire contribution is the deletion of the defective metrics machinery
(RC-1); a deleted line has no coverage to lose.

## Per-member line-rate for the six members named in the spec's Test Strategy

| Member | Baseline | Post-change | Verdict |
| --- | --- | --- | --- |
| `BuildQuickFileMetricLines` | 100.00% (16/16) | **100.00%** (20/20) | meets >= 90% |
| `SelectMoveMetricsItems` | 100.00% (4/4) | **100.00%** (4/4) | meets >= 90% |
| `TryBeginExecuteMoves` | 100.00% (7/7) | **100.00%** (3/3) | meets >= 90% |
| `ResetExecuteMovesState` | 100.00% (3/3) | **100.00%** (3/3) | meets >= 90% |
| `WriteMetricsAsync` | not addressable | **100.00%** (37/37) | meets >= 90% |
| `QuickFileMetrics_WRITE` (all overloads, both controllers) | 84.72% (61/72) | **88.37%** (76/86) | below 90%; named justification below |

### `WriteMetricsAsync` measurement method

`WriteMetricsAsync` is `async`, so its body compiles into a compiler-generated state machine and the
Koverage per-file merge retains no `<method>` wrapper for it. It is therefore measured by source-line
range, as [P6-T6] provides for: the declaration at
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:107` through the matching closing brace at
`:180`, intersected with the merged file-level coverable-line set. That range holds 37 coverable
lines and all 37 are covered. The baseline document has no comparable range because the member did
not exist in its post-fix form; the baseline artifact recorded it as "not addressable as a `<method>`
element" for the same reason.

### `QuickFileMetrics_WRITE` — named, member-specific justification for the 88.37% residual

The aggregate is below 90 percent. The shortfall is attributable in full to one pre-existing block
of Outlook Interop code, and **not** to any line this feature wrote. The per-overload breakdown:

| Overload | Baseline | Post-change |
| --- | --- | --- |
| `EfcHomeController.Metrics.cs` 1-argument (was a `NotImplementedException` throw) | 83.33% (5/6) | **100.00%** (11/11) |
| `EfcHomeController.Metrics.cs` 2-argument | 100.00% (2/2) | **100.00%** (11/11) |
| `EfcHomeController.Metrics.cs` 3-argument | 100.00% (15/15) | **100.00%** (15/15) |
| `QfcHomeController.Metrics.cs` | 79.59% (39/49) | **79.59%** (39/49) |

Every EFC overload is at 100 percent, and the 1-argument overload improved because AC-15 replaced
its throw with a real implementation. The entire residual sits in the QFC overload, whose figure is
**byte-for-byte unchanged**: 39 covered of 49 coverable on both sides, with the same ten uncovered
lines (baseline `64-73`, post-change `81-90` after the edit shifted them).

Those ten lines are the inline Outlook appointment-creation block:

```csharp
olAppointment = (AppointmentItem)olEmailCalendar.Items.Add();
olAppointment.Subject = $"Quick Filed {emailsLoaded} emails";
olAppointment.Start = startTime;
olAppointment.End = endTime;
olAppointment.Categories = "@ Email";
olAppointment.ReminderSet = false;
olAppointment.Sensitivity = OlSensitivity.olPrivate;
olAppointment.Save();
```

They execute only when `UtilitiesCS.Calendar.GetCalendar("Email Time", Globals.Ol.App.Session)`
returns a non-null MAPI calendar. In every unit fixture that call returns `null`, so the guarded
branch body is unreachable without a live Outlook process holding an "Email Time" calendar folder.
The spec records this same boundary independently at AC-8 and in
`evidence/other/coverage-boundaries.2026-08-26T11-31.md`.

Excluding that Outlook-Interop block, `QuickFileMetrics_WRITE` measures **76/76 = 100.00%**.

This disposition is consistent with the spec's own scoping in Test Strategy, which declares that
`BuildQuickFileMetricLines`, `SelectMoveMetricsItems`, `TryBeginExecuteMoves`,
`ResetExecuteMovesState` "and the metrics writers behind their injectable seams" are testable seams
and explicitly NOT exempt, "notwithstanding the COM/VSTO exemption that applies to
Outlook-Interop-bound members of the same classes". The uncovered block is not behind an injectable
seam; it is a direct Interop call sequence, and it is exactly what that exemption addresses. The
seam-reachable portion of the member is fully covered.

Per CLAUDE.md § UT5, this is called out explicitly rather than silently absorbed: **one of the six
named members reports 88.37 percent against a 90 percent target, and the reason is the ten
Outlook-Interop lines enumerated above.**
