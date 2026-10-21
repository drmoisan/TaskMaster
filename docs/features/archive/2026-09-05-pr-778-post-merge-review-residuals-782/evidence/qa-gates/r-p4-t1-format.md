# [P4-T1] Final QC step 1 — CSharpier format

Timestamp: 2026-09-06T01-48

Command:

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"

$before = @(git status --porcelain --untracked-files=all)
$beforeStat = @(git diff --stat HEAD)

dotnet tool run csharpier format .

$after = @(git status --porcelain --untracked-files=all)
$afterStat = @(git diff --stat HEAD)
```

EXIT_CODE: 0

Output Summary: the formatter rewrote nothing. The printed line, verbatim:

```text
Formatted 1583 files in 2047ms.
```

PATH_SETS_IDENTICAL: True
DIFFSTAT_IDENTICAL: True
BEFORE_COUNT: 30
AFTER_COUNT: 30

## Why the printed numeral is recorded but not asserted against

`Formatted <N> files` is a **processed** count, not a **changed** count. CSharpier prints the same
numeral whether it rewrote every file or none, so the numeral alone cannot distinguish a clean run
from a repairing one, and neither can the exit code, which is 0 in both cases. The two tree
observations below are what distinguish them.

## Observation 1 — the porcelain path sets

The set of paths reported by `git status --porcelain --untracked-files=all` is byte-identical before
and after the format run. This is the observation that would detect a rewrite of a file that was
previously unmodified: such a file would appear in the after capture and not in the before one.

Before the format run, 30 paths:

```text
 M UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs
 M UtilitiesCS.Test/Threading/UiThread_Tests.cs
 M docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/baseline/p0-t7-coverage.md
 M docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/ac-status-summary.2026-09-05T23-15.md
 M docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/code-review.2026-09-05T23-00.md
 M docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p1-t10-assertion-token-gate.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p1-t3-analyzer-build.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p1-t4-assertion-tests.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p2-t4-spec-claim-gate.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p2-t8-spec-wildcard-gate.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/r-p1-t5-mutation-applied.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/r-p1-t6-mutation-build.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/r-p1-t7-fail-before.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/r-p1-t8-mutation-reverted.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/r-p1-t9-pass-after.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t1-instructions-read.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t10-tests-coverage.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t11-anchor.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t12-dotclaude-baseline.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t2-claim-inventory.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t3-assertion-sites.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t4-pre782-message.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t5-retained-cobertura-reaggregation.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t6-retained-document-provenance.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t7-csharpier-check.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t8-analyzer-build.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t9-nullable-build.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/remediation-plan.2026-09-06T00-15.md
```

After the format run, the same 30 paths in the same order and with the same status codes. The two
captures were compared programmatically and reported `PATH_SETS_IDENTICAL=True`.

## Observation 2 — `git diff --stat HEAD` before and after

The path-set comparison alone cannot see a rewrite of a file that was **already** modified, because
such a file appears in both captures. The anchored `git diff --stat HEAD` closes that gap: a rewrite
would change the file's insertion and deletion counts.

Before the format run:

```text
 .../Folder/WpfDispatcherYieldTests.cs              |   2 +-
 UtilitiesCS.Test/Threading/UiThread_Tests.cs       |   4 +-
 .../evidence/baseline/p0-t7-coverage.md            | 142 ++++++++++++++++++---
 .../other/ac-status-summary.2026-09-05T23-15.md    |   4 +-
 .../evidence/other/code-review.2026-09-05T23-00.md |  14 +-
 .../spec.md                                        |  43 ++++---
 6 files changed, 171 insertions(+), 38 deletions(-)
```

After the format run:

```text
 .../Folder/WpfDispatcherYieldTests.cs              |   2 +-
 UtilitiesCS.Test/Threading/UiThread_Tests.cs       |   4 +-
 .../evidence/baseline/p0-t7-coverage.md            | 142 ++++++++++++++++++---
 .../other/ac-status-summary.2026-09-05T23-15.md    |   4 +-
 .../evidence/other/code-review.2026-09-05T23-00.md |  14 +-
 .../spec.md                                        |  43 ++++---
 6 files changed, 171 insertions(+), 38 deletions(-)
```

The two are byte-identical and were compared programmatically, reporting
`DIFFSTAT_IDENTICAL=True`.

## Consequence for the loop

The format step neither failed nor changed a file, so the toolchain loop proceeds to [P4-T2] without
restarting.
