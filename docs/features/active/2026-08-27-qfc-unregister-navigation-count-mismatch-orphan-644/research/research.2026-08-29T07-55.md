# Research — Issue #644: `UnregisterNavigation` count mismatch orphans `KbdActions` registrations

- Issue: #644 (`qfc-unregister-navigation-count-mismatch-orphan`), work mode `full-bug`
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Worktree: `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a9e13727f905b003a`
- Read SHA: `ecdb1c84ba8541ab67042985919cfed4df768c01` (branch tip, read from
  `.git/worktrees/agent-a9e13727f905b003a/HEAD` -> `refs/heads/bug/qfc-unregister-navigation-count-mismatch-orphan-644`;
  identical to the `origin/main` SHA the branch was cut from, so no commit has landed on the branch yet).
  Every line number in this document was read at that SHA.
- Timestamp: 2026-08-29T07-55
- Tooling limitation: this session had no shell (`Bash` was not in the available tool set). All facts
  below come from file reads and content search. Nothing was executed; no command output is quoted as
  observed evidence anywhere in this document.

---

## 1. Current shape of the navigation register/unregister pair

`QfcCollectionController` is a single non-partial class in one file:
`QuickFiler/Controllers/QfcCollectionController.cs`, 2437 lines (last line read is 2437, `}`).
A `Glob` for `QuickFiler/Controllers/QfcCollectionController*.cs` returns exactly that one file, so
there are **no partial-class siblings**. The class carries `[ExcludeFromCodeCoverage]` at line 21.

### 1.1 `RegisterNavigation` (lines 1170-1182)

```csharp
public void RegisterNavigation()
{
    var digits = Digits;
    _registeredDigits = digits;
    if (_digitRefreshNeeded)
    {
        SetVisualDigits(digits);
    }
    for (int i = 0; i < _itemGroups.Count; i++)
    {
        RegisterNavigationAsyncAction(i, digits);
    }
}
```

- Loop bound: **live `_itemGroups.Count`**.
- Key construction is delegated to `RegisterNavigationAsyncAction(int, int)` (lines 1195-1198),
  which calls `_kbdHandler.StringActionsAsync.Add(GenerateStringKbdAction(itemIndex, digits))`.
- `GenerateStringKbdAction(int i, int digits)` (lines 1200-1222) builds the key as
  `(i + 1).ToString()` when `digits == 1`, `(i + 1).ToString("00")` when `digits == 2`, and leaves
  it `""` otherwise. `SourceId` is the literal `"Collection"`. The delegate is
  `(s) => ChangeByIndexAsync(int.Parse(s) - 1)`. `Update` and `ToggleControl` are `null`.
- `KaStringAsync`'s constructor and `Key` setter both apply `.ToLower()`
  (`QuickFiler/Controllers/KaStringAsync.cs:23`, `:40`), so the **stored** key may differ in
  principle from the constructor argument. For digit keys the transform is an identity, but a ledger
  that records `action.Key` after construction is exact by definition and a ledger that records the
  pre-construction string is exact only by that coincidence.

### 1.2 `UnregisterNavigation` (lines 1184-1193)

```csharp
public void UnregisterNavigation()
{
    // Issue #472: replay the recorded registration width; re-reading the live width property
    // would remove keys this page never registered. Non-2 means width 1, so a field of 0 does.
    var format = _registeredDigits == 2 ? "00" : "";
    for (int i = 0; i < _itemGroups.Count; i++)
    {
        _kbdHandler.StringActionsAsync.Remove("Collection", (i + 1).ToString(format));
    }
}
```

- Loop bound: **live `_itemGroups.Count`**. This is the #644 defect.
- The `bool` return of `Remove` is discarded.

### 1.3 Is #472's fix present in this checkout?

**Yes, it is present.** Direct evidence:

- `QuickFiler/Controllers/QfcCollectionController.cs:120-121` declares
  `// Issue #472: the width the last RegisterNavigation actually used, replayed at unregister.`
  followed by `private int _registeredDigits;`.
- Line 1173 assigns it inside `RegisterNavigation`; line 1188 is its only read.
- `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` exists (226 lines)
  and is registered in `QuickFiler.Test/QuickFiler.Test.csproj:131`.
- The #444/#472 feature folder `docs/features/active/quickfiler-keyboard-action-defects-444/`
  contains completed audit artifacts (`policy-audit.2026-08-27T20-34.md`,
  `feature-audit.2026-08-27T20-34.md`).

The issue text's phrasing "the `_registeredDigits` width field that #472 introduces" is therefore
past tense in this checkout: the field is already on `main` at `ecdb1c84`. Plan wording should say
"already present" rather than "will be introduced".

---

## 2. Every `_itemGroups` mutation path, and whether it is bracketed

`_itemGroups` is declared at line 297 (`private List<QfcItemGroup> _itemGroups;`) and exposed
read/write through the `ItemGroups` property at lines 299-304.

