# Research — qfc-item-controller-defects (primary issue #484; closes #480, #481, #483, #484, #485)

- Date: 2026-08-24T09-45
- Workspace: authored in the isolated agent worktree `<repo-root>/.claude/worktrees/agent-a6aa711c9454af0d0`,
  whose preparation run was terminated mid-flight by a session usage limit. This artifact was salvaged
  from that worktree unmodified except for this provenance line. The file:line citations below were
  re-verified against the tree by the atomic-executor preflight before the plan was approved; see the
  preflight record for the result.
- Scope: bug-fix feature confined to four owned `QfcItemController` partials plus the four matching test files
- Method: source reading only. No build, no test run, no file modification.

---

## 0. Executive summary of corrections to the promoted potentials

The five promoted potential documents are accurate in their diagnosis. Five of their suggested fixes are
wrong or incomplete against what the code actually shows:

| # | Promoted claim | Correction (evidence below) |
|---|---|---|
| #480 | "Confirm against the intended behavior of the caller ... since some caller may have been written to compensate" | There are **zero** production callers of the one-argument overload. All four `QfcCollectionController` call sites use the two-argument overload. The only caller in the repository is one test. Removal of line 170 is unconditionally safe. See §1. |
| #481 | "`QuickFiler/Controllers/QfcItemController.EventWiring.cs` — 25 `+=` subscription operators" | 25 counts two commented-out lines and one arithmetic `+=`. The real figure is **22 event subscriptions** in `EventWiring.cs` (16 intent + 6 control-tree), plus **2** in `ViewerSetup.cs`. Total 24 across all ten partials. See §2. |
| #483 | "marshal ... through the existing UI dispatcher seam" | A dispatcher seam (`_uiDispatcher`, `IUiDispatcher`) does exist, but it does **not** make `MessageBox.Show` testable — the modal call is still a raw WinForms call. A second seam is required. The repo already has the exact pattern (delegate-typed `internal` property with a production default). See §3.2. |
| #483 | "rethrow ... **or return a failure result the caller can act on**" | Returning a failure result is **not available**: `Task MoveMailAsync()` is declared on the public interface `IQfcItemController` (`IQfcItemController.cs:78`) and implemented by the out-of-scope `EfcItemController`. Changing the return type requires writing two forbidden/unowned files. Rethrow is the only in-scope option. See §3.1. |
| #484 | "dispose the timer before nulling the field" | Necessary but **not sufficient**. `Timer.Dispose()` does not abort a callback already executing, and `ApplyReadEmailFormat` (`FocusAndTheme.cs:318-324`) dereferences four fields that `Cleanup()` nulls. A guard in `ApplyReadEmailFormat` (owned file) is required to close the race. See §4.3. |
| #485 | "Replace line 83 with `Uri.TryCreate(...)`... Add a null check" | Correct as a code change, but it leaves the fix **untestable**: the handler is an anonymous lambda registered inside an `[ExcludeFromCodeCoverage]` method that requires a live WebView2 runtime. The guards must be extracted into a pure internal method to be exercisable at all. See §5. |

---

## 1. Issue #480 — `ToggleNavigation(bool)` double toggle

### 1.1 Confirmed defect site

`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:168-179`:

```csharp
public void ToggleNavigation(bool async)
{
    _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(false)));   // 170 — unconditional
    if (async)
    {
        _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(false))); // 173
    }
    else
    {
        _itemViewer.Invoke(new System.Action(() => _itemPositionTips.Toggle(false)));      // 177
    }
}
```

Line 170 always runs; exactly one of 173/177 also runs. Both paths therefore call the flip-semantics
`Toggle(bool)` twice. The sibling overload at `FocusAndTheme.cs:181-195` calls the idempotent
`Toggle(desiredState, false)` once per branch and is correct.

### 1.2 Complete caller enumeration (repository-wide, `*.cs`)

Search: `ToggleNavigation` across all `*.cs`. Results classified by which overload is invoked.

**Callers of the ONE-argument overload `ToggleNavigation(bool)`:**

| Location | Kind | Written to compensate for the no-op? |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:320` | Test (`ToggleNavigation_Synchronous_TogglesPositionTips`) | No. Asserts `Times.AtLeastOnce()` (line 323), which is satisfied by 1 or 2 invocations. It neither depends on nor compensates for the double call. |

That is the **only** call site in the repository. There are **no production callers**.

**Callers of the TWO-argument overload `ToggleNavigation(bool, Enums.ToggleState)` — all production:**

| Location | Call |
|---|---|
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:186` | `ToggleNavigation(async: async, desiredState: Enums.ToggleState.Off)` |
| `QuickFiler/Controllers/QfcCollectionController.cs:1607-1610` | `ToggleNavigation(async: async, desiredState: Enums.ToggleState.Off)` (inside `ToggleOffNavigation`) |
| `QuickFiler/Controllers/QfcCollectionController.cs:1637-1640` | `ToggleNavigation(async: async, desiredState: Enums.ToggleState.On)` (inside `ToggleOnNavigation`) |
| `QuickFiler/Controllers/QfcCollectionController.cs:1907-1910` | `ToggleNavigation(async: true, desiredState: Enums.ToggleState.On)` |
| `QuickFiler/Controllers/QfcCollectionController.cs:1952-1955` | `ToggleNavigation(async: true, desiredState: Enums.ToggleState.On)` |
| `QuickFiler/Controllers/EfcFormController.cs:929, 945` | `_itemController.ToggleNavigation(async, Enums.ToggleState.Off/On)` — targets `EfcItemController`, not this type |

**Non-callers (declarations / unrelated implementations):**

- `QuickFiler/Interfaces/IQfcItemController.cs:89` — interface declaration. The overload cannot be deleted
  without editing this file and `EfcItemController.cs`, neither of which is owned.
- `QuickFiler/Controllers/EfcItemController.cs:958-979` — a **different** implementation with different
  semantics: the tips call is commented out (lines 962-967) and the body toggles `_activeUI` and
  registers/unregisters focus actions. It is not affected by, and must not be aligned with, this fix.
- `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:444` — stub that throws `NotImplementedException`.

### 1.3 Verdict and recommended fix

Removing the unconditional toggle at `FocusAndTheme.cs:170` is safe. No production code path reaches this
overload at all, so the change cannot regress an observable QuickFiler flow. The single test caller does
not depend on the double invocation.

Recommended in-scope change (`FocusAndTheme.cs`, owned):

- Delete line 170.
- The method then dispatches exactly one `Toggle(false)` through `BeginInvoke` (async) or `Invoke` (sync).

Recommended test change (`QfcItemController.FocusAndThemeTests.cs`, owned): tighten the existing assertion
at line 323 from `Times.AtLeastOnce()` to `Times.Once()`, and add the `async: true` branch (currently
untested) with the same exact-count assertion. `BuildExecutingViewer()` (lines 99-115) executes both
`Invoke` and `BeginInvoke` delegates synchronously, so both branches produce an observable, countable
`Toggle(false)` call on the `Mock<IQfcTipsDetails>`.

**Test that would NOT catch the regression:** `QfcItemController.FocusAndThemeTests.cs:310-324`
(`ToggleNavigation_Synchronous_TogglesPositionTips`), assertion at line 323 — `Times.AtLeastOnce()`. It is
green both before and after the fix. This is the exact mask the promoted potential's Detection Note names.

### 1.4 DOWNSTREAM NOTE (feature 464 / issue #463 — EFC controllers)

`EfcItemController.cs:958-979` is a distinct implementation and is not defective in the same way, but it
is worth the downstream owner confirming that `ToggleNavigation(bool)` with its `_activeUI`-flipping
semantics is intentional, given that `ToggleNavigation(bool, ToggleState)` at `:981-994` and
`ToggleNavigationAsync` at `:996` are idempotent-by-state. No action required from this feature.

---

## 2. Issue #481 — no event unwiring path

### 2.1 Complete `+=` / `-=` inventory across `QuickFiler/Controllers/QfcItemController.*.cs`

Search pattern `\+=|-=`, all ten partials. Comment lines (`EventWiring.cs:43`, `:80`) and the arithmetic
`totalDelay += newDelay` (`EventWiring.cs:136`) are excluded from the event counts below.

#### 2.1.1 `QfcItemController.EventWiring.cs` — `WireControlTreeEvents()` (6 subscriptions)

| Line | Event source | Event | Handler form | Classification |
|---|---|---|---|---|
| 40-42 | every `Control` yielded by `((ItemViewer)_itemViewer).ForAllControls(...)` | `PreviewKeyDown` | `new PreviewKeyDownEventHandler(_kbdHandler.KeyboardHandler_PreviewKeyDownAsync)` — method group on `_kbdHandler` | (a) unwireable in owned files |
| 44-46 | same control walk | `KeyDown` | `new KeyEventHandler(_kbdHandler.KeyboardHandler_KeyDownAsync)` | (a) |
| 55 | each `Button` in `Buttons` | `MouseEnter` | `this.Button_MouseEnter` (named private method, `EventHandlers.cs:137`) | (a) |
| 56 | each `Button` in `Buttons` | `MouseLeave` | `this.Button_MouseLeave` (`EventHandlers.cs:147`) | (a) |
| 61 | each `ToolStripMenuItem` in `_itemViewer.MenuItems` | `MouseEnter` | `this.MenuItem_MouseEnter` (`EventHandlers.cs:142`) | (a) |
| 62 | each `ToolStripMenuItem` in `_itemViewer.MenuItems` | `MouseLeave` | `this.MenuItem_MouseLeave` (`EventHandlers.cs:159`) | (a) |

#### 2.1.2 `QfcItemController.EventWiring.cs` — `WireIntentEvents()` (16 subscriptions)

Every source is the interface field `_itemViewer` (`IItemViewer`). Every handler is a **named method** on
the controller or on `_kbdHandler`. No lambdas.

