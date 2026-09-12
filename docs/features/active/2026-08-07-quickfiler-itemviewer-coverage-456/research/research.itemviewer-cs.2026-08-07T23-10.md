# Research — `QuickFiler/Viewers/ItemViewer.cs`

- Feature: F14 `quickfiler-itemviewer-coverage` (issue #456), child of epic #136 `quickfiler-per-file-coverage`
- Timestamp: 2026-08-07T23-10
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5e4b635834feedd7`
- Target file: `QuickFiler/Viewers/ItemViewer.cs` (432 lines) — the primary partial, carrying the family's
  only real `[ExcludeFromCodeCoverage]` at `:20`
- Compile entry: `QuickFiler/QuickFiler.csproj:412-414` (`<SubType>UserControl</SubType>`, no `DependentUpon`)

Claims are marked **[V]** (verified by direct file read, report inspection, or fetched documentation) or
**[E]** (estimated by a stated model). No claim rests on assumption alone.

> **Tooling note.** No shell tool was available this session, so `gh issue list --state open --search ...`
> could not be run; the open-issue scan (§8) used the public GitHub issue-search UI via WebFetch, and its
> method is recorded there. No build or test run was performed, so no figure in this artifact is a
> measurement of `ItemViewer.cs` itself — it cannot be, because the file is not instrumented today.

---

## 0. Headline determinations

| Question | Answer |
| --- | --- |
| **Q1** — one part's attribute suppresses the whole partial type incl. the Designer | **CONFIRMED**, with documentation and a provably-executed-yet-absent positive control. Full evidence in the companion artifact `research.itemviewer-designer-cs.2026-08-07T23-10.md` §1; summarised in §1.2 below. |
| **Q2** — repository-wide risk of removing the attribute | **Improves or is flat (+0.57 pp to −0.08 pp). Exempting the designer instead is the option that reduces it (−0.16 pp).** Sized in the companion artifact §2.4. |
| **Q3** — designer-only exemption mechanism | **None available and permitted.** Do not exempt; measure. Companion artifact §3. |
| **Q4** — seams | Two: **S1** the shared `ControlColumnTrimmer` extraction (already proposed by the `ItemViewerExpanded.cs` sibling — plan it once, not twice), and **S2** `internal` widening of the two menu handlers. **No COM-bound member exists in this file.** §4. |
| **STA** | **Not required.** Ten existing plain `[TestMethod]`s already construct a live headless `ItemViewer`. No `*.StaTests.cs` file is warranted for this file. §4.4. |
| **Q5** — 500-line rule | 432 now, **~390 after S1**. No split. One **new** production file (`ControlColumnTrimmer.cs`) needs a csproj entry, a ledger row, and >= 90% line coverage. §5. |
| **Q6** — existing tests | Substantial and previously unmeasurable. Twelve test files touch `ItemViewer`; ten construct it live. §6. |
| **Q7** — test plan | 17 atomic cases; after S1 the file's branch gate reduces to a single decision point, which sits in **dead code**. §7. |

---

## 1. Current state

### 1.1 Type shape [V]

`ItemViewer.cs:20-21`:

```csharp
[ExcludeFromCodeCoverage]
public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal
```

Confirming the orchestrator's premises:

- **`UserControl`, not `Form`** — the epic's STA last-resort clause (§ Shared Design 3) is *available*.
  §4.4 concludes it is not *needed*.
- **`ItemViewer.cs:20` carries the family's only real attribute.** `ItemViewer.Commands.cs:10`,
  `.DisplayState.cs:9`, `.FolderSearch.cs:17` and `.WebViewThread.cs:12` mention it in comments only
  (`epic.md:121-130` records the same correction). `ItemViewer.Designer.cs:5` is a bare
  `partial class ItemViewer` with no attribute. `ItemViewer.Breadcrumb.cs` carries none (its two
  method-level exclusions were removed under issue #400 P9-T12 — see the sibling artifact).
- **No Outlook Interop anywhere in this file.** The `using` block (`:1-16`) is `System`,
  `System.Collections.Generic`, `System.ComponentModel`, `System.Data`,
  `System.Diagnostics.CodeAnalysis`, `System.Drawing`, `System.Linq`, `System.Text`,
  `System.Threading`, `System.Threading.Tasks`, `System.Windows.Forms`, `System.Windows.Threading`,
  `SVGControl`, `UtilitiesCS`, `UtilitiesCS.Interfaces.IWinForm` (plus one commented-out line at `:11`).
  No member touches `Application`, `MailItem`, `Store`, or `MAPIFolder`. **`CLAUDE.md` §UT2's
  Outlook-Interop exemption ground therefore does not apply to this file at all**, and the WinForms
  ground (b) is for *form-derived* classes, which this is not. There is no ratifiable exemption ground
  for `ItemViewer.cs`; the attribute must come off.

### 1.2 Measured baseline: none exists, and why [V]

No `<class>` element with any `Viewers\ItemViewer*.cs` filename appears in the committed report
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.
Same-folder positive controls prove the folder was instrumented (`ItemViewerExpanded.cs` at XML `:5364`,
`ItemViewerExpanded.Designer.cs` at `:4112`, `BreadcrumbItemViewerLifecycleCoordinator.cs` at `:7850`), so
the absence is caused by the attribute, not by an instrumentation gap.

Two authorities settle Q1 (detail in the companion designer artifact §1):

1. Microsoft documentation for `ExcludeFromCodeCoverageAttribute`: `AllowMultiple = false`,
   `Inherited = false`, and *"Placing this attribute on a class or a structure excludes all the members of
   that class or structure from the collection of code coverage information."* A partial type is one class.
2. Repository positive control: `QfcFormViewer.cs:17` is attributed; `QfcFormViewer.Designer.cs:3` is not;
   `QfcFormViewer.Designer.cs:42` **provably executed** (it is the sole construction site of
   `ItemViewerExpanded`, whose designer shows `hits="1"` throughout); yet neither file produces a `<class>`
   element. Executed-but-absent is conclusive.

**Planning consequence.** "Assume 0%" is wrong for this file. Ten test files construct a live `ItemViewer`
(§6), so a meaningful fraction of `ItemViewer.cs` is *executed* today while being *unmeasured*. The plan's
first step must be **remove the attribute → run F1's harness → record the actual per-file line and branch
rate**, and every subsequent test task must be justified against that measured gap. The
`ItemViewer.Breadcrumb.cs` sibling artifact reached the same conclusion (its P4, §5.1, T0/T0b) and this
artifact adopts its sequencing.

### 1.3 Denominator estimate for this file [E]

Using the line-counting model validated to ±2 lines on `ItemViewerExpanded.Designer.cs` (companion
artifact §2.1), and measured inputs for this file [V] — 432 physical, 27 blank, 7 comment-only, 102 lines
containing `=>`, 8 private fields without initialisers, 51 property declarations with block bodies, 15
`using` lines:

```
432 − 27 blank − 7 comment − 15 using − 7 namespace/class/attribute/braces
    − 8 field declarations − 153 (51 property declaration + open/close brace lines)
    − 9 method signature lines − 2 region markers  ≈  204 coverable lines
```

**~205 coverable lines (±10).** Of these, **~96 are property-accessor bodies** (`get =>` / `set =>`), which
is the single most important fact for the test plan: half this file's denominator is trivially coverable
by assigning and reading properties on an instance.

**Branch inventory — 6 decision points / 12 outcomes** [V]:

| Line | Predicate |
| --- | --- |
| 79 | `if (furthestRight.Parent is TableLayoutPanel)` |
| 85 | `if (++columnNumber < tlp.ColumnCount)` |
| 100 | `for (int i = controlsToRemove.Count - 1; i >= 0; i--)` |
| 151 | `if (controlLocation.Any(tup => tup.Control == furthestRight))` |
| 161 | `.Where(tup => tup.Point.X > limit.X)` predicate |
| 179 | `if (menuItem.Checked)` |

This matches the measured shape of the near-verbatim twin `ItemViewerExpanded.cs` (`complexity="14"`, 6
branch points, 12 outcomes, report `:5364`), which is corroboration that the count is right.

---

## 2. The most consequential structural finding: three dead members [V]

**`MenuItem_CheckedChanged(object, EventArgs)` (`:171-175`), `MenuItem_CheckedChanged(ToolStripMenuItem)`
(`:177-187`), and `MoveOptionsMenu_Click(object, EventArgs)` (`:205`) have no caller and no designer
wiring anywhere in the solution.**

Evidence — a repository-wide search over `*.cs` for those three identifiers returns:

- `ItemViewerExpanded.Designer.cs:171, 180, 189, 198` — four `CheckedChanged +=` wirings, and
  `ItemViewerExpanded.cs:24-27` — four constructor calls. **All in the sibling type.**
- `ItemViewer.cs:171, 174, 177, 205` — the declarations themselves and the one internal call from the
  `(object, EventArgs)` overload to the typed overload.
- `EfcViewer.cs:109-139` — commented out.
- **`ItemViewer.Designer.cs` wires exactly one handler**: `:256`,
  `this._l0v2h2_WebView2.ParentChanged += new System.EventHandler(this.L0v2h2_WebView2_ParentChanged);`.
  It contains **no** `CheckedChanged` or `Click` wiring at all (verified by grep for
  `MenuItem_CheckedChanged|ParentChanged|MoveOptionsMenu_Click|EventHandler\(this\.` across all 6,224 lines
  — one hit).
- `ItemViewer`'s constructor (`:23-30`) does **not** call `MenuItem_CheckedChanged`, unlike
  `ItemViewerExpanded`'s (`ItemViewerExpanded.cs:24-27`).

**Three consequences, all load-bearing:**

1. **The file's only surviving branch point after seam extraction sits in dead code.** After S1 removes the
   five geometry branch points, `:179` is the sole decision point in `ItemViewer.cs`, and it is in a method
   nothing calls. Covering it means writing a test for unreachable production code.
2. **`ItemViewer` is arguably the *correct* one of the twins.** The `ItemViewerExpanded.cs` sibling artifact
   established (its §3, LD-1) that `ToolStripMenuItemCb`'s `Checked` setter sets `base.Image` and then
   raises the shadowed event, whose handler reads the *base* `Checked` (permanently `false`) and executes
   `menuItem.Image = null` — **wiping the check image the setter just applied**. `ItemViewer` never wires
   that handler, so its four move-option menu items keep their check image. This strengthens that sibling's
   LD-1 (the defect is in `ItemViewerExpanded`'s wiring, not in `ToolStripMenuItemCb` alone) and it means
   **F14 must not "fix" `ItemViewer` by adding the wiring** — that would introduce the defect.
3. **A cheap alternative to testing dead code exists**: delete the three members. See §7.1 for the
   recommendation and its rationale.

---

## 3. Member inventory (Q4)

`this` denotes an `ItemViewer` instance; "designer field" denotes a field declared in
`ItemViewer.Designer.cs:6178-6222`.

| # | Member | Lines | Class | Notes |
| --- | --- | --- | --- | --- |
| 1 | `.ctor()` | 23-30 | **thin wiring** | `InitializeComponent()`; captures `SynchronizationContext.Current` (`:26`); `TaskScheduler.FromCurrentSynchronizationContext()` (`:27`, **throws if no ambient context**); `Dispatcher.CurrentDispatcher` (`:28`); `InitControlGroups()` (`:29`). |
| 2 | `TipsLabels` / `LeftTipsLabels` / `ExpandedTipsLabels` getters | 35, 41, 47 | thin wiring | field reads; populated by `InitControlGroups` |
| 3 | `Controller` get/set | 55, 56 | thin wiring | `IItemControler` |
| 4 | `UiSyncContext` / `UiScheduler` / `UiDispatcher` getters | 62, 68, 74 | thin wiring | `UiDispatcher` (`:71-75`) is the only member `ItemViewerExpanded` lacks |
| 5 | `RemoveControlsColsRightOf(Control)` | 77-95 | **pure/host-neutral** except one field read | operates on `Control`/`TableLayoutPanel`; its sole instance dependency is `L0v2h2_WebView2` at `:83`, used purely as the *argument* to `tlp.SetColumnSpan` — any `Control` satisfies it. 2 branch points. Verbatim twin of `ItemViewerExpanded.cs:69-87`. |
| 6 | `RemoveControlsRightOf(Control)` | 97-107 | **pure/host-neutral** | private; walks and mutates `Control.Controls`. 1 branch point. Twin of `ItemViewerExpanded.cs:89-99`. |
| 7 | `InitControlGroups()` | 109-135 | thin wiring | builds three `List<Label>` from 11 designer Label fields |
| 8 | `ControlsRightOf(Control)` | 137-164 | **pure/host-neutral** except the `this` walk root | uses the **public** `UtilitiesCS` extension `Control.ForAllControls` (`:140`); the root is a parameterisable `Control`. 2 branch points. Twin of `ItemViewerExpanded.cs:129-156`. |
| 9 | `L0v2h2_WebView2_ParentChanged` | 166-169 | thin wiring | **the only designer-wired handler** (`ItemViewer.Designer.cs:256`); body is a single `Console.WriteLine` — LD-2 |
| 10 | `MenuItem_CheckedChanged(object, EventArgs)` | 171-175 | thin wiring | **dead** (§2); unguarded cast at `:173` — LD-3 |
| 11 | `MenuItem_CheckedChanged(ToolStripMenuItem)` | 177-187 | **pure/host-neutral** | **dead** (§2); uses no instance state; `ToolStripMenuItem` is a `Component`, needs no handle. Holds the file's last branch after S1. |
| 12 | `MenuItems` | 189 | thin wiring | `Initializer.GetOrLoad(ref _menuItems, LoadMenuItems)` — `UtilitiesCS/HelperClasses/Initializer.cs:103` is `public static`, so reachable. **Live**: `QfcItemController.EventWiring.cs:59` iterates it. |
| 13 | `LoadMenuItems()` | 192-203 | thin wiring | reads 5 designer menu fields |
| 14 | `MoveOptionsMenu_Click` | 205 | thin wiring | **dead** (§2); empty body |
| 15 | 44 field-to-property pairs | 209-428 | thin wiring | `#region Field to Property for Interface`; ~88 accessor lines. These exist solely to satisfy `IItemViewer` over `internal` designer fields. |

**Zero COM-bound members. Zero members that require a message pump. Zero members that read wall-clock time,
use a timer, or call `Thread.Sleep`/`Task.Delay`** (verified: no `DateTime`, `Stopwatch`, `Timer`,
`Task.Delay`, `Thread.Sleep` token anywhere in the file). The `issue.md:53-54` constraint that
"tests must use an injected clock and fake timers" does **not** apply to `ItemViewer.cs`. (For the record,
and correcting the orchestrator brief: `FakeTimeProvider` **is** available in `QuickFiler.Test` on net481
via `Microsoft.Bcl.TimeProvider` + `Microsoft.Extensions.TimeProvider.Testing` — verified and documented
by the `ItemViewer.Breadcrumb.cs` sibling artifact §4(b). It is simply not needed here.)

### 3.1 Lines reachable only by constructing the control [V]

- `.ctor` body (`:24-30`) — by definition.
- `InitControlGroups` (`:110-135`) and the three tips-label getters — they read 11 designer `Label` fields
  that only `InitializeComponent()` populates.
- `LoadMenuItems` (`:192-203`) — reads 5 designer menu fields.
- `L0v2h2_WebView2_ParentChanged` (`:167-169`) — reached by re-parenting the designer's `WebView2`.
- The `RemoveControlsColsRightOf` delegation line after S1 — reads the `L0v2h2_WebView2` field.

Everything else — all 44 property pairs, `Controller`, and (with S2) both menu handlers — is reachable on a
`FormatterServices.GetUninitializedObject`-style instance with no constructor at all, the technique already
used at `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:249`.

**Construction is empirically safe on the default MSTest apartment** — see §4.4. **No line in this file
requires the STA last-resort clause.**

---

## 4. Recommended seam set (Q4)

Applying the epic's hierarchy (interface seam > injectable delegate > adapter) and
`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy ("extract all logic into host-neutral,
testable modules and leave only the thinnest possible wiring in the host-bound entry point").

### S1 — Extract the three geometry methods into one shared host-neutral module

**This is the same extraction the `ItemViewerExpanded.cs` sibling artifact proposes as its S1
(`research.itemviewerexpanded-cs.2026-08-07T21-40.md` §4.1, CC-2). Plan it ONCE.** `ItemViewer.cs:77-95`,
`:97-107`, `:137-164` are verbatim twins of `ItemViewerExpanded.cs:69-87`, `:89-99`, `:129-156`; both files
are F14-owned, so this is an intra-child change and one tested module serves both.

New production file `QuickFiler/Viewers/ControlColumnTrimmer.cs`, `internal static class ControlColumnTrimmer`:

- `internal static void RemoveColumnsRightOf(Control root, Control furthestRight, Control columnSpanTarget)`
- `internal static void RemoveControlsRightOf(Control root, Control furthestRight)`
- `internal static List<Control> ControlsRightOf(Control root, Control furthestRight)`

`ItemViewer.cs` retains one expression-bodied wiring line:

```csharp
public void RemoveControlsColsRightOf(Control furthestRight) =>
    ControlColumnTrimmer.RemoveColumnsRightOf(this, furthestRight, L0v2h2_WebView2);
```

`internal` suffices: `QuickFiler/Properties/AssemblyInfo.cs:5` grants
`InternalsVisibleTo("QuickFiler.Test")` [V]. No interface is warranted — there is one implementation, and
the general policy's simplicity-first rule argues against an abstraction with a single implementor.

**`IItemViewer.cs:131` (`void RemoveControlsColsRightOf(Control furthestRight);`) does not change**, so
`EfcItemController.cs:247` — the sole production call site, reached through `IItemViewer` — is untouched,
and every `Mock<IItemViewer>` in `QuickFiler.Test` keeps compiling. The `IItemViewer.cs` sibling artifact
independently confirms this (its §5).

Effect on this file: removes 5 of 6 branch points, ~50 coverable lines, and every `TableLayoutPanel`
manipulation from the `UserControl`'s test surface.

### S2 — Widen the two menu handlers to `internal`

- `MenuItem_CheckedChanged(ToolStripMenuItem)` (`:177`) → `internal static` (it reads no instance state;
  `:174` continues to compile).
- `MenuItem_CheckedChanged(object, EventArgs)` (`:171`) → `internal`.

Precedent: `QuickFiler/Controllers/QfcHomeController.cs:111` declares `internal async Task InitAsync(...)`
and `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:220` calls it directly [V].

**S2 is only required if the planner keeps the dead members** (§7.1). If they are deleted, S2 is unnecessary.

### 4.3 Rejected alternatives (brief)

- **Interface seam over `ToolStripMenuItem.Checked`/`.Image`.** Rejected: adds a production abstraction with
  exactly one implementation and does not change what the test can assert.
- **Retyping any designer-backed property** (e.g. `L0v2h2_WebView2`, `TopicThread`). Rejected: a
  reflection-injection harness assigns them by concrete type
  (`QfcThemeHelperTests.cs:250-258`), and a contract test pins one property's exact concrete type
  (`QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:19-29`). The
  `ItemViewer.Breadcrumb.cs` sibling artifact §3.2 documents this rule; it applies to this file's region
  too.
- **Extracting `InitControlGroups` / `LoadMenuItems` behind a seam.** Rejected: they exist only to project
  designer fields into lists. A seam would be pure indirection; construction covers them for free.
- **STA-scoped tests.** Rejected on evidence — see §4.4.

### 4.4 STA determination — not required [V]

`ItemViewer` is constructed to completion inside plain `[TestClass]`/`[TestMethod]` in ten places today
(§6), including a real `Microsoft.Web.WebView2.WinForms.WebView2` (`ItemViewer.Designer.cs:46`, `:49`), a
`MenuStrip`, four `ToolStripMenuItemCb`, a `BrightIdeasSoftware.FastObjectListView`, six `ButtonSVG`
SVG-backed controls, and a `ComponentResourceManager` over `ItemViewer.resx`. No `[STATestClass]` exists
anywhere in `QuickFiler.Test` (the sibling artifact verified: `STATestClass` appears only in `Tags.Test`,
`TaskVisualization.Test`, and docs). The assembly initializer
(`QuickFiler.Test/SetupAssemblyInitializer.cs`) calls only `Application.EnableVisualStyles()` and
`SetCompatibleTextRenderingDefault(false)`.

**Recommendation: create no `*.StaTests.cs` file for `ItemViewer.cs`.** The epic's STA clause is a
last resort for cases where no seam works; here neither a seam nor STA is needed, because the plain
construction path is already proven. This keeps the STA surface at zero for `QuickFiler.Test`, which is the
stated purpose of the epic's dedicated-file rule.

**One hard fixture requirement, not optional** [V]: `ItemViewer.cs:27` calls
`TaskScheduler.FromCurrentSynchronizationContext()`, which throws `InvalidOperationException` when
`SynchronizationContext.Current` is null. Every test that constructs an `ItemViewer` **must** install a
context first:

```csharp
_previous = SynchronizationContext.Current;
SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
_viewer = new QuickFiler.ItemViewer();
```

Pattern in use at `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs:336-338` and six sibling
harnesses. Restore the previous context and dispose the viewer in `finally`/`[TestCleanup]`.

---

## 5. Q5 — 500-line rule and project-file impact

| File | Now | After | Limit |
| --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.cs` | **432** | **~390** (removes the ~48 source lines 80-94, 98-107, 138-164; adds ~3 for the delegation and a `using`) — **~373** if the three dead members are also deleted | 500 — compliant with wide margin |
| `QuickFiler/Viewers/ControlColumnTrimmer.cs` (new) | — | ~100-115 | 500 — compliant |
| `QuickFiler/Viewers/ItemViewer.Designer.cs` | 6,224 | 6,224 (untouched) | generated — exempt (epic § Shared Design 5) |

**No partial split of `ItemViewer.cs` is required.** S1 makes the file smaller, not larger; S2 changes
visibility keywords only. If a future step somehow pushed it past 500, the natural cleavage is the
`#region Field to Property for Interface` block (`:207-430`, 224 lines) into
`ItemViewer.InterfaceProperties.cs`, which would need
`<Compile Include="Viewers\ItemViewer.InterfaceProperties.cs"><DependentUpon>ItemViewer.cs</DependentUpon><SubType>UserControl</SubType></Compile>`
adjacent to `QuickFiler/QuickFiler.csproj:412-434` — but this is contingency, not plan.

**Required project-file edits (the epic's one sanctioned shared file):**

- `QuickFiler/QuickFiler.csproj` — add `<Compile Include="Viewers\ControlColumnTrimmer.cs" />` inside the
  `Viewers\` block (near `:392` or `:411`). **Preserve CRLF; use the Edit tool, never `sed -i`; keep the
  hunk minimal and adjacent** (epic § Cross-Child Constraints 1). Note `.csproj` uses no globbing — the
  file will not compile without this entry.
- `QuickFiler.Test/QuickFiler.Test.csproj` — add `<Compile Include=... />` entries for each new test file
  in the existing `Viewers\` block. Same CRLF rule.
- `ControlColumnTrimmer.cs` is **new production code**: per epic § Mid-Wave File Creation rules 3 and 4 it
  takes the **>= 90% line** target and needs its own ledger row appended **in the same change** that adds
  the `<Compile Include>` entry.

---

## 6. Q6 — Existing tests touching `ItemViewer`

Twelve `QuickFiler.Test` files reference `ItemViewer` (excluding `ItemViewerExpanded`,
`ItemViewerQueue`, and `BreadcrumbItemViewerLifecycleCoordinator`). Three distinct usage modes:

**(a) Live construction — ten sites, all plain `[TestClass]`/`[TestMethod]`** [V]:

| File | Line(s) | What it drives in `ItemViewer.cs` |
| --- | --- | --- |
| `Viewers/BreadcrumbDropDownIntegrationTests.cs` | 338 | ctor, `InitControlGroups`, designer getters |
| `Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | 477 | same |
| `Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 413 | same |
| `Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 255 | same |
| `Viewers/BreadcrumbSubfolderActivationTests.cs` | 305 | same |
| `Viewers/BreadcrumbPendingOpenCloseTests.cs` | 363 | same |
| `Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | 373 | same |
| `Controllers/QfcItemController.ViewerSetupTests.cs` | 386 | ctor + `TipsLabels`/`ExpandedTipsLabels`/`UiSyncContext` via `QfcItemController.ViewerSetup.cs:216,220,260,275,280` |
| `Controllers/QfcItemController.EventWiringTests.cs` | 236, 327 | ctor + **`MenuItems` and therefore `LoadMenuItems`**, via `QfcItemController.EventWiring.cs:59` |

Every one of these runs the constructor (`:24-30`), `InitControlGroups` (`:110-135`), and — crucially for
the companion designer artifact — the whole of `ItemViewer.Designer.cs:InitializeComponent()`.

**(b) Constructor-bypassed instance — `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:247-265`** [V]:
`CreateUninitialized<ItemViewer>()` then eight property **setters** (`LblItemNumber`, `LblSender`,
`LblSubject`, `MoveOptionsStrip`, `TxtboxSearch`, `TxtboxBody`, `TopicThread`, `L0v2h2_WebView2`, plus
`L0vhBreadcrumb_WebView2` from the Breadcrumb partial) and `SetPrivateField(viewer, "_menuItems", …)`.
Consumed by `SetupThemes_*` (`:111`, `:141`) and `BuildProductionControlSet_MapsControllerAndViewerInputs`,
which reads the **`MenuItems` getter** (`:156`) and the **`MoveOptionsStrip` getter** (`:157`). No ctor, no
`InitializeComponent`, no `InitControlGroups`.

**(c) Reflection contract test — `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:19-29`**:
pins `L0vhBreadcrumb_WebView2`'s declared type to the concrete
`Microsoft.Web.WebView2.WinForms.WebView2`. Constructs nothing. Constrains any retyping (§4.3).

**Also present, and worth flagging:**

- `QuickFiler.Test/Form1.cs` + `Form1.Designer.cs:32-34` declare a `System.Windows.Forms.Form` that
  constructs three `QuickFiler.ItemViewer` instances. **No test instantiates `Form1`** (verified: the only
  `Form1` references in the whole test project are its own two declaration files). It is dead scaffolding.
  Harmless today, but it is a live `Form` sitting in a test assembly, one `new Form1()` away from a
  unit-test-policy violation. See LD-4.
- `QuickFiler.Test/QfcViewer_Test.cs:29, 70` — commented-out `ItemViewer` usage. Dead.
- `Helper Classes/TlpCellSnapShotTests.cs:53,109` and `TlpCellStatesTests.cs:206` use
  `Mock<IItemViewer>`, never a real `ItemViewer`. They are F4 territory and are unaffected by F14.
- `Helper Classes/ViewerQueueStaticWrapperTests.cs:97,128,208,280` inject
  `CreateUninitialized<ItemViewer>()` as a factory result, so `ItemViewerQueue.cs:105`
  (`return new ItemViewer();`) is **not** exercised as a construction path in tests.

**No existing test targets `ItemViewer.cs`'s own behaviour.** Everything above exercises it incidentally as
a fixture for another subject. That is exactly the risk §9 CN-2 addresses.

---

## 7. Q7 — Test plan

### 7.1 A gating decision the planner must make first: the three dead members

After S1, `:179` is the **only** branch point in `ItemViewer.cs`, and §2 proves it is unreachable from any
production path. Two defensible dispositions:

**Option A (recommended) — delete `MenuItem_CheckedChanged` (both overloads, `:171-187`) and
`MoveOptionsMenu_Click` (`:205`).** They are `private`, unreferenced, and unwired, so removal is
observably behaviour-neutral and cannot break a caller — the compiler proves it. Effects: removes ~17
coverable lines and the file's last branch point; `ItemViewer.cs` then reports 0 branch outcomes, which
F1's harness must report as N/A (not 0%) — the same reporting rule the `interface-only` bucket already
needs (epic § Directives for F1's Ledger, harness requirement 2). It also removes any temptation to
"restore" the wiring, which §2 shows would import `ItemViewerExpanded`'s check-image defect. **Do not
delete `L0v2h2_WebView2_ParentChanged` (`:166-169`) — it is wired at `ItemViewer.Designer.cs:256`.**

**Option B — keep them and cover them via seam S2.** Two tests (IV-8, IV-9) reach 2/2 = 100% branch. Costs
one production visibility change and produces coverage of dead code, which the `ItemViewerExpanded.cs`
sibling artifact explicitly flags as a category to record (its LD-8).

The plan below is written for **Option B** (the conservative, no-deletion reading of the epic's
no-behaviour-change NFR) and marks the two cases that disappear under Option A. **Promote LD-1 either way.**

### 7.2 Post-S1 denominator [E]

~205 − ~50 (extracted) + 1 (delegation) ≈ **~156 coverable lines**, **1 branch point / 2 outcomes**
(Option B) or **~139 lines / 0 branch points** (Option A).

### 7.3 Case inventory

Every case: MSTest `[TestClass]`/`[TestMethod]`, Moq where a collaborator is needed, FluentAssertions,
AAA, a descriptive name, no temp files, no external services, no live `Form`, no popup, no
`Thread.Sleep`/`Task.Delay`/wall-clock wait, no `[STATestClass]`. Per issue #136 each row is one atomic task.

Proposed homes (both new; both need `<Compile Include=...>` entries in `QuickFiler.Test/QuickFiler.Test.csproj`):

- `QuickFiler.Test/Viewers/ItemViewerConstructionTests.cs` — fixture **V** = `new QuickFiler.ItemViewer()`
  inside a `SynchronizationContext` scope, disposed in `[TestCleanup]`.
- `QuickFiler.Test/Viewers/ItemViewerSurfaceTests.cs` — fixture **U** =
  `CreateUninitialized<ItemViewer>()` (pattern `QfcThemeHelperTests.cs:249`), no ctor.

Split either into `.Part2.cs` if it approaches 500 lines.

| # | Test name | Production lines / outcomes | Fixture | Seam | Mocks |
| --- | --- | --- | --- | --- | --- |
| IV-1 | `Constructor_CapturesAmbientSyncContextSchedulerAndDispatcher` | 24, 25, 26, 27, 28, 29, 30; getters 62, 68, 74 | V | — | none |
| IV-2 | `Constructor_WithNoAmbientSynchronizationContext_Throws` (negative) | 25, 26, **27 throw path** | V (context cleared) | — | none |
| IV-3 | `Constructor_PopulatesTipsLabelsInDeclaredOrder` | 110, 111-128, 135; getter 37 | V | — | none — assert the 11 labels are the designer instances, in order |
| IV-4 | `Constructor_PopulatesLeftAndExpandedTipsLabels` | 130, 134; getters 43, 49 | V | — | none |
| IV-5 | `ControllerProperty_RoundTripsAssignedValue` | 55, 56 | U | — | `Mock<IItemControler>` |
| IV-6 | `LabelProperties_RoundTripAssignedControls` | ~40 accessor lines in `:209-278`, `:304-308`, `:329-333`, `:339-343`, `:349-353`, `:364-378`, `:424-428` | U | — | none |
| IV-7 | `TextAndListProperties_RoundTripAssignedControls` | `:279-303` (TxtboxBody, TopicThread, Sender, SentDate, Infolder), `:389-393` | U | — | none |
| IV-8 | `WebViewAndLayoutProperties_RoundTripAssignedControls` | `:309-328` (L0v2h2_WebView2, L0vh_Tlp, L1h0L2hv3h_TlpBodyToggle, L1h1L2v1h3Panel) | U | — | none — `CreateUninitialized<WebView2>()` |
| IV-9 | `ButtonSvgProperties_RoundTripAssignedControls` | `:334-338`, `:344-348`, `:354-363`, `:379-388` | U | — | none — `ButtonSVG` from `SVGControl` |
| IV-10 | `MenuProperties_RoundTripAssignedControls` | `:394-423` (MoveOptionsStrip, MoveOptionsMenu, 4× `ToolStripMenuItemCb`) | U | — | none |
| IV-11 | `MenuItems_FirstAccess_LoadsFiveMenuComponentsInDeclaredOrder` | 189, 193-202 | V | — | none |
| IV-12 | `MenuItems_SecondAccess_ReturnsCachedInstance` | 189 (the `GetOrLoad` cached path) | V | — | none — `BeSameAs` |
| IV-13 † | `MenuItemCheckedChanged_WhenChecked_AppliesCheckedImage` | **179-true**, 181 | — | **S2** (`internal static`) | none — `new ToolStripMenuItem { Checked = true }`. **Gating for the branch gate.** |
| IV-14 † | `MenuItemCheckedChanged_WhenUnchecked_ClearsImage` | **179-false**, 185 | — | **S2** | none |
| IV-15 † | `MenuItemCheckedChangedHandler_WhenSenderIsMenuItem_DelegatesToTypedOverload` | 173, 174 | U | **S2** | none |
| IV-16 † | `MenuItemCheckedChangedHandler_WhenSenderIsNotMenuItem_ThrowsInvalidCast` (negative) | 173 | U | **S2** | none — documents LD-3 |
| IV-17 | `WebViewParentChanged_WhenReparented_InvokesWiredHandler` | 167, 168, 169 | V | designer wiring `ItemViewer.Designer.cs:256` | none — re-parent `L0v2h2_WebView2` into a local `Panel` |
| IV-18 | `RemoveControlsColsRightOf_DelegatesToTrimmerWithWebViewSpanTarget` | the S1 delegation line | V | S1 | none — assert the observable TLP mutation |
| IV-19 † | `MoveOptionsMenuClick_DoesNothing` | 205 | U | **S2** (or delete under Option A) | none |

† disappears under Option A (delete the dead members).

**Projected result (Option B): ~152 / 156 lines ≈ 97% line; 2 / 2 outcomes = 100% branch.** Both gates
clear with margin. **Minimum set to pass**: IV-1, IV-3, IV-4, IV-5, IV-6, IV-7, IV-8, IV-9, IV-10, IV-11
give ~125/156 = 80.1% line — exactly at the gate, so do not plan the minimum set; plan the full list.
**IV-13 is load-bearing for the branch gate under Option B**: without it the file sits at 1/2 = 50% branch
and fails the 75% gate regardless of line coverage.

### 7.4 Tests for the extracted `ControlColumnTrimmer` (new file, >= 90% target)

The `ItemViewerExpanded.cs` sibling artifact already enumerates these as its T10-T17
(`research.itemviewerexpanded-cs.2026-08-07T21-40.md` §6.3): eight cases covering the two outcomes of each
of the five extracted branch points, using `Panel`/`Label`/`TableLayoutPanel` fixtures with explicit
`Location`/`Size` so no layout pass is required. **Do not re-enumerate them here — plan that list once.**
Precedent for the fixture style: `UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs`, which already
covers the `RemoveSpecificColumn` extension this code calls at `ItemViewer.cs:88`.

One correction to carry into that plan: the sibling proposed `[STATestClass]` for those tests. Per §4.4,
the epic's dedicated-`*.StaTests.cs` requirement only binds tests that *need* STA; plain
`TableLayoutPanel`/`Panel`/`Label` manipulation does not (proven by `TableLayoutHelper_Tests.cs`, which is
`[STATestClass]` today but by the same evidence need not be). Either choice is compliant; a plain
`[TestClass]` keeps the STA surface at zero and is preferred.

### 7.5 Prerequisites, not test cases

- **T0 — remove `[ExcludeFromCodeCoverage]` from `ItemViewer.cs:20`** (and the now-unused
  `using System.Diagnostics.CodeAnalysis;` at `:5`, if nothing else in the file uses it — verify first).
  **This must land in the same commit as at least IV-1 and the designer's D1/D2**, so the ~6,000-line
  designer never appears in a measured state that depends solely on sibling-owned harnesses (companion
  artifact §2.5).
- **T0b — run F1's harness (#432) and record the actual per-file line and branch rate for all seven
  newly-visible files.** Prune the case list against measured data. Cite **#441** whenever quoting a
  `<class>` `line-rate` attribute; prefer harness-recomputed figures from deduplicated `<line>` nodes.

---

## 8. Q8 — Open-issue scan

Method: GitHub public issue-search UI via WebFetch
(`https://github.com/drmoisan/TaskMaster/issues?q=is%3Aissue+is%3Aopen+<term>`); the Bash tool was
unavailable, so `gh issue list --state open --search ...` could not be run. Terms searched: `ItemViewer`,
`viewer`, `WebView`, `coverage`, `ExcludeFromCodeCoverage`, `designer`. GitHub's UI truncates results, so
absence from the tables below is not proof of absence.

| Issue | Title | Bearing on `ItemViewer.cs` |
| --- | --- | --- |
| **#457** | Bug: excludefromcodecoverage-does-not-suppress-nested-lambdas | **Direct.** *Method-level* exclusion leaks hoisted lambdas. `ItemViewer.cs:142-147`, `:151`, `:153`, `:161-162` are lambdas that would be hoisted into closure types. Consequence: after T0, **do not re-exempt any individual member of `ItemViewer`** — the lambdas would stay in the denominator anyway. Also scopes Q1: type-level exclusion *is* complete (companion artifact §1.4). |
| **#441** | Cobertura post-processing double-counts `<line>` nodes | **Direct and load-bearing.** Independently verified at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122` (`.//class` then `.//lines/line` — the descendant axis matches both the class-level and per-method line lists). Every acceptance figure must come from F1's recomputed per-file numbers. |
| **#432** | Feature: quickfiler-coverage-ledger | F1. Owns the ledger row, the N/A-not-0% rule (needed by Option A, §7.1), and the harness that supersedes every estimate here. |
| **#230** | Build a WinForms message-pump test seam (`Application.Run()` background thread) to unblock 9 `QfcItemController` orchestration members | **Not needed for this file** — no member requires a pump (§4.4). If it lands, prefer its helper over any ad-hoc scheme; it does not change this plan. |
| **#438** | Bug: quickfiler-search-keystroke-focus-steal | Touches search-box focus behaviour. The search surface lives in `ItemViewer.FolderSearch.cs` (a sibling F14 file), not in `ItemViewer.cs`; `TxtboxSearch` here is a bare property pair (`:389-393`). No conflict with this file's plan, but the F14 planner should reconcile it against `ItemViewer.FolderSearch.cs`'s case list. |
| **#400** | quickfiler-folder-selector-dropdown-400 (active feature folder) | Highest textual-conflict risk for the family: its remediation plan authorises edits to `ItemViewer.Breadcrumb.cs` and it owns eight of the test harnesses that construct `ItemViewer`. **Read the merged state of #400 before planning** (the `ItemViewer.Breadcrumb.cs` sibling artifact §9 flags this; `epic.md:638` records the overlap only against F13). |
| #455, #458, #462, #463, #440, #467, #466, #460 | breadcrumb drop-down / WebView2 host retention / navigation / EfcViewer | F13/F12/F9 territory. **#458 and #462 touch harnesses that supply this file's incidental execution today** — see CN-2. No edit proposed to any of them. |
| #427 | quickfiler-post-show-duplicate-scoring | Not returned by any search performed; no relationship to `ItemViewer.cs` was found. Treat as not-applicable pending confirmation. |
| #468-#474 | qfc-collection-controller defect cluster | F11 territory. No bearing. |

---

## 9. Cross-child notes and sibling boundaries

**F14 requires ZERO changes in any F10, F12, or F13 file.** Verified: `ItemViewer.cs` references no
breadcrumb type, no `QfcItemController` type, and no WebView2 type other than the property pair at
`:309-313` whose declared type must not change (§4.3). `IItemViewer.cs:131`'s signature is preserved by S1,
so F10's `Mock<IItemViewer>` fixtures and F9's `EfcItemController.cs:247` call site are untouched.

- **CN-1 (F14 → F1, issue #432) — ledger rules for generated designers.** See the companion designer
  artifact §3.4. Additionally: F1's harness must report a file with **zero branch outcomes** as N/A rather
  than 0%, because Option A (§7.1) produces exactly that shape for `ItemViewer.cs`. This is the branch-side
  analogue of the existing `<line>`-count rule (epic § Directives, requirement 2).
- **CN-2 (F14 internal, and a freeze request to F10/F12/F13).** `ItemViewer.cs` and
  `ItemViewer.Designer.cs` currently execute only through sibling-owned harnesses
  (`Viewers/Breadcrumb*Tests.cs`, `Controllers/QfcItemController.*Tests.cs`). F14 must own its own
  construction fixture (IV-1 … IV-4) rather than inherit them. F13/F12/F10 should be told not to replace a
  live `new QuickFiler.ItemViewer()` with a mock in those harnesses while F14 is in flight; if they do,
  F14's own fixture absorbs the loss, but the transitional measurement would be misleading.
- **CN-3 (F14 internal — coordinate the S1 extraction with the `ItemViewerExpanded.cs` researcher).**
  `ItemViewer.cs:77-95`, `:97-107`, `:137-164` and `ItemViewerExpanded.cs:69-87`, `:89-99`, `:129-156` are
  verbatim twins. One `ControlColumnTrimmer.cs`, one csproj entry, one ledger row, one test file. Planning
  it twice would produce two modules or a merge conflict inside the same child.
- **CN-4 (F15-owned, no change requested).** `QuickFiler/Viewers/ToolStripMenuItemCb.cs:32-58` holds the
  `Checked`/`CheckedChanged` shadowing defect the `ItemViewerExpanded.cs` sibling documented. **F14 must
  not edit it.** `ItemViewer` is unaffected because it never wires the handler (§2). If F15 fixes the
  shadow, cases IV-13/IV-14 are unaffected — they invoke the seam with a plain base `ToolStripMenuItem`.

---

## 10. Latent defects — promotion candidates

Each is out of scope to fix under the epic's no-behaviour-change NFR and should be promoted through the MCP
lifecycle rather than left as prose (epic § Latent Defect Promotion).

**LD-1 — `ItemViewer` carries three dead private members that its twin `ItemViewerExpanded` wires and
calls; the divergence is undocumented and one of the two behaviours is wrong.**
`ItemViewer.cs:171-175`, `:177-187`, and `:205` have no caller and no designer wiring anywhere in the
solution (verified by repository-wide search over `*.cs`; `ItemViewer.Designer.cs` contains exactly one
`+=` handler wiring, at `:256`, and it is not one of these). The same three members in
`ItemViewerExpanded.cs:163-179` **are** wired four times
(`ItemViewerExpanded.Designer.cs:171,180,189,198`) and called four times from its constructor
(`ItemViewerExpanded.cs:24-27`). Combined with the `ToolStripMenuItemCb.Checked` shadowing defect, the
wired path is the *defective* one: it clears the check image the setter just applied. So the two twins have
silently divergent menu behaviour and the divergence is accidental. Disposition: delete the three dead
members from `ItemViewer.cs` (behaviour-neutral) and fix or document the `ItemViewerExpanded` path.

**LD-2 — Production `Console.WriteLine` in a wired WinForms event handler.**
`ItemViewer.cs:168` is `Console.WriteLine("Parent Changed");`, the entire body of
`L0v2h2_WebView2_ParentChanged`, wired at `ItemViewer.Designer.cs:256`. Identical at
`ItemViewerExpanded.cs:160`. Violates the General Code Change Policy § 3 ("use the project's logging
pattern instead of ad-hoc print/console output"). The handler is otherwise a no-op, so an alternative
disposition is deleting both the handler and its designer wiring — a behaviour change, hence promotion
rather than an in-scope fix. *(The `ItemViewerExpanded.cs` sibling raised this as its LD-2 for the twin;
promote once, citing both files.)*

**LD-3 — Unguarded downcast in an event handler.** `ItemViewer.cs:173` is
`var menuItem = (ToolStripMenuItem)sender;` with no `is`/`as` guard; any non-`ToolStripMenuItem` sender
raises `InvalidCastException` on the UI thread with no context. Same at `ItemViewerExpanded.cs:165`. Low
severity in `ItemViewer` (the member is dead, LD-1) but it is a fail-fast-without-context path that would
become live the moment anyone wires it. *(Sibling LD-3; promote once.)*

**LD-4 — A live `System.Windows.Forms.Form` is compiled into the `QuickFiler.Test` assembly.**
`QuickFiler.Test/Form1.cs:5` and `Form1.Designer.cs:3` declare `public partial class Form1 : System.Windows.Forms.Form`,
whose `InitializeComponent` constructs three `QuickFiler.ItemViewer` instances
(`Form1.Designer.cs:32-34`). No test instantiates it (verified: the only `Form1` references in the test
project are its own two files), so no policy violation occurs today — but a live `Form` in a unit-test
assembly is one `new Form1()` away from breaching `.claude/rules/general-unit-test.md` and the epic's
"never construct live forms" rule, and it is dead weight. Disposition: delete both files, or move them to a
manual harness project. Out of F14's production file set, hence promotion.

**Reference-only, already tracked — do not re-promote:**

- **#441** — Cobertura `<line>` double-count. Independently verified at
  `Invoke-MSTestWithCoverage.Helpers.ps1:121-122`.
- **#457** — method-level exclusion leaks hoisted lambdas. Constrains post-T0 options (§8).

---

## 11. Premises confirmed, extended, and corrected

**Confirmed as supplied:**

- `ItemViewer.cs:21` is `public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal`;
  `:20` is the family's only real `[ExcludeFromCodeCoverage]`; the four comment-only mentions are not
  exemptions.
- No `ItemViewer.*` partial appears in the committed Cobertura report; `ItemViewerExpanded.cs` and
  `ItemViewerExpanded.Designer.cs` do.
- `coverage.config` (`:10-24`) and `TaskMaster.runsettings` (`:9-29`) contain `<ModulePaths>` excludes only.
- All ten F14 files are compiled (`QuickFiler.csproj:392`, `:412-443`); the partials and designer carry
  `<DependentUpon>ItemViewer.cs</DependentUpon>`.
- `QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`; `UtilitiesCS`
  does not — but this file needs no `UtilitiesCS` internal (its two dependencies,
  `Control.ForAllControls` and `Initializer.GetOrLoad` at `UtilitiesCS/HelperClasses/Initializer.cs:103`,
  are both public).
- `issue.md:50-52`'s CS0579 constraint: **confirmed** via the fetched `AllowMultiple = false` declaration.
- The STA last-resort clause is *available* (the type is a `UserControl`).

**Corrected / extended:**

1. **"Removing the attribute could reduce repository-wide coverage" — the risk runs the other way.**
   The dominant term is `ItemViewer.Designer.cs`, ~6,013 coverable lines that go to ~99.95% on any single
   construction. Removing the attribute is +0.57 pp (primary model) to −0.08 pp (conservative model);
   *exempting the designer* is −0.16 pp. Full arithmetic in the companion artifact §2.4.
2. **`FakeTimeProvider` IS available in `QuickFiler.Test` on net481** (`Microsoft.Bcl.TimeProvider` +
   `Microsoft.Extensions.TimeProvider.Testing`, in active use in that project). The brief's statement to
   the contrary is disproved — see the `ItemViewer.Breadcrumb.cs` sibling artifact §4(b). It is not needed
   for `ItemViewer.cs`, which reads no clock.
3. **STA is not required, and no `*.StaTests.cs` file should be created for this file.** Ten existing plain
   `[TestMethod]`s already construct a live headless `ItemViewer` with WebView2, MenuStrip,
   FastObjectListView and SVG-backed controls (§4.4).
4. **`issue.md:53-54`'s "injected clock and fake timers" constraint does not apply to `ItemViewer.cs`.**
   No clock, no timer, no async member in this file.
5. **The file's last branch point is dead code** (§2). This is the single fact that most changes the shape
   of the test plan, and it is not recorded in `issue.md` or `epic.md`.
6. **Seam extraction shrinks this file rather than growing it**, so the `epic.md:426-433` framing of F14 as
   a 500-line-risk child does not apply to `ItemViewer.cs` (432 → ~390).

---

## 12. Verified vs inferred

**Verified (direct file read, report inspection, or fetched documentation):**

- Every member, line range, and branch predicate in §3; the `using` block; the absence of any Outlook
  Interop, clock, timer, or async member.
- The three dead members and the single designer handler wiring (§2), by exhaustive repository-wide search.
- All twelve existing test files and their exact usage modes (§6), including the ten construction sites.
- `InternalsVisibleTo("QuickFiler.Test")`; `Initializer.GetOrLoad` is `public static`; `ForAllControls` is
  a public extension.
- The csproj compile entries and the absence of globbing.
- #441's root cause in the harness script; #457's content, from its issue body.
- `ExcludeFromCodeCoverageAttribute`'s `AllowMultiple = false` and the "all the members of that class"
  remark; the Q1 positive and negative controls (companion artifact §1).

**Inferred / estimated:**

- ~205 coverable lines for this file and ~156 after S1 — a model-based estimate (§1.3), not a measurement;
  the file is not instrumented today so no measurement is possible before T0.
- The per-case line attributions in §7.3, which follow from the estimate.
- CS0579 as the specific compiler error code for the duplicated attribute (follows from
  `AllowMultiple = false`; no build was run this session).
- That the currently-executed fraction of this file is "meaningful but well under 80%" — supported by the
  ten construction sites and by the twin `ItemViewerExpanded.cs` measuring 37.7% under a single incidental
  construction path, but not measured. **T0b settles it.**
