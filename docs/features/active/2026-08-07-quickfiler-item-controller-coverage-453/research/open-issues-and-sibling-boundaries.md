# F10 (#453) — Open-Issue Survey, Sibling Boundary Contracts, and the `InternalsVisibleTo` Wall

- Epic: #136 `quickfiler-per-file-coverage`, child F10 `quickfiler-item-controller-coverage` (issue #453)
- Branch: `feature/quickfiler-item-controller-coverage`
- Research date: 2026-08-07
- Companion artifact: `cross-cutting-exemption-and-coverage-analysis.md` (same folder)

---

## 1. Method and its limitations

`epic.md:651-653` states that a promoted-but-not-yet-active issue is invisible to a
`docs/features/active/` scan, and that children must search open GitHub issues by keyword. That
instruction was followed.

**Constraint:** no Bash/PowerShell tool was available in this session, so `gh issue list` and
`gh issue view` could not be run. The repository `drmoisan/TaskMaster` is publicly readable, so the
survey was performed against the public GitHub web surface instead.

**Reliability note that materially affects trust in this list.** The GitHub issues list view returns
only 12 rows per fetch through this channel. A first pass on the default (newest-first) list returned
12 issues and did **not** include #426 — an issue the epic itself names — even though #426 is open.
The survey was therefore completed by paginating in both sort directions and then confirming every
individual issue number by direct fetch of `/issues/<n>`. The resulting set contains exactly **36
open issues**, which matches the count GitHub reports on the issues page. Direct per-issue fetches
are the authoritative part of this survey; the list views are not. **A future session with `gh`
available should re-run `gh issue list --state open --limit 200` to reconfirm.**

Keyword searches were also run. One negative result is worth recording so it is not repeated:
searching `is:issue is:open ItemController` returns **no results**, because GitHub tokenises the
issue titles on hyphens and the titles use `item-controller`. Camel-case keyword searches against
this repository's issue titles are unreliable; enumerate instead.

---

## 2. Complete open-issue set (36)

| # | Title | Touches F10 file set? |
|---|---|---|
| 136 | Feature: quickfiler-80-per-file-coverage | Parent epic |
| **230** | **Build a WinForms message-pump test seam (`Application.Run()` background thread) to unblock 9 `QfcItemController` orchestration members** | **Yes — direct, blocking** |
| 285 | Bug: timeouttask-runwithtimeout-exception-type-mismatch | No |
| 286 | Bug: qfc-collectioncontroller-removespecificcontrolgroup-counter-leak | Indirect (F11-owned collaborator) |
| 287 | Bug: storewrapper-dialog-imprecise-for-genuine-failure | No |
| 394 | Bug: utilitiescs-test-cs2002-duplicate-compile-entry | No |
| 395 | Refactor: system-reactive-7-packages-config-migration | No |
| 426 | Bug: emailmovemonitor-rejected-item-hook-retention | No (F4) |
| **427** | **Bug: quickfiler-post-show-duplicate-scoring** | **Yes — names `QfcItemController.LoadFolderHandlerAsync`** |
| 430 | Feature: quickfiler-keyboard-actions-coverage (F3) | Sibling contract (§4.2) |
| 431 | Feature: quickfiler-queue-admission-coverage (F2) | No |
| 432 | Feature: quickfiler-coverage-ledger (F1) | Upstream dependency |
| 433 | Feature: quickfiler-qfc-home-controller-coverage (F7) | No |
| 434 | Feature: quickfiler-helper-classes-coverage (F4) | Sibling contract (§4.1, §4.4) |
| 435 | Feature: quickfiler-qfc-form-explorer-controller-coverage (F6) | No |
| 436 | Feature: quickfiler-datamodel-coverage (F5) | Sibling contract (§4.3) |
| 437 | Feature: quickfiler-efc-home-controller-coverage (F8) | No |
| **438** | **QuickFiler Search Keystroke Focus Steal** | **Yes — `TextBoxSearch_TextChanged`** |
| 439 | EfcViewer Missing Lineage and Segment Navigation | No (F9/EfcViewer) |
| **440** | **Bug: breadcrumb-left-right-arrow-parent-child-navigation** | **Yes — `OnBreadcrumbUnhandledArrow`** |
| **441** | **Cobertura post-processing double-counts `<line>` nodes, inflating lines-valid and every coverage rate** | **Yes — F10's measurement instrument** |
| 442 | Bug: qfc-home-controller-metrics-never-flushed | No |
| 443 | qfc-home-controller-metrics-duration-misread | No |
| **444** | **kbdactions-enumerable-ctor-bypasses-duplicate-guard** | Textual only (§3) |
| **445** | **Bug: quickfiler-keyboard-action-contract-defects** | Textual/consumer (§3) |
| 446 | Bug: iteratequeueasync-deadline-closes-queue-early | No |
| 447 | Refactor: qfc-home-controller-dead-iterate-paths | No |
| 448 | Bug: quickfiler-undoconsumer-nonterminating-loop | No |
| 449 | Bug: quickfiler-explorer-controller-latent-defects | No |
| 450 | Refactor: quickfiler-formcontroller-tests-file-size-split | Precedent only (§3) |
| 451 | Bug: efc-home-controller-metrics-inert-duration | No |
| 452 | Feature: quickfiler-efc-form-item-controller-coverage (F9) | No |
| **453** | **Feature: quickfiler-item-controller-coverage** | **This child** |
| 454 | Feature: quickfiler-collection-controller-coverage (F11) | Sibling contract (§4.5) |
| 455 | Feature: quickfiler-breadcrumb-dropdown-webview-coverage (F13) | No |
| 456 | Feature: quickfiler-itemviewer-coverage (F14) | Sibling contract (§4.6) |

