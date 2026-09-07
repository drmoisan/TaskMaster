# EFC controller surface defects (#464) — implementation research

- **Feature:** `efc-controller-surface-defects-464` (epic child, wave 2, `quickfiler-bug-family`)
- **Issues in scope:** #459, #460, #461, #463, #464 (primary), #465, #466, #467
- **Author:** task-researcher
- **Created:** 2026-08-25T12-20
- **Working tree commit:** `036a205d` (merge base `2300becf`)
- **Method:** static reading only. No `msbuild`, no `vstest`, no `csharpier` was run. No file outside this
  research directory was modified.

> **Citation basis.** Every `file:line` in this document was read directly from the working tree at
> `036a205d` during this session. No citation is carried forward from `issue.md`, from a promoted
> potential document, or from an upstream spec without re-reading the line. Where a cited source
> disagrees with the tree, both values are printed and the tree wins.

> **Path placeholders.** `<repo-root>` is the workspace root. Absolute host paths, account names, and
> machine names are deliberately excluded.

---

## 0. Executive summary of the load-bearing findings

Six findings change the shape of the plan and are stated up front. Each is evidenced in the section
named.

1. **`EfcItemController.Cleanup()` has zero call sites repo-wide** (§7.4). `EfcFormController.Cleanup()`
   (`EfcFormController.cs:187-194`) never calls it, `IItemControler` does not declare it
   (`IItemControler.cs:9-14`), and no other caller exists. #460 A and #460 B are therefore **latent**,
   not live.
2. **The synchronous expansion path is dead on the EFC surface** (§7.5). `EfcItemController.ToggleExpansion()`
   (`:838`) and `ToggleExpansion(Enums.ToggleState)` (`:862`) have zero reachable call sites. This is the
   only code that touches `CharActions` for `'B'`/`'D'`, so **#459 B, #459 C and the `async void` lambdas
   at `:882`/`:887` are all latent**, and #459's stated reproduction ("expand through the sync path") is
   not reachable through the UI.
3. **`IQfcKeyboardHandler.CharActions` is never read on any compiled path** (§7.6). Its only reader is
   `KeyboardHandler.KeyboardHandler_KeyDown` (`KeyboardHandler.cs:114-131`), whose only call sites are in
   files with no `<Compile Include>` entry. This is a **disagreement with feature 444's spec**, which
   states that member is "reached… from the Alt-key `ProcessCmdKey` path".
4. **The Edit Filters command is NOT broken** (§Q7). `EfcFormController.WireEventHandlers` wires it
   directly at `EfcFormController.cs:398` to `EfcFormController.EditFiltersMenuItem_Click`
   (`:559-564`). Issue #466's claim that "the Edit Filters command is silently non-functional" is
   **incorrect**. Only the viewer-side duplicate (`EfcViewer.cs:157-160`) plus `SetController`
   (`:50-53`) are dead.
5. **#461's intended behaviour is already delivered by a different, live path** (§Q4).
   `EfcItemController.PopulateConversation` assigns `ConversationResolver.UpdateUI = SetTopicThread`
   (`EfcItemController.cs:314`), and the resolver invokes `UpdateUI(pair.Expanded)` on the UI thread at
   `ConversationResolver.Loading.cs:150`. Renaming the dead guard to `"ConversationInfo"` would produce a
   **second** `SetObjects`+`Sort`. The correct remedy is removal, not renaming.
6. **No `Initializer` change is required for #464 A** (§Q2). The QFC twin solves the eager-argument
   problem with a plain conditional expression at `QfcFormController.cs:134-142`; no new overload and no
   write to `UtilitiesCS/HelperClasses/Initializer.cs` is needed.

---

## 1. Verified anchor table (current tree)

Every anchor the orchestrator pre-verified was independently re-read and **confirmed**. The table adds
the anchors I verified beyond that set, and flags the five that `issue.md` or a promoted document states
imprecisely.

| Claim | Verified location | Status |
|---|---|---|
| `EfcFormController.cs` length | 1084 lines | confirmed |
| `EfcItemController.cs` length | 1170 lines | confirmed |
| `EfcViewer.cs` length | 162 lines | confirmed |
| `EfcViewer.Designer.cs` length | 4277 lines | confirmed |
| `QfcItemController.ViewerSetup.cs` length | 430 lines | confirmed |
| `EfcFormController.Cleanup` | `EfcFormController.cs:187-194`; `_globals.Ol` deref `:189`; `_parentCleanup.Invoke()` `:193` | confirmed |
| `ActiveTheme` `strict: true` | `EfcFormController.cs:255` | confirmed |
| `LoadTheme` `_themes` deref | `EfcFormController.cs:267` | confirmed |
| `DarkMode` property | `EfcFormController.cs:272-283`; getter `:274-281`; eager `_globals.Ol` `:280` | `issue.md` says `:272-282`; the `set` accessor is at `:282` and the closing brace at `:283` |
| `StartsWith("====")` | `EfcFormController.cs:706` | confirmed |
| `ActionDeleteAsync` | `EfcFormController.cs:740-748`; insert `:746`; rebind `:747` | confirmed |
| `RefreshSuggestionsAsync` | `EfcFormController.cs:795-804`; cross-thread read `:799` | confirmed |
| `BindFolderRows` write-back | method `EfcFormController.cs:871-881`; **the write-back statement is `:879`** | `issue.md` cites `:871`, which is the method signature, not the write-back |
| `Substring(0, 3)` | `EfcFormController.cs:1047` | confirmed |
| EN DASH literal 1 | `EfcItemController.cs:184` | confirmed |
| EN DASH literal 2 | `EfcItemController.cs:217` | confirmed |
| EN DASH literal 3 | `QfcItemController.ViewerSetup.cs:55` | confirmed |
| `EfcItemController.DarkMode` | `:439-450`; getter `:441-448`; eager `_globals.Ol` `:447` | confirmed |
| `EfcItemController.Cleanup` | `:255-278`; `Buttons.ForEach` `:257`; `-=` `:262`; `_itemViewer = null` at **`:264` and `:276`**; `_timer = null` `:277` | confirmed |
| `_timer` field | `EfcItemController.cs:377` | confirmed |
| `_selectorsCtrls` | declared `:381`; passed to `SetupThemes` at `:97` and `:144`; never assigned | confirmed |
| `InitializeWebView()` | `EfcItemController.cs:174-205` | confirmed |
| `RegisterActions` | `EfcItemController.cs:680-692`; filter `:687-689`; indexer assign `:691` | confirmed |
| 7-argument ctor | `EfcItemController.cs:44-57` | confirmed |
| `Subject` / `Sender` / `To` | `:610-613` / `:595-598` / `:621-624` | confirmed |
| `ConversationResolverPropertyChanged` | declared `:741-755`; **guard at `:746`**; body `:749-753`; subscription `:666-669` | `issue.md` cites `:741` (declaration) for the guard; the guard expression is at `:746` |
| `throw (e.InitializationException)` | `EfcItemController.cs:777` | confirmed |
| `ToggleExpansion(ToggleState)` | `:862-905`; `'B'`/`'D'` `Add` `:879-888` (lambdas `:882`, `:887`); `Remove` `:902-903` | confirmed |
| `ToggleExpansionOff` / `ToggleExpansionOn` | `:931-942` / `:944-956`; dispatched `:913`, `:922` | confirmed |
| `EfcViewer.SetController` | `EfcViewer.cs:50-53`; `_formController` `:48` | confirmed |
| `EfcViewer.ProcessCmdKey` | `EfcViewer.cs:94-105` | confirmed |
| `EfcViewer.EditFiltersMenuItem_Click` | `EfcViewer.cs:157-160` (`private`, not `internal`) | confirmed |

### 1.1 Citations in the promoted potentials that are now stale

The promoted documents were written on 2026-08-07. Their `EfcFormController.cs` citations are uniformly
**+2** relative to the current tree; their `EfcItemController.cs` citations are current except where noted.

| Promoted claim | Promoted cite | Current |
|---|---|---|
| #464 B — five `logger.Error(...); throw;` sites | `:424-428`, `:440-444`, `:456-460`, `:516-520`, `:529-533` | `:422-426` (`throw` `:425`), `:438-442` (`:441`), `:454-458` (`:457`), `:514-518` (`:517`), `:527-531` (`:530`) |
| #464 C — `_ = PopulateFolderCombobox()` | `:97`, `:117`; callee `:1024-1038`; sibling `:853`, `:858-868` | `:95`, `:115`; callee `:1022-1036`; sibling `:851`, `:856-866` |
| #465 A — `Keys.Return` binding / OK `Click` | `:365` / `:391` | `:363` / `:389` |
| #465 B — `SearchText_TextChanged` | `:558` | `:554-557`, read at `:556` |
| #465 D — `IsValidSelection` | `:1049` | property `:1038-1050`, `Substring` `:1047` |
| #461 — guard / subscription / body | `:746` / `:667` / `:749-753` | `:746` / `:667` (`+=`), `:668` (handler name) / `:749-753` — **current** |
| #460 A — `_buttons` assignment | `:341` | `:341` — **current** |
| #466 A — Designer references | `:67`, `:4123`, `:4136-4138`, `:4275` | all four **current** |
| #463 — QFC site | `QfcItemController.ViewerSetup.cs:52` | `:55` |

---

## Q1 — `KbdActions<>` contract, and what feature 444 does to it

### Q1.1 The contract as it stands at `036a205d`

`QuickFiler/Controllers/KbdActions.cs` is **146 lines**. The type is
`public class KbdActions<TKey, UClass, VDelegate> : IEnumerable<UClass> where UClass : IKbdAction<TKey, VDelegate>, new()`
(`:14-15`).

| Member | Location | Behaviour, verbatim from source |
|---|---|---|
| indexer `get` | `:38` | `this.Find(key).Delegate` — **throws `NullReferenceException`** when the key is absent, because `Find` returns `default(UClass)`. |
| indexer `set` | `:39-46` | `var element = this.Find(key); if (element is not null) { element.Delegate = value; }`. **A missing key is a silent no-op, never an insert.** `issue.md`'s claim is verified exactly. |
| `ContainsKey(TKey)` | `:49` | `_list.Any(x => x.KeyEquals(key))`. Ignores `SourceId`. |
| `Find(TKey)` | `:53-69` | 0 matches → `default(UClass)`; 1 → that element; **2 or more → `throw new InvalidOperationException`** (`:67`). |
| `FindIndex(TKey)` | `:71-88` | Same shape; `-1` for 0 matches; logs then throws for >1 (`:85-86`). |
| `Add(string, TKey, VDelegate)` | `:90-104` | Guard `:92-98`: if any element has the **same `SourceId` AND `StoredKeyEquals(x.Key, key)`**, logs and **throws `ArgumentException`** (`:97`). Otherwise constructs `new UClass()` and appends. |
| `Add(UClass)` | `:106-121` | Same guard `:108-119`, `throw new ArgumentException(message, nameof(instance))` (`:118`). |
| `Remove(string, TKey)` | `:123-135` | `FindIndex` on `(SourceId, StoredKeyEquals)`; **absent → `return false`, no throw** (`:126-129`); present → `RemoveAt` and `return true`. |
| `Keys` | `:141-144` | `_list.Select(x => x.Key).ToList()`. |
| `StoredKeyEquals` | `:33-34` | `private static`, `EqualityComparer<TKey>.Default.Equals`. Used by `Add`'s guards and by `Remove` — **not** by `Find`/`ContainsKey`, which use the element-defined `x.KeyEquals(key)`. |

### Q1.2 `overwriteDuplicates` is a parameter of `EfcItemController.RegisterActions`, not of `KbdActions<>`

`KbdActions<>` has no `overwriteDuplicates` concept anywhere. The flag is a parameter of
`EfcItemController.RegisterActions(Dictionary<char, Action<char>> actions, bool overwriteDuplicates)`
(`EfcItemController.cs:680-683`):

```csharp
// EfcItemController.cs:685-691
if (!overwriteDuplicates)
{
    actions = actions
        .Where(action => !_keyboardHandler.CharActions.ContainsKey(action.Key))
        .ToDictionary();
}
actions.ForEach(action => _keyboardHandler.CharActions[action.Key] = action.Value);
```

The complete truth table, derived from the two members above:

| `overwriteDuplicates` | key already present | outcome |
|---|---|---|
| `false` | yes | filtered out at `:687-689`; nothing happens |
| `false` | no | survives the filter; indexer setter finds nothing and **silently no-ops** (`KbdActions.cs:41-45`) |
| `true` | yes | indexer setter finds the element and **overwrites the delegate** |
| `true` | no | indexer setter **silently no-ops** — the insert never happens |

So `overwriteDuplicates: false` registers **nothing**, and `overwriteDuplicates: true` **overwrites but
never inserts**. `issue.md`'s RC4-A statement is correct and is here extended with the `true` branch,
which `issue.md` does not describe.

