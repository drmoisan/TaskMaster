# Research — `QuickFiler/Controllers/EfcDataModel.cs` (F5 / issue #436, epic #136)

- Timestamp: 2026-08-08T00-43
- Feature: `quickfiler-datamodel-coverage` (child F5, wave 1)
- Parent epic: `quickfiler-per-file-coverage` (#136), integration branch `epic/quickfiler-per-file-coverage-integration`
- Target production file: `QuickFiler/Controllers/EfcDataModel.cs` (397 lines, no `[ExcludeFromCodeCoverage]`)
- Upstream dependency: F1 (`coverage-ledger.md` + per-file coverage harness). Neither exists on disk yet; this
  artifact is written to consume that contract, not to substitute for it.
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a923053598cf4ccea`

## 0. Method and confidence

Everything below is grounded in files read in this session. Three distinct evidence classes are used and
labelled throughout:

1. **Source read** — direct reads of production and test files (highest confidence).
2. **Measured coverage** — the committed Cobertura report at
   `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
   which contains a `<class ... name="QuickFiler.Controllers.EfcDataModel" filename="QuickFiler\Controllers\EfcDataModel.cs">`
   element at line 16976 with a per-line hit map at lines 17219–17556. **The line numbers in that hit map are
   exactly aligned with the current `EfcDataModel.cs`** (e.g. `DescribeSynchronizationContext` at 27–29, the
   four-argument constructor at 47–80, the private constructor at 82–86, `get_Globals` at 150). `main` is at
   `74be1964`, whose recent commits are documentation-only merges, so the map is treated as current. This is a
   material improvement over inference: the uncovered-member map in §3 is **measured, not inferred**.
3. **Inference** — explicitly marked where used (projected post-change coverage, and the behavior of
   `FolderScorer.LoadFromField`, which was not read end-to-end).

Note on the headline percentage: the report's `<class>` element states `line-rate="0.55618"` and
`branch-rate="0.457143"`. An independent hand count of the per-line hit map in that same element yields
124 covered / 250 instrumented lines (49.6%) and 18/46 covered branch arms (39.1%). The two do not reconcile
because the tool's class-level rate is computed over a denominator that differs from the `<lines>` union (the
`<methods>` list in that element omits the async state-machine methods `CreateAsync`, `InitFolderHandlerAsync`,
`MoveToFolderAsync`, `OpenOlFolderAsync`, `OpenFsFolderAsync`, while the `<lines>` union includes their source
lines). **Both figures are far below the 80% floor, so the gap conclusion is robust either way.** The
authoritative number for acceptance must come from F1's harness; do not quote either figure as final.

## 1. Member inventory

