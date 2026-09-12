# Research — `QuickFiler/Viewers/EfcViewer.cs` (F9 / issue #452, epic #136)

- Feature: `2026-08-07-quickfiler-efc-form-item-controller-coverage-452`
- Epic child: F9 (wave 1), parent epic issue #136
- Target file: `QuickFiler/Viewers/EfcViewer.cs` (162 lines)
- Partner file: `QuickFiler/Viewers/EfcViewer.Designer.cs` (4,277 lines)
- Research date: 2026-08-07
- Scope: this artifact covers **only** `EfcViewer.cs` and the disposition of `EfcViewer.Designer.cs`.
  `EfcFormController.cs` and `EfcItemController.cs` are covered by their own artifacts.

---

## 0. Executive summary

1. `[ExcludeFromCodeCoverage]` is present at `QuickFiler/Viewers/EfcViewer.cs:20`, on the
   **partial type declaration**. In C# an attribute on any part of a partial type applies to the
   whole type. It therefore suppresses instrumentation of `EfcViewer.Designer.cs` as well. This is
   confirmed empirically: neither `EfcViewer.cs` nor `EfcViewer.Designer.cs` appears in the
   committed Cobertura report, while un-attributed designers such as
   `QuickFiler\Viewers\BayesianPerformanceViewer.Designer.cs` do appear.
2. **Removing the attribute exposes roughly 2,000 additional measurable lines** in
   `EfcViewer.Designer.cs`. If nothing constructs an `EfcViewer`, those land at 0% and
   repository-wide line coverage falls — a direct AC9 failure. This is the single largest planning
   consequence for F9 and it is not visible from the file inventory.
3. The brief's constraint "Forms … are NOT [permitted]" is **contradicted by live, passing,
   in-`QuickFiler.Test` precedent**. `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:21-47`
   constructs a real `Form` (`BayesianPerformanceViewer`) on a dedicated STA thread, never shows it,
   and disposes it — and that single construction is why
   `BayesianPerformanceViewer.Designer.cs` reports **99.14% line coverage**. Section 9 documents
   this in full; it needs a maintainer decision before the plan is fixed.
4. Even under the strictest reading (no `Form` may be constructed), `EfcViewer.cs` can reach
   **~84% line** coverage using `FormatterServices.GetUninitializedObject` — a technique already
   used 25+ times in this repository, including on `Form`-derived types. It **cannot** reach 75%
   branch, because the only branch in the file (`EfcViewer.cs:96`) has both false paths flowing into
   `base.ProcessCmdKey`, which is unreachable on an uninitialized instance. Branch coverage, not
   line coverage, is what forces the decision.
5. Two genuine seam gaps exist and both are justified independent of the STA question:
   `SetController` takes the concrete `EfcFormController`, whose `EditFiltersMenuItem_Click`
   implementation calls `filters.Show()` (`EfcFormController.cs:561-566`) — a popup, and therefore a
   unit-test-policy violation if ever invoked with the real type.
6. Both `SetController` and `EditFiltersMenuItem_Click` are **dead code with an armed
   NullReferenceException**. See section 10 (defect L1).
7. Open defect #439 touches exactly one member of this file (`BreadcrumbWebView`, line 92), and that
   member contains no lineage logic at all. `EfcViewer.cs` is effectively #439-safe; the risk sits in
   `EfcFormController` and `BreadcrumbBridgeRouter`.

---

## 1. Current-state structural map

`EfcViewer` is `public partial class EfcViewer : Form` (`EfcViewer.cs:21`). It has no interface.
There is no `IEfcViewer` anywhere in the repository (verified by grep across `QuickFiler/**/*.cs`).

### 1.1 Member inventory (`EfcViewer.cs`)

| # | Member | Lines | Category | Accessibility |
|---|---|---|---|---|
| M1 | `EfcViewer()` ctor | 23–30 | Form lifecycle (calls `InitializeComponent`) | public |
| M2 | `log` static field initializer | 32–34 | Dead field (never read) | private static readonly |
| M3 | `_context` field + `UiSyncContext` getter | 36–40 | Property accessor (captured state) | public get |
| M4 | `_uiScheduler` field + `UiScheduler` getter | 42–46 | Property accessor (captured state) | public get |
| M5 | `_formController` field | 48 | Field | private |
| M6 | `SetController(EfcFormController)` | 50–53 | Wiring setter — **zero callers** | internal |
| M7 | `_keyboardHandler` field + `KeyboardHandler` getter | 55–59 | Property accessor | internal get |
| M8 | `SetKeyboardHandler(IQfcKeyboardHandler)` | 61–64 | Wiring setter | public |
| M9 | `_tipsLabels` field + `TipsLabels` getter | 66–70 | Property accessor exposing controls | public get |
| M10 | `InitTipsLabelsList()` | 72–86 | Real logic (control-collection assembly) | private |
| M11 | `BreadcrumbWebView` expression-bodied property | 88–92 | Property accessor exposing a designer control | internal get |
| M12 | `ProcessCmdKey(ref Message, Keys)` | 94–105 | Form lifecycle override + real branching logic | protected override |
| — | commented-out `InitMenuItems` / `MenuItem_*` blocks | 107–155 | Dead comments (not coverable) | — |
| M13 | `EditFiltersMenuItem_Click(object, EventArgs)` | 157–160 | Event handler — **zero subscribers** | private |

### 1.2 Category separation requested by the brief

- **Designer-generated wiring**: none in `EfcViewer.cs`. All of it lives in `EfcViewer.Designer.cs`.
  Notably `EfcViewer.Designer.cs` contains **no event wiring at all** — a grep for `+=` over the
  whole 4,277-line file returns zero matches. All event subscription happens in
  `EfcFormController.cs:388-401`.
- **Form lifecycle overrides**: M1 (ctor) and M12 (`ProcessCmdKey`). `Dispose(bool)` is in the
  Designer file (`EfcViewer.Designer.cs:18-25`).
- **Event handlers**: M13 only, and it is unsubscribed.
- **Property accessors exposing controls**: M9 (`TipsLabels`, nine `Label`s), M11
  (`BreadcrumbWebView` → `FolderListBox`, a `Microsoft.Web.WebView2.WinForms.WebView2` declared at
  `EfcViewer.Designer.cs:4250`).