| Line | Event (declared at) | Handler |
|---|---|---|
| 68 | `ConversationModeChanged` (`IItemViewer.cs:65`) | `this.CbxConversation_CheckedChanged` (`EventHandlers.cs:27`) |
| 69 | `FlagTaskClicked` (`:60`) | `this.BtnFlagTask_Click` (`:49`) |
| 70 | `PopOutClicked` (`:61`) | `this.BtnPopOut_Click` (`:61`) |
| 71 | `DeleteItemClicked` (`:59`) | `this.BtnDelItem_Click` (`:72`) |
| 72 | `ReplyClicked` (`:62`) | `this.BtnReply_Click` (`:84`) |
| 73 | `ReplyAllClicked` (`:63`) | `this.BtnReplyAll_Click` (`:98`) |
| 74 | `ForwardClicked` (`:64`) | `this.BtnForward_Click` (`:112`) |
| 75 | `BodyDoubleClick` (`:53`) | `this.TxtboxBody_DoubleClick` (`:126`) |
| 77-79 | `SearchTextChanged` (`:108`) | `new EventHandler(this.TextBoxSearch_TextChanged)` (`:173`) |
| 81-83 | `FolderKeyDown` (`:106`) | `new KeyEventHandler(_kbdHandler.CboFolders_KeyDownAsync)` |
| 86 | `FolderSelectionChanged` (`:105`) | `this.CboFolders_SelectedIndexChanged` (`:213`) |
| 87-88 | `WebViewInitializationCompleted` (`:118`) | `WebView2Control_CoreWebView2InitializationCompleted` (`EventWiring.cs:100`) |
| 89-90 | `ConversationItemSelectionChanged` (`:122`) | `new ListViewItemSelectionChangedEventHandler(this.TopicThread_ItemSelectionChanged)` (`:195`) |
| 91 | `SearchKeyDown` (`:109`) | `this.TextBoxSearch_KeyDown` (`:184`) |
| 92 | `EmailCopyChanged` (`:67`) | `this.CbxEmailCopy_CheckedChanged` (`:208`) |
| 93 | `AttachmentsChanged` (`:69`) | `this.CbxAttachments_CheckedChanged` (`:218`) |

All 16 are classification **(a) — unwireable purely within the four owned files.**

#### 2.1.3 `QfcItemController.ViewerSetup.cs` (2 subscriptions)

| Line | Source | Event | Handler form | Classification |
|---|---|---|---|---|
| 159 | `_breadcrumbViewer` (concrete `ItemViewer`) | `BreadcrumbUnhandledArrow` | `OnBreadcrumbUnhandledArrow` (named, `:186`) | **(c) already unwired** — `-=` at `:155`, `:158`, and `:403` (inside `Cleanup()`) |
| 84-105 | `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2` | `WebResourceRequested` | **anonymous lambda** | (a) technically, but see §2.4 — the wiring site is inside an `[ExcludeFromCodeCoverage]` member |

#### 2.1.4 Totals

- Event `+=` operators in `EventWiring.cs`: **22** (6 control-tree + 16 intent).
- Event `+=` operators in `ViewerSetup.cs`: **2**.
- Event `+=` operators in the other eight partials: **0**.
- Repository total across all ten `QfcItemController.*.cs`: **24**.
- `-=` operators: **3**, all `BreadcrumbUnhandledArrow` (`ViewerSetup.cs:155`, `:158`, `:403`).

The promoted potential's "25 in EventWiring.cs" over-counts by including the two commented-out lines
(`:43`, `:80`) and the arithmetic `+=` at `:136`.

### 2.2 Delegate-identity mechanics for detachment

Every non-lambda subscription can be detached by re-forming the delegate at the unwire site.
`System.Delegate` equality compares `Method` and `Target`, so a freshly constructed
`new KeyEventHandler(_kbdHandler.KeyboardHandler_KeyDownAsync)` removes the earlier subscription **provided
`_kbdHandler` is the same instance** at unwire time. The same holds for `this.Button_MouseEnter` (the target
is `this`, which never changes). This is not speculation — the pattern is already in production in this
repository at `QuickFiler/Controllers/EfcItemController.cs:257-262`:

```csharp
Buttons.ForEach(x =>
{
    x.MouseEnter -= new EventHandler(this.Button_MouseEnter);
    x.MouseLeave -= new EventHandler(this.Button_MouseLeave);
});
_globals.Ol.PropertyChanged -= DarkMode_Changed;
```

`EfcItemController.Cleanup()` is the direct in-repo precedent for the symmetric-unwire design and should
be cited in the plan.

### 2.3 Can `UnwireIntentEvents()` / `UnwireControlTreeEvents()` be written without touching a forbidden file?

**Yes, for both.**

- `UnwireIntentEvents()`: every one of the 16 events is declared on the `IItemViewer` **interface**
  (`QuickFiler/Viewers/IItemViewer.cs:53-122`). The method dereferences only `_itemViewer` and
  `_kbdHandler`, both fields of the partial class. It lives in `EventWiring.cs` (owned). **No** edit to
  `ItemViewer*.cs`, `Navigation.cs`, or `KbdActions.cs`.

- `UnwireControlTreeEvents()`: requires the concrete cast `((ItemViewer)_itemViewer)` to call
  `ForAllControls`, exactly as `WireControlTreeEvents()` already does at `EventWiring.cs:37`. A cast is a
  **read** of `ItemViewer`'s public surface, not a write to `ItemViewer*.cs`. The traversal overload
  `ForAllControls(this Control parent, Action<Control> action, IList<Control> except)` is defined at
  `UtilitiesCS/Extensions/WinFormsExtensions.cs:57-71` and is a deterministic depth-first recursion with an
  exclusion set, so passing the same `except` list (`new List<Control> { ((ItemViewer)_itemViewer).L0vhBreadcrumb_WebView2 }`,
  matching `EventWiring.cs:50`) visits exactly the same control set. `Buttons` and `_itemViewer.MenuItems`
  are read the same way. **No** edit to a forbidden file.

### 2.4 The one residue: the `WebResourceRequested` lambda

`ViewerSetup.cs:84-105` subscribes an anonymous lambda to
`((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2.WebResourceRequested`.

**Field capture required to unwire it:** two new private fields in `ViewerSetup.cs` (owned):

1. `private EventHandler<CoreWebView2WebResourceRequestedEventArgs> _webResourceRequestedHandler;` — assigned
   the delegate before the `+=`, so the identical instance is available for `-=`.
2. `private CoreWebView2 _coreWebView2;` — the event source itself, because `Cleanup()` nulls `_itemViewer`
   at `:407`/`:423` and therefore cannot re-derive `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2`
   after that point.

Both fields and both statements live entirely in `ViewerSetup.cs`. **However**, the enclosing method
`InitializeWebViewAsync` carries `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`
(`ViewerSetup.cs:41`) with a documented residual barrier: the `.CoreWebView2` property is null unless the
real WebView2 runtime initialized the control, which the unit-test policy bars. **The unwire of this one
subscription is therefore not unit-testable.**

**PARTIAL-FIX BOUNDARY (issue #481).** The owned-file fix can reach:

- 22 of 24 subscriptions unwired symmetrically and **testably** (16 intent via `Mock<IItemViewer>`
  `VerifyRemove`; 6 control-tree via the existing headless-`ItemViewer` fixture, §6.3).
- The 23rd (`BreadcrumbUnhandledArrow`) is already unwired.
- The 24th (`WebResourceRequested`) can be unwired in code, but the change is confined to an
  already-exempt member and cannot carry a regression test. Recommendation: implement the capture-and-detach
  anyway (it is a two-field, three-line change with no test cost), and state explicitly in the plan that this
  member remains coverage-exempt and its detachment is verified by inspection, not by test.

### 2.5 Ordering constraint imposed on `Cleanup()`

`Cleanup()` (`ViewerSetup.cs:396-425`) currently nulls, in order:
`_breadcrumbViewer` (404), `_globals` (406), `_itemViewer` (407), `_parent` (408), `_listTipsDetails` (409),
`_mailItem` (410), `_folderHandler` (412, again 415), `_webViewEnvironment` (413), `_themes` (414),
`_tableLayoutPanels` (416), `_explorerController` (417), `_homeController` (419), `_kbdHandler` (420),
`_itemPositionTips` (421), `ItemHelper` (422), `_itemViewer` again (423), `_emailIsReadTimer` (424).
That is 16 distinct targets in 18 statements (`_folderHandler` and `_itemViewer` are each assigned twice).

**Yes, `Cleanup()` can call the unwire methods before the nulling, and the ordering constraint is exact:**

The unwire call must be placed **before line 406** and, specifically, before:

- `_itemViewer = null` (`:407`) — the intent-event source and the `ForAllControls` root.
- `_kbdHandler = null` (`:420`) — the delegate **target** for the two control-tree keyboard subscriptions
  and for `FolderKeyDown`. If `_kbdHandler` is null when the delegate is re-formed, the `-=` throws
  `NullReferenceException`; if it were a *different* instance, the `-=` would silently no-op.
- `_tableLayoutPanels = null` (`:416`) — not read by unwiring, but listed for completeness.

Fields that must **not** be nulled before unwiring: none beyond `_itemViewer` and `_kbdHandler`. `Buttons`
is never nulled by `Cleanup()` at all (`_buttons`, `QfcItemController.cs:95`), so the button loop is safe
wherever it is placed.

The existing `BreadcrumbUnhandledArrow` detach at `:403` is already correctly ordered (before `:404`
`_breadcrumbViewer = null`), and it establishes the "detach then null" convention this change extends.

Placement recommendation: insert `UnwireEvents();` (a wrapper calling `UnwireControlTreeEvents()` then
`UnwireIntentEvents()`, mirroring `WireEvents()` at `EventWiring.cs:28-32`) immediately after the
`ResetBreadcrumb()` call at `ViewerSetup.cs:400` and before the breadcrumb detach block at `:401`, or
immediately after that block and before `:406`. Either satisfies every constraint.

**Null-safety requirement:** `Cleanup()` is currently callable on a controller whose `_itemViewer`,
`_kbdHandler`, or `Buttons` are null. The existing test `QfcItemController.ViewerSetupTests.cs:347-376`
(`Cleanup_NullsTrackedPrivateFields`) sets only `_globals`, `_itemViewer`, `_homeController`, and
`ItemHelper` — `_kbdHandler` and `Buttons` are **null**, and `_itemViewer` is a plain `Mock<IItemViewer>`
that cannot be cast to the concrete `ItemViewer`. `QfcItemControllerBreadcrumbDropDownTests.cs:125-153`
(`Cleanup_ResetsInjectedHostForPooledViewerReuse`) also calls `Cleanup()` with `_kbdHandler` null.
**Both existing tests will fail if the unwire path is unguarded.** The unwire methods must therefore be
defensive:

- `UnwireIntentEvents()`: `if (_itemViewer is null) return;` plus `_kbdHandler`-null guard around the
  `FolderKeyDown` detach only.
- `UnwireControlTreeEvents()`: `if (!(_itemViewer is ItemViewer viewer)) return;` for the `ForAllControls`
  walk (mirroring the existing `EnsureBreadcrumbPipeline` guard at `ViewerSetup.cs:138-141`); `Buttons`
  and `MenuItems` loops guarded with `?? Enumerable.Empty<...>()` or an explicit null check.

This asymmetry with `WireControlTreeEvents()` (which is unguarded) is intentional and should be commented:
wiring runs only on the initialized path, teardown must tolerate a partially-constructed controller.

---

## 3. Issue #483 — `MailActions` error handling

### 3.1 Concrete exception types on the filer path

`MoveMailAsync` (`QfcItemController.MailActions.cs:83-126`). The `try` block spans lines 91-114 and contains
exactly four operations that can fault:

| Operation | Line | Exceptions actually reachable |
|---|---|---|
| `_globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive)` | 93 | `SpecialFolders` is `ConcurrentDictionary<string, string>` (`UtilitiesCS/Interfaces/IGlobals/IFileSystemFolderPaths.cs:7`). `TryGetValue` with a non-null literal key **cannot throw**. The only fault here is `NullReferenceException` if `_globals` or `_globals.FS` is null. |
| `new EmailFilerConfig() { ... }` (reads `_globals.Ol.ArchiveRootPath`) | 100-109 | `EmailFilerConfig` has a parameterless constructor (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs:22`) and the initializer only assigns properties. Reachable fault: `NullReferenceException` on `_globals.Ol`; `System.Runtime.InteropServices.COMException` if `ArchiveRootPath` is a live Outlook-backed read. |
| `_emailFilerFactory(config)` | 110 | Production default is `config => new EmailFiler(config)` (`Initialization.cs:394`). `EmailFiler(EmailFilerConfig)` assigns `Config = options` and nothing else (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:46-49`). It **cannot throw**. An injected test factory can throw anything. |
| `_homeController.FilerQueue.Enqueue(filer, helpers)` | 111 | `FilerQueue.Enqueue(EmailFiler, IList<MailItemHelper>)` (`QuickFiler/Controllers/FilerQueue.cs:31-38`) calls `Queue.Add(new FilerQueueItem(filer, helpers))`. `FilerQueueItem`'s constructor (`:70-78`) throws **`ArgumentNullException`** for a null filer, a null helpers list, or any null element. `BlockingCollection<T>.Add` throws **`ObjectDisposedException`** if the collection was disposed and **`InvalidOperationException`** if it was marked complete. `NullReferenceException` if `_homeController` is null. |
| `await Task.CompletedTask` | 112 | Cannot throw. |

**Assessment.** The genuinely anticipated set on this path is narrow:
`ArgumentNullException`, `InvalidOperationException`, `ObjectDisposedException`, plus `COMException` from
the Outlook-backed `ArchiveRootPath` read, plus `NullReferenceException` from an uninitialized collaborator.
`NullReferenceException` is a programming defect, not an anticipated runtime condition, and should not be
caught. There is no single narrow base type that covers the others without also covering `NullReferenceException`.

**Recommendation.** Do not attempt a type-narrowed multi-catch. Keep a single `catch (System.Exception e)` at
the boundary and satisfy the General Code Change Policy §3.1 by **adding context and re-raising**, which the
policy explicitly permits ("unless you immediately re-raise or propagate with added context"). Concretely:

```
catch (System.Exception e)
{
    logger.Error($"Error moving mail {ItemHelper.Subject} ...", e);
    NotifyMoveFailure($"Error moving mail ... : {e.Message}");   // seam, §3.2
    throw new InvalidOperationException($"Failed to file mail '{ItemHelper.Subject}' to '{SelectedFolder}'.", e);
}
```

Wrapping (rather than a bare `throw;`) is the correct choice here because it adds the subject and destination
folder that the caller's log line otherwise obtains only by re-reading COM (`QfcCollectionController.cs:2245-2252`
wraps that read in its own try/catch precisely because it can fail). A bare `throw;` is an acceptable
simpler alternative if the plan prefers to preserve the original stack unchanged; both satisfy the policy.

