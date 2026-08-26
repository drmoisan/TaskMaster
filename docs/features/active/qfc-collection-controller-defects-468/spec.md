# qfc-collection-controller-defects (Spec)

- **Issue:** #468
- **Parent (optional):** epic `quickfiler-bug-family` (integration branch `epic/quickfiler-bug-family-integration`)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-24
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** `full-bug`

> **Acceptance-criteria authority.** Work mode is `full-bug`. Under
> `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `spec.md` is the **sole** acceptance-criteria
> source for this work mode. `user-story.md` is intentionally absent. The `## Acceptance Criteria`
> section at the end of this document is the binding checklist that executors and reviewers check off.
> `issue.md`'s acceptance-criteria section is a pointer only.

All `file:line` citations are against base commit `988e819b` unless stated otherwise. Line numbers in
`QuickFiler/Controllers/QfcCollectionController.cs` shift once step 1 of the fix order (dead-code
removal) lands; that is called out explicitly in `## Proposed Fix`.

---

## Context

This feature closes seven pre-existing GitHub bug issues — #286, #468, #469, #470, #471, #473, #474 —
comprising approximately fifteen distinct defects, all concentrated in the single 2,349-line file
`QuickFiler/Controllers/QfcCollectionController.cs`. They are remediated as one change because seven
independent branches against the same file would produce serial merge conflicts for no reviewability
benefit.

**Observed environments.** All defects are logic defects in C# source and reproduce on any host
running QuickFiler. None is environment-specific. Every line number in all seven promoted potential
documents was re-verified against the current source and matched exactly; there has been no
line-number drift, and **no defect failed to reproduce**
(`research/qfc-collection-controller-defects.md` §1, verdict table).

