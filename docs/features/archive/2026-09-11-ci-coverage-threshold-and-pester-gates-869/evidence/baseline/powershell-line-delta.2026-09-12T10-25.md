# Phase 0 — Required PowerShell LINE uplift delta (P0-T10)

Timestamp: 2026-09-14T18-09

Command: `pwsh -NoProfile -Command '<worktree prologue>; $C=662; $T=839; "Pct=" + ([double](100*$C/$T)).ToString("0.00"); "Ceil=" + [math]::Ceiling(0.80*$T); "N=" + ([math]::Ceiling(0.80*$T) - $C)'`
EXIT_CODE: 0

## Inputs, from the P0-T9 artifact

- C, the covered LINE count: **662**
- missed LINE count: **177**
- T, the total, equal to covered plus missed: **839**

## Baseline LINE percentage

100 × 662 / 839 = 78.90 percent, to two decimal places.

This is the LINE figure. It is not Pester's command figure, which the P0-T8 artifact records separately as 79.04 percent and which carries no threshold.

## Formula and substitution

The formula, stated in the required form:

**N equals the ceiling of 0.80 times T, minus C.**

Substituted with the measured integers:

- 0.80 × 839 = 671.2
- ceiling of 671.2 = 672
- 672 − 662 = **10**

**N = 10.** The delivery must add at least 10 covered LINE entries under `scripts/vscode`, net of any covered lines it loses, for the measured LINE figure to reach or exceed 80 percent.

## Prior basis, cited and not used

The 2026-09-03 figures 535 covered, 681 total and 78.56 percent are cited as the prior basis only and are not used in the computation above. Every integer in the arithmetic comes from the P0-T9 measurement of the document written by the P0-T8 run on this tree. The prior basis describes a tree with a smaller denominator: it predates `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`, `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1`, and the denominator has moved from 681 to 839 accordingly. No fixed delta number is carried forward from it.

Output Summary: baseline LINE coverage is 662 of 839, which is 78.90 percent, below the 80 floor. The required uplift is N = 10 covered lines, computed as the ceiling of 0.80 times 839, minus 662.
