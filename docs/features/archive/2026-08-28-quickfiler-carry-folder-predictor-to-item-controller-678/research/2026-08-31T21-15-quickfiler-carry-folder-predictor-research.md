# Research: Carry the folder predictor from the confidence gate to the item controller (Issue #678)

- Timestamp: 2026-08-31T21-15
- Worktree: `<repo-root>/prep-678`
- Branch: `bug/quickfiler-carry-folder-predictor-to-item-controller-678` (base `origin/main` @ `2b85134b`)
- Work mode: `minor-audit` (no `spec.md`, no `user-story.md`)
- Requirements source: `<repo-root>/prep-678/docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/issue.md`

All line numbers below were re-derived against this worktree in this pass. Where the issue body's
citation (taken at `988e819b`) no longer matches, the correction is stated explicitly.

---

## 0. Citation reconciliation against the issue body

| Issue body citation | Status in this tree | Corrected location |
|---|---|---|
| `QfcHighConfidencePreFilter.cs:184` (predictor discarded) | Correct | `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:184` |
| `QfcItemController.FolderHandling.cs:193-199` (`AssignFolderComboBox` predetermined branch) | **Moved** | Method spans `:165-212`; the `_predeterminedFolder` branch is `:197-203`. Line 193 is now the `Suggestions != null` guard for `SetFolderSuggestions`. |
| `QfcItemController.cs:41` (`IFolderSearchHandler _folderHandler`) | Correct | `QuickFiler/Controllers/QfcItemController.cs:41` |
| `QfcItemController.cs:83-89` (predictor factories) | Correct | `_folderPredictorFactory` `:83-88`, `_folderPredictorEmptyFactory` `:89` |
| `QfcItemController.Initialization.cs:63-64` | Correct | seam capture in the primary ctor |
| `QfcItemController.Initialization.cs:108` | Correct | `_predeterminedFolder = predeterminedFolder;` |
| `QfcItemController.Initialization.cs:398-400` | Correct | `??=` production defaults for both predictor factories |
| `QfcItemGroup.cs:50` | Correct | `internal string PredeterminedFolder { get; set; }` |
| `QfcCollectionController.cs:428-471` | **Moved / mis-scoped** | The carrier overload is `LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, RowStyle, RowStyle)` at `:487-566`; `EncapsulateItemGroup` is `:646-672`. Lines 428-471 now sit inside the `IList<MailItem>` overload (`:403-478`). |
| `QfcCollectionController.cs:616` | **Moved** | The second-pass call is `LoadSecondaryAsync`'s folder task at `:604-611`, with `LoadFolderHandlerAsync(Token)` on `:607`. Line 616 is now the `while (combinedTasks.Count > 0)` loop header. |
| `QfcHomeController.cs:310` "the sole overload-selection call site" | **Moved and mischaracterised** | The call is `await _formController.LoadItemsAsync(listEmail);` at `QfcHomeController.cs:307`. It is **not** an overload-selection site: there is no branch. `RunAsync` unconditionally passes an `IList<MailItem>`, so the `IList<QfcPreScoredItem>` overload is never bound at any production call site. |
| `QfcHomeControllerIssue218Tests.cs:137-259` | Correct | Two test methods, `:137-182` and `:184-259` |
| `QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`, `:277` | Correct *lines*, **wrong characterisation** | Both are in **high-confidence-DISABLED** tests and must stay `Times.Never`. The enabled-mode test that will need rewriting is `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue` at `:111-210`, which the issue does not name. |

The issue also states "seven production files". The enumerated bullet list contains six paths
(`QfcHighConfidencePreFilter.cs`, `QfcItemGroup.cs`, `QfcCollectionController.cs`,
`QfcItemController.cs`, `QfcItemController.Initialization.cs`, `QfcHomeController.cs`);
`QfcItemController.FolderHandling.cs` is cited elsewhere in the body but omitted from the list.
Section 6 below gives the corrected list.

---

## 1. Producer-to-consumer flow

### 1.1 There are two producers, and the one the issue names is dormant

`QfcHighConfidencePreFilter.FilterAsync` (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:47-88`)
is reachable in production only through the injectable seam
`QfcHomeController.HighConfidencePreFilterLoader`
(`QuickFiler/Controllers/QfcHomeController.cs:233-241`). Grepping the `QuickFiler` production tree
for that property name returns only its declaration; no production member reads or invokes it.
The existing tests assert this deliberately — `preFilterInvoked.Should().BeFalse(...)` at
`QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs:157-159` with the reason
"remaining-queue admission now owns high-confidence filtering". **The pre-filter class is dormant.**

The **live** producer is the issue #233 dequeue-time gate:

- `QfcDatamodel.DequeueWithHighConfidenceGateWithOutcomeAsync`
  (`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:170-193`) constructs a
  `QfcStreamingDequeueConfidenceGate` (`:177-187`) whose `scoreLoader` is
  `QfcDatamodel.ScoreRemainingQueueMailItemAsync` (`:263-277`).
- `ScoreRemainingQueueMailItemAsync` resolves `IFolderScoringService` from the injectable
  `ScoringServiceFactory` (`:260-261`, default `() => new FolderScoringService()`) and calls
  `ScoreAsync` (`:269-271`).
- `FolderScoringService.ScoreAsync`
  (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:170-189`) is therefore the **single live
  scoring body**. It builds a `MailItemHelper` (`:178`), constructs a `FolderPredictor` (`:179-183`),
  awaits `predictor.InitAsync(helper, InitOptions.FromField)` (`:184`), reads
  `Suggestions.TopScore()` and `Suggestions.ToArray(1).FirstOrDefault()` (`:186-187`), and returns a
  `(long Score, string TopFolder)` tuple (`:188`). The initialised predictor goes out of scope.
- `QfcStreamingDequeueConfidenceGate.DequeueAsync` wraps each accepted item as
  `new QfcPreScoredItem(mailItem, topFolder)`
  (`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:195`).

The `Probability debug` line the issue's repro describes as coming from the pre-UI scan is emitted at
`QfcDatamodel.QueueProcessing.cs:272-275`
(`Probability debug [QfcDatamodel.ScoreRemainingQueueMailItemAsync (master-queue admission)]`) and
`QfcStreamingDequeueConfidenceGate.cs:237-239`, not at `QfcHighConfidencePreFilter.cs:71-75`.

### 1.2 The exact `QfcPreScoredItem` member set

`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:98-122`. `public readonly struct`, exactly two
members plus one constructor:

- `public MailItem MailItem { get; }` (`:115`)
- `public string PredeterminedFolder { get; }` (`:121`), coerced non-null at `:111`
- `public QfcPreScoredItem(MailItem mailItem, string predeterminedFolder)` (`:108-112`)

There is no folder-handler, scorer, score or helper member. This is the structural cause of the
defect.

### 1.3 The carriers reach the datamodel boundary and stop there

`QfcGateBatch` (`QfcStreamingDequeueConfidenceGate.cs:17-40`) exposes `Accepted`, `Stop`, `Scanned`.
`QfcDequeueBatch` (`QuickFiler/Interfaces/IQfcDatamodel.cs:49-81`) exposes `Items`, `PreScored`,
`Stop`. `QfcDatamodel.QueueProcessing.cs:190-192` builds it from the same accepted set.

The only production consumer of `DequeueNextItemGroupWithOutcomeAsync` is
`QfcHomeController.IterateQueueAsync` (`QuickFiler/Controllers/QfcHomeController.Iteration.cs:22-27`).
It reads `batch.Items` (`:28`) and `batch.Stop` (`:36`). **It never reads `batch.PreScored`.**
Grepping `\.PreScored` across the `QuickFiler` production tree returns only the declaration
(`IQfcDatamodel.cs:77`) and two doc-comment references. The carriers are produced and discarded.

### 1.4 The two reachable display paths, both of which re-score