`internal class EfcDataModel` (`EfcDataModel.cs:20`), namespace `QuickFiler.Controllers`, non-sealed, no base
type, no interface implementations. `QuickFiler/Properties/AssemblyInfo.cs:5` declares
`[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so the whole surface is directly reachable from tests.

| # | Member | Lines | Behavior |
|---|---|---|---|
| 1 | `static readonly log4net.ILog logger` (`.cctor`) | 22–24 | log4net logger for the declaring type. |
| 2 | `static string DescribeSynchronizationContext(SynchronizationContext)` | 26–29 | Returns the context's full type name, or `"null"`. |
| 3 | `static string BuildDataModelTimingContext()` | 31–34 | Formats `threadId=…; syncContext=…` diagnostic string. |
| 4 | `static void LogDataModelTiming(string phase, string details = null)` | 36–43 | Prefixes `[Data model timing]` when absent, appends details, logs at Debug. |
| 5 | `public EfcDataModel(IApplicationGlobals, MailItem, CancellationTokenSource, CancellationToken)` | 47–80 | Assigns Globals/Token/TokenSource; falls back to `TryGetFirstInSelection()` when `mail` is null; when a mail exists, constructs a `ConversationResolver`, loads its dataframe **synchronously** (`LoadDf()`), and subscribes `PropertyChanged`. Emits three timing log entries with a `Stopwatch`. |
| 6 | `private EfcDataModel(IApplicationGlobals, MailItem)` | 82–86 | Minimal constructor used only by `CreateAsync`. |
| 7 | `public static async Task<EfcDataModel> CreateAsync(IApplicationGlobals, IList<MailItem>, CancellationTokenSource, CancellationToken, bool loadAll)` | 88–141 | Guards `globals`/`mailItems`; **snapshots the live selection to an array** (`ToArray()`, line 98) before any await; builds the model from element 0; routes to the multi-item `ConversationResolver.LoadAsync` overload when the snapshot length > 1, else the single-mail overload with `loadAll`; sets `ConversationResolver.Parent = dataModel`. |
| 8 | `public IApplicationGlobals Globals { get; protected set; }` | 147–152 | Backing field `_globals`. |
| 9 | `public CancellationToken Token { get; protected set; }` | 154–159 | Backing field `_token`. |
| 10 | `public CancellationTokenSource TokenSource { get; protected set; }` | 161–166 | Backing field `_tokenSource`. |
| 11 | `public FolderPredictor FolderHelper { get; protected set; }` | 168–177 | Plain accessor over `_folderHelper`; getter body contains only a commented-out lazy-init line. |
| 12 | `public async Task InitFolderHandlerAsync(object folderList = null)` | 179–212 | Three-way branch. `folderList == null && MailInfo == null` → `new FolderPredictor(Globals)`. `folderList == null && MailInfo != null` → `new FolderPredictor(Globals, MailInfo, FromField).InitAsync(MailInfo, FromField)`. `folderList != null` → `new FolderPredictor(Globals, folderList, FromArrayOrString).InitAsync(folderList, FromArrayOrString)`. All three wrapped in `Task.Run(..., Token)` and assigned to `FolderHelper`. |
| 13 | `public ConversationResolver ConversationResolver { get; protected set; }` | 214–219 | Backing field `_conversationResolver`. |
| 14 | `public MailItem Mail { get; set; }` | 221–230 | Getter lazily falls back to `TryGetFirstInSelection()` when `_mail` is null. Public setter. |
| 15 | `public MailItemHelper MailInfo => ConversationResolver?.MailHelper;` | 232 | Null-propagating projection. |
| 16 | `private MailItem TryGetFirstInSelection()` | 234–252 | Reads `_globals.Ol.App.ActiveExplorer().Selection`; returns `selection[1] as MailItem` when `Count > 0`, else null; broad `catch (System.Exception)` returns null. |
| 17 | `public async Task<bool> MoveToFolderAsync(string folderpath, bool saveAttachments, bool saveEmail, bool savePictures, bool moveConversation)` | 258–296 | Returns false when `MailInfo` is null. Forces `saveAttachments` false for the literal folder `"Trash to Delete"`. Selects `ConversationResolver.ConversationInfo.SameFolder` when `moveConversation`, else a singleton of `MailInfo`. Returns false with a `logger.Warn` when `Globals.FS.SpecialFolders` has no `"OneDrive"` key. Builds an `EmailFilerConfig`, constructs `new EmailFiler(config)`, awaits `SortAsync(mailHelpers)`, calls `SortEmail.Cleanup_Files()`, returns the sorter result. |
| 18 | `internal async Task OpenOlFolderAsync(string folderpath)` | 298–315 | Returns silently when `"OneDrive"` is absent; otherwise builds an `EmailFilerConfig` and awaits `new EmailFiler(config).OpenOlFolderAsync()`. |
| 19 | `internal async Task OpenFsFolderAsync(string folderpath)` | 317–333 | Same shape as #18 but awaits `OpenFileSystemFolderAsync()`. |
| 20 | `public async Task MoveToFolderAsync(MAPIFolder folder, string olAncestor, bool saveAttachments, bool saveEmail, bool savePictures, bool moveConversation)` | 335–360 | Strips `olAncestor` from `folder.FolderPath` and trims one leading `\`; delegates to #17; on a false result calls `MessageBox.Show($"Cannot move to folderpath {folderpath}")`. |
| 21 | `public IList<MailItem> PackageItems(bool moveConversation)` | 362–372 | Returns `_conversationResolver.ConversationItems.SameFolder` when true, else a singleton list of `Mail`. |
| 22 | `public string[] FindMatches(string searchText)` | 374–387 | Wraps a non-empty `searchText` in `*…*`, then calls `_folderHelper.FindFolder(searchString:, reloadCTFStagingFiles: false, recalcSuggestions: false, objItem: _mail)`. |
| 23 | `public void RefreshSuggestions()` | 389–393 | Calls `_folderHelper.RefreshSuggestions(mailItem: Mail)`. |

**Dead member.** A repository-wide grep for `PackageItems` returns only the declaration at
`EfcDataModel.cs:362`, an unrelated `QfcItemController.PackageItems()` (different signature, returns
`IList<MailItemHelper>`), and a commented-out line at `EfcHomeController.cs:437`. **`EfcDataModel.PackageItems(bool)`
has no caller anywhere in the repository** and is nonetheless in the coverage denominator.

**Consumers of the type** (all owned by siblings F8/F9 — read only, never modified by F5):

- `EfcHomeController.ExecuteMoves.cs:95` → `_dataModel.MoveToFolderAsync(string, bool, bool, bool, bool)`
- `EfcHomeController.cs:425` / `:430` → `DataModel.OpenOlFolderAsync` / `DataModel.OpenFsFolderAsync`
- `EfcFormController.cs:492`, `:771` → `_dataModel.FolderHelper.CreateFolderAsync` / `.CreateFolder`
- `EfcFormController.cs:502`, `:781` → `_dataModel.MoveToFolderAsync(MAPIFolder, …)`
- `EfcFormController.cs:558`, `:801` → `_dataModel.FindMatches(...)`
- `EfcFormController.cs:799` → `_dataModel.RefreshSuggestions()`
- `EfcFormController.cs:891` → `_dataModel?.FolderHelper?.Suggestions?.ToScoredArray()`
- `EfcFormController.cs:1033`, `:1037` → `_dataModel.InitFolderHandlerAsync(folderList)` then `.FolderHelper.FolderArray`
- `EfcItemController.cs:282` → `dataModel.MailInfo`
- `EfcHomeControllerDependencies.cs:34–96` → constructs `EfcDataModel` through two injected factory delegates

Every seam proposed in §5 is **additive** (new `internal` members, plus behavior-preserving internal
restructuring of three method bodies). No signature, accessibility, or return type in the list above changes.

## 2. Relationship to the QFC datamodel

**They are independent types with no shared abstraction.** Specifics:

- `EfcDataModel` (`EfcDataModel.cs:20`) declares no base type and no interfaces. It does **not** implement
  `IQfcDatamodel`.
- `IQfcDatamodel` (`QuickFiler/Interfaces/IQfcDatamodel.cs:24–58`) declares a queue/batch contract:
  `DequeueNextItemGroupAsync` (two overloads), `DequeueNextItemGroup`, `UndoMove`, `MovedItems`,
  `InitEmailQueue`, `InitEmailQueueAsync`, `Complete`, `Cleanup`, plus the `SortOptionsEnum` flags enum.
  `EfcDataModel` has **none** of these members and no queue at all.
- `QfcDatamodel` (`QfcDatamodel.cs:25–26`) is `[ExcludeFromCodeCoverage] public partial class QfcDatamodel : IQfcDatamodel`.
- Overlap is limited to shared *dependencies*, not shared code: both take `IApplicationGlobals`, both hold
  `Token`/`TokenSource` properties with the same shape, and both reference `MailItem`. There is no duplicated
  method body and no common base.

**Consequence for F5's test infrastructure.** Test infrastructure **cannot** be shared at the fixture or
base-class level between `QfcDatamodelTests` and `EfcDataModel` tests, because the units under test share no
contract. What *can* and should be shared is the **fake/mock support layer**: an in-memory
`IApplicationGlobals` + `IFileSystemFolderPaths` pair, a `CreateUninitialized<T>` helper, and a private-field
reflection setter. Those already exist as private nested types in `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:374–433`
(`FakeApplicationGlobals`, `FakeFileSystemFolderPaths`) and in
`QuickFiler.Test/Controllers/EfcHomeControllerSeamTests.cs:284–288` (`CreateUninitialized<T>` via
`FormatterServices.GetUninitializedObject`). Because they are `private sealed` nested classes they are not
reusable as written; §7 proposes promoting equivalents into a shared
`QuickFiler.Test/Controllers/EfcDataModel.TestSupport.cs`, mirroring the existing
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` convention.

Also relevant: `QfcDatamodel.cs:112` already carries `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;`
and `QfcDatamodel.cs:128` `internal Func<CancellationToken, Task<bool>> RemainingEmailLoader { get; set; }`.
That establishes the **house style for this cluster**: `internal` property-injected seams with production
defaults. §5 follows it.

## 3. Current coverage reality (measured)

Mapping of every member to the measured hit map (`coverage-final.cobertura.xml:17219–17556`). "Covered" means
every instrumented line of the member has `hits="1"`.

### Fully covered today (do not re-test)

