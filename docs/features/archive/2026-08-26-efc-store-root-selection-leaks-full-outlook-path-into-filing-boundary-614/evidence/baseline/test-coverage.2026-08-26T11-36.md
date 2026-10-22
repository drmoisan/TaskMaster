# [P0-T9] Baseline Test-and-Coverage Run

Timestamp: 2026-08-26T11-36
Task: [P0-T9]
Issue: #614

Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`
Working directory: `<repo-root>`
EXIT_CODE: 0

`ExpectedExitCode` is deliberately OMITTED. The plan's P0-T9 acceptance says the field is declared
only when the #594 flake makes the run exit 1, and instructs that it be omitted if the run is fully
green. This run was fully green, so the default expectation of 0 is correct and matches the
observed exit code.

## Test results

| Metric | Reference (pre-plan baseline) | Observed this run |
| --- | --- | --- |
| Test assemblies discovered | 9 | 9 |
| Total tests | 6482 | 6482 |
| Passed | 6481 | 6482 |
| Failed | 1 (#594 Console.Out race) | 0 |
| vstest / runner exit code | 1 | 0 |
| Total time | — | 56.8740 s |

Runner verdict line: `Test Run Successful.`

vstest.console resolved by the runner via vswhere:
`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

## Divergence from the plan's reference values (recorded per Global rule 9)

The plan's reference baseline was 6482 total / 6481 passed / 1 failed / exit 1, the single failure
being the pre-existing `DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole` Console.Out
race tracked by OPEN issue #594. On this run that flake did NOT reproduce: the suite was fully
green at 6482/6482/0 with exit 0.

This is the more favourable of the two outcomes the plan anticipated and is explicitly handled by
the P0-T9 task text ("If the run is instead fully green, the runner performs the filtered rewrite in
place"). The effective baseline for the P9-T4 no-new-failure gate is therefore recorded as
**6482 total / 6482 passed / 0 failed**. Per Global rule 7, if #594 (or #592 / #586 / #584)
surfaces at P9-T4 it is recorded by issue number as a pre-existing flake and is not treated as a
new failure.

## Coverage

Because the run exited 0, `Invoke-DotnetCoverageCollection` did not throw and the runner performed
its in-place `ConvertTo-KoverageCoberturaXml` rewrite. The raw pre-post-processing Cobertura was
therefore **consumed in place**, and the unfiltered figure is **unavailable for this run**. Per the
plan that figure is informational only and gates nothing (its reference value was 74.4666%).

The on-disk `coverage\coverage.cobertura.xml` is the allowlist-filtered artifact, whose top-level
`line-rate` / `lines-covered` / `lines-valid` are recomputed by `ConvertTo-KoverageCoberturaXml`
over the allowlisted packages only. It was copied to
`coverage\coverage.cobertura.filtered.p0-t9.xml` (gitignored `coverage/` tree; never under
`evidence/`). That file is the P9-T5 baseline input.

| Figure | Reference | Observed this run |
| --- | --- | --- |
| Filtered (allowlisted first-party) line coverage | 84.8099% (53627 / 63232) | **84.7797% (53769 / 63422)** |
| Filtered branch coverage | — | 78.6938% (12676 / 16108) |
| Unfiltered line coverage | 74.4666% | unavailable (raw consumed in place) |

Allowlist packages present in the filtered artifact (9, matching
`Get-KoverageProjectAllowlist`): QuickFiler, SVGControl, Tags, TaskMaster, TaskTree,
TaskVisualization, ToDoModel, UtilitiesCS, VBFunctions.

### Filtered-figure divergence (recorded per Global rule 9)

Observed 84.7797% versus the reference 84.8099%, a difference of 0.0302 percentage points. Both the
numerator (53769 vs 53627) and the denominator (53627→63422 vs 63232) moved, which is the known
dotnet-coverage run-to-run denominator nondeterminism recorded in the plan's P9-T5 remediation note.
No source file changed between the reference measurement and this run (working tree clean at
`f602410674a20f8b5aa988847ba6d055b008ca11`).

This is recorded now because the P9-T5 gate (b) states a fixed floor of `>= 84.80` for the
post-change filtered figure. The measured merge-base baseline on this machine is 84.7797%, i.e.
marginally below that floor before any change is made. P9-T5 will therefore report BOTH comparisons
explicitly: post-change versus the plan's fixed 84.80 floor, and post-change versus this measured
84.7797% baseline (the no-regression comparison AC23 actually requires).

Output Summary: Baseline suite is fully green — 9 assemblies, 6482 total, 6482 passed, 0 failed,
exit 0, 56.87 s. Filtered first-party line coverage 84.7797% (53769/63422); filtered branch
coverage 78.6938%. Unfiltered figure unavailable because the green run let the runner post-process
in place. Effective no-new-failure baseline for P9-T4 recorded as 6482/6482/0. Two divergences from
the plan's reference values are recorded above: the #594 flake did not reproduce, and the filtered
figure is 0.0302 points below the 84.8099% reference under known dotnet-coverage nondeterminism.