**Leg A — first page (`RunAsync`).**
`QfcHomeController.RunAsync` (`QfcHomeController.cs:271-321`) calls the four-argument
`DequeueNextItemGroupAsync` (`:296-301`), which returns `IList<MailItem>` only
(`QfcDatamodel.QueueProcessing.cs:148-162` discards `batch.PreScored` by returning `batch.Items`).
It then calls `_formController.LoadItemsAsync(listEmail)` at `QfcHomeController.cs:307` — the
`IList<MailItem>` overload (`QfcFormController.Actions.cs:62-65` → `:67-105`), which calls
`LoadControlsAndHandlers_01Async(IList<MailItem>, ...)` (`QfcCollectionController.cs:403-478`) and
then `LoadSecondaryAsync` (`QfcFormController.Actions.cs:104`).
`QfcCollectionController.LoadSecondaryAsync` (`:584-638`) fans out
`grp.ItemController.LoadFolderHandlerAsync(Token)` at `:607` with `varList` defaulted to `null` →
the `FromField` branch → **second `InitAsync(FromField)`**.

**Leg B — every subsequent page (`IterateQueueAsync` → `QfcQueue`).**
`QfcHomeController.Iteration.cs:32-34` calls `QfcQueue.EnqueueAsync(listObjects, ...)`
(`QuickFiler/Controllers/QfcQueue.cs:211-276`), which calls `LoadControllersViewersAsync`
(`:380-421`). That method constructs `new QfcItemController(...)` with the eight-argument primary
constructor (`:405-414`) — no predetermined folder — and awaits `InitializeAsync()` (`:415`).
`QfcItemController.InitializeAsync` calls `PopulateFolderComboBoxAsync(default, null)` at
`QfcItemController.Initialization.cs:250` → `LoadFolderHandlerAsync(token, null)`
(`QfcItemController.FolderHandling.cs:161`) → **second `InitAsync(FromField)`**.

Leg B is the larger share of items in a session and is not mentioned in the issue.

### 1.5 The carrier path is dormant

`QfcFormController.LoadItemsAsync(IList<QfcPreScoredItem>)`
(`QfcFormController.Actions.cs:114-117`, `:120-164`) and
`QfcCollectionController.LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, ...)`
(`QfcCollectionController.cs:487-566`) exist and are declared on their interfaces
(`QuickFiler/Controllers/IQfcFormController.cs:32-33`;
`QuickFiler/Interfaces/IQfcCollectionController.cs:32-36`). Neither has a production call site.
`EncapsulateItemGroup` (`QfcCollectionController.cs:646-672`) accepts an optional
`predeterminedFolder` (`:652`), sets `QfcItemGroup.PredeterminedFolder` (`:655`) and passes it to the
nine-argument `QfcItemController` constructor (`:659-669`), which stores it at
`QfcItemController.Initialization.cs:108`.

**Answer to Q1:** the carrier path is not reachable at runtime under any setting. In high-confidence
mode (`QfSettings.HighConfidenceModeEnabled == true`,
`QfcDatamodel.QueueProcessing.cs:88` and `:119`) the carriers are built and then dropped, once in
`DequeueWithHighConfidenceGateAsync` (`:161`, returns `batch.Items`) and once in
`IterateQueueAsync` (`QfcHomeController.Iteration.cs:28`). In normal mode `PreScored` is
constructed empty (`QfcDatamodel.QueueProcessing.cs:132`).

---

## 2. Is the predictor instance safely reusable across the hop? — **Yes**

