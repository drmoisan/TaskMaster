# quickfiler-itemviewer-coverage — Spec

- **Issue:** #456
- **Parent:** epic `quickfiler-per-file-coverage`, issue #136 (child F14, wave 1)
- **Depends on:** F1 `quickfiler-coverage-ledger`, issue #432 (wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T23-45
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature

## Overview

The `ItemViewer` form family under `QuickFiler/Viewers/` is invisible to coverage measurement. `ItemViewer`
is a partial type spread across six hand-written source files plus a 6,224-line generated designer file, and
the single `[ExcludeFromCodeCoverage]` attribute at `QuickFiler/Viewers/ItemViewer.cs:20` suppresses
instrumentation for the whole type. The only member of the family that is measured,
`QuickFiler/Viewers/ItemViewerExpanded.cs`, sits below both the 80% line and 75% branch gates that issue #136
and `.claude/rules/general-unit-test.md` set (`epic.md:486-487`).

Under the epic's ratified policy reconciliation (`epic.md:205-225`), `[ExcludeFromCodeCoverage]` on a testable
seam is a Blocking finding. The attribute at `ItemViewer.cs:20` has never been argued against the
irreducible-remainder standard, so the family is unratified exempt rather than legitimately exempt. No member
of `ItemViewer.cs` touches `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or
`MAPIFolder`, and `ItemViewer` is a `UserControl`, not a `Form` (`ItemViewer.cs:21`), so neither the
Outlook-Interop ground nor the form-derived ground of the `CLAUDE.md` § UT2 exemption applies to it.

This feature brings every `testable` file in the family to at least 80% line and 75% branch coverage,
verified with F1's per-file harness, by extracting one host-neutral module, widening four narrow visibility
seams, and removing the type-level attribute. `QuickFiler/Viewers/IItemViewer.cs` is classified
`interface-only / not-measured`. Both `*.Designer.cs` files are classified `testable`. No observable behavior
change to QuickFiler flows.

## Scope — Per-File Disposition

Ten production files. Line counts are physical lines as of the working tree at `74be1964`. "Baseline" is the
committed indicative Cobertura report at
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
which is indicative and not authoritative (`epic.md:141-145`).

| File | Lines | Disposition | Baseline | Notes |
| --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.cs` | 432 | **testable** | absent from report | Carries the family's only real `[ExcludeFromCodeCoverage]`, at `:20`. Projected ~390 lines after seam S1. |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 298 | **testable** | absent from report | Branch-dense: ~26 decision points. The branch gate, not the line gate, binds. |
| `QuickFiler/Viewers/ItemViewer.Commands.cs` | 109 | **testable** | absent from report | 16 forwarding members, 32 coverable lines, **zero branch points**. |
| `QuickFiler/Viewers/ItemViewer.DisplayState.cs` | 81 | **testable** | absent from report | 12 forwarding members, 23 coverable lines, **zero branch points**, zero fields. |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | 74 | **testable** | absent from report | ~21 coverable lines, ~10 condition points. The only one of the three small partials with real branch logic. |
| `QuickFiler/Viewers/ItemViewer.WebViewThread.cs` | 37 | **testable** | absent from report | 9 coverable lines, **zero branch points**. One uncovered line is 11.1%; no slack. |
| `QuickFiler/Viewers/ItemViewerExpanded.cs` | 181 | **testable** | 37.74% line (40/106 distinct `<line>` children), 8.33% branch (1/12) | The reported `line-rate="0.390244"` is corrupted; see D10. |
| `QuickFiler/Viewers/IItemViewer.cs` | 133 | **interface-only / not-measured** | no `<class>` element | No attribute, no tests, reported N/A. See D4. |
| `QuickFiler/Viewers/ItemViewer.Designer.cs` | 6,224 | **testable** | absent from report | Generated; exempt from the 500-line rule (`epic.md:254-255`). See D3. |
| `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` | 821 | **testable** | ~98.5-99.5% line, 50% branch (2/4) | Generated; exempt from the 500-line rule. See D3. |

One production file is **created** by this feature: `QuickFiler/Viewers/ControlColumnTrimmer.cs` (seam S1).

### Files explicitly outside this feature's edit set

