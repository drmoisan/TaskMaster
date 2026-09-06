# [P0-T5] Re-aggregation of the retained baseline Cobertura document

Timestamp: 2026-09-06T01-30

Command:

```powershell
$CoberturaPath = 'coverage\782-p0-baseline.cobertura.xml'
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath $CoberturaPath).Path)
$firstParty = @('Tags','ToDoModel','TaskVisualization','UtilitiesCS','QuickFiler','TaskTree','TaskMaster','SVGControl','VBFunctions')
$lc = 0; $lv = 0; $bc = 0; $bv = 0
foreach ($pkg in $doc.SelectNodes('/coverage/packages/package')) {
    if ($firstParty -notcontains $pkg.GetAttribute('name')) { continue }
    foreach ($ln in $pkg.SelectNodes('.//line')) {
        $lv++
        $h = $ln.GetAttribute('hits')
        if ($h -and [int]$h -gt 0) { $lc++ }
        $cc = $ln.GetAttribute('condition-coverage')
        if ($cc -and $cc -match '\((\d+)/(\d+)\)') { $bc += [int]$Matches[1]; $bv += [int]$Matches[2] }
    }
}
"LINES_COVERED=$lc LINES_VALID=$lv BRANCHES_COVERED=$bc BRANCHES_VALID=$bv"
```

This is the pinned all-descendant `.//line` aggregation from the remediation plan's "The pinned
coverage aggregation" section, which is the same selection `evidence/baseline/p0-t7-coverage.md`
pins as load-bearing under SD22. `GetAttribute` is used rather than property access so a `<line>`
element lacking an attribute yields an empty string instead of throwing under `Set-StrictMode`.

EXIT_CODE: 0

Output Summary: the printed line, verbatim:

```text
LINES_COVERED=112359 LINES_VALID=132967 BRANCHES_COVERED=26496 BRANCHES_VALID=33480
```

- `LINES_COVERED=112359` — the expected value.
- `BRANCHES_COVERED=26496` — the expected value.
- `LINES_VALID=132967` — identical to the denominator `evidence/baseline/p0-t7-coverage.md` records
  for both collections, which is the recorded evidence that one selection produced both.
- `BRANCHES_VALID=33480` — identical to the branch denominator that artifact records.

## What this establishes for Phase 3

The two covered counters a reader obtains from the retained document,
`coverage/782-p0-baseline.cobertura.xml`, are 112359 and 26496. Those are exactly the two figures
`evidence/baseline/p0-t7-coverage.md` labels superseded and declares invalid as a baseline side,
while the same artifact's recorded `--output` argument names that document as the input for the
authoritative figures 112355 and 26500.

The amendment [P3-T3] writes records both collections with their own inputs and figures, and the
retained-document row records 112359 and 26496 on the strength of this measurement rather than on
the strength of the earlier artifact's own labelling.

`coverage/` is git-ignored, so `coverage/782-p0-baseline.cobertura.xml` is a local artifact and is
not committed evidence. It is cited here as the input a reader would have to obtain locally to
reproduce these two counters.