- **Real logic**: M10 (`InitTipsLabelsList`, an ordered nine-element control collection) and the
  guard expression in M12 (`EfcViewer.cs:96`) — the only branch in the file.

### 1.3 Consumers (who depends on this type)

| Consumer | Site | Member used |
|---|---|---|
| `Helper Classes/EfcViewerQueue.cs:83` | `return new EfcViewer();` | ctor — the **only** construction site in the compiled tree (F4-owned) |
| `Controllers/EfcFormController.cs:35, 55, 132` | ctor params + `private EfcViewer _formViewer` | concrete-type field |
| `Controllers/EfcFormController.cs:229, 240` | `_formViewer.TipsLabels` | M9 |
| `Controllers/EfcFormController.cs:420, 436, 452, 468, 705, 736, 744, 764, 790, 1035` | `_formViewer.UiSyncContext` | M3 |
| `Controllers/EfcFormController.cs:837` | `_formViewer.BreadcrumbWebView` | M11 (breadcrumb host wiring — #439 path) |
| `Controllers/EfcHomeController.cs:264` | `private EfcViewer _formViewer` | concrete-type field |
| `Controllers/EfcHomeControllerDependencies.cs` (8 sites) / `EfcHomeControllerDependencyFactories.cs` (6 sites) | `EfcViewer viewer` parameters | F8-owned; do not edit |
| `Controllers/KeyboardHandler.cs:35-39` | `KeyboardHandler(EfcViewer viewer, …)` → `viewer.SetKeyboardHandler(this)` | M8 (F3-owned) |
| — | **`SetController` (M6): no caller anywhere** | see defect L1 |
| — | **`EditFiltersMenuItem_Click` (M13): no subscriber anywhere** | see defect L1 |

---

## 2. Measurement baseline and denominator

### 2.1 Current measured state

`EfcViewer.cs` and `EfcViewer.Designer.cs` are **absent** from
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.
Absence is caused by the class-level attribute, not by low coverage. They are **unmeasured, not
covered** — the issue's framing is correct.

Positive controls proving the folder is instrumented and that designer files do appear when not
suppressed (all from the same report):

| Cobertura entry | line-rate | branch-rate |
|---|---|---|
| `QuickFiler\Viewers\ItemViewerExpanded.Designer.cs` | 0.9951 | 0.50 |
| `QuickFiler\Viewers\BayesianPerformanceViewer.Designer.cs` | 0.9914 | 0.50 |
| `QuickFiler\Viewers\ToolStripMenuItemCb.Designer.cs` | 0.7273 | 0.75 |
| `UtilitiesCS\…\ConfigViewer.Designer.cs` | 0.9960 | 0.75 |
| `UtilitiesCS\…\FolderRemapViewer.Designer.cs` | 1.0 | 1.0 |

Two facts follow. (a) A `*.Designer.cs` file reaches ~99–100% line coverage **automatically** the
moment its owning control/form is constructed once in any test. (b) A `*.Designer.cs` file's
branch-rate is typically **0.50**, because `Dispose(bool)`'s `disposing && (components != null)`
condition is only ever exercised in one direction (`components` is never assigned in these
designers — confirmed for `EfcViewer.Designer.cs`, where `components` is initialized to `null` at
line 12 and never reassigned).

### 2.2 Estimated coverable-line inventory for `EfcViewer.cs`

The harness is `scripts/vscode/Invoke-MSTestWithCoverage.ps1` driving `dotnet-coverage` with
`coverage.config` (not coverlet). The `<line>` model includes brace-only lines: verified against the
`ItemViewerExpanded.Designer.cs` entry, where `Dispose(bool)` reports 7 lines for a 7-physical-line
body including both braces.

Estimated `<line>` count per member (to be replaced with F1's measured numbers):

| Member | Est. lines | Reachable without constructing the form? |
|---|---|---|
| M1 ctor | 6 | **No** |
| M2 `.cctor` | 1 | Probably yes (type initializer runs on allocation) — verify |
| M3 `get_UiSyncContext` | 1 | Yes |
| M4 `get_UiScheduler` | 1 | Yes |
| M6 `SetController` | 3 | Yes |
| M7 `get_KeyboardHandler` | 1 | Yes |
| M8 `SetKeyboardHandler` | 3 | Yes |
| M9 `get_TipsLabels` | 1 | Yes |
| M10 `InitTipsLabelsList` | ~13 | Yes |
| M11 `get_BreadcrumbWebView` | 1 | Yes |
| M12 `ProcessCmdKey` | 10 | 9 of 10 — line 104 is **not** reachable |
| M13 `EditFiltersMenuItem_Click` | 3 | Yes (with the S1 seam) |
| **Total** | **~44** | **~37 (84%)** |

Branch inventory for `EfcViewer.cs`: exactly one branching line, `EfcViewer.cs:96`
(`(_keyboardHandler is not null) && (keyData.HasFlag(Keys.Alt))`), which `dotnet-coverage` reports
as two jump conditions / four outcomes. Only the two "both true" outcomes are reachable without a
constructed form; the other two flow to line 104.

**Therefore: ~84% line but ~50% branch without form construction. AC1 passes, AC2 fails.**

### 2.3 Denominator impact of removing the attribute

`EfcViewer.Designer.cs` is 4,277 physical lines. `InitializeComponent` spans lines 33–4239; roughly
600 of those lines are `byte[]` array-literal continuation lines for five `SvgResource.Data`
assignments (lines 253–866), which contribute few sequence points. A conservative estimate is
**1,500–2,500 coverable lines**. Against a repository baseline line rate of 70.19% (recorded by
#424 and cited in the epic's Coverage-Target Reconciliation), adding ~2,000 uncovered lines is a
measurable repository-wide regression. F9's plan must handle this explicitly; it cannot be deferred
to F16.

**Measurement caveat (AC1/AC2/AC9).** Open issue #441 reports that Cobertura post-processing
double-counts `<line>` nodes. F9 must confirm its per-file numbers are not inflated before citing
them as acceptance evidence — spot-check one file's `<line>` count against the source.

---

## 3. Candidate approaches

Two viable approaches were evaluated. Both remove `[ExcludeFromCodeCoverage]` from `EfcViewer.cs:20`.

### Approach A (recommended) — one STA construction + uninitialized-instance unit tests

Add a dedicated `QuickFiler.Test/Viewers/EfcViewer.StaTests.cs` that constructs one real `EfcViewer`
on a dedicated STA thread, never shows it, and disposes it in a `finally`, following
`QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:21-47` exactly. Everything
that does not require a constructed form is covered in a plain `QuickFiler.Test/Viewers/EfcViewerTests.cs`
using `FormatterServices.GetUninitializedObject`.

Result: `EfcViewer.cs` at ~100% line / ~100% branch; `EfcViewer.Designer.cs` at ~99% line / ~50%
branch, requiring **no** exemption attribute and **adding** ~2,000 covered lines to the repository.

- Advantages: satisfies AC1, AC2, AC3 and materially helps AC9; requires no edit to generated code;
  uses an existing in-`QuickFiler.Test` pattern; covers the constructor, which no seam can reach.
- Limitations: depends on `new EfcViewer()` succeeding headlessly (section 8 risk register); requires
  the "no Forms in tests" constraint to be relaxed to the repo's actual practice (section 9); the
  Designer file's ~50% branch-rate still needs a ledger classification (section 6).
- Alignment: matches `BayesianPerformanceController.TestSupport.cs`, `ProgressViewer_Tests.cs`,
  `ConfigViewer_Tests.cs`, `FolderSelector_Tests.cs`, and `QuickFiler.Test/SetupAssemblyInitializer.cs`
  (which already calls `Application.EnableVisualStyles()` / `SetCompatibleTextRenderingDefault(false)`
  — an `[AssemblyInitialize]` that exists precisely so real controls can be constructed).

### Approach B (fallback) — no form construction at all

Cover everything reachable via `GetUninitializedObject`, add a `ProcessCmdKeyBase` virtual seam
(section 4, S2) to make the `base.ProcessCmdKey` fall-through testable through an uninitialized
test-double subclass, and exempt the Designer file's two generated methods with **method-level**
`[ExcludeFromCodeCoverage]` on `InitializeComponent` and `Dispose(bool)`.

Result: `EfcViewer.cs` at ~82% line (ctor's 6 lines and the 1-line seam wrapper uncovered) /
100% branch. `EfcViewer.Designer.cs` stays out of the denominator.

- Advantages: no `Form` is ever constructed; fully satisfies the brief as literally written; no
  dependency on the WebView2/SVG construction risk.
- Limitations: forfeits ~2,000 lines of free coverage; requires editing a Visual-Studio-generated
  file (Visual Studio regenerates `InitializeComponent` and will silently drop the attribute — a
  durability defect); **no precedent** — a grep for `ExcludeFromCodeCoverage` across all
  `**/*.Designer.cs` in the repository returns zero matches; adds a production-code seam
  (`ProcessCmdKeyBase`) whose only purpose is testability, which is exactly the shape a maintainer
  previously scrutinised.

### Recommendation

**Approach A**, gated on a Phase-0 spike (section 8) that proves `new EfcViewer()` succeeds
headlessly, with Approach B as the pre-authorised fallback if the spike fails. Both approaches take
the same S1 seam and the same normal-test list; only the STA file and the Designer disposition
differ, so a spike failure costs one plan phase, not a re-plan.

### Rejected alternatives (brief)

- **`coverage.config` `<Sources>` exclusion of `.*\.Designer\.cs`.** Rejected: `coverage.config` is a
  repo-root shared file (guaranteed cross-child conflict), it would remove already-covered designer
  lines repo-wide and thus *lower* repository coverage, and `.claude/rules/general-unit-test.md`
  § Coverage Exclusion Policy makes a production-path exclude a Blocking finding.
- **Attributing only the Designer partial declaration.** Not possible: attributes on any part of a
  partial type apply to the whole type. This is why the current attribute already hides the Designer.
- **Extracting the Alt-key predicate to a host-neutral policy class.** Rejected: it moves the four
  covered branch outcomes *out* of `EfcViewer.cs`, leaving only the un-coverable `if`/`else` pair
  behind, which makes the file's branch rate worse, not better. It also adds a csproj entry for no
  gate benefit.
- **Reflectively initialising `Control.propertyStore` so `base.ProcessCmdKey` is safe on an
  uninitialized instance.** Rejected: depends on private `System.Windows.Forms` internals and is
  fragile across .NET Framework servicing updates.

---

## 4. Seam plan

Seam hierarchy per `.claude/rules/csharp.md:49-53`: interface seam > injectable delegate > adapter.

### S1 (required, both approaches) — `IEfcViewerCommands` interface seam for `_formController`

**Problem.** `SetController` (`EfcViewer.cs:50`) takes the concrete `EfcFormController`, and
`EditFiltersMenuItem_Click` (`EfcViewer.cs:159`) calls straight through to it. The real
implementation at `EfcFormController.cs:561-566` is:

```csharp
public void EditFiltersMenuItem_Click(object sender, EventArgs e)
{
    var filters = new ManageFilters();
    filters.LoadFilters(_globals);
    filters.Show();
}
```

`filters.Show()` displays a window. Invoking `EfcViewer.EditFiltersMenuItem_Click` with a real
`EfcFormController` in a test is a direct unit-test-policy violation (AC6: no popups). The concrete
type is therefore not merely inconvenient — it makes line 159 untestable.

**Seam.** New file `QuickFiler/Interfaces/IEfcViewerCommands.cs`:

```csharp
public interface IEfcViewerCommands
{
    void EditFiltersMenuItem_Click(object sender, EventArgs e);
}
```

`EfcFormController` adds `IEfcViewerCommands` to its base list (its existing member already matches
the signature and is already `public`). `EfcViewer` changes the field and parameter types from
`EfcFormController` to `IEfcViewerCommands`.

**Call-site impact: zero.** `SetController` has no callers (defect L1), so the parameter-type change
breaks nothing. Should a caller be added later, `EfcFormController` converts implicitly.

**Host-neutrality.** This is the shape the epic's Non-Goals ask for: a menu-command contract with no
WinForms type in its signature beyond `EventArgs`, reusable by a future WebView2/Office.js port.

**Ledger.** `IEfcViewerCommands.cs` is interface-only and emits no Cobertura `<class>` element. Per
the epic's "Directives for F1's Ledger and Harness", it belongs in the third bucket
(`interface-only / not-measured`), reported N/A, **not** `ratified-exempt`, and carries no
`[ExcludeFromCodeCoverage]`. F9 appends that ledger row in the same change as the
`<Compile Include>` entry (AC5).

### S2 (fallback only, Approach B) — `ProcessCmdKeyBase` virtual seam

Only if the Phase-0 spike fails.

```csharp
// Seam: lets a test double substitute the Form base implementation, which cannot run on an
// instance allocated without a constructor.
protected virtual bool ProcessCmdKeyBase(ref Message msg, Keys keyData) =>
    base.ProcessCmdKey(ref msg, keyData);
```

`ProcessCmdKey`'s final line becomes `return ProcessCmdKeyBase(ref msg, keyData);`. A test-double
subclass in `QuickFiler.Test`, itself allocated with `GetUninitializedObject`, overrides it. Cost:
one permanently-uncovered production line. Benefit: the two false-branch outcomes at line 96 become
reachable, taking branch coverage from 50% to 100%.

This is an adapter-tier seam (third preference). It is justified only because the interface and
delegate tiers cannot intercept a `base.` call.

### S3 (explicitly NOT recommended) — full `IEfcViewer` interface

A full `IEfcViewer` mirroring `IQfcFormViewer` would let `EfcFormController` and `EfcHomeController`
depend on an interface instead of the concrete `EfcViewer`. That is almost certainly the centre of
gravity for the **`EfcFormController.cs` research artifact**, not this one — it changes F8-owned
signatures (`EfcHomeControllerDependencies.cs`, `EfcHomeControllerDependencyFactories.cs`, 14 sites)
and an F3-owned overload (`KeyboardHandler.cs:35`). It buys `EfcViewer.cs` itself no coverage: the
members are already reachable. Flagging the cross-child blast radius here so the sibling artifact
does not rediscover it.

### Members reachable with no seam at all

Using `FormatterServices.GetUninitializedObject(typeof(EfcViewer))` — an established repository
technique (25+ call sites; applied to `Form`-derived types at `ProgressViewer_Tests.cs:34` and
`ConfigViewer_Tests.cs:28`, and to arbitrary viewers at
`QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:333`) — the following are reachable
with reflection on private fields and **no** production change:

- M3, M4, M7, M8, M9, M10, M11, and 9 of the 10 lines of M12.
- `QuickFiler/Properties/AssemblyInfo.cs:5` already declares
  `[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so the `internal` members M6, M7 and M11 need
  no visibility seam. (The brief's `UtilitiesCS` `InternalsVisibleTo` constraint is separate and
  remains correct — nothing in this file touches a `UtilitiesCS` internal.)

---

## 5. Honest STA determination

Per-member analysis against the "no seam can isolate this" standard.

| Member | Needs a constructed form? | Justification |
|---|---|---|
| M1 `EfcViewer()` ctor | **Yes** | The ctor's whole body is `InitializeComponent()`, `SynchronizationContext.Current`, `TaskScheduler.FromCurrentSynchronizationContext()`, `InitTipsLabelsList()`. No seam can execute a constructor without constructing the object, and `TaskScheduler.FromCurrentSynchronizationContext()` throws unless a `SynchronizationContext` is installed. There is no injectable-delegate or interface formulation of "run this type's constructor". **Irreducible.** |
| M12 `ProcessCmdKey`, line 104 and the two false branch outcomes | **Yes, unless S2 is added** | `base.ProcessCmdKey` resolves to `Form.ProcessCmdKey` → `Control.ProcessCmdKey`, which dereferences `Control.Properties` (the `PropertyStore` allocated in `Control`'s constructor). On an instance allocated without a constructor that store is `null`. **Reducible only by the S2 adapter seam**, which is why S2 exists as the Approach-B fallback. Approach A prefers the real base call over a production seam added purely for tests. |
| M10 `InitTipsLabelsList` with **real** `Label` instances | No (optional strengthening) | Reachable with `GetUninitializedObject(typeof(Label))` sentinels; a real-control variant only adds fidelity, not coverage. |
| All other members | **No** | Fully reachable per section 4. |

**Conclusion: exactly one member (the constructor) is genuinely irreducible.** Every other line is
reachable either with no seam at all or with the single S2 adapter seam. This file does not justify
a broad STA surface.

### Constraint conflict on the STA clause

The epic's Shared Design §3 and the brief permit never-shown WinForms **controls** on an STA thread
and exclude **Forms**. The inherited precedent
(`docs/features/epics/winforms-testability-refactor/epic.md:74`) states condition (d):
"`Form`-derived types remain prohibited in tests even when unshown."

`EfcViewer` is a `Form`. Under a literal reading of (d), Approach A is not available and Approach B
is mandatory. Section 9 sets out the contrary evidence. **This is the decision F9's plan must have
settled before Phase 1.**

### STA pattern to follow (do not invent a new one)

Two patterns exist in this repository. Use the second — it is the one already inside
`QuickFiler.Test`.

**Pattern 1 — MSTest STA attributes.** `[STATestClass]` / `[STATestMethod]` from
`Microsoft.VisualStudio.TestTools.UnitTesting`, available in **MSTest.TestFramework 4.3.3** which
`QuickFiler.Test/packages.config:119` already references. No new package. Used by
`Tags.Test/CheckBoxControllerWiring.StaTests.cs:20-23`,
`Tags.Test/TagControllerRendering.StaTests.cs`, the three
`TaskVisualization.Test/TaskController*.StaTests.cs` files, and — with plain `[TestMethod]` inside an
`[STATestClass]` — `UtilitiesCS.Test/EmailIntelligence/FolderSelector_Tests.cs:25`,
`UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:30`.

**Pattern 2 — explicit STA worker thread (the `QuickFiler.Test` precedent).**
`QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:16-53`:

- `new Thread(...)`, `thread.SetApartmentState(ApartmentState.STA)`, `Start()`, `Join()`.
- Inside the thread: capture `SynchronizationContext.Current`, install
  `new SynchronizationContext()`, construct the viewer, run the action.
- `finally`: `viewer?.Dispose()` and restore the previous `SynchronizationContext`.
- Marshal any exception back with `ExceptionDispatchInfo.Capture(captured).Throw()`.
- No `Show()`, no `ShowDialog()`, no `DoEvents`, no timer, no sleep, no message pump, no `Join`
  timeout.

Note there is currently **no `*.StaTests.cs` file in `QuickFiler.Test`**. F9 would create the first
one. AC7 requires the `*.StaTests.cs` naming, so use Pattern 2's helper *inside* a file named
`QuickFiler.Test/Viewers/EfcViewer.StaTests.cs`, and mark the class `[STATestClass]` for
belt-and-braces (the worker thread already guarantees the apartment; the attribute documents intent
and satisfies AC7's "or equivalent runsettings scoping" literally).

---

## 6. `EfcViewer.Designer.cs` disposition

**Attribute status (verified).** `EfcViewer.Designer.cs` carries **no** `[ExcludeFromCodeCoverage]`
attribute of its own — a grep for `ExcludeFromCodeCoverage` across `**/*.Designer.cs` repository-wide
returns zero matches. It is currently uninstrumented **solely because of the attribute on the
`EfcViewer.cs` partial**. The issue's table row ("No attribute … exempt-candidate") is literally true
but materially incomplete; see constraint correction C2.

**500-line limit: does not apply.** The epic's Shared Design §5 and AC4 both exempt generated
`*.Designer.cs` files. No split. `EfcViewer.cs` at 162 lines is well under the limit and needs no
split either.

**Recommended ledger classification: `ratified-exempt` (generated code) — but with no attribute and
still measured.**

Rationale, and the reason the wording matters:

1. Generated code meets the `CLAUDE.md` §UT2 exemption ground ("WinForms … Designer-generated code")
   on its own terms. Its content is machine-authored and re-authored by Visual Studio; a coverage
   gate on it would gate a tool's output, and the refactor remedy that `.claude/rules/general-unit-test.md`
   prescribes ("extract logic into host-neutral modules") has no meaning for it.
2. Its **branch** rate will be ~0.50 regardless of test effort, because `Dispose(bool)`'s
   `disposing && (components != null)` condition can only be exercised one way — `components` is
   initialized to `null` at `EfcViewer.Designer.cs:12` and never reassigned. Every comparable
   designer in the report sits at exactly 0.50. Classifying the file `testable` would make AC2
   unsatisfiable by construction.
3. **But it should NOT receive an `[ExcludeFromCodeCoverage]` attribute and should NOT be removed
   from instrumentation.** Under Approach A it will report ~99% line coverage for free, contributing
   ~2,000 covered lines toward AC9. Suppressing it would throw that away and would additionally
   require editing generated code.

**Required clarification from F1.** The epic's ledger currently offers `testable`,
`ratified-exempt`, and `interface-only / not-measured`. `EfcViewer.Designer.cs` needs a fourth
semantic that F1's bucket names do not clearly express: *measured, counted toward repository-wide
coverage, but not gated on the per-file 80/75 floors.* F9's plan should either request that F1 state
`ratified-exempt` means "exempt from the per-file gate", explicitly decoupled from "carries
`[ExcludeFromCodeCoverage]`", or request a `generated / measured-not-gated` bucket. Without that
clarification F16 will either fail the file on branch coverage or wrongly demand an attribute.

Apply the same reasoning to the other seven `*.Designer.cs` files in the epic (F14, F15) — this is a
cross-child clarification, not an F9-local one.

---

## 7. Proposed test inventory

Two files, mirroring the production tree per `.claude/rules/general-unit-test.md` § Test File
Location. MSTest, Moq, FluentAssertions, Arrange–Act–Assert, no temp files, no external services, no
`Show()`/`ShowDialog()`, no sleeps or timers. Each case below is a separate atomic task.

### 7.1 `QuickFiler.Test/Viewers/EfcViewerTests.cs` — plain `[TestClass]`, no form construction

Shared arrange helper (test-local, no production change):
`private static EfcViewer NewHeadless() => (EfcViewer)FormatterServices.GetUninitializedObject(typeof(EfcViewer));`
Instances are never disposed (there is no initialized base state to dispose) — the same caveat
`ProgressViewer_Tests.cs:26-28` documents for its cancel-path viewers.

| ID | Test name | Member(s) | Scenario class |
|---|---|---|---|
| N1 | `UiSyncContext_ReturnsCapturedContextInstance` | M3 | Positive / accessor identity |
| N2 | `UiScheduler_ReturnsCapturedSchedulerInstance` | M4 | Positive / accessor identity |
| N3 | `SetController_StoresSuppliedCommandsInstance` | M6, S1 | Positive / state transition |
| N4 | `SetController_WithNull_ClearsStoredCommands` | M6 | Negative / edge |
| N5 | `EditFiltersMenuItem_Click_ForwardsSenderAndArgsToController` | M13, S1 | Positive / delegation (`Verify(..., Times.Once)` on exact `sender` and `e`) |
| N6 | `EditFiltersMenuItem_Click_WhenControllerNeverSet_Throws` | M13 | Error / **characterization of defect L1** — pins today's `NullReferenceException`. Covers no new lines; include only if the plan wants the defect pinned before promotion. |
| N7 | `SetKeyboardHandler_ThenKeyboardHandler_ReturnsSameInstance` | M7, M8 | Positive / round-trip |
| N8 | `SetKeyboardHandler_WithNull_ClearsHandler` | M8 | Negative / edge |
| N9 | `TipsLabels_BeforeInitialization_ReturnsNull` | M9 | Edge / initial state |
| N10 | `InitTipsLabelsList_PopulatesNineLabelsInDesignerOrder` | M10, M9 | Positive / ordering. Reflection-assign the nine designer `Label` fields to nine distinct `GetUninitializedObject(typeof(Label))` sentinels; invoke the private method by reflection; assert `TipsLabels` equals exactly `[LblAcSearch, LblAcFolderList, LblAcTrash, LblAcEmail, LblAcFilters, LblAcOk, LblAcCancel, LblAcRefresh, LblAcNewFolder]` by reference and in that order. |
| N11 | `InitTipsLabelsList_WhenInvokedTwice_ReplacesPreviousList` | M10 | State transition (no new lines; pins idempotency shape) |
| N12 | `BreadcrumbWebView_ReturnsDesignerFolderListBoxInstance` | M11 | Positive / accessor identity. **#439 characterization** — reflection-assign `FolderListBox` to a `GetUninitializedObject(typeof(WebView2))` sentinel and assert reference equality, documenting that this member performs no lineage or segment transformation. |
| N13 | `ProcessCmdKey_WithHandlerAndAltModifier_InvokesToggleAndReturnsTrue` | M12 | Positive. `Mock<IQfcKeyboardHandler>`, `var msg = new Message { HWnd = IntPtr.Zero }`, `Keys.Alt \| Keys.F`. Assert `true`; `Verify(h => h.ToggleKeyboardDialogAsync(It.IsAny<object>(), It.Is<KeyEventArgs>(a => a.KeyData == (Keys.Alt \| Keys.F))), Times.Once)`. |
| N14 | `ProcessCmdKey_WithZeroWindowHandle_PassesNullSenderToHandler` | M12 | Edge / characterization that `Control.FromHandle(IntPtr.Zero)` yields a `null` sender |
| N15 | `ProcessCmdKey_WithAltOnlyKeyData_StillInvokesHandler` | M12 | Boundary (`keyData == Keys.Alt` exactly) |

N13–N15 reach `ProcessCmdKey` by `typeof(EfcViewer).GetMethod("ProcessCmdKey", BindingFlags.NonPublic | BindingFlags.Instance)` and `Invoke` with a boxed args array (the `ref Message` parameter requires the by-ref invoke form; read the mutated `args[0]` back if any assertion needs it — the production code does not mutate it).

### 7.2 `QuickFiler.Test/Viewers/EfcViewer.StaTests.cs` — `[STATestClass]`, Approach A only

Every method uses one shared `RunWithViewer(Action<EfcViewer>)` helper copied in shape from
`BayesianPerformanceController.TestSupport.cs:16-53`. Each test carries an XML doc comment stating
why no seam isolates the logic (AC7).

| ID | Test name | Member(s) | Scenario class | Why no seam |
|---|---|---|---|---|
| A1 | `Constructor_OnStaThread_CapturesSynchronizationContextAndScheduler` | M1, M3, M4 | Positive / construction | A constructor cannot be executed without constructing the object |
| A2 | `Constructor_OnStaThread_PopulatesNineNonNullTipsLabels` | M1, M10, M9 | Positive / real-control fidelity | Same; also the only way to prove the designer fields are non-null at ctor exit |
| A3 | `Constructor_OnStaThread_BreadcrumbWebViewIsTheDesignerFolderListBox` | M1, M11 | **#439 characterization** | Proves the exposed control is the designer instance and carries no lineage logic |
| A4 | `ProcessCmdKey_WithNoKeyboardHandler_DefersToBaseAndReturnsFalse` | M12 line 96 (false), line 104 | Negative / short-circuit branch | `base.ProcessCmdKey` dereferences `Control.Properties`, allocated only by `Control`'s constructor |
| A5 | `ProcessCmdKey_WithHandlerButNoAltModifier_DoesNotInvokeHandlerAndReturnsFalse` | M12 line 96 (second condition false), line 104 | Negative / second branch outcome | Same |
| A6 | `Dispose_AfterConstruction_DoesNotThrow` | `EfcViewer.Designer.cs:18-25` | Error / resource safety | Exercises the generated `Dispose(bool)` true path |

A4 and A5 together close the two branch outcomes that make AC2 achievable.

If the Phase-0 spike fails, A1–A3 and A6 are dropped (the ctor becomes permanently uncovered) and
A4/A5 move into `EfcViewerTests.cs` against an uninitialized `EfcViewerProcessCmdKeyDouble : EfcViewer`
that overrides the S2 seam.

### 7.3 Projected outcome

| File | Approach A | Approach B |
|---|---|---|
| `EfcViewer.cs` line | ~100% | ~82% |
| `EfcViewer.cs` branch | 100% (4/4) | 100% (4/4) |
| `EfcViewer.Designer.cs` line | ~99% (measured, not gated) | not measured |
| `EfcViewer.Designer.cs` branch | ~50% (not gated — see §6) | not measured |
| Repository-wide effect | **+~2,000 covered lines** | neutral |

---

## 8. Risk register for Approach A (Phase-0 spike scope)

`new EfcViewer()` runs a 4,200-line `InitializeComponent`. Verify with a single smoke test before
committing the plan to Approach A: construct on an STA thread inside try/finally, assert no throw,
dispose. Specific hazards, in descending order:

1. **`WebView2` `ISupportInitialize`.** `EfcViewer.Designer.cs:882` and `:891` call `BeginInit()` /
   `EndInit()` on `FolderListBox`. If `EndInit` triggers implicit CoreWebView2 initialization, the
   test would need the WebView2 Runtime. Mitigating evidence: the control's handle is never created
   (never shown, no `CreateControl()`), and `new Microsoft.Web.WebView2.WinForms.WebView2()` is
   already constructed in passing tests at `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs:68`
   and `Theme.DispatcherTests.cs:106`. `BeginInit`/`EndInit` specifically is **unproven** in this
   repository — this is the top spike item.
2. **Nested `ItemViewer`.** `EfcViewer.Designer.cs:4205-4216` embeds a `QuickFiler.ItemViewer`
   (`UserControl`, 6,224-line designer, second `WebView2`). Its constructor
   (`ItemViewer.cs:23-30`) calls `Dispatcher.CurrentDispatcher` — safe, it creates a dispatcher
   without running it — plus `InitControlGroups()`. No thread is started. Constructing `EfcViewer`
   would incidentally instrument `ItemViewer.Designer.cs` — but that file is suppressed by
   `ItemViewer.cs:20`'s own attribute, which is **F14-owned**. Coordinate: F9 must not remove it.
3. **`SVGControl.ButtonSVG` ×5 with `SvgResource.Data` byte arrays**
   (`EfcViewer.Designer.cs:36-40, 49-54, 253-867`). If SVG parsing occurs on property assignment it
   runs inside `InitializeComponent`. Related open work: `2026-08-04-svg-renderer-null-document-nre-418`.
   Mitigating evidence: `SVGControl\ButtonSVG.Designer.cs` reports 78.6% line coverage in the
   committed report, so `ButtonSVG` is already constructed by some existing test.
4. **`ComponentResourceManager`** (`EfcViewer.Designer.cs:35`) loading `Viewers\EfcViewer.resx`
   (`QuickFiler.csproj:490`) — embedded in the assembly; low risk.
5. **`SetCompatibleTextRenderingDefault` ordering.** Already handled: `QuickFiler.Test/SetupAssemblyInitializer.cs:14-20`
   runs it at `[AssemblyInitialize]`, before any test creates a control.
6. **Determinism.** The STA worker thread is created, joined, and torn down inside the test. No
   pump, no timer, no sleep, no shared static state. `Thread.Join()` with no timeout is a
   synchronous handoff, not a wall-clock wait, and is the shape the existing precedent uses.

---

## 9. Constraint corrections

**C1 — "no live forms / Forms are NOT permitted" does not match repository practice.**
The brief and the epic inherit condition (d) from
`docs/features/epics/winforms-testability-refactor/epic.md:74`. The repository contains multiple
passing counter-examples, including one inside `QuickFiler.Test` itself:

- `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:31` —
  `viewer = new BayesianPerformanceViewer(controller).Init();` where
  `BayesianPerformanceViewer : Form` (`QuickFiler/Viewers/BayesianPerformanceViewer.cs:8`), on a
  dedicated STA thread, never shown, disposed in `finally`.
- `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:49, 137, 205, 323` — `new ProgressViewer()`
  where `ProgressViewer : Form` (`UtilitiesCS/Threading/ProgressViewer.cs:16`), in an
  `[STATestClass]`.
- `UtilitiesCS.Test/ReusableTypeClasses/ConfigViewer_Tests.cs:53, 101, 158` — `new ConfigViewer()`,
  a `Form`.
- `UtilitiesCS.Test/EmailIntelligence/FolderSelector_Tests.cs:44, 68, 93` — `new FolderSelector()`,
  a `Form`, in an `[STATestClass]`.
- `FolderRemapViewer`, `FilterOlFoldersViewer`, `FolderInfoViewer`, `InputBoxViewer` — all `Form`s,
  all constructed in `UtilitiesCS.Test`.

The distinction the repository actually enforces is **shown vs. unshown**, not `Form` vs. `Control`:
no `Show()`/`ShowDialog()`, no message pump, no popup requiring human interaction. The
`QfcViewer_Test.cs:25` comment — "Disabled to avoid showing the form" — states exactly that rule.

This matters because the constraint as written costs F9 roughly 2,000 lines of coverage and forces an
edit to generated code. **F9's plan needs a maintainer decision**: either (a) confirm that unshown,
STA-constructed, disposed `Form`s are permitted as a last resort on the same conditions as controls —
in which case Approach A proceeds — or (b) reaffirm condition (d), in which case Approach B proceeds
and the Designer file must carry method-level attributes. Do not let the plan proceed with this
ambiguous.

**C2 — the issue's `EfcViewer.Designer.cs` row is incomplete.** `issue.md` records "No attribute /
Generated; exempt-candidate". True as to the file, but the file is already fully suppressed by the
attribute on the other partial, and removing that attribute exposes it. It is not a neutral
"candidate"; it is a ~2,000-line denominator change bundled into AC3.

**C3 — the epic's F9 sizing ("~2,418 testable lines / 4 files") understates the measured
denominator.** Once the attribute is removed, `EfcViewer.Designer.cs` contributes an estimated
1,500–2,500 measurable lines. The plan's coverage arithmetic must use the exposed figure.

**C4 — `Viewers/EfcViewer3.cs` and `Viewers/EfcViewer3.Designer.cs` are not compiled.** They exist in
the working tree and contain a near-duplicate `SetController` / `SetKeyboardHandler` /
`InitTipsLabelsList` surface (`EfcViewer3.cs:24-57`), but neither has a `<Compile Include>` entry in
`QuickFiler/QuickFiler.csproj`. They are outside the coverage denominator and outside F9. Do not
touch them; they are recorded as latent defect L5.

**C5 — the STA attributes need no new package, but `QuickFiler.Test` has no `*.StaTests.cs` yet.**
`STATestClassAttribute` / `STATestMethodAttribute` ship in
`Microsoft.VisualStudio.TestTools.UnitTesting` from MSTest.TestFramework, and
`QuickFiler.Test/packages.config:119` pins 4.3.3. The existing `QuickFiler.Test` STA idiom is a
manual `Thread` + `SetApartmentState`, not the attribute; F9 would create the project's first
`*.StaTests.cs` file. There is no `.runsettings` in `QuickFiler.Test`.

**C6 — `QuickFiler` grants `InternalsVisibleTo("QuickFiler.Test")`** (`QuickFiler/Properties/AssemblyInfo.cs:5`).
The brief's assembly-boundary constraint is about `UtilitiesCS` and is correct, but it should not be
read as implying `QuickFiler`'s own internals need a seam. `SetController`, `KeyboardHandler` and
`BreadcrumbWebView` are directly callable from tests.

**C7 — the brief's premise that this is "the F9 file most likely to need the STA last-resort clause"
holds, but for a narrower reason than expected.** Only the constructor is irreducible. The STA case
is driven by the Designer file's denominator and by one branch pair, not by the file's members being
broadly untestable.

---

## 10. Latent defects (record only — do not fix under AC10; promote per AC11)

**L1 — `SetController` and `EditFiltersMenuItem_Click` are dead code with an armed
`NullReferenceException`.**
`QuickFiler/Viewers/EfcViewer.cs:50-53` and `:157-160`.
Mechanism: `EfcViewer.SetController` has **zero callers** in the compiled tree. A repository-wide
grep finds `SetController` calls only at `QuickFiler/Controllers/QfcFormController.cs:44` (a
different viewer type) and `QuickFiler/Viewers/EfcViewer3.cs:39` (not compiled). `_formController`
(`EfcViewer.cs:48`) is therefore permanently `null`. Separately, `EfcViewer.Designer.cs` contains no
event wiring whatsoever (zero `+=` occurrences in 4,277 lines), and the only subscription to that
menu item — `EfcFormController.cs:400`, `_formViewer.EditFiltersMenuItem.Click += EditFiltersMenuItem_Click;`
— binds the **controller's own** handler at `EfcFormController.cs:561`, not the viewer's. The
viewer's private handler is unsubscribed. Impact: if the Designer is ever regenerated by Visual
Studio with the conventionally-named handler wired (the normal designer behaviour when a matching
handler exists), the first click throws `NullReferenceException` at `EfcViewer.cs:159`.

**L2 — `ProcessCmdKey` swallows every Alt-modified key once a keyboard handler is attached.**
`QuickFiler/Viewers/EfcViewer.cs:94-105`.
Mechanism: the guard tests only `keyData.HasFlag(Keys.Alt)` and unconditionally `return true`,
consuming the key. The form owns two menu strips (`FilterMenuStrip`, `MoveOptionsStrip`,
`EfcViewer.Designer.cs:4263, 4268`) whose Alt-mnemonic access keys are therefore unreachable while a
handler is set. Secondary: `IQfcKeyboardHandler.ToggleKeyboardDialogAsync(object, KeyEventArgs)`
returns `void` (`QuickFiler/Interfaces/IQfcKeyboardHandler.cs:15`), so any fault raised inside the
toggle is unobservable at this call site. Whether the swallow is intended is a product question, not
a refactor question — pin current behaviour with N13/N15 and promote.

**L3 — unused `log4net` logger and unused `using` directives.**
`QuickFiler/Viewers/EfcViewer.cs:32-34` declares `private static readonly log4net.ILog log`, never
referenced anywhere in the type. Candidates for unused `using` (verify with IDE0005 before acting):
`System.ComponentModel` (3), `System.Data` (4), `System.Drawing` (6), `System.Linq` (7),
`System.Runtime.Remoting.Contexts` (8), `System.Text` (9), `TaskVisualization` (15). The remoting
namespace in a WinForms viewer is particularly likely to be an accidental IDE insertion.

**L4 — duplicated commented-out dead code.**
`QuickFiler/Viewers/EfcViewer.cs:107-155` is 49 lines of commented-out code, of which lines 121-137
and 139-155 are **byte-identical duplicates** of each other (`MenuItem_CheckedChanged(ToolStripMenuItem)`
plus `MenuItem_Click`). Not coverage-relevant (comments carry no sequence points), but it is 30% of
the file's physical length.

**L5 — orphaned `EfcViewer3` viewer pair.**
`QuickFiler/Viewers/EfcViewer3.cs` and `QuickFiler/Viewers/EfcViewer3.Designer.cs` exist in the tree,
reference `EfcFormController`, and are absent from `QuickFiler/QuickFiler.csproj`. Dead files that
will drift from the live viewer and mislead future readers and greps.

---

## 11. Requirements mapping (AC → design)

| AC | How this file satisfies it |
|---|---|
| AC1 (>= 80% line) | Approach A ~100%; Approach B ~82%. Both clear. |
| AC2 (>= 75% branch) | Requires the two false outcomes at `EfcViewer.cs:96`. Approach A via A4/A5; Approach B via the S2 seam. **Not achievable without one of them.** |
| AC3 (attribute removed) | Remove `EfcViewer.cs:20`. Note this simultaneously un-suppresses `EfcViewer.Designer.cs`. |
| AC4 (500 lines) | `EfcViewer.cs` 162 lines — compliant, no split. `EfcViewer.Designer.cs` exempt as generated code. |
| AC5 (new files >= 90% + csproj + ledger row) | One new file: `QuickFiler/Interfaces/IEfcViewerCommands.cs`. Interface-only → `interface-only / not-measured` bucket, reported N/A, no attribute; add `<Compile Include>` (CRLF-preserving edit) and the ledger row in the same change. |
| AC6 (test hygiene) | No `Show()`/`ShowDialog()`, no pump, no `DoEvents`, no sleep/delay, no temp files, no external services. `Thread.Join()` is a synchronous handoff. All viewers disposed in `finally`. |
| AC7 (STA confinement) | Only `QuickFiler.Test/Viewers/EfcViewer.StaTests.cs`, `[STATestClass]`, six tests, each documenting why no seam applies. Zero STA usage in `EfcViewerTests.cs`. |
| AC8 (toolchain) | No analyzer-visible risk identified; the S1 interface addition is nullable-clean if `sender` is annotated consistently with the existing `EfcFormController.EditFiltersMenuItem_Click` signature. |
| AC9 (repo coverage retained/improved) | **The decisive AC.** Approach A adds ~2,000 covered lines. Approach B is neutral only because it re-suppresses the Designer. Removing the attribute with neither mitigation is an AC9 failure. |
| AC10 (no behavior change) | S1 is a parameter-type widening on a method with zero callers. No other production change under Approach A. Under Approach B, S2 adds one virtual indirection on an existing call. N12/A3 pin #439-adjacent behaviour as-is. |
| AC11 (defect promotion) | L1–L5 above, via the MCP promotion lifecycle. |

---

## 12. Open questions for the plan (resolve in Phase 0)

1. **Maintainer decision on C1** — are unshown, STA-constructed, disposed `Form`s permitted, given
   the `BayesianPerformanceController.TestSupport.cs` precedent? Determines Approach A vs. B.
2. **Spike result** — does `new EfcViewer()` succeed headlessly on an STA thread? (Section 8.)
3. **F1 ledger semantics for `*.Designer.cs`** — does `ratified-exempt` mean "not gated" or "carries
   `[ExcludeFromCodeCoverage]`"? F9 needs the former. (Section 6.)
4. **F1 harness availability** — `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`
   does not exist yet; only `epic.md` is present in the epic folder. F9's Phase 0 halt gate on F1's
   deliverables is real and currently unmet.
5. **Measured line/branch counts** — replace section 2.2's estimates with F1 harness output, and
   confirm against issue #441 that `<line>` nodes are not double-counted.
6. **F14 coordination** — constructing `EfcViewer` transitively constructs `ItemViewer`, whose
   `[ExcludeFromCodeCoverage]` at `ItemViewer.cs:20` is F14-owned. F9 must not remove it; F14 should
   know that F9's STA test will incidentally exercise `ItemViewer.Designer.cs` once F14 removes it.
