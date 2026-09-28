# Coverage comparison (P10-T9)

Timestamp: 2026-09-14T21-26

## PowerShell — LINE

| Quantity | Baseline (P0-T9) | Final (P10-T4) | Difference |
| --- | --- | --- | --- |
| LINE covered | 662 | 731 | **+69** |
| LINE missed | 177 | 140 | −37 |
| LINE total | 839 | 871 | +32 |
| LINE percentage | 78.90 | **83.93** | **+5.03 points** |

The baseline crossed from below the floor to above it: 78.90 percent was below 80, and 83.93 percent is above it by 34 covered lines against a floor of 697 on the 871-line denominator.

The denominator grew by 32 because this delivery added production code — the branch assertion, the two extracted main functions and the five wrapper seams — and new production code enters both the numerator and the denominator. The covered count grew by more than the denominator did, which is why the percentage rose rather than fell.

No branch figure is recorded for PowerShell. Pester measures none.

## New PowerShell code — against the 90 floor

From the P6-T5 artifact:

- Aggregate LINE covered across the eight new functions: **60**
- Aggregate LINE total: **61**
- **Aggregate percentage: 98.36**

98.36 is at or above the 90 percent new-code floor, clearing it by 8.36 points. The single uncovered line is the body of `Invoke-SyncPackageReferences`, the deliberately-mocked package-reference sync seam.

## C# — four figures, under explicit labels, compared only like with like

### Pair one: AssertedDenominator (all surviving packages)

These are the document-root attributes, and **this is the pair the CI gate is judged on**.

| Figure | Baseline (P0-T7) | Final (P10-T8) | Movement |
| --- | --- | --- | --- |
| root `line-rate` | 0.858723 | 0.858754 | +0.000031 |
| root `branch-rate` | 0.800376 | 0.800493 | +0.000117 |

**Neither regressed.** Both rose marginally. Both remain above their floors: the line rate against 0.80 and the branch rate against 0.75.

Supporting counts, same denominators on both runs: `lines-covered` moved 56346 to 56348 against an unchanged `lines-valid` of 65616; `branches-covered` moved 13624 to 13626 against an unchanged `branches-valid` of 17022.

### Pair two: PrintedDenominator (allowlist)

These are the percentages carried by the printed `First-party coverage:` text.

| Figure | Baseline (P0-T7) | Final (P10-T8) | Movement |
| --- | --- | --- | --- |
| printed line percentage | 85.87 | 85.88 | +0.01 |
| printed branch percentage | 80.04 | 80.05 | +0.01 |

**Neither regressed.**

### The two pairs are not interchangeable

The asserted figure and the printed figure are computed by two different functions over two different package filters, per the mechanism re-derived in P0-T7: the root attributes come from `Get-CoberturaCoverageSummary` over every package surviving the allowlist removal with no name filter of its own, while the printed figure applies the allowlist filter itself at line 72 of `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`. **The CI gate is judged on the asserted pair.**

Comparing a baseline printed figure against a final asserted figure, or the reverse, is prohibited by this task and no such comparison is made above. The two denominators are not guaranteed equal, so a cross-denominator comparison would state a no-regression result that neither pair supports.

### Note on the small upward movement

Both C# pairs moved upward by two covered lines and two covered branches against unchanged denominators. This delivery changed no C# source file, so the movement is not attributable to a code change; it is run-to-run variation in the coverage collector across two full MSTest runs of the same 7293-test suite. It is recorded as an observation rather than explained away, and it is in the non-regressing direction. Both runs' agreement verdicts are unaffected: P0-T7 recorded AGREE at 85.87 and P10-T8 recorded AGREE at 85.88, each comparing its own run's two operands.

## Summary against every floor this delivery is judged on

| Floor | Value | Measured | Verdict |
| --- | --- | --- | --- |
| C# line (asserted root figure) | 80 | 85.88 | pass |
| C# branch (asserted root figure) | 75 | 80.05 | pass |
| PowerShell line | 80 | 83.93 | pass |
| New code (PowerShell) | 90 | 98.36 | pass |

No floor was lowered and no production file was excluded from measurement.
