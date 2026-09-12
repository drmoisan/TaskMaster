# Research — `QuickFiler/Viewers/ItemViewer.Designer.cs`

- Feature: F14 `quickfiler-itemviewer-coverage` (issue #456), child of epic #136 `quickfiler-per-file-coverage`
- Timestamp: 2026-08-07T23-10
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5e4b635834feedd7`
- Target file: `QuickFiler/Viewers/ItemViewer.Designer.cs` (6,224 physical lines, generated)
- Compile entry: `QuickFiler/QuickFiler.csproj:435-437` (`<DependentUpon>ItemViewer.cs</DependentUpon>`)

Claims are marked **[V]** (verified by direct file read, report inspection, or fetched documentation)
or **[E]** (estimated by a stated and validated arithmetic model). No claim rests on assumption alone.

> **Tooling note.** No shell tool was available this session, so `gh issue list --state open --search ...`
> could not be run. Open-issue search was performed by fetching the public GitHub issue-search UI.
> Method and results are in §8. Everything else is direct file/report evidence.

---

## Headline determinations

| Question | Answer |
| --- | --- |
| **Q1** — does one part's `[ExcludeFromCodeCoverage]` suppress the whole partial type incl. the Designer? | **CONFIRMED.** Documentation + a repository positive control that is *provably executed yet absent from the report*. §1. |
| **Q2** — repository-wide impact of removing the attribute | **Improves or is flat; it does not materially reduce.** +0.57 pp (primary model) to −0.08 pp (conservative model). **Exempting the designer is strictly worse than measuring it (−0.16 pp).** §2. |
| **Q3** — mechanism to exempt only the designer file | **None is both mechanically available and permitted.** Attribute: impossible (CS0579 / re-hides the type). `<Sources>` exclude: mechanically real but policy-Blocking and F1-owned. Harness strip: report-level only, F1-owned. **Recommendation: do not exempt. Classify `testable` and measure it.** §3. |
| Ledger bucket | `testable`, with a recorded structural branch cap of **75%** (3 of 4 outcomes reachable). **Not** `ratified-exempt`. §5. |
| Edits to this file by F14 | **None.** Generated code; exempt from the 500-line rule. |
| Tests required | **Exactly two**, both trivial, both targeting `Dispose(bool)`. §6. |

---

## 1. Q1 — Type-level exemption covers every partial, including the Designer

### 1.1 Documentation [V]

Fetched `https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.excludefromcodecoverageattribute`:

- Declared `[AttributeUsage(..., AllowMultiple = false, Inherited = false)]`.
- Remarks, verbatim: *"Placing this attribute on a class or a structure excludes **all the members of that
  class or structure** from the collection of code coverage information."*

A partial type is one class. `InitializeComponent()` and `Dispose(bool)` in `ItemViewer.Designer.cs` are
members of the same `ItemViewer` class that `ItemViewer.cs:20` attributes. They are therefore excluded.

Fetched `https://learn.microsoft.com/en-us/visualstudio/test/customizing-code-coverage-analysis`: the
Microsoft coverage engine's sample settings show `^System\.Diagnostics\.CodeAnalysis\.ExcludeFromCodeCoverageAttribute$`
in the `<Attributes><Exclude>` list, confirming the attribute is honoured by this repo's collector family.
The repo's own `coverage.config` declares **no** `<Attributes>` section (`coverage.config:10-24`,
`ModulePaths` only) yet attributed types are absent from the report — so the exclusion is the collector's
default behaviour, not repo configuration. [V]

### 1.2 Positive control — executed but absent [V]

Absence from a report only proves exclusion if the code is known to have run. The repository provides
exactly that control:

- `QuickFiler/Viewers/QfcFormViewer.cs:17` carries `[ExcludeFromCodeCoverage]` on `partial class QfcFormViewer`.
- `QuickFiler/Viewers/QfcFormViewer.Designer.cs:3` is `partial class QfcFormViewer` with **no** attribute
  of its own.
- `QfcFormViewer.Designer.cs:42` executes `this._qfcItemViewerExpandedTemplate = new QuickFiler.ItemViewerExpanded();`
  — and the committed report shows `ItemViewerExpanded.Designer.cs`'s `InitializeComponent` at
  `hits="1"` for every line (`coverage-final.cobertura.xml:4135-4741`). **That construction can only have
  come from `QfcFormViewer.Designer.cs:42`** (it is the sole construction site in the solution).
