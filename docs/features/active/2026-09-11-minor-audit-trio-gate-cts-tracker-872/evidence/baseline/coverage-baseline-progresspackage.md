# Phase 0 — Per-File Coverage Baseline, UtilitiesCS/Threading/ProgressPackage.cs

Timestamp: 2026-09-13T05-13
Task: [P0-T11]

Command: pwsh -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; [xml]$x = Get-Content -Raw "TestResults/coverage/coverage-baseline.cobertura.xml"; $c = @($x.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -like "*Threading\ProgressPackage.cs" }); "ClassElements: " + $c.Count; foreach ($n in $c) { $s = Get-CoberturaClassLineSummary -ClassNode $n; "LineRateAttribute: " + $n.GetAttribute("line-rate"); "TotalLines: " + $s.TotalLines; "CoveredLines: " + $s.CoveredLines }'
EXIT_CODE: 0

ClassElements: 6
TotalLines: 52
CoveredLines: 52
LineRateAttribute: 1

The three field values immediately above are the merged, operative AC12 baseline. The merged line rate is
52 of 52, which is 1.0, or 100 percent. The per-class-element figures the command printed are recorded
below in full so the merge is auditable.

Output Summary: six Cobertura class elements resolve to this one source file. Their per-element line
summaries, in the order printed, are 27 of 27, 1 of 2, 5 of 5, 5 of 5, 8 of 8 and 7 of 7, with
`line-rate` attributes of 1, 0.5, 1, 1, 1 and 1 respectively. Merging them by line number across the six
elements yields 52 distinct line numbers, all 52 of which are covered. The AC12 baseline for this file is
therefore 52 covered of 52 total.

## The Six Class Elements

All six carry the same `filename` attribute, which resolves to
`UtilitiesCS/Threading/ProgressPackage.cs`. Their `name` attributes are:

| Class element name | TotalLines | CoveredLines | line-rate |
| --- | --- | --- | --- |
| UtilitiesCS.Threading.ProgressPackage | 27 | 27 | 1 |
| UtilitiesCS.Threading.ProgressPackage.<>c | 2 | 1 | 0.5 |
| UtilitiesCS.Threading.ProgressPackage.<CreateAsTupleAsync>d__3 | 5 | 5 | 1 |
| UtilitiesCS.Threading.ProgressPackage.<CreateAsTuplePaneAsync>d__4 | 5 | 5 | 1 |
| UtilitiesCS.Threading.ProgressPackage.<InitializeAsync>d__1 | 8 | 8 | 1 |
| UtilitiesCS.Threading.ProgressPackage.<InitializeAsync>d__2 | 7 | 7 | 1 |

Five of the six are compiler-generated types: one closure display class and four async state machines, two
of them for the two InitializeAsync overloads. They carry the file's lines because the compiler attributes
the rewritten bodies back to the original source file.

## Derivation, Stated As The Task Requires

The per-element figures come from the helper function named Get-CoberturaClassLineSummary, dot-sourced from
the repository's coverage helper script. That helper de-duplicates by line number. De-duplication is
required rather than optional: a Cobertura class element repeats every line under its method tree and again
in its class-level rollup, so a plain descendant-axis count double-counts every line.

More than one class element resolves to this filename, so the merge rule applies. The per-element line maps
were merged by line number, and a line number appearing in more than one element was resolved to the
maximum hits value across the elements carrying it. The merge is stated here explicitly, as the task
requires it to be stated even when the element count is one.

The merge is not a formality on this file. The naive sum of the six per-element figures is 54 total and 53
covered; the merged figures are 52 and 52. Two mechanisms account for the difference:

- Line-number overlap across elements reduces the total from 54 to 52.
- Exactly one line number, line 29, carries a zero-hits row. That row belongs to the compiler-generated
  closure display class, and the same line number carries a hits value of 1 in the primary class element.
  The maximum-hits rule therefore resolves line 29 to covered, which raises the merged covered count from
  51 to 52 and makes the merged rate 100 percent. Line 29 is the only line in the file with any zero-hits
  row; a probe over all rows reported the zero-hit set as line 29 only, appearing twice because the closure
  class repeats it under its method tree and its class rollup.

## Baseline Timing

This figure was captured before any Phase 1 edit, so it describes the pre-change file. Phase 1 has not
started and no Write Set path has been modified.

## Source Document Caveat

The Cobertura document read here is the raw dotnet-coverage output rather than the post-processed form,
because P0-T10 threw before its post-processing step. The derivation is unaffected in substance: the
document carries the same class, method and line elements, and the filename attributes use backslash
separators, so the task's `-like "*Threading\ProgressPackage.cs"` filter matched and returned six elements.
The filename attributes in the raw document are absolute host paths and none is transcribed into this
artifact; the class elements are identified by their `name` attributes instead.

The comparison this baseline feeds, in Phase 2, must be taken from a document produced the same way, or the
comparison is not like for like. That is a consequence of the P0-T10 failure and is recorded here so the
Phase 2 comparison is not silently taken against a post-processed document.