F14 proposes **no edit** to `QuickFiler/Viewers/ToolStripMenuItemCb.cs` (F15), any `QfcItemController.*` file
(F10), any breadcrumb bridge or messenger file (F12), any breadcrumb drop-down or WebView2 host file (F13),
`coverage.config`, `TaskMaster.runsettings`, `scripts/vscode/Invoke-MSTestWithCoverage*.ps1` (F1/#432),
`UtilitiesCS/Properties/AssemblyInfo.cs` (`epic.md:619-631`), or `QuickFiler/Viewers/IItemViewer.cs`. The
only shared file F14 edits is `QuickFiler/QuickFiler.csproj`, which is the epic's one sanctioned shared-file
exception (`epic.md:594-617`).

## Documented Deviations from the Epic Brief

Preparation research disproved several premises carried in `issue.md` and in `epic.md`'s F14 entry. Each
deviation below is recorded so the planner builds against reality rather than the brief. Each carries its
evidence.

### D1 — The partial-type instrumentation hypothesis is CONFIRMED

`ExcludeFromCodeCoverageAttribute` is declared `AllowMultiple = false, Inherited = false`, and its
documented behavior is that *"Placing this attribute on a class or a structure excludes all the members of
that class or structure from the collection of code coverage information."* A partial type is one class.

Repository positive control, executed-yet-absent: `QuickFiler/Viewers/QfcFormViewer.cs:17` is attributed;
`QuickFiler/Viewers/QfcFormViewer.Designer.cs:3` is a bare unattributed partial;
`QfcFormViewer.Designer.cs:42` is **provably executed** — it is the sole construction site of
`ItemViewerExpanded` in the solution, and `ItemViewerExpanded.Designer.cs` shows `hits="1"` throughout the
committed report — yet neither file emits a `<class>` element anywhere in the 190k-line report. Negative
controls: `ToolStripMenuItemCb` and `BayesianPerformanceViewer` carry no attribute and **both** their
partials appear.

**Consequence:** removing the one attribute at `ItemViewer.cs:20` makes SEVEN files measurable at once —
the six hand-written partials plus the 6,224-line `ItemViewer.Designer.cs`. `issue.md:50-52`'s CS0579
premise is confirmed: the designer partial cannot be exempted independently once the attribute is removed.

### D2 — The repository-coverage risk runs OPPOSITE to the brief's assumption

`ItemViewer.Designer.cs` adds ~6,013 coverable lines, and its `InitializeComponent()` has **zero branches**
(verified: a search of all 6,224 lines for `if`, `switch`, `??`, `&&`, `||`, `=>`, `for`, `foreach`, `while`
returns exactly two hits — `:18`, the disposal guard, and `:28`, a doc comment). A single construction
therefore covers ~99.95% of it.

Against the committed baseline root element (`coverage-final.cobertura.xml:2`,
`lines-covered="94937" lines-valid="110849"` = 85.645%):

| Scenario | New repo rate | Delta |
| --- | --- | --- |
| Remove attribute; designer measured (primary model) | 86.22% | **+0.57 pp** |
| Remove attribute; designer measured (conservative model) | 85.56% | −0.08 pp |
| Remove attribute but **exempt** the designer | 85.48% | **−0.16 pp** |

The option that reduces repository coverage is exempting the designer, not measuring it. **The real risk is
sequencing**: the designer's coverage today is entirely incidental, arising from harnesses owned by F10,
F12, and F13 (six breadcrumb harnesses plus two `QfcItemController` harnesses). Attribute removal MUST
therefore land in the same commit as F14-owned tests that construct a real `ItemViewer`, so the ~6,000-line
designer never appears in a measured state that depends solely on sibling-owned harnesses.

### D3 — Both `*.Designer.cs` files are `testable`, NOT exempt-candidates

This contradicts `epic.md:426-433`, which labels both designers "exempt-candidates". Two independent
researchers concurred that the label is disproved by measurement.

No designer-only exemption mechanism is both available and permitted:

1. **Attribute route — impossible.** On the designer partial (`ItemViewer.Designer.cs:5`,
   `ItemViewerExpanded.Designer.cs`) the attribute attaches to the *type* and re-hides all partials, which is
   the exact thing F14 must undo. On both partials it is CS0579 under `AllowMultiple = false`. C# has no
   file-scoped form of the attribute.
2. **`<Sources><Exclude>` route — Blocking and out of bounds.** It would work mechanically, since
   `InitializeComponent` and `Dispose(bool)` are *defined* in the designer file, but
   `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy states that no production file may be
   excluded and that a feature-review agent must treat any `exclude` entry matching a production source path
   as a **Blocking** finding. It would also require introducing a `<Sources>` section into both
   `coverage.config` (`:10-24`, `<ModulePaths>` only) and `TaskMaster.runsettings` (`:9-29`,
   `<ModulePaths>` only) — repo-root shared files the epic assigns to F1 (`epic.md:290-294`).
3. **Harness filename strip — report-level only.** It is F1/#432 territory, it exempts nothing (the file
   stays instrumented and stays in the dynamic denominator per `epic.md:574-576`), and it can only change how
   a row is labelled, which is the ledger's job anyway.

**Structural facts both designers share.** Each has exactly one branch point —
`ItemViewer.Designer.cs:18` and `ItemViewerExpanded.Designer.cs:16`, both
`if (disposing && (components != null))`, two jumps / four outcomes. `components` is declared `= null`
(`ItemViewer.Designer.cs:10`, `ItemViewerExpanded.Designer.cs:8`) and never assigned (verified: exactly
three occurrences in each file — declaration, guard, call), so one arm is unreachable dead code and branch
coverage is **capped at 3/4 = exactly 75%**. Both currently read 50%, which is only consistent with
`Dispose(true)` having run and `Dispose(false)` not having run. **Two trivial `Dispose` tests take each file
from 50% to a passing 75%.**

One documented caveat: `ItemViewer.Breadcrumb.cs:286-288` executes
`components ??= new Container(); components.Add(_breadcrumbResourceOwner);`, so on an `ItemViewer` whose
breadcrumb resource ownership was established first, the fourth outcome becomes reachable and 100% branch is
available. The 75% cap is the figure *without* that path; the two `Dispose` tests must be kept regardless so
the file passes even if that path is later restructured.

**Neither designer receives any edit from F14.**

### D4 — `IItemViewer.cs` is confirmed `interface-only / not-measured`

All 68 members between `IItemViewer.cs:17` and `:131` are bodiless declarations — every one terminates in
`;` or in a `{ get; }` / `{ get; set; }` accessor list with no body. Verified absent: default interface
members, `static` members, `const` fields, nested types, field initialisers, and attributes. The two lines
`#pragma warning disable CS0108` (`:124`) and `#pragma warning restore CS0108` (`:129`) are preprocessor-only
and emit no IL. `QuickFiler/QuickFiler.csproj:13` targets `v4.8.1`, which forecloses default interface
implementations regardless of `LangVersion`, so the file cannot silently acquire executable content later.

Empirical confirmation with a same-folder positive control: the report contains a `<class>` element for
`QuickFiler\Viewers\ItemViewerExpanded.cs` (XML `:5364`) and for
`QuickFiler\Viewers\ItemViewerExpanded.Designer.cs` (XML `:4112`), proving `QuickFiler\Viewers\` was
instrumented, while no `<class>` element exists for `IItemViewer.cs`. The file is compiled
(`QuickFiler/QuickFiler.csproj:392`), so its absence is not a build exclusion.

**Consequences, all binding:** the ledger bucket is `interface-only / not-measured`, **not**
`ratified-exempt` (`epic.md:509-522`); it receives **no** `[ExcludeFromCodeCoverage]`; it is reported **N/A**
and never 0%; and **zero tests are written for it**. Shape-assertion tests written to manufacture coverage
are prohibited (`epic.md:521-522`). F14 proposes **no edit** to this file, which matters because it is
consumed by F10 (`QfcItemController.*` mocks) and F9 (`EfcItemController.cs:247`).

### D5 — No STA tests are required anywhere in this feature

`issue.md:64-66` predicted F14 was the child most likely to need the epic's STA last-resort clause
(`epic.md:234-241`). Research disproved it.

Ten existing plain `[TestMethod]`s already construct a live headless `ItemViewer` —
`Viewers/BreadcrumbDropDownIntegrationTests.cs:338`, `Viewers/BreadcrumbCoordinatorLifecycleTests.cs:477`,
`Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs:413`, `Viewers/BreadcrumbSelectorOpenRetryTests.cs:255`,
`Viewers/BreadcrumbSubfolderActivationTests.cs:305`, `Viewers/BreadcrumbPendingOpenCloseTests.cs:363`,
`Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:373`,
`Controllers/QfcItemController.ViewerSetupTests.cs:386`, and
`Controllers/QfcItemController.EventWiringTests.cs:236,327` — including a real
`Microsoft.Web.WebView2.WinForms.WebView2` (`ItemViewer.Designer.cs:46`), a `MenuStrip`, four
`ToolStripMenuItemCb`, a `BrightIdeasSoftware.FastObjectListView`, six `ButtonSVG` controls, and a
`ComponentResourceManager` over `ItemViewer.resx`. `ItemViewerExpanded` is likewise constructed to
completion in plain `[TestMethod]`s via `QfcHomeControllerTests.cs:149` and `:220`. There is no
`[STATestClass]` anywhere in `QuickFiler.Test`.

**Do not create the first `*.StaTests.cs` file in `QuickFiler.Test`.**

**The real constraint is different and is not optional.** `ItemViewer.cs:27` calls
`TaskScheduler.FromCurrentSynchronizationContext()`, which throws `InvalidOperationException` when
`SynchronizationContext.Current` is null. **Every test that constructs an `ItemViewer` MUST install a
`SynchronizationContext` first** and restore the previous context in `finally`/`[TestCleanup]`. The
established pattern is `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs:336-338`. This also
makes dispatch deterministic: `BreadcrumbUiDispatcher.IsCurrentBoundary()` (`:271`) compares
`ReferenceEquals(SynchronizationContext.Current, _context)`, so every `Dispatch`/`PostAsync` issued from the
test method body on the same thread runs synchronously inline — no pump, no timer, no wait.

Two related fixture rules follow: (a) tests that do not need `InitializeComponent()` should use
`FormatterServices.GetUninitializedObject` plus public property assignment, the pattern already in use at
`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:249-265`, which runs no constructor and therefore
needs no `SynchronizationContext`; (b) every new `async` test must complete its readiness/TCS synchronously
on the test thread, because awaiting a continuation posted back to a `WindowsFormsSynchronizationContext` on
a pumpless MSTest thread can deadlock.

### D6 — `FakeTimeProvider` IS available on net481 here

`QuickFiler.Test/packages.config:18` pins `Microsoft.Bcl.TimeProvider 10.0.10` and `:84-88` pins
`Microsoft.Extensions.TimeProvider.Testing 10.8.0`; assembly references are wired at
`QuickFiler.Test/QuickFiler.Test.csproj:205-206` and `:255-256`, with the production reference at
`QuickFiler/QuickFiler.csproj:68-69`. `FakeTimeProvider` is in active use in this exact test project —
`Controllers/QfcHomeControllerMetricsTests.cs:318-319`, `Controllers/QfcDatamodelTests.cs:106,254,288` — and
`QfcHomeControllerMetricsTests.cs:316` states the repo rule in-code: *"Moq cannot mock the non-virtual
GetLocalNow(); FakeTimeProvider is the prescribed seam."* This matches
`.claude/rules/general-unit-test.md` § Determinism Infrastructure.

**No file in F14's scope reads a clock, uses a timer, or calls `Thread.Sleep`/`Task.Delay`** (verified by
full read of all six hand-written partials and `ItemViewerExpanded.cs`). **No clock seam is required.**
`issue.md:53-54`'s "tests must use an injected clock and fake timers" constraint therefore does not apply.
If a clock need emerges during execution, use `TimeProvider`/`FakeTimeProvider`, not a bespoke abstraction.

### D7 — `ItemViewer.FolderSearch.cs` touches no COM and has no filtering predicate

The `using` set is `System`, `System.Collections.Generic`, `System.Linq`, `System.Windows.Forms`,
`UtilitiesCS` (`:1-5`). There is no `Microsoft.Office.Interop.Outlook` reference, no `MAPIFolder`, no
`Store`, no `Application`. The legacy `CboFolders` owner-draw machinery and the `FolderHierarchyBuilder.Build`
call were decommissioned by issue #351, as the file's own comment at `:11-12` records. The filtering
predicate now lives in F12-owned `BreadcrumbBridgeCoordinator` / `FolderBreadcrumbBridgeRouter`
(`BreadcrumbBridgeCoordinator.cs:190,193,196` delegate to `_router`).

**The separation point is `QfcItemController.EventHandlers.cs:167`** (`_itemViewer.SearchText` — the viewer's
only contribution to the search is to surface the raw textbox string) **and
`QfcItemController.EventHandlers.cs:172-173`** (`ClearFolderItems()` / `SetFolderItems(folders)` — the viewer
receives an already-computed `string[]`). Nothing needs to be extracted from this file.

### D8 — `ItemViewer.DisplayState.cs` is not a state machine

`issue.md` and the brief describe it as a state-holding surface. It declares **zero fields**, has zero
invariants, and has no member whose legal input set depends on any other member. Every property is a pair of
expression-bodied accessors over a foreign `Control`'s property; the state lives in
`System.Windows.Forms.Label.Text` / `TextBox.Text` / `Control.BackColor`.

`.claude/rules/general-unit-test.md` § Scenario Completeness requires "state transitions for stateful
components." Correctly applied here, that obligation **resolves to round-trip and normalization coverage**:
`Control.Text` normalizes an assigned `null` to `string.Empty` and reads back `""` rather than `null` on a
fresh control; and `Control.BackColor`'s getter returns the *effective* value, so the initial read is the
inherited default rather than `Color.Empty` and assigning `Color.Empty` resets rather than stores. Assert
these once each on a representative member, with an in-code comment recording that the normalization is the
framework's and not this file's. Do not manufacture a state machine.

If F16's capstone expects a state-transition artefact for this file, the correct answer to record is: the
display state of an `ItemViewer` is a ten-tuple of independent unconstrained control property values, the
transition relation is the full cross-product, and there are no illegal transitions because there are no
invariants to violate.

### D9 — Several in-scope files have ZERO branch points, so the 75% branch gate is vacuous for them

Verified by full read: `ItemViewer.WebViewThread.cs` has 9 coverable lines and **no** `if`, `?:`, `?.`, `??`,
`&&`, `||`, loop, or `try`. `ItemViewer.Commands.cs` (32 coverable lines, 109 physical) and
`ItemViewer.DisplayState.cs` (23 coverable lines, 81 physical) likewise contain no branch point and no
lambda.

**F1's harness must report a file with `branches-valid = 0` as N/A for branch, never 0%, and it must never
count as a branch failure.** This is the branch-side analogue of the line-side `interface-only` rule the
epic already mandates at `epic.md:533-536`, which is stated for the line denominator only. The
discriminator must be `<condition>`-child count, not the `branch-rate` attribute. **Without this rule these
three files cannot pass the 75% branch gate, because they have no branches to cover.** Recorded as a
**blocking upstream requirement on F1 (#432)** in § Cross-Child Notes.

### D10 — The reported baseline coverage figures are corrupted by open issue #441

`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122` selects `.//class` and then `.//lines/line`.
The descendant axis matches both `class/lines/line` and `class/methods/method/lines/line`, double-counting
every line into `$totalLines` (`:123`) and hence into `LinesValid`/`LineRate` (`:137-143`). The per-file
merge path at `:181,219` correctly uses the child axes `./class[@filename]` and `./lines/line`, so a working
precedent exists inside the same script.

Observed on this feature's own files: `ItemViewerExpanded.cs` reports `line-rate="0.390244"` (32/82) but
recomputes to **37.74%** — 40 of 106 distinct `<line>` children — while its `branch-rate="0.083333"` (1 of 12
outcomes) is exact. `ItemViewerExpanded.Designer.cs` reports `line-rate="0.9950980392156863"` (203/204, one
uncovered line) against an element that enumerates three uncovered lines.

**Acceptance for this feature is measured against F1's recomputed per-file figure, derived from
deduplicated `<line>` children, and never against a `<class>` `line-rate` attribute.** Any `line-rate`
attribute quoted anywhere in F14's artifacts must carry an explicit "#441 — unreliable" annotation. This is
most acute for `ItemViewer.WebViewThread.cs`, whose 9-line denominator is small enough that the inflation is
not lost in aggregation.

### D11 — `ItemViewer.cs`'s last remaining branch point is in dead code

`ItemViewer.cs:171-175` (`MenuItem_CheckedChanged(object, EventArgs)`), `:177-187`
(`MenuItem_CheckedChanged(ToolStripMenuItem)`), and `:205` (`MoveOptionsMenu_Click`) have **no caller and no
designer wiring anywhere in the solution**. `ItemViewer.Designer.cs` wires exactly one handler, at `:256`
(`this._l0v2h2_WebView2.ParentChanged += new System.EventHandler(this.L0v2h2_WebView2_ParentChanged);`), and
it is not one of these; `ItemViewer`'s constructor (`:23-30`) does not call them, unlike
`ItemViewerExpanded`'s (`ItemViewerExpanded.cs:24-27`). The same three members in `ItemViewerExpanded` **are**
wired four times (`ItemViewerExpanded.Designer.cs:171,180,189,198`).

After seam S1 removes the five geometry branch points, `:179` (`if (menuItem.Checked)`) is the **sole**
decision point remaining in `ItemViewer.cs`, and it sits in a method nothing calls. This materially changes
the shape of the test plan: the planner must choose between covering dead code through seam S2, or deleting
the three unreferenced private members (which the compiler proves is behavior-neutral) and reporting the file
as zero-branch under the D9 rule. **F14 must not "fix" `ItemViewer` by adding the missing wiring** — the
wired path in `ItemViewerExpanded` is the defective one (tracked as issue #486), so adding the wiring would
import the defect.

### D12 — Seam extraction SHRINKS `ItemViewer.cs`

`epic.md:426-433` frames F14 as the child where the STA clause and the 500-line risk are most likely to
apply. For file size that framing does not hold. Seam S1 removes ~48 source lines
(`ItemViewer.cs:80-94`, `:98-107`, `:138-164`) and adds ~3, taking the file from **432 to ~390** — ~373 if
the three dead members of D11 are also deleted. `ItemViewerExpanded.cs` goes from 181 to ~126.
`ItemViewer.Breadcrumb.cs` is projected at ~344 from 298 after seams S3, still well inside its own historical
envelope of 399 lines before the issue #400 P9-T12 extraction. `ItemViewer.Commands.cs` (109),
`ItemViewer.DisplayState.cs` (81), and `ItemViewer.FolderSearch.cs` (74) receive no production addition at
all.

**No partial split is required for any file in scope.**

## Seam Design

The epic's hierarchy is interface seam > injectable delegate > adapter (`epic.md:227-232`,
`.claude/rules/csharp.md` § DI Seams). Four seams are specified. Two files need no seam at all.

### S1 — `ControlColumnTrimmer` (the feature's only new production file)

`ItemViewer.cs:77-95`, `:97-107`, and `:137-164` are **verbatim duplicates** of
`ItemViewerExpanded.cs:69-87`, `:89-99`, and `:129-156`. Both files are F14-owned, so this is an intra-child
change: extract once, and have both call it. Planning it twice would produce two modules or a merge conflict
inside the same child.

New file `QuickFiler/Viewers/ControlColumnTrimmer.cs`, `internal static class ControlColumnTrimmer`:

- `internal static void RemoveColumnsRightOf(Control root, Control furthestRight, Control columnSpanTarget)`
- `internal static void RemoveControlsRightOf(Control root, Control furthestRight)`
- `internal static List<Control> ControlsRightOf(Control root, Control furthestRight)`

Each caller retains one expression-bodied wiring line, for example:

```csharp
public void RemoveControlsColsRightOf(Control furthestRight) =>
    ControlColumnTrimmer.RemoveColumnsRightOf(this, furthestRight, L0v2h2_WebView2);
```

`internal` suffices: `QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`.
No interface is warranted — there is one implementation, and the simplicity-first principle argues against an
abstraction with a single implementor.

`IItemViewer.cs:131` (`void RemoveControlsColsRightOf(Control furthestRight);`) **does not change**, so
`EfcItemController.cs:247` — the sole production call site, reached through `IItemViewer` — is untouched and
every `Mock<IItemViewer>` in `QuickFiler.Test` keeps compiling.

Obligations attached to this file:

- A `<Compile Include="Viewers\ControlColumnTrimmer.cs" />` entry in `QuickFiler/QuickFiler.csproj`, inside
  the `Viewers\` block, **CRLF preserved**, minimal adjacent hunks, no property or reference changes, no
  reordering of unrelated entries (`epic.md:594-617`). The project uses no globbing; the file will not
  compile without the entry.
- The **>= 90% line coverage** new-module target (`epic.md:488`, `epic.md:583-585`).
- Under 500 lines. Projected ~100-115.
- A ledger row appended in the same change that adds the `<Compile Include>` entry (`epic.md:578-582`).

Effect: removes 5 of 6 branch points and ~50 coverable lines from `ItemViewer.cs`, and 5 branch points and
~50 coverable lines from `ItemViewerExpanded.cs`, and removes every `TableLayoutPanel` manipulation from both
`UserControl`s' test surfaces. Fixture style precedent for the extracted module's tests:
`UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs`, which already covers the `RemoveSpecificColumn`
extension this code calls.

### S2 — Widen the two `ItemViewerExpanded` menu handlers to `internal`

`MenuItem_CheckedChanged(ToolStripMenuItem)` (`ItemViewerExpanded.cs:169`) becomes `internal static` — it
reads no instance state, and `:166` continues to compile. `MenuItem_CheckedChanged(object, EventArgs)`
(`:163`) becomes `internal` and must stay an instance method, because the designer wires it as
`new System.EventHandler(this.MenuItem_CheckedChanged)` at `ItemViewerExpanded.Designer.cs:171,180,189,198`
and a static target would require editing the generated designer file.

Precedent: `QuickFiler/Controllers/QfcHomeController.cs:111` declares `internal async Task InitAsync(...)`
and `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:220` calls it directly.

This seam is **mandatory for the branch gate on `ItemViewerExpanded.cs`**. The `true` arm of
`ItemViewerExpanded.cs:171` (`if (menuItem.Checked)`) is unreachable through every public path, because
`ToolStripMenuItemCb`'s shadowing `Checked` setter (`ToolStripMenuItemCb.cs:32-51`) never assigns
`base.Checked`, while the handler's parameter is typed as the base `ToolStripMenuItem` and therefore reads
`base.Checked`, which is permanently `false`. Covering it requires invoking the seam directly with a plain
`ToolStripMenuItem` whose base `Checked` is `true`. Without it the file sits at 1/2 = 50% branch and fails
the 75% gate regardless of line coverage.

Note `UtilitiesCS` grants **no** `InternalsVisibleTo` to `QuickFiler.Test`
(`UtilitiesCS/Properties/AssemblyInfo.cs:18-20`; `epic.md:619-631`), so no `UtilitiesCS` internal may be used
by any F14 test. This is not a practical constraint: the `UtilitiesCS` members these files use —
`Control.ForAllControls`, `TableLayoutPanel.RemoveSpecificColumn`
(`UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs:106`), `Initializer.GetOrLoad`
(`UtilitiesCS/HelperClasses/Initializer.cs:103`), `IFolderHierarchyProvider`, `FolderRow`,
`BreadcrumbArrowDirection` — are all public.

### S3 — Two injectable-delegate seams in `ItemViewer.Breadcrumb.cs`, added as sibling overloads

- **S3a — drop-down host construction.** `ConfigureBreadcrumbDropDown(env, initializer)`
  (`ItemViewer.Breadcrumb.cs:142-177`) constructs the concrete `BreadcrumbDropDownHost` at `:158-168`.
  Extract that construction behind a private factory field defaulting to the current `new`, plus an
  `internal` overload accepting the factory and the two geometry `Func<Rectangle>` delegates. This makes the
  environment-identity idempotence branch at `:147-153` and the wiring at `:169-176` reachable without a real
  `CoreWebView2Environment`.
- **S3b — the `CoreWebView2` read.** `CreateCollapsedBreadcrumbCandidate` (`:77-98`) has exactly one
  irreducible line, `:82` (`_l0vhBreadcrumb_WebView2.CoreWebView2`). Extract it behind a private
  `Func<CoreWebView2>` and add an `internal` overload accepting it, so `:84-97` becomes reachable with a
  `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))` stand-in — the technique already used for
  `CoreWebView2Environment` at `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:30-31`.

**Both are added as sibling overloads that accept the collaborator**, exactly as
`ItemViewer.Breadcrumb.cs:40-43`, `:65-67`, and `:179-183` already do, keeping the zero-/one-argument
production wrapper unchanged so no call site moves. This is the sanctioned precedent: `:40-43` was added
under issue #400 P9-T28 as a *"narrow internal testability overload"*.

**Do NOT retype the Designer-backed fields** `L0vhBreadcrumb_WebView2`, `TopicThread`, `SentDate`,
`L0v2h2_WebView2`, or `MoveOptionsMenu`. Prior art records that a retyped Designer field breaks
reflection-injected tests and that injecting a router or collaborator is the working approach. The evidence:
`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:247-265` injects synthetic controls by concrete type
through the public property setters; `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:19-29`
pins `L0vhBreadcrumb_WebView2`'s declared type to the concrete
`Microsoft.Web.WebView2.WinForms.WebView2`, making any retyping a red test by construction; and
`QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs:328-473` demonstrates the working approach by
injecting a `Mock<IBreadcrumbDropDownHost>` through the existing overload seam at
`ItemViewer.Breadcrumb.cs:179-195`. Retyping would additionally break the production call site
`QfcItemController.ViewerSetup.cs:109`.

`FocusBreadcrumbCore` (`:211-221`) needs no seam: `Control.Focus()` on a never-shown handle-less control
returns `false` without side effects.

### S4 — One-token widening for `ItemViewer.FolderSearch.cs`

Fifteen of that file's seventeen members are reachable with no production change. The obstacle is the
**non-null** side of its seven `?.`/`&&`/`??` branches, which requires `ItemViewer.BreadcrumbCoordinator` to
be non-null. That property is `internal BreadcrumbBridgeCoordinator BreadcrumbCoordinator { get; private set; }`
at `ItemViewer.Breadcrumb.cs:25`; the getter is reachable via `InternalsVisibleTo`, but the setter is
`private` and is written only at `:59`. `BreadcrumbBridgeCoordinator` is `public sealed` with non-virtual
methods (`BreadcrumbBridgeCoordinator.cs:25`), so Moq cannot mock it.

**Primary — widen `ItemViewer.Breadcrumb.cs:25` from `private set` to `internal set`.** One token, F14-owned,
no new type, no new file, no csproj change, no public-surface change, zero behavior change. The test then
constructs the coordinator for real through its internal three-argument constructor
(`BreadcrumbBridgeCoordinator.cs:45-59`) with a `Mock<IWebViewMessenger>`, a
`Mock<IFolderHierarchyProvider>`, and `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()`
(`BreadcrumbUiDispatcher.cs:62-65`) — no `SynchronizationContext`, no `InitializeComponent`, no F12 lifecycle
machinery. Mock behavior must be **Loose**, not Strict, because the coordinator's constructor subscribes to
`IWebViewMessenger.MessageReceived` and builds a `FolderBreadcrumbBridgeRouter` from the provider.

**Fallback with zero production change:** drive
`InitializeBreadcrumbPipeline(provider, BreadcrumbPopupUiOperations.CreateForCurrentThreadTests())` under an
ambient `SynchronizationContext`. Cost: the test then executes `EnsureBreadcrumbLifecycle`
(`ItemViewer.Breadcrumb.cs:253-277`), `BreadcrumbMessengerHub`, `BreadcrumbCollapsedSurfaceController`, and
`EnsureBreadcrumbResourceOwnership` — all F12/F13 code — which weakens isolation
(`.claude/rules/general-unit-test.md` § Core Principles) and lets an F12 change break an F14 test. The
primary is preferred on isolation grounds; if the fallback is chosen, record the isolation cost in the
ledger row.

Rejected: reflection on the compiler-generated `<BreadcrumbCoordinator>k__BackingField`. Depending on a
compiler-generated name is brittle and buys nothing over the one-token widening.

**S4 and S3 both edit `ItemViewer.Breadcrumb.cs` at non-overlapping locations (`:25` versus `:77-98` and
`:142-177`). The plan must merge them into a single edit list for that file.**

### Files needing no seam

`ItemViewer.Commands.cs` and `ItemViewer.DisplayState.cs` need **no seam at all**. All eight backing controls
the commands file forwards to, and all nine the display-state file forwards to, already have public setters
on the primary partial at `ItemViewer.cs:334-423` and `:209-283` respectively, and none of the backing
controls (`SVGControl.ButtonSVG`, `ToolStripMenuItemCb`, `Label`, `TextBox`) requires a window handle, a
parent, a message pump, or an STA apartment. Adding a seam here would create production code subject to the
>= 90% new-file rule for zero coverage benefit and would change the shape of a public `IItemViewer`
implementation, which the no-behavior-change NFR forbids and which would ripple into F10's mocks.

`ItemViewer.WebViewThread.cs` needs one seam decision, not a structural one: `:35`
(`MoveOptionsMenu.ShowDropDown()`) shows a real popup window, which `epic.md:229-232` forbids outright in
unit tests, so it requires an injectable-delegate seam local to that file. Placing any new delegate field in
`ItemViewer.WebViewThread.cs` rather than in `ItemViewer.cs` keeps the covered field initialiser in that
file's own denominator; moving it would distort both files' per-file rates.

## Non-Functional Requirements

1. **No observable behavior change** to end-user QuickFiler flows (`epic.md:17`). Seams add overloads and
   widen visibility; they do not alter production call paths or defaults.
2. **Determinism and isolation.** MSTest, Moq, FluentAssertions, Arrange-Act-Assert. No temporary files, no
   external services, no live `Form`, no popup, no `Thread.Sleep`/`Task.Delay`/wall-clock wait, no dependence
   on mutable global state (`epic.md:243-248`, `.claude/rules/general-unit-test.md`).
3. **No production file over 500 lines** after refactor. Generated `*.Designer.cs` files are exempt as
   generated code (`epic.md:250-255`, `.claude/rules/general-code-change.md` § File Size Limit).
4. **Full C# toolchain green** in final form, in order: `csharpier .` → analyzer build → nullable build →
   `vstest.console.exe ... /EnableCodeCoverage` (`CLAUDE.md` § CUT3).
5. **Repository-wide coverage retained or improved** against the measured baseline (`epic.md:490`,
   `epic.md:492-498`). The absolute repository-wide floors in `CLAUDE.md` (80%) and
   `.claude/rules/general-unit-test.md` (85%) remain the standing repository aspiration and are explicitly
   **not** this child's gate, because the baseline was already below them before the epic began.

## Upstream Dependency — F1 (#432) and the Phase 0 Halt Gate

F1 `quickfiler-coverage-ledger` (issue #432, wave 0) delivers the per-file coverage harness and the ledger at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. **F1's feature folder does not exist on
this branch at spec-authoring time.** This spec is therefore written to consume F1's *contract* and does not
depend on F1's current content.

The contract F14 consumes:

- A repeatable per-file line **and** branch coverage report derived from the Cobertura output, whose per-file
  rates are recomputed from deduplicated `<line>` children and never read from a `<class>` `line-rate`
  attribute (`epic.md:257-264`, `epic.md:530-536`, and D10 above).
- The three ledger buckets `testable`, `ratified-exempt`, and `interface-only / not-measured`, with the
  classification **rules** stated so a file created mid-wave can be classified without re-running F1
  (`epic.md:509-522`, `epic.md:576-582`).
- The N/A-not-0% reporting rule, extended to the branch denominator per D9.

**Phase 0 halt gate (required).** Before any measurement task in F14's plan runs, Phase 0 must verify that
all three of the following are present on the branch, and **halt** if any is missing:

1. F1's per-file coverage harness is available and runnable on this branch.
2. `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` exists and states the classification
   rules, not merely rows.
3. The harness reports a zero-`<condition>` file as branch **N/A** and a compiled file with no `<class>`
   element as line **N/A** — both verified against a concrete file, not assumed.

If item 3 is not satisfied, F14 cannot close: `ItemViewer.WebViewThread.cs`, `ItemViewer.Commands.cs`, and
`ItemViewer.DisplayState.cs` have no branches to cover, and `IItemViewer.cs` has no lines to cover. The
correct response is to halt and escalate to F1, not to write shape-assertion or synthetic-branch tests.

### Intra-child sequencing (not negotiable)

- **T0 — Remove `[ExcludeFromCodeCoverage]` from `ItemViewer.cs:20`** (and the now-unused
  `using System.Diagnostics.CodeAnalysis;` at `:5`, after verifying nothing else in the file uses it), and
  correct the four stale comments in the same change (see AC 10). **T0 must land in the same commit as at
  least one F14-owned test that constructs a real `ItemViewer` and the two `Dispose` tests for
  `ItemViewer.Designer.cs`**, per D2.
- **T0b — Run F1's harness and record the actual per-file line and branch rate for all seven newly visible
  files.** Every subsequent test task is justified against the measured gap, not against an assumed zero.
  Research established that "assume 0%" is wrong for `ItemViewer.cs` and `ItemViewer.Breadcrumb.cs` (ten
  existing harnesses execute them incidentally; the breadcrumb partial is plausibly 50-75% already) and
  correct for `ItemViewer.Commands.cs`, `ItemViewer.DisplayState.cs`, `ItemViewer.FolderSearch.cs`, and
  `ItemViewer.WebViewThread.cs` (no test executes any line of those four). Prune the case inventory against
  measured data.

## Acceptance Criteria

This section is authoritative for this document. Each item is verifiable and is checked off only after the
work satisfying it has been implemented and verified.

- [ ] AC1 — Every file in scope classified `testable` reaches **>= 80% line and >= 75% branch** coverage,
      measured with F1's per-file harness on this feature's branch, with the numeric per-file result committed
      under `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/evidence/qa-gates/`. Figures are
      taken from F1's recomputed per-file numbers, never from a `<class>` `line-rate` attribute (#441). A
      zero-`<condition>` file is reported N/A for branch, not 0%. Aggregate assembly coverage alone does not
      satisfy this criterion (`epic.md:257-264`, `epic.md:486-487`).
- [ ] AC2 — `[ExcludeFromCodeCoverage]` is removed from `QuickFiler/Viewers/ItemViewer.cs:20` and the seven
      files it was suppressing are genuinely covered per AC1, **unless** F1's ledger ratifies a specific
      irreducible remainder with a file-specific rationale meeting the irreducible-remainder standard. The
      removal lands in the same commit as at least one F14-owned test that constructs a real `ItemViewer`.
      Re-exempting the whole partial type — by attributing `ItemViewer.Designer.cs`, by attributing both
      partials, or by adding a `<Sources><Exclude>` entry — is prohibited. Re-exempting individual members is
      also prohibited, because per issue #457 a method-level attribute does not suppress hoisted lambdas.
- [ ] AC3 — `QuickFiler/Viewers/IItemViewer.cs` is classified `interface-only / not-measured` in F1's ledger,
      receives **no** `[ExcludeFromCodeCoverage]` attribute and no other edit, is reported **N/A** rather than
      0%, and has **zero** tests written for it. No shape-assertion or reflection-contract test is added for it
      (`epic.md:509-522`).
- [ ] AC4 — `QuickFiler/Viewers/ItemViewer.Designer.cs` and
      `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` are each classified in F1's ledger per the ledger's
      generated-code rules; per deviation D3 that classification is **`testable`**, with the recorded
      structural caps (line ~98.5-99.95%, branch capped at 3/4 = 75% while `components` is never assigned).
      Neither file is edited by F14, and neither receives an exemption.
- [ ] AC5 — No production file in scope exceeds **500 lines** after refactor, and the generated
      `*.Designer.cs` files are recorded as exempt from that rule as generated code. Verified by line count on
      each file in the scope table plus any file this feature creates.
- [ ] AC6 — `QuickFiler/Viewers/ControlColumnTrimmer.cs` is created, is referenced by a
      `<Compile Include="Viewers\ControlColumnTrimmer.cs" />` entry added to `QuickFiler/QuickFiler.csproj`
      with CRLF preserved and minimal adjacent hunks, has a ledger row appended in that same change, and
      reaches **>= 90% line coverage** as newly created production code (`epic.md:488`, `epic.md:576-585`,
      `epic.md:594-617`). `IItemViewer.cs:131`'s signature is unchanged by the extraction.
- [ ] AC7 — All new and modified tests use **MSTest**, **Moq**, and **FluentAssertions**, follow
      Arrange-Act-Assert, and are independent, isolated, and deterministic: no temporary files, no external
      services, no live `Form`, no popup, no `Thread.Sleep`/`Task.Delay`/wall-clock wait, and no reliance on a
      cross-test disposal side effect. No `*.StaTests.cs` file is created in `QuickFiler.Test` (D5). Every test
      that constructs an `ItemViewer` installs a `SynchronizationContext` first and restores the previous
      context in `finally`/`[TestCleanup]`.
- [ ] AC8 — The full C# toolchain passes in order in its final form — `csharpier .`, the analyzer build, the
      nullable/`TreatWarningsAsErrors` build, and `vstest.console.exe ... /EnableCodeCoverage` — with the
      exact commands and results recorded as evidence. Repository-wide line coverage is measured before and
      after and is **retained or improved** against the measured baseline; the before/after figures are
      recorded in the evidence artifact (`epic.md:490`, `epic.md:492-498`).
- [ ] AC9 — No observable behavior change to QuickFiler flows. Every production edit is confined to seam
      addition (new overloads with unchanged production wrappers), visibility widening, extraction of verbatim
      duplicate bodies into `ControlColumnTrimmer`, comment corrections, and removal of the coverage attribute.
      No public `IItemViewer` member is added, removed, retyped, or renamed; no Designer-backed property is
      retyped; no event wiring is added or removed.
- [ ] AC10 — The three stale `[ExcludeFromCodeCoverage]` comments at `ItemViewer.Commands.cs:10`,
      `ItemViewer.DisplayState.cs:9-10`, and `ItemViewer.FolderSearch.cs:17`, and the header at
      `ItemViewer.WebViewThread.cs:8-12`, are corrected in the same change that removes the attribute, so they
      no longer assert an exemption that no longer exists. The CS0579 note at `ItemViewer.DisplayState.cs:10`
      is retained (moved if the comment is rewritten) because it documents why per-partial exemption is
      impossible. These are in-scope for F14's own execution, not deferred (`epic.md:121-130`).
- [ ] AC11 — Issue **#438** (`quickfiler-search-keystroke-focus-steal`) is **not** fixed by this feature, and
      every test case that asserts the current `SetFolderDroppedDown(true)` → `FocusBreadcrumb()` behavior
      carries an in-code comment citing #438 and stating that it pins current behavior, so a future fix
      produces a legible red test rather than a silently corrected assertion.
- [ ] AC12 — Any latent defect surfaced during execution that is out of scope under the no-behavior-change
      NFR is promoted to a GitHub issue through the MCP promotion lifecycle rather than left as prose in the
      feature folder (`epic.md:538-543`). The already-promoted set — #486, #487, #488, #489, #490, #491 — is
      referenced, not re-promoted.

## Cross-Child Notes

### To F1 (#432) — BLOCKING

1. **A file with `branches-valid = 0` must report branch as N/A, never 0%, and must never count as a branch
   failure.** The discriminator is `<condition>`-child count, not the `branch-rate` attribute. `epic.md:533-536`
   states the analogous rule for the line denominator only. Live instances in F14's scope:
   `ItemViewer.WebViewThread.cs`, `ItemViewer.Commands.cs`, and `ItemViewer.DisplayState.cs`. **Without this
   rule those three files cannot pass the 75% branch gate and F14 cannot close.**
2. **The harness must recompute per-file rates from deduplicated `<line>` nodes and must never read the
   `<class>` `line-rate` attribute** (issue #441; root cause verified at
   `Invoke-MSTestWithCoverage.Helpers.ps1:121-122`; a correct precedent already exists in the same script at
   `:181,219`).
3. **Ledger rule for generated designers.** State the rule as *"generated `*.Designer.cs` partials are
   `testable`, exempt from the 500-line rule, and carry a recorded branch cap of 75% where `components` is
   never assigned"* — not as a blanket `ratified-exempt`. `epic.md:431` and `:440-443` currently label all
   seven designers "exempt-candidate"; two of them have now been measured and neither needs an exemption.
4. **`IItemViewer.cs` needs an `interface-only / not-measured` row**, and a compiled file with no `<class>`
   element must be reported N/A rather than synthesised as a 0% row.

### To F13 (#455) — FREEZE

- `BreadcrumbPopupUiOperations.CaptureCurrentOrTests()` (`BreadcrumbPopupUiOperations.cs:86-89`) and
  `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`BreadcrumbUiDispatcher.cs:62-65`) are **F14's only
  off-context test path**. F13 must not remove, rename, or change the semantics of either. Record as frozen
  contracts.
