# Per-file research — `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`

- Epic: #136 QuickFiler Per-File 80% Coverage — child F10 (`quickfiler-item-controller-coverage`, issue #453)
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- File length verified on this branch: **219 lines** (matches the brief).

---

## 0. Measured baseline (indicative) and reconciliation

From `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
line 25411:

```
line-rate="0.7956989247311828" branch-rate="0.65" complexity="20"
name="QuickFiler.Controllers.QfcItemController"
filename="QuickFiler\Controllers\QfcItemController.EventHandlers.cs"
```

**Staleness check passed.** Method spans align exactly with the current file:
`CbxConversation_CheckedChanged` = 28–47, `BtnFlagTask_Click` = 50–56, `BtnPopOutCore` = 70,
`BtnDelItem_Click` = 73–79, `CbxAttachments_CheckedChanged` = 215–217.

| Gate | Target | Measured | Verdict |
| --- | --- | --- | --- |
| Line | >= 80% (issue #136 AC1) | **79.57 %** (74 of 93 coverable lines) | **FAIL** — by 0.43 points |
| Branch | >= 75% | **65.00 %** (26 of 40 conditions) | **FAIL** — by 10 points |

The brief's "79.7 %" is a rounding of the exact 0.7956989…; the precise figure is **79.57 %**.
Both gates fail. This is the only one of F10's three assigned event/navigation files that misses the
line floor.

Branch arithmetic (reconciles exactly): 20 conditions in the per-method `<lines>` blocks (13
covered) + the same 20 at class level (13 covered) = 40 / 26 = 0.65. **A branch fixed inside a
named method therefore counts twice.**

Line arithmetic: the class-level `<lines>` block holds 93 entries, 19 with `hits="0"`;
74 / 93 = 0.795699 exactly. (The epic's "187 lines" for this file is the method-block + class-block
sum, i.e. the double-counted figure — do not use it as a denominator.)

F1's harness on this child's branch is the acceptance authority; these are planning inputs.

---

## 1. Member inventory

No fields, properties, constructors, events, or nested types. Twenty-three methods.

| # | Member | Lines | Accessibility | `[ExcludeFromCodeCoverage]` |
| --- | --- | --- | --- | --- |
| 1 | `CbxConversation_CheckedChanged(object, EventArgs)` | 27–47 | `internal void` | No |
| 2 | `BtnFlagTask_Click(object, EventArgs)` | 49–56 | `internal void` | No |
| 3 | `BtnPopOut_Click(object, EventArgs)` | 61–68 | `internal async void` | **Yes — line 60** |
| 4 | `BtnPopOutCore()` | 70 | `internal Task` | No |
| 5 | `BtnDelItem_Click(object, EventArgs)` | 72–79 | `internal void` | No |
| 6 | `BtnReply_Click(object, EventArgs)` | 84–91 | `internal async void` | **Yes — line 83** |
| 7 | `BtnReplyCore()` | 93 | `internal Task` | No |
| 8 | `BtnReplyAll_Click(object, EventArgs)` | 98–105 | `internal async void` | **Yes — line 97** |
| 9 | `BtnReplyAllCore()` | 107 | `internal Task` | No |
| 10 | `BtnForward_Click(object, EventArgs)` | 112–119 | `internal async void` | **Yes — line 111** |
| 11 | `BtnForwardCore()` | 121 | `internal Task` | No |
| 12 | `TxtboxBody_DoubleClick(object, EventArgs)` | 126–133 | `internal async void` | **Yes — line 125** |
| 13 | `TxtboxBodyDoubleClickCore()` | 135 | `internal Task` | No |
| 14 | `Button_MouseEnter(object, EventArgs)` | 137–140 | `private void` | No |
| 15 | `MenuItem_MouseEnter(object, EventArgs)` | 142–145 | `private void` | No |
| 16 | `Button_MouseLeave(object, EventArgs)` | 147–157 | `private void` | No |
| 17 | `MenuItem_MouseLeave(object, EventArgs)` | 159–162 | `private void` | No |
| 18 | `TextBoxSearch_TextChanged(object, EventArgs)` | 164–178 | `internal void` | No |
| 19 | `TextBoxSearch_KeyDown(object, KeyEventArgs)` | 180–189 | `internal void` | No |
| 20 | `TopicThread_ItemSelectionChanged(object, ListViewItemSelectionChangedEventArgs)` | 191–202 | `private void` | No |
| 21 | `CbxEmailCopy_CheckedChanged(object, EventArgs)` | 204–207 | `private void` | No |
| 22 | `CboFolders_SelectedIndexChanged(object, EventArgs)` | 209–212 | `private void` | No |
| 23 | `CbxAttachments_CheckedChanged(object, EventArgs)` | 214–217 | `private void` | No |

One compiler-generated closure: `<TxtboxBodyDoubleClickCore>b__179_0` (line 135, covered).

### Exemption assessment (per the #227 maintainer precedent)

All five attributes are **method-level**, so 5 × 7 = 35 statements sit outside the denominator
today. Each of the five is the identical shape: a `SynchronizationContext` null-guard plus
`await <X>Core()`, where `<X>Core` is a one-line expression-bodied method that is **already at
100 % coverage** (`BtnPopOutCore` SeamCoreTests.cs:104, `BtnReplyCore` :120, `BtnReplyAllCore` :131,
`BtnForwardCore` :142, `TxtboxBodyDoubleClickCore` :153).

The residual barrier is the `async void` return type: the shell cannot be awaited, so a test cannot
observe its completion deterministically without a synchronization-context pump — a genuine
WinForms framework-signature constraint, not a missing seam.

**However, the exemption is inconsistently applied, and the #227 precedent says inconsistency is
evidence the boundary is wrong, not evidence the barrier is real.** Two structurally identical
siblings are **not** exempt and **are** tested:

- `BtnDelItem_Click` (72–79) — same guard, then `MarkItemForDeletion()`; tested at
  `EventHandlersTests.cs:241`.
- `BtnFlagTask_Click` (49–56) — same guard, then `FlagAsTask()`; tested at
  `EventHandlersTests.cs:272`, with an explicit comment (`:269`) noting it is "structurally
  identical to its non-exempt sibling".

The only structural difference is `void` vs `async void`. That difference is real but narrow: the
guard lines (63–66, 86–89, 100–103, 128–131) execute **synchronously before the first `await`**, so
calling the shell from a test and then asserting on the observable side effect of the already-tested
core is deterministic for those lines even though the continuation is not awaited. Recommendation:
**keep the five exemptions**, but record the barrier as "`async void` cannot be awaited" — *not* as
"the routing is untestable", because the routing is already proven. Removing them would add ~35
lines of which the guard portion is reachable and the `await` portion is not, lowering the measured
percentage. This is a defensible per-member analysis; it is not a blanket category exemption.

---

## 2. What is already covered

Existing test files: `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs`
(439 lines, class `QfcItemController_EventHandlersTests`) and
`QuickFiler.Test/Controllers/QfcItemController.SeamCoreTests.cs`.

| Member | Status | Covering test(s) |
| --- | --- | --- |
| `CbxConversation_CheckedChanged` | **PARTIALLY COVERED** — 29.4 % line, 33.3 % branch (the worst member in all three assigned files) | `CbxConversation_CheckedChanged_WhenSuppressed_StoresCheckedStateWithoutSideEffects` (EventHandlersTests.cs:145) — the `SuppressEvents == true` path only |
| `BtnFlagTask_Click` | **PARTIALLY COVERED** — 42.9 % / 50 % | `BtnFlagTask_Click_InvokesFlagAsTask` (:272) — reaches the factory then throws a sentinel, so line 56 never executes |
| `BtnPopOut_Click` | EXEMPT | — |
| `BtnPopOutCore` | **COVERED** 100 % | `BtnPopOutCore_PopsOutOwnItemGroup` (SeamCoreTests.cs:104) |
| `BtnDelItem_Click` | **PARTIALLY COVERED** — 57.1 % / 50 % | `BtnDelItem_Click_MarksItemForDeletion` (:241) |
| `BtnReply_Click` | EXEMPT | — |
| `BtnReplyCore` | **COVERED** 100 % | `BtnReplyCore_RoutesToReplySeam` (SeamCoreTests.cs:120) |
| `BtnReplyAll_Click` | EXEMPT | — |
| `BtnReplyAllCore` | **COVERED** 100 % | `BtnReplyAllCore_RoutesToReplyAllSeam` (SeamCoreTests.cs:131) |
| `BtnForward_Click` | EXEMPT | — |
| `BtnForwardCore` | **COVERED** 100 % | `BtnForwardCore_RoutesToForwardSeam` (SeamCoreTests.cs:142) |
| `TxtboxBody_DoubleClick` | EXEMPT | — |
| `TxtboxBodyDoubleClickCore` | **COVERED** 100 % | `TxtboxBodyDoubleClickCore_DisplaysMailThroughSeam` (SeamCoreTests.cs:153) |
| `Button_MouseEnter` | **COVERED** 100 % | `Button_MouseEnter_SetsMouseOverColor` (:46) |
| `MenuItem_MouseEnter` | **COVERED** 100 % | `MenuItem_MouseEnter_SetsMouseOverColor` (:105) |
| `Button_MouseLeave` | **COVERED** 100 % line, 100 % branch | `…_WhenDialogResultOk_SetsClickedColor` (:65), `…_WhenNotDialogResultOk_SetsBackColor` (:85) |
| `MenuItem_MouseLeave` | **COVERED** 100 % | `MenuItem_MouseLeave_SetsBackColor` (:124) |
| `TextBoxSearch_TextChanged` | **COVERED** 100 % / 100 % | `TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PopulatesAndSelectsFolder` (:314) |
| `TextBoxSearch_KeyDown` | **COVERED** 100 % / 100 % | `…_WhenDownArrow_DropsDownAndFocusesFolder` (:355), `…_WhenNotDownArrow_DoesNothing` (:374) |
| `TopicThread_ItemSelectionChanged` | **PARTIALLY COVERED** — 100 % line, **75 % branch** | `…_WhenItemSelected_NavigatesToItsHtml` (:393), `…_WhenNoSelection_DoesNotNavigate` (:418) |
| `CbxEmailCopy_CheckedChanged` | **COVERED** 100 % | `CbxEmailCopy_CheckedChanged_StoresCheckedState` (:167) |
| `CboFolders_SelectedIndexChanged` | **COVERED** 100 % | `CboFolders_SelectedIndexChanged_StoresSelectedFolder` (:215) |
| `CbxAttachments_CheckedChanged` | **COVERED** 100 % | `CbxAttachments_CheckedChanged_StoresCheckedState` (:191) |

**Do not re-author any of the above.** Seventeen of the eighteen measured members are already at
100 % line. The whole shortfall is concentrated in **three members**.

---

## 3. The gap list

### 3.1 Uncovered lines — all 19, in three members

| Member | Uncovered lines | Count | What reaches them |
| --- | --- | --- | --- |
| `CbxConversation_CheckedChanged` | 30, 31, 32 | 3 | ambient `SynchronizationContext.Current == null` at entry |
| `CbxConversation_CheckedChanged` | 37, 38, 39, 40, 41, 43, 44, 45, 46 | 9 | `SuppressEvents == false`, both `_optionConversationChecked` states |
| `BtnFlagTask_Click` | 52, 53, 54 | 3 | ambient context null |
| `BtnFlagTask_Click` | 56 | 1 | `FlagAsTask()` returning normally instead of throwing the test sentinel |
| `BtnDelItem_Click` | 75, 76, 77 | 3 | ambient context null |
| **Total** | | **19** | |

### 3.2 Uncovered branches

| Line | Construct | Coverage | Missing direction | Weight (method + class) |
| --- | --- | --- | --- | --- |
| 29 | `if (SynchronizationContext.Current is null)` | 1/2 | true | 2 |
| 36 | `if (!SuppressEvents)` | 1/2 | true | 2 |
| 38 | `if (_optionConversationChecked)` | 0/2 | both | 4 |
| 51 | `if (SynchronizationContext.Current is null)` | 1/2 | true | 2 |
| 74 | `if (SynchronizationContext.Current is null)` | 1/2 | true | 2 |
| 197 | `(objects is not null) && (objects.Count != 0)` | 3/4 | `objects == null` | 2 |

Lines 149, 175, 182 are already 2/2.

**Branch-heavy members:** `CbxConversation_CheckedChanged` is the only branch-dense member
(complexity 6, three branch points, 2 of 6 conditions covered). Everything else has one branch or
none.

**Path to both gates, cheapest first:**

1. Cover `SuppressEvents == false` with `_optionConversationChecked` true, then false: **+9 lines,
   +6 conditions.** Line 74 → 83/93 = **89.2 %** (PASS). Branch 26 → 32/40 = **80 %** (PASS).
2. Cover `objects == null` in `TopicThread_ItemSelectionChanged`: **+2 conditions** → 34/40 = 85 %.
3. Cover `FlagAsTask()` returning normally: **+1 line** → 84/93 = 90.3 %.
4. The three `SynchronizationContext` guard branches (29, 51, 74) and their 9 lines are the residual
   and need the seam in §5.2.

**Two tests (step 1) move this file from failing both gates to passing both gates.** That is the
single highest-value item in the whole of F10.

---

## 4. Event subscription lifecycle

This file contains **handlers only — it makes no `+=` or `-=` subscription of its own.** Every
member here is a subscription *target* of `WireIntentEvents()` /`WireControlTreeEvents()` in
`QfcItemController.EventWiring.cs:40–93`. The full subscribe/unsubscribe map, the leak analysis, and
the double-subscription analysis are recorded once in
`file-QfcItemController.EventWiring.md` § 4 and are not duplicated here.

What this file contributes to that analysis:

**H-1 (Medium) — these handlers are the *observable consequence* of the unwiring gap.** Because
`Cleanup()` (`QfcItemController.ViewerSetup.cs:392–421`) never detaches any of the 22 subscriptions,
each handler below can execute after its collaborators are null:

| Handler | Post-`Cleanup()` behaviour | Line |
| --- | --- | --- |
| `Button_MouseLeave`, `MenuItem_MouseLeave`, `Button_MouseEnter`, `MenuItem_MouseEnter` | `_themes[_activeTheme]` → `NullReferenceException` (`_themes` nulled at `ViewerSetup.cs:410`) | 139, 144, 151/155, 161 |
| `CbxConversation_CheckedChanged` | `_itemViewer.ConversationModeChecked` → `NullReferenceException` (`_itemViewer` nulled at `:403`) | 35 |
| `CbxEmailCopy_CheckedChanged`, `CbxAttachments_CheckedChanged`, `CboFolders_SelectedIndexChanged` | same | 206, 216, 211 |
| `TextBoxSearch_TextChanged` | `_folderHandler` nulled at `:408, :411` → `NullReferenceException` | 166 |
| `TopicThread_ItemSelectionChanged` | `_itemViewer` null → `NullReferenceException` | 196 |
| `BtnDelItem_Click`, `BtnFlagTask_Click` | route into `MailActions.cs` against nulled `_globals`/`_homeController` | 78, 55 |

The mouse handlers are the most exposed because `QfcCollectionController.RemoveControls()` removes
TLP rows (`QfcCollectionController.cs:999`) *before* calling `Cleanup()` (`:1003`) — control removal
can raise `MouseLeave`. The current ordering is what keeps the window mostly closed; nothing
enforces it.

**H-2 (Low) — no handler is re-entrancy guarded, and one is genuinely re-entrant.**
`CbxConversation_CheckedChanged` writes `_optionConversationChecked` from
`_itemViewer.ConversationModeChecked` and then, when `SuppressEvents` is false, calls
`CollapseConversation()` / `EnumerateConversation()` (`QfcItemController.MailActions.cs:27` and
`:36`), which call `_parent.ToggleGroupConv` / `_parent.ToggleUnGroupConv` — which rebuild the item
group and can set the checkbox again. `SuppressEvents` (`QfcItemController.cs:256–260`) is the
manual re-entrancy guard, and **nothing in this file sets or restores it** — the discipline lives in
the collection controller. A test that flips the checkbox from inside the handler and asserts the
guard prevents recursion pins this.

**H-3 — `sender` is ignored by every handler.** All 23 handlers except the four
`Button_/MenuItem_Mouse*` ones discard `sender` and read state from `_itemViewer` instead. The four
that use it cast unconditionally (`((Button)sender)`, `((ToolStripMenuItem)sender)`) with no type
check — an `InvalidCastException` if ever wired to a different control type. Wiring only attaches
them to `Buttons` and `MenuItems`, so this is contained, but it is an unguarded assumption worth a
negative test.

---

## 5. Seam analysis

### 5.1 Members needing no new seam (the 80 %-gate work)

`CbxConversation_CheckedChanged` lines 37–46 are reachable **today**:

- `SuppressEvents` is a public property (`QfcItemController.cs:256–260`) — settable directly, as
  `EventHandlersTests.cs:154` already does.
- `_optionConversationChecked` is driven by `_itemViewer.ConversationModeChecked`, an `IItemViewer`
  property — `Mock<IItemViewer>.SetupGet` (already used at `:151`).
- `CollapseConversation()` (`MailActions.cs:27–34`) needs only `_itemViewer.GetFolderItems()`,
  `_convOriginID` (public property, `QfcItemController.cs:103–107`), `_mailActions.EntryID`
  (`IMailItemActions`), and `_parent.ToggleGroupConv(entryID)` (`IQfcCollectionController`). **All
  four are interfaces or public properties. Fully mockable, no production change.**
- `EnumerateConversation()` (`MailActions.cs:36–47`) needs the same plus the concrete
  `ConversationResolver` for `ConversationResolver.Count.SameFolder`. See §5.3.

`BtnFlagTask_Click` line 56 needs `FlagAsTask()` to return without throwing — supply a
`_flagTasksFactory` that returns a `FlagTasks` whose `Run` is a no-op, rather than the sentinel-throw
stub at `EventHandlersTests.cs:277–286`. Verify against `MailActions.cs:167` whether `Run(modal:
true)` would surface a dialog; if it would, the correct move is a factory returning a stub that
records instead of the current throw. **No production change.**

`TopicThread_ItemSelectionChanged` line 197's missing `objects == null` direction is reached by
`viewer.Setup(v => v.GetSelectedConversationItems()).Returns((List<object>)null)`. **No production
change.**

### 5.2 The one real seam gap — the duplicated `SynchronizationContext` guard

Seven members open with the identical three-line block:

```csharp
if (SynchronizationContext.Current is null)
    SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
