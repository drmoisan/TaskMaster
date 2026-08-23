# Final QC Stage 4 — Coverage-Enabled Test Run

- Task: `[P2-T7]`
- Issue: #418
- Evidence series: `2026-08-05T05-00`
- Toolchain pass: **1**
- Timestamp: 2026-08-05T00-19

## Command

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
```

Run from the repository root. **`-SearchRoot .` was used as mandated** — the single-project form of this
wrapper family is defective under `Set-StrictMode` (a scalar `.Count` defect filed at
`docs/features/potential/2026-08-04-invoke-mstest-scalar-count-strictmode.md`).

```
EXIT_CODE: 0
```

Coverage report read: `coverage/coverage.cobertura.xml`.

## Execution metrics

| Metric | Value | Required by `[P2-T7]` | Verdict |
|---|---|---|---|
| **Test assemblies discovered** | **9** | expected 9 | met |
| **Total tests** | **6150** | at least the basis figure (6150) | met |
| **Passed** | **6150** | — | — |
| **Failed** | **0** | must equal 0 | **met** |
| Wall time | 1.0543 Minutes | — | — |
| Result line | `Test Run Successful.` | — | — |
| `grep -cE '^\s+Failed '` | **0** | — | independently measured |
| `grep -ci 'test host process crashed'` | 0 | — | no crash, no rerun |

The discovery line read verbatim: `Discovered 9 test assemblies.` This matches `[P0-T9]`'s independent
count of nine tracked `*.Test` projects and confirms the 8-not-9 `ExCSS.dll` glob was correctly explained
as `UtilitiesSwordfish.Test` being stale untracked output rather than an off-by-one.

Total is **6150**, equal to the basis figure transcribed in
`evidence/remediation-baseline/coverage-basis.2026-08-05T05-00.md` § 1 — satisfying "at least". The count
is unchanged rather than higher because this cycle adds no test; it changes only build configuration.

The final artifact line: `Done. Coverage artifact: C:\Users\DanMoisan\repos\TaskMaster\coverage\coverage.cobertura.xml`.

## Numeric repository-wide coverage

| Metric | Covered / Total | Percent | Floor | Verdict |
|---|---|---|---|---|
| **Line** | **93529 / 109518** | **85.4006%** | `>= 85%` | **PASS** (+0.4006 pts margin) |
| **Branch** | **21576 / 27418** | **78.6928%** | `>= 75%` | **PASS** (+3.6928 pts margin) |

Cobertura root attributes, read verbatim from `coverage/coverage.cobertura.xml`:

```
<coverage line-rate="0.854006" branch-rate="0.786928" complexity="24368" version="1.9"
          timestamp="1785901758" lines-covered="93529" lines-valid="109518"
          branches-covered="21576" branches-valid="27418">
```

Both floors are met.

## Counting method and its validation

The per-package and per-class figures below were computed with the method transcribed in
`evidence/remediation-baseline/coverage-basis.2026-08-05T05-00.md` § 2, so `[P2-T8]`'s comparison is
like-for-like: **every `<line>` descendant** of each deduplicated `<package>` element, with branch figures
summed from the `condition-coverage` fractions of `<line branch="True">` descendants.

**Method validated against the root element:** the summed per-package figures are
93529/109518 line and 21576/27418 branch, which reproduce the Cobertura root attributes **exactly**. This
confirms the method is the one that produced the basis figures and that no package was double-counted or
omitted.

## Per-package breakdown (nine first-party packages)

| Package | Line covered/total | Line % | Branch covered/total | Branch % |
|---|---|---|---|---|
| `UtilitiesCS` | 68371 / 76065 | 89.8850% | 15822 / 18980 | 83.3614% |
| `QuickFiler` | 13994 / 17158 | 81.5596% | 2964 / 3982 | 74.4350% |
| `TaskMaster` | 2762 / 4244 | 65.0801% | 557 / 942 | 59.1295% |
| **`SVGControl`** | **1696 / 3532** | **48.0181%** | **594 / 1248** | **47.5962%** |
| `ToDoModel` | 2032 / 3442 | 59.0354% | 468 / 928 | 50.4310% |
| `TaskVisualization` | 2736 / 3012 | 90.8367% | 649 / 768 | 84.5052% |
| `Tags` | 1374 / 1480 | 92.8378% | 342 / 374 | 91.4439% |
| `TaskTree` | 556 / 577 | 96.3605% | 180 / 196 | 91.8367% |
| `VBFunctions` | 8 / 8 | 100.0000% | 0 / 0 | n/a |
| **TOTAL** | **93529 / 109518** | **85.4006%** | **21576 / 27418** | **78.6928%** |

## The three `SVGControl` class figures

| Class | Line covered/total | Line % | Branch covered/total | Branch % |
|---|---|---|---|---|
| `SVGControl.SvgRenderer` | 332 / 414 | 80.1932% | 64 / 84 | 76.1905% |
| `SVGControl.SvgAssemblyProbe` | 102 / 102 | 100.0000% | 92 / 92 | 100.0000% |
| `SVGControl.SvgAssemblyResolver` | 106 / 172 | 61.6279% | 28 / 52 | 53.8462% |

**Every one of these six figures is byte-identical to the basis**, as expected for a cycle that modifies
no `.cs` file. Full comparison and disposition at `[P2-T8]`.

## Denominators are unchanged, which is the expected signature of a no-source-change cycle

Recorded here because it is the strongest single confirmation that this cycle altered no instrumented
code:

| Denominator | Basis | This run | Delta |
|---|---|---|---|
| Repository `lines-valid` | 109518 | **109518** | **0** |
| Repository `branches-valid` | 27418 | **27418** | **0** |
| `SVGControl` package line total | 3532 | **3532** | **0** |
| `SVGControl` package branch total | 1248 | **1248** | **0** |
| Every other package's line and branch total | — | — | **0** |

No instrumented line or branch entered or left the measurement, in any package.

The numerators moved slightly — line covered 93539 → 93529 (**−10**) and branch covered 21584 → 21576
(**−8**) — and the movement is confined entirely to two packages this cycle does not touch: `UtilitiesCS`
(−12 line, −8 branch) and `QuickFiler` (+2 line). **`SVGControl` did not move at all.** This is the
run-to-run instrumentation variance category the basis cycle itself disclosed (its own artifact recorded
`UtilitiesCS` +4 and `QuickFiler` −2 with no code change in either). Full accounting and verdict at
`[P2-T8]`.

## Output Summary

`EXIT_CODE: 0`. **9 assemblies discovered, 6150 total, 6150 passed, 0 failed**, no test host crash and no
rerun. Repository-wide **line 93529 / 109518 = 85.4006%** (PASS against `>= 85%`) and **branch
21576 / 27418 = 78.6928%** (PASS against `>= 75%`). All denominators are identical to the basis
(109518 and 27418 repository-wide, 3532 and 1248 for `SVGControl`), confirming no instrumented code
changed; the small numerator movement of −10 line and −8 branch is confined to `UtilitiesCS` and
`QuickFiler`, with all six `SVGControl` package and class figures byte-identical to the basis. The
per-package counting method was validated by reproducing the Cobertura root exactly. Stage 4 of toolchain
pass 1 is clean; the loop completed without a restart.
