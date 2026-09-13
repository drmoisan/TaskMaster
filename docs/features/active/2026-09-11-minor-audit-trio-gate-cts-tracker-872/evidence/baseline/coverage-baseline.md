# Phase 0 — Repository Coverage Baseline

Timestamp: 2026-09-13T15-03
Task: [P0-T10]

Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput TestResults/coverage/coverage-baseline.cobertura.xml
EXIT_CODE: 0

LineRate: 0.857099
LinesCovered: 56068
LinesValid: 65416
BranchRate: 0.798828
BranchesCovered: 13497
BranchesValid: 16896
TestsPassed: 7222

Output Summary: the coverage run completed with `Test Run Successful.`, `Total tests: 7222` and
`Passed: 7222` in 32.6713 seconds, then post-processed the Cobertura document and printed the
first-party headline `First-party coverage: lines 56068/65416 (85.71%), branches 13497/16896 (79.88%)`
and the terminal line `Done. Coverage artifact: <worktree>\TestResults\coverage\coverage-baseline.cobertura.xml`,
exiting 0. First-party line coverage is 85.71 percent and first-party branch coverage is 79.88
percent. Both clear the repository floors that CLAUDE.md governs, which are 80 percent for line
coverage and 75 percent for branch coverage.

## Reconciliation Of The Two Figure Sets

The root coverage element of the emitted document carries `line-rate` 0.857099, `lines-covered` 56068,
`lines-valid` 65416, `branch-rate` 0.798828, `branches-covered` 13497 and `branches-valid` 16896. Those
are the six numeric fields recorded above, read directly from the document rather than from the console
line. They reconcile with the printed first-party headline exactly: the covered and valid counts are
identical in both, and the printed percentages are the two rates rounded to two decimal places. The
reconciliation is what establishes that the committed figures describe the document P0-T11 will read.

## Transient Artifact Placement

The second span printed `TransientXml: True` and `FeatureFolderXml: 0`. The first asserts that the raw
Cobertura document exists at the git-ignored path `TestResults/coverage/coverage-baseline.cobertura.xml`;
the second asserts that no file of that name exists anywhere under this feature folder. The second span
is a filesystem name enumeration by the PowerShell `-Filter` wildcard rather than a text search, so no
regex engine is involved. Per D10 the raw document is transient tool output, is never committed, and
stays on disk for the remainder of the run because P0-T11 reads it. A separate check confirms that the
prohibited path artifacts/csharp/coverage.xml does not exist; it is named here in prose rather than in
path formatting so that no extractor reads this line as a write claim.

## Re-Run Note, Per D15, And The Previously Blocking Failure

This artifact overwrites a superseded capture. On the pre-merge tree this task was the point at which
the plan halted: the coverage runner passes an MSTest runsettings file internally, whose ClassLevel
parallelism at a worker count of zero produced three failures in the QuickFiler zero-batch email-queue
test class through a type initialization exception for the Deedle reflection type against
netstandard 2.1, as D14 records. The runner exposes no override for that switch.

That blocker is resolved on the post-merge tree. The re-run above discovered and passed 7222 tests
with zero failures under the runner's own wider population, which is the population D6 describes, and
exited 0. The fix for issue #877, merged as pull request #880, is the change between the two runs: it
moved the body of the UtilitiesCS test project's assembly initializer into a shared source file under
the repository-root TestSupport directory and referenced it from both test projects, so the QuickFiler
test assembly now resolves its dependencies without depending on the other assembly's initializer
having run first. The observed transcript shows the runner enabling ClassLevel parallelism at 24
workers per assembly, so the parallelism that previously triggered the failure was still in force and
the run still passed.

Per D7 the run was bounded at ten minutes. It terminated in well under that bound: the test phase took
32.6713 seconds and the whole invocation completed inside 90 seconds, so the non-termination hazard
that D7 records did not materialise on this tree. No halt was required.

## Population, Per D6

The coverage runner hard-codes its own LiveOutlook-only filter and offers no extension point, so its
population is wider than the pinned per-assembly filter used by P0-T8 and P0-T9 and its total of 7222
is not comparable with either of those totals. This run is used only for the per-file figure that
P0-T11 derives and for the repository-wide headline above. It is never the source of the AC11
per-assembly counts.

## Environment Note

Outlook was verified not running before this run. The build lock was held across the runner invocation
and released immediately afterwards.