`FolderPredictor` is `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` (1000 lines), with its
`IFolderSearchHandler` implementation declared on a second partial part at
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.IFolderSearchHandler.cs:10`.

### 2.1 What `InitAsync(helper, InitOptions.FromField)` establishes

`InitAsync` (`FolderPredictor.cs:50-69`) switches on the option; `FromField` calls
`InitializeFromEmail(objItem)` (`:59-61`). For a `MailItemHelper` argument that reaches
`FromFolderKey(MailItemHelper)` (`:87-91` → `:141-147`), which does exactly one of:

- `Suggestions.LoadFromField(mailInfo, _globals)` (`FolderScorer.cs:72-84`) — clears
  `_folderNameScores`, adds conversation-based suggestions, adds `FolderKey` user-property entries;
  or
- `await Suggestions.RefreshSuggestions(mailInfo, _globals)` (`FolderScorer.cs:130-151`) — clears
  `_folderNameScores`, runs `AddBayesianSuggestionsAsync` (`:153-180`) and
  `AddConversationBasedSuggestions` (`:304-326`).

The **only** state established is `FolderScorer._folderNameScores`, a
`ScoDictionaryNew<string, long>` of folder path → score (`FolderScorer.cs:28`). Nothing else on the
predictor is written by the `FromField` path.

Note: an item that takes the `LoadFromField` branch gets zero-valued scores
(`FolderScorer.cs:105`, `:220`), so `TopScore()` returns 0 and the gate rejects it
(`QfcStreamingDequeueConfidenceGate.cs:193`). Every **accepted** item therefore took the Bayesian
`RefreshSuggestions` branch. The two passes agree on branch selection.

### 2.2 What the instance holds

Complete private-field inventory (`FolderPredictor.cs:151-215`, `:263`, `:270`):

| Field | Type | Set by | Retains per-item state? |
|---|---|---|---|
| `_globals` | `IApplicationGlobals` | ctor (`:37`, `:44`) | No — application-scoped |
| `_olApp` | `Outlook.Application` | ctor (`:38`, `:45`) | **COM reference**, application-scoped |
| `_regex` | `Regex?` | `GetMatchingFolders` (`:891`) only | No — null until a user search |
| `_folderList` | `List<string>?` | `FolderArray` getter (`:220-227`), `FindFolder` (`:311`), `FromArrayOrString` (`:117`) | Lazily cached; **null after a `FromField` init** because the pre-filter never reads `FolderArray` |
| `_suggestions` / `Suggestions` | `FolderScorer` | ctor (`:39`, `:47`) | Yes — the scored dictionary. Pure in-memory. |
| `_blUpdateSuggestions` | `bool` | `RefreshSuggestions` (`:994`) | No |

Findings, each verified by reading the type:

- **No `MailItem`, `MAPIFolder`, `Store` or `Folder` field.** The only COM handle is the
  application-scoped `Outlook.Application` obtained from `appGlobals.Ol.App`, which every predictor
  instance in the process already shares.
- **No `MailItemHelper` field.** `FromFolderKey(MailItemHelper)` (`:141-147`) passes the helper to
  `FolderScorer`, which reads `mailInfo.Item` and `mailInfo.Tokens` and stores neither
  (`FolderScorer.cs:72-84`, `:130-151`, `:153-180`). The helper is not captured.
- **No `CancellationToken` field.** `InitAsync` takes no token; tokens appear only as parameters of
  `InputFoldernameAsync` (`:588`) and `CreateFolderAsync` (`:740`).
- **`FolderPredictor` implements no `IDisposable`** and holds no disposable member. `FolderScorer`
  (`FolderScorer.cs:18`) likewise.

### 2.3 Thread affinity

There is no apartment attribute or thread capture on `FolderPredictor` or `FolderScorer`. Stronger
evidence: the existing production code **already** moves the instance across threads.
`LoadFolderHandlerAsync` constructs and initialises the predictor inside `Task.Run(...)` on a
thread-pool thread (`QfcItemController.FolderHandling.cs:64-80`), assigns it to `_folderHandler`, and
`AssignFolderComboBox` then reads `FolderArray` / `FolderRowArray` after marshalling to the UI thread
(`:162`, `:174`, `:186`, `:195`). Construction reads `appGlobals.Ol.App` on that pool thread today
(`FolderPredictor.cs:45`). Carrying an instance from the gate thread to the UI thread introduces no
new marshalling that the current code does not already perform.

### 2.4 Post-`InitAsync` mutation in the item-controller path

Members of `_folderHandler` reached from `QfcItemController`:

- `FolderArray` (`FolderHandling.cs:174`, `:186`, `:207`) — the getter mutates `_folderList` on first
  read (`FolderPredictor.cs:220-227`). Benign: it is the intended lazy build, and because the gate
  never reads `FolderArray`, the carried instance arrives with `_folderList == null`, so recents are
  read at display time from `_globals.AF.RecentsList` (`:225`), not at scan time.
- `Suggestions` (`FolderHandling.cs:193`, `:39`, `:52`, `:84`, `:128`; `QfcItemController.cs:254`) —
  read-only use (`TopScore()`, `ToScoredArray`).
- `FolderRowArray` (`FolderHandling.cs:195`) — does **not** mutate `_folderList`
  (`FolderPredictor.cs:243-258`, documented at `:240-241`).
- `FindFolder` (`QfcItemController.EventHandlers.cs:175-180`) — **does** mutate: resets `_folderList`
  (`FolderPredictor.cs:311`) and sets `_regex` (`:891`). This already happens today on the freshly
  built predictor and is user-initiated search behaviour; it is unchanged by carrying.

No production member calls `InitAsync` twice on one instance. `LoadFolderHandlerAsync` calls it once
per newly constructed instance (`FolderHandling.cs:73-76`, `:117-120`); `LoadFolderHandler` (sync)
never calls it at all (see §3.1).

### 2.5 Are the two `MailItemHelper` instances the same object?

**No — they are distinct instances built by the same factory with the same arguments.**

- Gate side: `FolderScoringService.ScoreAsync` calls
  `MailItemHelper.FromMailItemAsync(mailItem, globals, token, false)`
  (`QfcHighConfidencePreFilter.cs:178`).
- Item-controller side (leg A): `QfcCollectionController.GetPartiallyInitializedHelperAsync`
  (`QfcCollectionController.cs:360-380`) calls the same factory with `loadAll: false` (`:362-367`)
  and then forces seven lazy properties (`:368-377`); the result is assigned to `ItemHelper` by
  `PopulateControls(MailItemHelper, int)` (`QfcItemController.ViewerSetup.cs:371-375`).
- Item-controller side (leg B): `PopulateControlsAsync` calls
  `MailItemHelper.FromMailItemAsync(mailItem, _globals, Token, loadAll)`
  (`QfcItemController.ViewerSetup.cs:387`).

What depends on the difference: only `MailItemHelper.Tokens` and `.Item`, which `FolderScorer`
consumes (`FolderScorer.cs:164`, `:171`, `:75`, `:76`). Both helpers wrap the same `MailItem` and are
built with `loadAll: false`, so the tokenization inputs are the same and the derived score set is the
same **given the same classifier state**. The forced property reads at
`QfcCollectionController.cs:368-377` do not affect tokenization; they warm display fields.

### 2.6 Lifetime hazards

1. **Retention window.** A carried handler is held from gate acceptance until the item controller is
   cleaned up. `QfcItemController.Cleanup` nulls `_folderHandler`
   (`QfcItemController.ViewerSetup.cs:465`, `:468`); a new carried field must be nulled there too or
   the `FolderScorer` dictionary and the `QfcItemGroup` reference outlive the row. Bounded by
   `ItemsPerIteration`, so the magnitude is small, but it must be handled explicitly.
2. **Rejected candidates.** Only accepted items carry a handler
   (`QfcStreamingDequeueConfidenceGate.cs:193-215`), so rejects retain nothing extra.
3. **Staleness — the one real behavioural delta.** `AddConversationBasedSuggestions` reads
   `_globals.AF.CtfMap` (`FolderScorer.cs:310`), which the session mutates as the user files items.
   Today the second pass re-reads it at display time; reusing the carried result freezes the whole
   suggestion set at scan time. `_globals.AF.RecentsList` is unaffected because `FolderArray` is
   still built lazily at display time (§2.4). This delta must be stated in the change description; it
   does not alter the preselected entry when `_predeterminedFolder` is honoured (§7).
4. **Deferred display.** Leg B items sit in `QfcQueue` between scan and display
   (`QfcQueue.cs:254`, `:161` via `Dequeue`). The staleness window is therefore longer for leg B than
   for leg A. Same mechanism, larger interval.

**Answer to Q2: the instance is safely reusable.** It holds no per-item COM handle, no helper, no
token and nothing disposable; its only per-item state is an in-memory score dictionary; it is not
thread-affine and already crosses threads in the current code; and nothing in the consuming path
mutates it in a way that a second consumer would observe. The only substantive concern is
scan-time-versus-display-time staleness of `CtfMap`-derived conversation suggestions.

---

## 3. `LoadFolderHandler`, `LoadFolderHandlerAsync`, and the `FromArrayOrString` branch

### 3.1 `LoadFolderHandler` (sync) — `QfcItemController.FolderHandling.cs:27-55`

Two paths, both of which only invoke `_folderPredictorFactory` and **never call `InitAsync`**:

- `varList is null` → factory with `(ItemHelper, InitOptions.FromField)` (`:31-35`), debug log
  `:36-40`.
- `varList is not null` → factory with `(varList, InitOptions.FromArrayOrString)` (`:44-48`), debug
  log `:49-53`.

The production default factory is
`(globals, objItem, options) => new FolderPredictor(globals, objItem, options)`
(`QfcItemController.Initialization.cs:398-399`). That three-argument constructor
(`FolderPredictor.cs:42-48`) **ignores both `objItem` and `options`**: it assigns `_globals`,
`_olApp` and a fresh empty `FolderScorer`, and returns.

**Consequence (pre-existing, out of scope):** the synchronous `LoadFolderHandler` produces an
uninitialised handler on both of its paths. `FolderArray` then contains only the
`"========= SUGGESTIONS ========="` separator plus recents (`FolderPredictor.cs:220-230`, `:804-808`,
`:785-792`), and on the `FromArrayOrString` path the base member's replicated combo strings are
silently dropped. This affects `LoadSequential_5` (`QfcCollectionController.cs:712`),
`EnumerateConversationMembers` (`:1872`) and `AddItemGroup` (`:1920`). It is a separate latent
defect; see §7.

### 3.2 `LoadFolderHandlerAsync` — `QfcItemController.FolderHandling.cs:57-131`

Three distinct code paths:

| Path | Guard | Body | Options value |
|---|---|---|---|
| P1 | `varList is null`, factory succeeds | `Task.Run` → factory `(ItemHelper.ThrowIfNull(), FromField)` (`:67-71`) then `fp.InitAsync(ItemHelper, FromField)` (`:73-76`); log `:81-85` | `FromField` |
| P2 | `varList is null`, factory or init throws `ArgumentNullException` | `catch` at `:87`, falls back to `_folderPredictorEmptyFactory(_globals)` (`:93`); an exception from the fallback is logged and rethrown (`:96-99`) | none (empty predictor, `FolderPredictor.cs:35-40`) |
| P3 | `varList is not null` | `Task.Run` → factory `(varList, FromArrayOrString)` (`:112-116`) then `fp.InitAsync(varList, FromArrayOrString)` (`:117-120`); log `:125-129` | `FromArrayOrString` |

A fourth path exists: any other `System.Exception` is logged and rethrown (`:101-105`).

Only two of the four `InitOptions` values (`FolderPredictor.cs:71-77`) appear anywhere in
`QuickFiler`: `FromField` and `FromArrayOrString`. `NoSuggestions` and `Recalculate` are unused by
this controller.

**Which caller reaches which path:**

- P1 is reached by `QfcCollectionController.LoadSecondaryAsync` (`:607`, `varList` defaulted) and by
  `PopulateFolderComboBoxAsync(default, null)` (`QfcItemController.Initialization.cs:250`,
  reached from `InitializeAsync`, which `QfcQueue.LoadControllersViewersAsync` awaits at
  `QfcQueue.cs:415`). **Both reachable production legs land on P1.**
- P3 has **no production caller**. `PopulateFolderComboBoxAsync` is invoked from exactly one
  production site (`QfcItemController.Initialization.cs:250`) and it passes `null`. The non-null
  `varList` case reaches only the synchronous `LoadFolderHandler` (§3.1). P3 is exercised solely by
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:264-295` and `:377-413`.

### 3.3 What a carried handler must satisfy per path