- A search of the entire 190k-line report for `QfcFormViewer` returns **two** hits, both inside method
  `signature=` strings for the interface `QuickFiler.IQfcFormViewer` (XML `:19397`, `:19717`). There is
  **no `<class>` element** for `QfcFormViewer.cs` or `QfcFormViewer.Designer.cs`.

A Designer partial that demonstrably executed, that carries no attribute itself, and that produces no
`<class>` element, is conclusive: **the type-level attribute on the sibling partial suppressed it.**

### 1.3 Negative controls [V]

Two partial WinForms types in the same folder carry **no** exemption, and **both** their partials appear:

| Type | Hand-written partial | Designer partial |
| --- | --- | --- |
| `ToolStripMenuItemCb` | report `:14278` | report `:14228` |
| `BayesianPerformanceViewer` | report `:6416` | report `:5683` |

So `<class>`-element presence tracks the attribute, not the file kind.

### 1.4 Scope limit worth recording — open issue #457 [V]

Open issue **#457, "Bug: excludefromcodecoverage-does-not-suppress-nested-lambdas"** states that a
**method-level** `[ExcludeFromCodeCoverage]` does *not* suppress lambdas hoisted out of the attributed
member, because the compiler places them in a separate closure type that does not inherit the attribute;
it cites `BreadcrumbPopupUiOperations.cs` capping at ~91.5%.

That limitation is **method-level and does not apply to `ItemViewer`.** Empirical check: `ItemViewer.Breadcrumb.cs`
contains lambdas (`:164`, `:172-175`), which are hoisted into closure types **nested inside `ItemViewer`**;
the report contains no `<class>` element with any `Viewers\ItemViewer*.cs` filename at all. Type-level
exclusion therefore does suppress the nested closures in this toolchain. **Practical consequence for the
plan: after removing the type attribute, do not attempt to re-exempt individual members — #457 says the
lambdas would leak back into the denominator anyway.**

---

## 2. Q2 — Sizing the denominator this file adds, and the repository-wide effect

### 2.1 How this report counts lines — a validated model [V]

The class-level `line-rate` attribute is not usable. For `ItemViewerExpanded.Designer.cs` the report
declares `line-rate="0.9950980392156863"` (exactly 203/204, `coverage-final.cobertura.xml:4112`) while the
same element enumerates **612** `<line>` children with **3** uncovered (17, 18, 19). No merge occurred for
that filename (`Merge-CoberturaClassesByFilename` short-circuits at `Invoke-MSTestWithCoverage.Helpers.ps1:191`
when a filename has one class node), so the attribute is the collector's own and is simply not
reconcilable with its own children. This is the family of defect open issue **#441** records; **#441 is
independently verified here**: `Invoke-MSTestWithCoverage.Helpers.ps1:121-122` selects `.//class` then
`.//lines/line`, and the descendant axis matches both `class/lines/line` and
`class/methods/method/lines/line`, double-counting every line into `$totalLines` (`:123`) and hence into
`LinesValid`/`LineRate` (`:137-143`). **Quote no `<class>` `line-rate` without citing #441.**

The 612 figure is derived structurally from the XML layout, and is exact: the class element runs from XML
`:4112` to `:5363` (next class at `:5364`); the `<methods>` block holds `.ctor` (1 line), `Dispose` (7 lines,
one of which expands to 6 XML lines for its two `<condition>` entries) and `InitializeComponent` (M lines);
the class-level `<lines>` block repeats all of them. Solving gives **M = 604** and a total of
**1 + 7 + 604 = 612 coverable lines** for an 821-physical-line file.

**The engine emits one `<line>` per physical source line spanned by a coverage block, not one per
statement.** Direct evidence: `ItemViewerExpanded.Designer.cs:143-144` is one statement spanning two lines
and `:154-158` is one statement spanning five, and the report lists `<line number="144">` (XML `:4245`) and
`<line number="155">` … `<line number="158">` (XML `:4253-4256`) individually. Blank lines, comment lines
and field declarations without initialisers are skipped (gaps at 138-140, 151-153).

