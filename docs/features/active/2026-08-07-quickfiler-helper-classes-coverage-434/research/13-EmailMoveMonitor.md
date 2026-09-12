# 13 — `QuickFiler/Helper Classes/EmailMoveMonitor.cs`

Timestamp: 2026-08-07T22-05

Cluster: MOVE-MONITOR (F4, epic `quickfiler-per-file-coverage` #136, child issue #434).
Companion artifacts: `00-cluster-overview.md` (test-project wiring, Interop mocking patterns, clock
abstraction, STA infrastructure), `14-IEmailMoveMonitor.md`.

Upstream contract: per-file line coverage is measured by F1's harness (Cobertura output of
`Invoke-MSTestWithCoverage.ps1`); the authority for `ratified-exempt` classification is F1's
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Neither exists on disk yet.
No coverage run was executed for this research; every numeric figure below is either (a) an
observation from an **archived** Cobertura artifact of an unrelated feature, explicitly labelled as
indicative-only, or (b) a projection to be confirmed at execution time with F1's harness.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/EmailMoveMonitor.cs` | — |
| Exact line count | **262** | line-count of the file; matches epic.md `:280` |
| `<Compile Include>` present | **Yes** — `<Compile Include="Helper Classes\EmailMoveMonitor.cs" />` | `QuickFiler/QuickFiler.csproj:347` |
| `[ExcludeFromCodeCoverage]` | **Absent** (confirmed) — a grep for `ExcludeFromCodeCoverage` across `QuickFiler/Helper Classes/` returns no match | — |
| Assembly-internal visibility to tests | Yes | `QuickFiler/Properties/AssemblyInfo.cs:5` (`InternalsVisibleTo("QuickFiler.Test")`) |
| Types declared | 2 — `internal class EmailMoveMonitor : IEmailMoveMonitor` (`:18`), `internal class EmailMoveAction` (`:226`) | — |
| Namespace | `QuickFiler.Helper_Classes` | `:15` |
| 500-line limit | Compliant, 238 lines of headroom | — |

Standing note in the source: `:17` carries `// TODO: Determine what EmailMoveMonitor was supposed to
be used for. It is now malfunctioning. Temprorarily disabling.` The comment is stale relative to the
code — the type is live and is instantiated by three production controllers (§9). Removing or
correcting the comment is optional and touches only this file; it is not required by any acceptance
criterion.

---

## 2. Member inventory (the coverage denominator)

Decision points counted as: `if`/`else`, `switch` arm, ternary, `??`, `?.`, loop, `catch`, plus
`await` continuations noted separately. Compiler-generated closures and async state machines are
listed because AltCover/Cobertura reports them as separate `class` entries that share this file's
`filename`, so they are part of the per-file denominator that issue #136 measures.

### 2.1 `EmailMoveMonitor` (`:18-224`)

| # | Member | Signature | Line span | Decision points | Notes |
| --- | --- | --- | --- | --- | --- |
| M1 | `logger` (static field initializer) | `private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(...)` | 20-22 | 0 | Runs in the static constructor; executes on first use of the type. |
| M2 | `_marshalToSta` (field decl) | `private readonly Action<System.Action> _marshalToSta;` | 29 | 0 | No initializer; assigned in M3. |
| M3 | constructor | `public EmailMoveMonitor(Action<System.Action> marshalToSta = null)` | 38-42 | 1 (`??` at `:40`) | Calls `SetupBeforeItemMove()` at `:41`. |
| M3a | default marshal lambda (compiler-generated, cached in `<>c`) | `action => UiThread.Dispatcher.Invoke(action)` | 40 | 0 | **1 sequence point. Host-bound irreducible — see §5 and §12.** |
| M4 | `_hookedItems` (instance field initializer) | `private List<EmailMoveAction> _hookedItems = [];` | 44 | 0 | Collection-expression initializer; also the lock object. |
| M5 | `HookItem` | `public void HookItem(MailItem mail, Action<MailItem> moveAction)` | 46-61 | 0 in the outer body | Outer body is one call: `_marshalToSta(<lambda>)`. |
| M5a | `HookItem` marshaled lambda (`<>c__DisplayClass4_0`) | `() => { ... }` | 50-60 | 2 (`if` at `:56`; `Any` short-circuit) | `lock (_hookedItems)`; `(Folder)mail.Parent` cast at `:54`; `folder.EntryID` at `:55`; `folder.BeforeItemMove +=` at `:57`; `new EmailMoveAction(...)` at `:58`. |
| M5b | `HookItem` `Any` predicate (`<>c__DisplayClass4_1.<HookItem>b__1`) | `x => x.FolderEntryId == folderEntryId` | 56 | 0 | |
| M6 | `UnhookItem` | `public void UnhookItem(MailItem mail)` | 63-88 | 1 (`if (mail is null)` at `:65`) | Null guard returns before any marshal invocation. |
| M6a | `UnhookItem` marshaled lambda (`<>c__DisplayClass5_0`) | `() => { ... }` | 72-87 | 3 (`?.` at `:75`; `if (hookedItem != null)` at `:80`; `if (count == 1)` at `:82`) | `mail.EntryID` at `:74`; `mail.Parent as Folder` at `:75`; `BeforeItemMove -=` at `:83`; `Remove` at `:84`. |
| M6b/c | `UnhookItem` predicates (`<>c__DisplayClass5_1.<UnhookItem>b__1`, `b__2`) | `x => x.FolderEntryId == parentFolderEntryId`, `x => x.MailEntryId == mailEntryId` | 78, 79 | 0 | |
| M7 | `UnhookItemAsync` | `public async Task UnhookItemAsync(MailItem mail, CancellationToken cancel)` | 90-124 | 2 (`if (mail is null)` at `:94`; `if (parent is null)` at `:100`) + 1 `await` continuation at `:99` | Compiles to state machine `<UnhookItemAsync>d__6`. `cancel.ThrowIfCancellationRequested()` at `:92`. **Not a member of `IEmailMoveMonitor`; no production caller (§9).** |
| M7a | `UnhookItemAsync` marshaled lambda (`<>c__DisplayClass6_0`) | `() => { ... }` | 108-123 | 2 (`if (hookedItem != null)` at `:116`; `if (count == 1)` at `:118`) | |
| M7b/c | `UnhookItemAsync` predicates (`<>c__DisplayClass6_1`) | `x => x.FolderEntryId == parentEntryId`, `x => x.MailEntryId == mailEntryId` | 114, 115 | 0 | |
| M8 | `GetParentFolderAsync` | `private async Task<Folder> GetParentFolderAsync(MailItem mail, int remaining = 2)` | 126-183 | 4 (`if (mail is null)` `:128`; `if (comFailure is null)` `:150`; `if (remaining > 0)` `:168`; `else` `:175`) + recursion/await at `:173` | State machine `<GetParentFolderAsync>d__7`. Two `logger.Error` calls at `:170` and `:177`. **No production caller other than M7.** |
| M8a | first COM-read lambda (`<>c__DisplayClass7_0`) | `() => { try { parentFolder = mail.Parent as Folder; } catch { comFailure = e; } }` | 138-148 | 2 (`catch` at `:145`; `as` null) | |
| M8b | second COM-read lambda | `() => { try { entryId = mail.EntryID; } catch { entryId = "[Error getting EntryID]"; } }` | 156-166 | 1 (`catch` at `:162`) | |
| M9 | `UnhookAll` | `public void UnhookAll()` | 185-200 | 0 in the outer body | |
| M9a | `UnhookAll` marshaled lambda | `() => { lock { foreach ... } }` | 189-199 | 1 (`foreach` at `:193`) | `BeforeItemMove -=` at `:195`; `Clear()` at `:197`. |
| M10 | `BeforeItemMove` (field decl) | `private MAPIFolderEvents_12_BeforeItemMoveEventHandler BeforeItemMove;` | 202 | 0 | |
| M11 | `SetupBeforeItemMove` | `private void SetupBeforeItemMove()` | 204-223 | 0 in the outer body | Single assignment of an anonymous delegate. |
| M11a | the handler delegate body (`<>c__DisplayClass10_0`) | `delegate(object item, MAPIFolder moveTo, ref bool cancel) { ... }` | 206-222 | 2 (`if (item is MailItem mail)` `:208`; `if (hookedItem != null)` `:215`) | Invokes `hookedItem.MoveAction(mail)` at `:217` and `_hookedItems.Remove(hookedItem)` at `:218`. Never sets `cancel`. |
| M11b | handler predicate (`<>c__DisplayClass10_0.<SetupBeforeItemMove>b__1`) | `x => x.Mail.EntryID == mail.EntryID` | 212-214 | 0 | Reads `EmailMoveAction.Mail` (live COM), not the cached `MailEntryId`. |