One additional hazard not in `issue.md`: because `Find` (`:53-69`) throws `InvalidOperationException`
when two different `SourceId`s registered the same key, the indexer setter can **throw** rather than
no-op in a multi-source registry. `RegisterActions` would therefore throw, not silently fail, if the
`"Controller"` and `"Item"` sources ever registered the same char.

### Q1.3 POST-444 contract — verified against 444's spec and plan

Feature 444's spec (`docs/features/active/quickfiler-keyboard-action-defects-444/spec.md`) changes
**exactly one** member of `KbdActions.cs`.

- **In scope** (`:289-290`): "Add a `(SourceId, Key)` duplicate guard to `KbdActions(IEnumerable<UClass>)`,
  using `StoredKeyEquals` semantics (#444)."
- **Out of scope** (`:303-304`): "Any change to `KbdActions.Remove`'s signature or contract, and any
  `TryRemove`-style addition."
- **Out of scope** (`:311`): "Any behaviour change to the Explorer surface (`EfcFormController.cs`,
  `EfcItemController.cs`)."
- **Files it may write** (`:317`): `QuickFiler/Controllers/KbdActions.cs` — "**Constructor guard only.**"
- Design summary (`:509`): the constructor "materialises the sequence, then scans it for a repeated
  `(SourceId, StoredKey)` pair and throws `ArgumentException` naming the offending pair."
- Constraints (`:530-545`): materialise before scanning so a null argument still yields
  `ArgumentNullException`; enumerate once; compare with `StoredKeyEquals` never `KeyEquals`; log before
  throwing; reuse the literal fragment `already exists`; keep the scan O(n²).

**POST-444 contract, and this is what 464 must be authored against:**

| Member | Post-444 |
|---|---|
| indexer `set` | **UNCHANGED** — still assign-only-if-present, still a silent no-op for a missing key |
| indexer `get` | **UNCHANGED** |
| `Add` (both overloads) | **UNCHANGED** — still throws `ArgumentException` on a duplicate `(SourceId, key)` |
| `Remove` | **UNCHANGED** — still returns `false` silently for an absent pair |
| `ContainsKey`, `Find`, `FindIndex`, `Keys` | **UNCHANGED** |
| `KbdActions(IEnumerable<UClass>)` | **CHANGED** — now throws `ArgumentException` when the seed list repeats a `(SourceId, StoredKey)` pair |

**Consequence for 464.** The only 464-visible effect of 444 is on the three enumerable-constructor call
sites inside `EfcFormController`. All three seed **distinct** keys, verified by direct read:

- `RegisterAlwaysOnAsyncKeyActions` — `EfcFormController.cs:354-366`; one entry,
  `new KaKeyAsync("Collection", Keys.Return, (k) => ActionOkAsync())` at `:363`.
- `GetAsyncCharacterActions` — `EfcFormController.cs:570-601`; eight `KaCharAsync` entries with keys
  `S F K X R N T M`, all distinct under `SourceId == "Controller"`.
- `GetKbdActions` — `EfcFormController.cs:627-675`; eight `KaChar` entries with the same eight distinct keys.

None of the three will throw under 444's new guard. **444 imposes no work on 464 and no re-authoring of
464's remedies.** This resolves the "single highest-risk coupling" as low risk.

**Disagreement to report.** 444's spec cites these same three sites (its `:146` and `:544`) as
`EfcFormController.cs:358-367`, `:574-602`, `:631-676`, and cites the `Keys.Return` binding (its `:560`)
as `EfcFormController.cs:365`. The current tree has `:354-366`, `:570-601`, `:627-675`, and `:363`. 444's
spec declares its citation basis as commit `988e819b`; `EfcFormController.cs` has changed since. 444
itself does not write that file, so the divergence is cosmetic for 444 but would be a defect if 464
copied those numbers forward.

---

## Q2 — `Initializer.GetOrLoad` overloads and the concrete remedy shape

`UtilitiesCS/HelperClasses/Initializer.cs` is **326 lines**. There are **eight** `GetOrLoad` overloads:

| # | Signature | Location | Dependency handling |
|---|---|---|---|
| 1 | `GetOrLoad<T>(ref T, Func<T> loader)` | `:103-110` | none |
| 2 | `GetOrLoad<T>(ref T, Func<T> loader, Action<T> callbackOnSet)` | `:112-120` | none |
| 3 | `GetOrLoad<T>(ref T, Func<T> loader, bool strict, params object[] dependencies)` | `:124-139` | check at `:131`; failure returns `default(T)` |
| 4 | `GetOrLoad<T>(ref T, Func<T>, Action<T>, bool strict, params object[])` | `:142-158` | check `:150`; failure returns `default(T)` |
| 5 | `GetOrLoad<T>(ref T, Func<T,bool> isInitialized, Func<T> loader, bool strict, params object[])` | `:160-176` | check `:168`; failure returns `variable` unchanged |
| 6 | `GetOrLoad<T>(ref T, Func<T,bool>, Func<T>)` | `:178-189` | none |
| 7 | `GetOrLoad<T>(ref T, T defaultValue, Func<T> loader, params object[])` | `:191-223` | check `:198` with `strict: false`; failure sets and returns `defaultValue` |
| 8 | `GetOrLoad<T>(ref T, T defaultValue, Func<T>, Action<T> defaultSetAndSaver, params object[])` | `:225-263` | check `:233`; same |

`DependenciesNotNull(bool strict, params object[] dependencies)` is at `:290-324`. It rejects a null
array (`:292-301`), an empty array (`:302-309`), and an array containing any null (`:310-322`). Under
`strict: true` each rejection **throws `ArgumentNullException`**; under `strict: false` each returns
`false`.

**Every overload takes `params object[] dependencies`.** There is **no** overload accepting
lazily-evaluated dependencies (`Func<object[]>`, `Func<bool>`, or an expression tree). The argument array
is therefore materialised at the call site **before** the method is entered, which is precisely why
`_globals.Ol` throws before any check can run.

### Q2.1 Which overload each defective accessor binds

- `EfcFormController.ActiveTheme` getter (`:255`) —
  `Initializer.GetOrLoad(ref _activeTheme, LoadTheme, strict: true, _themes)` binds **overload 3** with
  `T = string`. `_themes == null` makes `dependencies` an array containing null, so
  `DependenciesNotNull` throws `ArgumentNullException` at `Initializer.cs:321`.
- `EfcFormController.LoadTheme` (`:264-269`) — dereferences `_themes[activeTheme]` at `:267` with no
  guard, and reads `DarkMode` at `:266`, so it faults on a null `_themes` even when reached directly.
- `EfcFormController.DarkMode` getter (`:274-281`) — binds **overload 3** with `T = bool`,
  `strict: false`, `dependencies = [_globals, _globals.Ol]`. The **`_globals.Ol` element at `:280` is
  evaluated eagerly**, so a null `_globals` throws `NullReferenceException` before `DependenciesNotNull`
  can inspect it. The failure path of overload 3 would have returned `default(bool) == false`, which is
  the intended default.
- `EfcItemController.DarkMode` getter (`:441-448`) — identical shape, eager `_globals.Ol` at `:447`.
- **Not named in `issue.md`, but the same defect:** `EfcItemController.ActiveTheme` getter (`:395`) uses
  `strict: true` with `_themes` as the sole dependency, and `EfcItemController.LoadTheme` (`:404-409`)
  dereferences `_themes[activeTheme]` at `:407`. These are the item-side twins of the two
  `EfcFormController` sites `issue.md` does name. **They must be in scope for #464 A**, otherwise RC1's
  own argument ("fixing #464 A without fixing the adjacent members would leave the same class of defect
  live") is violated inside the very file the fix touches.

### Q2.2 The concrete remedy shape — no `Initializer` change is required

The QFC twin does **not** add an overload. It hoists the check into a plain conditional expression at the
call site, keeping the `Initializer` call intact for the non-null path:

```csharp
// QfcFormController.cs:131-142
public bool DarkMode
{
    get =>
        _globals?.Ol is null
            ? _darkMode
            : Initializer.GetOrLoad(ref _darkMode, () => _globals.Ol.DarkMode, false, _globals, _globals.Ol);
```

```csharp
// QfcFormController.cs:100-105
public string ActiveTheme
{
    get =>
        _themes is null
            ? _activeTheme
            : Initializer.GetOrLoad(ref _activeTheme, LoadTheme, strict: true, _themes);
```

**Recommendation: adopt the twin shape verbatim.** It requires **zero** change to
`UtilitiesCS/HelperClasses/Initializer.cs`, so the scope question `issue.md` anticipates does not arise.

The alternative — adding a `Func<object[]>` or `Func<bool>` overload to `Initializer` — is rejected:
`Initializer.cs` is outside 464's declared owned-file set, it is consumed repo-wide (the `GetOrLoad`
family alone is called from `EfcFormController`, `EfcItemController`, `QfcFormController`,
`ConversationResolver`, and many others), and a new overload would create a coverage obligation on an
unowned file for no additional defect closure.

---

## Q3 — QFC twin audit, per sub-defect

`issue.md` asserts "the already-merged QFC twins carry exactly the guards the EFC side lacks". That is
**true for two of the four sub-defects, partially true for one, and false for one**.

| Sub-defect | Twin member | Twin location | Verdict |
|---|---|---|---|
| **#464 A** (`ActiveTheme`, `LoadTheme`, `DarkMode`) | `QfcFormController.ActiveTheme` / `LoadTheme` / `DarkMode` | `QfcFormController.cs:100-105`, `:107-117` (setter `TryGetValue`), `:120-128`, `:131-142`, `:143-154` (setter `_globals?.Ol is not null`) | **TWIN EXISTS — mirror it.** All five guard shapes present. |
| **#465 A** (non-idempotent `Cleanup`) | `QfcFormController.Cleanup()` | `QfcFormController.SetupDisposal.cs:208-228` | **TWIN EXISTS — mirror it.** See below. |
| **#460 A** (`Buttons` deref in `Cleanup`) | `QfcItemController.Cleanup()` | `QfcItemController.ViewerSetup.cs:396-425` | **PARTIAL.** The pre-484 twin simply does not perform the dereference. The POST-484 twin does, with the guard shape 464 needs. |
| **#460 C** (`Subject` reads the viewer) | — | — | **NO TWIN.** `QfcItemController` declares no `Subject`, `Sender`, or `To` property at all. Design required. |
| **#461** (dead `nameof` guard) | — | — | **NO TWIN.** `QfcItemController.cs:285` carries the equivalent handler **commented out**. Design required. |

### Q3.1 #465 A — the twin is exact and is the whole remedy

```csharp
// QfcFormController.SetupDisposal.cs:208-228
public void Cleanup()
{
    if (_globals?.Ol is not null)              // :210
    {
        _globals.Ol.PropertyChanged -= DarkMode_CheckedChanged;   // :212
    }
    UnregisterFormEventHandlers();             // :215
    _undoQueue?.Dispose();                     // :216
    _globals = null;                           // :217
    _formViewer?.Dispose();                    // :218
    _formViewer = null;                        // :219
    ...
    _parentCleanup?.Invoke();                  // :226
    _parentCleanup = null;                     // :227
}
```

Against `EfcFormController.Cleanup()`:

```csharp
// EfcFormController.cs:187-194
public void Cleanup()
{
    _globals.Ol.PropertyChanged -= DarkMode_Changed;   // :189  unguarded
    _globals = null;                                   // :190
    _formViewer = null;                                // :191
    _dataModel = null;                                 // :192
    _parentCleanup.Invoke();                           // :193  unguarded, and never nulled
}
```

The twin is idempotent by construction: the `-=` is guarded, and `_parentCleanup` is null-conditionally
invoked **and then nulled**, so a second call is a total no-op. The EFC version throws
`NullReferenceException` at `:189` on a second call and, if that were merely guarded, would
**double-invoke `_parentCleanup`** — a second defect the promoted document does not name.

`Cleanup()` is reached from five sites in `EfcFormController`: `:479` and `:510` (`ButtonCreate_Click`),
`:727` (`ActionOkAsync`), `:737` (`ActionCancelAsync`), `:790` (`CreateFolderAsync`).

### Q3.2 #465 A — the re-entrancy precedent

`EfcHomeController.TryBeginExecuteMoves` (`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:48-57`),
paired with `ResetExecuteMovesState` (`:59-62`) inside a `try`/`finally` at `:38-45`, is the in-repo
`_isExecuting` guard shape. Note that `EfcHomeController.*` is owned by feature #442 (see Q11); 464 may
**cite** it as a pattern but must not edit it.