That yields a predictive model, **validated to within 2 lines** on `ItemViewerExpanded.Designer.cs`:

```
coverable ≈ physical − blank − comment − field-declarations-without-initialiser − ~10 structural
821 − 5 − 149 − 45 − 10 = 612   (derived actual: 612)
```

### 2.2 Applying the model to this file [E, from V inputs]

Measured inputs for `ItemViewer.Designer.cs` [V]: 6,224 physical; 6 blank lines; 149 comment lines; 45
field declarations without initialisers (`:6178-6222`); 706 lines ending in `;`; structural lines
(`using`, `namespace`, class declaration, two braces, `#region`/`#endregion`, two method signatures, two
closing braces) = 11.

```
6,224 − 6 − 149 − 45 − 11 ≈ 6,013 coverable lines
```

**Primary estimate: ~6,013 coverable lines (±3%).**

**Conservative alternative worth pricing.** 5,338 of the 6,224 physical lines are elements of six constant
`byte[]` initialisers holding embedded SVG payloads — `svgResourceN.Data = new byte[] {` at `:596`, `:1643`,
`:3129`, `:3974`, `:4623`, `:5383`, closing at `:1622`, `:3108`, `:3933`, `:4587`, `:5347`, `:6089` [V].
Each is a single statement compiled to `RuntimeHelpers.InitializeArray`. The validated model says a block's
whole span expands to per-line entries, so these should contribute ~5,338 lines; but that model was
validated on 2- and 5-line statements, not 1,000-line ones. If the collector instead records only the
statement's first line for these six, the file has **~675** coverable lines. Both cases are carried below.
**F1's harness measurement (#432) settles it; nothing in the recommendation changes either way.**

Only **~886** of the physical lines are ordinary designer content (6,224 − 5,338), which is close to
`ItemViewerExpanded.Designer.cs`'s 821 — the two designers are comparable in control count.

### 2.3 How much of it will be covered — near all of it [V]

`ItemViewer` is constructed in **ten** places in `QuickFiler.Test`, all plain `[TestClass]`/`[TestMethod]`:

| File | Line |
| --- | --- |
| `Viewers/BreadcrumbSubfolderActivationTests.cs` | 305 |
| `Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 255 |
| `Viewers/BreadcrumbPendingOpenCloseTests.cs` | 363 |
| `Viewers/BreadcrumbDropDownIntegrationTests.cs` | 338 |
| `Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | 477 |
| `Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 413 |
| `Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | 373 |
| `Controllers/QfcItemController.ViewerSetupTests.cs` | 386 |
| `Controllers/QfcItemController.EventWiringTests.cs` | 236, 327 |

Production also constructs it at `QfcFormViewer.Designer.cs:41`, `EfcViewer.Designer.cs:74`, and
`ItemViewerQueue.cs:105`.

`InitializeComponent()` is straight-line with **zero** branches (verified: a search of the whole 6,224-line
file for `if`, `switch`, `??`, `&&`, `||`, `=>`, `for`, `foreach`, `while` returns exactly two hits —
`:18`, the disposal guard, and `:28`, a doc-comment). So a single construction covers every line of it.
The only lines that can stay uncovered are inside `Dispose`.

**Projected after one construction: ~6,010 / 6,013 = 99.95% line (primary), ~672 / 675 = 99.6%
(conservative). Both clear the 80% line gate with no dedicated test.**

### 2.4 Repository-wide arithmetic [E, from V inputs]

Baseline from the committed report root element (`coverage-final.cobertura.xml:2`) [V]:
`lines-covered="94937" lines-valid="110849"` → **85.645%**. QuickFiler package `line-rate="0.8001906..."`
(`:7`). This baseline is indicative, not authoritative (epic § Measured Coverage Baseline).

The six hand-written `ItemViewer` partials total 1,031 physical lines (`ItemViewer.cs` 432,
`.DisplayState.cs` 81, `.Commands.cs` 109, `.Breadcrumb.cs` 298, `.FolderSearch.cs` 74,
`.WebViewThread.cs` 37) → **~520 coverable [E]**, of which perhaps **~260** are already executed by the ten
construction sites and the breadcrumb harnesses (the `ItemViewer.Breadcrumb.cs` sibling artifact §5.1
concludes the same and calls the measured figure "very plausibly 50-75%").

