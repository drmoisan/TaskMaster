# Whole-Footprint File-Size Audit — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-14-04
- Task: [P5-T10]
- Finding: R9d
- EXIT_CODE: 0

## Scope

Every path in the spec `## Write Set` under "Production PowerShell" and "Tests", plus
`.github/workflows/dependabot-repair.yml` and `.github/workflows/_pester.yml`.

**17 files: 7 production PowerShell, 8 test PowerShell, 2 workflows.**

Markdown under the feature folder is exempt from the 500-line cap per
`.claude/rules/general-code-change.md` and is deliberately not listed.

## Production PowerShell — 7 files

| # | Path | Lines | Headroom under 500 |
|---|---|---|---|
| 1 | `scripts/dependencies/PackageGraph.psm1` | 465 | 35 |
| 2 | `scripts/dependencies/PackageCompatibility.psm1` | 172 | 328 |
| 3 | `scripts/dependencies/AnalyzerItemRepair.psm1` | 402 | 98 |
| 4 | `scripts/dependencies/ProjectConsistency.psm1` | **373** | 127 |
| 5 | `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | **470** | 30 |
| 6 | `scripts/vscode/Sync-PackageReferences.ps1` | 423 | 77 |
| 7 | `scripts/dependencies/ConsistencyVerifier.psm1` | **499** | **1** |

## Test PowerShell — 8 files

| # | Path | Lines | Headroom under 500 |
|---|---|---|---|
| 8 | `tests/scripts/dependencies/PackageGraph.Tests.ps1` | 487 | 13 |
| 9 | `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` | 124 | 376 |
| 10 | `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1` | 311 | 189 |
| 11 | `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` | 453 | 47 |
| 12 | `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | **429** | 71 |
| 13 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | **468** | 32 |
| 14 | `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | **393** | 107 |
| 15 | `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | **312** | 188 |

## Workflows — 2 files

| # | Path | Lines | Headroom under 500 |
|---|---|---|---|
| 16 | `.github/workflows/dependabot-repair.yml` | **173** | 327 |
| 17 | `.github/workflows/_pester.yml` | 80 | 420 |

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| Files listed | exactly **17** | **17** | PASS |
| Composition, production / tests / workflows | 7 / 8 / 2 | **7 / 8 / 2** | PASS |
| Every count an integer | yes | yes | PASS |
| **Files over 500 lines** | **0** | **0** | PASS |

The eight files this cycle edited are marked in bold. All nine unmarked files are untouched by
this cycle and are listed because the Write Set names them.

## Standing Observation — Two Files Near the Cap

| File | Lines | Headroom | Note |
|---|---|---|---|
| `scripts/dependencies/ConsistencyVerifier.psm1` | **499** | **1** | edited by this cycle, +6 |
| `tests/scripts/dependencies/PackageGraph.Tests.ps1` | 487 | 13 | **not** edited by this cycle |

**`ConsistencyVerifier.psm1` is the tightest file in the footprint at 499 of 500.** R9d named it
at 493 and it is tighter now: [P2-T3] added one resolver call, one rationale comment and four
`.DESCRIPTION` lines, having first compressed an edit that reached 510. The next addition to
this file **must extract rather than append**, which is the same instruction R9d gave and which
decision D1 already applied to the composition root.

`tests/scripts/dependencies/PackageGraph.Tests.ps1` at 487 is the second tightest and is not
this cycle's work; it is recorded so the observation is complete.

The file R9d named first, `Repair-PackageManifestConsistency.ps1`, improved from 498 to **470**
because [P2-T1] extracted a 36-line function from it. It now has 30 lines of headroom where it
had 2.

## Output Summary

17 files listed — 7 production PowerShell, 8 test PowerShell, 2 workflows — every count an
integer and **every one at most 500**. The largest is `ConsistencyVerifier.psm1` at 499, which
carries a standing instruction to extract rather than append.
