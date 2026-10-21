# Research: qfc-collection-controller-defects (primary issue #468)

- Date: 2026-08-24
- Base commit: `988e819b`
- Worktree: `<repo-root>/.claude/worktrees/agent-aae5cc929932e2647`
- Primary subject: `QuickFiler/Controllers/QfcCollectionController.cs` (2349 lines + 1 trailing newline)
- Issues in scope: #286, #468, #469, #470, #471, #473, #474

All `file:line` citations below were verified by reading the file at the base commit. Where a claim
could not be established from the source, that is stated explicitly.

---

## 0. Summary of material findings

Five findings materially change the plan relative to the promoted documents:

1. **Every line number in all seven promoted documents matches the current source exactly.** There
   has been no drift. All ~15 defects reproduce on this base.
2. **#474's premise that the two form-controller interfaces are unrelated is wrong on this base.**
   `QuickFiler.Controllers.IQfcFormController` **derives from** `QuickFiler.Interfaces.IFilerFormController`
   (`QuickFiler/Controllers/IQfcFormController.cs:13`). It is a strict superset, not a sibling. This
   makes the recommended fix a two-file change entirely inside the owned set.
3. **#469 defect 4 triages to REMOVE, not POPULATE. Undo-after-move is NOT broken.** The undo record
   is written by `EmailFiler.PushToUndoStack` (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:185-189`)
   onto `Globals.AF.MovedMails`, which is the **same object instance** the caller passes
   (`QuickFiler/Controllers/QfcFormController.cs:49`). Severity does not rise to High.
4. **#468's stated motivation (coverage denominator) does not hold on this base.** The class carries
   `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcCollectionController.cs:21`, so none of
   the dead lines are currently in any coverage denominator. Removal remains justified on
   dead-code grounds alone.
5. **#474 defect 2 cannot be fully fixed inside the owned file set.** The only consumer of
   `ReadyForMove` is `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:121`, which is **not**
   an owned file. A caller-presents-the-UI fix requires editing it. A behaviour-preserving split
   (pure predicate + thin UI wrapper) stays inside the owned file and is recommended instead.

Two additional defects not filed in any of the seven documents were found and are recorded in
section 1.8.

---

## 1. Re-verification of every defect against the current source

### Verdict table

| Issue | Defect | Current location | Reproduces? |
|---|---|---|---|
| #286 | Reentrancy counter leaks on exception | `QfcCollectionController.cs:1157`, `:1161`, `:1237-1242`, `:1247` | Yes |
| #468 | 12 dead members + 1 dead field | see §2 | Yes (all 13) |
| #469-1 | Null guard after dereference in `GetMoveDiagnostics` | `:2288`, `:2289`, `:2312`, `:2313`, `:2318-2322` | Yes |
| #469-2 | Trailing null element in returned array | `:2284`, `:2286`, `:2324` | Yes |
| #469-3 | Positional access into `ConcurrentDictionary` | `:71`, `:2264` | Yes |
| #469-4 | `MoveEmailsAsync` ignores `stackMovedItems` | `:2206-2228` | Yes (but see §3 — benign) |
| #470-1 | `_itemGroups[-1]` via `PromoteFirstChild` | `:1743`, `:1745`, `:1749`, `:1972`, `:1975` | Yes |
| #470-2 | Reservation/insertion count mismatch | `:1823`, `:1827-1830`, `:1883-1893` | Yes |
| #470-3 | `SetVisualDigits` unguarded then guarded | `:140` vs `:141-142` | Yes |
| #471 | `EliminateSpaceForItems` sign error | `:2017`, `:2020`, `:2025` | Yes |
| #473-1 | `BackgroundLoadingTasks` reference reset race | `:80`, `:398-399`, `:492-493` | Yes |
| #473-2 | Double log + swallowed cancellation | `:2236-2258` | Yes |
| #474-1 | Concrete downcast to `QfcFormController` | `:64`, `:1232` | Yes |
| #474-2 | `MessageBox` inside `ReadyForMove` getter | `:152-194`, `MessageBox` at `:186-191` | Yes |

**No defect failed to reproduce.** No line-number drift was observed in any of the seven documents.

### 1.1 #286 — reentrancy counter leak

- `private static int removespecificcontrolgroupcounter = 0;` — `:1157`
- `Interlocked.Increment(ref removespecificcontrolgroupcounter);` — `:1161` (first statement of
  `RemoveSpecificControlGroupAsync`, `:1159`)
- Unsynchronized read + `logger.Error` — `:1237-1242`
- `Interlocked.Decrement(...)` — `:1247` (last statement, method ends `:1248`)
- No `try`/`finally` anywhere between `:1161` and `:1247`.

Exception sources in the protected span that are not themselves guarded include
`_itemGroups[selection - 1]` (`:1165-1166`, `ArgumentOutOfRangeException` on a stale `selection`),
`UnregisterNavigation()` (`:1162`), `TableLayoutHelper.RemoveSpecificRow` (`:1183`), and the awaited
dispatcher lambda at `:1226-1236` which itself calls `((QfcFormController)_parent).SkipGroupAsync()`
(the #474 downcast). So the #474 defect is one concrete producer of the #286 leak.

The synchronous sibling `RemoveSpecificControlGroup(int)` (`:1105-1155`) does **not** touch the
counter, so it needs no change.

### 1.2 #469 defect 1 — unreachable null guard in `GetMoveDiagnostics`

```
2288:  var qf = TryGetItemGroupByIndex(k)?.ItemController;
2289:  var helper = qf.ItemHelper;                          // unguarded dereference
...
2312:  $"... {xComma(qf.ItemHelper.Subject)} ..."           // unguarded dereference
2313:  if (qf is not null)                                  // guard, too late
2318-2322:                                                  // dead else branch
```

Two distinct null sources reach `:2289`: `TryGetItemGroupByIndex` returns `null` on any exception
(`:2266-2269`), and `QfcItemGroup.ItemController` can itself be null (see #470 defect 2's placeholder
groups created at `:2008`). The null-conditional at `:2288` therefore correctly anticipates null; the
next line negates it.

### 1.3 #469 defect 2 — trailing null element

`new string[_itemGroupsToMove.Count + 1]` (`:2284`); loop bound `loopTo = _itemGroupsToMove.Count`
(`:2285`), `for (k = 0; k < loopTo; k++)` (`:2286`), assignment `strOutput[k] = dataLine` (`:2324`).
Index `Count` is never assigned.

Consumer behaviour verified:

- `QfcHomeController.Metrics.cs:75` → `FileIO2.WriteTextFile` → iterates every element
  (`UtilitiesCS/To Depricate/FileIO2.cs:41-47`) → `WriteUTF8` → `sw.WriteLine(null)`
  (`FileIO2.cs:99`). Result: a spurious **blank line** appended to the metrics CSV on every move.
  Non-throwing.
- `QfcHomeController.Metrics.cs:144` → `NonBlockingProducer(string[], CancellationToken)`
  (`Metrics.cs:190-199`) → iterates every element with no null filter → `_metrics.TryAdd(null, 20, ct)`
  (`Metrics.cs:211`). A `null` element enters the metrics `BlockingCollection<string>`.

Neither consumer filters nulls. The defect is a data-quality defect, not a crash.

### 1.4 #469 defect 3 — positional access into an unordered dictionary

`private ConcurrentDictionary<QfcItemGroup, int> _itemGroupsToMove;` (`:71`);
`return _itemGroupsToMove.ElementAt(index).Key;` (`:2264`). See §7 for the full member inventory.

### 1.5 #470 defect 1 — `_itemGroups[-1]`

`ToggleGroupConv(string)` at `:1733`. `indexOriginal` from `FindIndex` (`:1738-1740`) is `-1` when the
original message is gone; `:1743-1746` routes to `PromoteFirstChild(originalId, ref childCount)`.
`PromoteFirstChild` (`:1970`) does its own `FindIndex` at `:1972` and dereferences
`_itemGroups[indexOriginal].ItemViewer` at `:1975` with no guard. `ChangeConversationSilently(indexOriginal, true)`
at `:1749` → `_itemGroups[indexOriginal]` at `:1716` fails the same way when `PromoteFirstChild`
returns `-1`.

Note `PromoteFirstChild` is `public` (`:1970`) and is **not** declared on `IQfcCollectionController`
(verified against `QuickFiler/Interfaces/IQfcCollectionController.cs`), and its only caller is `:1745`.

### 1.6 #470 defect 3 / #469 defect 1 — identical shape

```
138:  _itemGroups.ForEach(grp =>
140:      grp.ItemController.ItemNumberDigits = digits;                 // unguarded
141:      grp.ItemViewer.LblItemNumber.Text =
142:          grp.ItemController?.ItemNumber.ToString(format) ?? 0.ToString(format);   // guarded
```

`SetVisualDigits` is live: called from `:1200`, `:1335`, `:1841`, `:1938`.

### 1.7 #471 — sign error

```
2017:  var heightChange = -(int)Math.Round(_template.Height * removalCount, 0);
2020:      _itemTlp.MinimumSize.Height - heightChange
2025:      _itemTlp.Size.Height - heightChange
```

Sibling `MakeSpaceForItems` (`:2029-2042`) computes a positive magnitude and adds (`:2033`).
`EliminateSpaceForItems` has exactly **one** call site solution-wide: `:1779` inside
`ToggleGroupConv(int, int)`. It is also declared on the interface at
`QuickFiler/Interfaces/IQfcCollectionController.cs:47`, so removing it is not an option; only the
sign is wrong.

### 1.8 Two additional defects found, not filed in any of the seven documents

**A. `EnumerateConversationMembers` never reads its `conversationCount` parameter.** Declared at
`:1875-1881`; the body (`:1883-1921`) reads `entryID`, `resolver`, `insertionIndex`, and `folderList`
only. This is the same dead-parameter shape as #469 defect 4 and it is the *direct cause* of #470
defect 2: the reservation count is passed in but discarded, so the method has no way to detect
disagreement. Fixing #470 defect 2 (§5) necessarily makes this parameter live.

**B. `WireUpKeyboardHandler` registers `Keys.Down` twice but does not throw.** `:1265-1272` builds a
`List<KaKey>` containing `Keys.Down` at both `:1269` and `:1270` and passes it to the
`KbdActions(IEnumerable<UClass>)` constructor (`QuickFiler/Controllers/KbdActions.cs:26-29`), which
performs **no duplicate check** — unlike `Add` (`KbdActions.cs:90-104`, `:106-121`), which throws
`ArgumentException`. So issue #444's duplicate registration is a **silent** duplicate, not an
exception, and it is dormant only because the member has no caller. See §2 and the downstream note
in §8.

---

## 2. #468 — dead-code re-verification (per-member)

Method: solution-wide `*.cs` reference search across all projects in `TaskMaster.sln`, including all
test projects. Non-`.cs` assets were not searched for reflective use; see the residual-risk note
below.

| Member | Line | Access | Callers (whole solution) | Caller locations | On an interface? |
|---|---|---|---|---|---|
| `WireUpKeyboardHandler` | `:1254` | **public** | 0 | — | No |
| `AnyOpenDropDownsAsync` | `:1324` | internal | 0 | — | No |
| `LoadGroups_02cAsync` | `:587` | **public** | 0 | — | No |
| `LoadGroups_02bAsync` | `:635` | **public** | 0 live | commented-out `:402` only | No |
| `LoadGroup_03bAsync` | `:654` | private | 1, itself dead | `:647` (inside `LoadGroups_02bAsync`) | No |
| `LoadConversationsAndFoldersAsync` | `:761` | **public** | 0 | — | No |
| `LoadItemGroup` | `:776` | internal | 1, itself dead | `:772` (inside `LoadConversationsAndFoldersAsync`) | No |
| `LoadSequentialAsync` | `:827` | **public** | 0 | — | No |
| `LoadGroupSequential` | `:842` | **public** | 1, itself dead | `:838` (inside `LoadSequentialAsync`) | No |
| `CacheTlpForMove` | `:865` | internal | 1, itself dead | `:872` (inside `SwapTlp`) | No |
| `SwapTlp` | `:870` | internal | 0 | — | No |
| `CaptureTlpTemplate` | `:1991` | internal | 0 | — | No |
| `_templateTlp` (field) | `:70` | private | written at `:1994`, `:1995` only (both inside `CaptureTlpTemplate`); never read | — | n/a |

### Answers to the explicit sub-questions

- **Which are `public`?** Six: `WireUpKeyboardHandler`, `LoadGroups_02cAsync`, `LoadGroups_02bAsync`,
  `LoadConversationsAndFoldersAsync`, `LoadSequentialAsync`, `LoadGroupSequential`. `QfcCollectionController`
  is itself `public` (`:22`), so these are technically public API of the `QuickFiler` assembly. The
  assembly has no external consumer in this repository (referenced only by `TaskMaster` and
  `QuickFiler.Test`), so the API break is contained.
- **Is any declared on `IQfcCollectionController`?** **No.** `QuickFiler/Interfaces/IQfcCollectionController.cs`
  (118 lines, read in full) declares none of the thirteen. The interface therefore needs **no edit**
  for #468.
- **Does removing `WireUpKeyboardHandler` require touching `KbdActions.cs`?** **No.** The member is a
  *caller* of `_kbdHandler.CharActions.Add(...)` (`:1259`) and a constructor of
  `KbdActions<Keys, KaKey, Action<Keys>>` (`:1265`). Deleting the calling member deletes zero lines in
  `QuickFiler/Controllers/KbdActions.cs`. The prohibition on writing that file is satisfied.

### Disambiguation notes

- The 27 out-of-file `LoadSequentialAsync` hits all belong to unrelated types
  (`TaskMaster/AppGlobals/ApplicationGlobals.cs:139`, `AppToDoObjects.cs:63`, `AppAutoFileObjects.cs:84`
  and their tests). None is a caller of `QfcCollectionController.LoadSequentialAsync`.
- `AnyOpenDropDowns` (non-async, `:1319`) is **live** — called at `:1309` from
  `CustomReturnKeyHandler`. Only the `Async` overload is dead. Do not delete both.
- `LoadItemGroupsAndViewers_02` (`:740`), `LoadConversationsAndFolders_04` (`:756`) and
  `LoadSequential_5` (`:798`) are **live** (called from `:287`, `:295`, `:758` respectively). Their
  name similarity to the dead cluster is a trap.
- `ActivateQueuedTlp` (`:859`) is live (called at `:259`); only its `SwapTlp` wrapper is dead.

### Coverage-denominator claim

The class carries `[ExcludeFromCodeCoverage]` at `:21`. None of the ~229 dead lines is currently in
any coverage denominator, so #468's stated cost is not being paid today. The removal is still
correct — unreachable production code is a maintenance and comprehension cost regardless — but the
issue body's rationale should not be repeated verbatim in the PR description.

### Residual risk

The search covered `*.cs` only. No XAML, `.resx`, or designer file in `QuickFiler` was searched for a
late-bound reference, and no `Type.GetMethod`/`Invoke` reflection search was performed against these
names. Evidence that would settle it: a repository-wide search of all non-`.cs` files for the twelve
identifiers, plus a search for `GetMethod("Load` / `InvokeMember` in the `QuickFiler` tree. Given the
members are ordinary instance methods on a controller with no serialization or data-binding surface,
the risk is judged low but is not zero.

---

## 3. #469 defect 4 triage — POPULATE or REMOVE?

### Recommendation: **REMOVE the parameter from the contract.** Undo-after-move is not broken.

### Trace

1. **Declaration.** `Task MoveEmailsAsync(SloStack<IMovedMailInfo> StackMovedItems);` —
   `QuickFiler/Interfaces/IQfcCollectionController.cs:50`.
2. **Implementation.** `QfcCollectionController.cs:2206-2228`. The parameter is named in the
   signature (`:2206`) and in one commented-out trace call (`:2208`). It is read **nowhere** in the
   body.
3. **Call site.** `await _groups.MoveEmailsAsync(_movedItems);` —
   `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225`, inside `BackGroundMoveAsync`
   (`:215-234`). This is the only call site solution-wide.
4. **What `_movedItems` is.** `_movedItems = _globals.AF.MovedMails;` —
   `QuickFiler/Controllers/QfcFormController.cs:49`; field declared at `:86`. It is an **alias of the
   global stack**, not a fresh per-move collection.
5. **Who populates the global stack.** `Globals.AF.MovedMails.Push(info);` —
   `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:188`, inside
   `PushToUndoStack(MailItem, MailItem)` (`:185-189`), called from `ProcessMailHelperAsync` (`:179`)
   on every successful move. A second producer exists at
   `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:1325` (the non-QuickFiler auto-file
   path).
6. **How the QuickFiler move reaches that producer.** `MoveEmailsAsync` → `TryMoveEmailByGroupIndexAsync`
   (`:2230-2234`) → `TryMoveEmailByGroupAsync` (`:2236`) → `group.ItemController.MoveMailAsync()`
   (`:2240`) → `QuickFiler/Controllers/QfcItemController.MailActions.cs:83-113`, which builds an
   `EmailFilerConfig` and enqueues an `EmailFiler` onto `_homeController.FilerQueue` (`:110-111`). The
   filer's `ProcessMailHelperAsync` then calls `PushToUndoStack`.
7. **Who reads the stack for undo.** `QuickFiler/Controllers/QfcFormController.Actions.cs:206-250`
   (`_movedItems[i].UndoMoveMessage(...)` at `:218`, `_movedItems.Pop(i)` at `:232`,
   `_movedItems.Serialize()` at `:250`), reached from `ButtonUndo_Click` →
   `UndoDialog` (`EventHandlers.cs:236-245`). A second reader is
   `TaskMaster/Ribbon/RibbonController.cs:230` (`SortEmail.UndoAsync(Globals.AF.MovedMails, Globals)`).

### Conclusion

The undo record is **not dropped**. It is written by the layer that actually performs the move, onto
the same `SloStack<IMovedMailInfo>` instance that the caller redundantly hands to `MoveEmailsAsync`.
The parameter is pure noise: it makes `MoveEmailsAsync` look responsible for undo bookkeeping it does
not and should not own.

**Severity stays Medium at most; it does not escalate to High.**

**Recommended action:** delete the parameter from `QuickFiler/Interfaces/IQfcCollectionController.cs:50`
and from `QfcCollectionController.cs:2206`, and update the single call site.

**Scope caveat.** The call site `QfcFormController.EventHandlers.cs:225` is **not in the owned file
set**. Removing the parameter is a source-breaking change that forces a one-token edit there
(`MoveEmailsAsync(_movedItems)` → `MoveEmailsAsync()`).

Two ways to stay inside the owned set:

- **(i) Preferred — keep the signature, document the contract.** Leave the parameter, add an XML doc
  comment on the interface member stating that the undo stack is populated by
  `EmailFiler.PushToUndoStack` and that the parameter is retained only for source compatibility, and
  add `_ = stackMovedItems;` or an argument-null guard so the parameter is genuinely consumed. This
  removes the "silently ignored" defect (the reader now knows why) at zero blast radius.
- **(ii) Full removal.** Accept the one-line edit to `QfcFormController.EventHandlers.cs`. This is
  outside the declared owned set and needs an explicit scope decision.

Recommend **(i)** for this feature and file **(ii)** as a follow-up, because (ii) buys only signature
tidiness while widening the owned file set for a bugfix branch. If the orchestrator elects (ii),
record the `EventHandlers.cs` edit as an explicit scope extension.

### Deferred observation (not a defect to fix here)

There is a genuine ordering gap: `MoveMailAsync` only *enqueues* the filer (`MailActions.cs:111`) and
returns `Task.CompletedTask` (`:112`); the push happens later on the queue's worker. So when
`BackGroundMoveAsync` proceeds to `WriteMetrics` (`EventHandlers.cs:228-231`) and
`CleanupBackground()` (`:233`), the undo entries for that batch may not yet exist. This does not
break undo (the entries land eventually and are serialized), but it is an unsynchronized handoff. It
is out of scope for all seven issues and should be filed separately if it is judged worth pursuing.

---

## 4. #474 defect 1 — the two-interface split

### 4.1 Correction to the promoted document

The document states the two interfaces are unrelated and that "neither is a superset of the other."
On this base that is **false**:

```
QuickFiler/Controllers/IQfcFormController.cs:13
    public interface IQfcFormController : IFilerFormController
```

`IQfcFormController` inherits `IFilerFormController`. Every `IQfcFormController` **is** an
`IFilerFormController`, so widening the field's static type is type-safe and requires no member
migration.

### 4.2 There are three types named `IQfcFormController` — disambiguate before editing

| Type | File | Compiled? | Relevant? |
|---|---|---|---|
| `QuickFiler.Controllers.IQfcFormController` | `QuickFiler/Controllers/IQfcFormController.cs:13` | Yes (`QuickFiler.csproj:301`) | **This is the one that matters** |
| `QuickFiler.Interfaces.IQfcFormController` | `QuickFiler/Interfaces/IQfcFormController.cs:7` | Yes (`QuickFiler.csproj:361`) | Orphan; no implementer; only referent is `QuickFiler/Interfaces/IQfcHomeController.cs:9` |
| `QuickFiler.Notes.IQfcFormController` | `QuickFiler/Notes/notes_interfaces.cs:13` | **No** (absent from `QuickFiler.csproj`) | Design notes; does not compile (declares fields in interfaces) |

Inside `QfcCollectionController.cs` (namespace `QuickFiler.Controllers`), an unqualified
`IQfcFormController` binds to the `QuickFiler.Controllers` one by same-namespace preference. No
`using` alias is needed. The orphan `QuickFiler.Interfaces.IQfcFormController` is a latent trap for
any future file placed in the `QuickFiler.Interfaces` namespace and is a follow-up candidate; do not
touch it here.

### 4.3 Member inventory

**`QuickFiler.Interfaces.IFilerFormController`** (`QuickFiler/Interfaces/IFilerFormController.cs:9-24`) — 12 members:
`ActionCancelAsync()`, `ActionOkAsync()`, `ButtonCancel_Click(object, EventArgs)`,
`ButtonOK_Click(object, EventArgs)`, `Cleanup()`, `MaximizeFormViewer()`, `MinimizeFormViewer()`,
`ToggleOffNavigation(bool)`, `ToggleOffNavigationAsync()`, `ToggleOnNavigation(bool)`,
`ToggleOnNavigationAsync()`, `IntPtr FormHandle { get; }`.

**`QuickFiler.Controllers.IQfcFormController`** (`QuickFiler/Controllers/IQfcFormController.cs:13-42`) —
12 inherited + 27 own:
properties `ActiveTheme`, `DarkMode`, `FormViewer`, `Groups`, `ItemsPerIteration`, `SpaceForEmail`,
`Token`, `TokenSource`; methods `ButtonSkip_Click`, `ButtonUndo_Click()`,
`ButtonUndo_Click(object, EventArgs)`, `CaptureItemSettings`, `LoadItems(IList<MailItem>)`,
`LoadItems(TableLayoutPanel, List<QfcItemGroup>)`, four `LoadItemsAsync` overloads,
`LoadItemsPerIteration`, `RegisterFormEventHandlers`, `RemoveTemplatesAndSetupTlp`, `SetupLightDark`,
**`SkipGroupAsync()` (`:38`)**, `SpnEmailPerLoad_ValueChanged`, `UnregisterFormEventHandlers`,
`Viewer_Activate`.

### 4.4 Implementers

- `IFilerFormController`:
  - `QfcFormController` — `QuickFiler/Controllers/QfcFormController.cs:19`
    (`internal partial class QfcFormController : IQfcFormController`, i.e. transitively)
  - `EfcFormController` — `QuickFiler/Controllers/EfcFormController.cs:28`
    (`internal class EfcFormController : IFilerFormController`) — **does not implement `SkipGroupAsync`**
    (verified: the only `SkipGroupAsync` declaration/definition sites solution-wide are
    `IQfcFormController.cs:38` and `QfcFormController.EventHandlers.cs:361`).
- `IQfcFormController` (Controllers): `QfcFormController` only.

### 4.5 Consumers

**`IFilerFormController`** — production:
`QfcCollectionController.cs:35` (ctor param), `:64` (field);
`QuickFiler/Viewers/QfcFormViewer.cs:31` (field), `:46` (`SetController` param);
`QuickFiler/Interfaces/IQfcFormViewer.cs:20` (`SetController` param);
`QuickFiler/Interfaces/IFilerHomeController.cs:31` (`FormController` property);
`QuickFiler/Controllers/QfcHomeController.cs:416` (property implementation);
`QuickFiler/Controllers/EfcHomeController.cs:364` (property implementation).

**`IFilerFormController`** — tests:
`QfcItemController.SeamFactoryTests.cs:81`; `QfcItemController.EventHandlersTests.cs:289`;
`QfcFormControllerTests.cs:31`, `:105`, `:106`, `:159`; `QfcExplorerControllerTests.cs:45`, `:74`;
**`QfcCollectionControllerDarkModeTests.cs:45`** (owned).

**`QuickFiler.Controllers.IQfcFormController`** — production:
`QuickFiler/Viewers/QfcFormViewerExpanded.cs:28`, `:31`;
`QuickFiler/Viewers/QfcFormViewerDark.cs:28`, `:31`;
`QuickFiler/Controllers/QfcHomeController.cs:208` (loader `Func<...>` return type), `:415` (field);
`QuickFiler/Controllers/QfcFormController.cs:53` (`Init()` return type).

**`QuickFiler.Controllers.IQfcFormController`** — tests (all `Mock<IQfcFormController>`):
`QfcHomeControllerTests.cs:136`, `:207`; `QfcHomeControllerRunAsyncTests.cs:131`, `:199`, `:262`;
`QfcHomeControllerRunAsyncHighConfidenceTests.cs:28`, `:62`, `:150`, `:344`, `:443`;
`QfcHomeControllerPropertyTests.cs:140`; `QfcHomeControllerMetricsTests.cs:125`, `:211`, `:302`;
`QfcHomeControllerIterationTests.cs:149`, `:223`, `:283`, `:329`, `:376`, `:411`;
`QfcHomeControllerIssue218Tests.cs:84`, `:115`, `:219`.

### 4.6 Option evaluation

Production construction of `QfcCollectionController` occurs at exactly three sites, all passing
`parent: this` from inside `QfcFormController`:
`QuickFiler/Controllers/QfcFormController.Actions.cs:54`, `:88`, `:144`. The fourth construction is
the owned test at `QfcCollectionControllerDarkModeTests.cs:50-59`.

| Option | Change | Files touched | Any file outside the owned set? |
|---|---|---|---|
| **(a) Add `SkipGroupAsync` to `IFilerFormController`** | 1 line added to the interface + a new implementation on `EfcFormController` | `IFilerFormController.cs` (owned), **`EfcFormController.cs` (NOT owned)** | **Yes** |
| **(b) Retype `_parent` to `IQfcFormController`** | change ctor param `:35` and field `:64`, delete the cast at `:1232`; change `Mock<IFilerFormController>` → `Mock<IQfcFormController>` at the owned test `:45` | `QfcCollectionController.cs` (owned), `QfcCollectionControllerDarkModeTests.cs` (owned) | **No** |
| **(c) Merge the two interfaces** | collapse `IFilerFormController` into `IQfcFormController` or vice versa | both interface files, `QfcFormViewer.cs`, `IQfcFormViewer.cs`, `IFilerHomeController.cs`, `QfcHomeController.cs`, `EfcHomeController.cs`, `EfcFormController.cs`, plus ~8 test files | **Yes, many** |

### 4.7 Recommendation: option (b)

Retype the constructor parameter and the `_parent` field from `IFilerFormController` to
`QuickFiler.Controllers.IQfcFormController` and delete the downcast at `:1232`.

Justification:

- Type-safe by construction: `IQfcFormController : IFilerFormController` (`IQfcFormController.cs:13`),
  so every member `QfcCollectionController` currently invokes on `_parent` (`ActionOkAsync()` at
  `:1153`, `:1270`, `:1312`) remains available.
- The three production call sites already pass a `QfcFormController`, which implements
  `IQfcFormController`. Zero production caller edits.
- It converts the runtime `InvalidCastException` into a compile-time constraint, which is strictly
  stronger.
- Blast radius is two files, both owned.
- It removes the current situation where a `public` type's method body casts to an `internal` type
  (`QfcFormController` is `internal partial class`, `QfcFormController.cs:19`).

Cost: the owned dark-mode test at `QfcCollectionControllerDarkModeTests.cs:45` currently supplies a
`Mock<IFilerFormController>` and must become `Mock<IQfcFormController>`. Because `IQfcFormController`
is a strict superset, Moq will happily create the wider mock and the test's existing setups are
unaffected.

Option (a) is rejected because it forces a stub `SkipGroupAsync` onto `EfcFormController`, a type for
which "skip to the next group" is meaningless — it would be a `NotImplementedException` or a no-op
introduced solely to satisfy the interface. Option (c) is rejected on blast radius; it is a genuine
follow-up candidate but is a refactor, not a bugfix.

---

## 5. #470 defect 2 — count reconciliation

### 5.1 How the two counts are produced

**Reservation side** (`ToggleUnGroupConv`, `:1808-1847`):

```
1823:  int insertCount = conversationCount - 1;
1827:  MakeSpaceForItems(insertionIndex, insertCount);      // reserves insertCount TLP rows
1829:  InsertItemGroups(insertionIndex, insertCount);       // inserts insertCount empty QfcItemGroups
1830:  RenumberGroups(insertionIndex + insertCount);
```

`conversationCount` originates at the sole caller,
`QuickFiler/Controllers/QfcItemController.MailActions.cs:44`:
`ConversationResolver.Count.SameFolder`.

`ConversationResolver.Count` is a lazily loaded `Pair<int>`
(`QuickFiler/Helper Classes/ConversationResolver.Loading.cs:265-271`) whose loader is:

```
Loading.cs:273-286   internal Pair<int> LoadCount()
Loading.cs:279           count.SameFolder = df.SameFolder.Rows.Count();
```

i.e. **a DataFrame row count**.

**Insertion side** (`EnumerateConversationMembers`, `:1875-1922`):

```
1883:  var insertions = resolver
1884:      .ConversationItems.SameFolder.Where(mailItem => mailItem.EntryID != entryID)
1885:      .OrderByDescending(mailItem => mailItem.SentOn)
1886:      .ToList();
1888:  Enumerable.Range(0, insertions.Count).ForEach(i => { ... _itemGroups[i + insertionIndex] ... });
```

`ConversationItems` is a separate lazily loaded `Pair<IList<MailItem>>`
(`Loading.cs:160-176`) whose loader is:

```
Loading.cs:178-183   internal Pair<IList<MailItem>> LoadConversationItems()
Loading.cs:180           var sameFolder = ConversationInfo.SameFolder.Select(itemInfo => itemInfo.Item).ToList();
```

and `ConversationInfo.SameFolder` is produced by a **runtime folder-name string comparison**:

```
Loading.cs:65-66     var convInfoSameFolder = convInfoExpanded
                         .Where(itemInfo => itemInfo.FolderName == ((Folder)_mailItem.Parent).Name)
```

### 5.2 Why they can disagree — four independent mechanisms

1. **The base-item filter is conditional.** `insertCount = conversationCount - 1` assumes the base
   message is one of the `SameFolder` members, so `.Where(EntryID != entryID)` (`:1884`) removes
   exactly one. If the base message is **not** in `ConversationItems.SameFolder` — which happens
   whenever the folder-name comparison at `Loading.cs:66` excludes it, or when its `Item` failed to
   resolve — the `Where` removes zero and `insertions.Count == conversationCount`, i.e. **one more**
   than reserved. This is the overwrite branch and it is the most likely mechanism.
2. **Different filter predicates.** `Count.SameFolder` counts rows of `Df.SameFolder`;
   `ConversationInfo.SameFolder` filters the *expanded* helper list by folder **name**. Two distinct
   folders sharing a display name (a common Outlook configuration across stores) are conflated by the
   name comparison but not necessarily by the DataFrame filter, in either direction.
3. **The single-item fallback path.** `LoadConversationInfo` (`Loading.cs:37-56`) returns a
   one-element list when `Count.Expanded <= 0`. In that state `ConversationItems.SameFolder.Count`
   is 1 regardless of `Count.SameFolder`.
4. **Time-of-check / time-of-use.** `Count.SameFolder` is read at the *call site*
   (`MailActions.cs:44`) and `ConversationItems.SameFolder` is read *inside*
   `EnumerateConversationMembers` (`:1883`). These are two separate lazy-property evaluations at
   different instants. `ConversationItems` is also assigned from a background task
   (`Loading.cs:194: await Task.Run(() => ConversationItems = LoadConversationItems(), token)`), so a
   background completion between the two reads changes the second value.

### 5.3 Consequences (both confirmed against source)

- **`insertions.Count > insertCount`:** `_itemGroups[i + insertionIndex]` (`:1893`) walks past the
  reserved slots and `InitializeGroup` (`:1849-1864`) re-initializes a group that already holds a
  different message — silently replacing its `ItemViewer`, `MailItem` and `ItemController`. No
  exception. This is the serious branch.
- **`insertions.Count < insertCount`:** the surplus placeholder `QfcItemGroup`s created at `:2008`
  keep a `null` `ItemController`. The next `RenumberGroups()` dereferences
  `_itemGroups[i].ItemController.ItemNumber` (`:2068`) and throws `NullReferenceException`. The same
  null also reaches `SetVisualDigits` (`:140`) — that is #470 defect 3's live trigger.

### 5.4 Proposed reconciliation

Do **not** clamp the loop. Reconcile before any mutation, and surface disagreement.

Restructure so `ToggleUnGroupConv` derives the reservation from the same list the insertion uses:

1. Move the member resolution out of `EnumerateConversationMembers` into a new pure helper on the
   controller, for example
   `internal static IReadOnlyList<MailItem> ResolveConversationInsertions(ConversationResolver resolver, string entryID)`,
   containing exactly the current `:1883-1886` expression. Being static and taking the resolver as a
   parameter, it is directly unit-testable with a mocked/handcrafted resolver.
2. In `ToggleUnGroupConv`, call it **once**, before `MakeSpaceForItems`:
   ```
   var insertions = ResolveConversationInsertions(resolver, entryID);
   ```
3. Compare against the caller-supplied reservation:
   ```
   int expected = conversationCount - 1;
   if (insertions.Count != expected) { /* see decision below */ }
   int insertCount = insertions.Count;   // single source of truth
   ```
4. Pass `insertions` into `EnumerateConversationMembers` instead of having it re-resolve, and make
   the previously-dead `conversationCount` parameter (§1.8 finding A) either consumed by the
   reconciliation or removed from that method's signature.

### 5.5 What should happen on disagreement — recommendation

**Log at `Warn` with full context, then proceed using `insertions.Count`. Do not throw.**

Rationale:

- The repository's general code-change policy prefers failing fast, but this method sits directly on
  a UI event path (`QfcItemController.EnumerateConversation` → `EnumerateConversationAsync`,
  `MailActions.cs:36-52`, invoked from a context-menu action). An exception here propagates to the
  VSTO UI thread. The repository has already made exactly this trade once, in
  `ConversationResolver.Loading.cs:41-50`, where the comment reads: *"Throwing here propagated an
  unhandled exception to the VSTO UI thread for a recoverable scenario."* Following that established
  precedent is the consistent choice.
- The disagreement is a *resolver* fidelity problem, not a controller invariant violation. The
  controller can still do the correct thing — insert exactly the members that exist — once it stops
  trusting the stale count.
- Deriving `insertCount` from `insertions.Count` makes both failure branches in §5.3 **structurally
  impossible**, so the log is diagnostic rather than load-bearing.

The log message should carry `entryID`, `conversationCount`, `insertions.Count`,
`resolver.Count.SameFolder`, `resolver.Count.Expanded`, and `baseEmailIndex`, because those are the
values needed to decide later whether the resolver or the DataFrame filter is at fault.

Re-reserving (calling `MakeSpaceForItems` a second time for the delta) is rejected: it doubles the
TLP mutation surface for no benefit once the counts are derived from one source.

**Guard also needed:** `baseEmailIndex` from `FindIndex` (`:1819-1821`) can be `-1`, making
`insertionIndex == 0` and `_itemGroups[insertionIndex - 1]` at `:1900-1902` throw
`ArgumentOutOfRangeException`. This is the #470-defect-1 shape appearing a second time in the same
method and should be guarded in the same change.

---

## 6. #473 defect 1 — `BackgroundLoadingTasks` reset

### 6.1 Complete site inventory (solution-wide `*.cs` search)

| Site | Line | Kind |
|---|---|---|
| `internal ConcurrentBag<Task> BackgroundLoadingTasks = [];` | `:80` | declaration |
| `BackgroundLoadingTasks.Add(Task.Run(() => items.ForEach(... HookItem ...)))` | `:361-367` | Add (in `LoadControlsAndHandlers_01Async(IList<MailItem>, …)`) |
| `BackgroundLoadingTasks.Add(Task.Run(CreateEmptyKbdHandlerCharActions, Token));` | `:370` | Add (same method) |
| `await Task.WhenAll(BackgroundLoadingTasks);` | `:398` | await |
| `BackgroundLoadingTasks = [];` | `:399` | **reference reset** |
| `BackgroundLoadingTasks.Add(Task.Run(() => items.ForEach(... HookItem ...)))` | `:448-454` | Add (in `LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, …)`) |
| `BackgroundLoadingTasks.Add(Task.Run(CreateEmptyKbdHandlerCharActions, Token));` | `:457` | Add (same method) |
| `await Task.WhenAll(BackgroundLoadingTasks);` | `:492` | await |
| `BackgroundLoadingTasks = [];` | `:493` | **reference reset** |

**The document's claim is confirmed: there is no consumer outside `QfcCollectionController.cs`.** The
field is `internal`, is not declared on any interface, and does not appear in any test file. It is
not read by `QuickFiler.Test` despite that project's other reflection-based field access.

### 6.2 Defect mechanics

`Task.WhenAll(BackgroundLoadingTasks)` (`:398`) enumerates a **snapshot** of the bag at the moment
the overload's `IEnumerable<Task>` is materialized. The subsequent `BackgroundLoadingTasks = [];`
(`:399`) replaces the field with a new bag. Any `Add` executing between those two statements lands in
the old bag, which is then unreferenced and collected. That task is never awaited; if it faults, the
exception becomes an unobserved `TaskScheduler.UnobservedTaskException`.

On the current base, whether the window is actually reachable is **unproven**: both `Add` pairs occur
in the same method body strictly before the `WhenAll`, and no other member adds to the bag. A
concurrent second invocation of either `LoadControlsAndHandlers_01Async` overload against the same
controller instance would reach it, but each production construction site creates a fresh controller
(`QfcFormController.Actions.cs:49`, `:83`, `:139`) and each is awaited. The defect is therefore
**latent under the current call graph**; it is a correctness hazard for any future caller, not an
observed failure. This should be stated plainly in the PR body rather than claimed as an active bug.

### 6.3 Recommended fix

**Drain a locally captured reference; never reassign the field while it can still receive `Add`s.**

```
// Capture, then publish a fresh bag BEFORE awaiting, so any concurrent Add lands in the new bag.
var pending = Interlocked.Exchange(ref BackgroundLoadingTasks, []);
await Task.WhenAll(pending);
```

Properties of this shape:

- `Interlocked.Exchange` makes the swap atomic, so no `Add` can be lost between the read and the
  write. An `Add` that races the swap targets either the old bag (and is awaited) or the new bag
  (and is awaited by the *next* drain) — never a dropped bag.
- The await happens after the swap, so the window in which the old bag can still receive an `Add` is
  reduced to the instruction boundary of the exchange rather than the entire duration of `WhenAll`.
- Awaiting a local `pending` cannot be invalidated by a subsequent field reassignment.
- `Interlocked.Exchange<T>(ref T, T)` requires a reference-type field; `ConcurrentBag<Task>` is a
  class, so this compiles on the project's target framework without change. Note that
  `Interlocked.Exchange` cannot bind a target-typed `[]` collection expression directly in every
  compiler version; if it does not, use `new ConcurrentBag<Task>()` explicitly.

Rejected alternative: switching the field to a `List<Task>` under a `lock`. It is more code, it makes
every `Add` a contended operation, and it does not eliminate the drain/reset ordering question — it
only relocates it inside the lock. The `ConcurrentBag` + atomic-swap shape is smaller and expresses
the intent directly.

Also recommended in the same change: make the field `private` rather than `internal`, since nothing
outside the file reads it. That narrows the public surface per policy §C#5.2 at zero cost.

### 6.4 Testability

The two drain sites sit inside `LoadControlsAndHandlers_01Async`, which requires WinForms and COM.
Extracting the drain into a small `internal async Task DrainBackgroundLoadingTasksAsync()` member
makes it directly testable using the existing `FormatterServices.GetUninitializedObject` +
reflection-injection technique already established in
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:36-37`, `:69-71`. A test can inject a bag
containing a `TaskCompletionSource`-backed task, call the drain, and assert both that the field is a
different instance afterwards and that a task added during the await is still awaited by the next
drain.

---

## 7. #469 defect 3 — ordered replacement for `_itemGroupsToMove`

### 7.1 Complete member inventory touching `_itemGroupsToMove`

| Member | Line(s) | Operation |
|---|---|---|
| field declaration | `:71` | `private ConcurrentDictionary<QfcItemGroup, int> _itemGroupsToMove;` |
| `EmailsToMove` (property) | `:150` | `_itemGroupsToMove?.Count ?? 0` |
| `CacheItemGroupsForMove()` | `:876-881`, write at `:878-880` | whole-field assignment via `.Select(g => new KeyValuePair<QfcItemGroup,int>(g, 1)).ToConcurrentDictionary()` |
| `CleanupBackground()` | `:1015-1019` | null check `:1015`; `ForEach(kvp => kvp.Key.ItemController.Cleanup())` `:1017`; `.Clear()` `:1018` |
| `MoveEmailsAsync()` | `:2209` | `_itemGroupsToMove?.Count() ?? 0` |
| `TryGetItemGroupByIndex(int)` | `:2264` | `_itemGroupsToMove.ElementAt(index).Key` — **the defect** |
| `GetMoveDiagnostics(...)` | `:2284`, `:2285` | `.Count` twice |
| (test) `QfcCollectionControllerTests.cs` | `:66-71` | reflection injection of a `ConcurrentDictionary<QfcItemGroup, int>` |

Related but distinct: `_itemTlpToMove` (`:69`) is written at `:867` (inside the dead `CacheTlpForMove`)
and `:900` (inside live `CacheMoveObjects`), and disposed at `:1020-1021`. After #468 removes
`CacheTlpForMove`, `:900` becomes its only writer. No further change needed there.

### 7.2 Analysis

The `int` value is **always `1`** (`:879`) and is **never read** anywhere. The dictionary is being
used as an ordered set of `QfcItemGroup`, and `ConcurrentDictionary` provides neither ordering nor a
stable `ElementAt`.

Concurrency requirement: there is **no concurrent mutation**. The only writes are the whole-field
assignment at `:878` and the `Clear()` at `:1018`. There is no `TryAdd`/`TryRemove` anywhere. The
concurrent collection type is therefore not earning anything.

Read concurrency does exist: `MoveEmailsAsync` (`:2220-2223`) drives
`TryMoveEmailByGroupIndexAsync` through `ForEachAwaitAsync`, so `TryGetItemGroupByIndex` is called
repeatedly during an async walk. Those are reads only.

### 7.3 Recommendation

Replace the field with:

```
private IReadOnlyList<QfcItemGroup> _itemGroupsToMove;
```

- `CacheItemGroupsForMove()` (`:878-880`) becomes `_itemGroupsToMove = _itemGroups.ToList();` — this
  captures a **snapshot in list order**, which is the order every consumer already assumes.
- `TryGetItemGroupByIndex` (`:2260-2270`) becomes an explicit bounds check instead of a try/catch:
  ```
  var groups = _itemGroupsToMove;
  return (groups is not null && index >= 0 && index < groups.Count) ? groups[index] : null;
  ```
  This also removes a broad `catch (System.Exception)` that the code-change policy discourages
  (§General 3.1), and makes the method allocation-free and O(1) instead of `ElementAt`'s O(n).
- `EmailsToMove` (`:150`), `MoveEmailsAsync` (`:2209`), `GetMoveDiagnostics` (`:2284-2285`) work
  unchanged (`.Count` on `IReadOnlyList<T>`; note `:2209` uses the LINQ `Count()` extension and can
  be simplified to the `.Count` property).
- `CleanupBackground()` (`:1015-1019`) must change: `IReadOnlyList<T>` has no `Clear()`. Replace
  `.ForEach(kvp => kvp.Key.ItemController.Cleanup())` with a `foreach` over groups calling
  `grp.ItemController?.Cleanup()` (adding the null-conditional closes a real NRE that the `null`
  `ItemController` of §5.3 can produce here too), and replace `.Clear()` with
  `_itemGroupsToMove = null;` or assignment to an empty array.
- The owned test `QfcCollectionControllerTests.cs:66-71` must construct a `List<QfcItemGroup>`
  instead of a `ConcurrentDictionary`.

If a defensive stance against a hypothetical future concurrent mutation is preferred,
`System.Collections.Immutable.ImmutableArray<QfcItemGroup>` gives the same ordering with atomic
whole-value replacement. That adds a package dependency to `QuickFiler`; verify against the project's
existing references before choosing it. `IReadOnlyList<QfcItemGroup>` backed by `List<T>` is
sufficient given the write inventory above and is the recommended choice.

---

## 8. Interaction, overlap, and recommended fix order

### 8.1 Overlap map

| Overlap | Members involved | Interaction |
|---|---|---|
| **#468 deletes code no other issue touches** | the 12 dead members | Verified: no other issue's defect lines fall inside `:587-605`, `:635-738`, `:761-796`, `:827-857`, `:865-874`, `:1254-1273`, `:1324-1328`, `:1991-1996`. **No fix becomes moot.** |
| **#468 ↔ #444 (`KbdActions.cs`)** | `WireUpKeyboardHandler` `:1254-1273` | Deleting the member removes the only site producing the duplicate `Keys.Down`. No edit to `KbdActions.cs`. See downstream note below. |
| **#468 ↔ `_itemTlpToMove`** | `CacheTlpForMove` `:865-868` | After deletion, `:900` is the sole writer of `_itemTlpToMove`. Verify no compiler "assigned but never used" analyzer trip. |
| **#469-1 ↔ #470-3** | `GetMoveDiagnostics` `:2288-2313`, `SetVisualDigits` `:138-143` | **Same guard-placement shape.** Fix with one consistent idiom (guard first, then use) in both places. |
| **#470-1 ↔ #470-2** | `PromoteFirstChild` `:1972-1975`, `ToggleUnGroupConv` `:1819-1822` | **Same `FindIndex == -1` shape**, in two different methods. Fix both with the same `-1` guard idiom. |
| **#469-2 ↔ #469-1** | `GetMoveDiagnostics` `:2284`, `:2288-2324` | Same method. Do together: one edit pass over `:2272-2328`. |
| **#469-3 ↔ #469-1/-2** | `TryGetItemGroupByIndex` `:2260-2270` feeds `:2288` | Changing the collection type (§7) changes when `TryGetItemGroupByIndex` returns null (bounds check vs swallowed exception). Fix #469-3 **before** #469-1 so the guard is written against the final null contract. |
| **#470-2 → #470-3 and #469-1** | placeholder `QfcItemGroup` at `:2008` with `null ItemController` | #470-2's fix removes the *production* of the null; #470-3 and #469-1 remove the *unhandled consumption*. Both are still wanted — defence in depth — but the severity of #470-3 drops once #470-2 lands. |
| **#474-1 → #286** | `:1232` downcast inside `:1161`–`:1247` | The `InvalidCastException` at `:1232` is one producer of #286's counter leak. Fixing #286's `try`/`finally` is correct regardless; fixing #474-1 removes one cause. |
| **#473-2 ↔ #469-3** | `TryMoveEmailByGroupAsync` `:2236-2258` receives `TryGetItemGroupByIndex`'s null | #473-2's fix (guard the null at the boundary) depends on #469-3's null contract. Do #469-3 first. |
| **#471 ↔ #468** | `EliminateSpaceForItems` `:2013-2027` | No overlap. `EliminateSpaceForItems` is live (`:1779`) and interface-declared (`IQfcCollectionController.cs:47`); it is **not** removed by #468. |
| **#474-2 ↔ nothing** | `ReadyForMove` `:152-194` | Isolated; one consumer, outside the owned set (see §8.4). |

**No fix becomes moot because of #468.** All twelve dead members are disjoint from every other
defect's line range. The only "moot" relationship is the *dormant* #444 duplicate registration, which
is resolved as a side effect but is a different issue.

### 8.2 Recommended fix order

Ordered to minimise rework; each step's edits are disjoint from or strictly prior to the next.

1. **#468 — delete the twelve dead members, `_templateTlp`, and the commented reference at `:402`.**
   First, because it shrinks the file by ~10% and renumbers everything below. Doing it first means
   every subsequent step is planned against final line numbers exactly once.
2. **#474-1 — retype `_parent` to `IQfcFormController`, delete the downcast at `:1232`.**
   Second, because it is a type-level change that must compile before any behavioural edit in
   `RemoveSpecificControlGroupAsync` is attempted, and it touches the owned test file.
3. **#286 — wrap `RemoveSpecificControlGroupAsync`'s body in `try`/`finally`.**
   Third, because it wraps the region step 2 just modified. Doing it after step 2 avoids re-indenting
   the same block twice.
4. **#469-3 — replace `_itemGroupsToMove` with `IReadOnlyList<QfcItemGroup>`; rewrite
   `TryGetItemGroupByIndex` with an explicit bounds check.**
   Fourth, because steps 5 and 6 both depend on its null contract.
5. **#473-2 — guard the null at the `TryMoveEmailByGroupAsync` boundary; return early after the
   first failure; let `OperationCanceledException` propagate.**
6. **#469-1 and #469-2 — one edit pass over `GetMoveDiagnostics` (`:2272-2328` pre-renumber):**
   move the `qf is not null` guard above the first dereference, and size the array to
   `_itemGroupsToMove.Count`.
7. **#470-2 — reconcile the counts in `ToggleUnGroupConv`; extract the pure
   `ResolveConversationInsertions` helper; consume or remove the dead `conversationCount` parameter
   on `EnumerateConversationMembers`; guard `baseEmailIndex == -1`.**
   Before #470-3, because it removes the primary producer of the `null ItemController` that #470-3
   defends against — the #470-3 regression test then needs an explicitly injected null rather than
   one produced by the expansion path.
8. **#470-1 — guard `indexOriginal == -1` in `PromoteFirstChild` and at the `ChangeConversationSilently`
   call.** Same idiom as step 7's `baseEmailIndex` guard; write both the same way.
9. **#470-3 — make `SetVisualDigits`'s three reads consistently guarded.**
10. **#471 — remove the double inversion in `EliminateSpaceForItems`.**
    Independent of everything above; last because it is the smallest and least entangled.
11. **#473-1 — atomic-swap drain for `BackgroundLoadingTasks`; narrow the field to `private`.**
    Independent; can be done at any point, but placing it last keeps the `LoadControlsAndHandlers_01Async`
    bodies untouched while the rest of the file is being edited.
12. **#469-4 — document the `stackMovedItems` contract (or remove it, if the scope extension in §3
    is approved).**

### 8.3 DOWNSTREAM NOTE FOR #444 (`QuickFiler/Controllers/KbdActions.cs` — not edited by this feature)

Deleting `WireUpKeyboardHandler` (`QfcCollectionController.cs:1254-1273`) removes the *only* site in
the solution that constructs a `KbdActions<Keys, KaKey, Action<Keys>>` containing a duplicate key.
Two facts #444 should know:

1. The duplicate at `:1269`/`:1270` (`Keys.Down` twice) is **silent**, not an exception. The
   collection constructor `KbdActions(IEnumerable<UClass> list)` at
   `QuickFiler/Controllers/KbdActions.cs:26-29` performs a bare `new List<UClass>(list)` with no
   duplicate check, whereas both `Add` overloads (`KbdActions.cs:90-104` and `:106-121`) do check and
   throw `ArgumentException`. This asymmetry means the *collection* constructor is a hole in
   `KbdActions`' own invariant.
2. Because this feature removes the only exploiter of that hole, #444 may find its reproduction case
   gone after this feature merges. #444 should consider hardening
   `KbdActions(IEnumerable<UClass>)` at `KbdActions.cs:26-29` to apply the same duplicate check as
   `Add`, so the invariant holds for future callers. **This feature must not make that edit.**

### 8.4 SCOPE NOTE — #474 defect 2 cannot be fully fixed inside the owned file set

`ReadyForMove` (`:152-194`) has exactly **one** consumer solution-wide:
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:121` — `else if (_groups?.ReadyForMove == true)`
inside `ActionOkAsync` (`:110-134`). It is also declared on the interface at
`QuickFiler/Interfaces/IQfcCollectionController.cs:96`.

The `MessageBox.Show(...)` at `:186-191` is the **only** user-facing feedback when a group has no
destination folder — `ActionOkAsync` simply falls through the `else if` and does nothing visible. So
deleting the dialog without relocating it is a user-visible behaviour regression, and relocating it
requires editing `QfcFormController.EventHandlers.cs`, which is **not owned**.

**Recommended in-scope resolution — split, do not relocate:**

Add a pure, UI-free predicate inside the owned file:

```
/// <summary>Pure readiness check. Produces the notification text without presenting it.</summary>
internal bool TryGetMoveReadiness(out string notifications)
```

containing exactly the current `:156-184` logic, then reduce the property to:

```
public bool ReadyForMove
{
    get
    {
        if (TryGetMoveReadiness(out var notifications)) { return true; }
        ShowNotReadyNotification(notifications);
        return false;
    }
}
```

where `ShowNotReadyNotification` is a small `internal virtual` (or an injectable
`internal Action<string>` field defaulting to the `MessageBox.Show` call) so tests can substitute it.

This achieves the testability goal of #474 defect 2 — a readiness result the caller can inspect
without presenting UI — with **zero** behaviour change and **zero** edits outside the owned set. The
remaining architectural improvement (the caller owns the presentation) should be filed as a follow-up
that is scheduled together with an `IQfcCollectionController` contract change.

If the orchestrator instead approves extending the owned set to include
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, the full fix is: add
`bool TryGetMoveReadiness(out string notifications)` to `IQfcCollectionController`, delete the getter
entirely (or make it call the predicate and return the bool with no UI), and move the `MessageBox` into
`ActionOkAsync`'s `else` branch. That is a two-file change plus the interface. It should be an explicit
decision, not an inference.

Note that adding a member to `IQfcCollectionController` has a low blast radius: the interface has one
implementer (`QfcCollectionController`, `:22`) and no `Mock<IQfcCollectionController>` appears in
`QuickFiler.Test` for the readiness path. This was not exhaustively verified; a
`Mock<IQfcCollectionController>` search should be run before committing to the contract change.

---

## 9. File-size constraint

`QuickFiler/Controllers/QfcCollectionController.cs` is **2349 lines** (plus one trailing newline) at
the base commit. The repository policy caps files at 500 lines
(`.claude/rules/general-code-change.md`, "File Size Limit"; `CLAUDE.md` §4.1).

### Estimated reduction from #468

| Member | Span | Declaration lines |
|---|---|---|
| `_templateTlp` field | `:70` | 1 |
| commented reference | `:402` | 1 |
| `LoadGroups_02cAsync` | `:587-605` | 19 |
| `LoadGroups_02bAsync` | `:635-652` | 18 |
| `LoadGroup_03bAsync` | `:654-738` | 85 |
| `LoadConversationsAndFoldersAsync` | `:761-774` | 14 |
| `LoadItemGroup` | `:776-796` | 21 |
| `LoadSequentialAsync` | `:827-840` | 14 |
| `LoadGroupSequential` | `:842-857` | 16 |
| `CacheTlpForMove` | `:865-868` | 4 |
| `SwapTlp` | `:870-874` | 5 |
| `WireUpKeyboardHandler` | `:1254-1273` | 20 |
| `AnyOpenDropDownsAsync` | `:1324-1328` | 5 |
| `CaptureTlpTemplate` | `:1991-1996` | 6 |
| **Subtotal (declarations)** | | **229** |
| Blank separator lines (one per removed member, 12) | | 12 |
| **Estimated total removed** | | **~241** |

### Residual size

**Approximately 2108 lines** (2349 − 241). Net of the small additions the other six fixes introduce
(a `try`/`finally` block, a `TryGetMoveReadiness` extraction, a `ResolveConversationInsertions`
helper, guard clauses, XML doc comments), the realistic post-feature figure is **2120–2180 lines**.

That is roughly **4.2×–4.4× the 500-line cap.**

### Position

- This is a **pre-existing condition**, not one this feature creates or worsens. The file exceeded
  the cap by 4.7× before any change and will exceed it by ~4.3× after, so the change is
  cap-**improving**, just not cap-**satisfying**.
- **Do NOT propose a large file split as part of this bugfix feature.** Seven defect fixes plus a
  partial-class or type decomposition in one branch would make the diff unreviewable and would
  destroy the ability to attribute a regression to a specific defect fix. It also conflicts with the
  bugfix workflow in `CLAUDE.md` ("Change only what is needed… avoid opportunistic refactors").
- **Record as a follow-up candidate.** A natural decomposition already exists in the file's own
  `#region` structure: `UI Add and Remove QfcItems` (`:251-1250`), `Event Wiring` (`:1252-1387`),
  `UI Select QfcItems` (`:1389-1704`), `UI Conversation Expansion` (`:1706-1987`),
  `Helper Functions` (`:1989-2109`), `UI Light Dark` (`:2111-2174`), `Major Actions` (`:2176-2347`).
  Splitting into `partial class` files along those region boundaries would produce seven files of
  roughly 100–1000 lines, which is a smaller-but-still-non-compliant outcome; the
  `UI Add and Remove QfcItems` region alone is ~1000 lines and would need a further split. The
  follow-up should therefore be scoped as a genuine decomposition, not a mechanical region split.
- Note the file already carries `[ExcludeFromCodeCoverage]` (`:21`). A decomposition follow-up should
  be scheduled together with removing that attribute from whichever extracted parts become testable,
  otherwise the split buys nothing measurable.

---

## 10. Testing implications (strategy only, no test code)

Framework and libraries per `CLAUDE.md` §CUT1/CUT2: MSTest, Moq, FluentAssertions.

### Established seam

`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` already demonstrates the technique that
makes this WinForms/COM-bound controller testable without a live host:
`FormatterServices.GetUninitializedObject(typeof(QfcCollectionController))` (`:36-37`, `:147-148`,
`:254-255`) followed by reflection injection of private fields (`:69-71`, `:167-169`, `:178-180`,
`:358-362`). `QfcCollectionControllerDarkModeTests.cs:50-59` demonstrates the alternative — the real
constructor with every collaborator mocked — which works because the constructor's only real work is
field assignment plus `SetupLightDark` (`:52`). Both files are owned and both techniques should be
reused rather than reinvented.

Note the trap documented at `QfcCollectionControllerTests.cs:333-336`: `GetUninitializedObject`
bypasses field initializers, so `_digits` must be explicitly set to `1` or the `Digits` getter
(`:114-128`) flips `_digitRefreshNeeded` and drags the test into WinForms-bound `SetVisualDigits`.
The same applies to `BackgroundLoadingTasks` (`:80`) and `_moveMonitor` (`:78`), which are field
initializers and will be `null` under `GetUninitializedObject`.

### Per-defect strategy

| Defect | Approach |
|---|---|
| #286 | Inject an `_itemGroups` whose first element's `ItemController.IsActiveUI` getter throws; assert the static counter (read by reflection) returns to its pre-call value. Reset the static in `[TestInitialize]` — it is `static` (`:1157`), so tests are order-coupled without an explicit reset. |
| #468 | No new tests. The correct verification is that the solution still compiles and the existing `QuickFiler.Test` suite is green. |
| #469-1 | Inject a `_itemGroupsToMove` containing a group whose `ItemController` is `null`; assert `GetMoveDiagnostics` returns the "Unknown" diagnostic line and does not throw. |
| #469-2 | Assert `result.Length == _itemGroupsToMove.Count` and `result.Should().NotContainNulls()`. |
| #469-3 | Inject an ordered list; assert `TryGetItemGroupByIndex(i)` returns the i-th group for every `i`, returns `null` for `-1` and for `Count`, and that order is unchanged after a `Clear`/re-cache cycle. |
| #469-4 | If the document-the-contract option is taken: assert `MoveEmailsAsync(null)` behaves identically to `MoveEmailsAsync(stack)` for an empty `_itemGroupsToMove`. |
| #470-1 | Inject groups where no `ConvOriginID` matches; assert `ToggleGroupConv(id)` does not throw `ArgumentOutOfRangeException` and takes the documented no-op / recovery path. |
| #470-2 | Test the extracted pure `ResolveConversationInsertions` directly with a handcrafted resolver for the three cases (resolver count above, equal to, below `conversationCount - 1`), and assert the reconciliation logs and derives `insertCount` from the resolved list. Constructing a `ConversationResolver` in a test needs care — its private constructor (`ConversationResolver.cs:62`) plus settable `ConversationItems` (`Loading.cs:171-176`) and `Count` (`Loading.cs:270`) make direct field/property seeding feasible. |
| #470-3 | Inject one group with a `null` `ItemController`; assert `SetVisualDigits` does not throw and writes the fallback text. Requires `_itemGroups` and a mock `ItemViewer` — check whether `ItemViewer.LblItemNumber` is reachable headlessly before committing to this test shape. |
| #471 | Pure arithmetic. Assert `MinimumSize.Height` and `Size.Height` each decrease by `_template.Height * removalCount`, and that `MakeSpaceForItems(i, n)` followed by `EliminateSpaceForItems(i, n)` is height-neutral. Requires a real `TableLayoutPanel` for `_itemTlp`; a bare `new TableLayoutPanel()` is constructible without a form and `TableLayoutHelper.RemoveSpecificRow` behaviour on an empty panel must be checked first — if it throws, extract the arithmetic into a pure helper and test that instead. |
| #473-1 | Test the extracted `DrainBackgroundLoadingTasksAsync`: seed the bag with a `TaskCompletionSource`-backed task, add a second task during the await, assert both complete and neither is dropped. Use `TaskCompletionSource` for sequencing — `Task.Delay`/`Thread.Sleep` are prohibited by `.claude/rules/general-unit-test.md` ("Banned APIs in test code"). |
| #473-2 | Assert a `null` group produces exactly one `logger.Error` call and that `OperationCanceledException` propagates. Counting log calls requires a log4net appender seam or an injectable logging delegate; if neither exists, restructure so the failure path returns a discriminated result the test can assert on instead of asserting on logs. |
| #474-1 | Compile-time only; the type change is the proof. Optionally assert via reflection that `QfcCollectionController`'s constructor parameter type is `IQfcFormController`. |
| #474-2 | Assert `TryGetMoveReadiness` returns `false` with a non-empty notification string for a group with a `null` `SelectedFolder`, and `true` with an empty string otherwise — with no dialog presented. This is the test that the current property makes impossible; the repository unit-test policy prohibits popups in tests. |

### Coverage

`QfcCollectionController` carries `[ExcludeFromCodeCoverage]` (`:21`), so none of these tests will
move any coverage number. That is acceptable for a bugfix branch, but it means the coverage gate
cannot be used as evidence that the fixes are exercised. The PR should instead cite the specific
test names per defect. Removing the attribute is out of scope here and belongs with the
decomposition follow-up (§9).

---

## 11. Open questions and what would settle them

1. **Reflective or non-`.cs` callers of the twelve dead members.** Not searched. Settled by a
   repository-wide search of non-`.cs` files for the twelve identifiers plus a search for
   `GetMethod(`/`InvokeMember(` in `QuickFiler`.
2. **Whether `Mock<IQfcCollectionController>` exists anywhere.** Relevant only if the §8.4 contract
   change is approved. Settled by a `Mock<IQfcCollectionController>` search across `QuickFiler.Test`.
3. **Whether `TableLayoutHelper.RemoveSpecificRow` and `ItemViewer.LblItemNumber` are reachable
   headlessly.** Determines whether the #471 and #470-3 tests can target the production members
   directly or need a pure-helper extraction. Settled by reading
   `TableLayoutHelper` and `ItemViewer` and attempting a headless construction.
4. **Whether a log-assertion seam exists for `log4net`.** Determines the shape of the #473-2 test.
   Settled by searching `QuickFiler.Test` and `UtilitiesCS.Test` for an existing memory-appender or
   logger-injection pattern.
5. **Whether the #473-1 window is reachable at all on the current call graph.** §6.2 argues it is
   latent. Settled only by an execution trace; the recommendation does not depend on the answer,
   since the fix is small and strictly safer.
