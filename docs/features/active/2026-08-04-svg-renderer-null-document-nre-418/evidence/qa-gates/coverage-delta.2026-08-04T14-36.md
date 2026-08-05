# [P2-T9] Coverage Comparison — Baseline vs Post-Change

Timestamp: 2026-08-04T20-04

Baseline source: `evidence/baseline/test-coverage.2026-08-04T21-04.md`
Post-change source: `evidence/qa-gates/test-coverage.2026-08-04T14-36.md`, read from
`coverage/coverage.cobertura.xml`

Baseline line/branch coverage: **line 93252 / 109252 = 85.3550%**, **branch 21448 / 27310 = 78.5353%**.
`SVGControl` package line 1412 / 3266 = 43.2333%. `SVGControl.SvgRenderer` class 264 / 422 = 62.559%.

Post-change line/branch coverage: **line 93484 / 109486 = 85.3844%**, **branch 21528 / 27406 = 78.5521%**.
`SVGControl` package line 1648 / 3500 = 47.0857%. `SVGControl.SvgRenderer` class 424 / 588 = 72.109%.

## Metric definition

Each per-member percentage below is the Cobertura `<method>` element's **`line-rate`** for the matching
`name` **and** `signature`, expressed as a percentage. The `>= 90%` newly-added-member gate is assessed
on `line-rate`, consistent with `CLAUDE.md`'s new-member threshold. Each member's `branch-rate` is
recorded alongside **for information only**; member-level branch coverage is not gated, because the
`>= 75%` branch floor in `.claude/rules/general-unit-test.md` is a repository-level floor and is gated
by the `Repo-wide floor verdict:` line below.

Package- and class-level figures use the same per-`<line>`-descendant counting method the
`2026-08-04T21-04` baseline used, so every comparison in this artifact is like-for-like.

## Newly added members (`>= 90%` required)

| Type | Member | Signature | `line-rate` | Lines | `branch-rate` |
|---|---|---|---|---|---|
| `SVGControl.SvgRenderer` | `OpenFromBytes` | `(byte[])` | **100.000%** | 5/5 | 100.0% |
| `SVGControl.SvgRenderer` | `TryGetSvgDocument` (internal, seam) | `(byte[], System.Func<byte[], Svg.SvgDocument>, out Svg.SvgDocument, out System.Exception)` | **100.000%** | 23/23 | 87.5% |
| `SVGControl.SvgRenderer` | `TryGetSvgDocument` (public) | `(byte[], out Svg.SvgDocument, out System.Exception)` | **100.000%** | 3/3 | 100.0% |
| `SVGControl.SvgRenderer` | `GetSvgDocumentOrThrow` | `(byte[])` | **100.000%** | 6/6 | 100.0% |
| `SVGControl.SvgRenderer` | `DescribeFailure` (private logging helper) | `(System.Exception)` | **100.000%** | 5/5 | 100.0% |
| `SVGControl.SvgAssemblyProbe` | `TryGetDirectoryFromCodeBase` | `(string)` | **100.000%** | 11/11 | 100.0% |
| `SVGControl.SvgAssemblyProbe` | `GetProbeDirectories` | `(string, string, string)` | **100.000%** | 23/23 | 100.0% |

**New member minimum: >= 90% — PASS.** All seven members measure 100.000% line-rate. The minimum
observed across the set is 100.000%, 10 points above the gate. No member in this set is below 90%, so
no additional tests are required and `[P2-T7]` is not rerun.

`SVGControl.SvgAssemblyProbe` is named as the owning type for the last two rows rather than
`SvgRenderer`, per Design Decision 12 / `[P1-T19]`. Its class-level coverage is **68 / 68 = 100.000%**.

`GetSvgDocumentOrThrow` reaching 100% is the confirmed outcome of `[P2-T1]`. Before that task its
`return document!;` at `SVGControl/SvgRenderer.cs:469` was driven by no test and the member projected
approximately 2/3 = 66.7%, below the gate. The single added test
`GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument` closes it.

### Sub-75% `branch-rate` in the newly-added set

One member is below 100% branch-rate, and it is still **above** the 75% reference figure, so no
named-condition disclosure is strictly required. It is recorded for completeness:

- Internal seam `TryGetSvgDocument`, branch-rate **87.5% (7/8 conditions)**. The single undriven
  condition is at `SVGControl/SvgRenderer.cs:421`, `condition-coverage 50% (1/2)` — the `parse == null`
  half of the argument guard `if (file == null || parse == null)`. That is a defensive guard against an
  input no in-scope caller produces: the only production call site is the public overload at `:458`,
  which always passes `OpenFromBytes`. The other two branch points in the member measure 100%
  (`:419` 4/4, `:427` 2/2). No test is added to drive it.

## Changed pre-existing members (no-regression on changed lines required, `>= 90%` not required)

| Member | Signature | Baseline `line-rate` | Post-change `line-rate` | Lines | `branch-rate` | Direction |
|---|---|---|---|---|---|---|
| `GetSvgDocument` | `(byte[])` | 62.50% | **100.000%** | 4/4 | 100.0% | improved |
| `SvgRenderer` ctor | `(byte[], System.Drawing.Size, SVGControl.AutoSize)` | **0%** | **76.471%** | 13/17 | 50.0% | improved |
| `SvgRenderer` ctor | `(byte[], System.Drawing.Size, System.Windows.Forms.Padding, SVGControl.AutoSize)` | 100.00% | **100.000%** | 18/18 | 100.0% | unchanged |
| `ResolveByNameAndKey` | `(object, System.ResolveEventArgs)` | 72.09% | **68.116%** | 47/69 | 45.5% | rate fell, covered lines rose 31 -> 47 |

**No regression on changed lines: yes.**

The verdict basis specified by this task is the `SVGControl.SvgRenderer` class baseline of
**264 / 422 = 62.559%** from `evidence/baseline/test-coverage.2026-08-04T21-04.md`. Post-change the same
class measures **424 / 588 = 72.109%**, an improvement of 9.55 percentage points with the covered-line
count rising from 264 to 424 (+160). No changed line lost coverage.

Per-member notes:

- `SvgRenderer(byte[], Size, AutoSize)` had a **0%** baseline, so as this task states it cannot register
  a regression; any coverage is an improvement. Its four remaining uncovered lines (`:168-171`) are the
  **success** branch of that overload — `_doc = parsed; _original = parsed!.Draw().Size;` — which no
  in-scope test drives, because the AC-1 regression tests target the failure branch that produced the
  NRE. The four-argument overload's equivalent success branch **is** covered, at 18/18.
- `ResolveByNameAndKey` is the only member whose rate fell, and the fall is a denominator effect, not a
  loss of coverage. Covered lines rose from 31 to 47 while the member grew from 43 to 69 lines as
  `[P1-T18]` added the strategy-3 block. Its 22 uncovered lines partition cleanly:
  - `:67-72` and `:91-96` — pre-existing strategies 1 and 2 inner blocks, which were uncovered at
    baseline as well. Both regions call `PublicKeyTokensEqual`, which measures **0 / 15 = 0%** in this
    run, meaning it is never invoked; the lines that call it therefore cannot have been covered at
    baseline either. `[P1-T18]` required strategies 1 and 2 be preserved unchanged, so these are not
    changed lines.
  - `:123-135` — the **new** strategy-3 `Assembly.LoadFrom` block, the untestable wiring covered by
    this task's named exception below. The rest of strategy 3 (`:108-119`, the probe-directory
    resolution and `File.Exists` loop) **is** covered.

## Removed exception — audit trail

A `GetSvgDocumentOrThrow` named branch exception was added to this task at plan version 0.8 on the
basis that its null-`InnerException` branch was unreachable. Preflight pass 7 established by inspection
that the branch is not in that member: `SVGControl/SvgRenderer.cs:471` is a single statement,
`throw new InvalidOperationException(ParseFailed + DescribeFailure(error), error)`, and the null /
non-null decision lives inside `DescribeFailure`, whose null arm is covered through
`TryGetSvgDocument`'s `DescribeFailure(null)` call, exercised by
`TryGetSvgDocument_WhenTheParseSeamReturnsNull_ReturnsFalseWithNoCapturedError`. The exception was
removed as moot at plan version 0.9. **This run confirms the removal was correct on both counts:**
`GetSvgDocumentOrThrow` measures 100% line-rate and 100% branch-rate against the ordinary `>= 90%`
gate with no exception applied, and `DescribeFailure` measures 5/5 = 100% line-rate.