- **P1** — the consumer needs `FolderArray`, `Suggestions` and `FolderRowArray`
  (`FolderHandling.cs:174`, `:186`, `:193`, `:195`, `:207`) plus `FindFolder` later
  (`EventHandlers.cs:175`). A carried `FromField` predictor satisfies all four: it is the same type
  produced by the same factory shape with the same option, differing only in *when* it was
  initialised. **Substitutable.**
- **P2** — a carried handler makes the fallback unreachable for carried items, because the
  `ArgumentNullException` source is `ItemHelper.ThrowIfNull()` (`:69`). This is a coverage
  consideration, not a correctness one: the fallback must remain reachable for non-carried items and
  its existing test (`FolderHandlingTests.cs:298-324`) must continue to pass.
- **P3** — a carried `FromField` predictor is **not** substitutable and must be excluded. The
  `varList` on that path is `_itemViewer.GetFolderItems()`
  (`QfcItemController.MailActions.cs:61`), i.e. the *base* conversation member's already-populated
  combo strings, replicated verbatim onto expanded members via
  `ToggleUnGroupConv` (`QfcCollectionController.cs:1674-1679`, `:1729-1735`) →
  `EnumerateConversationMembers` (`:1853-1895`) → `PopulateFolderComboBox(folderList)` (`:1872`).
  Substituting the member's own `FromField` result would replace the replicated list with a
  different one and change what the user sees. Any adoption must be gated inside the
  `varList is null` branch.

### 3.4 `_folderPredictorFactory` — declaration, default, injection sites

- Declaration: `QfcItemController.cs:83-88`,
  `Func<IApplicationGlobals, object, FolderPredictor.InitOptions, FolderPredictor>`. The rationale
  comment at `:79-82` states the concrete return type is required because `LoadFolderHandlerAsync`
  calls `InitAsync`, which is not on `IFolderSearchHandler`.
- Companion: `_folderPredictorEmptyFactory` (`QfcItemController.cs:89`),
  `Func<IApplicationGlobals, FolderPredictor>`.
- Constructor injection: the primary constructor's optional parameters
  (`QfcItemController.Initialization.cs:45-51`), captured at `:63-64`.
- Production defaults: `SaveParameters` `??=` at `QfcItemController.Initialization.cs:398-400`.
  Because `SaveParameters` is the single funnel for **every** constructor and both static factories
  (`:65-74`, `:98-107`, `:123-132`, `:150-159`, `:433-442`, `:475-484`), no path leaves the factories
  null — including the nine-argument predetermined-folder constructor, which does not set them
  explicitly.
- Test injection: reflection field-set, e.g.
  `QfcItemController.FolderHandlingTests.cs:177`, `:214`, `:253`, `:286`, `:314-315`, `:345`,
  `:369`, `:403`.

There are **no** injection sites in the production `QuickFiler` tree other than the `??=` defaults:
`QfcCollectionController.EncapsulateItemGroup` (`:659-669`) and
`QfcQueue.LoadControllersViewersAsync` (`:405-414`) both use constructor overloads that do not accept
a factory.

### 3.5 The `IFolderSearchHandler` seam and its downstream readers

Declared at `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs:14-39` with four members:
`FolderArray` (`:17`), `Suggestions` (`:20`), `FolderRowArray` (`:27`), `FindFolder` (`:30-38`).
The field is `QfcItemController.cs:41`.

Complete list of reads, from a grep of `_folderHandler` across `QuickFiler`:

| Member | Read at | Purpose |
|---|---|---|
| `FolderArray` | `FolderHandling.cs:174` (guard), `:186` (`AddFolderItems`), `:207` (index fallback) | combo population and index-1 selection |
| `Suggestions` | `FolderHandling.cs:193` (null guard), `:39`, `:52`, `:84`, `:128` (debug logs), `QfcItemController.cs:254` (`TopFolderScore`) | suggestion presence, logging, score property |
| `FolderRowArray` | `FolderHandling.cs:195` (`SetFolderSuggestions`) | #325 row model with probabilities |
| `FindFolder` | `QfcItemController.EventHandlers.cs:175-180` | live folder search on keystroke |

`_predeterminedFolder` is read only at `FolderHandling.cs:198`, `:199`, `:202` — i.e. only for combo
**selection**, exactly as the issue states.

### 3.6 A latent mismatch on the (dormant) carrier path

`FolderScoringService.ScoreAsync` returns the **raw** suggestion path
(`Suggestions.ToArray(1).FirstOrDefault()`, `QfcHighConfidencePreFilter.cs:187`), while
`FolderPredictor.FolderArray` stores the **archive-prefix-stripped** projection
(`ProjectSuggestionPath`, `FolderPredictor.cs:807`, `:845-858`). `AssignFolderComboBox` compares the
raw carried string against the projected combo contents via
`_itemViewer.FolderContains(_predeterminedFolder)` (`FolderHandling.cs:199`). For any suggestion
under the archive root the comparison fails and the code silently falls back to index 1
(`:206-208`). This is currently unobservable because the carrier path is dormant; it becomes
observable the moment the carrier path is activated, and it directly threatens the "preselected
folder must not change" constraint. Any activation work must normalise one side.

---

## 4. Which existing tests pin the current behaviour

### 4.1 `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` (499 lines)

| Test | Lines | Pins | Disposition |
|---|---|---|---|
| `PopulateAndSelectFolder_ExactMatchAtIndexZero_SelectsIndexZero` | 27-44 | pure WinForms seam | Unaffected |
| `PopulateAndSelectFolder_AllMissingPredetermined_SelectsIndexOne` | 46-62 | pure WinForms seam | Unaffected |
| `PopulateAndSelectFolder_EmptyArray_ThrowsOnIndexOneSelection` | 64-81 | pure WinForms seam | Unaffected |
| `PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_SelectsIndexZeroWithoutThrowing` | 83-97 | pure WinForms seam | Unaffected |
| `LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore` | 132-148 | **source-text** assertions on four exact debug-log literals | **At risk.** Reads `QfcItemController.FolderHandling.cs` from disk (`:120-130`) and asserts the literal strings at `:139`, `:143`, `:145`, `:146`, `:147`. Any change to those log lines breaks it. If a fifth "carried handler adopted" log line is added the test still passes; if an existing literal is reworded it must be updated. |
| `LoadFolderHandler_WhenVarListNull_InvokesFactoryWithItemHelperAndFromFieldOptions` | 152-188 | `_folderPredictorFactory` invoked once with `(globals, ItemHelper, FromField)` | Unaffected if adoption is confined to `LoadFolderHandlerAsync`. **Must be rewritten** if `LoadFolderHandler` (sync) also adopts. |
| `LoadFolderHandler_WhenVarListProvided_InvokesFactoryWithArrayOrStringOptions` | 190-225 | factory args on the `FromArrayOrString` path | Unaffected (P3 must stay unchanged) |
| `LoadFolderHandlerAsync_WhenVarListNull_InvokesFactoryWithExpectedArgs` | 229-261 | **the factory IS invoked** on P1, with `(globals, ItemHelper, FromField)`; asserts a sentinel throw | **This is the closest thing to a test that pins the double-initialisation.** It asserts the factory is reached with no carried handler present, which remains true for non-carried items. It should be **kept as-is** and a new sibling added for the carried case. No rewrite required, provided adoption is a guarded early return that leaves the un-carried path byte-identical. |
| `LoadFolderHandlerAsync_WhenVarListProvided_InvokesFactoryWithArrayOrStringArgs` | 263-295 | P3 factory args | Unaffected; becomes the guard-regression test for §3.3 P3 |
| `LoadFolderHandlerAsync_WhenPrimaryFactoryThrowsArgumentNull_InvokesEmptyFactoryFallback` | 297-324 | P2 fallback | Unaffected (no carried handler in arrange) |
| `PopulateFolderComboBox_WhenFactorySucceeds_LoadsHandlerAndAssignsComboFromViewer` | 328-350 | sync path | Unaffected |
| `PopulateFolderComboBox_WhenInvokeRequired_MarshalsAssignFolderComboBoxViaInvoke` | 352-374 | sync marshalling | Unaffected |
| `PopulateFolderComboBoxAsync_WhenFactorySucceeds_DispatchesAssignFolderComboBoxThroughViewerDispatcher` | 376-413 | P3 through a real WPF dispatcher | Unaffected |
| `AssignFolderComboBox_WhenNoPredeterminedFolder_SelectsTopSuggestionViaViewer` | 415-437 | index-1 selection | Unaffected |
| `AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder` | 439-462 | preselection by name | Unaffected — **this is the guard for the §7 invariant** |
| `AssignFolderComboBox_WhenFolderHandlerNull_DoesNotTouchViewer` | 464-478 | null guard | Unaffected |
| `AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero` | 480-496 | single-item bounds | Unaffected |

