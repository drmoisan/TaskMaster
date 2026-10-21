# P5-T8 — Coverage Thresholds And The No-Regression Comparison

Timestamp: 2026-09-09T11-37
Task: [P5-T8]
Command: `pwsh -NoProfile -Command '[xml]$j = Get-Content -Raw -LiteralPath "docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t7-coverage-final.jacoco.xml"; $sf = @(); foreach ($n in @($j.SelectNodes("//sourcefile"))) { if ($n.name -like "*Invoke-MSTestWithCoverage.FirstParty.ps1") { $sf += $n } }; if ($sf.Count -ne 1) { throw "FirstParty sourcefile node not found in JaCoCo output" }; $ln = $null; foreach ($c in @($sf[0].SelectNodes("./counter"))) { if ($c.type -eq "LINE") { $ln = $c } }; if ($null -eq $ln) { throw "FirstParty LINE counter not found" }; $cov = [int]$ln.covered; $mis = [int]$ln.missed; if (($cov + $mis) -eq 0) { throw "FirstParty LINE denominator is zero" }; Write-Output ("FIRSTPARTY_LINE_PCT=" + ((100 * $cov / ($cov + $mis)).ToString("0.00")) + " COVERED=" + $cov + " MISSED=" + $mis)'`
EXIT_CODE: 0

```
FIRSTPARTY_LINE_PCT=96.97 COVERED=32 MISSED=1
```

The command carries four explicit guards — a single-node requirement, a LINE-counter requirement, a
zero-denominator requirement, and a node-not-found requirement — each throwing a distinct message,
so a silent zero cannot be mistaken for a measurement.

## Acceptance, part one — the new-module floor

| Item | Value |
| --- | --- |
| `FIRSTPARTY_LINE_PCT` | **96.97** |
| Floor set by `CLAUDE.md` section UT2 for newly added modules and functions | 90.00 |
| Verdict | **PASS**, by 6.97 points |

## Acceptance, part two — no regression on `Invoke-MSTestWithCoverage.Helpers.ps1`

| Measurement | LINE covered |
| --- | --- |
| Baseline, `evidence/baseline/p0-t8-coverage-baseline.md` | 191 |
| Post-change | **192** |
| Verdict | **PASS**: greater than or equal to the baseline, and in fact one higher |

The covered count rose by one because the added dot-source line at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` line 5 is executed whenever the file is
dot-sourced. The missed count is unchanged at 19.

## Acceptance, part three — bounded regression on `Invoke-MSTestWithCoverage.ps1`

| Measurement | LINE missed |
| --- | --- |
| Baseline, `evidence/baseline/p0-t8-coverage-baseline.md` | 10 |
| Permitted ceiling, baseline plus 1 | 11 |
| Post-change | **10** |
| Verdict | **PASS**: no more than the ceiling, and in fact unchanged |

## Baseline, post-change and new-code values

| Scope | Baseline | Post-change |
| --- | --- | --- |
| `scripts/vscode` folder line coverage | 78.56% (535/681) | **79.47%** (569/716) |
| `Invoke-MSTestWithCoverage.FirstParty.ps1` (new code) | not present | **96.97%** (32/33) |
| `Invoke-MSTestWithCoverage.Helpers.ps1` | 190.95% is not applicable; covered 191, missed 19 | covered 192, missed 19 |
| `Invoke-MSTestWithCoverage.ps1` | covered 89, missed 10 | covered 90, missed 10 |

Folder line coverage improved by 0.91 points. No file's coverage regressed.

## Measured finding — the single uncovered line is not the one plan decision D3 predicted

Plan decision D3 predicted that the one line added to `Invoke-MSTestWithCoverageMain` would be
uncovered by construction, on the premise that no unit test can reach the post-processing block past
the `-NoExecute` early return, and the AC10 gate was written to permit the entry point's missed count
to rise by 1 on that basis.

**The measurement refutes the premise and the allowance was not needed.** The entry point's missed
count did not rise at all: it is 10 before and 10 after, while its covered count rose from 89 to 90.
The added wiring line **is** covered, because six pre-existing tests call
`Invoke-MSTestWithCoverageMain` without `-NoExecute` and do reach lines 339 to 346. Those are the
same six tests whose failure on the first pass of P5-T6 exposed the false premise; see
`evidence/qa-gates/p5-t6-test.md`.

The one uncovered line in the new production file is line 55,
`[string[]]$ProjectNames = (Get-KoverageProjectAllowlist)`, the parameter default on
`Get-CoberturaFirstPartyCoverageSummary`. It is uncovered because every test binds `-ProjectNames`
explicitly, and plan decision D9 requires exactly that: the default derives its names from the
tracked project files, so it can never contain the fixture package name `Ns`, and invoking it would
additionally perform a recursive repository scan that `.claude/rules/general-unit-test.md` forbids in
a unit test. Test T-C instead asserts the default by reading its extent text from the function AST,
which pins the contract without executing the scan. Trading one uncovered parameter-default line for
a deterministic, filesystem-free unit test is the correct trade, and the file still reaches 96.97%.

## No threshold was lowered, weakened or deleted

Nothing in this delivery changes any coverage threshold, analyzer severity or policy requirement, and
no production file was added to a coverage exclusion list. The threshold script is byte-identical to
its pre-change state, verified by SHA-256 in `evidence/qa-gates/p4-t6-threshold-unchanged-gate.md`.
The standing 80-versus-85 line-floor divergence in the repository's own policy documents is recorded
as a finding in that same artifact, per `spec.md` Non-Goal 4 and epic Non-Goal 5, and was not
actioned.

Output Summary: `FIRSTPARTY_LINE_PCT` is 96.97, clearing the 90.00 floor for newly added modules. The
`Invoke-MSTestWithCoverage.Helpers.ps1` covered LINE value rose from 191 to 192, so it is greater
than or equal to its baseline. The `Invoke-MSTestWithCoverage.ps1` missed LINE value is 10, unchanged
from its baseline of 10 and therefore within the baseline-plus-1 ceiling. Folder line coverage rose
from 78.56% to 79.47%. All three acceptance parts pass. The single uncovered line in the new file is
line 55, the `ProjectNames` parameter default, recorded above as a measured finding with its reason.