| Member | Line(s) | Mutation | Bracketed by `UnregisterNavigation`/`RegisterNavigation`? | Count divergence |
|---|---|---|---|---|
| `LoadItemGroupsAndViewers_02` | 671, 679 | reassign to new list, then `Add` per mail item | No, but it is a fresh-page build reached before `WireUpAsyncKeyboardHandler()` (lines 351, 458, 549), which then calls `RegisterNavigation()` (line 1123). | None in practice: no registration is live across the mutation. |
| `SwapItemGroups` | 737-745 | `ActivateQueuedItemGroups` reassigns `_itemGroups` (line 734) | **Yes** — `UnregisterNavigation()` at 739, `RegisterNavigation()` at 744 | None, provided unregister is total. Under the current count bound, unregister runs against the **outgoing** count, which is correct here. |
| `RemovedItemMonitor(string)` | 903-908 | delegates to `RemoveSpecificControlGroup(string)` | **Yes** — 905/907 | None. |
| `RemoveSpecificControlGroup(string)` | 910-915 | delegates to `RemoveSpecificControlGroup(int)` | **No bracket of its own** | Inherits its caller's bracketing. |
| **`RemoveSpecificControlGroup(int)`** | 962-1012 | `_itemGroups.RemoveAt(selection - 1)` at line 984 | **No** | **-1 per call.** This is the defect's origin. |
| `RemoveSpecificControlGroupAsync(int)` | 1016-1115 | `_itemGroups.RemoveAt(selection - 1)` at line 1049 | **Yes** — `UnregisterNavigation()` at 1021, `RegisterNavigation()` at 1104 (guarded by `swapAlreadyRegistered`, line 1102) | None. |
| `RemoveBelowThresholdAsync(double)` | 934-954 | calls `RemoveGroupByEntryId` (line 952) per below-threshold EntryID | **No** | **-N**, where N is the number of below-threshold groups. |
| `RemoveGroupByEntryId` (seam) | 924-931 | default delegate calls `RemoveSpecificControlGroup(entryID)` (line 929), i.e. the unbracketed `int` overload | **No** | Propagates the -1 per removal. |
| `PopOutControlGroup(int)` | 813-823 | calls `RemoveSpecificControlGroup(selection)` (line 819) | **No** | **-1.** This is a **third** unbracketed reach, not named in the issue. Its keyboard entry point is the `'P'` char action at `QfcItemController.EventWiring.cs:194-198`. |
| `PopOutControlGroupAsync(int)` | 825-838 | calls `RemoveSpecificControlGroupAsync` (line 833) | Yes (inside the async overload) | None. |
| `RemoveControls()` | 840-860 | `_itemGroups.Clear()` at line 854 | **No** | Clears to 0; a later `UnregisterNavigation()` would iterate zero times. Reached only from `Cleanup()` (line 2229), which then nulls `_itemGroups` (line 2238). |
| `RemoveControlsAsync()` | 881-901 | `_itemGroups.Clear()` at line 897 | **No** | Same; reached from `CleanupAsync()` (line 2215), which nulls `_itemGroups` at 2224. |
| `ToggleGroupConv` | 1625-1655 | `_itemGroups.RemoveAt(removalIndex)` at 1641, in a loop | **Yes** — 1630/1653 | None. |
| `ToggleUnGroupConv` | 1665-... | `_itemGroups.Insert(insertionIndex, grp)` at 1980 via `InsertItemGroups` | **Yes** — `UnregisterNavigation()` at 1674; `RegisterNavigation()` at 1690 (early-return path) and 1733 (normal path) | None. |
| `AddItemGroup(MailItem)` | 1888-1932 | `InsertItemGroups(index, 1)` at 1895 | **Yes** — 1890/1930 | None. |
| `Cleanup` / `CleanupAsync` | 2224, 2238 | `_itemGroups = null` | **No** | After this, `UnregisterNavigation` would throw `NullReferenceException` on `_itemGroups.Count`. |

**Summary of the divergence.** Three unbracketed reaches into `RemoveSpecificControlGroup(int)`
exist: `RemoveBelowThresholdAsync` -> `RemoveGroupByEntryId` -> `RemoveSpecificControlGroup(string)`
-> `RemoveSpecificControlGroup(int)`; the `'R'` char action (section 3); and
`PopOutControlGroup(int)`. Each drops `_itemGroups.Count` by one while the `KbdActions` registry
still holds the key set registered at the higher count. The next `UnregisterNavigation` then removes
only the first `Count` keys, orphaning the tail.

Note the orphan is always a **tail** orphan under width 1 (keys `"1".."N"`, so removing the first
`N-k` leaves `"N-k+1".."N"`), and under width 2 the residual is exactly what
`QfcCollectionControllerNavigationDigitsTests.cs:184` pins as `{"10"}`.

---

## 3. The second unbracketed reach — the `'R'` char action

`QuickFiler/Controllers/QfcItemController.EventWiring.cs:199-203`, inside
`RegisterFocusActions` (the synchronous focus-action registration):

```csharp
_kbdHandler.CharActions.Add(
    ItemHelper.EntryId,
    'R',
    (x) => this._parent.RemoveSpecificControlGroup(ItemNumber)
);
```

Trace: keypress -> `KbdActions<char, KaChar, Action<char>>` lookup on `_kbdHandler.CharActions` with
`SourceId == ItemHelper.EntryId` -> the lambda -> `IQfcCollectionController.RemoveSpecificControlGroup(int)`
(declared at `QuickFiler/Interfaces/IQfcCollectionController.cs:48`) -> the unbracketed body at
`QfcCollectionController.cs:962`. No `UnregisterNavigation`/`RegisterNavigation` anywhere on that
path.

Two corrections to related claims:

- The **async** `'R'` action at `QfcItemController.EventWiring.cs:247-251` is **Reply**, not remove.
  The async remove is bound to `'Z'` at lines 286-291 and routes to
  `_parent.RemoveSpecificControlGroupAsync`, which **is** bracketed. So only the synchronous `'R'`
  path is defective.
- The teardown at `QfcItemController.EventWiring.cs:348` (`CharActions.Remove(ItemHelper.EntryId, 'R')`)
  removes the item-scoped `'R'` binding; it has no effect on the `"Collection"`-sourced navigation
  keys, which live in a different registry (`StringActionsAsync`) with a different `SourceId`.

---

## 4. `KbdActions` contract

File: `QuickFiler/Controllers/KbdActions.cs` (183 lines).

Type: `public class KbdActions<TKey, UClass, VDelegate> : IEnumerable<UClass> where UClass : IKbdAction<TKey, VDelegate>, new()`.
Backing store: `private List<UClass> _list` (line 67). Identity comparison for storage is
`private static bool StoredKeyEquals(TKey left, TKey right) => EqualityComparer<TKey>.Default.Equals(left, right);`
(lines 69-70).

### 4.1 Registration entry shape

