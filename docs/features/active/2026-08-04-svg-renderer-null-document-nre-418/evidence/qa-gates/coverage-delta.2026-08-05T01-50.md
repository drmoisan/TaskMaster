# Coverage Comparison — Remediation Cycle 1 Before vs After

- Task: `[P2-T7]`
- Issue: #418
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T02-07 (UTC)

Before source: `evidence/remediation-baseline/test-coverage.2026-08-05T01-50.md` (`[P0-T9]`, measured in
this session at `ea106111` with no source modification).
After source: `evidence/qa-gates/test-coverage.2026-08-05T01-50.md` (`[P2-T6]`), read from
`coverage/coverage.cobertura.xml`.
Cited prior basis: `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md`.

## Counting method (like-for-like with the cited artifact)

Package- and class-level figures count **every `<line>` descendant** of the element, which is the same
per-`<line>`-descendant method `coverage-delta.2026-08-04T14-36.md` used. Cobertura repeats each line
both under its `<method>` element and under the class-level `<lines>` element, so class and package
figures produced this way are exactly **twice** the sum of their members' figures. This is confirmed
empirically here: the method-level sums for `SVGControl.SvgRenderer` are 212/294 before and 166/207
after, and the class-level figures are 424/588 and 332/414 — exactly double. The method is applied
identically to both the before and after reports, so every comparison below is like-for-like, and the
figures reproduce the cited artifact exactly (`SVGControl` package 3500, `SvgRenderer` class 588).

Every **per-member** gate is assessed on the Cobertura `<method>` element's **`line-rate`**, with
`branch-rate` recorded **for information only**; member-level branch coverage is not gated.

---

## 1. Repository-wide

| Metric | Before (`[P0-T9]`) | After (`[P2-T6]`) | Delta | Floor | Verdict |
|---|---|---|---|---|---|
| Line | 93489 / 109486 = **85.3890%** | 93539 / 109518 = **85.4097%** | +50 covered, +32 total, **+0.0207 pts** | `>= 85%` | **PASS** (+0.4097 pts margin) |
| Branch | 21534 / 27406 = **78.5740%** | 21584 / 27418 = **78.7220%** | +50 covered, +12 total, **+0.1480 pts** | `>= 75%` | **PASS** (+3.7220 pts margin) |

**Repo-wide verdict: PASS.** Both metrics improved, so no explained denominator-change regression is
recorded and `COVERAGE_DENOMINATOR_CHANGE` is **not** reported.

Reconciliation of the repository deltas: the line denominator grew by exactly **+32**, identical to the
`SVGControl` package growth, confirming that all denominator movement is inside `SVGControl`. Of the +50
newly covered lines, **+48 are inside `SVGControl`**; the remaining +2 net is instrumentation variance in
other packages (`UtilitiesCS` +4, `QuickFiler` −2), matching the same small run-to-run variance
`[P0-T9]` disclosed and unrelated to any code change. All +50 branch-covered and +12 branch-valid
movement is inside `SVGControl`.

---

## 2. `SVGControl` package

| Metric | Before | After | Delta |
|---|---|---|---|
| Line | 1648 / 3500 = **47.0857%** | 1696 / 3532 = **48.0181%** | +48 covered, +32 total, **+0.9324 pts** |
| Branch | 544 / 1236 = **44.0129%** | 594 / 1248 = **47.5962%** | +50 covered, +12 total, **+3.5833 pts** |

Reconciliation, in class-level (doubled) units:

| Class | Covered delta | Total delta |
|---|---|---|
| `SVGControl.SvgRenderer` | −92 | −174 |
| `SVGControl.SvgAssemblyProbe` | +34 | +34 |
| `SVGControl.SvgAssemblyResolver` (new file) | +106 | +172 |
| **Sum** | **+48** | **+32** |

Exactly matches the measured package delta.

---

## 3. Class-level figures, with the relocation accounted for

