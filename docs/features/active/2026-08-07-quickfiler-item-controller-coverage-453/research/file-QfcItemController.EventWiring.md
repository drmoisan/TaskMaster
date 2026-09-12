# Per-file research — `QuickFiler/Controllers/QfcItemController.EventWiring.cs`

- Epic: #136 QuickFiler Per-File 80% Coverage — child F10 (`quickfiler-item-controller-coverage`, issue #453)
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- File length verified on this branch: **391 lines** (matches the brief).

---

## 0. Measured baseline (indicative) and reconciliation

The most recent committed QuickFiler-wide Cobertura report is
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.
Its `<class>` element for this file (line 24601 of that artifact) reads:

```
line-rate="0.81993" branch-rate="0.65625" complexity="22"
name="QuickFiler.Controllers.QfcItemController"
filename="QuickFiler\Controllers\QfcItemController.EventWiring.cs"
```

**Staleness check passed.** The artifact's method line-spans align exactly with the current file:
`WireEvents` = 29–32 (current file 28–32), `WireControlTreeEvents` = 36–64, `WireIntentEvents` =
67–94, `<HandleWebViewInitializedAsync>b__158_0` = 141 (current line 141 is the marshalled
`NavigateToString` lambda). The report is therefore usable as a per-line gap map for this file.