### 3.2 Is there an existing UI-dispatcher seam? Is it sufficient?

**A dispatcher seam exists.** `private UtilitiesCS.Threading.IUiDispatcher _uiDispatcher;`
(`QfcItemController.cs:66`), injected as an optional constructor parameter
(`Initialization.cs:38, 57`) and defaulted in `SaveParameters` to `new WpfUiDispatcher()`
(`Initialization.cs:383`). The interface is `UtilitiesCS/Threading/IUiDispatcher.cs:15-42` and exposes
`Invoke(Action)`, `InvokeAsync(Action)`, `BeginInvoke(Action)`, `InvokeAsync<TResult>(Func<TResult>)`,
`InvokeAsync<TResult>(Func<Task<TResult>>)`. A synchronous-executing mock builder already exists at
`QfcItemController.TestSupport.cs:102-137` (`BuildSyncDispatcher`).

**It is not sufficient on its own.** Marshalling through `_uiDispatcher.Invoke(() => MessageBox.Show(...))`
puts the call on the UI thread but leaves `MessageBox.Show` — a modal, blocking WinForms dialog — in the
executed delegate. With `BuildSyncDispatcher`, the delegate *is* executed, so a test would launch a real
modal dialog and hang. There is **no** `IUserPrompt`-style abstraction reachable from `QuickFiler`
(`IUserPrompt` exists only in the `Tags` assembly: `Tags/IUserPrompt.cs:10`, `Tags/WinFormsUserPrompt.cs:15`).
No `Invoke`/`BeginInvoke` on `_itemViewer` helps either, for the same reason.

**Minimal seam that lives entirely inside the four owned files.** The repository already uses exactly this
pattern for `MessageBox` in two places in the same assembly:

- `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:22-23`
  ```csharp
  internal Action<string> MoveFailureMessageAction { get; set; } = text => MessageBox.Show(text);
  ```
- `QuickFiler/Controllers/QfcExplorerController.cs:56-63`
  ```csharp
  internal System.Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> NotInViewDialogInvoker { get; set; } =
      (text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon);
  ```

**Recommended seam** (declared in `QfcItemController.MailActions.cs`, owned; visible to tests via
`InternalsVisibleTo`, §5.5):

```csharp
// Injectable seam for the user-facing move-failure notification. The default forwards to the modal
// WinForms dialog, which cannot run in a headless unit test. Mirrors
// EfcHomeController.MoveFailureMessageAction and QfcExplorerController.NotInViewDialogInvoker.
internal Action<string> MoveFailureNotifier { get; set; } = text => MessageBox.Show(text);
```

and a private helper that marshals it, composing the two seams:

```csharp
private void NotifyMoveFailure(string message)
{
    var notifier = MoveFailureNotifier;
    var dispatcher = _uiDispatcher;
    if (dispatcher is null) { notifier(message); return; }
    dispatcher.Invoke(() => notifier(message));
}
```

This fixes both the non-UI-thread defect (via `_uiDispatcher`) and the untestability (via
`MoveFailureNotifier`), adds no new interface, and touches only `MailActions.cs`.

Note the existing tests set `_uiDispatcher` only when they need it; `MoveMailAsync_*` tests in
`SeamFactoryTests.cs` do not, hence the null guard above.

### 3.3 Cancellation-token checks

**The check `MarkItemForDeletionAsync` performs — exact line and form:**

`QuickFiler/Controllers/QfcItemController.MailActions.cs:213`:

```csharp
public async Task MarkItemForDeletionAsync()
{
    Token.ThrowIfCancellationRequested();            // line 213 — first statement of the body
    await _uiDispatcher.InvokeAsync(() => { ... });
}
```

`Token` is `public CancellationToken Token { get; set; }` (`QfcItemController.cs:267`), assigned from
`_homeController.Token` in `SaveParameters` (`Initialization.cs:377`). Its default value is
`default(CancellationToken)`, on which `ThrowIfCancellationRequested()` is a no-op — so adding the call is
safe for every existing test that leaves `Token` unset.

The same one-line form is already used at `FocusAndTheme.cs:223` (`ToggleTipsAsync`),
`ViewerSetup.cs:46` (`InitializeWebViewAsync`), `:260` (`ResolveControlGroupsAsync`), `:334`
(`PopulateControlsAsync`), and `Initialization.cs:207, 297`.

**Exact insertion points for the three methods that lack it:**

| Method | File:line | Insertion point | Rationale |
|---|---|---|---|
| `MoveMailAsync` | `MailActions.cs:83-126` | New first statement of the body, at line 84 — **before** `if (ItemHelper is not null)` at `:87` | The whole method is the cancellable unit. Placing it inside the `try` (lines 91-114) would cause the new catch to swallow/rewrap `OperationCanceledException`, which must propagate unchanged to the bulk loop. Placing it before the `if` also matches `MarkItemForDeletionAsync`'s "first statement" form. |
| `FlagAsTaskAsync` | `MailActions.cs:183-200` | New first statement, at line 184 — **before** `List<MailItem> itemList = [Mail];` at `:185` | `Mail` is a COM read; the check must precede it. Direct structural analogue of `MarkItemForDeletionAsync`. |
| `EnumerateConversationAsync` | `MailActions.cs:49-52` | New first statement, at line 50 — **before** `await _uiDispatcher.InvokeAsync(EnumerateConversation);` at `:51` | Identical shape to `MarkItemForDeletionAsync`: one dispatcher call, nothing else. |

**Not in scope:** the synchronous siblings `FlagAsTask` (`:167`), `MarkItemForDeletion` (`:202`),
`EnumerateConversation` (`:36`), and `CollapseConversation` (`:27`) — the promoted potential names only the
three async members, and the sync ones have no established cancellation convention in this type.

### 3.4 Blast radius of the rethrow — verified

The **sole production caller** of `MoveMailAsync` is `QfcCollectionController.TryMoveEmailByGroupAsync`
(`QuickFiler/Controllers/QfcCollectionController.cs:2236-2258`):

```csharp
private static async Task TryMoveEmailByGroupAsync(QfcItemGroup group)
{
    try { await group.ItemController.MoveMailAsync(); }
    catch (System.Exception e)
    {
        var subject = "";
        try { subject = group.MailItem.Subject; }
        catch (System.Exception e2) { logger.Error($"Unable to retrieve subject {e2.Message}", e2); }
        logger.Error($"Error moving message {subject}. Continuing execution.\n{e.Message}", e);
    }
}
```

It already catches, logs with subject context, and continues. Rethrowing from `MoveMailAsync` therefore
**cannot** abort the bulk move loop (`MoveEmailsAsync`, `:2206-2228`); it converts a silently-swallowed
failure into a per-item logged failure at the caller, which is the intended behaviour change. `QfcCollectionController.cs`
is **not** modified by this feature. The only other reference is the commented-out line `:2227`.

The other three `MoveMailAsync` mentions are tests: `SeamFactoryTests.cs:162, 185, 228` and a
`NotImplementedException` stub at `QfcThemeHelperTests.cs:442`. None of the three live tests drives the
catch block (they exercise the `ItemHelper is null`, OneDrive-missing, and happy paths respectively), so
none breaks under the rethrow.

---

## 4. Issue #484 — `Cleanup()` timer and stale fields

### 4.1 Current state — confirmed

`_emailIsReadTimer` declaration: `QuickFiler/Controllers/QfcItemController.cs:53`

```csharp
private System.Threading.Timer _emailIsReadTimer;
```

Complete site list (repository-wide grep for `_emailIsReadTimer`):

