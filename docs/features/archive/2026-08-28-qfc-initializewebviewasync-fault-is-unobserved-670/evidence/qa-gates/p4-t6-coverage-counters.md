# P4-T6 — Post-change coverage counters

Timestamp: 2026-09-01T20-17
Command: the identical inline PowerShell expression P0-T13 used, evaluated against `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/postchange.cobertura.xml`
EXIT_CODE: 0

## Expression, verbatim

    [xml]$c = Get-Content -LiteralPath 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/postchange.cobertura.xml'
    $rows = $c.SelectNodes('//class/lines/line') | ForEach-Object { [pscustomobject]@{ File = $_.ParentNode.ParentNode.GetAttribute('filename'); Num = [int]$_.GetAttribute('number'); Hits = [int]$_.GetAttribute('hits') } }
    $g = $rows | Group-Object File, Num
    $valid = $g.Count
    $covered = ($g | Where-Object { ($_.Group | Measure-Object -Property Hits -Maximum).Maximum -gt 0 }).Count
    '{0} covered / {1} valid = {2:N4}%' -f $covered, $valid, (100 * $covered / $valid)

Printed result:

    54988 covered / 64406 valid = 85.3771%

## Derived values

    POSTCHANGE_LINES_COVERED = 54988
    POSTCHANGE_LINES_VALID   = 64406
    POSTCHANGE_LINE_PERCENT  = 85.3771

This is the same expression evaluated in P0-T13 against the baseline document, with only the input path changed. Deriving both sides the same way is what makes the P4-T8 comparison valid; a figure taken from the document's root aggregate on one side and from the `line` nodes on the other would not be comparable in general.

As on the baseline document, the filename merge is a no-op here: the raw `//class/lines/line` node count is 64406, equal to the grouped count, so no `(filename, line number)` pair appears more than once. That is the expected shape for a post-processed document. The derived figures also agree exactly with the document's own root attributes (`lines-covered` 54988, `lines-valid` 64406, `line-rate` 0.853771), which is a consistency check on the extraction rather than an independent measurement.

## The new module is present in the post-change document

An XPath query for `//class[contains(@filename,'QfcItemController.WebViewFaultBoundary.cs')]` returns **1** node.

The same query returned **0** against the baseline document in P0-T13. That transition from 0 to 1 is the evidence that the new file was genuinely instrumented by this run rather than silently omitted from measurement. It matters because the P4-T7 per-file threshold would be meaningless against an absent denominator, and a file that is never instrumented produces no `class` node at all rather than producing a zero-coverage one.

## Movement against the baseline

    BASELINE_LINES_VALID     = 64393
    POSTCHANGE_LINES_VALID   = 64406      (+13)

    BASELINE_LINES_COVERED   = 54983
    POSTCHANGE_LINES_COVERED = 54988      (+5)

The denominator grew by 13 lines, which is the instrumented line count the new production file contributes. The numerator grew by 5. The arithmetic consequence is examined in P4-T8; it is stated here only as the raw movement, without disposition.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
