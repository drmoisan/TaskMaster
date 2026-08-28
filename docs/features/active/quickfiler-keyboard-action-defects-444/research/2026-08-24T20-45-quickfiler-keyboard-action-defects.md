# Research — quickfiler-keyboard-action-defects (issues #444, #472, #482)

- **Feature folder:** `docs/features/active/quickfiler-keyboard-action-defects-444`
- **Primary issue:** #444. Also closes #472 and #482.
- **Work mode:** `full-bug`
- **Epic:** `quickfiler-bug-family`, wave 1. Upstream dependency: issue #468 (`qfc-collection-controller-defects`), prepared but not yet landed as code.
- **Research timestamp:** 2026-08-24T20-45
- **Source commit inspected:** `988e819b` (branch head of the isolated research worktree; `git diff --stat origin/main origin/epic/quickfiler-bug-family-integration -- QuickFiler/` was reported empty by the delegating orchestrator, so `988e819b` is byte-equivalent to the integration branch for all `QuickFiler/` paths).
- **Tooling constraint (disclosure):** this research session had no shell/Bash tool. Every fact below is grounded in a direct file read of the working tree or of the checked-out epic-integration worktree. No `git show`, `git log`, or `git diff` was executed by this agent. Statements that would require running a command are marked `UNVERIFIED`.

Path placeholders: `<repo-root>` is the workspace root. `<CTRL>` is `QuickFiler/Controllers/QfcCollectionController.cs`. `<NAV>` is `QuickFiler/Controllers/QfcItemController.Navigation.cs`. `<WIRE>` is `QuickFiler/Controllers/QfcItemController.EventWiring.cs`. `<KBD>` is `QuickFiler/Controllers/KbdActions.cs`.

---

## 0. Policy inputs read

- `CLAUDE.md` (all four embedded policies) — read in full.
- `.claude/rules/general-code-change.md` — 500-line file cap; mandatory format → lint → type-check → test loop with restart-on-change.
- `.claude/rules/general-unit-test.md` — determinism infrastructure; banned APIs in test code (`Thread.Sleep`, `Task.Delay`, real wall-clock waits); no temporary files; tests mirror production layout.
- `.claude/rules/csharp.md` — CSharpier via `dotnet tool run`; `/t:Rebuild` for both MSBuild gates; no `/p:Nullable=enable`; MSTest + Moq + FluentAssertions; DI-seam preference order.