- **Advisory:** the self-referential closure at `ItemViewer.Breadcrumb.cs:158-164` — where the `host` local is
  captured by the `() => host.ControlHost?.Control.Focus()` closure at `:164` before `host` is assigned at
  `:159` — is safe **only** while no `BreadcrumbDropDownHost` constructor invokes its `returnFocus` delegate
  (verified against `BreadcrumbDropDownHost.cs:37-160`). F13 should not introduce constructor-time invocation
  of that delegate. This is a note, not a change request.

### To F12 — FREEZE

- `BreadcrumbItemViewerLifecycleCoordinator`'s six-argument constructor
  (`BreadcrumbItemViewerLifecycleCoordinator.cs:29-36`) is consumed verbatim at
  `ItemViewer.Breadcrumb.cs:268-275`. Any reordering or type change is a compile break in F14.
- `BreadcrumbBridgeCoordinator`'s internal three-argument constructor
  (`BreadcrumbBridgeCoordinator.cs:45-59`) is required to construct the coordinator that makes every
  non-null branch arm in `ItemViewer.FolderSearch.cs` reachable under seam S4. Record as a frozen contract.

### To F10 (#453) — ADVISORY

F10 owns all seven production call sites of `ItemViewer.WebViewThread.cs`'s members —
`QfcItemController.EventWiring.cs:87-90,139-146`, `.EventHandlers.cs:196-200`, `.Conversation.cs:221-233`,
`.FocusAndTheme.cs:293`, `.Navigation.cs:81-84`. F14 requests no change to any of them. **F10 and F14 must
not both attempt the UI-thread marshalling divergence fix now tracked as issue #489.** If F10 introduces
`Mock<IItemViewer>` at additional call sites, F14's numbers are unaffected, because a mock bypasses this file
entirely.

