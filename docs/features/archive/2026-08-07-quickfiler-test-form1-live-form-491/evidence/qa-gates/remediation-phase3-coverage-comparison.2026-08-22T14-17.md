Timestamp: 2026-08-22T14-17

Command: (comparison of P3-T2 figures against the primary plan's cited baseline; no new capture)

EXIT_CODE: N/A (comparison/analysis task, not a command-step gate)

Output Summary:

| Metric | Baseline (primary plan) | Post-change (this cycle) | Delta |
|---|---|---|---|
| lines-covered | 53402 | 53392 | -10 |
| lines-valid | 62401 | 62401 | 0 |
| line-rate (%) | 85.5788% | 85.5627% | -0.0161 percentage points |

Baseline source: `evidence/baseline/phase0-coverage-baseline.2026-08-22T13-13.md`
(85.5788%, lines-covered=53402, lines-valid=62401), cited directly and not re-captured.
Post-change source: `evidence/qa-gates/remediation-phase3-coverage-postchange.2026-08-22T14-17.md`
(85.5627%, lines-covered=53392, lines-valid=62401).

Both figures were produced by `scripts\vscode\Invoke-MSTestWithCoverage.ps1` and are therefore both
Koverage-filtered first-party figures. No raw `dotnet-coverage collect` figure was substituted on
either side of this comparison.

ACCEPTANCE CONDITION NOT MET: the post-change percentage (85.5627%) is LOWER than the baseline
percentage (85.5788%). This task's stated acceptance ("the post-change percentage is greater than
or equal to the baseline percentage") is not satisfied by this evidence. This checkbox is left
unchecked.

Root-cause analysis (recorded in full in `remediation-phase3-coverage-capture.2026-08-22T14-17.md`,
summarized here):

1. The change this plan makes (deleting the dead `QfcFormViewerDerived` nested class from
   `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`) has a measured effect of exactly ZERO on
   this figure. No `QuickFiler.Test` package or class appears in any per-class diff between the
   baseline capture and any of the four capture attempts made during this cycle (one canonical, three
   diagnostic). This is the expected result under the harness's documented `.Test`-suffix exclusion
   from both instrumentation and the Koverage allowlist (spec.md, "Coverage impact and targets for
   changed lines/modules").
2. The entire 10-line shortfall traces to two unrelated production files:
   `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs` (-4 lines) and
   `UtilitiesCS\HelperClasses\SegmentStopWatch.cs` (-6 lines). Neither file is touched by this
   change.
3. Three additional diagnostic capture attempts (not used as this task's official artifact) confirm
   this is a reproducible, environment-driven coverage-measurement condition rather than a one-off
   fluke: `SegmentStopWatch.cs` showed the identical -6-line shortfall in all three valid attempts
   (the canonical capture plus two of the three diagnostic re-runs), while a second, different
   unrelated file contributed the remaining shortfall each time (`OlTableExtensions.Etl.cs`,
   `EfcHomeController.cs`+`PropertyStore.cs`, and `SubjectMapSco.Orchestration.cs` in turn). A third
   diagnostic attempt aborted entirely on an unrelated flaky test failure before Koverage
   post-processing could run, independently corroborating elevated test/coverage flakiness in the
   current session. This pattern is consistent with documented run-to-run `dotnet-coverage`
   measurement noise in this repository's parallel-test-execution harness, distinct from the session
   in which the baseline was captured roughly 65 minutes earlier.
4. No threshold was lowered, no `coverage.config` entry was edited, and no production file was
   excluded from measurement in the course of this investigation. All four capture attempts used the
   plan's specified command unmodified.

Disposition: this task and the downstream AC10 check-off (P4-T4) are left unmet by this evidence.
The finding is carried into the plan's completion report for escalation, per the atomic-executor's
non-blocking-mid-execution protocol; it is not treated as license to lower the bar, exclude a file,
or substitute a more favorable reading.