**No test in this file asserts that `InitAsync` runs twice.** The double initialisation is not
directly pinned anywhere; it is an emergent property of the call graph.

### 4.2 `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` (393 lines)

This file contains **zero `[TestMethod]` members**. It is the shared `PumpHarness` fixture for the
`#230` pump-hosted initialization tests (`partial class`, no second `[TestClass]`, per the header at
`:24-28`).

Relevant content:
- `:94-98` comment: seams are injected first and then `SaveParameters` supplies the folder-predictor
  and conversation-resolver factory defaults, "Injecting fields one by one instead would leave those
  factories null and fail inside `LoadFolderHandlerAsync` rather than at the seam under test."
- `BuildInitGlobals` (`:138-184`) exists specifically because `InitializeAsync` drives
  `PopulateFolderComboBoxAsync` → `FolderPredictor` → `FolderScorer`: it stubs `AF.CtfMap` (`:155`),
  `AF.LngConvCtPwr` (`:156`), `AF.UseLcppnPredictor` + `AF.FolderPredictor` (`:163-173`) and
  `AF.RecentsList` (`:177`).

**Disposition: unaffected and must remain working.** The fixture builds controllers with no carried
handler, so the existing `FromField` path still runs. It is nevertheless a regression tripwire: if
adoption changed `SaveParameters` or the factory defaults, every pump-hosted initialization test
would fail here rather than at its own assertion.

### 4.3 `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs`

One test touches the folder seam:
`TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PresentsSearchResultsWithoutFocusOrCommit`
at `:331-389`. It injects a `Mock<IFolderSearchHandler>` directly into `_folderHandler` (`:370-374`)
and asserts `FindFolder` receives `"*query*"` and that its exact result is handed to
`PresentFolderSearchResults` (`:382-383`), with negative assertions at `:386-388`.
**Unaffected** — it bypasses `LoadFolderHandlerAsync` entirely. It is also the proof that
`IFolderSearchHandler` is directly mockable, which matters for §5.

The remaining tests in this file (`:45-304`, `:393-478`) are theme, checkbox, delete, flag,
key-down and topic-thread tests with no folder-handler involvement.

### 4.4 `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs`

| Test | Lines | Assertions of interest | Disposition |
|---|---|---|---|
| `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch` | 137-182 | `preFilterInvoked == false` (`:157-159`); `LoadItemsAsync(IList<MailItem>)` `Times.Once` (`:160-164`); `DequeueNextItemGroupAsync(4-arg)` `Times.Once` (`:165-176`); `LoadItemsAsync(IList<QfcPreScoredItem>)` **`Times.Never`** (`:177-181`) | **Rewrite required** if `RunAsync` activates the carrier overload in enabled mode. The `preFilterInvoked == false` assertion at `:157-159` **must be preserved** — it encodes #233. |
| `RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter` | 184-259 | `sequence.Should().Equal("LoadItemsAsync")` (`:244`) — only tracks the `IList<MailItem>` overload (`:220-223`); `DequeueNextItemGroupAsync(4-arg)` `Times.Once` (`:245-254`); `LoadItemsAsync(IList<QfcPreScoredItem>)` **`Times.Never`** (`:255-258`) | **Rewrite required**, same reason. The ordering intent (pre-filter never runs) must be preserved. |

### 4.5 `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`

| Test | Lines | Mode | Disposition |
|---|---|---|---|
| `HighConfidencePreFilterLoader_CanBeOverridden_ForTesting` | 87-109 | n/a | Unaffected — pins the seam's overridability only |
| `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue` | 111-210 | **enabled** | **Rewrite required.** Pins `DequeueNextItemGroupAsync(itemsPerIteration, 200, DefaultFirstBatchDeadline, non-null sink)` `Times.Once` (`:180-191`) and `LoadItemsAsync(IList<MailItem>)` carrying the streamed candidate `Times.Once` (`:192-201`). Activating the carrier path changes both the dequeue member and the load overload. **The issue does not name this test.** |
| `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload` | 216-250 | disabled | **Unaffected — `Times.Never` at `:245-249` must stay** |
| `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly` | 256-~285 | disabled | **Unaffected — `Times.Never` at `:276-279` must stay** |
| `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand` | 288-~390 | enabled | **At risk** — arranges `DequeueNextItemGroupAsync(4-arg)` (`:347` sets up `LoadItemsAsync(IList<MailItem>)`); needs the same dequeue-member/overload update |
| `RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration` | 395-~470 | enabled | **At risk** — asserts `LoadItemsAsync(It.Is<IList<MailItem>>(items => items.Count == 0))` at `:462` |

**Correction to the issue body:** it cites `:246` and `:277` as the sites needing deliberate rewrite.
Both are in **disabled-mode** tests and must be left alone. The enabled-mode rewrites are at
`QfcHomeControllerIssue218Tests.cs:177-181`, `:255-258` and
`QfcHomeControllerRunAsyncHighConfidenceTests.cs:180-201` (plus the two "at risk" tests above).

### 4.6 Other tests in the blast radius (not named in the issue)

- `QfcFormControllerSeamTests.cs:330-352` — source-text test asserting the exact signature literal
  `"public async Task LoadItemsAsync(IList<QfcPreScoredItem> preScored)"` at `:339` and its ordering
  relative to the `IList<MailItem>` overload. **Breaks on any signature change to the carrier
  overload.**
- `QfcFormControllerTests.cs:799-823` (`LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval`) —
  constructs `new QfcPreScoredItem(mail, @"\\A\folder")` at `:814`. **Breaks on any constructor
  signature change to `QfcPreScoredItem`.**
- `QfcCollectionControllerTests.cs:302-326` (`CarrierLoad_SetsPredeterminedFolderOnItemGroup`) —
  constructs `new QfcPreScoredItem(mail, ...)` at `:307`. Same exposure.
- `QfcQueueCoverageExpansionTests.cs:194-213` — `Dequeue_WithHighConfidenceCarrier_PreservesPredeterminedFolder`,
  sets `group.PredeterminedFolder` (`:199`) and asserts it survives a queue round-trip (`:212`).
  Relevant if `QfcItemGroup` gains a carried-handler member.
- `QfcStreamingDequeueConfidenceGateTests.Part3.cs:256-262` — reads
  `batch.Accepted[..].PredeterminedFolder`.
- `Mock<IFolderScoringService>` construction sites (three files):
  `QfcDatamodelTests.cs:337` (setup at `:340`), `QfcHighConfidencePreFilterTests.cs:72` (setup at
  `:74`) and `:348`, `QfcQueuePurePathsTests.cs:160` (setup `:163`) and `:221` (setup `:224`). All
  are `MockBehavior.Strict`, so **widening `IFolderScoringService.ScoreAsync` requires editing all
  three files.**
- `Func<MailItem, CancellationToken, Task<(long Score, string TopFolder)>>` shape sites:
  `QfcStreamingDequeueConfidenceGateTests.cs:28` and `:73`,
  `QfcStreamingDequeueConfidenceGateTests.Part2.cs` (one occurrence).
- `QfcItemController.InitializationTests.cs:91-123`
  (`PredeterminedFolderConstructor_StoresPredeterminedFolder`) — pins the nine-argument constructor's
  field storage via reflection (`:116-119`). **Must be extended, not rewritten**, if that constructor
  gains a tenth parameter.