| Scenario | New lines-valid | New lines-covered | New repo rate | Δ |
| --- | --- | --- | --- | --- |
| **A. Remove attribute; designer measured (primary model)** | 117,382 | 101,207 | **86.22%** | **+0.57 pp** |
| **B. Remove attribute; designer measured (conservative model)** | 112,044 | 95,869 | **85.56%** | −0.08 pp |
| **C. Remove attribute but exempt the designer** | 111,369 | 95,197 | **85.48%** | **−0.16 pp** |

**Conclusion, and it inverts the risk stated in the delegation brief:** removing
`[ExcludeFromCodeCoverage]` does **not** meaningfully reduce repository-wide coverage. In the primary
model it improves it by more than half a point; in the pessimistic model it is flat to within a rounding
step. The one option that *does* reduce repository coverage is **exempting the designer file** (scenario C)
— because that removes ~6,000 near-fully-covered lines from the numerator while leaving the ~50%-covered
hand-written partials in the denominator.

QuickFiler assembly line rate (denominator estimated at ~13,600 from the package's XML span at the
validated ~2.04 XML-lines-per-coverable-line ratio): scenario A ≈ 85.2%, scenario B ≈ 79.9%. The epic
gates per-file and repository-wide, not per-assembly, so this is context only.

### 2.5 The one real sequencing risk

Coverage of this file is **entirely incidental** — it comes from tests owned by nobody in particular that
happen to construct an `ItemViewer`. Six of the ten sites are breadcrumb harnesses whose production
subjects belong to F12/F13, and two belong to F10's `QfcItemController`. If a sibling replaces a live
`ItemViewer` with a mock, ~6,000 lines fall to zero in one commit.

**Requirement on the plan:** the change that removes `ItemViewer.cs:20` must land in the same commit as at
least one F14-owned test that constructs a real `ItemViewer` (tests IV-1/IV-3 in the `ItemViewer.cs`
artifact, plus D1/D2 below). Do not inherit the sibling harnesses. This matches the conclusion the
`ItemViewerExpanded.Designer.cs` sibling reached for its own file (its CC-1/CC-2).

---

## 3. Q3 — Which exemption mechanisms exist for the designer file alone

### 3.1 `[ExcludeFromCodeCoverage]` on the Designer partial — unavailable [V]

Two independent blocks:

1. Placing it on `ItemViewer.Designer.cs:5` (`partial class ItemViewer`) attributes **the type**, so it
   re-hides all six hand-written partials — the exact thing F14 must undo. §1 proves this.
2. Placing it on *both* `ItemViewer.cs:20` and `ItemViewer.Designer.cs:5` is a duplicate application of an
   `AllowMultiple = false` attribute to one type. Confirmed from the fetched `AttributeUsage`
   declaration [V]; Roslyn reports this as **CS0579 "Duplicate 'ExcludeFromCodeCoverage' attribute"**
   (the error code is inferred from `AllowMultiple = false`, not compiled here — no build was run this
   session). The `issue.md:50-52` premise is therefore **CONFIRMED**.

There is no file-scoped form of the attribute in C#, and `ItemViewer.Designer.cs` declares no type of its
own to attach one to.

### 3.2 `<Sources><Exclude><Source>` in coverage settings — mechanically real, but not permitted

**Mechanically it would work, and precisely.** The fetched Microsoft documentation defines
`<Source>` as matching *"elements by the path name of the source file in which they're defined"*. For a
partial class, `InitializeComponent` and `Dispose(bool)` are *defined* in the Designer file, so
`<Source>.*\\ItemViewer\.Designer\.cs$</Source>` would drop exactly those two methods and leave the six
hand-written partials measured. This is the only mechanism that achieves per-file granularity.

**It is nonetheless ruled out, on two independent grounds:**

