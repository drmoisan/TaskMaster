# Research — `QuickFiler/Viewers/ItemViewerExpanded.cs`

- Feature: `quickfiler-itemviewer-coverage` (issue #456), epic child F14 of `quickfiler-per-file-coverage` (#136)
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5e4b635834feedd7`
- Produced: 2026-08-07T21-40
- Scope: one production file — `QuickFiler/Viewers/ItemViewerExpanded.cs` (181 lines)

> **Tooling note (affects auditability).** This research session had no shell tool (no Bash/PowerShell
> invocation available), so `gh issue list --state open --search ...` could not be run. Open-issue
> search was performed instead by fetching the public GitHub issue-search UI
> (`https://github.com/drmoisan/TaskMaster/issues?q=is%3Aissue+is%3Aopen+<term>`) for the terms
> `ItemViewer`, `expanded`, `designer`, `viewer`, and `coverage`. Results are recorded in
> § "Open-issue scan". All other findings are from direct file reads and repository-wide `rg`
> searches and are cited file:line.

---

## 1. Verified current state

### 1.1 Type shape

`ItemViewerExpanded.cs:16` declares `public partial class ItemViewerExpanded : UserControl`.

Verified facts:

- It carries **no** `[ExcludeFromCodeCoverage]` attribute. The exclusion sweep over
  `QuickFiler/Viewers/` returns attributes on `EfcViewer.cs:20`, `EfcViewer3.cs:17`,
  `ItemViewer.cs:20`, `QfcFormViewer.cs:17`, `QfcFormViewerDark.cs:16`,
  `QfcFormViewerExpanded.cs:16`, `QfcItemViewer.cs:18`, `QfcItemViewerExpanded.cs:18`,
  `QfcItemViewerExpandedLight.cs:14`, `QfcItemViewerLightSelected.cs:15`, `QfcItemViewerV1.cs:14`,
  `WebView2Messenger.cs:20`, `WebView2BreadcrumbHost.cs:29`, `WebView2CoreInitializer.cs:15`, and
  seven method-level attributes in `BreadcrumbPopupUiOperations.cs`. `ItemViewerExpanded.cs` is not
  among them. It is therefore instrumented today, which the committed Cobertura report confirms.
- It does **not** implement `IItemViewer`. Contrast `ItemViewer.cs:21`
  (`public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal`).
- It is compiled: `QuickFiler/QuickFiler.csproj:438` `<Compile Include="Viewers\ItemViewerExpanded.cs">`,
  with `Viewers\ItemViewerExpanded.Designer.cs` at `:441` marked `<DependentUpon>ItemViewerExpanded.cs</DependentUpon>`
  and `Viewers\ItemViewerExpanded.resx` at `:498`.

### 1.2 Committed baseline

`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml:5364`:

```xml
<class line-rate="0.390244" branch-rate="0.083333" complexity="14"
       name="QuickFiler.ItemViewerExpanded" filename="QuickFiler\Viewers\ItemViewerExpanded.cs">
```

**Only two `<class>` elements in the entire report carry a filename containing `ItemViewerExpanded`**
(XML lines 4112 and 5364), and they have different `filename` attributes
(`...\ItemViewerExpanded.Designer.cs` and `...\ItemViewerExpanded.cs`). The epic's harness directive
about a source file producing multiple `<class>` elements sharing one `filename` (a type plus its
`<>c` closure class) **does not apply to this file** — verified: a `<>c__DisplayClass` element would
carry the substring `ItemViewerExpanded` in its `name` attribute and would have been returned by the
same search. No union/max-hits merge is needed here.

The source is line-for-line identical to the version that was measured: the covered line ranges
(18–28 constructor, 102–127 `InitControlGroups`, 170–179 `MenuItem_CheckedChanged`) map exactly onto
the current working-tree file. The baseline is therefore directly attributable.

### 1.3 Arithmetic inconsistency in the committed report — a material correction

Counting the class-level `<lines>` block directly (XML 5550–5681):

- **106 distinct `<line>` children.** (130 XML lines in the block; 6 of them are branch entries
  consuming 5 XML lines each = 30; 130 − 30 = 100 plain entries; 100 + 6 = 106.)
- **40 have `hits="1"`.** 40 / 106 = **37.74%**, not 39.02%.
- Branch outcomes: 6 branch lines × 2 conditions = **12 outcomes, 1 covered** = **8.333%**, which
  matches the reported `branch-rate="0.083333"` exactly.

So the reported `branch-rate` is consistent with the `<line>` children, but the reported `line-rate`
(`0.390244` = 32/82) is **not** — neither its numerator nor its denominator can be reconciled with
the element's own 106 line children.

This is not a new discovery: **open issue #441, "Cobertura post-processing double-counts `<line>`
nodes, inflating lines-valid and every coverage rate"**, states that
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (`Get-CoberturaCoverageSummary` and
`Merge-CoberturaClassesByFilename`) miscounts `<line>` nodes and that "class-level `line-rate`
attributes are consequently incorrect". The committed report is post-processed output, which is also
visible in its mixed numeric formatting (six-decimal values such as `0.390244` alongside
full-precision values such as `0.9950980392156863`).

**Consequence for this feature (binding on the plan):** the acceptance figure for
`ItemViewerExpanded.cs` must be recomputed from the distinct `<line>` children, not read from the
`line-rate` attribute. This extends the epic's existing harness directive ("decide the denominator on
`<line>` child count, never `line-rate`") — the *rate itself* must also be recomputed, not just the
denominator decision. Verified: 40/106 = 37.74% line, 1/12 = 8.33% branch. Both fail their gates
under either reading, so the target set below is unaffected by the discrepancy.

### 1.4 Which existing tests produce the coverage — verified call chain

No test constructs `ItemViewerExpanded` directly. A repository-wide `rg` over `*.cs` returns exactly
one construction site in the whole solution:

`QuickFiler/Viewers/QfcFormViewer.Designer.cs:42` — `this._qfcItemViewerExpandedTemplate = new QuickFiler.ItemViewerExpanded();`

and one assignment to its `Controller` property:

`QuickFiler/Viewers/QfcFormViewer.Designer.cs:211` — `this._qfcItemViewerExpandedTemplate.Controller = null;`

`QfcFormViewer` is constructed in production at `QuickFiler/Controllers/QfcHomeController.cs:93`
(`Init()`) and `:133` (`InitAsync(...)`). Both are exercised by
`QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`:

| Test | Line | Path |
| --- | --- | --- |
| `Init_InitializesCorrectly` | `QfcHomeControllerTests.cs:114`, acts at `:149` | `QfcHomeController.Init()` → `QfcHomeController.cs:93 new QfcFormViewer()` → `QfcFormViewer.Designer.cs:42 new ItemViewerExpanded()` |
| `InitAsync_InitializesCorrectly` | `QfcHomeControllerTests.cs:179`, acts at `:220` | `QfcHomeController.InitAsync(...)` → `QfcHomeController.cs:133 new QfcFormViewer()` → same |

`QfcFormViewerDerived : QfcFormViewer` (`QfcHomeControllerTests.cs:243`) is declared but **never
instantiated** anywhere in `QuickFiler.Test` (verified by search) and contributes nothing.

That single incidental construction path explains all 40 covered lines: the constructor
(18–28), `InitControlGroups` (102–127) called from it, the four `MenuItem_CheckedChanged` calls it
makes (which take the `else` branch — see § 3), and the `Controller` setter at line 54 written by
`QfcFormViewer.Designer.cs:211`.

Two consequences the plan must absorb:

1. The 37.7% is **not** deliberate coverage of this file. It is a by-product of another child's
   (F7's) test. F7 owns `QfcHomeController.cs`; if F7 replaces the live-form construction at
   `QfcHomeController.cs:133` with a seam, **this file's coverage collapses to near zero**. F14 must
   not depend on that path. See § 8 cross-child note CC-3.
2. `QfcHomeControllerTests` is a plain `[TestClass]` with `[TestMethod]` (`:22`, `:113`, `:178`) —
   no STA attribute. This is empirical proof that `ItemViewerExpanded` (including its `WebView2`,
   `MenuStrip`, `ComboBox`, and `FastObjectListView` children) can be constructed to completion on
   the default MSTest apartment without a message pump, a shown window, or a popup. The assembly
   initializer at `QuickFiler.Test/SetupAssemblyInitializer.cs:14` calls only
   `Application.EnableVisualStyles()` and `SetCompatibleTextRenderingDefault(false)`.

---

## 2. Line-attributed gap list (Q1)

Derived from the class-level `<lines>` block, XML 5550–5681. 106 coverable lines; 40 covered; 66
uncovered. 12 branch outcomes; 1 covered; 11 untaken.

### 2.1 Covered today (40 lines)

| Lines | Member | Reached by |
| --- | --- | --- |
| 18–28 (11) | `.ctor()` | `QfcHomeControllerTests.Init_InitializesCorrectly` / `InitAsync_InitializesCorrectly` |
| 54 (1) | `set_Controller` | `QfcFormViewer.Designer.cs:211` |
| 102–120, 122, 126, 127 (22) | `InitControlGroups()` | called from `.ctor` line 23 |
| 170, 171, 176, 177, 178, 179 (6) | `MenuItem_CheckedChanged(ToolStripMenuItem)` — `else` arm only | four calls from `.ctor` lines 24–27 |

### 2.2 Uncovered lines (66)

| Lines | Count | Member | Why uncovered | What reaches it |
| --- | --- | --- | --- | --- |
| 35 | 1 | `get_TipsLabels` | never read | read the property on a constructed instance |
| 41 | 1 | `get_LeftTipsLabels` | never read | same |
| 47 | 1 | `get_ExpandedTipsLabels` | never read | same |
| 53 | 1 | `get_Controller` | only the setter is used | same |
| 60 | 1 | `get_UiSyncContext` | never read | same |
| 66 | 1 | `get_UiScheduler` | never read | same |
| 70–75, 77–82, 84–87 | 16 | `RemoveControlsColsRightOf(Control)` | **no production caller on this type** (see § 4.2) | direct call with a `TableLayoutPanel`-parented control (TLP arm) and a non-TLP-parented control (else arm) |
| 90–99 | 10 | `RemoveControlsRightOf(Control)` (private) | only reachable via the else arm above | else arm with a control tree that yields ≥ 1 control to the right |
| 130–140 | 11 | `ControlsRightOf(Control)` — `ForAllControls` walk + lambda body 135–139 | private, only reachable via `RemoveControlsRightOf` | same as above |
| 143–147, 149–156 | 13 | `ControlsRightOf` — limit selection and LINQ filter | same | two cases: `furthestRight` present in the walk, and absent |
| 159–161 | 3 | `L0v2h2_WebView2_ParentChanged` | designer-wired at `ItemViewerExpanded.Designer.cs:274`; the WebView2 is never re-parented in tests | re-parent `L0v2h2_WebView2` on a constructed instance |
| 164–167 | 4 | `MenuItem_CheckedChanged(object, EventArgs)` | designer-wired at `ItemViewerExpanded.Designer.cs:171, 180, 189, 198` to `ToolStripMenuItemCb.CheckedChanged`; never raised after construction | set `viewer.ConversationMenuItem.Checked = true` (public field, `Designer.cs:811`) |
| 172–174 | 3 | `MenuItem_CheckedChanged(ToolStripMenuItem)` — `if` arm | **structurally unreachable through the wired event path** — see § 3 | direct invocation with a base `ToolStripMenuItem` whose `Checked == true`; requires a seam |

### 2.3 Untaken branches (11 of 12 outcomes)

`complexity="14"` over 106 coverable lines resolves to exactly six branch points:

| Source line | Predicate | Outcomes | Covered | To take the missing outcome(s) |
| --- | --- | --- | --- | --- |
| 71 | `if (furthestRight.Parent is TableLayoutPanel)` | 2 | 0 | one call with a TLP-parented control, one with a `Panel`-parented control |
| 77 | `if (++columnNumber < tlp.ColumnCount)` | 2 | 0 | TLP where `furthestRight` is in a non-last column (true) and in the last column (false) |
| 92 | `for (int i = controlsToRemove.Count - 1; i >= 0; i--)` | 2 | 0 | one case with ≥ 1 control to the right (enters loop) and one with none (skips) |
| 143 | `if (controlLocation.Any(tup => tup.Control == furthestRight))` | 2 | 0 | `furthestRight` inside the walked tree (true) and outside it (false) |
| 152 | `.Where(tup => tup.Point.X > limit.X)` predicate | 2 | 0 | a tree with at least one control right of the limit and one left of it |
| 171 | `if (menuItem.Checked)` | 2 | **1** (false) | direct invocation with base `Checked == true` — blocked by the defect in § 3 |

Five of the six branch points (71, 77, 92, 143, 152) are in the three geometry methods. **Every one
of them lives in code that is a verbatim duplicate of `ItemViewer.cs:77–164`** (see § 5), which is
why the recommended approach extracts them rather than testing them twice.

---

## 3. Blocking defect on the last branch — `ToolStripMenuItemCb` shadows `Checked`

This is the single most consequential finding for the branch gate.

`QuickFiler/Viewers/ToolStripMenuItemCb.cs:11` declares
`public partial class ToolStripMenuItemCb : ToolStripMenuItem`, and at `:32` it **shadows** the base
member:

```csharp
public new bool Checked
{
    get => _checked;
    set
    {
        _checked = value;
        if (value) { base.Image = Properties.Resources.CheckBoxChecked; }
        else       { base.Image = null; }
        CheckedChanged?.Invoke(this, new EventArgs());   // :47 — the shadowed event, :58
        base.Invalidate();
    }
}
```

`ToolStripMenuItemCb.cs:58` likewise declares `public new event EventHandler CheckedChanged`.
The shadowed setter **never assigns `base.Checked`**.

`ItemViewerExpanded.cs:169` takes the **base** type as its parameter:

```csharp
private void MenuItem_CheckedChanged(ToolStripMenuItem menuItem)
{
    if (menuItem.Checked) { ... }   // :171 — binds to ToolStripMenuItem.Checked, not the shadow
```

The four designer wirings (`ItemViewerExpanded.Designer.cs:171, 180, 189, 198`) subscribe through a
`Viewers.ToolStripMenuItemCb`-typed field, so they attach to the **shadowed** event.

Verified consequences:

1. Setting `ConversationMenuItem.Checked = true` sets `_checked`, sets `base.Image` to
   `CheckBoxChecked`, then raises the shadowed `CheckedChanged`, which enters
   `MenuItem_CheckedChanged` and reads `base.Checked` — still `false` — and executes line 178,
   `menuItem.Image = null`, **erasing the check image the setter had just applied**.
2. Setting the base `((ToolStripMenuItem)item).Checked = true` raises the *base* `CheckedChanged`,
   which has no subscribers, so the handler never runs.

Therefore lines 172–174 and the `true` outcome of the line-171 branch are **unreachable through any
public path**. They can only be covered by invoking `MenuItem_CheckedChanged(ToolStripMenuItem)`
directly with a plain `ToolStripMenuItem` whose base `Checked` is `true`. That makes a seam
**mandatory**, not optional, for the branch gate.

The same defect exists verbatim in `ItemViewer.cs:177–187` (also F14-owned).

> **Promotion candidate LD-1 — `ToolStripMenuItemCb.Checked` shadow never reaches the base property,
> so the menu check image is cleared immediately after it is set.**
> Evidence: `ToolStripMenuItemCb.cs:32-51` sets only the private `_checked` field and `base.Image`,
> never `base.Checked`; `ToolStripMenuItemCb.cs:58` shadows `CheckedChanged`;
> `ItemViewerExpanded.Designer.cs:171,180,189,198` subscribe through the shadowed event;
> `ItemViewerExpanded.cs:169` declares the handler parameter as the base `ToolStripMenuItem`, so
> `:171` reads `base.Checked`, which is permanently `false`, and `:178` sets `Image = null`. The
> committed Cobertura shows `condition-coverage="50% (1/2)"` on line 171 with lines 172–174 at
> `hits="0"` after four constructor invocations, which is the observable signature of the defect.
> `ToolStripMenuItemCb.cs` is assigned to epic child **F15**, so the fix is out of F14's file
> boundary.

---

## 4. Member-by-member seam classification (Q4)

| Member | Lines | Class | Notes |
| --- | --- | --- | --- |
| `.ctor()` | 18–28 | thin wiring | `InitializeComponent()`, captures `SynchronizationContext.Current` and `TaskScheduler.FromCurrentSynchronizationContext()`, calls `InitControlGroups()` and four `MenuItem_CheckedChanged`. Already 100% covered. |
| `TipsLabels` / `LeftTipsLabels` / `ExpandedTipsLabels` / `Controller` / `UiSyncContext` / `UiScheduler` getters | 35, 41, 47, 53, 60, 66 | thin wiring | trivial field reads; need only a constructed instance |
| `set_Controller` | 54 | thin wiring | already covered |
| `RemoveControlsColsRightOf(Control)` | 69–87 | **pure/host-neutral** except one field read | operates on `Control`/`TableLayoutPanel` only. Its sole instance dependency is `L0v2h2_WebView2` at `:75`, used purely as the *argument* to `tlp.SetColumnSpan` — its `WebView2`-ness is irrelevant; any `Control` satisfies it. |
| `RemoveControlsRightOf(Control)` | 89–99 | **pure/host-neutral** | walks and mutates a `Control.Controls` collection |
| `InitControlGroups()` | 101–127 | thin wiring | builds three `List<Label>` from designer fields; already 100% covered |
| `ControlsRightOf(Control)` | 129–156 | **pure/host-neutral** except `this` | uses `this.ForAllControls(...)` (`UtilitiesCS` extension) as the tree root; the root is a parameterisable `Control` |
| `L0v2h2_WebView2_ParentChanged` | 158–161 | thin wiring | body is a single `Console.WriteLine` — see LD-2 |
| `MenuItem_CheckedChanged(object, EventArgs)` | 163–167 | thin wiring | unguarded cast at `:165` — see LD-3 |
| `MenuItem_CheckedChanged(ToolStripMenuItem)` | 169–179 | **pure/host-neutral** | uses no instance state at all; `ToolStripMenuItem` is a `Component`, not a `Control`, so it needs no window handle and no STA |

**No member of this file is COM-bound.** There is no `Microsoft.Office.Interop.Outlook` reference in
the file (`using` block, `:1-12`), and no member touches `Application`, `MailItem`, `Store`, or
`MAPIFolder`. The `CLAUDE.md` §UT2 Outlook-Interop exemption ground does not apply to this file at
all.

### 4.1 Recommended seam set

Following the epic hierarchy (interface seam > injectable delegate > adapter), and
`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy ("extract all logic into
host-neutral, testable modules and leave only the thinnest possible wiring in the host-bound entry
point"):

**S1 — Extract the three geometry methods into a new host-neutral static module.**

New file `QuickFiler/Viewers/ControlColumnTrimmer.cs` (`internal static class ControlColumnTrimmer`):

- `internal static void RemoveColumnsRightOf(Control root, Control furthestRight, Control columnSpanTarget)` — body of `ItemViewerExpanded.cs:69-87`, with `L0v2h2_WebView2` replaced by the `columnSpanTarget` parameter and `this` by `root`.
- `internal static void RemoveControlsRightOf(Control root, Control furthestRight)` — body of `:89-99`.
- `internal static List<Control> ControlsRightOf(Control root, Control furthestRight)` — body of `:129-156`.

`ItemViewerExpanded.cs` then keeps one expression-bodied wiring line:

```csharp
public void RemoveControlsColsRightOf(Control furthestRight) =>
    ControlColumnTrimmer.RemoveColumnsRightOf(this, furthestRight, L0v2h2_WebView2);
```

This is an interface-free extraction (no polymorphism is needed — there is one implementation), which
matches the general policy's "create a standalone function when the operation is pure, stateless, and
simple". It removes five of the six branch points from the `UserControl`, removes the `WebView2`
reference from every test, and — because `ItemViewer.cs:77-164` is a verbatim duplicate (§ 5) —
serves both F14 files from one tested module.

**S2 — Widen the two private menu handlers to `internal`.**

`QuickFiler/Properties/AssemblyInfo.cs:5` contains `[assembly: InternalsVisibleTo("QuickFiler.Test")]`
— **verified**. The repository already relies on this: `QuickFiler/Controllers/QfcHomeController.cs:111`
declares `internal async Task InitAsync(...)` and `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:220`
calls it directly. Precedent established.

- `MenuItem_CheckedChanged(ToolStripMenuItem)` → `internal static` (it reads no instance state;
  `ItemViewerExpanded.cs:24-27` and `:166` continue to compile unchanged).
- `MenuItem_CheckedChanged(object, EventArgs)` → `internal` (must stay an instance method: the
  designer wires it as `new System.EventHandler(this.MenuItem_CheckedChanged)` at
  `ItemViewerExpanded.Designer.cs:171,180,189,198`, and a static target would require editing the
  generated designer file).

`internal` widening is the minimum sufficient seam here; an interface seam over
`ToolStripMenuItem.Checked`/`.Image` would add a production abstraction with exactly one
implementation, which the general policy's simplicity-first rule argues against, and would not
change what the test can assert.

### 4.2 Instance-only lines

After S1 and S2, the only lines that still require a constructed `ItemViewerExpanded` are:

- the six getters (35, 41, 47, 53, 60, 66) — they read fields populated by `InitControlGroups`;
- the delegation line for `RemoveControlsColsRightOf` (it reads the `L0v2h2_WebView2` field);
- `MenuItem_CheckedChanged(object, EventArgs)` (164–167) — reached by raising the wired event;
- `L0v2h2_WebView2_ParentChanged` (159–161) — reached by re-parenting the WebView2.

Construction is empirically proven safe and already happens in the current suite on the default
apartment (§ 1.4). Note that `RemoveControlsColsRightOf` has **no production caller on this type**:
the only `RemoveControlsColsRightOf` call sites in the solution are
`QuickFiler/Controllers/EfcItemController.cs:247` (`_itemViewer`, an `IItemViewer`, i.e. `ItemViewer`)
and the `IItemViewer.cs:131` declaration. `ItemViewerExpanded` does not implement `IItemViewer`.

### 4.3 STA infrastructure already in the repository (reuse, do not invent)

Verified:

- `[STATestClass]` / `[STATestMethod]` come from `Microsoft.VisualStudio.TestTools.UnitTesting` —
  they are provided by **MSTest.TestFramework 4.3.3**, not by a separate package. Evidence:
  `Tags.Test/CheckBoxControllerWiring.StaTests.cs` uses `[STATestClass]` at `:20` with no `using`
  beyond `Microsoft.VisualStudio.TestTools.UnitTesting` (`:7`), and `Tags.Test/packages.config:110-111`
  lists only `MSTest.TestAdapter 4.3.3` / `MSTest.TestFramework 4.3.3`. No `STAExtensions` package
  exists anywhere in the repository (searched all `packages.config`).
- **`QuickFiler.Test/packages.config:118-119` already pins `MSTest.TestAdapter 4.3.3` and
  `MSTest.TestFramework 4.3.3`.** No package change is needed to use STA attributes in
  `QuickFiler.Test`.
- Dedicated `*.StaTests.cs` file precedent: `Tags.Test/CheckBoxControllerWiring.StaTests.cs`,
  `Tags.Test/TagControllerRendering.StaTests.cs`. Both use `[STATestClass]` + `[STATestMethod]`,
  construct never-shown controls, dispose them in `using`, and document in the class summary why the
  STA scope is required.
- Per-method STA in an ordinary `[TestClass]` also exists
  (`UtilitiesCS.Test/Extensions/WinFormsExtensions_Tests.cs:10` is `[TestClass]` with
  `[STATestMethod]` at `:15`), and `UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs:10` is a
  full `[STATestClass]` covering `RemoveSpecificColumn` — the same `UtilitiesCS` extension this file
  calls at `ItemViewerExpanded.cs:80`. **The epic requires the dedicated-file form**, so F14 uses
  `*.StaTests.cs`, not the per-method form.
- The manual-thread form (`new Thread(...); t.SetApartmentState(ApartmentState.STA)`) is used
  throughout `UtilitiesCS.Test` (e.g. `TipsController_Tests.cs:72`). Do not reuse it — the attribute
  form is simpler and is the established form in the newer `Tags.Test` files.

---

## 5. Relationship to `ItemViewer` (Q5)

**There is no hosting, construction, or delegation relationship in either direction.** Verified by
searching every `.cs` file in the solution for `ItemViewerExpanded`: `ItemViewer.Designer.cs` and the
`ItemViewer.*` partials contain no reference to it, and `ItemViewerExpanded.*` contains no reference
to `ItemViewer`.

The actual relationship is **copy-paste duplication plus a shared host**:

1. **Duplication.** `ItemViewerExpanded.cs:16-179` is a near-verbatim copy of `ItemViewer.cs:21-187`.
   Line-for-line equivalents: `TipsLabels`/`LeftTipsLabels`/`ExpandedTipsLabels`/`Controller`/
   `UiSyncContext`/`UiScheduler` (`IVE:32-67` ≡ `IV:34-69`), `RemoveControlsColsRightOf`
   (`IVE:69-87` ≡ `IV:77-95`), `RemoveControlsRightOf` (`IVE:89-99` ≡ `IV:97-107`),
   `InitControlGroups` (`IVE:101-127` ≡ `IV:109-135`), `ControlsRightOf` (`IVE:129-156` ≡
   `IV:137-164`), `L0v2h2_WebView2_ParentChanged` (`IVE:158-161` ≡ `IV:166-169`), and both
   `MenuItem_CheckedChanged` overloads (`IVE:163-179` ≡ `IV:171-187`). Differences: `ItemViewer` adds
   `_uiDispatcher`/`UiDispatcher` (`IV:71-75`), implements `IItemViewer, IContainerControlLocal`,
   sets `Dispatcher.CurrentDispatcher` in its constructor (`IV:28`), and continues past line 187 with
   `MenuItems`/`LoadMenuItems` (`IV:189-203`) and a large field-to-property block (`IV:207+`);
   `ItemViewerExpanded`'s constructor additionally calls `MenuItem_CheckedChanged` four times
   (`IVE:24-27`).
2. **Shared host.** `QfcFormViewer` owns one of each: `_qfcItemViewerExpandedTemplate` (an
   `ItemViewerExpanded`, `QfcFormViewer.Designer.cs:256`) and `_QfcItemViewerTemplate`. They are
   sibling layout templates whose `TableLayoutPanel` cell geometry is snapshotted into the
   `"Expanded"` and `"Compressed"` display states by `QfcFormViewer.CaptureTlpCellStates()`
   (`QfcFormViewer.cs:187-240`).

**Practical implications for the plan:**

- Test fixtures are **not** interchangeable. `ItemViewer` implements `IItemViewer` and can be mocked
  behind that interface; `ItemViewerExpanded` cannot, and there is no interface to share.
- The **production** code is worth sharing. The S1 extraction is the correct shared artifact: one
  tested `ControlColumnTrimmer` covers the five duplicated branch points for both files. Both files
  are F14-owned, so this is an intra-child change — coordinate with the parallel `ItemViewer.*`
  research so both delegate to the same module and the extraction is planned once.

### 5.1 The type's production role is narrower than its API suggests

The only members of `_qfcItemViewerExpandedTemplate` read by production code are designer control
fields: `L0vh_Tlp`, `L1h0L2hv3h_TlpBodyToggle`, `TxtboxBody`, `TopicThread`, `L0v2h2_WebView2`,
`LblAcOpen`, `LblAcBody` (`QfcFormViewer.cs:202-224`), plus designer-set properties
(`Controller = null`, `Margin`, `MinimumSize`, `Name`, `Size`, `TabIndex`,
`QfcFormViewer.Designer.cs:199-218`).

No production code reads `TipsLabels`, `LeftTipsLabels`, `ExpandedTipsLabels`, the `Controller`
*getter*, `UiSyncContext`, `UiScheduler`, or calls `RemoveControlsColsRightOf` **on an
`ItemViewerExpanded`** (verified by repository-wide search). `InitControlGroups` — 22 of the 40
currently covered lines — exists solely to populate three lists nothing reads.

> **Promotion candidate LD-8 — `ItemViewerExpanded` carries a dead public surface inherited from its
> `ItemViewer` copy-paste origin.** Evidence: repository-wide `rg` for `ItemViewerExpanded` over
> `*.cs` returns production references only from `QfcFormViewer.Designer.cs` (construction,
> `Controller = null`, layout properties) and `QfcFormViewer.cs:189-224` (designer control fields).
> `TipsLabels`/`LeftTipsLabels`/`ExpandedTipsLabels` getters (`:33-48`), the `Controller` getter
> (`:53`), `UiSyncContext` (`:60`), `UiScheduler` (`:66`), and `RemoveControlsColsRightOf` (`:69`)
> have no reader. Removing them is a public-API change and is therefore out of scope under the
> epic's no-behavior-change NFR; promote rather than delete. Note the tension: covering them (§ 6)
> is coverage of dead code, which the plan should record explicitly.

---

## 6. Test plan sketch (Q6)

Each row is one atomic task per issue #136. Coverage arithmetic assumes seam set S1 + S2.

### 6.1 Post-extraction denominator

Extraction removes 50 currently-uncovered coverable lines from `ItemViewerExpanded.cs`
(70–87 = 16, 90–99 = 10, 130–140 = 11, 143–156 = 13) and adds one expression-bodied delegation line.
New denominator: **106 − 50 + 1 = 57 coverable lines** and **1 branch point (2 outcomes)** — line
171 only.

### 6.2 Tests for `ItemViewerExpanded.cs`

New file `QuickFiler.Test/Viewers/ItemViewerExpanded.StaTests.cs` (`[STATestClass]`), constructing a
never-shown control in a `using` block. Add `<Compile Include="Viewers\ItemViewerExpanded.StaTests.cs" />`
to `QuickFiler.Test/QuickFiler.Test.csproj` (explicit-include project; see § 7).

| # | Test name | Production lines covered | Branch outcomes | Seam | Mocks |
| --- | --- | --- | --- | --- | --- |
| T1 | `Constructor_PopulatesTipsLabelCollections` | 33–36 (`get_TipsLabels`, line 35), 39–42 (line 41), 45–48 (line 47) | — | construction | none |
| T2 | `Constructor_CapturesUiSyncContextAndScheduler` | 60, 66 | — | construction | none |
| T3 | `ControllerProperty_RoundTripsAssignedValue` | 53 (getter; 54 already covered) | — | construction | `Mock<IItemControler>` |
| T4 | `MenuItemCheckedChangedHandler_WhenMenuItemUnchecked_ClearsImage` | 170, 171(false), 176–179 | 171-false (already covered; re-pinned deterministically) | S2 (`internal static`) | none — plain `ToolStripMenuItem` |
| T5 | `MenuItemCheckedChangedHandler_WhenMenuItemChecked_AppliesCheckedImage` | **172, 173, 174** | **171-true — the gating outcome** | S2 (`internal static`) | none — `new ToolStripMenuItem { Checked = true }` |
| T6 | `MenuItemCheckedChangedEvent_WhenMenuItemCheckStateChanges_InvokesTypedOverload` | 164, 165, 166, 167 | — | construction + designer wiring (`Designer.cs:171`) | none — set `viewer.ConversationMenuItem.Checked = true` (public field, `Designer.cs:811`) |
| T7 | `MenuItemCheckedChangedEvent_WhenSenderIsNotMenuItem_Throws` (negative) | 164, 165 | — | S2 | none — assert `InvalidCastException` on `:165` |
| T8 | `RemoveControlsColsRightOf_DelegatesToTrimmerWithWebViewSpanTarget` | the delegation line | — | construction | none — assert the observable TLP mutation |
| T9 | `WebViewParentChanged_WhenReparented_RunsHandler` | 159, 160, 161 | — | construction + designer wiring (`Designer.cs:274`) | none — re-parent `L0v2h2_WebView2` into a local `Panel` |

Projected result: **57/57 lines = 100%**, **2/2 branch outcomes = 100%**.

Minimum set to clear both gates: T1–T3 (6 lines) + T4–T6 (13 lines) added to the 34 lines that
survive extraction and are already covered = **53/57 = 93.0% line**, and T5 alone lifts branch from
1/2 to **2/2 = 100%**. T7–T9 are additive hardening. **T5 is load-bearing: without it the file sits
at 50% branch and fails the 75% gate regardless of line coverage.**

T4/T5/T7 touch only `ToolStripMenuItem` (a `Component`, no window handle) and could run in a plain
`[TestClass]`. Recommendation: put T4, T5, T7 in an ordinary
`QuickFiler.Test/Viewers/ItemViewerExpandedMenuTests.cs` and reserve the `*.StaTests.cs` file for the
construction-dependent T1–T3, T6, T8, T9 — this keeps the STA surface minimal, which is the stated
purpose of the epic's dedicated-file rule.

### 6.3 Tests for the extracted `ControlColumnTrimmer`

New file `QuickFiler.Test/Viewers/ControlColumnTrimmer.StaTests.cs` (`[STATestClass]`). All controls
are `Panel`/`Label`/`TableLayoutPanel`, never shown, disposed in `using`. This mirrors the existing
`UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs` (`[STATestClass]`, `:10`) that already
covers the `RemoveSpecificColumn` extension this code calls.

| # | Test name | Extracted lines (original numbering) | Branch outcomes | Notes |
| --- | --- | --- | --- | --- |
| T10 | `RemoveColumnsRightOf_WhenParentIsTableLayoutPanel_TrimsTrailingColumns` | 70–75, 77–82 | 71-true, 77-true | TLP with `furthestRight` in a non-last column |
| T11 | `RemoveColumnsRightOf_WhenTargetIsInLastColumn_LeavesColumnsIntact` | 70–75, 77 | 77-false | asserts `ColumnCount` unchanged |
| T12 | `RemoveColumnsRightOf_WhenParentIsNotTableLayoutPanel_FallsBackToControlRemoval` | 70, 71, 84–87 | 71-false | `Panel`-parented control |
| T13 | `RemoveControlsRightOf_WhenControlsExistToTheRight_RemovesAndDisposesThem` | 90–99 | 92-true | assert removed from `Parent.Controls` and `IsDisposed` |
| T14 | `RemoveControlsRightOf_WhenNothingIsToTheRight_MakesNoChange` | 90, 91, 92 | 92-false | empty result list |
| T15 | `ControlsRightOf_WhenAnchorIsInTree_UsesItsWalkedLocationAsLimit` | 130–140, 143–147, 149?, 152–156 | 143-true, 152-true | nested child so the walked location differs from `Location` |
| T16 | `ControlsRightOf_WhenAnchorIsOutsideTree_FallsBackToAnchorOwnLocation` | 149, 150, 151 | 143-false | anchor not parented into the walked root |
| T17 | `ControlsRightOf_WhenAllControlsAreLeftOfLimit_ReturnsEmpty` | 152–156 | 152-false | — |

Projected: 100% line and 100% branch on the new module, against a **>= 90%** target for newly created
files.

### 6.4 Determinism compliance

Every test above is deterministic: no `Thread.Sleep`, no `Task.Delay`, no wall-clock read, no timer,
no temporary file, no external service, no `Show()`/`ShowDialog()`, no message pump. `Control.Location`
and `Control.Size` are set explicitly in each fixture rather than being read from a laid-out form, so
no layout pass is required. No `DateTime.Now` is involved anywhere in this file.

---

## 7. File-size and project-file impact (Q7)

| File | Now | After | Limit |
| --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewerExpanded.cs` | 181 | ~126 (removes the 59 source lines 69–99 and 129–156, adds ~4 for the delegation and `using`) | 500 — compliant with wide margin |
| `QuickFiler/Viewers/ControlColumnTrimmer.cs` (new) | — | ~100–115 (59 extracted body lines + signatures, XML docs, `using`s) | 500 — compliant |
| `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` | 821 | 821 (untouched) | generated — exempt from the 500-line rule per epic § 5 |

Project-file edits required (both are explicit-include, non-SDK, CRLF projects — use the Edit tool,
never `sed -i`, and keep hunks minimal and adjacent):

- `QuickFiler/QuickFiler.csproj` — add `<Compile Include="Viewers\ControlColumnTrimmer.cs" />` near the
  existing `Viewers\` block (`:392`, `:438-454`).
- `QuickFiler.Test/QuickFiler.Test.csproj` — add `<Compile Include="Viewers\...Tests.cs" />` entries
  in the `Viewers\` block (`:60-91`).
- `ControlColumnTrimmer.cs` is a new production file: it takes the **>= 90% line** target and needs
  its own row in F1's ledger, appended in the same change that adds the `<Compile Include>` entry
  (epic § "Mid-Wave File Creation and the Ledger Denominator", rules 3 and 4).

---

## 8. Cross-child notes and sibling boundaries

`ItemViewerExpanded.cs` has **no dependency** on any F13 (breadcrumb drop-down / WebView2 host),
F12 (breadcrumb bridge / messenger), or F10 (`QfcItemController.*`) file. Its only WebView2 contact
is the field read at `:75`, and the S1 extraction removes even that from the test surface. No edit to
any F10/F12/F13 file is proposed.

- **CC-1 — `QuickFiler/Viewers/ToolStripMenuItemCb.cs` (owner: F15).** The `Checked` shadow defect in
  § 3 lives there. F14 must **not** edit it. F14's T5 works around it by invoking the seam with a
  plain base `ToolStripMenuItem`. If F15 later fixes the shadow (e.g. by assigning `base.Checked` in
  the setter, `ToolStripMenuItemCb.cs:35-50`), T6's assertion on the *resulting image* must be
  revisited — the line coverage T6 produces is unaffected, but the expected image would flip.
  Exact change, if taken: `ToolStripMenuItemCb.cs:37`, add `base.Checked = value;` alongside
  `_checked = value;`. Owning child: F15 (or the promoted bug from LD-1).
- **CC-2 — `QuickFiler/Viewers/ItemViewer.cs` (owner: F14, parallel researcher).** The S1 extraction
  is only worth doing once. `ItemViewer.cs:77-95`, `:97-107`, and `:137-164` are the verbatim twins
  of the extracted bodies and should delegate to the same `ControlColumnTrimmer`. Coordinate so the
  extraction is planned in one atomic phase, not twice.
- **CC-3 — `QuickFiler/Controllers/QfcHomeController.cs:93` and `:133` (owner: F7).** These are the
  only paths that construct `ItemViewerExpanded` in the current test run. F7's research already
  identified `:133` as a live-form line. If F7 seams it away, this file's incidental 37.7% drops to
  roughly the constructor's 11 lines or to zero. F14's plan must therefore own its own construction
  fixture (T1–T3, T6, T8, T9) rather than relying on F7's tests. No edit to `QfcHomeController.cs` is
  proposed by F14.

### `UtilitiesCS` boundary

Verified: `UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants `InternalsVisibleTo` only to
`DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test` — **not** `QuickFiler.Test`. No
`UtilitiesCS` internal is reachable from F14's tests and that file must not be edited. This is not a
constraint in practice for this file: the two `UtilitiesCS` members it uses,
`Control.ForAllControls` (`ItemViewerExpanded.cs:132`) and
`TableLayoutPanel.RemoveSpecificColumn` (`:80`, defined at
`UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs:106`), are both **public** extension
methods.

---

## 9. Latent defects — promotion candidates

Each is out of scope to fix under the epic's no-behavior-change NFR and should be promoted through
the MCP lifecycle rather than left as prose.

- **LD-1 — `ToolStripMenuItemCb.Checked` shadow never reaches the base property; the menu check image
  is cleared immediately after being set.** Full evidence in § 3. Files:
  `QuickFiler/Viewers/ToolStripMenuItemCb.cs:32-58` (F15), consumed at
  `QuickFiler/Viewers/ItemViewerExpanded.cs:169-179` and `QuickFiler/Viewers/ItemViewer.cs:177-187`.
  Severity: user-visible — the four move-option menu items never display a check mark.
- **LD-2 — Production `Console.WriteLine` in a WinForms event handler.**
  `QuickFiler/Viewers/ItemViewerExpanded.cs:160` is `Console.WriteLine("Parent Changed");`, the entire
  body of the designer-wired `L0v2h2_WebView2_ParentChanged` handler
  (`ItemViewerExpanded.Designer.cs:274`). `ItemViewer.cs:168` is identical. This violates the General
  Code Change Policy § 3 ("Use the project's logging pattern instead of ad-hoc print/console output").
  The handler is otherwise a no-op, so an alternative disposition is deletion of both the handler and
  its designer wiring — a behavior change, hence promotion rather than in-scope fix.
- **LD-3 — Unguarded downcast in an event handler.**
  `QuickFiler/Viewers/ItemViewerExpanded.cs:165` is `var menuItem = (ToolStripMenuItem)sender;` with no
  `is`/`as` guard. Any non-`ToolStripMenuItem` sender raises `InvalidCastException` on the UI thread.
  Same at `ItemViewer.cs:173`. Low severity (all four current wirings pass a `ToolStripMenuItemCb`),
  but it is a fail-fast-without-context path.
- **LD-8 — Dead public surface on `ItemViewerExpanded`.** Full evidence in § 5.1.

Reference-only (already tracked, do not re-promote):

- **#441** — Cobertura post-processing double-counts `<line>` nodes and corrupts class-level
  `line-rate`. This research independently reproduced the symptom on this exact file (§ 1.3):
  `line-rate="0.390244"` (32/82) against 106 `<line>` children with 40 covered (37.74%). F1's harness
  must recompute the rate from distinct `<line>` children.

---

## 10. Open-issue scan

Method: GitHub public issue-search UI via WebFetch (no shell available for `gh`). Terms searched:
`ItemViewerExpanded`, `ItemViewer`, `expanded`, `designer`, `viewer`, `coverage`.

`ItemViewerExpanded` as a term returns no dedicated issue. Open issues returned that touch this
child's territory:

| Issue | Title | Relevance to this file |
| --- | --- | --- |
| #456 | Feature: quickfiler-itemviewer-coverage | this child |
| #441 | Cobertura post-processing double-counts `<line>` nodes, inflating lines-valid and every coverage rate | **directly explains § 1.3**; the acceptance figure must be recomputed from `<line>` children |
| #432 | Feature: quickfiler-coverage-ledger | F1 — the ledger this file's classification lands in |
| #230 | Build a WinForms message-pump test seam (`Application.Run()` background thread) to unblock 9 `QfcItemController` orchestration members | adjacent: an alternative to the STA clause. Not needed for this file — no member requires a running message pump; construction alone suffices (§ 1.4). |
| #455, #440, #438, #439 | breadcrumb drop-down coverage / navigation / focus-steal / EfcViewer lineage | F13/F12 territory; **no overlap** with `ItemViewerExpanded.cs` |

No promoted-but-not-yet-active issue was found that conflicts with this file.

---

## 11. Premises confirmed and corrected

Confirmed as stated in the delegation brief:

- `ItemViewerExpanded.cs:16` is `public partial class ItemViewerExpanded : UserControl`, carries no
  exclusion attribute, and is instrumented.
- `ItemViewer` is suppressed by a type-level `[ExcludeFromCodeCoverage]` at `ItemViewer.cs:20`, and no
  `ItemViewer.*` partial appears in the report.
- `branch-rate="0.083333"` is exact: 1 of 12 outcomes. Independently recomputed from the `<line>`
  children.
- The STA last-resort clause is available (the type is a `UserControl`), and STA infrastructure
  already exists in-repo and needs no new package.

Corrected / extended:

1. **`line-rate="0.390244"` is not trustworthy.** Recomputed from the element's own `<line>` children
   the figure is 37.74% (40/106). Open issue #441 documents the cause. Both figures fail the 80%
   gate, so no target changes — but the plan must state which number it is measuring against.
2. **The multi-`<class>`-per-filename union directive does not apply here.** Verified: only two
   `<class>` elements carry an `ItemViewerExpanded` filename and they name different files.
3. **STA is not required for construction.** `ItemViewerExpanded` is already constructed to
   completion inside a plain `[TestMethod]` in the current suite
   (`QfcHomeControllerTests.cs:114`/`:179`). The STA scoping recommended in § 6.2 is defensive
   consistency with the epic's convention, not a technical necessity for construction.
4. **A seam is mandatory, not discretionary, for the branch gate.** The `true` arm of line 171 is
   unreachable through every public path because of the `ToolStripMenuItemCb` shadowing defect (§ 3).
   The `issue.md` framing that STA construction is the fallback for the 39.0%/8.3% gap is incomplete:
   construction alone cannot reach 75% branch.
5. **The 37.7% is incidental, not intentional.** It is produced entirely by F7-owned tests via a live
   `QfcFormViewer` construction and is at risk from F7's own seam work (CC-3).
