# quickfiler-helper-classes-coverage — Spec

- **Issue:** #434
- **Parent (optional):** epic `quickfiler-per-file-coverage` (issue #136), child F4, wave 1, band C3
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T23-10
- **Status:** Draft
- **Version:** 1.0

## Overview

Issue #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to reach
at least 80% line coverage, or to sit on an explicitly ratified classification in the epic's ledger.
This feature is child F4: the thirteen files under `QuickFiler/Helper Classes/` plus
`QuickFiler/Interfaces/IEmailMoveMonitor.cs` — 14 files, roughly 2,860 lines. All fourteen are
confirmed as `<Compile Include>` entries in `QuickFiler/QuickFiler.csproj`, and a grep for
`[ExcludeFromCodeCoverage]` across the set returns no match, so F4 inherits none of the epic's 33
disputed attributes and must add none.

**The change is overwhelmingly tests-only.** No new production file is created, no production file is
deleted, and `QuickFiler/QuickFiler.csproj` requires no edit. The production diff is limited to five
additive `internal` seams across three files (see *API / CLI Surface*); ten of the fourteen files
receive no production change at all.

### Corrections to the inherited draft

Three statements carried into this feature folder from the potential-entry draft are inaccurate and
are superseded here:

1. **`cInfoMail.cs` has no test presence and touches no Outlook Interop.** Its only active content is
   8 `using` directives (`cInfoMail.cs:1-7`, `:10`); lines `:13-231` are entirely comment lines,
   including `//namespace QuickFiler` at `:13` and `//    public class cInfoMail` at `:16`. The file
   declares no namespace and no type. `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs` targets
   `UtilitiesCS.MailItemHelper` (`MailItemInfoTests.cs:120-123`), not `cInfoMail`, and both of its
   test-method bodies are commented out (`:125-138`, `:140-168`), so it contributes zero coverage to
   any F4 file.
2. **The theme helpers do not need seam extraction.** The seams already exist and were ratified by
   the issue #236 refactor: `QfcThemeControlSet` (`QfcThemeControlSet.cs:12`) is the host-neutral
   value object, and `internal static Dictionary<string, Theme> SetupThemes(QfcThemeControlSet)`
   (`QfcThemeHelper.cs:96`) is the pure entry point reachable from `QuickFiler.Test` via
   `InternalsVisibleTo` (`QuickFiler/Properties/AssemblyInfo.cs:5`). `TlpCellSnapShot.ApplyState`
   already takes `IContainerControlLocal` (`TlpCellSnapShot.cs:192`), a seam introduced by a prior
   de-exemption cycle and documented at `QuickFiler.Test/Helper Classes/TlpCellSnapShotTests.cs:11-19`.
   `EfcThemeHelper.SetupThemes`/`SetupFormThemes` touch no WinForms API at all; control mutation lives
   in `UtilitiesCS`'s `ThemeControlGroup.ApplyTheme*` (`ThemeControlGroup.cs:231-296`).
3. **No injected clock is required anywhere in F4.** A grep of `QuickFiler/Helper Classes/` for
   `DateTime.Now`, `DateTime.UtcNow`, `Thread.Sleep`, `Task.Delay`, and `Random.Shared` returns no
   match. No banned-API finding exists in any of the 14 production files, and no file has any time
   dependence, so no `System.TimeProvider` seam is warranted. The two `Stopwatch` uses in the
   conversation cluster (`ConversationResolver.cs:221`, `:231`;
   `ConversationResolver.Loading.cs:236`, `:256`) feed log-message interpolation only; no control-flow
   decision reads elapsed time.

### File dispositions

| Disposition | Files (line counts) |
| --- | --- |
| **testable, target >= 80%** (10) | `EfcThemeHelper.cs` (499), `QfcThemeHelper.cs` (375), `QfcThemeControlSet.cs` (110), `TlpCellSnapShot.cs` (223), `ConversationResolver.cs` (358), `ConversationResolver.Loading.cs` (329), `ViewerQueueCore.cs` (161), `ItemViewerQueue.cs` (123), `EfcViewerQueue.cs` (101), `EmailMoveMonitor.cs` (262) |
| **zero coverable lines** (4) | `IConversationResolver.cs` (33), `IEmailMoveMonitor.cs` (39), `QfEnums.cs` (16), `cInfoMail.cs` (231) |

The four zero-coverable-line files are **not** `ratified-exempt`. That classification, per epic
`Shared Design` §1 and `CLAUDE.md` § UT2, presumes coverable-but-untestable lines and requires an
irreducible-remainder argument. These four have no coverable lines at all, so the exemption test has
no subject. They require a distinct F1-ledger classification — `no-coverable-lines` /
`no-executable-code` / `interface-only, omitted from measurement` — anchored in
`.claude/rules/general-unit-test.md` § Coverage Requirements ("Type-only / interface-only modules
with no executable behavior may be omitted from coverage measurement"). `QfEnums.cs`'s absence from
the committed #424 Cobertura artifacts was verified empirically: a search of
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
and the sibling baseline for `filename="QuickFiler\Helper Classes\QfEnums.cs"` returns no match,
while every other F4 file does emit a `<class>` element (for example `:2213` `EfcViewerQueue.cs`,
`:2394` `ItemViewerQueue.cs`, `:2614` `QfcThemeControlSet.cs`, `:3771` `ViewerQueueCore.cs`).

### Projected coverage

Theme cluster ~100% line coverage (`EfcThemeHelper.cs`, `QfcThemeHelper.cs`, `QfcThemeControlSet.cs`,
`TlpCellSnapShot.cs`); the `ConversationResolver` pair ~95%; `EmailMoveMonitor.cs` >= 95%; the viewer
queues high, with 7 irreducible lines in `ItemViewerQueue.cs` (`:21`, `:27`, `:88`, `:90` dereference
`UiThread.Dispatcher`; `:104-106` is `new ItemViewer()`), 5 in `EfcViewerQueue.cs` (`:20`, `:67`,
`:82-84`), and 0 in `ViewerQueueCore.cs`. **No file in the F4 set requires a coverage exemption
request.** All ten testable files are expected to be classified `testable` in F1's ledger.

## Behavior

Raise each of the ten testable files to at least 80% per-file line coverage without changing any
observable QuickFiler behaviour, and record the four zero-coverable-line files in F1's ledger rather
than authoring unsatisfiable tests for them.

The work is delivered as **255 individually enumerated test cases**, distributed as follows and
enumerated case-by-case in the fifteen research artifacts under `research/`:

| Cluster | Cases | Source artifact |
| --- | --- | --- |
| Theme / layout | 109 (37 + 20 + 25 + 27) | `01-EfcThemeHelper.md`, `02-QfcThemeHelper.md`, `03-QfcThemeControlSet.md`, `04-TlpCellSnapShot.md` |
| Conversation resolution | 71 (29 + 42) | `05-ConversationResolver.md`, `06-ConversationResolver.Loading.md` |
| Viewer queues | 56 (20 + 19 + 17) | `09-ViewerQueueCore.md`, `10-ItemViewerQueue.md`, `11-EfcViewerQueue.md` |
| Move monitor | 19 | `13-EmailMoveMonitor.md` |
| Declaration-only files | 0 (ledger classification only) | `07-IConversationResolver.md`, `08-cInfoMail.md`, `12-QfEnums.md`, `14-IEmailMoveMonitor.md` |

Each artifact records the cases it deliberately **excludes** as duplicates of existing coverage,
citing the existing test by `file:line`. Those exclusions are binding: the plan must not re-author
them.

**No STA test is required anywhere in F4.** This is an explicit scope decision, not an omission. The
epic's STA last-resort clause (`Shared Design` §3) is available but is not invoked, because in every
case the seam hierarchy is satisfied at rank 1 or 2 before STA becomes relevant: theme tests drive
in-memory `Button`/`CheckBox`/`Label`/`TableLayoutPanel` instances that create no window handle
(precedent `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:167-194`, a plain `[TestClass]`);
`WebView2` and `ItemViewer` instances come from `FormatterServices.GetUninitializedObject`
(`QfcThemeHelperTests.cs:331-335`, `ViewerQueueStaticWrapperTests.cs:330-334`); `EmailMoveMonitor`'s
`_marshalToSta` delegate replaces STA dispatch with a synchronous pass-through
(`EmailMoveMonitorTests.cs:63-70`); and `ConversationResolver`'s dispatcher dependency is replaced by
the pre-existing `UtilitiesCS.Threading.IUiDispatcher` interface. `QuickFiler.Test` therefore remains
free of `[STATestClass]`, `[STATestMethod]`, and `*.StaTests.cs`, and F4 introduces no
`.runsettings` file.

## Inputs / Outputs

### Inputs

- **The 14 production files** listed in *Overview*, read as-is. Ten are the coverage target; four are
  ledger-classification subjects.
- **The 8 existing test files** in `QuickFiler.Test/Helper Classes/` (2,425 lines, 58 `[TestMethod]`
  declarations), which establish the non-duplication baseline: `ConversationResolverTests.cs` (578),
  `EmailMoveMonitorTests.cs` (314), `MailItemInfoTests.cs` (170), `QfcThemeHelperTests.cs` (463),
  `TlpCellSnapShotTests.cs` (122), `TlpCellStatesTests.cs` (247), `ViewerQueueCoreTests.cs` (195),
  `ViewerQueueStaticWrapperTests.cs` (336).
- **F1's per-file coverage harness**, derived from the Cobertura output of
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. It is the sole per-file measurement mechanism for
  this child.
- **F1's ledger** at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`, the
  authority for every classification.
- **Fifteen research artifacts** under `research/` (`00-cluster-overview.md`, `01-` through `14-`),
  which are the authoritative per-file inputs to the atomic plan.

### Outputs

- **New MSTest files under `QuickFiler.Test/Helper Classes/`** (13): `EfcThemeHelperTests.cs`,
  `QfcThemeHelperTests.TestSupport.cs`, `QfcThemeControlSetTests.cs`,
  `ViewerQueueCoreValidationTests.cs`, `ItemViewerQueueTests.cs`, `EfcViewerQueueTests.cs`,
  `PairTests.cs`, `ConversationResolverLifecycleTests.cs`, `ConversationResolverLoadingTests.cs`,
  `ConversationResolverNotificationTests.cs`, `EmailMoveMonitorEventHandlerTests.cs`,
  `EmailMoveMonitorAsyncUnhookTests.cs`, `EmailMoveActionTests.cs`.
- **Extensions to existing test files** (5): `QfcThemeHelperTests.cs`, `TlpCellStatesTests.cs`,
  `TlpCellSnapShotTests.cs`, `ViewerQueueCoreTests.cs`, `ViewerQueueStaticWrapperTests.cs`
  (additive only). `EmailMoveMonitorTests.cs` is deliberately left unmodified so that issue #426
  (`emailmovemonitor-rejected-item-hook-retention`, promoted 2026-08-07, no active folder yet) can
  extend it later without a rebase conflict.
- **`<Compile Include>` additions** to `QuickFiler.Test/QuickFiler.Test.csproj`, one per new test
  file, inserted alphabetically inside the existing contiguous `Helper Classes\` block at lines
  **158-165**.
- **Per-file numeric coverage evidence** under `<FEATURE>/evidence/qa-gates/`, produced by F1's
  harness, per the canonical evidence locations in
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
- **Four ledger-classification requests** to F1 for the zero-coverable-line files.

### Config keys and defaults

None. This feature introduces no configuration key, no environment variable, and no CLI flag. The
repository-root `coverage.config` is explicitly out of bounds.

### Versioning and backward-compatibility constraints

No public API changes. All five seams are `internal` and additive; every existing call site — in
production and in sibling-owned test code — compiles byte-identically. The binding compatibility pins
are enumerated in *Constraints & Risks*.

## API / CLI Surface

**There is no CLI surface.** This feature adds no command, flag, or executable entry point. What
follows is the complete additive `internal` seam surface. No `public` API is added, removed, renamed,
or re-signed anywhere in the F4 file set.

### `QuickFiler/Helper Classes/ConversationResolver.cs` — three additive seams

1. **UI dispatch (interface seam, rank 1 — reuses an existing interface).**

   ```csharp
   internal UtilitiesCS.Threading.IUiDispatcher UiDispatcher { get; set; }
       = new UtilitiesCS.Threading.WpfUiDispatcher();
   ```

   Declared in the Properties region near `ConversationResolver.cs:295`; consumed by replacing
   `UiThread.Dispatcher.InvokeAsync(...)` at `ConversationResolver.Loading.cs:150` and `:320`. The
   interface already exists at `UtilitiesCS/Threading/IUiDispatcher.cs:15` with the production adapter
   `UtilitiesCS/Threading/WpfUiDispatcher.cs:17`; the parameterless adapter constructor passes a
   **lazy** `() => UiThread.Dispatcher` provider (`WpfUiDispatcher.cs:24-25`), so constructing the
   default touches nothing — proven at `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:26`. This
   replaces a static `UiThread.Dispatcher` dereference that returns `null` in a unit-test process
   (`UtilitiesCS/Threading/UiThread.cs:135-140`) and whose initialisation would `Show()` a live
   WinForms form (`UiThread.cs:48-79`).

2. **`MailItemHelper` projection factories (injectable delegate seam, rank 2).**

   ```csharp
   internal Func<DataFrame, long, IApplicationGlobals, CancellationToken, MailItemHelper> HelperFromDf
       { get; set; } = MailItemHelper.FromDf;
   internal Func<DataFrame, long, IApplicationGlobals, CancellationToken, bool, Task<MailItemHelper>> HelperFromDfAsync
       { get; set; } = MailItemHelper.FromDfAsync;
   ```

   Defaults are the existing static method groups
   (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs:18`, `:88`), so behaviour is
   unchanged. Consumed at `ConversationResolver.Loading.cs:61` and `:98-104`. These statics
   dereference `appGlobals.Ol.NamespaceMAPI` and resolve a live item by EntryID, which is why the
   multi-row projection path is currently unreachable in tests.

3. **Awaitable core extraction for the `async void` handler (extract-to-core, rank 1 shape).**

   ```csharp
   public async void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e) =>
       await HandlePropertyChangedCoreAsync(e).ConfigureAwait(false);

   internal async Task HandlePropertyChangedCoreAsync(PropertyChangedEventArgs e) { /* current body */ }
   ```

   The existing body, including the `catch (OperationCanceledException)` at
   `ConversationResolver.Loading.cs:314`, moves wholesale into the core, so cancellation is swallowed
   at the same point and `FullyLoaded` retains `false` on cancellation. **The
   `Handler_PropertyChanged` signature is preserved**, which is load-bearing: it is bound by method
   group at `QuickFiler/Controllers/EfcDataModel.cs:69` (F5) and
   `QuickFiler/Controllers/EfcItemController.cs:667` (F9), and it is declared `void` on
   `IConversationResolver.cs:25`.

### `QuickFiler/Helper Classes/ItemViewerQueue.cs` and `EfcViewerQueue.cs` — one additive method each

```csharp
/// <summary>
/// Restores the production collaborator delegates and then rebuilds the queue core from them.
/// Intended for test setup/teardown so each test starts from a known state.
/// </summary>
internal static void ResetForTesting()
{
    ResetProductionCoreDefaultsForTesting();
    ResetCoreForTesting();
}
```

Two executable lines each. This removes an order-sensitive two-call protocol: `ResetCoreForTesting`
rebuilds the core from the *current* `Production*` delegate values
(`ItemViewerQueue.cs:77-81` → `:93-101`; `EfcViewerQueue.cs:56-60` → `:71-79`), so defaults must be
restored first. `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:15-22` gets this
right today but is easy to invert. In `EfcViewerQueue.cs` the new member is **appended after line 99**
rather than inserted mid-file, so that the line citation
`EfcViewerQueue.cs:83` in
`docs/features/potential/promoted/2026-07-16-efcviewer-breadcrumb-webview2.md:47` does not go stale.

### Seams considered and rejected (do not re-derive)

- `IEfcThemePaletteSource` / a host-neutral `EfcThemePalette` extraction — no coverage benefit, and
  the extraction would move ~430 lines out of a 499-line file during a 14-way parallel wave.
- `IItemViewerThemeSource` replacing the concrete `ItemViewer` parameter of `QfcThemeHelper.SetupThemes`
  — would change a `public` signature consumed at four F10-owned call sites
  (`QfcItemController.Initialization.cs:175`, `:209`, `:267`, `:299`).
- `ITlpCellHost` replacing `TableLayoutPanel` in `TlpCellSnapShot.SnapCell` — would require editing
  twelve F15-owned construction sites (`QuickFiler/Viewers/QfcFormViewer.cs:201-251`).
- `IViewerScheduler` replacing the three `ViewerQueueCore` delegate parameters — churn with no
  coverage gain; `.claude/rules/csharp.md:52` prefers the delegate seam for a single call path.
- An injectable `Func<Dispatcher>` accessor on either viewer-queue wrapper — buys 2-4 lines in files
  already above the floor and attaches a live `Dispatcher` to a pooled MSTest worker thread.
- `IStaMarshaller` replacing `EmailMoveMonitor`'s `_marshalToSta` delegate — pure churn; the delegate
  already achieves full isolation and is the shape the repository standardised on in issues #214/#420.
- Any new member on `IConversationResolver` or `IEmailMoveMonitor` — see *Constraints & Risks*.

## Data & State

This feature introduces no storage, no cache, no persistence, and no migration. What it must preserve
is a set of ordering, async-loading, and static-state invariants that the new tests turn into
executable documentation. These are current contract; F4 asserts them and does not change them.

### Conversation-resolution ordering and async invariants

- **Subscription is last.** `LoadAsync` subscribes `Handler_PropertyChanged` **after** `LoadDfAsync`
  (`ConversationResolver.cs:114`, `:120`, `:150`, `:156`, `:213`; comments at `:118`, `:154`), because
  the `Df` setter unconditionally raises `PropertyChanged("Df")`
  (`ConversationResolver.Loading.cs:205`) and the handler reacts to exactly that name (`:306`).
- **`loadAll == true` order is Df → ConversationInfo → ConversationItems**, strictly
  (`ConversationResolver.cs:111-113`, `:147-149`). `loadAll == false` performs Df only; conversation
  info and items stay lazy (`:119`, `:155`).
- **`LoadAsync(IEnumerable<MailItem>)` sets `ConversationInfo` and `Count` from the same materialized
  list** (`:207-212`), assigning the *same list instance* to both `SameFolder` and `Expanded`, and
  assigning `Count` **after** `LoadConversationItemsAsync`, so a consumer reading `Count` during that
  await observes the `(-1,-1)` sentinel. Helper ordering preserves input order
  (`:186-202`), which `EfcDataModel.CreateAsync` relies on (`EfcDataModel.cs:106`).
- **The two `ConversationInfo` loaders order divergently and intentionally**: the synchronous loader
  uses `OrderByDescending(ConversationID)` (`Loading.cs:62`), the async loader `OrderBy` (`:109`).
  Consumers read `ConversationInfo.Expanded` positionally (`EfcItemController.cs:1103`,
  `QfcItemController.Conversation.cs:121`), so the order is part of the contract. Tests must pin both
  and document the divergence so a future reader does not "fix" one to match the other.
- **`MailHelper` reference identity is preserved** through the pre-projection short-circuit
  (`Loading.cs:91-95`) and the post-ordering identity repair (`:112-119`), with an empty-result
  fallback assigning `[MailHelper]` (`:120-123`).
- **`ConversationInfo` is assigned before `UpdateUI` is invoked, and `UpdateUI` receives the local
  list, not the lazy property** (`:138` then `:150`) — the Bug-3 regression documented at
  `ConversationResolverTests.cs:183-202`.
- **Cancellation is checked at method entry and again immediately before the UI publish**
  (`Loading.cs:80` and `:142`; `:187`; `:233`; `ConversationResolver.cs:219`).
- **`FullyLoaded` transitions `true → false → true`** across a `"Df"` notification
  (`Loading.cs:306-315`); on `OperationCanceledException` the catch at `:314` swallows and leaves
  `FullyLoaded == false` permanently. That silent-failure shape is existing contract and must be
  locked by a test, not changed.
- **The lazy `Pair<>` properties are load-once, notify-on-first-load** via
  `Initializer.GetOrLoad(ref field, loader, callback, strict: false, _mailItem)`
  (`UtilitiesCS/HelperClasses/Initializer.cs:142-158`); when `_mailItem` is `null` the getter returns
  `default(Pair<...>)` without invoking the loader and without notifying. `Count` uses a `(-1,-1)`
  sentinel with a predicate keyed on `Expanded` (`Loading.cs:263`, `:269`), not on `default`.
- **`UpdateUI` assignment raises `PropertyChanged("UpdateUI")` unconditionally**, including when
  assigned `null` (`ConversationResolver.cs:273-278`).
- **The `#pragma warning disable CS0618` suppression at `ConversationResolver.cs:185-203` is
  intentional and narrow** and must not be widened or removed.

### Viewer-queue state invariants

- **Strictly FIFO.** The backing store is `Queue<TViewer>` (`ViewerQueueCore.cs:16`); the
  `DispatcherPriority` arguments select *when the scheduler runs the enqueue action*, never the queue
  order.
- **No capacity limit; `DequeueChunk` replenishes by the pre-call depth, not by `count`**
  (`ViewerQueueCore.cs:104`), so repeated chunk dequeues from a well-stocked queue grow it
  geometrically. Assert the exact post-condition counts; do not "fix".
- **Validation precedes mutation.** `ValidateCount` runs at `:41`, `:54`, `:72`, `:73`, `:93` before
  any enqueue or dequeue, and `Dequeue` validates both replacement counts before touching the queue.
- **`CreateWithPriority` returns `null` if the injected scheduler does not invoke the action**
  (`:131`, `:136`) — which is exactly what the production fire-and-forget
  `Dispatcher.InvokeAsync` delegate does (`ItemViewerQueue.cs:21`, `EfcViewerQueue.cs:20`).
- **`Reset` invokes the dispose delegate once per queue entry** (`:121-122`), so a pooled viewer
  enqueued twice is disposed twice. Currently unreachable in production because neither wrapper
  supplies a dispose delegate.
- **No synchronisation anywhere.** No `lock`, no `Interlocked`, no `Concurrent*` type; `Queue<T>` is
  not thread-safe.
- **Wrapper argument mapping is the wrappers' entire contract.** `ItemViewerQueue.Dequeue` always
  passes `(Render, 1, 1, ContextIdle)` (`:48-54`) and `DequeueChunk` `(Render, ContextIdle)`
  (`:59-63`); `EfcViewerQueue.BuildQueue` always passes `Background` (`:31`) and `Dequeue`
  `(CancellationToken.None, Render, 1, 2, Background)` (`:36-42`).
- **`EfcViewerQueue.Dequeue()` is uncancellable by design** — `CancellationToken.None` is hard-coded
  at `:37`, so `ViewerQueueCore`'s two cancellation guards can never fire through that façade.
- **Both wrappers hold five pieces of mutable process-global state** (four `Production*` delegate
  properties plus the `_core` field), with no `volatile`, `Interlocked`, or `lock`. Every test class
  touching either type must carry `[DoNotParallelize]` and both `[TestInitialize]` and
  `[TestCleanup]` calling the new `ResetForTesting()`, so each test starts from a known state
  regardless of execution order. `[TestCleanup]` alone gives a post-condition guarantee only; the
  policy requirement in `.claude/rules/general-unit-test.md` § Core Principles is order-independence,
  which needs a pre-condition guarantee.
- **`EfcViewerQueue`'s "no injection" accident is strictly worse than its sibling's**: because its
  blocking scheduler is inline (`:25`), a test that calls `Dequeue()` without injecting constructs a
  live `Form` (`:83`) *before* any null-dispatcher failure, whereas `ItemViewerQueue`'s
  dispatcher-bound blocking scheduler fails fast first.

### Move-monitor subscription invariants

- **Exactly one Interop event is involved**: `Folder.BeforeItemMove`, typed
  `MAPIFolderEvents_12_BeforeItemMoveEventHandler`, subscribed at `EmailMoveMonitor.cs:57` under an
  `Any(x => x.FolderEntryId == folderEntryId)` guard (`:56`) that permits at most one subscription per
  distinct folder EntryID.
- **The handler delegate instance is created once per monitor** (`:206-222`) and stored in the field
  `BeforeItemMove` (`:202`), so `+=` and `-=` always use the same delegate identity — which is what
  makes `-=` actually detach.
- **Unsubscription happens only when the removed item was the last for that folder** (`:82`, `:118`)
  or unconditionally via `UnhookAll` (`:195`).
- **A leak is reachable and must be pinned, not fixed**: `HookItem` keys on the folder EntryID read at
  hook time, while `UnhookItem` re-reads `(mail.Parent as Folder)?.EntryID` live at `:75`. If the mail
  has moved, the entry is removed at `:84` without unsubscribing, and the original folder stays
  subscribed.
- **The handler reads live COM (`x.Mail.EntryID`, `:213`) rather than the cached `MailEntryId`**,
  inconsistent with the caching contract documented at `:228-233`. Report-only.
- **`EmailMoveAction` caches both EntryIDs at construction** (`:239-240`) and never re-reads them.

## Implementation Strategy

### Scope

Tests-only for ten of the fourteen production files. Five additive `internal` members across three
files (`ConversationResolver.cs` ×3, `ItemViewerQueue.cs` ×1, `EfcViewerQueue.cs` ×1) plus four
in-place call-site substitutions inside `ConversationResolver.Loading.cs` (`:61`, `:98-104`, `:150`,
`:320`) and the `Handler_PropertyChanged` body extraction. Four files receive a ledger classification
and no code change.

### Sequencing

Work proceeds **one production file at a time**, per the #136 mandate. The atomic plan will carry:

- **one phase per production file** — fourteen phases, ordered so that the zero-conflict,
  zero-production-diff files (`QfEnums.cs`, `cInfoMail.cs`, `IConversationResolver.cs`,
  `IEmailMoveMonitor.cs`, `ViewerQueueCore.cs`, `QfcThemeControlSet.cs`) come first and the
  seam-bearing conversation cluster last;
- **one atomic task per individual test case** — 255 test-case tasks;
- **one atomic task per new test file** and **one task for the single contiguous
  `QuickFiler.Test.csproj` insertion hunk**, batched so that all fourteen `<Compile Include>` lines
  land in one commit and therefore one git hunk (fourteen rather than thirteen because
  `research/01-EfcThemeHelper.md` §9 authorises splitting `EfcThemeHelper`'s 37 cases across
  `EfcThemeHelperTests.cs` and `EfcThemeHelperFormThemesTests.cs`).

### Test-file sizing

`.claude/rules/general-code-change.md` § File Size Limit applies to test code. Two files force a
split before new cases are added:

- **`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs` is 463 of 500 lines.** Its test-support
  region (`:226-461` — `CreateControlSet`, `CreateItemViewer`, `CreateUninitialized`,
  `SetPrivateField`, `RaiseMouseEnter`/`RaiseMouseLeave`, `FakeQfcItemController`) must be extracted
  to a `partial class` continuation file `QfcThemeHelperTests.TestSupport.cs`, matching the existing
  convention at `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`. That leaves the test
  class at ~230 lines with room for the 20 new `QfcThemeHelper` cases.
- **`ViewerQueueStaticWrapperTests.cs` is 336 of 500 lines**, so the 19 `ItemViewerQueue` and 17
  `EfcViewerQueue` cases go to new dedicated files rather than being appended.

### Established techniques to reuse (do not reinvent)

- **Outlook Interop is mocked directly with Moq.** `MailItem`, `Folder`, `MAPIFolder`, `Store`,
  `Items`, `Application`, `NameSpace`, and `Conversation` are COM interfaces and are proxyable by
  Castle DynamicProxy. The enabling build setting is
  `<EmbedInteropTypes>False</EmbedInteropTypes>` at `QuickFiler.Test.csproj:270-272` and `:318-320`;
  it must stay off. No new adapter interface is needed to reach an Interop member.
  `MailItemActionsAdapter`'s own XML doc records the governing rationale
  (`QuickFiler/Interfaces/MailItemActionsAdapter.cs:5-11`).
- **COM event capture** uses `folder.SetupAdd(f => f.BeforeItemMove += It.IsAny<...>()).Callback(...)`,
  with reflection on the private field as a documented fallback. `VerifyAdd`/`VerifyRemove` on Interop
  events is already proven at `EmailMoveMonitorTests.cs:101-104`, `:120-131`.
- **Headless viewer construction** uses `FormatterServices.GetUninitializedObject`
  (`QfcThemeHelperTests.cs:331-335`, `ViewerQueueStaticWrapperTests.cs:330-334`). Call
  `GC.SuppressFinalize` on such instances inside the helper, since an uninitialised `Control` still
  inherits a finaliser whose `Dispose(bool)` reads fields that are null.
- **`SystemColors` reads are machine-theme dependent.** Assertions must compare symbolically
  (`.Should().Be(SystemColors.Control)`), never against a literal ARGB value. Precedent
  `QfcThemeHelperTests.cs:64`, `:90-93`, `:178`.
- **Never call the async overloads that route through `UiThread.Dispatcher` or `Control.Invoke`**:
  `ThemeControlGroup.ApplyTheme(bool async)` (`ThemeControlGroup.cs:212-229`),
  `Theme.SetTheme(bool async)`, `Theme.SetMailRead(bool async)`/`SetMailUnread(bool async)`
  (`Theme.cs:359-366`, `:397-404`). Use the parameterless overloads only.
- **Namespace inconsistency in `QuickFiler.Test/Helper Classes/` is not fixed by F4.** Three
  namespaces are in use (`QuickFiler.Test.HelperClasses`, `QuickFiler.Helper_Classes.Tests`,
  `Z.Unfinished.QuickFiler.Test`). New files adopt the namespace of the file they sit beside for the
  same production type.
- **Do not modify, delete, or reference `QuickFiler.Test/QuickFiler.Test.csproj.bak` or
  `QuickFiler/QuickFiler.csproj.bak`.** They are stale snapshots; the `.bak` `EmbedInteropTypes=True`
  line is a directly misleading precedent. Every csproj edit task must name the exact live path.

### Dependency changes

None. `MSTest.TestFramework` 4.3.3, `Moq` 4.20.72, `FluentAssertions` 8.10.0,
`Microsoft.Bcl.TimeProvider` 10.0.10, and `Microsoft.Extensions.TimeProvider.Testing` 10.8.0 are
already referenced by `QuickFiler.Test` (`packages.config`; `QuickFiler.Test.csproj:193-317`). No
package is added or removed.

### Logging / telemetry

None added. `ConversationResolver`'s existing log4net timing helpers
(`ConversationResolver.cs:36-58`) are exercised by tests but are not modified.

### Rollout

Not applicable — no runtime behaviour change, no feature flag, no staged deploy. The fallback path is
the unchanged production code.

## Upstream Dependency on F1

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`) is a genuine upstream contract, not
stylistic ordering. F4 consumes two F1 deliverables and defines neither:

1. **The ledger** at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` is the
   authority for any `ratified-exempt` classification and for the zero-coverable-line classification
   the four declaration-only files require. F4 requests classifications; it does not decide them, and
   it does not add `[ExcludeFromCodeCoverage]` to any file.
2. **The harness**, derived from the Cobertura output of
   `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, is the sole per-file measurement mechanism. F4
   defines no alternative and treats aggregate assembly coverage as insufficient evidence.

### Two harness requirements this research feeds back to F1

- **Per-file aggregation must union the multiple Cobertura `<class>` elements that share one
  `filename`, taking the maximum hit count per line.** `ItemViewerQueue.cs` and `EfcViewerQueue.cs`
  each emit a main class plus a compiler-generated `<>c` closure class carrying the lambda bodies;
  the same line numbers appear in both — as the *assignment* site (hit) in the main class and as the
  *lambda body* (not hit) in `<>c`. Evidence:
  `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml:25839-25880`
  (`QuickFiler.ItemViewerQueue.<>c`, `line-rate="0"`, lines 15, 21, 27, 86, 88, 90) and `:25797-25838`
  (`QuickFiler.EfcViewerQueue.<>c`, lines 14, 20, 25, 65, 67, 68). Summing class elements without
  de-duplication produces a materially harsher denominator (84.3% vs 92.2% for `ItemViewerQueue.cs`;
  82.1% vs 92.0% for `EfcViewerQueue.cs`).
- **The has-a-denominator decision must key on the `<line>` child count, never on `line-rate`.** A
  declaration-only file is represented in one of two ways depending on the emitter: absent from the
  report entirely, or present as a `<class>` with an empty `<lines/>` collection and
  `line-rate="0"` or `"1"`. If the harness defaults either representation to 0%, `QfEnums.cs`,
  `cInfoMail.cs`, `IConversationResolver.cs`, and `IEmailMoveMonitor.cs` — and the ~24 other
  declaration-only files the epic anticipates (`epic.md:112`) — appear as failures that no amount of
  test authoring can fix.

## Constraints & Risks

### Hard compatibility pins (invariants the plan must not violate)

- **`EfcViewerQueue.Dequeue()`'s parameter list is frozen.** It is bound as a **method group** to
  `Func<EfcViewer>` at `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:40` and `:112`
  (F8-owned). C# method-group conversion does not fill optional parameters, so adding one is a compile
  break in a sibling-owned file. If a token surface is ever needed, it must arrive as a **new
  overload**, never as a new optional parameter. Apply the "new overload, never a new optional
  parameter" rule uniformly across the cluster.
- **`ConversationResolver`'s constructors are called positionally.** The 5-argument form at
  `QuickFiler/Controllers/QfcItemController.Initialization.cs:382-388` (F10) and the 4-argument form
  at `QuickFiler/Controllers/EfcDataModel.cs:66` (F5). New parameters may only be **appended with
  defaults**; the positional shape of the existing parameters must not change.
- **`internal Pair<DataFrame> LoadDf()` (`ConversationResolver.Loading.cs:208`) must keep its
  signature.** It is consumed at `QuickFiler/Controllers/EfcDataModel.cs:67` (F5).
- **The two `[Obsolete(..., true)]` members at `ConversationResolver.cs:301` and `:333` are
  irreducible by compiler contract.** `error: true` makes any call site compile error CS0619, so no
  production code and no compiled test can invoke them. They are retained unchanged and recorded in
  F1's ledger as uncoverable-by-compiler-contract, distinct from `ratified-exempt`. They must not
  receive `[ExcludeFromCodeCoverage]`, and deleting them is out of scope for this child (see
  *Non-Goals*).
- **`IEmailMoveMonitor` members must not be added, renamed, or promoted, and `IDisposable` must not
  be added.** Two sibling-owned test files mock it with `MockBehavior.Strict` —
  `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs:113`, `:140`, `:203` and
  `QfcQueuePurePathsTests.cs:119` (both F2) — and a strict mock fails when the subject calls a member
  that was not explicitly set up, so adding a member breaks sibling tests even with no production call
  site change. In particular, `UnhookItemAsync` must **not** be promoted onto the interface; tests
  call it directly on the concrete `internal` class via `InternalsVisibleTo`.
- **`EmailMoveMonitor`'s `marshalToSta` parameter type, order, and default must not change.** Three
  sibling-owned call sites construct `new EmailMoveMonitor()` with no arguments:
  `QfcQueue.cs:40` (F2), `QfcDatamodel.cs:103` (F5), `QfcCollectionController.cs:78` (F11). Any
  further dependency must be appended as an additional optional parameter **after** `marshalToSta`.
- **`TlpCellStates`, `TlpCellSnapShotList`, and `TlpCellSnapShot` must be strictly additive and
  signature-preserving.** 38 sibling-owned production references exist across F2, F6, F10, F11, and
  F15. The three types must not be renamed, must not be split into separate files, and no property
  type may change.
- **`QfEnums.InitTypeEnum` must not receive `[Flags]`.** It would change observable
  `ToString()`/`Parse` behaviour across ten sibling-owned consumer files.

### Shared-file conflict surface

`QuickFiler.Test/QuickFiler.Test.csproj` declares test files by **explicit `<Compile Include>` entries
— there is no globbing** (`:57-169`). Every new test file therefore requires an edit to that file,
which all fourteen wave-1 children of epic #136 also edit. This is the single highest-probability
merge-conflict surface for this epic and the only shared file F4 touches.

Mitigation: insert all fourteen F4 entries **inside the existing contiguous `Helper Classes\` block at
lines 158-165**, in alphabetical order, in one commit (fourteen rather than thirteen because
`research/01-EfcThemeHelper.md` §9 authorises splitting `EfcThemeHelper`'s 37 cases across
`EfcThemeHelperTests.cs` and `EfcThemeHelperFormThemesTests.cs`). Siblings append to the `Controllers\`
(`:58-151`) and `Viewers\` (`:60-91`) regions, which are textually distant, so a three-way merge of
disjoint hunks inside one `<ItemGroup>` resolves cleanly in the common case.

`QuickFiler/QuickFiler.csproj` requires **no edit**, because no production file is created or deleted.

### Other risks

- **Concurrency isolation.** Thirteen sibling children run against the same integration branch. This
  child touches only the 14 production files, `QuickFiler.Test/Helper Classes/**`, the one
  `QuickFiler.Test.csproj` hunk, and its own feature folder.
- **Test-side type dependencies on sibling-owned surfaces.** The new tests reference `ItemViewer`
  (F14), `EfcViewer` (F9), `IQfcItemController` (F10), and `IItemViewer` (F14) through their existing
  public/internal surface. No sibling production file is edited. If a sibling changes one of those
  surfaces mid-wave, the result is a compile error in F4's test files — handled by the child's R1-R5
  remediation loop — not a merge conflict.
- **Determinism hazards to avoid copying.** `QuickFiler.Test/Controllers/EfcDataModelTests.cs:144-147`
  and `:173-176` use `SpinWait.SpinUntil(..., 250)`, a real wall-clock budget, in an F5-owned file.
  F4 must not copy that pattern and must not edit that file; the `HandlePropertyChangedCoreAsync`
  extraction is F4's deterministic alternative for the same assertion.
- **Third in-flight conflict risk not listed in `epic.md`.** Issue #426
  (`emailmovemonitor-rejected-item-hook-retention`, promoted 2026-08-07,
  `docs/features/potential/promoted/2026-08-07-emailmovemonitor-rejected-item-hook-retention.md:9-10`)
  has no active feature folder yet but will land in F4's territory when it does; its stated
  unit-coverage areas include "`EmailMoveMonitor` hook lifecycle" (`:65`). F4 therefore leaves
  `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` unmodified.
- **`AssemblyInitialize` constraint.** `QuickFiler.Test/SetupAssemblyInitializer.cs:18-19` calls
  `Application.EnableVisualStyles()` and `Application.SetCompatibleTextRenderingDefault(false)` once
  per assembly load. The latter throws `InvalidOperationException` if any WinForms control has already
  been created in the process, which reinforces the rule that no test constructs a live form.

## Non-Goals

- **Deleting `cInfoMail.cs`.** Deletion is behaviour-neutral — the file declares no type, so removing
  it cannot change any compiled symbol — but it buys **zero** coverage, because an empty denominator
  is empty in either state, and it would require deleting line 342 of the shared
  `QuickFiler/QuickFiler.csproj`. Deferred to a post-fan-in repository-hygiene issue that should also
  bundle `QuickFiler/QuickFiler.csproj.bak`, `QuickFiler.Test/QuickFiler.Test.csproj.bak`, and the two
  `[Obsolete(..., true)]` methods at `ConversationResolver.cs:299-356`.
- **Deleting or widening `IConversationResolver.cs`.** Deletion would remove a `public` type from a
  `public` assembly and edit the shared csproj; widening it to cover `Mail`, `MailHelper`, the
  `Parent` setter, and `LoadDf` would require edits in F5, F9, F10, and F11.
- **Fixing the latent defects enumerated in the research.** Each is out of scope for a coverage child
  bound by a no-behaviour-change acceptance criterion, and each should be promoted as its own issue
  through the promotion lifecycle rather than left as prose in a feature folder that disappears at
  merge:
  - `EmailMoveMonitor`'s leaked `BeforeItemMove` subscription when the parent folder changes between
    hook and unhook (`EmailMoveMonitor.cs:75` vs `:78` vs `:82`), and its live-COM predicate read at
    `:213` instead of the cached `MailEntryId`.
  - The unsynchronised `Queue<T>` mutated across the WPF dispatcher boundary
    (`ViewerQueueCore.cs:16` with fire-and-forget `InvokeAsync` at `ItemViewerQueue.cs:21` /
    `EfcViewerQueue.cs:20`, while `QfcQueue.cs:336` (F2) and `QfcCollectionController.cs:617`, `:958`
    (F11) dequeue from other threads).
  - `Reset`'s double-dispose of a viewer enqueued twice (`ViewerQueueCore.cs:121-122`).
  - `DequeueChunk`'s replenish-by-`originalCount` unbounded growth (`ViewerQueueCore.cs:104`).
  - The missing `[Flags]` attribute on `QfEnums.InitTypeEnum` (`QfEnums.cs:5`).
  - `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:25` (`private DateTime now = DateTime.Now;`,
    a banned symbol per `BannedSymbols.txt:1`), plus that file's misleading name and its two
    assertion-free test methods.
  - The `UpdateUI` republish asymmetry at `ConversationResolver.Loading.cs:321`, which reads the lazy
    property where `LoadConversationInfoAsync` deliberately passes the local.
- **Any edit to `coverage.config`, to `QuickFiler/QuickFiler.csproj`, or to any sibling-owned
  production or test file.**
- **Introducing STA test infrastructure**, a QuickFiler-specific `.runsettings`, a new clock
  abstraction, or any new external dependency.
- **Normalising the three test namespaces** in `QuickFiler.Test/Helper Classes/`, or splitting
  `TlpCellSnapShot.cs` into one file per type. Both are recorded as follow-ups against the cohesion
  and consistency rules, to be scheduled after the capstone F16 closes.

## Acceptance Criteria

- [ ] Each of the 10 testable files (`EfcThemeHelper.cs`, `QfcThemeHelper.cs`, `QfcThemeControlSet.cs`, `TlpCellSnapShot.cs`, `ConversationResolver.cs`, `ConversationResolver.Loading.cs`, `ViewerQueueCore.cs`, `ItemViewerQueue.cs`, `EfcViewerQueue.cs`, `EmailMoveMonitor.cs`) reaches >= 80% line coverage, verified with F1's per-file harness, with the numeric per-file result committed under `<FEATURE>/evidence/qa-gates/`.
- [ ] Each of the 4 zero-coverable-line files (`IConversationResolver.cs`, `IEmailMoveMonitor.cs`, `QfEnums.cs`, `cInfoMail.cs`) is recorded in F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` with a zero-coverable-line classification distinct from `ratified-exempt`, and none receives an `[ExcludeFromCodeCoverage]` attribute.
- [ ] No production file in scope exceeds 500 lines, and `EfcThemeHelper.cs` remains at 499 lines with no partial split.
- [ ] All new and modified tests use MSTest, Moq, and FluentAssertions, are deterministic, isolated, and order-independent, and use no temporary files, external services, live forms, or popups.
- [ ] Per-file coverage spans the positive, invalid-input, boundary, and error-handling categories for each of the 10 testable files.
- [ ] The full C# toolchain passes in final form in a single pass: csharpier, the analyzer build, the nullable build, and coverage-enabled vstest.
- [ ] No observable behaviour change to QuickFiler flows: every seam is additive and every existing call site compiles unchanged.
- [ ] No edit is made to `coverage.config`, to `QuickFiler/QuickFiler.csproj`, or to any sibling-owned file, and the only shared-file change is `<Compile Include>` additions inside the `Helper Classes\` block of `QuickFiler.Test/QuickFiler.Test.csproj` at lines 158-165.

## Definition of Done

The `## Acceptance Criteria` section above is the sole authoritative checklist for this feature; the
items below are supporting completion notes and are deliberately not checkbox items, so that the AC
tally is unambiguous.

* All eight acceptance criteria above are checked off with evidence.
* All 255 enumerated test cases are implemented, or each omission is justified in the plan against the
  research artifact that enumerated it.
* Per-file numeric coverage evidence for all 14 files is committed under
  `<FEATURE>/evidence/qa-gates/`.
* The four zero-coverable-line ledger classifications are recorded and accepted by F1.
* The full C# toolchain passes in order (csharpier → analyzer build → nullable build → vstest with
  coverage) without any step changing files.
* The latent defects listed under *Non-Goals* are promoted as their own issues through the promotion
  lifecycle.