1. **Policy.** `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy: *"No production file may
   be excluded from coverage measurement"*; permitted `exclude` entries are build output, tests, config,
   and `node_modules` only; and *"Feature-review agents must treat any `exclude` entry that matches a
   production source path as a **Blocking** finding."* `QuickFiler\Viewers\ItemViewer.Designer.cs` is a
   production source path.
2. **Ownership — cross-child constraint.** Both files that would need editing are repo-root shared files:
   `coverage.config` (`:10-24`) drives the `dotnet-coverage --settings` outer pass and
   `TaskMaster.runsettings` (`:9-29`) drives the Visual Studio / inner-vstest pass; **both currently carry
   `<ModulePaths>` excludes only**, so a `<Sources>` section would have to be introduced in both, in
   duplicate, and kept in sync. The epic names `coverage.config` explicitly as a shared file that F1
   settles (epic § "Why F1 is a real dependency", point 3: children *"would collide on the shared
   `coverage.config`"*), and the epic's shared-file exception is granted **only** for
   `QuickFiler/QuickFiler.csproj` (epic § "Cross-Child Constraints" 1). **F14 must not edit either file.**

### 3.3 Post-processing in `Invoke-MSTestWithCoverage.ps1` / F1's harness — report-level only

The script already strips third-party packages from the Cobertura output after collection
(`coverage.config:6-8` documents this). A filename strip is therefore mechanically precedented. But it is
(a) the same script F1 owns and #441 is already open against, and (b) it does not exempt anything —
the file stays instrumented and stays in the epic's *dynamic denominator* (epic § "Mid-Wave File Creation",
rule 1: the denominator is the set of `<Compile Include=...>` entries at evaluation time). It can only
change how the row is *labelled*, which is the ledger's job anyway.

### 3.4 Recommendation — measure it

**Classify `ItemViewer.Designer.cs` as `testable`; add no attribute; edit no settings file; write the two
`Dispose` tests in §6.** The file passes the 80% line gate at ~99.9% with no work, and reaches its
structural branch maximum of 75% with two trivial tests. Claiming an exemption for a file that
demonstrably passes both gates would itself be a Blocking finding under the epic's ratified reconciliation
("refactor first, exempt only the irreducible remainder"; `[ExcludeFromCodeCoverage]` on a testable seam is
Blocking).

**This refines, with new evidence, the disposition proposed in
`research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md` §1.3**, which recommended accepting the designer as
`ratified-exempt` via a harness/filename exclusion, on the assumption that it would enter the denominator
"at near-0%". The measurement shows the opposite: the file enters at ~99.9%, and a filename exclusion is
Blocking under `.claude/rules/general-unit-test.md`. That artifact's central point — that the exemption
decision is F14's gating decision and must be settled in `spec.md` before planning — stands unchanged, and
is settled here. It also agrees with `research.itemviewerexpanded-designer-cs.2026-08-07T21-40.md` §4,
which reached the identical `testable`-not-exempt conclusion for the sibling designer and explicitly
recorded that the epic's "exempt-candidate" label at `epic.md:431` is disproved by measurement.

**Cross-child note CN-1 (F14 → F1, issue #432).** F1's ledger rules must state the generated-designer rule
as *"generated `*.Designer.cs` partials are `testable`, exempt from the 500-line rule, and carry a recorded
branch cap of 75% where `components` is never assigned"* — not as a blanket `ratified-exempt` for
designers. `epic.md:431` and `:440-443` currently label all seven designers "exempt-candidate"; two of
them have now been measured and neither needs an exemption. F14 proposes **no edit** to `coverage.config`,
`TaskMaster.runsettings`, or `Invoke-MSTestWithCoverage*.ps1`.

---

## 4. Structure of the file (Q3, "report the actual structure") [V]

| Region | Lines | Content |
| --- | --- | --- |
| `using SVGControl;` / `namespace` / `partial class ItemViewer` | 1-6 | `ItemViewer.Designer.cs:5` declares `partial class ItemViewer` with **no attribute** |
| `components` field | 10 | `private System.ComponentModel.IContainer components = null;` — the file's `.ctor` contribution |
| `Dispose(bool)` | 16-23 | the **only** method with a branch |
| `#region` … `InitializeComponent()` … `#endregion` | 25-6177 | one method, zero branches |
| 45 field declarations | 6178-6222 | no initialisers ⇒ no sequence points |