Approximate total decision points for `EmailMoveMonitor` and its closures: **21**, plus 2 `await`
continuations and 1 recursion.

### 2.2 `EmailMoveAction` (`:226-261`)

| # | Member | Signature | Line span | Decision points |
| --- | --- | --- | --- | --- |
| A1 | constructor | `public EmailMoveAction(MailItem mail, Folder folder, Action<MailItem> moveAction)` | 234-241 | 0 (5 assignments; two of them are live COM reads: `mail.EntryID` `:239`, `folder.EntryID` `:240`) |
| A2 | `Mail` | `public MailItem Mail => _mail;` | 244 | 0 |
| A3 | `Folder` | `public Folder Folder => _folder;` | 247 | 0 |
| A4 | `MoveAction` | `public Action<MailItem> MoveAction => _moveAction;` | 250 | 0 |
| A5 | `MailEntryId` | `public string MailEntryId => _mailEntryId;` | 255 | 0 |
| A6 | `FolderEntryId` | `public string FolderEntryId => _folderEntryId;` | 260 | 0 |
| — | backing fields | `_mail` `:243`, `_folder` `:246`, `_moveAction` `:249`, `_mailEntryId` `:252`, `_folderEntryId` `:257` | — | 0 |

`EmailMoveAction` has **zero decision points**. Its entire coverage story is "is each member
invoked".

---

## 3. Existing test inventory

Single file: `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` (314 lines, 13,739 bytes),
declared in the project at `QuickFiler.Test/QuickFiler.Test.csproj:159`.
`[TestClass]` at `:21`, `[DoNotParallelize]` at `:22`, class `EmailMoveMonitorTests` at `:23`,
namespace `QuickFiler.Helper_Classes.Tests` at `:13`. Eight `[TestMethod]`s.

Fixture mechanics worth preserving:

- `:32-37` — reflective snapshot of the static `UiThread.Dispatcher` property, taken specifically to
  avoid a compile-time `System.Windows.Threading.Dispatcher` (WindowsBase) dependency.
- `:44-51` `[TestInitialize]` snapshots the dispatcher and resets `_marshalInvocationCount`.
- `:53-60` `[TestCleanup]` asserts the static dispatcher is unchanged — an order-independence guard.
- `:63-70` `CountingPassThrough()` — the synchronous pass-through marshal delegate that increments
  an invocation counter.
- `:72-78` `CreateMail(entryId, parent)` → `Mock<MailItem>` loose, `SetupGet(EntryID)`,
  `SetupGet(Parent)`.
- `:80-85` `CreateFolder(entryId)` → `Mock<Folder>` loose, `SetupGet(EntryID)`.