| Member | Lines | Exercised by |
|---|---|---|
| `.cctor` (logger) | 22–24 | any test that touches the type |
| `DescribeSynchronizationContext` | 27–29 | constructor / `CreateAsync` tests |
| `BuildDataModelTimingContext` | 32–34 | same |
| `LogDataModelTiming` (both branches) | 37–43 | same |
| 4-arg constructor (all lines, both branches) | 47–80 | `Constructor_WhenMailProvided_LoadsConversationSnapshotSynchronously`, `Constructor_WhenMailProvided_LeavesBackgroundInitializationStaged` (`EfcDataModelTests.cs:152`, `:186`) |
| private 2-arg constructor | 82–86 | `CreateAsync_*` tests |
| `CreateAsync` (all lines incl. both arms of the `Length > 1` branch at 113) | 95–141 | `CreateAsync_StagesSnapshotLoadBeforeBackgroundInitialization` (2 mails → multi arm), `CreateAsync_WithSingleSelectedMail_UsesSingleMailConversationResolverPath` and `…_LeavesBackgroundInitializationStaged` (1 mail → single arm) |
| `Globals` / `Token` / `TokenSource` get+set | 150–165 | constructor tests |
| `ConversationResolver` get+set | 217–218 | constructor / `CreateAsync` tests |
| `Mail` get (both branches) + set | 225–229 | `dataModel.Mail.Should().BeSameAs(...)` assertions |

`EfcDataModelTests.cs:83` (`LoadConversationInfoAsync_WhenGlobalsDoNotExposeOutlookApp_DoesNotRequireApp`)
exercises `ConversationResolver` and `MailItemHelper` directly and contributes **no** `EfcDataModel` line
coverage; it is a `ConversationResolver` test living in the wrong file (F4 owns `ConversationResolver.cs`).
Leave it in place — moving it is out of F5's scope and would create a cross-child conflict.

### Uncovered today — these are the genuine gaps F5 must close

| Member | Uncovered lines | Count |
|---|---|---|
| `FolderHelper` get + set | 172, 174, 175, 176 | 4 |
| `InitFolderHandlerAsync` (all three branches) | 180–186, 188–199, 201–212 | 31 |
| `MailInfo` (both branches of `?.`) | 232 | 1 |
| `TryGetFirstInSelection` — the non-throwing path | 239–241, 244–245 | 5 |
| `MoveToFolderAsync(string, …)` | 265–268, 271–274, 276–279, 281–290, 292–296 | 27 |
| `OpenOlFolderAsync` — body after the guard | 305–311, 313–314 | 9 |
| `OpenFsFolderAsync` — body after the guard | 323–329, 331–332 | 9 |
| `MoveToFolderAsync(MAPIFolder, …)` | 343–360 | 18 |
| `PackageItems(bool)` | 363–366, 369–370, 372 | 7 |
| `FindMatches(string)` | 375–379, 381–387 | 12 |
| `RefreshSuggestions()` | 390, 392, 393 | 3 |
| **Total** | | **126** |

Partial-branch notes: line 300 (`OpenOlFolderAsync` OneDrive guard) and line 319 (`OpenFsFolderAsync` OneDrive
guard) are at `condition-coverage="50% (1/2)"` — only the *missing-OneDrive* arm is exercised today, incidentally,
because the mocked globals in existing tests have no `"OneDrive"` entry. Line 239
(`(selection?.Count ?? 0) > 0`) is `0% (0/4)`: the whole comparison is unreached because the strict
`Mock<IOlObjects>` in `EfcDataModelTests.CreateGlobals()` (`:220–228`) sets up only `EmailPrefixToStrip`, so
`_globals.Ol.App` throws and control lands in the `catch` at 248.