## `ResolveByNameAndKey` named exception

Measured percentage: **68.116% line-rate (47 / 69 lines), 45.5% branch-rate.**

Untestable wiring: yes — strategy-3 logic covered via TryGetDirectoryFromCodeBase and GetProbeDirectories

This member is `private static` and is invoked only by the CLR on a failed assembly bind. `[P1-T16]` and
`[P1-T17]` extracted all of its new decision logic into the two pure helpers on
`SVGControl.SvgAssemblyProbe`, both of which measure **100%** line-rate and **100%** branch-rate in the
newly-added table above. What remains in the handler is host-bound wiring whose strategy-3
`Assembly.LoadFrom` branch cannot be driven from a unit test without staging a real mismatched-key
assembly on disk, which `.claude/rules/general-unit-test.md` UT4 prohibits with zero approved
exceptions. No test was added that triggers a live assembly bind, and `[P2-T7]` was not rerun on
account of this member.

`COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgRenderer.ResolveByNameAndKey`

## Repo-wide floor verdict

**Repo-wide floor verdict: PASS.** Line **85.3844%** (93484 / 109486) against the `>= 85%` floor, margin
+0.3844 points. Branch **78.5521%** (21528 / 27406) against the `>= 75%` floor, margin +3.5521 points.
Both metrics **improved** over the baseline (line 85.3550%, branch 78.5353%), so no explained
denominator-change regression is recorded and `COVERAGE_DENOMINATOR_CHANGE` is **not** reported.

`SVGControl` package before / after:

| Metric | Baseline `2026-08-04T21-04` | Post-change | Delta |
|---|---|---|---|
| Line | 1412 / 3266 = 43.2333% | **1648 / 3500 = 47.0857%** | +236 covered, +234 total, **+3.85 pts** |
| Branch | 460 / 1140 = 40.3509% | **544 / 1236 = 44.0129%** | +84 covered, +96 total, **+3.66 pts** |

`SVGControl.SvgRenderer` class before / after: 264 / 422 = 62.559% -> **424 / 588 = 72.109%**, +9.55 pts.
`SVGControl.SvgAssemblyProbe` is new and enters at 68 / 68 = **100.000%**.

## Denominator change note

The `SVGControl` package denominator grew by 234 measured lines and the repository denominator by 234
lines (109252 -> 109486), an identical figure, confirming that all Phase 2 denominator movement is
inside the `SVGControl` package.

**The cause is not `SVGControl.Test` entering the measured set.** That occurred before the authoritative
baseline was captured: the `2026-08-04T21-04` baseline already records nine assemblies, 6112 tests, and
`SVGControl` at 1412 / 3266, as the plan's Open Questions section states. The two real movements are:

1. **The new production file `SVGControl/SvgAssemblyProbe.cs` (67 source lines) entering the
   `SVGControl` package.** It contributes **68** measured lines to the denominator, all 68 covered, so it
   raises both numerator and denominator equally and lifts the package rate.
2. **`SVGControl/SvgRenderer.cs` growing from 354 to 497 source lines.** Its class-level denominator
   moves from 422 measured lines at baseline to 588, a rise of 166, of which 160 are covered.

The numerator moves through the **28** new tests: 27 delivered in Phase 1 plus the one added by
`[P2-T1]`. Because the newly-added production lines are 96.4% covered in aggregate
(228 of 234 new measured lines), the denominator growth is more than paid for by numerator growth and
the repository-wide rate rose rather than fell. The decision rule in this task — record an explained
denominator-change regression and report `COVERAGE_DENOMINATOR_CHANGE` — was therefore retained as a
fallback and **did not fire**.

The pre-existing `SVGControl` production code that no test in this plan's scope exercises
(`DropDownEditor` 0/99, `SVGParser` 0/122, `ToggleSwitch` 0/62 plus 0/23 Designer, `SvgFileNameEditor`
0/104, and the three converters at 0/48, 0/48, 0/26 — all measured at 0.000% at baseline and unchanged
here) remains the dominant drag on the `SVGControl` package rate. It is out of scope for this
minor-audit change and no attempt was made to raise repository-wide coverage inside it.
