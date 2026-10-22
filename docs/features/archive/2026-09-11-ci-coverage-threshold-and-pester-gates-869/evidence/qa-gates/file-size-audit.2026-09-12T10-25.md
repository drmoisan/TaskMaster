# File size audit after the final format pass (P10-T10)

Timestamp: 2026-09-14T21-28

Tool: Grep, count output mode, over the start-of-line pattern, run against each of the three directories `scripts/vscode`, `tests/scripts/vscode` and `.github/workflows`. Taken after the P10-T1 final formatter pass, which rewrote nothing.

Ceiling: **500 lines**, per the File Size Limit in `.claude/rules/general-code-change.md` and section 4 of the General Code Change Policy in CLAUDE.md.

## scripts/vscode — 15 files

| File | Lines |
| --- | --- |
| `Invoke-MSTestWithCoverage.Helpers.ps1` | 471 |
| `Invoke-MSTestWithCoverage.ps1` | **439** |
| `Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 413 |
| `Invoke-VSBuild.ps1` | 270 |
| `Invoke-MSTest.ps1` | 262 |
| `Invoke-MSTestWithCoverage.Projection.ps1` | 197 |
| `Invoke-MSTestWithCoverage.FirstParty.ps1` | 162 |
| `Sync-PackageReferences.ps1` | 159 |
| `Invoke-MSTest.TrxSummary.ps1` | 150 |
| `Invoke-MSTestWithCoverage.Threshold.ps1` | 126 |
| `Invoke-Restore.ps1` | 122 |
| `Install-RepoDotNetSdk.ps1` | 111 |
| `TestProcessCleanup.ps1` | 70 |
| `Invoke-MSTestWithCoverage.PackageRate.ps1` | 65 |
| `TaskMaster.cli.runsettings` | 9 |

## tests/scripts/vscode — 18 files

| File | Lines |
| --- | --- |
| `Invoke-MSTest.RunSettings.Tests.ps1` | **498** |
| `Invoke-MSTestWithCoverage.Projection.Tests.ps1` | 495 |
| `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | 494 |
| `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` | 486 |
| `Invoke-VSBuild.Tests.ps1` | 282 |
| `Invoke-MSTestWithCoverage.FirstParty.Tests.ps1` | 271 |
| `Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` | 268 |
| `Invoke-MSTest.TrxSummary.Tests.ps1` | 193 |
| `Invoke-MSTestWithCoverage.Merge.Tests.ps1` | 187 |
| `Invoke-MSTest.Main.Tests.ps1` | 177 |
| `Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | 175 |
| `Invoke-Restore.Tests.ps1` | 160 |
| `TestProcessCleanup.Tests.ps1` | 126 |
| `Invoke-MSTest.ResultsDirectory.Tests.ps1` | 119 |
| `Install-RepoDotNetSdk.Tests.ps1` | 90 |
| `Invoke-MSTest.AssemblyDiscovery.Tests.ps1` | 79 |
| `Invoke-MSTestWithCoverage.PackageRate.Tests.ps1` | 70 |
| `Invoke-MSTestWithCoverage.Threshold.Tests.ps1` | 26 |

## .github/workflows — 9 files

| File | Lines |
| --- | --- |
| `README.md` | 234 |
| `codex-web-setup-test.yml` | 110 |
| `_mstest-coverage.yml` | 103 |
| `_pester.yml` | 80 |
| `_build-nullable.yml` | 76 |
| `_build-analyzers.yml` | 69 |
| `_format-check.yml` | 41 |
| `ci.yml` | 35 |
| `_actionlint.yml` | 29 |

## Verdict

A count is listed above for **every file in all three directories**, 42 files in total. **Every count is at most 500.** The largest is 498.

`README.md` at 234 lines is a Markdown documentation file, which the File Size Limit explicitly exempts; it is well under the ceiling in any case.

## The two named files

### `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`

- Pre-change figure: **498**
- Post-change count: **498**

The two figures are equal. The pre-change figure is the value P0-T4 measured and halts on, so this task's historical claim and the Phase 0 measurement are the same number read twice, not two independent assertions. Equality is the required outcome: P2-T4 edited this file strictly in place, replacing three literals on their own existing lines and adding no line. A post-change count differing from 498 would mean a line was added or removed in a file every task in this plan edits in place, and that discrepancy would itself be the finding.

### `scripts/vscode/Invoke-MSTestWithCoverage.ps1`

- Pre-change figure: **438**
- Post-change count: **439**

The count is exactly 439, as required. P2-T2 adds exactly one statement line to that file and no other task in this plan changes its length, so the increase of exactly one is the expected and asserted result.

Output Summary: 42 files audited across the three directories, every count at most 500, the largest being 498. The runsettings test file is 498 both before and after the change, and the coverage entry point moved from 438 to exactly 439.