Note also that feature #442's spec (`quickfiler-home-controller-metrics-442/spec.md:315`) replaces that
`volatile` check-then-set with `Interlocked.CompareExchange`. If 464 mirrors the precedent it should
mirror the **post-442** shape, or use the simpler `_parentCleanup?.Invoke(); _parentCleanup = null;`
idempotence of the QFC twin, which needs no flag at all. **Recommendation: use the QFC twin's
idempotence, not a new `_isExecuting` flag.** It is smaller, it is the twin, and it does not duplicate a
concurrency primitive that a sibling feature is currently changing.

### Q3.3 #460 A — what the post-484 twin gives

Pre-484, `QfcItemController.Cleanup()` (`ViewerSetup.cs:396-425`) does not touch `Buttons` at all, so
there is nothing to mirror. Post-484 it will, and 484's spec fixes the required guard shape
(`qfc-item-controller-defects-484/spec.md:425-426`): "the `Buttons` and `MenuItems` loops guarded against
null". 484's spec also states (`:358`) that `Cleanup()` "must tolerate a null `_itemViewer`, a null
`_kbdHandler`, a null `Buttons`". **464 should adopt exactly that wording and shape.**

484's spec additionally names `EfcItemController.Cleanup()` at `EfcItemController.cs:257-262` as the
**in-repo precedent for delegate-identity detachment** (`:439`). That citation is **current and correct**
against the tree. The EFC side is therefore the source of the pattern 484 is porting, while being the
side that lacks the guards.

484's downstream note 6 (`:279-281`) records that `QfcItemController.Cleanup()` has its own duplicate
assignments (`ViewerSetup.cs:407`/`:423` for `_itemViewer`), matching the EFC duplicate at
`EfcItemController.cs:264`/`:276`, and recommends promoting them rather than absorbing them. 464 owns
`EfcItemController.cs` and #460 A explicitly names the duplicate, so 464 **may** remove its own; it must
not touch the QFC one.

---

## Q4 — `ConversationResolver` raised property names, and why #461's remedy is removal

### Q4.1 Exhaustive enumeration of raised names

`NotifyPropertyChanged` is declared at `ConversationResolver.Loading.cs:292-300` with
`[CallerMemberName] string propertyName = ""`. Every invocation, explicit or caller-defaulted:

| Raised name | Site | Form |
|---|---|---|
| `"ConversationInfo"` | `ConversationResolver.Loading.cs:26` | explicit `nameof(ConversationInfo)` |
| `"ConversationInfo"` | `ConversationResolver.Loading.cs:33` | caller-defaulted, inside the `ConversationInfo` setter (`:30-34`) |
| `"ConversationItems"` | `ConversationResolver.Loading.cs:167` | explicit `nameof(ConversationItems)` |
| `"ConversationItems"` | `ConversationResolver.Loading.cs:174` | caller-defaulted, inside the `ConversationItems` setter (`:171-175`) |
| `"Df"` | `ConversationResolver.Loading.cs:205` | explicit |
| `"Df"` | `ConversationResolver.Loading.cs:227` | explicit, inside `DfNotifyIfNotNull` (`:223-229`) |
| `"UpdateUI"` | `ConversationResolver.cs:277` | explicit |

**Exactly four distinct names: `"ConversationInfo"`, `"ConversationItems"`, `"Df"`, `"UpdateUI"`.**
`"Expanded"` is never raised. `issue.md`'s RC6 claim is **verified**.

`nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded)` at `EfcItemController.cs:746` resolves
to `"Expanded"` because `ConversationInfo` is `Pair<List<MailItemHelper>>` (`Loading.cs:20`) and `Expanded`
is a member of `Pair<T>`. The guard can never be true; the body at `:749-753` is dead.

### Q4.2 The intended behaviour is already delivered by a live, different path

This is the decisive finding for #461.

1. `EfcItemController.PopulateConversation()` assigns
   `_dataModel.ConversationResolver.UpdateUI = SetTopicThread;` at **`EfcItemController.cs:314`**.
2. `SetTopicThread(List<MailItemHelper>)` at **`EfcItemController.cs:354-359`** performs
   `_itemViewer.TopicThread.SetObjects(conversationInfo)` then
   `_itemViewer.TopicThread.Sort(_itemViewer.SentDate, SortOrder.Descending)`.
3. The dead handler body at **`EfcItemController.cs:750-753`** performs
   `SetObjects(_dataModel.ConversationResolver.ConversationInfo.Expanded)` then the **identical** sort.
4. `ConversationResolver.LoadConversationInfoAsync` assigns `ConversationInfo = pair` at
   **`Loading.cs:138`** and then, when `UpdateUI is not null` (`:140`), awaits
   `UiThread.Dispatcher.InvokeAsync(() => UpdateUI(pair.Expanded))` at **`Loading.cs:150`**.
5. Separately, `ConversationResolver` self-subscribes: `EfcDataModel.cs:68-69` performs
   `_conversationResolver.PropertyChanged += _conversationResolver.Handler_PropertyChanged;`, and
   `Handler_PropertyChanged` (`Loading.cs:304-325`) reacts to `"Df"` (`:306`) by running
   `BackgroundInitInfoItemsAsync` and to `"UpdateUI"` (`:316`) by dispatching
   `UpdateUI(ConversationInfo.Expanded)` (`:320-322`).

So the background-loaded conversation rows **do** reach the topic thread, through `UpdateUI`. The
subscription at `EfcItemController.cs:666-669` is redundant with it, and the handler body is a duplicate
of `SetTopicThread`.

### Q4.3 Remedy options, with a recommendation

- **Option A (recommended) — delete the dead handler and its subscription.** Remove
  `ConversationResolverPropertyChanged` (`:741-755`) and the `if (…) { … += … }` block at `:666-669`.
  Behaviour is unchanged because `UpdateUI` already carries the intent. This closes #461's real defect
  (a member that reads as live and is not) and removes an `async void` from the file, which also
  discharges part of RC3.