| Site | File:line | Content |
|---|---|---|
| Declaration | `QfcItemController.cs:53` | `private System.Threading.Timer _emailIsReadTimer;` |
| Dispose (re-arm path) | `QfcItemController.Navigation.cs:211-214` | `if (_emailIsReadTimer is not null) { _emailIsReadTimer.Dispose(); }` inside `ToggleExpansionOff()` |
| Create + arm | `QfcItemController.Navigation.cs:223-224` | `_emailIsReadTimer = new System.Threading.Timer(ApplyReadEmailFormat); _emailIsReadTimer.Change(4000, System.Threading.Timeout.Infinite);` inside `ToggleExpansionOn()`, guarded by `(ItemHelper is not null) && ItemHelper.UnRead == true` (`:221`) |
| Null without dispose | `QfcItemController.ViewerSetup.cs:424` | `_emailIsReadTimer = null;` inside `Cleanup()` |

**There are no other arming sites.** `ToggleExpansionOn()` is the only creator.

### 4.2 Does disposing in `Cleanup()` require any change to `Navigation.cs`? — No

`Cleanup()` lives in `ViewerSetup.cs` (owned). The field is declared in `QfcItemController.cs` and is
accessible from every partial. Replacing line 424 with

```csharp
_emailIsReadTimer?.Dispose();
_emailIsReadTimer = null;
```

is a purely local edit to `ViewerSetup.cs`. `Navigation.cs:211-213` continues to work unchanged: it
null-checks before disposing, and `Timer.Dispose()` is idempotent (a second `Dispose()` on an already-disposed
`System.Threading.Timer` does not throw). `Navigation.cs:223` unconditionally overwrites the field with a new
`Timer`, so it is likewise unaffected. **`Navigation.cs` requires no change and must not be written.**

### 4.3 Is `Dispose()` alone sufficient? — No, and the residue must be closed in an owned file

`System.Threading.Timer.Dispose()` prevents *future* callbacks but does **not** abort a callback that is
already executing on a thread-pool thread. `Timer.Dispose(WaitHandle notifyObject)` signals only after all
callbacks complete, but using it inside `Cleanup()` would introduce a blocking wait on the UI thread during
teardown — an unacceptable production change for a bug-fix feature, and one that adds a real deadlock risk
because `ApplyReadEmailFormat` calls `Theme.SetMailRead(async: true)` which itself dispatches.

**Recommendation: use plain `Dispose()` and additionally guard the callback.**
`ApplyReadEmailFormat` (`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:318-324`) is in an owned
file and currently dereferences four things that `Cleanup()` invalidates:

```csharp
public void ApplyReadEmailFormat(object state)
{
    ItemHelper.UnRead = false;              // 320 — ItemHelper nulled at ViewerSetup.cs:422
    _themes[_activeTheme].SetMailRead(true);// 321 — _themes nulled at :414 (_activeTheme is NOT nulled)
    _mailActions.UnRead = false;            // 322 — _mailActions currently retained (§4.4)
    _mailActions.Save();                    // 323
}
```

Add an early-return guard:

```csharp
public void ApplyReadEmailFormat(object state)
{
    // Runs on a thread-pool thread from _emailIsReadTimer. Cleanup() may have released the
    // collaborators between the timer firing and this body running; a callback already in flight
    // when Dispose() is called still executes, so this guard is required in addition to disposal.
    if (ItemHelper is null || _themes is null || _activeTheme is null || _mailActions is null) { return; }
    ...
}
```

`ApplyReadEmailFormat` is a **public** member declared on `IQfcItemController` (`IQfcItemController.cs:50`).
Adding an internal guard does not change the signature and is not a breaking change (§7).

### 4.4 Deterministic testing approach (no `Thread.Sleep`, no `Task.Delay`)

