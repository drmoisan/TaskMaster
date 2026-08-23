# Coverage Comparison — Remediation Cycle 2 Before vs After

- Task: `[P2-T8]`
- Issue: #418
- Evidence series: `2026-08-05T05-00`
- Timestamp: 2026-08-05T00-21

Before source: `evidence/remediation-baseline/coverage-basis.2026-08-05T05-00.md` (`[P0-T11]`),
transcribing `evidence/qa-gates/test-coverage.2026-08-05T01-50.md` and
`evidence/qa-gates/coverage-delta.2026-08-05T01-50.md`.
After source: `evidence/qa-gates/test-coverage.2026-08-05T05-00.md` (`[P2-T7]`), read from
`coverage/coverage.cobertura.xml`.

## Counting method — like-for-like with the cited artifact

The same per-`<line>`-descendant method `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md` uses:
every `<line>` descendant of each deduplicated `<package>` element, with branch figures summed from the
`condition-coverage` fractions of `<line branch="True">` descendants. Cobertura repeats each line both
under its `<method>` element and under the class-level `<lines>` element, so class and package figures
produced this way are exactly twice the sum of their members' figures.

**Validated:** the summed per-package figures reproduce the Cobertura root element exactly
(93529/109518 line, 21576/27418 branch). Applying a different method would yield package and class
denominators roughly 2x different and could manufacture a false denominator-change finding; that risk is
foreclosed by the reproduction check.

## This cycle modified no `.cs` file, so the expected delta on every figure is zero

The only functional change is two build-configuration files: four added lines plus one `<Private>` element
in `SVGControl.Test/SVGControl.Test.csproj`, and one `<package>` line in
`SVGControl.Test/packages.config`. No production or test source was added, removed, or altered — measured
at `[P1-T7]`, where `git diff --name-only | grep -c '\.cs$'` returned **0**.

---

## 1. Repository-wide

| Metric | Before | After | Delta | Floor | Verdict |
|---|---|---|---|---|---|
| Line | 93539 / 109518 = **85.4097%** | 93529 / 109518 = **85.4006%** | −10 covered, **0** total, **−0.0091 pts** | `>= 85%` | **PASS** (+0.4006 pts margin) |
| Branch | 21584 / 27418 = **78.7220%** | 21576 / 27418 = **78.6928%** | −8 covered, **0** total, **−0.0292 pts** | `>= 75%` | **PASS** (+3.6928 pts margin) |

**Repository-wide verdict: PASS.** Both floors are met with margin.

**Both denominators are unchanged** — `lines-valid` 109518 and `branches-valid` 27418 in both runs — so
`COVERAGE_DENOMINATOR_CHANGE` is **not** reported. This is the expected signature of a cycle that
instruments no new code and removes none.

---

## 2. Per-package comparison — every non-zero delta explained by name

| Package | Line before | Line after | Line covered delta | Line total delta | Branch before | Branch after | Branch covered delta |
|---|---|---|---|---|---|---|---|
| `UtilitiesCS` | 68383 / 76065 | 68371 / 76065 | **−12** | 0 | 15830 / 18980 | 15822 / 18980 | **−8** |
| `QuickFiler` | 13992 / 17158 | 13994 / 17158 | **+2** | 0 | 2964 / 3982 | 2964 / 3982 | 0 |
| `TaskMaster` | 2762 / 4244 | 2762 / 4244 | 0 | 0 | 557 / 942 | 557 / 942 | 0 |
| **`SVGControl`** | **1696 / 3532** | **1696 / 3532** | **0** | **0** | **594 / 1248** | **594 / 1248** | **0** |
| `ToDoModel` | 2032 / 3442 | 2032 / 3442 | 0 | 0 | 468 / 928 | 468 / 928 | 0 |
| `TaskVisualization` | 2736 / 3012 | 2736 / 3012 | 0 | 0 | 649 / 768 | 649 / 768 | 0 |
| `Tags` | 1374 / 1480 | 1374 / 1480 | 0 | 0 | 342 / 374 | 342 / 374 | 0 |
| `TaskTree` | 556 / 577 | 556 / 577 | 0 | 0 | 180 / 196 | 180 / 196 | 0 |
| `VBFunctions` | 8 / 8 | 8 / 8 | 0 | 0 | 0 / 0 | 0 / 0 | 0 |
| **TOTAL** | **93539 / 109518** | **93529 / 109518** | **−10** | **0** | **21584 / 27418** | **21576 / 27418** | **−8** |