| # | `[TestMethod]` | Line | Production member(s) exercised |
| --- | --- | --- | --- |
| E1 | `HookItem_FirstItemOfFolder_SubscribesBeforeItemMoveOnce_AndSharedFolderDoesNotResubscribe` | `:88` | M3 (non-null arg), M5, M5a, M5b, A1, A6 |
| E2 | `UnhookItem_RemovesLastItemForFolder_UnsubscribesBeforeItemMoveOnlyOnLastItem` | `:108` | M5/M5a; M6, M6a (`hookedItem != null` true; `count == 1` both branches), M6b, M6c, A3, A5, A6 |
| E3 | `UnhookItem_Null_IsNoOp_NoComAccessNoMarshalInvocation` | `:135` | M6 null-guard true branch only |
| E4 | `UnhookItem_UsesCachedEntryIds_RemovesExactlyTheMatchingEntry` | `:148` | M6a `count != 1` branch, M6b/M6c predicates, A5, A6 |
| E5 | `AllComAccess_FlowsThroughInjectedMarshalDelegate` | `:177` | M5, M6, M9 marshal-count contract |
| E6 | `UnhookAll_UnsubscribesEveryFolder_AndClearsState` | `:201` | M9, M9a (`foreach` with 2 iterations), A3; post-clear `UnhookItem` no-op |
| E7 | `DuplicateHookOfSameItem_AndUnhookNeverHookedItem_DoNotThrowOrSpuriouslyUnsubscribe` | `:235` | M5a `if` false branch (duplicate folder), M6a `hookedItem == null` branch |
| E8 | `UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread` | `:267` | Thread-affinity contract of the `_marshalToSta` seam (issues #214 / #420) |

No other test file in the repository references `EmailMoveMonitor` or `EmailMoveAction` (a
repository-wide `.cs` grep returns only this file, the production file, and the three production
call sites in §9). Sibling test files mock the **interface** `IEmailMoveMonitor` only
(`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:351`,
`QfcQueueCoverageExpansionTests.cs:113, 140, 203`, `QfcQueuePurePathsTests.cs:119`).

---

## 4. Per-member coverage gap

Status derived by reading the eight existing tests against §2. Indicative confirmation, from the
**archived** Cobertura artifact of an unrelated feature
(`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`),
is quoted where a class entry exists; the authoritative per-file figure comes from F1's harness.

| Member | Status | Missed branches / lines | Indicative archived evidence |
| --- | --- | --- | --- |
| M1 `logger` | covered | — | part of `EmailMoveMonitor` class entry (`:16132`, line-rate 0.694) |
| M3 ctor | **partially covered** | `??` **false** branch (default-lambda path) never taken — all 8 tests pass a non-null delegate | same class entry |
| M3a default marshal lambda | **uncovered** | its single sequence point | `<>c` class entry, line-rate **0** (`:30177`) |
| M4 `_hookedItems` initializer | covered | — | — |
| M5 / M5a / M5b `HookItem` | covered | `Any` over a **non-empty non-matching** list is never exercised (E1 and E7 only cover empty-list and matching-folder states) | `<>c__DisplayClass4_0` line-rate 1 (`:30201`); `4_1` line-rate 1 (`:30239`) |
| M6 `UnhookItem` guard | covered | — | — |
| M6a lambda | **partially covered** | `?.` at `:75` **null** branch (parent not a `Folder`) never taken | `<>c__DisplayClass5_0` line-rate 1, branch-rate **0.833** (`:30251`) |
| M6b / M6c predicates | covered | — | `5_1` line-rate 1 (`:30315-30322`) |
| M7 `UnhookItemAsync` | **uncovered** | all paths | `<UnhookItemAsync>d__6` line-rate **0**, branch-rate **0** (`:30545`) |
| M7a lambda | **uncovered** | all paths | `<>c__DisplayClass6_0` line-rate **0**, branch-rate **0** (`:30333`) |
| M7b / M7c predicates | **uncovered** | all | `6_1` line-rate **0** (`:30389-30396`) |
| M8 `GetParentFolderAsync` | **uncovered** | all paths, both `logger.Error` calls, the recursion | `<GetParentFolderAsync>d__7` line-rate **0**, branch-rate **0** (`:30457`) |
| M8a / M8b lambdas | **uncovered** | both `catch` blocks | `<>c__DisplayClass7_0` line-rate **0** (`:30407`) |
| M9 / M9a `UnhookAll` | **partially covered** | zero-iteration `foreach` (empty `_hookedItems`) never exercised | covered by E6 for the 2-iteration case |
| M10 `BeforeItemMove` field | covered | — | — |
| M11 `SetupBeforeItemMove` (outer) | covered | — | invoked from every construction |
| M11a handler delegate body | **uncovered** | all — `if (item is MailItem)` both branches, `if (hookedItem != null)` both branches, the `MoveAction` invocation, the `Remove` | `<>c__DisplayClass10_0` line-rate **0** (`:30189`) |
| M11b handler predicate | **uncovered** | — | `<SetupBeforeItemMove>b__1` line-rate **0** (`:30191`) |

### `EmailMoveAction` assessed separately

`EmailMoveAction` is **not** entirely uncovered. It is constructed by `HookItem` (`:58`), which
every existing hook test exercises. The archived artifact reports the class at line-rate
**0.846**, branch-rate **1** (`coverage-baseline.cobertura.xml:16310`).

| Member | Status | Reason |
| --- | --- | --- |
| A1 ctor | covered | via `HookItem` (`:58`) |
| A2 `Mail` | **uncovered** | read only by M11b (`:213`), which is never invoked |
| A3 `Folder` | covered | read by M6a `:83`, M7a `:119`, M9a `:195` |
| A4 `MoveAction` | **uncovered** | invoked only by M11a (`:217`), which is never invoked |
| A5 `MailEntryId` | covered | M6b/M6c predicates |
| A6 `FolderEntryId` | covered | M5b, M6b predicates |

The 0.846 figure is consistent with exactly two uncovered getters out of thirteen sequence points.
Covering A2 and A4 requires invoking the `BeforeItemMove` handler — the same gap as M11a.

---

## 5. Testability classification per member

| Member | Classification | Interop type / API touched | Mockable with Moq? |
| --- | --- | --- | --- |
| M1 `logger` | pure-testable-now | none (`log4net.LogManager`) | n/a — `LogManager.GetLogger` works without configuration and never throws |
| M3 ctor (`??` false branch) | pure-testable-now | none at construction time | yes — `new EmailMoveMonitor()` assigns but does not invoke the default lambda |
| M3a default marshal lambda body | **host-bound-irreducible** | `UtilitiesCS.UiThread.Dispatcher` (`UtilitiesCS/Threading/UiThread.cs:135-140`) → `UiThread.Init()` → `new SyncContextForm(); ...Show()` (`UiThread.cs:51-54`) | **no** — executing it constructs and shows a live WinForms form and mutates set-once process-global static state. Both are prohibited by `CLAUDE.md` UT4 and epic.md Shared Design §2. **1 sequence point.** |
| M5 / M5a / M5b | pure-testable-now (via existing seam) | `MailItem.Parent`, `Folder.EntryID`, `Folder.BeforeItemMove` add-accessor | yes — precedent `EmailMoveMonitorTests.cs:72-85, 101-104` |
| M6 / M6a / M6b / M6c | pure-testable-now (via existing seam) | `MailItem.EntryID`, `MailItem.Parent`, `Folder.BeforeItemMove` remove-accessor | yes — precedent `EmailMoveMonitorTests.cs:120-131` |
| M7 `UnhookItemAsync` + M7a/b/c | pure-testable-now | same set as M6 | yes. `public` on an `internal` class with `InternalsVisibleTo("QuickFiler.Test")`, so the test calls it directly. Completes synchronously under a synchronous pass-through marshal delegate — no timers, no waits. |
| M8 `GetParentFolderAsync` + M8a/M8b | pure-testable-now | `MailItem.Parent`, `MailItem.EntryID` | yes. Error paths are driven with `mail.SetupGet(x => x.Parent).Throws(new COMException(...))`. The retry is a plain recursion with no delay, so it is deterministic. |
| M9 / M9a | pure-testable-now (via existing seam) | `Folder.BeforeItemMove` remove-accessor | yes |
| M10 / M11 | pure-testable-now | none | yes |
| M11a / M11b handler body | **needs-seam — but the seam already exists in the test layer, not the production layer** | `MAPIFolderEvents_12_BeforeItemMoveEventHandler` (delegate with a `ref bool` parameter), `MailItem.EntryID` | yes. Two routes, both **production-change-free**: (1) capture the handler at subscribe time via `folder.SetupAdd(f => f.BeforeItemMove += It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>()).Callback((MAPIFolderEvents_12_BeforeItemMoveEventHandler h) => captured = h)` — Moq interception of Interop event accessors is proven at `TaskMaster.Test/AppGlobals/AppEventsCoverageExpansionTests.cs:139-143` and by the existing `VerifyAdd` at `EmailMoveMonitorTests.cs:101-104`; (2) fallback — read the private `BeforeItemMove` field by reflection, a technique already used throughout `QuickFiler.Test` (`Viewers/BreadcrumbSelectorCoordinatorTests.cs:152-161`, `Viewers/BreadcrumbDropDownHostTests.cs:364-377`) and in this very fixture (`EmailMoveMonitorTests.cs:33-37`). |
| A1–A6 `EmailMoveAction` | pure-testable-now | `MailItem.EntryID`, `Folder.EntryID` in the constructor | yes — the type is `internal` and directly constructible from the test assembly |

Summary verdict for the file: **pure-testable-now for every member except the single-sequence-point
default marshal lambda (M3a), which is host-bound-irreducible.**

---

## 6. Event-subscription and lifetime invariants

### 6.1 Enumerated subscriptions

There is exactly **one** Outlook Interop event in this file: `Folder.BeforeItemMove` (the
`MAPIFolderEvents_12_Event` member), typed `MAPIFolderEvents_12_BeforeItemMoveEventHandler`. There
is no `Items.ItemAdd`, `Items.ItemRemove`, or `MailItem.PropertyChange` subscription anywhere in
this file.

| Subscription | Site | Guard | Unsubscription path(s) |
| --- | --- | --- | --- |
| `folder.BeforeItemMove += BeforeItemMove` | `:57` inside the `HookItem` marshaled lambda | `if (!_hookedItems.Any(x => x.FolderEntryId == folderEntryId))` at `:56` — at most one subscription per distinct folder EntryID | (a) `UnhookItem` `:83`, only when `count == 1`; (b) `UnhookItemAsync` `:119`, same condition; (c) `UnhookAll` `:195`, unconditional per retained entry |

The handler delegate instance is created once per monitor in `SetupBeforeItemMove` (`:206-222`) and
stored in the field `BeforeItemMove` (`:202`), so `+=` and `-=` always use the same delegate
identity. That is what makes `-=` actually detach.

### 6.2 Failure modes

| # | Failure mode | Mechanism | Currently reachable? | Deterministic test |
| --- | --- | --- | --- | --- |
| L1 | **Leaked subscription when the parent folder changes between hook and unhook.** `HookItem` keys on `folder.EntryID` read at hook time; `UnhookItem` re-reads `(mail.Parent as Folder)?.EntryID` **live** at `:75`. If the mail has since moved, `parentFolderEntryId` differs from the cached `FolderEntryId`, so `count == 0`, the `count == 1` branch is skipped, the entry is removed at `:84`, and the original folder stays subscribed forever. | `:75` vs `:78` vs `:82` | Yes — and it is the concrete consequence of the `?.` null/mismatch branch that is currently uncovered | T14 (§11): mutate the mock's `Parent` after hooking and assert entry removal **without** unsubscribe. Pins present behavior and makes the leak visible. |
| L2 | **Session-scoped hook retention for items dropped outside `UnhookItem`.** `_hookedItems` only shrinks via `UnhookItem`, `UnhookItemAsync`, `UnhookAll`, or the handler's own `Remove` at `:218`. | `:44`, `:58`, `:185-200` | Yes — this is open **issue #426** (`docs/features/potential/promoted/2026-08-07-emailmovemonitor-rejected-item-hook-retention.md:35, 57`) | Out of F4 scope (it is a defect in F2/F5-owned dequeue paths). Do not fix here; record only. |
| L3 | **Double subscription for the same folder.** Prevented by the `Any` guard at `:56`. If two mails share a folder, only the first subscribes. | `:56` | No | Already pinned by E1 (`:88`). T16 adds the complementary non-empty/non-matching-list state. |
| L4 | **Double unsubscribe.** Prevented by `count == 1` (`:82`, `:118`) and by `Clear()` (`:197`). | — | No | Already pinned by E2 and E6. |
| L5 | **Handler fires for an item no longer tracked.** `if (hookedItem != null)` at `:215` guards it. | `:215` | Yes, but harmless | T3 (§11). |
| L6 | **`EmailMoveAction` retains a live `MailItem` COM reference.** `_mail` (`:243`) is never released; only `_hookedItems.Clear()` drops it. | `:243`, `:197` | Yes | Covered indirectly by T13 (`UnhookAll` on empty) and E6. Not a defect this child fixes. |
| L7 | **Handler reads live COM (`x.Mail.EntryID`, `:213`) instead of the cached `MailEntryId`.** Inconsistent with the caching contract documented at `:228-233` and with the `UnhookItem` predicate at `:79`. If the moved item's COM object has been released, this read can throw inside an Outlook event callback. | `:213` | Yes | T1/T3 (§11) exercise it with a mocked `EntryID`. **Report-only inconsistency**; changing `:213` to `x.MailEntryId` would be a behavior change and is out of F4's no-behavior-change scope. Promote as a separate issue if desired. |

### 6.3 How each is tested without a live Outlook process

Every subscription assertion uses Moq's interception of the Interop event accessors on
`Mock<Folder>` — `VerifyAdd` / `VerifyRemove` (already in use at
`EmailMoveMonitorTests.cs:101-104, 120-131, 163-173, 216-231, 260-263`) and `SetupAdd(...).Callback`
to capture the handler for invocation (§5, M11a). The `_marshalToSta` constructor seam (`:29, 38,
40`) replaces the STA dispatch with a synchronous pass-through so no dispatcher, message loop, or
apartment is required. **No STA test and no live form is needed for this file.**

### 6.4 Banned-API audit of the production file — RESULT: CLEAN

A full read of all 262 lines finds **no** occurrence of `Task.Delay`, `Thread.Sleep`,
`DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `Random.Shared`, `Task.Run`, or any
wall-clock read. The file therefore has **no banned-API finding** and needs **no clock seam**. The
comment at `:133-135` records that a prior `Task.Run` hop was the cross-thread defect pattern and
has already been removed. `System.TimeProvider` (see `00-cluster-overview.md` §4) is not required
here.

Incidental (non-blocking) observations, all confined to this file: the `using` directives
`System.Collections.Concurrent` (`:2`), `System.ComponentModel` (`:4`),
`System.Reactive.Disposables` (`:6`), `System.Text` (`:7`), and `log4net.Repository.Hierarchy`
(`:10`) appear unused. Removing them is an optional analyzer-hygiene task with zero cross-child
impact; it is not required by any acceptance criterion.

---

## 7. Interface-file coverage semantics

Not applicable to this file — `EmailMoveMonitor.cs` declares two concrete classes with executable
bodies. See `14-IEmailMoveMonitor.md` for the interface-file disposition.

---

## 8. Seam proposal

### 8.1 Finding: the required seam already exists. No new production seam is needed.

`EmailMoveMonitor` already carries a tier-2 **injectable-delegate seam** per
`.claude/rules/csharp.md:49-53`:

- Type: `Action<System.Action>`, field `_marshalToSta` (`:29`).
- Injection point: optional constructor parameter `Action<System.Action> marshalToSta = null`
  (`:38`).
- Production default: `action => UiThread.Dispatcher.Invoke(action)` (`:40`), applied via `??`.
- Documented contract: `:24-28` and `:31-37`; the XML doc explicitly names the pattern —
  *"Mirrors the default-to-real-implementation seam style used for `TimeProvider` in
  `QfcDatamodel`"*.

Ranked against the epic's hierarchy (interface seam > injectable delegate > adapter): an
**interface seam** (e.g. `IStaMarshaller`) would be the nominally-preferred tier, but it would be a
pure churn substitution — the delegate already achieves full isolation, is already consumed by eight
passing tests, and is the shape the repository standardized on for this exact problem in issues
#214 and #420. Introducing an interface here would provide **zero additional coverage** and would
create diff surface in a file that open issue #426 will later touch. **Rejected.**

### 8.2 What remains uncovered is covered by test-layer technique, not by a new seam

| Gap | Technique | Production change required |
| --- | --- | --- |
| M11a / M11b handler body, and therefore `EmailMoveAction.Mail` / `.MoveAction` | Capture the handler via `Mock<Folder>.SetupAdd(...).Callback(...)`; fall back to reflection on the private `BeforeItemMove` field | **None** |
| M7 / M7a-c `UnhookItemAsync` | Call it directly (`public` + `InternalsVisibleTo`) | **None** |
| M8 / M8a / M8b `GetParentFolderAsync` | Drive `mail.Parent` and `mail.EntryID` to throw with Moq `.Throws<COMException>()`; reached transitively through `UnhookItemAsync` | **None** |
| M3 `??` false branch | `new EmailMoveMonitor()` with no argument (assigns, does not invoke, the default lambda) | **None** |
| M6a `?.` null branch | Set the mock's `Parent` to a non-`Folder` object | **None** |
| M9a zero-iteration `foreach` | `UnhookAll()` on a fresh monitor | **None** |
| M3a default lambda **body** | Not reachable without a live form | Irreducible — accept (§12) |

### 8.3 Conditional seam, considered and rejected

If a future reviewer insists on covering M3a's single line, the only shape that does not construct a
live form is to promote the marshal default to an injectable static hook, for example
`internal static Func<Action<System.Action>> DefaultMarshalFactory { get; set; }`. That would (a)
introduce mutable process-global static state — exactly what this fixture's `[TestCleanup]` guard at
`EmailMoveMonitorTests.cs:53-60` exists to prevent, (b) add a public-surface member for test-only
purposes, and (c) still not execute `UiThread.Dispatcher.Invoke`. It buys one sequence point at the
cost of a determinism regression. **Rejected.** Record M3a as the irreducible remainder instead.

### 8.4 Rejected alternative: delete the dormant async members

Deleting `UnhookItemAsync` (`:90-124`) and `GetParentFolderAsync` (`:126-183`) would remove ~55
uncovered sequence points from the denominator and is behaviorally safe (both are unreachable from
production; §9). **Rejected** for three reasons: (a) `UnhookItemAsync` is `public` on the type and
its removal is an API change the epic's "no behavior change" posture disfavours; (b) issue #426's
candidate fixes (`docs/features/potential/promoted/2026-08-07-emailmovemonitor-rejected-item-hook-retention.md:71-73`)
plausibly want an unhook path on the dequeue thread, i.e. exactly this member; (c) the epic's
Shared Design §1 direction is *refactor first, exempt only the irreducible remainder* — covering is
the stated preference over deleting. Covering them costs eight tests and is fully deterministic.

---

## 9. CRITICAL — cross-child conflict analysis

### 9.1 Every file outside F4 scope that calls into this file

A repository-wide `.cs` grep for `EmailMoveMonitor|IEmailMoveMonitor|EmailMoveAction` yields exactly
these production call sites. All three construct `new EmailMoveMonitor()` with **no arguments**.

| # | Call site | Line | Member used | Owning sibling |
| --- | --- | --- | --- | --- |
| C1 | `QuickFiler/Controllers/QfcQueue.cs` | `:40` | `private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();` | **F2** `quickfiler-queue-admission-coverage` (epic.md `:262`) |
| C2 | `QuickFiler/Controllers/QfcQueue.cs` | `:76` | `_moveMonitor.UnhookItem(group.MailItem)` | **F2** |
| C3 | `QuickFiler/Controllers/QfcQueue.cs` | `:130` | `_moveMonitor.UnhookItem(group.MailItem)` | **F2** |
| C4 | `QuickFiler/Controllers/QfcQueue.cs` | `:230` | `_moveMonitor.HookItem(item, async (x) => await RemoveItem(x))` | **F2** |
| C5 | `QuickFiler/Controllers/QfcDatamodel.cs` | `:80` | `_moveMonitor.UnhookAll()` | **F5** `quickfiler-datamodel-coverage` (epic.md `:287`) |
| C6 | `QuickFiler/Controllers/QfcDatamodel.cs` | `:81` | `_moveMonitor = null` | **F5** |
| C7 | `QuickFiler/Controllers/QfcDatamodel.cs` | `:103` | `private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();` | **F5** |
| C8 | `QuickFiler/Controllers/QfcDatamodel.cs` | `:357` | `_moveMonitor.HookItem` (method-group argument) | **F5** |
| C9 | `QuickFiler/Controllers/QfcDatamodel.cs` | `:400` | `_moveMonitor.HookItem(mailItem, (x) => _masterQueue.Remove(x))` | **F5** |
| C10 | `QuickFiler/Controllers/QfcDatamodel.cs` | `:452` | `_moveMonitor.HookItem(item, (x) => _masterQueue.Remove(x))` | **F5** |
| C11 | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | `:44` | `_moveMonitor.UnhookItem(node)` | **F5** |
| C12 | `QuickFiler/Controllers/QfcCollectionController.cs` | `:78` | `private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();` | **F11** `quickfiler-collection-controller-coverage` (epic.md `:332`) |
| C13 | `QuickFiler/Controllers/QfcCollectionController.cs` | `:256, :284, :364, :451, :1942` | `_moveMonitor.HookItem(...)` | **F11** |
| C14 | `QuickFiler/Controllers/QfcCollectionController.cs` | `:1007` | `_moveMonitor.UnhookAll()` | **F11** |
| C15 | `QuickFiler/Controllers/QfcCollectionController.cs` | `:1124, :1187` | `_moveMonitor.UnhookItem(...)` | **F11** |

Sibling-owned **test** files that mock `IEmailMoveMonitor` (these break if the interface's member
set changes):

| # | Test call site | Line | Owning sibling |
| --- | --- | --- | --- |
| C16 | `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | `:333, :351` (`Mock<IEmailMoveMonitor>` loose) | **F11** |
| C17 | `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs` | `:113, :140, :203` (`MockBehavior.Strict`) | **F2** |
| C18 | `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | `:119` (`MockBehavior.Strict`) | **F2** |

Documentation-only references (no code impact): `docs/features/...` artifacts, and the stale
research/evidence Cobertura files cited in §4.

### 9.2 Conflict statement per proposed change

| Proposed change | Conflict statement |
| --- | --- |
| Add new test files under `QuickFiler.Test/Helper Classes/` | **Requires no sibling-owned file change.** F4 exclusively owns `QuickFiler.Test/Helper Classes/**` (epic.md `:276-283`, issue.md `:73-76`). |
| Add `<Compile Include>` entries to `QuickFiler.Test/QuickFiler.Test.csproj` | **Shared-file edit** — unavoidable for any new test file (explicit-list project, `:57-169`). Mitigation: insert only inside the contiguous `Helper Classes\` block at `:158-165`, alphabetically. Siblings edit the `Controllers\` (`:58-151`) and `Viewers\` (`:60-91`) regions. See `00-cluster-overview.md` §1.3. |
| Keep the existing `marshalToSta` optional-parameter seam unchanged | **Requires no sibling-owned file change.** C1, C7 and C12 call `new EmailMoveMonitor()` with no arguments and continue to compile byte-identically. |
| **Do NOT** add, remove, or rename any member of `IEmailMoveMonitor` | Any change breaks C16, C17, C18 — `MockBehavior.Strict` mocks in F2- and F11-owned test files — and potentially C1/C7/C12. Explicitly out of scope. |
| **Do NOT** promote `UnhookItemAsync` onto `IEmailMoveMonitor` | Same reason. It buys no coverage; the test calls the concrete class directly. |
| **Do NOT** change the `_marshalToSta` parameter's type, order, or default | Would force edits at C1, C7, C12 (F2/F5/F11-owned). If a future seam ever needs another dependency, add it as a **further optional parameter after** `marshalToSta`, so the three existing no-argument call sites remain valid. |
| **Do NOT** fix the issue #426 hook-retention defect here | Its candidate fixes edit `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` (F5) and the `QfcStreamingDequeueConfidenceGate` rejection path (F2). |
| **Do NOT** restructure `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | Issue #426 lists *"`EmailMoveMonitor` hook lifecycle"* among its unit-coverage areas (`:65`). Additive-only changes to that file (or, as recommended, none at all) keep #426's future rebase clean. |

**Net result: the recommended plan for this file requires ZERO edits to any sibling-owned
production or test file, and ZERO edits to `QuickFiler/QuickFiler.csproj`.** The only shared-file
touch is the `QuickFiler.Test.csproj` `<Compile Include>` insertion, confined to the
`Helper Classes\` block.

---

## 10. 500-line compliance

| File | Lines | Limit | Headroom |
| --- | --- | --- | --- |
| `QuickFiler/Helper Classes/EmailMoveMonitor.cs` | 262 | 500 | 238 — compliant, no split needed |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` (existing) | 314 | 500 | 186 |

The 500-line limit applies to **test code as well as production code**
(`.claude/rules/general-code-change.md`, File Size Limit). Appending all 19 recommended tests to the
existing 314-line fixture would land near 560 lines and breach the limit. The recommendation in §11
therefore places them in three new files.

**No new production file is proposed for this cluster**, so no `<Compile Include>` line is needed in
`QuickFiler/QuickFiler.csproj` (whose `Helper Classes\` block is `:342-354` and whose
`Interfaces\IEmailMoveMonitor.cs` entry is `:355`). Had one been needed, it would be a shared-file
conflict risk of the same class as the test csproj.

---

## 11. Recommended test cases — enumerated individually

Framework: MSTest 4.3.3 + Moq 4.20.72 + FluentAssertions 8.10.0. Every test uses the existing
`CountingPassThrough()` synchronous marshal delegate (or a local equivalent), Arrange–Act–Assert,
no temporary files, no live forms, no sleeps, no wall-clock reads.

**Destination files (all new, all under `QuickFiler.Test/Helper Classes/`):**

- **File D1** — `EmailMoveMonitorEventHandlerTests.cs` (`[TestClass] EmailMoveMonitorEventHandlerTests`,
  `[DoNotParallelize]`, namespace `QuickFiler.Helper_Classes.Tests`): T1–T4, T13–T16. ~250 lines.
- **File D2** — `EmailMoveMonitorAsyncUnhookTests.cs` (`[TestClass] EmailMoveMonitorAsyncUnhookTests`,
  `[DoNotParallelize]`, same namespace): T5–T12. ~240 lines.
- **File D3** — `EmailMoveActionTests.cs` (`[TestClass] EmailMoveActionTests`, same namespace):
  T17–T19. ~100 lines.

Each file reproduces the static-`UiThread.Dispatcher` reflective guard from
`EmailMoveMonitorTests.cs:32-37, 44-60` so order-independence is preserved.

### Duplicate check — excluded from the recommendation

The following candidate scenarios are **already covered** by `EmailMoveMonitorTests.cs` and are
deliberately **excluded**: first-hook subscribe-once (E1 `:88`); shared-folder no-resubscribe
(E1 `:88`); unsubscribe-only-on-last-item (E2 `:108`); null-argument no-op for `UnhookItem`
(E3 `:135`); cached-EntryID selective removal (E4 `:148`); marshal-count contract for
`HookItem`/`UnhookItem`/`UnhookAll` (E5 `:177`); `UnhookAll` unsubscribing every folder and clearing
state (E6 `:201`); duplicate hook of the same item and unhook of a never-hooked item (E7 `:235`);
cross-thread marshal-target execution (E8 `:267`).

### Enumerated tests

| ID | `[TestMethod]` name | Arrange / Act / Assert (one line) | Category | Destination |
| --- | --- | --- | --- | --- |
| T1 | `BeforeItemMoveHandler_WhenMovedMailIsHooked_InvokesMoveActionAndRemovesEntry` | Arrange: hook `mail-1` on `folder-1` with a recording move action, capturing the handler from `folder.SetupAdd(...).Callback`; Act: invoke the captured handler with `(mail.Object, destFolder.Object, ref cancel)`; Assert: the move action ran exactly once with `mail.Object` and a subsequent `UnhookItem(mail)` performs no `VerifyRemove` (entry already gone). | positive | D1 |
| T2 | `BeforeItemMoveHandler_WhenMovedItemIsNotAMailItem_LeavesBookkeepingUntouched` | Arrange: hook `mail-1`, capture the handler; Act: invoke it with `new object()` as `item`; Assert: no move action ran, and a later `UnhookAll()` still issues exactly one `VerifyRemove` for `folder-1`. | invalid-input | D1 |
| T3 | `BeforeItemMoveHandler_WhenMovedMailIsNotHooked_DoesNotInvokeAnyMoveAction` | Arrange: hook `mail-1`, capture the handler; Act: invoke it with an unhooked `mail-2` mock; Assert: no move action ran and `UnhookItem(mail1)` still issues exactly one `VerifyRemove`. | boundary | D1 |
| T4 | `BeforeItemMoveHandler_NeverSetsCancelFlag` | Arrange: hook `mail-1`, capture the handler, `bool cancel = false`; Act: invoke the handler for the hooked mail; Assert: `cancel.Should().BeFalse()` — the monitor observes moves and never vetoes them. | boundary | D1 |
| T5 | `UnhookItemAsync_WithNullMail_CompletesWithoutMarshaling` | Arrange: fresh monitor with a counting pass-through; Act: `await monitor.UnhookItemAsync(null, CancellationToken.None)`; Assert: no throw and the marshal counter is 0. | invalid-input | D2 |
| T6 | `UnhookItemAsync_WithAlreadyCancelledToken_ThrowsOperationCanceledException` | Arrange: fresh monitor, `new CancellationToken(canceled: true)`; Act/Assert: `await act.Should().ThrowAsync<OperationCanceledException>()` and the marshal counter is 0. | error-handling | D2 |
| T7 | `UnhookItemAsync_WhenParentIsNotAFolder_ReturnsWithoutUnsubscribing` | Arrange: hook `mail-1` on `folder-1`, then re-point `mail.Parent` to `new object()`; Act: `await monitor.UnhookItemAsync(mail, none)`; Assert: `folder.VerifyRemove(..., Times.Never)` and no throw. | boundary | D2 |
| T8 | `UnhookItemAsync_WhenMailIsLastHookedItemForFolder_UnsubscribesAndRemovesEntry` | Arrange: hook `mail-1` on `folder-1`; Act: `await monitor.UnhookItemAsync(mail1, none)`; Assert: `VerifyRemove(..., Times.Once)`, and a subsequent `UnhookAll()` issues no further remove. | positive | D2 |
| T9 | `UnhookItemAsync_WhenAnotherItemRemainsInFolder_RemovesEntryWithoutUnsubscribing` | Arrange: hook `mail-1` and `mail-2` on the same `folder-1`; Act: `await monitor.UnhookItemAsync(mail1, none)`; Assert: `VerifyRemove(..., Times.Never)`, then `UnhookItem(mail2)` issues exactly one remove. | boundary | D2 |
| T10 | `UnhookItemAsync_ForNeverHookedMail_IsNoOpAndDoesNotThrow` | Arrange: hook `mail-1`, create an unhooked `mail-9` on the same folder; Act: `await monitor.UnhookItemAsync(mail9, none)`; Assert: no throw and `VerifyRemove(..., Times.Never)`. | invalid-input | D2 |
| T11 | `UnhookItemAsync_WhenParentReadThrowsComException_RetriesThreeTimesThenReturnsNullWithoutThrowing` | Arrange: `mail.SetupGet(x => x.Parent).Throws(new COMException("rpc failed"))`, `EntryID` returns `"mail-1"`; Act: `await monitor.UnhookItemAsync(mail, none)`; Assert: no throw and `mail.VerifyGet(x => x.Parent, Times.Exactly(3))` (attempts at `remaining` = 2, 1, 0). | error-handling | D2 |
| T12 | `UnhookItemAsync_WhenBothParentAndEntryIdReadsThrow_StillCompletesWithoutThrowing` | Arrange: both `Parent` and `EntryID` throw `COMException`; Act: `await monitor.UnhookItemAsync(mail, none)`; Assert: no throw, `Parent` read exactly 3 times, no unsubscribe (exercises the `"[Error getting EntryID]"` fallback at `:164`). | error-handling | D2 |
| T13 | `UnhookAll_WithNoHookedItems_MarshalsOnceAndTouchesNoFolder` | Arrange: fresh monitor, a `Mock<Folder>` that is never hooked; Act: `monitor.UnhookAll()`; Assert: marshal counter is 1, `folder.VerifyNoOtherCalls()`, no throw (zero-iteration `foreach`). | boundary | D1 |
| T14 | `UnhookItem_WhenParentChangedAfterHook_RemovesEntryButLeavesOriginalFolderSubscribed` | Arrange: hook `mail-1` on `folder-1`, then re-point `mail.Parent` to a different `folder-2` mock; Act: `monitor.UnhookItem(mail1)`; Assert: `folder1.VerifyRemove(..., Times.Never)` and a later `UnhookAll()` also performs no remove (entry gone) — pins failure mode L1 (§6.2). | boundary | D1 |
| T15 | `Constructor_WithoutMarshalDelegate_AssignsProductionDefaultWithoutTouchingUiThread` | Arrange: reflective snapshot of `UiThread.Dispatcher`; Act: `var monitor = new EmailMoveMonitor();`; Assert: construction does not throw and the `UiThread.Dispatcher` snapshot is unchanged (exercises the `??` right-hand branch at `:40` without invoking the lambda body). | boundary | D1 |
| T16 | `HookItem_WithTwoDistinctFolders_SubscribesExactlyOncePerFolder` | Arrange: `mail-A` on `folder-A`, `mail-B` on `folder-B`; Act: hook both; Assert: `folderA.VerifyAdd(..., Times.Once)` and `folderB.VerifyAdd(..., Times.Once)` — exercises the `Any` predicate over a **non-empty non-matching** list (`:56`), a state E1 and E7 do not reach. | positive | D1 |
| T17 | `EmailMoveAction_ExposesConstructorArgumentsAndCachedEntryIds` | Arrange: `Mock<MailItem>("mail-1")`, `Mock<Folder>("folder-1")`, a recording action; Act: `new EmailMoveAction(mail.Object, folder.Object, action)`; Assert: `Mail`, `Folder`, `MoveAction`, `MailEntryId` (`"mail-1"`), `FolderEntryId` (`"folder-1"`) all match — covers A1–A6 independently of the handler path. | positive | D3 |
| T18 | `EmailMoveAction_ReadsEntryIdsExactlyOnceAtConstructionAndNotOnSubsequentPropertyReads` | Arrange/Act: construct, then read `MailEntryId` and `FolderEntryId` three times each; Assert: `mail.VerifyGet(x => x.EntryID, Times.Once)` and `folder.VerifyGet(x => x.EntryID, Times.Once)` — pins the caching contract documented at `:228-233`. | boundary | D3 |
| T19 | `EmailMoveAction_WhenMailEntryIdReadThrows_PropagatesComExceptionFromConstructor` | Arrange: `mail.SetupGet(x => x.EntryID).Throws(new COMException("released"))`; Act/Assert: `act.Should().Throw<COMException>()` — pins fail-fast at the hook boundary rather than a silently half-built entry. | error-handling | D3 |

**Totals: 19 new tests.** Category spread — positive 4 (T1, T8, T16, T17), invalid-input 3 (T2, T5,
T10), boundary 8 (T3, T4, T7, T9, T13, T14, T15, T18), error-handling 4 (T6, T11, T12, T19). All
four categories are represented, satisfying issue #434 acceptance criterion 5.

Per issue #136, each of T1–T19 becomes its own atomic plan task, and each of D1–D3 plus the single
`QuickFiler.Test.csproj` insertion hunk becomes its own task.

---

## 12. Projected coverage

After T1–T19 every member enumerated in §2 is executed at least once, and every decision point
listed there is exercised in both directions, with a single exception: **M3a**, the default marshal
lambda body at `:40` (`action => UiThread.Dispatcher.Invoke(action)`), which is one sequence point.

Reasoning for clearing 80%:

1. The three currently-zero-coverage regions dominate the file's uncovered mass — the
   `<UnhookItemAsync>d__6` state machine, the `<GetParentFolderAsync>d__7` state machine and their
   four closures, and the `<>c__DisplayClass10_0` handler body with its predicate. §4 shows all of
   them at line-rate 0 in the archived artifact. T1–T4 and T5–T12 drive every one of them.
2. The two remaining `EmailMoveAction` gaps (`Mail`, `MoveAction`) are closed twice over — by T1
   through the handler and by T17 directly — taking that type from an indicative 0.846 to 1.000.
3. The three partial branches in already-covered members (M3's `??` false branch, M6a's `?.` null
   branch, M9a's zero-iteration `foreach`) are closed by T15, T7/T14, and T13 respectively.
4. What remains is exactly one sequence point out of a whole-file denominator on the order of 150
   sequence points (the `EmailMoveMonitor` class entry alone reports 49 in the archived artifact,
   plus `EmailMoveAction`'s 13, plus the two async state machines and eleven closure classes).

**Projected per-file line coverage: >= 95%, with a realistic expectation near 99%.** The 80% floor
is cleared with a very wide margin, and the >= 90% "new/modified code" target in `CLAUDE.md` UT2 is
also met.

**Irreducible fraction: 1 sequence point (≈ 0.7% of the file).** This is below any threshold and
therefore **does not require an exemption request** against F1's ledger and **must not** receive an
`[ExcludeFromCodeCoverage]` attribute — per epic.md Shared Design §1, such an attribute on a file
that reaches 95%+ would itself be a Blocking finding. The recommended ledger entry for
`QuickFiler/Helper Classes/EmailMoveMonitor.cs` is **`testable`**, with a footnote naming the single
irreducible line (`:40`, the `UiThread.Dispatcher` production default) so the capstone F16 can
account for it without treating it as a gap.

The numeric confirmation of all figures above is produced at execution time by F1's per-file
coverage harness and committed under `<FEATURE>/evidence/qa-gates/`.
