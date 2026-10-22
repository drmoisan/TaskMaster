# P5-T5 — Coverage Delta and Thresholds (remediation cycle 1, issue #614)

Timestamp: 2026-08-26T22-34

Command: XML analysis of the two filtered Cobertura artifacts produced by P0-T9 and P5-T4 —
`coverage\coverage.cobertura.filtered.p0-t9r.xml` (baseline) and
`coverage\coverage.cobertura.filtered.p5-t4.xml` (post-change). Figures are read from the
`/coverage` root attributes for repository-level rates, and from deduplicated per-`<line>` `hits`
across all `<class>` nodes sharing a `filename` for per-file and per-line figures.

EXIT_CODE: 0

Output Summary: line coverage rose from 84.8712% to 84.8790% and branch coverage from 78.8454% to
78.8523%. `EfcSelectionGuard.cs` is at 100% line and 100% branch. No changed line lost coverage. All
stated gates met.

---

## (a) Baseline filtered figures

Source: `coverage\coverage.cobertura.filtered.p0-t9r.xml`, produced by the P0-T9 baseline run on the
unmodified tree at HEAD `0fb0efec`.

| Figure | Value | Covered / valid |
| --- | ---: | --- |
| Filtered first-party line coverage | **84.8712%** | 53973 / 63594 |
| Filtered branch coverage | **78.8454%** | 12743 / 16162 |

The delivery-cycle reference values quoted by the remediation directive are 84.8696% line and
78.8331% branch. The P0-T9 measurement is marginally higher on the same denominators (63594 lines,
16162 branches), which is the known run-to-run nondeterminism of `dotnet-coverage`. The **stricter**
of the two — the measured 84.8712% / 78.8454% — is used as this cycle's gating baseline.

## (b) Post-change filtered figures and the no-regression gate

Source: `coverage\coverage.cobertura.filtered.p5-t4.xml`, produced by the P5-T4 final clean-pass run.

| Figure | Baseline (P0-T9) | Post-change (P5-T4) | Delta | Gate | Verdict |
| --- | ---: | ---: | ---: | --- | --- |
| Filtered line coverage | 84.8712% (53973 / 63594) | **84.8790% (54000 / 63620)** | **+0.0078 pp** | `>=` baseline | **PASS** |
| Filtered branch coverage | 78.8454% (12743 / 16162) | **78.8523% (12752 / 16172)** | **+0.0069 pp** | `>=` baseline | **PASS** |

Both comparisons clear the gate on the first measurement, so the plan's single-re-run allowance for
a marginal miss was not needed and was not used.

Denominator movement is small and explicable: +26 lines and +10 branches, from the growth of
`EfcSelectionGuard.cs` (9 instrumented lines to 31) and the seven added lines in
`EfcFormController.cs`, net of the CSharpier reflow.

Both figures also exceed the delivery-cycle reference (84.8696% / 78.8331%).

**Pre-existing shortfall, reported and explicitly not gated.** The repo-wide filtered line figure of
84.8790% remains below the 85% floor in `.claude/rules/general-unit-test.md`. That shortfall
pre-dates this cycle, was already recorded as a FAIL by the delivery-cycle review, is *improved* by
this cycle, and is listed in `remediation-inputs.2026-08-26T21-00.md` as explicitly out of scope. No
gate in this plan requires the repo-wide figure to reach 85%.

## (c) New and changed methods — 100% line AND 100% branch

Gate: 100% line and 100% branch coverage for `QuickFiler/Controllers/EfcSelectionGuard.cs` as a
file, aggregating Cobertura `<class>` entries by `filename`.

The Koverage post-processing pre-merges per-file `<class>` entries, so exactly one `<class>` node
carries `filename="QuickFiler\Controllers\EfcSelectionGuard.cs"`.

| Measure | Baseline | Post-change | Gate | Verdict |
| --- | --- | --- | --- | --- |
| `<class>` `line-rate` attribute | 1 | **1** | 100% | **PASS** |
| `<class>` `branch-rate` attribute | 1 | **1** | 100% | **PASS** |
| Deduplicated covered / total lines | 9 / 9 | **31 / 31** | all covered | **PASS** |
| Uncovered line numbers | none | **none** | empty | **PASS** |

Every branch point in the file reports full condition coverage:

| Line | Condition coverage | Construct |
| ---: | --- | --- |
| 64 | 100% (2/2) | `IsValidFilingSelection` null/whitespace guard |
| 70 | 100% (2/2) | `IsValidFilingSelection` banner check |
| 75 | 100% (2/2) | `IsValidFilingSelection` `IsFullOutlookPath` early accept |
| 80 | 100% (2/2) | `IsValidFilingSelection` root-resolution conjunction |
| 99 | 100% (2/2) | `IsValidCreationSelection` null/whitespace guard |
| 105 | 100% (4/4) | `IsValidCreationSelection` three-way conjunction |

All three methods the gate names are covered, including both arms of the resolver:
`IsValidFilingSelection`, `IsValidCreationSelection`, and `ResolveArchiveRootOrEmpty` (its success
path by `ResolveArchiveRootOrEmpty_AccessorSucceeds_ReturnsRootAndLogsNothing`, its catch branch by
`ResolveArchiveRootOrEmpty_AccessorThrowsInvalidOperation_DegradesToEmpty`). Placing the resolver in
the guard rather than inline in `ActionOkAsync` is what makes the catch branch unit-reachable.

## (d) Changed lines in `EfcFormController.cs`

The change touches exactly two sites, confirmed by `git diff -U0`: seven added lines at `:706-712`
and two at `:1044-1045`.

| Line | Content | Instrumented | Hits | Justification if uncovered |
| ---: | --- | --- | ---: | --- |
| 706 | `// The D6-validated archive root read can throw; ...` | no (comment) | n/a | not an executable line |
| 707 | `// rejecting rooted selections rather than tearing the form down.` | no (comment) | n/a | not an executable line |
| 708 | `string archiveRoot = EfcSelectionGuard.ResolveArchiveRootOrEmpty(` | yes | 0 | WinForms UI event glue: `ActionOkAsync` is the OK-button handler; it reads `SynchronizationContext.Current`, drives `_formViewer`, and shows `MessageBox`, so it is not reachable from a headless unit test |
| 709 | `() => _globals.Ol.ArchiveRootPath,` | yes | 0 | same method; the delegate's own behaviour (a throwing and a non-throwing accessor) is exercised through `ResolveArchiveRootOrEmpty`, which is at 100% |
| 710 | `message => logger.Error(message)` | yes | 0 | same method; the sink's contract is exercised through `ResolveArchiveRootOrEmpty`, which asserts the sink receives exactly `RootUnavailableDiagnostic` |
| 711 | `);` | yes | 0 | same method |
| 712 | `if (!EfcSelectionGuard.IsValidFilingSelection(selectedFolder, archiveRoot))` | yes | 0 | same method; the predicate itself is at 100% line and branch |
| 1044 | `internal bool IsValidSelection =>` | no (declaration line after CSharpier wrap) | n/a | not an instrumented sequence point |
| 1045 | `EfcSelectionGuard.IsValidCreationSelection(SelectedFolder);` | yes | 0 | WinForms UI glue: the property reads `SelectedFolder` off the viewer, which requires a live form; the predicate it delegates to is at 100% |

This mirrors the section-(d) convention ratified in the delivery cycle's
`coverage-delta.2026-08-26T19-50.md`: uncovered changed lines in `EfcFormController.cs` are the
thinnest possible WinForms wiring, and the logic they wire is unit-tested through the extracted
seam. The design decision recorded in the plan (resolver in the guard, not inline in
`ActionOkAsync`) exists precisely to keep this count at 7 rather than ~12 and to keep the catch
branch testable.

## (e) No changed line lost coverage

Baseline coverage of the lines these edits replace, read from
`coverage\coverage.cobertura.filtered.p0-t9r.xml`:

| Baseline line | Content at baseline | Baseline hits |
| ---: | --- | ---: |
| 705 | `var selectedFolder = SelectedFolder;` | 0 |
| 706 | `if (!EfcSelectionGuard.IsValidFilingSelection(selectedFolder))` | 0 |
| 707 | `{` | 0 |
| 1038 | `internal bool IsValidSelection => EfcSelectionGuard.IsValidFilingSelection(SelectedFolder);` | 0 |

Every line the change replaced was already uncovered at baseline, and every line the change
introduced at those sites is uncovered for the same structural reason. **No changed line lost
coverage.**

In the other direction, `EfcSelectionGuard.cs` went from 9/9 covered lines to 31/31 covered lines,
so the change adds 22 fully covered production lines and no uncovered production logic outside the
WinForms wiring enumerated in section (d). The whole-file figure for `EfcFormController.cs` is
81 / 717 covered post-change against 81 / 713 at baseline: the covered count is unchanged and the
denominator grew by the 4 newly instrumented wiring lines, which is exactly the section-(d) set.

All required numeric values were available; no value is a placeholder. Outcome: **PASS**.