An entry is a `UClass : IKbdAction<TKey, VDelegate>` carrying `SourceId` (string), `Key` (`TKey`) and
`Delegate` (`VDelegate`). For navigation the concrete type is `KaStringAsync`
(`IKbdAction<string, Func<string, Task>>`), with `SourceId = "Collection"`, `Key` the lower-cased
digit string, and `Delegate = (s) => ChangeByIndexAsync(int.Parse(s) - 1)`.

The registry the navigation keys live in is
`IQfcKeyboardHandler.StringActionsAsync` of type
`KbdActions<string, KaStringAsync, Func<string, Task>>`
(`QuickFiler/Interfaces/IQfcKeyboardHandler.cs:26`).

### 4.2 `Add`

Two overloads:

- `public void Add(string sourceId, TKey key, VDelegate @delegate)` (lines 126-140)
- `public void Add(UClass instance)` (lines 142-157)

Both scan `_list.Any(x => x.SourceId == ... && StoredKeyEquals(x.Key, ...))` and, on a hit, log and
throw **`ArgumentException`** with message
`$"Cannot add key because it already exists. Key {key} SourceId {sourceId}"`. The instance overload
passes `nameof(instance)` as `paramName`; the three-argument overload passes none.

**Navigation registration uses the `Add(UClass instance)` overload** — `RegisterNavigationAsyncAction`
(line 1197) calls `Add(GenerateStringKbdAction(itemIndex, digits))`. This matters for a ledger
design: the exact stored key is `KaStringAsync.Key` on the constructed instance, available to the
caller before the `Add` call.

The seeding constructor `KbdActions(IEnumerable<UClass> list)` (lines 42-65) enforces the same
uniqueness invariant with the same `ArgumentException`.

### 4.3 `Remove`

```csharp
public bool Remove(string sourceId, TKey key)
{
    var index = _list.FindIndex(x => x.SourceId == sourceId && StoredKeyEquals(x.Key, key));
    if (index == -1) { return false; }
    else { _list.RemoveAt(index); return true; }
}
```
(lines 159-171)

**Confirmed: `Remove` returns `bool`** and is total-by-`(SourceId, Key)` exact match, with no
substring semantics (it uses `StoredKeyEquals`, not `KeyEquals`). It removes at most one element per
call.