| Gate | Target | Measured | Verdict |
| --- | --- | --- | --- |
| Line | >= 80% (issue #136 AC1) | **81.99%** | PASS (marginal) |
| Branch | >= 75% (`.claude/rules/general-unit-test.md`) | **65.63%** | **FAIL** |

**Correction to the brief.** The brief describes this file as "measured 82.0%" and treats it as
compliant. It is compliant on the *line* gate only. Its **branch coverage is 65.63%, nearly ten
points under the 75% floor** — this file is a branch-gate failure, and that is the primary numeric
work item here, not line coverage. epic.md § "Coverage-Target Reconciliation" already warns that
line and branch are independent gates; this file is a second instance of the F8
`EfcHomeController.Timing.cs` pattern.

Branch arithmetic (reconciles exactly): the Cobertura class branch-rate counts conditions in the
per-method `<lines>` blocks **and** the class-level `<lines>` block, so branch points inside a
non-lambda method are double-counted. Per-method conditions = 12 (8 covered); class-level
conditions = 20 (13 covered); total 32 (21 covered) = 0.65625. Consequence for planning: **covering
one `if (_expanded)` branch raises the count by 2, not 1.** The four `_expanded` guards alone move
the file from 21/32 (65.6%) to 29/32 (90.6%).

F1's harness run on this child's own branch remains the acceptance authority. These numbers are
planning inputs only.

---

## 1. Member inventory

The file contains no fields, properties, constructors, events, or nested types — 13 methods only,
all `internal`, all on the `internal partial class QfcItemController`.

| # | Member | Lines | Accessibility | `[ExcludeFromCodeCoverage]` |
| --- | --- | --- | --- | --- |
| 1 | `WireEvents()` | 28–32 | `internal void` | No |
| 2 | `WireControlTreeEvents()` | 35–64 | `internal void` | No |
| 3 | `WireIntentEvents()` | 66–94 | `internal void` | No |
| 4 | `WebView2Control_CoreWebView2InitializationCompleted(object, CoreWebView2InitializationCompletedEventArgs)` | 100–106 | `internal async void` | **Yes — attribute at line 99** |
| 5 | `HandleWebViewInitializedAsync(bool, System.Exception)` | 108–155 | `internal async Task` | No |
| 6 | `RegisterFocusActions()` | 157–212 | `internal void` | No |
| 7 | `RegisterFocusAsyncActions()` | 214–304 | `internal void` | No |
| 8 | `RegisterExpandedActions()` | 306–318 | `internal void` | No |
| 9 | `RegisterExpandedAsyncActions()` | 320–332 | `internal void` | No |
| 10 | `UnregisterFocusActions()` | 334–353 | `internal void` | No |
| 11 | `UnregisterFocusAsyncActions()` | 355–377 | `internal void` | No |
| 12 | `UnregisterExpandedActions()` | 379–383 | `internal void` | No |
| 13 | `UnregisterExpandedAsyncActions()` | 385–389 | `internal void` | No |

The compiler additionally emits **33 closure methods** (`<RegisterFocusActions>b__159_0` …
`<RegisterExpandedAsyncActions>b__162_1`, plus `<WireControlTreeEvents>b__155_0` and
`<HandleWebViewInitializedAsync>b__158_0`). Every one of these is in the coverage denominator and
28 of them are at 0%. **They are the single largest line-coverage gap in the file** and they are the
registered keyboard-action bodies — that is, the behaviour a user actually invokes.

Exemption note: the sole attribute (line 99) is **method-level**, so member #4's 3 statements are
outside the denominator today. Removing it would add ~4 coverable lines of which the
`await HandleWebViewInitializedAsync(e.IsSuccess, e.InitializationException)` line is unreachable
without constructing a `CoreWebView2InitializationCompletedEventArgs` — a sealed WebView2 SDK type
with no public constructor. **Recommendation: retain this exemption.** It is a genuine
bucket-(iii) `async void` framework-signature shell whose entire substantive body is already
100 % covered through `HandleWebViewInitializedAsync`. Removing it would lower measured coverage
with no behavioural benefit, which is exactly the trade epic.md § "Measured Coverage Baseline"
warns about. This is the *only* exemption in the file and it does not carry a testable seam, so the
#227 maintainer precedent (no blanket exemptions; per-member barrier analysis) is satisfied by the
analysis above rather than defeated by it.

---

## 2. What is already covered

Existing test file: `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs`
(375 lines, class `QfcItemController_EventWiringTests`), plus
`QuickFiler.Test/Controllers/QfcItemController.SeamCoreTests.cs` for the WebView core, and the
shared `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` harness
(`HarnessController`, `QfcItemControllerTestSupport.SetField/GetField/InvokeNonPublic`,
`EnsureSynchronizationContext`, `BuildSyncDispatcher`, `InjectThemes`, `StartRunningDispatcher`).

| Member | Status | Covering test(s) |
| --- | --- | --- |
| `WireEvents()` | **COVERED** (100 % line, 100 % branch) | `WireEvents_WithHeadlessItemViewer_WiresBothControlTreeAndIntentEvents` (EventWiringTests.cs:320) |
| `WireControlTreeEvents()` | **COVERED** (100 % / 100 %) | `WireControlTreeEvents_WithHeadlessItemViewer_WiresKeyboardAndMouseHandlers` (EventWiringTests.cs:229) |
| `WireIntentEvents()` | **COVERED** (100 % / n/a — no branches) | indirectly via `WireEvents_…` (EventWiringTests.cs:320); asserted through `ConversationMenuItem.Checked = true` → `_optionConversationChecked` |
| `WebView2Control_CoreWebView2InitializationCompleted` | EXEMPT — not measured | — |
| `HandleWebViewInitializedAsync` | **PARTIALLY COVERED** | `HandleWebViewInitializedAsync_WhenSuccessful_NavigatesToItemHtml` (SeamCoreTests.cs:167), `…_WhenInvokeRequired_MarshalsNavigate` (:187), `…_WhenFailure_SwallowsExceptionAndDoesNotInitialize` (:208). Uncovered: 125, 126, 128–133, 135–137 |
| `RegisterFocusActions()` | **PARTIALLY COVERED** (93.5 % / 50 %) | `RegisterFocusActions_RegistersExpectedSyncKeyAndCharActions` (:142). Uncovered: 209–211 and 11 of its 13 lambda bodies |
| `RegisterFocusAsyncActions()` | **PARTIALLY COVERED** (95.4 % / 50 %) | `RegisterFocusAsyncActions_RegistersExpectedCharActions` (:56). Uncovered: 301–303 and all 14 lambda bodies |
| `RegisterExpandedActions()` | **PARTIALLY COVERED** (100 % method / lambdas 0 %) | `RegisterExpandedActions_RegistersBAndDWithoutInvokingLambdaBodies` (:185). Uncovered: 311, 316 |
| `RegisterExpandedAsyncActions()` | **PARTIALLY COVERED** (100 % method / lambdas 0 %) | `RegisterExpandedAsyncActions_RegistersBAndD` (:89). Uncovered: 325, 330 |
| `UnregisterFocusActions()` | **PARTIALLY COVERED** (84.2 % / 50 %) | `UnregisterFocusActions_AfterRegister_RemovesSyncKeyAndCharActions` (:160). Uncovered: 350–352 |
| `UnregisterFocusAsyncActions()` | **PARTIALLY COVERED** (85 % / 50 %) | `UnregisterFocusAsyncActions_AfterRegister_RemovesCharActions` (:72). Uncovered: 374–376 |
| `UnregisterExpandedActions()` | **COVERED** (100 % / 100 %) | `UnregisterExpandedActions_AfterRegister_RemovesSyncBAndD` (:200) |
| `UnregisterExpandedAsyncActions()` | **COVERED** (100 % / 100 %) | `UnregisterExpandedAsyncActions_AfterRegister_RemovesBAndD` (:104) |

**Do not re-author any of the above.** In particular the two headless-real-`ItemViewer` wiring
tests already close what earlier cycles treated as the hard barrier; the remaining gap is entirely
in *branches and lambda bodies*, not in the wiring act itself.

---

## 3. The gap list

### 3.1 Uncovered lines (exact, from the committed Cobertura class-level `<lines>` block)

| Cluster | Uncovered lines | Count | What reaches them |
| --- | --- | --- | --- |
| `HandleWebViewInitializedAsync` delay/timeout loop | 125, 126, 128, 129, 130, 131, 132, 133, 135, 136, 137 | 11 | requires `ItemHelper == null` at entry **and** a delay seam |
| `RegisterFocusActions` sync lambda bodies | 162, 167, 172, 177, 182, 187, 195, 200, 206 | 9 | invoke the registered delegate |
| `RegisterFocusActions` expanded branch | 209, 210, 211 | 3 | `_expanded = true` |
| `RegisterFocusAsyncActions` async lambda bodies | 224, 230–233, 238, 243, 248, 253, 258, 263, 269–272, 277, 282, 288, 293, 298 | 20 | invoke the registered delegate |
| `RegisterFocusAsyncActions` expanded branch | 301, 302, 303 | 3 | `_expanded = true` |
| `RegisterExpandedActions` lambda bodies | 311, 316 | 2 | concrete `ItemViewer` controls (see §5) |
| `RegisterExpandedAsyncActions` lambda bodies | 325, 330 | 2 | concrete `ItemViewer` controls (see §5) |
| `UnregisterFocusActions` expanded branch | 350, 351, 352 | 3 | `_expanded = true` |
| `UnregisterFocusAsyncActions` expanded branch | 374, 375, 376 | 3 | `_expanded = true` |
| **Total** | | **56** | |

Note lines 189, 190, 191 report `hits="1"` at class level even though their lambdas
(`b__159_6/7/8`) are at 0 %: the `Add(...)` statement and its lambda share one physical line, and
the harness must take **max hits per line** (epic.md § "Two harness correctness requirements").
Three lambda bodies are therefore already masked and invisible to the line metric.

### 3.2 Uncovered branches — the binding gate

| Line | Construct | Coverage | Missing direction |
| --- | --- | --- | --- |
| 124 | `while (ItemHelper is null)` | 1/2 | loop-entry (true) |
| 128 | `if (totalDelay > maxDelay)` | 0/2 | both |
| 208 | `if (_expanded)` in `RegisterFocusActions` | 1/2 | true |
| 300 | `if (_expanded)` in `RegisterFocusAsyncActions` | 1/2 | true |
| 349 | `if (_expanded)` in `UnregisterFocusActions` | 1/2 | true |
| 373 | `if (_expanded)` in `UnregisterFocusAsyncActions` | 1/2 | true |

Lines 53, 59, 115, 139 are already 2/2.

**Branch-heavy members:** `HandleWebViewInitializedAsync` (2 of the 6 deficient branch points, and
the only branch point at 0/2) and the four `_expanded`-guarded registration members. No other
member in this file has a branch.

**Projected result of the plan in §8:** the four `_expanded` tests move branch coverage to 29/32
(90.6 %) and add 12 lines; the WebView delay-seam tests close the remaining 3/32 and add 11 lines;
the lambda-invocation tests add up to 33 lines. Projected end state: **line ≈ 100 %, branch = 100 %**
(barring the four concrete-control lambdas at 311/316/325/330, see §5.3).

---

## 4. Event subscription lifecycle — priority analysis

### 4.1 Subscribe map — `WireIntentEvents()` (EventWiring.cs:66–94)

All 14 subscriptions target `_itemViewer`, typed as the `IItemViewer` interface
(`QuickFiler/Viewers/IItemViewer.cs`).

| # | Event (declaration in `IItemViewer.cs`) | Subscribed at | Handler | Unsubscribed anywhere? |
| --- | --- | --- | --- | --- |
| 1 | `ConversationModeChanged` (:65) | 68 | `CbxConversation_CheckedChanged` | **No** |
| 2 | `FlagTaskClicked` (:60) | 69 | `BtnFlagTask_Click` | **No** |
| 3 | `PopOutClicked` (:61) | 70 | `BtnPopOut_Click` | **No** |
| 4 | `DeleteItemClicked` (:59) | 71 | `BtnDelItem_Click` | **No** |
| 5 | `ReplyClicked` (:62) | 72 | `BtnReply_Click` | **No** |
| 6 | `ReplyAllClicked` (:63) | 73 | `BtnReplyAll_Click` | **No** |
| 7 | `ForwardClicked` (:64) | 74 | `BtnForward_Click` | **No** |
| 8 | `BodyDoubleClick` (:53) | 75 | `TxtboxBody_DoubleClick` | **No** |
| 9 | `SearchTextChanged` (:98) | 77 | `TextBoxSearch_TextChanged` | **No** |
| 10 | `FolderKeyDown` (:96) | 81 | `_kbdHandler.CboFolders_KeyDownAsync` | **No** |
| 11 | `FolderSelectionChanged` (:95) | 86 | `CboFolders_SelectedIndexChanged` | **No** |
| 12 | `WebViewInitializationCompleted` (:108) | 87 | `WebView2Control_CoreWebView2InitializationCompleted` | **No** |
| 13 | `ConversationItemSelectionChanged` (:112) | 89 | `TopicThread_ItemSelectionChanged` | **No** |
| 14 | `SearchKeyDown` (:99) | 91 | `TextBoxSearch_KeyDown` | **No** |
| 15 | `EmailCopyChanged` (:67) | 92 | `CbxEmailCopy_CheckedChanged` | **No** |
| 16 | `AttachmentsChanged` (:69) | 93 | `CbxAttachments_CheckedChanged` | **No** |

(Sixteen `+=` statements; the brief's "14" undercounts.) `PicturesChanged` (`IItemViewer.cs:71`) is
declared but **never subscribed** — a declared-unused event, not a leak.

### 4.2 Subscribe map — `WireControlTreeEvents()` (EventWiring.cs:35–64)

| Target | Event | Subscribed at | Handler | Unsubscribed? |
| --- | --- | --- | --- | --- |
| every control returned by `((ItemViewer)_itemViewer).ForAllControls(...)` except `L0vhBreadcrumb_WebView2` | `PreviewKeyDown` | 40 | `_kbdHandler.KeyboardHandler_PreviewKeyDownAsync` | **No** |
| same set | `KeyDown` | 44 | `_kbdHandler.KeyboardHandler_KeyDownAsync` | **No** |
| every `Button` in `Buttons` | `MouseEnter` | 55 | `Button_MouseEnter` | **No** |
| every `Button` in `Buttons` | `MouseLeave` | 56 | `Button_MouseLeave` | **No** |
| every `ToolStripMenuItem` in `_itemViewer.MenuItems` | `MouseEnter` | 61 | `MenuItem_MouseEnter` | **No** |
| every `ToolStripMenuItem` in `_itemViewer.MenuItems` | `MouseLeave` | 62 | `MenuItem_MouseLeave` | **No** |

### 4.3 Findings

**F-1 (Medium) — There is no unwiring path at all.** Grepping `-=` across the whole
`QfcItemController.*` family returns exactly three hits, all for one unrelated event:
`_breadcrumbViewer.BreadcrumbUnhandledArrow -= OnBreadcrumbUnhandledArrow` at
`QfcItemController.ViewerSetup.cs:152`, `:155`, and `:399`. **No `UnwireEvents()`,
`UnwireIntentEvents()`, or `UnwireControlTreeEvents()` method exists.** `Cleanup()`
(`QfcItemController.ViewerSetup.cs:392–421`) nulls 17 collaborator fields — including
`_itemViewer` (:403, :419), `_kbdHandler` (:416), `_themes` (:410) and `ItemHelper` (:418) — while
every one of the 22 subscriptions above remains attached. The pairing is **entirely asymmetric**.
This is the same defect class epic.md attributes to sibling F4's leaked `BeforeItemMove`
subscription, at 22× the scale.

Two concrete consequences, both testable:

- *Retention.* The `ItemViewer` holds strong delegate references back to the controller, so the
  controller (and, through it, the `MailItem` COM RCW in `_mailItem`) outlives `Cleanup()` for as
  long as the viewer object does.
- *Post-cleanup handler execution → `NullReferenceException`.* `Button_MouseLeave`
  (`QfcItemController.EventHandlers.cs:147–157`) and `MenuItem_MouseLeave` (:159–162) read
  `_themes[_activeTheme]`; `_themes` is null after `Cleanup()`. Removing a control from a
  `TableLayoutPanel` — which `QfcCollectionController.RemoveControls()` does at
  `QfcCollectionController.cs:999`, *before* calling `Cleanup()` at `:1003` — can raise
  `MouseLeave`. The window is small but real, and the ordering (remove rows, then Cleanup) is what
  currently keeps it mostly closed.

**F-2 (Medium) — the `WebViewInitializationCompleted` subscription is the highest-risk single
leak.** WebView2 core initialisation is asynchronous and fire-and-forget
(`QfcItemController.Initialization.cs:193` `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync)`).
If it completes after `Cleanup()`, handler #12 runs `async void` and reaches
`HandleWebViewInitializedAsync`, which dereferences `_itemViewer` at line 139. The `try/catch` at
115/148 swallows the resulting `NullReferenceException` into `logger.Error`, so the failure is
silent-but-logged rather than fatal. That is why this leak has not surfaced as a crash.

**F-3 (Low) — double-subscription on re-entry is *not* protected, but is currently unreachable.**
Neither `WireEvents()` nor either of its two callees guards against being called twice. Calling
`WireEvents()` twice on the same viewer would attach every handler twice, so each user gesture
would fire its handler twice. Every production call site
(`QfcItemController.Initialization.cs:190, 253, 285, 319`) is on a mutually-exclusive
initialisation path, so this is presently unreachable — but nothing in the code enforces it. A
regression test pinning "double `WireEvents()` produces double dispatch" documents the invariant
that a future idempotence fix would change.

**F-4 (Medium) — the keyboard-action registry *does* hard-fail on double registration.** Unlike
the .NET events above, `KbdActions<TKey,UClass,VDelegate>.Add(...)`
(`QuickFiler/Controllers/KbdActions.cs:90–104`) **throws `ArgumentException`** when the same
`(sourceId, key)` pair is already present:

```csharp
if (_list.Any(x => x.SourceId == sourceId && StoredKeyEquals(x.Key, key)))
{
    string message = $"Cannot add key because it already exists. Key {key} SourceId {sourceId}";
    logger.Error(message);
    throw new ArgumentException(message);
}
```

So `RegisterFocusActions()` / `RegisterFocusAsyncActions()` are **not idempotent — they throw on
re-entry**. Production protects this with the `_activeUI` guard in
`QfcItemController.FocusAndTheme.cs:32/48` and `:88/104`, and with the `_expanded` guard in
`QfcItemController.Navigation.cs:149/161`, not with anything in this file. That guard is the
load-bearing state-transition invariant of the whole cluster and is currently untested.
Correspondingly, `KbdActions.Remove` (`KbdActions.cs:123–135`) returns `bool` and **every one of
the 30 call sites in this file discards it** — an unregister that matches nothing fails silently.

**F-5 (Low) — `Cleanup()` nulls `ItemHelper` before any unregister can run.** All 30
`Register*`/`Unregister*` calls key on `ItemHelper.EntryId` (lines 160–206, 222–298, 309–316,
322–330, 336–348, 359–372, 381–382, 387–388). `Cleanup()` sets `ItemHelper = null`
(`ViewerSetup.cs:418`), so any `Unregister*Actions()` after `Cleanup()` throws
`NullReferenceException` on the first `ItemHelper.EntryId` read. Combined with F-1 this means the
keyboard registrations for a cleaned-up item can never be removed.

**F-6 — the "live COM predicate" defect class does NOT reproduce here.** epic.md asks children to
look for F4's "handler predicate reading live COM instead of a cached ID". Verified: the identity
used throughout this file is `MailItemHelper.EntryId`, a plain cached property with a backing field
(settable in tests — `QfcItemController.EventWiringTests.cs:36–37` does
`helper.EntryId = entryId`), inherited from the `MinedMailInfo` family
(`UtilitiesCS/EmailIntelligence/EmailParsingSorting/MinedMailInfo.cs:75–80`). No `Microsoft.Office.Interop.Outlook`
member is dereferenced on any register/unregister path. **Report this as checked-and-clear**, not
as a finding.

**F-7 — ordering dependency: `ResolveControlGroups` must precede `WireControlTreeEvents`.**
`Buttons` (`QfcItemController.cs:95–100`) has a private setter and is null until
`ResolveControlGroups` runs; `WireControlTreeEvents` line 53 iterates it. The existing test
documents this as an Arrange prerequisite (`EventWiringTests.cs:253–257`) but no test asserts the
failure mode. Wiring order inside `WireEvents()` is control-tree first, intent second (lines 30–31);
no dependency runs the other way, so the ordering is a one-way precondition rather than a coupling.

**F-8 — `foreach (ToolStripMenuItem menuItem in _itemViewer.MenuItems)` (line 59) is an implicit
downcast.** `IItemViewer.MenuItems` is declared `List<Component>` (`IItemViewer.cs:34`), so a
non-`ToolStripMenuItem` component in that list produces an `InvalidCastException` at wiring time,
after the key handlers and button handlers have already been attached — a partial-wiring state with
no rollback. Reachable and assertable with `Mock<IItemViewer>`.

---

## 5. Seam analysis

### 5.1 Members needing no new seam

The four `_expanded` branches (208, 300, 349, 373) and all 33 lambda bodies are reachable **today**
with the existing harness: `HarnessController` + `QfcItemControllerTestSupport.SetField` for
`_expanded`, `_kbdHandler`, `_itemViewer`, `_parent`, `_explorerController`, `_uiDispatcher`, plus
the real `KbdActions<>` stub pattern already established in `EventWiringTests.cs:41–53` and
`:127–139`. **Zero production change required for 45 of the 56 uncovered lines and 4 of the 6
deficient branch points.** This is the highest-yield, lowest-risk work in the file.

Individual lambda bodies need only their own collaborator mocked:

| Lambda line | Body | Collaborator to mock |
| --- | --- | --- |
| 162, 167, 177 | `ToggleConversationCheckbox(...)` | `_uiDispatcher` (`BuildSyncDispatcher`) + `Mock<IItemViewer>` |
| 172, 238 | `_explorerController.OpenQFItem(Mail)` | `Mock<IQfcExplorerController>` |
| 182 | `ToggleSaveAttachments()` | `Mock<IItemViewer>` |
| 187 | `ToggleSaveCopyOfMail()` | `Mock<IItemViewer>` |
| 189 | `ToggleExpansion()` | override the virtual `ToggleExpansion(ToggleState)` (spy pattern already in `NavigationTests.cs:139–157`) |
| 190, 269–272 | `JumpToSearchTextbox()` | `Mock<IQfcKeyboardHandler>` + `Mock<IItemViewer>` |
| 191 | `FlagAsTask()` | `_flagTasksFactory` sentinel (pattern at `EventHandlersTests.cs:264–304`) |
| 195, 282 | `_parent.PopOutControlGroup[Async](ItemNumber)` | `Mock<IQfcCollectionController>` |
| 200, 288 | `_parent.RemoveSpecificControlGroup[Async](ItemNumber)` | `Mock<IQfcCollectionController>` |
| 202, 293 | `MarkItemForDeletion[Async]()` (`MailActions.cs:202/211`) | `Mock<IItemViewer>` |
| 206, 298 | `JumpToFolderDropDown[Async]()` | `Mock<IQfcKeyboardHandler>` + `Mock<IItemViewer>` / `_uiDispatcher` |
| 224, 263 | `ToggleExpansionAsync()` | virtual-override spy |
| 230–233 | `ToggleConversationCheckbox(); return Task.CompletedTask;` | `_uiDispatcher` + `Mock<IItemViewer>` |
| 243, 248, 253, 258, 277 | `KbdExecuteAsync(...)` | `Mock<IFilerHomeController>` exposing `KeyboardHandler` (pattern at `NavigationTests.cs:35–41`) |

### 5.2 The one real seam gap — wall-clock delay in `HandleWebViewInitializedAsync`

Lines 121–137 are a growing-backoff spin waiting for `ItemHelper` to become non-null:

```csharp
var newDelay = 100 * ++delayCount;
if (totalDelay > maxDelay) { throw new TimeoutException(...); }
await Task.Delay(newDelay);
totalDelay += newDelay;
```

Reaching the timeout at line 130 requires 14 iterations totalling **10 500 ms of real wall-clock
time**. `.claude/rules/general-unit-test.md` § "Determinism Infrastructure" bans `Task.Delay` and
real wall-clock waits in tests, and net481 has no `TimeProvider`/`FakeTimeProvider`. These 11
lines and the 0/2 branch at 128 are therefore **unreachable without a production seam**.

**Minimum seam, per the epic hierarchy (interface > injectable delegate > adapter):** no interface
exists or is warranted for "delay N milliseconds", and an adapter type would be heavier than the
call site. Use an **injectable delegate**, matching the six factory-delegate seams already on this
class (`QfcItemController.cs:66–89`):

```csharp
// QfcItemController.cs, private-fields region
private Func<int, Task> _delayAsync = milliseconds => Task.Delay(milliseconds);
```

and at `QfcItemController.EventWiring.cs:135` replace `await Task.Delay(newDelay);` with
`await _delayAsync(newDelay);`. One production line changed, one field added, no behaviour change
(the default is the current expression), injectable through the existing
`QfcItemControllerTestSupport.SetField` harness with a `_ => Task.CompletedTask` stub. This is a
**one-line, one-field change that converts 11 uncovered lines and the file's only 0/2 branch into
deterministic coverage** — clearly the best value/risk ratio in the file.

Do **not** put the default in `SaveParameters` alongside the other seams unless the field is left
uninitialised; a field initialiser is sufficient here because the delay has no constructor
dependency, and it guarantees no path leaves the seam null (the concern documented in the comment
at `QfcItemController.cs:62–65`).

### 5.3 Residual barrier — the four expanded-action lambdas (311, 316, 325, 330)

These bodies are `JumpToAsync(((ItemViewer)_itemViewer).L0v2h2_WebView2)` and
`JumpToAsync(((ItemViewer)_itemViewer).TopicThread)` — a hard cast to the concrete `ItemViewer` to
reach two Designer-generated control fields. Options:

1. **Headless real `ItemViewer` (recommended).** Already proven in this exact test class:
   `EventWiringTests.cs:229` and `:320` construct `new QuickFiler.ItemViewer()` inside a
   `SynchronizationContext.SetSynchronizationContext(new SynchronizationContext())` try/finally, and
   `JumpToAsync` is already proven to work against a handle-less `Control`
   (`NavigationTests.cs:208`, which notes `Control.Focus()` returns `false` silently with no
   handle). Combining the two proven techniques reaches all four lambdas with **zero production
   change**. Per epic.md § "Shared Design" 3 these must live in a dedicated `*.StaTests.cs` file
   with `[STATestClass]`/`[STATestMethod]` scoping if a real `ItemViewer` is constructed; note the
   two existing tests do *not* currently do so, which is a pre-existing convention gap this child
   should reconcile with F1 rather than propagate silently.
2. **Narrow `IItemViewer` with `Control BreadcrumbWebView { get; }` / `Control TopicThreadControl { get; }`.**
   Cleaner long-term and consistent with the #227 direction of travel, but it changes the
   `IItemViewer` contract — a file owned by **F14** (`Viewers/IItemViewer.cs`, 133 lines). Out of
   F10's assignment.

**Recommendation: option 1.** It is a test-only change, reuses two techniques already proven in this
repository, and avoids a cross-child contract edit.

---

## 6. State-transition invariants

| # | Invariant | Held by | Pinning test (see §8) |
| --- | --- | --- | --- |
| I-1 | `WireEvents()` calls `WireControlTreeEvents()` then `WireIntentEvents()`, in that order | lines 30–31 | already pinned (`WireEvents_…`, EventWiringTests.cs:320) |
| I-2 | `WireControlTreeEvents()` requires `ResolveControlGroups` to have populated `Buttons`; calling it first throws `NullReferenceException` | line 53 vs `QfcItemController.cs:95–100` | EW-12 |
| I-3 | `WireIntentEvents()` attaches exactly 16 handlers to `IItemViewer` events and nothing else | lines 68–93 | EW-13 |
| I-4 | Wiring is **not** idempotent: a second `WireEvents()` double-dispatches every intent event | no guard | EW-14 |
| I-5 | `RegisterFocusActions()` is **not** idempotent: a second call throws `ArgumentException` from `KbdActions.Add` | `KbdActions.cs:92–98` | EW-15 |
| I-6 | `RegisterFocusAsyncActions()` — same | `KbdActions.cs:92–98` | EW-16 |
| I-7 | When `_expanded`, `RegisterFocusActions()` also registers `'B'`/`'D'`; when not, it must not | 208–211 | EW-1, and the existing negative case at `:142` |
| I-8 | When `_expanded`, `UnregisterFocusActions()` also removes `'B'`/`'D'` | 349–352 | EW-3 |
| I-9 | Async variants I-7/I-8 hold identically for `KeyActionsAsync`/`CharActionsAsync` | 300–303, 373–376 | EW-2, EW-4 |
| I-10 | Register/unregister key sets are exact inverses. Verified by reading both: sync = `{Right, Left}` + `{O,C,A,M,E,S,T,P,R,X,F}` (11 chars) both ways; async = `{Right}` + `{C,O,M,R,L,W,E,S,T,P,Z,X,F}` (13 chars) both ways; expanded = `{B,D}` both ways. **Symmetric — no leak.** | 336–348 vs 160–206; 359–372 vs 222–298 | EW-17 (round-trip empties the registry) |
| I-11 | `Unregister*` on an empty registry is a silent no-op (`KbdActions.Remove` returns `false`, discarded) | `KbdActions.cs:126–128` | EW-18 |
| I-12 | Every registration is keyed by `ItemHelper.EntryId` as `SourceId`; two controllers with different `EntryId` values coexist in one shared `KbdActions` without collision | 160 etc. | EW-19 |
| I-13 | If `ItemHelper` is replaced (different `EntryId`) between register and unregister, the original registrations are orphaned and remain | `Remove` matches on `SourceId` | EW-20 (characterisation) |
| I-14 | `HandleWebViewInitializedAsync` sets `_isWebViewerInitialized = true` **before** the `ItemHelper` wait, so a timeout leaves the flag set | 119 vs 130 | EW-9 |
| I-15 | On `isSuccess == false` the method must not set `_isWebViewerInitialized` and must not throw out of the method | 115–117, 148–154 | already pinned (SeamCoreTests.cs:208) |
| I-16 | Dispose-before-setup: after `Cleanup()`, `Unregister*Actions()` throws `NullReferenceException` (`ItemHelper` is null) and the subscriptions from §4.1/§4.2 remain attached | `ViewerSetup.cs:403–420` | EW-21, EW-22 (characterisation of the F-1/F-5 defects) |

---

## 7. Determinism requirements

| Concern | Location | Disposition |
| --- | --- | --- |
| **`await Task.Delay(newDelay)`** — real wall-clock backoff up to 10 500 ms | line 135 | **In-scope banned-API-adjacent finding in production code this child touches.** The rule text bans `Task.Delay` *in tests*; production code that forces a test to wait 10.5 s violates the determinism intent just as squarely. Resolve with the `_delayAsync` seam (§5.2). No test may call the un-seamed path. |
| `DateTime.Now` / `DateTime.UtcNow` / `Random` | none | Verified absent from this file. |
| Thread-pool / UI-thread marshalling | line 139–142 `_itemViewer.InvokeRequired` → `_itemViewer.Invoke(...)` | Already seamed behind `IItemViewer`; both directions covered (branch 139 is 2/2) via `Mock<IItemViewer>` with a `Callback<Delegate>(d => d.DynamicInvoke())` (SeamCoreTests.cs:193–196). No change needed. |
| `async void` | line 100 (`WebView2Control_CoreWebView2InitializationCompleted`) | Exempt shell; the awaited core is a `Task` and is directly awaitable in tests. No fire-and-forget in the covered surface. |
| Ambient `SynchronizationContext` mutation | not in this file (it is in `EventHandlers.cs`) | Tests touching `WireIntentEvents` must still restore the ambient context, as the two existing wiring tests do in `finally` (EventWiringTests.cs:305–308, 369–371). |
| STA / real `ItemViewer` construction | `EventWiringTests.cs:236`, `:327`, and any test added for §5.3 | Must move to a dedicated `*.StaTests.cs` per epic.md § "Shared Design" 3. |

No `Thread.Sleep`, `Task.Delay`, or wall-clock wait may appear in any test added by this child.

---

## 8. Proposed test case list

Each row is one atomic task. Fixtures: **A** = `HarnessController` + `SetField`;
**B** = `BuildKbdHandlerStub()` / `BuildSyncKbdHandlerStub()` (already in `EventWiringTests.cs`);
**C** = `Mock<IItemViewer>`; **D** = `BuildSyncDispatcher()`; **E** = headless real `ItemViewer`
under `SynchronizationContext` guard (STA file).

| ID | Target member | Scenario | Fixture | Closes |
| --- | --- | --- | --- | --- |
| EW-1 | `RegisterFocusActions` | ordering / edge — `_expanded = true` also registers `'B'`,`'D'` | A+B | lines 209–211, branch 208 (×2) |
| EW-2 | `RegisterFocusAsyncActions` | ordering / edge — `_expanded = true` also registers async `'B'`,`'D'` | A+B | lines 301–303, branch 300 (×2) |
| EW-3 | `UnregisterFocusActions` | dispose — `_expanded = true` also removes `'B'`,`'D'` | A+B | lines 350–352, branch 349 (×2) |
| EW-4 | `UnregisterFocusAsyncActions` | dispose — `_expanded = true` also removes async `'B'`,`'D'` | A+B | lines 374–376, branch 373 (×2) |
| EW-5 | `HandleWebViewInitializedAsync` | positive — `ItemHelper` null then supplied on the 2nd poll; navigates once | A+C+`_delayAsync` stub | lines 121–126, 135–137, branch 124 |
| EW-6 | `HandleWebViewInitializedAsync` | error — `ItemHelper` never supplied; loop exits via `TimeoutException`, swallowed and logged | A+C+`_delayAsync` stub | lines 128–133, branch 128 (both) |
| EW-7 | `HandleWebViewInitializedAsync` | edge — the seamed delay is invoked with the growing 100·n backoff sequence | A+`_delayAsync` recording stub | asserts the seam preserves the backoff |
| EW-8 | `HandleWebViewInitializedAsync` | error — `isSuccess == false` with `initException == null` (see D-4) | A | characterises the `throw null` path |
| EW-9 | `HandleWebViewInitializedAsync` | state — `_isWebViewerInitialized` is set before the wait and stays set through a timeout | A+C | I-14 |
| EW-10 | `WireIntentEvents` | negative — a mock `IItemViewer` raising each of the 16 events reaches the expected handler (one test per event, or one table-driven test) | A+C | I-3, guards the wiring map |
| EW-11 | `WireControlTreeEvents` | error — non-`ToolStripMenuItem` in `MenuItems` throws `InvalidCastException` after key/button wiring has already run | A+C | F-8, partial-wiring characterisation |
| EW-12 | `WireControlTreeEvents` | error — called before `ResolveControlGroups`; `Buttons` null → `NullReferenceException` | A+E | I-2 |
| EW-13 | `WireIntentEvents` | positive — exactly 16 subscriptions, verified by raising every `IItemViewer` event on the mock and counting handler entries | A+C | I-3 |
| EW-14 | `WireEvents` | re-entrancy — calling twice double-dispatches a single intent event | A+C | I-4 |
| EW-15 | `RegisterFocusActions` | re-entrancy — second call throws `ArgumentException` naming the duplicate key and `SourceId` | A+B | I-5 |
| EW-16 | `RegisterFocusAsyncActions` | re-entrancy — same | A+B | I-6 |
| EW-17 | `RegisterFocusActions` + `UnregisterFocusActions` | round-trip — register then unregister leaves the registry with zero entries for that `SourceId` (all 13 keys, not the current 4-key sample) | A+B | I-10 |
| EW-18 | `UnregisterFocusActions` / `…AsyncActions` | negative — unregister with nothing registered is a silent no-op, does not throw | A+B | I-11 |
| EW-19 | `RegisterFocusActions` | edge — two controllers with distinct `ItemHelper.EntryId` register into one shared `KbdActions` without collision | A+B | I-12 |
| EW-20 | `UnregisterFocusActions` | edge — `ItemHelper.EntryId` changed after registration; original entries survive (characterisation of F-5/I-13) | A+B | I-13 |
| EW-21 | `Cleanup` → `UnregisterFocusActions` | dispose-before-setup — after `Cleanup()`, unregister throws `NullReferenceException` | A | I-16, F-5 |
| EW-22 | `WireIntentEvents` → `Cleanup` | dispose — after `Cleanup()` the mock viewer still holds all 16 handlers, and raising `ConversationModeChanged` reaches a controller with a null `_itemViewer` | A+C | I-16, F-1 |
| EW-23..EW-35 | 13 sync lambda bodies (162, 167, 172, 177, 182, 187, 189, 190, 191, 195, 200, 202, 206) | positive — register, then invoke the stored delegate and assert the single collaborator call | A+B+C+D (+ per-row mock from §5.1) | 9 uncovered lines + 3 masked |
| EW-36..EW-49 | 14 async lambda bodies (224, 230–233, 238, 243, 248, 253, 258, 263, 269–272, 277, 282, 288, 293, 298) | positive — same, awaited | A+B+C+D | 20 uncovered lines |
| EW-50 | `RegisterExpandedActions` `'B'` lambda (311) | positive — invoking it focuses the breadcrumb WebView2 control and toggles the keyboard dialog | E (STA file) | line 311 |
| EW-51 | `RegisterExpandedActions` `'D'` lambda (316) | positive — same for `TopicThread` | E (STA file) | line 316 |
| EW-52 | `RegisterExpandedAsyncActions` `'B'` lambda (325) | positive | E (STA file) | line 325 |
| EW-53 | `RegisterExpandedAsyncActions` `'D'` lambda (330) | positive | E (STA file) | line 330 |

**Minimum set to pass both gates: EW-1 … EW-4 plus EW-5 and EW-6** — six tests, one production
field and one production line changed. That alone takes branch from 65.6 % to ~100 % and line from
82.0 % to ~89 %. EW-23…EW-53 are the remainder needed to approach 100 % line and are the tests that
actually assert user-visible keyboard behaviour; they should not be dropped merely because the
80 % gate is already met.

**Test-file placement.** EW-1 … EW-49 extend
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` (currently 375 lines) — this
will breach the 500-line test-file limit. Plan a split into
`QfcItemController.EventWiringTests.cs` (registration membership + lifecycle invariants) and a new
`QfcItemController.EventWiringActionsTests.cs` (lambda-body invocation). EW-50 … EW-53, and the
migration of the two existing real-`ItemViewer` tests, go to a new
`QfcItemController.EventWiringStaTests.cs`. Test projects are SDK-style? — verify: if
`QuickFiler.Test.csproj` uses explicit `<Compile Include=...>` (as `QuickFiler.csproj` does), each
new test file needs its own entry.

---

## 9. File-size and creation impact

- `QfcItemController.EventWiring.cs` is **391 lines**. The only production edit proposed is
  replacing `Task.Delay(newDelay)` with `_delayAsync(newDelay)` at line 135 — net zero lines. No
  split needed; the file stays well under 500.
- `QfcItemController.cs` is **323 lines**; adding the `_delayAsync` field costs 1–3 lines
  (including a comment). No split needed.
- **No new production file is required by this file's plan.** Consequently no
  `<Compile Include=...>` entry in `QuickFiler/QuickFiler.csproj` and no ledger row are owed *by
  this file*. Should that change, the obligations are: add the entry to the legacy non-SDK
  `QuickFiler.csproj` (no globbing), **preserve CRLF** — use the Edit tool or
  `perl -0777` with explicit `\r\n`, never git-bash `sed -i`, which strips CRLF and produces a
  whole-file diff guaranteed to conflict at fan-in — keep the hunk minimal and adjacent, and append
  a ledger row classified `testable` at **>= 90 %** per epic.md § "Mid-Wave File Creation".
- New **test** files (see §8) are not in the coverage denominator and need no ledger row, but do
  need csproj entries if `QuickFiler.Test.csproj` is non-globbing.

---

## 10. Sibling boundaries — do not edit

| Artefact | Owner | Dependency from this file | Rule |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/KeyboardHandler.cs` (414 lines, `[ExcludeFromCodeCoverage]`) | **F3 (#430)** | `WireControlTreeEvents` binds `_kbdHandler.KeyboardHandler_PreviewKeyDownAsync` (line 41) and `KeyboardHandler_KeyDownAsync` (line 45); `WireIntentEvents` binds `_kbdHandler.CboFolders_KeyDownAsync` (line 82). All three are consumed **through `IQfcKeyboardHandler`** (`QuickFiler/Interfaces/IQfcKeyboardHandler.cs`), never through the concrete type. | **Do not edit.** No change is needed: `Mock<IQfcKeyboardHandler>` already suffices (proven at `EventWiringTests.cs:238`). Record as an interface-level dependency only. |
| `QuickFiler/Controllers/KbdActions.cs`, `KaChar.cs`, `KaKey.cs`, `KaStringAsync.cs` | **F3 (#430)** | The registry semantics this file depends on (throw-on-duplicate `Add` at `KbdActions.cs:90–104`, silent-false `Remove` at `:123–135`) | **Do not edit.** Tests construct real `KbdActions<>` instances (already done at `EventWiringTests.cs:48–49`) and assert against current behaviour. If F3 changes `Add` to be idempotent, EW-15/EW-16 must be updated — flag as a cross-child contract note. |
| `QuickFiler/Helper Classes/ConversationResolver.cs` | **F4 (#434)** | Not referenced from this file. | No contact. |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | **F5** | Not referenced from this file. | No contact. |
| `QuickFiler/Viewers/IItemViewer.cs`, `ItemViewer.cs` | **F14** | All 16 intent subscriptions target `IItemViewer` events; §5.3 option 2 would add two members to it. | **Do not edit.** §5.3 recommends option 1 (headless real viewer) precisely to avoid this. If a future cycle prefers the interface route, raise it as a cross-child contract note to F14. |
| `UtilitiesCS/Properties/AssemblyInfo.cs` | out of every child's assignment | `QuickFiler.Test` has **no** `InternalsVisibleTo` grant from `UtilitiesCS` (epic.md § "Cross-Child Constraints" 2). | **Do not edit.** Nothing in this file's plan requires a `UtilitiesCS` internal; `IUiDispatcher`, `Theme`, and `MailItemHelper` are all public. |

**Cross-child contract notes raised by this file:** none blocking. One conditional: if F3 makes
`KbdActions.Add` idempotent, EW-15/EW-16 change from "throws" to "no-op".

---

## Latent defects for promotion

Per epic.md § "Latent Defect Promotion", these must be promoted to GitHub issues via the MCP
promotion lifecycle. **Do not fix them under this child** — the epic's no-behaviour-change NFR
forbids it, and prose in a feature folder is lost at merge.

| ID | Defect | Location | Severity |
| --- | --- | --- | --- |
| D-1 | **No unwiring path exists.** All 22 subscriptions made by `WireIntentEvents()` and `WireControlTreeEvents()` survive `Cleanup()`, which unsubscribes only `BreadcrumbUnhandledArrow`. Causes controller/`MailItem`-RCW retention and a `NullReferenceException` window if any handler fires after `Cleanup()` (`Button_MouseLeave`/`MenuItem_MouseLeave` read the nulled `_themes`). Same defect class as F4's leaked `BeforeItemMove`. | wire: `EventWiring.cs:40–62`, `:68–93`; cleanup: `ViewerSetup.cs:392–421` | **Medium** |
| D-2 | **`WebViewInitializationCompleted` leak with an async continuation.** WebView2 core init is fire-and-forget (`Initialization.cs:193`); a completion arriving after `Cleanup()` runs `HandleWebViewInitializedAsync` against a null `_itemViewer` (line 139). The `catch` at 148 converts it to a log line, so the fault is silent. | `EventWiring.cs:87–88`, `:139` | **Medium** |
| D-3 | **`throw (initException)` can throw `null`.** `CoreWebView2InitializationCompletedEventArgs.InitializationException` is not guaranteed non-null when `IsSuccess` is false; `throw null` raises `NullReferenceException`, and the bare `throw (x)` also resets the stack trace of a captured exception (should be `ExceptionDispatchInfo.Capture(x).Throw()`). Net effect is a misleading `logger.Error` message. | `EventWiring.cs:117` | **Low–Medium** |
| D-4 | **Timeout ceiling is wrong and the message misreports it.** `if (totalDelay > maxDelay)` is evaluated *before* `totalDelay += newDelay`, so the loop exits after 14 iterations totalling 10 500 ms while the message says "10 seconds" via `Math.Round(maxDelay/1000, 1)`. | `EventWiring.cs:128–134` | **Low** |
| D-5 | **`KbdActions.Remove`'s `bool` result is discarded at all 30 call sites in this file.** A failed unregister — e.g. after `ItemHelper` is replaced — is silent. | `EventWiring.cs:336–348`, `:359–372`, `:381–382`, `:387–388`; contract at `KbdActions.cs:123–135` | **Low** |
| D-6 | **`Cleanup()` nulls `ItemHelper` before any unregister can run**, so keyboard registrations for a cleaned-up item are permanently unremovable and a post-`Cleanup` `Unregister*Actions()` throws `NullReferenceException`. | `ViewerSetup.cs:418` vs `EventWiring.cs:336` | **Medium** |
| D-7 | **`WireEvents()` is not idempotent and has no guard.** Currently unreachable because all four call sites are on mutually exclusive init paths, but the invariant is unenforced. | `EventWiring.cs:28–32` | **Low** |
| D-8 | **`foreach (ToolStripMenuItem menuItem in _itemViewer.MenuItems)` is an unguarded downcast** over a `List<Component>`; a non-`ToolStripMenuItem` element throws mid-wiring, leaving keyboard and button handlers attached with no rollback. | `EventWiring.cs:59`; declaration at `IItemViewer.cs:34` | **Low** |
| D-9 | **`RegisterFocusActions`/`UnregisterFocusActions` (sync) are dead in production.** Every live call site uses the `…AsyncActions` variants; the sync calls are commented out at `FocusAndTheme.cs:45, 61, 101, 117`. They remain reachable, tested, and in the coverage denominator. Candidate for deletion in a later cycle — not under this epic's no-behaviour-change NFR. | `EventWiring.cs:157–212`, `:334–353`; call sites `FocusAndTheme.cs:45–46, 61–62, 101–102, 117–118` | **Low (dead code)** |

## Checked and clear

- **No live-COM predicate.** All register/unregister keys use the cached `MailItemHelper.EntryId`
  property (backing field in `MinedMailInfo.cs:75–80`), not an Outlook Interop read. F4's
  "predicate reads live COM instead of the cached ID" defect class does **not** reproduce here.
- **Register/unregister key sets are exact inverses** for all three pairs (§6, I-10). No key leaks.
- **No `DateTime.Now`, `DateTime.UtcNow`, `Random`, or `Thread.Sleep`** in this file.