- `QfcItemController.FolderSuggestionsTests.cs:110-134`, `:136-166` — use a hand-written
  `FakeFolderHandler` implementing `IFolderSearchHandler`; `:152` sets `_predeterminedFolder`. These
  are the cleanest existing precedent for the new tests in §5.

---

## 5. Testability of a carried-predictor path

### 5.1 No new seam is required

Every assertion the change needs can be made through seams that already exist:

| Assertion | Existing seam | Evidence |
|---|---|---|
| A carried handler is adopted and the factory is **not** invoked | `_folderPredictorFactory` field-injection + `Mock<IFolderSearchHandler>` | injection precedent `FolderHandlingTests.cs:177`, `:253`; sentinel-throwing factory precedent `:241-252`; handler mock precedent `EventHandlersTests.cs:337-374` |
| No carried handler → current behaviour byte-identical | same | `FolderHandlingTests.cs:229-261` already asserts exactly this |
| A carried handler is **ignored** on the `FromArrayOrString` path | same | `FolderHandlingTests.cs:263-295` is the existing shape |
| The gate publishes the handler it initialised | `IFolderScoringService` (internal, mockable via `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` at `QfcHighConfidencePreFilter.cs:11`) | `Mock<IFolderScoringService>(MockBehavior.Strict)` precedent `QfcHighConfidencePreFilterTests.cs:72` |
| The datamodel propagates the handler onto `QfcPreScoredItem` | `QfcDatamodel.ScoringServiceFactory` (`QfcDatamodel.QueueProcessing.cs:260-261`) | precedent `QfcDatamodelTests.cs:349`, `QfcQueuePurePathsTests.cs:178`, `:242` |
| The gate propagates the handler into `QfcGateBatch.Accepted` | `QfcStreamingDequeueConfidenceGate`'s `scoreLoader` delegate ctor parameter (`QfcStreamingDequeueConfidenceGate.cs:73`, `:105`) | precedent `QfcStreamingDequeueConfidenceGateTests.cs:28`, `:73` |
| `RunAsync` selects the carrier overload in enabled mode and the plain overload in disabled mode | `Mock<IQfcDatamodel>` + `Mock<IQfcFormController>` + `SetPrivateField(_controller, "_formController", ...)` | precedent `QfcHomeControllerRunAsyncHighConfidenceTests.cs:124-165` |
| The carried folder is preselected and the index-1 fallback is not taken | `Mock<IItemViewer>` + reflection set of `_predeterminedFolder` | precedent `FolderHandlingTests.cs:439-462`, `FolderSuggestionsTests.cs:136-166` |

All of these are MSTest + Moq + FluentAssertions, no live Outlook COM, no temporary files, so they
satisfy `.claude/rules/general-unit-test.md` UT4 and the C# unit-test policy.

### 5.2 Where a new seam form would be needed, and which one

The only assertion with no existing seam is **leg B**: proving that
`QfcQueue.EnqueueAsync` carries the handler through to the item controllers it constructs.
`QfcQueue.LoadControllersViewersAsync` (`QfcQueue.cs:380-421`) calls
`new QfcItemController(...)` directly and then `InitializeAsync()`, which requires a real
`ItemViewer` and a WinForms message pump. If leg B is in scope, the smallest sufficient seam is
form **2, the injectable delegate seam** from `.claude/rules/csharp.md:52`: a
`Func<..., IQfcItemController>` controller-factory field on `QfcQueue` defaulting to the current
`new QfcItemController(...)` expression, mirroring the `_folderPredictorFactory` /
`_conversationResolverFactory` / `ScoringServiceFactory` pattern already used throughout this
assembly. An interface seam (form 1) is excessive for one construction expression, and there is no
static or third-party API to wrap, so form 3 does not apply.

If leg B is deferred, **no new seam is required at all.**

### 5.3 Proposed test strategy (no test code written here)

1. **RED regression, item controller.** Inject a `Mock<IFolderSearchHandler>` as the carried handler
   and a `_folderPredictorFactory` that throws a sentinel; call
   `LoadFolderHandlerAsync(CancellationToken.None)`; assert no throw and that `_folderHandler` is
   `BeSameAs` the mock. Fails before the change (sentinel escapes), passes after.
2. **Negative guard.** Same arrangement plus a non-null `varList`; assert the sentinel **does**
   escape, proving the `FromArrayOrString` path never adopts.
3. **Null-carrier regression.** Keep `FolderHandlingTests.cs:229-261` unchanged as the proof that the
   un-carried path is untouched.
4. **Producer.** `Mock<IFolderScoringService>` returning a known handler; assert the handler reaches
   `QfcPreScoredItem` through `QfcStreamingDequeueConfidenceGate.DequeueAsync` and
   `QfcDequeueBatch.PreScored`.
5. **Overload selection.** Extend the `QfcHomeController` enabled-mode tests to assert the carrier
   overload is used in enabled mode; keep the two disabled-mode `Times.Never` assertions
   (`QfcHomeControllerRunAsyncHighConfidenceTests.cs:245-249`, `:276-279`) and both
   `preFilterInvoked == false` assertions verbatim.
6. **Selection invariant.** Assert `SetFolderSelectedItem(carriedFolder)` and
   `SetFolderSelectedIndex` never, on a carried item whose folder is present, and the index-1
   fallback when it is absent — extending the existing `AssignFolderComboBox` tests rather than
   replacing them.
7. **Path-projection regression** (§3.6): assert that the carried `PredeterminedFolder` and the
   `FolderArray` entries use the same normalisation, so `FolderContains` matches.

---

## 6. Files that must change

### 6.1 Recommended approach

Widen the scoring seam to publish the handler it already builds, carry it on `QfcPreScoredItem`
alongside the folder string, activate the existing dormant carrier overload chain in
`QfcHomeController.RunAsync`, and have `LoadFolderHandlerAsync` adopt a carried handler inside the
`varList is null` branch only, falling back to the current construction when none is present.
The carried type is `IFolderSearchHandler`, not `FolderPredictor`: that is the exact type
`_folderHandler` is declared as (`QfcItemController.cs:41`), it keeps the concrete `FolderPredictor`
out of the QuickFiler seam, and it is directly mockable.

**Rejected alternatives**

- *Carry the concrete `FolderPredictor`.* Works, but leaks a `UtilitiesCS` concrete class into the
  `IFolderScoringService` contract and offers nothing the interface does not, since the consumer
  never calls `InitAsync` on a carried instance.
- *Add a second `ScoreWithHandlerAsync` member instead of widening `ScoreAsync`.* Avoids editing
  `QfcHighConfidencePreFilterTests.cs`, but leaves two near-duplicate members on a seam whose only
  live implementation is coverage-exempt, and the two datamodel test files must change either way.
  Contradicts "simplicity first".
- *Memoise inside `FolderScoringService` keyed on `EntryID`.* Requires no plumbing but introduces
  process-scoped mutable state (banned by `.claude/rules/general-unit-test.md`), does not intercept
  the item controller's own `_folderPredictorFactory` call, and creates an unbounded staleness
  window.

### 6.2 Production files