**Exactly two methods.** [V] A structural grep for member declarations returns `:16 Dispose(bool)` and
`:31 InitializeComponent()` and nothing else. No event handler is *defined* here; the only handler wiring
is `:256`, `this._l0v2h2_WebView2.ParentChanged += new System.EventHandler(this.L0v2h2_WebView2_ParentChanged);`,
whose target lives in `ItemViewer.cs:166`.

**Exactly one branch point.** [V] `:18`, `if (disposing && (components != null))` — two jumps, four
outcomes. Nothing else in 6,224 lines.

**The fourth outcome is dead.** [V] `components` occurs exactly three times in the file: the declaration
`= null` at `:10`, the guard at `:18`, and the call at `:20`. It is never assigned a non-null value by
`InitializeComponent()`, because no component on the design surface required a `Container`. Therefore:

- source lines **19, 20, 21** are unreachable;
- three of four outcomes on `:18` are reachable — (a) `disposing == false` (short-circuit),
  (b) `disposing == true` then (c) `components != null` false; outcome (d) is unreachable.
- **Maximum branch coverage = 3/4 = 75.00%, exactly the floor. Maximum line coverage ≈ 99.95%.**

> **Caveat the plan must absorb.** `ItemViewer.Breadcrumb.cs:286-288` executes
> `components ??= new Container(); components.Add(_breadcrumbResourceOwner);`. Once breadcrumb
> configuration has run on an instance, `components` **is** non-null for that instance, and the fourth
> outcome becomes reachable — so this file can in principle reach 4/4 = 100% branch and 100% line, by
> disposing a viewer whose breadcrumb resource ownership was established first. The 75% figure is the cap
> *without* that path. Test D3 in §6 exploits it and is cheap; keep D1/D2 regardless so the file passes
> even if D3's dependency on the F14-sibling file is later restructured.

**Same shape as its sibling** [V]: `ItemViewerExpanded.Designer.cs` has the identical `:8/:16/:18` triple
with `components` never assigned, which is why its measured branch rate is `0.5` (2 of 4).

---

## 5. Ledger classification (Q3c)

| Field | Value |
| --- | --- |
| Bucket | **`testable`** |
| `[ExcludeFromCodeCoverage]` | **none** — must not be added (see §3.1) |
| Line target / projected | >= 80% / **~99.95%** after any single construction |
| Branch target / projected | >= 75% / **50% today, 75% after D1+D2, 100% after D3** |
| Structural caps to record | 3 lines (19-21) unreachable unless the breadcrumb container path ran first |
| 500-line rule | **exempt** — generated code (epic § Shared Design 5; `.claude/rules/general-code-change.md` § File Size Limit) |
| Edits by F14 | **none** |

---

## 6. Test plan (Q7 for this file)

Per issue #136, each row is one atomic task. All are MSTest `[TestMethod]`, FluentAssertions, AAA, no
Moq needed, no temp files, no external services, no live Form, no popup, no `Thread.Sleep`/`Task.Delay`.
**No STA is required** — ten existing plain `[TestMethod]`s already construct and use `ItemViewer` on the
default apartment (§2.3), so the epic's STA last-resort clause is not engaged by this file.

