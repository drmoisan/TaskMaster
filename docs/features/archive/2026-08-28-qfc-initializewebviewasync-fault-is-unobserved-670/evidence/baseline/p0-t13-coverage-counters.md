# P0-T13 — Baseline coverage counters

Timestamp: 2026-09-01T19-50
Command: the inline PowerShell expression reproduced verbatim below, evaluated against `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/baseline/baseline.cobertura.xml`
EXIT_CODE: 0

## Expression, verbatim

    [xml]$c = Get-Content -LiteralPath 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/baseline/baseline.cobertura.xml'
    $rows = $c.SelectNodes('//class/lines/line') | ForEach-Object { [pscustomobject]@{ File = $_.ParentNode.ParentNode.GetAttribute('filename'); Num = [int]$_.GetAttribute('number'); Hits = [int]$_.GetAttribute('hits') } }
    $g = $rows | Group-Object File, Num
    $valid = $g.Count
    $covered = ($g | Where-Object { ($_.Group | Measure-Object -Property Hits -Maximum).Maximum -gt 0 }).Count
    '{0} covered / {1} valid = {2:N4}%' -f $covered, $valid, (100 * $covered / $valid)

Printed result:

    54983 covered / 64393 valid = 85.3866%

## Derived values

    BASELINE_LINES_COVERED = 54983
    BASELINE_LINES_VALID   = 64393
    BASELINE_LINE_PERCENT  = 85.3866

The expression groups by the `(filename, line number)` pair and takes the maximum `hits` within each group, which reproduces explicitly the filename merge the runner performs in `Merge-CoberturaClassesByFilename`. Reproducing the merge rather than relying on it is what makes the same expression correct against a raw `dotnet-coverage` document as well as against a post-processed one, and P4-T6 evaluates the identical expression against the post-change document so the two figures are derived the same way.

On this particular document the merge is a no-op: the raw `//class/lines/line` node count is 64393, equal to the grouped count of 64393, so no `(filename, line number)` pair appears more than once. That is the expected shape for a post-processed document, in which classes have already been merged by filename. The derived figures also agree exactly with the document's own root attributes (`lines-covered` 54983, `lines-valid` 64393, `line-rate` 0.853866), which is a consistency check on the extraction rather than an independent measurement.

## Absence of the new module in the baseline

An XPath query for `//class[contains(@filename,'QfcItemController.WebViewFaultBoundary.cs')]` returns **0** nodes. No `class` node in the baseline document has a `filename` containing `QfcItemController.WebViewFaultBoundary.cs`, which is correct: the file does not exist yet at Phase 0. This is recorded because it establishes that the `NEWFILE_LINE_PERCENT` figure P4-T7 derives is measuring a genuinely new denominator rather than a pre-existing one, and because a non-zero count here would mean the file already existed and the plan's premise was wrong.

## Coverage authority applied

No acceptance condition in this plan asserts a repository-wide percentage as pass or fail, because two repository authorities state different floors (80% in CLAUDE.md and `.claude/rules/csharp.md`; 85% line and 75% branch in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`) and #670 does not resolve that divergence. The two gates actually used are the unambiguous `>= 90%` new-module rule on the new file (P4-T7) and a no-regression comparison against these baseline counters (P4-T8).

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
