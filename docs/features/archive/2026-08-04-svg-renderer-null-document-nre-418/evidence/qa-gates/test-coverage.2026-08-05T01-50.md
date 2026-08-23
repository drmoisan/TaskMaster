# Final QC Stage 4 — Coverage-Enabled Test Run

- Task: `[P2-T6]`
- Issue: #418
- Evidence series: `2026-08-05T01-50`
- Toolchain pass: **1**

Timestamp: 2026-08-05T02-05 (UTC)

Command:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
```

`-SearchRoot .` was used as mandated.

EXIT_CODE: 0

Coverage report read: `coverage/coverage.cobertura.xml`.

## Execution metrics

| Metric | Value |
|---|---|
| Test assemblies discovered | **9** |
| Total tests | **6150** |
| Passed | **6150** |
| **Failed** | **0** |
| Skipped | **0** |
| Wall time | 54.4016 s |
| Result line | `Test Run Successful.` |
| `grep -c "^  Failed "` | 0 |
| `grep -ci "test host process crashed"` | 0 — no crash, no rerun |

## Numeric repository-wide coverage

| Metric | Covered / Total | Percent | Floor | Verdict |
|---|---|---|---|---|
| Line | **93539 / 109518** | **85.4097%** | `>= 85%` | **PASS** (+0.4097 pts) |
| Branch | **21584 / 27418** | **78.7220%** | `>= 75%` | **PASS** (+3.7220 pts) |

Cobertura root attributes agree exactly: `line-rate="0.854097" lines-covered="93539"
lines-valid="109518"`, `branch-rate="0.78722" branches-covered="21584" branches-valid="27418"`.

Counting method: every `<line>` descendant of each deduplicated `<package>` element, with branch figures
summed from the `condition-coverage` fractions of `<line branch="True">` descendants — the same
per-`<line>`-descendant method `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` uses.

Per-package breakdown (nine first-party packages; no vendored assembly inflates the denominator):

| Package | Line covered/total | Line % | Branch covered/total | Branch % |
|---|---|---|---|---|
| `UtilitiesCS` | 68383 / 76065 | 89.9007% | 15830 / 18980 | 83.4036% |
| `QuickFiler` | 13992 / 17158 | 81.5480% | 2964 / 3982 | 74.4350% |
| `TaskMaster` | 2762 / 4244 | 65.0801% | 557 / 942 | 59.1295% |
| `SVGControl` | 1696 / 3532 | 48.0181% | 594 / 1248 | 47.5962% |
| `ToDoModel` | 2032 / 3442 | 59.0354% | 468 / 928 | 50.4310% |
| `TaskVisualization` | 2736 / 3012 | 90.8367% | 649 / 768 | 84.5052% |
| `Tags` | 1374 / 1480 | 92.8378% | 342 / 374 | 91.4439% |
| `TaskTree` | 556 / 577 | 96.3605% | 180 / 196 | 91.8367% |
| `VBFunctions` | 8 / 8 | 100.0000% | 0 / 0 | n/a |
| **TOTAL** | **93539 / 109518** | **85.4097%** | **21584 / 27418** | **78.7220%** |

## Class and member figures (full comparison in `coverage-delta.2026-08-05T01-50.md`)

| Scope | Covered / Total | Percent |
|---|---|---|
| `SVGControl.SvgRenderer` class line | 332 / 414 | 80.1932% |
| `SVGControl.SvgAssemblyProbe` class line | 102 / 102 | **100.0000%** |
| `SVGControl.SvgAssemblyResolver` class line | 106 / 172 | 61.6279% |
| `SvgAssemblyProbe.PublicKeyTokensEqual` | 15 / 15 | **100.0000%** (branch 18/18 = 100%) |
| `SvgAssemblyResolver.Install()` | 6 / 6 | **100.0000%** (branch 4/4 = 100%) |
| `SvgRenderer.ctor(byte[], Size, AutoSize)` | 17 / 17 | **100.0000%** |
| `SvgRenderer.ctor(byte[], Size, Padding, AutoSize)` | 18 / 18 | 100.0000% |
| `SvgAssemblyResolver.ResolveByNameAndKey` | 47 / 80 | 58.7500% (ratified exception) |

## Output Summary

`EXIT_CODE: 0`. **9 assemblies discovered, 6150 total, 6150 passed, 0 failed, 0 skipped**, no test host
crash and no rerun. Repository-wide **line 93539 / 109518 = 85.4097%** (PASS against `>= 85%`) and
**branch 21584 / 27418 = 78.7220%** (PASS against `>= 75%`). Stage 4 of toolchain pass 1 is clean; the
loop completed without a restart.
