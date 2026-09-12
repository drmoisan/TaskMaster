# Research — `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs`

- Feature: `quickfiler-itemviewer-coverage` (issue #456), epic child F14 of `quickfiler-per-file-coverage` (#136)
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5e4b635834feedd7`
- Produced: 2026-08-07T21-40
- Scope: one production file — `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` (821 lines, generated)

---

## Recommendation (Q3) — one line

**Classify `ItemViewerExpanded.Designer.cs` as `testable` in F1's ledger and leave it measured. Do not
mark it `ratified-exempt`. Add one test that disposes the control with `disposing == false`; that
single test lifts branch coverage from 50% to 75%, the maximum the file can structurally reach, and
clears the gate. Do not add `[ExcludeFromCodeCoverage]` — it is mechanically impossible to apply it to
this file alone.**

| Gate | Now | After one test | Structural maximum | Verdict |
| --- | --- | --- | --- | --- |
| Line >= 80% | ~98.5–99.5% | unchanged | 98.5% (3 lines are dead code) | **passes today** |
| Branch >= 75% | 50% (2/4) | **75% (3/4)** | 75% (the 4th outcome is unreachable) | **fails today, passes after one test** |

---

## 1. Verified baseline

`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml:4112`:

```xml
<class line-rate="0.9950980392156863" branch-rate="0.5" complexity="4"
       name="QuickFiler.ItemViewerExpanded" filename="QuickFiler\Viewers\ItemViewerExpanded.Designer.cs">
```

Three methods, read directly from the report (XML `:4113-4166+`):

| Method | Report line-rate | Lines | Detail |
| --- | --- | --- | --- |
| `.ctor()` | 1 | source line 8 | `private System.ComponentModel.IContainer components = null;` — the field initialiser, hit |
| `Dispose(bool)` | 0.5714 (4/7) | 15, 16, 17, 18, 19, 20, 21 | **17, 18, 19 uncovered**; 15, 16, 20, 21 covered. Line 16 carries `condition-coverage="50% (2/4)"` with two `<condition type="jump">` entries, each at 50% |
| `InitializeComponent()` | 1 | 30 onward | fully covered — every generated construction and property-assignment line |

Note that `line-rate="0.9950980392156863"` (= 203/204) is not arithmetically reconcilable with the
three uncovered lines the same element enumerates. This is the same defect documented in **open issue
#441** ("Cobertura post-processing double-counts `<line>` nodes… class-level `line-rate` attributes are
consequently incorrect") and is analysed at length in the sibling artifact
`research.itemviewerexpanded-cs.2026-08-07T21-40.md` § 1.3. The `branch-rate="0.5"` figure **is**
consistent with the per-condition data (2 of 4 outcomes) and is used as authoritative below. The
line figure is quoted as a range (98.5% recomputed from the method blocks vs 99.5% as attributed);
both clear the 80% floor by a wide margin, so the discrepancy does not affect the recommendation.

---

## 2. Which test constructs it (Q3a)

**No test constructs `ItemViewerExpanded` directly.** The sole construction site in the entire solution
is `QuickFiler/Viewers/QfcFormViewer.Designer.cs:42`:

```csharp
this._qfcItemViewerExpandedTemplate = new QuickFiler.ItemViewerExpanded();
```

`QfcFormViewer` is constructed in production at `QuickFiler/Controllers/QfcHomeController.cs:93`
(`Init()`) and `:133` (`InitAsync(...)`), and both are exercised by
`QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` — `Init_InitializesCorrectly` (`:114`, acts at
`:149`) and `InitAsync_InitializesCorrectly` (`:179`, acts at `:220`).

The chain is: test → `QfcHomeController.Init/InitAsync` → `new QfcFormViewer()` →
`QfcFormViewer.InitializeComponent()` → `QfcFormViewer.Designer.cs:42 new ItemViewerExpanded()` →
`ItemViewerExpanded.InitializeComponent()`.

Both tests are plain `[TestMethod]` in a plain `[TestClass]` (`QfcHomeControllerTests.cs:22`, `:113`,
`:178`) — no STA attribute, no message pump, no shown window. The assembly initializer
(`QuickFiler.Test/SetupAssemblyInitializer.cs:14-20`) only calls `Application.EnableVisualStyles()`
and `Application.SetCompatibleTextRenderingDefault(false)`.

**This is the empirical proof the delegation brief asked for: a generated WinForms designer partial
reaches near-total line coverage purely as a side effect of the control being constructed once in an
existing test — no designer-specific test, no STA scoping, and no seam.** It holds even for a designer
that constructs a `Microsoft.Web.WebView2.WinForms.WebView2` (`ItemViewerExpanded.Designer.cs:44`), a
`MenuStrip` (`:34`), a `ComboBox` (`:41`), four `ToolStripMenuItemCb` (`:36-39`), a
`BrightIdeasSoftware.FastObjectListView` (`Designer.cs:816`), and a `ComponentResourceManager` reading
the embedded `ItemViewerExpanded.resx` (`:31`, resource declared at `QuickFiler.csproj:498`).

**Caveat the plan must absorb:** the construction is incidental and F7-owned. `QfcHomeController.cs` is
epic child F7's file, and F7's research already flagged `:133` as a live-form line. If F7 replaces it
with a seam, this designer's 99.5% collapses. F14 must own its own construction fixture rather than
inherit F7's. See § 6 CC-1.

---

## 3. Why line coverage is 99.5% but branch coverage is 50% (Q3b)

### 3.1 The only branch in the file

A generated WinForms designer partial contains exactly one conditional: the disposal guard. Here it is
`ItemViewerExpanded.Designer.cs:14-21`:

```csharp
protected override void Dispose(bool disposing)
{
    if (disposing && (components != null))   // :16 — two jumps, four outcomes
    {
        components.Dispose();                // :18
    }
    base.Dispose(disposing);                 // :20
}
```

`complexity="4"` for the whole class is entirely this method. `InitializeComponent()` — the other ~750
source lines — is straight-line construction and property assignment with **zero** branches, which is
why it reports `line-rate="1"` and `branch-rate="1"`. Line coverage is therefore governed by
"was the control constructed at all" (yes → ~100%), while branch coverage is governed solely by "which
disposal paths ran" (one of three reachable → 50%).

### 3.2 The fourth outcome is dead code — verified

`components` is declared and initialised at `ItemViewerExpanded.Designer.cs:8`:

```csharp
private System.ComponentModel.IContainer components = null;
```

A search of the whole file for `components` returns exactly three hits: the declaration at `:8`, the
guard at `:16`, and the call at `:18`. **`components` is never assigned a non-null value anywhere.**
The generated `InitializeComponent()` for this control does not create a
`System.ComponentModel.Container` because no component on the design surface required one.

Therefore:

- `components != null` is **permanently false**;
- source lines **17, 18, 19 are unreachable dead code** — they can never execute, at any point in the
  product's life;
- of the four branch outcomes on line 16, **only three are reachable**: (a) `disposing == false`
  (short-circuit — jump 0 false), (b) `disposing == true` (jump 0 true) followed by (c)
  `components != null` evaluating false (jump 1 false). Outcome (d), `components != null` true, is
  unreachable.

**Maximum achievable branch coverage for this file is 3/4 = 75.00%** — exactly the floor. Maximum
achievable line coverage is 201/204 ≈ 98.5%.

### 3.3 Which two outcomes are covered today — deduced from the data

The report shows **both** jumps at 50%. That is only consistent with `Dispose(true)` having run:
`Dispose(true)` takes jump 0's true path (1 of 2) and then evaluates jump 1, taking its false path
(1 of 2) — 50% on each, 2/4 overall. If instead only `Dispose(false)` had run (the finalizer path),
jump 0 would read 50% and jump 1 would read **0%**, because jump 1 would never be evaluated. The
report does not show that.

**Conclusion: `Dispose(true)` runs in the current suite; `Dispose(false)` does not. The single missing
reachable outcome is jump 0's false path — `disposing == false`.**

The caller of `Dispose(true)` was not identified: `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`
contains no `Dispose` call (verified by search) and `QuickFiler/Viewers/QfcFormViewer.cs` contains no
`Dispose` override (verified by search). The disposal therefore originates elsewhere in the test run —
most plausibly a parent control's disposal cascade in another test in the same assembly. **This is a
determinism hazard**: today's 50% branch figure depends on an unpinned, incidental disposal in a
different test. The plan must pin both disposal paths with explicit tests rather than inherit the
current figure. See LD-2.

---

## 4. Classification argument (Q3c) — why `testable`, not `ratified-exempt`

### 4.1 The mechanical argument is decisive

`[ExcludeFromCodeCoverage]` is a **type-level** attribute, and `ItemViewerExpanded` is a partial type
spread across `ItemViewerExpanded.cs` and `ItemViewerExpanded.Designer.cs`. Applying the attribute to
the designer partial exempts the **whole type**, including the hand-written partial that F14 is
required to bring to 80%/75%. Applying it to both partials is CS0579 (duplicate attribute). This is
the same constraint `issue.md:50-52` records for `ItemViewer`.

There is therefore **no mechanism** to exempt this file independently, other than a `coverage.config`
file-level exclude — and `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy makes any
`exclude` entry matching a production source path a **Blocking** finding for feature-review. Both
routes to `ratified-exempt` are closed.

### 4.2 The substantive argument reaches the same conclusion

Even if the attribute could be applied, it should not be. Exemption exists for the irreducible
untestable remainder. This file is not that: it already **passes the line gate at ~98.5–99.5%** with no
test written for it, and reaches its structural branch maximum of 75% — a passing figure — with one
small test. Claiming an exemption for a file that demonstrably passes both gates would be a fabricated
exemption, and per the epic's ratified reconciliation ("refactor first, exempt only the irreducible
remainder") an `[ExcludeFromCodeCoverage]` on something testable is itself a Blocking finding.

The epic's file inventory labels this file an "exempt-candidate" (`epic.md:431`). **That premise is
disproved by the measurement.** The file does not need an exemption and cannot be given one in
isolation. Recommend F1's ledger record it as `testable` with a note that its line coverage is
structurally capped at 98.5% and its branch coverage at 75% by dead generated code.

### 4.3 The 500-line rule

821 lines. Generated `*.Designer.cs` files are exempt from the 500-line limit as generated code
(epic § "Shared Design" 5; `.claude/rules/general-code-change.md` § File Size Limit). **No action, no
split, no edit to this file.**

---

## 5. Test plan for this file (Q6, scoped to what § 3 concludes is warranted)

Exactly **one** test is warranted, plus one companion that pins the currently-incidental path.
Both belong in the construction fixture recommended for `ItemViewerExpanded.cs`
(`QuickFiler.Test/Viewers/ItemViewerExpanded.StaTests.cs`, `[STATestClass]`).

| # | Test name | Production lines / outcomes | Mechanism | Mocks |
| --- | --- | --- | --- | --- |
| D1 | `Dispose_WhenDisposingIsFalse_SkipsComponentDisposalAndCallsBase` | **branch outcome jump-0-false on `Designer.cs:16`** — the gating outcome; lines 15, 20, 21 | a test-local subclass `private sealed class DisposableProbe : ItemViewerExpanded { internal void DisposeUnmanagedOnly() => base.Dispose(false); }` — `Dispose(bool)` is `protected`, so a derived type reaches it without reflection | none |
| D2 | `Dispose_WhenDisposingIsTrue_EvaluatesComponentGuardAndCallsBase` | jump-0-true and jump-1-false on `:16`; lines 15, 20, 21 | `viewer.Dispose()` on a constructed instance (public `Control.Dispose()` → `Dispose(true)`) | none |

Result: 3 of 3 reachable outcomes = **75% branch — passing**. Lines 17–19 remain at zero and cannot be
raised.

D2 covers an outcome that is *already* covered incidentally; it is nonetheless required, because § 3.3
shows the current coverage depends on an unidentified disposal in another test. Pinning it makes this
file's figure reproducible and independent of test ordering, satisfying
`.claude/rules/general-unit-test.md` § Core Principles 1 and 4 (independence, determinism).

D1 is deterministic and side-effect-free: `Control.Dispose(false)` on a never-shown, handle-less
control performs no unmanaged teardown of consequence. No `Thread.Sleep`, no timer, no temporary file,
no shown window, no popup.

**No test is proposed for `InitializeComponent()`.** It is already at 100% and any test written against
generated layout constants would be a change-detector that breaks whenever the designer is reopened.

---

## 6. Transferable reasoning for `ItemViewer.Designer.cs` (6,224 lines)

The delegation brief asked that the reasoning generalise to the parallel researcher's file. It does,
and one premise was verified directly:

**`QuickFiler/Viewers/ItemViewer.Designer.cs` has the identical disposal shape.** A search of that file
for `components` returns exactly three hits — `:10` (`private System.ComponentModel.IContainer components = null;`),
`:18` (`if (disposing && (components != null))`), and `:20` (`components.Dispose();`). `components` is
never assigned. The same dead-code conclusion holds: three source lines unreachable, branch capped at
3/4 = 75%.

The transferable rules:

1. **A WinForms designer partial cannot be exempted independently of its hand-written partial.**
   `[ExcludeFromCodeCoverage]` is type-level; two partials both carrying it is CS0579; and a
   `coverage.config` production-path exclude is Blocking under
   `.claude/rules/general-unit-test.md`. Once `[ExcludeFromCodeCoverage]` is removed from
   `ItemViewer.cs:20` — which F14 is required to do — `ItemViewer.Designer.cs` becomes instrumented and
   **must** be classified `testable`. There is no third option.
2. **Line coverage of a designer partial is a pure function of "is the control constructed in any
   test".** If yes, expect ~98–100% with no designer-specific test. If no, expect ~0% across all 6,224
   lines, which would be a catastrophic drag on the file-level metric. F14's plan for `ItemViewer` must
   therefore include at least one test that constructs an `ItemViewer` — that single test is worth more
   to the ledger than any number of seam tests. Unlike `ItemViewerExpanded`, `ItemViewer` has **no**
   incidental construction path in the current suite (it is `[ExcludeFromCodeCoverage]` and absent from
   the report entirely), so this must be built, not inherited.
3. **Branch coverage of a designer partial is governed solely by `Dispose(bool)`** and is capped at 75%
   whenever `components` is never assigned. Verify the `components` assignment first: if a designer
   *does* assign `components = new System.ComponentModel.Container()`, all four outcomes become
   reachable and 100% is available; if it does not, 75% is the ceiling and the D1/D2 pair is both
   necessary and sufficient.
4. **Ledger annotation.** Any designer row classified `testable` should record its structural caps
   (line ~98.5%, branch 75%) so F16's capstone does not read a sub-100% figure as a shortfall.

---

## 7. Cross-child notes

- **CC-1 — `QuickFiler/Controllers/QfcHomeController.cs:93` and `:133` (owner: F7).** These are the only
  paths that construct `ItemViewerExpanded` — and therefore run this designer — in the current test
  run. F14 proposes **no edit** to them, but F14's plan must not depend on them: the D1/D2 tests plus
  the `ItemViewerExpanded.cs` construction fixture make this file's coverage self-sufficient. If F7
  seams away `:133`, F14 is unaffected once those tests land.
- **CC-2 — `QuickFiler/Viewers/ItemViewer.cs:20` (owner: F14, parallel researcher).** Removing that
  `[ExcludeFromCodeCoverage]` is what pulls the 6,224-line `ItemViewer.Designer.cs` into the
  denominator. Sequence it deliberately: land the `ItemViewer` construction test in the same change
  that removes the attribute, or the ledger sees a 6,224-line file at 0%.
- No F10, F12, or F13 file is referenced or affected. This file is generated and receives **no edit**
  from F14.

---

## 8. Latent defects — promotion candidates

- **LD-1 — Dead disposal branch in every QuickFiler WinForms designer partial.**
  `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs:16-19` and
  `QuickFiler/Viewers/ItemViewer.Designer.cs:18-21` guard on `components != null`, but `components` is
  initialised to `null` at `:8` / `:10` respectively and is never assigned anywhere in either file
  (verified by exhaustive search: three hits each — declaration, guard, call). The guarded
  `components.Dispose()` is therefore unreachable in both, permanently capping branch coverage of every
  such file at 75%. This is generated code, so the correct disposition is a ledger annotation rather
  than a source edit — but it should be recorded once as an issue so that F16 and future coverage work
  do not repeatedly rediscover the 75% ceiling and misread it as a shortfall. The epic lists seven
  `*.Designer.cs` files plus three generated `Properties/` files; the same check should be applied to
  each.
- **LD-2 — This file's current branch coverage is non-deterministic.**
  `Dispose(true)` runs in the current suite (deduced in § 3.3 from both jumps reading 50%), but no test
  in `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` calls `Dispose` and
  `QuickFiler/Viewers/QfcFormViewer.cs` declares no `Dispose` override — so the disposal comes from an
  unidentified cascade elsewhere in the assembly. A file's reported coverage that depends on an
  unpinned cross-test side effect violates `.claude/rules/general-unit-test.md` § Core Principles 1
  and 4. In-scope mitigation for F14: test D2 pins it. The broader question — how many other QuickFiler
  files draw coverage from incidental cross-test side effects — is out of scope and worth promoting.

Reference-only (already tracked, do not re-promote):

- **#441** — Cobertura post-processing double-counts `<line>` nodes and corrupts class-level
  `line-rate`. Reproduced here: `line-rate="0.9950980392156863"` (203/204, i.e. one uncovered line)
  against an element that enumerates three uncovered lines (17, 18, 19). The `branch-rate` and the
  per-`<condition>` data are unaffected and are what this artifact relies on.

---

## 9. Open-issue scan

Method: GitHub public issue-search UI via WebFetch. No shell tool was available in this session, so
`gh issue list --state open --search ...` could not be run; this is recorded so the evidence trail is
honest about its method. Terms: `ItemViewerExpanded`, `ItemViewer`, `expanded`, `designer`, `viewer`,
`coverage`.

No open issue names this file. Relevant open issues:

| Issue | Title | Relevance |
| --- | --- | --- |
| #441 | Cobertura post-processing double-counts `<line>` nodes, inflating lines-valid and every coverage rate | § 1 — the `line-rate` attribute on this element is unreliable; branch/condition data is not |
| #432 | Feature: quickfiler-coverage-ledger | F1 — owns the bucket assignment recommended in § 4 |
| #456 | Feature: quickfiler-itemviewer-coverage | this child |
| #230 | Build a WinForms message-pump test seam (`Application.Run()` background thread) | not required for this file — construction and disposal need no pump (§ 2) |

---

## 10. Verified vs inferred

**Verified:**

- Report figures and per-method line/condition data (`coverage-final.cobertura.xml:4112-4166`).
- `components` is declared `= null` at `ItemViewerExpanded.Designer.cs:8` and never reassigned
  (3 total occurrences in the file); identical in `ItemViewer.Designer.cs` (`:10`, `:18`, `:20`).
- The sole construction site is `QfcFormViewer.Designer.cs:42`, reached from
  `QfcHomeController.cs:93` / `:133`, exercised by `QfcHomeControllerTests.cs:149` / `:220`, both plain
  `[TestMethod]`.
- `QfcHomeControllerTests.cs` contains no `Dispose` call; `QfcFormViewer.cs` declares no `Dispose`
  override.
- The file is compiled with `<DependentUpon>ItemViewerExpanded.cs</DependentUpon>`
  (`QuickFiler/QuickFiler.csproj:441-442`).

**Inferred:**

- That the two covered outcomes are jump-0-true and jump-1-false (i.e. `Dispose(true)` ran and
  `Dispose(false)` did not) is deduced from both jumps reporting 50%; Cobertura does not name which
  outcome of a jump was taken. The deduction is forced — no other combination yields 50%/50% — but it
  is a deduction, not a direct reading. Test D1 is correct either way: whichever of the two
  `disposing` values is missing, running both D1 and D2 guarantees 3/4.
- The identity of the code that currently triggers `Dispose(true)` is unresolved (§ 3.3).