Proposed home: `QuickFiler.Test/Viewers/ItemViewerDisposalTests.cs` (new; requires a
`<Compile Include="Viewers\ItemViewerDisposalTests.cs" />` entry in `QuickFiler.Test/QuickFiler.Test.csproj`,
CRLF preserved, adjacent to the existing `Viewers\` block).

Fixture: a `SynchronizationContext` must be installed before `new QuickFiler.ItemViewer()` — the
constructor calls `TaskScheduler.FromCurrentSynchronizationContext()` at `ItemViewer.cs:27`, which throws
without one. Pattern: `BreadcrumbDropDownIntegrationTests.cs:336-338`.

| # | Test name | Production lines / outcomes | Mechanism |
| --- | --- | --- | --- |
| **D1** | `Dispose_WhenDisposingIsFalse_SkipsComponentDisposalAndCallsBase` | **jump-0-false on `:18`** — the gating outcome; lines 17, 22, 23 | test-local `private sealed class DisposeProbe : QuickFiler.ItemViewer { internal void DisposeUnmanagedOnly() => base.Dispose(false); }`. `Dispose(bool)` is `protected`, so a derived type reaches it with no reflection. |
| **D2** | `Dispose_WhenDisposingIsTrue_EvaluatesComponentGuardAndCallsBase` | jump-0-true and jump-1-false on `:18`; lines 17, 22, 23 | `viewer.Dispose()` on a constructed instance (public `Control.Dispose()` → `Dispose(true)`) |
| **D3** *(optional, lifts 75% → 100%)* | `Dispose_AfterBreadcrumbResourceOwnershipEstablished_DisposesComponentContainer` | **jump-1-true on `:18`, plus lines 19, 20, 21** | construct, drive `ItemViewer.Breadcrumb.cs:279-289` (`EnsureBreadcrumbResourceOwnership`) so `components` becomes non-null, then `Dispose()`. Assert the `BreadcrumbResourceOwner`'s dispose action ran. |

D1 + D2 give **3/3 reachable outcomes = 75% branch — passing**. D3 gives 4/4 = 100% and covers the last
three lines. D1 is deterministic and side-effect-free: `Control.Dispose(false)` on a never-shown,
handle-less control performs no consequential teardown.

**D2 is not redundant even though `Dispose(true)` may already run incidentally.** The sibling
`ItemViewerExpanded.Designer.cs` research (§3.3) showed its 50% branch figure depends on an unidentified
disposal cascade in another test; a reported figure that depends on an unpinned cross-test side effect
violates `.claude/rules/general-unit-test.md` § Core Principles 1 and 4.

**No test is proposed for `InitializeComponent()`.** It is fully covered by construction and any assertion
over generated layout constants would be a change detector that breaks whenever the designer is reopened.

---

## 7. Latent defects — promotion candidates

**LD-D1 — Dead disposal branch in every QuickFiler WinForms designer partial.**
`QuickFiler/Viewers/ItemViewer.Designer.cs:18-21` guards on `components != null`, but `components` is
initialised to `null` at `:10` and is never assigned by `InitializeComponent()` (verified: exactly three
occurrences in 6,224 lines — `:10`, `:18`, `:20`). The same holds verbatim for
`ItemViewerExpanded.Designer.cs:8/16/18` and `QfcFormViewer.Designer.cs:8/16/18`. Every such file is
therefore capped at 3/4 = 75% branch unless another partial creates the container (which only
`ItemViewer.Breadcrumb.cs:286` does, and only for `ItemViewer`). This is generated code, so the correct
disposition is a ledger annotation rather than a source edit — but it should be recorded once as an issue
so F16 and future coverage work do not repeatedly rediscover the 75% ceiling and misread it as a
shortfall. The epic lists seven `*.Designer.cs` files; the same three-occurrence check applies to each.
*(Duplicate-check: the `ItemViewerExpanded.Designer.cs` sibling artifact raised this as its LD-1. Do not
promote twice — promote once, citing both files plus `QfcFormViewer.Designer.cs`.)*

**LD-D2 — Six embedded SVG payloads are inlined as ~5,338 lines of `byte[]` literal in generated code.**
`ItemViewer.Designer.cs:596-1622`, `:1643-3108`, `:3129-3933`, `:3974-4587`, `:4623-5347`, `:5383-6089` are
constant `byte[]` initialisers for `SVGControl.SvgResource.Data`, one element per line, accounting for 86%
of the file's 6,224 lines. `ItemViewer.resx` already exists as an embedded resource and
`ItemViewer.Designer.cs:35` already constructs a `ComponentResourceManager`, so these payloads could live
in the `.resx` like every other resource. The practical cost is that a single generated file dominates
QuickFiler's coverage denominator by ~6,000 lines of un-asserted generated data, distorting every
assembly-level figure. Out of scope for F14 (regenerating a designer is a behaviour risk with no test
coverage behind it), and worth an issue.

**Reference-only, already tracked — do not re-promote:**

- **#441** — Cobertura post-processing double-counts `<line>` nodes. Independently verified at
  `Invoke-MSTestWithCoverage.Helpers.ps1:121-122` (§2.1) and reproduced on the sibling designer element
  (`line-rate="0.9950980392156863"` = 203/204 against 612 enumerated `<line>` children with 3 uncovered).
- **#457** — method-level `[ExcludeFromCodeCoverage]` does not suppress hoisted lambdas. Bears on §1.4:
  it constrains what F14 may do *after* removing the type attribute, not the removal itself.

---

## 8. Open-issue scan

Method: GitHub public issue-search UI via WebFetch (`https://github.com/drmoisan/TaskMaster/issues?q=is%3Aissue+is%3Aopen+<term>`);
no shell was available for `gh`. Terms: `ItemViewer`, `viewer`, `coverage`, `designer`,
`ExcludeFromCodeCoverage`, `WebView`.

| Issue | Title | Bearing on this file |
| --- | --- | --- |
| **#457** | Bug: excludefromcodecoverage-does-not-suppress-nested-lambdas | **Direct.** Scopes Q1: the leak is *method-level*; type-level exclusion is complete (§1.4). Also forbids re-exempting individual members after removal. |
| **#441** | Cobertura post-processing double-counts `<line>` nodes | **Direct and load-bearing.** Verified in-script (§2.1). All figures here are recomputed from `<line>` children or from the validated model, never from a `line-rate` attribute. Not returned by the truncated web search; its existence and content are taken from the orchestrator brief and corroborated by the script defect verified here. |
| **#432** | Feature: quickfiler-coverage-ledger | F1 — owns the bucket assignment in §5 and the harness that must confirm the §2.2 estimate. Cross-child note CN-1 addressed to it. |
| **#456** | Feature: quickfiler-itemviewer-coverage | this child |
| **#230** | Build a WinForms message-pump test seam (`Application.Run()` background thread) | Not required for this file — construction and disposal need no pump (§2.3). |
| #455, #458, #462, #463, #440, #438, #467 | breadcrumb drop-down / WebView2 host / navigation / focus-steal / EfcViewer | F13/F12/F9 territory. **#458 (`webview2breadcrumbhost-handler-retention-pooled-viewer`) and #462 touch the harnesses that currently provide this file's incidental coverage** — reinforcing §2.5. No edit proposed to any of them. |
| #427 | quickfiler-post-show-duplicate-scoring | Not returned by any search performed; no relationship to this file was found. |

---

## 9. Verified vs inferred

**Verified (file read, report inspection, or fetched documentation):**

- `ItemViewer.Designer.cs` structure: `:5` bare `partial class ItemViewer`; `:10` `components = null`;
  `:16-23` `Dispose(bool)`; `:31-6175` `InitializeComponent()`; `:6178-6222` 45 field declarations; one
  branch at `:18`; `components` occurs exactly three times; one event wiring at `:256`.
- Six `byte[]` initialiser spans and their exact start/end lines.
- Q1 positive control: `QfcFormViewer.cs:17` attributed; `QfcFormViewer.Designer.cs:3` unattributed;
  `:42` provably executed; zero `<class>` elements for either file in the report.
- Q1 negative controls: `ToolStripMenuItemCb` and `BayesianPerformanceViewer` — both partials present.
- `AllowMultiple = false, Inherited = false`, and the "all the members of that class" remark.
- `<Source>` matches by defining source file; `coverage.config` and `TaskMaster.runsettings` carry
  `<ModulePaths>` only.
- #441 root cause at `Invoke-MSTestWithCoverage.Helpers.ps1:121-122`; the merge path's short-circuit at `:191`.
- The per-physical-line expansion of multi-line statements (report `:4245`, `:4253-4256`).
- Ten `new QuickFiler.ItemViewer()` sites in `QuickFiler.Test`, all plain `[TestClass]`.
- Repository baseline `lines-covered="94937" lines-valid="110849"` at report `:2`.

**Inferred / estimated (model stated, validated to ±2 lines on the sibling file):**

- ~6,013 coverable lines for this file (primary) or ~675 (conservative); the collector's handling of a
  1,000-line constant array initialiser was not measured.
- ~520 coverable lines and ~260 covered across the six hand-written partials, hence the repo-wide deltas
  in §2.4. These are estimates; F1's harness supersedes them.
- CS0579 as the specific compiler error code for a duplicated attribute (follows from
  `AllowMultiple = false`; no build was run this session).