| File | Current size | Reason it must change |
|---|---|---|
| `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | 191 | Widen `IFolderScoringService.ScoreAsync` (`:143-147`) and `FolderScoringService.ScoreAsync` (`:170-189`) to publish the initialised handler instead of discarding it at `:184-188`; add the carried member and constructor parameter to `QfcPreScoredItem` (`:98-122`); update the dormant `FilterAsync` tuple destructuring at `:70` and the projection at `:86`. |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 245 | Widen the `_scoreLoader` delegate type (`:58-62`, `:73`, `:105`) and the acceptance projection at `:195` so the handler reaches `QfcGateBatch.Accepted`. **Not named in the issue.** |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 288 | Update `ScoreRemainingQueueMailItemAsync` (`:263-277`) to forward the handler. **Not named in the issue.** |
| `QuickFiler/Controllers/QfcHomeController.cs` | 449 | In `RunAsync`, switch the enabled-mode branch (`:289-302`) to `DequeueNextItemGroupWithOutcomeAsync` and select the carrier overload at `:307`. The issue's `:310` citation is off by three lines and the site is not currently a selection point. |
| `QuickFiler/Controllers/QfcItemGroup.cs` | 52 | Add the carried `IFolderSearchHandler` member alongside `PredeterminedFolder` (`:46-50`). |
| `QuickFiler/Controllers/QfcCollectionController.cs` | **2446** | Thread the carried handler through `LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, ...)` (`:487-566`, specifically the group projection at `:521-534`) and `EncapsulateItemGroup` (`:646-672`). **Already ~5x the 500-line cap and `[ExcludeFromCodeCoverage]` (`:21`)** — additions must go in a new partial part, which requires adding `partial` to the class declaration at `:22`. |
| `QuickFiler/Controllers/QfcItemController.cs` | 323 | Add the carried-handler field next to `_predeterminedFolder` (`:243-248`). |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | **489** | Accept and store the carried handler in the nine-argument constructor (`:86-109`). **Only 11 lines of headroom under the 500-line cap** — a new partial part is likely required. |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 239 | The adoption point: guard inside `LoadFolderHandlerAsync`'s `varList is null` branch (`:61-106`) before `:64`. **This file is cited in the issue body but omitted from its file list.** |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | **499** | Null the carried handler in `Cleanup` alongside `_folderHandler` (`:465`, `:468`) so the group does not outlive the row. **One line of headroom under the cap.** |

Files the issue lists that are confirmed: `QfcHighConfidencePreFilter.cs`, `QfcItemGroup.cs`,
`QfcCollectionController.cs`, `QfcItemController.cs`, `QfcItemController.Initialization.cs`,
`QfcHomeController.cs`. Files added by this research:
`QfcStreamingDequeueConfidenceGate.cs`, `QfcDatamodel.QueueProcessing.cs`,
`QfcItemController.FolderHandling.cs`, `QfcItemController.ViewerSetup.cs`. No listed file is removed.

Interface files that change with them (declaration-only edits):
`QuickFiler/Interfaces/IQfcDatamodel.cs` (only if `QfcDequeueBatch` shape changes; it does **not**
need to, since `QfcPreScoredItem` carries the handler),
`QuickFiler/Interfaces/IQfcCollectionController.cs` and
`QuickFiler/Controllers/IQfcFormController.cs` (unchanged — the carrier overloads already exist).
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`,
`FolderPredictor.IFolderSearchHandler.cs`, `IFolderSearchHandler.cs` and `FolderScorer.cs`
require **no change**.

### 6.3 Leg-B scope decision

`QuickFiler/Controllers/QfcQueue.cs` (610 lines, `public class QfcQueue(...)` with a primary
constructor, not currently `partial`, not coverage-exempt) is required to remove the second pass for
**every page after the first**: `EnqueueAsync` (`:211-276`) and `LoadControllersViewersAsync`
(`:380-421`) both take `IList<MailItem>`, and `QfcHomeController.Iteration.cs:32-34` already has
`batch.PreScored` in hand. Leaving it out means the fix removes the second scoring pass only for the
first `ItemsPerIteration` items of a session.

Given `minor-audit` mode, the 610-line non-partial file, and the new controller-factory seam leg B
needs (§5.2), the recommendation is: **implement legs A and B in one change if the plan's budget
allows; otherwise implement leg A and promote leg B as a separate issue rather than silently closing
#678 with the symptom still present after page one.** Do not close #678 on leg A alone without
saying so.

### 6.4 Test files

| File | Reason |
|---|---|
| `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` | New adoption tests + negative `FromArrayOrString` guard; possible literal update in the source-text test at `:132-148` |
| `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` | Rewrite `:177-181` and `:255-258`; **preserve** `:157-159` and `:236-244` |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs` | Rewrite `:180-201`; likely `:288-390` and `:395-470`; **preserve** `:245-249` and `:276-279` |
| `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs` | `Mock<IFolderScoringService>` strict setups at `:72-80` and `:348` |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | Strict setup at `:337-349` |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | Strict setups at `:160-178` and `:221-242` |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` (+ `.Part2`, `.Part3`) | `scoreLoader` delegate shape at `:28`, `:73`; carrier read at `.Part3:256-262` |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | Source-text signature literal at `:339` |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | `new QfcPreScoredItem(...)` at `:814` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | `new QfcPreScoredItem(...)` at `:307`; extend `:302-326` for the carried handler |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | Extend `:91-123` for a tenth constructor parameter |
| `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs` | Extend `:194-213` if `QfcItemGroup` gains a member |

### 6.5 Coverage impact

`FolderScoringService` **is** `[ExcludeFromCodeCoverage]` today —
`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:166`, with the justification at `:157-165`
(COM-bound body: `MailItemHelper.FromMailItemAsync` plus live Outlook classification). Widening
`IFolderScoringService.ScoreAsync` does **not** change the denominator for that class: the attribute
stays and the class remains excluded. `coverage.config` (repo root) contains no QuickFiler
assembly-level exclusion (`:12-22` excludes only Deedle, FSharp, Castle.Core, FluentAssertions, Moq,
Microsoft.Testing and MSTest), so nothing else is masked.

Denominator effects of the change, by file:

- `QfcHighConfidencePreFilter.cs` — the interface declaration is not executable; `QfcPreScoredItem`'s
  new member is a get-only auto-property (trivially covered by existing construction tests);
  `FolderScoringService` stays exempt. **Net effect ≈ neutral.**
- `QfcStreamingDequeueConfidenceGate.cs`, `QfcHomeController.cs`, `QfcQueue.cs`,
  `QfcItemController.*` — **not** coverage-exempt; every added line enters the denominator and needs
  covering tests. §5 shows the seams exist for all of them except leg B.
- `QfcCollectionController.cs` (`:21`) and `QfcDatamodel.cs` (`:25`) **are**
  `[ExcludeFromCodeCoverage]`; additions there do not enter the denominator, and correspondingly
  cannot be pinned by coverage.

---

## 7. Risks and non-goals

### 7.1 Behaviour that must be preserved exactly

1. **The preselected combo entry must not change.** The selection logic
   (`QfcItemController.FolderHandling.cs:197-209`) is: predetermined folder if non-empty **and**
   `_itemViewer.FolderContains` returns true; otherwise index 1, or index 0 when `FolderArray.Length
   == 1`. Guarded by `FolderHandlingTests.cs:415-437`, `:439-462`, `:480-496` and
   `FolderSuggestionsTests.cs:110-134`, `:136-166`.
   **Concrete threat:** §3.6 — `PredeterminedFolder` is the raw suggestion path
   (`QfcHighConfidencePreFilter.cs:187`) while `FolderArray` holds the archive-prefix-stripped
   projection (`FolderPredictor.cs:807`, `:845-858`). Activating the carrier path without
   normalising one side will make `FolderContains` fail for archive-rooted suggestions and silently
   change the selection from "predetermined" to "index 1". This must be resolved deliberately, with
   a test, before the carrier path goes live.
2. **`HighConfidencePreFilterLoader` must stay uninvoked.** `QfcHighConfidencePreFilter.FilterAsync`
   remains dormant; the live producer is the dequeue gate. Preserve
   `QfcHomeControllerIssue218Tests.cs:157-159` and the equivalent assertions at
   `QfcHomeControllerRunAsyncHighConfidenceTests.cs:239`.
3. **The `FromArrayOrString` conversation-expansion path must be untouched** (§3.3 P3).
4. **`QfcDequeueStop` handling must be untouched.** `IterateQueueAsync`'s
   `SourceExhausted`-only close (`QfcHomeController.Iteration.cs:36-45`) is issue #446 behaviour and
   is unrelated to this change.
5. **The empty-batch path must keep working.** `RunAsync` deliberately reaches `LoadItemsAsync` with
   an empty list; the carrier overload's guard (`QfcFormController.Actions.cs:125-135`) must behave
   the same as the `IList<MailItem>` overload's guard (`:69-79`) — both return early on `null`, not
   on empty.

