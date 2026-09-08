# [P0-T14] Baseline per-file coverage of `UtilitiesCS/Threading/UiThread.cs`

Timestamp: 2026-09-08T00-45

Command: the pinned per-file lookup applied to `coverage\809-p0-baseline.cobertura.xml`, the document [P0-T13] wrote. The lookup selects `<class>` elements whose `filename` attribute ends with the backslash suffix `UtilitiesCS\Threading\UiThread.cs`, takes `.//line` under each, counts each line number once, and treats a line number as covered when any matching element for that file carries `hits` greater than zero. A line number that matches no element is not executable and is excluded from both numerator and denominator. The `filename` attribute in this document carries an absolute path with backslash separators; a forward-slash match returns zero rows.

EXIT_CODE: 0

BASELINE_UITHREAD_LINES_COVERED: 63
BASELINE_UITHREAD_LINES_VALID: 82
BASELINE_UITHREAD_LINE_PCT: 76.83
BASELINE_UITHREAD_UNCOVERED_LINES: 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120

Output Summary: 63 of 82 executable line numbers covered, 76.83%, with 19 uncovered line numbers.

## Relation to the figure quoted in `spec.md`

`spec.md` carries a figure of **76.83% line and 65.00% branch** quoted from the #782 records (`spec.md:367`, restating `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md:74-83`). **This task's own measured figure is the baseline of record for [P6-T1].** The two are not required to agree, because the #782 figure was produced by an unrecorded selection.

As measured, they do agree on the line percentage and on the uncovered-line set: this run reproduces 76.83% and the identical 19-line list `28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120`. The agreement is recorded as an observation, not as a requirement.

## Exclusion check

UITHREAD_COVERAGE_EXCLUSION_APPLIES: false

Neither `UiThread` nor any nested type carries `[ExcludeFromCodeCoverage]`: a search of `UtilitiesCS/Threading/UiThread.cs` for that attribute returned zero matches. Repository-root `coverage.config` excludes only third-party module paths (`Deedle`, `FSharp`, `Castle.Core`, `FluentAssertions`, `Moq`, `Microsoft.Testing`, `MSTest`), so no exclusion applies to this file. The derived settings file used for the run adds one further exclude, `.*\.Test\.dll$`, which excludes test assemblies rather than any production file.