| Class | Before | After | Delta |
|---|---|---|---|
| `SVGControl.SvgRenderer` | 424 / 588 = **72.1088%** (branch 86/168 = 51.1905%) | 332 / 414 = **80.1932%** (branch 64/84 = 76.1905%) | **+8.0844 pts line**, +24.9999 pts branch |
| `SVGControl.SvgAssemblyProbe` | 68 / 68 = **100.0000%** (branch 48/48 = 100%) | 102 / 102 = **100.0000%** (branch 92/92 = 100%) | held at 100% on a 50%-larger denominator |
| `SVGControl.SvgAssemblyResolver` | did not exist | 106 / 172 = **61.6279%** (branch 28/52 = 53.8462%) | new file, relocation |

### `SvgRenderer`'s denominator fell because members moved out, not because any line lost coverage

Stated explicitly as `[P2-T7]` requires. Exact member-level accounting (method units; class units are
double):

| Member | Before | After | Covered delta | Total delta | Cause |
|---|---|---|---|---|---|
| `ResolveByNameAndKey(object, ResolveEventArgs)` | 47 / 69 | **moved to `SvgAssemblyResolver`** | −47 | −69 | R-6 relocation (`[P1-T3]`) |
| `PublicKeyTokensEqual(byte[], byte[])` | 0 / 15 | **moved to `SvgAssemblyProbe`** | −0 | −15 | R-6 / CR-6 relocation (`[P1-T1]`) |
| `.cctor()` | 9 / 9 | 6 / 6 | −3 | −3 | static-constructor body reduced to `SvgAssemblyResolver.Install();` (`[P1-T3]`) |
| `.ctor(byte[], Size, AutoSize)` | 13 / 17 | **17 / 17** | **+4** | 0 | R-4 item 1 covered the success branch (`[P1-T14]`) |
| all 25 other members | unchanged | unchanged | 0 | 0 | untouched |
| **Sum (method units)** | 212 / 294 | 166 / 207 | **−46** | **−87** | |
| **Sum (class units, x2)** | 424 / 588 | 332 / 414 | **−92** | **−174** | |

Reading: of the 92 class-unit covered lines the class lost, **94 left with `ResolveByNameAndKey` and 6
left with the shortened static constructor, while 8 were newly covered** in the three-argument
byte-array constructor (−94 − 6 + 8 = −92). **Not one line that was covered before and still exists in
the class became uncovered.** Every other member's figure is byte-identical before and after, verified
member-by-member in the table dump. The rate rise from 72.109% to 80.193% is therefore a genuine
improvement (+8 newly covered lines) compounded with an accounting effect (a 174-line denominator
reduction), and the two are separated above so a reaudit cannot mistake one for the other.

---

## 4. Per-member `line-rate`, the four members `[P2-T7]` names

| Type | Member | Before `line-rate` | After `line-rate` | Lines | After `branch-rate` |
|---|---|---|---|---|---|
| `SVGControl.SvgAssemblyProbe` | `PublicKeyTokensEqual(byte[], byte[])` | **0.0000%** (0/15, on `SvgRenderer`) | **100.0000%** | 15/15 | 100.0000% (18/18) |
| `SVGControl.SvgRenderer` | `.ctor(byte[], Size, AutoSize)` | **76.4706%** (13/17) | **100.0000%** | 17/17 | 100.0000% (2/2) |
| `SVGControl.SvgAssemblyResolver` | `ResolveByNameAndKey(object, ResolveEventArgs)` | **68.1159%** (47/69, on `SvgRenderer`) | **58.7500%** | 47/80 | 45.4545% (10/22) |
| `SVGControl.SvgAssemblyResolver` | **`Install()`** — the sole genuinely new member | did not exist | **100.0000%** | 6/6 | 100.0000% (4/4) |

**`SvgAssemblyResolver.Install()` measures 100.0000% `line-rate`, ten points above the `>= 90%`
new-member gate — PASS.** Its `branch-rate` also measures 100.0000% (4/4), better than the 50% the plan
predicted for the `Interlocked.Exchange(...) == 0` false arm; branch-rate is recorded for information
only and is not gated.

