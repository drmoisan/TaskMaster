# Per-File Research — `QuickFiler/Viewers/BreadcrumbPopupPlacement.cs`

- Epic: #136 `quickfiler-per-file-coverage`, child F13, feature issue #455
- Production file: `QuickFiler/Viewers/BreadcrumbPopupPlacement.cs` (87 lines, 413 lines of headroom)
- csproj entry: `QuickFiler/QuickFiler.csproj:401`
- Research date: 2026-08-07
- Builds on: `research/00-cross-cutting-context.md`

---

## 0. Headline and acceptance bar

**This file is at 100% line and 100% branch. There is no coverage work to do. The only real finding
available on it is that branch coverage does not imply boundary-value coverage, and this file
demonstrates that gap concretely: four boundary classes are unrepresented despite every branch being
covered.**

Recomputed from `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(class element at XML line 12218; class-level `<lines>` block 12229-12302; denominator from `<line>`
child count per epic Directive B):

| Metric | Value | Floor | Margin |
| --- | --- | --- | --- |
| Line | **48/48 = 100.00%** | 80% | +20.00 |
| Branch | **12/12 = 100.00%** | 75% | +25.00 |

Both the recomputed values and the `<class>` attributes (`line-rate="1" branch-rate="1"`) agree here —
the #441 double-counting bug is invisible when the rate is exactly 1. The delegating brief's figures
are confirmed.

The brief's estimate of "87 source lines, ~48 instrumented" is exact: **48 instrumented lines**.

---

## 1. Structural map

### 1.1 Two public types

| Lines | Type | Kind |
| --- | --- | --- |
| 8-21 | `BreadcrumbPopupPlacementResult` | **`public readonly struct`** |
| 24-86 | `BreadcrumbPopupPlacement` | **`public static class`** |

Both are `public`, not `internal` — the only two `public` types in this artifact batch. The
`InternalsVisibleTo` grant at `QuickFiler/Properties/AssemblyInfo.cs:5` is therefore **not needed** to
reach them, which matters for §8's recommendation.

`readonly struct` is valid on this project's target framework. (For the record: `init` accessors,
`record`, and `record struct` are **not** available here — net48 lacks `IsExternalInit` and they fail
`CS0518`. This file correctly uses a plain `readonly struct` with getter-only auto-properties and an
explicit constructor.)

### 1.2 Members with line ranges

| Lines | Member | Visibility |
| --- | --- | --- |
| 10-14 | `BreadcrumbPopupPlacementResult(Rectangle bounds, bool opensBelow)` | public ctor |
| 17 | `Rectangle Bounds { get; }` | public |
| 20 | `bool OpensBelow { get; }` | public |
| 30-76 | `static BreadcrumbPopupPlacementResult Calculate(Rectangle anchorScreenBounds, Rectangle workingArea, Size desiredSize)` | public static |
| 78-85 | `static int Clamp(int value, int minimum, int maximum)` | private static |

### 1.3 Purity confirmation — **CONFIRMED PURE GEOMETRY**

`Calculate` is a **pure function**. Verified exhaustively:

- `using` set (`:2-3`) is `System` and `System.Drawing` only. No `System.Windows.Forms`, no
  `Microsoft.Web.WebView2.*`, no `Microsoft.Office.Interop.Outlook`, no `System.Threading`.
- No field, no static state, no `Screen.FromControl`, no `Control`, no handle, no DPI API call, no
  I/O, no logging, no exception.
- Deterministic: identical inputs always yield identical output. No clock, no RNG, no ambient state.
- Total: **no input can make it throw.** Every arithmetic path is unchecked `int` arithmetic and both
  `Math.Max`/`Math.Min` clamps and the `Clamp` helper are total.

There are **no constructor dependencies and no seams**, because there is nothing to inject. This is
exactly the "pure logic separated from I/O" shape that `CLAUDE.md` §1.4 and
`.claude/rules/general-code-change.md` ask for, and it should be cited in the ledger as the
positive exemplar for the rest of F13.

### 1.4 Cobertura topology — confirms the brief and reinforces the harness directive

The report emits **exactly one `<class>` element** for this file:

```
<class line-rate="1" branch-rate="1" complexity="13"
       name="QuickFiler.Viewers.BreadcrumbPopupPlacementResult"
       filename="QuickFiler\Viewers\BreadcrumbPopupPlacement.cs">
