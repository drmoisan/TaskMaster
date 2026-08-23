# Coverage Delta — Baseline (P0-T16) vs Post-Change (P4-T3) (P4-T6)

Timestamp: 2026-08-10T23-10

Target file: `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
$coverageXmlPath = Join-Path $root 'docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\evidence\qa-gates\pester-coverage-final.2026-08-10T23-10.xml'
$srcPath = 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1'
$src     = Get-Content -LiteralPath (Join-Path $root $srcPath)
$first   = ($src | Select-String -SimpleMatch 'function Get-CoberturaClassLineSummary').LineNumber
if (-not $first) { throw 'Get-CoberturaClassLineSummary not found; cannot compute new-code coverage.' }
$last    = 259
$jc      = [xml](Get-Content -LiteralPath $coverageXmlPath -Raw)
$lines   = @($jc.report.package.sourcefile.line | Where-Object { [int]$_.nr -ge $first -and [int]$_.nr -le $last })
if ($lines.Count -eq 0) { throw 'No JaCoCo line records in the helper range; the range or the report path is wrong.' }
$cov = @($lines | Where-Object { [int]$_.ci -gt 0 }).Count
'new-code: {0}/{1} = {2:P2}  (first={3} last={4})' -f $cov, $lines.Count, ($cov / $lines.Count), $first, $last
```

EXIT_CODE: 0

Output Summary:

```
new-code: 39/40 = 97.50%  (first=161 last=259)
covered=39 uncovered=1
uncovered line numbers: 220
```

---

## 1. Line coverage — whole file

| Counter | Baseline (P0-T16) | Post-change (P4-T3) | Delta |
| --- | --- | --- | --- |
| LINE missed | 19 | 19 | 0 |
| LINE covered | 146 | 183 | +37 |
| LINE total | 165 | 202 | +37 |
| **LINE percentage** | **88.48%** (146/165) | **90.59%** (183/202) | **+2.11 pp** |
| INSTRUCTION | 170/192 = 88.54% | 213/236 = 90.25% | +1.71 pp |
| METHOD | 7/7 = 100% | 8/8 = 100% | 0 |
| CLASS | 1/1 = 100% | 1/1 = 100% | 0 |

| Gate | Threshold | Post-change | Verdict |
| --- | --- | --- | --- |
| Line coverage floor (`.claude/rules/general-unit-test.md`, uniform T1-T4) | >= 85% | 90.59% | **PASS** |
| Line coverage floor (`CLAUDE.md` § UT2, repository-wide) | >= 80% | 90.59% | PASS |
| No regression versus baseline | >= 88.48% | 90.59% | **PASS** (+2.11 pp) |

Where `CLAUDE.md` (>= 80%) and `.claude/rules/general-unit-test.md` (>= 85%) differ, **the stricter
figure — 85% — is the one recorded and applied.** No threshold is modified anywhere by this change;
threshold reconciliation is owned by #494.

Every added production line is executed except one (see § 3). The 19 missed lines are the same 19 as
at baseline: they lie in `ConvertTo-KoverageRelativePath`, the two `throw` guards, and the
`Merge-CoberturaClassesByFilename` element-creation and condition-rewrite paths, all of which
pre-date this change.

## 2. Branch coverage — NOT MEASURABLE, recorded as an auditable negative-evidence claim

The `>= 75%` branch floor in `.claude/rules/general-unit-test.md` and
`.claude/rules/powershell.md:64` has **no available instrument for PowerShell in this repository.**
This is stated rather than silently omitted, and no number is invented in its place.

- **SearchScope:** `<FEATURE>/evidence/qa-gates/pester-coverage-final.2026-08-10T23-10.xml`
  (the P4-T3 `OutputPath`)
- **SearchPatterns:** `report/counter[@type='BRANCH']`
- **SearchResult:** `none`
- **Counter types the report actually contains:** `INSTRUCTION`, `LINE`, `METHOD`, `CLASS` — and
  nothing else. That exhaustive enumeration is the proof of absence.
- **Pester version:** 5.6.1

This limitation is a property of the PowerShell coverage tooling. It is **not caused by this
change** — the identical four counters and no `BRANCH` counter were recorded at baseline in P0-T16,
before any production edit — and it is **not** grounds for altering any threshold. Thresholds are
owned by child feature #494 (wave 2). This feature proposes and makes no threshold change.

## 3. New-code coverage — `Get-CoberturaClassLineSummary`

| Quantity | Value |
| --- | --- |
| `$first` (function declaration line, post-change file) | **161** |
| `$last` (closing brace line, post-change file) | **259** |
| JaCoCo `<line>` records in `[161, 259]` | **40** |
| Records with `ci > 0` (covered) | **39** |
| Records with `ci = 0` (uncovered) | **1** |
| **New-code line coverage** | **39 / 40 = 97.50%** |

`$last = 259` was determined by reading the file: line 259 is the closing brace of
`Get-CoberturaClassLineSummary`, immediately preceding the blank line 260 and
`function Merge-CoberturaClassesByFilename {` at line 261.

| Gate | Threshold | Observed | Verdict |
| --- | --- | --- | --- |
| New modules/classes/methods (`CLAUDE.md` § UT2) | >= 90% | **97.50%** | **PASS** |
| No regression on changed lines (`.claude/rules/powershell.md`) | no decrease | whole-file rate rose 88.48% -> 90.59%; the changed region is new code at 97.50% | **PASS** |

The line set is non-empty (40 records), so no percentage is reported over a zero-length range.

### The single uncovered statement, recorded precisely

**Line 220: `$existing.Hits = $hits`** — the body of `if ($hits -gt $existing.Hits)`, i.e. the
assignment that fires only when a *later*-enumerated entry carries strictly more hits than the entry
already in the map.

The enclosing `if` condition at line 219 **is** executed and evaluated (it is covered); only the
assignment body is not. The reason is structural: the helper enumerates the class-level rollup
first, and in every fixture and in both committed sample documents the class-level rollup already
carries the maximum hits value for a repeated line number, so the candidate never exceeds it. F4
exercises the repeat-key `max(hits)` rule in the direction the real data produces — class-level
`hits=1` versus method-level `hits=1` and `hits=0` — and asserts the correct deduplicated result
(`lines-valid` = `'1'`, `lines-covered` = `'1'`).

This is recorded as an observation, not concealed. It does **not** trigger the P4-T6 remediation
path: that path fires only when the whole-file line rate falls below 88.48% or the new-code rate
falls below 90%, and neither condition holds (90.59% and 97.50% respectively). Accordingly **N = 0**
`It` blocks were added, and the P4-T3 / P4-T4 acceptance count remains 19 + 0 = 19.

No threshold was lowered, no line was excluded from measurement, and no shortfall is reported as a
pass.
