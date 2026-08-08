# Research — `QuickFiler/Viewers/ItemViewer.Commands.cs`

- Feature: F14 `quickfiler-itemviewer-coverage` (issue #456), child of epic #136 `quickfiler-per-file-coverage`
- Timestamp: 2026-08-07T23-05
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5e4b635834feedd7`
- Target file: `QuickFiler/Viewers/ItemViewer.Commands.cs` (109 physical lines)
- Compile entry: `QuickFiler/QuickFiler.csproj:419-422` (`<DependentUpon>ItemViewer.cs</DependentUpon>`, `<SubType>UserControl</SubType>`)

Claims are marked **[V]** (verified by direct file read, grep, or fetched issue text) or **[I]** (inferred
from verified facts). No tool capable of executing code or `gh` was available this session; the Bash tool
is not in this agent's toolset. GitHub issue text was obtained via WebFetch against the public issue
pages and is marked **[V-web]**.

---

## 0. Premise verification

| # | Supplied premise | Verdict | Evidence |
|---|---|---|---|
| P1 | `ItemViewer` is `public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal` | **CONFIRMED [V]** | `QuickFiler/Viewers/ItemViewer.cs:21` |
| P2 | This file carries **no** real `[ExcludeFromCodeCoverage]`; `ItemViewer.Commands.cs:10` only mentions it in a comment | **CONFIRMED [V]** | Full read of all 109 lines. The only occurrence of the token is inside the `//` comment block at `:7-10`: *"The whole ItemViewer type is [ExcludeFromCodeCoverage] via its primary partial in ItemViewer.cs."* The sole real attribute is `ItemViewer.cs:20`. The epic itself records the correction at `epic.md:126-128`. |
| P3 | No `ItemViewer.*` partial appears in the committed Cobertura report | **CONFIRMED [V]** (via sibling artifact, re-checked) | `research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:20` documents the grep against `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`: only `Helper Classes\ItemViewerQueue.cs`, `Viewers\ItemViewerExpanded.Designer.cs`, `Viewers\ItemViewerExpanded.cs` match; no `Viewers\ItemViewer*.cs` partial. |
| P4 | "Assume effectively 0% measured coverage. Do not assume existing coverage." | **CONFIRMED for this file [V]** — unlike `ItemViewer.Breadcrumb.cs`, this file has **no** existing-test exposure. Repo-wide grep for its 16 member names returns production call sites plus `Mock<IItemViewer>` setups only (`QfcItemController.SeamFactoryTests.cs:250-281`, `SeamDispatcherTests.cs:113-151`, `ViewerSetupTests.cs:249-274`, `EventHandlersTests.cs:151-195`). **A `Mock<IItemViewer>` never executes a line of this file.** The two tests that construct a real `ItemViewer` (`QfcItemController.EventWiringTests.cs:236`, `ViewerSetupTests.cs:386`) invoke `ResolveControlGroups` and `WireControlTreeEvents` — **not** `WireIntentEvents`, which is the only production caller of these events (`QfcItemController.EventWiring.cs:66-94`). Baseline is genuinely ~0%. |
| P5 | `InternalsVisibleTo("QuickFiler.Test")` is granted by QuickFiler | **CONFIRMED [V]** | `QuickFiler/Properties/AssemblyInfo.cs:5`. Not needed for this file: every member here is already `public`. |
| P6 | MSTest 4.3.3 supplies `[STATestClass]`; no new package needed | **NOT EXERCISED** — §7 concludes no STA test is required for this file. |
| P7 | Issue #441 corrupts `<class>` `line-rate` | **CONFIRMED [V]** | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122` uses `.//class` then `.//lines/line`, a descendant axis that also matches `<methods><method><lines><line>`. Any `line-rate` quoted for this file must be annotated "#441 — unreliable". |

---

## 1. What the file is

A partial of `ItemViewer` holding **16 forwarding members**: 10 `event` add/remove pairs and 6
properties. Every member is a one-expression forwarder onto a Designer-backed control reached through
the public wrapper properties in `ItemViewer.cs`'s `#region Field to Property for Interface`
(`ItemViewer.cs:207-430`). The file's own header comment (`:7-9`) states the design intent accurately:
*"Forwarding implementations for the narrowed IItemViewer button command events and menu intent members
(Seam B, Cluster 2b)."*

Backing control types **[V]** (`ItemViewer.cs:334-423`):

| Wrapper property | Declared type | Declaring line |
|---|---|---|
| `BtnDelItem`, `BtnPopOut`, `BtnFlagTask`, `BtnForward`, `BtnReply`, `BtnReplyAll` | `SVGControl.ButtonSVG` (`: System.Windows.Forms.Button`, `SVGControl/ButtonSVG.cs:13`) | `:334`, `:344`, `:354`, `:359`, `:379`, `:384` |
| `ConversationMenuItem`, `SaveAttachmentsMenuItem`, `SaveEmailMenuItem`, `SavePicturesMenuItem` | `QuickFiler.Viewers.ToolStripMenuItemCb` (`: ToolStripMenuItem`, `ToolStripMenuItemCb.cs:11`) | `:404`, `:409`, `:414`, `:419` |

All eight wrapper properties used by this file have **public setters**, which is what makes the
`CreateUninitialized<ItemViewer>()` fixture in §5 possible.

### 1.1 The `ToolStripMenuItemCb` binding — decisive for testability

`ToolStripMenuItemCb` re-declares `Checked` and `CheckedChanged` with `new`
(`ToolStripMenuItemCb.cs:32-51`, `:58`). Because `ItemViewer.cs:404/409/414/419` type the wrapper
properties as **`Viewers.ToolStripMenuItemCb`** (the derived type), the four `*Checked` properties and
four `*Changed` events in this file bind at compile time to the **shadowing** members, not the base
ones. Consequences, all verified from `ToolStripMenuItemCb.cs`:

- `ConversationModeChecked` get/set (`:57-58`) round-trips the private `_checked` field (`:34`, `:37`)
  — **a real round-trip, testable with no host**.
- The `Checked` setter raises the shadowing `CheckedChanged` (`:47`) on every assignment, **including
  assignments to the same value** (there is no equality short-circuit). So `ConversationModeChanged`
  (`:51-52`) can be exercised purely by setting `ConversationModeChecked`.
- The setter also writes `base.Image` (`:40`, `:44`) and calls `base.Invalidate()` (`:48`). Neither
  requires a handle or a parent.

This corrects a generalisation carried in this agent's memory (`project_qfc_itemviewer_coverage_456.md`
item 5), which stated that "both `ItemViewer` and `ItemViewerExpanded`" read the base `Checked` and
therefore clear the check image. That is true of **`ItemViewerExpanded`** — its Designer wires
`CheckedChanged += MenuItem_CheckedChanged` on all four items
(`ItemViewerExpanded.Designer.cs:171,180,189,198`) and that handler downcasts to the base
`ToolStripMenuItem` (`ItemViewerExpanded.cs:169-176`). It is **not** true of `ItemViewer`: a grep for
`CheckedChanged` and `.Click +=` across all 6,224 lines of `ItemViewer.Designer.cs` returns **zero
matches**, and the file's only event wiring at all is
`ItemViewer.Designer.cs:256` (`_l0v2h2_WebView2.ParentChanged`). See LD-2 for the defect this creates
instead.