Note on the coverage floor: `CLAUDE.md` § UT2 states `>= 80%` repo-wide and `>= 90%` for new members, and explicitly names `KbdActions<>` as a **testable seam that is NOT exempt**. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state `>= 85%` line / `>= 75%` branch. These two figures conflict. This is a pre-existing repository-wide conflict (recorded previously under issue #494) and is **not** created by this feature; planning should cite `CLAUDE.md`'s 80/90 as the binding pair because `CLAUDE.md` is first in the stated policy-compliance order, and should record the conflict rather than silently pick one.

---

## 1. Reproduction verdict table (against `988e819b`)

| Defect | Verdict at `988e819b` | Anchor evidence | Line drift vs. promoted document |
| --- | --- | --- | --- |
| **#444** — enumerable ctor bypasses the duplicate guard | **REPRODUCES as a latent (dormant) defect.** The guard-free constructor and the duplicate seed both exist verbatim. The exception is **not reachable at runtime** because the only method that seeds the duplicate has zero callers. | Ctor with no guard: `<KBD>:26-29`. `Add(string,TKey,VDelegate)` guard: `<KBD>:90-98`. `Add(UClass)` guard: `<KBD>:106-121`. Duplicate seed: `<CTRL>:1265-1272`, two `KaKey("Collection", Keys.Down, …)` at `:1269` and `:1270`. `Find` ambiguity throw: `<KBD>:63-67`. `FindIndex` ambiguity throw: `<KBD>:81-86`. Consumer: `QuickFiler/Controllers/KeyboardHandler.cs:118-123` (`KeyActions[e.KeyCode]` → indexer → `Find`). | Promoted doc cited the ctor at `:25-28`; actual is `:26-29` (**+1**). `Add` cited at `:90-92`; actual guard block `:92-98` within the method `:90-104` (no material drift). `<CTRL>:1265-1272` is **exact**. `KeyboardHandler.cs:122` is **exact**. |
| **#472** — register/unregister digit-width desync | **REPRODUCES.** Both halves of the asymmetry are present verbatim, and a reachable count-drift path exists (see §1.1). | Single capture: `<CTRL>:1332` (`var digits = Digits;`) inside `RegisterNavigation` `:1330-1341`. Per-iteration re-evaluation: `<CTRL>:1347` (`if (Digits == 1)`) inside `UnregisterNavigation` `:1343-1356`. Live side-effecting `Digits`: `<CTRL>:114-128` (reads `_itemGroups?.Count >= 10 ? 2 : 1`, sets `_digitRefreshNeeded` and `_digits`, `[MethodImpl(MethodImplOptions.Synchronized)]`). Silent `false`: `<KBD>:123-135`. Delayed throw: `<KBD>:90-98`. | Every cited range is **exact**: `:1330-1341`, `:1343-1356`, `:114-128`, `<KBD>:123-135`, and `<KBD>:90-98`. |
| **#482** — expansion registry divergence | **REPRODUCES**, but the promoted document's stated trigger is **wrong**. The divergence is real; the specific `ActivateBySelectionAsync` → synchronous `ToggleExpansion()` trigger at `<CTRL>:1439` is **currently unreachable with `blExpanded == true`** (see §1.2). A different, live trigger exists. | Sync toggle: `<NAV>:174-187`, registering via `RegisterExpandedActions()` `:180` / `UnregisterExpandedActions()` `:185`. Async toggle: `<NAV>:192-205`, registering via `RegisterExpandedAsyncActions()` `:198` / `UnregisterExpandedAsyncActions()` `:203`. Shared flag `_expanded`: declared `QuickFiler/Controllers/QfcItemController.cs:146`, written at `<NAV>:210` and `<NAV>:220`. Non-idempotent `Add`: `<KBD>:90-98`. | `<CTRL>:1439` is **exact** as a line citation. The `QuickFiler/Controllers/QfcItemController.Navigation.cs` attribution is correct for the two *toggle* methods, but the four *registration* methods the document describes live in `<WIRE>:306-332` and `<WIRE>:379-389` — a file this feature must not write. |

### 1.1 #472 — the reachable trigger, and a second defect the promoted document does not name

The promoted document's scenario ("allow the item count to drop below 10 … before `UnregisterNavigation` runs") requires a path that mutates `_itemGroups` **without** an intervening `UnregisterNavigation`. Such a path exists and is live:

- `RemoveSpecificControlGroup(int)` at `<CTRL>:1105-1155` removes a group (`:1127` `_itemGroups.RemoveAt(selection - 1)`) and renumbers (`:1132`), and performs **no** navigation unregister or register. Verified by reading the whole method body.
- It is reached unbracketed from three places:
  - `RemoveBelowThresholdAsync` → `RemoveGroupByEntryId` seam (`<CTRL>:1069-1074`, invoked in a loop at `:1093-1096`) → `RemoveSpecificControlGroup(string)` (`:1053-1058`) → `RemoveSpecificControlGroup(int)`. A page of 10+ items dropping several below-threshold items crosses the boundary in one call.
  - The `'R'` character action registered at `<WIRE>:197-201` (`_parent.RemoveSpecificControlGroup(ItemNumber)`).
  - `RemoveSpecificControlGroup(string)` directly.
- By contrast, `RemovedItemMonitor` (`<CTRL>:1046-1051`) *does* bracket correctly (`UnregisterNavigation()` `:1048`, `RegisterNavigation()` `:1050`), and `RemoveSpecificControlGroupAsync` unregisters first at `:1162` and re-registers at `:1245`.

**Second defect, not named in the promoted document:** `UnregisterNavigation` bounds its loop with the *current* `_itemGroups.Count` (`<CTRL>:1345`), not the count that was registered. A capture-`Digits`-once fix does **not** address this. After an unbracketed removal, the loop under-iterates and the highest registered key is orphaned regardless of digit width. This is the same failure family and is reachable by the same paths.

**Consequence for the fix design:** a fix that merely hoists `var digits = Digits;` above the loop **does not fix the filed defect**. Hoisting still computes the *current* width, so the 10→9 crossing still removes `"1".."9"` where `"01".."09"` were registered. See §5.2.

### 1.2 #482 — the promoted document's trigger is unreachable; a live trigger exists

`<CTRL>:1439` (`itemController.ToggleExpansion()` inside `ActivateBySelectionAsync`) is guarded by `if (blExpanded)` at `:1437`. Both callers pass `false` on the async path:

- `<CTRL>:1480` — `await ActivateBySelectionAsync(idx + 1, expanded)` in `ChangeByIndexAsync`, where `expanded` comes from `await ToggleOffActiveItemAsync(false)` at `:1479`. `ToggleOffActiveItemAsync` (`<CTRL>:1687-1701`) has its expansion branch **commented out** at `:1694-1698`, so it returns `parentBlExpanded` unchanged — always `false` here.
- `<CTRL>:1661` — `await ActivateByIndexAsync(ActiveIndex, false)`, a literal `false`.

So `<CTRL>:1439` is currently dead with respect to the interleaving. **State this plainly in `spec.md`; the issue text is inaccurate on this point.**

The interleaving is nonetheless live. The reachable sequence, fully grounded:

1. Only the **async** focus registration runs in the QuickFiler surface. `RegisterFocusActions()` / `UnregisterFocusActions()` are commented out at `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:45`, `:61`, `:101`, `:117`; the live calls are `RegisterFocusAsyncActions()` at `:46`, `:118`, `:150` and `UnregisterFocusAsyncActions()` at `:62`, `:102`, `:165`.
2. `Keys.Right` is bound to `ToggleExpansionAsync()` at `<WIRE>:221-225`, and `'E'` is bound to `ToggleExpansionAsync()` at `<WIRE>:260-264`. Both live in `CharActionsAsync` / `KeyActionsAsync`, which is what the live handler reads (`KeyboardHandler.cs:150-204`, `KeyDownTaskAsync`, consults `AlwaysOnKeyActionsAsync`, `KeyActionsAsync`, `CharActionsAsync`, `StringActionsAsync` only).
3. `Keys.Down` is bound to `SelectNextItemAsync()` at `<CTRL>:1288`. `SelectNextItemAsync` (`:1498-1501`) marshals to the **synchronous** `SelectNextItem()` (`:1486-1496`) → `ChangeByIndex` (`:1450-1464`) → `ToggleOffActiveItem(false)` (`:1459`, body at `:1667-1685`) which calls the **synchronous** `itemController.ToggleExpansion()` at `:1679` when the item is expanded, and → `ActivateBySelection(idx + 1, expanded)` (`:1460`) which calls the synchronous `ToggleExpansion()` at `:1414`.

Repro, three keystrokes:

- **Right** on item *i*: `ToggleExpansionAsync()` → `_expanded = true` (`<NAV>:220`), `RegisterExpandedAsyncActions()` adds `('B','D')` to `CharActionsAsync` (`<WIRE>:320-332`).
- **Down**: `SelectNextItemAsync` → `SelectNextItem` → `ChangeByIndex` → `ToggleOffActiveItem` sees `IsExpanded == true` and calls the **synchronous** `ToggleExpansion()` → `_expanded` true → `ToggleExpansion(Off)` → `ToggleExpansionOff()` clears `_expanded` and `UnregisterExpandedActions()` removes from `CharActions`, where nothing was ever added — `Remove` returns `false` silently (`<KBD>:126-129`). `CharActionsAsync` still holds `('B','D')` for item *i*.
- **Right** on item *i* again: `_expanded` is now `false`, so `ToggleExpansionAsync(On)` runs → `RegisterExpandedAsyncActions()` → `CharActionsAsync.Add(entryId, 'B', …)` → the entry is already present → **`ArgumentException`** at `<KBD>:97`, surfacing through the `async void` boundary at `KeyboardHandler.cs:133-148` (caught and logged there, so the user sees the key stop working rather than a crash dialog).

`KeyboardHandler.cs:141-147` catches and logs, so the observable symptom is a silently dead `B`/`D` key plus a log entry, not an unhandled exception. Adjust the issue's "Medium-High / reachable unhandled `ArgumentException`" severity wording accordingly.

### 1.3 The two "disjoint registries" of #482, named exactly

They are two distinct `KbdActions<>` instances hanging off `IQfcKeyboardHandler`:

| Registry | Declared | Written by the sync expansion path | Written by the async expansion path | Read by |
| --- | --- | --- | --- | --- |
| `_kbdHandler.CharActions` (`KbdActions<char, KaChar, Action<char>>`) | `QuickFiler/Interfaces/IQfcKeyboardHandler.cs:21`; backing field `QuickFiler/Controllers/KeyboardHandler.cs:44` | `RegisterExpandedActions()` `<WIRE>:306-318`; `UnregisterExpandedActions()` `<WIRE>:379-383` | never | `KeyboardHandler_KeyDown` `KeyboardHandler.cs:114-131`, reached **only** from `ProcessCmdKey` on an Alt-key command (`QuickFiler/Viewers/QfcFormViewerExpanded.cs:41-50`, `QuickFiler/Viewers/QfcFormViewerDark.cs:41-50`) |
| `_kbdHandler.CharActionsAsync` (`KbdActions<char, KaCharAsync, Func<char, Task>>`) | `IQfcKeyboardHandler.cs:22`; backing field `KeyboardHandler.cs:51` | never | `RegisterExpandedAsyncActions()` `<WIRE>:320-332`; `UnregisterExpandedAsyncActions()` `<WIRE>:385-389` | `KeyDownTaskAsync` `KeyboardHandler.cs:170-177` (the ordinary keystroke path) |

The shared state is the single `bool _expanded` at `QuickFiler/Controllers/QfcItemController.cs:146`, exposed read-only as `IsExpanded` at `:142-145`.

The four registration methods are `internal` members of the same `partial class QfcItemController`, declared in `<WIRE>`. Because they are same-class members, `<NAV>` can call all four **without any edit to `<WIRE>`**. This is what makes a containable fix possible (see §5.3).

---

## 2. Post-#468 delta table

Sources read (from the checked-out epic-integration worktree at `.claude/worktrees/epic-quickfiler-bug-family/`): `docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md` (397 lines, read in full) and `.../spec.md` (targeted sections `:390-419`, `:1005-1049`, plus a full-file identifier sweep).

**#468 removes twelve dead members from `<CTRL>` in P1-T2, in one isolated commit (D15).** The removal spans are `:587-605`, `:635-738`, `:761-796`, `:827-857`, `:865-874`, `:1254-1273`, `:1324-1328`, `:1991-1996`, plus the field at `:70` and a commented reference at `:402` (plan D1, spec `:388-401`). P1-T2's acceptance requires the file to shrink by at least 200 lines.

| Region (line numbers at `988e819b`) | Does #468 modify it? | Post-#468 shape | Consequence for this feature |
| --- | --- | --- | --- |
| **`Digits` property, `<CTRL>:114-128`** | **No.** No plan task names `Digits`. The only mentions are plan D14 and spec `:823-827`, which are *test-harness guidance* ("any test reaching `RegisterNavigation`, `UnregisterNavigation`, or `RemoveSpecificControlGroupAsync` must inject `_digits = 1`"), not a production edit. | Unchanged. `Digits` remains **live and side-effecting**: it still sets `_digitRefreshNeeded` and mutates `_digits` inside the getter, and it remains `[MethodImpl(MethodImplOptions.Synchronized)]`. | This feature owns whatever it does with `Digits`. No coordination needed. The line number shifts (see the note below the table). |
| **`KbdActions` enumerable-ctor seeding with the duplicate `Keys.Down`, `<CTRL>:1265-1272`** | **Yes — DELETED.** It is inside `WireUpKeyboardHandler` (`:1254-1273`), one of the twelve dead members removed by P1-T2. Plan D1: *"The only moot relationship is the dormant duplicate-`KaKey` registration owned by sibling issue #444, which step 1 resolves as a side effect."* Spec `:1012-1016`: *"Removing it **resolves #444's duplicate-registration defect as a side effect**. #444 may find its reproduction case gone after this feature merges."* | **The block no longer exists.** `WireUpKeyboardHandler` has zero callers today (verified independently: a repository-wide `*.cs` search for the identifier returns exactly one hit, its own declaration at `<CTRL>:1254`; spec `:398` records the same finding). | **MATERIAL FINDING.** #444's second acceptance criterion ("the duplicate registration in `QfcCollectionController.cs` is resolved to a single entry") is **already satisfied by #468** at the point this feature starts. This feature must **not** re-do it and must **not** assume the block is present. The remaining live work for #444 is the `KbdActions` constructor guard plus the recorded product decision. See §3 and §5.1. |
| **`RegisterNavigation`, `<CTRL>:1330-1341`** | **No.** Not in any removal span; no task edits its body. (Plan D14 mentions it only as a test-harness constraint.) | Unchanged text; renumbered. | This feature edits `UnregisterNavigation` and, under the recommended fix, adds one assignment inside `RegisterNavigation`. No conflict with #468's diff. |
| **`UnregisterNavigation`, `<CTRL>:1343-1356`** | **No.** | Unchanged text; renumbered. Note that #468's P3-T4 wraps the *body of `RemoveSpecificControlGroupAsync`* in `try`/`finally`, and that method **calls** `UnregisterNavigation()` at `:1162` — but the callee is not edited. | Free to edit. |
| **`ActivateBySelectionAsync` calling the synchronous `ToggleExpansion()`, `<CTRL>:1439`** | **No.** No #468 task names `ActivateBySelection`, `ActivateBySelectionAsync`, `ActivateByIndex`, or `ActivateByIndexAsync`. | Unchanged text; renumbered. Still calls the synchronous `ToggleExpansion()`, and its `blExpanded` argument is still always `false` on both async callers. | This feature does **not** need to touch it. The #482 fix lives entirely in `<NAV>` (§5.3). If planning wants to record the mixed-mode call as a design smell, do it as a follow-up issue, not an edit. |

**Line-number policy for the plan.** Every region above is renumbered by #468. Cumulative deletions strictly before `<CTRL>:1330` total approximately **227 lines** (1 at `:70`, 19 for `:587-605`, 1 at `:402`, 104 for `:635-738`, 36 for `:761-796`, 31 for `:827-857`, 10 for `:865-874`, 20 for `:1254-1273`, 5 for `:1324-1328`), so `RegisterNavigation` lands near `:1103`. But #468 also **adds** lines before that point (P3-T4's `try`/`finally` in `RemoveSpecificControlGroupAsync`, P9-T2's guard in `SetVisualDigits` at `:130-146`, P13-T1's `TryGetMoveReadiness` and `_notifyNotReady` near `ReadyForMove` at `:152`), so the net shift is not derivable from the plan text alone. **Do not hard-code any post-#468 line number in `spec.md` or in the atomic plan.** Anchor every edit on the member name and require the executor to re-derive line numbers at Phase 0 against the actual branch head. Mark that requirement as an explicit Phase 0 task.

**One #468 change materially helps this feature's test harness.** P9-T2 makes `SetVisualDigits` skip a group whose `ItemController` or `ItemViewer` is null (AC-10, spec `:1216`). Today `SetVisualDigits` (`<CTRL>:130-146`) dereferences `grp.ItemController.ItemNumberDigits` at `:140` behind only an `EmailsLoaded > 0` guard (`:132`), and `EmailsLoaded` is `_itemGroups?.Count ?? 0` (`:148`). A #472 test that injects ten controller-less groups therefore throws `NullReferenceException` today if it lets `_digitRefreshNeeded` become true. Post-#468 that hazard is gone. §8 gives a harness arrangement that avoids it either way, so the feature does not *depend* on #468's fix landing — but it is worth recording.

---

## 3. #444 product decision — which `Keys.Down` action is correct

### Evidence

1. **The live analogue registers `SelectNextItemAsync` and nothing else.** `RegisterAsyncKeyActions` (`<CTRL>:1282-1291`) builds `KeyActionsAsync` with exactly two entries: `("Collection", Keys.Up, SelectPreviousItemAsync())` at `:1287` and `("Collection", Keys.Down, SelectNextItemAsync())` at `:1288`. This is the registration that actually runs — it is called from `WireUpAsyncKeyboardHandler` (`:1275-1280`), which also calls `RegisterNavigation()` (`:1277`) and `RegisterAlwaysOnAsyncKeyActions()` (`:1279`). Spec `:1026-1028` records the same: *"Production key wiring is unaffected. Keys are wired through `WireUpAsyncKeyboardHandler` and `RegisterAsyncKeyActions`, which register `Keys.Up` and `Keys.Down` exactly once each."*
2. **`ActionOk` is bound to Return, not Down, on every surface.** `<CTRL>:1302` registers `("Collection", Keys.Return, CustomReturnKeyHandler())`, and `CustomReturnKeyHandler` (`:1307-1314`) awaits `_parent.ActionOkAsync()` at `:1312`. The Explorer sibling does the same: `EfcFormController.cs:365` registers `("Collection", Keys.Return, ActionOkAsync())`.
3. **Up/Down are a symmetric navigation pair everywhere in the codebase.** `KeyboardHandler.cs:333` treats `Keys.Up` and `Keys.Down` identically ("Don't handle the instruction so that it moves the selection"), while `:367` treats `Keys.Return` and `Keys.Escape` identically (close the dropdown). Legacy confirms the same split: `QuickFiler/Legacy/QfcController.cs:1686` (`Keys.Down`) and `:1691` (`Keys.Up`) are the navigation cases, `:1608` (`Keys.Return`) is the action case; `QuickFiler/Legacy/QuickFileController.cs:419`/`:425`/`:583`/`:589` are Down/Up navigation and `:624` is `Keys.Enter`.
4. **The duplicate entry is second in the list literal.** `<CTRL>:1269` is `SelectNextItem()`, `:1270` is `_parent.ActionOkAsync()`. Had they been added through `Add`, the *second* would have been rejected — i.e. `SelectNextItem()` is what would have survived. That is weak evidence, but it points the same way.
5. **`KeyboardHandler.cs`, the shared handler, contains no `Keys.Down`-to-`ActionOk` mapping anywhere.** A repository-wide `*.cs` search for `Keys.Down` returns 12 hits; none binds an OK/commit action.
6. **Existing tests assert nothing about this pair.** `KbdActionsTests.cs`, `KbdActionsRemainingBranchesTests.cs`, `QfcCollectionControllerTests.cs`, and `QfcItemController.NavigationTests.cs` contain no assertion involving `Keys.Down`. `QfcItemController.EventWiringTests.cs` asserts `Keys.Right`/`Keys.Left` membership only (`:152-153`, `:173-174`). No existing test constrains the decision.

### Recommendation

**`Keys.Down` on the QuickFiler collection surface means `SelectNextItem()`. Confidence: HIGH.**

Five independent lines of evidence agree and none dissents: the surviving live registration, the Return-binds-ActionOk convention on two surfaces, the Up/Down symmetry in the shared handler, the legacy controller, and the `Add`-ordering argument. The `_parent.ActionOkAsync()` entry at `<CTRL>:1270` is best read as an editing accident that was never exercised because the whole method is dead.

**Recording, not implementing.** Because #468 deletes `WireUpKeyboardHandler` before this feature starts, there is **no live registration to correct**. The deliverable for #444 acceptance criterion 1 is therefore a **recorded decision** in `spec.md` (and mirrored as an XML comment or in-code note next to `RegisterAsyncKeyActions` only if planning judges a code comment in `<CTRL>` worthwhile — that is inside this feature's allowed region), plus a regression test that pins the intended `Keys.Down` → `SelectNextItemAsync` binding of `RegisterAsyncKeyActions` so a future edit cannot silently re-introduce the wrong action. That test is cheap and is the only durable form the decision can take.

**How a reviewer could overturn this.** Only by producing a user-facing statement (help text, a keyboard-shortcut card, or a maintainer statement) that Down is a commit gesture in QuickFiler. No such artifact was found in `docs/` during this research. `UNVERIFIED`: no runtime observation of the shipped add-in was possible in this session.

---

## 4. `KbdActions` contract blast radius

Method surface of `KbdActions<TKey, UClass, VDelegate>` (`<KBD>:14-145`): two constructors (`:21-24`, `:26-29`), indexer (`:36-47`), `ContainsKey` (`:49`), `FilterKeys` (`:51`), `Find` (`:53-69`), `FindIndex` (`:71-88`), `Add(string, TKey, VDelegate)` (`:90-104`), `Add(UClass)` (`:106-121`), `Remove(string, TKey)` (`:123-135`), `GetEnumerator` (`:137-139`), `Keys` (`:141-144`).

### 4.1 Construction sites

| File | Line | Form | Contains a duplicate `(SourceId, Key)` pair? | Effect of adding a ctor duplicate guard |
| --- | --- | --- | --- | --- |
| `<CTRL>` | `:583`, `:584` | `new KbdActions<…>()` parameterless | n/a | none |
| `<CTRL>` | `:743`, `:744` | parameterless | n/a | none |
| `<CTRL>` | `:1265-1272` | **enumerable** — `KaKey` × 3 | **YES** — `("Collection", Keys.Down)` at `:1269` and `:1270` | **Would throw at construction.** But the enclosing method `WireUpKeyboardHandler` is deleted by #468 P1-T2 before this feature starts, so post-#468 **no site throws.** |
| `<CTRL>` | `:1284-1290` | enumerable — `KaKeyAsync` × 2 | No: `("Collection", Keys.Up)`, `("Collection", Keys.Down)` | none |
| `<CTRL>` | `:1295-1304` | enumerable — `KaKeyAsync` × 1 | No | none |
| `QuickFiler/Controllers/EfcFormController.cs` | `:358-367` | enumerable — `KaKeyAsync` × 1 (`"Collection"`, `Keys.Return`) | No | none |
| `QuickFiler/Controllers/EfcFormController.cs` | `:574-602` | enumerable — `KaCharAsync` × 8, all `"Controller"`, keys `S F K X R N T M` | No — all eight distinct (three further entries are commented out at `:583-586`) | none |
| `QuickFiler/Controllers/EfcFormController.cs` | `:631-676` | enumerable — `KaChar` × 8, all `"Controller"`, keys `S F K X R N T M` | No | none |
| `QuickFiler/Controllers/KeyboardHandler.cs` | `:44`, `:51`, `:58`, `:65`, `:72`, `:83` | collection expression `= []` | n/a | **none.** An empty collection expression on a type that implements `IEnumerable<T>` and exposes an applicable `Add` lowers to `new T()` plus zero `Add` calls; the enumerable constructor is not involved. |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | `:17`, `:35`, `:53` | parameterless | n/a | none |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | `:21-22` | parameterless | n/a | none |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | `:346` | parameterless | n/a | none |
| `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` | `:232`, `:233` | parameterless | n/a | none |
| `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs` | `:209`, `:210` | parameterless | n/a | none |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | `:69-74` | parameterless | n/a | none |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | `:48`, `:49`, `:134`, `:135` | parameterless | n/a | none |

**Conclusion:** **exactly one** construction site in the repository would begin throwing under a duplicate guard, and #468 deletes it first. Post-#468 the guard is a **zero-call-site-impact** change. This removes the coupling the promoted document worried about ("must land together with decision 1"): the coupling was real at the time of filing and is dissolved by #468.

### 4.2 `Remove` call sites

`Remove` returns `bool` (`<KBD>:123-135`) and **every** production call site discards it. Grouped by file (42 production call sites, not the ~30 the #482 document estimated):

| File | Lines | Count | Registry |
| --- | --- | --- | --- |
| `<WIRE>` | `:336-348` (`UnregisterFocusActions`) | 13 | `KeyActions` × 2, `CharActions` × 11 |
| `<WIRE>` | `:359-372` (`UnregisterFocusAsyncActions`) | 14 | `KeyActionsAsync` × 1, `CharActionsAsync` × 13 |
| `<WIRE>` | `:381-382` (`UnregisterExpandedActions`) | 2 | `CharActions` |
| `<WIRE>` | `:387-388` (`UnregisterExpandedAsyncActions`) | 2 | `CharActionsAsync` |
| `QuickFiler/Controllers/EfcItemController.cs` | `:723`, `:724`, `:727`, `:728`, `:734`, `:902`, `:903` | 7 | `CharActionsAsync` × 4, `CharActions` × 3 (`:734` is a `ForEach` over a key list) |
| `<CTRL>` | `:1349`, `:1353` (`UnregisterNavigation`) | 2 | `StringActionsAsync` |
| `QuickFiler/Controllers/EfcFormController.cs` | `:926`, `:935` | 2 | `CharActions`, `CharActionsAsync` |

Only tests observe the return value (`KbdActionsRemainingBranchesTests.cs:110`, `:124`).

### 4.3 `Add` and `Find` consumers relevant to this feature

- `Add` throw sites: `<KBD>:97` (three-argument overload) and `<KBD>:118` (instance overload). Both log via `logger.Error` first.
- `Find` is reached from the indexer getter (`<KBD>:38`) and setter (`<KBD>:41`). The indexer is what `KeyboardHandler` uses: `KeyboardHandler.cs:122` (`KeyActions[e.KeyCode]`), `:128` (`CharActions[…]`), `:159` (`AlwaysOnKeyActionsAsync[…]`), `:168` (`KeyActionsAsync[…]`), `:176` (`CharActionsAsync[…]`), `:194` (`StringActionsAsync[…]`).
- **`Find`/`ContainsKey`/`FilterKeys` use `x.KeyEquals(key)` (element-defined, possibly fuzzy), while `Add`'s guard and `Remove` use `StoredKeyEquals` (`<KBD>:33-34`, `EqualityComparer<TKey>.Default`).** This distinction is load-bearing:
  - `KaStringAsync.KeyEquals` (`QuickFiler/Controllers/KaStringAsync.cs:106+`) is **substring-matching and side-effecting** — its XML doc at `:64-68` states it drives `Update` and `ToggleControl` as an observable side effect.
  - `KbdActionsTests.cs:14-29` is an explicit characterization test that `"10"` and `"1"` may coexist under the same `SourceId`.
  - **Therefore a duplicate guard added to the enumerable constructor MUST use `StoredKeyEquals`, never `KeyEquals`.** Using `KeyEquals` would both reject the legal `"10"`/`"1"` pair (breaking `KbdActionsTests.cs:14`) and fire the latch side effects during construction.
  - `KaChar.KeyEquals` (`KaChar.cs:42`) and `KaCharAsync.KeyEquals` (`KaChar.cs:77`) are plain `Key == other`, so the distinction is invisible for char registries — but it is real for `KaStringAsync`, which is exactly the registry `UnregisterNavigation` operates on.

---

## 5. Design options and recommendations

### 5.1 #444 — the `IEnumerable<UClass>` constructor guard

| Option | Description | Advantages | Limitations |
| --- | --- | --- | --- |
| **A. Throw on duplicate (recommended)** | Detect any `(SourceId, Key)` pair repeated in `list` using `StoredKeyEquals`, and throw `ArgumentException` naming the offending pair, mirroring `Add(UClass)`'s message shape and `nameof(list)` parameter name. | Restores the class invariant on every entry point; identical failure mode and message vocabulary as both `Add` overloads; fails fast at construction rather than at an unrelated later `Find`; zero call sites affected post-#468 (§4.1). | Converts a latent defect into a construction-time throw. Risk is empirically zero here, but a future caller that intentionally seeds duplicates would break. |
| **B. De-duplicate silently** | Keep the first entry per `(SourceId, Key)` and drop the rest. | Never throws. | Contradicts both `Add` overloads, which throw. Silently discards a registration the caller asked for — precisely the "silent divergence" failure mode that produced #472 and #482. Rejected. |
| **C. Document why not** | Leave the ctor alone with an XML comment explaining the asymmetry. | No behaviour change. | Leaves the invariant hole open. #444's own acceptance criterion permits this, but there is no defensible reason to prefer it now that the blast radius is zero. Rejected. |

**Recommendation: Option A.** The coupling to the `Keys.Down` decision that the promoted document required ("must land together with decision 1") is dissolved by #468's removal of the only duplicate-bearing call site. Record that dissolution explicitly in `spec.md` so a reviewer does not re-impose the coupling.

Implementation constraints for Option A:
- Preserve the existing null behaviour: `new List<UClass>(list)` currently throws `ArgumentNullException` for a null `list`. Materialize the list **first**, then scan, so a null argument still produces `ArgumentNullException` and not a `NullReferenceException`.
- Enumerate `list` exactly once (materialize into the backing `List<UClass>`, then scan that list). The parameter is `IEnumerable<UClass>` and may be a one-shot sequence.
- Use `StoredKeyEquals` (§4.3).
- Log via the existing `logger.Error` before throwing, matching `<KBD>:96` and `<KBD>:117`.
- Message shape: reuse the existing literal fragment `already exists` so the two existing `.WithMessage("*already exists*")` assertions (`KbdActionsTests.cs:46`, `KbdActionsRemainingBranchesTests.cs:66`) remain the vocabulary the new test also asserts against.
- Keep the scan O(n²) over the seed list, consistent with `Add`'s existing `_list.Any(...)`. Seeds are ≤ 8 entries; introducing a hash set here would be premature and would need an `IEqualityComparer` for `TKey`.

### 5.2 #472 — digit-width desync

| Option | Description | Advantages | Limitations |
| --- | --- | --- | --- |
| **1. Capture `Digits` once in `UnregisterNavigation`** | Mirror `RegisterNavigation`: hoist `var digits = Digits;` above the loop. | Smallest diff; removes the per-iteration side-effecting read; makes the two methods textual mirrors. | **Does not fix the filed defect.** The hoisted value is still the *current* width, so the 10→9 crossing still removes `"1".."9"` where `"01".."09"` are registered. It fixes only the (unlikely) case where the count changes *during* the loop. **Insufficient on its own.** |
| **2. Record the registered digit width in state (recommended)** | Add `private int _registeredDigits;` to `<CTRL>`. `RegisterNavigation` assigns it from its already-captured `digits` local. `UnregisterNavigation` uses `_registeredDigits == 2 ? "00" : ""` as the format and reads `Digits` zero times. | Delivers exactly the promoted document's stated expected behaviour ("the digit width used to unregister must be the same width used to register"). Removes the side-effecting read from the unregister path entirely. Immune to a count change *during* the loop as well as between calls. Behaviour-preserving for every existing test (see the compatibility note below). | Does not address the *count* mismatch identified in §1.1. |
| **3. Record the registered keys (a key ledger)** | Add `private readonly List<string> _registeredNavigationKeys`. `RegisterNavigationAsyncAction` appends the key it registered; `UnregisterNavigation` iterates the ledger, removes each key, and clears it. | Design-complete: fixes both the width mismatch **and** the count mismatch; every `Remove` returns `true`, so checking the result becomes meaningful; makes the register/unregister pair exactly symmetric by construction. | **Breaks two existing characterization tests.** `QfcCollectionControllerTests.cs:409-423` and `:474-492` seed keys directly via `SeedCollectionKey` (`:386-389`) and then rely on `UnregisterNavigation` removing computed keys. With a ledger, the ledger is empty and those tests change outcome — `:409` would report `Key 1` where it asserts `Key 2`. Those tests are #232's regression suite and are "part of the spec" (`CLAUDE.md` §7.3). Repairing them means editing a file that #468 D12 pins at exactly 500 lines and forbids adding methods to. |
| **4. Make `Digits` non-side-effecting** | Move the `_digitRefreshNeeded` / `_digits` mutation out of the getter into an explicit `RefreshDigits()` method. | Removes a genuine design smell (a property with side effects read inside a loop, flagged in the promoted document's "Suspected Cause"). | Touches four other consumers (`<CTRL>:1197-1201`, `:1332-1336`, `:1838-1842`, `:1935-1939`), two of which sit inside members #468 edits in P3 and P7. Does not by itself fix the mismatch. High conflict surface for no direct defect closure. Reject for this feature. |

**Recommendation: Option 2**, plus **Option 1 folded in for free** (Option 2's `UnregisterNavigation` reads `Digits` zero times, which subsumes the hoist), plus a **new potential entry / follow-up issue for the count mismatch of §1.1**, per `CLAUDE.md`'s Bugfix Workflow step 2 ("If you uncover deeper design problems, open a new issue instead of widening scope").

Compatibility note for Option 2: `FormatterServices.GetUninitializedObject` bypasses field initializers, so a test-built controller sees `_registeredDigits == 0`. Write the format selection as `_registeredDigits == 2 ? "00" : ""` (i.e. treat anything that is not 2 as single-digit) rather than `== 1 ? … : "00"`. With that formulation, all four existing navigation tests in `QfcCollectionControllerTests.cs` (`:409`, `:430`, `:452`, `:474`) operate on 1–2 item pages at width 1 and pass unchanged, and no test needs to inject the new field.

Also record in `spec.md`: `RegisterNavigation` is on `IQfcCollectionController` (`:101`) and `UnregisterNavigation` at `:100`, but **neither signature changes**, so no interface edit is required (see §6).

### 5.3 #482 — unify the expansion registries behind one owner

The registration bodies live in `<WIRE>` (forbidden). The *callers* live in `<NAV>` (owned). Because all four are `internal` members of the same `partial class`, `<NAV>` can call any of them. So a fix is containable to `<NAV>`.

| Option | Description | Advantages | Limitations |
| --- | --- | --- | --- |
| **A. Unregister-both-then-register-both, keyed on `_expanded` (recommended)** | Introduce `private void SyncExpandedRegistrations(bool expanded)` in `<NAV>`. It unconditionally calls `UnregisterExpandedActions()` and `UnregisterExpandedAsyncActions()` (both are idempotent no-ops when absent), then, when `expanded` is true, calls `RegisterExpandedActions()` and `RegisterExpandedAsyncActions()`. Both toggle overloads call it once, after `ToggleExpansionOn()`/`ToggleExpansionOff()` has set `_expanded`, passing `_expanded`. | Exactly the fix #482's own "Suspected Fix" prescribes: *"a single registration owner keyed on the actual current state rather than on which code path performed the toggle."* Makes registration idempotent without changing `Add`'s contract (which #482 notes would affect all consumers and interact with #444). Both registries always agree with `_expanded`, so `B`/`D` work after either kind of expansion — closing the pre-existing behavioural gap where Alt+B worked only after a sync expansion and plain `B` only after an async one. Contained to `<NAV>`. | Widens observable behaviour: `B`/`D` in the async registry now also respond after a synchronous expansion, and Alt+B/Alt+D now also respond after an async expansion. This is a deliberate behaviour change and must be stated in `spec.md` and the PR body. |
| **B. Unregister both, register only the path's own registry** | Same owner method, but the ON branch registers only into the registry belonging to the calling path. | Strictly minimal: removes the throw and nothing else; no behavioural widening. | Leaves the two registries permanently disagreeing with each other — after a sync expansion, plain `B`/`D` still do nothing, and after an async expansion, Alt+B/Alt+D still do nothing. Preserves a latent inconsistency the issue explicitly asks to remove. |
| **C. Make `Add` idempotent in `<KBD>`** | Change `Add` to no-op on a duplicate instead of throwing. | Fixes #482 and any future duplicate-add anywhere. | Directly contradicts the #444 direction (tighten the invariant), and #482 itself flags it: *"making `Add` idempotent is a contract change affecting all consumers and interacts with #444."* It would also break `KbdActionsTests.cs:32-47` and `KbdActionsRemainingBranchesTests.cs:54-67`. Rejected. |

**Recommendation: Option A.** Shape (illustrative, not final source):

```
public virtual void ToggleExpansion(Enums.ToggleState desiredState)
{
    _parent.ToggleExpansionStyle(ItemIndex, desiredState);
    if (desiredState == Enums.ToggleState.On) { ToggleExpansionOn(); } else { ToggleExpansionOff(); }
    SyncExpandedRegistrations(_expanded);
}
```

with the async overload mirroring it. `SyncExpandedRegistrations` is new, private, lives in `<NAV>`, and must **not** carry `[ExcludeFromCodeCoverage]` so its lines are measured.

Residual risks to record:
- `RegisterFocusAsyncActions` (`<WIRE>:300-303`) and `UnregisterFocusAsyncActions` (`<WIRE>:373-376`) each conditionally touch the async expanded registry gated on `_expanded`. Under Option A those two remain balanced with each other in every sequence traced during this research (expand-while-focused, focus-off, focus-on, collapse). The one arrangement that could still double-add is *expand while the item is unfocused, then focus on*, and that hazard exists identically today on the pure-async path — it is not introduced by Option A. Record it as a known pre-existing residual, and, if planning wants belt-and-braces, note that a follow-up issue could give the focus paths the same idempotent owner.
- `ToggleExpansion(Enums.ToggleState)` and `ToggleExpansionAsync(Enums.ToggleState)` carry `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` at `<NAV>:173` and `<NAV>:191` and are `virtual` for test-override reasons documented at `<NAV>:171-172` and `:189-190`. Sibling #489 may de-exempt them. Keep this feature's edit inside those bodies minimal (a single call) and put the logic in the new un-exempted helper, so #489's de-exemption is unaffected.
- `RegisterExpandedActions()` retains its other caller at `<WIRE>:210`, so removing its `<NAV>` call site would not orphan it. Under Option A it keeps a `<NAV>` caller anyway.

### 5.4 Cross-cutting — should `Remove`'s `bool` be checked?

Both #472 and #482 name the silently-discarded `Remove` result as the compounding factor. Three shapes were considered:

- **Check the result at every call site.** 42 production call sites (§4.2), 31 of them in `<WIRE>` — a forbidden file. Not achievable within this feature's ownership and not desirable as a shotgun edit.
- **Add `TryRemove` / a logging `Remove` overload to `<KBD>`.** Owned file, so mechanically possible, but adding a member that no owned call site can adopt (because the call sites are in `<WIRE>`) produces dead API surface and a coverage obligation with no consumer.
- **Recommended: do not change `Remove`'s contract in this feature.** Instead, make the *specific* removals this feature owns provably total: Option 5.2/2 makes `UnregisterNavigation`'s width correct, and Option 5.3/A makes the expansion unregisters unconditional-and-idempotent so a `false` return is expected and meaningful rather than a symptom. Then file a follow-up issue proposing either a `TryRemove` naming change or a debug-level log inside `Remove` on the `false` branch, scoped so it can edit `<WIRE>` in coordination with #484/#489.

Record the reasoning in `spec.md` so a reviewer does not read the omission as an oversight — both promoted documents raise it explicitly.

---

## 6. File-ownership conflict report

**Writable by this feature:** `<KBD>`, `<NAV>`, and the regions of `<CTRL>` required by #444 and #472.

**Forbidden:** `QuickFiler/Controllers/KeyboardHandler.cs` (sibling #498); any other `QfcItemController` partial (siblings #484, #489); `QuickFiler/Interfaces/IQfcCollectionController.cs` (sibling #468).

**The complete set of `QfcItemController` partial files, so the boundary is unambiguous.** Exactly one of these ten is writable:

| File | Status for this feature |
| --- | --- |
| `QuickFiler/Controllers/QfcItemController.cs` | FORBIDDEN |
| `QuickFiler/Controllers/QfcItemController.Conversation.cs` | FORBIDDEN |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | FORBIDDEN |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | FORBIDDEN |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | FORBIDDEN |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | FORBIDDEN |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | FORBIDDEN |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | FORBIDDEN |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | FORBIDDEN |
| **`QuickFiler/Controllers/QfcItemController.Navigation.cs`** | **WRITABLE** |

### Per-fix containment verdict

| Fix | Files touched | Containable without a forbidden file? |
| --- | --- | --- |
| **#444 ctor guard (5.1/A)** | `<KBD>` only | **YES.** No call site changes (§4.1). No interface change: `KbdActions<>` implements only `IEnumerable<UClass>` and is not declared on `IQfcKeyboardHandler` as anything but a property *type* (`IQfcKeyboardHandler.cs:21-26`), and a constructor is not part of an interface contract. |
| **#444 recorded decision** | `spec.md`; optionally an in-code comment near `RegisterAsyncKeyActions` in `<CTRL>` | **YES.** `<CTRL>` comment lands in this feature's allowed region. |
| **#472 registered-width state (5.2/2)** | `<CTRL>` only — one new private field, one assignment inside `RegisterNavigation`, a rewritten `UnregisterNavigation` body | **YES.** `RegisterNavigation` and `UnregisterNavigation` are declared on `IQfcCollectionController.cs:100-101`, but the fix changes **no signature**, so the interface file is not edited. Confirm this explicitly in the plan's scope-lock audit. |
| **#482 single registration owner (5.3/A)** | `<NAV>` only — one new private method, two call-site simplifications inside the two `ToggleState` overloads | **YES.** The four register/unregister methods in `<WIRE>` are `internal` members of the same partial class and are **called, not modified**. `ToggleExpansion()` / `ToggleExpansionAsync()` are declared on `IQfcItemController.cs:43` and `:93`; no signature changes, so no interface edit. |

### Cross-feature notes to record (things this feature deliberately does not do)

1. **`<WIRE>` (#484 / #489):** the `_expanded`-gated calls at `<WIRE>:208-211`, `:300-303`, `:349-352`, `:373-376` retain the un-idempotent add/remove shape. If a sibling wants full symmetry, those four should route through the same owner. Also, `RegisterFocusActions` / `UnregisterFocusActions` (`<WIRE>:157`, `:334`) are effectively dead in the QuickFiler surface because their call sites are commented out at `QfcItemController.FocusAndTheme.cs:45`, `:61`, `:101`, `:117` — a dead-code candidate for #484/#489, not for this feature.
2. **`KeyboardHandler.cs` (#498):** `Remove`'s silent `false` (§5.4) and the `KeyboardHandler_KeyDown` / `KeyDownTaskAsync` registry asymmetry (`CharActions` is read only on the Alt-key `ProcessCmdKey` path) belong to #498's surface if anyone wants to unify them.
3. **`IQfcCollectionController.cs` (#468):** no edit needed from this feature. If a later design chooses the key-ledger option (5.2/3) and wants to expose the ledger, that would need #468's file — avoid it.
4. **`<CTRL>` count-mismatch orphan (§1.1):** `RemoveSpecificControlGroup(int)` at `<CTRL>:1105-1155` mutates `_itemGroups` with no navigation re-registration, and `RemoveBelowThresholdAsync` drives it in a loop. File as a new issue.
5. **`<CTRL>:1439`:** the mixed-mode synchronous `ToggleExpansion()` on an async path is currently unreachable but remains a design smell. File as a new issue rather than editing it here.

---

## 7. Downstream contract statement (copy into `spec.md`)

Siblings #464 and #489 are authored against this feature's contract for `<KBD>` and `<NAV>`. Under the recommended fixes the changes are:

### `QuickFiler/Controllers/KbdActions.cs`

**Signature changes: NONE.** No member is added, removed, or re-signed. The public surface after this feature is identical to `<KBD>:14-145` today.

**Observable behaviour changes: ONE.**

- `KbdActions(IEnumerable<UClass> list)` now throws `ArgumentException` (parameter name `list`) when `list` contains two or more elements sharing the same `SourceId` **and** a `StoredKeyEquals`-equal `Key`. The message contains the literal fragment `already exists`, matching both `Add` overloads. The error is logged via the existing `logger.Error` before the throw.
- Unchanged: `ArgumentNullException` for a null `list`; acceptance of any duplicate-free sequence; acceptance of elements whose `KeyEquals` overlaps but whose stored keys differ (for example `KaStringAsync` `"10"` and `"1"` under the same `SourceId`) — the guard uses `StoredKeyEquals`, not `KeyEquals`, so this pair remains legal and `KbdActionsTests.cs:14-29` continues to pass.
- Unchanged: `Add(string, TKey, VDelegate)`, `Add(UClass)`, `Remove(string, TKey)` (still returns `bool`, still silently `false` for an absent pair), `Find`, `FindIndex`, `ContainsKey`, `FilterKeys`, the indexer, `Keys`, and both enumerators.
- The parameterless constructor `KbdActions()` is unchanged, so every `= []` collection-expression initializer in `KeyboardHandler.cs:44-84` is unaffected.

### `QuickFiler/Controllers/QfcItemController.Navigation.cs`

**Public/internal signature changes: NONE.** `ToggleExpansion()`, `ToggleExpansionAsync()`, `ToggleExpansion(Enums.ToggleState)`, and `ToggleExpansionAsync(Enums.ToggleState)` keep their current accessibility, `virtual`ness, parameter lists, and return types. The two `ToggleState` overloads keep `[ExcludeFromCodeCoverage]`.

**Added member (private, not part of the sibling-visible contract but named here so a sibling does not collide):** `private void SyncExpandedRegistrations(bool expanded)`.

**Observable behaviour changes: THREE.**

1. `ToggleExpansion(Enums.ToggleState)` and `ToggleExpansionAsync(Enums.ToggleState)` now maintain **both** `_kbdHandler.CharActions` and `_kbdHandler.CharActionsAsync`, not one each. After either overload completes, both registries hold `('B','D')` for `ItemHelper.EntryId` if and only if `_expanded` is true.
2. Expansion registration is now **idempotent**. Calling either overload with `ToggleState.On` twice in a row, or interleaving the sync and async overloads in any order, no longer throws `ArgumentException` from `KbdActions.Add`.
3. `'B'` and `'D'` now respond after a synchronous expansion (previously only after an async one), and Alt+`B` / Alt+`D` now respond after an async expansion (previously only after a sync one).

**Unchanged:** `ToggleExpansionOn()` / `ToggleExpansionOff()` remain private and keep their existing `_tlpStates` application, `_expanded` write, and `_emailIsReadTimer` handling. `JumpToFolderDropDown`, `JumpToFolderDropDownAsync`, `JumpToSearchTextbox`, `JumpToAsync`, both `KbdExecuteAsync` overloads, `MenuDropDown`, `Reply`, `ReplyAll`, `Forward`, and both `ToggleConversationCheckbox` overloads are untouched.

**Not changed by this feature, contrary to what a reader might expect:** the four registration methods themselves (`RegisterExpandedActions`, `RegisterExpandedAsyncActions`, `UnregisterExpandedActions`, `UnregisterExpandedAsyncActions`) remain in `<WIRE>` with their current `internal` accessibility and bodies.

### `QuickFiler/Controllers/QfcCollectionController.cs` (for #468's and #484's awareness)

- New `private int _registeredDigits;` field.
- `RegisterNavigation()` gains one assignment; signature unchanged.
- `UnregisterNavigation()` body rewritten; signature unchanged; it no longer reads the `Digits` property, so it no longer has the `Digits` getter's side effect of setting `_digitRefreshNeeded`.
- `IQfcCollectionController.cs` is **not** edited.

---

## 8. Test-harness feasibility

All four named files exist with `Compile Include` entries in `QuickFiler.Test/QuickFiler.Test.csproj`: `Controllers\KbdActionsTests.cs` (`:96`), `Controllers\KbdActionsRemainingBranchesTests.cs` (`:97`), `Controllers\QfcCollectionControllerTests.cs` (`:116`), `Controllers\QfcItemController.NavigationTests.cs` (`:143`).

Current sizes (against the 500-line cap): `KbdActionsTests.cs` **88**; `KbdActionsRemainingBranchesTests.cs` **181**; `QfcCollectionControllerTests.cs` **500** (at the cap); `QfcItemController.NavigationTests.cs` **391**; `QfcItemController.EventWiringTests.cs` **375**.

### Established construction/seam patterns in those files

- **`KbdActionsTests.cs` / `KbdActionsRemainingBranchesTests.cs`** — pure collection tests. Plain `new KbdActions<…>()`, MSTest + FluentAssertions, **no Moq at all**. `KbdActionsRemainingBranchesTests.cs:21-22` exposes a `NewRegistry()` helper for `KbdActions<Keys, KaKey, Action<Keys>>`. `KaKey` is directly constructible: `new KaKey("src", Keys.Enter, _ => { })` (`:42`). No Outlook, no WinForms, no dispatcher.
- **`QfcCollectionControllerTests.cs`** — the controller is built with `FormatterServices.GetUninitializedObject(typeof(QfcCollectionController))` (`:36-37`, `:147-148`, `:254-255`, `:343-344`), then private fields are injected by reflection through `SetControllerField` (`:380-383`). The navigation harness is `CreateControllerForSwap` (`:338-365`): a **real** `KbdActions<string, KaStringAsync, Func<string, Task>>` behind a Loose `Mock<IQfcKeyboardHandler>` whose `StringActionsAsync` getter returns it (`:346-349`); Loose `Mock<IEmailMoveMonitor>` (`:351`); Loose `Mock<IQfcFormViewer>` whose `L1v0L2L3v_TableLayout` returns `null` (`:353-356`); `_digits` injected as `1` (`:361`); `_itemGroups` injected from `MakeGroups(n)` (`:362`, `:368-378`), which builds `QfcItemGroup`s carrying only a `Mock<MailItem>` with an `EntryID` — **`ItemController` and `ItemViewer` are left null**. Helper assertions `SeedCollectionKey` (`:386-389`) and `CountCollectionKey` (`:392-395`) already exist. The XML comment at `:334-337` records exactly why `_digits = 1` is mandatory.
- **`QfcItemController.NavigationTests.cs`** — three construction patterns: a `NavController` subclass injecting `_homeController` (`:24-33`); the shared `HarnessController` from `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:25-29` (protected parameterless ctor); and an `ExpansionSpyController` (`:139-157`) that overrides both virtual `ToggleState` overloads to record the requested state. Field injection uses `QfcItemControllerTestSupport.SetField` / `GetField` / `InvokeNonPublic` (`TestSupport.cs:37-80`), which **assert the member exists before use**. `BuildSyncDispatcher()` (`TestSupport.cs:102-137`) gives a `Mock<IUiDispatcher>` that runs delegates synchronously. `:291-336` and `:344-389` show `ToggleExpansionOff`/`ToggleExpansionOn` driven end-to-end against a `Mock<IItemViewer>` plus a real `TlpCellStates` built from bare handle-less `TableLayoutPanel`/`Label` controls — no form is shown and no message pump runs.
- **`QfcItemController.EventWiringTests.cs`** — `KbdController` (`:26-39`) subclasses `QfcItemController`, injects `_kbdHandler` by reflection, and sets `ItemHelper = new MailItemHelper { EntryId = … }`. `BuildKbdHandlerStub()` (`:41-53`) and `BuildSyncKbdHandlerStub()` (`:127-139`) return a `Mock<IQfcKeyboardHandler>` wired to **real** `KbdActions` instances for the async and sync registries respectively. `:185-215` proves that `RegisterExpandedActions()` and `UnregisterExpandedActions()` run headlessly: the `((ItemViewer)_itemViewer)` casts at `<WIRE>:311`, `:316`, `:325`, `:330` sit **inside** the lambda bodies and are never evaluated at registration time.

A repository-level guard `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:16-36` asserts the test assembly compiles no `System.Windows.Forms.Form`-derived type. Nothing proposed below adds one.

### Per-defect feasibility

| Defect | Deterministic failing test achievable in an existing file? | Where | Arrangement |
| --- | --- | --- | --- |
| **#444** | **YES.** | `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` (181 lines; ~2 tests ≈ 40 lines, ends well under 500) | Pure. `new KbdActions<Keys, KaKey, Action<Keys>>(new List<KaKey> { new KaKey("src", Keys.Down, _ => { }), new KaKey("src", Keys.Down, _ => { }) })` — assert `ArgumentException` with `.WithMessage("*already exists*")`. Companion negative test: a list containing `("src", Keys.Up)` and `("src", Keys.Down)` must not throw. A third test in `KbdActionsTests.cs` (88 lines) should pin the `StoredKeyEquals`-not-`KeyEquals` rule: a `KbdActions<string, KaStringAsync, Func<string, Task>>` seeded with `("Collection","10")` and `("Collection","1")` must construct without throwing, mirroring the existing `Add` characterization at `:14-29`. **No `.csproj` edit needed.** Pre-fix state: the throwing test fails (no exception raised) — a clean deterministic red. |
| **#444 (Keys.Down decision pin)** | **YES, but not in an existing file** — see the `QfcCollectionControllerTests.cs` constraint below. | New file (see below) | Build the controller with `CreateControllerForSwap`-style reflection, inject a real `KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>` behind the `KeyActionsAsync` property of a Loose `Mock<IQfcKeyboardHandler>`, call `RegisterAsyncKeyActions()`, and assert exactly one `("Collection", Keys.Down)` entry exists and exactly one `("Collection", Keys.Up)` entry exists. `RegisterAsyncKeyActions` is `internal` (`<CTRL>:1282`); `QuickFiler.Test` already reaches `internal` members of `QuickFiler` (for example `controller.RegisterExpandedActions()` at `EventWiringTests.cs:192`), so no accessibility change is needed. This is a **pass-after-only** test with no pre-fix red state — record it in the fail-before dossier as such. |
| **#472** | **NO — a new test file is required.** `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is **exactly 500 lines**, i.e. at the `.claude/rules/general-code-change.md` cap, and #468's plan D12/P4-T5 pins it there and forbids adding any `[TestMethod]` to it (P4-T5 acceptance: *"its `[TestMethod]` count is unchanged from the P0-T15 baseline"*, and P14-T11 re-verifies). | New file | See the arrangement and the `.csproj` guidance below. |
| **#482** | **YES.** | `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` (391 lines; the fix lives in `<NAV>`, so the test belongs beside the existing `ToggleExpansion` routing tests at `:229-283`). Budget ~90 lines including a local registry builder; ends near 480, under the cap. If planning judges that too tight, `QfcItemController.EventWiringTests.cs` (375 lines) is the alternative and already owns both registry builders. | See below. |

### #472 test arrangement (deterministic, no `SetVisualDigits` hazard)

Build on `CreateControllerForSwap`'s shape but inject `_digits = 2` and ten groups, so `Digits`'s `digitNeed` (`<CTRL>:119`) already equals `_digits` and `_digitRefreshNeeded` stays `false` — `RegisterNavigation` therefore never enters `SetVisualDigits` (`<CTRL>:1333-1336`) and the null `ItemController` in `MakeGroups`-style groups is never dereferenced. Then:

1. Inject 10 groups and `_digits = 2`; call `RegisterNavigation()`. Assert the registry holds `"01".."10"` under `SourceId "Collection"`.
2. Remove one group from the injected `List<QfcItemGroup>` directly (mirroring `GetItemGroups(controller).RemoveAt(0)` at `QfcCollectionControllerTests.cs:483`), bringing the count to 9. This models the unbracketed `RemoveSpecificControlGroup` path of §1.1 without touching WinForms.
3. Call `UnregisterNavigation()`.
4. **Pre-fix (red):** the registry still holds all ten `"01".."10"` entries because `Digits` now evaluates to 1 and the loop removed the absent `"1".."9"`. **Post-fix (green):** `_registeredDigits == 2`, so the loop removes `"01".."09"` and the assertion is that no `"0"`-prefixed key survives except the one the count-mismatch of §1.1 leaves behind — write the assertion as "the registry holds at most the single `"10"` entry the loop bound cannot reach", and state in the test's XML comment that the residual `"10"` is the separately-filed count-mismatch defect, not this fix's scope. That keeps the assertion honest and prevents it from silently absorbing the second defect.
5. A second test drives the mirror direction (register at 9 items with `_digits = 1`, grow to 10, unregister) and asserts the same width-fidelity property.

Both tests are pure reflection + Moq + a real `KbdActions`; no timer, no dispatcher, no `Thread.Sleep`/`Task.Delay`, no temporary file.

**New file and `.csproj` guidance.** Name it `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`. The `Compile Include` block spans lines 57-175 of `QuickFiler.Test/QuickFiler.Test.csproj`; it is only loosely alphabetical (`Controllers\QfcCollectionControllerTests.cs` at `:116` precedes `Controllers\QfcCollectionControllerDarkModeTests.cs` at `:117`). **Insert the new entry immediately after line 116 (`Controllers\QfcCollectionControllerTests.cs`) and before line 117 (`Controllers\QfcCollectionControllerDarkModeTests.cs`).** That slot is deliberately chosen to sit *outside* #468's insertion point: #468's plan D13/P2-T2 inserts its five entries *after* the dark-mode entry and *before* `Controllers\QfcDatamodelTests.cs` at `:118`. Using the pre-dark-mode slot gives a non-overlapping single-line insertion and minimises the merge surface with every sibling that shares this item group.

If the controller-field reflection helpers are needed, note that #468's P2-T1 creates `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` with an asserting `SetField`/`GetField`/`InvokeNonPublic` and a `CreateUninitializedController` builder that injects `_digits = 1`. Since this feature branches from an integration branch carrying #468's code, **prefer reusing that file over duplicating the reflection boilerplate** — but confirm at Phase 0 that it exists, and fall back to a private local helper if it does not.

### #482 test arrangement

Reuse `EventWiringTests.cs`'s `BuildKbdHandlerStub()` / `BuildSyncKbdHandlerStub()` shape, but with **one** `Mock<IQfcKeyboardHandler>` wired to **both** a real `KbdActions<char, KaChar, Action<char>>` (via `CharActions`) and a real `KbdActions<char, KaCharAsync, Func<char, Task>>` (via `CharActionsAsync`), so the interleaving is observable in one arrangement. Additional injections needed to drive the real (non-overridden) `ToggleState` overload bodies:

- `_parent` — a `Mock<IQfcCollectionController>` (the field type is `IQfcCollectionController`, `QuickFiler/Controllers/QfcItemController.cs:44`), Loose, so `ToggleExpansionStyle` / `ToggleExpansionStyleAsync` are no-ops.
- `_itemViewer` — a `Mock<IItemViewer>` whose `Controls` returns a bare `Control().Controls`, exactly as `NavigationTests.cs:319-320` and `:372-373` already do.
- `_tlpStates` — a real `TlpCellStates` carrying `"Expanded"` and `"Compressed"` snapshots built from handle-less `TableLayoutPanel`/`Label` controls, as at `NavigationTests.cs:296-327`.
- `_uiDispatcher` — `QfcItemControllerTestSupport.BuildSyncDispatcher().Object`, so `ToggleExpansionAsync`'s `InvokeAsync(() => ToggleExpansionOn())` (`<NAV>:197`, `:202`) runs synchronously.
- `ItemHelper` — set via the `KbdController` pattern (`EventWiringTests.cs:35-37`). **`UnRead` must be false**, otherwise `ToggleExpansionOn()` constructs a `System.Threading.Timer` with a 4000 ms due time (`<NAV>:221-225`), which is a wall-clock dependency the determinism rules forbid. `MailItemHelper` is mockable (`QfcCollectionControllerTests.cs:40` uses `new Mock<MailItemHelper>(MockBehavior.Loose)`), so either a plain `new MailItemHelper()` whose `UnRead` defaults false, or a mock with `SetupGet(x => x.UnRead).Returns(false)`, works. **Verify the default at Phase 0** — this is the single most likely source of a flaky #482 test.

The failing test is the three-step interleaving of §1.2:

```
ToggleExpansionAsync(On)   // async registry gains B,D; _expanded = true
ToggleExpansion(Off)       // sync unregister no-ops; _expanded = false
ToggleExpansionAsync(On)   // pre-fix: ArgumentException from KbdActions.Add
```

Pre-fix the third call throws `ArgumentException` — a clean, deterministic red. Post-fix it succeeds, and the assertion becomes "both registries hold exactly one `B` and one `D` entry for the entry identifier". Add a companion test asserting the collapse direction leaves both registries empty, and a third asserting idempotence (two consecutive `On` calls do not throw).

A cheaper, complementary unit test drives the new `SyncExpandedRegistrations` directly through `QfcItemControllerTestSupport.InvokeNonPublic`, asserting the both-registries invariant for `true` and `false` without needing `_tlpStates` or `_parent`. Recommend having both: the reflection test for coverage of the helper, the end-to-end test for the actual regression proof.

### Test-policy compliance summary

Every test proposed above is MSTest (`[TestClass]`/`[TestMethod]`), uses Moq for boundaries and FluentAssertions for assertions, creates no temporary file, starts no `Form` and no `BackgroundWorker`, contains no `Thread.Sleep` / `Task.Delay` / wall-clock wait, and is order-independent (no shared static state is touched; note that `QfcCollectionController.removespecificcontrolgroupcounter` at `<CTRL>:1157` **is** process-wide static, but no test proposed here reaches it).

---

## 9. Open questions and risks for planning to resolve

1. **[HIGH] Does `spec.md` still carry a "resolve the duplicate registration" acceptance criterion?** #468 removes `WireUpKeyboardHandler` before this feature starts (§2). If the criterion is copied verbatim from the promoted document it becomes unsatisfiable-by-construction — the code it names will not exist. Restate it as *"the removal of the duplicate registration by #468 is verified, and the intended `Keys.Down` binding is recorded and pinned by a test."* Also add a Phase 0 verification task that greps `<CTRL>` for `WireUpKeyboardHandler` and **blocks** if it is still present (which would mean #468 has not actually landed on the branch this feature is cut from).
2. **[HIGH] Post-#468 line numbers are unknown.** Nothing in this artifact should be transcribed into the plan as a line number for `<CTRL>` or for `QuickFiler.Test.csproj`. Require a Phase 0 source-fact baseline that re-derives every anchor by member name.
3. **[HIGH] #472's fix does not close the count-mismatch orphan of §1.1.** Decide explicitly: file a follow-up issue (recommended), or widen scope to the key-ledger design (5.2/3) and accept editing `QfcCollectionControllerTests.cs`, which #468 pins at 500 lines with a frozen `[TestMethod]` count. These two constraints are in direct tension; planning must pick one and record the reason.
4. **[MEDIUM] The #482 promoted document's stated trigger is factually wrong** (§1.2). `spec.md` must say so and must substitute the Right/Down/Right sequence, otherwise the PR body will repeat an unsupported claim.
5. **[MEDIUM] Option 5.3/A widens observable behaviour.** `B`/`D` and Alt+`B`/Alt+`D` start responding in cases where they previously did nothing. Confirm this is acceptable, or fall back to Option 5.3/B (minimal, no widening) and record the residual inconsistency.
6. **[MEDIUM] `MailItemHelper.UnRead` default is `UNVERIFIED`.** If it defaults true, the #482 end-to-end test creates a 4000 ms timer. Resolve at Phase 0 by reading `MailItemHelper` or by mocking `UnRead` explicitly.
7. **[MEDIUM] `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` existence is `UNVERIFIED`** on the branch this feature will be cut from — it is created by #468 P2-T1, which has not executed. Plan for both cases.
8. **[LOW] Coverage-floor conflict** (80/90 in `CLAUDE.md` vs 85/75 in the rules files, §0). `CLAUDE.md` names `KbdActions<>` as explicitly non-exempt, so `<KBD>`'s new guard lines must be covered either way. Record the conflict; do not silently resolve it.
9. **[LOW] `<CTRL>` carries `[ExcludeFromCodeCoverage]`** at `:21` (per #468 spec AC-25 and plan line 106). Every `<CTRL>` line this feature changes for #472 therefore contributes nothing to any coverage denominator. State that plainly and carry per-defect proof by named test instead of by a coverage delta, exactly as #468's plan does. Do **not** write an acceptance condition that claims a coverage increase attributable to this feature — `.claude/rules/plan-acceptance-gates.md` rejects conditions that cannot fail.
10. **[LOW] Six of the twelve members #468 removes are `public` on a `public` type**, making #468 a public-API change of the `QuickFiler` assembly (spec `:410-412`). This feature adds no further public-API change: the ctor guard is a behaviour change, not a signature change, and no member is added to any interface.
11. **[LOW] `.csproj` insertion contention.** The recommended slot (immediately after line 116) avoids #468's block, but other epic siblings may also be inserting into lines 57-175. Keep the insertion to a single line and record its exact neighbours in the plan so a conflict is trivially resolvable.