- **Option B — retarget the guard to `"ConversationInfo"`.** This makes the handler fire. It would then
  run `SetObjects` + `Sort` **in addition to** the `UpdateUI` dispatch at `Loading.cs:150`, doubling the
  work on every background load, on a different thread-marshalling path
  (`await _itemViewer.UiSyncContext` at `:749` vs `UiThread.Dispatcher.InvokeAsync`), and reading the
  **lazy** `ConversationInfo.Expanded` getter, which `Loading.cs:148-149` explicitly documents as
  something the resolver avoids ("Pass `pair.Expanded` directly to avoid triggering the lazy property
  getter and the associated synchronous `LoadConversationInfo()` call"). **Rejected.**

The promoted document (`2026-08-07-efc-item-controller-dead-conversation-expanded-handler.md:78-81`)
says "`ConversationInfo` is the most likely, but that should be confirmed against the intended behavior
rather than assumed." The confirmation performed here says otherwise: the intent is already met, so the
correct disposition is removal.

**No QFC twin exists.** `QfcItemController.cs:285` shows a commented-out `Handler_PropertyChanged`. The
QFC side reached the same conclusion by commenting the member out rather than deleting it.

---

## Q5 — `EfcViewer.ProcessCmdKey`

### Q5.1 The member, verbatim

```csharp
// EfcViewer.cs:94-105
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    if ((_keyboardHandler is not null) && (keyData.HasFlag(Keys.Alt)))
    {
        object sender = FromHandle(msg.HWnd);
        var e = new KeyEventArgs(keyData);
        _keyboardHandler.ToggleKeyboardDialogAsync(sender, e);
        return true;
    }
    return base.ProcessCmdKey(ref msg, keyData);
}
```

### Q5.2 Is there a "do you claim this key" query? — **NO**

`QuickFiler/Interfaces/IQfcKeyboardHandler.cs` is 37 lines and declares, in full: `KbdActive` (`:11`),
`ToggleKeyboardDialog()` (`:12`), `ToggleKeyboardDialog(object, KeyEventArgs)` (`:13`),
`ToggleKeyboardDialogAsync()` (`:14`), `ToggleKeyboardDialogAsync(object, KeyEventArgs)` (`:15`),
`KeyboardHandler_PreviewKeyDownAsync` (`:16`), `KeyboardHandler_KeyDown` (`:17`),
`KeyboardHandler_KeyDownAsync` (`:18`), the six `KbdActions<>` registries (`:21-26`),
`CboFolders_KeyDownAsync` (`:28`), and `BreadcrumbArrowFallThrough` (`:32-35`).

**There is no `Claims`, `CanHandle`, `Handles`, `ShouldHandle`, or `TryHandle` member.** The nearest
available primitives are the registries' own `ContainsKey(TKey)` (`KbdActions.cs:49`) and `Keys`
(`:141-144`), but these are keyed on a bare `char` or `Keys` value and carry no notion of an Alt chord.

### Q5.3 The QFC viewer twin has the same over-claim

```csharp
// QfcFormViewer.cs:56-73
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    if ((_keyboardHandler is not null) && Controllers.QfcFormKeyHandler.IsAltKeyCommand(keyData))
    {
        SynchronizationContext.SetSynchronizationContext(UiSyncContext);
        object sender = FromHandle(msg.HWnd);
        var e = new KeyEventArgs(keyData);
        e.Handled = true;
        _ = _keyboardHandler.ToggleKeyboardDialogAsync();
        return true;
    }
    return base.ProcessCmdKey(ref msg, keyData);
}
```

`QfcFormKeyHandler.IsAltKeyCommand(Keys keyData) => keyData.HasFlag(Keys.Alt)` is at
`QuickFiler/Controllers/QfcFormKeyHandler.cs:18`, in a **20-line** `internal static` class whose XML
comment (`:6-9`) states its purpose: "so the key-command logic can be unit tested without a live `Form`
window handle."

So the QFC twin **does not fix the over-claim**. What it supplies is the **testability seam pattern**:
a pure `internal static` predicate lifted out of the `ProcessCmdKey` override, exercised by
`QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` (**67 lines**, four `[TestMethod]`s at `:16`,
`:29`, `:42`, `:55`) with no `Form` instance anywhere.

Two other differences worth carrying into the fix: the QFC twin sets the synchronization context before
dispatching, and it calls the **parameterless** `ToggleKeyboardDialogAsync()` with a `_ =` discard,
whereas `EfcViewer.cs:100` calls the `(object, KeyEventArgs)` overload whose declared return is `void`
(`IQfcKeyboardHandler.cs:15`), so no discard is possible and no fault is observable.

### Q5.4 Minimal remedy

Narrow the guard so it consults the registries for a claim before returning `true`. Because there is no
handler-side query member and `IQfcKeyboardHandler.cs` is **not** in 464's owned-file set, the claim
predicate must be a pure function over `(IQfcKeyboardHandler, Keys)` that reads the public registries.
The concrete shape, mirroring `QfcFormKeyHandler`:

```
internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)
    => handler is not null
       && keyData.HasFlag(Keys.Alt)
       && <the registry lookup the spec selects>;
```

**Placement is a plan decision with a scope consequence:**

- **Option 1 (recommended): an `internal static` member on `EfcViewer` itself.** `EfcViewer.cs` is owned
  (162 lines, ample headroom), `[assembly: InternalsVisibleTo("QuickFiler.Test")]` exists at
  `QuickFiler/Properties/AssemblyInfo.cs:5`, a static member is callable without instantiating the
  `Form`, and **no `QuickFiler.csproj` edit is required**. `EfcViewer` carries
  `[ExcludeFromCodeCoverage]` at `EfcViewer.cs:20`, so the member would not be *measured*; it would
  still be fully *tested*. Note that `qfc-item-controller-defects-484/spec.md:235` forbids **adding** an
  exemption; reusing the file's pre-existing class-level attribute adds none.
- **Option 2: a new `QuickFiler/Controllers/EfcViewerKeyHandler.cs`.** Cleanest for coverage (no
  exemption in the way), but it requires a `QuickFiler.csproj` `<Compile Include>` edit, which
  `issue.md:220` permits "only if RC11-D removes orphaned files". §Q7.6 shows RC11-D needs **no** csproj
  edit, so this option would introduce the project-file edit rather than piggyback on one, and it would
  contend with feature #501, which adds one line after `QuickFiler.csproj:392`. **Surface as a scope
  question if the spec prefers measured coverage over a zero-csproj diff.**
- **Option 3: extend `QfcFormKeyHandler.cs`.** Rejected — that file is not in 464's owned set.

### Q5.5 Menu strips and mnemonics currently lost

Two `MenuStrip` instances on the form, both constructed in `EfcViewer.Designer.cs`:

| Strip | Constructed | Field declared | Notes |
|---|---|---|---|
| `FilterMenuStrip` | `:61` | `:4263` (`private`) | added to `Tlp` at `:137`; configured `:4082-4093` |
| `MoveOptionsStrip` | `:68` | `:4268` (`internal`) | set as `this.MainMenuStrip` at `:4224` |

Items carrying an `&` mnemonic, confirmed exhaustively by searching the Designer for `.Text = "…&…"`:

| Item | Designer line | Text | Level |
|---|---|---|---|
| `FiltersMenu` | `:4102` | `"&Filters"` | **top-level on `FilterMenuStrip`** — Alt+F |
| `MoveOptionsMenu` | `:4162` | `"&Move Options"` | **top-level on `MoveOptionsStrip`** — Alt+M |
| `ConversationMenuItem` | `:4173` | `"Move &Conversation"` | drop-down child |
| `SaveAttachmentsMenuItem` | `:4183` | `"Save &Attachments"` | drop-down child |
| `SaveEmailMenuItem` | `:4193` | `"Save E&mail Copy"` | drop-down child |
| `SavePicturesMenuItem` | `:4203` | `"Save &Pictures"` | drop-down child |

**Exactly two Alt chords are lost: Alt+F and Alt+M.** The four drop-down mnemonics are reached only once
a menu is already open, at which point `ProcessCmdKey` on the form is not the routing path, so they are
not directly affected. `EditFiltersMenuItem.Text = "Edit Existing Filters"` (`:4138`) carries **no**
mnemonic.

**Collision note.** `EfcFormController.GetAsyncCharacterActions` registers `'M'` at
`EfcFormController.cs:594-598` bound to `ShowMenu(_formViewer.MoveOptionsMenu)`. Any narrowing of the
Alt guard must decide whether Alt+M is a keyboard-handler claim (open the menu via the registered action)
or a WinForms mnemonic (open the same menu via `base.ProcessCmdKey`). Both reach `MoveOptionsMenu`, so
either answer is behaviourally acceptable; the spec should state which and pin it by test.

---

## Q6 — the EN DASH incognito literal

### Q6.1 The correct spelling

Chromium command-line switches are introduced by **two ASCII hyphen-minus characters (U+002D U+002D)**.
`CoreWebView2EnvironmentOptions.AdditionalBrowserArguments` is passed through to the browser process
verbatim; an unrecognised token is ignored silently. The correct literal is `"--incognito "`. The
in-file counter-evidence is the commented alternative directly above two of the three sites, which uses
ASCII correctly: `"--disk-cache-size=1 "` at `EfcItemController.cs:182` and `:215`, and at
`QfcItemController.ViewerSetup.cs:54`.

`UNVERIFIED`: no runtime observation of WebView2 behaviour was performed. The claim that Chromium
ignores an unrecognised switch is standard Chromium behaviour, not something I established from this
repository.

### Q6.2 The three sites, and whether fixing each is meaningful

| Site | Enclosing member | Reachable? | Meaningful to fix? |
|---|---|---|---|
| `EfcItemController.cs:184` | `InitializeWebView()` (`:174-205`) | **NO** — zero call sites (§Q7.2) | **No.** Superseded by RC11-B removal. |
| `EfcItemController.cs:217` | `InitializeWebViewAsync()` (`:207-240`) | **YES** — `Task.Run(() => InitializeWebViewAsync())` at `:110` and `:164` | **Yes.** |
| `QfcItemController.ViewerSetup.cs:55` | `InitializeWebViewAsync()` (`:42-128`) | **YES** — see below | **Yes.** |

`issue.md`'s framing needs one correction. It states RC11-B says `InitializeWebView()` at `:174` is dead,
implying `:184` and `:217` are both in dead code. **`:217` is in the live `async` member.** Only `:184`
is dead. So:

- **`:184` — do not edit. Delete the member.** Editing a literal inside a member the same feature deletes
  in another phase is churn, and if the phases were ordered the other way the edit would be lost.
- **`:217` — fix.** This is the live EFC site.
- **`ViewerSetup.cs:55` — fix.** See the collision check below.

### Q6.3 Collision check against upstream 484 — **NO COLLISION**

I searched the entire `docs/features/active/qfc-item-controller-defects-484/` folder for `incognito`,
`CoreWebView2EnvironmentOptions`, and `ViewerSetup.cs:5x`. **Zero matches.** 484 does not mention the
literal anywhere.

484's declared change to `InitializeWebViewAsync()` is enumerated exactly in its upstream-contract table
(`484/spec.md:363`): "The `WebResourceRequested` lambda body is replaced by a two-statement adapter over
`TryResolveCidResource`; the delegate and its `CoreWebView2` source are captured into fields. Remains
`internal async Task` and remains `[ExcludeFromCodeCoverage]`."

Reading the member confirms the edit region is disjoint from line 55:

- `[ExcludeFromCodeCoverage]` — `ViewerSetup.cs:41`
- member signature — `:42`
- **`CoreWebView2EnvironmentOptions options = new("–incognito ");` — `:55`**
- `var coreWebView2 = ((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2;` — `:79` (a capture site)
- `coreWebView2.WebResourceRequested += (sender, e) => { … };` — **`:84-105`** (the replaced lambda)

Line 55 sits **29 lines above** the earliest line 484 touches, so 484's edit cannot move, rewrite, or fix
it, and cannot shift its line number. **464 must edit `ViewerSetup.cs:55` itself.**

The residual risk is a *textual merge* risk, not a semantic one: both features write the same file. 464
owns exactly one line of it (`issue.md:212-214`); 484 owns lines `:79` onward plus new private fields.
Since 464 branches from an integration branch that already carries 484, the edits are sequential and no
merge is required. **Recommendation: the plan states that this is the only line of `ViewerSetup.cs`
464 writes, and an acceptance criterion asserts the diff for that file is exactly one line.**

Two consequences the spec must record:

1. `InitializeWebViewAsync` is `[ExcludeFromCodeCoverage]` on both sides (`ViewerSetup.cs:41`; the EFC
   member has no per-member attribute but its whole class does — `EfcItemController.cs:25`), so **no
   regression test can execute either literal**. See §Q8.6 for the alternative.
2. The literal appears in **three** files but the fix appears in **two** after `:184` is deleted.

---

## Q7 — dead-code verification (RC11 / #466)

Every claim was tested by a repository-wide `*.cs` search on the identifier.

### Q7.1 `EfcViewer.SetController` — **ZERO CALL SITES, confirmed**

Declared `internal void SetController(EfcFormController controller)` at `EfcViewer.cs:50-53`; assigns
`_formController` (`:48`). Repository-wide, the only other `EfcFormController`-typed `SetController` is
the declaration in the **uncompiled** `QuickFiler/Viewers/EfcViewer3.cs:39`. `EfcFormController` never
calls it, unlike `QfcFormController.cs:44` (`_formViewer.SetController(this);`). So
`EfcViewer._formController` is permanently null.

### Q7.2 `InitializeWebView()` — **ZERO CALL SITES, confirmed**

`EfcItemController.cs:174` is the only occurrence of the token `InitializeWebView()` in the repository.
The two `Task.Run(() => InitializeWebViewAsync())` sites at `:110` and `:164` invoke the different,
`Async`-suffixed member.

### Q7.3 `RegisterActions` — **ZERO CALL SITES, confirmed**

`EfcItemController.cs:680` is the only occurrence. (`RegisterAsyncFocusActions` at `:694` and
`UnregisterActions` at `:732` are distinct identifiers and do not match.)

### Q7.4 The 7-argument constructor — **ZERO CALL SITES, confirmed** (plus a larger finding)

`new EfcItemController(` occurs at exactly two sites, both in `EfcFormController.cs`:

- `:67-73` — the **5-argument** overload (`EfcItemController.cs:59-74`), from the second public
  `EfcFormController` constructor.
- `:85-92` — the **6-argument** overload (`EfcItemController.cs:30-42`), from `Initialize()`.

The **7-argument** overload at `EfcItemController.cs:44-57` has zero call sites. Confirmed.

**Larger finding, not in `issue.md`.** In the same search I established that
**`EfcItemController.Cleanup()` (`:255-278`) has zero call sites repo-wide**:

- `EfcFormController` references `_itemController` at `:67`, `:85`, `:105`, `:114`, `:142`, `:927`,
  `:936`, `:943`, `:952` — and **never** calls `.Cleanup()` on it.
- Every `.Cleanup()` call in `QuickFiler/**/*.cs` is at `QfcFormLegacyViewer.cs:87` (uncompiled),
  `QfcCollectionController.cs:1003`, `:1017`, `:1038`, `:1783`,
  `QfcFormController.EventHandlers.cs:92`, and `QfcHomeController.cs:390`. All are QFC or Legacy.
- `IItemControler` (`QuickFiler/Interfaces/IItemControler.cs:9-14`) declares only `CounterEnter`,
  `CounterComboRight`, and `RightKeyActions`. **It does not declare `Cleanup`.**
- `EfcItemController` never registers itself as `_itemViewer.Controller` — the assignment is commented
  out at `EfcItemController.cs:129`, whereas the QFC side does it at
  `QfcItemController.Initialization.cs:374`. So it is not reachable through `IItemViewer.Controller`
  either.

**Consequence:** #460 A (`Cleanup` NRE) and #460 B (timer leak on `Cleanup`) are **latent**, exactly like
#459 A. This does not remove them from scope — the members are public, the defects are real, and a
direct-invocation regression test is the correct instrument — but it changes their user-visible severity
from High to latent and it means neither can be reproduced through the UI.

### Q7.5 `_selectorsCtrls` — **confirmed**

Declared `private List<Control> _selectorsCtrls = null;` at `EfcItemController.cs:381`. Exactly three
occurrences repository-wide: the declaration, and reads at `:97` (`InitializeDataFields` →
`EfcThemeHelper.SetupThemes`) and `:144` (`Initialize` → same). Never assigned.

### Q7.6 Orphaned uncompiled files — **confirmed, and wider than `issue.md` implies**

`QuickFiler/QuickFiler.csproj` contains **no** `Viewers\EfcViewer3` entry of any kind. The only `Efc`
entries are `Viewers\EfcViewer.cs` (`:384`), `Viewers\EfcViewer.Designer.cs` (`:387`), and
`<EmbeddedResource Include="Viewers\EfcViewer.resx">` (`:492`).

Three `EfcViewer3` files exist on disk: `EfcViewer3.cs`, `EfcViewer3.Designer.cs`, `EfcViewer3.resx`.
`EfcViewer3.cs:17` carries `[ExcludeFromCodeCoverage]` on `public partial class EfcViewer3 : Form`
(`:18`) — the misleading attribute #466 D names. **Confirmed.**

Comparing the `QuickFiler/Viewers/*.cs` directory listing against the csproj's 42 `Viewers\` `Compile
Include` entries, **twenty** files in that one directory have no entry:

`EfcViewer3.cs`, `EfcViewer3.Designer.cs`, `Form1.cs`, `Form1.Designer.cs`,
`QFCItemViewerDarkNew.cs`, `QFCItemViewerDarkNew.Designer.cs`, `QFCItemViewerLightNew.cs`,
`QFCItemViewerLightNew.Designer.cs`, `QfcFormViewerDark.cs`, `QfcFormViewerDark.Designer.cs`,
`QfcFormViewerExpanded.cs`, `QfcFormViewerExpanded.Designer.cs`, `QfcItemViewer.cs`,
`QfcItemViewer.Designer.cs`, `QfcItemViewerExpandedLight.cs`, `QfcItemViewerExpandedLight.Designer.cs`,
`QfcItemViewerLightSelected.cs`, `QfcItemViewerLightSelected.Designer.cs`, `QfcItemViewerV1.cs`,
`QfcItemViewerV1.Designer.cs`.

`QuickFiler/Legacy/**` and `QuickFiler/Notes/**` are likewise entirely uncompiled (no
`Compile Include="Legacy` or `="Notes` matches anywhere in the csproj).

**Recommendation: RC11-D deletes only the three `EfcViewer3.*` files.** They are the ones #466 D names,
they are EFC-surface files this feature owns by subject matter, and their removal is provably
zero-impact. The seventeen `Qfc*`/`Form1` orphans are QFC-surface files that no epic child owns; deleting
them is a repository-hygiene refactor, not a bug fix, and should be promoted as a separate potential.

**Important scope consequence: RC11-D requires NO `QuickFiler.csproj` edit.** The files carry no project
entries to remove. `issue.md:220` conditions the csproj edit on RC11-D; that condition is not met, so
**464 should declare `QuickFiler/QuickFiler.csproj` untouched.** This eliminates all contention with
feature #501, which adds exactly one line after `QuickFiler.csproj:392`
(`breadcrumb-coordinator-hub-defects-501/spec.md:604`).

### Q7.7 Resolving the RC11-B / RC4-A tension — **RC4-A is moot; delete, do not repair**

`issue.md` flags the tension itself: RC11-B says `RegisterActions` is dead, RC4-A says it mis-registers.
Both are true (§Q7.3, §Q1.2). The resolution:

- `RegisterActions` has zero call sites and is `internal`, so no consumer exists.
- Its only correct repair would require deciding the intended `KbdActions<>` indexer-setter contract
  (upsert vs assign-if-present), which is a change to `KbdActions.cs` — a **file owned by feature 444**
  (`quickfiler-keyboard-action-defects-444/spec.md:317`), and one 444 explicitly restricts to
  "Constructor guard only".
- Repairing it inside `EfcItemController` instead (replace the indexer assignment with
  `Remove` + `Add`) would create correct-but-uncalled code, adding a `>= 90%` coverage obligation on a
  new code path with no consumer.

**Recommendation: delete `RegisterActions` (`:680-692`) under RC11-B. RC4-A is then closed by removal,
not by repair.** #459 A's promoted acceptance idea ("Decide and document the intended `KbdActions<>`
indexer-setter contract, then align `RegisterActions`") is satisfied by *documenting the contract in the
spec* — which §Q1.1 supplies — and by removing the sole mis-user. The contract decision itself belongs
with the owner of `KbdActions.cs`, i.e. feature 444 or a follow-up.

### Q7.8 Two further dead members found, not in `issue.md`

- **`EfcItemController.ToggleExpansion()` (`:838-848`) and `ToggleExpansion(Enums.ToggleState)`
  (`:862-905`) have zero reachable call sites.** `ToggleExpansion(ToggleState)` is called only from
  `ToggleExpansion()` (`:842`, `:846`). `ToggleExpansion()` is called from nowhere in `Efc*.cs`; the
  five `ItemController.ToggleExpansion()` call sites in the repository are all in
  `QfcCollectionController.cs` (`:1140`, `:1414`, `:1439`, `:1679`, and `:1212`/`:1696` for the async
  form) and all target `QfcItemController` through `IQfcItemGroup.ItemController`. `IItemControler`
  does not declare `ToggleExpansion`, and `EfcItemController` never sets `_itemViewer.Controller`
  (commented at `:129`), so no interface route exists. The live EFC expansion path is
  `RegisterAsyncFocusActions` `'E'` (`:701-705`) → `KbdExecuteAsync(ToggleExpansionAsync)` →
  `ToggleExpansionAsync()` (`:850-860`) → `ToggleExpansionAsync(ToggleState)` (`:907-929`) →
  `ToggleExpansionOn`/`Off`.
- **`EfcItemController.ToggleNavigation(bool async)` (`:958-979`) has zero call sites.**
  `EfcFormController.cs:927` and `:943` call the **two-argument** overload (`:981-994`).

### Q7.9 A disagreement with feature 484's upstream contract

`484/spec.md:369-370` states: "`ToggleNavigation(bool async)` is retained specifically because it is
declared on `IQfcItemController.cs:89` and implemented by `EfcItemController.cs:958`." The same claim
appears at `:226-227`.

**`EfcItemController` does not implement `IQfcItemController`.** Its declaration is
`internal class EfcItemController : IItemControler` (`EfcItemController.cs:26`), and `IItemControler`
declares three members, none named `ToggleNavigation` (`IItemControler.cs:9-14`). The member at
`EfcItemController.cs:958` is a coincidentally same-named method, not an interface implementation, and
it is itself dead (§Q7.8).

484's **decision** to retain the member may still be correct — it has a test caller — but its **stated
reason** is not supported by the source. This is reported, not acted on: `IQfcItemController.cs` is not
464's file, and 464 must not delete `EfcItemController.ToggleNavigation(bool)` on the strength of a
claim that a sibling feature is relying on the opposite reading. **Recommendation: 464 leaves
`ToggleNavigation(bool)` alone and records this as a cross-feature note to 484's owner.**

---

## Q8 — test harness feasibility

### Q8.1 Existing `QuickFiler.Test` coverage of the EFC surface

| File | Lines | Subject |
|---|---|---|
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | **168** | `EfcFormController` — 2 `[TestMethod]`s |
| `QuickFiler.Test/Controllers/EfcDataModelTests.cs` | not read | `EfcDataModel` |
| `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` and six siblings | not read | `EfcHomeController` — owned by feature #442 |
| **`EfcItemControllerTests.cs`** | — | **DOES NOT EXIST** |
| **`EfcViewerTests.cs`** | — | **DOES NOT EXIST** |

`EfcItemController` carries a class-level `[ExcludeFromCodeCoverage]` at `EfcItemController.cs:25`;
`EfcViewer` carries one at `EfcViewer.cs:20`. `EfcFormController` carries **none** (`:26`). The two
exempt types have zero tests today. **Exemption is a measurement decision, not a testability barrier** —
tests against exempt types run and assert normally; they simply do not move the coverage number.

### Q8.2 The proven construction seams

**Seam A — private no-argument constructor plus reflection field injection.** `EfcFormController` has
`private EfcFormController() { }` at `EfcFormController.cs:77`.
`EfcFormControllerTests.CreateMinimalController()` (`:22-32`) invokes it through
`GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null)`, and
`SetPrivateField` (`:159-166`) injects fields. This is the ideal seam for **every `EfcFormController`
defect** (#464 A/B/C, #465 A/B/C/D), because it produces exactly the all-fields-null post-`Cleanup` state
the defects describe.

**Seam B — `FormatterServices.GetUninitializedObject`.** Used in **26** `QuickFiler.Test` files, including
`QfcCollectionControllerTests.cs`, `QfcItemControllerBreadcrumbDropDownTests.cs`, and five
`EfcHomeController*` files. `EfcItemController` has no parameterless constructor, so this is the seam for
`EfcItemController` state-only tests.

**Seam C — headless real `ItemViewer`.** `QfcItemController.EventWiringTests.cs:229-309` constructs
`new QuickFiler.ItemViewer()` at `:236` with **no `Show()`, no message loop, no worker**, installs a bare
`SynchronizationContext` at `:232-233`, restores it in `finally` at `:305-308`, and raises events by
reflecting onto `Control.OnPreviewKeyDown` / `OnKeyDown` / `OnMouseEnter` (`:262-286`). 484's spec
authorises exactly this as its "one documented exception" (`484/spec.md:617-625`).

**Seam D — `CreateUninitialized<EfcViewer>()`.** Proven at
`QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:33`, `:62`, `:166`, `:247`. Produces an
`EfcViewer` without running `InitializeComponent()`.

**Seam E — full constructor injection for `EfcItemController`.** The 5-argument constructor
(`EfcItemController.cs:59-74`) requires `IApplicationGlobals` (interface, mockable),
`IFilerHomeController` (interface — `KeyboardHandler` and `ExplorerController` are both interface
properties at `IFilerHomeController.cs:30` and `:32`, so mockable), `EfcFormController parent`
(concrete — obtain via Seam A), `ItemViewer itemViewer` (concrete — obtain via Seam C), and a
`CancellationToken`. It does **not** call `Initialize()`, so no theme setup and no
`Task.Run(InitializeWebViewAsync)` runs. That is precisely the constructed-but-uninitialised state #460 A
requires.

**Structural guard to respect:** `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` (54 lines) asserts
that no `System.Windows.Forms.Form`-derived type is **compiled into** the test assembly (`:17-36`). It
constrains type declarations, not instantiation, so Seams C, D and E do not violate it. A test **fixture
class** deriving from `Form` would.

### Q8.3 Deterministic timer technique for RC2, inherited from 484

484's spec section `### Deterministic timer test for #484 (no Thread.Sleep, no Task.Delay)`
(`484/spec.md:634-650`) prescribes:

> **T1 — disposal is observable via `ObjectDisposedException` on `Change`.** Arrange a
> `new Timer(_ => { }, null, Timeout.Infinite, Timeout.Infinite)` — armed with `Timeout.Infinite` so it can
> never fire during the test — and reflection-inject it into `_emailIsReadTimer`. Act: call `Cleanup()`.
> Assert: the field is null, and `Action act = () => timer.Change(0, Timeout.Infinite);` throws
> `ObjectDisposedException`.
>
> **T2 — the callback guard is directly invocable.** `ApplyReadEmailFormat(object state)` is public. Call
> it on a freshly-`Cleanup()`ed controller and assert `act.Should().NotThrow()`.

**Both transfer to RC2 unchanged.** The EFC field is `_timer` (`EfcItemController.cs:377`), nulled at
`:277`, armed at `:875-876` and `:953-954` with the same
`new System.Threading.Timer(ApplyReadEmailFormat)` + `Change(4000, Timeout.Infinite)` pattern, and
`EfcItemController.ApplyReadEmailFormat(object state)` is `public` at `:1125-1129`. 484's downstream
note 1 (`484/spec.md:243-249`) addresses this feature directly and recommends
`_timer?.Dispose(); _timer = null;` plus "a null-collaborator early return to
`EfcItemController.ApplyReadEmailFormat`". Its `EfcItemController` citations (`:277`, `:953-954`) are
**current and correct** against the tree.

`ApplyReadEmailFormat` dereferences `_itemInfo` (`:1127`) and `_themes[_activeTheme]` (`:1128`), both of
which `Cleanup()` invalidates (`_itemInfo = null` at `:275`, `_themes = null` at `:269`), so the T2
guard is genuinely needed. **Reuse 484's technique; do not invent a new one.**

### Q8.4 Per-issue feasibility

| Issue / sub-defect | Seam | Deterministic MSTest achievable? | Notes |
|---|---|---|---|
| **#459 A** (`RegisterActions` mis-registers) | — | **N/A if deleted (recommended)** | If the spec instead repairs it: Seam B + `Mock<IQfcKeyboardHandler>` returning a real `KbdActions<char, KaChar, Action<char>>`; assert `Keys` content. Trivially deterministic. |
| **#459 B** (async path omits `'B'`/`'D'`) | B or E + `Mock<IQfcKeyboardHandler>` | **YES** | Invoke the `private` `ToggleExpansionOn()`/`ToggleExpansionOff()` (`:944`, `:931`) by reflection. Their bodies touch only `_itemViewer`, `_itemInfo`, `_timer` and (post-fix) `_keyboardHandler`. **They do not touch `_parent`**, so no `EfcViewer`/`Form` is needed. See the design note below. |
| **#459 C** (duplicate `Add` throws) | as above | **YES** | Drive On→Off→On through the single registration owner and assert `NotThrow` plus final `Keys` content. |
| **#460 A** (`Cleanup` NRE) | **E** (5-arg ctor, no `Initialize`) or B | **YES** | With Seam E the arrange is the literal repro from the promoted document. `Cleanup_AfterFiveArgumentConstructor_DoesNotThrow`. |
| **#460 B** (timer leak) | B + reflection-inject `_timer` | **YES** | 484's T1 verbatim. |
| **#460 C** (`Subject` inconsistency) | B + inject `_itemInfo` only | **YES** | Assert `Subject`, `Sender`, `To` all read the model; assert post-`Cleanup` behaviour is uniform across the three. |
| **#461** (dead handler) | B + `Mock<IConversationResolver>` | **YES** | Removal is asserted structurally (see below) plus a live-path test that `UpdateUI` is the sole route. |
| **#463** (`--incognito`) | **none — the literal is in an exempt, WebView2-bound member** | **NO end-to-end test** | See §Q8.6. |
| **#464 A** (eager dependency args) | **A** for the form; **B** for the item | **YES** | `DarkMode`/`ActiveTheme`/`LoadTheme` on an all-null controller must return the default rather than throw. Exactly the shape of the existing `PopulateFolderCombobox_WhenFormViewerIsNull_...` test (`EfcFormControllerTests.cs:38-57`). |
| **#464 B** (`async void` rethrow ×5) | **A** + `Mock<EfcViewer>`? **NO** — see below | **PARTIAL** | The five handlers dereference `_formViewer.UiSyncContext` (`:418`, `:434`, `:450`, `:466`) which is a concrete `EfcViewer` member, and an `async void` fault cannot be awaited by the test. **Test the extracted boundary instead**, not the handler. |
| **#464 C** (unobserved fire-and-forget) | **A** | **YES** | `PopulateFolderCombobox` is `public async Task` (`:1022`). Inject a `_dataModel` whose `InitFolderHandlerAsync` faults; assert `NotThrowAsync` and that the logged boundary ran. Requires `_formViewer` non-null to pass the guard at `:1027-1029`. |
| **#464 D** (`async void` lambdas) | — | **N/A if `:882`/`:887` are in dead code (§Q7.8)** | If retained: assert the registered delegate's runtime type is `Func<char, Task>`-shaped, or that a faulting jump target is contained. Weak. |
| **#464 E** (`throw (e.InitializationException)`) | **B** + a real `CoreWebView2InitializationCompletedEventArgs`? | **PARTIAL** | `WebView2Control_CoreWebView2InitializationCompleted` is `internal` (`:770-799`). Constructing `CoreWebView2InitializationCompletedEventArgs` with `IsSuccess == false` requires a non-public SDK constructor. **See §Q8.5.** |
| **#465 A** (non-idempotent `Cleanup`) | **A** | **YES** | `Cleanup(); Cleanup();` → `NotThrow`, and a `Mock`-backed `_parentCleanup` invoked `Times.Once()`. Injectable because `_parentCleanup` is `System.Action` (`:128`). |
| **#465 B** (cross-thread read) | **A** | **YES** | The read must be hoisted out of `Task.Run`. Assert by injecting a `_formViewer` substitute? **Not possible — `_formViewer` is the concrete `EfcViewer`.** See §Q8.5. |
| **#465 C** (duplicate trash rows) | **A** | **YES** | `_folderRows` is `string[]` (`:134`); `BindFolderRows` is `private` (`:871`) but reachable via `ActionDeleteAsync` (`public async Task`, `:740`). Inject `_folderRows` and a real `BreadcrumbBridgeRouter` (already proven at `EfcFormControllerTests.cs:113-125`), call `ActionDeleteAsync()` twice, assert one trash row. `await _formViewer.UiSyncContext` at `:742` requires `_formViewer` non-null → see §Q8.5. |
| **#465 D** (banner-prefix arity) | **A** | **YES, and it is the cleanest test in the feature** | `IsValidSelection` is `internal` (`:1038`) and reads `SelectedFolder` → `_router?.SelectedFolderPath` (`:292`). Inject a router; assert a three-`=` row and a four-`=` row classify identically in `IsValidSelection` and in `ActionOkAsync`'s guard. |
| **#466** (dead code) | — | **Structural assertions** | See below. |
| **#467** (`ProcessCmdKey`) | **the `QfcFormKeyHandler` pattern** | **YES, via the extracted predicate** | Four `[TestMethod]`s mirroring `QfcFormKeyHandlerTests.cs:16-65`: claimed Alt chord, unclaimed Alt chord, non-Alt chord, null handler. **No `Form` instance.** |

### Q8.5 The three real blockers, and the seam each needs

Three sub-defects cannot be tested through the current shape of `EfcFormController` / `EfcItemController`.
Each blocker is named with the minimal seam that removes it, so the plan can price it.

**Blocker 1 — `_formViewer` is the concrete `EfcViewer` (a `Form`).** Affects #465 B, #465 C, and the
`_formViewer.UiSyncContext` awaits at `:418`, `:434`, `:450`, `:466`, `:703`, `:734`, `:742`, `:762`,
`:788`. `EfcViewer` has no interface. Options, cheapest first:

1. **Seam D — `CreateUninitialized<EfcViewer>()`.** Yields an `EfcViewer` whose `_context` is null, so
   `UiSyncContext` returns null. `await (SynchronizationContext)null` — the repository's `await` on a
   `SynchronizationContext` is an extension awaiter; **`UNVERIFIED`: I did not read the awaiter to
   establish its null behaviour.** The plan must verify this before relying on it.
2. **Hoist the value under test out of the untestable call.** For **#465 B** this is not merely a test
   aid, it *is* the fix: the remedy is to read `_formViewer.SearchText.Text` on the UI thread before
   `Task.Run`. Extract `internal static string[] FindMatchesFor(IEfcDataModel model, string searchText)`
   or simply assert the read happens outside the lambda by a structural test. For **#465 C** the
   remedy — stop writing the bound result back into `_folderRows` at `:879`, or dedupe before inserting
   at `:746` — can be tested against a pure helper
   `internal static string[] WithTrashRow(string[] rows)`.
3. **Add an `IEfcViewer` interface.** Rejected: it would require writing `EfcViewer.cs` extensively and
   changing `EfcFormController`'s field type, a refactor of the kind CLAUDE.md's Bugfix Workflow step 2
   prohibits.

**Recommendation: option 2 for both.** Extracting a pure helper is a genuine part of each remedy (it is
what "hoist the read out of the `Task.Run` lambda" means), it is testable with no seam at all, and it
keeps the diff inside owned files.

**Blocker 2 — `async void` cannot be awaited.** Affects #464 B (five handlers). A test cannot observe
whether an `async void` method's continuation threw. The remedy shape resolves this: replace
`logger.Error(ex.Message, ex); throw;` with `logger.Error(ex.Message, ex);` (log and contain), and
extract the body to an awaitable `internal async Task` that the `async void` handler wraps. The test then
targets the extracted `Task`-returning member and asserts `NotThrowAsync` plus one logged error. The
in-repo precedent for the wrapped shape is `EfcFormController.InitializeBreadcrumbHostAsync`
(`:856-866`), which `issue.md` already nominates as the remedy pattern for #464 C.

**Blocker 3 — `CoreWebView2InitializationCompletedEventArgs` is not constructible.** Affects #464 E.
`WebView2Control_CoreWebView2InitializationCompleted` (`:770-799`) takes that SDK type and reads
`e.IsSuccess` (`:775`) and `e.InitializationException` (`:777`). Remedy shape that removes the blocker:
extract `internal static void ThrowInitializationFailure(System.Exception initializationException)`
carrying the `ExceptionDispatchInfo.Capture(initializationException).Throw()` call, and reduce the
handler's failure branch to a one-line adapter. The extracted member takes a plain `System.Exception`
and is fully testable: assert the rethrown exception preserves the original stack trace
(`ex.StackTrace.Should().Contain(<the throwing frame>)`), which is exactly what the current
`throw (e.InitializationException)` destroys.

### Q8.6 #463 — no executable test; use a structural assertion

The literal at `EfcItemController.cs:217` sits inside a member of a class carrying
`[ExcludeFromCodeCoverage]` (`:25`) and requires the real WebView2 runtime; the literal at
`QfcItemController.ViewerSetup.cs:55` sits inside a member carrying its own `[ExcludeFromCodeCoverage]`
(`:41`), which 484's spec (`:363`) states will remain. Neither can be executed under the unit-test
policy.

**Recommended instrument — a structural (source-reading) test**, which the repository already uses for
this class of assertion (`NoLiveFormInTestAssemblyTests.cs` is a structural guard; `484/spec.md:539-542`
relies on literal-fragment assertions; `.claude/rules/plan-acceptance-gates.md` governs literal searches
in plans). A test that reads the two source files and asserts that every
`CoreWebView2EnvironmentOptions` argument literal is pure ASCII and begins with `--` closes #463 and
prevents recurrence, which is exactly what the promoted document asks for
(`2026-08-07-quickfiler-webview2-incognito-arg-en-dash.md:83`: "Add a test asserting the
additional-browser-arguments string is pure ASCII and starts with `--`").

**Caveat the plan must handle:** a source-reading test needs a path to the source, and CLAUDE.md's
General Unit Test Policy §UT4 prohibits temporary files and mutable external configuration. Reading a
file that is committed to the repository is not a temporary file, but the path resolution must be
deterministic. **`UNVERIFIED`: I did not find an existing `QuickFiler.Test` structural test that reads a
production `.cs` file from disk.** If none exists, the alternative is a **compile-time** assertion: hoist
the literal into an `internal const string IncognitoArgument = "--incognito ";` in each owned file and
assert on the constant, which needs no file I/O at all. **Recommendation: the constant.** It is a
one-line change per site, it is directly assertable, and it removes the duplication that produced the
defect.

### Q8.7 #466 — structural assertions for dead-code removal

Removal is asserted by absence. The repository idiom (`484/spec.md:134`: "The executor verifies the site
is **absent**; it must not recreate the block in order to remove it") applies. Concretely: after RC11,
`typeof(EfcViewer).GetMethod("SetController", NonPublic|Instance)` is null;
`typeof(EfcItemController).GetMethod("InitializeWebView", NonPublic|Instance)` is null;
`typeof(EfcItemController).GetConstructors()` has length 2. Reflection-over-metadata assertions of this
shape are already used at `NoLiveFormInTestAssemblyTests.cs:20-28`.

### Q8.8 Test-file routing and line counts

| File | Current lines | Disposition |
|---|---|---|
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | **168** | **Extend.** 332 lines of headroom under the 500-line ceiling. Add `SetPrivateField`-driven tests for #464 A/C, #465 A/B/C/D. |
| **`QuickFiler.Test/Controllers/EfcItemControllerTests.cs`** | **0 — new file** | **Create.** #459, #460, #461, #464 D/E. Keep under 500; split into `EfcItemControllerTests.cs` + `EfcItemController.CleanupTests.cs` if it grows. |
| **`QuickFiler.Test/Controllers/EfcViewerTests.cs`** | **0 — new file** | **Create.** #467 (the extracted predicate) and, if RC11 removes members, the structural assertions. Model on `QfcFormKeyHandlerTests.cs` (67 lines). |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | **500 (exactly)** | **DO NOT TOUCH.** At the ceiling; `[TestMethod]` count frozen by feature #468. |
| `QuickFiler.Test/Controllers/QfcItemController.*Tests.cs` (11 files) | not measured | **DO NOT TOUCH.** Owned by features #484 and #489. |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs`, `KbdActionsRemainingBranchesTests.cs` | not measured | **DO NOT TOUCH.** Owned by feature #444. |

### Q8.9 Determinism compliance

Every proposed test above satisfies the hard rules: no `Thread.Sleep`, no `Task.Delay`, no wall-clock
wait (the only timer test uses `Timeout.Infinite` arming plus `ObjectDisposedException` observation, per
484's T1), no temporary file, no live Outlook, no `BackgroundWorker`, no shown WinForms form. Seam C
constructs a real `ItemViewer` but never shows it and never starts a message loop, matching the
authorised exception at `484/spec.md:617-625`.

---

## Q9 — file-size ceiling reality

`.claude/rules/general-code-change.md` states: "No production code, test code, or reusable script file
may exceed **500 lines**." CLAUDE.md's General Code Change Policy §4.1 states the same.

**Confirmed: the ceiling can only be asserted over files this feature creates.** `issue.md:230-234`
already scopes it that way and the source supports it: both controllers exceed 500 lines at the merge
base and predate this feature. Sibling feature #498 records the identical position for the same file
(`breadcrumb-router-navigation-defects-498/spec.md:321`, `:872`): "`FolderPredictor.cs` (983 lines) and
`EfcFormController.cs` … are PRE-EXISTING 500-line violations". Feature #484 records the same treatment
for `QfcItemController` partials.

Current line count of every file 464 would write:

| File | Lines | Ceiling status |
|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | **1084** | Pre-existing violation. Net change expected **negative** (deleting five `throw;` statements; no member added if the `async void` bodies are extracted in place). |
| `QuickFiler/Controllers/EfcItemController.cs` | **1170** | Pre-existing violation. Net change expected **strongly negative**: RC11-B removes `InitializeWebView()` (32 lines) and `RegisterActions` (13); #461 removes the handler (15) and the subscription (4); §Q7.8's optional removals would add ~60 more. |
| `QuickFiler/Viewers/EfcViewer.cs` | **162** | **Safe.** Adding the `ClaimsAltChord` predicate (~8 lines) and removing `SetController` (4) plus `EditFiltersMenuItem_Click` (4) and `_formController` (1) leaves it near 160. |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | **430** | Unchanged length — one-character-class edit on `:55`. 484's own change adds fields; 464 adds none. |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | **168** | Safe with 332 lines of headroom; **assert < 500 in the AC**. |
| `QuickFiler.Test/Controllers/EfcItemControllerTests.cs` | **new** | **AT RISK.** Six sub-defects with Moq arrange blocks. **Recommendation: plan for two files from the outset**, e.g. `EfcItemControllerTests.cs` (#459, #461, #464 D/E) and `EfcItemController.CleanupTests.cs` (#460 A/B/C). |
| `QuickFiler.Test/Controllers/EfcViewerTests.cs` | **new** | Safe — the `QfcFormKeyHandlerTests.cs` precedent is 67 lines for four methods. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 1,000+ (project file) | Not subject to the ceiling. |

**No acceptance criterion may assert a line count under 500 for `EfcFormController.cs` or
`EfcItemController.cs`.** Every new test file must carry one.

---

## Q10 — `QuickFiler.Test.csproj` insertion region

The `<Compile Include>` item group is **not** globally alphabetical — 484's spec is correct on this
point (`484/spec.md:563-567`: "ordered by area and by insertion history, not alphabetically"). There is
nevertheless a contiguous, locally alphabetical `Efc*` cluster. The exact region, verbatim:

```xml
<!-- QuickFiler.Test/QuickFiler.Test.csproj -->
103    <Compile Include="Controllers\QfcQueueCoverageExpansionTests.cs" />
104    <Compile Include="Controllers\QfcQueuePurePathsTests.cs" />
105    <Compile Include="Controllers\EfcDataModelTests.cs" />
106    <Compile Include="Controllers\EfcFormControllerTests.cs" />
107    <Compile Include="Controllers\EfcHomeControllerDependenciesTests.cs" />
108    <Compile Include="Controllers\EfcHomeControllerDependenciesProductionFactoryTests.cs" />
109    <Compile Include="Controllers\EfcHomeControllerExecuteMovesTests.cs" />
110    <Compile Include="Controllers\EfcHomeControllerLifecycleTests.cs" />
111    <Compile Include="Controllers\EfcHomeControllerMetricsTests.cs" />
112    <Compile Include="Controllers\EfcHomeControllerTests.cs" />
113    <Compile Include="Controllers\EmailSorterTests.cs" />
114    <Compile Include="Controllers\BayesianPerformanceControllerTests.cs" />
115    <Compile Include="Controllers\BayesianPerformanceController.TestSupport.cs" />
116    <Compile Include="Controllers\EfcHomeControllerSeamTests.cs" />
```

The cluster is `:105-112`. It is alphabetical within itself except that `:108` precedes `:107`
alphabetically, and `:116` is a later out-of-order append. `EmailSorterTests.cs` at `:113` immediately
follows the cluster.

**Recommended insertion: immediately after line 112, before `EmailSorterTests.cs`.** New entries in
alphabetical order — `EfcItemControllerTests.cs`, then any `EfcItemController.*Tests.cs` split, then
`EfcViewerTests.cs` — so the cluster reads `EfcData… < EfcForm… < EfcHome… < EfcItem… < EfcViewer…` and
the whole diff is a contiguous insertion at one position.

**Contention check.** Feature #484 states it makes **no** `QuickFiler.Test.csproj` edit
(`484/spec.md:221`, `:560-567`). Feature #444 adds **one** line for one new test file
(`444/spec.md:321`). Feature #476 adds one line
(`webview2-host-initializer-defects-476/spec.md:1023`). Feature #442 lists the file among those it may
write (`442/spec.md:163`). The item group is a well-known shared append point; inserting at `:112`
rather than at the end of the group reduces, but does not eliminate, the chance of a textual conflict.

---

## Q11 — sibling-owned files and additional collisions

### Q11.1 Confirmed ownership

| Path | Owner | Evidence |
|---|---|---|
| `QuickFiler/Controllers/EfcHomeController.cs`, `EfcHomeController.ExecuteMoves.cs` | **#442** | `442/spec.md:384`, `:308`, `:315` |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` and the breadcrumb router surface | **#498** | `498/spec.md:153` reciprocally lists `EfcFormController.cs` as "feature 464" |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator*.cs`, `BreadcrumbMessengerHub.cs` | **#501** | `501/spec.md:132`, `:604` |
| `QuickFiler/Controllers/QfcItemController.*.cs` (all ten partials) | **#484**, with `Navigation.cs` to **#444** | `484/spec.md:183-188`, `:205-207`; `444/spec.md:329-344` |
| `QuickFiler/Controllers/KbdActions.cs` | **#444** | `444/spec.md:317`; `484/spec.md:207` |
| `QuickFiler/Controllers/KeyboardHandler.cs` | **#498** | `444/spec.md:331` |
| `QuickFiler/Interfaces/IQfcCollectionController.cs`, `QfcCollectionController.cs` | **#468** | `444/spec.md:332`; `468` freezes `QfcCollectionControllerTests.cs` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | **#468** (`[TestMethod]` count frozen) | `444/spec.md:410-412`, `:620-625` |
| `QuickFiler/Viewers/ItemViewer*.cs`, `IItemViewer.cs` | **#489** | `484/spec.md:206`, `:270-277` |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, `WebView2CoreInitializer.cs` | **#476** | `476/spec.md:954`, `:983` |

Reciprocal confirmation that 464 owns the EFC controllers: `484/spec.md:213-214`
("`QuickFiler/Controllers/EfcItemController.cs`, `QuickFiler/Controllers/EfcFormController.cs` — owned
by feature 464"), `498/spec.md:153`, `476/spec.md:223` and `:610`, `442/spec.md:162`. **The partition is
mutually consistent for the EFC controllers.**

### Q11.2 Additional collisions found

1. **`QuickFiler/QuickFiler.csproj` — feature #501 adds one line after `:392`**
   (`501/spec.md:604`). §Q7.6 shows 464 needs **no** csproj edit, so this collision disappears if the
   spec declares the file untouched. **Act on this: state it explicitly.**
2. **`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — 464 and #484 both write it.** Sequential,
   not concurrent (464 branches from an integration branch already carrying 484). Disjoint line ranges
   (§Q6.3). Constrain 464's diff to the single line `:55`.
3. **`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` — read-only reference by 464 for RC7.**
   `BannerPrefix` is `public const string BannerPrefix = "===="` at `BreadcrumbRowBuilder.cs:19`.
   Feature #498's acceptance criteria assert that `BreadcrumbRowBuilder.cs` is **not modified**
   (`498/spec.md:779`), so a read-only reference is safe. **464 must not edit it.**
4. **A fourth, unowned copy of the banner constant.**
   `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16` declares
   `private const string BannerPrefix = "====";`, used at `:197`. This is a fifth classification site
   for RC7 and is **outside 464's owned set**. Record as a downstream note; do not consolidate it.
5. **#498's `EfcFormController.cs` citations are stale by +2 and are internally inconsistent.**
   `498/spec.md` cites `BindFolderRows` at `:873-883` (tree: `:871-881`), `ConfigureBreadcrumbControl` at
   `:834-854` (tree: `:832-852`), the router construction at `:843-849` (tree: `:841-847`),
   `IsValidSelection` at `:1039-1050` and its null disjunct at `:1046` (tree: `:1038-1050`, null disjunct
   `:1044`), the two `!IsValidSelection` guards at `:470`/`:754` (tree: `:468`/`:752`), the file length as
   1086 (tree: **1084**), and `EfcFormController.cs:493-494`, `:772-773`. **464 must not consume any of
   these numbers.** Report to the epic; #498 is already in re-scope.
6. **#476's `EfcFormController.cs` citations are stale by the same -2.** `476/spec.md:158`, `:296`,
   `:321`, `:351`, `:930`, `:954` cite `EfcFormController.cs:836-839` for the
   `new WebView2BreadcrumbHost(...)` construction (tree: **`:834-837`**) and `:451-452`, `:704-705` for
   the null-`SynchronizationContext` checks (tree: **`:449-450`**, **`:702-703`**). 476 does not write
   the file, so this is informational — but 476's acceptance criterion "`EfcFormController.cs:836-839`
   requires no edit" (`476/spec.md:954`) names a line range that no longer holds that code.
   **Cross-feature note: 464 must not move or reshape `EfcFormController.cs:834-837`**, because 476's
   design depends on that call site compiling unchanged.
7. **#444's `EfcFormController.cs` citations are stale** (§Q1.3). Informational only.

---

## RC1-RC11 grouping — validation and recommended revision

`issue.md`'s eleven-group model is **sound in its main lines** and is confirmed by source in eight of
eleven groups. Five changes are recommended, each justified below.

### Confirmed as written

| RC | Verdict | Confirming evidence |
|---|---|---|
| **RC1** — no post-teardown null-state contract, shared by #460, #464, #465 | **CONFIRMED**, and the shared cause is real. All four sub-defects fail on state that `Cleanup` produces, and three of the four have a single twin remedy shape. | `EfcFormController.cs:187-194`, `:255`, `:267`, `:272-283`; `EfcItemController.cs:255-278`, `:395`, `:407`, `:439-450`, `:610-613`; twins at `QfcFormController.cs:100-155`, `QfcFormController.SetupDisposal.cs:208-228` |
| **RC2** — dereference-instead-of-dispose | **CONFIRMED** and distinct from RC1. The field *is* correctly nulled; the resource is not released. | `EfcItemController.cs:277` vs `:875-876`, `:953-954`, `:898-901`, `:938-941` |
| **RC5** — non-ASCII in a machine-parsed literal | **CONFIRMED**, with a correction: only two of the three sites are worth fixing (§Q6.2). | `EfcItemController.cs:184`, `:217`; `QfcItemController.ViewerSetup.cs:55` |
| **RC6** — `nameof` bound to an unraised name | **CONFIRMED**, with a changed remedy: removal, not renaming (§Q4.3). | `EfcItemController.cs:746`; `ConversationResolver.Loading.cs:26/33/167/174/205/227`; `ConversationResolver.cs:277` |
| **RC7** — duplicated magic constant with divergent arity | **CONFIRMED**, and wider: a **fifth** site exists at `FolderSuggestionTree.cs:16`. | `EfcFormController.cs:706`, `:1047`; `BreadcrumbRowBuilder.cs:19`; `BreadcrumbStateModel.cs:249`; `FolderSuggestionTree.cs:16` |
| **RC8** — illegal cross-thread control read | **CONFIRMED**. | `EfcFormController.cs:799` inside the `Task.Run` at `:798-801`, vs the correct UI-thread read at `:556` |
| **RC9** — read-modify-write through a rebind that writes back | **CONFIRMED**. | `EfcFormController.cs:745-747` and the write-back at `:879` |
| **RC10** — input-routing over-claim | **CONFIRMED**, with the addition that the QFC twin shares the defect and supplies only the *testability* pattern (§Q5.3). | `EfcViewer.cs:96`; `QfcFormViewer.cs:60`; `QfcFormKeyHandler.cs:18` |

### Recommended revisions

**R1 — RC1 must add the two item-side theme accessors it currently omits.** `issue.md`'s RC1 names
`EfcItemController.DarkMode` (`:439`) but not `EfcItemController.ActiveTheme` (`:395`, `strict: true`
with `_themes`) or `EfcItemController.LoadTheme` (`:404-409`, unguarded `_themes[activeTheme]` at
`:407`). These are the same cause on the same file, and RC1's own justification ("fixing #464 A without
fixing #460 A/C and #465 A would leave the same class of defect live on adjacent members") applies with
full force. **Add them.**

**R2 — RC3-D shrinks from six citations to two, and RC3 gains a correction.** `issue.md` lists
`EfcItemController.cs:704`, `:711`, `:716`, `:741`, `:882`, `:887` as `async void` lambdas "registered
into `CharActions`, which is `KbdActions<char, KaChar, Action<char>>`". Verified against source:

- `:704`, `:711`, `:716` register into **`CharActionsAsync`**, whose delegate type is
  `Func<char, Task>` (`IQfcKeyboardHandler.cs:22`). An `async (x) => await …` bound to
  `Func<char, Task>` is an **async `Task`** lambda, **not** `async void`. Furthermore its fault **is**
  observed: `KeyboardHandler.cs:176` awaits it, inside `KeyDownTaskAsync` (`:150`), awaited inside the
  `try` at `KeyboardHandler.cs:139` with `catch` + `logger.Error` at `:141-147`. **These three are not
  defects. Remove them from RC3-D.**
- `:699` is `(x) => _ = _explorerController.OpenQFItem(_itemInfo.Item)`, also a `Func<char, Task>`
  registration. Not `async void`.
- `:741` is `public async void ConversationResolverPropertyChanged` — a genuine `async void`, but it is
  an `INotifyPropertyChanged` **event handler**, not a keyboard-action lambda. It belongs to RC6, and
  RC6's recommended remedy (delete it) closes it.
- `:882` and `:887` register into **`CharActions`** (`Action<char>`, `IQfcKeyboardHandler.cs:21`) via
  `CharActions.Add` at `:879` and `:884`. These **are** `async void`, and the sync reader
  (`KeyboardHandler.cs:114-131`) has **no** try/catch and invokes via `DynamicInvoke` at `:128`. **These
  two are the whole of RC3-D.**

**R3 — RC3-D, RC4-B and RC4-C should be re-grouped under RC11 as latent dead code, or the spec must
justify repairing dead code.** §Q7.8 establishes that `EfcItemController.ToggleExpansion(ToggleState)`
(`:862-905`) — the sole writer of the `'B'`/`'D'` `CharActions` entries and the sole home of the two
`async void` lambdas — has zero reachable call sites. §Q7.6 establishes that `CharActions` itself has no
compiled reader. So:

- **RC4-B** ("`ToggleExpansionOn`/`Off` do not mirror the sync path") describes an asymmetry between a
  **dead** member and a **live** one.
- **RC4-C** ("a sync-On / async-Off / sync-On sequence throws") requires entering the dead member.
- **RC3-D** requires the dead member's lambdas to be invoked through an unread registry.

Two coherent dispositions, and the spec must pick one:

- **(a) Delete the sync overloads.** `ToggleExpansion()` (`:838-848`) and `ToggleExpansion(ToggleState)`
  (`:862-905`) are removed under RC11. RC4-B, RC4-C and RC3-D are then closed by removal. `issue.md`'s
  "shared edit site" observation dissolves — there is no edit site. This removes ~70 lines from a
  1170-line file and is consistent with RC11-B's own logic. **Recommended.**
- **(b) Repair in place.** Introduce a single `SyncExpandedRegistrations(bool expanded)` owner called by
  both `ToggleExpansion(ToggleState)` and `ToggleExpansionAsync(ToggleState)` after the flag is written,
  mirroring feature 444's #482 fix for `QfcItemController`
  (`444/spec.md:630-656`). This preserves the shared-edit-site sequencing constraint `issue.md`
  identifies, produces an exact symmetry with the upstream sibling, and yields a directly testable
  private member (§Q8.4). It repairs code no user can reach.

If the spec chooses (b), the `async void` lambdas at `:882`/`:887` must still be corrected under RC3-D,
because the single owner would register into both registries.

**R4 — RC11-A's stated impact is wrong and its remedy narrows.** #466 A and `issue.md`'s RC11-A both
assert that the Edit Filters command is non-functional. It is functional:
`EfcFormController.WireEventHandlers` performs
`_formViewer.EditFiltersMenuItem.Click += EditFiltersMenuItem_Click;` at **`EfcFormController.cs:398`**,
targeting `EfcFormController.EditFiltersMenuItem_Click` (`:559-564`), which constructs `ManageFilters`,
calls `LoadFilters(_globals)`, and `Show()`s it. The controller subscribes directly to the Designer
control, bypassing the viewer entirely.

Therefore the dead surface is **only**: `EfcViewer.SetController` (`:50-53`), `EfcViewer._formController`
(`:48`), and `EfcViewer.EditFiltersMenuItem_Click` (`:157-160`). Deleting all three is **behaviour-preserving**
and removes the trap. `issue.md:238-240` already forbids "adding new Edit Filters functionality"; this
finding shows no functionality is at risk. **Remedy: delete. The "wire it up" alternative is moot.**

**R5 — the severity profile of #459 and #460 drops to latent, and this must be stated.** With
`EfcItemController.Cleanup()` unreachable (§Q7.4) and the sync expansion path unreachable (§Q7.8),
**four** of the eight issues are wholly or partly latent: #459 (all three sub-defects), #460 A and B,
#464 D, and #466 (latent by definition). Only #461, #463, #464 A/B/C/E, #465 and #467 are live. This
does not reduce scope — every one is a real defect on a public or internal member, and CLAUDE.md's
Bugfix Workflow requires a failing regression test for each regardless — but the spec's severity table
and its "user-visible impact" statements must be truthful about it, exactly as feature 444 corrected
#482's severity (`444/spec.md:56-64`).

### Recommended grouping, restated

| RC | Cause | Issues / sub-defects | Change vs `issue.md` |
|---|---|---|---|
| RC1 | No post-teardown null-state contract | #460 A, #460 C, #464 A (**+ item `ActiveTheme` `:395` and `LoadTheme` `:407`**), #465 A | **R1** |
| RC2 | Dereference-instead-of-dispose | #460 B | unchanged |
| RC3 | Fault escapes an unlogged boundary | #464 B (`:425`, `:441`, `:457`, `:517`, `:530`), #464 C (`:95`, `:115`, `:1022-1036`), #464 E (`:777`) | **R2** — RC3-D removed from this group |
| RC4 | `KbdActions<>` contract misuse | #459 A (**moot on removal**), #459 B, #459 C | **R3** |
| RC5 | Non-ASCII in a machine-parsed literal | #463 (**two live sites, not three**) | §Q6.2 |
| RC6 | `nameof` bound to an unraised name | #461 (**remedy: removal**) | §Q4.3 |
| RC7 | Duplicated magic constant, divergent arity | #465 D | + a fifth site, out of scope |
| RC8 | Illegal cross-thread control read | #465 B | unchanged |
| RC9 | Read-modify-write through a write-back rebind | #465 C | write-back is `:879`, not `:871` |
| RC10 | Input-routing over-claim | #467 | + the twin shares the defect |
| RC11 | Dead code carrying a latent trap | #466 A (**narrowed, R4**), #466 B, #466 C, #466 D (**three files, no csproj edit**), **+ `Cleanup()` itself, `ToggleExpansion` ×2, `ToggleNavigation(bool)`** | **R3, R4, §Q7.8** |

---

## Was feature 484's upstream-contract table sufficient?

**Mostly yes, with one gap and one error.**

**Sufficient for.** The ADDED-members table (`484/spec.md:335-347`), the CHANGED-members table
(`:353-363`), the "no member is removed, no public member is added, no interface is modified" guarantee
(`:365-372`), the event-wiring order facts (`:374-383`), the three `Cleanup()` statement-order
constraints (`:385-398`), and the post-`Cleanup()` lifecycle invariant (`:400-408`) were consumed as
written. I did **not** re-derive the member list, the wiring/detach ordering, or the detach count from
source, as instructed. The deterministic-timer technique (`:634-650`) transfers to RC2 with no
adaptation.

**Gap — the `--incognito` literal.** The table describes 484's change to
`InitializeWebViewAsync()` (`:363`) but says nothing about `ViewerSetup.cs:55`. I had to go to source to
establish that line 55 lies outside 484's edit region. That was necessary and the answer is favourable
(§Q6.3), but the table would be strengthened by naming the line, since 464 was told to author against
the table without re-deriving.

**Error — the `ToggleNavigation(bool)` retention rationale.** `484/spec.md:369-370` and `:226-227` state
that `EfcItemController.cs:958` implements `IQfcItemController.cs:89`. `EfcItemController` implements
only `IItemControler` (`EfcItemController.cs:26`), which declares three members and not that one
(`IItemControler.cs:9-14`). The retention decision is probably still right; the reason is not. §Q7.9.

**Corroborations.** Every `EfcItemController.cs` citation in 484's downstream notes was re-read and is
**current**: `:277` (timer), `:257-262` (partial unwire and the delegate-identity precedent),
`:953-954` (timer arming), `:958-979` / `:962-967` / `:981-994` / `:996` (`ToggleNavigation` shapes).
484's EFC-side work is well-grounded.

---

## Disagreements found, consolidated

| # | Source | Claim | Tree |
|---|---|---|---|
| D1 | `issue.md` RC3-D | `:704`, `:711`, `:716` are `async void` lambdas in `CharActions` | They are `Func<char, Task>` lambdas in `CharActionsAsync` (`IQfcKeyboardHandler.cs:22`), awaited with a logged boundary at `KeyboardHandler.cs:139-147`, `:176` |
| D2 | `issue.md` RC11-B + `#466` | "the Edit Filters command is silently non-functional" | Wired and functional at `EfcFormController.cs:398` → `:559-564` |
| D3 | `issue.md` citation table | `BindFolderRows` write-back at `:871` | `:871` is the signature; the write-back is `:879` |
| D4 | `issue.md` RC1 | `EfcFormController.DarkMode` at `:272-282` | Property block `:272-283`; getter `:274-281`; eager arg `:280` |
| D5 | `issue.md` RC1 | RC1 omits `EfcItemController.ActiveTheme` (`:395`) and `LoadTheme` (`:407`) | Same defect, same file |
| D6 | `#460` promoted | Repro reaches `Cleanup()` "through the constructor" | `EfcItemController.Cleanup()` has zero call sites; the defect is latent |
| D7 | `#459` promoted | Repro "expand through the sync path" | `ToggleExpansion(ToggleState)` has zero reachable call sites |
| D8 | `#465` promoted | "the second `ActionOkAsync` throws `NullReferenceException` at `:705`" | `:705` reads `SelectedFolder` → `_router?.SelectedFolderPath`; `_router` is **not** nulled by `Cleanup`, so it does not throw. The first post-`Cleanup` throw is `:703` (`_formViewer.UiSyncContext`, conditional) or `:713` (`_formViewer.Hide()`, unconditional) |
| D9 | `484/spec.md:369-370`, `:226-227` | `EfcItemController.cs:958` implements `IQfcItemController.cs:89` | `EfcItemController : IItemControler` only (`:26`); `IItemControler` declares three members (`IItemControler.cs:9-14`) |
| D10 | `444/spec.md:234` (table) | `CharActions` is "reached only from the Alt-key `ProcessCmdKey` path" | Its only reader `KeyboardHandler.cs:114-131` has no compiled caller: `QfcFormViewerDark.cs:48` and `QfcFormViewerExpanded.cs:48` have no `<Compile Include>`, and `QfcFormViewer.cs:68` calls `ToggleKeyboardDialogAsync()` instead |
| D11 | `444/spec.md:146`, `:544`, `:560` | `EfcFormController.cs:358-367`, `:574-602`, `:631-676`, `:365` | `:354-366`, `:570-601`, `:627-675`, `:363` |
| D12 | `498/spec.md` (many) | `EfcFormController.cs` is 1086 lines; `BindFolderRows` `:873-883`; `IsValidSelection` `:1039-1050`; guards `:470`/`:754` | 1084; `:871-881`; `:1038-1050`; `:468`/`:752` |
| D13 | `476/spec.md:158`, `:954` | `EfcFormController.cs:836-839`, `:451-452`, `:704-705` | `:834-837`, `:449-450`, `:702-703` |
| D14 | `#466` promoted | "`EfcViewer3.cs` and its siblings" are the orphans | Twenty `QuickFiler/Viewers/*.cs` files have no `<Compile Include>`; `Legacy/**` and `Notes/**` are wholly uncompiled |
| D15 | `484/spec.md:571`; `.claude/rules/general-unit-test.md` | 80% floor vs 85%/75% | CLAUDE.md (authoritative under `policy-compliance-order`) states 80% repo-wide and 90% for new modules. Report the discrepancy; do not resolve it in this feature. |

---

## Unverified items

| Item | Why it could not be verified |
|---|---|
| Chromium's runtime treatment of an unrecognised `AdditionalBrowserArguments` token | Requires running WebView2. Bash and all execution tools are disabled for this session; the repository-policy prohibition on live-host tests applies regardless. |
| Behaviour of `await (SynchronizationContext)null` under the repository's `SynchronizationContext` awaiter extension | I did not locate and read the awaiter implementation. §Q8.5 flags this as a plan-time verification. |
| Whether any existing `QuickFiler.Test` test reads a production `.cs` file from disk | Not found in the files I read. §Q8.6 proposes the `internal const` alternative that avoids the question. |
| Whether `EfcViewer.ProcessCmdKey`'s `base.ProcessCmdKey` is safe to invoke on a `CreateUninitialized<EfcViewer>()` instance | Requires execution. §Q5.4's recommended remedy (a pure static predicate) makes the question unnecessary. |
| Line counts of `EfcDataModelTests.cs`, the six `EfcHomeController*Tests.cs`, and the `QfcItemController.*Tests.cs` files | Not read; none is a file this feature would write. |
| Exact `git log` history of `EfcFormController.cs` since `988e819b` | No git tooling available this session. The -2/-4 drift is established by direct comparison of cited line numbers against the tree, not by reading the diff. |

---

## Recommended plan shape (informational; the spec is authoritative)

Sequencing is driven by two constraints found here: RC11 removals should precede the fixes so that no
phase edits a member a later phase deletes, and the `ViewerSetup.cs:55` edit should be isolated.

1. **Phase A — RC11 removals** (`EfcItemController.cs`, `EfcViewer.cs`): `InitializeWebView()` with its
   `:184` literal; `RegisterActions`; the 7-argument constructor; `_selectorsCtrls` (assign or stop
   passing); `EfcViewer.SetController` + `_formController` + `EfcViewer.EditFiltersMenuItem_Click`;
   the three `EfcViewer3.*` files; the `ConversationResolverPropertyChanged` handler and its
   subscription (RC6); and — subject to the R3 decision — the two sync `ToggleExpansion` overloads.
   Structural regression tests first (§Q8.7).
2. **Phase B — RC5**: the single-line `QfcItemController.ViewerSetup.cs:55` edit plus
   `EfcItemController.cs:217`, hoisted into an `internal const` per §Q8.6.
3. **Phase C — RC1** across both controllers, mirroring `QfcFormController.cs:100-155` and
   `QfcFormController.SetupDisposal.cs:208-228`, including the R1 additions. Plus **RC2** using 484's
   T1/T2.
4. **Phase D — RC3** boundary extraction in `EfcFormController.cs` (five handlers, `PopulateFolderCombobox`)
   and `EfcItemController.cs` (`:777`), each extracted to an awaitable member so it is testable (§Q8.5).
5. **Phase E — RC7, RC8, RC9** in `EfcFormController.cs`, each with a pure extracted helper so the
   `_formViewer` blocker does not apply.
6. **Phase F — RC10** in `EfcViewer.cs`, mirroring the `QfcFormKeyHandler` seam pattern.
7. **Phase G — RC4** only if R3 selects disposition (b).