**Confidence: high** for the uncovered-member map (measured, line-aligned). **Medium** for the 126/250 totals
(hand-counted from the XML; the tool's own class-level rate uses a different denominator).

## 4. Testability blockers, per uncovered member

| Member | What specifically blocks a deterministic unit test today |
|---|---|
| `FolderHelper` get/set | Nothing structural. The blocker is purely that no test constructs an `EfcDataModel` without running the COM-touching public constructor. Solved by construction technique, not by a seam. |
| `InitFolderHandlerAsync` | Constructs `FolderPredictor` inline. Two of the three branches are actually reachable COM-free: `new FolderPredictor(IApplicationGlobals)` (`FolderPredictor.cs:35–40`) only stores `AppGlobals.Ol.App` and allocates a `FolderScorer`, and `InitAsync(…, FromArrayOrString)` → `FromArrayOrString` (`FolderPredictor.cs:104–131`) is pure string/array logic. The **`FromField` branch is the hard blocker**: `InitAsync(…, FromField)` → `InitializeFromEmail` → `FromFolderKey(MailItemHelper)` (`FolderPredictor.cs:141–147`) → `FolderScorer.LoadFromField(mailInfo, _globals)` / `Suggestions.RefreshSuggestions(...)`, which reach into the globals-backed classifier stack (*inferred* — `FolderScorer` was not read end-to-end). Testing that path end-to-end would be testing `FolderPredictor`, not `EfcDataModel`. |
| `MailInfo` | No blocker beyond construction; needs a substitutable `ConversationResolver` instance. |
| `TryGetFirstInSelection` (non-throwing path) | Requires the COM chain `IOlObjects.App` → `Application.ActiveExplorer()` → `Explorer.Selection` → `Selection.Count` / `Selection[1]`. Mockable, but no test in this file sets it up today. |
| `MoveToFolderAsync(string, …)` | Three blockers: (a) `Globals.FS.SpecialFolders` (`ConcurrentDictionary<string,string>` per `UtilitiesCS/Interfaces/IGlobals/IFileSystemFolderPaths.cs:7`) and `Globals.Ol.ArchiveRootPath`; (b) `ConversationResolver.ConversationInfo.SameFolder`; (c) **the hard blocker** — `new EmailFiler(config)` then `await sorter.SortAsync(mailHelpers)`. `EmailFiler.SortAsync(IList<MailItemHelper>)` (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:128`) is **non-virtual**, so Moq cannot intercept it, and its body immediately dereferences `MailHelpers.FirstOrDefault().FolderInfo.OlFolder` cast to `Folder` (COM). A `Func<EmailFilerConfig, EmailFiler>` factory alone (the `QfcItemController` shape) is therefore **insufficient** here. Secondary: `SortEmail.Cleanup_Files()` is a static that mutates static `YesNoToAllResponse` fields — mutable global state; it is proven not to throw (`UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:175`), so it is tolerable but must be acknowledged. |
| `OpenOlFolderAsync` / `OpenFsFolderAsync` | Same OneDrive/ArchiveRoot dependency, plus `EmailFiler.OpenOlFolderAsync()` (non-virtual; `TryOpenOlFolder` sets `Ol.App.ActiveExplorer().CurrentFolder` **and shows a `MessageBox` on error**) and `OpenFileSystemFolderAsync()` (non-virtual; calls `Process.Start("explorer.exe", …)` — an external process, prohibited by UT4). |
| `MoveToFolderAsync(MAPIFolder, …)` | `folder.FolderPath` is trivially mockable (existing precedent: `folder.SetupGet(x => x.FolderPath)` at `EfcDataModelTests.cs:93`). The blockers are (a) the inner `MoveToFolderAsync` (above) and (b) **`MessageBox.Show` at line 358** — a modal popup requiring human interaction, an outright unit-test-policy violation per epic.md Shared Design §2. |
| `PackageItems(bool)` | Needs a substitutable `ConversationResolver` whose `ConversationItems` (`Pair<IList<MailItem>>`, settable per `IConversationResolver.cs:15`) is populated. No COM required. |
| `FindMatches(string)` | Calls `_folderHelper.FindFolder(...)` on the concrete `FolderPredictor`, whose `FindFolder` (`FolderPredictor.cs:292`) is non-virtual and walks the Outlook folder tree via `GetMatchingFolders`. |
| `RefreshSuggestions()` | Calls `_folderHelper.RefreshSuggestions(mailItem: Mail)` (`FolderPredictor.cs:968`), non-virtual, scorer/COM bound. |

None of these is an irreducible COM remainder. Every one is reachable behind a seam of the kind
`.claude/rules/csharp.md` § "DI Seams" already prescribes and that this exact code family already uses.
Under the epic's ratified reconciliation (refactor first, exempt only the irreducible remainder), **no part of
`EfcDataModel.cs` qualifies for `[ExcludeFromCodeCoverage]`**, and none is present today. F5 must not add one.

## 5. Seam proposals

### 5.1 Precedent survey (checked before designing anything)

| Precedent | Location | Shape |
|---|---|---|
| Constructor-injected factory-delegate bundle | `EfcHomeControllerDependencies.cs:34–127` | ~11 optional `Func<>`/named-delegate ctor parameters, each `?? ProductionDefault`, exposed as get-only properties; static `…WithFactory` helpers do the null-argument validation. |
| Property-injected delegate with null-check at call site | `EfcHomeController.ExecuteMoves.cs:13–20` | `internal Func<string,bool,bool,bool,bool,Task<bool>> MoveToFolderAsyncAction { get; set; }`, consumed as `MoveToFolderAsyncAction is null ? _dataModel.MoveToFolderAsync(...) : MoveToFolderAsyncAction(...)`. |
| **MessageBox seam** | `EfcHomeController.ExecuteMoves.cs:22–23` | `internal Action<string> MoveFailureMessageAction { get; set; } = text => MessageBox.Show(text);` |
| MessageBox / dialog seams as `internal static` properties | `FolderPredictor.cs:160–172` | `ShowPromptMessageAction`, `PromptForFolderNameDialog`. |
| `??=`-defaulted factory fields | `QfcItemController.Initialization.cs:389–397` | `_emailFilerFactory ??= config => new EmailFiler(config);` `_folderPredictorFactory ??= (globals, objItem, options) => new FolderPredictor(globals, objItem, options);` `_folderPredictorEmptyFactory ??= globals => new FolderPredictor(globals);` |
| **Narrow interface seam over `FolderPredictor`** | `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs` + `FolderPredictor.IFolderSearchHandler.cs` | `IFolderSearchHandler { string[] FolderArray; FolderScorer Suggestions; FolderRow[] FolderRowArray; string[] FindFolder(...) }`, implemented by `FolderPredictor` on a second partial part so the 823-line source file was not disturbed. |
| Property-injected `TimeProvider` + loader delegate | `QfcDatamodel.cs:112`, `:128` | `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;` |
| In-memory `IFolderSearchHandler` fake | `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:30–45` | `FakeFolderHandler : IFolderSearchHandler`, COM-free. |
| Construction without running a ctor | `QuickFiler.Test/Controllers/EfcHomeControllerSeamTests.cs:284–288` | `FormatterServices.GetUninitializedObject(typeof(T))`, applied to `EfcDataModel` at `:277–282`. |
| Private-field injection | `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:47–50` | reflection `SetPrivate(controller, "_folderHandler", handler)`. |
| In-memory globals fakes | `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:374–433` | `FakeApplicationGlobals`, `FakeFileSystemFolderPaths` (real `ConcurrentDictionary` for `SpecialFolders`). |

**Chosen approach: extend the established EFC-family pattern rather than invent a new one.** Concretely, F5
adopts (a) the `EfcHomeController.ExecuteMoves` *property-injected `internal` delegate with a null check at the
call site* for the filing/dialog boundaries, (b) the `QfcItemController` *`FolderPredictor` factory delegate*
shape verbatim for construction, and (c) the *existing, unmodified* `IFolderSearchHandler` interface for the
`FindFolder` consumption path. The already-in-repo `FakeFolderHandler`, `FakeApplicationGlobals`,
`CreateUninitialized<T>` and `SetPrivate` techniques are reused rather than rebuilt.

### 5.2 Proposed seams, ranked by the seam hierarchy

**S1 — Interface seam (rank 1). Folder search.** No new interface; reuse `UtilitiesCS.IFolderSearchHandler`,
which `FolderPredictor` already implements and which already declares `FindFolder` with the exact signature
`FindMatches` calls.

```csharp
// additive, internal; production leaves it null and falls through to _folderHelper
internal IFolderSearchHandler FolderSearchOverride { get; set; }
private IFolderSearchHandler FolderSearchHandler => FolderSearchOverride ?? _folderHelper;
```
Injection point: `FindMatches` (line 381) calls `FolderSearchHandler.FindFolder(...)` instead of
`_folderHelper.FindFolder(...)`. `FolderHelper` stays typed `FolderPredictor`, so `EfcFormController.cs:1037`
(`.FolderHelper.FolderArray`), `:891` (`.FolderHelper.Suggestions`) and `:492`/`:771`
(`.FolderHelper.CreateFolderAsync` / `.CreateFolder`, which are **not** on `IFolderSearchHandler`) are
unaffected. **No change to `UtilitiesCS`.**

**S2 — Injectable delegate (rank 2). `FolderPredictor` construction/initialization.**

```csharp
internal Func<IApplicationGlobals, FolderPredictor> FolderPredictorEmptyFactory { get; set; }
internal Func<IApplicationGlobals, object, FolderPredictor.InitOptions, Task<FolderPredictor>> FolderPredictorInitializer { get; set; }
```
Defaults (assigned with `??=` in both constructors, mirroring `QfcItemController.Initialization.cs:391–397`):
`globals => new FolderPredictor(globals)` and
`(globals, item, options) => new FolderPredictor(globals, item, options).InitAsync(item, options)`.
Injection point: `InitFolderHandlerAsync` (lines 179–212), restructured to keep its three branches and its
`Task.Run(..., Token)` wrapper byte-for-byte equivalent in behavior:

```csharp
if (folderList is null)
{
    FolderHelper = MailInfo is null
        ? await Task.Run(() => FolderPredictorEmptyFactory(Globals), Token)
        : await Task.Run(() => FolderPredictorInitializer(Globals, MailInfo, FolderPredictor.InitOptions.FromField), Token);
}
else
{
    FolderHelper = await Task.Run(() => FolderPredictorInitializer(Globals, folderList, FolderPredictor.InitOptions.FromArrayOrString), Token);
}
```
Two delegates rather than the three-field `QfcItemController` shape because `EfcDataModel` always follows
construction with `InitAsync` on the non-empty branches; folding init into the delegate removes the need to
call the non-mockable `InitAsync` on a test double.

**S3 — Injectable delegate (rank 2). Email filing operations.** An interface seam was considered first and is
recorded as rejected in §5.3.

```csharp
internal Func<EmailFilerConfig, IList<MailItemHelper>, Task<bool>> SortAsyncAction { get; set; }
internal Func<EmailFilerConfig, Task> OpenOlFolderAction { get; set; }
internal Func<EmailFilerConfig, Task> OpenFsFolderAction { get; set; }
```
Defaults, one physical line each: `(config, helpers) => new EmailFiler(config).SortAsync(helpers)`,
`config => new EmailFiler(config).OpenOlFolderAsync()`, `config => new EmailFiler(config).OpenFileSystemFolderAsync()`.
Injection points: `MoveToFolderAsync` lines 292–293, `OpenOlFolderAsync` lines 313–314, `OpenFsFolderAsync`
lines 331–332. Each seam must carry the *whole* construct-and-invoke step, because the `EmailFiler` methods are
non-virtual (see §4).

**S4 — Injectable delegate (rank 2). Modal dialog.** Copied verbatim from
`EfcHomeController.ExecuteMoves.cs:22–23`:

```csharp
internal Action<string> MoveFailureMessageAction { get; set; } = text => MessageBox.Show(text);
```
Injection point: `MoveToFolderAsync(MAPIFolder, …)` line 358.

**S5 — Injectable delegate (rank 2). Suggestion refresh.** `IFolderSearchHandler` does **not** declare
`RefreshSuggestions`, and adding it would mean editing `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs`
— a cross-project change outside the epic's file set. Use a delegate instead:

```csharp
internal Action<MailItem> RefreshSuggestionsAction { get; set; }
```
Injection point: line 392 becomes `(RefreshSuggestionsAction ?? (mail => _folderHelper.RefreshSuggestions(mailItem: mail)))(Mail);`

**S6 — Pure-function extraction (no seam needed).** Three fragments are pure and should be lifted to
`internal static` methods, mirroring the `EfcHomeController.SelectMoveMetricsItems` and
`EmailFilerConfig.GetStem` precedents. This converts branch-heavy lines into directly testable units and is the
cheapest coverage in the file:

```csharp
internal static string BuildSearchPattern(string searchText);              // from lines 376–379
internal static bool ShouldSaveAttachments(string folderpath, bool saveAttachments); // from line 271
internal static string StripAncestorPrefix(string folderPath, string olAncestor);    // from lines 344–348
```

**S7 — No seam; construction technique only.** `FolderHelper`, `MailInfo`, `PackageItems`,
`TryGetFirstInSelection` need only `FormatterServices.GetUninitializedObject<EfcDataModel>()` plus reflection
assignment of `_globals`, `_mail`, `_conversationResolver`, `_folderHelper`, `_token` (all private fields;
`Globals`/`Token`/`TokenSource`/`FolderHelper`/`ConversationResolver` have `protected set`, so reflection on the
backing field is the mechanism, exactly as `QfcItemController.FolderSuggestionsTests` does).

### 5.3 Rejected alternatives (brief)

- **`IEmailFilingOperations` interface + `EmailFilerAdapter` production class.** Nominally rank 1 in the seam
  hierarchy. Rejected: it creates a *new production file* whose only implementation is an untestable COM/`Process.Start`
  adapter. That relocates the uncovered lines instead of removing them, adds a 122nd compiled file to the epic
  denominator that F16 must then account for, and contradicts §6's "leave only the thinnest possible wiring".
  The delegate seam keeps the irreducible remainder to ~5 one-line lambdas inside the file already being measured.
- **Adding `RefreshSuggestions(MailItem, int)` to `IFolderSearchHandler`.** Technically additive and already
  satisfied by `FolderPredictor.cs:968`. Rejected: it edits `UtilitiesCS`, which is outside the epic's file
  assignments, and pulls a COM type into that interface's signature for one call site. S5 achieves the same
  isolation with no cross-project edit.
- **Making `EmailFiler.SortAsync(IList<MailItemHelper>)` / `OpenOlFolderAsync` / `OpenFileSystemFolderAsync`
  `virtual` so Moq can subclass.** Rejected: modifies `UtilitiesCS` public behavior surface for the benefit of
  one consumer's tests, and `SortAsync(IList<>)`'s body would still be the thing under test.
- **Constructor-parameter injection in the `EfcHomeControllerDependencies` style.** Rejected for `EfcDataModel`
  specifically: `EfcHomeControllerDependencies.CreateDataModelWithFactory` (`:145–173`) and the
  `dataModelFactory`/`asyncDataModelFactory` delegates pin the two `EfcDataModel` constructor signatures. Adding
  optional constructor parameters would change delegate compatibility and force edits in F8's file. Property
  injection with `??=` defaults is additive and leaves both constructor signatures untouched.
- **STA `*.StaTests.cs`.** Not required anywhere in this file. `EfcDataModel` constructs no WinForms control;
  the only UI touch is `MessageBox.Show`, which S4 removes from the test path. Record explicitly: **the STA
  last-resort clause does not apply to `EfcDataModel.cs`.**

### 5.4 File-size effect

`EfcDataModel.cs` is 397 lines; the 500-line ceiling leaves 103 lines of headroom. Estimated net additions:
S1 ≈ 3, S2 ≈ 10 (declarations + `??=` defaults in both ctors), S3 ≈ 9, S4 ≈ 2, S5 ≈ 2, S6 ≈ +12 net (extracted
bodies replace inline expressions). Projected ≈ **435–450 lines**, within the ceiling. Contingency if the plan's
final edits exceed ~480: add `partial` to the class declaration (a one-word additive edit) and move the seam
declarations to `QuickFiler/Controllers/EfcDataModel.Seams.cs`, with a `<Compile Include>` entry in
`QuickFiler/QuickFiler.csproj` — this is exactly the precedent `FolderPredictor.IFolderSearchHandler.cs:5–8`
documents. Flag in `spec.md` that this adds a 122nd compiled file for F16 to account for.

## 6. Enumerated test cases

Categories: P = positive, I = invalid-input, B = boundary, E = error-handling, S = state-transition,
C = concurrency/ordering. Every case is deterministic, mock/fake-based, constructs no live form, shows no
popup, touches no filesystem or external process, uses no `Thread.Sleep`/`Task.Delay`/wall-clock wait, and
creates no temporary file. Framework: MSTest + Moq + FluentAssertions, Arrange–Act–Assert.

**Target file A — `QuickFiler.Test/Controllers/EfcDataModel.TestSupport.cs`** (no `[TestMethod]`; shared
support). Contains: `CreateUninitialized<T>()`; `SetPrivateField(EfcDataModel, string, object)`;
`FakeApplicationGlobals` / `FakeFileSystemFolderPaths` (real `ConcurrentDictionary` for `SpecialFolders`,
`Mock<IOlObjects>` for `ArchiveRootPath`/`App`); `FakeFolderSearchHandler : IFolderSearchHandler` (copied in
spirit from `QfcItemController.FolderSuggestionsTests.cs:30–45`, recording the arguments `FindFolder` receives);
`CreateResolverWith(Pair<List<MailItemHelper>> info, Pair<IList<MailItem>> items, MailItemHelper helper)`
built via `FormatterServices.GetUninitializedObject(typeof(ConversationResolver))` plus property assignment
(`ConversationInfo`/`ConversationItems` are `{ get; set; }` per `IConversationResolver.cs:14–15`; `Pair<T>` is a
public struct with a public `(sameFolder, expanded)` constructor, `ConversationResolver.cs:18–28`).

### Target file B — `QuickFiler.Test/Controllers/EfcDataModelPureLogicTests.cs`

| # | Test method | Member | Cat | Arrange / Act / Assert sketch |
|---|---|---|---|---|
| 1 | `BuildSearchPattern_WithNonEmptyText_WrapsValueInWildcards` | `BuildSearchPattern` (S6) | P | A: `"invoice"`. Act: call. Assert: `"*invoice*"`. |
| 2 | `BuildSearchPattern_WithEmptyString_ReturnsEmptyStringUnwrapped` | `BuildSearchPattern` | B | A: `""`. Assert: `""` — the `searchText != ""` guard's false arm. |
| 3 | `BuildSearchPattern_WithNull_ProducesBareWildcardPair` | `BuildSearchPattern` | I | A: `null`. Assert: `"**"`, locking today's behavior (`null != ""` is true, so `"*" + null + "*"`). Comment must state this is a characterization test, not an endorsement. |
| 4 | `ShouldSaveAttachments_ForTrashToDeleteFolder_ReturnsFalseEvenWhenRequested` | `ShouldSaveAttachments` (S6) | B | A: `("Trash to Delete", true)`. Assert: `false`. |
| 5 | `ShouldSaveAttachments_ForOrdinaryFolder_ReturnsRequestedValue` | `ShouldSaveAttachments` | P | A: `("Archive\\Finance", true)` and `(…, false)`. Assert: mirrors the request. |
| 6 | `StripAncestorPrefix_WhenRemainderStartsWithBackslash_RemovesAncestorAndSeparator` | `StripAncestorPrefix` (S6) | P | A: `("\\\\Archive\\\\Finance", "\\\\Archive")`. Assert: `"Finance"`. |
| 7 | `StripAncestorPrefix_WhenRemainderHasNoLeadingBackslash_LeavesRemainderIntact` | `StripAncestorPrefix` | B | A: ancestor equals the full path prefix with no separator left. Assert: no leading character removed. |
| 8 | `StripAncestorPrefix_WhenAncestorNotPresentInPath_ReturnsPathUnchanged` | `StripAncestorPrefix` | I | A: `("\\\\Other\\\\Folder", "\\\\Archive")`. Assert: input returned unchanged. |

### Target file C — `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`

| # | Test method | Member | Cat | Arrange / Act / Assert sketch |
|---|---|---|---|---|
| 9 | `InitFolderHandlerAsync_WithNoFolderListAndNoMailInfo_UsesEmptyPredictorFactory` | `InitFolderHandlerAsync` (S2) | P | A: uninitialized model, `_conversationResolver = null`, `FolderPredictorEmptyFactory` records the call and returns a sentinel `FolderPredictor`; `FolderPredictorInitializer` throws if called. Act: `await InitFolderHandlerAsync()`. Assert: empty factory called once with the injected globals; initializer never called; `FolderHelper` is the sentinel. |
| 10 | `InitFolderHandlerAsync_WithNoFolderListAndMailInfo_InitializesFromFieldWithMailInfo` | `InitFolderHandlerAsync` | P | A: resolver whose `MailHelper` is a stub `MailItemHelper`; initializer records `(globals, item, options)`. Assert: `options == FromField`, `item` same as `MailInfo`, empty factory never called, `FolderHelper` assigned. |
| 11 | `InitFolderHandlerAsync_WithFolderList_InitializesFromArrayOrString` | `InitFolderHandlerAsync` | P | A: `folderList = new[]{"A","B"}`. Assert: `options == FromArrayOrString`, `item` same as the array, `FolderHelper` assigned. |
| 12 | `InitFolderHandlerAsync_WithAlreadyCancelledToken_ThrowsWithoutInvokingFactory` | `InitFolderHandlerAsync` | C | A: `_token` set (reflection) from an already-cancelled `CancellationTokenSource`; factories throw if called. Act/Assert: `await act.Should().ThrowAsync<TaskCanceledException>()`; factories never invoked. No timers, no waits — `Task.Run` with a pre-cancelled token completes as cancelled synchronously. |
| 13 | `InitFolderHandlerAsync_WhenFactoryThrows_PropagatesAndLeavesFolderHelperUnchanged` | `InitFolderHandlerAsync` | E | A: initializer throws `InvalidOperationException`; `_folderHelper` pre-seeded with a sentinel. Assert: exception surfaces; `FolderHelper` still the pre-seeded sentinel. |
| 14 | `FolderHelper_AfterInitialization_ReturnsAssignedPredictor` | `FolderHelper` get/set (S7) | P | A: uninitialized model, `_folderHelper` set by reflection. Assert: getter returns the same instance. Covers 172–176. |
| 15 | `FindMatches_WithNonEmptySearchText_PassesWildcardPatternToHandler` | `FindMatches` (S1) | P | A: `FolderSearchOverride` = recording `FakeFolderSearchHandler`; `_mail` = `Mock<MailItem>`. Act: `FindMatches("invoice")`. Assert: recorded `searchString == "*invoice*"`. |
| 16 | `FindMatches_PassesFixedFlagsAndCurrentMailAsObjItem` | `FindMatches` | P | Same arrange. Assert: `reloadCTFStagingFiles == false`, `recalcSuggestions == false`, `objItem` same instance as `Mail`. |
| 17 | `FindMatches_WithEmptySearchText_PassesEmptyPatternThrough` | `FindMatches` | B | Act: `FindMatches("")`. Assert: recorded `searchString == ""`. |
| 18 | `FindMatches_ReturnsHandlerResultUnmodified` | `FindMatches` | P | A: fake returns `["a","b"]`. Assert: returned array equals `["a","b"]` and is the same reference. |
| 19 | `RefreshSuggestions_InvokesRefreshActionWithCurrentMail` | `RefreshSuggestions` (S5) | P | A: `RefreshSuggestionsAction` records its argument; `_mail` = mock. Act/Assert: invoked exactly once with the same `MailItem` instance. |

### Target file D — `QuickFiler.Test/Controllers/EfcDataModelSelectionTests.cs`

| # | Test method | Member | Cat | Arrange / Act / Assert sketch |
|---|---|---|---|---|
| 20 | `MailInfo_WhenConversationResolverIsNull_ReturnsNull` | `MailInfo` | B | A: uninitialized model, `_conversationResolver = null`. Assert: `MailInfo` is null. Covers the null arm of line 232. |
| 21 | `MailInfo_WhenConversationResolverPresent_ReturnsItsMailHelper` | `MailInfo` | P | A: resolver with a stub `MailHelper`. Assert: same instance. Covers the non-null arm. |
| 22 | `Mail_WhenBackingFieldNull_ReturnsFirstSelectedMailItem` | `Mail` / `TryGetFirstInSelection` | P | A: `Mock<Selection>` with `Count == 1` and `x => x[1]` returning a `Mock<MailItem>`; `Mock<Explorer>.Selection`; `Mock<Application>.ActiveExplorer()`; `Mock<IOlObjects>.App`. Indexer-mocking precedent: `columns.Setup(x => x[index + 1])`, `EfcDataModelTests.cs:372`. Assert: `Mail` is the mocked item. Covers 239–241. |
| 23 | `Mail_WhenSelectionIsEmpty_ReturnsNull` | `TryGetFirstInSelection` | B | Same chain with `Count == 0`. Assert: null. Covers 244–245. |
| 24 | `Mail_WhenFirstSelectedItemIsNotAMailItem_ReturnsNull` | `TryGetFirstInSelection` | I | `selection[1]` returns a non-`MailItem` object; the `as MailItem` cast yields null. Assert: null. |
| 25 | `Mail_WhenExplorerAccessThrows_ReturnsNullWithoutPropagating` | `TryGetFirstInSelection` | E | `Mock<IOlObjects>.App` set up to throw. Assert: `Mail` is null, no exception escapes. *No line-coverage delta (248–250 already covered) — included for UT2 scenario completeness.* |
| 26 | `PackageItems_WithMoveConversationTrue_ReturnsSameFolderConversationItems` | `PackageItems` | P | A: resolver with `ConversationItems = new Pair<IList<MailItem>>(sameFolder: [m1,m2], expanded: [m1,m2,m3])`. Assert: result equals `[m1,m2]`. |
| 27 | `PackageItems_WithMoveConversationFalse_ReturnsSingletonOfCurrentMail` | `PackageItems` | P | A: `_mail = m1`. Assert: single element, same instance as `Mail`. |

### Target file E — `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`

| # | Test method | Member | Cat | Arrange / Act / Assert sketch |
|---|---|---|---|---|
| 28 | `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutInvokingSorter` | `MoveToFolderAsync(string,…)` | I | A: `_conversationResolver = null`; `SortAsyncAction` throws if called. Assert: result `false`; action never invoked. Covers 266–268. |
| 29 | `MoveToFolderAsync_WhenOneDriveSpecialFolderMissing_ReturnsFalseWithoutInvokingSorter` | same | E | A: fake globals with an empty `SpecialFolders` dictionary. Assert: `false`; `SortAsyncAction` never invoked. Covers 276–279. |
| 30 | `MoveToFolderAsync_ForTrashToDeleteFolder_SuppressesAttachmentSaving` | same | B | A: `folderpath = "Trash to Delete"`, `saveAttachments: true`; capture the `EmailFilerConfig`. Assert: `config.SaveAttachments == false`. |
| 31 | `MoveToFolderAsync_ForOrdinaryFolder_HonoursRequestedAttachmentFlag` | same | P | A: `folderpath = "Archive\\Finance"`, `saveAttachments: true`. Assert: `config.SaveAttachments == true`. |
| 32 | `MoveToFolderAsync_WithMoveConversationTrue_PassesSameFolderConversationHelpers` | same | S | A: resolver whose `ConversationInfo.SameFolder` is `[h1,h2]`. Assert: the helper list handed to `SortAsyncAction` equals `[h1,h2]`. |
| 33 | `MoveToFolderAsync_WithMoveConversationFalse_PassesOnlyCurrentMailInfo` | same | P | Assert: helper list is a single element, the resolver's `MailHelper`. |
| 34 | `MoveToFolderAsync_BuildsConfigFromRequestAndGlobals` | same | P | Assert on the captured `EmailFilerConfig`: `SaveMsg`, `SavePictures`, `DestinationOlStem == folderpath`, `Globals` same instance, `OlAncestor == Ol.ArchiveRootPath`, `FsAncestorEquivalent == SpecialFolders["OneDrive"]`. |
| 35 | `MoveToFolderAsync_WhenSorterSucceeds_ReturnsTrue` | same | P | `SortAsyncAction` returns `Task.FromResult(true)`. Assert: `true`. |
| 36 | `MoveToFolderAsync_WhenSorterReportsFailure_ReturnsFalse` | same | E | `SortAsyncAction` returns `Task.FromResult(false)`. Assert: `false`. |
| 37 | `MoveToFolderAsync_FolderOverload_StripsAncestorAndLeadingSeparatorBeforeDelegating` | `MoveToFolderAsync(MAPIFolder,…)` | P | A: `Mock<MAPIFolder>` with `FolderPath = "\\\\Archive\\\\Finance"`, `olAncestor = "\\\\Archive"`; `SortAsyncAction` captures `config.DestinationOlStem`. Assert: `"Finance"`. Covers 343–348. |
| 38 | `MoveToFolderAsync_FolderOverload_WhenInnerMoveFails_ShowsFailureMessageWithStrippedPath` | same | E | A: `MoveFailureMessageAction` records text; inner move forced false via empty `SpecialFolders`. Assert: recorded text is `"Cannot move to folderpath Finance"`. Covers 356–359 with no popup. |
| 39 | `MoveToFolderAsync_FolderOverload_WhenInnerMoveSucceeds_DoesNotShowFailureMessage` | same | P | Assert: `MoveFailureMessageAction` never invoked. |
| 40 | `OpenOlFolderAsync_WhenOneDriveMissing_DoesNotInvokeOpenAction` | `OpenOlFolderAsync` | E | A: empty `SpecialFolders`; `OpenOlFolderAction` throws if called. Assert: completes; action never invoked. Closes the missing arm of the 50% branch at line 300. |
| 41 | `OpenOlFolderAsync_WithOneDrivePresent_InvokesOpenActionWithResolvedConfig` | same | P | Assert: action invoked once; captured config has `DestinationOlStem`, `OlAncestor`, `FsAncestorEquivalent`, `Globals` as expected. Covers 305–314. |
| 42 | `OpenFsFolderAsync_WhenOneDriveMissing_DoesNotInvokeOpenAction` | `OpenFsFolderAsync` | E | As #40, for line 319. |
| 43 | `OpenFsFolderAsync_WithOneDrivePresent_InvokesOpenActionWithResolvedConfig` | same | P | As #41, covers 323–332. |

### Target file F — extends `QuickFiler.Test/Controllers/EfcDataModelTests.cs` (existing, 409 lines)

`EfcDataModelTests.cs` is 409 lines, of which 220–407 is COM mock scaffolding. It has **~90 lines of headroom**
against the 500-line limit. Only the two contract tests below are added there, because they need the existing
`CreateGlobals`/`CreateMailItem` scaffolding; everything else goes to the new files B–E.

| # | Test method | Member | Cat | Arrange / Act / Assert sketch |
|---|---|---|---|---|
| 44 | `CreateAsync_WithNullGlobals_ThrowsArgumentNullException` | `CreateAsync` guard, line 96 | I | Act: `CreateAsync(null, [mail], cts, token, false)`. Assert: `ArgumentNullException` (via `ThrowIfNull`, `UtilitiesCS/Extensions/NullExtensions.cs:16`). *No `EfcDataModel` line delta — line 96 is already covered; included for UT2 negative-flow completeness.* |
| 45 | `CreateAsync_WithEmptyMailItemList_Throws` | `CreateAsync` guard, line 97 | I | Act: `CreateAsync(globals, new List<MailItem>(), …)`. Assert: throws (via `ThrowIfNullOrEmpty`, `NullExtensions.cs:56`). Same coverage caveat. |

**Concurrency/ordering category coverage** is satisfied by the *existing*
`CreateAsync_StagesSnapshotLoadBeforeBackgroundInitialization` (`EfcDataModelTests.cs:24`, which mutates the
live selection list mid-flight and asserts the snapshot held) plus new case #12. No new ordering test is
proposed — do not duplicate the existing one.

**Projected result (inference).** Cases 1–43 target all 126 measured-uncovered lines. The expected residual is
the five one-line production default lambdas in S2/S3/S5, which only execute outside tests. Projected line
coverage for `EfcDataModel.cs` is **> 90%**, comfortably above the 80% floor and above the >= 90% new/changed-code
target in `CLAUDE.md` § UT2. This projection must be replaced by F1's harness output before any acceptance
criterion is checked off.

## 7. Mechanics the plan must not omit

1. **`QuickFiler.Test/QuickFiler.Test.csproj` uses explicit `<Compile Include>` items** (legacy non-SDK project;
   see `:58–69`). Each new test file (A–E) requires its own `<Compile Include="Controllers\…" />` entry, or it
   silently will not build.
2. If the S-seams push `EfcDataModel.cs` past ~480 lines and the partial split contingency is used,
   `QuickFiler/QuickFiler.csproj` needs a matching entry next to `:291`.
3. Coverage evidence goes to `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/evidence/qa-gates/`
   per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Do not write to `artifacts/`.
4. Toolchain order is fixed: `csharpier .` → analyzer msbuild → nullable msbuild → `vstest.console.exe … /EnableCodeCoverage`,
   restarting from step 1 on any failure or file change.
5. `SortEmail.Cleanup_Files()` (line 294) remains in the covered path. It mutates static state but is proven
   non-throwing (`UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:175`). It resets rather than accumulates,
   so test independence holds; note it in the policy audit as a known static touch rather than leaving it silent.

## 8. Risks and open questions

| # | Risk / question | Impact | Handling |
|---|---|---|---|
| R1 | **F1's ledger may classify `EfcDataModel.cs` differently.** The file carries no `[ExcludeFromCodeCoverage]` and is not designer-generated, form-derived, or a VSTO lifecycle class, so `testable` is the only defensible classification under the irreducible-remainder standard. | Low | Plan assumes `testable`. If F1 rules otherwise, escalate — a ratified exemption on a file this seamable would be inconsistent with the epic's own §1. |
| R2 | **F1's harness does not exist yet**, so the 80% figure cannot be verified during planning. | Medium | The measured 2026-08-06 Cobertura report is used as the *baseline*; F1's harness output is the *acceptance* evidence. Do not close ACs on the baseline. |
| R3 | **Cross-child contract.** All seams are additive `internal` members plus internal restructuring of three method bodies. The nine consumer call sites in F8/F9 files (§1) compile unchanged. | Low | No `spec.md` cross-child breaking-change note is required as designed. If planning finds a case where an existing member must change shape, that becomes a `spec.md` cross-child note — do not edit F8/F9 files. |
| R4 | **`FolderScorer.LoadFromField` behavior is inferred, not read.** It is the reason the `FromField` branch is seamed rather than exercised end-to-end. | Low | The seam is correct regardless: even if `LoadFromField` turned out COM-free, exercising it would be testing `FolderPredictor`, which belongs to `UtilitiesCS.Test`. |
| R5 | **`PackageItems(bool)` is dead code.** Cases 26–27 cover it rather than delete it. | Low | Covering is the conservative choice under "no behavior change". Record deletion as an explicit option in `spec.md`; if taken, it removes 7 lines from the denominator and both test cases. |
| R6 | **File-size headroom.** Projected 435–450 lines is within 500 but not generous. | Medium | Measure after each atomic task. Contingency in §5.4. |
| R7 | **Existing `LoadConversationInfoAsync_…` test in `EfcDataModelTests.cs:83` is really a `ConversationResolver` test** (F4's file). | Low | Leave it. Moving it would create an F4/F5 conflict for no coverage gain. |
| R8 | **`Mock<Selection>` indexer support** (case 22) is assumed workable from the `Mock<Columns>` indexer precedent at `EfcDataModelTests.cs:372`. `Selection`'s indexer is `this[object Index]`, so the setup must be `x => x[1]` with the boxed `int`. | Low–Medium | If Moq cannot intercept the `Selection` indexer, fall back to a hand-written stub implementing `Selection`, or seam `TryGetFirstInSelection` behind an `internal Func<MailItem> FirstSelectedItemProvider`. Decide at implementation time; do not pre-commit the fallback. |
| R9 | **Concurrent QuickFiler work on `main`.** epic.md "Known Conflict Risks" names #400 (F13 territory) and #424 (F7/F2 territory). Neither touches `EfcDataModel.cs`; the #424 branch's committed coverage report is *consumed* here but its code changes are elsewhere. | Low | No coordination needed for this file. |