### To F15 — FREEZE

`ToolStripMenuItemCb`'s `Checked` setter raising `CheckedChanged` unconditionally, with no equality
short-circuit (`ToolStripMenuItemCb.cs:32-51`), is what makes F14's menu test cases work: it round-trips
`_checked` with no host and raises on every assignment, including no-change assignments. The candidate fix —
adding `base.Checked = value;` at `ToolStripMenuItemCb.cs:37` — is tracked as issue **#486** and **must not
be applied while F14 is in flight**. F14 must not edit `ToolStripMenuItemCb.cs`. F14's affected cases should
carry an in-code comment naming `ToolStripMenuItemCb.cs:35-49` so a future break is legible.

### To F7 — ADVISORY

`ItemViewerExpanded.cs`'s current 37.7% is incidental. It is produced entirely by F7-owned
`QfcHomeControllerTests` (`:149`, `:220`) via `QfcHomeController.cs:93`/`:133` →
`QfcFormViewer.Designer.cs:42`. If F7 seams away the live-form construction, that coverage collapses. **F14
must own its own construction fixture rather than depend on F7's.** The same applies to `ItemViewer.cs` and
`ItemViewer.Designer.cs`, whose incidental execution comes from six F12/F13-owned breadcrumb harnesses and
two F10-owned controller harnesses; F13/F12/F10 should be asked not to replace a live
`new QuickFiler.ItemViewer()` with a mock in those harnesses while F14 is in flight.