```

The brief's warning is confirmed: **the writer names the class after the struct, not after the static
class that carries all the logic.** A grep for `name="QuickFiler.Viewers.BreadcrumbPopupPlacement"`
returns no matches.

Its `<methods>` block (XML 12219-12228) contains **only the struct's `.ctor`** (source lines 11-14).
Its class-level `<lines>` block (XML 12229-12302) contains **both** types' lines — 11-14 *and* 35-85.

This is the second independent instance of the same topology in this batch (the first is
`BreadcrumbWebViewSurfaceFactory.cs`; see artifact `06-…` §1.4). The binding harness directives:

1. **Key on `filename`, never on `<class name>`.** A name-keyed reader reports
   `BreadcrumbPopupPlacement` as absent/0%.
2. **Sum the class-level `<lines>` block, never the `<method>` blocks.** Summing `<method>` children
   here would report 4 lines instead of 48 — a **91.7% undercount** on a fully covered file.

Note also that the property getters at `:17` and `:20` produce **no instrumented lines at all** (they
are auto-property getters; the `<lines>` block jumps from 14 straight to 35). They are not a gap.

---

## 2. Branch inventory

### 2.1 Complete conditional inventory — all covered

| file:line | Construct | `condition-coverage` | Which existing test covers each outcome |
| --- | --- | --- | --- |
| `:52` | `if (desiredHeight <= belowSpace)` | `100% (2/2)` | true → `Calculate_FullHeightFitsBelow_PrefersBelow` (`BreadcrumbPopupPlacementTests.cs:15`); false → `:30`, `:45`, `:60`, `:124` |
| `:56` | `else if (desiredHeight <= aboveSpace)` | `100% (2/2)` | true → `Calculate_BelowInsufficientAndFullHeightFitsAbove_UsesAbove` (`:30`); false → `Calculate_NeitherFits_UsesGreaterAvailableSideAndClampsHeight` (`:45`), `Calculate_EqualSpaceTie_PrefersBelowAndClampsHeight` (`:60`), `Calculate_ZeroWorkingArea_ProducesZeroSizeAtWorkingOrigin` (`:124`) |
| `:66` | ternary `opensBelow ? belowSpace : aboveSpace` | `100% (2/2)` | both arms via the tests above |
| `:68` | ternary `opensBelow ? anchorScreenBounds.Bottom : anchorScreenBounds.Top - height` | `100% (2/2)` | both arms |
| `:80` | `if (value < minimum)` in `Clamp` | `100% (2/2)` | true → `Calculate_AnchorOutsideVerticalBounds_ClampsLocation` (`:109`, `proposedY = -25 < workingArea.Top = 0`); false → every other test |
| `:84` | `return value > maximum ? maximum : value;` | `100% (2/2)` | true → `Calculate_RightEdgeAndOversizeWidth_ClampsLocationAndSize` (`:75`, `x = Clamp(750, 0, 500) → 500`); false → `Calculate_FullHeightFitsBelow_PrefersBelow` |

`:62` (`opensBelow = belowSpace >= aboveSpace;`) is correctly reported `branch="False"` — a comparison
producing a bool, not a jump.

No `if`/`else` beyond the above, no `switch`, no `??`, no `?.`, no `&&`/`||`, no `catch` filter, no
loop, no pattern match anywhere in the file.

### 2.2 Uncovered conditions: **none**

There are zero uncovered lines and zero uncovered branch outcomes. The nested-lambda
`[ExcludeFromCodeCoverage]` instrumentation defect is **not applicable** — the file carries no such
attribute (grep: zero occurrences) and contains no lambda.

---

## 3. Boundary-value analysis — the only real finding on this file

**Branch coverage does not imply boundary-value coverage.** All 12 branch outcomes are covered by 8
existing tests, yet four boundary classes are unrepresented. Each is a surviving mutant: a small,
plausible edit to the production code that no current test would catch.

### 3.1 Boundary classes represented by the existing tests

| Boundary | Covered by | Concrete values |
| --- | --- | --- |
| Full height fits below | `:14-27` | anchor `(100,100,200,25)`, wa `(0,0,800,600)`, desired `300×200` → `(100,125,300,200)`, below |
| Below insufficient, fits above | `:29-42` | anchor `(100,400,200,25)` → `(100,100,300,300)`, above |
| Neither side fits, greater side wins, height clamped | `:44-57` | wa `(0,0,800,500)`, desired `300×400` → `(100,0,300,300)`, above |
| **Exact tie** `belowSpace == aboveSpace` at `:62` | `:59-72` | both spaces `225` → below wins, height clamped to `225` |
| Right screen edge, x clamped to `maximum` | `:74-92` | anchor `(750,100,40,25)` → x `500` |
| Oversize width clamped to working width | `:74-92` | desired width `1000` → width `800`, x `0` |
| **Negative-coordinate monitor** (multi-monitor left of primary) | `:94-106` | wa `(-1920,0,1920,1080)` → `(-500,125,500,300)` |
| Anchor above the working area, y clamped to `minimum` | `:108-121` | anchor `(100,-50,80,25)`, proposed y `-25` → `0`. Also the **only** test that exercises the `Math.Min(workingHeight, …)` clamp at `:43-46` (raw belowSpace 625 → clamped to 600). |
| **Zero-size working area** | `:123-136` | wa `(-10,-20,0,0)` → `(-10,-20,0,0)`, below by the tie rule |

That is a genuinely good boundary suite for a file of this size. The gaps below are the residue.

### 3.2 Unrepresented boundary classes — four surviving mutants

| # | Boundary | file:line | Why no current test catches it | Surviving mutation |
| --- | --- | --- | --- | --- |
| **B1** | **Exact-fit equality** `desiredHeight == belowSpace` and `desiredHeight == aboveSpace` | `:52`, `:56` | Every test uses a strict inequality. T1 has `200 <= 475`; T2 has `300 <= 400`. Nothing sits *on* the boundary. | Changing `<=` to `<` at `:52` or `:56` changes behaviour only when the values are exactly equal, so **the whole existing suite still passes**. A popup whose desired height exactly equals the available space would flip from "opens below at full height" to "opens above" (or to the tie rule). |
| **B2** | **Negative `Size` / negative `Rectangle` dimensions** | `:36-37` (`Math.Max(0, workingArea.Width/Height)`), `:40-41` (`Math.Max(0, desiredSize.Width/Height)`) | `Calculate_ZeroWorkingArea` uses **zero**, not negative. `Rectangle` and `Size` both permit negative `Width`/`Height` and neither validates. | Removing `Math.Max(0, …)` at `:36`, `:37`, `:40` or `:41` is invisible to the suite, because zero is a fixed point of `Math.Max(0, ·)`. With a negative working width the result would acquire a negative `workingRight`, producing an inverted `Clamp` range at `:67`. |
| **B3** | **The `aboveSpace` `Math.Min(workingHeight, …)` clamp** | `:47-50` | The clamp binds only when `anchorScreenBounds.Top - workingArea.Top > workingHeight`, i.e. the anchor sits *below* the working area. `Calculate_AnchorOutsideVerticalBounds` exercises the *symmetric* clamp at `:43-46` (anchor above), but nothing exercises this one. | Deleting `Math.Min(workingHeight, …)` at `:47` leaves the whole suite green. An anchor below the working area would then report an above-space larger than the screen and could be assigned a height exceeding the working height. |
| **B4** | **Zero desired size** `Size(0,0)` | `:40-41`, `:65-66` | Never supplied. Distinct from B2 (zero working area with a non-zero desired size, which *is* tested at `:123-136`). | Weak on its own; included because a zero-size popup is a plausible degenerate input from a not-yet-laid-out control and the function is `public`. Note that `BreadcrumbDropDownOpenLifetime.ValidatePlacement` (`:278`) exists downstream, so a degenerate result is presumably rejected there — worth confirming when that file is researched. |

### 3.3 Boundary classes deliberately excluded from the recommendation

| Boundary | file:line | Assessment |
| --- | --- | --- |
| **Integer overflow** | `:38`, `:39`, `:45`, `:49`, `:67`, `:70`, `:71` | All are unchecked `int` arithmetic and would wrap silently at extreme magnitudes (e.g. `workingArea.Left + workingWidth` near `int.MaxValue`). **Not worth a test.** `Rectangle`/`Size` do not constrain their values, but the *only* production caller is `BreadcrumbPopupUiOperations.PlaceSurfaceAsync` (`:193-221`), which supplies real screen geometry and a real control size. An overflow test would pin behaviour nobody wants to guarantee and would document wrap-around as a contract. Record it in the ledger as a known, accepted limitation instead. |
| **Inverted `Clamp` range** (`minimum > maximum`) | `:78-85` | **Unreachable by construction**, and worth stating because `Clamp` would silently return `minimum` (a value greater than `maximum`) if it ever happened. Proof: `width = Math.Min(desiredWidth, workingWidth)` (`:65`) so `workingRight - width = workingArea.Left + workingWidth - width >= workingArea.Left`; and `height = Math.Min(desiredHeight, belowSpace|aboveSpace)` (`:66`) where both spaces are `Math.Min(workingHeight, …)` (`:43-50`), so `workingBottom - height >= workingArea.Top`. Both `Clamp` calls therefore always receive `minimum <= maximum`. **B2 is the input class that would break this proof** (a negative `workingArea.Width` survives `Math.Max` only because `Math.Max` is there), which is a second reason to add B2. |
| **DPI scaling** | — | **Not applicable to this file.** There is no DPI logic, no `AutoScaleMode`, no `DeviceDpi` read, no scale factor. The method operates on already-scaled pixel `Rectangle`/`Size` values supplied by the caller. The DPI concern for the breadcrumb feature lives upstream in the callers (and the sibling artifact records an unscaled-`ColumnHeader`-width DPI defect in a different feature entirely). **Do not add a "DPI" test here** — it would be a test of the caller wearing this file's name. |
| **Multi-monitor to the right of the primary** | — | Symmetric to the negative-origin case already covered at `:94-106`. Adding it would be a near-duplicate; the arithmetic is sign-agnostic. |

---

## 4. Concurrency, ordering, and time

**None. At all.**

Exhaustively: no `CancellationToken`, no `lock`, no `Interlocked`, no `Volatile`, no `SemaphoreSlim`,
no `Task`, no `TaskCompletionSource`, no `async`/`await`, no `async void`, no timer, no wall-clock
read, no timeout, no `SynchronizationContext`, no thread-affinity assumption, no static mutable state.

`Calculate` is thread-safe by construction (pure, no shared state) and can be called from any thread,
including concurrently, with no synchronisation. Under
`scripts/vscode/TaskMaster.cli.runsettings` (`Parallelize Workers=0 Scope=ClassLevel`) this file is
completely safe to test from parallel test classes.

**No injected clock or `TimeProvider` seam exists, and none could be relevant** — the function reads
no clock.

**Deterministic mechanism required by any untested path: none.** Every recommended test in §8 is a
direct synchronous call with literal `Rectangle`/`Size` arguments and a single equality assertion.
There is no sleep, no timer, no fake, no mock, no form, no popup, and no file.

---

## 5. Error paths

**There are none.**

| Category | Count | Detail |
| --- | --- | --- |
| `throw` statements | **0** | The file throws nothing. Grep confirms zero occurrences of `throw`. |
| `catch` blocks | **0** | No `try` anywhere. No bare `catch {}`; the sibling-recorded `BreadcrumbPopupUiOperations.cs:349` / `BreadcrumbDropDownOpenLifetime.cs:197` finding does not extend here. |
| Guard clauses | **0** | `Rectangle` and `Size` are value types and cannot be null; `Calculate` accepts every representable value. |
| Early-return null checks | **0** | The method returns a value type. |
| Logged-and-swallowed exceptions | **0** | No logger is referenced; the file does not import log4net. |

This is deliberate and correct for a total pure function. `CLAUDE.md` §3 "fail fast and explicitly"
is satisfied vacuously: there is no invalid input, because every input has a defined, clamped result.

**Nothing needs a seam. Not an interface, not an injectable delegate, not an adapter.**

The one thing worth recording: because the function is *total*, callers get a silently degenerate
result (a zero-size `Rectangle`) rather than an exception when the geometry is nonsensical. The
downstream guard is `BreadcrumbDropDownOpenLifetime.ValidatePlacement` (`:278`), consumed at `:226`
and `:257`. That file is F13-owned but outside this artifact's batch; whoever researches it should
confirm the guard actually rejects a zero-size placement.

---

## 6. Coupling to sibling-owned files

### 6.1 Production coupling — **none outbound**

| Direction | File:line | Coupling |
| --- | --- | --- |
| we → anyone | **none** | The file's entire dependency set is `System` and `System.Drawing`. It references no QuickFiler type at all. |
| F13 → us | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:206` | `BreadcrumbPopupPlacement.Calculate(anchorBounds, workingArea, desiredSize)` inside `PlaceSurfaceAsync` (`:193-221`), which re-checks `isCurrent()` between each control mutation. Same child. |
| F13 → us | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:226, 257, 278` | Consumes `BreadcrumbPopupPlacementResult?`; `ValidatePlacement` at `:278`. Same child. |
| F12/F14 → us | **none** | No F12- or F14-owned file references either type. Neither `BreadcrumbPopupLifecycleOperations` (`BreadcrumbItemViewerLifecycleCoordinator.cs:355`) nor `BreadcrumbNavigationSubscription` (`:337`) touches placement, so F12's expected split of that 481-line file is irrelevant to this file. |

### 6.2 Test coupling — **one real cross-child hazard**

`QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs:140`:

```csharp
Type type = typeof(BreadcrumbBridgeCoordinator).Assembly.GetType(
    "QuickFiler.Viewers.BreadcrumbPopupPlacement",
    false
);
```

`BreadcrumbBridgeCoordinator` (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`, 487 lines) is
**F12-owned** (`epic.md:409-412`). It is used here purely as an assembly anchor for reflection — the
test does not exercise it. Two consequences:

1. **If F12 moves, renames, or namespace-changes `BreadcrumbBridgeCoordinator`, this F13 test file
   stops compiling.** That is a genuine cross-child compile coupling created by an incidental
   reflection anchor, and it is invisible to a file-set disjointness check.
2. The reflection is **unnecessary**: `BreadcrumbPopupPlacement` and `BreadcrumbPopupPlacementResult`
   are both `public` (`:8`, `:24`), so the tests can call `BreadcrumbPopupPlacement.Calculate(...)`
   directly and read `.Bounds` / `.OpensBelow` with full compile-time checking. The reflection is a
   leftover from #400's failure-first authoring (the file's doc comment at `:10` reads
   "Failure-first pure popup placement contracts for issue #400" — the type did not exist when the
   test was written).

**Recommendation: the new test file in §8 must call the API directly and must not take a dependency
on any F12-owned type.** Rewriting the *existing* file's reflection is a defensible cleanup but is
out of the coverage mandate; record it as observation O-B in §10 and leave the decision to the
planner.

---

## 7. Existing test inventory

| Test file | Lines | Headroom | What it asserts about this file |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs` | **169** | **331** | The dedicated fixture — the only file in this batch that has one. Eight `[TestMethod]`s at `:15, 30, 45, 60, 75, 95, 109, 124`, enumerated in §3.1. Private `Calculate(...)` reflection helper at `:138-155`; private `Placement` DTO at `:157-167`. All assertions are exact-`Rectangle` equality via FluentAssertions `Should().Be(...)`. |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 461 | 39 | `:99` consumes `Task<BreadcrumbPopupPlacementResult?>` from `operations.PlaceSurfaceAsync(...)`. Exercises the struct indirectly through `BreadcrumbPopupUiOperations`; contributes no additional `Calculate` coverage. |

**This is the only file in the four-file batch whose owner fixture has real headroom (331 lines).**
That makes it the one file where adding cases to the existing file is *mechanically* possible.

However, `BreadcrumbPopupPlacementTests.cs` funnels every test through the reflection helper at
`:138-155`. Adding direct-call tests to that file would mix two invocation styles in one fixture, and
adding reflection-style tests would propagate the F12 anchor coupling of §6.2. **Recommend a new
file.** This is a judgement call, not a hard constraint: the sibling artifact's 500-line pressure
finding does not bind here.

---

## 8. Recommended test-case list

**No new test is required to satisfy any coverage gate.** The file is at 100%/100% and every
recommended case below adds **zero** coverage percentage. They are recommended on mutation-resistance
grounds only — they close the four surviving mutants of §3.2 and are genuine behavioural assertions,
not shape assertions, so they do not fall under the `epic.md:521-522` prohibition.

Target file: **`QuickFiler.Test/Viewers/BreadcrumbPopupPlacementBoundaryTests.cs`** (new).
Must call `BreadcrumbPopupPlacement.Calculate(...)` **directly** (both types are `public`), with no
reflection and no reference to any F12-owned type (§6.2).

| # | Test name | Closes | Arrange / Act / Assert | Coverage delta | One atomic task? |
| --- | --- | --- | --- | --- | --- |
| P1 | `Calculate_DesiredHeightExactlyEqualsBelowSpace_OpensBelowAtFullHeight` | B1 (`:52`) | anchor `(100,100,200,25)`, wa `(0,0,800,600)`, desired `300×475` (belowSpace is exactly `600-125 = 475`); assert `OpensBelow == true` and `Bounds == (100,125,300,475)`. Kills the `<=` → `<` mutant at `:52`. | 0 | Yes |
| P2 | `Calculate_DesiredHeightExactlyEqualsAboveSpace_OpensAboveAtFullHeight` | B1 (`:56`) | anchor `(100,400,200,25)`, wa `(0,0,800,600)`, desired `300×400` (belowSpace `175`, aboveSpace exactly `400`); assert `OpensBelow == false` and `Bounds == (100,0,300,400)`. Kills the `<=` → `<` mutant at `:56`. | 0 | Yes |
| P3 | `Calculate_NegativeWorkingAreaAndDesiredDimensions_ClampToZero` | B2 (`:36-37`, `:40-41`) | wa `(10,20,-5,-7)`, desired `(-3,-9)`, any anchor; assert the result is a zero-size rectangle at the working-area origin and `OpensBelow == true` (the tie rule with both spaces zero). Kills all four `Math.Max(0, …)` mutants and protects the `Clamp`-range invariant proved in §3.3. | 0 | Yes |
| P4 | `Calculate_AnchorBelowWorkingArea_ClampsAboveSpaceToWorkingHeight` | B3 (`:47-50`) | anchor `(100,900,200,25)`, wa `(0,0,800,300)`, desired `300×400`; raw above-space is `900 - 0 = 900` and must clamp to `workingHeight = 300`; assert `OpensBelow == false` and `Bounds.Height == 300` with `Bounds.Top >= workingArea.Top`. Kills the `Math.Min` deletion mutant at `:47`. | 0 | Yes |
| P5 (optional) | `Calculate_ZeroDesiredSize_ReturnsZeroSizeBelowAnchor` | B4 | anchor `(100,100,200,25)`, wa `(0,0,800,600)`, desired `(0,0)`; assert `OpensBelow == true` and `Bounds == (100,125,0,0)`. | 0 | Yes |

All five are MSTest `[TestMethod]`s with FluentAssertions exact-`Rectangle` equality, no Moq needed
(nothing to mock), deterministic, independent, isolated, no temp file, no live form, no popup, no
sleep, no external service. Each is one atomic plan task.

**Explicitly NOT recommended:**

- **No overflow test** (§3.3) — it would document wrap-around as a contract.
- **No DPI test** (§3.3) — this file contains no DPI logic; such a test would belong to the caller.
- **No right-of-primary multi-monitor test** — a near-duplicate of the covered negative-origin case.
- **No shape-assertion / reflection-existence test.** The existing `:144` assertion
  `type.Should().NotBeNull("issue #400 requires a pure popup placement calculator")` is already of
  that kind; do not add more.
- **No test for the property getters** at `:17`/`:20` — they emit no instrumented lines and are
  exercised by every assertion.

**Honest bottom line:** if the child's change budget is constrained, this file needs **nothing**. P1
and P2 are the highest value per unit of effort (two equality-boundary mutants, four lines of arrange
each); P3 and P4 are worthwhile but secondary; P5 is marginal. **The correct plan entry if the budget
is tight is retain-and-verify only.**

---

## 9. csproj impact

- **`QuickFiler/QuickFiler.csproj`: no change.** No new production file. Existing entry at `:401`,
  inside the contiguous F13 block `:396-411` (F12-owned entries interleave at `:393-395` and `:400`;
  fan-in conflicts there are additive and resolved by keeping both sides, `epic.md:594-617`).
- **`QuickFiler.Test/QuickFiler.Test.csproj`: one new line** if §8 is taken. Insert
  `    <Compile Include="Viewers\BreadcrumbPopupPlacementBoundaryTests.cs" />`
  adjacent to `:73` (`Viewers\BreadcrumbPopupPlacementTests.cs`), inside the breadcrumb block at
  `:60-89`.
- **CRLF must be preserved.** Both projects are non-SDK with explicit compile lists and CRLF line
  endings. Use the `Edit` tool or `perl -0777` with explicit `\r\n`. A git-bash `sed -i` strips CRLF
  and produces a whole-file diff guaranteed to conflict at fan-in (`epic.md:610-612`).
- **Coverage ledger:** update the existing `testable` row with the measured 100%/100% and a note that
  the file is the pure-logic exemplar for the child. **No new row** — no new production file, so the
  `>= 90%` new-file rule does not engage.

---

## 10. Latent defects

**No production defect found in this file.** It is a total, pure, thread-safe function with no error
paths, no state, and no dependencies. None of the sibling-recorded defects extends here: no lock (so
not the `BreadcrumbDropDownOpenCoordinator.cs:95` lock-ordering issue), no null-forgiving dereference
(so not the `BreadcrumbDropDownOpenLifetime.cs:229-230` issue), no `catch` at all (so neither bare
`catch {}` finding), and no `[ExcludeFromCodeCoverage]` and no lambda (so not the nested-lambda
instrumentation defect).

Three observations, none warranting issue promotion:

| ID | file:line | Observation | Why not promoted |
| --- | --- | --- | --- |
| O-A | `:38`, `:39`, `:45`, `:49`, `:67`, `:70`, `:71` | Unchecked `int` arithmetic wraps silently at extreme magnitudes. `Rectangle`/`Size` impose no bound, and the method is `public`. | Unreachable from the only production caller (`BreadcrumbPopupUiOperations.cs:206`), which supplies real screen geometry. A `checked` block would be a behaviour change under a no-behaviour-change epic. Record as an accepted limitation in the ledger. |
| O-B | `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs:138-155` | The fixture invokes a **`public`** API through `Assembly.GetType(string)` + `MethodInfo.Invoke` + `GetProperty(...).GetValue(...)`, anchored on the **F12-owned** `BreadcrumbBridgeCoordinator` (`:140`). Costs: no compile-time checking (a rename becomes a runtime `NotBeNull` failure with a poor message instead of a build error), a boxed round-trip through `object`, and a cross-child compile coupling that a file-set disjointness check cannot see. | A **test-quality** issue in existing F13-owned test code, not a production defect. On the F4 precedent (`epic.md:556-558`, where test-policy violations in existing tests were held in-scope for the owning child's execution), this is **in-scope for F13 to fix** if the planner wants it — a mechanical rewrite of `:138-155` to a direct call, deleting the `Placement` DTO at `:157-167` and the F12 anchor. It buys zero coverage; its value is removing the cross-child coupling. Recommend as a low-priority in-scope cleanup rather than a promoted issue. |
| O-C | `:78-85` | `Clamp` returns `minimum` when `minimum > maximum`, i.e. it silently violates its own upper bound on an inverted range. Proved unreachable today (§3.3), but the proof depends on the `Math.Max(0, …)` guards at `:36-37`, which are themselves untested (B2). | Not currently a defect. Test P3 converts the proof into a regression test, which is the cheapest mitigation. No production change recommended. |

---

## 11. Deviations from the delegation brief

| Brief statement | Finding |
| --- | --- |
| "`BreadcrumbPopupPlacement.cs` (87 source lines, ~48 instrumented, fully covered on both axes)" | **Confirmed exactly.** 87 source lines, 48 instrumented, 48/48 line, 12/12 branch. |
| "Confirm it is pure geometry" | **Confirmed.** `using System; using System.Drawing;` only; no state, no I/O, no logging, no exception, no clock, total function (§1.3). |
| "the Cobertura writer emits its class as `…BreadcrumbPopupPlacementResult`" | **Confirmed**, with the additional detail that the `<methods>` block contains *only* the struct's `.ctor` (4 lines) while the class-level `<lines>` block carries all 48 — a **91.7% undercount** for any harness that sums `<method>` blocks (§1.4). |
| "report whether any boundary case is unrepresented in tests even though every branch is nominally covered" | **Four are** (§3.2): B1 exact-fit equality at `:52`/`:56`; B2 negative `Size`/`Rectangle` dimensions at `:36-37`/`:40-41`; B3 the `aboveSpace` `Math.Min` clamp at `:47-50`; B4 zero desired size. Each is a surviving mutant. |
| "enumerate … DPI scaling" | **Refuted as applicable.** There is no DPI logic in this file; it consumes already-scaled pixel geometry. A DPI test here would test the caller. |
| "enumerate … overflow" | **Enumerated but deliberately excluded** from the recommendation (§3.3) — testing it would document wrap-around as a contract, and the only production caller cannot reach it. |
| "each with a named target test file under `QuickFiler.Test/Viewers/`" | `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementBoundaryTests.cs`. Note the sibling artifact's 500-line pressure **does not bind here** — the existing `BreadcrumbPopupPlacementTests.cs` is 169 lines with 331 of headroom. A new file is recommended for a different reason: to avoid propagating the reflection style and the F12 assembly anchor (§6.2). |
| "thirteen F13-relevant test files are within 25 lines of the 500-line limit" | **Confirmed as a general constraint, but not for this file.** Of the two fixtures touching it, `BreadcrumbPopupPlacementTests.cs` has 331 lines of headroom and `BreadcrumbSelectorOpenRetryTests.cs` has 39. |

---

*No commands were executed in this session; all findings are derived from the working-tree files and
the committed Cobertura report cited in §0, with exact paths and line numbers given throughout.*