**Impact and severity.** Six issues carry severity Medium and one (#468, #473) carries Low. The
user-visible impacts are:

- Conversation collapse grows the item panel instead of shrinking it, cumulatively over a session
  (#471).
- Conversation expansion can silently re-initialize a group holding a different message, or throw
  `NullReferenceException` in `RenumberGroups` (#470 defect 2).
- Expanding or collapsing a conversation whose original message was already filed throws
  `ArgumentOutOfRangeException` (#470 defect 1).
- Move diagnostics can throw `NullReferenceException`, and always append a spurious blank line to
  the metrics CSV plus a `null` element into the metrics `BlockingCollection<string>` (#469 defects
  1 and 2).
- After any exception inside `RemoveSpecificControlGroupAsync`, every subsequent call logs a
  false-positive race-condition error for the remaining life of the process (#286).
- Cancelled moves are logged as generic errors rather than propagating as cancellations, and one
  root failure produces two misleading log entries (#473 defect 2).

**Severity corrections established by research** (these supersede the promoted documents):

- **#469 defect 4 does NOT escalate to High.** Undo-after-move is not broken. See
  `## Root Cause Analysis` RC-I and `## Proposed Fix` step 12.
- **#468's stated rationale does not hold.** The class carries `[ExcludeFromCodeCoverage]` at
  `QuickFiler/Controllers/QfcCollectionController.cs:21`, so none of the ~229 dead lines is in any
  coverage denominator today. Removal remains correct on dead-code grounds alone — unreachable
  production code is a maintenance and comprehension cost regardless — but **the PR body must not
  repeat the issue's coverage-denominator rationale**, which is factually wrong on this base.

**First observed.** #286 was captured 2026-07-09 from a follow-up-candidate note in the archived
issue-#232 feature folder. The remaining six were captured 2026-08-07 from a single solution-wide
review performed as preparation research for issue #454.

---

## Repro & Evidence

Every defect is established by static analysis of the source; none requires a running Outlook host to
observe. The per-defect repro steps are in the seven promoted potential documents, which are the
authoritative requirement text:

| Issue | Defects | Authoritative potential document |
|---|---|---|
| #286 | 1 | `docs/features/potential/promoted/2026-07-09-qfc-collectioncontroller-removespecificcontrolgroup-counter-leak.md` |
| #468 | 1 (13 members/fields) | `docs/features/potential/promoted/2026-08-07-qfc-collection-controller-unreachable-load-paths.md` |
| #469 | 4 | `docs/features/potential/promoted/2026-08-07-qfc-collection-move-diagnostics-defects.md` |
| #470 | 3 | `docs/features/potential/promoted/2026-08-07-qfc-collection-conversation-index-defects.md` |
| #471 | 1 | `docs/features/potential/promoted/2026-08-07-qfc-collection-eliminate-space-sign-error.md` |
| #473 | 2 | `docs/features/potential/promoted/2026-08-07-qfc-collection-background-task-and-catch-defects.md` |
| #474 | 2 | `docs/features/potential/promoted/2026-08-07-qfc-collection-controller-coupling-and-modal-getter.md` |

### Expected vs actual, per defect

| Defect | Location (base commit) | Actual | Expected |
|---|---|---|---|
| #286 | `:1157`, `:1161`, `:1237-1242`, `:1247` | Any throw between `:1161` and `:1247` skips the decrement; the counter is permanently inflated and `:1239-1241` logs a false-positive race-condition error on every later call | The counter returns to its pre-call value on both the normal and the exceptional exit path |
| #468 | see `## Proposed Fix` step 1 table | 12 members and 1 field are unreachable from any production or test caller | The members and the field are absent from the file |
| #469-1 | `:2288`, `:2289`, `:2312`, `:2313`, `:2318-2322` | `qf` may be null by construction of the `?.` at `:2288`; `:2289` dereferences it and throws `NullReferenceException`; the guard at `:2313` and its `else` at `:2318-2322` are unreachable | The guard precedes every dereference; a null `ItemController` produces the intended "Unknown" diagnostic line |
| #469-2 | `:2284`, `:2286`, `:2324` | `new string[_itemGroupsToMove.Count + 1]` with a loop filling `0..Count-1`; element `[Count]` is always `null` | The array length equals `_itemGroupsToMove.Count` and contains no null element |
| #469-3 | `:71`, `:2264` | `_itemGroupsToMove.ElementAt(index).Key` over a `ConcurrentDictionary`, whose enumeration order is unspecified and unstable across mutation | Index-to-group resolution comes from an explicitly ordered collection |
| #469-4 | `:2206-2228` | `stackMovedItems` is declared but never read anywhere in the body | The contract states truthfully how the undo stack is populated (see RC-I) |
| #470-1 | `:1743`, `:1745`, `:1749`, `:1972`, `:1975`, `:1716` | `FindIndex` returns `-1`, then `_itemGroups[-1].ItemViewer` throws `ArgumentOutOfRangeException` | A `-1` index is handled explicitly and is never used as a subscript |
| #470-2 | `:1823`, `:1827-1830`, `:1883-1893` | Reservation count `conversationCount - 1` and insertion count `insertions.Count` are derived independently and can disagree in four distinct ways (see RC-C) | Both counts derive from one source; disagreement is surfaced before any mutation |
| #470-3 | `:140` vs `:141-142` | `:140` dereferences `grp.ItemController` unguarded; `:141-142` guard the same object with `?.` | All reads of `grp.ItemController` in the lambda are consistently guarded |
| #471 | `:2017`, `:2020`, `:2025` | A negative magnitude is assigned and then subtracted, so the panel **grows** by `_template.Height * removalCount` on every removal | The panel shrinks by `_template.Height * removalCount`, mirroring `MakeSpaceForItems` (`:2029-2042`) |
| #473-1 | `:80`, `:398-399`, `:492-493` | `await Task.WhenAll(BackgroundLoadingTasks); BackgroundLoadingTasks = [];` replaces the bag **reference**; an `Add` landing in the window targets a bag that is then dropped unawaited | No task added to the set can be dropped unawaited |
| #473-2 | `:2236-2258` | The broad catch at `:2242` logs, then execution falls through to `:2247`, which dereferences the same null and throws again, logging a second error at `:2253-2256`; both catches also swallow `OperationCanceledException` | One root failure produces one log entry; `OperationCanceledException` propagates |
| #474-1 | `:64`, `:1232` | `((QfcFormController)_parent).SkipGroupAsync()` — a runtime downcast from an interface-typed field to a concrete `internal` type | No downcast; the field's static type declares the member |
| #474-2 | `:152-194`, `MessageBox` at `:186-191` | Reading the `ReadyForMove` property displays a modal dialog, blocking the caller and making the `false` path untestable under repository policy | A readiness result is obtainable without presenting UI |

### Determinism

All defects are deterministic given their triggering input. Two qualifications, both established by
research and both to be stated in the PR body rather than overclaimed:

1. **#473 defect 1 is latent under the current call graph.** Both `Add` pairs occur in the same method
   body strictly before their `WhenAll`, no other member adds to the bag, and each of the three
   production construction sites (`QuickFiler/Controllers/QfcFormController.Actions.cs:49`, `:83`,
   `:139`) creates a fresh controller that is awaited. The window is a correctness hazard for any
   future caller, not an observed failure
   (`research/qfc-collection-controller-defects.md` §6.2).
2. **#474 is latent in the current single-implementation configuration.** `QfcFormController` is the
   only production parent, so the downcast at `:1232` does not throw today.

---

## Scope & Non-Goals

### Files this feature owns (may be changed)

- `QuickFiler/Controllers/QfcCollectionController.cs`
- `QuickFiler/Interfaces/IQfcCollectionController.cs`
- `QuickFiler/Controllers/IQfcFormController.cs`
- `QuickFiler/Interfaces/IFilerFormController.cs`
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (existing)
- `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` (existing)
- New test files under `QuickFiler.Test/Controllers/QfcCollectionController*` (see `## Test Strategy`)
- `QuickFiler.Test/QuickFiler.Test.csproj` — **only** to add `Compile Include` entries for the new
  test files, at the exact insertion point given in `## Test Strategy`

### Files this feature MUST NOT write

- **`QuickFiler/Controllers/KbdActions.cs`** — owned by sibling epic child issue #444, which depends
  on this feature and merges after it. Research established that removing
  `WireUpKeyboardHandler` requires **no** edit to this file: the removed member is a *caller* of
  `_kbdHandler.CharActions.Add(...)` (`:1259`) and a constructor of
  `KbdActions<Keys, KaKey, Action<Keys>>` (`:1265`); deleting a caller deletes zero lines in the
  callee (`research/qfc-collection-controller-defects.md` §2). Findings that *do* require an edit
  there are recorded in `## Downstream Notes for Sibling Issues` and kept out of the plan.
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` — not in the owned set. This constrains
  the resolution of #469 defect 4 and of #474 defect 2 (see `## Proposed Fix` steps 12 and 11).
- `QuickFiler/Controllers/EfcFormController.cs` — not in the owned set. This rules out option (a) for
  #474 defect 1.
- `QuickFiler/Interfaces/IQfcFormController.cs` (the orphan `QuickFiler.Interfaces` type at `:7`) —
  see `## Follow-up Candidates`.

### Out of scope / non-goals

- **No file split of `QfcCollectionController.cs`.** See `## Follow-up Candidates`.
- **No removal of `[ExcludeFromCodeCoverage]`** (`:21`). Removing it is a coverage-scope decision that
  belongs with the decomposition follow-up.
- **No change to the `IQfcCollectionController` member set for `ReadyForMove`.** The architectural end
  state (caller owns the presentation) requires editing a non-owned file.
- **No removal of the `stackMovedItems` parameter.** Same reason.
- **No new NuGet package reference to `QuickFiler.Test`.** In particular, no log4net `MemoryAppender`.
- **No behavioural change to the four live members whose names resemble the dead cluster**:
  `AnyOpenDropDowns` (non-async, `:1319`, live — called at `:1309`), `LoadItemGroupsAndViewers_02`
  (`:740`, live — called at `:287`), `LoadConversationsAndFolders_04` (`:756`, live — called at
  `:295`), `LoadSequential_5` (`:798`, live — called at `:758`), and `ActivateQueuedTlp` (`:859`,
  live — called at `:259`).

### Explicitly excluded systems

Live Outlook, a running VSTO host, a WPF `Dispatcher`, and any real `ItemViewer` (a `UserControl`
with a WebView2 surface, `QuickFiler/Viewers/ItemViewer.cs:21`). No test may call `UiThread.Init()`,
which constructs and calls `.Show()` on a `SyncContextForm` (`UtilitiesCS/Threading/UiThread.cs:54`).

---

## Root Cause Analysis

The ~15 defects are not fifteen unrelated slips. They fall into ten recurring shapes. Fixing each
shape with **one consistent idiom** across all of its instances is a requirement of this feature, not
a stylistic preference, because the promoted documents themselves observe the recurrence
(`2026-08-07-qfc-collection-conversation-index-defects.md:92-94`: "the same shape as the
`GetMoveDiagnostics` defect filed separately … suggesting a systematic guard-placement habit rather
than isolated slips").

### RC-A — Null guard placed after the dereference it protects

- **#469 defect 1.** `var qf = TryGetItemGroupByIndex(k)?.ItemController;` (`:2288`) explicitly
  anticipates null, and `:2289` (`qf.ItemHelper`) immediately negates it. `:2312` dereferences again.
  The guard `if (qf is not null)` appears only at `:2313`, making its `else` branch (`:2318-2322`)
  unreachable. Two independent null sources reach `:2288`: `TryGetItemGroupByIndex` returns `null` on
  any exception (`:2266-2269`), and `QfcItemGroup.ItemController` can itself be null (produced by the
  placeholder groups created at `:2008`).
- **#470 defect 3.** `SetVisualDigits` (`:130-146`) dereferences `grp.ItemController.ItemNumberDigits`
  unguarded at `:140`, then guards the *same object* with `?.` at `:141-142`.

Both instances must be fixed with the same idiom: **guard first, then use**.

### RC-B — An unvalidated `-1` from `List.FindIndex` used as a subscript

- **#470 defect 1.** `ToggleGroupConv(string)` (`:1733`) obtains `indexOriginal` from `FindIndex`
  (`:1738-1740`), routes `-1` to `PromoteFirstChild` (`:1745`), which performs its own `FindIndex`
  (`:1972`) and dereferences `_itemGroups[indexOriginal].ItemViewer` (`:1975`) with no guard. The
  second consumer, `ChangeConversationSilently(indexOriginal, true)` (`:1749`), subscripts
  `_itemGroups` the same way at `:1716`.
- **#470 defect 2, second instance.** `baseEmailIndex` from `FindIndex` (`:1819-1821`) can be `-1`,
  making `insertionIndex == 0` and `_itemGroups[insertionIndex - 1]` at `:1900-1902` throw
  `ArgumentOutOfRangeException`. This is the same shape appearing a second time inside the method
  #470 defect 2 already touches.

Both must be guarded with the same idiom.

### RC-C — Two independent sources of truth for one count

- **#470 defect 2.** The reservation `insertCount = conversationCount - 1` (`:1823`, applied at
  `:1827-1830`) and the insertion bound `insertions.Count` (`:1883-1888`) are derived from two
  different lazily-loaded properties of `ConversationResolver`. Research identified **four independent
  mechanisms** by which they disagree
  (`research/qfc-collection-controller-defects.md` §5.2): a conditional base-item filter (the
  `.Where(EntryID != entryID)` at `:1884` removes zero rather than one when the base message is not in
  `ConversationItems.SameFolder`); two different filter predicates (a DataFrame row count at
  `QuickFiler/Helper Classes/ConversationResolver.Loading.cs:279` versus a runtime folder-*name*
  string comparison at `Loading.cs:65-66`); a single-item fallback path (`Loading.cs:37-56`); and a
  time-of-check/time-of-use gap, since `ConversationItems` is also assigned from a background task
  (`Loading.cs:194`). Consequences are both confirmed: `insertions.Count > insertCount` silently
  re-initializes a group holding a different message (`:1893` → `InitializeGroup` `:1849-1864`);
  `insertions.Count < insertCount` leaves placeholder groups with a `null` `ItemController` that
  `RenumberGroups` dereferences at `:2068`.
- **#469 defect 2.** The array is allocated at `_itemGroupsToMove.Count + 1` (`:2284`) while the loop
  bound is `_itemGroupsToMove.Count` (`:2285-2286`). Index `Count` is never assigned.

### RC-D — A collection type that does not carry the guarantee the code relies on

- **#469 defect 3.** `_itemGroupsToMove` is a `ConcurrentDictionary<QfcItemGroup, int>` (`:71`) but is
  used as an **ordered set**: the `int` value is always `1` (`:879`) and is never read anywhere, while
  `TryGetItemGroupByIndex` does `ElementAt(index).Key` (`:2264`). `ConcurrentDictionary` provides
  neither ordering nor a stable `ElementAt`. Two independent `0..Count-1` walks exist
  (`MoveEmailsAsync` `:2220-2223`, `GetMoveDiagnostics` `:2286-2288`), so a diagnostic line can be
  attributed to the wrong message. Research further established there is **no concurrent mutation** to
  justify the concurrent type: the only writes are the whole-field assignment at `:878` and the
  `.Clear()` at `:1018`; there is no `TryAdd`/`TryRemove` anywhere.
- **#473 defect 1.** `BackgroundLoadingTasks = [];` (`:399`, `:493`) replaces the field *reference*
  rather than clearing the bag, so an `Add` between the `WhenAll` snapshot and the assignment lands in
  a bag that is then unreferenced and never awaited.

### RC-E — Broad catch that continues instead of returning or re-raising

- **#473 defect 2.** `TryMoveEmailByGroupAsync` (`:2236-2258`) catches broadly at `:2242`, logs, then
  falls through to `:2247` (`group.MailItem.Subject`), which dereferences the same null and throws
  again into the second broad catch at `:2249`, producing a second log at `:2253-2256`. Both catches
  also swallow `OperationCanceledException`.
- Related, and fixed as part of RC-D: `TryGetItemGroupByIndex` (`:2260-2270`) uses a broad
  `catch (System.Exception)` at `:2266-2269` in place of a bounds check.

Both violate CLAUDE.md § General Code Change Policy 3.1 and § C#4.1.

### RC-F — Cleanup on the normal exit path only

- **#286.** `Interlocked.Increment` at `:1161` is the first statement of
  `RemoveSpecificControlGroupAsync` (`:1159`); `Interlocked.Decrement` at `:1247` is the last
  statement (method ends `:1248`); there is no `try`/`finally` between them. Unguarded exception
  sources inside the protected span include `UnregisterNavigation()` (`:1162`),
  `_itemGroups[selection - 1]` (`:1165-1166`), `TableLayoutHelper.RemoveSpecificRow` (`:1183`), and
  the awaited dispatcher lambda at `:1226-1236`, which itself performs the #474 downcast at `:1232`.
  The synchronous sibling `RemoveSpecificControlGroup(int)` (`:1105-1155`) does not touch the counter
  and needs no change.

### RC-G — A collaborator's static type narrower than the role it plays

- **#474 defect 1.** `_parent` is declared `IFilerFormController` (`:64`, constructor parameter at
  `:35`) but `:1232` needs `SkipGroupAsync`, which is declared on
  `QuickFiler.Controllers.IQfcFormController` (`QuickFiler/Controllers/IQfcFormController.cs:38`), so
  the code downcasts to the concrete `internal` `QfcFormController`
  (`QuickFiler/Controllers/QfcFormController.cs:19`). A `public` type's method body casting to an
  `internal` type is itself a smell.

  **The promoted document's premise is FALSE on this base and must not be repeated.**
  `2026-08-07-qfc-collection-controller-coupling-and-modal-getter.md:35-39` states the two interfaces
  are unrelated and that "neither is a superset of the other." Verified in source:

  ```
  QuickFiler/Controllers/IQfcFormController.cs:13
      public interface IQfcFormController : IFilerFormController
  ```

  `IQfcFormController` is a **strict superset** of `IFilerFormController`. There is no consolidation
  to perform; only a field retype.

  The same document (`:95-98`) also states that issue #454 introduces injectable delegate seams around
  both call sites. **Issue #454 has NOT landed on this base**; those seams do not exist. This feature
  creates its own seams (see `## Test Strategy`).

- Disambiguation, required before editing: three types are named `IQfcFormController`.
  `QuickFiler.Controllers.IQfcFormController` (`QuickFiler/Controllers/IQfcFormController.cs:13`) is
  the one that matters. `QuickFiler.Interfaces.IQfcFormController`
  (`QuickFiler/Interfaces/IQfcFormController.cs:7`) is an orphan with no implementer.
  `QuickFiler.Notes.IQfcFormController` (`QuickFiler/Notes/notes_interfaces.cs:13`) is not compiled.
  Inside `QfcCollectionController.cs` (namespace `QuickFiler.Controllers`) an unqualified
  `IQfcFormController` binds to the `QuickFiler.Controllers` one by same-namespace preference; **no
  `using` alias is needed**.

### RC-H — UI presentation embedded in domain logic

- **#474 defect 2.** `ReadyForMove`'s getter (`:152-194`) calls `MessageBox.Show(...)` at `:186-191`.
  Reading a property therefore blocks on user interaction, cannot run on a background thread, cannot
  be read twice without side effects, and cannot be exercised in a test at all — repository policy
  prohibits popups in tests, and `docs/features/epics/winforms-testability-refactor/epic.md:58-59`
  states this explicitly.

### RC-I — Declared parameters the body never reads

- **#469 defect 4.** `MoveEmailsAsync(SloStack<IMovedMailInfo> stackMovedItems)` (`:2206-2228`) names
  the parameter in the signature and in one commented-out trace call (`:2208`) and reads it nowhere.

  **Triage verdict (supersedes the promoted document's open question): undo is NOT broken; the
  parameter is REDUNDANT, not dropped. Severity stays Medium and does NOT rise to High.**
  The caller supplies `_movedItems` (`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225`),
  which is assigned `_globals.AF.MovedMails` at `QuickFiler/Controllers/QfcFormController.cs:49`
  (field at `:86`) — an alias of the global stack, not a fresh per-move collection. That same instance
  is populated on the real move path by
  `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:188`
  (`Globals.AF.MovedMails.Push(info)`, inside `PushToUndoStack` at `:185-189`), reached from
  `MoveEmailsAsync` → `TryMoveEmailByGroupIndexAsync` (`:2230-2234`) → `TryMoveEmailByGroupAsync`
  (`:2236`) → `group.ItemController.MoveMailAsync()` (`:2240`) →
  `QuickFiler/Controllers/QfcItemController.MailActions.cs:83-113`. The undo consumer reads the same
  global stack at `QuickFiler/Controllers/QfcFormController.Actions.cs:206-250`.

- **Unfiled companion defect (found by research, not in any of the seven documents).**
  `EnumerateConversationMembers` (`:1875-1881`) **never reads its `conversationCount` parameter**; the
  body (`:1883-1921`) reads `entryID`, `resolver`, `insertionIndex` and `folderList` only. This is the
  same shape as #469 defect 4 and it is the *direct cause* of #470 defect 2: the reservation count is
  passed in and discarded, so the method has no way to detect disagreement. Fixing #470 defect 2
  necessarily makes this parameter live.

### RC-J — Superseded code left in place

- **#468.** Twelve members plus the field `_templateTlp` (`:70`) have no caller anywhere in the
  solution, including all test projects. The `Load*` cluster is a superseded loading strategy; the
  commented-out reference at `:402` supports that reading. Full per-member inventory with caller
  counts is in `research/qfc-collection-controller-defects.md` §2.

  **Residual risk, stated rather than hidden:** the reference search covered `*.cs` only. No XAML,
  `.resx`, or designer file in `QuickFiler` was searched for a late-bound reference, and no
  `Type.GetMethod`/`InvokeMember` reflection search was performed against these names. The risk is
  judged low (these are ordinary instance methods on a controller with no serialization or
  data-binding surface) but it is not zero. Settling it requires a repository-wide search of non-`.cs`
  files for the twelve identifiers plus a `GetMethod(`/`InvokeMember(` search in the `QuickFiler`
  tree; the plan should include that search as a verification step for #468.

### Affected components

Only two production assemblies are involved. `QuickFiler` carries every defect. `UtilitiesCS` is read
for evidence only (`EmailFiler.cs`, `TableLayoutHelper.cs`, `UiThread.cs`, `SloStack.cs`) and is not
modified. `QuickFiler.Test` gains regression tests.

---

## Proposed Fix

### Design summary

Fourteen targeted fixes plus one dead-code removal, applied in the order below, all confined to the
owned file set. Three minimal, behaviour-preserving production seams are introduced solely to make
otherwise-untestable defects testable; each defaults to the exact prior call and is modelled on the
existing ratified precedent in this same file at `QuickFiler/Controllers/QfcCollectionController.cs:1060-1074`
(`_removeGroupByEntryId`), whose XML comment already states the intent: "Tests inject a recording
delegate so the … logic can be verified without WinForms/COM state."

### Fix order (authoritative; from `research/qfc-collection-controller-defects.md` §8.2)

The order below is chosen to minimise rework: each step's edits are disjoint from, or strictly prior
to, the next.

**Statement required by the caller: no fix becomes moot under this order.** All twelve dead members
removed in step 1 are disjoint from every other defect's line range — verified against the spans
`:587-605`, `:635-738`, `:761-796`, `:827-857`, `:865-874`, `:1254-1273`, `:1324-1328`, `:1991-1996`
(`research/qfc-collection-controller-defects.md` §8.1). The only "moot" relationship is the *dormant*
duplicate-`KaKey` registration of sibling issue #444, which step 1 resolves as a side effect; that is
a different issue and is recorded in `## Downstream Notes for Sibling Issues`.

---

**Step 1 — #468: delete the dead members, the dead field, and the commented reference.**

Removed first because it shrinks the file by roughly 10% and renumbers everything below it. Doing it
first means every subsequent step is planned against final line numbers exactly once.

| Member / field | Span (base commit) | Access | Callers solution-wide |
|---|---|---|---|
| `_templateTlp` (field) | `:70` | private | written at `:1994`, `:1995` (both inside `CaptureTlpTemplate`); never read |
| commented reference to `LoadGroups_02bAsync` | `:402` | — | n/a |
| `LoadGroups_02cAsync` | `:587-605` | **public** | 0 |
| `LoadGroups_02bAsync` | `:635-652` | **public** | 0 live (commented `:402` only) |
| `LoadGroup_03bAsync` | `:654-738` | private | 1, itself dead (`:647`) |
| `LoadConversationsAndFoldersAsync` | `:761-774` | **public** | 0 |
| `LoadItemGroup` | `:776-796` | internal | 1, itself dead (`:772`) |
| `LoadSequentialAsync` | `:827-840` | **public** | 0 |
| `LoadGroupSequential` | `:842-857` | **public** | 1, itself dead (`:838`) |
| `CacheTlpForMove` | `:865-868` | internal | 1, itself dead (`:872`) |
| `SwapTlp` | `:870-874` | internal | 0 |
| `WireUpKeyboardHandler` | `:1254-1273` | **public** | 0 |
| `AnyOpenDropDownsAsync` | `:1324-1328` | internal | 0 |
| `CaptureTlpTemplate` | `:1991-1996` | internal | 0 |

Constraints on this step:

- **`IQfcCollectionController` needs no edit for #468.** `QuickFiler/Interfaces/IQfcCollectionController.cs`
  (118 lines, read in full) declares **none** of the thirteen.
- **`KbdActions.cs` is not edited.** Removing `WireUpKeyboardHandler` deletes a caller, not a callee.
- **`AnyOpenDropDowns` (non-async, `:1319`) is LIVE** — called at `:1309` from
  `CustomReturnKeyHandler`. Only the `Async` overload at `:1324` is dead. Do not remove the live
  overload.
- Six of the removed members are `public` on a `public` type (`:22`), so this is a public-API change
  of the `QuickFiler` assembly. The assembly is referenced only by `TaskMaster` and `QuickFiler.Test`
  within this repository, so the break is contained.
- After removal, `:900` (inside the live `CacheMoveObjects`) becomes the sole writer of
  `_itemTlpToMove` (`:69`). Verify no "assigned but never used" analyzer diagnostic appears.

**Step 2 — #474 defect 1: retype `_parent` to `IQfcFormController` and delete the downcast.**

Second, because it is a type-level change that must compile before any behavioural edit inside
`RemoveSpecificControlGroupAsync` is attempted, and because it touches an owned test file.

Changes:

1. `QuickFiler/Controllers/QfcCollectionController.cs:35` — constructor parameter
   `IFilerFormController parent` → `IQfcFormController parent`.
2. `QuickFiler/Controllers/QfcCollectionController.cs:64` — field
   `private IFilerFormController _parent;` → `private IQfcFormController _parent;`.
3. `QuickFiler/Controllers/QfcCollectionController.cs:1232` — replace
   `await ((QfcFormController)_parent).SkipGroupAsync();` with `await _parent.SkipGroupAsync();`.
4. `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:45` —
   `new Mock<IFilerFormController>()` → `new Mock<IQfcFormController>()`.

Why this compiles with **zero out-of-scope edits**:

- `IQfcFormController : IFilerFormController` (`QuickFiler/Controllers/IQfcFormController.cs:13`), so
  every member currently invoked on `_parent` — `ActionOkAsync()` at `:1153`, `:1270`, `:1312` —
  remains available.
- All three production construction sites pass `parent: this` from inside `QfcFormController`
  (`QuickFiler/Controllers/QfcFormController.Actions.cs:49`, `:83`, `:139`), and `QfcFormController`
  implements `IQfcFormController` (`QuickFiler/Controllers/QfcFormController.cs:19`). Zero production
  caller edits.
- The only other affected site is the owned dark-mode test at
  `QfcCollectionControllerDarkModeTests.cs:45`. Because `IQfcFormController` is a strict superset, Moq
  creates the wider mock and the test's existing setups are unaffected.

This converts a runtime `InvalidCastException` into a compile-time constraint, which is strictly
stronger, and removes the public-type-casts-to-internal-type situation.

Two options were considered and rejected. **(a) Add `SkipGroupAsync` to `IFilerFormController`** —
rejected because it forces a stub onto `EfcFormController`
(`QuickFiler/Controllers/EfcFormController.cs:28`), which is **not** an owned file and for which
"skip to the next group" is meaningless. **(c) Merge the two interfaces** — rejected on blast radius
(both interface files plus `QfcFormViewer.cs`, `IQfcFormViewer.cs`, `IFilerHomeController.cs`,
`QfcHomeController.cs`, `EfcHomeController.cs`, `EfcFormController.cs`, and roughly eight test files);
it is a refactor, not a bugfix. Recorded in `## Follow-up Candidates`.

**Step 3 — #286: wrap `RemoveSpecificControlGroupAsync`'s body in `try`/`finally`.**

Third, because it wraps the region step 2 just modified; doing it after step 2 avoids re-indenting the
same block twice. Move `Interlocked.Decrement(ref removespecificcontrolgroupcounter);` (`:1247`) into a
`finally` covering everything from `:1162` to `:1246`. The increment at `:1161` stays outside the
`try`.

The unsynchronized plain read at `:1237` is a secondary concern noted in the potential document
(`2026-07-09-…-counter-leak.md:56`) and is **not** required by this feature; it may be left as a
best-effort diagnostic. If the executor changes it, the change must be called out separately.

**Step 4 — #469 defect 3: replace `_itemGroupsToMove` with an ordered collection.**

Fourth, because steps 5 and 6 both depend on its null contract.

- `:71` — `private ConcurrentDictionary<QfcItemGroup, int> _itemGroupsToMove;` →
  `private IReadOnlyList<QfcItemGroup> _itemGroupsToMove;`
- `CacheItemGroupsForMove()` (`:876-881`, write at `:878-880`) → `_itemGroupsToMove = _itemGroups.ToList();`
  — a snapshot in list order, which is the order every consumer already assumes.
- `TryGetItemGroupByIndex` (`:2260-2270`) → an explicit bounds check replacing the broad
  `catch (System.Exception)`:
  `var groups = _itemGroupsToMove; return (groups is not null && index >= 0 && index < groups.Count) ? groups[index] : null;`
  This makes the method O(1) instead of `ElementAt`'s O(n) and removes a policy-discouraged broad
  catch.
- `CleanupBackground()` (`:1015-1019`) — `IReadOnlyList<T>` has no `Clear()`. Replace
  `.ForEach(kvp => kvp.Key.ItemController.Cleanup())` (`:1017`) with a `foreach` calling
  `grp.ItemController?.Cleanup()` (the null-conditional closes a real NRE that RC-C's null
  `ItemController` can produce here too), and replace `.Clear()` (`:1018`) with `_itemGroupsToMove = null;`
  or an empty array.
- `EmailsToMove` (`:150`), `MoveEmailsAsync` (`:2209`) and `GetMoveDiagnostics` (`:2284-2285`) work
  unchanged; `:2209` uses the LINQ `Count()` extension and may be simplified to the `.Count` property.
- The owned test `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:66-71` must construct a
  `List<QfcItemGroup>` instead of a `ConcurrentDictionary`.

`ImmutableArray<QfcItemGroup>` was considered and rejected: it would add a package dependency to
`QuickFiler` and buys nothing, since the write inventory shows no concurrent mutation at all.

**Step 5 — #473 defect 2: guard the null at the boundary; return after the first catch; let
cancellation propagate.**

In `TryMoveEmailByGroupAsync` (`:2236-2258`):

1. Add `catch (OperationCanceledException) { throw; }` **ahead of** the broad catch at `:2242`.
2. Return early after the first catch so execution never reaches `:2247` (the second dereference of
   the same null). This is the potential document's stated preference
   (`2026-08-07-…-background-task-and-catch-defects.md:97-98`).
3. Guard the `null` group that `TryGetItemGroupByIndex` can return (`:2260-2270`, now bounds-checked
   by step 4) at the boundary in `TryMoveEmailByGroupIndexAsync` (`:2230-2234`) rather than letting it
   reach the dereference at `:2240`.

**Step 6 — #469 defects 1 and 2: one edit pass over `GetMoveDiagnostics` (`:2272-2328` pre-renumber).**

1. Size the array to `_itemGroupsToMove.Count` at `:2284` (defect 2).
2. Move the `qf is not null` guard above the first dereference so the branch currently at
   `:2318-2322` becomes reachable and produces the intended
   `"To Unknown,Sender Unknown,Email,Folder Unknown,…"` line for a null `ItemController`
   (defect 1). Use the same guard-first idiom as step 9.

**Step 7 — #470 defect 2: reconcile the counts in `ToggleUnGroupConv`.**

Placed before step 8 (#470 defect 3) because it removes the primary *producer* of the null
`ItemController` that step 8 defends against; step 8's regression test then needs an explicitly
injected null rather than one produced by the expansion path.

1. Extract the member resolution out of `EnumerateConversationMembers` into a pure static helper on
   the controller, containing exactly the current `:1883-1886` expression, for example
   `internal static IReadOnlyList<MailItem> ResolveConversationInsertions(ConversationResolver resolver, string entryID)`.
   Being static and taking the resolver as a parameter, it is directly unit-testable.
2. In `ToggleUnGroupConv`, call it **once, before `MakeSpaceForItems`** (`:1827`).
3. Compare against the caller-supplied reservation
   (`int expected = conversationCount - 1;`) and derive `int insertCount = insertions.Count;` as the
   **single source of truth**.
4. Pass `insertions` into `EnumerateConversationMembers` instead of having it re-resolve, and make the
   previously-dead `conversationCount` parameter (RC-I) genuinely consumed by the reconciliation.
5. Guard `baseEmailIndex == -1` (`:1819-1821`) before `_itemGroups[insertionIndex - 1]` at
   `:1900-1902`, using the same idiom as step 9's `-1` guard.

**Behaviour on disagreement — decision, with its rationale.** Log at `Warn` with full context, then
proceed using `insertions.Count`. **Do not throw.** The general code-change policy prefers failing
fast, but this method sits directly on a UI event path
(`QuickFiler/Controllers/QfcItemController.MailActions.cs:36-52`, invoked from a context-menu action),
and an exception here propagates to the VSTO UI thread. The repository has already made exactly this
trade once, in `QuickFiler/Helper Classes/ConversationResolver.Loading.cs:41-50`, whose comment reads:
"Throwing here propagated an unhandled exception to the VSTO UI thread for a recoverable scenario."
Following that precedent is the consistent choice. Deriving `insertCount` from `insertions.Count`
makes **both** failure branches structurally impossible, so the log is diagnostic rather than
load-bearing. The log message must carry `entryID`, `conversationCount`, `insertions.Count`,
`resolver.Count.SameFolder`, `resolver.Count.Expanded`, and `baseEmailIndex`, because those are the
values needed to decide later whether the resolver or the DataFrame filter is at fault.

Re-reserving (calling `MakeSpaceForItems` a second time for the delta) is rejected: it doubles the TLP
mutation surface for no benefit once both counts come from one source. Clamping the loop is rejected
by the potential document itself (`…conversation-index-defects.md:103-104`).

**Unresolved point, stated rather than invented.** The test-harness research
(`research/test-harness-feasibility.md` §3.5) recommends that the post-fix reconciliation surface
disagreement as "an explicit typed exception" **before** the loop, which is what makes its
above-the-reservation test case COM-free; the defect research (§5.5) recommends a `Warn` log and
proceeding. **This spec adopts the log-and-proceed decision** for the reasons above, and requires the
planner to design the above-the-reservation test against that decision — asserting the reconciled
`insertCount` and the emitted diagnostic rather than an exception. If the planner concludes that no
deterministic COM-free assertion exists under log-and-proceed, it must record that in the
fail-before dossier rather than silently switching the production behaviour to throw.

**Step 8 — #470 defect 1: guard `indexOriginal == -1`.**

Guard in `PromoteFirstChild` (`:1972-1975`) and at the `ChangeConversationSilently(indexOriginal, true)`
call (`:1749`, which subscripts at `:1716`). Write both with the same idiom as step 7's
`baseEmailIndex` guard. `PromoteFirstChild` is `public` (`:1970`) but is **not** declared on
`IQfcCollectionController`, and its only caller is `:1745`; the planner must decide and document
whether the guarded path returns a sentinel (`-1`) or throws an explicit typed error — the potential
document (`…conversation-index-defects.md:59`) requires only that a `-1` be "handled explicitly rather
than used to subscript."

**Step 9 — #470 defect 3: make `SetVisualDigits`'s reads consistently guarded.**

In the `_itemGroups.ForEach` lambda at `:138-143`, the correct minimal fix is to **skip the group
entirely when `ItemController` is null** (and when `ItemViewer` is null). Guarding only `:140` is
insufficient: execution then reaches `:141`, which dereferences `grp.ItemViewer.LblItemNumber`, and
`ItemViewer` is also null in the same arrangement, producing a different `NullReferenceException`.
Constructing a real `ItemViewer` is not an option (`QuickFiler/Viewers/ItemViewer.cs:21` is a
`UserControl` with a WebView2 surface).

`SetVisualDigits` is live — called from `:1200`, `:1335`, `:1841`, `:1938`.

**Step 10 — #471: remove the double inversion in `EliminateSpaceForItems`.**

`:2017` assigns a negative magnitude and `:2020` and `:2025` subtract it. Fix by **negating `:2017`
or by changing `:2020`/`:2025` to `+`, not both** (`…eliminate-space-sign-error.md:81-82`). The
resulting behaviour must mirror `MakeSpaceForItems` (`:2029-2042`) in the opposite direction.

`EliminateSpaceForItems` has exactly one call site solution-wide (`:1779`, inside
`ToggleGroupConv(int, int)`) and is declared on `QuickFiler/Interfaces/IQfcCollectionController.cs:47`,
so removal is not an option; only the sign is wrong.

**Seam (see `## Test Strategy`):** extract the shared arithmetic into a pure
`internal static Size ShrinkByRows(Size current, float templateHeight, int removalCount)` used by both
`:2018-2026` and `:2031-2034`.

**Step 11 — #473 defect 1: atomic-swap drain for `BackgroundLoadingTasks`; narrow the field.**

Placed late so the `LoadControlsAndHandlers_01Async` bodies stay untouched while the rest of the file
is being edited.

- Extract the two byte-identical statement pairs at `:398-399` and `:492-493` into one member
  (the seam; see `## Test Strategy`) and apply the fix there once:

  ```csharp
  // Capture, then publish a fresh bag BEFORE awaiting, so any concurrent Add lands in the new bag.
  var pending = Interlocked.Exchange(ref BackgroundLoadingTasks, new ConcurrentBag<Task>());
  await Task.WhenAll(pending);
  ```

  `Interlocked.Exchange` makes the swap atomic, so an `Add` racing the swap targets either the old bag
  (awaited now) or the new bag (awaited by the next drain) — never a dropped bag. Awaiting a local
  cannot be invalidated by a later field reassignment. Note that `Interlocked.Exchange` may not bind a
  target-typed `[]` collection expression on every compiler version; use `new ConcurrentBag<Task>()`
  explicitly. A drain **loop** (repeat while the swapped-out bag is non-empty) is the shape that
  satisfies the test in `## Test Strategy`.
- Narrow `BackgroundLoadingTasks` (`:80`) from `internal` to `private` if and only if the test seam
  does not require `internal` access. The field has no consumer outside the file and appears in no
  test today; if the regression test drives the extracted drain method, that method's accessibility
  governs, not the field's.

Rejected alternative: a `List<Task>` under a `lock`. More code, makes every `Add` contended, and only
relocates the drain/reset ordering question inside the lock.

**Step 12 — #469 defect 4: document the `stackMovedItems` contract. Do NOT remove the parameter.**

**Scope-bound decision.** Removing the parameter is a source-breaking change that forces a one-token
edit at `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225`, which is **not** in the owned
file set. The in-scope resolution is therefore:

- Add an XML doc comment on `QuickFiler/Interfaces/IQfcCollectionController.cs:50` (and on the
  implementation at `:2206`) stating that the undo stack is populated by
  `EmailFiler.PushToUndoStack` (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:185-189`)
  onto the same `SloStack<IMovedMailInfo>` instance the caller passes, and that the parameter is
  retained only for source compatibility.
- Make the parameter genuinely consumed (a discard `_ = stackMovedItems;` or an argument-null guard)
  so the "silently ignored parameter" reading is removed.

This removes the defect (the reader now knows why the parameter exists) at zero blast radius. Full
removal is recorded in `## Follow-up Candidates`.

**Step 13 — #474 defect 2: split, do not relocate.**

**Scope-bound decision.** `ReadyForMove` has exactly one consumer solution-wide,
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:121`
(`else if (_groups?.ReadyForMove == true)` inside `ActionOkAsync`, `:110-134`), which is **not** an
owned file. The `MessageBox` at `:186-191` is the **only** user-facing feedback when a group has no
destination folder — `ActionOkAsync` simply falls through the `else if` and does nothing visible — so
deleting the dialog without relocating it is a user-visible behaviour regression, and relocating it
requires editing a non-owned file.

The in-scope resolution is a behaviour-preserving split inside the owned file:

```csharp
/// <summary>Pure readiness check. Produces the notification text without presenting it.</summary>
internal bool TryGetMoveReadiness(out string notifications)
```

containing exactly the current `:156-184` logic, with the property reduced to:

```csharp
public bool ReadyForMove
{
    get
    {
        if (TryGetMoveReadiness(out var notifications)) { return true; }
        _notifyNotReady(notifications);
        return false;
    }
}
```

where `_notifyNotReady` is an injectable `Action<string>` defaulting to the exact existing
`MessageBox.Show(msg, "Error Notification", MessageBoxButtons.OK, MessageBoxIcon.Error)` call, in the
shape of the in-file precedent at `:1060-1074`. Production behaviour is bit-for-bit identical; a test
injects a recording delegate and asserts both the returned `false` and the captured message text.

`TryGetMoveReadiness` is **not** added to `IQfcCollectionController` in this feature; the contract
change belongs with the follow-up that relocates the presentation.

---

### Boundaries and invariants to preserve

- **No behaviour change from any seam.** Each of the three seams must default to the exact prior call
  and be landed in a step that changes no observable production behaviour, verified by the existing
  `QuickFiler.Test` suite passing unchanged, **before** the defect fix lands on top of it.
- **No user-visible regression from #474 defect 2.** The `MessageBox` must still appear in production
  for a group with no destination folder.
- **No new public API.** Six `public` members are removed by step 1; nothing `public` is added.
  Seams are `internal` or `private`.
- **`IQfcCollectionController` member set is unchanged** except for XML doc comments (step 12).
- **`EliminateSpaceForItems` and `MakeSpaceForItems` remain symmetric** after step 10.
- **No temporary files, no live Outlook, no visible UI, no message pump** in any test.

### Dependencies and blocked work

- **Depends on nothing.** Issue #454's injectable delegate seams do **not** exist on this base; this
  feature creates its own.
- **Blocks sibling issue #444**, which merges after this feature on the epic integration branch. See
  `## Downstream Notes for Sibling Issues`.

### Files / modules to change

| File | Change |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | All fifteen fixes and all three seams |
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | XML doc comment on `MoveEmailsAsync` (`:50`) only |
| `QuickFiler/Controllers/IQfcFormController.cs` | **No change expected.** Owned defensively; the retype in step 2 consumes this interface without modifying it |
| `QuickFiler/Interfaces/IFilerFormController.cs` | **No change expected.** Owned defensively; option (a) for #474-1 was rejected |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | `_itemGroupsToMove` injection at `:66-71` becomes a `List<QfcItemGroup>` (step 4). **No new test methods** — the file is at the 500-line cap |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` | `Mock<IFilerFormController>` → `Mock<IQfcFormController>` at `:45` (step 2) |
| New test file(s) under `QuickFiler.Test/Controllers/` | All new regression tests |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `Compile Include` entries for the new test file(s), at the exact insertion point in `## Test Strategy` |

### Error handling and logging updates

- Step 5 replaces a fall-through broad catch with an early return and adds an
  `OperationCanceledException` re-throw clause ahead of it.
- Step 4 replaces a broad `catch (System.Exception)` (`:2266-2269`) with an explicit bounds check.
- Step 7 adds one `Warn`-level log carrying six named values (listed in step 7).
- Step 3 preserves the existing `logger.Error` race-condition message at `:1239-1241` unchanged; the
  fix makes it stop firing spuriously rather than removing it.
- No new logging framework, appender, or package reference is added.

### Rollback considerations

No feature flag. Rollback is branch revert. The change is confined to one production file plus a
doc-comment edit on one interface, so a revert is mechanical.

### Backward-compatibility expectations

- Six `public` members disappear from the `QuickFiler` assembly (step 1). No consumer exists outside
  `TaskMaster` and `QuickFiler.Test`, both in-repository and both verified caller-free.
- `QfcCollectionController`'s constructor parameter 5 changes type from `IFilerFormController` to
  `IQfcFormController` (step 2). This is a source-breaking change for any caller that passes a
  non-`IQfcFormController`; no such caller exists.
- `IQfcCollectionController`'s member signatures are unchanged.

### Performance constraints

None applicable. Step 4 improves `TryGetItemGroupByIndex` from O(n) `ElementAt` to O(1) indexing and
removes an exception-based control path. No other change is on a hot path.

---

## Assumptions, Constraints, Dependencies

**Assumptions**

- The `*.cs`-only reference search behind #468 is complete. Not proven for non-`.cs` and reflective
  callers; see RC-J residual risk and the verification step required in `## Acceptance Criteria`.
- `QfcItemGroup`'s members are `internal` (`QuickFiler/Controllers/QfcItemGroup.cs:26,32,39,50`) and
  `QuickFiler/Properties/AssemblyInfo.cs:5` declares `[assembly: InternalsVisibleTo("QuickFiler.Test")]`,
  so tests can construct and populate groups directly.

**Constraints**

- Owned/forbidden file lists in `## Scope & Non-Goals` are binding.
- MSTest, Moq, FluentAssertions (CLAUDE.md §CUT1, §CUT2).
- CLAUDE.md Bugfix Workflow: a failing regression test comes **first** for every defect, before its
  fix.
- No temporary files in tests (CLAUDE.md §UT4; currently approved exceptions: none).
- No `Thread.Sleep`, `Task.Delay`, or wall-clock waits in tests
  (`.claude/rules/general-unit-test.md`, "Banned APIs in test code").
- 500-line file cap (`.claude/rules/general-code-change.md`). `QfcCollectionControllerTests.cs` is
  **exactly 500 lines** and cannot receive new methods; every new test file must also stay under 500
  lines.
- C# toolchain order: `dotnet tool run csharpier format .` → msbuild analyzers (`/t:Rebuild`) →
  msbuild nullable (`/t:Rebuild`) → `vstest.console.exe`. Restart from step 1 on any failure or
  auto-fix.

**External dependencies**

None. No new NuGet package is added to any project.

---

## Data / API / Config Impact

- **User-facing changes:** the conversation-collapse panel-height regression (#471) is corrected; the
  "no destination folder" `MessageBox` continues to appear unchanged.
- **Data changes:** `GetMoveDiagnostics` stops appending a spurious blank line to the metrics CSV via
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs:75` → `UtilitiesCS/To Depricate/FileIO2.cs:41-47`
  → `:99`, and stops inserting a `null` element into the metrics `BlockingCollection<string>` via
  `Metrics.cs:144` → `:190-199` → `:211`. Neither consumer filters nulls today. Existing metrics files
  are not migrated; only new output changes.
- **API changes:** as listed under "Backward-compatibility expectations".
- **Logging changes:** one new `Warn` message (step 7). The #473 defect 2 fix reduces two `logger.Error`
  entries per root failure to one and stops logging cancellations as errors.
- **Config changes:** none. No CLI flag, config schema, or version is affected.

---

## Test Strategy

### Framework, policy, and the two established construction patterns

MSTest + Moq + FluentAssertions. Tests must be independent, isolated, fast, and deterministic, must
not touch the filesystem or a live Outlook, must not display UI, and must not use timers or sleeps.

Two construction patterns already exist in the owned test files. **The planner must reuse these
rather than re-deriving them.**

**Pattern A — `FormatterServices.GetUninitializedObject` + reflection field injection.**
Established in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`; the class docstring at
`:17-22` states the rationale (the constructor requires WinForms UI components). Canonical shape at
`:36-37`:

```csharp
var controller = (QfcCollectionController)
    FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
```

Three purpose-built builders exist and are reusable: `CreateControllerWithOneGroup` (`:30-74`),
`CreateControllerWithGroups` (`:142-183`), `CreateControllerForSwap` (`:338-365`).

Two pitfalls the planner must design around:

1. The existing field-injection helper at `:380-383` uses `?.SetValue(...)`, so a typo in a field name
   silently no-ops. **New helpers must follow the asserting form** at
   `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:37-47`, which asserts
   `field.Should().NotBeNull(...)` before setting.
2. `GetUninitializedObject` bypasses field initializers, so `_digits` is `0` rather than its declared
   `1` (`QuickFiler/Controllers/QfcCollectionController.cs:113`), and the `Digits` getter (`:114-128`)
   then sets `_digitRefreshNeeded = true`, routing `RegisterNavigation` into the WinForms-bound
   `SetVisualDigits` path (`:1332-1336`). Documented at
   `QfcCollectionControllerTests.cs:334-337`. Any test reaching `RegisterNavigation`,
   `UnregisterNavigation`, or `RemoveSpecificControlGroupAsync` must inject `_digits = 1` unless it
   *wants* that path. `_moveMonitor` (`:78`) and `BackgroundLoadingTasks` (`:80`) are likewise `null`
   on an uninitialized instance.

**Pattern B — the real eight-parameter constructor with all-mocked dependencies.**
Established in `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:31-60`. Every
constructor parameter is an interface or a side-effect-free concrete type; the two WinForms-typed
reads in the constructor body (`QuickFiler/Controllers/QfcCollectionController.cs:44-45`) come from
the mocked `IQfcFormViewer` and return `null` under a default mock. Use this pattern **only** when the
test must observe constructor-installed state.

**Apartment model.** Neither existing file uses `[STATestClass]`, `[STAThread]`, or a pump, and all
Tier-1 tests below run MTA. `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:26-69` exists but **no
defect in this feature requires it**.

### Per-defect testing approach

**Tier 1 — directly testable, no production seam. Write these first.**

| Defect | Test target and arrangement | Fail-before mechanism |
|---|---|---|
| #469-2 | Reuse `CreateControllerWithOneGroup` verbatim; assert `result.Length.Should().Be(1)`. Add a three-group variant asserting `Length == 3` and `Should().NotContainNulls()` | Length is `Count + 1` with a trailing `null` (`:2284`) |
| #469-1 | `CreateControllerWithOneGroup` but with a `QfcItemGroup` **without** an `ItemController` injected into `_itemGroupsToMove`. Post-fix, assert no throw **and** that `result[0]` contains the `"To Unknown,Sender Unknown,Email,Folder Unknown,…"` text from `:2320-2321` — proving the dead branch became live. `ref AppointmentItem olAppointment` may be `null`, as at `QfcCollectionControllerTests.cs:90`, `:118` | `NullReferenceException` at `:2289` |
| #286 | Uninitialized controller, so `_itemGroups` is `null`; `:1161` increments, then `:1162` calls `UnregisterNavigation()`, whose first statement `for (int i = 0; i < _itemGroups.Count; i++)` (`:1345`) throws `NullReferenceException` unambiguously *after* the increment. Read the counter by reflection: `typeof(QfcCollectionController).GetField("removespecificcontrolgroupcounter", BindingFlags.NonPublic \| BindingFlags.Static)`. **`[TestInitialize]` and `[TestCleanup]` must reset the static field to `0`** — it is process-wide shared state and the General Unit Test Policy requires independence. Add a **second** test forcing a throw later in the body (inject `_itemGroups` with one group whose `Mock<IQfcItemController>` throws from `IsActiveUI`, `:1165`) so the `finally` is proven to cover the whole span, not just the first statement | Counter is left at pre-call + 1 |
| #470-1 | Uninitialized controller with `_itemGroups` holding one or two groups whose `Mock<IQfcItemController>` returns `ConvOriginID = null` and whose `Mail` is a `Mock<MailItem>` with a non-matching `EntryID`. Test 1: `PromoteFirstChild("missing", ref childCount)` directly. Test 2: end-to-end through `ToggleGroupConv("missing")`, which also exercises the second `-1` consumer at `:1749`. Critical ordering fact: `_itemTlp` is not touched until `:1976`, **after** the `_itemGroups[-1]` subscript at `:1975`, so no WinForms control is needed | `ArgumentOutOfRangeException` at `:1975` |
| #470-3 | Uninitialized controller, `_itemGroups = new List<QfcItemGroup> { new QfcItemGroup() }` (so `ItemController` is `null` and `EmailsLoaded == 1`, passing the `> 0` guard at `:132`). `SetVisualDigits` is `private`, so invoke by reflection in the shape of `QfcItemController.TestSupport.cs:66-80`; because reflection wraps the throw, assert `.Should().Throw<TargetInvocationException>().WithInnerException<NullReferenceException>()` or unwrap explicitly. Post-fix assert no throw **and** no viewer text written | `NullReferenceException` at `:140` |
| #473-2 (cancellation half) | Inject one group whose `Mock<IQfcItemController>.Setup(c => c.MoveMailAsync()).ThrowsAsync(new OperationCanceledException())`; `await controller.Invoking(c => c.MoveEmailsAsync(null)).Should().ThrowAsync<OperationCanceledException>()` | Swallowed by the broad catch at `:2242` |
| #473-2 (double-log half) | **Logger-free observable proxy.** Arrange a group whose `ItemController` is `null` (so `:2240` throws and the outer catch is entered) and whose `MailItem` is a `Mock<MailItem>` with `SetupGet(x => x.Subject).Throws(new COMException())`. Post-fix assert `mockMail.VerifyGet(x => x.Subject, Times.Never())` — proving the second dereference does not occur. Cover the true *null group* path separately with a "does not throw" test through `MoveEmailsAsync` after forcing `TryGetItemGroupByIndex` to return `null` | `:2247` reads `Subject` after the first catch |
| #470-2 | `EnumerateConversationMembers` is `public` (`:1875`); `new ConversationResolver(globals, mailItem)` is already constructed by an existing test at `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs:75`, and `ConversationItems` has a public setter (`QuickFiler/Helper Classes/ConversationResolver.Loading.cs:171-176`) that bypasses the lazy COM path. Members are `Mock<MailItem>` with `EntryID` and `SentOn` set up. **Caveat: the loop body needs COM** (`InitializeGroup` at `:1894` → `LoadItemViewer_03` `:1851` → `ItemViewerQueue.Dequeue` `:958` → a real `QfcItemController` `:1853-1862`), so all three cases must be arranged so **no iteration executes**: above (`_itemGroups.Count == insertionIndex`, `insertions.Count == 1`), equal (`conversationCount == 1`, `insertions.Count == 0`), below (`conversationCount == 3`, `insertions.Count == 0`). Additionally test the extracted pure `ResolveConversationInsertions` helper directly | Above: `ArgumentOutOfRangeException` at `:1893`; below: silent placeholder groups with a `null` `ItemController` |
| #469-4 | Contract test: assert via reflection that `IQfcCollectionController.MoveEmailsAsync`'s parameter list is unchanged and that `MoveEmailsAsync(null)` behaves identically to `MoveEmailsAsync(stack)` for an empty `_itemGroupsToMove`. `new SloStack<IMovedMailInfo>()` is constructible in memory with no filesystem access (`UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStack.cs:31-35`) | See the dossier note below |

**Tier 2 — a small production seam is required first.** Each seam is named here so the planner does
not invent a different one:

| Defect | Seam (exact) | Size |
|---|---|---|
| #473-1 | `internal async Task DrainBackgroundLoadingTasksAsync()` on `QfcCollectionController`, called from `:398-399` and `:492-493` | Extract-method over two byte-identical statement pairs; also removes duplication |
| #474-2 | `internal bool TryGetMoveReadiness(out string notifications)` containing `:156-184`, plus `private Action<string> _notifyNotReady` defaulting to the existing `MessageBox.Show(...)` call at `:186-191`, mirroring `:1060-1074` | One method, one field, one call-site substitution |
| #471 | `internal static Size ShrinkByRows(Size current, float templateHeight, int removalCount)` shared by `:2018-2026` and `:2031-2034` | One pure static helper |

**#473-1 deterministic interleaving with no sleeps.** Continuations registered on a `Task` run in
registration order, and `TaskContinuationOptions.ExecuteSynchronously` runs them on the completing
thread before control returns. That yields a timing-free construction of the reset window:

1. `var gate = new TaskCompletionSource<bool>(); var late = new TaskCompletionSource<bool>();`
2. `controller.BackgroundLoadingTasks.Add(gate.Task);`
3. Register, **before** starting the drain, a synchronous continuation on `gate.Task` that adds
   `late.Task` to `controller.BackgroundLoadingTasks`. Registered first, it runs before the
   continuation `Task.WhenAll` installs, so the add lands while the *old* bag is still current.
4. `Task drain = controller.DrainBackgroundLoadingTasksAsync();`
5. `gate.SetResult(true);`
6. Assert `drain.IsCompleted` is **false** — post-fix the drain must still be awaiting `late.Task`.
   Pre-fix the drain has already completed, having replaced the bag reference and dropped `late.Task`.
   **This is the failing assertion.**
7. `late.SetResult(true); await drain;`

No `Thread.Sleep`, `Task.Delay`, or wall-clock wait appears anywhere in this recipe.

**#474-2 test.** Inject a recording `Action<string>` into `_notifyNotReady`, read `ReadyForMove`, and
assert the returned `false` and the captured message text — with no dialog presented. The loop at
`:161-184` reads `grp.ItemController.SelectedFolder`, `.ItemNumber`, `.Mail.SentOn`, `.Mail.Subject`,
all satisfiable with `Mock<IQfcItemController>` and `Mock<MailItem>`. Assert the three header sentinel
strings at `:165-167` are each treated as "not assigned" — a genuine edge case worth covering.

**#471 tests.** The pure `ShrinkByRows` helper carries the arithmetic assertions and runs MTA. One
optional STA test carries the end-to-end panel assertion that the sign is applied correctly at the
call site, because the helper alone cannot prove that. `EliminateSpaceForItems` calls
`TableLayoutHelper.RemoveSpecificRow(_itemTlp, removalIndex, removalCount)` **first** (`:2015`), and
`RemoveSpecificRow` early-returns when `rowIndex >= panel.RowCount`
(`UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs:68-71`), so a removal index at or
beyond `RowCount` isolates the size arithmetic. It evaluates `panel.InvokeRequired` at `:62` before
that early return, which creates a window handle on the calling thread — hence STA. Repository
precedent: `UtilitiesCS.Test/HelperClasses/WindowsForms/ScreenAndTableLayoutTests.cs:41`
(`[STATestClass]`, bare `new TableLayoutPanel()` at `:47`); `UtilitiesCS.Test/test.runsettings:2-5`
records that global STA is disabled and STA is a per-class/per-method opt-in. The ratified refinement
at `docs/features/epics/winforms-testability-refactor/epic.md:62-74` permits in-memory, never-shown
WinForms controls on STA **as a last resort**, requires a dedicated `*.StaTests.cs` file marked
`[STATestClass]`/`[STATestMethod]`, forbids `Show()`/`ShowDialog()` and pump reliance, and requires
per-test disposal. If the STA test is taken, it must carry an in-file comment stating why no seam
covers the call-site sign.

**Tier 3 — no deterministic pre-fix red state; a fail-before exception dossier is required
instead.**

- **#469 defect 3 (`TryGetItemGroupByIndex` ordering stability).** A test asserting "index `i`
  resolves to a different group after a mutation" is **flaky by construction**:
  `ConcurrentDictionary` enumeration order is unspecified and, with reference-type keys under the
  default comparer, is a function of runtime identity hash codes that differ between processes. Such a
  test would violate `.claude/rules/general-unit-test.md`. The two-part substitute:
  1. **Structural fail-before guard (deterministic).** Assert that the declared type of
     `_itemGroupsToMove` is assignable to an ordered contract (`IList` / `IReadOnlyList<>`). This
     fails deterministically before the fix and passes after. Same species of guard as
     `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:17-36`, which is established repository
     practice.
  2. **Behavioural contract test (deterministic post-fix only).** Build `[A, B, C]`, remove `B`, add
     `D`, and assert index resolution yields `A, C, D`.
  The dossier must record that part 2 has **no deterministic pre-fix red state**, citing the
  `ConcurrentDictionary` unspecified-order reason.
- **#468 (dead-code removal).** No new test is appropriate. The correct verification is that the
  solution compiles and the existing `QuickFiler.Test` suite is green, plus the non-`.cs` and
  reflective-caller search required by RC-J.
- **#474 defect 1 (downcast removal).** The proof is compile-time. A test substituting a
  non-`QfcFormController` `IFilerFormController` would assert `InvalidCastException`, but the call
  site at `:1232` is unreachable without `UiThread.Dispatcher` (`:1226`), which is `null!` until
  `UiThread.Init()` runs, and `Init()` calls `.Show()` on a form
  (`UtilitiesCS/Threading/UiThread.cs:48-79`, `:54`) — prohibited outright. Add a reflection assertion
  that `QfcCollectionController`'s constructor parameter 5 and the `_parent` field are both typed
  `QuickFiler.Controllers.IQfcFormController`; record the absence of a runtime red state in the
  dossier.
- **#469 defect 4** under the document-the-contract resolution has no behavioural red state. The
  dossier must record that the resolution is a contract-documentation change, with the triage evidence
  from RC-I.

### New test files and the exact csproj insertion point

`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is **exactly 500 lines** — at the repo
file-size ceiling in `.claude/rules/general-code-change.md` — so **new test methods cannot be added to
it**. `QfcCollectionControllerDarkModeTests.cs` is 155 lines but is topically wrong for these defects.
New file(s) are therefore required:

1. `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` — MTA, the bulk of the
   Tier-1 and Tier-2 tests. If it would exceed 500 lines, split by defect family into a second file
   (for example `…Defects468MoveTests.cs`) rather than exceeding the cap.
2. `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs` — **only if** the #471 STA
   call-site test is taken. The `*.StaTests.cs` suffix is mandatory per
   `docs/features/epics/winforms-testability-refactor/epic.md:68-70`.
3. Optionally `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` for the asserting
   reflection helpers and shared builders, if more than one new file is created.

**Exact insertion point.** The item group spans `QuickFiler.Test/QuickFiler.Test.csproj:57-175` and is
**not alphabetical**; it is grouped by class family. A new `QfcCollectionController*` entry belongs
**immediately after line 117**, between these two lines (verbatim, including leading indentation):

```
    <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
    <Compile Include="Controllers\QfcDatamodelTests.cs" />
```

That is, the new element becomes line 118, pushing `QfcDatamodelTests.cs` to 119. **This item group is
shared with sibling epic children, so the insertion point must be exact** — it keeps the new file
inside the `QfcCollectionController` family block and minimises the merge-conflict surface.

### Sequencing (CLAUDE.md Bugfix Workflow)

1. Add the asserting reflection helpers and the Tier-1 tests. **Confirm each is red for the stated
   reason before touching production code.**
2. Land the three Tier-2 seams as behaviour-preserving steps, each verified against the unchanged
   existing suite.
3. Add the Tier-2 tests; confirm red.
4. Apply the defect fixes in the fix order in `## Proposed Fix`; confirm green.
5. Write the fail-before exception dossier for the Tier-3 items.

### Coverage impact

`QfcCollectionController` carries `[ExcludeFromCodeCoverage]` at
`QuickFiler/Controllers/QfcCollectionController.cs:21`, so **none of these tests moves any coverage
number for this file**. That is acceptable for a bugfix branch, but it means the coverage gate cannot
serve as evidence that the fixes are exercised. **The PR body must cite specific test names per
defect instead.** Removing the attribute is out of scope and belongs with the decomposition follow-up.

### Toolchain commands

Run in this exact order; restart from step 1 on any failure or auto-fix:

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

### Manual validation

Not required for merge; recorded for the release notes. The potential documents each name an
integration scenario: exercise the full QuickFiler load, conversation-expansion, and move flows after
the #468 removal; expand and collapse a conversation whose original message was filed in a previous
step (#470); collapse and re-expand a conversation repeatedly and confirm the panel height returns to
its original value (#471); cancel a move mid-flight and confirm it is reported as cancelled (#473);
attempt a move with a group that has no destination folder and confirm the prompt still appears
(#474).

---

## Downstream Notes for Sibling Issues

### For issue #444 (`QuickFiler/Controllers/KbdActions.cs` — NOT edited by this feature)

1. **This feature removes `WireUpKeyboardHandler`** (`QuickFiler/Controllers/QfcCollectionController.cs:1254-1273`,
   step 1). That member builds a `List<KaKey>` containing `Keys.Down` at both `:1269` and `:1270` and
   is the **only site in the solution** that constructs a `KbdActions<Keys, KaKey, Action<Keys>>`
   containing a duplicate key. Removing it **resolves #444's duplicate-registration defect as a side
   effect**. #444 may find its reproduction case gone after this feature merges.

2. **The duplicate is SILENT, not an exception.** The collection constructor
   `KbdActions(IEnumerable<UClass> list)` (`QuickFiler/Controllers/KbdActions.cs:26-29`) performs a
   bare `new List<UClass>(list)` with **no duplicate check**, whereas both `Add` overloads
   (`KbdActions.cs:90-104` and `:106-121`) do check and throw `ArgumentException`. That asymmetry is a
   hole in `KbdActions`' own invariant and is a **real defect in a file this feature must not touch**.
   #444 should consider hardening `KbdActions(IEnumerable<UClass>)` to apply the same duplicate check
   as `Add`, so the invariant holds for future callers.

3. **Production key wiring is unaffected.** Keys are wired through `WireUpAsyncKeyboardHandler`
   (`:1275-1280`) and `RegisterAsyncKeyActions` (`:1282-1291`), which register `Keys.Up` and
   `Keys.Down` exactly once each.

### Unfiled defect recorded for a future issue

**`EnumerateConversationMembers` never reads its `conversationCount` parameter.** Declared at
`QuickFiler/Controllers/QfcCollectionController.cs:1875-1881`; the body (`:1883-1921`) reads
`entryID`, `resolver`, `insertionIndex`, and `folderList` only. This is the same dead-parameter shape
as #469 defect 4 and it is the **direct cause** of #470 defect 2 — the reservation count is passed in
and discarded, so the method has no way to detect disagreement. **This feature makes the parameter
live** as part of step 7, so no separate issue is required for the parameter itself; it is recorded
here because it is a finding outside the seven filed documents.

### Deferred observation — unsynchronized undo handoff (out of scope for all seven issues)

`MoveMailAsync` only *enqueues* the filer
(`QuickFiler/Controllers/QfcItemController.MailActions.cs:111`) and returns `Task.CompletedTask`
(`:112`); the push onto the undo stack happens later on the queue's worker. So when
`BackGroundMoveAsync` proceeds to `WriteMetrics`
(`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:228-231`) and `CleanupBackground()`
(`:233`), the undo entries for that batch may not yet exist. This does **not** break undo — the
entries land eventually and are serialized — but it is an unsynchronized handoff. It should be filed
separately if judged worth pursuing.

---

## Follow-up Candidates

Each of the following is a real finding that is deliberately **out of scope** for this bugfix branch.
Per repository practice, each should be promoted through the potential-to-issue lifecycle rather than
left as prose in a feature folder that disappears at merge.

1. **`QfcCollectionController.cs` exceeds the 500-line cap by roughly 4.3x.** The file is 2,349 lines
   at the base commit. Step 1 removes approximately 241 lines (229 declaration lines plus 12 blank
   separators); net of the additions the other fixes introduce (a `try`/`finally`, the three seams,
   guard clauses, XML doc comments), the realistic post-feature figure is **2,120-2,180 lines** —
   still about **4.3x** the 500-line cap in `.claude/rules/general-code-change.md`. **This is a
   PRE-EXISTING condition**: the file exceeded the cap by 4.7x before any change and will exceed it by
   ~4.3x after, so this feature is cap-*improving*, not cap-*satisfying*. **Do not propose a file
   split in this feature** — seven defect fixes plus a type decomposition in one branch would make the
   diff unreviewable, would destroy the ability to attribute a regression to a specific defect fix,
   and conflicts with CLAUDE.md's Bugfix Workflow ("Change only what is needed… avoid opportunistic
   refactors"). A follow-up should be scoped as a genuine decomposition, not a mechanical `#region`
   split: the existing regions are `UI Add and Remove QfcItems` (`:251-1250`), `Event Wiring`
   (`:1252-1387`), `UI Select QfcItems` (`:1389-1704`), `UI Conversation Expansion` (`:1706-1987`),
   `Helper Functions` (`:1989-2109`), `UI Light Dark` (`:2111-2174`), `Major Actions` (`:2176-2347`),
   and the first alone is ~1,000 lines. The follow-up should be scheduled together with removing
   `[ExcludeFromCodeCoverage]` (`:21`) from whichever extracted parts become testable, otherwise the
   split buys nothing measurable.

2. **Remove the `stackMovedItems` parameter entirely.** The full fix deletes it from
   `QuickFiler/Interfaces/IQfcCollectionController.cs:50` and
   `QuickFiler/Controllers/QfcCollectionController.cs:2206` and updates the single call site at
   `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225` (`MoveEmailsAsync(_movedItems)` →
   `MoveEmailsAsync()`). It is deferred because the call site is outside the owned file set and the
   change buys only signature tidiness.

3. **Relocate the `ReadyForMove` presentation to the caller.** Add
   `bool TryGetMoveReadiness(out string notifications)` to `IQfcCollectionController`, remove the
   dialog from the getter, and move the `MessageBox` into `ActionOkAsync`'s `else` branch in
   `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`. This is the potential document's
   preferred end state (`…coupling-and-modal-getter.md:52-54`). Deferred because it edits a non-owned
   file and changes an interface contract. **Unresolved prerequisite:** research did not exhaustively
   verify whether any `Mock<IQfcCollectionController>` exists in `QuickFiler.Test`; that search must be
   run before committing to the contract change.

4. **Consolidate `IFilerFormController` and `IQfcFormController`.** Option (c) from step 2's option
   table. It touches both interface files plus `QfcFormViewer.cs`, `IQfcFormViewer.cs`,
   `IFilerHomeController.cs`, `QfcHomeController.cs`, `EfcHomeController.cs`, `EfcFormController.cs`,
   and roughly eight test files. It is a refactor, not a bugfix.

5. **Remove the orphan `QuickFiler.Interfaces.IQfcFormController`**
   (`QuickFiler/Interfaces/IQfcFormController.cs:7`). It has no implementer and its only referent is
   `QuickFiler/Interfaces/IQfcHomeController.cs:9`. It is a latent name-collision trap for any future
   file placed in the `QuickFiler.Interfaces` namespace.

6. **Harden `KbdActions(IEnumerable<UClass>)`** (`QuickFiler/Controllers/KbdActions.cs:26-29`) with the
   same duplicate check both `Add` overloads perform. Belongs to sibling issue #444; see
   `## Downstream Notes for Sibling Issues`.

7. **File the unsynchronized undo handoff** described in `## Downstream Notes for Sibling Issues`.

8. **Revisit the unsynchronized plain read of `removespecificcontrolgroupcounter` at `:1237`**, noted
   in `…-counter-leak.md:56` as a secondary concern to the primary leak-on-exception defect.

9. **Settle the #468 residual risk properly** with a repository-wide search of non-`.cs` files for the
   twelve removed identifiers plus a `GetMethod(`/`InvokeMember(` search in the `QuickFiler` tree, if
   the search required by AC-16 is judged insufficient.

---

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| A reflective or non-`.cs` caller of one of the twelve removed members exists and was not found by the `*.cs`-only search (RC-J residual risk) | AC-16 requires the non-`.cs` and reflection search before merge. The removed members are ordinary instance methods on a controller with no serialization or data-binding surface, so the prior probability is low |
| Fifteen fixes in one file produce a diff too large to attribute a regression to a specific fix | The fix order is fixed and each step is disjoint; every defect except the four Tier-3 items carries a named regression test that fails before its own fix and passes after |
| A seam changes production behaviour inadvertently | Each seam defaults to the exact prior call and is landed in its own step, verified by the unchanged existing suite before the fix lands on top of it |
| The #470 defect 2 reconciliation changes user-visible conversation-expansion behaviour | Deriving `insertCount` from `insertions.Count` makes both current failure branches structurally impossible; the added `Warn` log is diagnostic. The decision not to throw follows the in-repo precedent at `ConversationResolver.Loading.cs:41-50` |
| The `[ExcludeFromCodeCoverage]` attribute means the coverage gate provides no evidence the fixes are exercised | The PR body cites specific test names per defect instead of a coverage delta |
| Step 1's public-API removal breaks an out-of-repository consumer | The `QuickFiler` assembly is referenced only by `TaskMaster` and `QuickFiler.Test` within this repository; there is no published package |
| The csproj `Compile Include` insertion conflicts with a sibling epic child | The exact insertion point (after line 117) keeps the new entry inside the `QfcCollectionController` family block, which siblings are least likely to touch |
| The optional STA test introduces flakiness | It is taken only if the pure `ShrinkByRows` helper leaves the call-site sign unproven; it must live in a dedicated `*.StaTests.cs`, dispose its panel per test, and never call `Show()`/`ShowDialog()` |

---

## Rollout & Follow-up

**Rollout.** Merge into the epic integration branch `epic/quickfiler-bug-family-integration` ahead of
sibling issue #444, which depends on this feature.

**On merge, close:** #286, #468, #469, #470, #471, #473, #474.

**Post-fix tasks.**

- Promote every entry in `## Follow-up Candidates` through the potential-to-issue lifecycle.
- Hand `## Downstream Notes for Sibling Issues` to #444 before it starts.
- Do **not** repeat #468's coverage-denominator rationale or #474's "unrelated sibling interfaces"
  premise in the PR body; both are false on this base.
- State plainly in the PR body that #473 defect 1 is **latent** under the current call graph and that
  #474 is latent in the current single-implementation configuration.

**Links.** Issue #468 — https://github.com/drmoisan/TaskMaster/issues/468. Sibling issues #286, #469,
#470, #471, #473, #474. Research: `research/qfc-collection-controller-defects.md`,
`research/test-harness-feasibility.md`.

---

## Acceptance Criteria

This section is the **sole** acceptance-criteria source for this `full-bug` feature. Each criterion is
individually verifiable from evidence.

### Per-defect behaviour

- [x] **AC-1 (#286).** `RemoveSpecificControlGroupAsync`'s `Interlocked.Decrement` executes on the
      exceptional exit path as well as the normal one, so `removespecificcontrolgroupcounter` returns
      to its pre-call value after a throw. Verified by two named MSTest tests: one forcing a throw at
      the first statement after the increment (`UnregisterNavigation()` on a `null` `_itemGroups`) and
      one forcing a throw later in the body, each reading the static field by reflection and each
      resetting it in `[TestInitialize]`/`[TestCleanup]`.
- [x] **AC-2 (#468).** The twelve members `WireUpKeyboardHandler`, `AnyOpenDropDownsAsync`,
      `LoadGroups_02cAsync`, `LoadGroups_02bAsync`, `LoadGroup_03bAsync`,
      `LoadConversationsAndFoldersAsync`, `LoadItemGroup`, `LoadSequentialAsync`,
      `LoadGroupSequential`, `CacheTlpForMove`, `SwapTlp`, `CaptureTlpTemplate`, plus the field
      `_templateTlp` and the commented reference at `:402`, are absent from
      `QuickFiler/Controllers/QfcCollectionController.cs`. Verified by a source search returning zero
      hits for each identifier in that file.
- [x] **AC-3 (#468, non-regression).** The live members `AnyOpenDropDowns` (non-async),
      `LoadItemGroupsAndViewers_02`, `LoadConversationsAndFolders_04`, `LoadSequential_5`, and
      `ActivateQueuedTlp` are still present and unmodified. Verified by a source search plus an
      empty diff hunk for each.
- [x] **AC-4 (#469 defect 1).** `GetMoveDiagnostics` returns without throwing when a group's
      `ItemController` is `null`, and the returned line for that group contains the
      `"To Unknown,Sender Unknown,Email,Folder Unknown"` text — proving the previously dead branch is
      now reachable. Verified by a named MSTest test that throws `NullReferenceException` before the
      fix.
- [x] **AC-5 (#469 defect 2).** `GetMoveDiagnostics` returns an array whose `Length` equals
      `_itemGroupsToMove.Count` and which contains no `null` element. Verified by named MSTest tests
      for a one-group and a three-group arrangement, asserting `Length` and
      `Should().NotContainNulls()`.
- [x] **AC-6 (#469 defect 3).** The `_itemGroupsToMove` field's declared type is an ordered contract
      (`IReadOnlyList<QfcItemGroup>` or an equivalent `IList`-assignable type), and
      `TryGetItemGroupByIndex` performs an explicit bounds check rather than catching
      `System.Exception`. Verified by (a) a named structural MSTest test asserting the field's
      `FieldType` is assignable to an ordered contract, which fails before the fix, and (b) a named
      behavioural MSTest test asserting that for `[A, B, C]` with `B` removed and `D` added, index
      resolution yields `A, C, D`.
- [x] **AC-7 (#469 defect 4).** `IQfcCollectionController.MoveEmailsAsync`'s `stackMovedItems`
      parameter carries an XML doc comment stating that the undo stack is populated by
      `EmailFiler.PushToUndoStack` onto the same instance the caller passes and that the parameter is
      retained for source compatibility, and the parameter is genuinely consumed in the body (discard
      or argument guard). Verified by reading the interface and implementation, plus a named MSTest
      test asserting `MoveEmailsAsync(null)` and `MoveEmailsAsync(stack)` behave identically for an
      empty `_itemGroupsToMove`. The parameter is **not** removed and
      `QfcFormController.EventHandlers.cs` is **not** edited.
- [x] **AC-8 (#470 defect 1).** `PromoteFirstChild` and `ChangeConversationSilently` handle a `-1`
      index explicitly and never use it to subscript `_itemGroups`. Verified by two named MSTest
      tests — one calling `PromoteFirstChild` directly and one driving `ToggleGroupConv(string)`
      end-to-end with no matching `ConvOriginID` — each of which throws
      `ArgumentOutOfRangeException` before the fix and does not after.
- [x] **AC-9 (#470 defect 2).** `ToggleUnGroupConv` resolves the insertion list exactly once before
      `MakeSpaceForItems`, derives `insertCount` from `insertions.Count` as the single source of
      truth, and emits one `Warn` log carrying `entryID`, `conversationCount`, `insertions.Count`,
      `resolver.Count.SameFolder`, `resolver.Count.Expanded`, and `baseEmailIndex` when the
      caller-supplied reservation disagrees. `baseEmailIndex == -1` is guarded before
      `_itemGroups[insertionIndex - 1]`. Verified by named MSTest tests for the above-, equal-, and
      below-reservation cases, each arranged so no loop iteration executes, plus a direct test of the
      extracted pure `ResolveConversationInsertions` helper. The loop is **not** clamped.
- [x] **AC-10 (#470 defect 3).** `SetVisualDigits` skips a group entirely when its `ItemController`
      (or `ItemViewer`) is `null` and does not throw. Verified by a named MSTest test that throws
      `NullReferenceException` (wrapped in `TargetInvocationException`) before the fix and asserts no
      throw and no viewer text written after.
- [x] **AC-11 (#471).** `EliminateSpaceForItems` reduces `_itemTlp.MinimumSize.Height` and
      `_itemTlp.Size.Height` by exactly `_template.Height * removalCount`, and
      `MakeSpaceForItems(i, n)` followed by `EliminateSpaceForItems(i, n)` returns
      `_itemTlp.MinimumSize.Height` to its starting value. `MakeSpaceForItems` adjusts `MinimumSize`
      only (`:2031-2034`) and never `Size`, so `Size.Height` neutrality is out of scope and is not
      asserted; the asymmetry is recorded in the P10 evidence.
      Verified by named MSTest tests against the pure `ShrinkByRows` helper (MTA) and, if taken, one
      `[STATestClass]` call-site test in `QfcCollectionControllerLayout.StaTests.cs`. The inversion is
      removed in exactly one place, not both.
- [x] **AC-12 (#473 defect 1).** A task added to `BackgroundLoadingTasks` during the drain window is
      still awaited. Verified by a named MSTest test using two `TaskCompletionSource` instances and an
      `ExecuteSynchronously` continuation (no `Thread.Sleep`, `Task.Delay`, or wall-clock wait), which
      asserts `drain.IsCompleted == false` after the gate completes — false before the fix, true
      after. The drain logic exists in exactly one place (`DrainBackgroundLoadingTasksAsync`), not two.
- [x] **AC-13 (#473 defect 2).** `OperationCanceledException` propagates out of `MoveEmailsAsync`
      rather than being swallowed by the broad catch, and a single root failure produces a single log
      entry — proven by `mockMail.VerifyGet(x => x.Subject, Times.Never())` after the first catch.
      Verified by two named MSTest tests, each red before the fix.
- [x] **AC-14 (#474 defect 1).** `QfcCollectionController`'s constructor parameter 5 and its `_parent`
      field are both typed `QuickFiler.Controllers.IQfcFormController`, and the expression
      `(QfcFormController)_parent` appears nowhere in the file. Verified by a source search returning
      zero hits for `(QfcFormController)_parent` plus a named reflection-based MSTest test asserting
      both declared types. `EfcFormController.cs` and all three production construction sites are
      unmodified.
- [x] **AC-15 (#474 defect 2).** `TryGetMoveReadiness(out string notifications)` returns `false` with a
      non-empty notification string for a group with a `null` `SelectedFolder` and `true` with an
      empty string otherwise, **without presenting any dialog**, and the `ReadyForMove` getter still
      invokes the notification in production via a delegate defaulting to the exact prior
      `MessageBox.Show` call. Verified by a named MSTest test injecting a recording `Action<string>`
      and asserting both the returned value and the captured text, including the three header sentinel
      strings at `:165-167`.

### Verification, scope, and process

- [x] **AC-16 (#468 residual risk).** A build-input-file search plus an enumerated reflective-call
      review returns no reference to any removed member: (a) a search of build-input file types only
      (`*.csproj`, `*.resx`, `*.config`, `*.xaml`, `*.json`, `*.settings`, excluding `docs/`,
      `.claude/`, `packages/`, and `TestResults/`) for the twelve removed identifiers returns zero
      hits, and (b) every `GetMethod(` / `InvokeMember(` hit across the `QuickFiler` and
      `QuickFiler.Test` trees is enumerated with a per-hit statement that its string literal is not
      one of the twelve identifiers. A repository-wide sweep is deliberately not performed:
      `LoadSequentialAsync` names three unrelated live members in `TaskMaster/AppGlobals/` and the
      feature's own documents quote every identifier, so a repository-wide zero-hit condition would be
      unsatisfiable by construction. The search commands and their verbatim output are recorded in the
      feature's evidence folder.
- [x] **AC-17 (fix order).** The commit sequence follows the fix order in `## Proposed Fix`
      (#468 dead-code removal first, then #474-1, then #286, then #469-3, then the remainder), with
      the dead-code removal isolated in its own commit so the file renumbering is a single reviewable
      hunk.
- [x] **AC-18 (bugfix workflow).** Every defect with a Tier-1 or Tier-2 regression test has that test
      committed and demonstrated failing **before** its production fix, per CLAUDE.md's Bugfix
      Workflow. The fail-before evidence is recorded in the feature's evidence folder.
- [x] **AC-19 (fail-before dossier).** A fail-before exception dossier records, with reasons, the four
      items that have no deterministic pre-fix red state: #469 defect 3's behavioural ordering test
      (`ConcurrentDictionary` enumeration order is unspecified), #468 (removal, verified by compilation
      and the existing green suite), #474 defect 1 (call site unreachable without `UiThread.Init()`,
      which shows a form), and #469 defect 4 (a contract-documentation change with no behavioural
      delta).
- [x] **AC-20 (seams are behaviour-preserving).** Each of the three seams —
      `DrainBackgroundLoadingTasksAsync`, `TryGetMoveReadiness` + `_notifyNotReady`, and
      `ShrinkByRows` — was landed in a commit that changed no observable production behaviour, with
      the pre-existing `QuickFiler.Test` suite passing unchanged at that commit.
- [x] **AC-21 (owned-file discipline).** The diff touches only the files listed under "Files this
      feature owns" in `## Scope & Non-Goals`. In particular
      `QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
      and `QuickFiler/Controllers/EfcFormController.cs` are **not** modified. Verified by
      `git diff --name-only` against the merge base.
- [x] **AC-22 (test-file constraints).** No new test method is added to
      `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (it is at the 500-line cap; its
      only change is the `_itemGroupsToMove` injection type at `:66-71`), every new test file is under
      500 lines, and each new file's `Compile Include` entry sits between the existing
      `Controllers\QfcCollectionControllerDarkModeTests.cs` and `Controllers\QfcDatamodelTests.cs`
      entries in `QuickFiler.Test/QuickFiler.Test.csproj`.
- [x] **AC-23 (test policy).** Every new test uses MSTest, Moq, and FluentAssertions; creates no
      temporary file; requires no live Outlook; displays no UI; never calls `UiThread.Init()`; and
      contains no `Thread.Sleep`, `Task.Delay`, or wall-clock wait. Any STA test lives in a
      `*.StaTests.cs` file marked `[STATestClass]`, disposes its `TableLayoutPanel` per test, and calls
      neither `Show()` nor `ShowDialog()`.
- [x] **AC-24 (toolchain).** A single clean toolchain pass completes in order —
      `dotnet tool run csharpier format .` (and `check .`), the analyzer `msbuild … /t:Rebuild …
      /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, the nullable `msbuild … /t:Rebuild
      … /p:TreatWarningsAsErrors=true`, then `vstest.console.exe … /EnableCodeCoverage` — with zero
      errors in the final pass and the full `QuickFiler.Test` suite green. The exact commands and
      their results are reported.
- [x] **AC-25 (no scope creep).** `QfcCollectionController.cs` is **not** split into partial classes,
      `[ExcludeFromCodeCoverage]` at `:21` is **not** removed, no NuGet package is added to any
      project, and the `stackMovedItems` parameter is **not** removed. Each is recorded in
      `## Follow-up Candidates` instead.
- [x] **AC-26 (downstream handoff).** `## Downstream Notes for Sibling Issues` records for #444 that
      `WireUpKeyboardHandler` is removed by this feature (resolving the duplicate-`KaKey` registration
      as a side effect), that `KbdActions(IEnumerable<UClass>)` at `KbdActions.cs:26-29` skips the
      duplicate check both `Add` overloads perform, and that `EnumerateConversationMembers` never read
      its `conversationCount` parameter.
- [x] **AC-27 (PR accuracy).** The PR body does **not** repeat #468's coverage-denominator rationale
      (invalid because of `[ExcludeFromCodeCoverage]` at `:21`) and does **not** repeat #474's premise
      that the two form-controller interfaces are unrelated siblings (false: `IQfcFormController :
      IFilerFormController` at `QuickFiler/Controllers/IQfcFormController.cs:13`). It states that #473
      defect 1 is latent under the current call graph, states that #474 is latent in the current
      single-implementation configuration, and cites specific test names per defect in place of a
      coverage delta.
- [ ] **AC-28 (issue closure).** All seven issues — #286, #468, #469, #470, #471, #473, #474 — are
      closed by the merge, each with its corresponding acceptance criteria above checked off.
- [x] **AC-29 (follow-ups filed).** Every entry in `## Follow-up Candidates` is promoted through the
      potential-to-issue lifecycle, with the resulting issue numbers recorded in the feature folder.