`ResolveByNameAndKey` is the only member whose rate fell, and the fall is a pure denominator effect:
**covered lines are unchanged at 47** while the member grew from 69 to 80 measured lines. The +11 lines
are the containment `catch (Exception ex)` clause added by `[P1-T10]`, which is reachable only when an
exception escapes both inner handlers during a live assembly bind — the same host-bound wiring the member's
ratified exception covers. No line of this member lost coverage.

Supporting members, for completeness:

| Member | Before | After | Note |
|---|---|---|---|
| `SvgAssemblyProbe.GetProbeDirectories(string, string, string)` | 23/23 = 100% | **25/25 = 100%** | +2 lines from the `[P1-T11]` `baseDirectory` filter, both covered |
| `SvgAssemblyProbe.TryGetDirectoryFromCodeBase(string)` | 11/11 = 100% | 11/11 = 100% | unchanged |
| `SvgRenderer.ctor(byte[], Size, Padding, AutoSize)` | 18/18 = 100% | 18/18 = 100% | unchanged |
| `SvgRenderer.DescribeFailure(Exception)` | 5/5 = 100% | 5/5 = 100% | unchanged by the `[P1-T2]` accessibility widening |

---

## 5. `SVGControl.SvgAssemblyProbe` remains at 100% line and branch coverage

Stated explicitly as `[P2-T7]` and `remediation-inputs.2026-08-04T20-25.md` § R-3 Verification both
require:

**`SVGControl.SvgAssemblyProbe` measures 102/102 = 100.0000% line coverage and 92/92 = 100.0000% branch
coverage.** It held 100% on both metrics while its denominator grew by 50% — from 68 to 102 class-unit
lines and from 48 to 92 branch conditions — absorbing the relocated `PublicKeyTokensEqual` (15 lines, 18
conditions) and the two lines the `[P1-T11]` `baseDirectory` filter added to `GetProbeDirectories`. All
three of its members are individually at 100% line-rate and 100% branch-rate.

---

## 6. Named exception, re-recorded

Measured percentage: **58.7500% line-rate (47 / 80 lines), 45.4545% branch-rate (10 / 22).**

Untestable wiring: yes — the strategy-3 `Assembly.LoadFrom` block and the new outer containment catch.
All of this member's decision logic lives in `SVGControl.SvgAssemblyProbe`, whose three members all
measure 100% line-rate and 100% branch-rate.

This member is `private static` and is invoked only by the CLR on a failed assembly bind. Driving its
remaining uncovered lines would require staging a real mismatched-key assembly on disk, which
`.claude/rules/general-unit-test.md` UT4 prohibits with zero approved exceptions. No test triggering a
live assembly bind was added, and `[P2-T6]` was not rerun on account of this member.

```
COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey
```

**Cross-reference to the original ratification:** this is the same exception ratified in
`plan.2026-08-04T14-36.md` and recorded in
`evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` § "`ResolveByNameAndKey` named exception" as
`COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgRenderer.ResolveByNameAndKey`. `[P1-T3]` relocated the member
without changing its accessibility, its implementation, or its invocation mechanism, so the exception
travels with it and only its declaring type changed in the identifier. See
`evidence/other/resolver-extraction.2026-08-05T01-50.md` § "Coverage exception travels with the member".

---

## 7. `SVGControl.SvgAssemblyResolver` is a relocation, not a new module

Stated explicitly as `[P2-T7]` requires. The `>= 90%` newly-added-module threshold does **not** attach to
`SVGControl.SvgAssemblyResolver`, and neither does the new-file line-coverage floor, because the file is
the destination of a pure move rather than new behavior:

| Member | Existed at `ea106111` | Figure at `ea106111` |
|---|---|---|
| `_resolverInstalled`, `_resolving` (fields) | yes, on `SvgRenderer` | n/a |
| `ResolveByNameAndKey` | yes, on `SvgRenderer` | 47/69 = 68.1159%, ratified exception applies |
| `Install()` | **no** | the sole genuinely new member — measures **100.0000%**, above the `>= 90%` gate |