### 7.2 Accepted behavioural delta (must be stated in the change description)

Reusing the scan-time suggestion set freezes `CtfMap`-derived conversation suggestions
(`FolderScorer.cs:304-326`) at scan time rather than re-deriving them at display time. Bayesian
suggestions and recents are unaffected (recents are read lazily at display time, §2.4). For leg B
the interval between scan and display is longer than for leg A.

### 7.3 Things that will tempt a wider refactor and must stay out of scope

1. **`LoadFolderHandler` (sync) never initialises its predictor** (§3.1). This is a real latent
   defect affecting `LoadSequential_5` (`QfcCollectionController.cs:712`),
   `EnumerateConversationMembers` (`:1872`) and `AddItemGroup` (`:1920`), and it is why the
   conversation-expansion replication is currently a no-op. It is adjacent, it will be obvious while
   reading `FolderHandling.cs`, and fixing it changes user-visible combo contents on three paths.
   **Promote as a separate issue; do not fix here.**
2. **`QfcCollectionController.cs` at 2446 lines and `QfcQueue.cs` at 610 lines** both breach the
   500-line cap. Splitting them is not this issue's work; add new members in new partial parts.
3. **`QfcCollectionController` and `QfcDatamodel` are `[ExcludeFromCodeCoverage]`.** Do not attempt
   to de-exempt them as part of this change.
4. **`QfcHighConfidencePreFilter.FilterAsync` and the `ApplyHighConfidenceFilterAsync` /
   `RemoveBelowThresholdAsync` post-display filter** (`QfcFormController.Actions.cs:171-182`) are
   dormant #169/#171 code. Deleting them is a separate decision.
5. **Refactoring `IFolderSearchHandler`** to add `InitAsync` (which would let
   `_folderPredictorFactory` return the interface instead of the concrete type,
   `QfcItemController.cs:79-88`) would widen a `UtilitiesCS` public interface for a QuickFiler
   convenience. Out of scope.
6. **The five `MailItemHelper.FromMailItemAsync` duplications** across the gate and the collection
   controller are a separate COM-traffic reduction opportunity. Out of scope.

---

## 8. Numeric Derivation Evidence

Work mode is `minor-audit`; no `spec.md` exists and none is created, so no numeric acceptance
criterion is proposed by this research. The one count in §4 that is most likely to be lifted into a
plan task is derived below to the required standard; all other counts in this document are
descriptive enumerations with per-item citations and are not offered as acceptance criteria.

**Claim under derivation:** the number of Moq `Verify` sites in `QuickFiler.Test` that constrain the
invocation count of `IQfcFormController.LoadItemsAsync(IList<QfcPreScoredItem>)`.

- **Complete Family:** every Moq `Verify` expression in the `QuickFiler.Test` project whose
  expression tree binds the `IList<QfcPreScoredItem>` overload of
  `IQfcFormController.LoadItemsAsync`. `IQfcFormController.cs:32-33` declares two such overloads
  (with and without `ProgressTracker`); both are in the family.
- **Exhaustive Search Scope:** the entire `QuickFiler.Test` tree, all `*.cs` files, no path filter.
- **Inclusion Rules:** the call is `Mock<IQfcFormController>.Verify(...)` (or `Mock.Get(...).Verify`)
  and the verified member is one of the two `QfcPreScoredItem` overloads.
- **Exclusion Rules:** `Setup(...)` arrangements are excluded (they configure, not constrain);
  `Task.FromResult<IList<QfcPreScoredItem>>(...)` returns from the `HighConfidencePreFilterLoader`
  stub are excluded; XML-doc `<see cref>` references are excluded; source-text string literals
  naming the signature are excluded; `new QfcPreScoredItem(...)` constructions are excluded.
- **Primary Search Strategy:** regex
  `LoadItemsAsync\(It\.IsAny<IList<QfcPreScoredItem>>\(\)\)` over `QuickFiler.Test`, then manual
  classification of each hit as `Setup` or `Verify` by reading the enclosing statement.
- **Primary Member Set:**
  1. `QfcHomeControllerIssue218Tests.cs:178`
  2. `QfcHomeControllerIssue218Tests.cs:256`
  3. `QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`
  4. `QfcHomeControllerRunAsyncHighConfidenceTests.cs:277`
  (Discarded as `Setup`: `QfcHomeControllerIssue218Tests.cs:120`,
  `QfcHomeControllerRunAsyncHighConfidenceTests.cs:67`.)
- **Primary Count:** 4
- **Cross-check Search Strategy:** a deliberately broader, differently-shaped query — bare token
  `QfcPreScoredItem` over `QuickFiler.Test`, returning all 17 occurrences across 7 files, each then
  read in context and classified against the inclusion/exclusion rules. This query does not mention
  `LoadItemsAsync`, `It.IsAny` or `Verify`, so it cannot inherit the primary query's shape bias, and
  it enumerates the whole type-usage family rather than one call pattern.
- **Cross-check Member Set:** of the 17 occurrences —
  `QfcCollectionControllerTests.cs:298` (doc), `:307` (construction) — excluded;
  `QfcFormControllerSeamTests.cs:339` (source-text literal) — excluded;
  `QfcFormControllerTests.cs:788` (doc), `:812`, `:814` (construction) — excluded;
  `QfcStreamingDequeueConfidenceGateTests.Part3.cs:256` (carrier read) — excluded;
  `QfcHomeControllerIssue218Tests.cs:120` (Setup), `:152` (loader stub return), `:239` (loader stub
  return) — excluded; `QfcHomeControllerRunAsyncHighConfidenceTests.cs:67` (Setup), `:95` (loader
  stub return), `:232` (loader stub return) — excluded; leaving
  `QfcHomeControllerIssue218Tests.cs:178`, `QfcHomeControllerIssue218Tests.cs:256`,
  `QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`,
  `QfcHomeControllerRunAsyncHighConfidenceTests.cs:277`.
- **Cross-check Count:** 4
- **Member-set Comparison:** normalised as `<file>:<line>`, the primary set
  `{Issue218:178, Issue218:256, RunAsyncHC:246, RunAsyncHC:277}` and the cross-check set
  `{Issue218:178, Issue218:256, RunAsyncHC:246, RunAsyncHC:277}` are identical; no member is present
  in one and absent from the other. Both counts are 4 and the sets agree.

Derived split, used in §4.4-§4.5: two of the four (`Issue218:178`, `Issue218:256`) sit in
high-confidence-**enabled** tests and require deliberate rewrite; two (`RunAsyncHC:246`,
`RunAsyncHC:277`) sit in high-confidence-**disabled** tests and must remain `Times.Never`.

---

## 9. Open questions this research could not settle from code

1. **Is `AddBayesianSuggestionsAsync` deterministic across the two passes for the same item?** It
   resolves its predictor through `new OlFolderClassifierGroup(globals).GetFolderPredictorAsync()`
   (`FolderScorer.cs:163`, `:170`) on every call. Whether that resolution can observe a
   mid-session-retrained model — which would make the two passes legitimately differ — is not
   determinable from the QuickFiler call sites alone. **Evidence that would settle it:** reading
   `OlFolderClassifierGroup.GetFolderPredictorAsync` and establishing whether the returned predictor
   is memoised for the session or rebuilt from mutable state.
2. **Is the scan-to-display staleness window (§2.6 item 4) user-observable for leg B?** It depends on
   how long items sit in `QfcQueue` before `Iterate2` dequeues them
   (`QfcHomeController.Iteration.cs:83`), which is driven by user filing pace. **Evidence:**
   instrumented timing from a live session, or an explicit product decision that scan-time
   suggestions are acceptable.
3. **Whether the raw-versus-projected path mismatch (§3.6) was intentional.** The carrier path has
   never run, so no behaviour distinguishes the two readings. **Evidence:** the #171 design record,
   or a maintainer decision on which normalisation is canonical for `PredeterminedFolder`.