---

## 2. Q1 — Member-by-member classification (exhaustive)

Classification vocabulary per the delegation brief. **Zero members are COM-bound**: a grep of this file
for `Microsoft.Office` returns nothing, and its three `using` directives are `System`, `System.Drawing`,
`System.Windows.Forms` (`:1-3`).

| # | Member | Lines | Coverable lines | Branches | Class | Rationale |
|---|---|---|---|---|---|---|
| 1 | `DeleteItemClicked` add/remove | 13-17 | 15, 16 | 0 | **thin wiring** | `BtnDelItem.Click +/- value` |
| 2 | `FlagTaskClicked` | 19-23 | 21, 22 | 0 | thin wiring | `BtnFlagTask.Click` |
| 3 | `PopOutClicked` | 25-29 | 27, 28 | 0 | thin wiring | `BtnPopOut.Click` |
| 4 | `ReplyClicked` | 31-35 | 33, 34 | 0 | thin wiring | `BtnReply.Click` |
| 5 | `ReplyAllClicked` | 37-41 | 39, 40 | 0 | thin wiring | `BtnReplyAll.Click` |
| 6 | `ForwardClicked` | 43-47 | 45, 46 | 0 | thin wiring | `BtnForward.Click` |
| 7 | `ConversationModeChanged` | 49-53 | 51, 52 | 0 | thin wiring | `ToolStripMenuItemCb.CheckedChanged` (shadowing) |
| 8 | `ConversationModeChecked` get/set | 55-59 | 57, 58 | 0 | thin wiring | `ToolStripMenuItemCb.Checked` (shadowing) |
| 9 | `EmailCopyChanged` | 61-65 | 63, 64 | 0 | thin wiring | `SaveEmailMenuItem.CheckedChanged` |
| 10 | `EmailCopyChecked` | 67-71 | 69, 70 | 0 | thin wiring | `SaveEmailMenuItem.Checked` |
| 11 | `AttachmentsChanged` | 73-77 | 75, 76 | 0 | thin wiring | `SaveAttachmentsMenuItem.CheckedChanged` |
| 12 | `AttachmentsChecked` | 79-83 | 81, 82 | 0 | thin wiring | `SaveAttachmentsMenuItem.Checked` |
| 13 | `PicturesChanged` | 85-89 | 87, 88 | 0 | thin wiring | `SavePicturesMenuItem.CheckedChanged` — **no production subscriber, see LD-1** |
| 14 | `PicturesChecked` | 91-95 | 93, 94 | 0 | thin wiring | `SavePicturesMenuItem.Checked` |
| 15 | `FlagTaskDialogResult` | 97-101 | 99, 100 | 0 | thin wiring | `Button.DialogResult` (an `IButtonControl` property) |
| 16 | `FlagTaskBackColor` | 103-107 | 105, 106 | 0 | thin wiring | `Control.BackColor` |

**Totals: 32 coverable lines, 0 branch points, 0 pure/host-neutral members, 16 thin-wiring members,
0 COM-bound members.**

### 2.1 The critical structural finding: the branch gate is vacuous

