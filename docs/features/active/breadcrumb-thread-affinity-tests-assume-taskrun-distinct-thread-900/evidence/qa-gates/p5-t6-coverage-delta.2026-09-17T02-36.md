# P5-T6 — Coverage Delta

Timestamp: 2026-09-17T02-36

Command: `CMD-POSTPROCESS` with `STAGE` `final`: dot-source
`scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1`, post-process
`coverage\final-900.cobertura.xml` in place with
`ConvertTo-KoverageCoberturaXml -XmlContent <raw> -RepoRoot <repo-root>`, then read the six root
attributes, the two class counts and the line-coverage threshold.

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

BASELINE-COVERAGE: (P0-T10)

    line-rate=0.852658
    branch-rate=0.796851
    lines-covered=55948
    lines-valid=65616
    branches-covered=13564
    branches-valid=17022

FINAL-COVERAGE:

    line-rate=0.852566
    branch-rate=0.796675
    lines-covered=55942
    lines-valid=65616
    branches-covered=13561
    branches-valid=17022

Arithmetic difference, final minus baseline:

| Attribute | Baseline | Final | Difference |
| --- | --- | --- | --- |
| `line-rate` | 0.852658 | 0.852566 | -0.000092 |
| `branch-rate` | 0.796851 | 0.796675 | -0.000176 |
| `lines-covered` | 55948 | 55942 | -6 |
| `lines-valid` | 65616 | 65616 | 0 |
| `branches-covered` | 13564 | 13561 | -3 |
| `branches-valid` | 17022 | 17022 | 0 |

All twelve numeric values are recorded.

ItemViewerBreadcrumbClassCount=0

ThreadAffinityTestsClassCount=0

FLOOR: MET

`Assert-CoberturaLineCoverageThreshold` did not throw against the post-processed document, so the
repository line rate remains above its 80 percent floor.

## COMPARABILITY: A

The two `lines-valid` values are both 65616, so their absolute difference is 0, which is at most
1 percent of the baseline `lines-valid` (656.16). The two rates were therefore computed over the
same merged denominator and are directly comparable.

Under branch A the gate is that the final `line-rate` is not more than 0.005 below the baseline
`line-rate`. The final rate is below the baseline by 0.000092, which is well within 0.005, so the
gate holds. The branch rate moved by 0.000176 in the same direction and is recorded for
completeness.

The residual movement of 6 covered lines and 3 covered branches out of 65616 and 17022 is
run-to-run variation in a nine-assembly, fully parallel instrumented run, in which the exact set of
lines executed by timing-dependent and concurrency-related tests is not bit-identical between runs.
It is not attributable to this change, which alters no production line: the denominator is
identical, so nothing entered or left the measured code base.

## CHANGED-CODE COVERAGE: NOT MEASURABLE

Both class counts are 0, which selects this branch. The rationale, from D-7:

- No production line changed. The whole change is two test method bodies and one new private static
  test helper in `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`.
- The test assembly is outside the instrumented denominator. `ConvertTo-DerivedCoverageSettingsXml`
  appends the module exclusion `.*\.Test\.dll$` to the effective coverage settings, so
  `QuickFiler.Test.dll` is never instrumented and
  `ThreadAffinityTestsClassCount=0` follows by construction.
- The production type the tests exercise is excluded from coverage in source. `ItemViewer` carries
  `[ExcludeFromCodeCoverage]` at `QuickFiler/Viewers/ItemViewer.cs:20`, and
  `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` is a partial of that same class, so
  `ItemViewerBreadcrumbClassCount=0` follows as well.

There is therefore no changed line in the coverage denominator, and a changed-line coverage
percentage has no source. This is a structural property of the change, not an absence of
measurement.

Substitute behavioural evidence, named rather than implied:

- P4-T2: the two rewritten tests pass, measured, under the unchanged CLI runsettings.
- P4-T3: all seven tests in the class pass.
- P3-T1: with the boundary guard disabled, both rewritten tests fail on the operation-name message
  assertion, so the boundary assertions are exercised.
- P3-T3: with the delegate run inline, both rewritten tests fail on the distinct-thread
  precondition, so the precondition is exercised.
- P5-T5: all seven pass inside the full 7288-test repository-wide run.

`INSTRUMENTATION SCOPE CHANGED` was not reached; the observed pair matches the pair P0-T10 recorded.

## Acceptance

All three conditions hold: all twelve numeric values are recorded; there is exactly one
`COMPARABILITY:` line and, under branch `A`, the rate gate holds; there is exactly one
`CHANGED-CODE COVERAGE:` line.

## Copy deferral

This task does not copy the Cobertura document to `artifacts/csharp/coverage.xml`. That copy happens
only after the final clean iteration of the P5-T1 through P5-T7 loop, in P5-T8, because
`.csharpierignore` does not cover that path and a loop restart from P5-T1 would otherwise run
`csharpier format .` over the generated document.