**Call-site audit (corrects the issue's "42 / 31" figures).** Searching `*.cs` under the worktree for
`(CharActions|CharActionsAsync|KeyActions|KeyActionsAsync|StringActions|StringActionsAsync|AlwaysOnKeyActionsAsync)\.Remove\(`
returns 41 raw occurrences across 4 production files: `QfcItemController.EventWiring.cs` 33,
`EfcItemController.cs` 5, `EfcFormController.cs` 2, `QfcCollectionController.cs` 1. Two of the
`EventWiring` hits (lines 359, 360) are commented out, so the live count is **39 production call
sites, 31 of them in `QfcItemController.EventWiring.cs`**. The "31 in EventWiring" figure in the
issue is exact; the "42" total is 3 high against this checkout. There are **zero** test-code call
sites matched by that pattern.

Every one of the 39 discards the `bool`. Two of them are inside `Action<T>` lambdas passed to
`ForEach` (`EfcFormController.cs:1019-1021` and `1028-1030`), where the discard is structural rather
than a bare expression statement. **The issue's claim that all production call sites discard the
result is confirmed.**

### 4.4 `Find` and the collision exception

```csharp
public UClass Find(TKey key)
{
    var matches = _list.Where(x => x.KeyEquals(key));
    var count = matches.Count();
    switch (count)
    {
        case 0:  return default(UClass);
        case 1:  return matches.First();
        default: /* build message */ throw new InvalidOperationException(message);
    }
}
```
(lines 89-105)

`FindIndex` (107-124) has the identical shape and also throws `InvalidOperationException` on
multi-match. `ContainsKey` (85) and `FilterKeys` (87) do not throw on multi-match.

Note the asymmetry that makes the orphan dangerous: `Find`/`FindIndex`/`ContainsKey`/`this[key]` use
the element-defined **`KeyEquals`**, which for `KaStringAsync` is a **substring** test
(`Key.Contains(other)`, `KaStringAsync.cs:125`), whereas `Add`/`Remove` use exact `StoredKeyEquals`.
An orphaned `"10"` therefore collides with a probe of `"1"` under `Find` even though `Remove("Collection","1")`
would never have removed it.

**Exception summary for the orphan-collision case:**
- duplicate `Add` -> **`ArgumentException`** ("Cannot add key because it already exists. Key {k} SourceId {s}")
- ambiguous `Find` / `FindIndex` / indexer -> **`InvalidOperationException`** ("Multiple sources have registered actions for Key {k}. SourceId list [...]")

Both match the issue's statement.

---

## 5. Scope boundary against #472

#472's landed change is exactly two artifacts inside `QfcCollectionController.cs`:

1. the field `_registeredDigits` (line 121) with its assignment in `RegisterNavigation` (line 1173);
2. the `var format = _registeredDigits == 2 ? "00" : "";` expression in `UnregisterNavigation`
   (line 1188), which is the field's **only reader**.

The boundary is therefore mechanical and unambiguous:

- **#472 owns the `format` argument** passed to `(i + 1).ToString(format)` — the *shape* of each key.
- **#644 owns the `for (int i = 0; i < _itemGroups.Count; i++)` header** — the *cardinality* of the
  key set.

A key ledger replaces the whole expression `(i + 1).ToString(format)` **and** the loop header with a
replay of recorded strings. That makes both #472 artifacts unreachable state.

### 5.1 Minimum-scope treatment of the #472 artifact (this is a hard constraint on the plan)

Deleting the `format` line while leaving `_registeredDigits` in place is **not** a valid minimum:
the field would then be assigned at line 1173 and never read, which the C# compiler reports as
**CS0414** ("The private field '...' is assigned but its value is never used"). The repository's
type-check gate is
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`,
which promotes CS0414 to an error. `.editorconfig:27` sets `dotnet_analyzer_diagnostic.severity = suggestion`
as a catch-all, which covers analyzer (`IDE`/`MA`/`S`) rules but **not** compiler `CSxxxx`
diagnostics, so it provides no cover here.

**Therefore the minimum-scope treatment is: delete both the `_registeredDigits` field declaration
(with its `// Issue #472:` comment) and its assignment in `RegisterNavigation`, in the same commit
that removes the `format` expression.** This is a *supersession*, not a revert or a re-litigation of
#472: #472's guarantee — "unregistration removes keys in the width they were registered at" — is
strictly strengthened by the ledger, which removes the keys *verbatim* rather than reconstructing
them from a width. No #472 behaviour is lost, and no #472 test is weakened (see 6(c)). The plan must
not reopen, revert, or re-argue #472; it should cite this paragraph as the supersession record.

---

## 6. Test-surface constraint

### 6(a) Tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` that change outcome

The file is **exactly 500 lines** (last line read is 500, `}`) and contains **13 `[TestMethod]`
attributes** (lines 82, 110, 185, 201, 217, 233, 250, 273, 302, 408, 429, 451, 473).

Four of the 13 exercise navigation registration. The critical mechanism is the helper
`SeedCollectionKey` (lines 385-389), which injects `"Collection"`-sourced keys **directly into the
`KbdActions` registry without going through `RegisterNavigation`**. Under a ledger, keys seeded that
way are not in the ledger and are therefore not removed by `UnregisterNavigation`.

| Test | Line | Behaviour today | Behaviour under a key ledger |
|---|---|---|---|
| `LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix` | 409 | Seeds `"1"`,`"2"`; outgoing count 1 so unregister removes `"1"`; register of the 2-item incoming page collides on `"2"` -> asserts `WithMessage("*Key 2 SourceId Collection*")` | Ledger empty -> unregister removes nothing -> the collision is on `"1"`, message is `Key 1`. **Assertion fails.** |
| `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys` | 430 | Seeds `"1"`,`"2"`; outgoing count 2 so unregister removes both; asserts the post-state counts | Ledger empty -> unregister removes nothing -> registering the 1-item incoming page throws `ArgumentException` on `"1"`. **Test fails (unexpected throw).** |
| `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` | 452 | No seeding; two `RegisterNavigation()` calls collide | Unchanged — still throws `ArgumentException` with `*SourceId Collection*`. **No outcome change.** |
| `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey` | 474 | Seeds `"1"`; unregister removes it; `RemoveAt(0)`; swap in a 2-item page succeeds | Ledger empty -> `"1"` survives -> the swap's register collides on `"1"`. `act.Should().NotThrow()` **fails.** |

So **3 of the 13** change outcome. The remediation is small and does not add or remove a
`[TestMethod]`: replace the out-of-band `SeedCollectionKey(...)` arrangement with a
`controller.RegisterNavigation()` call, which is what production does and which populates the ledger.
Worked through:

- Line 409 test: `controller.RegisterNavigation();` (1-item outgoing page registers `"1"` and ledgers
  it) plus the surviving `SeedCollectionKey(kbd, "2");` to model the pre-existing orphan. Unregister
  then removes `"1"` from the ledger, register adds `"1"` then collides on `"2"` -> `Key 2` message
  preserved, and the test's #232 meaning is preserved exactly.
- Line 430 test: replace both seeds with `controller.RegisterNavigation();` (2-item page ledgers
  `"1"`,`"2"`). All three assertions hold unchanged.
- Line 474 test: replace the single seed with `controller.RegisterNavigation();`. All assertions hold
  unchanged.

Net line delta: **-1 line at most** (two `SeedCollectionKey` calls collapse to one
`RegisterNavigation` call in the line-430 test; the other two are 1-for-1). The file stays at or
under 500 lines and keeps 13 `[TestMethod]` attributes.

`SeedCollectionKey` itself remains used (line-409 test), so it does not become dead.

### 6(b) Is the #468 freeze still in force?

- The 500-line ceiling is repository policy (`CLAUDE.md` General Code Change Policy section 4.1 and
  `.claude/rules/general-code-change.md`, "File Size Limit"). It applies unconditionally and is
  independent of #468. The file is at exactly 500, so **it may not grow by even one line**.
- The `[TestMethod]`-count pin is attested by
  `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t11-frozen-test-file.2026-08-27T09-45.md`,
  which records "500 lines / 13 `[TestMethod]` occurrences" and states: "upstream #468 decision `D12`
  / task `[P4-T5]` pins its `[TestMethod]` count".
- **Verification limit, stated explicitly:** I could not verify decision `D12` at first hand. A
  `Glob` for `docs/features/**/*468*` returns only two files — the #444 evidence artifact
  `p0-t12-upstream-468-verification.2026-08-27T09-45.md` and
  `docs/features/potential/promoted/2026-08-26-issue-468-residual-reflective-caller-risk.md`. There
  is no `docs/features/active/...-468/` folder in this checkout, so the #468 decision log itself is
  not present and the freeze is known here only second-hand through the #444 artifact. I also could
  not query GitHub (no shell, no `gh`).
- Practical conclusion for planning: treat the count pin as **in force**. The remediation in 6(a)
  satisfies it regardless, because it changes assertion arrangement only and leaves the count at 13
  and the line count at or below 500. This is the same posture #444 took (it added its tests to a new
  file rather than touching this one).

### 6(c) `QfcCollectionControllerNavigationDigitsTests.cs` (226 lines)

The file has 3 `[TestMethod]`s (lines 67, 154, 198). The one that pins the #644 residual is
`UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys`
(line 154). Its XML doc (lines 139-152) says verbatim:

> The single residual "10" entry is expected and is NOT this fix's scope. The loop is bounded by the
> current `_itemGroups.Count`, which is now nine, so the tenth key is never visited whatever the
> digit width. That count mismatch is the separately-promoted defect recorded in
> `### Downstream notes` item 3 of this feature's spec, and the assertion below is written as an
> explicit at-most bound so it cannot silently absorb it.