The file contains **no `if`, no `?.`, no `??`, no `&&`/`||`, no ternary, no `switch`, no loop, and no
lambda**. Verified by full read of all 109 lines. Coverlet therefore emits no `<condition>` entries for
this file's members and the epic's `>= 75%` branch gate is satisfied vacuously (reported as `100%` or
`N/A` depending on how F1's harness renders a zero-denominator branch rate).

**Planner directive:** the *line* gate is the only real gate on this file. F1's harness must not report
a zero-branch file as `0%` branch — that is the same failure mode as the `interface-only` false alarm
described at `epic.md:533-536`, applied to the branch denominator instead of the line denominator. See
the harness note in §9.

### 2.2 The "thin wiring" classification is genuine, not a concession

Every member is a single delegating expression with no decision, no transformation, and no state of its
own. There is **no pure logic to extract**: extracting `add => BtnDelItem.Click += value;` into a
host-neutral class would produce a class whose sole content is the same WinForms event access. The
correct response under `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy ("leave only the
thinnest possible wiring in the host-bound entry point") is that this file **already is** that thinnest
wiring — and, critically, it is *reachable* wiring, because the WinForms controls it touches are
constructible in-process without a handle, a parent, a message pump, or an STA apartment (§4).

---

## 3. Q2 — Seam recommendation

**Recommendation: introduce NO seam. Add no production code to this file and no new production file.**

Justification against the epic's hierarchy (`epic.md:227-232`, interface seam > injectable delegate >
adapter): a seam exists to make an unreachable line reachable. Every one of this file's 32 lines is
already reachable from a plain `[TestClass]` `[TestMethod]` with no seam, because:

1. **The backing controls are injectable today.** `ItemViewer.cs:334-423` exposes all eight required
   controls as `public` get/set properties. A test assigns `new ButtonSVG()` / `new ToolStripMenuItemCb()`
   into an `ItemViewer` created by `FormatterServices.GetUninitializedObject`. **In-repo precedent, same
   pattern, same type:** `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:247-265` builds an
   `ItemViewer` exactly this way (`CreateUninitialized<ItemViewer>()` at `:249`, then
   `viewer.LblItemNumber = new Label();` etc.), with the helper at `:331-335`.
2. **No control needs a window handle.** `Control.Click` / `ToolStripItem` event subscription writes to
   an `EventHandlerList`; `Button.DialogResult` and `Control.BackColor` are plain property writes;
   `ToolStripMenuItemCb.Checked` writes a private field, `base.Image`, and calls `Invalidate()` (a no-op
   without a handle).
3. **No apartment requirement.** Six existing `QuickFiler.Test` classes construct a full
   `new QuickFiler.ItemViewer()` — which runs `InitializeComponent()` and therefore constructs all six
   `ButtonSVG`s and all four `ToolStripMenuItemCb`s — inside a **plain `[TestClass]`**
   (`QfcItemController.EventWiringTests.cs:236`, `QfcItemController.ViewerSetupTests.cs:386`,
   `Viewers/BreadcrumbDropDownIntegrationTests.cs:338`, `Viewers/BreadcrumbCoordinatorLifecycleTests.cs:477`,
   `Viewers/BreadcrumbPendingOpenCloseTests.cs:363`, `Viewers/BreadcrumbSelectorOpenRetryTests.cs:255`,
   `Viewers/BreadcrumbSubfolderActivationTests.cs:305`, `Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs:413`,
   `Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:373`). There is **no `[STATestClass]` anywhere in
   `QuickFiler.Test`** — the repo's only STA files are `Tags.Test/CheckBoxControllerWiring.StaTests.cs`
   and `TaskVisualization.Test/TaskControllerAccelerator.StaTests.cs`.

Adding a seam here would violate two epic constraints at once: it would create new production code
subject to the `>= 90%` new-file rule (`epic.md:583-585`) for zero coverage benefit, and it would change
the shape of a `public` `IItemViewer` implementation, which is an API change the epic's
no-behavior-change NFR forbids and which would ripple into F10's `QfcItemController.*` mocks.

### 3.1 Rejected alternatives (brief)

- **Extract an `ItemViewerCommandWiring` host-neutral class** taking `Func<Control>` accessors. Rejected:
  it relocates the same WinForms access without removing it, adds a file to the denominator, and adds an
  indirection the General Code Change Policy § Design Principle 1 (simplicity first) disfavours.
- **Retype the wrapper properties to interfaces (`IButtonControl`, an `ICheckableMenuItem`).** Rejected:
  `ItemViewer.cs:334-423` is F14-owned but the concrete types are consumed by name elsewhere
  (`QfcItemController.EventWiring.cs:59` iterates `_itemViewer.MenuItems` as `ToolStripMenuItem`;
  `QfcThemeHelperTests.cs:253-258` assigns concrete controls). The breadcrumb sibling artifact records
  the same conclusion for `L0vhBreadcrumb_WebView2` with a pinned contract test
  (`research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:256-266`).
- **STA + real window handles + synthesised `WM_LBUTTONUP`.** Rejected: unnecessary (see §4) and barred
  as anything but a last resort by `epic.md:234-241`.

---

## 4. Q4/Q5 — Command inventory: trigger, effect, failure mode

Dispatch verified end-to-end. **These files have no dependency on the F3-owned keyboard-action types.**
A grep of the entire `QuickFiler/Viewers/` folder for `KbdActions`, `KaChar`, `KaKey`, `KaStringAsync`,
and `IMailItemActions` returns **no matches [V]**. The viewer forwards raw WinForms events; the
controller binds them in `QfcItemController.EventWiring.cs:66-94` (F10-owned).

| Command member | Trigger (production) | Effect | Failure mode |
|---|---|---|---|
| `DeleteItemClicked` | user clicks `BtnDelItem` | invokes `QfcItemController.BtnDelItem_Click` (`EventWiring.cs:71`) | `NullReferenceException` at `:15`/`:16` if `BtnDelItem` is unassigned; no null guard anywhere in the file |
| `FlagTaskClicked` | click `BtnFlagTask` | `BtnFlagTask_Click` (`EventWiring.cs:69`) | as above |
| `PopOutClicked` | click `BtnPopOut` | `BtnPopOut_Click` (`EventWiring.cs:70`) | as above |
| `ReplyClicked` / `ReplyAllClicked` / `ForwardClicked` | click the respective `ButtonSVG` | `BtnReply_Click` / `BtnReplyAll_Click` / `BtnForward_Click` (`EventWiring.cs:72-74`) | as above |
| `ConversationModeChanged` | `ConversationMenuItem.Checked` assignment (menu has `CheckOnClick = true`, `ItemViewer.Designer.cs:6121`, so a user click assigns it) | `CbxConversation_CheckedChanged` (`EventWiring.cs:68`) → caches `_optionConversationChecked` (`EventHandlers.cs:35`) | raised on every assignment, **including no-change assignments** (`ToolStripMenuItemCb.cs:35-49` has no equality short-circuit) |
| `ConversationModeChecked` | read/written by `QfcItemController.Navigation.cs:117,133-141` and `ViewerSetup.cs:380` | toggles conversation-move mode | assignment side-effects: writes `base.Image` and raises `CheckedChanged` |
| `EmailCopyChanged` / `EmailCopyChecked` | `SaveEmailMenuItem` (`CheckOnClick`, Designer `:6138`) | `CbxEmailCopy_CheckedChanged` (`EventWiring.cs:92`) → `_optionEmailCopy` (`EventHandlers.cs:206`); also toggled by `FocusAndTheme.cs:271` | as above |
| `AttachmentsChanged` / `AttachmentsChecked` | `SaveAttachmentsMenuItem` (`CheckOnClick`, Designer `:6130`) | `CbxAttachments_CheckedChanged` (`EventWiring.cs:93`) → `_optionAttachments` (`EventHandlers.cs:216`) | as above |
| `PicturesChanged` | `SavePicturesMenuItem` (`CheckOnClick`, Designer `:6146`) | **nothing — no subscriber exists in production. LD-1.** | user toggle is silently discarded |
| `PicturesChecked` | written once by `ViewerSetup.cs:389` from `_globals.QfSettings.SavePictures` | seeds the menu check state | read side never re-reads it; `MailActions.cs:102` consumes the stale `_optionsPictures` field |
| `FlagTaskDialogResult` | written by `MailActions.cs:176,194` and `ViewerSetup.cs:371,375`; read back by `MailActions.cs:177,195` | used as a **cross-call scratch variable**, not as a WinForms dialog result | none in the viewer; see LD-3 |
| `FlagTaskBackColor` | written by `MailActions.cs:179,197` | tints the flag-task button after a successful flag | `Control.BackColor` set to `Color.Transparent` on a control lacking `SupportsTransparentBackColor` throws `ArgumentException` — **not verified for `ButtonSVG`; tests must not assert this** |

### 4.1 Bearing of issues #445 and #444

- **#445 `quickfiler-keyboard-action-contract-defects` [V-web]** — three contract defects in the QuickFiler
  keyboard-action types, explicitly not fixed by #430. **No bearing on this file.** Evidenced by the
  zero-match grep for `KbdActions|KaChar|KaKey|KaStringAsync|IMailItemActions` across `QuickFiler/Viewers/`.
  The only keyboard-action coupling anywhere near F14 is `QfcItemController.EventWiring.cs:81-83`
  (`_itemViewer.FolderKeyDown += _kbdHandler.CboFolders_KeyDownAsync`), which is F10-owned and reaches
  `ItemViewer.FolderSearch.cs`, not this file.
- **#444 `kbdactions-enumerable-ctor-bypasses-duplicate-guard` [V-web]** — the `IEnumerable<UClass>`
  constructor of `KbdActions<TKey,UClass,VDelegate>` bypasses the `Add` duplicate guard. **No bearing on
  this file**, same evidence.

**Reporting conclusion for Q5:** dispatch does **not** go through any keyboard/action abstraction from
the viewer side. F14 requires nothing from F3 and must edit none of `KbdActions.cs`, `KaChar.cs`,
`KaKey.cs`, `KaStringAsync.cs`, `Interfaces/IMailItemActions.cs`, or `Interfaces/MailItemActionsAdapter.cs`.

---

## 5. Q6 — Test plan

### 5.1 Fixture

One fixture, no variants:

```
U = FormatterServices.GetUninitializedObject(typeof(ItemViewer)) cast to ItemViewer,
    then assign only the controls the case needs via the public wrapper setters.
```

Precedent: `QfcThemeHelperTests.cs:249-265` + helper `:331-335`. Advantages over a full
`new QuickFiler.ItemViewer()`: no `InitializeComponent()`, no `SynchronizationContext` requirement
(`ItemViewer.cs:26-27` calls `TaskScheduler.FromCurrentSynchronizationContext()`, which throws without an
ambient context), no 6,224-line Designer tree, and no `Dispose` obligation.

Event-raise helper: reflect the protected `Control.OnClick(EventArgs)` /
`Control.OnDoubleClick(EventArgs)` methods, exactly as `QfcThemeHelperTests.cs:277-285`
(`InvokeControlEvent`) already does for `OnMouseEnter`/`OnMouseLeave`. `ToolStripMenuItemCb.CheckedChanged`
needs no reflection at all — assigning `Checked` raises it (`ToolStripMenuItemCb.cs:47`).

MSTest `[TestClass]`/`[TestMethod]`, Moq where a collaborator is needed (none is, here),
FluentAssertions, Arrange–Act–Assert, no temp files, no external services, no live Form, no popup, no
`Thread.Sleep`/`Task.Delay`/wall-clock wait.

**Proposed home:** `QuickFiler.Test/Viewers/ItemViewerCommandsForwardingTests.cs` (new). Requires a
`<Compile Include="Viewers\ItemViewerCommandsForwardingTests.cs" />` entry in
`QuickFiler.Test/QuickFiler.Test.csproj` (CRLF preserved, minimal adjacent hunk). Projected size at 16
cases with a shared fixture region: ~330 lines — under the 500-line limit; if it approaches the limit,
split at case C11 into `...Tests.Part2.cs`.

### 5.2 Case inventory

Per issue #136 each case is one atomic task. Gate arithmetic: 32 coverable lines, `>= 80%` line means
`>= 26` lines; the 16 cases below cover **32/32 = 100%**. Branch gate is vacuous (§2.1).

| # | Test name | Production lines covered | Seam | Mocks | Assertion shape |
|---|---|---|---|---|---|
| C1 | `DeleteItemClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick` | 15, 16 | none — `BtnDelItem` setter (`ItemViewer.cs:337`) | none | subscribe, reflect `OnClick`, expect 1 invocation; unsubscribe, reflect `OnClick` again, expect still 1 |
| C2 | `FlagTaskClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick` | 21, 22 | `BtnFlagTask` setter | none | as C1 |
| C3 | `PopOutClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick` | 27, 28 | `BtnPopOut` setter | none | as C1 |
| C4 | `ReplyClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick` | 33, 34 | `BtnReply` setter | none | as C1 |
| C5 | `ReplyAllClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick` | 39, 40 | `BtnReplyAll` setter | none | as C1 |
| C6 | `ForwardClicked_AddThenRemove_SubscribesAndUnsubscribesButtonClick` | 45, 46 | `BtnForward` setter | none | as C1 |
| C7 | `ConversationModeChanged_AddThenRemove_TracksMenuItemCheckedChanged` | 51, 52 (and incidentally 58) | `ConversationMenuItem` setter | none | subscribe; set `ConversationModeChecked = true`; expect 1; unsubscribe; set `= false`; expect still 1 |
| C8 | `ConversationModeChecked_RoundTripsMenuItemCheckedState` | 57, 58 | `ConversationMenuItem` setter | none | default `false`; set `true` → get `true`; set `false` → get `false` |
| C9 | `EmailCopyChanged_AddThenRemove_TracksMenuItemCheckedChanged` | 63, 64 | `SaveEmailMenuItem` setter | none | as C7 |
| C10 | `EmailCopyChecked_RoundTripsMenuItemCheckedState` | 69, 70 | `SaveEmailMenuItem` setter | none | as C8 |
| C11 | `AttachmentsChanged_AddThenRemove_TracksMenuItemCheckedChanged` | 75, 76 | `SaveAttachmentsMenuItem` setter | none | as C7 |
| C12 | `AttachmentsChecked_RoundTripsMenuItemCheckedState` | 81, 82 | `SaveAttachmentsMenuItem` setter | none | as C8 |
| C13 | `PicturesChanged_AddThenRemove_TracksMenuItemCheckedChanged` | 87, 88 | `SavePicturesMenuItem` setter | none | as C7. Add an in-test comment citing **LD-1 / the issue it is promoted to**: this event has no production subscriber today, so the test pins the viewer contract only. |
| C14 | `PicturesChecked_RoundTripsMenuItemCheckedState` | 93, 94 | `SavePicturesMenuItem` setter | none | as C8 |
| C15 | `FlagTaskDialogResult_RoundTripsButtonDialogResult` | 99, 100 | `BtnFlagTask` setter | none | default `DialogResult.None`; set `OK` → get `OK`; set `Cancel` → get `Cancel` (both values are the ones production writes, `MailActions.cs:176-195`, `ViewerSetup.cs:371-375`) |
| C16 | `FlagTaskBackColor_RoundTripsButtonBackColor` | 105, 106 | `BtnFlagTask` setter | none | set `Color.Red` → get `Color.Red`. **Do not** assert on the default getter value (it is the ambient `Button.DefaultBackColor`, not `Color.Empty`) and **do not** use `Color.Transparent` (unverified throw risk). |

### 5.3 Negative-path cases the planner should NOT author

`.claude/rules/general-unit-test.md` § Scenario Completeness requires negative flows. The only negative
flow this file admits is "the backing control is null", which throws `NullReferenceException` from the
same line the positive case already covers. **Sixteen `Should().Throw<NullReferenceException>()` tasks
would add 0 lines and 0 branches of coverage and would pin an accidental behaviour (an unguarded
dereference) as a contract.** Recommendation: author **one** representative case if the reviewer requires
a negative flow to be present —

| # | Test name | Purpose |
|---|---|---|
| C17 (optional) | `CommandMembers_OnViewerWithUnassignedControls_ThrowNullReference` | documents that this file has no null guards by design, in one test, with an in-code comment stating that guarding is out of scope under the no-behavior-change NFR |

— and no more. State this explicitly in the plan so a later reviewer does not read the absence as an
omission.

### 5.4 STA determination

**No case requires the STA clause.** This is the expected answer and the evidence is direct, not
inferential: nine existing `QuickFiler.Test` classes construct the full `ItemViewer` control tree —
including all six `ButtonSVG`s and all four `ToolStripMenuItemCb`s — inside plain `[TestClass]`es (§3
item 3), and the `U` fixture recommended here constructs strictly less than that. No `Show()`, no
message pump, no window handle, no clipboard, no drag-drop, no modal dialog, no OLE. **Do not create
the first `*.StaTests.cs` in `QuickFiler.Test` for this file.**

---

## 6. Q7 — 500-line rule

- Current: **109 physical lines** (108 code + trailing newline). Limit 500. Headroom **391 lines**.
- Projected additions: **zero** — §3 recommends no seam and no production change.
- **Projected post-refactor: 109 lines. No split required. No `<Compile Include>` addition to
  `QuickFiler/QuickFiler.csproj`. No mid-wave ledger row for a new production file.**

The only csproj touched by this file's work is `QuickFiler.Test/QuickFiler.Test.csproj` (one
`<Compile Include>` for the new test file), which is not the shared file the epic warns about at
`epic.md:594-617`.

---

## 7. Cross-child notes

**F14 requires zero changes from any sibling child for this file.** Every collaborator is either
F14-owned or already public.

| Symbol | Declared in | Owner | Sufficient as-is? |
|---|---|---|---|
| `BtnDelItem`, `BtnFlagTask`, `BtnPopOut`, `BtnReply`, `BtnReplyAll`, `BtnForward` wrapper properties | `ItemViewer.cs:334-388` | **F14 (own)** | yes — `public` get/set |
| `ConversationMenuItem`, `SaveAttachmentsMenuItem`, `SaveEmailMenuItem`, `SavePicturesMenuItem` wrapper properties | `ItemViewer.cs:404-423` | **F14 (own)** | yes |
| `ToolStripMenuItemCb` | `QuickFiler/Viewers/ToolStripMenuItemCb.cs:11` | **F15** (`quickfiler-form-viewers-bayesian-coverage`) | yes — `public` type, `public` parameterless ctor (`:13`), `public new bool Checked` (`:32`), `public new event EventHandler CheckedChanged` (`:58`). **No change requested.** |
| `SVGControl.ButtonSVG` | `SVGControl/ButtonSVG.cs:13` | outside the epic (`SVGControl` assembly; cf. open issue #418 on `SVGControl.Test`) | yes — `public`, `public` parameterless ctor (`:17`), inherits `Button`. **No change requested.** |
| `IItemViewer` members `:59-74` | `QuickFiler/Viewers/IItemViewer.cs` | **F14 (own)** | yes — the sibling artifact `research.iitemviewer-cs.2026-08-07T21-40.md` records that F14 proposes **no edit** to that file; this artifact does not contradict it |
| `QfcItemController.EventWiring.cs:66-94` call sites | `QuickFiler/Controllers/` | **F10** (issue #453) | consumer only; **no edit requested to any `QfcItemController.*` file** |

**X-C1 (advisory, F15 → F14 — freeze request).** `ToolStripMenuItemCb`'s shadowing `Checked` setter
(`ToolStripMenuItemCb.cs:32-50`) is what makes cases C7-C14 work: it raises `CheckedChanged` on every
assignment and round-trips `_checked` with no host. If F15's own coverage work "fixes" the shadowing —
for example by assigning `base.Checked` or by adding an equality short-circuit before raising
`CheckedChanged` — cases C7, C9, C11, and C13 break (they set the same value twice in the unsubscribe
half). F15 should be told that a change to that setter's raise semantics is a cross-child break, and
F14's cases should carry an in-code comment naming `ToolStripMenuItemCb.cs:35-49` so the break is
legible. **This is a note, not a change request; F14 must not edit `ToolStripMenuItemCb.cs`.**

**X-C2 (intra-F14 ordering, not cross-child).** No per-file number for this file exists until the
type-level `[ExcludeFromCodeCoverage]` at `ItemViewer.cs:20` is removed, because the attribute is applied
to the type and a partial type has one identity. That decision is shared across all six `ItemViewer`
partials and is analysed in `research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:69-102`. This artifact
does not re-decide it and does not contradict it; it only records the dependency. **Note that issue #457
(§9) bears on that decision.**

---

## 8. Latent defect promotion candidates

Each is a distinct promotion candidate for the MCP promotion lifecycle per `epic.md:538-543`. All are
out of scope to fix under F14's no-behavior-change NFR.

### LD-1 — `PicturesChanged` has no subscriber, so toggling "Save Pictures" in QuickFiler is silently discarded

`QuickFiler/Viewers/ItemViewer.Commands.cs:85-89` declares `PicturesChanged` and
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:389` seeds `PicturesChecked` from
`_globals.QfSettings.SavePictures` (`:388`). But `QfcItemController.EventWiring.cs:66-94` wires
`ConversationModeChanged` (`:68`), `EmailCopyChanged` (`:92`), and `AttachmentsChanged` (`:93`) — and
**not** `PicturesChanged`. A repo-wide grep for `PicturesChanged` returns only the declaration in this
file, the `IItemViewer` declaration (`IItemViewer.cs:71`), and a `Mock<IItemViewer>` reference; there is
no `CbxPictures_CheckedChanged` handler anywhere (grep for `CbxPictures` returns nothing). The menu item
has `CheckOnClick = true` (`ItemViewer.Designer.cs:6146`), so a user click *does* flip
`SavePicturesMenuItem.Checked` and *does* raise the event — into a void. The controller's
`_optionsPictures` field (`QfcItemController.cs:57`) is therefore never updated after `ViewerSetup`, and
`QfcItemController.MailActions.cs:102` builds the move operation from the stale settings value
(`SavePictures = _optionsPictures`). Net effect: in the QuickFiler item view, "Save Pictures" is a
read-only display of the saved setting that appears interactive. Contrast `EfcFormController.cs:389`,
which *does* wire `SavePicturesMenuItem.CheckedChanged` for the Explorer form — so the asymmetry is
between two sibling controllers, which is what makes it a defect rather than an intentional omission.

### LD-2 — `ItemViewer.MenuItem_CheckedChanged` is dead code, leaving the four move-option menu items visually desynchronised at construction

`QuickFiler/Viewers/ItemViewer.cs:171-175` and `:177-187` declare two `private void
MenuItem_CheckedChanged` overloads. Neither is ever invoked: a grep of `ItemViewer.Designer.cs` (all
6,224 lines) for `CheckedChanged` and for `.Click +=` returns **zero matches**, its only event wiring
being `_l0v2h2_WebView2.ParentChanged` at `:256`; and `ItemViewer.cs` itself never calls them. The
matching code in the sibling viewer *is* live — `ItemViewerExpanded.Designer.cs:171,180,189,198` wire
`CheckedChanged += MenuItem_CheckedChanged` and `ItemViewerExpanded.cs:24-27` calls it once per menu item
from the constructor. The consequence in `ItemViewer` is a real, user-visible initial-state desync:
`ItemViewer.Designer.cs:6139` and `:6147` assign
`Image = global::QuickFiler.Properties.Resources.CheckBoxChecked` to `SaveEmailMenuItem` and
`SavePicturesMenuItem` at design time, while `ToolStripMenuItemCb`'s `_checked` backing field
(`ToolStripMenuItemCb.cs:51`) defaults to `false` and its constructor's `if (Checked)` guard
(`:16-19`) reads that same `false` and so never clears the image. The two items therefore render as
checked while `EmailCopyChecked`/`PicturesChecked` return `false`, until `ViewerSetup.cs:383/389`
overwrites them. Two candidate fixes exist (call the normaliser from the `ItemViewer` constructor as
`ItemViewerExpanded` does, or delete the dead overloads and fix the Designer's design-time `Image`); the
issue should record both. Note that the second overload (`ItemViewer.cs:177-187`) also has the
base-vs-shadowed `Checked` bug — it takes a `ToolStripMenuItem`, so `menuItem.Checked` binds to the base
property, which `ToolStripMenuItemCb` never assigns — which is why simply calling it would be the wrong
fix without also addressing the downcast.

### LD-3 — `FlagTaskDialogResult` uses a WinForms control property as cross-call mutable scratch state

`ItemViewer.Commands.cs:97-101` exposes `BtnFlagTask.DialogResult` on the `IItemViewer` contract
(`IItemViewer.cs:73`). `QfcItemController.MailActions.cs:176-179` writes it from
`flagTask.Run(modal: true)` and reads it back at `:177` to decide whether to tint the button; the same
pair repeats at `:194-197`. `ViewerSetup.cs:371,375` writes it again during viewer restore. `ButtonSVG`
derives from `Button`, whose `DialogResult` property has real WinForms semantics: when the button is
clicked inside a modal `Form`, WinForms closes the form and returns that value. Storing an unrelated
"did the flag-task dialog succeed" result there couples a controller-local boolean to live dialog
behaviour, and means any future placement of `BtnFlagTask` on a modal form will close that form on the
first click. The value should be a controller field or a returned result, not a control property. This
is a design defect, not a live crash, and is out of scope for a coverage child.

### LD-4 — `ItemViewer.Commands.cs:10` states an exemption that has never been true of this file

The comment reads *"The whole ItemViewer type is [ExcludeFromCodeCoverage] via its primary partial in
ItemViewer.cs."* That is factually correct today but becomes false the moment F14 removes the attribute
at `ItemViewer.cs:20`, and identical stale comments exist at `ItemViewer.DisplayState.cs:9-10` and
`ItemViewer.FolderSearch.cs:17`. This is a documentation-accuracy item rather than a defect: it is the
comment that produced the epic's original 33-file over-count (`epic.md:121-130`). **In-scope for F14's own
execution** — the three comments must be updated in the same change that removes the attribute, so the
next `grep ExcludeFromCodeCoverage` survey does not repeat the error. Not a promotion candidate; listed
here so the planner schedules it.

---

## 9. Open-issue scan

**Method:** WebFetch against the public GitHub issue pages for `drmoisan/TaskMaster` (the Bash tool and
therefore `gh` are unavailable to this agent). Search terms run: `ItemViewer`, `focus OR viewer OR
"folder search"`, `coverage`. Individual issues fetched in full: #438, #444, #445, #457.

| Issue | Title | Bearing on `ItemViewer.Commands.cs` |
|---|---|---|
| **#457** | `excludefromcodecoverage-does-not-suppress-nested-lambdas` **[V-web]** | **Indirect but load-bearing for F14's shared decision.** The issue reports that a method-level `[ExcludeFromCodeCoverage]` does not suppress lambdas the compiler hoists out of that method, so the lambda bodies stay in the denominator (cited ceiling: `BreadcrumbPopupUiOperations.cs` cannot exceed ~91.5%). **This file contains no lambda** (verified §2.1), so its own numbers are unaffected. It matters because F14's plan must decide the disposition of `ItemViewer.Designer.cs` (X-C2), and #457 is evidence that attribute-based suppression is not a reliable instrument. Cite it alongside the filename-based-exclusion recommendation in `research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:93-101`. |
| **#441** | Cobertura post-processing double-counts `<line>` nodes | Direct. Any `line-rate` quoted for this file must carry a "#441 — unreliable" annotation; use F1's recomputed per-file figure. |
| **#432** | `quickfiler-coverage-ledger` (F1) | Direct. This file needs a `testable` ledger row. **Additional harness requirement discovered here:** a file with 0 branch points must be reported `N/A` or `100%` for branch, never `0%`. `epic.md:533-536` states this rule for the *line* denominator only. Recommend F1 extend it to the branch denominator, keyed on `<condition>`-child count rather than on `branch-rate`. |
| **#445**, **#444** | keyboard-action contract defects | **No bearing** — §4.1, zero-match grep. |
| **#438** | `quickfiler-search-keystroke-focus-steal` | **No bearing on this file** — it concerns `TxtboxSearch`/`SetFolderDroppedDown`, i.e. `ItemViewer.FolderSearch.cs`. Analysed in that file's artifact. |
| **#230** | WinForms message-pump test seam | Not needed — §5.4 establishes no pump and no STA is required. |
| **#418** | `SVGControl.Test` is absent from the solution and cannot build | Adjacent: `ButtonSVG` (this file's backing type for six members) lives in `SVGControl`. F14 must not attempt to add `SVGControl` tests; it consumes `ButtonSVG` as an ordinary `Button` subclass. |

No open issue targets `ItemViewer.Commands.cs` directly.

---

## 10. Verified vs inferred

**Verified** (direct read, grep, or fetched issue text, with citations above):

- The file's full contents, its 16 members, its three `using` directives, its zero branch points, and
  its zero lambdas.
- The absence of a real `[ExcludeFromCodeCoverage]` in the file and its presence at `ItemViewer.cs:20`.
- The concrete backing types and the `public` get/set visibility of all eight wrapper properties.
- `ToolStripMenuItemCb`'s shadowing `Checked`/`CheckedChanged` and the setter's unconditional raise.
- `ItemViewer.Designer.cs` wires exactly one event (`ParentChanged`, `:256`) and no `CheckedChanged`/`Click`.
- All four move-option menu items set `CheckOnClick = true` (`:6121`, `:6130`, `:6138`, `:6146`).
- `PicturesChanged` has no production subscriber; `EmailCopyChanged`/`AttachmentsChanged` do.
- `MenuItem_CheckedChanged` in `ItemViewer.cs` has no caller.
- No `QuickFiler/Viewers/` file references `KbdActions`, `KaChar`, `KaKey`, `KaStringAsync`, or `IMailItemActions`.
- Nine existing `QuickFiler.Test` classes construct a real `ItemViewer` in a plain `[TestClass]`.
- `QfcThemeHelperTests.cs:247-265,277-294,331-335` provides the uninitialised-viewer fixture, the
  protected-event reflection helper, and the private-field setter.
- The compile entry at `QuickFiler.csproj:419-422`.

**Inferred** (reasoning from the above, not executed):

- That the 16 cases in §5.2 will measure 32/32 lines. The line-to-sequence-point mapping for
  expression-bodied accessors was not confirmed against a generated Cobertura report for this file
  (none exists yet — P3). If Roslyn merges an accessor pair into fewer sequence points, the count
  falls but the *rate* does not, because covered and coverable move together.
- That `Control.BackColor` set to `Color.Transparent` may throw on `ButtonSVG`. Flagged only to keep it
  out of the test plan; not asserted anywhere.
- That the branch rate for a zero-condition class renders as `1`/`N/A` rather than `0` — this depends on
  F1's harness implementation, which does not yet exist on this branch, hence the §9 recommendation.