### 2.1 CORRECTION — #400 and #424 are Closed

`epic.md:636-641` lists #400 (`quickfiler-folder-selector-dropdown`) and #424
(`quickfiler-high-confidence-queue-init-stall`) as "active on `main` concurrently with this epic".
Both were verified by direct fetch to be **Closed**. They are no longer conflict risks. Their feature
folders remain under `docs/features/active/`, which is presumably why the epic recorded them as live
— another instance of the folder-scan failure mode `epic.md:651` warns about, in the opposite
direction.

### 2.2 CORRECTION — the epic's conflict-risk list is incomplete for F10

`epic.md` names #400, #424 and #426 as the known conflict risks. For F10 the material risks are a
different set entirely: **#230, #427, #438, #440, #441**. None appears in `epic.md`. #230 in
particular predates the epic (opened 2026-07-03) and is the single largest determinant of F10's
achievable exemption count.

---

## 3. Per-issue overlap assessment for issues touching the F10 file set

### #230 — WinForms message-pump test seam (Open, opened 2026-07-03) — **blocking, semantic**

Title: "Build a WinForms message-pump test seam (`Application.Run()` background thread) to unblock 9
`QfcItemController` orchestration members". The body names
`Initialize(bool async)`, `InitializeAsync`, `InitializeGraphicsAsync`, `InitializeSequentialAsync`
and others, and states the barrier precisely: those members await continuations through
`WindowsFormsSynchronizationContext` on threadpool threads that have no message pump, which can hang
indefinitely.

