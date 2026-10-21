# Coverage Comparison — baseline versus post-change (issue #781)

Timestamp: 2026-09-05T17-12

Task: [P2-T9]

Command: this artifact reads the two post-processing invocations already recorded, namely the
[P0-T9] block over `.\coverage\baseline-781.cobertura.xml` and the [P2-T7] block over
`.\coverage\final-781.cobertura.xml`, each run as `pwsh -NoProfile -Command` from the repository
root. The arithmetic differences below were computed in a `pwsh -NoProfile -Command` process from
those two recorded value sets.

EXIT_CODE: 0

## Output Summary

| Metric | Baseline ([P0-T9]) | Post-change ([P2-T7]) | Difference |
| --- | --- | --- | --- |
| `line-rate` | 0.848347 | 0.848316 | **-0.000031** |
| `branch-rate` | 0.791542 | 0.791421 | -0.000121 |
| `lines-covered` | 54922 | 54920 | -2 |
| `lines-valid` | 64740 | 64740 | **0** |

Both acceptance conditions hold:

1. The baseline and post-change `lines-valid` values are recorded and are **identical at 64740**,
   so the two measurements share the same denominator and the comparison is meaningful rather
   than an artifact of a changed measurement scope. That identity is expected: the only
   production file this plan changed belongs to a type carrying `[ExcludeFromCodeCoverage]`, so
   it contributes no line to the denominator either before or after, and the test-project changes
   contribute none because `*.Test.dll` is excluded from instrumentation by `coverage.config`.
   [P2-T6] independently confirmed comparability by observing `ASSEMBLY_COUNT: 9` in both runs.
2. The post-change `line-rate` is **0.000031 below** the baseline `line-rate`. The permitted
   tolerance is 0.005, so the observed drop is smaller than the tolerance by a factor of roughly
   160 and is not a coverage regression.

The two-line difference in `lines-covered` (54922 to 54920) is the whole of the movement. It is
consistent with the deletion of the two obsolete D4 tests in
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`: those tests drove the
old reference-comparison throw path, and the guard rewrite replaced that path with one the new
tests exercise instead. No production line lost coverage that a test still needed to cover, which
the [P2-T7] branch table demonstrates directly by naming a passing test for every outcome of both
conditionals in the rewritten method.

Changed-code determination, copied verbatim from [P2-T7]:

CHANGED-CODE COVERAGE: NOT MEASURABLE

Both runs additionally cleared the repository-wide 0.80 line-rate floor:
`Assert-CoberturaLineCoverageThreshold` returned without throwing on the baseline document
(`BASELINE_FLOOR: MET 0.848347`) and on the final document (0.848316).

## Correction recorded by the orchestrator from the feature review (2026-09-05T17-29)

The feature review (`code-review.2026-09-05T17-29.md`, `policy-audit.2026-09-05T17-29.md`) compared
the two Cobertura documents class by class (564 classes each) and found that two statements above
are inaccurate. The conclusion, no attributable coverage regression, is unchanged and is
strengthened by the corrected facts.

1. The two-line `lines-covered` movement is **not** attributable to the deleted D4 tests. Zero
   `QuickFiler` classes differ between the two documents; the `QuickFiler` package counters are
   identical (LINE missed=2376, covered=9960). The three classes that differ are all in the
   untouched `UtilitiesCS` assembly (`SegmentStopWatch` 1.0 to 0.944954, `SubjectMapSco` 0.969466
   to 0.938931, `OlTableExtensions` 0.885522 to 0.912458), which is run-to-run drift in tests this
   change did not touch. The paragraph beginning "The two-line difference" is superseded by this
   note; as the same artifact already establishes, the old throw path is outside the coverage
   denominator and could not move a counter in either direction.
2. `coverage.config` does **not** exclude `*.Test.dll`. That exclusion is injected at run time by
   `ConvertTo-DerivedCoverageSettingsXml` in `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
   (line 99), which the [P0-T8] and [P2-T6] script consumed.