```

(lines 29–32, 51–54, 63–66, 74–77, 86–89, 100–103, 128–131). Three of those are non-exempt, giving
9 uncovered lines and 3 half-covered branch points.

**The true branch is not safely testable as written.** Forcing it requires
`SynchronizationContext.SetSynchronizationContext(null)` before the call, after which production
constructs a real `WindowsFormsSynchronizationContext`. On .NET Framework that creates the thread's
marshalling control (a real window handle) and **installs itself as the ambient context on the test
thread for the remainder of the process**, breaking test independence for every subsequent test in
that MSTest thread. `.claude/rules/general-unit-test.md` § "Core Principles" 1 (independence) rules
this out. The existing harness deliberately avoids it: `QfcItemControllerTestSupport.EnsureSynchronizationContext()`
(TestSupport.cs:87–93) documents itself as exercising the guard "as a deterministic no-op (never
constructing a WinForms sync context)".

**Minimum seam, per the hierarchy (interface > injectable delegate > adapter):** an interface for
"ensure an ambient synchronization context" would be a one-method interface with a single
implementation — heavier than warranted. Use an **injectable delegate plus one extracted helper**,
matching the six factory-delegate seams already on this class (`QfcItemController.cs:66–89`):

```csharp
// QfcItemController.cs, private-fields region
private Func<SynchronizationContext> _uiSyncContextFactory =
    () => new WindowsFormsSynchronizationContext();