**The exact assertion that changes outcome** is lines 181-186:

```csharp
remaining
    .Should()
    .Equal(
        new[] { "10" },
        "only the key the shortened loop bound cannot reach survives, which is the separately-promoted count mismatch"
    );
```

Under a key ledger the ten-item registration ledgers `"01".."10"`, the unbracketed
`groups.RemoveAt(0)` does not touch the ledger, and unregistration removes all ten. `remaining` is
empty, so `Equal(new[] { "10" })` **fails**. This assertion must become `BeEmpty(...)` with a because
string naming #644, and the XML doc paragraph quoted above must be rewritten to record that the
residual is now closed. The sibling assertion at lines 175-180
(`.Where(k => k.StartsWith("0")).Should().BeEmpty(...)`) remains true and needs no change.

The second navigation test (line 198,
`..._AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys`) already asserts
`CollectionKeys(registry).Should().BeEmpty(...)` and **passes unchanged** under the ledger: the
ledger holds `"1".."9"`, all nine are removed, and the extra group added afterwards was never
registered. The first test (line 67) does not touch `StringActionsAsync` at all.

This file is 226 lines — 274 lines of headroom under the 500 ceiling — but it is #472's file. Editing
one assertion and its doc there is unavoidable (the assertion is a direct pin on the #644 residual);
adding #644's *new* coverage there would blur the #472/#644 boundary established in section 5.

### 6(d) Placement options for the new tests

Three options were considered.

| Option | Verdict |
|---|---|
| Append to `QfcCollectionControllerTests.cs` | **Rejected.** File is at exactly 500 lines and its `[TestMethod]` count is pinned (6(b)). Any addition violates both. |
| Append to `QfcCollectionControllerNavigationDigitsTests.cs` | **Rejected.** Headroom exists, but the file's `[TestClass]` doc declares it "covering issues #444 and #472" and "deliberately self-contained ... it introduces no cross-feature coupling". Putting #644 coverage there erases the scope line section 5 depends on. |
| **New file `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs`** | **Recommended.** Matches the established one-file-per-defect-family convention already visible in the directory (`...Defects468Tests.cs`, `...Defects468MoveTests.cs`, `...Defects468ConversationTests.cs`, `...NavigationDigitsTests.cs`). Satisfies the `tests/`-mirroring layout rule, since `QuickFiler.Test/Controllers/` mirrors `QuickFiler/Controllers/`. |

**The new file must be registered in `QuickFiler.Test/QuickFiler.Test.csproj`.** That project is
legacy non-SDK style with explicit `<Compile Include="..." />` items (the
`Controllers\QfcCollectionController*.cs` block is at lines 130-137); a new `.cs` file that is not
listed is silently not compiled.

**Existing shared fixture — `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs`**
(155 lines, `internal static class QfcCollectionControllerTestSupport`, registered at csproj line
133). It already provides:

- `SetField(QfcCollectionController, string, object)` — asserting non-public instance field setter
- `GetField(QfcCollectionController, string)` — asserting non-public instance field getter
  (this is what a ledger-state assertion would use if the ledger is a private field)
- `GetFieldInfo(string)` — for structural assertions on a field's declared type
- `SetStaticField` / `GetStaticField` — for `removespecificcontrolgroupcounter`
- `InvokeNonPublic(controller, name, params object[])` — non-public method invoker
- `CreateUninitializedController()` — `FormatterServices.GetUninitializedObject` allocation that
  injects `_digits = 1`, with a documented rationale (lines 130-144) explaining that leaving
  `_digits` at 0 routes `RegisterNavigation` into the WinForms-bound `SetVisualDigits` path

It does **not** provide an item-group builder or a `KbdActions`-wired keyboard-handler mock; both
`QfcCollectionControllerTests.cs` (lines 338-395) and `QfcCollectionControllerNavigationDigitsTests.cs`
(lines 47-137) carry private copies. The new file should either add a builder to `TestSupport.cs`
(preferred — it is the designated shared location and has ample headroom at 155 lines) or carry its
own, matching the digits file's self-contained precedent.

---

## 7. Testability seams

**A unit test can observe the `KbdActions` registry contents directly, with no live Outlook process,
no COM, no WinForms control, and no STA apartment.** The technique is already proven three times in
this test project:

1. Allocate the controller with `FormatterServices.GetUninitializedObject(typeof(QfcCollectionController))`
   to bypass the WinForms-dependent constructor (`QfcCollectionController.cs:30-53` dereferences
   `_formViewer.L1v0L2L3v_TableLayout`, `_formViewer.L1v0L2_PanelMain`, and
   `_globals.Ol.DarkMode`). Use `QfcCollectionControllerTestSupport.CreateUninitializedController()`.
2. Construct a **real, empty** `KbdActions<string, KaStringAsync, Func<string, Task>>` — it has a
   parameterless constructor (`KbdActions.cs:21-24`) and no dependencies — and return it from a
   `Mock<IQfcKeyboardHandler>(MockBehavior.Loose).SetupGet(x => x.StringActionsAsync)`. Precedents:
   `QfcCollectionControllerNavigationDigitsTests.cs:114-117`,
   `QfcCollectionControllerTests.cs:346-349`, `QfcCollectionControllerDefects468Tests.cs:205-210`.
3. Inject `_kbdHandler`, `_digits` and `_itemGroups` by reflection.
4. Assert on the registry by LINQ over `KbdActions<...>`, which implements `IEnumerable<UClass>`
   (`KbdActions.cs:173`). Existing shapes:
   `registry.Count(a => a.SourceId == "Collection" && a.Key == key)` and
   `registry.Where(a => a.SourceId == "Collection").Select(a => a.Key).ToArray()`
   (`QfcCollectionControllerNavigationDigitsTests.cs:128-137`).

**Item groups without COM.** `MakeGroups` in both files builds
`new QfcItemGroup { MailItem = new Mock<OutlookMailItem>(MockBehavior.Loose).Object }` with
`SetupGet(x => x.EntryID)`. `ItemController` and `ItemViewer` are left null; the navigation paths do
not dereference them **provided `_digits` matches the width the page needs**, because a mismatch sets
`_digitRefreshNeeded` and routes `RegisterNavigation` into `SetVisualDigits`, which dereferences
`grp.ItemController` / `grp.ItemViewer` (`QfcCollectionController.cs:146-163`). This constraint is
documented at `QfcCollectionControllerNavigationDigitsTests.cs:96-102` and must be honoured by any
new test.

For the `RemoveBelowThresholdAsync` scenario, groups need an `IQfcItemController` mock with
`TopFolderScore` set, injected into `QfcItemGroup` through its private `_itemController` field —
precedent at `QfcCollectionControllerTests.cs:150-165`.

**Reaching `RemoveSpecificControlGroup(int)` in a unit test is NOT possible** without WinForms: its
body calls `TableLayoutHelper.RemoveSpecificRow(_itemTlp, ...)` (line 978), `_moveMonitor.UnhookItem`
(981), `ResetPanelHeight()` (1007), and `_parent.ActionOkAsync()` (1010). The established substitute
is the **`_removeGroupByEntryId` seam** (`QfcCollectionController.cs:924-931`): tests inject a
recording delegate and assert on the selection logic
(`QfcCollectionControllerTests.cs:171-182`). For #644 the seam should be injected with a delegate
that performs **only** the list mutation (`itemGroups.RemoveAt(index)`), which models the unbracketed
mutation exactly and keeps the test host-free. A test that reproduces the count divergence through
`RemoveBelowThresholdAsync` + this seam is therefore fully deterministic and host-free.

**STA / WinForms handle:** **not required** for any test proposed here. The project does have STA
precedent if it were ever needed — `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` (csproj line
191) and `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs` (csproj line 137) —
but the ledger tests observe only in-memory list state and should not use it.

**Policy conformance of the proposed tests:** MSTest attributes, Moq for `IQfcKeyboardHandler` /
`MailItem` / `IQfcItemController`, FluentAssertions for every assertion; no temporary files; no
network, disk, or external process; no wall-clock wait; no mutable static state (the only static in
the class, `removespecificcontrolgroupcounter` at line 1014, is not touched by these paths, and
`QfcCollectionControllerDefects468Tests.cs:41-57` shows the reset protocol if it ever is).

---

## 8. Toolchain facts

Run in this exact order; restart from step 1 if any step fails or rewrites a file
(`CLAUDE.md`, "C# Toolchain (run in this exact order)" and CUT3).

1. Format (once per clone/worktree, first run only: `dotnet tool restore`)
   - apply: `dotnet tool run csharpier format .`
   - verify: `dotnet tool run csharpier check .`
   - CSharpier is pinned to 1.2.6 by `dotnet-tools.json`; always invoke through `dotnet tool run`.
2. Analyzers
   - `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   - `/t:Rebuild`, not `/t:Build`: a warm `/t:Build` skips `CoreCompile` and runs no analyzers.
3. Type check / nullable
   - `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
   - Do **not** add `/p:Nullable=enable`; do **not** use `/t:Build`. This is the gate that makes the
     CS0414 finding in section 5.1 load-bearing.
4. Test
   - `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
   - This mirrors `.github/workflows/_mstest-coverage.yml:83`, which runs
     `& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`.
     CI discovers `*.Test.dll` under `\bin\<Configuration>\`, excluding `\obj\` and `\ref\`
     (lines 70-76). `vstest.console.exe` is located via
     `vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`.
   - When running locally over a discovered set, exclude `\.claude\` worktree paths as well as
     `\obj\` and `\ref\`, or nested agent worktrees contribute duplicate assemblies.

**Build output location.** `QuickFiler.Test/QuickFiler.Test.csproj` sets
`<AssemblyName>QuickFiler.Test</AssemblyName>` (line 17) and `<OutputPath>bin\Debug\</OutputPath>`
for the Debug|AnyCPU configuration (line 36). The assembly is therefore
**`QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`**. Other configurations map to `bin\Release\`
(line 44), `bin\x86\Debug\` (line 51) and `bin\x86\Release\` (line 55).

**Coverage note.** `QfcCollectionController` carries `[ExcludeFromCodeCoverage]`
(`QfcCollectionController.cs:21`), so the production side of this fix contributes nothing to the
coverage denominator and cannot move the repository coverage figure. The new test file must still
exist and pass; the coverage gate is simply not the instrument that proves it.

---

## 9. Design options

### Option A (recommended) — a private key ledger field on `QfcCollectionController`

Add one lazily-initialised private field and rewrite the two loops.

```
private List<(string SourceId, string Key)> _registeredNavigationKeys;

private List<(string SourceId, string Key)> RegisteredNavigationKeys =>
    _registeredNavigationKeys ??= new List<(string, string)>();
```

- `RegisterNavigation`: after each successful `Add`, append the **stored** key. Because
  `RegisterNavigationAsyncAction` currently constructs and adds in one statement, the smallest
  faithful change is to have `RegisterNavigationAsyncAction` (or `RegisterNavigation`) hold the
  `KaStringAsync` instance, call `Add(instance)`, then append `(instance.SourceId, instance.Key)`.
  Appending *after* `Add` means a duplicate-key `ArgumentException` leaves the ledger unpolluted,
  which preserves `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException`
  (`QfcCollectionControllerTests.cs:452`) exactly.
- `UnregisterNavigation`: `foreach` the ledger calling `_kbdHandler.StringActionsAsync.Remove(src, key)`,
  then `Clear()` it. Delete the `format` expression, the `_registeredDigits` field and its assignment
  (section 5.1). Guard a null ledger with the lazy property so that reflection-allocated instances
  (`GetUninitializedObject` bypasses field initialisers) do not `NullReferenceException`.
- `_itemGroups` is not read by `UnregisterNavigation` at all after this change.

**Diff surface:** one production file (`QfcCollectionController.cs`), roughly `+8/-6` lines,
confined to lines 117-121 and 1170-1198. No interface change:
`IQfcCollectionController.UnregisterNavigation()` / `RegisterNavigation()` (interface lines 113-114)
keep their signatures, so `QfcFormControllerTests.cs:510-523`, which verifies the pair on a
`Mock<IQfcCollectionController>`, is untouched. No `.csproj` change on the production side.

**Risk:** low, and enumerable.
- *Null ledger on reflection-allocated instances.* Handled by the lazy property. Consequence: the
  `because:` string and XML doc of
  `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter`
  (`QfcCollectionControllerDefects468Tests.cs:152-186`) become inaccurate — it currently states that
  `UnregisterNavigation()` "dereferences the null `_itemGroups` field". Under Option A the
  `NullReferenceException` still occurs and still propagates, so the **test outcome is unchanged**,
  but it now originates two statements later at `_itemGroups[selection - 1]` (line 1024). The
  doc/because text must be corrected; the assertions must not.
- *Ledger vs. registry drift if a caller reassigns `_kbdHandler.StringActionsAsync` between register
  and unregister.* Searched: `StringActionsAsync` is assigned nowhere in production. `CharActions`
  and `CharActionsAsync` are reassigned in `LoadItemGroupsAndViewers_02` (lines 672-673), but those
  are different registries. So no drift path exists today.
- *Growth of a 2437-line file.* Already 4.9x over the 500-line ceiling before this change. Adding
  ~8 lines does not create the violation and splitting the file is a much larger refactor that
  `CLAUDE.md`'s Bugfix Workflow explicitly directs to a separate issue rather than into a bugfix's
  scope. Record it as a known pre-existing violation; do not widen scope.

**Interaction with each mutation path from section 2:** every unbracketed path
(`RemoveSpecificControlGroup(int)`, `RemoveBelowThresholdAsync`, the `'R'` char action,
`PopOutControlGroup`, `RemoveControls`, `Cleanup`) becomes harmless to unregistration, because the
ledger is not derived from `_itemGroups`. The bracketed paths are unaffected: each already calls
unregister before the mutation and register after it, so the ledger is drained and refilled exactly
as before. `Cleanup`/`CleanupAsync` nulling `_itemGroups` (lines 2224, 2238) additionally stops being
a latent `NullReferenceException` source for a post-cleanup `UnregisterNavigation`.

**Alignment with repository conventions:** the lazy-initialised private backing field with an
`??=` accessor is the idiom already used twice in this same file for the `_removeGroupByEntryId`
seam (lines 924-931) and the `_notifyNotReady` seam (line 178). The "unregister by replaying a
recorded set" shape is already the pattern in `EfcFormController.ToggleOffNavigation` /
`ToggleOffNavigationAsync` (`EfcFormController.cs:1017-1033`), which iterates
`CharacterActions.Keys` — a recorded catalogue — rather than a count. Option A brings
`QfcCollectionController` into line with the sibling controller.

### Option B — a small extracted `NavigationKeyLedger` type

A new file `QuickFiler/Controllers/NavigationKeyLedger.cs` holding the recorded pairs behind
`Record(string sourceId, string key)` / `DrainInto(KbdActions<string, KaStringAsync, Func<string, Task>> registry)`,
composed into `QfcCollectionController` as a readonly field.

- **Advantage:** unit-testable in isolation with no reflection at all; nudges the 2437-line file
  toward decomposition.
- **Limitations:** (i) `QuickFiler/QuickFiler.csproj` is legacy non-SDK style, so a new production
  `.cs` requires a `<Compile Include>` edit — a second production file in the diff; (ii) the ledger
  would need the registry passed in or a reference to `_kbdHandler`, reintroducing the coupling the
  extraction was meant to remove; (iii) the type is ~30 lines of behaviour wrapping a `List<T>`,
  which is thinner than the indirection costs; (iv) two files and one project file change instead of
  one file change, against a `CLAUDE.md` Bugfix Workflow that mandates the minimal targeted fix.

### Rejected alternatives (brief)

- **Bracket the three unbracketed paths.** Wrap `RemoveSpecificControlGroup(int)`,
  `PopOutControlGroup` and the `'R'` action in `UnregisterNavigation()`/`RegisterNavigation()`.
  Rejected: it fixes the three reaches known today and leaves the invariant "unregistration must be
  total" still expressed as a coincidence between two independently-computed counts, so the next
  unbracketed mutation reintroduces the defect. It also changes runtime behaviour on the removal path
  (a full re-registration per removal inside `RemoveBelowThresholdAsync`'s loop), which is a wider
  behavioural change than the ledger.
- **Ledger plus a retained count-bounded loop (belt and braces).** Would keep all four navigation
  tests in `QfcCollectionControllerTests.cs` green with no edits at all. Rejected: it leaves the
  defective count bound in the source as apparently-live code, forces `_registeredDigits` to be
  retained to feed the residual loop's format, and violates "simplicity first". The test edits it
  avoids are three arrangement lines.
- **Change `KbdActions.Remove` to remove all matches, or make `Remove` throw when it removes
  nothing.** Rejected: `KbdActions` is shared by every keyboard surface in QuickFiler and
  ExpandedFiler (39 production call sites, section 4.3), all of which discard the `bool`; changing
  its contract is a cross-cutting change the issue itself defers to `### Downstream notes` item 5 of
  the #444 spec. It also would not fix #644, because the un-visited tail keys are never passed to
  `Remove` at all.

**Recommendation: Option A.** It is the smallest seam that makes the invariant structural, it needs
no new file and no `.csproj` edit on the production side, it reuses two idioms already present in the
same file, and it is the design the issue's "Proposed Fix" names.

---

## 10. Behaviour semantics and requirements mapping

### Invariant

> After any `RegisterNavigation()` / `UnregisterNavigation()` pair, the `"Collection"`-sourced key set
> in `IQfcKeyboardHandler.StringActionsAsync` is exactly what it was before the `RegisterNavigation()`
> call, for every interleaving of `_itemGroups` mutations between the two.

### State model

| Ledger state | Meaning | Transition |
|---|---|---|
| `null` | instance allocated without running field initialisers (reflection-built test instance) | first access through the lazy accessor -> `empty` |
| `empty` | no navigation keys are registered by this controller | `RegisterNavigation()` with `_itemGroups.Count == n` -> `populated(n)` |
| `populated(n)` | exactly the `n` recorded `(SourceId, Key)` pairs are live in the registry | `UnregisterNavigation()` -> `empty`; a second `RegisterNavigation()` throws `ArgumentException` on the first key and leaves the state at `populated(n)` |

`_itemGroups` mutations do not appear in this table. That is the point of the fix.

### Success / failure conditions

- **Success:** after `UnregisterNavigation()`, `StringActionsAsync.Count(a => a.SourceId == "Collection")`
  equals its value immediately before the matching `RegisterNavigation()`, and the ledger is empty.
- **Failure (pre-fix):** the count is higher by the number of groups removed through an unbracketed
  path since registration.
- **Ordering rule:** registration must record only after a successful `Add`, so a partial
  registration cannot leave the ledger claiming keys the registry does not hold.
- **Edge cases:** empty page (`_itemGroups.Count == 0`) -> empty ledger, unregister is a no-op;
  `_itemGroups == null` after `Cleanup` -> unregister is a no-op instead of `NullReferenceException`;
  width crossing at 9/10 groups -> the ledger's recorded strings are authoritative and no width is
  recomputed at unregister time (this is what preserves #472's guarantee).

### Proposed test set for the new file

1. Register a 10-group page, remove one group through the `_removeGroupByEntryId` seam (modelling
   `RemoveBelowThresholdAsync`), unregister -> registry holds zero `"Collection"` entries. **This is
   the red-before-fix regression test**: today it leaves `"10"` behind.
2. Register a 5-group page, remove one group directly from the injected `_itemGroups` list
   (modelling the `'R'` char action's reach into `RemoveSpecificControlGroup(int)`), unregister,
   then `RegisterNavigation()` again -> no exception, and exactly one entry per key of the new page.
   Red before the fix with `ArgumentException`.
3. Register, unregister, register, unregister -> registry empty and no throw (idempotence /
   state-transition coverage).
4. Unregister without a prior register -> no throw, registry unchanged (negative / empty-ledger).
5. Register a 10-group page (width 2), shrink to 9 groups, unregister -> zero residual. This is the
   #644-side companion to the #472 width test and is the assertion that flips in 6(c).

---

## Automation Feasibility

**Fully automatable. No step requires human interaction and no step requires a live Outlook host.**

- The production change is confined to three regions of one existing `.cs` file (field block at
  117-121, `RegisterNavigation` at 1170-1182, `UnregisterNavigation` at 1184-1193) plus
  `RegisterNavigationAsyncAction` at 1195-1198.
- Every proposed test is host-free: `FormatterServices.GetUninitializedObject` bypasses the
  WinForms constructor, a real parameterless `KbdActions<string, KaStringAsync, Func<string, Task>>`
  provides the registry, `Mock<MailItem>` and `Mock<IQfcItemController>` cover the COM-typed members
  actually touched, and the `_removeGroupByEntryId` seam substitutes for the WinForms-bound removal
  body. All four techniques are already load-bearing in this test project.
- No STA apartment and no WinForms handle is needed (section 7). The tests must avoid
  `SetVisualDigits` by keeping `_digits` equal to the width the page needs; that is an arrangement
  constraint, not a host requirement.
- All four toolchain steps are non-interactive (section 8), and the vstest invocation carries
  `/TestCaseFilter:"TestCategory!=LiveOutlook"`, which excludes the only category that would demand a
  running Outlook process.
- The manual verification note in the issue ("bring up the QuickFiler collection surface...") is a
  *description of the user-visible symptom*, not a required validation step: the defect and its fix
  are both fully observable through the `KbdActions` registry in a unit test, which is exactly how
  #472 pinned the same code path.

One caveat that is a **tooling**, not a **human**, dependency: this research session had no shell, so
the executor must be the first actor to run `git`, `dotnet`, `msbuild` and `vstest.console.exe`. The
line counts and `[TestMethod]` counts cited above were derived by reading files, not by running the
counting commands the #444 evidence artifacts used; the executor should re-derive them in Phase 0
before relying on the 500/13 figures as gate baselines.

---

## Files In Scope

`QuickFiler/Controllers/QfcCollectionController.cs`
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs`
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`
`QuickFiler.Test/QuickFiler.Test.csproj`
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md`

Notes on this list:

- `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` is **created**; all
  other paths are **modified**.
- `QuickFiler.Test/QuickFiler.Test.csproj` is required solely to add the `<Compile Include>` entry
  for the new test file (the project is legacy non-SDK style; see 6(d)).
- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` changes only in XML
  documentation and one `because:` string (lines 152-161 and 176-178); no assertion and no
  `[TestMethod]` changes, and its outcome is unaffected. It is listed because keeping comments
  synchronized with behaviour is a policy requirement (`CLAUDE.md` C#6.3).
- `QuickFiler/QuickFiler.csproj` is **not** in scope: the recommended design (Option A) adds no
  production file.
- `QuickFiler/Interfaces/IQfcCollectionController.cs` and
  `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` are **not** in scope: no signature changes.
- `QuickFiler/Controllers/KbdActions.cs` is **not** in scope: its contract is correct as written and
  the discarded-`bool` question is explicitly deferred to a separate issue.
- Timestamped plan, AC-tracking and evidence artifacts written under
  `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/` during
  execution are process outputs of the orchestration workflow, not part of the fix's code diff, and
  are deliberately not enumerated here.