Reconciliation: −12 (`UtilitiesCS`) + 2 (`QuickFiler`) = **−10**, matching the repository line delta
exactly. The branch delta of −8 is entirely `UtilitiesCS`. Seven of nine packages moved by zero on every
metric.

### Explanation of the two non-zero deltas, by name

**`UtilitiesCS` −12 line covered, −8 branch covered.** Run-to-run instrumentation variance in a package
this cycle does not touch. `UtilitiesCS` appears nowhere in this cycle's diff — `[P1-T7]` measured exactly
two functional files, both under `SVGControl.Test`, with zero `.cs` paths anywhere. Its denominator is
unchanged at 76065 line and 18980 branch, so no `UtilitiesCS` code entered or left measurement; only which
already-instrumented lines happened to execute varied. This is the same phenomenon and the same package
the prior cycle disclosed: `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md` § 1 recorded
"`UtilitiesCS` +4, `QuickFiler` −2 ... matching the same small run-to-run variance `[P0-T9]` disclosed and
unrelated to any code change." The direction is reversed this time and the magnitude is comparable
(−12 of 68383 covered lines is 0.018% of that package's numerator). `UtilitiesCS` is by far the largest
package at 76065 measured lines and hosts the concurrency-, timing-, and reflection-sensitive suites, so
it is where such variance is expected to surface.

**`QuickFiler` +2 line covered.** The same variance, in the opposite direction, in another package this
cycle does not touch. Denominator unchanged at 17158.

**`SVGControl` 0 on all four metrics.** This is the material finding. The package this cycle's change
affects moved by **zero** covered lines, **zero** covered branches, **zero** line denominator, and **zero**
branch denominator.

`evidence/remediation-baseline/coverage-basis.2026-08-05T05-00.md` § 6 recorded in advance that a
`SVGControl` *improvement* was possible — because making `ExCSS` resolvable could let parse paths execute
further — and that a *decrease* would require investigation. Measured outcome: **neither**. The figures are
identical. The reason is that the nine-assembly wrapper run already supplied `ExCSS` to the test host from
a sibling assembly's output directory before the fix, so under this particular run shape the fix changes
which directory satisfies the bind but not which lines execute. That is precisely why the plan's Design
Decision 6 makes the order-dependence proof, not this run, the decisive verification: a nine-assembly run
passes and measures identically with or without the fix. **No investigation trigger fires, because no
`SVGControl` figure decreased.**

---

## 3. Class-level figures — the three classes `[P2-T8]` names

| Class | Line before | Line after | Delta | Branch before | Branch after | Delta |
|---|---|---|---|---|---|---|
| `SVGControl.SvgRenderer` | 332 / 414 = **80.1932%** | 332 / 414 = **80.1932%** | **0** | 64 / 84 = 76.1905% | 64 / 84 = 76.1905% | **0** |
| `SVGControl.SvgAssemblyProbe` | 102 / 102 = **100.0000%** | 102 / 102 = **100.0000%** | **0** | 92 / 92 = 100.0000% | 92 / 92 = 100.0000% | **0** |
| `SVGControl.SvgAssemblyResolver` | 106 / 172 = **61.6279%** | 106 / 172 = **61.6279%** | **0** | 28 / 52 = 53.8462% | 28 / 52 = 53.8462% | **0** |

All six figures byte-identical. `SVGControl.SvgAssemblyProbe` holds **100% line and 100% branch**.

---

## 4. No changed line lost coverage

The set of changed lines this cycle is **five lines of `.csproj` and one line of `packages.config`**.
Neither file type is instrumented for code coverage — MSBuild project files and NuGet package manifests
contain no executable IL — so the changed-line set is empty in coverage terms and cannot have lost
coverage.

Stated for the class of code that *could* have regressed: every `SVGControl` figure at package, class, and
denominator level is identical before and after, so **no line that was covered before became uncovered**.
The `-10`/`-8` repository movement is entirely in `UtilitiesCS` and `QuickFiler`, is unaccompanied by any
denominator change, and is attributable to run-to-run execution variance rather than to any line losing
its ability to be covered.

**No regression on changed lines: PASS.**

---

## 5. Gate verdicts

| Gate | Value | Threshold | Verdict |
|---|---|---|---|
| Repository line coverage | **85.4006%** | `>= 85%` | **PASS** |
| Repository branch coverage | **78.6928%** | `>= 75%` | **PASS** |
| No regression on changed lines | changed lines are non-instrumented build config; all `SVGControl` figures identical | required | **PASS** |
| Denominator stability | 109518 / 27418 unchanged | — | no `COVERAGE_DENOMINATOR_CHANGE` |
| Modified-file floor, `SVGControl/SvgRenderer.cs` | 80.1932% | `>= 85%` | **not targeted this cycle** (G-1) |
| New-file floor, `SVGControl/SvgAssemblyResolver.cs` | 61.6279% | `>= 85%` / `>= 90%` | **not targeted this cycle** (G-9) |

No placeholder appears in this artifact; every figure is numeric.

---

## 6. The two file-level floors are NOT targeted this cycle

Stated explicitly as `[P2-T8]` requires.

### G-1 — `SVGControl/SvgRenderer.cs` at 80.1932% against the `>= 85%` floor

**Not targeted this cycle.** The figure is unchanged from the basis at 332/414 = 80.1932%. The residual is
pre-existing members that neither cycle of issue #418 touched: `AddMargins` (0/15), `Render()` (18/26), the
two `SvgDocument` constructor overloads (0/8 each), `get_Margin()` (0/1), and one line of
`AdjustSizeProportionately` (22/23), in method units.

**The entry that owns this residual is
`docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`.** Closing the gap would require
writing tests for pre-existing WinForms/GDI-bound rendering members, which is scope widening prohibited by
the binding `## Do Not Do` list ("Do not widen scope beyond the enumerated items. Work mode is
`minor-audit`") and by `remediation-inputs.2026-08-04T22-28.md` § Non-actionable, which states of G-1:
"**Do not target in this cycle.**"

### G-9 — `SVGControl/SvgAssemblyResolver.cs` at 61.6279% against the `>= 85%` / `>= 90%` floors

**Not targeted this cycle, and it awaits a maintainer decision rather than code.** The figure is unchanged
from the basis at 106/172 = 61.6279%. The entire shortfall is one member, `ResolveByNameAndKey` at
47/80 = 58.75%, which is `private static`, invoked only by the CLR on a failed assembly bind, and which
carries the ratified exception:

```
COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey
```

That member was relocated verbatim by R-6 rather than authored, so `SVGControl.SvgAssemblyResolver` is a
relocation and not a new module. `Install()`, the only genuinely new member, measures 6/6 = 100%.

`remediation-inputs.2026-08-04T22-28.md` § Non-actionable records the reviewer's disposition: G-9 "Needs a
maintainer decision, not code: either extend the ratified exception to file scope or fold the residual
into the coverage-uplift follow-up that owns G-1", and directs "**Do not attempt to raise this by testing
the CLR callback end-to-end.**" **This cycle surfaces it to the maintainer and does not remediate it.**

No task in this plan targets either floor, and in particular **no testable member was relocated into
either file to lift its ratio**, which would game the metric rather than measure behavior. No
`[ExcludeFromCodeCoverage]` attribute and no `coverage.config` exclusion was added to either file, which
`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy makes a Blocking finding.

---

## Output Summary

Repository line coverage 85.4097% → **85.4006%** (93529/109518) and branch 78.7220% → **78.6928%**
(21576/27418); **both floors PASS** with margins of +0.4006 and +3.6928 points. Both denominators are
unchanged (109518, 27418), so `COVERAGE_DENOMINATOR_CHANGE` is not reported. The **−10 line / −8 branch**
numerator movement is explained by name and confined to two packages this cycle does not touch:
`UtilitiesCS` (−12 line, −8 branch) and `QuickFiler` (+2 line), both with unchanged denominators — the same
run-to-run instrumentation variance the prior cycle disclosed for the same two packages. **`SVGControl` is
byte-identical on all four package metrics and all six class metrics**: `SvgRenderer` 332/414 = 80.1932%,
`SvgAssemblyProbe` 102/102 = 100.0000% line and 92/92 = 100.0000% branch, `SvgAssemblyResolver`
106/172 = 61.6279%. This cycle modified no `.cs` file, so the expected delta of zero on every `SVGControl`
figure is exactly what was measured; the anticipated possible *improvement* did not materialize because
the nine-assembly wrapper already supplied `ExCSS` from a sibling output, which is why the order-dependence
proof and not this run is the decisive verification. **No changed line lost coverage** — the changed lines
are non-instrumented build configuration. The `>= 85%` file-level floors on `SVGControl/SvgRenderer.cs`
(G-1, owned by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`) and
`SVGControl/SvgAssemblyResolver.cs` (G-9, awaiting a maintainer decision rather than code) are **not
targeted this cycle**. **Repository-wide verdict: PASS.**