// QfcItemController.EventHandlers.cs, once
private void EnsureUiSynchronizationContext()
{
    if (SynchronizationContext.Current is null)
    {
        SynchronizationContext.SetSynchronizationContext(_uiSyncContextFactory());
    }
}
```

and replace all seven inline blocks with `EnsureUiSynchronizationContext();`.

Effects, all favourable:

- Removes 9 uncovered lines and 3 half-covered branch points; replaces them with **one** branch that
  is fully coverable by injecting `() => new SynchronizationContext()` (a plain, inert context that
  can be restored in a `finally`, exactly as the two `EventWiringTests` do at `:305–308`).
- Removes 21 lines of duplication (7 × 3) — General Code Change Policy § 1.2 (reusability) and § 2.2
  (small pure helpers).
- Shrinks the file from 219 to roughly 205 lines.
- No behaviour change: the default delegate reproduces the current expression exactly.

**Note this seam is optional for the gates.** §3.2 step 1 alone passes both. Recommend doing it
anyway, because it is the only way these 9 lines ever become covered and because the duplication is
itself a policy issue.

### 5.3 `EnumerateConversation()` and the F4 boundary

`EnumerateConversation` reads `ConversationResolver.Count.SameFolder`
(`QfcItemController.MailActions.cs:44`). `ConversationResolver`
(`QuickFiler/Helper Classes/ConversationResolver.cs:30`) is **F4-owned (#434)**. Its two-argument
constructor is inert:

```csharp
public ConversationResolver(IApplicationGlobals appGlobals, MailItem mailItem)  // line 64
{
    _globals = appGlobals;
    _mailItem = mailItem;
}
```

— field assignment only, no COM call, so `new ConversationResolver(Mock<IApplicationGlobals>.Object,
Mock<MailItem>.Object)` is safe in a unit test. The `Count` property may need its backing state
seeded by reflection; verify at plan time and record the exact member.

**Do not edit `ConversationResolver.cs`.** Per epic.md and the F4 research note, it is constructed
**positionally**; depend on the *current* two-argument shape and pin it with a compile-time
reference so an F4 signature change surfaces as a build break rather than a silent behaviour change.

If seeding `Count` proves to require F4-owned internals, the fallback is to promote
`CollapseConversation`/`EnumerateConversation` (`MailActions.cs:27`, `:36`) to `internal virtual`
and use a spy subclass — the pattern already established in this repository by
`public virtual void ToggleExpansion(Enums.ToggleState)` (`QfcItemController.Navigation.cs:174`) and
`ExpansionSpyController` (`NavigationTests.cs:139–157`). Both files are F10-owned, so this stays
inside the child's boundary. Prefer full mocking; use the virtual-spy only if §5.3's first option
fails.

### 5.4 Barrier classification for every uncovered member

| Uncovered element | Barrier | Minimum seam |
| --- | --- | --- |
| 37–46 (`SuppressEvents == false` body) | none | none — mock `IItemViewer`, `IMailItemActions`, `IQfcCollectionController` |
| 44 (`EnumerateConversation()`) | concrete `ConversationResolver` (F4) | none — use the inert 2-arg ctor; virtual-spy fallback |
| 56 (`BtnFlagTask_Click` normal return) | none | none — non-throwing `_flagTasksFactory` stub |
| 30–32, 52–54, 75–77 (guards) | ambient `SynchronizationContext` is process/thread state; forcing it constructs a real `WindowsFormsSynchronizationContext` | `Func<SynchronizationContext>` injectable delegate (§5.2) |
| 197 (`objects == null`) | none | none |

No Outlook Interop type is dereferenced anywhere in this file. `Microsoft.Office.Interop.Outlook` is
imported (line 12) but used only transitively. **No STA thread and no live WinForms form is required
for any test in this file** — the existing tests already prove that handle-less `new Button()` and
`new ToolStripMenuItem()` senders suffice (`EventHandlersTests.cs:50`, `:109`). The STA last-resort
clause does **not** apply here.

---

## 6. State-transition invariants

| # | Invariant | Held by | Pinning test (§8) |
| --- | --- | --- | --- |
| I-1 | Every handler that can be raised off the UI thread installs an ambient context before touching viewer state | 29, 51, 63, 74, 86, 100, 128 | EH-6, EH-7 |
| I-2 | The guard is a no-op when a context already exists (idempotent) | 29 false branch | already pinned via `EnsureSynchronizationContext()` (TestSupport.cs:87) |
| I-3 | `CbxConversation_CheckedChanged` **always** mirrors the viewer's checkbox into `_optionConversationChecked`, regardless of `SuppressEvents` | 35 precedes 36 | already pinned (`:145`) |
| I-4 | `SuppressEvents == true` suppresses *only* the conversation side effects, never the field write | 36 | already pinned (`:145`) |
| I-5 | `SuppressEvents == false` + checked → `CollapseConversation()`; + unchecked → `EnumerateConversation()`. Exactly one runs. | 38–46 | **EH-1, EH-2** |
| I-6 | `SuppressEvents` is the sole re-entrancy guard for the conversation regroup cycle; it is set by the caller, never by this file | `QfcItemController.cs:256–260` | EH-3 |
| I-7 | `TopicThread_ItemSelectionChanged` navigates only when the selection is non-null **and** non-empty; short-circuit order matters (`objects.Count` must not be evaluated on null) | 197 | EH-4 (null), already pinned for empty (`:418`) |
| I-8 | `TextBoxSearch_TextChanged` always clears before setting, and selects index 1 only when >= 2 folders returned | 172–177 | already pinned (`:314`); add the < 2 case — EH-5 |
| I-9 | `TextBoxSearch_KeyDown` marks the event `Handled` **and** `SuppressKeyPress` only for `Keys.Down` | 182–188 | already pinned both ways (`:355`, `:374`) |
| I-10 | `Button_MouseLeave` selects clicked-colour vs back-colour purely on `DialogResult == OK` | 149 | already pinned both ways (`:65`, `:85`) |
| I-11 | Handlers ignore `sender` except the four mouse handlers, which cast it unconditionally | 139, 144, 149–155, 161 | EH-8 (negative: wrong sender type → `InvalidCastException`) |
| I-12 | **Dispose-before-setup:** after `Cleanup()`, every handler in this file throws `NullReferenceException` if invoked, because its collaborator field is null and no handler is detached | `ViewerSetup.cs:392–421` | EH-9, EH-10 (characterisation of D-1 in the EventWiring artifact) |
| I-13 | `BtnFlagTask_Click`/`BtnDelItem_Click` complete synchronously (they are `void`, not `async void`), so a test can assert immediately after the call | 49, 72 | already pinned (`:241`, `:272`) |

---

## 7. Determinism requirements

| Concern | Location | Disposition |
| --- | --- | --- |
| **Ambient `SynchronizationContext` mutation** — `SetSynchronizationContext(new WindowsFormsSynchronizationContext())` is process/thread-global state written by production code | 30–32, 52–54, 63–66, 75–77, 86–89, 100–103, 128–131 | The principal determinism hazard in this file. Every test touching these members **must** capture and restore `SynchronizationContext.Current` in a `finally`, as `EventWiringTests.cs:305–308` does. The `_uiSyncContextFactory` seam (§5.2) makes the true branch testable without installing a WinForms context. |
| `DateTime.Now` / `DateTime.UtcNow` / `Random` / `Random.Shared` | none | **Verified absent from this file.** |
| `Thread.Sleep` / `Task.Delay` / wall-clock wait | none in production | Verified absent. None may be introduced in tests. |
| `Task.Run` | line 135 (`TxtboxBodyDoubleClickCore`) | Returns a `Task` the caller awaits; the existing test awaits it (SeamCoreTests.cs:159). Deterministic. No fire-and-forget. |
| UI-thread marshalling | none directly — all viewer access is through `IItemViewer` | The four `async void` shells marshal implicitly via the installed context; all four are exempt. |
| `async void` | 61, 84, 98, 112, 126 (all exempt) | No test may call these five; a fault after the first `await` would escape to the thread pool. Test their `*Core` counterparts instead — already done. |

---

## 8. Proposed test case list

Fixtures: **A** = `HarnessController` + `QfcItemControllerTestSupport.SetField/GetField/InvokeNonPublic`;
**C** = `Mock<IItemViewer>`; **P** = `Mock<IQfcCollectionController>`; **M** = `Mock<IMailItemActions>`;
**R** = real `ConversationResolver` via the inert 2-arg ctor; **S** = `SynchronizationContext`
capture/restore in `finally`.

| ID | Target member | Scenario | Fixture | Closes |
| --- | --- | --- | --- | --- |
| **EH-1** | `CbxConversation_CheckedChanged` | positive — `SuppressEvents = false`, `ConversationModeChecked = true` → `CollapseConversation()` runs, `_parent.ToggleGroupConv(entryId)` called once, `EnumerateConversation` not called | A+C+P+M+S | lines 37–41, branch 36 true, branch 38 true |
| **EH-2** | `CbxConversation_CheckedChanged` | positive — `SuppressEvents = false`, `ConversationModeChecked = false` → `EnumerateConversation()` runs, `_parent.ToggleUnGroupConv(...)` called once | A+C+P+M+R+S | lines 43–46, branch 38 false |
| EH-3 | `CbxConversation_CheckedChanged` | re-entrancy — with `SuppressEvents = true`, a nested raise from inside `ToggleGroupConv` does not recurse into `CollapseConversation` | A+C+P+S | I-6 |
| EH-4 | `TopicThread_ItemSelectionChanged` | negative — `GetSelectedConversationItems()` returns `null` → no `NavigateToString`, no `NullReferenceException` (proves short-circuit order) | A+C | branch 197 (×2) |
| EH-5 | `TextBoxSearch_TextChanged` | edge — handler returns 0 or 1 folders → `SetFolderSelectedIndex` never called, `SetFolderDroppedDown(true)` still called | A+C | I-8 (line 175 false direction is already covered; this pins the behaviour) |
| EH-6 | `EnsureUiSynchronizationContext` (new helper) | positive — with the ambient context null and `_uiSyncContextFactory` injected, the factory result is installed | A+S | lines 30–32 equivalent, branch true |
| EH-7 | `EnsureUiSynchronizationContext` (new helper) | negative — with a context already present, the factory is never invoked and the context is unchanged | A+S | branch false |
| EH-8 | `Button_MouseEnter` / `MenuItem_MouseEnter` | error — a `Label` passed as `sender` throws `InvalidCastException` | A | I-11 |
| EH-9 | `Button_MouseLeave` after `Cleanup()` | dispose — `_themes` null → `NullReferenceException` | A | I-12, characterises D-1 |
| EH-10 | `CbxConversation_CheckedChanged` after `Cleanup()` | dispose — `_itemViewer` null → `NullReferenceException` | A+S | I-12, characterises D-1 |
| EH-11 | `BtnFlagTask_Click` | positive — non-throwing `_flagTasksFactory` stub; the handler returns normally | A+S | line 56 |
| EH-12 | `BtnDelItem_Click` | negative — `FolderContains("Trash to Delete")` returns `false`; assert the alternative path (complements the existing `:241` true case) | A+C+S | branch coverage inside `MarkItemForDeletion` (credited to `MailActions.cs`, not this file — schedule with that file's plan) |

**Minimum set to pass both gates: EH-1 and EH-2.** Two tests take line 79.57 % → 89.2 % and branch
65 % → 80 %. EH-4 adds 5 branch points of margin for one more test. EH-6/EH-7 depend on the §5.2
seam and are what take the file to ~100 % line.

**Test-file placement.** `QfcItemController.EventHandlersTests.cs` is at **439 lines** against the
500-line ceiling; adding 12 tests will breach it. Plan the split up front:
`QfcItemController.EventHandlersTests.cs` (mouse/theme + checkbox field-write handlers, existing)
and a new `QfcItemController.EventHandlersConversationTests.cs` (EH-1 … EH-3, EH-9, EH-10, EH-11).
If `QuickFiler.Test.csproj` is a legacy non-SDK project with explicit `<Compile Include=...>` (as
`QuickFiler.csproj` is), each new test file needs its own entry — verify before planning.

---

## 9. File-size and creation impact

- `QfcItemController.EventHandlers.cs` is **219 lines**. The §5.2 seam **reduces** it to roughly
  205 (7 three-line blocks → 7 one-line calls, plus a 7-line helper). No split needed; no risk of
  breaching 500.
- `QfcItemController.cs` is **323 lines**; the `_uiSyncContextFactory` field costs 2–4 lines
  including its comment. No split needed.
- **No new production file is required.** Therefore no `<Compile Include=...>` entry in
  `QuickFiler/QuickFiler.csproj` and no ledger row are owed by this file. Should that change:
  `QuickFiler.csproj` is legacy non-SDK with **no globbing**, so the entry is mandatory; **preserve
  CRLF** by using the Edit tool or `perl -0777` with explicit `\r\n` — never a git-bash `sed -i`,
  which strips CRLF and produces a whole-file diff guaranteed to conflict at fan-in; keep the hunk
  minimal and adjacent; and append a ledger row classified `testable` at **>= 90 %** per epic.md
  § "Mid-Wave File Creation".
- The new helper `EnsureUiSynchronizationContext()` lives in this existing file, so it creates no
  new denominator entry — it is new *code* inside an existing file, subject to the "no regression on
  changed lines" rule rather than the >= 90 % new-file rule.

---

## 10. Sibling boundaries — do not edit

| Artefact | Owner | Dependency from this file | Rule |
| --- | --- | --- | --- |
| `QuickFiler/Helper Classes/ConversationResolver.cs` | **F4 (#434)** | `EnumerateConversation()` reads `ConversationResolver.Count.SameFolder` (`MailActions.cs:44`); the property is set from `_conversationResolverFactory` (`QfcItemController.cs:69`). | **Do not edit.** Construct it **positionally** with the current two-argument constructor `(IApplicationGlobals, MailItem)` at `ConversationResolver.cs:64`, which is inert (field assignment only). Pin the shape with a direct `new` so an F4 signature change breaks the build rather than the behaviour. If `Count` cannot be seeded without F4 internals, use the virtual-spy fallback (§5.3) inside F10-owned files. |
| `QuickFiler/Controllers/KeyboardHandler.cs` | **F3 (#430)** | Not referenced from this file. `_kbdHandler.CboFolders_KeyDownAsync` is wired in `EventWiring.cs:82`, not here. | No contact. |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | **F5** | Not referenced from this file. | No contact. |
| `QuickFiler/Viewers/IItemViewer.cs` | **F14** | 15 of the 23 handlers read or call `IItemViewer` members (`ConversationModeChecked`, `EmailCopyChecked`, `AttachmentsChecked`, `SearchText`, `GetSelectedFolder`, `GetSelectedConversationItems`, `NavigateToString`, `ClearFolderItems`, `SetFolderItems`, `SetFolderSelectedIndex`, `SetFolderDroppedDown`, `FocusFolderDropDown`, `FolderContains`, `SetFolderSelectedItem`, `GetFolderItems`). | **Do not edit.** No change is needed — every member used is already on the interface and already mocked in the existing tests. |
| `QuickFiler/Interfaces/IMailItemActions.cs`, `MailItemActionsAdapter.cs` | **F3 (#430)** | `_mailActions.Display()` (line 135) and `_mailActions.EntryID` (via `MailActions.cs`). | **Do not edit.** `Mock<IMailItemActions>` already used at SeamCoreTests.cs:155. |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | **F10 — this child** | `CollapseConversation()` (:27), `EnumerateConversation()` (:36), `FlagAsTask()` (:167), `MarkItemForDeletion()` (:202). | In scope, but owned by a **different per-file artifact**. Any `internal virtual` promotion (§5.3 fallback) must be recorded in that file's plan too, and its coverage effect credited there. |
| `UtilitiesCS/Properties/AssemblyInfo.cs` | out of every child's assignment | `QuickFiler.Test` has no `InternalsVisibleTo` grant from `UtilitiesCS` (epic.md § "Cross-Child Constraints" 2). | **Do not edit.** Nothing here needs a `UtilitiesCS` internal — `Theme`, `IApplicationGlobals`, and `IUiDispatcher` are all public. |

**Cross-child contract notes raised by this file:** one — F4 must not change the
`ConversationResolver(IApplicationGlobals, MailItem)` constructor shape while F10 is in flight, or
EH-2's fixture breaks.

---

## Latent defects for promotion

Per epic.md § "Latent Defect Promotion", promote via the MCP promotion lifecycle. **Do not fix
under this child.**

| ID | Defect | Location | Severity |
| --- | --- | --- | --- |
| E-1 | **Handlers execute against nulled collaborators after `Cleanup()`.** No handler in this file is detached by `Cleanup()`, and all of them dereference a field that `Cleanup()` nulls. `Button_MouseLeave`/`MenuItem_MouseLeave` are the most exposed because `QfcCollectionController.RemoveControls()` removes TLP rows (`:999`) before calling `Cleanup()` (`:1003`), and control removal can raise `MouseLeave`. This is the consumer-side face of D-1 in `file-QfcItemController.EventWiring.md`. | handlers at 139, 151, 155, 161, 35, 166, 196, 206, 211, 216; cleanup at `ViewerSetup.cs:392–421` | **Medium** |
| E-2 | **The `SynchronizationContext` guard is duplicated verbatim seven times** and each copy silently mutates thread-global state. Beyond the duplication, installing a `WindowsFormsSynchronizationContext` from an arbitrary handler thread is a side effect no caller can observe or undo. | 29–32, 51–54, 63–66, 74–77, 86–89, 100–103, 128–131 | **Low** |
| E-3 | **`TopicThread_ItemSelectionChanged` dereferences an unchecked `as` result.** Line 199 `var info = objects[0] as MailItemHelper;` followed by line 200 `_itemViewer.NavigateToString(info.Html);` — if element 0 is not a `MailItemHelper`, `info` is null and line 200 throws `NullReferenceException`. The `is not null` check at 197 guards the *list*, not the *cast*. | 199–200 | **Low–Medium** |
| E-4 | **Four mouse handlers cast `sender` unconditionally** (`((Button)sender)`, `((ToolStripMenuItem)sender)`) with no type check. Contained today because wiring only attaches them to `Buttons`/`MenuItems`, but the assumption is unenforced. | 139, 144, 149–155, 161 | **Low** |
| E-5 | **`CbxConversation_CheckedChanged`'s re-entrancy guard lives outside the handler.** `SuppressEvents` is a public mutable property with no scoping helper; a caller that forgets to restore it leaves conversation side effects permanently suppressed. A `using`-scoped guard would make the invariant self-enforcing. | 36; property at `QfcItemController.cs:256–260` | **Low** |

## Checked and clear

- **No `DateTime.Now`, `DateTime.UtcNow`, `Random`, `Random.Shared`, `Thread.Sleep`, or `Task.Delay`**
  anywhere in this file.
- **No Outlook Interop member is dereferenced.** `Microsoft.Office.Interop.Outlook` is imported at
  line 12 but only transitively used; no `MailItem`, `Store`, `MAPIFolder`, or `Application` access.
- **No live WinForms form and no STA thread is required** for any test proposed here; handle-less
  `new Button()` / `new ToolStripMenuItem()` senders are already proven sufficient
  (`EventHandlersTests.cs:50`, `:109`). The epic's STA last-resort clause does not apply to this file.
- **No `+=`/`-=` subscription is made in this file**; it is a pure handler surface.
