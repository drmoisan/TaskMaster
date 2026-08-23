# Numeric Coverage Basis — Remediation Cycle 2

- Task: `[P0-T11]`
- Timestamp: 2026-08-04T23-46 — **this is the transcription time, not an execution time**
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`

## Field shape — read this before reading any figure below

**This artifact is a transcription, not an execution.** The nine-assembly coverage suite was **not**
re-run by `[P0-T11]`. Consequently:

- `Timestamp:` above is the time this transcription was written.
- The `Command:` and `EXIT_CODE:` values below are **quoted from the named source artifact**, not
  produced by any run in this cycle.
- A reaudit must not read the quoted exit code as evidence of a coverage run performed during
  remediation cycle 2. The cycle-2 execution is `[P2-T7]`, which writes
  `evidence/qa-gates/test-coverage.2026-08-05T05-00.md`.

`EXIT_CODE: 0` for this task itself: both source artifacts were read successfully and every required
figure was found as a number.

Reuse licence: `[P0-T5]` invariant (c) measured **0** `.cs`/`.csproj`/`packages.config`/`app.config`
differences between the executing HEAD and `a62391f7`, the commit the `2026-08-05T01-50` series was
captured in. Re-running the suite as a *baseline* would therefore reproduce these numbers. Evidence:
`evidence/remediation-baseline/tree-state.2026-08-05T05-00.md`.

## Source artifacts

| Figure class | Source artifact | Quoted `Command:` | Quoted `EXIT_CODE:` |
|---|---|---|---|
| Run metrics and repository/package/class figures | `evidence/qa-gates/test-coverage.2026-08-05T01-50.md` (its `[P2-T6]`) | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` | **0** |
| Counting method, member figures, gate verdicts | `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md` (its `[P2-T7]`) | (comparison artifact; no command of its own) | n/a |

Coverage report read by the source run: `coverage/coverage.cobertura.xml`.

## 1. Run metrics

Source: `evidence/qa-gates/test-coverage.2026-08-05T01-50.md` § Execution metrics.

| Metric | Value |
|---|---|
| **Test assemblies discovered** | **9** |
| **Total tests** | **6150** |
| **Passed** | **6150** |
| **Failed** | **0** |
| Skipped | **0** |
| Wall time | 54.4016 s |
| Result line | `Test Run Successful.` |
| `grep -c "^  Failed "` | 0 |
| `grep -ci "test host process crashed"` | 0 — no crash, no rerun |

The assembly count of 9 is corroborated independently by `[P0-T9]`'s census, which counted 9 tracked
`*.Test` projects via `git ls-files '*.Test/*.csproj'`.

## 2. Repository-wide figures

Source: same artifact § Numeric repository-wide coverage. Cobertura root attributes are quoted there as
agreeing exactly.

| Metric | Covered / Total | Percent | Floor | Verdict |
|---|---|---|---|---|
| **Line** | **93539 / 109518** | **85.4097%** | `>= 85%` | PASS (+0.4097 pts margin) |
| **Branch** | **21584 / 27418** | **78.7220%** | `>= 75%` | PASS (+3.7220 pts margin) |

Cobertura root, quoted: `line-rate="0.854097" lines-covered="93539" lines-valid="109518"`,
`branch-rate="0.78722" branches-covered="21584" branches-valid="27418"`.

### Counting method — must be reproduced by `[P2-T8]`

Quoted from `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md` § Counting method: package- and
class-level figures count **every `<line>` descendant** of the element, with branch figures summed from
the `condition-coverage` fractions of `<line branch="True">` descendants. Cobertura repeats each line
both under its `<method>` element and under the class-level `<lines>` element, so class and package
figures produced this way are exactly **twice** the sum of their members' figures. Per-member gates are
assessed on the `<method>` element's `line-rate`, with `branch-rate` recorded for information only.

`[P2-T8]` must apply this identical method so the comparison stays like-for-like. A different method
would produce a package/class denominator roughly 2x different and could manufacture a false
denominator-change finding.

### Per-package breakdown (nine first-party packages)

| Package | Line covered/total | Line % | Branch covered/total | Branch % |
|---|---|---|---|---|
| `UtilitiesCS` | 68383 / 76065 | 89.9007% | 15830 / 18980 | 83.4036% |
| `QuickFiler` | 13992 / 17158 | 81.5480% | 2964 / 3982 | 74.4350% |
| `TaskMaster` | 2762 / 4244 | 65.0801% | 557 / 942 | 59.1295% |
| **`SVGControl`** | **1696 / 3532** | **48.0181%** | **594 / 1248** | **47.5962%** |
| `ToDoModel` | 2032 / 3442 | 59.0354% | 468 / 928 | 50.4310% |
| `TaskVisualization` | 2736 / 3012 | 90.8367% | 649 / 768 | 84.5052% |
| `Tags` | 1374 / 1480 | 92.8378% | 342 / 374 | 91.4439% |
| `TaskTree` | 556 / 577 | 96.3605% | 180 / 196 | 91.8367% |
| `VBFunctions` | 8 / 8 | 100.0000% | 0 / 0 | n/a |
| **TOTAL** | **93539 / 109518** | **85.4097%** | **21584 / 27418** | **78.7220%** |

## 3. `SVGControl` package figures — the basis `[P2-T8]` compares against

| Metric | Covered / Total | Percent |
|---|---|---|
| Line | **1696 / 3532** | **48.0181%** |
| Branch | **594 / 1248** | **47.5962%** |

## 4. The three class figures `[P2-T8]` must report before and after