Both APIs are banned by `.claude/rules/general-unit-test.md` ("Banned APIs in test code — `setTimeout`,
`Thread.Sleep`, `Task.Delay`, real wall-clock waits"). Two deterministic assertions are available and
neither requires the timer to fire:

**T1 — disposal is observable via `ObjectDisposedException` on `Change`.** After `Timer.Dispose()`, calling
`Timer.Change(...)` throws `ObjectDisposedException`. The test therefore:

```
Arrange: var timer = new Timer(_ => { }, null, Timeout.Infinite, Timeout.Infinite);   // never fires
         SetField(controller, "_emailIsReadTimer", timer);
Act:     controller.Cleanup();
Assert:  GetField(controller, "_emailIsReadTimer").Should().BeNull();
         Action act = () => timer.Change(0, Timeout.Infinite);
         act.Should().Throw<ObjectDisposedException>();
```

The timer is armed with `Timeout.Infinite` so it can never fire during the test — the assertion is on the
disposal state, not on a race. Fully deterministic, no wall-clock dependency, no temp file.

**T2 — the callback guard is directly invocable.** `ApplyReadEmailFormat(object state)` is public. A second
test calls `controller.ApplyReadEmailFormat(null)` on a freshly-`Cleanup()`ed controller and asserts
`act.Should().NotThrow()`, plus `mailActions.Verify(m => m.Save(), Times.Never())` against a `Mock<IMailItemActions>`
captured before `Cleanup()`. This proves the post-teardown callback is inert without ever scheduling a timer.

**No existing test arms a real timer.** `QfcItemController.NavigationTests.cs:345-389`
(`ToggleExpansionOn_AppliesExpandedSnapshotAndSetsExpandedFlag`) deliberately leaves `ItemHelper` null so the
timer branch at `Navigation.cs:221-225` is skipped (see the test's own doc comment at `:341-342`). Adding T1/T2
introduces no cross-test timing coupling.

### 4.5 `_mailActions` lifetime across the `Cleanup()` / `SaveParameters` boundary

**Is nulling it safe? Yes — and it is required for correct pooled reuse.**

`SaveParameters` binds the field with a **null-coalescing assignment** (`Initialization.cs:395-397`):

```csharp
_mailActions ??= mailItem is null ? null : new QuickFiler.Interfaces.MailItemActionsAdapter(mailItem);
```

Because `??=` assigns only when the target is null, a controller that is re-parameterised after `Cleanup()`
**retains the adapter bound to the previous `MailItem`**. That is precisely the latent defect the promoted
potential describes, and the `??=` operator is the mechanism. Nulling `_mailActions` in `Cleanup()` makes the
next `SaveParameters` call rebind it to the new `mailItem`. **No change to `Initialization.cs` is required.**

**Does any caller depend on its retention?** Complete consumer list (grep `_mailActions`):

| Consumer | File:line | Owned? | Post-`Cleanup()` reachable? |
|---|---|---|---|
| `Reply()` | `Navigation.cs:90` | No (forbidden) | No — requires a live viewer |
| `ReplyAll()` | `Navigation.cs:96` | No (forbidden) | No |
| `Forward()` | `Navigation.cs:102` | No (forbidden) | No |
| `CollapseConversation()` | `MailActions.cs:32` | Yes | No |
| `EnumerateConversation()` | `MailActions.cs:43` | Yes | No |
| `ApplyReadEmailFormat()` | `FocusAndTheme.cs:322-323` | Yes | **Yes** — via the orphaned timer (§4.3) |
| `TxtboxBodyDoubleClickCore()` | `EventHandlers.cs:135` | No (not owned, not forbidden) | No |
| Assignment | `Initialization.cs:59, 395` | No | n/a |

Only the timer callback can reach `_mailActions` after `Cleanup()`, and §4.3's guard covers it. Nulling is
safe. Six tests reflection-inject `_mailActions` (`SeamDispatcherTests.cs:170, 334`; `SeamCoreTests.cs:43, 92, 157`;
`MailActionsTests.cs:172`); none of them calls `Cleanup()`, so none is affected.

### 4.6 Other field-state asymmetries observed (report-only, not in the five issues)

`Cleanup()` nulls `_listTipsDetails` (`:409`) but **not** `_listTipsExpanded`; it does not null `_tlpStates`,
`_conversationResolver`, `_convOriginID`, `_selectedFolder`, `_activeTheme`, `_expanded`, `_activeUI`,
`_isWebViewerInitialized`, `_buttons`, or the four `_option*` flags. `_folderHandler` and `_itemViewer` are
each assigned twice (`:412`/`:415` and `:407`/`:423`). These are consistent with the promoted potential's
"other collaborator fields in an inconsistent state" note but are **not** enumerated as defects in any of the
five issues. Recommendation: fix only the duplicate assignments (a formatting-neutral tidy) and `_mailActions`;
promote the remainder as a separate potential rather than widening this feature's blast radius, since
`_isWebViewerInitialized` and `_activeUI` in particular are read by the pooled-reuse path and changing them is
a behaviour change with no issue backing it.

---

## 5. Issue #485 — WebView handler unguarded inputs

### 5.1 Confirmed defect site

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:84-105`:

```csharp
coreWebView2.WebResourceRequested += (sender, e) =>
{
    var requestedId = new Uri(e.Request.Uri).Segments.LastOrDefault()?.Trim('/');   // 86 — unguarded ctor
    if (string.IsNullOrEmpty(requestedId)) { return; }                              // 87-90
    var contentIdMap = CidImageResolver.BuildContentIdMap(ItemHelper.AttachmentsInfo); // 92
    if (!contentIdMap.TryGetValue(requestedId, out var match)) { return; }           // 93-96
    var mimeType = ResolveImageMimeType(match.FileExtension);                        // 98
    e.Response = _webViewEnvironment.CreateWebResourceResponse(
        new MemoryStream(match.AttachmentData), 200, "OK", $"Content-Type: {mimeType}"); // 99-104 — unguarded
};
```

Line numbers differ from the promoted potential (which cites `:83`/`:97`) by three; the current lines are
**86** and **100**. A third unguarded dereference the potential does not mention: `ItemHelper.AttachmentsInfo`
at **line 92** throws `NullReferenceException` if `ItemHelper` is null — reachable after `Cleanup()` because
the subscription is never detached (issue #481, §2.4). The three defects are coupled.

### 5.2 Types involved — verified

| Symbol | Definition | Notes |
|---|---|---|
| `CidImageResolver` | `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs:14` | `public static class`, `#nullable enable`, documented as "performs no I/O and has no COM/WebView2 dependency" |
| `CidImageResolver.BuildContentIdMap` | `:34-36` | `public static IReadOnlyDictionary<string, IAttachment> BuildContentIdMap(IReadOnlyCollection<IAttachment>? attachments)`. Returns an empty `Dictionary<string, IAttachment>(StringComparer.OrdinalIgnoreCase)` for a null argument (`:38-42`). Skips entries whose `ContentId` is null/empty (`:46`). **Does not** filter on `AttachmentData`, confirming the potential's claim that a map hit does not imply a non-null payload. |
| `CidImageResolver.DefaultVirtualHost` | `:20` | `public const string DefaultVirtualHost = "cid.quickfiler.local";` |
| `IAttachment.AttachmentData` | `UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs:8` | **`byte[] AttachmentData { get; set; }`** — yes, a `byte[]`, mutable, nullable in practice |
| `IAttachment.ContentId` | `:11` | `string ContentId { get; set; }` |
| `IAttachment.FileExtension` | `:13` | `string FileExtension { get; set; }` |
| `MailItemHelper.AttachmentsInfo` | `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Properties.cs:262-266` | `public IAttachment[]? AttachmentsInfo { get; protected set; }` backed by `Lazy<IAttachment[]>? _attachmentsInfo` (`:261`). The setter is **protected** — a test cannot assign it without reflection on `_attachmentsInfo` or a derived type. |
| `ResolveImageMimeType` | `ViewerSetup.cs:197-205` | `private static string`, already null-safe (`fileExtension?.ToLowerInvariant()`), defaults to `"application/octet-stream"` |
| `_webViewEnvironment` | `QfcItemController.cs:39` | `private CoreWebView2Environment _webViewEnvironment;` |

**`_webViewEnvironment.CreateWebResourceResponse` requirements.** The call at `:99-104` passes
`(Stream content, int statusCode, string reasonPhrase, string headers)` and returns a
`CoreWebView2WebResourceResponse` assigned to `e.Response`. `CoreWebView2Environment` is a WebView2 SDK type
with **no public constructor** — production obtains it only through
`IWebViewCoreInitializer.CreateEnvironmentAsync` (`QuickFiler/Viewers/IWebViewCoreInitializer.cs:19-22`).
The repository's only technique for materialising one in a test is
`FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment))`
(`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs:305-306`,
`BreadcrumbDropDownCoverageThresholdTests.cs:300-301`), and in both cases the instance is only **passed
through**, never invoked. An uninitialized instance has no native COM pointer, so calling
`CreateWebResourceResponse` on it is not viable. **The SDK call must stay outside the tested unit.**

### 5.3 Recommended extraction — yes, the handler body can be made directly unit-testable

The proposal in the task brief is correct and is the right design. Concretely, in `ViewerSetup.cs` (owned):

```csharp
/// <summary>
/// Pure decision half of the WebResourceRequested handler (#485). Resolves an intercepted virtual-host
/// URI to the attachment bytes and MIME type that should be served, or reports that the request must be
/// ignored. Takes plain values so it is directly unit-testable without a WebView2 runtime; the SDK
/// response construction stays in the thin lambda adapter.
/// </summary>
internal static bool TryResolveCidResource(
    string requestedUri,
    IReadOnlyDictionary<string, IAttachment> contentIdMap,
    out byte[] payload,
    out string mimeType)
{
    payload = null;
    mimeType = null;

    if (!Uri.TryCreate(requestedUri, UriKind.Absolute, out var uri))   // #485 defect 1
    {
        logger.Debug($"Ignoring cid: request with unparsable URI '{requestedUri}'.");
        return false;
    }

    var requestedId = uri.Segments.LastOrDefault()?.Trim('/');
    if (string.IsNullOrEmpty(requestedId)) { return false; }
    if (contentIdMap is null) { return false; }
    if (!contentIdMap.TryGetValue(requestedId, out var match) || match is null) { return false; }
    if (match.AttachmentData is null)                                   // #485 defect 2
    {
        logger.Debug($"Attachment for content-id '{requestedId}' has no data payload; skipping.");
        return false;
    }

    payload = match.AttachmentData;
    mimeType = ResolveImageMimeType(match.FileExtension);
    return true;
}
```

and the lambda becomes a thin adapter:

```csharp
_webResourceRequestedHandler = (sender, e) =>
{
    var map = CidImageResolver.BuildContentIdMap(ItemHelper?.AttachmentsInfo);   // #485 defect 3 (ItemHelper null)
    if (!TryResolveCidResource(e.Request.Uri, map, out var payload, out var mimeType)) { return; }
    e.Response = _webViewEnvironment.CreateWebResourceResponse(
        new MemoryStream(payload), 200, "OK", $"Content-Type: {mimeType}");
};
coreWebView2.WebResourceRequested += _webResourceRequestedHandler;
```

**Why `UriKind.Absolute` specifically:** `Uri.Segments` throws `InvalidOperationException` on a relative
`Uri`, so `TryCreate` with `UriKind.RelativeOrAbsolute` would merely move the throw one line later.
`UriKind.Absolute` is required.

**Why `ResolveImageMimeType` must stay `static`:** the extracted method is `static`, and
`ResolveImageMimeType` (`:197`) already is. `logger` is a `static readonly` field on the partial class
(`QfcItemController.cs:30`), so it is reachable from a static member.

**How a regression test exercises this without a live WebView2 runtime and without touching `ItemViewer*.cs`:**
call `QfcItemController.TryResolveCidResource(...)` directly (it is `internal static`, visible to
`QuickFiler.Test` via `InternalsVisibleTo`) with:

- a `Dictionary<string, IAttachment>` built from `Mock<IAttachment>` objects (`IAttachment` is a public
  interface — trivially mockable with Moq), or from the real
  `CidImageResolver.BuildContentIdMap(new[] { mockAttachment.Object })` to exercise the real map builder;
- plain `string` URIs.

No `CoreWebView2*` type, no `MailItemHelper`, no `ItemViewer`, no controller instance is needed. Test cases:
malformed URI (`"::not a uri::"`), relative URI (`"/x/y"`), absolute URI with empty final segment
(`"https://cid.quickfiler.local/"`), map miss, map hit with null `AttachmentData`, map hit with a real
`byte[]` plus a known `FileExtension` asserting the MIME type. Every case is deterministic and I/O-free.

**Residual after extraction:** the lambda adapter itself (3 statements) remains inside the
`[ExcludeFromCodeCoverage]` `InitializeWebViewAsync`. That residue is unavoidable and is exactly the two lines
that require the SDK. This is the correct trade: the decision logic — everything the issue is about — moves
into a covered member.

### 5.4 Correction to the promoted potential

The potential's suggested fix is code-correct but stops short of making the fix verifiable. Stated plainly:
`Uri.TryCreate` + a null check applied **in place** inside the lambda would satisfy the issue's letter and
close the runtime fault, but would add zero covered lines and zero regression tests, because the enclosing
member is coverage-exempt and cannot execute in a unit test. The extraction is what turns the fix into
something the test policy can hold. Additionally, the potential omits the `ItemHelper`-null dereference at
line 92, which is the same defect class on the same line range.

### 5.5 `InternalsVisibleTo` — confirmed

`QuickFiler/Properties/AssemblyInfo.cs:5`:

```csharp
[assembly: InternalsVisibleTo("QuickFiler.Test")]
```

A duplicate declaration also exists at `QuickFiler/Controllers/QfcHomeController.cs:18`. Both grant
`QuickFiler.Test` access to `internal` members of the `QuickFiler` assembly, which is why the existing tests
can subclass the `internal partial class QfcItemController` (e.g. `FocusAndThemeTests.cs:29-33`) and call
`internal` members such as `controller.WireIntentEvents()` (`SeamFactoryTests.cs:248`).

---

## 6. Existing test infrastructure

### 6.1 Shared harness

`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (referenced by three of the four owned test
files) provides:

| Member | Line | Purpose |
|---|---|---|
| `internal sealed class HarnessController : QfcItemController` | `:25-29` | Exposes the `protected QfcItemController()` parameterless constructor (`Initialization.cs:27`) |
| `SetField(controller, name, value)` | `:37-47` | Reflection write to a `NonPublic \| Instance` field of `typeof(QfcItemController)` |
| `GetField(controller, name)` | `:49-59` | Reflection read |
| `InvokeNonPublic(controller, name, args)` | `:66-80` | Reflection invoke of a non-public instance method |
| `EnsureSynchronizationContext()` | `:87-93` | Installs a bare `SynchronizationContext` if none |
| `BuildSyncDispatcher()` | `:102-137` | `Mock<IUiDispatcher>` whose `Invoke`/`InvokeAsync`/`BeginInvoke` execute the delegate synchronously |
| `InjectThemes`, `BuildColorTheme`, `BuildThemeDictionary`, `BuildDispatchableTheme` | `:143-211` | Handle-less `Theme` construction with a reflection-injected `_uiDispatcher` |
| `EnsureUiThreadDispatcher()` / `GetDedicatedDispatcher()` | `:238-285` | Seeds the static `UiThread._dispatcher` with a **parked, never-pumped** STA dispatcher |
| `StartRunningDispatcher()` / `ShutdownDispatcher()` | `:297-326` | A running STA `Dispatcher.Run()` on a dedicated background thread |

`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:26` — `internal sealed class WinFormsPumpHost : IDisposable`
runs a real `Application.Run(ApplicationContext)` message pump on a dedicated STA background thread.

Framework/library usage across all four files: **MSTest** (`[TestClass]`, `[TestMethod]`, `[Timeout]`),
**Moq** (`Mock<T>`, `Setup`, `SetupGet`, `Verify`, `VerifySet`, `VerifyAdd`, `Times.*`, `MockBehavior.Strict`
in some files), **FluentAssertions** (`Should().Be/BeNull/NotBeNull/BeSameAs/ContainSingle/Throw/ThrowAsync/NotThrow`).
This matches `CUT1`/`CUT2` exactly. No `Assert.*` MSTest calls appear in the four owned files.

### 6.2 Per-file summary of the four owned test files

| File | Controller construction | Collaborators mocked | Touches the four defect surfaces? | Real `ItemViewer` / message loop? |
|---|---|---|---|---|
| `QfcItemController.FocusAndThemeTests.cs` (498 lines) | `private sealed class FocusController : QfcItemController` (`:29-33`) via the protected ctor, then reflection field injection through `QfcItemControllerTestSupport` (`:35-39`) | `Mock<IItemViewer>` (executing, `:99-115`), `Mock<IQfcTipsDetails>`, `Mock<IQfcKeyboardHandler>`; real `Theme` objects with 15 reflection-injected handle-less WinForms doubles (`EnableHandlelessThemeInvoke`, `:136-158`) | **`ToggleNavigation` — yes** (`:310`, `:327`, `:344`). Cleanup/MoveMail/WebView — no | **No.** All controls are bare `new Label()`/`new Panel()`/`new TextBox()`; `_topicThread`/`_webView2`/`_breadcrumbWebView2` via `Activator.CreateInstance` (`:170-178`). No pump. |
| `QfcItemController.EventWiringTests.cs` (375 lines) | `private sealed class KbdController : QfcItemController` (`:26-39`) which reflection-injects `_kbdHandler` in its own ctor; also `HarnessController` for the two headless-viewer tests | `Mock<IQfcKeyboardHandler>`; real `KbdActions<...>` instances (not mocked) | Wiring — yes. Cleanup/ToggleNavigation/MoveMail/WebView — no | **Yes, two tests.** `:236` and `:327` construct `new QuickFiler.ItemViewer()`. Both install and restore a bare `SynchronizationContext` in try/finally (`:232-233`/`:307`, `:323-324`/`:370`). **No message loop, no `Show()`, no UX** — the viewer is a headless control tree; events are raised by reflecting onto `Control.OnPreviewKeyDown`/`OnKeyDown`/`OnMouseEnter` (`:262-286`). |
| `QfcItemController.ViewerSetupTests.cs` (475 lines) | `HarnessController` + `QfcItemControllerTestSupport.SetField` | `Mock<IItemViewer>`, `Mock<IApplicationGlobals>`, `Mock<IAppQuickFilerSettings>`, `Mock<IOlObjects>`, `Mock<MailItem>` with a full COM property graph (`BuildMailItemMock`, `:97-142`), `Mock<IFilerHomeController>` | **`Cleanup()` — yes** (`:347-376`, `Cleanup_NullsTrackedPrivateFields`). WebView setup — no. ToggleNavigation/MoveMail — no | **Yes, two tests.** `:395` `new QuickFiler.ItemViewer()` headless with SynchronizationContext save/restore. `:424-472` uses **`WinFormsPumpHost`** — a real `Application.Run` message pump on an STA background thread, with `[Timeout(60000)]` (`PumpTimeoutMs`, `:34`). Also `StartRunningDispatcher()` (a real WPF `Dispatcher.Run`) at `:208`, `:316`. |
| `QfcItemController.MailActionsTests.cs` (185 lines) | `private sealed class MailController : QfcItemController` (`:23-27`); local `SetField` helper (`:29-32`) rather than the shared one | `Mock<IItemViewer>`, `Mock<IQfcCollectionController>`, `Mock<IMailItemActions>`, `Mock<IApplicationGlobals>`, `Mock<MailItem>`; real `ConversationResolver` (`:97-104`) | **`MoveMailAsync` — NO.** Covers `PackageItems`, `MarkItemForDeletion` (both branches), `RightKeyActions`/`RightKeyActionsAsync` getters, `CollapseConversation`, `EnumerateConversation` | **No.** Pure mocks. No real control, no pump, no dispatcher. |

### 6.3 UX / live-worker exposure — direct answer

**Two of the four owned test files already instantiate a real `QuickFiler.ItemViewer`, and one already starts
a real WinForms message loop.**

- Real headless `ItemViewer` (no `Show()`, no visible UX, no worker): `EventWiringTests.cs:236`, `:327`;
  `ViewerSetupTests.cs:395`.
- Real WinForms message pump (`Application.Run` on a dedicated STA background thread):
  `ViewerSetupTests.cs:429` via `WinFormsPumpHost`, guarded by `[Timeout(60000)]`.
- Real WPF `Dispatcher.Run` on a dedicated STA background thread: `ViewerSetupTests.cs:208`, `:316` via
  `StartRunningDispatcher()`.

These are pre-existing, deliberately introduced under issue #230 and cycle-5 de-exemption work, and documented
in the test doc comments. **They are not a licence for new tests.** Every regression test this feature adds
should be seam-and-inject only:

- **#480** — `Mock<IItemViewer>` + `Mock<IQfcTipsDetails>`, exactly as `FocusAndThemeTests.cs:310-341` already does.
- **#483** — `Mock<IApplicationGlobals>` / `Mock<IFileSystemFolderPaths>` / `Mock<IFilerHomeController>` +
  the injected `_emailFilerFactory` and the new `MoveFailureNotifier` seam. The shape is already proven at
  `SeamFactoryTests.cs:150-235`.
- **#484** — reflection field injection of a `Timeout.Infinite` `Timer` + `Mock<IMailItemActions>`.
- **#485** — no controller instance at all; call the extracted `internal static` method with plain values and
  `Mock<IAttachment>`.
- **#481 intent half** — `Mock<IItemViewer>` with `viewer.VerifyRemove(v => v.ConversationModeChanged -= It.IsAny<EventHandler>(), Times.Once())`.
  Moq's `VerifyAdd` is already used against this exact mock at `SeamFactoryTests.cs:250-259`, so `VerifyRemove`
  is a proven technique on this surface.
- **#481 control-tree half** — this is the one case where a real headless `ItemViewer` is unavoidable, because
  `WireControlTreeEvents`/`UnwireControlTreeEvents` walk a concrete control tree. Mirror
  `EventWiringTests.cs:229-309` exactly: wire, unwire, then raise `OnPreviewKeyDown`/`OnKeyDown`/`OnMouseEnter`
  by reflection and assert `mockKbd.Verify(..., Times.Never())` and an unchanged `BackColor`. No pump, no
  `Show()`, no worker — same risk profile as the already-accepted precedent.

---

## 7. Public-surface stability (upstream contract for features 464 and 489)

Downstream consumers: **464** (EFC controllers, via #463) and **489** (`ItemViewer`, via #486/#489). The
following is the exhaustive list of surface changes the recommended fixes imply.

### 7.1 ADDED members

| Member | File | Accessibility | Static? | Signature | Purpose |
|---|---|---|---|---|---|
| `UnwireEvents()` | `EventWiring.cs` | `internal` | no | `void UnwireEvents()` | Mirror of `WireEvents()` (`:28`); calls the two below |
| `UnwireControlTreeEvents()` | `EventWiring.cs` | `internal` | no | `void UnwireControlTreeEvents()` | Mirror of `WireControlTreeEvents()` (`:35`) |
| `UnwireIntentEvents()` | `EventWiring.cs` | `internal` | no | `void UnwireIntentEvents()` | Mirror of `WireIntentEvents()` (`:66`) |
| `MoveFailureNotifier` | `MailActions.cs` | `internal` | no | `Action<string> { get; set; }`, default `text => MessageBox.Show(text)` | Test seam for the move-failure dialog (§3.2) |
| `NotifyMoveFailure(string)` | `MailActions.cs` | `private` | no | `void NotifyMoveFailure(string message)` | Composes `_uiDispatcher` + `MoveFailureNotifier`; **not** part of the surface |
| `TryResolveCidResource(...)` | `ViewerSetup.cs` | `internal` | **yes** | `static bool TryResolveCidResource(string requestedUri, IReadOnlyDictionary<string, IAttachment> contentIdMap, out byte[] payload, out string mimeType)` | Pure decision half of the WebResourceRequested handler (§5.3) |
| `_webResourceRequestedHandler` | `ViewerSetup.cs` | `private` field | no | `EventHandler<CoreWebView2WebResourceRequestedEventArgs>` | Delegate capture for `-=` (§2.4) |
| `_coreWebView2` | `ViewerSetup.cs` | `private` field | no | `CoreWebView2` | Event-source capture for `-=` after `_itemViewer` is nulled (§2.4) |

All added members are `internal` or `private`. **No public member is added.** No interface is modified.
`IQfcItemController`, `IItemControler`, and `IItemViewer` are untouched.

### 7.2 CHANGED members (behaviour only; no signature change)

| Member | File:line | Change | Downstream impact |
|---|---|---|---|
| `ToggleNavigation(bool async)` | `FocusAndTheme.cs:168-179` | One `_itemPositionTips.Toggle(false)` call instead of two. Dispatch count on `_itemViewer` drops from 2 to 1. | A downstream mock asserting `Times.Exactly(2)` on `Invoke`/`BeginInvoke` or `Toggle(false)` for this member would break. No such assertion exists today (`FocusAndThemeTests.cs:323` uses `Times.AtLeastOnce()`). |
| `Cleanup()` | `ViewerSetup.cs:396-425` | **(a)** Now detaches 22 additional event subscriptions before nulling. **(b)** Now disposes `_emailIsReadTimer` before nulling. **(c)** Now nulls `_mailActions`. **(d)** Must tolerate null `_itemViewer`, `_kbdHandler`, `Buttons`. Signature `public void Cleanup()` unchanged; still on `IQfcItemController:77`. | **Lifecycle contract change — the most important item for downstream.** After `Cleanup()`, a viewer that raises a wired event will no longer invoke the controller. Feature 489 must not assume post-`Cleanup()` handler delivery from a pooled `ItemViewer`. `_mailActions` becomes null after `Cleanup()`; `SaveParameters`' `??=` (`Initialization.cs:395`) rebinds it on reuse. |
| `MoveMailAsync()` | `MailActions.cs:83-126` | **(a)** Rethrows (wrapped in `InvalidOperationException`) instead of swallowing. **(b)** Adds `Token.ThrowIfCancellationRequested()` as the first statement, so `OperationCanceledException` can now escape. **(c)** The user message is routed through `MoveFailureNotifier` on the UI dispatcher instead of a direct `MessageBox.Show`. Return type stays `Task`. | **Behavioural contract change.** Any future caller must handle a faulted task. The existing sole caller already does (`QfcCollectionController.cs:2238-2257`). Feature 464 must not copy the swallow-and-continue shape into `EfcItemController`. |
| `FlagAsTaskAsync()` | `MailActions.cs:183-200` | Adds `Token.ThrowIfCancellationRequested()` as the first statement. | Can now throw `OperationCanceledException`. |
| `EnumerateConversationAsync()` | `MailActions.cs:49-52` | Adds `Token.ThrowIfCancellationRequested()` as the first statement. | Can now throw `OperationCanceledException`. Reachable via `RightKeyActionsAsync["&Expand"]` (`MailActions.cs:78`). |
| `ApplyReadEmailFormat(object state)` | `FocusAndTheme.cs:318-324` | Adds an early-return guard on null `ItemHelper` / `_themes` / `_activeTheme` / `_mailActions`. Signature unchanged; still on `IQfcItemController:50`. | Becomes a silent no-op against a torn-down controller instead of throwing `NullReferenceException`. A downstream test asserting the throw would break; none exists. |
| `InitializeWebViewAsync()` | `ViewerSetup.cs:42-128` | The `WebResourceRequested` lambda body is replaced by a two-statement adapter over `TryResolveCidResource`; the delegate and its source are captured into fields. Remains `internal async Task`, remains `[ExcludeFromCodeCoverage]`. | No signature change. Feature 489 should be aware that the handler now tolerates a malformed URI, a null attachment payload, and a null `ItemHelper` by returning without setting `e.Response` (the request falls through to the runtime's default handling). |

### 7.3 REMOVED members

**None.** In particular, `ToggleNavigation(bool async)` is **retained**, because it is declared on the public
interface `IQfcItemController.cs:89` and implemented by `EfcItemController.cs:958` — deleting it would require
writing two files this feature does not own. It remains dead production code with one test caller.

### 7.4 Event-wiring ORDER changes

- **Wiring order: unchanged.** `WireEvents()` still calls `WireControlTreeEvents()` then `WireIntentEvents()`
  (`EventWiring.cs:28-32`). Neither method's internal ordering changes.
- **Unwiring order: newly defined.** `UnwireEvents()` calls `UnwireControlTreeEvents()` then
  `UnwireIntentEvents()`, mirroring the wiring order. Because detachment is order-independent for disjoint
  event sources, this is a convention rather than a constraint, but downstream code that mirrors the pattern
  should follow it.
- **`Cleanup()` statement order: newly constrained.** The unwire call must precede `_itemViewer = null`
  (`:407`) and `_kbdHandler = null` (`:420`); the timer disposal must precede `_emailIsReadTimer = null`
  (`:424`); the existing `BreadcrumbUnhandledArrow` detach must continue to precede
  `_breadcrumbViewer = null` (`:404`). Any downstream reordering of `Cleanup()` must preserve all three.
- **New lifecycle invariant for feature 489:** a pooled `ItemViewer` handed back after `Cleanup()` carries
  **zero** subscriptions from the released controller (with the single documented exception of the WebView2
  `WebResourceRequested` handler if the capture-and-detach is deferred — see §2.4). Feature 489 may rely on
  this when reasoning about viewer reuse.

---

## 8. Toolchain baseline facts

Reported from files only. No build, restore, or test command was executed.

### 8.1 Test assembly path

`QuickFiler.Test/QuickFiler.Test.csproj`:
- `<OutputType>Library</OutputType>` (line 14)
- `<AssemblyName>QuickFiler.Test</AssemblyName>` (line 17)
- `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` (line 18)
- `Debug|AnyCPU` → `<OutputPath>bin\Debug\</OutputPath>` (line 36)

**Test assembly path (Debug, Any CPU):** `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`

### 8.2 Worktree state — what is missing

| Item | Present in this worktree? | Evidence |
|---|---|---|
| `packages/` (NuGet packages dir) | **No** | Glob `packages/**` → no files |
| `QuickFiler.Test/bin/Debug/*.dll` | **No** | Glob → no files |
| `.dotnet-sdk/` (repo-local SDK) | **No** | Glob `.dotnet-sdk/**/dotnet.exe` → no files |
| `dotnet-tools.json` (CSharpier manifest) | **Yes**, at repo root (not `.config/`) | pins `csharpier` `1.2.6`, `rollForward: false` |
| `global.json` | **Yes** | SDK `8.0.205`, `rollForward: latestFeature`, `paths: [".dotnet-sdk", "$host$"]`, with an error message pointing at `./scripts/vscode/Install-RepoDotNetSdk.ps1` |
| `scripts/vscode/Install-RepoDotNetSdk.ps1` | **Yes** | Glob confirms |
| `QuickFiler.Test/packages.config` | **Yes** | legacy `packages.config` restore model |
| `coverage.config` | **Yes**, at repo root | excludes only third-party modules (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest). `QuickFiler` is **not** assembly-excluded; exemption is per-member via `[ExcludeFromCodeCoverage]`. |

**`dotnet tool restore` has NOT been run in this worktree** — there is no way to confirm this from the file
tree alone (the tool cache lives under the user profile, outside the worktree), so treat it as unknown and
run it unconditionally. This is a fact worth stating precisely: the manifest exists, the local tool payload
location is not observable from the repository, and `dotnet tool run csharpier` fails with a clear error if
restore has not happened. Running `dotnet tool restore` is idempotent and cheap.

**Because `global.json` declares `paths: [".dotnet-sdk", "$host$"]` and `.dotnet-sdk/` is absent, every
`dotnet` invocation in this worktree resolves through `$host$` (the machine-installed SDK) or fails with the
declared `errorMessage`.** If `dotnet tool restore` fails with that message, `scripts/vscode/Install-RepoDotNetSdk.ps1`
must be run from the repository root first.

### 8.3 What a plan's Phase 0 baseline must bootstrap

In this order, before any of the four toolchain stages can produce a meaningful result:

1. `nuget restore TaskMaster.sln` — mandatory. `packages/` is absent and the `.csproj` files import
   `..\packages\...\*.props` conditionally (`QuickFiler.Test.csproj:3-8`), so without restore the analyzer,
   MSTest adapter, and AltCover props silently do not import and the build produces a different (weaker)
   diagnostic set. CI does this at `.github/workflows/_mstest-coverage.yml:43-45`.
2. `dotnet tool restore` — mandatory before the first `dotnet tool run csharpier` invocation, per CLAUDE.md
   ("Run `dotnet tool restore` once per clone or worktree before the first invocation"). If it fails, run
   `scripts/vscode/Install-RepoDotNetSdk.ps1` first.
3. A full `msbuild ... /t:Rebuild` — `bin/Debug` is empty, so there is no baseline binary. Note that CLAUDE.md
   mandates `/t:Rebuild` (not `/t:Build`) for the analyzer and nullable gates precisely because MSBuild's
   up-to-date check does not invalidate on a command-line `/p:` change. CI uses `/t:Build /m` only because a
   runner checkout is always cold; this worktree is cold too, but the plan should still use `/t:Rebuild` to
   match the repository policy verbatim.
4. Only then can `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
   produce a baseline.

**Local vstest invocation notes** (from CI and from repository practice):
- CI runs `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
  (`_mstest-coverage.yml:83`). `/InIsolation` is load-bearing.
- CI discovers assemblies with `Get-ChildItem -Recurse -Filter '*.Test.dll'` filtered to `\bin\<Config>\` and
  excluding `\obj\` and `\ref\` (`:70-76`). A local run inside a worktree must additionally exclude
  `\.claude\worktrees\` if run from the repository root, or the run will pick up sibling agent worktrees.
- vstest names TRX files `<account>_<HOST>_<timestamp>.trx` by default. Control `/ResultsDirectory:` and
  `LogFileName=` (or rename before citing) so no host account name or machine name reaches a committed
  evidence artifact.

**Baseline expectation.** Because this feature adds behaviour-changing fixes, the Phase 0 baseline must record
the **pre-change** pass/fail counts for `QuickFiler.Test` specifically, so that the three assertions this
feature intentionally tightens or must accommodate are attributable:
- `QfcItemController_FocusAndThemeTests.ToggleNavigation_Synchronous_TogglesPositionTips` (assertion tightened, §1.3)
- `QfcItemController_ViewerSetupTests.Cleanup_NullsTrackedPrivateFields` (must survive the new unwire path, §2.5)
- `QfcItemControllerBreadcrumbDropDownTests.Cleanup_ResetsInjectedHostForPooledViewerReuse` (same, §2.5)

### 8.4 `.csproj` avoidance

All four target test files already carry `Compile Include` entries in
`QuickFiler.Test/QuickFiler.Test.csproj`:

| File | csproj line |
|---|---|
| `Controllers\QfcItemController.EventWiringTests.cs` | 142 |
| `Controllers\QfcItemController.MailActionsTests.cs` | 144 |
| `Controllers\QfcItemController.ViewerSetupTests.cs` | 150 |
| `Controllers\QfcItemController.FocusAndThemeTests.cs` | 153 |

Adding test methods to these four files requires **no** `.csproj` edit, avoiding a conflict on the
alphabetically-ordered item group (lines 57-175) shared with sibling epic children.

Two adjacent files also already carry entries and are natural homes for some of the work, but note the
placement trade-off:
- `Controllers\QfcItemController.SeamFactoryTests.cs` (line 156) already holds the three `MoveMailAsync` tests
  (`:150-235`) and the `WireIntentEvents` subscription test (`:239-259`). Adding the #483 and #481-intent tests
  there would keep related tests together, but the brief's instruction is to prefer the four named files.
  Recommendation: add the new #483 tests to `QfcItemController.MailActionsTests.cs` (the named owned file) and
  the new #481-intent tests to `QfcItemController.EventWiringTests.cs`. Leave `SeamFactoryTests.cs` untouched
  to minimise the sibling-conflict surface, and cite its existing tests as the pattern source.
- `Controllers\QfcItemController.TestSupport.cs` (line 146) holds the shared harness. If a new shared helper is
  needed it belongs here, but the recommended tests need none.

### 8.5 File-size ceiling check (General Code Change Policy §4.1, 500 lines)

| File | Current lines | Headroom |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 326 | 174 |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 391 | 109 |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 430 | **70** |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 224 | 276 |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | **3** |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 374 | 126 |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 474 | **26** |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 184 | 316 |

**Three constraints the plan must honour:**

1. `QfcItemController.FocusAndThemeTests.cs` has **3 lines of headroom**. The #480 test work must be done by
   *tightening the existing assertion in place* (a zero-line change) plus at most a very small `async: true`
   test — which will not fit. Options: (a) delete the ~120-line `EnableHandlelessThemeInvoke` helper if a
   smaller form suffices (risky, it is load-bearing for four other tests); (b) put the new `async: true`
   `ToggleNavigation` test in `QfcItemController.EventWiringTests.cs` (126 lines headroom) with a comment
   cross-referencing #480; (c) add a new `QfcItemController.FocusAndThemeTests.Part2.cs`, which requires a
   `.csproj` edit — the precedent exists (`QfcItemController.InitializationTests.Part2.cs` / `.Part3.cs`,
   csproj lines 148-149) but it collides with the sibling-conflict constraint.
   **Recommendation: option (b).** It is the only zero-`.csproj` route with real headroom, and the #484/#485
   tests also need homes.
2. `QfcItemController.ViewerSetupTests.cs` has **26 lines of headroom** — enough for the two #484 timer tests
   (T1/T2, ~30 lines combined) only if written tightly, and not enough if the #485 tests also land there.
   **Recommendation:** #484 tests → `ViewerSetupTests.cs` (T1, ~18 lines); #484 T2 and all #485 tests →
   `MailActionsTests.cs` / `EventWiringTests.cs` where headroom is ample, with a header comment naming the
   issue. #485's extracted method is `static`, so its test has no natural file affinity.
3. `QfcItemController.ViewerSetup.cs` has **70 lines of headroom** for production. The #485 extraction
   (~28 lines including the doc comment), the two capture fields (~4 lines), and the `Cleanup()` changes
   (~6 lines) total roughly 40 lines. It fits, but with under 30 lines to spare afterwards. The plan should
   state this explicitly so no later task inflates the file past 500.

---

## 9. Consolidated recommendation

### 9.1 Selected approach

For each issue, one approach is recommended; rejected alternatives are summarised in §9.2.

| Issue | Recommended change | Owned file(s) | Testable? |
|---|---|---|---|
| #480 | Delete the unconditional toggle at `FocusAndTheme.cs:170` | `FocusAndTheme.cs` | Yes — exact-count Moq assertion |
| #481 | Add `UnwireEvents()` / `UnwireControlTreeEvents()` / `UnwireIntentEvents()` mirroring the three wire methods; call `UnwireEvents()` from `Cleanup()` before `_itemViewer`/`_kbdHandler` are nulled; guard all three for null collaborators; capture the `WebResourceRequested` delegate and its source in fields and detach | `EventWiring.cs`, `ViewerSetup.cs` | 22/24 yes; 1 already unwired; 1 (WebResourceRequested) code-only |
| #483 | Add `MoveFailureNotifier` seam + `NotifyMoveFailure` helper; log, notify via the dispatcher, and wrap-and-rethrow in the catch; add `Token.ThrowIfCancellationRequested()` as the first statement of `MoveMailAsync`, `FlagAsTaskAsync`, `EnumerateConversationAsync` | `MailActions.cs` | Yes |
| #484 | `_emailIsReadTimer?.Dispose();` before the null at `ViewerSetup.cs:424`; null `_mailActions` in `Cleanup()`; add the null guard to `ApplyReadEmailFormat` | `ViewerSetup.cs`, `FocusAndTheme.cs` | Yes — `ObjectDisposedException` on `Change`, plus direct callback invocation |
| #485 | Extract `internal static bool TryResolveCidResource(...)` with `Uri.TryCreate`, null-map, null-match, and null-`AttachmentData` guards; reduce the lambda to a two-statement adapter that also null-guards `ItemHelper` | `ViewerSetup.cs` | Yes — the extracted method is pure and directly callable |

### 9.2 Rejected alternatives (brief)

- **#483, return a failure result (`Task<bool>` or a result object).** Rejected: `Task MoveMailAsync()` is on
  the public `IQfcItemController` (`:78`) and implemented by `EfcItemController`; changing it requires writing
  two unowned files. Rethrow achieves the same caller-observable outcome with zero surface change.
- **#483, type-narrowed multi-catch.** Rejected: no narrow type set covers the real fault surface
  (`ArgumentNullException` + `InvalidOperationException` + `ObjectDisposedException` + `COMException`) without
  either omitting a real case or admitting `NullReferenceException`. The policy explicitly permits a broad
  catch that propagates with added context.
- **#483, introduce an `IUserPrompt`-style interface in `QuickFiler`.** Rejected: heavier than needed, and the
  assembly already has two instances of the delegate-property pattern (`EfcHomeController.ExecuteMoves.cs:22`,
  `QfcExplorerController.cs:56`). Matching existing style is required by the General Code Change Policy §7.1.
- **#484, `Timer.Dispose(WaitHandle)` in `Cleanup()`.** Rejected: introduces a blocking wait during teardown on
  the UI thread, with a real deadlock risk because the callback itself dispatches. The callback guard achieves
  the same safety without blocking.
- **#484, replace `System.Threading.Timer` with an injectable `ITimer`/`TimeProvider` seam.** Rejected as
  out of scope: the arming site is `Navigation.cs:223-224`, a **forbidden** file. The seam cannot be
  introduced without writing it. Recorded as a downstream note below.
- **#485, guard in place inside the lambda.** Rejected: closes the runtime fault but adds no covered lines and
  no regression test, because the enclosing member is `[ExcludeFromCodeCoverage]` and requires a live WebView2
  runtime. See §5.4.
- **#485, mock `CoreWebView2Environment`.** Rejected: no public constructor; the repository's
  `FormatterServices.GetUninitializedObject` technique (`BreadcrumbDropDownHostTests.cs:305`) produces an
  instance that can be passed through but not invoked.
- **#480, delete the one-argument overload entirely.** Rejected: it is on the public interface
  (`IQfcItemController.cs:89`) and implemented by `EfcItemController.cs:958`; deletion requires writing two
  unowned files.

### 9.3 DOWNSTREAM NOTES (out of scope; for the named owners)

1. **Feature 464 (`EfcItemController.cs`, issue #463) — the same timer defect exists.**
   `EfcItemController.Cleanup()` at `:277` does `_timer = null;` without disposing, while `_timer` is armed at
   `:953-954` with the identical `new System.Threading.Timer(ApplyReadEmailFormat)` + `Change(4000, Timeout.Infinite)`
   pattern. The owner should apply the same `?.Dispose()` + callback-guard fix. **Concretely:** replace
   `_timer = null;` with `_timer?.Dispose(); _timer = null;`, and add a null-collaborator early return to
   `EfcItemController.ApplyReadEmailFormat`.

2. **Feature 464 — `EfcItemController.Cleanup()` unwires only 3 of its subscriptions.**
   `:257-262` detaches `MouseEnter`/`MouseLeave` on buttons and `_globals.Ol.PropertyChanged`. Its
   `WireEventHandlers` equivalent (`EfcFormController.cs:375` and the `EfcItemController` wiring) subscribes
   more than that. The owner should audit for the same asymmetry this feature is fixing in
   `QfcItemController`. **Concretely:** grep `+=` across `EfcItemController.cs` / `EfcFormController.cs` and
   mirror each with a `-=` in `Cleanup()`, using the same delegate-identity technique documented in §2.2.

3. **Feature 444 (`Navigation.cs`, `KbdActions.cs`) — timer-seam opportunity.**
   `Navigation.cs:223-224` hard-constructs a `System.Threading.Timer` with a 4000 ms literal. A
   `Func<TimerCallback, ITimerHandle>` factory seam (mirroring the six factory-delegate seams already in
   `QfcItemController.cs:69-89`) would make `ToggleExpansionOn`'s arming branch and the read-format flow
   deterministically testable without the callback-guard workaround. **Concretely:** add
   `private Func<TimerCallback, System.Threading.Timer> _readTimerFactory;` defaulted in `SaveParameters`, and
   replace `new System.Threading.Timer(ApplyReadEmailFormat)` at `:223` with `_readTimerFactory(ApplyReadEmailFormat)`.
   This feature must not do it (`Navigation.cs` is forbidden) and does not depend on it.

4. **Feature 489 (`ItemViewer*.cs`) — WebView2 handler detachment on the viewer side.**
   If feature 489 introduces an `ItemViewer`-owned teardown for `L0v2h2_WebView2.CoreWebView2`, it would remove
   the need for `QfcItemController` to hold the `_coreWebView2`/`_webResourceRequestedHandler` capture fields
   added by §2.4. **Concretely:** an `ItemViewer.ResetWebResourceInterception()` intent member on `IItemViewer`,
   called from `QfcItemController.Cleanup()`, would replace the two capture fields with one interface call and
   would be mockable — turning the one untestable residue of #481 into a covered assertion. This feature does
   not create that member (it would require writing `ItemViewer*.cs` and `IItemViewer.cs`).

5. **Report-only, no issue yet: `Cleanup()` field asymmetries.** `_listTipsExpanded`, `_tlpStates`,
   `_conversationResolver`, `_activeTheme`, `_selectedFolder`, `_isWebViewerInitialized`, `_activeUI`, and
   `_expanded` are not reset by `Cleanup()` while their siblings are (§4.6). Duplicate assignments exist at
   `ViewerSetup.cs:412`/`:415` (`_folderHandler`) and `:407`/`:423` (`_itemViewer`). Recommend promoting this as
   a new potential rather than absorbing it here, since resetting `_isWebViewerInitialized` and `_activeUI` is a
   behaviour change on the pooled-reuse path with no issue backing it.

---

## 10. Testing implications (strategy only; no test code)

Per `.claude/rules/general-unit-test.md`, `CUT1`/`CUT2`, and the coverage floor in CLAUDE.md.

- **Framework/library:** MSTest `[TestClass]`/`[TestMethod]`, Moq for all collaborators, FluentAssertions for
  all assertions. Matches every existing test in the four owned files.
- **Determinism:** no `Thread.Sleep`, no `Task.Delay`, no wall-clock wait, no temp file. The #484 timer test
  arms with `Timeout.Infinite` so the timer can never fire, and asserts on the disposal state via
  `ObjectDisposedException` from `Timer.Change`. The #485 tests are pure-function calls.
- **Isolation:** every new test uses seam injection (`Mock<IItemViewer>`, `Mock<IApplicationGlobals>`,
  `Mock<IMailItemActions>`, `Mock<IAttachment>`, the `_emailFilerFactory` delegate, the new `MoveFailureNotifier`
  delegate) with the single exception of the #481 control-tree test, which must mirror the existing headless
  real-`ItemViewer` fixture at `EventWiringTests.cs:229-309` (no `Show()`, no message loop, no worker).
- **Scenario completeness per issue:**
  - #480 — sync branch exact count; async branch exact count; both assert `Times.Once()` on `Toggle(false)`.
  - #481 — intent: `VerifyRemove` for each of the 16 events (or a representative subset plus a
    "no event remains" assertion); control-tree: wire, unwire, raise, assert `Times.Never()`; teardown
    robustness: `Cleanup()` on a controller with null `_kbdHandler`/`Buttons`/non-`ItemViewer` `_itemViewer`
    must not throw (this protects the two existing `Cleanup()` tests).
  - #483 — happy path unchanged (existing `SeamFactoryTests.cs:191` must stay green); faulting
    `_emailFilerFactory` → asserts the wrapped rethrow, one `MoveFailureNotifier` invocation, and one
    `logger`-visible error; faulting `FilerQueue.Enqueue` via a null helper → `ArgumentNullException` wrapped;
    pre-cancelled `Token` → `OperationCanceledException` with the factory never invoked, for each of the three
    async members.
  - #484 — timer disposed and field nulled; `ApplyReadEmailFormat` after `Cleanup()` is a no-op and never calls
    `IMailItemActions.Save()`; `_mailActions` null after `Cleanup()`; `SaveParameters` after `Cleanup()` rebinds
    `_mailActions` to the new `MailItem` (proves the `??=` reuse fix).
  - #485 — six cases on `TryResolveCidResource`: malformed URI, relative URI, empty final segment, map miss,
    map hit with null `AttachmentData`, map hit with real bytes (asserting payload identity and the MIME type
    for a known extension). Plus one case asserting an unrecognised extension yields
    `"application/octet-stream"`.
- **Coverage posture:** all added production members except the `InitializeWebViewAsync` lambda adapter are
  fully coverable. `InitializeWebViewAsync` retains its existing `[ExcludeFromCodeCoverage]`
  (`ViewerSetup.cs:41`) and its documented residual barrier; no new exemption attribute is added anywhere.
  The plan should state explicitly that **no new `[ExcludeFromCodeCoverage]` is introduced by this feature**.
- **Regression-first ordering (Bugfix Workflow):** each of the five issues gets its failing test written and
  observed failing before the corresponding production change. #480's test is a *tightening* of an existing
  assertion, which must be demonstrated to fail against the unfixed code (it will: `Times.Once()` against two
  actual invocations).
