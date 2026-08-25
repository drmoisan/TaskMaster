# quickfiler-keyboard-action-defects (Spec)

- **Issue:** #444 (also closes #472 and #482)
- **Parent epic:** `quickfiler-bug-family` (integration branch `epic/quickfiler-bug-family-integration`)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-24
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** `full-bug`

> **Acceptance-criteria authority.** Work mode is `full-bug`. Under
> `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `spec.md` is the **sole** acceptance-criteria
> source for this work mode. `user-story.md` is intentionally absent and must be reported as `NONE`.
> The `## Acceptance Criteria` section of this document is the binding checklist.

> **Citation basis.** Every `file:line` citation in this document is against base commit `988e819b`
> unless the text states otherwise, and was established by direct file read of the working tree at
> that commit. Claims that could not be established by direct read are marked `UNVERIFIED` together
> with the reason. Citations into `QuickFiler/Controllers/QfcCollectionController.cs` **will shift**
> once upstream issue #468's deletions land — see *Line-number volatility* under
> `## Assumptions, Constraints, Dependencies`. No post-#468 line number appears anywhere in this
> document.

> **Path placeholders.** `<repo-root>` is the workspace root. Absolute host paths, account names, and
> machine names are deliberately excluded.

---

## Context

Three keyboard-action defects in the QuickFiler surface share one root-cause family: the
`KbdActions<TKey, UClass, VDelegate>` registry (`QuickFiler/Controllers/KbdActions.cs:14-145`) admits
inconsistent `(SourceId, Key)` state through entry points that do not agree with one another, and
every production call site discards `Remove`'s `bool` result
(`QuickFiler/Controllers/KbdActions.cs:123-135`), so a divergence stays silent until a later `Add` or
`Find` throws.

| Issue | One-line statement | Reachability at `988e819b` |
| --- | --- | --- |
| **#444** | The `KbdActions(IEnumerable<UClass>)` constructor bypasses the duplicate guard that both `Add` overloads enforce. | Latent (dormant). The only duplicate-bearing construction site is inside a method with zero callers. |
| **#472** | `RegisterNavigation` captures the side-effecting `Digits` property once; `UnregisterNavigation` re-evaluates it per loop iteration, so a 10-item boundary crossing between the two calls unregisters under a different digit width than it registered. | Live. |
| **#482** | The synchronous and asynchronous expansion paths maintain disjoint `'B'`/`'D'` registries behind a single shared `_expanded` flag, so interleaving the two paths drives flag and registries out of agreement and the next registration throws. | Live, but by a different keystroke sequence than the promoted document states. |

**Observed environment.** Logic defects in first-party C# (`QuickFiler`), reproducible wherever the
add-in runs. No environment-specific behaviour was identified.

**User-visible impact and severity.**

- **#444** — none today. The exception path exists but is unreachable, because the only method that
  seeds a duplicate has zero callers (see `## Repro & Evidence`).
- **#472** — orphaned navigation-key registrations survive an unregister, and a later re-register of
  the same key throws `ArgumentException` from `KbdActions.Add`
  (`QuickFiler/Controllers/KbdActions.cs:92-98`). The user-visible symptom is a delayed,
  hard-to-attribute failure of number-key item selection after items are filed. Severity: **Medium**,
  as filed.
- **#482** — **the promoted document's severity wording is incorrect and is corrected here.** The
  promoted document states "Medium-High. Reachable unhandled `ArgumentException` from ordinary user
  interaction with expansion". The `ArgumentException` is in fact **caught and logged** at
  `QuickFiler/Controllers/KeyboardHandler.cs:141-147`, inside the `async void`
  `KeyboardHandler_KeyDownAsync` boundary (`:133-148`). The observable symptom is therefore a
  **silently dead `B`/`D` key plus a log entry**, not a crash dialog and not an unhandled exception.
  The defect is real and the user-facing consequence (expansion sub-navigation stops working until the
  item is re-created) is real; only the "unhandled" characterisation is wrong. Severity is restated
  as **Medium**.

**First observed.** All three were captured by static analysis on 2026-08-07 during preparation
research for epic #136. `UNVERIFIED`: no runtime observation of the shipped add-in was performed for
any of the three, in the originating research or in this feature's preparation. Every reproduction
argument below is a source-grounded control-flow argument.

---

## Repro & Evidence

### #444 — `KbdActions` enumerable constructor bypasses the duplicate guard

**Determinism:** deterministic given the code shape; **not reachable at runtime**.

Both `Add` overloads enforce a `(SourceId, Key)` duplicate guard and throw `ArgumentException`:

- `Add(string sourceId, TKey key, VDelegate @delegate)` — guard at
  `QuickFiler/Controllers/KbdActions.cs:92-98`, message literal
  `"Cannot add key because it already exists. Key {key} SourceId {sourceId}"` at `:94-95`,
  `logger.Error` at `:96`, `throw` at `:97`.
- `Add(UClass instance)` — guard at `QuickFiler/Controllers/KbdActions.cs:108-119`, `logger.Error` at
  `:117`, `throw new ArgumentException(message, nameof(instance))` at `:118`.

The enumerable constructor has no equivalent check:

```csharp
// QuickFiler/Controllers/KbdActions.cs:26-29
public KbdActions(IEnumerable<UClass> list)
{
    _list = new List<UClass>(list);
}
```

The promoted document cites this constructor at `:25-28`; the actual span at `988e819b` is `:26-29`
(a one-line drift).

Production seeds a duplicate `("Collection", Keys.Down)` pair through that constructor at
`QuickFiler/Controllers/QfcCollectionController.cs:1265-1272`, with `Keys.Down` bound to
`SelectNextItem()` at `:1269` and again to `_parent.ActionOkAsync()` at `:1270`. A subsequent
`Find(Keys.Down)` would resolve against a two-element match set and throw `InvalidOperationException`
(`QuickFiler/Controllers/KbdActions.cs:63-67`; `FindIndex` mirrors it at `:81-86`). The consuming
indexer path is `QuickFiler/Controllers/KeyboardHandler.cs:118-123`.

**Established fact — the site is dead.** The enclosing method `WireUpKeyboardHandler`
(`QuickFiler/Controllers/QfcCollectionController.cs:1254-1273`) has **exactly zero callers**. A
repository-wide search of `*.cs` for the identifier `WireUpKeyboardHandler` returns exactly one hit —
its own declaration at `QuickFiler/Controllers/QfcCollectionController.cs:1254`. The duplicate pair at
`:1265-1272` therefore cannot execute. The live equivalent is `WireUpAsyncKeyboardHandler`
(`:1275-1280`), which calls `RegisterNavigation()` (`:1277`), `RegisterAsyncKeyActions()` (`:1278`),
and `RegisterAlwaysOnAsyncKeyActions()` (`:1279`).

**Established fact — upstream #468 deletes the site before this feature starts.** #468's committed
plan task `[P1-T2]` names `WireUpKeyboardHandler` among twelve dead members it deletes from
`QfcCollectionController.cs`. #468's decision `D2` states verbatim:

> **D2 — `KbdActions.cs` is never written.** Removing `WireUpKeyboardHandler` deletes a *caller*; it
> deletes zero lines in `QuickFiler/Controllers/KbdActions.cs`. Any further hardening of
> `KbdActions(IEnumerable<UClass>)` is recorded as a downstream note for #444 and is out of scope.

#468's `[P14-T2]` writes a downstream handoff artifact to #444 recording that #468 "resolves the
duplicate keyboard-action registration as a side effect".

**Consequence for this feature — stated explicitly.** Against the post-#468 shape this feature
branches from, **the duplicate registration site does not exist**. Two of the four promoted #444
acceptance criteria are therefore satisfied upstream by #468 and are **not work items here**:

| Promoted #444 criterion | Disposition in this feature |
| --- | --- |
| "The intended `Keys.Down` behavior for the QuickFiler collection surface is decided and recorded." | **Recorded here** (see `## Proposed Fix`, *#444 product decision*). The code site whose ambiguity prompted the question is deleted by #468 `[P1-T2]`; recording the decision and pinning the surviving live registration by test is the only durable form it can take. |
| "The duplicate registration in `QfcCollectionController.cs` is resolved to a single entry." | **Satisfied upstream by #468 `[P1-T2]` / `D2`.** Inherited and verified, not re-performed. The executor verifies the site is **absent**; it must not recreate the block in order to remove it. |
| "`KbdActions(IEnumerable<UClass>)` either enforces the same duplicate guard as `Add` or documents in-code why it deliberately does not." | **This feature's work.** |
| "A regression test covers the chosen behavior, including the duplicate-input case." | **This feature's work.** |

**The promoted document's coupling is dissolved.** The promoted document required that the
constructor guard "must land together with decision 1", because at filing time the guard would have
converted a live call site into a construction-time throw. A full inventory of `KbdActions`
construction sites establishes that **exactly one** site in the repository contains a duplicate
`(SourceId, Key)` pair — `QfcCollectionController.cs:1265-1272` — and #468 deletes it. Post-#468,
**zero** surviving construction sites would throw under the guard, so the guard is a
zero-call-site-impact change and the coupling no longer exists. Notable non-duplicating sites, for
completeness: `QfcCollectionController.cs:1284-1290` (`Keys.Up`, `Keys.Down` — distinct),
`:1295-1304` (single entry), `EfcFormController.cs:358-367`, `:574-602`, `:631-676` (all distinct
keys). The `= []` collection-expression initializers in `KeyboardHandler.cs` do not invoke the
enumerable constructor: an empty collection expression on a type implementing `IEnumerable<T>` with an
applicable `Add` lowers to the parameterless constructor plus zero `Add` calls.

### #472 — navigation register/unregister digit-width desync

**Determinism:** deterministic given a 10-item boundary crossing between the two calls.

The asymmetry is present verbatim:

```csharp
// QuickFiler/Controllers/QfcCollectionController.cs:1330-1341
public void RegisterNavigation()
{
    var digits = Digits;                       // :1332 — captured ONCE
    if (_digitRefreshNeeded) { SetVisualDigits(digits); }
    for (int i = 0; i < _itemGroups.Count; i++)
    { RegisterNavigationAsyncAction(i, digits); }
}

// QuickFiler/Controllers/QfcCollectionController.cs:1343-1356
public void UnregisterNavigation()
{
    for (int i = 0; i < _itemGroups.Count; i++)
    {
        if (Digits == 1)                        // :1347 — re-evaluated PER ITERATION
        { _kbdHandler.StringActionsAsync.Remove("Collection", (i + 1).ToString()); }
        else
        { _kbdHandler.StringActionsAsync.Remove("Collection", (i + 1).ToString("00")); }
    }
}
```

`Digits` (`QuickFiler/Controllers/QfcCollectionController.cs:114-128`) reads `_itemGroups?.Count >= 10 ? 2 : 1`
live at `:119`, and is **side-effecting**: it sets `_digitRefreshNeeded = true` and mutates `_digits`
inside the getter (`:120-125`). It carries `[MethodImpl(MethodImplOptions.Synchronized)]` at `:116`.

`KbdActions.Remove` returns `false` silently for an absent pair
(`QuickFiler/Controllers/KbdActions.cs:123-135`, `return false` at `:128`), so the mismatch surfaces
nothing at the point of failure. The delayed failure is `KbdActions.Add`'s throw
(`:92-98`), reached from `RegisterNavigationAsyncAction` (`:1358-1361`) via `Add(UClass)`.

Every line range cited by the promoted document for this issue is **exact** at `988e819b`.

**Reachable trigger.** A path that mutates `_itemGroups` without an intervening `UnregisterNavigation`
is required, and one exists: `RemoveSpecificControlGroup(int)`
(`QuickFiler/Controllers/QfcCollectionController.cs:1105-1155`) removes a group at `:1127` and
renumbers at `:1132`, and performs no navigation unregister or register anywhere in its body. It is
reached unbracketed from `RemoveBelowThresholdAsync` (`:1077-1097`) through the
`RemoveGroupByEntryId` seam (`:1069-1074`, invoked in a loop at `:1093-1096`) and
`RemoveSpecificControlGroup(string)` (`:1053-1058`), and from the `'R'` char action registered at
`QuickFiler/Controllers/QfcItemController.EventWiring.cs:197-201`. By contrast `RemovedItemMonitor`
(`:1046-1051`) brackets correctly, and `RemoveSpecificControlGroupAsync` unregisters first at `:1162`.

**Established fact — "capture `Digits` once" does not fix this defect.** Hoisting the read out of
`UnregisterNavigation`'s loop still computes the *current* width. A collection registered at ten items
(`"01".."10"`) that drops to nine before unregistering still evaluates `Digits == 1` and still removes
`"1".."9"`, leaving all ten `"0"`-prefixed registrations orphaned. Hoisting fixes only the narrower
case where the count changes *during* the loop. The fix must record the width **actually used at
registration** and unregister with that recorded width.

**Second, unfiled defect in the same family (recorded, not fixed here).** `UnregisterNavigation`
bounds its loop with the *current* `_itemGroups.Count` (`:1345`), not the count that was registered.
After an unbracketed removal the loop under-iterates and the highest registered key is orphaned
**regardless of digit width**. This is a distinct defect from the filed width mismatch, it is reached
by the same paths, and it is **out of scope** here — see `### Downstream notes` below.

### #482 — expansion registry divergence

**Determinism:** deterministic given the keystroke sequence below.

The two toggle overloads maintain different registries:

```csharp
// QuickFiler/Controllers/QfcItemController.Navigation.cs:174-187 (sync)
if (desiredState == On)  { ToggleExpansionOn();  RegisterExpandedActions();  }   // :179-180
else                     { ToggleExpansionOff(); UnregisterExpandedActions(); }  // :184-185

// QuickFiler/Controllers/QfcItemController.Navigation.cs:192-205 (async)
if (desiredState == On)  { await _uiDispatcher.InvokeAsync(() => ToggleExpansionOn());  RegisterExpandedAsyncActions();  }   // :197-198
else                     { await _uiDispatcher.InvokeAsync(() => ToggleExpansionOff()); UnregisterExpandedAsyncActions(); }  // :202-203
```

The two registries are disjoint instances hanging off `IQfcKeyboardHandler`:

| Registry | Written only by | Read by |
| --- | --- | --- |
| `_kbdHandler.CharActions` (`KbdActions<char, KaChar, Action<char>>`) | `RegisterExpandedActions()` (`QuickFiler/Controllers/QfcItemController.EventWiring.cs:306-318`), `UnregisterExpandedActions()` (`:379-383`) | `KeyboardHandler_KeyDown` (`QuickFiler/Controllers/KeyboardHandler.cs:114-131`), reached only from the Alt-key `ProcessCmdKey` path |
| `_kbdHandler.CharActionsAsync` (`KbdActions<char, KaCharAsync, Func<char, Task>>`) | `RegisterExpandedAsyncActions()` (`QuickFiler/Controllers/QfcItemController.EventWiring.cs:320-332`), `UnregisterExpandedAsyncActions()` (`:385-389`) | `KeyDownTaskAsync` (`QuickFiler/Controllers/KeyboardHandler.cs:170-177`) — the ordinary keystroke path |

The shared state is the single `private bool _expanded` at
`QuickFiler/Controllers/QfcItemController.cs:146`, exposed read-only as `IsExpanded` at `:142-144`,
written at `QuickFiler/Controllers/QfcItemController.Navigation.cs:210` and `:220`.

**The promoted document's stated trigger is unreachable, and this document corrects it.** The
promoted document names `QfcCollectionController.cs:1439` (`itemController.ToggleExpansion()` inside
`ActivateBySelectionAsync`) as "what makes the interleaving reachable in production rather than
theoretical". That call is guarded by `if (blExpanded)` at `:1437`, and both async callers pass a
value that is always `false`:

- `ChangeByIndexAsync` (`:1466-1484`) passes `expanded` from
  `await ToggleOffActiveItemAsync(false)` at `:1479`. `ToggleOffActiveItemAsync` (`:1687-1702`) has
  its expansion branch **commented out** at `:1694-1698`, so it returns `parentBlExpanded` unchanged —
  always `false` here.
- `:1661` passes the literal `false`.

`QfcCollectionController.cs:1439` is therefore currently dead with respect to the interleaving.

**The live trigger is Right → Down → Right.** Fully grounded:

1. Only the **async** focus registration runs in the QuickFiler surface: the synchronous
   `RegisterFocusActions()` / `UnregisterFocusActions()` call sites are commented out at
   `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:45`, `:61`, `:101`, `:117`; the live
   calls are `RegisterFocusAsyncActions()` at `:46`, `:118`, `:150` and
   `UnregisterFocusAsyncActions()` at `:62`, `:102`, `:165`.
2. `Keys.Right` is bound to `ToggleExpansionAsync()` at
   `QuickFiler/Controllers/QfcItemController.EventWiring.cs:221-225`, in `KeyActionsAsync` — the
   registry the live handler reads.
3. `Keys.Down` is bound to `SelectNextItemAsync()` at
   `QuickFiler/Controllers/QfcCollectionController.cs:1288`. `SelectNextItemAsync` (`:1498-1501`)
   marshals to the **synchronous** `SelectNextItem()` (`:1486-1496`) → `ChangeByIndex` (`:1450-1464`)
   → `ToggleOffActiveItem(false)` (`:1459`; body `:1667-1685`), which calls the **synchronous**
   `itemController.ToggleExpansion()` at `:1679` when the item is expanded. `ChangeByIndex` then calls
   `ActivateBySelection(idx + 1, expanded)` at `:1460`, which calls the synchronous
   `ToggleExpansion()` at `:1414`.

| Step | Keystroke | Effect |
| --- | --- | --- |
| 1 | **Right** on item *i* | `ToggleExpansionAsync(On)` → `_expanded = true` (`Navigation.cs:220`); `RegisterExpandedAsyncActions()` adds `'B'`,`'D'` to `CharActionsAsync` (`EventWiring.cs:320-332`). |
| 2 | **Down** | `SelectNextItemAsync` → `SelectNextItem` → `ChangeByIndex` → `ToggleOffActiveItem` sees `IsExpanded == true` and calls the **synchronous** `ToggleExpansion()` → `ToggleExpansionOff()` clears `_expanded` and `UnregisterExpandedActions()` removes from `CharActions`, **where nothing was ever added** — `Remove` returns `false` silently (`KbdActions.cs:126-129`). `CharActionsAsync` still holds `'B'`,`'D'` for item *i*. |
| 3 | **Right** on item *i* again | `_expanded` is now `false`, so `ToggleExpansionAsync(On)` runs → `RegisterExpandedAsyncActions()` → `CharActionsAsync.Add(entryId, 'B', …)` → the entry is already present → **`ArgumentException`** at `KbdActions.cs:97`. |

The exception surfaces through `KeyboardHandler_KeyDownAsync`
(`QuickFiler/Controllers/KeyboardHandler.cs:133-148`), whose `catch` at `:141-147` logs it. The
observable symptom is a **dead `B`/`D` key with a log entry**, not a crash.

---

## Scope & Non-Goals

### In scope

- Add a `(SourceId, Key)` duplicate guard to `KbdActions(IEnumerable<UClass>)`, using
  `StoredKeyEquals` semantics (#444).
- Record the intended `Keys.Down` binding for the QuickFiler collection surface and pin the surviving
  live registration by test (#444).
- Verify — not re-perform — #468's removal of the duplicate `("Collection", Keys.Down)` registration
  site (#444, inherited).
- Record the digit width actually used at registration and unregister with that recorded width (#472).
- Unify expansion registration behind a single owner keyed on the current `_expanded` state, contained
  to `QfcItemController.Navigation.cs` (#482).
- Failing-first regression tests for each of the three defects, plus the supporting negative and
  characterisation tests named in `## Test Strategy`.

### Out of scope / non-goals

- Any change to `KbdActions.Remove`'s signature or contract, and any `TryRemove`-style addition
  (rationale below).
- Any change to `Digits`' side-effecting getter shape, or to `SetVisualDigits`.
- Any change to `QfcCollectionController.cs:1439`'s mixed-mode synchronous `ToggleExpansion()` call on
  an async path.
- The `UnregisterNavigation` **count** mismatch identified under `### #472` above.
- Repairing or extending the QuickFiler surface's commented-out synchronous focus path
  (`QfcItemController.FocusAndTheme.cs:45`, `:61`, `:101`, `:117`).
- Any behaviour change to the Explorer surface (`EfcFormController.cs`, `EfcItemController.cs`).

### Files this feature MAY write

| Path | Note |
| --- | --- |
| `QuickFiler/Controllers/KbdActions.cs` | Constructor guard only. |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | The single writable `QfcItemController` partial. |
| `QuickFiler/Controllers/QfcCollectionController.cs` | Only the regions required by #444 and #472: the new width field, `RegisterNavigation`, `UnregisterNavigation`, and optionally an explanatory comment adjacent to `RegisterAsyncKeyActions`. |
| `QuickFiler.Test/**` (this feature's own test files) | Including one new file — see `## Test Strategy`. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | A single `<Compile Include>` line for the new test file. |
| `docs/features/active/quickfiler-keyboard-action-defects-444/**` | This feature's own documentation and evidence. |

### Forbidden files

Owned by concurrent siblings on the same integration branch. A fix that appears to require one of
these is recorded as a cross-feature note and kept out of the plan.

| Path | Owner |
| --- | --- |
| `QuickFiler/Controllers/KeyboardHandler.cs` | sibling #498 |
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | sibling #468 |
| `QuickFiler/Controllers/QfcItemController.cs` | siblings #484, #489 |
| `QuickFiler/Controllers/QfcItemController.Conversation.cs` | siblings #484, #489 |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | siblings #484, #489 |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | siblings #484, #489 |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | siblings #484, #489 |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | siblings #484, #489 |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | siblings #484, #489 |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | siblings #484, #489 |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | siblings #484, #489 |

Exactly one of the ten `QfcItemController` partials —
`QuickFiler/Controllers/QfcItemController.Navigation.cs` — is writable by this feature.

**The partition is mutually consistent.** Sibling #484's spec reciprocally lists
`QfcItemController.Navigation.cs` and `KbdActions.cs` as "(feature 444)" forbidden files and carries an
acceptance criterion that `Navigation.cs` is not modified. **VERIFIED** by direct read of
`docs/features/active/qfc-item-controller-defects-484/spec.md` at
`origin/epic/quickfiler-bug-family-integration` (commit `2670d4b6`). That document's
`### Out of scope / non-goals` section lists, under "**Forbidden files.**", the entries
`QuickFiler/Controllers/QfcItemController.Navigation.cs (feature 444)` and
`QuickFiler/Controllers/KbdActions.cs (feature 444)`; and its `## Acceptance Criteria` section
carries the criterion `QuickFiler/Controllers/QfcItemController.Navigation.cs is not modified.`
The partition therefore requires no Phase 0 reconfirmation.

Sibling #484 additionally records a downstream note addressed to this feature (its note 4),
proposing a `Func<TimerCallback, System.Threading.Timer>` factory seam at
`QuickFiler/Controllers/QfcItemController.Navigation.cs:223-224`. This feature **declines** it: no
one of issues #444, #472 or #482 is backed by it, and adopting it would be an opportunistic refactor
of the kind CLAUDE.md's Bugfix Workflow step 2 prohibits. It remains available to a later feature.

**Containment is achievable for all three fixes without writing a forbidden file.** The four
expansion register/unregister methods used by the #482 fix are declared in the forbidden
`QuickFiler/Controllers/QfcItemController.EventWiring.cs` (`RegisterExpandedActions` `:306-318`,
`RegisterExpandedAsyncActions` `:320-332`, `UnregisterExpandedActions` `:379-383`,
`UnregisterExpandedAsyncActions` `:385-389`), but they are `internal` members of the **same**
`partial class QfcItemController`. `Navigation.cs` may therefore **call** all four without editing
`EventWiring.cs`. No interface file needs editing either: `RegisterNavigation` and
`UnregisterNavigation` are declared on `IQfcCollectionController` but neither signature changes, and
a constructor is not part of any interface contract.

### Downstream notes (out of scope; recorded for the named owners)

1. **To the owner of `QfcItemController.EventWiring.cs` (sibling #484) — residual expansion/focus
   asymmetry.** `UnregisterFocusActions()` conditionally calls `UnregisterExpandedActions()` on
   `_expanded` (`EventWiring.cs:349-352`); `UnregisterFocusAsyncActions()` conditionally calls
   `UnregisterExpandedAsyncActions()` on `_expanded` (`:373-376`); `RegisterFocusActions()`
   conditionally calls `RegisterExpandedActions()` (`:208-211`); `RegisterFocusAsyncActions()`
   conditionally calls `RegisterExpandedAsyncActions()` (`:300-303`). Two precise consequences:

   - *Under a single-registry unification*, one of the two cleanup paths would remove from the
     registry that no longer holds the entries, re-creating exactly the silent-`false` divergence
     these three issues describe. **This is the primary reason this feature's #482 fix maintains
     both registries rather than collapsing onto one** (see `## Proposed Fix`, Option A vs B).
   - *Under this feature's chosen fix*, both registries hold `'B'`/`'D'` whenever `_expanded` is
     true, so each of the four focus-path calls removes or adds from a registry that genuinely holds
     the entries — with **one residual**: `RegisterFocusActions()` (`:208-211`) would double-add into
     `CharActions` if it ran while `_expanded` were already true. That path is currently dead in the
     QuickFiler surface because its call sites are commented out at
     `QfcItemController.FocusAndTheme.cs:45` and `:117`. **If #484 or #489 re-enables the synchronous
     focus path, it must route the expansion register/unregister calls through an idempotent owner
     rather than calling `RegisterExpandedActions()` directly.** This feature cannot make that change
     because `EventWiring.cs` and `FocusAndTheme.cs` are both forbidden to it.

2. **To the owner of `QfcItemController.EventWiring.cs` / `FocusAndTheme.cs` (siblings #484, #489) —
   dead synchronous focus path.** `RegisterFocusActions` (`EventWiring.cs:157` onward) and
   `UnregisterFocusActions` (`:334-353`) are effectively dead in the QuickFiler surface: their four
   call sites are commented out at `QfcItemController.FocusAndTheme.cs:45`, `:61`, `:101`, `:117`.
   Dead-code disposition belongs to the owner of those files, not to this feature.

3. **To the owner of `QfcCollectionController.cs` after this feature (sibling #484 and the
   epic) — the unfiled count-mismatch orphan.** `UnregisterNavigation` bounds its loop with the
   current `_itemGroups.Count` (`QfcCollectionController.cs:1345`) while
   `RemoveSpecificControlGroup(int)` (`:1105-1155`) mutates `_itemGroups` with no unregister/register
   bracket, reached unbracketed from `RemoveBelowThresholdAsync` (`:1077-1097` via the
   `RemoveGroupByEntryId` seam at `:1069-1074`) and from the `'R'` char action
   (`EventWiring.cs:197-201`). **This feature does not fix it.** It is a distinct defect from the
   filed width mismatch, and fixing it requires the key-ledger design, which breaks the existing
   characterisation tests at `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:409-423` and
   `:474-492` — a file that is exactly 500 lines and whose `[TestMethod]` count #468 freezes. Per
   `CLAUDE.md`'s Bugfix Workflow step 2 ("If you uncover deeper design problems, open a new issue
   instead of widening scope"), it **must be promoted as a new potential entry and GitHub issue**
   during this feature's execution, not absorbed silently and not left as prose in this folder. This
   feature's #472 regression test is written so that the residual orphan it leaves behind is asserted
   explicitly and attributed to that follow-up issue, so the assertion does not silently absorb the
   second defect.

4. **To the owner of `QfcCollectionController.cs` — mixed-mode toggle at `:1439`.**
   `ActivateBySelectionAsync` calls the synchronous `ToggleExpansion()` from an asynchronous path. It
   is currently unreachable with `blExpanded == true` (see `### #482` above) but remains a design
   defect. Promote as a new potential entry rather than editing it here.

5. **To the owner of `KeyboardHandler.cs` (sibling #498) — `Remove`'s discarded `bool`, scoped
   honestly.** Both #472 and #482 name the silently-discarded `Remove` result as the compounding
   factor. There are 42 production call sites of `KbdActions.Remove`. Their distribution decides the
   scope: 31 are in the forbidden `QfcItemController.EventWiring.cs`
   (`UnregisterFocusActions` `:336-348` — 13 sites; `UnregisterFocusAsyncActions` `:359-372` — 14
   sites; `UnregisterExpandedActions` `:381-382` — 2; `UnregisterExpandedAsyncActions` `:387-388` —
   2), 7 are in `EfcItemController.cs`, 2 in `EfcFormController.cs`, and only **2** are in a file this
   feature owns (`QfcCollectionController.cs:1349` and `:1353`, both inside `UnregisterNavigation`).
   Checking the result at every call site is therefore not achievable within this feature's ownership.
   Adding a `TryRemove` overload or a logging `Remove` variant to `KbdActions.cs` **is** mechanically
   possible — the file is owned — but would create a member no owned call site could adopt, producing
   dead API surface plus a coverage obligation with no consumer. **This feature therefore changes
   nothing about `Remove`, and instead makes the specific removals it owns provably total** (see
   `## Proposed Fix`). The cross-cutting question is promoted as a follow-up issue scoped to edit
   `EventWiring.cs` in coordination with #484/#489. This omission is a recorded decision, not an
   oversight.

6. **Sibling #484's downstream note to this feature is declined.** #484 records a note (its note 4)
   proposing a timer-factory seam at `QfcItemController.Navigation.cs:223-224`
   (`_emailIsReadTimer = new System.Threading.Timer(ApplyReadEmailFormat)` and the 4000 ms
   `Change(...)`). **This feature declines it**: none of #444, #472, or #482 names the read-email
   timer, and adopting it would widen this feature's diff in `Navigation.cs` beyond its three issues,
   increasing the merge surface against #489 for no defect closure. The seam is a reasonable idea and
   should be filed as its own potential entry by whichever feature needs it. This feature's #482 test
   avoids the timer by construction (see `## Test Strategy`), not by changing production code.

---

## Root Cause Analysis

**Confirmed common root cause.** `KbdActions<TKey, UClass, VDelegate>` has one class invariant — at
most one entry per `(SourceId, StoredKey)` pair — and three entry points that disagree about
enforcing it:

| Entry point | Enforces the invariant? | Evidence |
| --- | --- | --- |
| `Add(string, TKey, VDelegate)` | Yes, throws | `KbdActions.cs:92-98` |
| `Add(UClass)` | Yes, throws | `KbdActions.cs:108-119` |
| `KbdActions(IEnumerable<UClass>)` | **No** | `KbdActions.cs:26-29` |
| `Remove(string, TKey)` | Reports failure only through a `bool` that every production caller discards | `KbdActions.cs:123-135`; 42 discarding call sites |

Each of the three issues is a different way to reach an inconsistent registry through that gap:

- **#444** — an entry point that does not check (the constructor), given input that violates the
  invariant.
- **#472** — a *key-name* computation that is not stable between the register and unregister halves,
  so the removal targets keys that were never registered and the silent `false` hides it.
- **#482** — two registries maintained by two code paths behind one shared flag, so a removal targets
  the registry that never held the entry and the silent `false` hides it, until the *other* registry's
  `Add` throws.

**A second, load-bearing distinction inside the class.** `Find`, `FindIndex`, `ContainsKey`, and
`FilterKeys` compare with the element-defined `x.KeyEquals(key)` (`KbdActions.cs:49`, `:51`, `:55`,
`:73`), while `Add`'s guards and `Remove` compare with the static
`StoredKeyEquals` (`KbdActions.cs:33-34`), which is `EqualityComparer<TKey>.Default`. The two are not
interchangeable:

- `KaStringAsync.KeyEquals` (`QuickFiler/Controllers/KaStringAsync.cs:106` onward) is
  **substring-matching and side-effecting**. Its XML documentation at `:57-105` states that
  `Activated` is a per-keystroke latch gating both `Update` and `ToggleControl`, that a matching probe
  deliberately does not clear the latch and returns early, and that this early return "is therefore
  load-bearing and must not be 'completed' into a fall-through" (`:72-78`).
- `QuickFiler.Test/Controllers/KbdActionsTests.cs:13-29` is an explicit characterisation test that
  `"10"` and `"1"` legally coexist under the same `SourceId`, asserting
  `actions.Keys.Should().Equal("10", "1")` at `:28`.

**Established constraint.** The new constructor guard **must** compare with `StoredKeyEquals`, never
`KeyEquals`. A `KeyEquals`-based guard would (a) mis-compare, rejecting the legal `"10"`/`"1"` pair
and breaking `KbdActionsTests.cs:13-29`, and (b) fire `KaStringAsync`'s latch side effects during
construction.

**Affected components.** `QuickFiler/Controllers/KbdActions.cs`,
`QuickFiler/Controllers/QfcCollectionController.cs`,
`QuickFiler/Controllers/QfcItemController.Navigation.cs`. Read-only dependencies:
`QuickFiler/Controllers/KeyboardHandler.cs`, `QuickFiler/Controllers/QfcItemController.EventWiring.cs`,
`QuickFiler/Controllers/KaStringAsync.cs`, `QuickFiler/Controllers/KaChar.cs`.

---

## Proposed Fix

### Design summary (what changes where)

| Issue | File | Change |
| --- | --- | --- |
| #444 | `QuickFiler/Controllers/KbdActions.cs` | The `IEnumerable<UClass>` constructor materialises the sequence, then scans it for a repeated `(SourceId, StoredKey)` pair and throws `ArgumentException` naming the offending pair. |
| #444 | `docs/.../spec.md` (+ optional in-code comment) | The `Keys.Down` product decision is recorded, and the surviving live registration is pinned by a regression test. |
| #472 | `QuickFiler/Controllers/QfcCollectionController.cs` | A new private field records the digit width used at registration; `RegisterNavigation` assigns it; `UnregisterNavigation` formats from it and reads `Digits` zero times. |
| #482 | `QuickFiler/Controllers/QfcItemController.Navigation.cs` | A new private `SyncExpandedRegistrations(bool)` becomes the single owner of expansion registration, called by both `ToggleState` overloads after the flag is written. |

### #444 — constructor duplicate guard

**Chosen: throw on duplicate.** Detect any `(SourceId, Key)` pair repeated in `list` using
`StoredKeyEquals`, and throw `ArgumentException` naming the offending pair, mirroring `Add(UClass)`'s
message shape (`KbdActions.cs:114-118`) with the parameter name `list`.

Rejected alternatives, with reasons:

- *De-duplicate silently* — contradicts both `Add` overloads, which throw, and silently discards a
  registration the caller asked for. That is precisely the silent-divergence failure mode that
  produced #472 and #482.
- *Document in-code why the constructor deliberately does not check* — permitted by #444's promoted
  criterion, but leaves the invariant hole open for no benefit now that the blast radius is zero.

Implementation constraints, all load-bearing:

- **Preserve the existing null behaviour.** `new List<UClass>(list)` currently throws
  `ArgumentNullException` for a null `list` (`KbdActions.cs:28`). Materialise first, then scan, so a
  null argument still produces `ArgumentNullException` and never `NullReferenceException`.
- **Enumerate `list` exactly once.** The parameter is `IEnumerable<UClass>` and may be a one-shot
  sequence; materialise into the backing `List<UClass>` and scan that list.
- **Compare with `StoredKeyEquals` (`KbdActions.cs:33-34`), never `KeyEquals`.** See
  `## Root Cause Analysis`.
- **Log before throwing**, via the existing `logger` (`KbdActions.cs:17-19`), matching `:96` and
  `:117`.
- **Reuse the literal fragment `already exists`** so the existing assertions
  `.WithMessage("*already exists*")` at `QuickFiler.Test/Controllers/KbdActionsTests.cs:46` and
  `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs:66` remain the vocabulary the new
  test asserts against.
- **Keep the scan O(n²)** over the seed list, consistent with `Add`'s existing `_list.Any(...)`. Seed
  lists in this repository are at most eight entries (`EfcFormController.cs:574-602`, `:631-676`); a
  hash set would be premature and would require an `IEqualityComparer<TKey>`.

### #444 — product decision: `Keys.Down` means `SelectNextItem()`

**Decision: on the QuickFiler collection surface, `Keys.Down` means `SelectNextItem()`.
Confidence: HIGH.** Five independent lines of evidence agree and none dissents:

1. **The surviving live registration binds `SelectNextItemAsync` and nothing else.**
   `RegisterAsyncKeyActions` (`QfcCollectionController.cs:1282-1291`) builds `KeyActionsAsync` with
   exactly two entries: `("Collection", Keys.Up, SelectPreviousItemAsync())` at `:1287` and
   `("Collection", Keys.Down, SelectNextItemAsync())` at `:1288`. This is the registration that
   actually runs, reached from `WireUpAsyncKeyboardHandler` (`:1275-1280`).
2. **`ActionOk` is bound to Return, not Down, on every surface.**
   `QfcCollectionController.cs:1302` registers `("Collection", Keys.Return, CustomReturnKeyHandler())`
   and `CustomReturnKeyHandler` (`:1307-1314`) awaits `_parent.ActionOkAsync()` at `:1312`. The
   Explorer sibling does the same at `QuickFiler/Controllers/EfcFormController.cs:365`.
3. **Up/Down are a symmetric navigation pair across the codebase.**
   `QuickFiler/Controllers/KeyboardHandler.cs:333` treats `Keys.Up` and `Keys.Down` identically, while
   `:367` treats `Keys.Return` and `Keys.Escape` identically. Legacy agrees:
   `QuickFiler/Legacy/QfcController.cs:1686` (`Keys.Down`) and `:1691` (`Keys.Up`) are the navigation
   cases; `:1608` (`Keys.Return`) is the action case.
4. **The duplicate entry is second in the list literal.** `QfcCollectionController.cs:1269` is
   `SelectNextItem()`, `:1270` is `_parent.ActionOkAsync()`. Had they gone through `Add`, the second
   would have been rejected — i.e. `SelectNextItem()` is what would have survived. Weak evidence, but
   it points the same way.
5. **No `Keys.Down`-to-`ActionOk` mapping exists anywhere.** A repository-wide search of `*.cs` for
   `Keys.Down` returns 12 hits; none binds an OK/commit action.

No existing test constrains the decision: `KbdActionsTests.cs`,
`KbdActionsRemainingBranchesTests.cs`, `QfcCollectionControllerTests.cs`, and
`QfcItemController.NavigationTests.cs` contain no assertion involving `Keys.Down`.

**Why this is recorded rather than implemented.** #468 `[P1-T2]` deletes the ambiguous site before
this feature starts, so there is no live registration to correct. The decision is what #468's deletion
implicitly ratifies, and it is recorded here so a reviewer can see that the deletion preserved the
intended behaviour rather than arbitrarily discarding one of two candidates. Its durable form is a
regression test pinning `RegisterAsyncKeyActions`'s `Keys.Down` → `SelectNextItemAsync` binding, so a
future edit cannot silently re-introduce the wrong action.

**How a reviewer could overturn this.** Only by producing a user-facing artifact (help text, a
keyboard-shortcut card, or a maintainer statement) establishing that Down is a commit gesture in
QuickFiler. No such artifact exists under `docs/`. `UNVERIFIED`: no runtime observation of the shipped
add-in was possible.

### #472 — record the registered digit width

**Chosen: record the width in controller state.**

- Add `private int _registeredDigits;` to `QfcCollectionController`.
- `RegisterNavigation` (`:1330-1341`) assigns it from the `digits` local it already captures at
  `:1332`. No other change to that method.
- `UnregisterNavigation` (`:1343-1356`) selects its format from `_registeredDigits` and reads the
  `Digits` property **zero times**.

This delivers exactly the promoted document's stated expected behaviour ("The digit width used to
unregister a navigation key must be the same width used to register it"), removes the side-effecting
property read from the unregister path entirely, and is immune to a count change during the loop as
well as between calls. The "hoist `var digits = Digits;`" variant is subsumed for free and, on its
own, would not have fixed the filed defect.

**Mandatory formulation detail.** Tests build the controller with
`FormatterServices.GetUninitializedObject`, which bypasses field initialisers
(`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:36-37`, `:147-148`, `:254-255`,
`:343-344`), so a test-built controller sees `_registeredDigits == 0`. Write the format selection as
`_registeredDigits == 2 ? "00" : ""` — i.e. treat anything that is not 2 as single-digit — rather than
`== 1 ? … : "00"`. With that formulation the four existing navigation tests in
`QfcCollectionControllerTests.cs` (at `:409`, `:430`, `:452`, `:474`) operate on one- and two-item
pages at width 1 and pass unchanged, and no existing test needs to inject the new field.

Rejected alternatives, with reasons:

- *Key ledger* (record every registered key in a `List<string>` and unregister from the ledger) —
  design-complete: it would fix the width mismatch **and** the count mismatch of
  `### Downstream notes` item 3, and would make every `Remove` return `true`. It is rejected because
  it changes the outcome of the existing characterisation tests at
  `QfcCollectionControllerTests.cs:409-423` and `:474-492`, which seed keys directly through
  `SeedCollectionKey` (`:386-389`) and rely on `UnregisterNavigation` removing computed keys. With a
  ledger the ledger is empty and those assertions change. Those tests are #232's regression suite and
  are part of the spec under `CLAUDE.md` §7.3, and repairing them requires editing a file that is
  exactly 500 lines with a frozen `[TestMethod]` count. The count mismatch is promoted as a separate
  issue instead.
- *Make `Digits` non-side-effecting* — a genuine design improvement, but it touches four other
  consumers of `Digits` in `QfcCollectionController.cs`, two of which sit inside members #468 edits,
  and it does not by itself fix the mismatch. High conflict surface, no defect closure.

### #482 — a single expansion-registration owner

**Chosen: unregister both registries, then register both when expanded.** Introduce
`private void SyncExpandedRegistrations(bool expanded)` in
`QuickFiler/Controllers/QfcItemController.Navigation.cs`. It unconditionally calls
`UnregisterExpandedActions()` and `UnregisterExpandedAsyncActions()` — both are safe no-ops when the
entries are absent, because `Remove` returns `false` rather than throwing
(`KbdActions.cs:126-129`) — and then, when `expanded` is true, calls `RegisterExpandedActions()` and
`RegisterExpandedAsyncActions()`. Both `ToggleState` overloads call it once, after
`ToggleExpansionOn()` / `ToggleExpansionOff()` has written `_expanded`, passing `_expanded`.

Illustrative shape, not final source:

```csharp
public virtual void ToggleExpansion(Enums.ToggleState desiredState)
{
    _parent.ToggleExpansionStyle(ItemIndex, desiredState);
    if (desiredState == Enums.ToggleState.On) { ToggleExpansionOn(); } else { ToggleExpansionOff(); }
    SyncExpandedRegistrations(_expanded);
}
```

This is exactly what #482's own "Suspected Fix" prescribes: "a single registration owner keyed on the
actual current state rather than on which code path performed the toggle". It makes registration
idempotent without changing `Add`'s contract — which #482 itself flags as undesirable ("making `Add`
idempotent is a contract change affecting all consumers and interacts with #444") — and it is
contained to `Navigation.cs`.

Rejected alternatives, with reasons:

- *Unregister both, register only the calling path's own registry* — strictly minimal and introduces
  no behavioural widening, but leaves the two registries permanently disagreeing: after a synchronous
  expansion plain `B`/`D` still do nothing, and after an asynchronous expansion Alt+`B`/Alt+`D` still
  do nothing. It preserves the latent inconsistency the issue asks to remove. It would also
  reintroduce the cleanup hazard described in `### Downstream notes` item 1.
- *Make `Add` idempotent* — directly contradicts the #444 direction (tighten the invariant), and would
  break `KbdActionsTests.cs:31-47` and `KbdActionsRemainingBranchesTests.cs:54-67`.

**Deliberate behaviour widening — stated explicitly.** After this change, `'B'` and `'D'` respond
following a synchronous expansion (previously only after an asynchronous one), and Alt+`B`/Alt+`D`
respond following an asynchronous expansion (previously only after a synchronous one). This closes a
pre-existing behavioural gap rather than introducing a new binding: both keys were always intended to
be available while an item is expanded. It must be stated in the PR body.

**Constraints on the edit.** `ToggleExpansion(Enums.ToggleState)` and
`ToggleExpansionAsync(Enums.ToggleState)` carry
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` at
`QfcItemController.Navigation.cs:173` and `:191` and are `virtual` for the test-override reason
documented at `:171-172` and `:189-190`. Sibling #489 may de-exempt them. Keep the edit inside those
two bodies to a single call each and put all logic in the new helper, which must **not** carry
`[ExcludeFromCodeCoverage]`, so #489's de-exemption is unaffected and the helper's lines are measured.
`RegisterExpandedActions()` retains its other caller at `EventWiring.cs:210`, so it is not orphaned.

### Boundaries and invariants to preserve

- `KbdActions`' public surface is unchanged; only one constructor's behaviour changes.
- `ArgumentNullException` for a null `list` is preserved.
- `KaStringAsync`'s substring `KeyEquals` semantics and latch contract
  (`KaStringAsync.cs:57-105`) are untouched, and legal `"10"`/`"1"` coexistence is preserved.
- `Remove`'s `bool` return, and its silent `false` for an absent pair, are unchanged.
- No interface file is edited. `RegisterNavigation`, `UnregisterNavigation`, `ToggleExpansion`, and
  `ToggleExpansionAsync` keep their declared signatures.
- No new `public` member is added to any `public` type, so this feature adds no public-API change.
- `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` continues to pass: no `Form`-derived type is
  added to the test assembly.

### Dependencies and blocked work

This feature is a wave-1 child of epic `quickfiler-bug-family` and branches from
`epic/quickfiler-bug-family-integration` **after** #468 has merged. #468 is a hard upstream
dependency for the #444 criteria marked inherited. Phase 0 must verify #468's deletions are present;
if `WireUpKeyboardHandler` is still declared in `QfcCollectionController.cs`, #468 has not landed and
planning assumptions in this document do not hold.

### Upstream contract (exhaustive) — required by features 464 and 489

Siblings **#464** and **#489** are being authored against this feature's contract for
`QuickFiler/Controllers/KbdActions.cs` and `QuickFiler/Controllers/QfcItemController.Navigation.cs`.
The tables below are exhaustive for those two files: any member not listed as ADDED, CHANGED, or
REMOVED is unchanged in signature, accessibility, static-ness, attributes, and behaviour.

#### `QuickFiler/Controllers/KbdActions.cs`

**ADDED members: none.**

**REMOVED members: none.**

**CHANGED members: one (behaviour only; signature identical).**

| Member | Accessibility | Static | Signature after this feature | Behavioural delta |
| --- | --- | --- | --- | --- |
| Enumerable constructor | `public` | instance | `public KbdActions(IEnumerable<UClass> list)` | Now throws `ArgumentException` (parameter name `list`) when `list` contains two or more elements sharing the same `SourceId` **and** a `StoredKeyEquals`-equal `Key`. The message contains the literal fragment `already exists`, matching both `Add` overloads, and is logged via `logger.Error` before the throw. `ArgumentNullException` for a null `list` is unchanged. A duplicate-free sequence is accepted unchanged. Elements whose `KeyEquals` overlaps but whose stored keys differ — for example `KaStringAsync` `"10"` and `"1"` under one `SourceId` — remain **legal**, because the guard uses `StoredKeyEquals`, not `KeyEquals`. |

**UNCHANGED members (exhaustive, for #464's and #489's certainty).** All are `public` instance members
of `KbdActions<TKey, UClass, VDelegate>` except where noted; none changes signature, accessibility,
static-ness, or behaviour.

| Member | Signature | Note |
| --- | --- | --- |
| Parameterless constructor | `public KbdActions()` | Unchanged. Every `= []` collection-expression initializer in `KeyboardHandler.cs` binds here, not to the enumerable constructor, so no `KeyboardHandler.cs` initializer is affected. |
| `StoredKeyEquals` | `private static bool StoredKeyEquals(TKey left, TKey right)` | `private`, `static`. Unchanged; now also consumed by the constructor guard. |
| Indexer | `public VDelegate this[TKey key] { get; set; }` | Unchanged. Still routes through `Find`. |
| `ContainsKey` | `public bool ContainsKey(TKey key)` | Unchanged. Still `KeyEquals`-based. |
| `FilterKeys` | `public UClass[] FilterKeys(TKey key)` | Unchanged. Still `KeyEquals`-based. |
| `Find` | `public UClass Find(TKey key)` | Unchanged, including the `InvalidOperationException` on an ambiguous match. |
| `FindIndex` | `public int FindIndex(TKey key)` | Unchanged, including the `InvalidOperationException` on an ambiguous match. |
| `Add` (three-argument) | `public void Add(string sourceId, TKey key, VDelegate @delegate)` | Unchanged. |
| `Add` (instance) | `public void Add(UClass instance)` | Unchanged, including `nameof(instance)` as the thrown parameter name. |
| `Remove` | `public bool Remove(string sourceId, TKey key)` | Unchanged. **Still returns `bool`, still returns `false` silently for an absent pair.** No `TryRemove` is added. |
| `GetEnumerator` | `public IEnumerator<UClass> GetEnumerator()` and the explicit `IEnumerator IEnumerable.GetEnumerator()` | Unchanged. |
| `Keys` | `public ICollection<TKey> Keys { get; }` | Unchanged. |

#### `QuickFiler/Controllers/QfcItemController.Navigation.cs`

**ADDED members: one.**

| Member | Accessibility | Static | Signature | Attributes | Behavioural note |
| --- | --- | --- | --- | --- | --- |
| `SyncExpandedRegistrations` | `private` | instance | `private void SyncExpandedRegistrations(bool expanded)` | **none** — must not carry `[ExcludeFromCodeCoverage]` | Single owner of expansion registration. Unconditionally calls `UnregisterExpandedActions()` and `UnregisterExpandedAsyncActions()`; when `expanded` is true, then calls `RegisterExpandedActions()` and `RegisterExpandedAsyncActions()`. Idempotent for repeated calls with the same argument. It is `private`, so it is not part of the sibling-visible surface — it is named here so a sibling does not introduce a colliding member on another partial of the same class. |

**REMOVED members: none.**

**CHANGED members: two (body only; signature, accessibility, `virtual`ness, and attributes all
identical).**

| Member | Accessibility | Static | Signature after this feature | Attributes after | Behavioural delta |
| --- | --- | --- | --- | --- | --- |
| `ToggleExpansion(Enums.ToggleState)` | `public` | instance, `virtual` | `public virtual void ToggleExpansion(Enums.ToggleState desiredState)` | `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` retained | Now maintains **both** `_kbdHandler.CharActions` and `_kbdHandler.CharActionsAsync` instead of only the former, by delegating to `SyncExpandedRegistrations(_expanded)`. No longer throws `ArgumentException` when the expansion state was previously set by the async overload. |
| `ToggleExpansionAsync(Enums.ToggleState)` | `public` | instance, `virtual`, `async Task` | `public virtual async Task ToggleExpansionAsync(Enums.ToggleState desiredState)` | `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` retained | Now maintains **both** registries instead of only `CharActionsAsync`, by delegating to `SyncExpandedRegistrations(_expanded)`. No longer throws `ArgumentException` when the expansion state was previously set by the sync overload. |

**Observable behaviour changes for the file, enumerated:**

1. After either `ToggleState` overload completes, **both** `CharActions` and `CharActionsAsync` hold
   `('B','D')` for `ItemHelper.EntryId` if and only if `_expanded` is `true`.
2. Expansion registration is **idempotent**: calling either overload with `ToggleState.On` twice in a
   row, or interleaving the two overloads in any order, no longer throws `ArgumentException` from
   `KbdActions.Add`.
3. `'B'` and `'D'` now respond after a synchronous expansion (previously only after an asynchronous
   one), and Alt+`B`/Alt+`D` now respond after an asynchronous expansion (previously only after a
   synchronous one).

**UNCHANGED in this file (exhaustive for the members #464 and #489 could depend on).** The
parameterless `ToggleExpansion()` and `ToggleExpansionAsync()` routing overloads;
`ToggleExpansionOn()` and `ToggleExpansionOff()` (both remain `private`, keep their `_tlpStates`
application, their `_expanded` write at `:210` and `:220`, and their `_emailIsReadTimer` handling
including the 4000 ms `Change` at `:224`); `JumpToFolderDropDown`; `JumpToFolderDropDownAsync`;
`JumpToSearchTextbox`; `JumpToAsync`; both `KbdExecuteAsync` overloads; `MenuDropDown`; `Reply`;
`ReplyAll`; `Forward`; and both `ToggleConversationCheckbox` overloads.

**Explicitly NOT changed, contrary to what a reader might expect.** The four registration methods
themselves — `RegisterExpandedActions`, `RegisterExpandedAsyncActions`, `UnregisterExpandedActions`,
`UnregisterExpandedAsyncActions` — remain in the forbidden
`QuickFiler/Controllers/QfcItemController.EventWiring.cs` (`:306-318`, `:320-332`, `:379-383`,
`:385-389`) with their current `internal` accessibility and their current bodies. This feature calls
them; it does not edit them.

#### `QuickFiler/Controllers/QfcCollectionController.cs` (informational, for #468's and #484's awareness)

Not part of the #464/#489 contract, but recorded so no sibling is surprised.

| Change | Detail |
| --- | --- |
| ADDED | `private int _registeredDigits;` — a field, `private`, instance. |
| CHANGED | `public void RegisterNavigation()` — one assignment added; signature unchanged. |
| CHANGED | `public void UnregisterNavigation()` — body rewritten; signature unchanged. It **no longer reads the `Digits` property**, so it no longer triggers that getter's side effect of setting `_digitRefreshNeeded` and mutating `_digits`. |
| REMOVED | none. |
| NOT edited | `QuickFiler/Interfaces/IQfcCollectionController.cs`. Neither method's signature changes. |

---

## Assumptions, Constraints, Dependencies

**Assumptions.**

- The branch this feature is cut from carries #468's merged code, so `WireUpKeyboardHandler` is
  absent. Phase 0 must verify this and halt if it is present.
- `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` — created by #468 `[P2-T1]`
  with asserting `SetField` / `GetField` / `InvokeNonPublic` helpers and a
  `CreateUninitializedController` builder — exists on the branch. **Verified absent at spec time**:
  the file does not exist at base commit `988e819b`, and `git diff origin/main
  origin/epic/quickfiler-bug-family-integration -- QuickFiler.Test/` is empty, so #468 has prepared
  but not executed `[P2-T1]`. Whether it exists at execution time depends entirely on #468 having
  run. Phase 0 must therefore test for it and branch: reuse it when present, and otherwise fall back
  to a private local reflection helper in the new test file. Do not assume either outcome.

  By contrast, `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` — the helper the #482
  test depends on for `SetField` / `GetField` / `InvokeNonPublic` / `BuildSyncDispatcher` — **is
  verified present** at `988e819b` and is unaffected by #468.
- `MailItemHelper.UnRead` defaults to `false`. **VERIFIED** by direct read:
  `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Properties.cs:298-301` declares
  `private Lazy<bool>? _unread;` and implements the getter as `get => _unread?.Value ?? false;`. A
  default-constructed `MailItemHelper` leaves `_unread` null, so `UnRead` reports `false` and
  `ToggleExpansionOn()` does **not** enter the timer-arming branch at
  `QuickFiler/Controllers/QfcItemController.Navigation.cs:221-225`. The #482 test therefore needs no
  timer mitigation and no explicit `UnRead` mock.

  **Constraint that follows.** Tests must never *assign* `UnRead`. The setter at
  `MailItemHelper.Properties.cs:302-307` writes through to `Item.UnRead` and then calls
  `Item.Save()`, which would dereference the backing Outlook item. Rely on the default only.

**Line-number volatility (constraint on the atomic plan).** #468 `[P1-T2]` deletes twelve dead members
from `QfcCollectionController.cs` and other #468 tasks insert lines before the navigation region, so
the net shift is not derivable from #468's plan text. **No post-#468 line number may be transcribed
into the atomic plan.** Every edit must be anchored on the member name, and Phase 0 must re-derive
every anchor against the actual branch head. The following citations in this document will shift:
every citation into `QuickFiler/Controllers/QfcCollectionController.cs`, and every citation into
`QuickFiler.Test/QuickFiler.Test.csproj`. Citations into `KbdActions.cs`,
`QfcItemController.Navigation.cs`, `QfcItemController.EventWiring.cs`,
`QfcItemController.FocusAndTheme.cs`, `KeyboardHandler.cs`, and `KaStringAsync.cs` are not affected by
#468, which does not edit those files. `QfcCollectionController.cs:1254-1273` and `:1265-1272` will
not merely shift — they will **cease to exist**.

**Coverage-policy conflict (recorded, not silently resolved).** `CLAUDE.md` §UT2 states a repo-wide
floor of `>= 80%` with `>= 90%` for new modules/classes/methods, and explicitly names `KbdActions<>` as
a testable seam that is **not** exempt. `.claude/rules/general-unit-test.md` and
`.claude/rules/quality-tiers.md` state `>= 85%` line and `>= 75%` branch. These figures conflict. The
conflict is pre-existing and repository-wide; it is not created by this feature. This spec adopts the
**stricter** of each pair for its own acceptance criteria, which satisfies both documents
simultaneously, and does not attempt to resolve the conflict for the repository.

**`[ExcludeFromCodeCoverage]` on `QfcCollectionController`.** The class carries the attribute at
`QuickFiler/Controllers/QfcCollectionController.cs:21`, so every line this feature changes for #472
contributes nothing to any coverage denominator. Per-defect proof for #472 is therefore carried by a
**named test**, never by a coverage delta. No acceptance condition in this document claims a coverage
increase attributable to this feature's `QfcCollectionController.cs` changes;
`.claude/rules/plan-acceptance-gates.md` rejects conditions that cannot fail.

**Constraints.**

- No production or test file **added** by this feature may exceed **500 lines**, and every
  pre-existing file changed by this feature must be either at or below **500 lines** or no larger
  than its Phase 0 baseline line count
  (`.claude/rules/general-code-change.md`). `QuickFiler/Controllers/QfcCollectionController.cs`
  already exceeds the cap for pre-existing reasons (2349 lines at `988e819b`; roughly 2120-2180
  after #468 deletes `WireUpKeyboardHandler`). That excess is out of scope here: this feature
  neither creates nor is permitted to remediate it, exactly as #468 records the same excess as a
  pre-existing finding rather than a work item. The plan carries the same disposition under its
  decision `D-P6`.
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is **exactly 500 lines** with zero
  spare, and #468 `D12` / `[P4-T5]` freezes its `[TestMethod]` count (re-verified by #468's
  `[P14-T11]`). **No test may be added to it.** Current size (not spare capacity) elsewhere, measured at `988e819b`:
  `KbdActionsTests.cs` 88; `KbdActionsRemainingBranchesTests.cs` 181;
  `QfcItemController.NavigationTests.cs` 391; `QfcCollectionControllerDarkModeTests.cs` 155.
- Framework: **MSTest**; mocking: **Moq**; assertions: **FluentAssertions**.
- Target framework is `net48`; `init`, `record`, and `record struct` are unavailable.
- Analyzer and nullable builds must use `/t:Rebuild`, and the nullable build must **not** add
  `/p:Nullable=enable` (see `CLAUDE.md` § C#1.3).

---

## Data / API / Config Impact

- **User-facing behaviour changes:** one, and it is deliberate — `'B'`/`'D'` and Alt+`B`/Alt+`D`
  become available after either kind of expansion (see `## Proposed Fix`, #482). No other user-facing
  behaviour changes.
- **Public API changes:** none. No member is added to, removed from, or re-signed on any `public`
  type, and no interface file is edited. The constructor guard is a behaviour change, not a signature
  change.
- **Data or migration considerations:** none. No persisted format, settings schema, or serialized type
  is touched.
- **Logging/telemetry:** one new `logger.Error` call inside the `KbdActions` enumerable constructor,
  emitted immediately before the new `ArgumentException`, matching the existing pattern at
  `KbdActions.cs:96` and `:117`. No logging level, sink, or configuration changes.
- **Configuration keys:** none added, removed, or defaulted differently.
- **Compatibility:** a caller that deliberately constructs a `KbdActions` from a duplicate-bearing
  sequence would begin to throw. No such caller exists in the repository post-#468; the full
  construction-site inventory is in `## Repro & Evidence`.
- **Performance:** the constructor gains an O(n²) scan over the seed list. Seed lists are at most
  eight entries, so the cost is negligible. `UnregisterNavigation` becomes cheaper: it no longer
  invokes the `[MethodImpl(MethodImplOptions.Synchronized)]` `Digits` getter once per iteration.

---

## Test Strategy

**Framework and policy.** MSTest (`[TestClass]` / `[TestMethod]`), Moq for boundaries,
FluentAssertions for assertions. Every test below: creates **no** temporary file; starts **no**
`System.Windows.Forms.Form` and calls no `Show()`; constructs **no** real `BackgroundWorker`;
contains **no** `Thread.Sleep`, `Task.Delay`, real wall-clock wait, `DateTime.Now`, or unseeded
randomness; touches no mutable static state; and is order-independent. Tests live in
`QuickFiler.Test/Controllers/`, mirroring the production layout.
`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` must continue to pass.

**Failing-first ordering is mandatory.** `CLAUDE.md`'s Bugfix Workflow step 1 requires the smallest
deterministic reproducing test **before** the fix. For each of the three defects the plan must:
(a) add the regression test, (b) run it and capture a **RED** result to the evidence dossier,
(c) apply the fix, (d) re-run and capture **GREEN**. The dossier is written to
`docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/` per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.

### Per-defect tests

**#444 — constructor guard.** File: `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs`
(181 lines; roughly 40 added lines keeps it well under 500). Pure collection tests; no Moq needed —
that file already uses plain construction and a `NewRegistry()` helper at `:21-22`.

- *Positive (RED before the fix):* construct
  `new KbdActions<Keys, KaKey, Action<Keys>>(new List<KaKey> { new KaKey("src", Keys.Down, _ => { }), new KaKey("src", Keys.Down, _ => { }) })`
  and assert `ArgumentException` with `.WithMessage("*already exists*")`. Pre-fix this raises no
  exception — a clean deterministic red.
- *Negative:* a list containing `("src", Keys.Up)` and `("src", Keys.Down)` must not throw.
- *Negative:* a list containing the same `Key` under **different** `SourceId` values must not throw.
- *Null:* `new KbdActions<…>(null)` must still throw `ArgumentNullException`, not
  `NullReferenceException`.

**#444 — `StoredKeyEquals`-not-`KeyEquals` pin.** File:
`QuickFiler.Test/Controllers/KbdActionsTests.cs` (88 lines). Construct a
`KbdActions<string, KaStringAsync, Func<string, Task>>` from a seed list containing
`("Collection","10")` and `("Collection","1")` and assert it does **not** throw, mirroring the
existing `Add` characterisation at `:13-29`. This test fails if a future edit swaps the guard to
`KeyEquals`.

**#444 — `Keys.Down` decision pin.** File: the new test file named below (it cannot go in
`QfcCollectionControllerTests.cs`). Build the controller by the `CreateControllerForSwap` pattern
(`QfcCollectionControllerTests.cs:338-365`), inject a real
`KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>` behind the `KeyActionsAsync` property of a Loose
`Mock<IQfcKeyboardHandler>`, call `RegisterAsyncKeyActions()`, and assert exactly one
`("Collection", Keys.Down)` entry and exactly one `("Collection", Keys.Up)` entry exist.
`RegisterAsyncKeyActions` is `internal`; `QuickFiler.Test` already reaches `internal` members of
`QuickFiler` (for example `controller.RegisterExpandedActions()` at
`QfcItemController.EventWiringTests.cs:192`), so no accessibility change is needed. **This test has no
pre-fix red state** — it pins behaviour #468 already established. Record it in the fail-before dossier
explicitly as *pass-after-only, no red expected*, so the dossier is not read as a missing red.

**#444 — inherited-criterion verification.** A Phase 0 check, not a `[TestMethod]`: a repository-wide
search of `*.cs` for `WireUpKeyboardHandler` must return **zero** hits. Non-zero means #468 has not
landed and the plan's assumptions do not hold; the executor halts.

**#472 — digit-width fidelity.** File: a **new** file
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`. A new file is required
because `QfcCollectionControllerTests.cs` is exactly 500 lines with a frozen `[TestMethod]` count.

Arrangement, chosen to avoid the `SetVisualDigits` null-dereference hazard: inject ten groups **and**
`_digits = 2`, so `Digits`' `digitNeed` already equals `_digits`, `_digitRefreshNeeded` stays `false`,
and `RegisterNavigation` never enters `SetVisualDigits`
(`QfcCollectionController.cs:1333-1336`) where `grp.ItemController` would be dereferenced at `:140`.

1. Inject 10 groups and `_digits = 2`; call `RegisterNavigation()`. Assert the registry holds
   `"01".."10"` under `SourceId "Collection"`.
2. Remove one group directly from the injected `List<QfcItemGroup>`, bringing the count to 9. This
   models the unbracketed `RemoveSpecificControlGroup` path without touching WinForms, mirroring
   `QfcCollectionControllerTests.cs:483`.
3. Call `UnregisterNavigation()`.
4. **RED before the fix:** all ten `"01".."10"` entries survive, because `Digits` now evaluates to 1
   and the loop removed the absent `"1".."9"`. **GREEN after the fix:** `_registeredDigits == 2`, so
   the loop removes `"01".."09"`. The assertion must be written as *"no `"0"`-prefixed key survives
   except the single `"10"` entry the loop bound cannot reach"*, with an XML comment on the test
   stating that the residual `"10"` is the separately-promoted count-mismatch defect
   (`### Downstream notes` item 3) and not this fix's scope. Writing it this way keeps the assertion
   honest and prevents it from silently absorbing the second defect.
5. A mirror-direction test: register at 9 items with `_digits = 1`, grow to 10, unregister, and assert
   the same width-fidelity property.

`.csproj` registration: insert a single `<Compile Include="Controllers\QfcCollectionControllerNavigationDigitsTests.cs" />`
line into `QuickFiler.Test/QuickFiler.Test.csproj` **immediately after**
`Controllers\QfcCollectionControllerTests.cs` and **before**
`Controllers\QfcCollectionControllerDarkModeTests.cs` (at `988e819b`, lines `:116` and `:117`). That
slot is chosen deliberately: #468 `D13` / `[P2-T2]` inserts its entries *after* the dark-mode entry,
so this slot does not overlap. Keep the insertion to a single line and record its exact neighbours in
the plan so any conflict is trivially resolvable.

**#482 — expansion registry interleaving.** File:
`QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` (391 lines; roughly 90 added lines
ends near 480, under the cap). If planning judges that too tight,
`QfcItemController.EventWiringTests.cs` (375 lines) is the alternative and already owns both registry
builders.

Arrangement: one `Mock<IQfcKeyboardHandler>` wired to **both** a real
`KbdActions<char, KaChar, Action<char>>` (via `CharActions`) and a real
`KbdActions<char, KaCharAsync, Func<char, Task>>` (via `CharActionsAsync`), so the interleaving is
observable in one arrangement. Additional injections needed to drive the real (non-overridden)
`ToggleState` bodies:

- `_parent` — a Loose `Mock<IQfcCollectionController>`, so `ToggleExpansionStyle` /
  `ToggleExpansionStyleAsync` are no-ops.
- `_itemViewer` — a `Mock<IItemViewer>` whose `Controls` returns a bare `Control().Controls`, exactly
  as `QfcItemController.NavigationTests.cs:319-320` and `:372-373` already do.
- `_tlpStates` — a real `TlpCellStates` carrying `"Expanded"` and `"Compressed"` snapshots built from
  handle-less `TableLayoutPanel` / `Label` controls, as at
  `QfcItemController.NavigationTests.cs:296-327`.
- `_uiDispatcher` — `QfcItemControllerTestSupport.BuildSyncDispatcher().Object`, so
  `ToggleExpansionAsync`'s `InvokeAsync(() => ToggleExpansionOn())`
  (`QfcItemController.Navigation.cs:197`, `:202`) runs synchronously.
- `ItemHelper` — set by the `KbdController` pattern (`QfcItemController.EventWiringTests.cs:35-37`).
  **`UnRead` must be `false`**, otherwise `ToggleExpansionOn()` constructs a 4000 ms
  `System.Threading.Timer` (`QfcItemController.Navigation.cs:221-225`), which the determinism rules
  forbid. Assert or mock this explicitly rather than relying on a default.

The failing test is the three-step interleaving:

```
ToggleExpansionAsync(On)   // async registry gains B,D; _expanded = true
ToggleExpansion(Off)       // sync unregister no-ops; _expanded = false
ToggleExpansionAsync(On)   // RED: ArgumentException from KbdActions.Add
```

Pre-fix the third call throws `ArgumentException` — a clean deterministic red. Post-fix it succeeds
and the assertion becomes "both registries hold exactly one `'B'` and one `'D'` entry for
`ItemHelper.EntryId`". Companions: a collapse-direction test asserting both registries end empty, and
an idempotence test asserting two consecutive `On` calls do not throw. A cheaper complementary test
drives `SyncExpandedRegistrations` directly through
`QfcItemControllerTestSupport.InvokeNonPublic`, asserting the both-registries invariant for `true` and
`false` without needing `_tlpStates` or `_parent`; both the direct test (for helper coverage) and the
end-to-end test (for regression proof) should exist.

### Coverage targets

- `SyncExpandedRegistrations` in `QfcItemController.Navigation.cs` is a **new member** and must reach
  `>= 90%` line coverage (`CLAUDE.md` §UT2). It must not carry `[ExcludeFromCodeCoverage]`.
- The new guard lines in `KbdActions.cs` must be covered on both the throwing and non-throwing
  branches. `CLAUDE.md` §UT2 names `KbdActions<>` explicitly as a testable seam that is **not**
  exempt.
- No coverage claim is made for `QfcCollectionController.cs` changes: the class carries
  `[ExcludeFromCodeCoverage]` at `:21`, so those lines are outside every denominator.
- Repo-wide line `>= 85%` and branch `>= 75%` (`.claude/rules/general-unit-test.md`,
  `.claude/rules/quality-tiers.md`) are **measured and reported** against a baseline captured at Phase
  0, with the binding condition being *no regression versus that baseline on the coverage figure and
  no regression on changed lines*. The repo-wide floor is not encoded as an independently blocking
  gate in this feature's checklist, because this feature has no baseline evidence that the floor is
  currently met repo-wide and a gate that a correct fix cannot satisfy would be unfalsifiable in the
  wrong direction. If the Phase 0 baseline shows the floor is already met, the executor records that
  fact and the no-regression condition enforces it.

### Toolchain commands

Run in this exact order, restarting from step 1 if any step fails or changes files
(`CLAUDE.md` § C# Toolchain, quoted verbatim):

1. `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`; always via `dotnet tool run`, never a global install)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Two operational notes on step 4, both required:

- The local invocation must **mirror CI's switches**:
  `/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`.
  Omitting `/InIsolation` produces assembly-load failures that present as empty-message, sub-millisecond
  test failures and are not real regressions.
- The assembly path set must **exclude any assembly whose path contains the relative segment
  `\.claude\`**, so build outputs inside concurrent sibling agent worktrees are not swept into this
  run. Without that exclusion the run picks up sibling worktrees' copies of the same test assemblies.

`/p:Nullable=enable` must **not** be added to step 3 (`CLAUDE.md` § C#1.3): no project in this
repository carries a `<Nullable>` element, so the property conscripts files that never opted in and
the gate cannot pass. Step 3 as written is character-for-character CI's command.

### Manual validation

Optional and not a gate. If an Outlook-attached session is available: expand an item with Right, press
Down, press Right on the original item again, and confirm `B`/`D` still function and no
`ArgumentException` appears in the log. `UNVERIFIED` whether such a session will be available; the
automated tests are the binding evidence.

---

## Acceptance Criteria

Every item below is a checkbox and names the test, file, or command that settles it. Per
`.claude/skills/acceptance-criteria-tracking/SKILL.md`, an item is checked off only after the work
satisfying it is implemented **and** verified, one item at a time, with the criterion text preserved
verbatim.

### Issue #444 — `KbdActions` enumerable constructor bypasses the duplicate guard

- [ ] **(Inherited from #468 — verify, do not re-perform.)** A repository-wide search of `*.cs` for the identifier `WireUpKeyboardHandler` returns zero hits, confirming that #468 `[P1-T2]` removed the method containing the duplicate `("Collection", Keys.Down)` registration and that this feature therefore did not need to resolve it. The command and its zero-hit output are recorded in the evidence dossier. The executor must **not** recreate the deleted block in order to remove it.
- [ ] **(Inherited from #468 — recorded, not implemented.)** The promoted #444 criterion "The duplicate registration in `QfcCollectionController.cs` is resolved to a single entry" is recorded in this spec as satisfied upstream by #468 `[P1-T2]` and decision `D2`, with that citation present in `## Repro & Evidence`, and the feature-audit reports it as inherited rather than delivered.
- [ ] The intended `Keys.Down` behaviour for the QuickFiler collection surface is decided and recorded in `## Proposed Fix` of this spec as `SelectNextItem()`, with its five supporting evidence citations present.
- [ ] `KbdActions(IEnumerable<UClass>)` in `QuickFiler/Controllers/KbdActions.cs` throws `ArgumentException` when the supplied sequence contains two or more elements sharing the same `SourceId` and a `StoredKeyEquals`-equal `Key`, and the thrown message contains the literal fragment `already exists`. Verified by the new duplicate test in `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` asserting `.WithMessage("*already exists*")`.
- [ ] The constructor guard compares using `KbdActions.StoredKeyEquals` and not `KeyEquals`. Verified by the new test in `QuickFiler.Test/Controllers/KbdActionsTests.cs` that constructs a `KbdActions<string, KaStringAsync, Func<string, Task>>` from a seed list containing `("Collection","10")` and `("Collection","1")` and asserts no exception is thrown.
- [ ] The pre-existing characterization test `KbdActionsTests.Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate` (`QuickFiler.Test/Controllers/KbdActionsTests.cs:13-29`) still passes unmodified.
- [ ] `new KbdActions<…>(null)` still throws `ArgumentNullException` and not `NullReferenceException`. Verified by a named null test in `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs`.
- [ ] A duplicate-free seed sequence, and a sequence repeating a `Key` under different `SourceId` values, both construct without throwing. Verified by two named negative tests in `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs`.
- [ ] The `KbdActions` constructor logs via the existing `logger.Error` immediately before throwing, matching the pattern at `QuickFiler/Controllers/KbdActions.cs:96` and `:117`. Verified by code review against those two line citations.
- [ ] `RegisterAsyncKeyActions` registers exactly one `("Collection", Keys.Down)` entry bound to `SelectNextItemAsync` and exactly one `("Collection", Keys.Up)` entry. Verified by the decision-pin test in `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`. This test is recorded in the fail-before dossier as pass-after-only with no red state expected.
- [ ] The duplicate-guard regression test was observed **failing before** the `KbdActions.cs` change and **passing after**, with both runs recorded in `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/`.

### Issue #472 — navigation register/unregister digit-width desync

- [ ] `QfcCollectionController` records the digit width used at registration in a new `private int _registeredDigits` field, assigned inside `RegisterNavigation` from the value that method already captures.
- [ ] `UnregisterNavigation` selects its key format from `_registeredDigits` and contains **zero** reads of the `Digits` property. Verified by a source search of `QuickFiler/Controllers/QfcCollectionController.cs` showing no occurrence of `Digits` within the `UnregisterNavigation` body.
- [ ] The format selection is written as `_registeredDigits == 2 ? "00" : ""` so that a controller built via `FormatterServices.GetUninitializedObject` (`_registeredDigits == 0`) behaves as single-digit. Verified by code review and by the four pre-existing navigation tests below continuing to pass.
- [ ] Registering at 10 items and unregistering at 9 leaves no orphaned `"0"`-prefixed navigation key other than the single `"10"` entry the loop bound cannot reach. Verified by the named width-fidelity test in the new file `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`.
- [ ] The width-fidelity test carries an XML documentation comment attributing the residual `"10"` entry to the separately-promoted count-mismatch defect and stating that it is out of this feature's scope.
- [ ] The mirror-direction test (register at 9 items with `_digits = 1`, grow to 10, unregister) asserts the same width-fidelity property and passes.
- [ ] The four pre-existing navigation tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (at `:409`, `:430`, `:452`, `:474` as of base commit `988e819b`) pass unmodified, and that file's `[TestMethod]` count is unchanged from its state at the branch head.
- [ ] `QuickFiler/Interfaces/IQfcCollectionController.cs` is not modified. Verified by `git status` / the branch diff showing the path absent.
- [ ] The #472 regression test was observed **failing before** the `QfcCollectionController.cs` change and **passing after**, with both runs recorded in `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/`.
- [ ] The unbracketed-removal count-mismatch defect described in `### Downstream notes` item 3 is promoted through the feature-promotion lifecycle into a new potential entry **and** a new GitHub issue, and the issue number is recorded in this feature's PR body. Prose in this folder alone does not satisfy this criterion.

### Issue #482 — expansion registry divergence

- [ ] A new `private void SyncExpandedRegistrations(bool expanded)` exists in `QuickFiler/Controllers/QfcItemController.Navigation.cs`, carries no `[ExcludeFromCodeCoverage]` attribute, and is the sole caller of the four expansion register/unregister methods from that file.
- [ ] Both `ToggleExpansion(Enums.ToggleState)` and `ToggleExpansionAsync(Enums.ToggleState)` delegate expansion registration to `SyncExpandedRegistrations(_expanded)`, called after `ToggleExpansionOn()` / `ToggleExpansionOff()` has written `_expanded`.
- [ ] Both `ToggleState` overloads retain their existing accessibility, `virtual` modifier, parameter list, return type, and `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` attribute. Verified by code review against `QuickFiler/Controllers/QfcItemController.Navigation.cs:173-174` and `:191-192` at base commit `988e819b`.
- [ ] The sequence `ToggleExpansionAsync(On)` → `ToggleExpansion(Off)` → `ToggleExpansionAsync(On)` completes without throwing `ArgumentException`. Verified by the named interleaving regression test in `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs`.
- [ ] After either `ToggleState` overload completes with `_expanded == true`, both `_kbdHandler.CharActions` and `_kbdHandler.CharActionsAsync` hold exactly one `'B'` and one `'D'` entry for `ItemHelper.EntryId`. Verified by the same interleaving test.
- [ ] After either `ToggleState` overload completes with `_expanded == false`, both registries hold zero `'B'` and zero `'D'` entries for `ItemHelper.EntryId`. Verified by the named collapse-direction test.
- [ ] Two consecutive `ToggleState.On` calls on the same overload do not throw. Verified by the named idempotence test.
- [ ] `SyncExpandedRegistrations` is exercised directly for both `true` and `false` through `QfcItemControllerTestSupport.InvokeNonPublic` by a named test, and reaches `>= 90%` line coverage as a new member.
- [ ] The #482 end-to-end test constructs no `System.Threading.Timer`: `ItemHelper.UnRead` is `false` in the arrangement, established explicitly rather than by relying on a default. Verified by inspection of the test arrangement and by the absence of any wall-clock wait in the test.
- [ ] The #482 regression test was observed **failing before** the `QfcItemController.Navigation.cs` change (as `ArgumentException` on the third step) and **passing after**, with both runs recorded in `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/`.
- [ ] The deliberate behaviour widening — `'B'`/`'D'` responding after a synchronous expansion and Alt+`B`/Alt+`D` after an asynchronous one — is stated in the PR body.
- [ ] The correction to #482's filed trigger and severity (the filed `QfcCollectionController.cs:1439` trigger is unreachable; the live trigger is Right → Down → Right; the exception is caught and logged at `KeyboardHandler.cs:141-147` so the symptom is a dead key, not a crash) is stated in this spec and repeated in the PR body, so the PR does not restate an unsupported claim.

### Upstream contract and scope discipline

- [ ] The `### Upstream contract (exhaustive) — required by features 464 and 489` section of this spec matches the delivered code exactly: every ADDED, CHANGED, and REMOVED member listed for `QuickFiler/Controllers/KbdActions.cs` and `QuickFiler/Controllers/QfcItemController.Navigation.cs` is present as described, with the stated accessibility and static-ness, and no member outside those tables changed. Verified by reviewing the branch diff for those two files against the tables.
- [ ] `QuickFiler/Controllers/KeyboardHandler.cs` is not modified.
- [ ] `QuickFiler/Interfaces/IQfcCollectionController.cs` is not modified.
- [ ] None of the following nine `QfcItemController` partials is modified: `QfcItemController.cs`, `QfcItemController.Conversation.cs`, `QfcItemController.EventHandlers.cs`, `QfcItemController.EventWiring.cs`, `QfcItemController.FocusAndTheme.cs`, `QfcItemController.FolderHandling.cs`, `QfcItemController.Initialization.cs`, `QfcItemController.MailActions.cs`, `QfcItemController.ViewerSetup.cs`. Verified by the branch diff file list containing none of these nine paths.
- [ ] The branch diff's production-file list is a subset of exactly three paths: `QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/Controllers/QfcItemController.Navigation.cs`, `QuickFiler/Controllers/QfcCollectionController.cs`. Verified by the branch diff file list.
- [ ] `KbdActions.Remove` retains its `bool` return and its silent `false` for an absent pair, and no `TryRemove`-style member is added. Verified by the branch diff for `QuickFiler/Controllers/KbdActions.cs`.
- [ ] No member is added to, removed from, or re-signed on any `public` type, so this feature contributes no public-API change. Verified by the branch diff for the three production files.
- [ ] Sibling #484's downstream note proposing a timer-factory seam at `QfcItemController.Navigation.cs:223-224` is explicitly declined in this spec, and no timer-factory seam appears in the delivered diff.
- [ ] Phase 0 re-derived every `QfcCollectionController.cs` and `QuickFiler.Test.csproj` anchor by member name or element text against the actual branch head, and no post-#468 line number was transcribed into the atomic plan. Verified by inspecting the plan for hard-coded line numbers in those two files.
- [ ] `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` passes: no `System.Windows.Forms.Form`-derived type was added to the test assembly.
- [ ] The new test file is registered in `QuickFiler.Test/QuickFiler.Test.csproj` by a single `<Compile Include>` line inserted between the `Controllers\QfcCollectionControllerTests.cs` and `Controllers\QfcCollectionControllerDarkModeTests.cs` entries, and no other line of that file changed. Verified by the branch diff for the `.csproj`.

### File-size, toolchain, and coverage

- [ ] No production or test file **added** by this feature exceeds **500 lines**, and every pre-existing file changed by this feature is either at or below **500 lines** or no larger than its Phase 0 baseline line count (`.claude/rules/general-code-change.md`). `QuickFiler/Controllers/QfcCollectionController.cs` exceeds the cap for pre-existing reasons recorded in the atomic plan decision `D-P6` and in the #468 spec; this feature neither creates nor is permitted to remediate that excess. Verified by a line count of every file in the branch diff against the Phase 0 baseline.
- [ ] `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is unchanged by this feature: its line count and its `[TestMethod]` count are identical to the branch head, and the path is absent from the branch diff.
- [ ] `dotnet tool run csharpier check .` reports zero unformatted files in the final toolchain pass.
- [ ] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` completes with zero errors and no new analyzer warnings in the final pass.
- [ ] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` completes with zero errors in the final pass, and `/p:Nullable=enable` was **not** added to that command.
- [ ] `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` completes with zero failed tests in the final pass, with every assembly path whose relative path contains `\.claude\` excluded from the run.
- [ ] All four toolchain steps passed in a single final pass with no step auto-fixing files, and the commands actually run are stated in the completion report.
- [ ] `SyncExpandedRegistrations` reaches `>= 90%` line coverage as a new member (`CLAUDE.md` §UT2). Verified from the coverage report produced by the final `vstest.console.exe` run.
- [ ] The new duplicate-guard branch in `QuickFiler/Controllers/KbdActions.cs` is covered on **both** the throwing and non-throwing paths. Verified from the same coverage report.
- [ ] A Phase 0 coverage baseline was captured and recorded in `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/`, and the final coverage figure shows **no regression** against that baseline in repo-wide line coverage, repo-wide branch coverage, or coverage of the lines this feature changed (`.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`: line `>= 85%`, branch `>= 75%`, no regression on changed lines).
- [ ] The coverage-policy conflict between `CLAUDE.md` §UT2 (`>= 80%` / `>= 90%`) and `.claude/rules/general-unit-test.md` plus `.claude/rules/quality-tiers.md` (`>= 85%` line / `>= 75%` branch) is recorded in the completion report as pre-existing and unresolved, not silently resolved.
- [ ] No acceptance condition in the atomic plan claims a coverage increase attributable to changes in `QuickFiler/Controllers/QfcCollectionController.cs`, which carries `[ExcludeFromCodeCoverage]` at `:21` and is therefore outside every coverage denominator (`.claude/rules/plan-acceptance-gates.md`).
- [ ] All evidence artifacts produced by this feature are written under `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/<kind>/` per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, and the working tree is clean at completion.

---

## Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| #468 has not landed on the branch, so `WireUpKeyboardHandler` still exists and the constructor guard breaks its construction site. | Low | High — the analyzer/type-check builds still pass but a runtime path begins throwing. | Phase 0 blocks on a zero-hit search for `WireUpKeyboardHandler`. If it is present, halt and report rather than proceeding. |
| Post-#468 line numbers are transcribed into the atomic plan and no longer resolve. | Medium | Medium — wasted execution cycles. | Every anchor is a member name; Phase 0 re-derives line numbers against the branch head. Encoded as an acceptance criterion. |
| `MailItemHelper.UnRead` defaults to `true`, so the #482 test constructs a 4000 ms timer and becomes flaky. | Medium | Medium | The arrangement sets `UnRead` explicitly rather than relying on the default; encoded as an acceptance criterion. |
| `QfcCollectionController.TestSupport.cs` (created by #468 `[P2-T1]`) is absent, so the #472 test cannot reuse its reflection helpers. | Medium | Low | Phase 0 confirms; the fallback is a private local reflection helper in the new test file. |
| `.csproj` `<Compile Include>` insertion conflicts with another epic sibling editing the same item group. | Medium | Low | Insertion is a single line in a slot deliberately chosen outside #468's block; the plan records its exact neighbours so the conflict is trivially resolvable. |
| The #482 behaviour widening is unwanted. | Low | Medium | The widening is stated in this spec and required in the PR body. If rejected at review, the fallback is the register-only-the-calling-path variant described in `## Proposed Fix`, which removes the throw without widening but preserves the registry inconsistency. |
| The #472 test's residual-`"10"` assertion is later read as endorsing the count-mismatch defect. | Low | Low | The assertion is written as an explicit at-most bound with an XML comment attributing the residual to the separately-promoted issue; both are acceptance criteria. |
| A concurrent sibling's worktree build output is swept into the local `vstest.console.exe` run, producing failures unrelated to this feature. | Medium | Low | The assembly path set excludes any path containing `\.claude\`, and `/InIsolation` is passed; both are acceptance criteria. |

---

## Rollout & Follow-up

**Rollout.** This feature merges into `epic/quickfiler-bug-family-integration`, not directly into
`main`. No feature flag, no configuration change, and no migration is required. No rollback procedure
beyond reverting the merge commit is needed: the change is confined to three production files and adds
no persisted state.

**Follow-up issues to promote during execution** (each through the feature-promotion lifecycle,
producing a real GitHub issue — prose in this folder disappears at merge):

1. **`UnregisterNavigation` count mismatch** — the loop is bounded by the current `_itemGroups.Count`
   while `RemoveSpecificControlGroup(int)` mutates `_itemGroups` unbracketed. Blocking acceptance
   criterion under `### Issue #472`.
2. **Mixed-mode toggle at `QfcCollectionController.cs:1439`** — `ActivateBySelectionAsync` calls the
   synchronous `ToggleExpansion()`; currently unreachable but a real design defect.
3. **`KbdActions.Remove`'s discarded `bool`** — 42 production call sites, 31 in the forbidden
   `QfcItemController.EventWiring.cs`. Scoped to be delivered in coordination with #484/#489.

**Post-merge verification.** Once the epic integration branch reaches `main`, confirm in an
Outlook-attached session that the Right → Down → Right sequence leaves `B`/`D` functional and produces
no `ArgumentException` in the log. This is confirmation, not a gate.

**Links.**

- Issue #444 — https://github.com/drmoisan/TaskMaster/issues/444
- Issue #472 — https://github.com/drmoisan/TaskMaster/issues/472
- Issue #482 — https://github.com/drmoisan/TaskMaster/issues/482
- Promoted requirement documents (on `origin/main`):
  `docs/features/potential/promoted/2026-08-07-kbdactions-enumerable-ctor-bypasses-duplicate-guard.md`,
  `docs/features/potential/promoted/2026-08-07-qfc-collection-navigation-digits-desync.md`,
  `docs/features/potential/promoted/2026-08-07-qfc-item-controller-expansion-registry-divergence.md`
- Research artifact:
  `docs/features/active/quickfiler-keyboard-action-defects-444/research/2026-08-24T20-45-quickfiler-keyboard-action-defects.md`
- Upstream dependency: issue #468 (`qfc-collection-controller-defects`), tasks `[P1-T2]`, `[P14-T2]`,
  decisions `D2`, `D12`, `D13`
