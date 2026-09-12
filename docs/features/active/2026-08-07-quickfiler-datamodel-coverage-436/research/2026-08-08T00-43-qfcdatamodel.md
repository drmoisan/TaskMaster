# Research: `QuickFiler/Controllers/QfcDatamodel.cs`

- Feature: `quickfiler-datamodel-coverage` (issue #436), child F5 of epic `quickfiler-per-file-coverage` (#136)
- Target file: `QuickFiler/Controllers/QfcDatamodel.cs` — 496 lines, `[ExcludeFromCodeCoverage]` at line 25
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a923053598cf4ccea`
- Created: 2026-08-08T00-43
- Scope: this one production file. Sibling partials `QfcDatamodel.QueueProcessing.cs` and
  `QfcDatamodel.FrameBuilding.cs`, and `EfcDataModel.cs`, are researched separately; they appear here
  only where a cross-file consequence is unavoidable.

---

## 0. Executive summary

1. `[ExcludeFromCodeCoverage]` at `QfcDatamodel.cs:25` can be **removed entirely**. After the seams
   proposed in §5 there is **no irreducible remainder in this file**. Nothing needs to go to F1's
   ledger on account of `QfcDatamodel.cs` itself.
2. **Hard sequencing constraint.** The attribute sits on one declaration of a three-file partial type,
   so it currently excludes `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs` **and**
   `QfcDatamodel.FrameBuilding.cs` from measurement simultaneously. This is verified, not inferred
   (§3.1). Removing it exposes all three files at once, so the removal task must be sequenced after
   the FrameBuilding phase's seams land, or FrameBuilding must carry member-level attributes instead.
3. **Three members of this file are dead code** (`Worker_RunWorkerCompleted`,
   `LoadRemainingEmailsToQueue(BackgroundWorker, CancellationToken)`,
   `LoadRemainingEmailsToQueueAsync(BackgroundWorker, CancellationToken)`), plus one unused static
   field (`log`) and one empty `#region`. All are verified unreferenced (§3.4). Deleting them removes
   ~123 lines — which both resolves the 4-line file-size headroom problem and removes the largest
   block of permanently-uncoverable lines (two `MessageBox.Show` call sites and a `#pragma warning
   disable CS0618` suppression).
4. Existing coverage of this file is thin and partly mislabelled: five tests in `QfcDatamodelTests.cs`
   named `TryQueueRemainingMailItemAsync_*` do not touch `QfcDatamodel` at all — they construct
   `QfcRemainingQueueAdmission` directly (a *different* file, assigned to sibling **F2**). The
   datamodel method of that name is genuinely uncovered (§4.2).
5. All proposed seams are additive. `IQfcDatamodel` is unchanged; the public constructor
   `QfcDatamodel(IApplicationGlobals, CancellationToken)` and the public static
   `QfcDatamodel.LoadAsync(IApplicationGlobals, CancellationToken, CancellationTokenSource, ProgressTracker)`
   keep their exact signatures, which is required because `QfcHomeController.cs:163` and
   `QfcHomeController.cs:173` (sibling **F7**) bind to them.

---

## 1. Method and evidence basis

Everything below is grounded in files read in this session. Where a claim could not be verified
without building or running, it is marked **INFERRED** with the reason.

Files read in full:

| Path | Purpose |
| --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.cs` | subject |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | sibling partial, consumes members of the subject |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | sibling partial, consumes `TimeProvider`, defines `InitDf`/`InitDfAsync` |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | cross-child contract |
| `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` | collaborator constructed inside the subject |
| `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | source of the existing `IFolderScoringService` seam and of `FolderScoringService` |
| `QuickFiler/Helper Classes/EmailMoveMonitor.cs`, `QuickFiler/Interfaces/IEmailMoveMonitor.cs` | move-monitor seam |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` (317 lines) | existing tests |
| `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` (255 lines) | existing tests |
| `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` (212 lines) | existing tests |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` (136 lines) | existing tests |
| `CLAUDE.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `docs/features/epics/quickfiler-per-file-coverage/epic.md`, `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/issue.md` | policy and contract |

Coverage-reality method: F1's per-file harness does not exist yet, so current coverage is derived by
(a) reading every test that names `QfcDatamodel` and mapping each assertion to the member it drives,
and (b) cross-checking against a committed Cobertura report from a prior feature (see §3.1).
**Confidence: high for "which members are exercised at all", medium for per-branch claims** — the
report contains no `QfcDatamodel` entries at all, so branch-level statements are read-derived only.

---

## 2. Member inventory — `QfcDatamodel.cs`

`public partial class QfcDatamodel : IQfcDatamodel`, namespace `QuickFiler.Controllers`.

| # | Member | Lines | Vis. | Behavior (one line) |
| --- | --- | --- | --- | --- |
| M1 | `logger` static readonly field | 28–30 | private | log4net logger; used at 209, 338, 372, 410, 490 and by both sibling partials |
| M2 | `QfcDatamodel(IApplicationGlobals)` | 34–41 | private | Stores globals, caches `Ol.App` and `ActiveExplorer()`, subscribes `NewMailEx`, defaults `RemainingEmailLoader`. Does **not** build the frame. Only caller is `LoadAsync` (line 64). |
| M3 | `QfcDatamodel(IApplicationGlobals, CancellationToken)` | 43–52 | public | Same as M2 plus stores the token and builds `_frame` synchronously via `InitDf(_activeExplorer)` (defined in `QfcDatamodel.FrameBuilding.cs:13`). Bound by `QfcHomeController.cs:163`. |
| M4 | `LoadAsync(globals, token, tokenSource, progress)` | 54–73 | public static | Async factory: reports progress 0, constructs via M2, assigns `Token`/`TokenSource`, awaits `InitDfAsync(appGlobals.Ol.App.ActiveExplorer(), progress.Increment(2))`, returns the model. Bound by `QfcHomeController.cs:173`. |
| M5 | `Cleanup()` | 75–91 | public | `IQfcDatamodel` member. Cancels token source, cancels worker, unsubscribes `NewMailEx`, `_moveMonitor.UnhookAll()`, then nulls `_moveMonitor`, `_activeExplorer`, `_olApp`, `_globals`, `_frame`, `_masterQueue`, `_worker`. |
| M6 | `log` static readonly field | 97–99 | private | **Unused.** Duplicate of M1. |
| M7 | Private fields `_globals`, `_activeExplorer`, `_masterQueue`, `_moveMonitor`, `_olApp`, `_frame`, `_worker` | 100–106 | private | State. `_masterQueue` and `_moveMonitor` have field initializers (`[]`, `new EmailMoveMonitor()`) that are bypassed by `FormatterServices.GetUninitializedObject`. |
| M8 | `TimeProvider` property | 108–112 | internal | Time/delay seam (issue #222), defaults to `TimeProvider.System`. Consumed by `QueueProcessing.cs:173` and `FrameBuilding.cs:43`. |
| M9 | `RemainingEmailLoader` property | 114–128 | internal | `Func<CancellationToken, Task<bool>>` worker-body seam (issue #244/#424), defaulted in M2/M3 to M18. |
| M10 | `_complete` / `Complete` | 134–139 | public | `IQfcDatamodel` member. Plain backing-field property. |
| M11 | `MovedItems` | 141–144 | public | `IQfcDatamodel` member. `=> _globals.AF.MovedMails` (`SloStack<IMovedMailInfo>`). |
| M12 | `_token` / `Token` | 146–151 | public | Cancellation token property; read by `QueueProcessing.cs:85,104,128,136`. Not on `IQfcDatamodel`. |
| M13 | `_tokenSource` / `TokenSource` | 153–158 | public | Token-source property; read by `FrameBuilding.cs:86`. Not on `IQfcDatamodel`. |
| M14 | `SetupWorker(BackgroundWorker)` | 164–171 | public | Sets `WorkerSupportsCancellation = true`, registers `_token` → `worker.CancelAsync()`, subscribes `Worker_DoWork`. The `RunWorkerCompleted` subscription is commented out (line 170). Not on `IQfcDatamodel`. |
| M15 | `Worker_DoWork(object, DoWorkEventArgs)` | 173–211 | private async void | Awaits `RemainingEmailLoader(_token)` into `e.Result` inside a `try/finally` that clears `_remainingLoadActive`; then sets `e.Cancel` if `bw.CancellationPending`; outer `catch` logs. |
| M16 | `Worker_RunWorkerCompleted(object, RunWorkerCompletedEventArgs)` | 216–235 | private | **Dead.** Shows `MessageBox` on cancel/error. Only reference is the commented-out subscription at line 170. |
| M17 | `InitEmailQueue(int batchSize, BackgroundWorker)` | 241–285 | public | `IQfcDatamodel` member. `batchSize <= 0` short-circuit (issue #244) starts the worker and returns an empty list; otherwise clamps batch to `_frame.RowCount`, slices the first batch, drops those rows from `_frame`, projects rows through `GetRowsAs<IEmailSortInfo>()`, resolves each to a `MailItem` via `_olApp.GetNamespace("MAPI").GetItemFromID(EntryId, StoreId)`, then sets `_remainingLoadActive = true` and starts the worker. |
| M18 | `InitEmailQueueAsync(int, BackgroundWorker, CancellationToken, CancellationTokenSource)` | 287–303 | public async | `IQfcDatamodel` member. Throws if the token is already cancelled, stores `_token`/`_tokenSource`/`_worker`, then runs M17 on `Task.Run`. |
| M19 | `LoadRemainingEmailsToQueueAsync(CancellationToken)` | 305–346 | private async | The live worker body. Empty/null `_frame` → `MessageBox.Show("Email Frame is empty")` and `false`. Otherwise projects all rows, resolves each item on `Task.Run`, and calls M20 for `MailItem`s. `OperationCanceledException` → `false`; any other exception → log and `throw e`. `await Task.Yield()` per row. |
| M20 | `TryQueueRemainingMailItemAsync(MailItem, CancellationToken)` | 348–361 | internal async | Builds a `QfcRemainingQueueAdmission` bound to `_globals`, M21, `_masterQueue.AddLast`, `_moveMonitor.HookItem`, and `x => _masterQueue.Remove(x)`; delegates to `TryQueueAsync`. |
| M21 | `ScoreRemainingQueueMailItemAsync(MailItem, CancellationToken)` | 363–377 | private async | `new FolderScoringService()` → `ScoreAsync(mailItem, _globals, cancel)`, logs `Subject`/`EntryID`/score, returns `score.Score`. Also passed as the scorer to `QfcStreamingDequeueConfidenceGate` at `QueueProcessing.cs:119`. |
| M22 | `LoadRemainingEmailsToQueue(BackgroundWorker, CancellationToken)` | 379–417 | private | **Dead.** Synchronous predecessor of M19. Only reference is the commented-out line 186. |
| M23 | `LoadRemainingEmailsToQueueAsync(BackgroundWorker, CancellationToken)` | 419–466 | private async | **Dead.** Obsolete-API predecessor of M19, carrying `#pragma warning disable CS0618`. Only reference is the commented-out line 185. |
| M24 | Empty `#region Linked List Locking` | 470–473 | — | No members. |
| M25 | `Application_NewMailEx(string entryID)` | 477–492 | private | `NewMailEx` handler subscribed in M2/M3 and unsubscribed in M5. Resolves `_globals.Ol.App.Session.GetItemFromID(entryID) as MailItem` and, when non-null, `_masterQueue.AddFirst(item)`. Swallows and logs exceptions. |

Region line accounting (used by the split plan in §7): usings 1–22 (22); namespace/class/logger 23–31
(9); Constructors 32–93 (62); Private Variables 95–130 (36); Public Properties 132–160 (29);
BackgroundWorker 162–237 (76); Email Queue Initial Setup 239–468 (230); Linked List Locking 470–473
(4); Event Handlers 475–494 (20); closing braces 495–496 (2). Total 496.

---

## 3. Current coverage reality

### 3.1 The whole partial type is outside the coverage denominator today — verified

`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
is a committed full-suite Cobertura report from issue #424 (merged as part of the current `main`
lineage). A search of that report for `QuickFiler.Controllers.QfcDatamodel` returns **no class
entry**; the only textual hit is an unrelated `set_DataModel` signature at line 21903. The companion
evidence file `coverage-delta.2026-08-07T00-48.md` states it explicitly:

- line 25: "`QuickFiler/Controllers/QfcDatamodel.cs` … `[ExcludeFromCodeCoverage]` (`QfcDatamodel.cs:25`) — outside the denominator"
- line 26: "`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` … same partial class, therefore also excluded"

This establishes two things:

1. Current measured line coverage for `QfcDatamodel.cs` is **not 0% — it is undefined**; the file is
   absent from the report.
2. The attribute is **type-scoped**, so `QueueProcessing.cs` and `FrameBuilding.cs` are excluded by it
   as well even though neither carries the attribute. Removing it is a three-file event.

### 3.2 Test-to-member map (read-derived)

| Test file | Test method | Members of `QfcDatamodel.cs` actually driven |
| --- | --- | --- |
| `QfcDatamodelTests.cs:49,76,141,168,198` | the five `TryQueueRemainingMailItemAsync_*` tests | **None.** Each calls `CreateQueueAdmission(...)` (line 21) and then `admission.TryQueueAsync(...)` directly. The subject under test is `QfcRemainingQueueAdmission` (`QuickFiler/Controllers/QfcRemainingQueueAdmission.cs`), which belongs to sibling **F2**. M20 is not invoked. |
| `QfcDatamodelTests.cs:103` | `DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive` | M8 setter; `_remainingLoadActive` (declared in `QueueProcessing.cs:21`). Drives `QueueProcessing.cs` members, not this file. |
| `QfcDatamodelTests.cs:250` | `ToggleOfflineMode_WhenOnline_AwaitsInjectedFiveMillisecondDelay` | M8 setter; body is `FrameBuilding.cs:34`. |
| `QfcDatamodelTests.cs:284` | `WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay` | M8 setter; body is `QueueProcessing.cs:168`. |
| `QfcDatamodelLivenessTests.cs:80` | `DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` | M9 setter, **M17 zero-batch branch (249–257)**, **M14**, **M15 happy path**. |
| `QfcDatamodelLivenessTests.cs:189` | `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces` | M9, M17 zero-batch, M14, M15 (up to the first await). |
| `QfcDatamodelLivenessTests.cs:211` | `RemainingLoadActive_AfterLoaderCompletes_BecomesFalse` | M15 `finally` (192–198). |
| `QfcDatamodelLivenessTests.cs:233` | `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` | M15 `finally` on the throwing path plus the outer `catch` (207–210). |
| `QfcInitEmailQueueZeroBatchTests.cs:118` | `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` | **M17 zero-batch branch**, M14. |
| `QfcInitEmailQueueZeroBatchTests.cs:147` | `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` | **M17 zero-batch branch**, M14. |
| `QfcInitEmailQueueZeroBatchTests.cs:177` | `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` | **M17 positive branch (259–284)** with `batchSize == RowCount == 2`, M14, M15. |
| `QfcQueuePurePathsTests.cs:105` | `DequeueNextItemGroupAsync_HighConfidenceDisabled_PreservesDirectBatchDequeue` | Drives `QueueProcessing.cs`; touches no member of this file. |

### 3.3 Members with **zero** coverage today

M2, M3, M4, M5, M6, M10, M11, M12, M13, M16, M18, M19, M20, M21, M22, M23, M25.

Members with **partial** coverage: M14 (fully driven, no direct test), M15 (all but the
`bw.CancellationPending` branch at 202–205 and the `e.Result` assertion), M17 (zero-batch branch and
one positive-batch shape; the `batchSize < _frame.RowCount` **true** arm of the clamp at line 260 is
uncovered because the only positive-batch test uses `batchSize == RowCount == 2`, which takes the
false arm), M8/M9 (setters only).

Rough read-derived line arithmetic: of ~230 executable lines in the file, roughly 40 are currently
reached. **Confidence: medium.** This is a hand count, not a measurement — F1's harness is the
authority and the plan must record its numeric output under `<FEATURE>/evidence/qa-gates/`.

### 3.4 Dead members — verified unreferenced

A repository-wide `*.cs` search for `Worker_RunWorkerCompleted` and `LoadRemainingEmailsToQueue`
returns, for this type:

- `Worker_RunWorkerCompleted` → declaration at `QfcDatamodel.cs:216` and the **commented-out**
  subscription at `QfcDatamodel.cs:170`. (The other hits — `QfcHomeController.cs:94,134,326` and
  `QfcHomeControllerRunAsyncTests.cs:326,349` — are a same-named member on a different type.)
- `LoadRemainingEmailsToQueue(BackgroundWorker, CancellationToken)` → declaration at line 379, plus
  **`nameof(...)` uses inside its own body and inside M19's log strings** (lines 333, 339, 405, 411).
  No call site.
- `LoadRemainingEmailsToQueueAsync(BackgroundWorker, CancellationToken)` → declaration at line 419;
  the only other textual reference is the commented-out line 185. The method-group assignments at
  lines 40 and 51 bind to the **one-argument** overload (M19), because
  `RemainingEmailLoader` is `Func<CancellationToken, Task<bool>>`.
- `log` (line 97) → a `\blog\b` search across `QfcDatamodel*.cs` returns only the declaration.

---

## 4. Testability blockers, per uncovered member

| Member | Blocker(s) | Blocking? |
| --- | --- | --- |
| M2 / M3 ctors | `_globals.Ol.App` and `_olApp.ActiveExplorer()` are COM types but both are Moq-able interop interfaces (precedent: `QfcInitEmailQueueZeroBatchTests.cs:190–198` mocks `Application` and `NameSpace`; `QfcDatamodelTests.cs:259` mocks `Explorer`). `_globals.Ol.App.NewMailEx += Application_NewMailEx` is an **interop event add-accessor on a dynamic proxy** — see risk R1. **M3 additionally calls `InitDf(_activeExplorer)`**, which reaches `DfDeedle.GetEmailDataInView` → `activeExplorer.GetTableInView()` and `DfDeedle.TableEtlInvoker`. `TableEtlInvoker` is `internal static` in `UtilitiesCS` (`DfDeedle.cs:69`) and `UtilitiesCS/Properties/AssemblyInfo.cs:19-20` grants `InternalsVisibleTo` only to `UtilitiesCS.Test` and `ToDoModel.Test` — **not** `QuickFiler.Test` (there is even a deliberately commented-out grant at `UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs:15`). So the frame build is unreachable from this test assembly. | Yes for M3 |
| M4 `LoadAsync` | Same as M2, plus `model.InitDfAsync(...)` → `GetEmailsInViewDfAsync` → `DfDeedle.GetEmailDataInViewAsync` (same `InternalsVisibleTo` wall). `ProgressTracker` is **not** a blocker: it is a concrete class whose `Report`/`Increment`/`SpawnChild` members are all `virtual` (`ProgressTracker.cs:103,109,116,121,141,218`), so `Mock<ProgressTracker>` with a `CancellationTokenSource` ctor arg works. (A bare `new ProgressTracker(cts)` without `Initialize()` would NRE inside `Report(double)` at `ProgressTracker.cs:158` because `_parent.Progress` is null — so tests must mock, not construct.) | Yes |
| M5 `Cleanup` | None that are structural. `_moveMonitor` is already an interface seam (`IEmailMoveMonitor`, mocked at `QfcQueuePurePathsTests.cs:119`). `NewMailEx -=` shares risk R1. `_worker?.CancelAsync()` throws `InvalidOperationException` when `WorkerSupportsCancellation` is false — a test-arrangement detail, not a blocker. | No — testable today |
| M10–M13 properties | None. `MovedItems` needs `Mock<IApplicationGlobals>.AF.MovedMails`. | No |
| M14 `SetupWorker` | None. | No |
| M15 `Worker_DoWork` cancellation branch | `bw.CancellationPending` must be true when the awaited loader returns. Reachable by holding the loader open with a `TaskCompletionSource`, calling `worker.CancelAsync()`, then releasing — the exact gating pattern already used at `QfcDatamodelLivenessTests.cs:89-95,153-181`. Method is `async void`, so completion must be observed with the existing bounded `SpinWait.SpinUntil` helper (`QfcDatamodelLivenessTests.cs:54-57`), which is condition-driven, not a wall-clock sleep. | No |
| M16 `Worker_RunWorkerCompleted` | `MessageBox.Show` at lines 221 and 227 — a modal popup requiring human interaction, which epic.md § "Seam hierarchy" names as a unit-test-policy violation. **Also dead.** | Delete |
| M17 uncovered arms | None. Negative `batchSize`, `batchSize < RowCount`, and `batchSize > RowCount` are all reachable with the existing `CreateTwoRowEmailFrame` fixture (`QfcInitEmailQueueZeroBatchTests.cs:63`). | No |
| M18 `InitEmailQueueAsync` | None. | No |
| M19 `LoadRemainingEmailsToQueueAsync` | `MessageBox.Show("Email Frame is empty")` at line 309 gates the entire null/empty-frame branch and would pop a modal dialog in any test that reaches it — this is the exact defect the `QfcInitEmailQueueZeroBatchTests` remarks block (lines 23–31) says "this file must never reproduce". The rest (`_olApp.GetNamespace("MAPI").GetItemFromID`, `Task.Run`, `Task.Yield`) is Moq-able. | Yes — needs a MessageBox seam |
| M20 `TryQueueRemainingMailItemAsync` | None structural; needs `_globals`, `_masterQueue`, `_moveMonitor` set. | No |
| M21 `ScoreRemainingQueueMailItemAsync` | `new FolderScoringService()` is hard-coded at line 368. `FolderScoringService` (`QfcHighConfidencePreFilter.cs:167`) is itself `[ExcludeFromCodeCoverage]` and COM-bound (`MailItemHelper.FromMailItemAsync` + `FolderPredictor`), so any test reaching it needs live Outlook. | Yes — needs the existing `IFolderScoringService` seam injected |
| M22 / M23 | `MessageBox.Show` (lines 383, 426) plus COM; `M23` also carries a `CS0618` suppression. **Both dead.** | Delete |
| M25 `Application_NewMailEx` | `_globals.Ol.App.Session.GetItemFromID(entryID)` — Moq-able. Private, so invoke by reflection (established pattern, `QfcDatamodelTests.cs:263,298`). | No |

---

## 5. Seam proposals

All five are additive. `IQfcDatamodel` is untouched. The public ctor and the public 4-argument
`LoadAsync` keep byte-identical signatures.

### S1 — `IFolderScoringService` injection into M21 (interface seam — highest tier)

Reuses an interface that **already exists** at `QfcHighConfidencePreFilter.cs:130`, so no new
abstraction is introduced.

```csharp
/// <summary>Scoring seam. Null means "use the production FolderScoringService".</summary>
internal IFolderScoringService ScoringService { get; set; }
```

Call site change at `QfcDatamodel.cs:368`:

```csharp
var scoringService = ScoringService ?? new FolderScoringService();
```

Null-coalescing rather than a property initializer, because `FormatterServices.GetUninitializedObject`
bypasses initializers and every existing datamodel test uses that construction path. This mirrors the
idiom already in the repo at `QfcHighConfidencePreFilter.cs:63`
(`var service = scoringService ?? new FolderScoringService();`). Moq can proxy the internal interface
because `QfcHighConfidencePreFilter.cs:11` declares
`[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]`, and
`QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`.

**Bonus:** M21 is also the scorer handed to `QfcStreamingDequeueConfidenceGate` at
`QueueProcessing.cs:119`, so this seam raises the reachable surface in the sibling partial too.

### S2 — MessageBox invoker for M19 (injectable delegate seam)

```csharp
/// <summary>
/// Injectable message-box seam. Null means the production <see cref="MessageBox.Show(string)"/>.
/// Exists so the empty-frame branch of the remaining-email loader can be exercised without a modal
/// dialog, which unit-test policy prohibits.
/// </summary>
internal Func<string, DialogResult> MessageBoxInvoker { get; set; }
```

Call site at line 309:

```csharp
(MessageBoxInvoker ?? MessageBox.Show)("Email Frame is empty");
```

A delegate rather than an interface because there is exactly one call shape. Direct precedent:
`DfDeedle.MessageBoxInvoker` at `UtilitiesCS/Extensions/DfDeedle.cs:52-60`. Difference from that
precedent: declare it as an **instance** property, not a mutable `static`, so tests stay independent
per `.claude/rules/general-unit-test.md` § Core Principles item 1.

### S3 — Frame-builder parameter on an additive internal constructor (injectable delegate seam)

```csharp
internal QfcDatamodel(
    IApplicationGlobals appGlobals,
    CancellationToken token,
    Func<Explorer, Frame<int, string>> frameBuilder)
{
    _globals = appGlobals;
    _token = token;
    _olApp = _globals.Ol.App;
    _activeExplorer = _olApp.ActiveExplorer();
    _frame = (frameBuilder ?? InitDf)(_activeExplorer);
    _globals.Ol.App.NewMailEx += Application_NewMailEx;
    RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;
}

public QfcDatamodel(IApplicationGlobals appGlobals, CancellationToken token)
    : this(appGlobals, token, null) { }
```

Statement order is preserved exactly, so there is no observable behavior change. `InitDf` is an
instance method defined in `QfcDatamodel.FrameBuilding.cs:13`; referencing it as a method group in a
constructor **body** is legal (unlike a field initializer — the CS0236 constraint documented at
`QfcDatamodel.cs:114-127` applies only to initializers). **This does not require editing
`FrameBuilding.cs`.**

### S4 — Data-frame initializer parameter on an additive internal `LoadAsync` overload (injectable delegate seam)

```csharp
internal static async Task<QfcDatamodel> LoadAsync(
    IApplicationGlobals appGlobals,
    CancellationToken token,
    CancellationTokenSource tokenSource,
    ProgressTracker progress,
    Func<QfcDatamodel, Explorer, ProgressTracker, Task> dataFrameInitializer)

public static Task<QfcDatamodel> LoadAsync(
    IApplicationGlobals appGlobals,
    CancellationToken token,
    CancellationTokenSource tokenSource,
    ProgressTracker progress)
    => LoadAsync(appGlobals, token, tokenSource, progress, null);
```

Inside the internal overload, line 69–71 becomes:

```csharp
var initializer = dataFrameInitializer ?? ((m, e, p) => m.InitDfAsync(e, p));
await initializer(model, appGlobals.Ol.App.ActiveExplorer(), progress.Increment(2))
    .ConfigureAwait(false);
```

A parameter rather than an instance property is required because `LoadAsync` constructs the model
itself, so a test has no instance to configure beforehand. A mutable `static` seam is rejected for the
same independence reason as S2. The public 4-argument signature bound by `QfcHomeController.cs:173`
is unchanged.

### S5 — `NewMailEx` subscribe/unsubscribe delegates (injectable delegate seam) — **contingency only**

Adopt **only if** risk R1 materialises (Moq cannot proxy the interop event add/remove accessors):

```csharp
internal Action<Outlook.Application> NewMailSubscriber { get; set; }
internal Action<Outlook.Application> NewMailUnsubscriber { get; set; }
```

with call sites `(NewMailSubscriber ?? (app => app.NewMailEx += Application_NewMailEx))(_globals.Ol.App);`
in M2/M3 and the mirror in M5. Listed last deliberately: if R1 does not materialise this seam is
unnecessary indirection and violates "introduce the smallest seam that enables reliable unit testing"
(`.claude/rules/csharp.md` § DI Seams).

### Additivity confirmation

| Seam | Touches `IQfcDatamodel`? | Touches a public signature? | Cross-child impact |
| --- | --- | --- | --- |
| S1 | No | No | None |
| S2 | No | No | None |
| S3 | No | No — public ctor becomes a `: this(...)` chain with identical parameters | `QfcHomeController.cs:163` (F7) unaffected |
| S4 | No | No — public static overload retained verbatim | `QfcHomeController.cs:173` (F7) unaffected |
| S5 | No | No | None |

**No cross-child contract note for `spec.md` is required.** No breaking change is proposed.

---

## 6. `[ExcludeFromCodeCoverage]` disposition

**Recommendation: remove the attribute at `QfcDatamodel.cs:25` entirely. No irreducible remainder is
claimed for this file.**

Justification against the epic's irreducible-remainder standard (epic.md § "Shared Design" 1):

- The only members with no injectable seam and an unavoidable host dependency were M16, M22 and M23
  (`MessageBox.Show` plus live COM). All three are **dead code** and are deleted, so they never reach
  the ledger.
- Every surviving member is reachable through S1–S4 plus the already-present `IEmailMoveMonitor`,
  `TimeProvider` and `RemainingEmailLoader` seams. The CLAUDE.md § UT2 qualifier "without an
  injectable seam" therefore does not apply to any member of this file.
- This file is not WinForms form-derived and contains no Designer-generated code, so exemption clauses
  (b) and the designer part of (a) are inapplicable.

**Two conditions the plan must honour:**

1. **Sequencing (blocking).** Because the attribute is type-scoped (§3.1), the task that removes it
   also admits `QfcDatamodel.QueueProcessing.cs` and `QfcDatamodel.FrameBuilding.cs` into the
   denominator. `FrameBuilding.cs` contains the genuinely COM-bound `InitDf` (line 13),
   `InitDfAsync` (line 48), `GetEmailsInViewDfAsync` (line 69) and `ToggleOfflineMode` (line 34)
   whose `DfDeedle` dependencies are behind an `InternalsVisibleTo` wall this test assembly cannot
   cross. The removal task must therefore be the **last** production task of the feature, after the
   FrameBuilding phase has either seamed those members or applied **member-level**
   `[ExcludeFromCodeCoverage]` attributes with a ledger entry. Member-level attributes are the
   recommended shape, because they let this file reach the floor without keeping a type-wide blanket.
2. **Ledger reference (informational).** F1's ledger at
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` remains the authority for the
   `testable` vs `ratified-exempt` classification of all three partial files. This artifact asserts
   `QfcDatamodel.cs` = `testable` with no exempt members; if the ledger disagrees, the ledger wins and
   this section must be revisited.

---

## 7. 500-line split plan

Current: **496 lines. Headroom: 4.** The plan below produces headroom without a large mechanical move,
because most of the reduction comes from deleting verified-dead code.

### Step 1 — deletions (no behavior change; all verified unreferenced in §3.4)

| Removed | Lines removed (incl. adjacent blank/comment) | Count |
| --- | --- | --- |
| `log` static field | 96–99 | 4 |
| `Worker_RunWorkerCompleted` + its 3-line explanatory comment | 212–236 | 25 |
| `LoadRemainingEmailsToQueue(BackgroundWorker, CancellationToken)` | 378–417 | 40 |
| `LoadRemainingEmailsToQueueAsync(BackgroundWorker, CancellationToken)` | 418–466 | 49 |
| Empty `#region Linked List Locking` | 469–473 | 5 |
| **Total** | | **123** |

Result: **496 − 123 = 373 lines.**

Note two incidental cleanups that fall out of this: two `MessageBox.Show` call sites disappear, and
the `#pragma warning disable CS0618` / `restore` pair at lines 437/458 disappears with M23, removing a
suppression rather than adding one.

### Step 2 — seam additions in place

| Addition | Est. lines (with XML docs) |
| --- | --- |
| S1 `ScoringService` property + call-site edit | 10 |
| S2 `MessageBoxInvoker` property + call-site edit | 10 |
| S3 internal ctor + public `: this(...)` chain | 22 |
| S4 internal `LoadAsync` overload + public delegating overload | 26 |
| **Total** | **68** |

Delete-only outcome: **373 + 68 = ~441 lines** — compliant, but only 59 lines of headroom, which is
thin for a file that sibling phases may also touch.

### Step 3 — recommended partial split

Create **`QuickFiler/Controllers/QfcDatamodel.Construction.cs`** and move the lifecycle-and-seams
concern into it. This is a new file, so it does not collide with the `QueueProcessing` or
`FrameBuilding` phases.

| File | Contents after split | Est. lines |
| --- | --- | --- |
| `QfcDatamodel.cs` | usings; `logger`; private fields M7; public properties M10–M13; BackgroundWorker region (M14, M15); Email Queue Initial Setup region (M17, M18, M19, M20, M21); Event Handlers region (M25) | **~311** |
| `QfcDatamodel.Construction.cs` (new) | usings/namespace/class scaffolding (~20); constructors M2, M3 + the S3 internal ctor; `LoadAsync` M4 + the S4 internal overload; `Cleanup` M5; seam properties `TimeProvider` (M8), `RemainingEmailLoader` (M9), `ScoringService` (S1), `MessageBoxInvoker` (S2) | **~168** |
| `QfcDatamodel.QueueProcessing.cs` | unchanged by this file's plan | 177 |
| `QfcDatamodel.FrameBuilding.cs` | unchanged by this file's plan | 154 |

Rationale for the cut line: constructors, the async factory, `Cleanup` and the DI seam declarations
are one cohesive concern (object lifecycle and dependency injection), and all four new seams land in
one file rather than being scattered. Moving M8/M9 out of `QfcDatamodel.cs` is safe — both are members
of the same partial type and their consumers (`QueueProcessing.cs:173`, `FrameBuilding.cs:43`,
`QfcDatamodelTests.cs`, `QfcDatamodelLivenessTests.cs`, `QfcInitEmailQueueZeroBatchTests.cs`) bind by
member name, not by file.

**Coordination note.** If the `QueueProcessing` or `FrameBuilding` phases also plan to add seam
properties, they should add them to `QfcDatamodel.Construction.cs` rather than to their own files, to
keep one DI surface. This is a plan-ordering item, not a file-ownership conflict.

---

## 8. Enumerated test cases

Each item is intended to become a single atomic plan task. All use MSTest `[TestClass]`/`[TestMethod]`,
Moq, FluentAssertions and Arrange–Act–Assert. None uses `Thread.Sleep`, `Task.Delay`, a real
wall-clock wait, a temporary file, an external service, a live form, a modal dialog, or the UI thread.
Where a state transition on a `BackgroundWorker` must be observed, tests use the existing
condition-driven bounded helper `WaitForState` / `SpinWait.SpinUntil` pattern
(`QfcDatamodelLivenessTests.cs:54-57`), which returns as soon as the predicate holds and fails with a
message otherwise — it is not a fixed sleep.

Shared arrangement helpers (`CreateUninitializedDatamodel`, `SetPrivateField`,
`CreateTwoRowEmailFrame`) are duplicated per test file, following the convention already documented
at `QfcDatamodelLivenessTests.cs:18-24`.

### T-file A — `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs` (new)

| # | Test method | Member | Category | Arrange / Act / Assert sketch |
| --- | --- | --- | --- | --- |
| 1 | `Constructor_WithInjectedFrameBuilder_AssignsGlobalsExplorerAndFrame` | M3 via S3 | positive | **A:** `Mock<Explorer>`; `Mock<Application>` with `ActiveExplorer()` → explorer; `Mock<IOlObjects>.App` → application; `Mock<IApplicationGlobals>.Ol` → olObjects; a `Frame` from `CreateTwoRowEmailFrame()`; frame-builder delegate returning it. **Act:** `new QfcDatamodel(globals, CancellationToken.None, e => frame)`. **Assert:** `_globals`, `_olApp`, `_activeExplorer`, `_frame` (read by reflection) are the supplied instances; the builder received the explorer returned by `ActiveExplorer()`. |
| 2 | `Constructor_DefaultsRemainingEmailLoaderToTheLiveLoader` | M3 | positive | Same arrangement. **Assert:** `RemainingEmailLoader` is not null and `Method.Name` is `LoadRemainingEmailsToQueueAsync`. Pins the CS0236 workaround documented at `QfcDatamodel.cs:114-127`. |
| 3 | `Constructor_SubscribesToApplicationNewMailEx` | M3 | positive | **Assert:** `application.VerifyAdd(a => a.NewMailEx += It.IsAny<ApplicationEvents_11_NewMailExEventHandler>(), Times.Once)`. **Fallback if R1 materialises:** drop to S5 and verify the injected `NewMailSubscriber` delegate ran once. |
| 4 | `Constructor_WithNullFrameBuilder_FallsBackToInitDf` | M3 / S3 | boundary | **A:** as #1 but pass `null` as the builder and a `Mock<Explorer>` whose `GetTableInView()` throws a sentinel. **Act/Assert:** the sentinel escapes, proving the null path binds `InitDf` rather than silently skipping the frame build. Chosen deliberately so the test does **not** need `DfDeedle` internals. |
| 5 | `LoadAsync_ReportsZeroProgressBeforeConstructingTheModel` | M4 / S4 | ordering | **A:** `Mock<ProgressTracker>(MockBehavior.Loose, new CancellationTokenSource())` recording call order; mocked globals; an initializer delegate that records. **Act:** internal `LoadAsync`. **Assert:** `Report(0, "Initializing Data Model")` was invoked, and it was invoked before the initializer. |
| 6 | `LoadAsync_AssignsTokenAndTokenSourceToTheReturnedModel` | M4 | state-transition | **Assert:** `model.Token` equals the supplied token and `model.TokenSource` is the supplied source. |
| 7 | `LoadAsync_PassesActiveExplorerAndIncrementedProgressToTheInitializer` | M4 / S4 | positive | **Assert:** the initializer received the `Explorer` from `appGlobals.Ol.App.ActiveExplorer()` and the tracker returned by `progress.Increment(2)`; verify `Increment(2)` once. |
| 8 | `LoadAsync_WhenInitializerThrows_PropagatesAndReturnsNoModel` | M4 | error-handling | **A:** initializer returns a faulted task. **Assert:** `await act.Should().ThrowAsync<InvalidOperationException>()`. Confirms `LoadAsync` has no swallow path. |
| 9 | `Cleanup_CancelsTokenSourceAndBackgroundWorker` | M5 | positive | **A:** uninitialized model; real `CancellationTokenSource`; `BackgroundWorker { WorkerSupportsCancellation = true }`; `Mock<IEmailMoveMonitor>`; mocked globals. **Act:** `Cleanup()`. **Assert:** `tokenSource.IsCancellationRequested` true; `worker.CancellationPending` true. |
| 10 | `Cleanup_UnsubscribesNewMailExAndUnhooksAllMonitoredItems` | M5 | positive | **Assert:** `application.VerifyRemove(a => a.NewMailEx -= It.IsAny<...>(), Times.Once)`; `moveMonitor.Verify(m => m.UnhookAll(), Times.Once)`. Same R1 fallback as #3. |
| 11 | `Cleanup_NullsEveryRetainedReference` | M5 | state-transition | **Assert:** each of `_moveMonitor`, `_activeExplorer`, `_olApp`, `_globals`, `_frame`, `_masterQueue`, `_worker` reads back null. |
| 12 | `Cleanup_WithNullTokenSourceAndWorker_DoesNotThrow` | M5 | boundary | **A:** leave `_tokenSource` and `_worker` null (the `?.` arms at lines 77–78). **Assert:** `act.Should().NotThrow()`. |

### T-file B — `QuickFiler.Test/Controllers/QfcDatamodelWorkerTests.cs` (new)

| # | Test method | Member | Category | Sketch |
| --- | --- | --- | --- | --- |
| 13 | `SetupWorker_EnablesCancellationSupportAndAttachesDoWorkHandler` | M14 | positive | **A:** uninitialized model, plain `BackgroundWorker`. **Act:** `SetupWorker(worker)`. **Assert:** `WorkerSupportsCancellation` true; running the worker with an injected inert `RemainingEmailLoader` reaches that loader (bounded `TaskCompletionSource` wait), proving `DoWork` was wired. |
| 14 | `SetupWorker_WhenTokenIsCancelled_RequestsWorkerCancellation` | M14 | state-transition | **A:** `_token` from a real `CancellationTokenSource`. **Act:** `SetupWorker(worker)` then `cts.Cancel()`. **Assert:** `worker.CancellationPending` true — covers the `_token.Register(...)` callback at line 168. |
| 15 | `WorkerDoWork_AssignsLoaderResultToEventArgsResult` | M15 | positive | **A:** invoke `Worker_DoWork` by reflection with a `BackgroundWorker` sender and a locally constructed `DoWorkEventArgs`; loader returns `true` after a released `TaskCompletionSource`. **Assert:** `WaitForState(() => e.Result is bool)` then `e.Result.Should().Be(true)` — covers line 189, currently unasserted. |
| 16 | `WorkerDoWork_WhenCancellationPending_SetsEventArgsCancel` | M15 | state-transition | **A:** hold the loader open; `worker.CancelAsync()`; release. **Assert:** `WaitForState(() => e.Cancel)` — covers lines 202–205, the only wholly uncovered branch of M15. |

### T-file C — `QuickFiler.Test/Controllers/QfcDatamodelInitEmailQueueTests.cs` (new)

Kept separate from `QfcInitEmailQueueZeroBatchTests.cs` so these atomic tasks do not serialise against
that issue-scoped file.

| # | Test method | Member | Category | Sketch |
| --- | --- | --- | --- | --- |
| 17 | `InitEmailQueue_NegativeBatchSize_ReturnsEmptyListAndStartsWorker` | M17 | invalid-input | **A:** two-row frame; inert `RemainingEmailLoader`. **Act:** `InitEmailQueue(-1, worker)`. **Assert:** empty non-null list; `WorkerSupportsCancellation` true; loader reached. Distinct boundary from the existing zero case. |
| 18 | `InitEmailQueue_BatchSmallerThanRowCount_TakesRequestedRowsAndRetainsRemainder` | M17 | boundary | **A:** two-row frame; mocked `NameSpace.GetItemFromID`. **Act:** `InitEmailQueue(1, worker)`. **Assert:** one item returned, matching `EntryId-1`; `_frame.RowCount == 1`. Covers the currently-uncovered **true** arm of the clamp at line 260. |
| 19 | `InitEmailQueue_BatchLargerThanRowCount_ClampsToRowCount` | M17 | boundary | **Act:** `InitEmailQueue(5, worker)` on a two-row frame. **Assert:** two items; `_frame.RowCount == 0`; no exception. |
| 20 | `InitEmailQueue_PositiveBatch_SetsProducerLivenessFlagBeforeStartingWorker` | M17 | ordering | **A:** loader held open by a `TaskCompletionSource`. **Act:** `InitEmailQueue(1, worker)`. **Assert:** `_remainingLoadActive` reads true while the loader is held. Covers line 281 — the zero-batch twin (line 254) is already covered, this arm is not. |
| 21 | `InitEmailQueueAsync_DelegatesToInitEmailQueueAndReturnsTheSameItems` | M18 | positive | **Act:** `await InitEmailQueueAsync(1, worker, CancellationToken.None, new CancellationTokenSource())`. **Assert:** the returned list equals the `InitEmailQueue(1, ...)` result shape. |
| 22 | `InitEmailQueueAsync_AssignsTokenTokenSourceAndWorkerFields` | M18 | state-transition | **Assert:** `_token`, `_tokenSource`, `_worker` (reflection) are the supplied instances — covers lines 296–298. |
| 23 | `InitEmailQueueAsync_WithPreCancelledToken_ThrowsOperationCanceled` | M18 | error-handling | **A:** already-cancelled token. **Assert:** `await act.Should().ThrowAsync<OperationCanceledException>()`; `_worker` unchanged, proving line 294 short-circuits. |

### T-file D — `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs` (new)

| # | Test method | Member | Category | Sketch |
| --- | --- | --- | --- | --- |
| 24 | `LoadRemainingEmailsToQueueAsync_WithNullFrame_ReportsEmptyFrameAndReturnsFalse` | M19 / S2 | invalid-input | **A:** `_frame` null; `MessageBoxInvoker` records the message and returns `DialogResult.OK`. **Assert:** result false; recorded message == `"Email Frame is empty"`; no queue mutation. |
| 25 | `LoadRemainingEmailsToQueueAsync_WithZeroRowFrame_ReportsEmptyFrameAndReturnsFalse` | M19 / S2 | boundary | As #24 with a zero-row `Frame`; covers the second disjunct of line 307. |
| 26 | `LoadRemainingEmailsToQueueAsync_QueuesEveryResolvedMailItemInFrameOrder` | M19 | positive | **A:** two-row frame; `NameSpace.GetItemFromID` returns distinct `Mock<MailItem>`s; `_masterQueue` real `LockingLinkedList<MailItem>`; `Mock<IEmailMoveMonitor>`; mocked globals. **Assert:** result true; queue holds both in row order; `HookItem` called twice. |
| 27 | `LoadRemainingEmailsToQueueAsync_SkipsRowsThatDoNotResolveToAMailItem` | M19 | negative | **A:** `GetItemFromID` returns null for row 1 and a `MailItem` for row 2. **Assert:** result true; queue holds exactly one item — covers the false arm of line 326. |
| 28 | `LoadRemainingEmailsToQueueAsync_WithPreCancelledToken_ReturnsFalseWithoutQueueing` | M19 | error-handling | **A:** already-cancelled token. **Assert:** result false; queue empty; `HookItem` never called — covers the `OperationCanceledException` arm at 331–335. |
| 29 | `LoadRemainingEmailsToQueueAsync_WhenItemResolutionThrows_Rethrows` | M19 | error-handling | **A:** `GetItemFromID` throws `InvalidOperationException`. **Assert:** `await act.Should().ThrowAsync<InvalidOperationException>()` — covers the `throw e` arm at 336–342. |
| 30 | `TryQueueRemainingMailItemAsync_AddsItemToMasterQueueAndHooksTheMoveMonitor` | M20 | positive | **A:** real `LockingLinkedList<MailItem>`; `Mock<IEmailMoveMonitor>`; mocked globals; `Mock<MailItem>`. **Act:** `await model.TryQueueRemainingMailItemAsync(mail, CancellationToken.None)`. **Assert:** true; queue count 1; `HookItem(mail, It.IsAny<Action<MailItem>>())` once. **This is the first test that actually invokes the datamodel method** (see §3.2). |
| 31 | `TryQueueRemainingMailItemAsync_NullMailItem_ReturnsFalseWithoutTouchingTheQueue` | M20 | invalid-input | **Assert:** false; queue empty; `HookItem` never called. |
| 32 | `TryQueueRemainingMailItemAsync_HookCallback_RemovesTheItemFromTheMasterQueue` | M20 | state-transition | **A:** capture the `Action<MailItem>` passed to `HookItem`. **Act:** invoke the captured callback. **Assert:** the queue no longer contains the item — covers the `x => _masterQueue.Remove(x)` closure at line 358, which nothing exercises today. |
| 33 | `ScoreRemainingQueueMailItemAsync_ReturnsTheScoreFromTheInjectedScoringService` | M21 / S1 | positive | **A:** `Mock<IFolderScoringService>` returning `(1234L, "Some\\Folder")`; `Mock<MailItem>` with `Subject`/`EntryID` for the log line; mocked globals. **Act:** reflection-invoke `ScoreRemainingQueueMailItemAsync`. **Assert:** returns `1234`. |
| 34 | `ScoreRemainingQueueMailItemAsync_PropagatesScoringServiceFailure` | M21 / S1 | error-handling | **A:** the mock throws. **Assert:** the exception escapes unchanged — M21 has no catch, and this pins that. |

### T-file E — `QuickFiler.Test/Controllers/QfcDatamodelStateTests.cs` (new)

| # | Test method | Member | Category | Sketch |
| --- | --- | --- | --- | --- |
| 35 | `Complete_RoundTripsTheAssignedValue` | M10 | state-transition | **Assert:** default false; after `Complete = true` reads true. |
| 36 | `MovedItems_ReturnsTheMovedMailsStackFromGlobals` | M11 | positive | **A:** `Mock<IAppAutoFileObjects>.MovedMails` → a `SloStack<IMovedMailInfo>`; `Mock<IApplicationGlobals>.AF` → it. **Assert:** `model.MovedItems` is the same instance. |
| 37 | `TokenAndTokenSource_RoundTripTheAssignedValues` | M12, M13 | positive | **Assert:** both getters return what was set. |
| 38 | `ApplicationNewMailEx_WithResolvableMailItem_AddsItToTheFrontOfTheMasterQueue` | M25 | positive/ordering | **A:** queue pre-seeded with one existing item; `Session.GetItemFromID(entryId)` returns a `Mock<MailItem>`. **Act:** reflection-invoke `Application_NewMailEx("Entry-1")`. **Assert:** the new item is first (`AddFirst`, not `AddLast`). |
| 39 | `ApplicationNewMailEx_WhenItemIsNotAMailItem_DoesNotEnqueue` | M25 | negative | **A:** `GetItemFromID` returns a non-`MailItem` object. **Assert:** queue unchanged — covers the false arm of line 483. |
| 40 | `ApplicationNewMailEx_WhenSessionThrows_SwallowsTheException` | M25 | error-handling | **A:** `Session` getter throws. **Assert:** `act.Should().NotThrow()`; queue unchanged — covers the catch at 488–491. |

**Scenario-completeness check against `.claude/rules/general-unit-test.md` § Scenario Completeness:**
positive (1,2,3,5,6,7,9,10,13,21,26,30,33,35,36,37,38), invalid input (17,24,31,39), boundary
(4,12,18,19,25), error handling (8,20?,23,28,29,34,40), state transition (6,11,14,16,22,32,35),
concurrency/ordering (5,15,16,20,26,38). Concurrency is exercised only through the deterministic
`FakeTimeProvider`/`TaskCompletionSource` gating already established in this test tree; no new
threading primitives are introduced.

---

## 9. Risks and open questions

| ID | Item | Impact | Handling |
| --- | --- | --- | --- |
| **R1** | **Moq proxying of the Outlook interop event `Application.NewMailEx`.** Tests 3 and 10 rely on `VerifyAdd`/`VerifyRemove` against a `Mock<Microsoft.Office.Interop.Outlook.Application>`. Castle DynamicProxy normally implements interface events, but embedded-interop / `[ComEventInterface]` event declarations are an edge case I could **not** verify without building. **INFERRED.** | Two of forty tests; also affects whether M2/M3/M5 can be covered without extra indirection. | Plan the seam S5 as an explicit contingency task, taken only if the direct approach fails at build/run time. Do not adopt S5 pre-emptively. |
| **R2** | **Attribute-removal sequencing.** Removing `[ExcludeFromCodeCoverage]` admits `FrameBuilding.cs` (154 lines, heavily COM-bound) into the denominator in the same commit. | If done early, the feature's own per-file gate reports a large regression on a file this phase does not own. | Make attribute removal the final production task; require the FrameBuilding phase to have landed member-level exemptions or seams first. Record the dependency in `spec.md` and the plan. |
| **R3** | **F1 ledger disagreement.** This artifact asserts `QfcDatamodel.cs` = `testable`, zero exempt members. F1's ledger does not exist on disk yet. | If F1 ratifies a type-wide exemption for `QfcDatamodel`, §6 and the AC in `issue.md` line 61 change. | Treat the ledger as authoritative on arrival; re-read §6 at plan time. |
| **R4** | **Deleting dead code is a scope judgement.** M16/M22/M23 removal is not strictly "adding coverage". | ~112 lines of diff not directly attributable to a test. | Recommended and justified: they are verified unreachable (§3.4), they are the only permanently-uncoverable lines in the file, and `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy directs refactoring over exclusion. Record the decision explicitly in `spec.md` so review does not read it as scope creep. |
| **R5** | **`Cleanup()` is not idempotent.** A second call NREs at line 80 (`_moveMonitor.UnhookAll()` after `_moveMonitor = null`). Verified by reading lines 79–84. | Latent defect, not caused by this feature. | Do **not** add a guard here — that is a behavior change, which AC7 forbids. Promote as a separate issue through the MCP promotion lifecycle rather than leaving it as prose. No test in §8 pins the NRE. |
| **R6** | **`QfcDatamodelTests.cs` contains five tests for a file owned by sibling F2.** Lines 21–219 test `QfcRemainingQueueAdmission`. | Misleading names; F2's coverage evidence may double-count them. | Report to the epic; do **not** move them (that would conflict with F2's plan and churn a 317-line file). Note it in `spec.md` as a cross-child observation. |
| **R7** | **`ProgressTracker` mocking depth.** Tests 5–8 mock a concrete class with virtual members. `SpawnChild`/`Increment` return `ProgressTracker`, so a loose mock returns null unless configured. | Test-arrangement complexity only. | Configure `Increment(It.IsAny<double>())` to return the mock itself. Verified that all four members used are `virtual` (`ProgressTracker.cs:103,109,121,218`). |
| **Q1** | Should the `#region` structure be preserved after the split? | Cosmetic. | Recommend keeping the existing region names in whichever file the members land, for reviewability of the move diff. |
| **Q2** | Should `TryQueueRemainingMailItemAsync` stay `internal` or become `private`? It has no production caller outside M19. | Public-surface minimality (`.claude/rules/csharp.md` § Public surface). | Keep `internal`: tests 30–32 call it directly, and narrowing it would force reflection for no benefit. |

---

## 10. Files this phase would touch

| Path | Action |
| --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.cs` | Delete M6, M16, M22, M23, M24; add S1/S2 call-site edits; move M2–M5, M8, M9 out |
| `QuickFiler/Controllers/QfcDatamodel.Construction.cs` | **New.** Lifecycle members + all four seams |
| `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs` | **New.** Tests 1–12 |
| `QuickFiler.Test/Controllers/QfcDatamodelWorkerTests.cs` | **New.** Tests 13–16 |
| `QuickFiler.Test/Controllers/QfcDatamodelInitEmailQueueTests.cs` | **New.** Tests 17–23 |
| `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs` | **New.** Tests 24–34 |
| `QuickFiler.Test/Controllers/QfcDatamodelStateTests.cs` | **New.** Tests 35–40 |
| `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj` | `<Compile Include>` entries for the new files (legacy non-SDK projects require explicit items) |

Explicitly **not** touched: `coverage.config`, any shared build property file, `QfcDatamodel.QueueProcessing.cs`,
`QfcDatamodel.FrameBuilding.cs`, `IQfcDatamodel.cs`, `EfcDataModel.cs`, `QfcRemainingQueueAdmission.cs`,
`QfcHighConfidencePreFilter.cs`, `QfcHomeController.cs`, `UtilitiesCS/Properties/AssemblyInfo.cs`, or any
existing test file.
