# Phase 2 — Post-Change Repository Coverage

Timestamp: 2026-09-13T15-38
Task: [P2-T7]

Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput TestResults/coverage/coverage-postchange.cobertura.xml
EXIT_CODE: 0

LineRate: 0.857169
LinesCovered: 56040
LinesValid: 65378
BranchRate: 0.798923
BranchesCovered: 13497
BranchesValid: 16894
TestsPassed: 7218

Output Summary: the coverage run completed with `Test Run Successful.`, `Total tests: 7218` and
`     Passed: 7218` in 46.3231 seconds, announced `Post-processing coverage XML for Koverage
compatibility...`, then printed the first-party headline
`First-party coverage: lines 56040/65378 (85.72%), branches 13497/16894 (79.89%)` and the terminal line
`Done. Coverage artifact: <worktree>\TestResults\coverage\coverage-postchange.cobertura.xml`, exiting 0.
First-party line coverage is 85.72 percent and first-party branch coverage is 79.89 percent. Both clear
the repository floors that CLAUDE.md governs, which are 80 percent for line coverage and 75 percent for
branch coverage.

## Reconciliation Of The Two Figure Sets

The root coverage element of the emitted document carries `line-rate` 0.857169, `lines-covered` 56040,
`lines-valid` 65378, `branch-rate` 0.798923, `branches-covered` 13497 and `branches-valid` 16894. Those
are the six numeric fields recorded above, read from the document rather than from the console line.
They reconcile with the printed first-party headline exactly: the covered and valid counts are
identical in both, and the printed percentages are the two rates rounded to two decimal places. The
reconciliation establishes that the committed figures describe the same document P2-T9 reads.

## Movement Against The P0-T10 Baseline

| Figure | Baseline (P0-T10) | Post-change (P2-T7) | Movement |
|---|---|---|---|
| Line rate | 0.857099 (85.71%) | 0.857169 (85.72%) | +0.00007 |
| Lines covered | 56068 | 56040 | -28 |
| Lines valid | 65416 | 65378 | -38 |
| Branch rate | 0.798828 (79.88%) | 0.798923 (79.89%) | +0.000095 |
| Branches covered | 13497 | 13497 | 0 |
| Branches valid | 16896 | 16894 | -2 |
| Tests passed | 7222 | 7218 | -4 |

The repository-wide line rate rises. Deleting the dormant tracker removes a production file from the
denominator: the valid-line count falls by 38 and the covered-line count by 28, so the ratio of the two
improves. That upward movement is expected and is the stated purpose of the dormant-code issue, #841.
No coverage regression occurred: both rates moved up, not down.

The tests-passed figure falls by four rather than by the minus-six-plus-two net of the AC11
per-assembly arithmetic. Per D6 that is not a discrepancy. This runner hard-codes its own
LiveOutlook-only filter and offers no extension point, so its population is wider than the pinned
per-assembly filter and includes tests that the AC11 runs exclude; its total is not comparable with
either per-assembly total and is never the source of the AC11 counts. Within this wider population the
change is the same one: nine tests removed with the dormant tracker's test class, three added by AC4
and two added by AC1 and AC2, which is minus four.

## Transient Artifact Placement

The second span printed `TransientXml: True` and `FeatureFolderXml: 0`. The first asserts that the raw
Cobertura document exists at the git-ignored path
`TestResults/coverage/coverage-postchange.cobertura.xml`; the second asserts that no file of that name
exists anywhere under this feature folder. The second span is a filesystem name enumeration by the
PowerShell `-Filter` wildcard rather than a text search, so no regex engine is involved. Per D10 the
raw document is transient tool output, is never committed, and stays on disk for the remainder of the
run because P2-T9 reads it. The same span confirmed that the prohibited path artifacts/csharp/coverage.xml
does not exist; it is named here in prose rather than in path formatting so that no extractor reads
this line as a write claim.

## Termination, Per D7

The run was bounded at ten minutes. It terminated well inside that bound: the test phase took 46.3231
seconds and the whole invocation completed in about 95 seconds. No halt was required and no narrower
run was substituted.

## Post-Processing Confirmation

The runner printed its post-processing announcement before emitting the headline, so the document P2-T9
reads is the post-processed one. That matters for the like-for-like comparison: the P0-T11 baseline was
derived from a post-processed document in which the progress package source resolves to exactly one
class element, and a raw document would resolve it to six.

Outlook was verified not running before this run. The build lock was held across the runner invocation
and released immediately afterwards.