The class's aggregate 106/172 = 61.6279% is therefore the arithmetic consequence of hosting one
ratified-exception member (47/80) alongside one fully covered new member (6/6); it is not a new module
entering below a threshold. `evidence/other/resolver-extraction.2026-08-05T01-50.md` records the
move-only evidence: the only three permitted deltas were CSharpier wrapping and two type qualifications.

---

## 8. The `>= 85%` modified-file floor on `SVGControl/SvgRenderer.cs` is NOT targeted this cycle

Stated explicitly as `[P2-T7]` requires.

`SVGControl/SvgRenderer.cs` measures **332 / 414 = 80.1932%** against the 85% modified-file floor. It
does not clear the floor, and **closing that gap is deliberately outside this cycle's scope** per R-4's
explicit scope boundary in `remediation-inputs.2026-08-04T20-25.md`, which directs: "do **not** attempt to
reach 85% on this file in this cycle", and per the binding `## Do Not Do` list, which repeats the same
prohibition.

Progression of the file across the two cycles:

| Point | Figure |
|---|---|
| Pre-issue-#418 baseline | 264 / 422 = 62.559% |
| Issue #418 first cycle end (`ea106111`) | 424 / 588 = 72.1088% |
| **This cycle end** | **332 / 414 = 80.1932%** |

Total improvement from the original baseline: **+17.63 points**, of which **+8.08 points** were delivered
by this cycle. The residual 82 uncovered class-unit lines are `AddMargins` (0/15), `Render()` (18/26),
and the two `SvgDocument` constructor overloads (0/8 each) in method units, plus `get_Margin()` (0/1) and
one line of `AdjustSizeProportionately` (22/23) — every one of them pre-existing code that neither cycle
of issue #418 touched.

**The entry that owns this residual is
`docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`** (created by `[P1-T18]`), which
enumerates every figure above plus the 532 uncovered lines in the rest of the `SVGControl` assembly and
names issue #418 as its origin.

---

## 9. Gate verdicts

| Gate | Value | Threshold | Verdict |
|---|---|---|---|
| Repository line coverage | 85.4097% | `>= 85%` | **PASS** |
| Repository branch coverage | 78.7220% | `>= 75%` | **PASS** |
| No regression on changed lines | no line lost coverage; every retained member's figure is identical or improved | required | **PASS** |
| `SvgAssemblyResolver.Install()` line-rate | 100.0000% | `>= 90%` | **PASS** |
| `SVGControl.SvgAssemblyProbe` line and branch | 100.0000% / 100.0000% | stated requirement | **PASS** |
| Modified-file floor on `SvgRenderer.cs` | 80.1932% | `>= 85%` | **not targeted this cycle** — owned by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` |

No placeholder appears in this artifact; every figure is numeric. No loop restart is required: no
repository-wide floor failed, `Install()` is above 90%, and no changed line lost coverage.

## Output Summary

Repository line coverage rose 85.3890% -> **85.4097%** (93539/109518) and branch 78.5740% -> **78.7220%**
(21584/27418); both floors PASS and both improved. `SVGControl` package 47.0857% -> 48.0181%.
`SvgRenderer` class 72.1088% -> **80.1932%**, with the denominator reduction (−174 class-unit lines from
the `ResolveByNameAndKey` and `PublicKeyTokensEqual` relocations and the shortened static constructor)
separated from the numerator gain (+8 class-unit lines in the three-argument byte-array constructor);
**no line lost coverage**. `PublicKeyTokensEqual` 0% -> **100%** (15/15, branch 18/18). Three-argument
byte-array constructor 76.4706% -> **100%** (17/17). `SvgAssemblyProbe` **100% line and 100% branch**.
`SvgAssemblyResolver.Install()`, the only genuinely new member, **100% line-rate** (PASS vs `>= 90%`).
`COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey` re-recorded;
`SvgAssemblyResolver` is a relocation, not a new module. The 85% modified-file floor on
`SVGControl/SvgRenderer.cs` is not targeted this cycle and is owned by
`docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`. **Repo-wide verdict: PASS.**