Overlap: **this is the sole shared root cause of all four `irreducible-candidate` exemption sites**
identified in the companion artifact (`Initialization.cs:200,260,291` and `ViewerSetup.cs:253`). It
is not merely adjacent to F10 — it is the upstream enabler that decides whether F10's residual
exemption count is four or zero. (`Initialization.cs:403,436` — `CreateAsync` /
`CreateSequentialAsync` — were initially in this set but a solution-wide call-site grep shows they
are dead, so they are removed by deletion rather than by #230.)

Recommended handling: F10 must **not** attempt to build a general WinForms pump seam. That work is
outside F10's file assignment (it would live in `QuickFiler.Test` shared infrastructure or in
`UtilitiesCS`), it is already tracked, and the epic's decomposition did not budget for it. F10 should
instead cite #230 as the named, externally-tracked justification for the four retained attributes, and
record in its spec that if #230 lands the four become removable without further research. Conflict
class: **semantic** (a #230 fix changes what F10's ledger should say), not textual.

### #427 — quickfiler-post-show-duplicate-scoring (Open) — **semantic**

Body names `QfcItemController.LoadFolderHandlerAsync` explicitly: the dequeue gate computes folder
predictions that are discarded, then `LoadFolderHandlerAsync` recomputes identical classifications
after the form displays. The named method is `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:57-131`
— an F10-owned file whose branch coverage is 63.33% and which F10 must edit.

Overlap: a #427 fix would most likely add a "prediction already computed" short-circuit at the top of
`LoadFolderHandlerAsync`, changing both the line set and the branch set of a file F10 is measuring.
Conflict class: **semantic**. If #427 is scheduled while F10 is in flight, expect real merge
semantics, not just text. Recommended handling: F10 should not pre-empt or partially implement #427;
it should note in its plan that `LoadFolderHandlerAsync`'s branch structure may change.

### #438 — QuickFiler Search Keystroke Focus Steal (Open) — **semantic**

Body: typing into the folder-search textbox loses focus after one or two characters; the cause is
`TextChanged` opening a breadcrumb dropdown that steals focus.

The handler is `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:164-178`
(`TextBoxSearch_TextChanged`), and the offending call is `_itemViewer.SetFolderDroppedDown(true)` at
`:177` (with `_itemViewer.SetFolderSelectedIndex(1)` at `:176` as a secondary suspect). This is an
F10-owned file and this handler is already covered by
`QfcItemController.EventHandlersTests.cs:314`, which pins the current behaviour including the
`SetFolderDroppedDown(true)` call.

Overlap: a #438 fix will change or remove lines `172-177` and will require rewriting that existing
test. Conflict class: **semantic**. Recommended handling: F10 must not change this handler's
behaviour (no-behaviour-change NFR). It should, however, avoid adding *more* tests that pin
`SetFolderDroppedDown(true)`, so that #438 is not made harder to fix. Note also latent defect L9 in
the companion artifact (missing `_folderHandler` null guard on the same handler), which is plausibly
related.

### #440 — breadcrumb-left-right-arrow-parent-child-navigation (Open) — **semantic**

Body: left/right arrows fail to navigate parent-child relationships in the folder selectors across
QuickFiler and EfcViewer.

The QuickFiler-side entry point is
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:183-189` (`OnBreadcrumbUnhandledArrow`),
which forwards to `_kbdHandler.BreadcrumbArrowFallThrough(viewer, direction)`
(`QuickFiler/Interfaces/IQfcKeyboardHandler.cs:32-35`). The subscription/unsubscription lifecycle is
in `EnsureBreadcrumbPipeline` (`ViewerSetup.cs:148-157`) and `Cleanup` (`:396-401`) — both F10-owned,
and `EnsureBreadcrumbPipeline` is exemption site 18, which F10 intends to de-exempt.

Overlap: a #440 fix would most likely change the forwarding contract or add direction handling at or
below this call site. Conflict class: **semantic**, and it also crosses into F3's `KeyboardHandler.cs`.
Recommended handling: F10 should cover `OnBreadcrumbUnhandledArrow`'s existing routing behaviour
exactly (the test at `QfcItemControllerBreadcrumbDropDownTests.cs:156` already does) and add nothing
that constrains `BreadcrumbArrowFallThrough`'s semantics.

### #441 — Cobertura post-processing double-counts `<line>` nodes (Open) — **blocking, instrumental**

Body: `Invoke-MSTestWithCoverage.ps1` and its helper use an XPath descendant axis that matches
`<line>` elements twice — once under `<methods>` and again as class-level rollups — inflating
`lines-valid` and every rate computed by `Get-CoberturaCoverageSummary` and
`Merge-CoberturaClassesByFilename`.

This is the **same defect** the companion artifact derived independently from the committed
artifact's arithmetic (`cross-cutting-exemption-and-coverage-analysis.md` §2.1), and the derivation
is exact to six decimal places on two separate files. Independent confirmation from two directions.

Two consequences for F10, both important:

1. **The harness F10 is required to use for acceptance evidence (`epic.md` "Shared Design § 6") is
   the defective one.** Unless #441 is fixed first, F10's committed per-file numbers will not be the
   true per-file numbers. F10 must either (a) wait for #441, (b) compute its evidence from the
   class-level `<lines>` union directly and say so, or (c) commit both figures side by side with an
   explicit note. Option (c) is recommended: it satisfies the epic's "use F1's harness" directive
   while remaining truthful.
2. **A refinement worth adding to #441:** the title says "inflating", but the defect can deflate as
   well. Where covered lines are the ones duplicated (e.g. `QfcItemController.Conversation.cs`) the
   reported rate is too high (91.18% vs a true 88.24%); where a method-level entry is uncovered but
   the class-level union masks it via max-hits (e.g. `QfcItemController.Initialization.cs`, whose
   four `<InitializeAsync>b__115_0`-style closure methods report `hits="0"` at
   `coverage-final.cobertura.xml:23308-23332`) the reported rate is too low (90.11% vs a true
   91.79%). The error direction is data-dependent, so no uniform correction factor exists. This
   should be added as a comment to #441.

Conflict class: **instrumental, not textual** — #441's fix touches `scripts/`, not any F10 file.

### #444 — kbdactions-enumerable-ctor-bypasses-duplicate-guard (Open) — **textual only**

The defect is in `KbdActions<TKey, UClass, VDelegate>`'s `IEnumerable<UClass>` constructor
(`QuickFiler/Controllers/KbdActions.cs:26-29`), which bypasses the duplicate guard that
`Add(string, TKey, VDelegate)` enforces at `:90-104`.

F10 consumes `KbdActions` only through `Add(...)` and `Remove(...)`
(`QfcItemController.EventWiring.cs:157-389`) and, in tests, through `ContainsKey` and the indexer.
F10 never uses the enumerable constructor. **No conflict.** F3 (#430) owns the file.

### #445 — quickfiler-keyboard-action-contract-defects (Open) — **consumer-side, low risk**

Three contract defects in QuickFiler's keyboard-action types, verified at commit `56ca1cea` on the
epic branch, explicitly not resolved by #430 (which was behaviour-preserving).

F10 consumes those types (`KaChar`, `KaKey`, `KaCharAsync`, `KaKeyAsync` via
`IQfcKeyboardHandler.CharActions`/`KeyActions`/`CharActionsAsync`/`KeyActionsAsync`). If a #445 fix
changes `Add`/`Remove` contract semantics, F10's `EventWiring` tests that assert registration
membership could need updating. Conflict class: **textual to weakly semantic**. Low risk; note only.

### #450 — quickfiler-formcontroller-tests-file-size-split — **precedent, not conflict**

Relevant to F10 only as precedent: F10's two largest test files are at 498 and 497 of the 500-line
limit (`QfcItemController.FolderHandlingTests.cs`, `QfcItemController.FocusAndThemeTests.cs`). Any
new test for `.FolderHandling.cs` or `.FocusAndTheme.cs` must go into a new file, registered in
`QuickFiler.Test/QuickFiler.Test.csproj`. Following #450's approach keeps the two children
consistent.

### #286 — qfc-collectioncontroller-removespecificcontrolgroup-counter-leak — **indirect**

`QfcCollectionController` is F11-owned (#454). F10 calls
`_parent.RemoveSpecificControlGroup[Async](ItemNumber)` from the `'R'`/`'Z'` keyboard actions
(`QfcItemController.EventWiring.cs:197-201`, `:284-289`) and always through a
`Mock<IQfcCollectionController>`, so a #286 fix cannot break an F10 test. No conflict.

---

## 4. Sibling boundary contracts

`epic.md:266-297` asserts that child file sets are disjoint and that "no sibling edges exist among
F2-F15 because ... none consumes another's production contract". For F10 that assertion is **too
strong**: F10 does not *own* any sibling file, but it *consumes* six sibling-owned production
contracts, and its tests bind to their current shapes. Each is recorded below with the exact shape
F10 depends on, so that a sibling's additive change can be checked against it.

### 4.1 `ConversationResolver` — owned by F4 (#434), file `QuickFiler/Helper Classes/ConversationResolver.cs`

**Construction sites in the F10 file set — there is exactly one.**

`QuickFiler/Controllers/QfcItemController.Initialization.cs:382-388`, inside `SaveParameters`:

```csharp
_conversationResolverFactory ??= mail => new ConversationResolver(
    _globals,
    mail,
    _tokenSource,
    Token,
    SetTopicThread
);
```

Five positional arguments, in that order. It binds to the constructor at
`QuickFiler/Helper Classes/ConversationResolver.cs:70-76`:

```csharp
public ConversationResolver(
    IApplicationGlobals appGlobals,
    MailItem mailItem,
    CancellationTokenSource tokenSource,
    CancellationToken token,
    System.Action<List<MailItemHelper>> updateUI = null
)
```

**The exact shape F10 depends on:** the first four parameters must remain, in this order, with these
types, and the fifth must remain assignable from `System.Action<List<MailItemHelper>>`. F4 may append
further parameters **only** if they all have defaults. A two-argument overload also exists at
`ConversationResolver.cs:64` (`(IApplicationGlobals, MailItem)`); F10 does not use it directly, but
`ConversationResolver.LoadAsync` does internally (`:95`).

**Static-method site.** `QuickFiler/Controllers/QfcItemController.Conversation.cs:85-92`:

```csharp
ConversationResolver.LoadAsync(_globals, ItemHelper, tokenSource, token, loadAll, SetTopicThread)
```

Six positional arguments, binding to the `MailItemHelper` overload at `ConversationResolver.cs:126-133`
(there are three `LoadAsync` overloads, at `:86`, `:126` and `:164`; overload selection here depends
on `ItemHelper` being a `MailItemHelper`). **F10 depends on the `:126` overload's parameter order
`(IApplicationGlobals, MailItemHelper, CancellationTokenSource, CancellationToken, bool, Action<List<MailItemHelper>>)`.**
If F4 adds a fourth overload whose signature could make this call ambiguous, F10 breaks at compile
time.

**Member surface consumed** (all must survive F4 unchanged):
`ConversationResolver.Count.SameFolder` (`Conversation.cs:36,43,105,136`; `MailActions.cs:44`),
`ConversationResolver.Count.Expanded` (`FocusAndTheme.cs:294`),
`ConversationResolver.ConversationInfo.Expanded` (`Conversation.cs:121`; `FocusAndTheme.cs:296`),
`ConversationResolver.ConversationInfo.SameFolder` (`MailActions.cs:163`).

**Type-shape dependency, which is the important one.** `QfcItemController.ConversationResolver` is a
public property typed as the **concrete** `ConversationResolver`
(`QuickFiler/Controllers/QfcItemController.cs:109-114`) and is declared as such on the public
interface `IQfcItemController` (`QuickFiler/Interfaces/IQfcItemController.cs:69` —
`void PopulateConversation(ConversationResolver resolver)`). An `IConversationResolver` interface
exists in F4's assignment (`epic.md:352`), but **F10's contract is on the concrete type**. If F4
retypes this surface to the interface, that is a breaking change for F10 and for
`IQfcCollectionController.ToggleUnGroupConv` (called at `MailActions.cs:41-46`).

**Cross-child contract note for F4:** F10 requires the concrete `ConversationResolver` type to remain
usable as (a) the type of the `_conversationResolverFactory` delegate's return value, (b) the
parameter type of `IQfcItemController.PopulateConversation(ConversationResolver)`, and (c) the return
type of `ConversationResolver.LoadAsync`. Appending defaulted parameters is safe; retyping to
`IConversationResolver` is not.

**Can F10 reach both gates without an F4 change?** `.Conversation.cs` is at 88.24% line / 94.44%
branch today and needs +8 denominator lines when site 1 is de-exempted. Its 12 uncovered lines
(`130-139`, `212-214`) are all reachable through `Mock<IItemViewer>` + `BuildSyncDispatcher` with a
resolver instance supplied by the injectable factory. **Yes — no upstream change required.**

### 4.2 `KeyboardHandler.cs` / keyboard-action types — owned by F3 (#430)

**F10 must not edit `QuickFiler/Controllers/KeyboardHandler.cs`.** It does not need to: every
reference from the F10 file set is through the interface
`QuickFiler/Interfaces/IQfcKeyboardHandler.cs`, held in the private field `_kbdHandler`
(`QfcItemController.cs:49`), assigned once from `_homeController.KeyboardHandler`
(`Initialization.cs:372`).

Complete reference list from the F10 file set:

| Member consumed | Declared at | Call sites in F10 files |
|---|---|---|
| `ToggleKeyboardDialog()` | `IQfcKeyboardHandler.cs:12` | `Navigation.cs:29,53,67,76` |
| `ToggleKeyboardDialogAsync()` | `:14` | `Navigation.cs:42,60` |
| `KeyboardHandler_PreviewKeyDownAsync` | `:16` | `EventWiring.cs:40-42` |
| `KeyboardHandler_KeyDownAsync` | `:18` | `EventWiring.cs:44-46` |
| `CharActions` (`KbdActions<char, KaChar, Action<char>>`) | `:21` | `EventWiring.cs:169-207, 338-348, 381-382` |
| `CharActionsAsync` (`KbdActions<char, KaCharAsync, Func<char, Task>>`) | `:22` | `EventWiring.cs:226-299, 322-331, 360-372, 387-388` |
| `KeyActions` (`KbdActions<Keys, KaKey, Action<Keys>>`) | `:23` | `EventWiring.cs:159-168, 336-337` |
| `KeyActionsAsync` (`KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>`) | `:24` | `EventWiring.cs:221-225, 359` |
| `CboFolders_KeyDownAsync` | `:28` | `EventWiring.cs:81-83` |
| `BreadcrumbArrowFallThrough(ItemViewer, BreadcrumbArrowDirection)` | `:32-35` | `ViewerSetup.cs:187` |

**The contract F10 relies on**, precisely: `IQfcKeyboardHandler` must keep the four `KbdActions<...>`
properties with **exactly** their current three type arguments, and `KbdActions<TKey, UClass, VDelegate>`
must keep `Add(string sourceId, TKey key, VDelegate @delegate)` (`KbdActions.cs:90`),
`Remove(string sourceId, TKey key)` (`:123`), `ContainsKey(TKey)` (`:49`) and the indexer
`public VDelegate this[TKey key]` (`:36-47`). The indexer is load-bearing for F10's plan: it is how a
test retrieves a registered lambda and **invokes** it, which is the only way to cover the 32
registered-lambda lines in `.EventWiring.cs`.

`KbdActions<>` is `public` and lives in `QuickFiler`, not `UtilitiesCS`, so no visibility problem
arises. `IQfcKeyboardHandler` is `public` and fully mockable — the existing fixture already builds
real `KbdActions` instances behind a `Mock<IQfcKeyboardHandler>`
(`QfcItemController.EventWiringTests.cs:41-53`).

**Can F10 reach both gates without an F3 change?** `.EventWiring.cs` needs its 32 registered-lambda
lines, its 12 `if (_expanded)` tail lines, and the four `((ItemViewer)_itemViewer)`-bound expanded
lambdas. The first two need only the existing fixture; the third needs the headless real-`ItemViewer`
scope that already exists. The one genuinely unreachable block is the `Task.Delay` polling loop at
`:121-137` (latent defect L6). Line coverage after removing those 11 lines from consideration:
`(247 + 32 + 12 + 4) / 306 = 92.5%`. Branch: the six uncovered conditions at `:124`, `:128`, `:208`,
`:300`, `:349`, `:373` — four of the six (`:208`, `:300`, `:349`, `:373`) are the `_expanded` tails
and are reachable, giving `17/20 = 85%`. **Yes — no upstream change required.**

### 4.3 `IQfcDatamodel` — owned by F5 (#436)

**F10 has no reference to `IQfcDatamodel` at all.** Verified: no occurrence of `IQfcDatamodel`,
`QfcDatamodel`, or `EfcDataModel` in any of the ten `QfcItemController*.cs` files or in
`Interfaces/IQfcItemController.cs`. The datamodel is consumed by `QfcHomeController` (F7) and
`QfcCollectionController` (F11), not by the item controller. **No contract, no risk.** The brief's
inclusion of `IQfcDatamodel` in the sibling list is a false positive for F10 — recorded here so the
plan does not carry a phantom dependency.

### 4.4 `QfcThemeHelper`, `Theme`, `TlpCellSnapShot` — owned by F4 (#434)

Three further F4-owned surfaces, none listed in the brief but all load-bearing for F10:

- **`QfcThemeHelper.SetupThemes(this, (ItemViewer)_itemViewer, this.HtmlDarkConverter, _uiDispatcher)`**
  — called from `Initialization.cs:175-180, 209-214, 266-273, 299-304`. Four-argument shape. This is
  the residual barrier on exemption site 9 (`Initialize(bool async)`). If F4 introduces an
  `IItemViewer`-typed overload, site 9 becomes fully removable. **Cross-child contract note for F4:**
  an `IItemViewer`-accepting overload of `SetupThemes` (additive, non-breaking) would let F10 remove
  exemption sites 8 and 9. F10 must not add it itself — `QfcThemeHelper.cs` is F4's file.
- **`Theme`** (`UtilitiesCS`, not F4) — consumed as `_themes[_activeTheme].SetQfcTheme(bool)`,
  `.SetQfcThemeAsync()`, `.SetMailRead(bool)`, `.ButtonMouseOverColor`, `.ButtonClickedColor`,
  `.ButtonBackColor` (`FocusAndTheme.cs:64,80,120,135,279,284,307,312,321`;
  `EventHandlers.cs:139,144,151,155,161`; `MailActions.cs:179,197`). Already fully handled by
  `QfcItemControllerTestSupport.BuildColorTheme` / `BuildDispatchableTheme`.
- **`TlpCellStates` / `TlpCellSnapShotList.ApplyState(IContainerControlLocal)`**
  (`QuickFiler/Helper Classes/TlpCellSnapShot.cs:12, 72, 192`) — consumed at
  `Navigation.cs:209, 219`. **This retrofit is already complete on this branch**, which is what makes
  exemption sites 15 and 16 `removable-as-is` (see companion artifact §1.2). **Cross-child contract
  note for F4:** F10's ability to remove those two attributes depends on `ApplyState` continuing to
  accept `IContainerControlLocal` rather than a concrete `Control`. Reverting that signature would
  re-block F10.

### 4.5 `IQfcCollectionController` — owned by F11 (#454)

Consumed through the `_parent` field (`QfcItemController.cs:44`), always via the interface
`QuickFiler/Interfaces/IQfcCollectionController.cs`: `PopOutControlGroup[Async](int)`,
`RemoveSpecificControlGroup[Async](int)`, `ToggleGroupConv(string)`,
`ToggleUnGroupConv(ConversationResolver, string, int, ...)`, `ToggleExpansionStyle[Async](int, ToggleState)`.
All are mocked in existing tests. Note `ToggleUnGroupConv`'s first parameter is the **concrete**
`ConversationResolver` (`MailActions.cs:41-46`) — the same type-shape dependency as §4.1, now
spanning three children (F4 owns the type, F10 passes it, F11 receives it). **Cross-child contract
note:** retyping `ConversationResolver` to an interface is a three-child breaking change, not a
two-child one.

### 4.6 `ItemViewer` / `IItemViewer` — owned by F14 (#456)

`QuickFiler/Viewers/IItemViewer.cs:15` — `public interface IItemViewer : IUserControl, IContainerControlLocal`.
F10 binds to it in two distinct ways, and the distinction matters:

1. **Through the interface** (mockable): `InvokeRequired` (`:125`), `Invoke`, `BeginInvoke`,
   `UiDispatcher` (`:36`), `UiSyncContext` (`:38`), `TipsLabels` (`:35`), `ExpandedTipsLabels`
   (`:18`), `MenuItems` (`:34`), and the whole cycle-1 "intent member" set (`SenderText`,
   `SubjectText`, `BodyText`, `ConversationModeChecked`, `SetFolderItems`, `SetFolderSelectedIndex`,
   `GetSelectedFolder`, `FolderContains`, `NavigateToString`, `SetConversationItems`, and the
   intent events).
2. **Through a concrete `(ItemViewer)` cast** — 12 sites:
   `Initialization.cs:172,177,207,211,264,269,297,301`; `ViewerSetup.cs:66,76,109,112,205,254`;
   `EventWiring.cs:37,50,311,316,325,330`. These are the sites that force either a headless real
   `ItemViewer` or an exemption.

**Cross-child contract notes for F14:** F10 depends on (a) `IItemViewer` continuing to derive from
`IContainerControlLocal` (§4.4), (b) `new QuickFiler.ItemViewer()` remaining constructible headlessly
under a plain `SynchronizationContext` — already relied on by six passing tests, and (c) the
concrete members reached by cast (`L0v2h2_WebView2`, `L0vhBreadcrumb_WebView2`, `TopicThread`,
`LblItemNumber`, `GetAllChildren()`, `ForAllControls(...)`, `BreadcrumbCoordinator`,
`InitializeBreadcrumbPipeline`, `BreadcrumbUnhandledArrow`, `ResetBreadcrumb`,
`ConfigureBreadcrumbDropDown`, `SetBreadcrumbTheme`, `AttachBreadcrumbWebViewAsync`) keeping their
current names and signatures.

**Non-mockable member to be aware of:** `IItemViewer.UiDispatcher` is a concrete, sealed
`System.Windows.Threading.Dispatcher` (`IItemViewer.cs:36`), not the injectable `IUiDispatcher`. That
is why `QfcItemControllerTestSupport.StartRunningDispatcher()` exists. F10 should keep using that
helper rather than proposing a re-type — re-typing `IItemViewer` is F14's decision.

### 4.7 `FlagTasks` — owned by no epic child (`TaskVisualization`, outside the epic)

This is the one place where F10 cannot reach the gates without a change, and it is worth stating
precisely because the change is **F10-local**, not a sibling edit.

`_flagTasksFactory` is typed
`Func<IApplicationGlobals, List<MailItem>, bool, IntPtr, FlagTasks>` (`QfcItemController.cs:70-76`),
returning the **concrete** `TaskVisualization.FlagTasks`. That type:

- has a non-virtual `public DialogResult Run(bool modal = false)` (`TaskVisualization/FlagTasks.cs:89`)
  which calls `_viewer.ShowDialog()` at `:95` — a modal dialog, prohibited in unit tests;
- has a constructor (`FlagTasks.cs:43-84`) that calls `globals.Ol.App.ActiveExplorer()` at `:52` and
  can raise a `MessageBox` at `:56-61`;
- carries `[ExcludeFromCodeCoverage]` on both the constructor (`:42`) and `Run` (`:88`).

Because `Run` is not virtual, `Mock<FlagTasks>` cannot override it, and because the factory's return
type is concrete, a test cannot substitute an interface. The existing tests therefore throw a
`SentinelException` from the factory (`QfcItemController.SeamFactoryTests.cs:110,135`), which is
exactly why `MailActions.cs:176-181` and `:194-200` (13 lines, 4 branch conditions) are uncovered.

**Recommended F10-local remedy** (does not touch `TaskVisualization`, does not touch any sibling
file): add one further **optional** constructor parameter and private field, defaulted in
`SaveParameters` alongside the six existing factory seams —

```
Func<FlagTasks, bool, DialogResult> flagTasksRunner = null
...
_flagTasksRunner ??= (flagTask, modal) => flagTask.Run(modal);
```

and change `FlagAsTask` / `FlagAsTaskAsync` to call `_flagTasksRunner(flagTask, modal: true)`. This
is additive and non-breaking (all existing call sites keep compiling), it follows the established
seam pattern in this exact file, and it leaves a single uncovered default-lambda line instead of 13
uncovered lines behind a modal dialog. The same technique applies to the `MessageBox.Show` at
`MailActions.cs:119-121` (latent defect L4) — a `Action<string>` prompt seam defaulted to
`MessageBox.Show` unblocks the `MoveMailAsync` catch path.

### 4.8 Can F10 reach >= 80% line and >= 75% branch without any upstream change?

| File | Post-removal denominator | Blocked lines requiring upstream work | Achievable without upstream change? |
|---|---|---|---|
| `QfcItemController.cs` | 73 | none | Yes (100% / 78.57%; 3 more branch conditions available at `:254`) |
| `.Initialization.cs` | 210 if sites 9-12 all removed; **148 if the three #230-blocked sites are retained** | sites 10, 11, 12 need #230; sites 8, 13, 14 are dead and deleted | **Yes, if the three #230-blocked attributes are retained.** Deleting sites 8/13/14 costs nothing. Removing site 9 (`Initialize(bool)`, +14, live and heavily used) gives 148 total; the 11 currently-uncovered lines are 9 closure lines (blocked) + 2 default-lambda lines (reachable). Reaching 80% needs +8 of the 14 new plus the 2 lambda lines — feasible with a headless-viewer `Initialize(false)` test. |
| `.ViewerSetup.cs` | 240 if all 3 removed; **183 with site 19 retained** | site 19 needs #230; site 17's WebView2 core is irreducible | Yes with sites 17/19 retained and site 18 removed: 116 + 23 (site 18) + 10 (`178`, `195-202`, `424`) = ~149/183 = 81.4%. Branch: +12 from `ResolveImageMimeType` alone lifts 30/54 to 42/54 = 77.8%. |
| `.Conversation.cs` | 110 | none | Yes — 88.24% today; +12 reachable lines gives ~92%. |
| `.FolderHandling.cs` | 147 | none | Yes — 87.76% line already; branch 63.33% -> ~85% by exercising the three `?.` interpolations and the `InvokeRequired` branch. |
| `.EventWiring.cs` | 306 | `:121-137` polling loop (needs a delay seam — latent defect L6) | Yes — ~92.5% line, ~85% branch excluding the loop. |
| `.EventHandlers.cs` | 128 | none | Yes — the `!SuppressEvents` body alone (9 lines, 2 branches) plus the five de-exempted shells reaches ~90%. |
| `.Navigation.cs` | 142 | none | Yes — sites 15/16 are `removable-as-is`; 90.68% -> ~93%. |
| `.FocusAndTheme.cs` | 237 | none | Yes — all 61 uncovered lines are private-field-gated; 74.26% -> ~97%. Largest single win. |
| `.MailActions.cs` | 125 | `FlagTasks.Run` and `MessageBox.Show` — **remedied F10-locally** per §4.7 | Yes, with the two F10-local seams. Without them: 96 + 9 (RightKeyActions lambdas) = 105/125 = 84% line, but branch stalls near 72.7%, **below the 75% gate**. |

**Bottom line: F10 can meet both gates on all ten files without any sibling edit**, provided it (a)
deletes the three dead members (`Initialize(9 params)`, `CreateAsync`, `CreateSequentialAsync` —
zero call sites solution-wide), (b) retains the four #230-blocked attributes with #230 cited as the
justification, and (c) adds the two additive, F10-local seams described in §4.7. The only genuine cross-child *requests* — neither of
which F10 should make itself — are the optional `QfcThemeHelper.SetupThemes(IItemViewer, ...)`
overload from F4 (§4.4) and the delivery of #230.

---

## 5. `InternalsVisibleTo` wall — verified on this tree

**`epic.md:619-631` is confirmed accurate.** `UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants:

```
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("UtilitiesCS.Test")]
[assembly: InternalsVisibleTo("ToDoModel.Test")]
```

There is **no grant to `QuickFiler.Test`**. Any `internal` `UtilitiesCS` type or member is therefore
unreachable at compile time from a QuickFiler test.

By contrast, `QuickFiler/Properties/AssemblyInfo.cs:5` **does** grant
`[assembly: InternalsVisibleTo("QuickFiler.Test")]`, which is why `internal partial class
QfcItemController` and all its `internal` members are directly testable.

### 5.1 Does F10 hit the wall?

**No compile-time block was found.** Enumerating every `UtilitiesCS` surface the F10 file set
depends on, each is public:

| `UtilitiesCS` surface | Where used in F10 files | Accessibility |
|---|---|---|
| `IApplicationGlobals`, `IOlObjects`, `IAppQuickFilerSettings` | throughout | public |
| `MailItemHelper` (`FromMailItemAsync`, `EntryId`, `Html`, `ToggleDark`, `UnRead`, ...) | `ViewerSetup.cs`, `Conversation.cs`, `EventWiring.cs`, `FocusAndTheme.cs` | public |
| `FolderPredictor`, `FolderPredictor.InitOptions`, `IFolderSearchHandler` | `FolderHandling.cs:31-128` | public |
| `EmailFiler`, `EmailFilerConfig` | `MailActions.cs:100-110` | public |
| `Theme` | `FocusAndTheme.cs`, `EventHandlers.cs`, `MailActions.cs` | public |
| `UtilitiesCS.Threading.IUiDispatcher`, `WpfUiDispatcher` | `QfcItemController.cs:66`, `Initialization.cs:380` | public |
| `UtilitiesCS.Threading.UiThread` | reached only from `TestSupport.cs:240` | public type; private static field reached by reflection |
| `CidImageResolver` (`DefaultVirtualHost`, `BuildContentIdMap`) | `ViewerSetup.cs:78,89` | `public static class` (`UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs:14,20,34`) |
| `IAttachment` (`AttachmentData`, `FileExtension`) | `ViewerSetup.cs:96,95` | public interface (`UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs:8,13`) |
| `OutlookFolderHierarchyProvider`, `BreadcrumbArrowDirection`, `IContainerControlLocal` | `ViewerSetup.cs:142`, `:183`, `IItemViewer.cs:15` | public |

Where F10's tests do reach non-public state, they do so by **reflection over public types**, which
`InternalsVisibleTo` does not govern: `Theme._uiDispatcher` and `Theme._lblSender`
(`TestSupport.cs:174-176, 204-209`) and `UiThread._dispatcher` (`TestSupport.cs:240-248`). This is
the existing, working pattern; it needs no grant and F10 should continue it.

### 5.2 Where F10 meets the *substance* of F3's problem, without meeting the wall

F3 hit the wall on `MyBox.DialogInvoker`, a `UtilitiesCS` internal, and resolved it by building a
local dialog seam rather than widening the grant (`epic.md:626-631`). F10 has the same *kind* of
problem in two places, but from a different direction — the offending calls are direct framework
calls in QuickFiler's own code, so there is nothing to reach into `UtilitiesCS` for:

1. `QfcItemController.MailActions.cs:119-121` — `System.Windows.Forms.MessageBox.Show(...)` called
   directly (not through `MyBox`). Blocks the `MoveMailAsync` catch path.
2. `QfcItemController.MailActions.cs:176, 194` — `flagTask.Run(modal: true)` reaching
   `TaskViewer.ShowDialog()` in `TaskVisualization` (§4.7).

**Both are resolved entirely inside F10's own assignment** by the additive optional-seam pattern
already used seven times in `SaveParameters` (`Initialization.cs:380-397`). F10 therefore follows
F3's precedent — build a local seam, do not widen the internals grant, do not edit
`UtilitiesCS/Properties/AssemblyInfo.cs` — but does not need to reach into `UtilitiesCS` at all.

**Conclusion: F10 does not hit the `InternalsVisibleTo` wall.** No grant change is needed or should
be proposed.

---

## 6. Actions this survey implies for the F10 plan

1. Record #230 in the spec as the named, externally-tracked justification for the four retained
   `[ExcludeFromCodeCoverage]` sites. Do not attempt to build a WinForms message-pump seam.
1b. Delete the three dead members (`Initialization.cs:139` `Initialize(9 params)`, `:404`
   `CreateAsync`, `:437` `CreateSequentialAsync`) after confirming no reflection caller, following
   the #447 precedent. This removes three exemptions at zero coverage cost. Do not write tests for
   them.
2. Decide the #441 posture explicitly before the first coverage run: commit both the harness figure
   and the class-level-union figure, with a note. Add the "can deflate as well as inflate" refinement
   as a comment on #441.
3. Add the two additive F10-local seams (`flagTasksRunner`, prompt delegate) in the same phase as the
   `.MailActions.cs` tests, not before.
4. Do not change `TextBoxSearch_TextChanged` behaviour and do not add tests that further pin
   `SetFolderDroppedDown(true)` — #438 must remain fixable.
5. Do not constrain `BreadcrumbArrowFallThrough` semantics beyond the routing already asserted —
   #440 must remain fixable.
6. Note in the plan that `LoadFolderHandlerAsync`'s branch structure may change under #427.
7. Promote latent defects L1-L12 (companion artifact §5) via the MCP promotion lifecycle before F10
   completes. L4 and L6 should be promoted with an explicit note that they are also permanent
   coverage blockers.
8. Send two cross-child contract notes: to F4 (#434) — do not retype `ConversationResolver` to
   `IConversationResolver`, do not revert `TlpCellSnapShotList.ApplyState` away from
   `IContainerControlLocal`, and consider an additive `SetupThemes(IItemViewer, ...)` overload; to
   F14 (#456) — keep `IItemViewer : IContainerControlLocal` and keep the cast-reached concrete
   members stable.
9. Add new test files (not extensions of `FolderHandlingTests.cs` at 498 lines or
   `FocusAndThemeTests.cs` at 497) and register each in `QuickFiler.Test/QuickFiler.Test.csproj`,
   following the #450 precedent.