### F3 is NOT a dependency

A grep of `QuickFiler/Viewers/` for `KbdActions|KaChar|KaKey|KaStringAsync|IMailItemActions` returns **zero
matches**. Issues **#444** and **#445** therefore have no bearing on any file in F14's scope. The only
keyboard coupling near F14 is the controller-side subscription at `QfcItemController.EventWiring.cs:81-83`,
which F14 does not touch.

### Conflict risk not recorded in `epic.md`

`epic.md:638` records issue **#400** (`2026-07-21-quickfiler-folder-selector-dropdown-400`) as overlapping
F13 only. That is incomplete: #400's live remediation plan explicitly authorises edits to
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`
(`remediation-plan.2026-07-21T21-37.md:626` P9-T12, `:725` P9-T28) and it owns eight of the test harnesses
that construct `ItemViewer`. **#400 overlaps F14, not only F13.** F14 must read the current merged state of
#400 before planning and must not assume the file shapes recorded in the research artifacts survive a #400
merge.

## In Scope But Do Not Fix — Issue #438

Issue **#438** (`quickfiler-search-keystroke-focus-steal`, open, bug, High/Blocker) enters through
`ItemViewer.FolderSearch.cs:31-32`. The chain is verified end to end:

```
TxtboxSearch.TextChanged
  -> ItemViewer.FolderSearch.cs:60-64   (SearchTextChanged, the subscription seam)
  -> QfcItemController.EventWiring.cs:77-79   TextBoxSearch_TextChanged
  -> QfcItemController.EventHandlers.cs:164-178
       :172 ClearFolderItems()         -> FolderSearch.cs:34
       :173 SetFolderItems(folders)    -> FolderSearch.cs:20
       :177 SetFolderDroppedDown(true) -> FolderSearch.cs:31-32
  -> ItemViewer.Breadcrumb.cs:223-235   SetBreadcrumbDropDownState(true)
       :227-229  FocusBreadcrumb()     <- the unconditional focus on open