| Class | Line covered / total | Line % | Branch covered / total | Branch % |
|---|---|---|---|---|
| `SVGControl.SvgRenderer` | **332 / 414** | **80.1932%** | **64 / 84** | **76.1905%** |
| `SVGControl.SvgAssemblyProbe` | **102 / 102** | **100.0000%** | **92 / 92** | **100.0000%** |
| `SVGControl.SvgAssemblyResolver` | **106 / 172** | **61.6279%** | **28 / 52** | **53.8462%** |

Figures are class-level (doubled) units per the counting method above.

## 5. Supporting member figures, transcribed for continuity

| Type | Member | `line-rate` | Lines | `branch-rate` |
|---|---|---|---|---|
| `SvgAssemblyProbe` | `PublicKeyTokensEqual(byte[], byte[])` | 100.0000% | 15/15 | 100.0000% (18/18) |
| `SvgAssemblyProbe` | `GetProbeDirectories(string, string, string)` | 100.0000% | 25/25 | — |
| `SvgAssemblyProbe` | `TryGetDirectoryFromCodeBase(string)` | 100.0000% | 11/11 | — |
| `SvgAssemblyResolver` | `Install()` | 100.0000% | 6/6 | 100.0000% (4/4) |
| `SvgAssemblyResolver` | `ResolveByNameAndKey(object, ResolveEventArgs)` | 58.7500% | 47/80 | 45.4545% (10/22) — ratified exception |
| `SvgRenderer` | `.ctor(byte[], Size, AutoSize)` | 100.0000% | 17/17 | 100.0000% (2/2) |
| `SvgRenderer` | `.ctor(byte[], Size, Padding, AutoSize)` | 100.0000% | 18/18 | — |
| `SvgRenderer` | `DescribeFailure(Exception)` | 100.0000% | 5/5 | — |

The named exception carried forward, quoted from the source artifact § 6:

```
COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey
```

## 6. Expected delta this cycle, and what a non-zero delta means

**This cycle modifies no `.cs` file.** The only functional change is two build-configuration files —
one `<Reference>` block plus one `<Private>True</Private>` element in
`SVGControl.Test/SVGControl.Test.csproj`, and one `<package>` line in `SVGControl.Test/packages.config`.
No production or test source is added, removed, or altered, so no instrumented line enters or leaves the
denominator and no line's reachability changes as a consequence of an edit.

**The expected post-change delta on every coverage figure in this artifact is therefore zero.**

Any non-zero delta at `[P2-T8]` requires an explanation by name in that artifact. Two categories are
anticipated and are not code-attributable:

1. **Small run-to-run instrumentation variance.** The `2026-08-05T01-50` cycle itself disclosed this:
   its `coverage-delta` artifact records `UtilitiesCS` +4 and `QuickFiler` −2 net covered lines with no
   code change in either package. A delta of this magnitude, confined to packages outside `SVGControl`,
   is variance rather than regression.
2. **`SVGControl` figures may legitimately *improve*.** This is the one substantive prediction worth
   recording in advance. The fix makes `ExCSS` resolvable inside the `SVGControl.Test` host, so parse
   paths that previously threw `FileNotFoundException` before reaching their target lines may now
   execute further. Under the nine-assembly wrapper the sibling ordering already supplied `ExCSS`, so
   the effect may be nil; but if `SVGControl` line or branch coverage rises, that is a consequence of
   the fix restoring intended execution, not of a source change, and `[P2-T8]` must say so by name
   rather than treat it as unexplained.

A **decrease** in any `SVGControl` figure, by contrast, would not be explicable on these grounds and
would require investigation before the pass is accepted.

## 7. Gate thresholds carried into `[P2-T8]`

| Gate | Basis value | Threshold | Basis verdict |
|---|---|---|---|
| Repository line coverage | 85.4097% | `>= 85%` | PASS |
| Repository branch coverage | 78.7220% | `>= 75%` | PASS |
| No regression on changed lines | no line lost coverage | required | PASS |
| Modified-file floor, `SVGControl/SvgRenderer.cs` | 80.1932% | `>= 85%` | **not targeted** — G-1, owned by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` |
| New-file floor, `SVGControl/SvgAssemblyResolver.cs` | 61.6279% | `>= 85%` / `>= 90%` | **not targeted** — G-9, awaits a maintainer decision rather than code |

Neither G-1 nor G-9 is targeted by any task in this cycle, and `[P2-T8]` must state that explicitly.

## Output Summary

The numeric coverage basis is registered with no placeholder. Source run (quoted, not executed here):
`Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`, `EXIT_CODE: 0`, **9 assemblies
discovered, 6150 total, 6150 passed, 0 failed, 0 skipped**. Repository-wide **line 93539 / 109518 =
85.4097%** and **branch 21584 / 27418 = 78.7220%**, both PASS. `SVGControl` package line **1696 / 3532 =
48.0181%**, branch **594 / 1248 = 47.5962%**. Class figures: `SVGControl.SvgRenderer` **332 / 414 =
80.1932%** line and 64/84 = 76.1905% branch; `SVGControl.SvgAssemblyProbe` **102 / 102 = 100.0000%** line
and 92/92 = 100.0000% branch; `SVGControl.SvgAssemblyResolver` **106 / 172 = 61.6279%** line and 28/52 =
53.8462% branch. The per-`<line>`-descendant counting method is transcribed so `[P2-T8]` reproduces it.
Because this cycle modifies no `.cs` file, the expected delta on every figure is **zero**; any non-zero
delta must be explained by name at `[P2-T8]`, with instrumentation variance outside `SVGControl` and a
possible `SVGControl` *improvement* from newly resolvable `ExCSS` recorded here in advance as the two
anticipated non-code causes.
