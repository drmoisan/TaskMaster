# Phase 6 — Coverage Delta

Timestamp: 2026-08-26T11-30
Task: [P6-T6]
Command: XML aggregation over `evidence/baseline/mstest-coverage.2026-08-26T10-42.md`'s source
document and over `coverage\coverage.cobertura.xml` as left by [P6-T5]
EXIT_CODE: 0

## Document comparability (required disclosure)

The two Cobertura documents are **not** in the same form.

- The [P0-T9] baseline document is **post-processed**. That run exited zero, so
  `Invoke-MSTestWithCoverage.ps1` reached `ConvertTo-KoverageCoberturaXml` at `:340` and the
  `Set-Content` at `:344`. Its `filename` attributes are repository-relative and it carries one
  pre-merged `<class>` element per source file.
- The [P6-T5] document is **un-post-processed**. That run had one failing test, so the throw at
  `:236` fired before `:340`. Its `filename` attributes are absolute host paths, it has no
  `<sources>` element, third-party and vendored packages remain in the denominator, and it carries
  one `<class>` element per compiler-generated type.

**Which document was un-post-processed: the [P6-T5] post-change document.**

Both sides of every per-file and per-member comparison below were read using the same `filename`
form and the same aggregation: match the file by path suffix, union every `<class>` element that
shares that file, key the `<line>` elements by line number, sum hits per line number, and count a
line number as covered when its summed hits exceed zero. Suffix matching makes the absolute and the
repository-relative spellings resolve to the same file, so the per-file and per-member figures are
method-comparable.

The **repository-wide** figures are **not** comparable, because the raw denominator includes
third-party and vendored code that the post-processed denominator excludes. They are recorded below
for completeness and are explicitly not treated as a delta. Per the spec, the repository-wide figure
is a record-and-report obligation and carries no pass/fail condition for this feature in any case,
because no merge-base baseline existed at spec time.

## Repository-wide (recorded values only, no pass/fail condition, not comparable)

| Metric | Baseline [P0-T9] (post-processed) | Post-change [P6-T5] (raw) | Signed difference |
| --- | --- | --- | --- |
| Line rate | 84.84% | 70.28% | **-14.56 pp** |
| Branch rate | 78.82% | 58.87% | -19.95 pp |
| Lines covered | 53912 | 57392 | +3480 |
| Lines valid | 63543 | 81667 | +18124 |

The negative difference is an artifact of the denominator change, not a coverage regression: lines
covered rose by 3480 while lines valid rose by 18124, because the raw document counts 18124 more
lines of third-party and vendored code that the post-processed document excludes. Comparing the two
figures as a delta is not meaningful, and no conclusion is drawn from it.

## Per-file line-rate, five owned production files (comparable)

| Owned production file | Baseline | Post-change | Signed difference |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcHomeController.cs` | 68.40% | 76.23% | **+7.83 pp** |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 63.31% | 80.00% | **+16.69 pp** |
| `QuickFiler/Controllers/EfcHomeController.cs` | 97.81% | 98.25% | **+0.44 pp** |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 97.73% | 100.00% | **+2.27 pp** |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | 90.41% | 82.61% | **-7.80 pp** |

Four of the five owned production files gained coverage. The two files carrying the bulk of the
change, `QfcHomeController.cs` and `QfcHomeController.Metrics.cs`, gained the most, which is the
combined effect of the new flush tests and of [P5-T9] and [P5-T10] deleting a large block of
unreachable code from the denominator.

### The `EfcHomeController.ExecuteMoves.cs` regression is caused by the [P6-T5] test failure

This is the only per-file regression, and it is attributable to the failing test rather than to the
change. `ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields` is the only test
in the suite that drives `ExecuteMovesAsync` through its guard-taken path. It threw inside its
arrange step at `EfcHomeControllerTests.cs:64` before reaching the act step, so `ExecuteMovesAsync`
was never invoked in this run and its lines went uncovered.

The members that test would otherwise have exercised are `ExecuteMovesAsync` and its `finally`
block. `TryBeginExecuteMoves` and `ResetExecuteMovesState`, the two members this feature actually
rewrote in that file, both measure 100.00% below.

Once the conflict recorded in [P6-T5] is resolved, this figure is expected to return to or exceed
its baseline. It is reported here as measured, not adjusted.

## Per-member line-rate for the members named in the spec's Test Strategy

| Member | Line-rate | Covered | Total | Meets the 90.00% floor |
| --- | --- | --- | --- | --- |
| `BuildQuickFileMetricLines` | **100.00%** | 20 | 20 | yes |
| `SelectMoveMetricsItems` | **100.00%** | 4 | 4 | yes |
| `TryBeginExecuteMoves` | **100.00%** | 3 | 3 | yes |
| `ResetExecuteMovesState` | **100.00%** | 3 | 3 | yes |
| `QuickFileMetrics_WRITE` (all four overloads across both controllers) | **88.37%** | 76 | 86 | see justification |
| `WriteMetricsAsync` | **100.00%** | 37 | 37 | yes |

Five of the six members reach or exceed 90.00%. Four reach 100.00%.

`BuildQuickFileMetricLines` rose from 16 to 20 covered lines, reflecting the added
`folderText` extraction and the wider interpolation introduced by [P2-T5] through [P2-T7], with
every added line covered.

### `WriteMetricsAsync` measurement method

`WriteMetricsAsync` is an `async` method, so its body compiles into a compiler-generated state
machine and does not appear as a `<method>` element attributable by name. It was measured by source
line range instead: lines 107 through 181 of
`QuickFiler/Controllers/QfcHomeController.Metrics.cs`, which span its signature through its closing
brace, taken against the aggregated file-level line map. Every one of the 37 instrumented lines in
that range is covered.

### `QuickFileMetrics_WRITE` justification for the residual 10 lines

The 88.37% figure aggregates **four** overloads across two controllers, which is how the spec names
the member. The residual uncovered lines are confined to the Outlook-appointment block of the QFC
overload at `QuickFiler/Controllers/QfcHomeController.Metrics.cs:79-89`:

```csharp
if (olEmailCalendar is not null)
{
    olAppointment = (AppointmentItem)olEmailCalendar.Items.Add();
    olAppointment.Subject = ...;
    ...
    olAppointment.Save();
}
```

Those lines are unreachable without a live Outlook process. `UtilitiesCS.Calendar.GetCalendar`
resolves an "Email Time" calendar subfolder through `Globals.Ol.App.Session`, and in every unit
fixture that lookup returns `null`, so the `is not null` branch is never entered. Entering it
requires a real `Folder` whose `Items.Add()` returns a real `AppointmentItem` that can be assigned a
`Subject`, `Start`, `End`, `Categories`, `ReminderSet`, and `Sensitivity` and then `Save()`d to a
MAPI store. A Moq stand-in cannot satisfy `Save()`, which is the operation whose side effect the
block exists to produce, so the branch is exempt under the COM/VSTO coverage exemption in
CLAUDE.md § UT2 (c).

This is a member-specific justification, not a blanket exemption: the same overload's guard,
duration computation, culture formatting, diagnostics call, and write are all covered, and the three
EFC overloads of `QuickFileMetrics_WRITE` are fully covered, including the newly implemented
single-argument overload from [P2-T8].

The same boundary is recorded independently by [P7-T3].

## Changed-line regression check

No line changed by this feature lost coverage. Every production line this feature added or modified
sits inside one of the six members above, and each of those members is either at 100.00% or, for
`QuickFileMetrics_WRITE`, has its uncovered residue confined to the pre-existing Outlook-appointment
block that this feature did not touch.
