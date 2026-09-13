# Phase 0 — Per-File Coverage Baseline for ProgressPackage.cs

Timestamp: 2026-09-13T15-04
Task: [P0-T11]

Command: pwsh -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; [xml]$x = Get-Content -Raw "TestResults/coverage/coverage-baseline.cobertura.xml"; $c = @($x.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -like "*Threading\ProgressPackage.cs" }); "ClassElements: " + $c.Count; foreach ($n in $c) { $s = Get-CoberturaClassLineSummary -ClassNode $n; "LineRateAttribute: " + $n.GetAttribute("line-rate"); "TotalLines: " + $s.TotalLines; "CoveredLines: " + $s.CoveredLines }'
EXIT_CODE: 0

ClassElements: 1
TotalLines: 52
CoveredLines: 52
LineRateAttribute: 1

Output Summary: exactly one class element in the baseline Cobertura document resolves to the filename
`UtilitiesCS/Threading/ProgressPackage.cs`. Its de-duplicated line map holds 52 executable lines, all
52 of which are covered, and its own `line-rate` attribute is 1. The baseline ratio for AC12 is
therefore 52 divided by 52, which is 1.

## Derivation

The figures come from the helper function named Get-CoberturaClassLineSummary, dot-sourced from the
coverage helper script beside the runner. That helper de-duplicates by line number. De-duplication is
required because a Cobertura class element repeats every line under its method tree and again in its
class-level rollup, so a plain descendant-axis count double-counts.

Where more than one class element resolves to that filename, the per-class line maps are merged by
line number and a repeated line number is resolved by its maximum hits value. The merge rule is stated
even though the class-element count on this run is one, so that the derivation is identical to the one
P2-T9 will apply and the two figures are comparable.

This is the AC12 baseline and it is captured before any Phase 1 edit, so it describes the pre-change
file.

## Re-Run Note And Comparison With The Superseded Figures, Per D15

This artifact overwrites a superseded capture. The superseded capture recorded `ClassElements: 6` with
the same `TotalLines: 52`, `CoveredLines: 52` and `LineRateAttribute: 1`. The line figures and the
ratio are unchanged; only the class-element count moved, and the cause is identified rather than
assumed.

The cause is the state of the document each derivation read, not a change in the source file. The
runner throws on a non-zero coverage exit code before it reaches its post-processing step: the throw
sits at line 236 of `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and the post-processing announcement
at line 339 of the same file. The superseded derivation therefore read a raw, unprocessed Cobertura
document left behind by the run that failed under the defect D14 records, in which this source file
maps to six separate class elements, one per compiled class including the compiler-generated ones. The
re-derivation above read the post-processed document that the passing P0-T10 run produced, in which the
post-processing step has merged those elements into a single class element per filename.

The operative AC12 baseline is the post-processed figure recorded above, because P2-T9 will read a
post-processed document produced the same way. The ratio is unchanged at 1 either way, so the AC12
comparison is unaffected by the shift: a post-change ratio must be greater than or equal to 1, which
means every executable line of the file must remain covered.