```

The defect is the "droppedDown implies focus" coupling inside `SetBreadcrumbDropDownState`, reached once per
keystroke. `ItemViewer.FolderSearch.cs` itself contains no defect; it is a faithful forwarder.

**F14 must NOT fix #438.** The epic's no-behavior-change NFR forbids it, and a correct fix reaches into
`BreadcrumbItemViewerLifecycleCoordinator.SetDroppedDown`, which is F12-owned and out of bounds for this
child.

**F14 must also not cement the defect.** Every test case that asserts the current
`SetFolderDroppedDown(true)` → `FocusBreadcrumb()` behavior — and the paired closed-state case — must carry an
in-code comment citing #438 and stating explicitly that the assertion pins *current* behavior. Whoever
schedules #438 should expect one or two red F14 tests and treat them as the intended signal, not as tests to
be "corrected".

The same annotation discipline applies to issue **#440**
(`breadcrumb-left-right-arrow-parent-child-navigation`), which targets the ternary at
`ItemViewer.Breadcrumb.cs:246`: cases pinning the Right→`Keys.Right` / Left→`Keys.Left` mapping must cite
#440 in a comment, and the `FolderKeyDown` case in `ItemViewer.FolderSearch.cs` should assert only that the
handler field is invoked, not the mapping.

## Latent Defects Already Promoted — Reference Only, Do Not Re-Promote

| Issue | Subject | Relationship to F14 |
| --- | --- | --- |
| **#486** | Move-option menu defects (`ToolStripMenuItemCb` `Checked` shadow; `ItemViewer`'s dead handlers versus `ItemViewerExpanded`'s wired ones) | F15-owned. Freeze request above. |
| **#487** | `ParentChanged` handler `Console.WriteLine` and unguarded cast (`ItemViewer.cs:168`, `:173`; `ItemViewerExpanded.cs:160`, `:165`) | Out of scope under the no-behavior-change NFR. |
| **#488** | Breadcrumb pipeline lifecycle defects (host leak on environment change, theme lost off the UI thread, silently discarded second provider, non-atomic initialisation, container created after disposal) | Out of scope. |
| **#489** | UI-thread marshalling divergence across `ItemViewer.WebViewThread.cs`'s callers (WinForms `Control.Invoke` versus WPF `Dispatcher.InvokeAsync` versus unguarded) | F10 advisory above. |
| **#490** | Display and folder contract defects (`SetFolderItems` appends rather than sets; `FocusSubject` targets a non-selectable `Label` and discards its result; ten ungrouped display projections; nullability erasure at the boundary) | Out of scope. |
| **#491** | `QuickFiler.Test` contains a live `Form1` constructing three `ItemViewer` instances, plus test-only production surface | Out of scope; no test instantiates `Form1` today. |
| **#441** | Cobertura post-processing double-counts `<line>` nodes | Binding on measurement; see D10. |
| **#457** | `[ExcludeFromCodeCoverage]` does not suppress hoisted lambdas — the leak is method-level only | Consequence: after removing the type attribute, do **not** re-exempt individual members, or the lambdas leak back into the denominator anyway. |
| **#230** | WinForms message-pump test seam | Not required by any file in F14's scope. |
| **#432** | F1 `quickfiler-coverage-ledger` | The upstream dependency. |

## Non-Goals

- Fixing issue #438, #440, #441, #457, #486, #487, #488, #489, #490, or #491.
- Any behavior change to end-user QuickFiler flows.
- Editing any F10, F12, F13, or F15 production file, or `QuickFiler/Viewers/IItemViewer.cs`.
- Editing `coverage.config`, `TaskMaster.runsettings`,
  `scripts/vscode/Invoke-MSTestWithCoverage*.ps1`, or `UtilitiesCS/Properties/AssemblyInfo.cs`.
- Editing either `*.Designer.cs` file, including regenerating them to move the six embedded SVG `byte[]`
  payloads (~5,338 of `ItemViewer.Designer.cs`'s 6,224 lines) into `.resx`.
- Introducing `[STATestClass]`/`[STATestMethod]` or any `*.StaTests.cs` file into `QuickFiler.Test`.
- Introducing a clock abstraction; no file in scope reads a clock.
- Changing repository-wide coverage thresholds, or meeting the absolute repository-wide floors that predate
  this epic.
- Removing the dead public surface on `ItemViewerExpanded` (a public-API change).

## Definition of Done

The `## Acceptance Criteria` section above is the authoritative checklist. The following are the completion
obligations that accompany it:

1. Every acceptance criterion is checked off with evidence recorded, per
   `.claude/skills/acceptance-criteria-tracking/SKILL.md`.
2. Per-file coverage evidence for all ten in-scope files plus `ControlColumnTrimmer.cs` is committed under
   `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/evidence/qa-gates/`, with an
   ISO-8601 `yyyy-MM-ddTHH-mm` timestamp, the exact command, and `EXIT_CODE`.
3. Baseline evidence (pre-change repository-wide and per-file figures) is committed under
   `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/evidence/baseline/`.
4. The final toolchain pass is recorded with the exact commands run and the statement that all four steps
   passed without errors in that pass.
5. `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` carries a row for every file in the
   scope table plus `ControlColumnTrimmer.cs`, with the bucket, target, measured figure, and any recorded
   structural cap.
6. The four cross-child freeze requests and the F1 blocking requirement are communicated to their owning
   children and recorded in the epic's cross-child notes.
7. Any latent defect surfaced during execution is promoted through the MCP promotion lifecycle.

## Seeded Test Conditions (from the promoted issue, reconciled against research)

These are planning inputs, not acceptance criteria. The `## Acceptance Criteria` section above is the only
checkbox block in this document and is the authoritative AC source.

1. Unit coverage for command dispatch and display-state round-trip and normalization. **Reconciled:**
   folder-search "filtering" does not exist in this scope (D7); display state is not a state machine (D8).
2. Breadcrumb ordering invariants across the WebView2 thread boundary. **Reconciled:** determinism here is a
   scheduling problem, not a time problem; use ambient-`SynchronizationContext` inline dispatch and the
   existing drainable test context, not a fake clock (D6).
3. Expanded-viewer population and teardown paths, currently 37.74% line / 8.33% branch by recomputation
   (D10), including the `Dispose(false)` path that lifts both designers from 50% to 75% branch (D3).
4. **Struck:** STA-scoped construction in a dedicated `*.StaTests.cs` file. Disproved by D5; no STA test is
   required or permitted in this feature.
