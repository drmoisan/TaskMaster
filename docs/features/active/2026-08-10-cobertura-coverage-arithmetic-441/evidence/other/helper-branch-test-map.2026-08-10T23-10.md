# `Get-CoberturaClassLineSummary` — Branch-to-Test Map (P4-T7)

Timestamp: 2026-08-10T23-10

This artifact is a **scenario-completeness map, not a coverage measurement.** It names, for every
branch of `Get-CoberturaClassLineSummary`, at least one `It` block that exercises it. The numeric
`>= 90%` new-code proof is carried by P4-T6 (39/40 = 97.50%); an enumeration of branches does not
satisfy a coverage threshold and is not offered as one.

Function location in the post-change file: `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
lines **161-259**.

| # | Branch | Source | Named `It` block(s) that exercise it | Test file location |
| --- | --- | --- | --- | --- |
| 1 | **New-key insert** — line number not yet in the map | `:207-216` (`if (-not $lineMap.Contains($lineNumber))`, true path) | `counts each source line once when methods repeat the class-level rollup` (F1); also every other fixture, since the first entry for any line number takes this path | `Describe 'ConvertTo-KoverageCoberturaXml'` |
| 2 | **Repeat-key** — line number already present | `:218` onward (false path of the same `if`) | `deduplicates a repeated line number by taking the maximum hits value` (F4); `counts each source line once when methods repeat the class-level rollup` (F1) | `Describe 'ConvertTo-KoverageCoberturaXml'` |
| 3 | **Repeat-key `max(hits)`** — hits comparison on a repeated key | `:219` (`if ($hits -gt $existing.Hits)`) | `deduplicates a repeated line number by taking the maximum hits value` (F4) — line 5 appears with `hits=1` in `.ctor ()`, `hits=0` in `.ctor (int)` and `hits=1` in the class-level rollup, and the block asserts the deduplicated result `lines-valid` = `'1'`, `lines-covered` = `'1'` | `Describe 'ConvertTo-KoverageCoberturaXml'` |
| 4 | **Repeat-key branch promotion** — `branch=True` if either entry is a branch | `:223-225` (`if ($isBranch) { $existing.Branch = $true }`) | `counts each branch line once when methods repeat the class-level rollup` (F2) — line 12 carries `branch="True" condition-coverage="50% (1/2)"` on **both** axes, so the promotion assignment executes on the repeat key; asserts `branches-valid` = `'2'`, `branches-covered` = `'1'` | `Describe 'ConvertTo-KoverageCoberturaXml'` |
| 5 | **Precedence — candidate `Total` greater** | `:227-234` (first disjunct true) | `retains the candidate condition-coverage when its total is greater` — class-level `100% (2/2)` versus method-level `50% (2/4)`; asserts `TotalBranches` = 4, `CoveredBranches` = 2 | `Describe 'Get-CoberturaClassLineSummary'` |
| 6 | **Precedence — `Total` equal and `Covered` greater** | `:227-234` (second disjunct true) | `retains the candidate condition-coverage when totals tie and its covered count is greater` — class-level `0% (0/2)` versus method-level `50% (1/2)`; asserts `TotalBranches` = 2, `CoveredBranches` = 1 | `Describe 'Get-CoberturaClassLineSummary'` |
| 7 | **Precedence — neither condition holds** | `:227-234` (both disjuncts false; existing retained) | `retains the existing condition-coverage when neither precedence condition holds` — class-level `50% (2/4)` versus method-level `100% (2/2)`; asserts the class-level 4 / 2 survives | `Describe 'Get-CoberturaClassLineSummary'` |
| 8 | **Empty class** — no `<lines>` and no `<methods>` | `:196-197` selecting nothing; the `foreach` body never runs | `returns zero totals for a class with neither a lines nor a methods element` — asserts all four totals are 0 and `Should -Not -Throw` | `Describe 'Get-CoberturaClassLineSummary'` |
| 9 | **Rollup absent, methods present** (related boundary) | `:196` selects nothing, `:197` selects the method lines | `retains method-level lines when the class-level rollup element is absent` (F5) — asserts `lines-valid` = `'2'`, `lines-covered` = `'1'` | `Describe 'ConvertTo-KoverageCoberturaXml'` |
| 10 | **Accumulation — covered-line count** | `:242-244` (`if ($entry.Hits -gt 0)`), both outcomes | F1 (line 11 has `hits=0`, lines 10 and 12 have `hits=1`) | `Describe 'ConvertTo-KoverageCoberturaXml'` |
| 11 | **Accumulation — branch totals** | `:246-249` (`if ($entry.Branch)`), both outcomes | F2 (line 12 is a branch; lines 10 and 11 are not) | `Describe 'ConvertTo-KoverageCoberturaXml'` |

**Every listed branch names at least one `It` block.**

## Recorded qualification (do not read this map as a coverage claim)

One statement inside branch 3 is not executed by any current test: **line 220,
`$existing.Hits = $hits`**, the assignment body of `if ($hits -gt $existing.Hits)`. The `if`
*condition* at line 219 is executed and evaluated; only the assignment body is not, because the
helper enumerates the class-level rollup first and in every fixture — and in both committed sample
documents — the class-level rollup already carries the maximum hits value for any repeated line
number, so a later candidate never exceeds it. F4 exercises the `max(hits)` rule in the direction
the real data produces and asserts the correct deduplicated outcome.

This is stated here rather than papered over. It is quantified in P4-T6 (39 of 40 new-code lines
covered = 97.50%, against a `>= 90%` floor) and does not trigger the P4-T6 remediation path, which
fires only below a floor.
