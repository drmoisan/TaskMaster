<!-- markdownlint-disable-file -->

# Task Research Notes: outlook-startup-ui-lockup-followup-148

## Research Executed

### File Analysis

- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/issue.md`
  - Confirmed the research objective: isolate remaining startup and first-email UI-thread stalls after issue `#141`, estimate minimal safe refactor scope, and identify missing instrumentation.
- `change-plan.md`
  - Confirmed the repository is already tracking startup responsiveness work and that the current task is a follow-up research pass rather than an implementation task.
- `TaskMaster/ThisAddIn.cs`
  - Verified `Application_Startup()` still queues startup work through `IdleAsyncQueue.AddEntry(true, ...)`, so startup still begins as a UI-thread idle callback rather than a fully detached background flow.
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`
  - Verified the startup coordinator already yields between phases and offloads `Engines.InitAsync()` with `Task.Run`, but still awaits `_olObjects.LoadAsync()`, `_toDoObjects.LoadAsync()`, `_autoFile.LoadAsync(false)`, and `_events.LoadAsync()` in a single startup sequence.
- `TaskMaster/AppGlobals/AppOlObjects.cs`
  - Verified startup still performs synchronous inbox/store enumeration and store-wrapper restoration through Outlook COM-backed objects.
- `TaskMaster/AppGlobals/AppToDoObjects.cs`
  - Verified disk-bound work is already split from Outlook rebuild work by capturing `Parent.Ol.App` on the caller thread before `Task.Run`, matching the `#141` threading fix.
- `TaskMaster/AppGlobals/AppEvents.cs`
  - Verified startup still hooks Outlook events and then immediately processes unprocessed inbox items, including `Restrict`, `Cast`, `MailItem` materialization, and `MailItemHelper.FromMailItemAsync(...)` during startup.
- `UtilitiesCS/Threading/UiThread.cs`
  - Verified the repository centralizes UI affinity through captured `SynchronizationContext`, `Dispatcher`, and thread ID.
- `UtilitiesCS/Threading/IdleAsyncQueue.cs`
  - Verified queued UI-thread entries run through `UiThread.Dispatcher.InvokeAsync(async () => { await actionAsync(); await Task.Yield(); })`, which preserves UI-thread ownership but does not inherently reduce queued work volume.
- `QuickFiler/Controllers/EfcHomeController.cs`
  - Verified first-email interaction begins by reading `globals.Ol.App.ActiveExplorer().Selection` and starts `EfcDataModel.CreateAsync(...)`, making the first-click path overlap with live Outlook UI state.
- `QuickFiler/Controllers/EfcDataModel.cs`
  - Verified the constructor still contains a synchronous conversation-frame load path (`_conversationResolver.Df = _conversationResolver.LoadDf();`) and the async factory also triggers resolver/model initialization that depends on conversation data.
- `QuickFiler/Helper Classes/ConversationResolver.cs`
  - Verified the resolver calls `_mailItem.GetConversationDfAsync(Token).ConfigureAwait(false)`, then loads per-row mail info and marshals UI updates through `UiThread.Dispatcher.InvokeAsync(UpdateUI)`.
- `UtilitiesCS/Extensions/DfDeedle.cs`
  - Verified `GetEmailDataInViewAsync(...)` still touches `Explorer`, `CurrentFolder`, `GetTableInViewAsync(...)`, `AddQfcColumnsAsync(...)`, and `table.EtlAsync(...)` before only the final DataFrame conversion moves to `Task.Run`.
- `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`
  - Verified conversation loading still calls Outlook conversation APIs and table ETL, with async wrappers that use timeouts and `Task.Run` around COM-backed work.
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`
  - Verified `FromMailItemAsync(...)` is not a true asynchronous decomposition: it eagerly materializes COM-backed tokenization dependencies on the caller thread and returns `Task.FromResult(info)`.
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`
  - Verified `GetTableInViewAsync(...)`, `GetTableAsync(...)`, and `EtlAsync(...)` still wrap Outlook `GetTable`, `GetArray`, and per-row ETL paths with `Task.Run`, meaning COM-backed calls are being shifted to worker threads rather than cleanly snapshotted on the Outlook thread.
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`
  - Verified store rewire after deserialization remains sequential and explicitly yielded between stores, but each store restore still depends on UI-thread COM calls.
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`
  - Verified store initialization still dereferences display name, root folder, inbox, SMTP, and restore paths synchronously through Outlook COM.
- `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs`
  - Verified relative-path restore still walks folder trees synchronously and therefore remains a startup COM-affine hotspot.
- `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/other/thread-affinity-inspection.2026-05-05T09-30-00.md`
  - Confirmed the prior bug fix deliberately kept `AppOlObjects.LoadAsync()`, `StoresWrapper.RewireOlObjectsAsync()`, and `AppEvents.LoadAsync()` on the caller STA/UI thread.
- `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/other/implementation-scope.2026-05-05T09-23-00.md`
  - Confirmed the prior implementation scope focused on `ApplicationGlobals`, `AppOlObjects`, `StoresWrapper`, and `AppToDoObjects`, leaving `AppEvents` and QuickFiler conversation paths outside the active production-file budget.
- `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/qa-gates/targeted-regression.2026-05-06T14-37-21.md`
  - Confirmed the prior tests validated the coordinator and store/threading fixes, but did not add direct `AppEvents` or selection-responsiveness regression coverage.
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`
  - Verified existing tests cover coordinator sequencing, yields, and engine offload behavior.
- `TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs`
  - Verified existing tests cover store load/rewire coordination, not first-click responsiveness.
- `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs`
  - Verified existing tests cover worker-thread avoidance for Outlook application access.
- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`
  - Verified existing tests cover rewire ordering and yielded iteration behavior.
- `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs`
  - Verified current coverage focuses on resolver correctness and fallback behavior, not startup/selection responsiveness.
- `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`
  - Verified current coverage focuses on controller correctness and re-entrancy, not first-selection thread interaction.
- `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelper_ExtendedTests.cs`
  - Verified there is an existing test home for conversation helpers that can absorb new thread-affinity regression coverage.
- `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs`
  - Verified there is an existing test home for `MailItemHelper.FromMailItemAsync(...)` and tokenization dependency materialization.
- `UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs`
  - Verified a unit-test home already exists for pure DataFrame transforms.
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`
  - Verified a COM-oriented test home already exists for `DfDeedle` seams.

### Code Search Results

- `ProcessNewInboxItemsAsync|FromMailItemAsync\(|GetTableInViewAsync\(|GetConversationDfAsync\(`
  - `grep_search` returned 36 matches across `TaskMaster/AppGlobals/AppEvents.cs`, `QuickFiler/Helper Classes/ConversationResolver.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, and multiple downstream consumers/tests, confirming these methods are shared hotspots rather than isolated helpers.
- `AppEventsTests|ProcessNewInboxItemsAsync|ProcessMailItemAsync`
  - `grep_search` over `**/*Test*.cs` returned no matches, confirming there is currently no direct unit-test home covering startup inbox processing in `AppEvents`.
- `**/*DfDeedle*Test*.cs`
  - `file_search` found `UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs` and `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`, confirming the data/table boundary already has test homes.
- `QuickFiler.Test/**/*Efc*Test*.cs`
  - `file_search` found `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` and `QuickFiler.Test/Controllers/EfcFormControllerTests.cs`, confirming `EfcHomeController` already has a regression location.
- `QuickFiler.Test/**/*ConversationResolver*Tests.cs`
  - `file_search` found `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs`, confirming a direct resolver test home exists.

### External Research

- #githubRepo:"dotnet/runtime SynchronizationContext.Current TaskScheduler.Current"
  - `github_text_search` found runtime implementations in `YieldAwaitable.cs`, `AsyncOperation.cs`, and `TaskContinuation.cs` that check `SynchronizationContext.Current` first and then `TaskScheduler.Current` when deciding continuation placement, matching the continuation-capture explanation in the .NET documentation.
- #fetch:https://learn.microsoft.com/en-us/visualstudio/vsto/threading-support-in-office?view=vs-2022
  - Microsoft documents that Office runs in the main STA, background-thread calls into the Office object model are marshaled across the apartment boundary, and those calls may be delayed or rejected when Office is busy; VSTO interop surfaces rejected calls as `COMException`.
- #fetch:https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.yield?view=net-9.0
  - Microsoft documents that `await Task.Yield()` posts the remainder of the method back to the current context and explicitly warns not to rely on it to keep a UI responsive.
- #fetch:https://learn.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext?view=net-9.0
  - Microsoft documents that `SynchronizationContext` is the abstraction used to propagate scheduling behavior and that UI environments provide derived contexts such as Windows Forms and Dispatcher-based contexts.
- #fetch:https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.configureawait?view=net-9.0
  - Microsoft documents that `ConfigureAwait(false)` avoids marshaling continuations back to the captured context and can reduce deadlock/performance issues, but continuation placement remains a context-sensitive application design choice.
- #fetch:https://devblogs.microsoft.com/dotnet/configureawait-faq/
  - Stephen Toub’s Microsoft-authored guidance states that application-level UI code generally wants the default captured context behavior, while general-purpose library code should use `ConfigureAwait(false)` when the continuation no longer depends on UI/app-model state.
- #fetch:https://learn.microsoft.com/en-us/windows/win32/api/objidl/nn-objidl-imessagefilter
  - Microsoft documents that `IMessageFilter` exists to handle rejected or pending COM calls while synchronous calls are waiting, which is relevant because Office background-thread COM calls can be retried or rejected when the server is busy.
- #fetch:https://learn.microsoft.com/en-us/windows/win32/api/objidl/nf-objidl-imessagefilter-retryrejectedcall
  - Microsoft documents that rejected COM calls can be retried after a delay and that callers must decide whether to retry or cancel, reinforcing that a `Task.Run` wrapper is not a safe substitute for explicit COM-affinity design.

### Project Conventions

- Standards referenced: repository general code change policy, C# code change policy, C# unit test policy, tone policy, issue `#141` scope evidence, and issue `#148` bug statement.
- Instructions followed: `policy-compliance-order`, `feature-promotion-lifecycle`, and `evidence-and-timestamp-conventions` were read before analysis; research was limited to evidence gathering and writing under `artifacts/research/` only.

## Key Discoveries

### Project Structure

Startup and first-selection work are split across two separate pipelines that can overlap in the same Outlook session:

1. Startup still begins in `ThisAddIn.Application_Startup()` and is queued onto the UI-thread idle queue.
2. `ApplicationGlobals.LoadSequentialAsync()` already yields between phases, but the heaviest remaining COM-affine phases are still `AppOlObjects.LoadAsync()` and `AppEvents.LoadAsync()`.
3. The first-email interaction path is largely independent of the startup coordinator and flows through `EfcHomeController` -> `EfcDataModel` -> `ConversationResolver` -> `ConversationHelper` / `DfDeedle` / `OlTableExtensions` / `MailItemHelper`.
4. Because both pipelines still enter Outlook COM-backed objects, the most likely lock-up is additive: startup continues to occupy the Outlook STA while first-selection work tries to materialize conversations, tables, recipients, and attachments during the same window.

The prior `#141` fix reduced startup coordinator blocking but did not yet cover the startup inbox-processing path in `AppEvents` or the selection-driven QuickFiler conversation path, which is consistent with the follow-up symptom described in issue `#148`.

### Implementation Patterns

The codebase already separates some background-safe work from UI-thread work, but the remaining hotspots show a repeating pattern: COM-heavy operations are frequently wrapped in `Task.Run` rather than snapshotted on the Outlook thread and then transformed off-thread.

Verified COM-affine work that should remain on the Outlook/UI thread:

- `Application_Startup()` and the start of the startup coordinator.
- Store enumeration, store-wrapper init/restore, and folder-tree restore in `AppOlObjects`, `StoresWrapper`, `StoreWrapper`, and `FolderMinimalWrapper`.
- `AppEvents.Hook()` and inbox `Restrict` / row / `MailItem` materialization in `AppEvents.ProcessNewInboxItemsAsync()`.
- `ActiveExplorer`, `Selection`, `CurrentView`, `CurrentFolder`, `MailItem.GetConversation()`, `Conversation.GetTable()`, `MAPIFolder.GetTable()`, `Table.GetRowCount()`, `Table.GetNextRow()`, `Row.GetValues()`, and folder user-property mutation.
- `MailItemHelper.MaterializeTokenizationDependencies()` because it eagerly dereferences COM-backed properties such as body, HTML, recipients, and attachments.
- Final UI projection methods such as `UiThread.Dispatcher.InvokeAsync(UpdateUI)` and progress-pane/task-pane interaction.

Verified background-safe work once a stable snapshot already exists:

- Disk/config reads already handled in `AppToDoObjects` and engine initialization already offloaded by `ApplicationGlobals`.
- Converting `object[,]` + column maps into Deedle or `Microsoft.Data.Analysis` frames.
- Tokenization after COM-backed fields have been materialized.
- Filtering, sorting, ranking, and predictor/model work that only depends on immutable DTO/dataframe snapshots.
- Aggregating metrics and timing summaries.

The highest-risk pattern is the current use of `Task.Run` around Outlook COM calls in `ConversationHelper`, `OlTableExtensions`, `DfDeedle`, and related helpers. Microsoft’s Office threading guidance does not describe this as background-safe work; it describes it as marshaled COM work that may block, be delayed, or be rejected while Office is busy.

### Complete Examples

```csharp
// Source: UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs
public static async Task<Outlook.Table> GetTableInViewAsync(
    this Explorer activeExplorer,
    CancellationToken token,
    int counter
)
{
    Outlook.TableView view = activeExplorer.CurrentView as Outlook.TableView;
    table = await Task.Run(view.GetTable, token).TimeoutAfter(2000);
}

// Source: UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs
public static Task<MailItemHelper> FromMailItemAsync(
    MailItem item,
    IApplicationGlobals appGlobals,
    CancellationToken token,
    bool loadAll
)
{
    var info = new MailItemHelper(item, appGlobals);
    info.MaterializeTokenizationDependencies();
    return Task.FromResult(info);
}
```

The first example shows Outlook `GetTable()` being moved onto a worker thread instead of being snapshotted on the Outlook thread. The second example shows that `FromMailItemAsync(...)` is effectively synchronous for the expensive COM-backed portion, so callers awaiting it are not actually deferring that cost away from the current thread.

### API and Schema Documentation

- Office/VSTO threading support:
  - Office object model code runs on the main STA.
  - Background-thread calls into Office COM are marshaled and may be delayed or rejected while Office is busy.
  - VSTO interop surfaces rejected calls as `COMException`.
- `Task.Yield()`:
  - Posts the continuation back to the current context.
  - Microsoft explicitly warns that UI contexts may prioritize posted work ahead of input/rendering, so it is not a reliable responsiveness fix.
- `SynchronizationContext` and `ConfigureAwait`:
  - Await captures the current `SynchronizationContext` first, then a non-default `TaskScheduler`.
  - `ConfigureAwait(false)` prevents queueing back to the captured context for continuations that do not require UI/app-model state.
  - Application-level UI code should usually keep the captured context when it will continue touching UI or Outlook-owned state.
- COM rejected-call handling:
  - `IMessageFilter` / `RetryRejectedCall` exist because synchronous COM calls can be rejected or told to retry later when the callee is busy.

### Configuration Examples

```text
# Existing #141 scope evidence (docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/other/implementation-scope.2026-05-05T09-23-00.md)
Production Files CSV: TaskMaster/AppGlobals/ApplicationGlobals.cs, TaskMaster/AppGlobals/AppOlObjects.cs, UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs, TaskMaster/AppGlobals/AppToDoObjects.cs
Contingent Production Files CSV: TaskMaster/ThisAddIn.cs, TaskMaster/AppGlobals/AppEvents.cs, TaskMaster/AppGlobals/AppAutoFileObjects.cs, TaskMaster/AppGlobals/AppItemEngines.cs
Test Files CSV: TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs, TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs, TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs, UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs
Promoted Contingent File Count: 0
```

This matters because the strongest remaining hotspot in issue `#148` is in a file (`TaskMaster/AppGlobals/AppEvents.cs`) that was explicitly left outside the active implementation scope for `#141`.

### Technical Requirements

Most likely remaining root-cause hotspots, in order of probability from the current evidence:

1. **First-email conversation/table acquisition during the startup window**
   - `EfcHomeController`, `EfcDataModel`, `ConversationResolver`, `ConversationHelper`, `DfDeedle`, and `OlTableExtensions` still perform Outlook selection, conversation, and table work while startup is not yet idle.
   - Several of these paths use `Task.Run` around COM-backed methods, which can still serialize onto the Office STA or fail with busy/retry behavior.
2. **Startup inbox processing in `AppEvents.ProcessNewInboxItemsAsync()`**
   - This path still runs during startup, iterates restricted inbox items, builds `MailItemHelper` instances, and even nudges the synchronization context with `Application.DoEvents()`-style behavior.
   - There are currently no direct unit tests covering this path.
3. **Store restore/rewire and folder-path walking in `AppOlObjects` / `StoresWrapper`**
   - This remains a legitimate startup hotspot, but `#141` already instrumented and constrained it. It is still likely part of the freeze window, but the research evidence suggests it is not the only remaining cause.
4. **Eager mail-item projection in `MailItemHelper.FromMailItemAsync(...)`**
   - The method name suggests asynchronous relief, but the expensive COM-backed property reads still occur synchronously on the caller thread.

Recommended instrumentation additions and exact locations:

- `TaskMaster/AppGlobals/ApplicationGlobals.cs`
  - Add per-phase start/stop timings with thread ID, current synchronization context type, and queue source so first-click events can be correlated against the active startup phase.
- `TaskMaster/AppGlobals/AppEvents.cs`
  - Add timings around `Hook()`, inbox `Restrict`, restricted item enumeration count, each `ProcessMailItemAsync(...)` call, and the total startup inbox-processing duration.
- `QuickFiler/Controllers/EfcHomeController.cs`
  - Add a first-selection timing envelope from selection capture through model init completion, including the selected-count and whether startup is still running.
- `QuickFiler/Controllers/EfcDataModel.cs`
  - Time synchronous constructor-based `LoadDf()` versus async factory-based initialization so the actual first-click path can be distinguished.
- `QuickFiler/Helper Classes/ConversationResolver.cs`
  - Time `LoadDfAsync(...)`, `BackgroundInitInfoItemsAsync(...)`, per-row helper materialization, and each UI-dispatch update.
- `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`
  - Time `GetConversation()`, `GetConversationTable()`, `GetDataFrameAsync(...)`, and retries/timeouts separately.
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`
  - Time `GetTableInViewAsync(...)`, `GetTableAsync(...)`, `GetRowCount()`, `CastToRowArray(...)`, and `EtlByRowAsync(...)` so COM acquisition and pure ETL can be distinguished.
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`
  - Time `MaterializeTokenizationDependencies()`, recipient load, attachment load, and tokenization as separate segments.
- `UtilitiesCS/Extensions/DfDeedle.cs`
  - Time `AddQfcColumnsAsync(...)` separately from `table.EtlAsync(...)` and the final `Email2dArrayToDf(...)` transform.

Minimal safe refactor direction:

- Keep Outlook COM acquisition on the Outlook/UI thread.
- Replace `Task.Run`-wrapped COM calls with an explicit **UI-thread snapshot -> background transform -> single UI-thread publish** pattern.
- Use `ConfigureAwait(false)` only in helper/library segments that operate exclusively on snapshots, DTOs, arrays, or frames and do not touch Outlook or UI state afterward.
- Do not treat additional `Task.Yield()` calls as a responsiveness fix; use explicit staging boundaries and, where needed, separate idle-queue items or batched resumes.
- If startup inbox processing remains expensive after instrumentation, defer it until the first interactive window has passed or batch it into smaller UI-thread slices separated by explicit idle rescheduling instead of one long loop.

Likely impacted production files for a minimal follow-up implementation:

- `TaskMaster/AppGlobals/AppEvents.cs`
- `QuickFiler/Controllers/EfcHomeController.cs`
- `QuickFiler/Controllers/EfcDataModel.cs`
- `QuickFiler/Helper Classes/ConversationResolver.cs`
- `UtilitiesCS/Extensions/DfDeedle.cs`
- `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`

Files to observe but only touch if instrumentation proves they still dominate:

- `TaskMaster/AppGlobals/ApplicationGlobals.cs`
- `TaskMaster/AppGlobals/AppOlObjects.cs`
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`
- `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs`

Likely impacted test files for a minimal follow-up implementation:

- `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`
- `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelper_ExtendedTests.cs`
- `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs`
- `UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs`
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`
- `TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs`
- **new likely test home:** `TaskMaster.Test/AppGlobals/AppEventsTests.cs`

Regression risks tied to Outlook COM affinity and synchronization-context resumes:

- Moving Outlook COM calls behind `Task.Run` can appear asynchronous while still being marshaled back to the Office STA, increasing the risk of `COMException` busy/retry behavior.
- Using `ConfigureAwait(false)` in orchestration code that later touches Outlook objects, task panes, or controller/UI state can resume on a worker thread and break COM/UI invariants.
- Relying on `Task.Yield()` to improve perceived responsiveness can backfire because the UI synchronization context may prioritize queued continuations ahead of user input and painting.
- Publishing partial UI updates too frequently from `ConversationResolver` can create a second responsiveness problem even after data work is moved off-thread.

**Mandatory unachievable objective callout**:
- **No research objective was proven unachievable.** Manual Outlook validation remains deferred in prior `#141` evidence because changed-code coverage was below the required threshold, but the requested research outputs for issue `#148` were achievable from the available repository and Microsoft documentation evidence.

## Recommended Approach

Use a single staged-loading approach built around explicit Outlook-thread snapshots rather than more `Task.Run` around COM:

1. **Instrument first, then move only the verified snapshot-safe work.** Add per-segment timings in `AppEvents`, `EfcHomeController`, `EfcDataModel`, `ConversationResolver`, `ConversationHelper`, `OlTableExtensions`, `DfDeedle`, and `MailItemHelper` so startup-overlap and first-click costs can be separated.
2. **Preserve COM-affine acquisition on the Outlook thread.** Selection access, conversation/table creation, row/value extraction, recipient/attachment/body reads, and folder property mutation should stay on the Outlook thread.
3. **Create immutable snapshots at the COM boundary.** Once table rows, mail-item fields, or folder metadata have been copied into arrays/DTOs, perform DataFrame creation, filtering, tokenization, predictor work, and ranking off-thread with `ConfigureAwait(false)` in helper code that no longer references Outlook/UI state.
4. **Publish back once per UI-visible stage.** Prefer one marshal back for completed model/update publication instead of repeated fine-grained dispatcher calls.
5. **If instrumentation confirms startup inbox processing is still expensive, batch or defer it.** The safest follow-up is to stage `ProcessNewInboxItemsAsync()` behind a later idle/interactive checkpoint rather than trying to make Outlook inbox enumeration itself background-thread-safe.

Brief rejected alternatives:

- **Add more `Task.Yield()` calls:** rejected because Microsoft explicitly warns that `Task.Yield()` is not a reliable UI-responsiveness mechanism, and the coordinator already uses it.
- **Wrap more Outlook COM in `Task.Run`:** rejected because Office COM calls remain marshaled/busy-sensitive and can still block or be rejected.
- **Apply `ConfigureAwait(false)` broadly in controller/orchestration code:** rejected because the continuation often still needs Outlook/UI affinity after the await.

## Implementation Guidance

- **Objectives**: isolate the remaining freeze to specific startup and first-click segments; preserve Outlook COM affinity; move only snapshot-safe transforms off-thread; add the minimum regression coverage needed to protect the change.
- **Key Tasks**: add timing instrumentation; classify each measured segment as COM-affine or background-safe; refactor the first-click conversation/table path into snapshot plus background transform; add direct `AppEvents` regression coverage; rerun the standard C# toolchain and manual Outlook validation once coverage gates are satisfied.
- **Dependencies**: Outlook/VSTO STA rules from Microsoft documentation; existing `UiThread` and `IdleAsyncQueue` infrastructure; test homes in `QuickFiler.Test`, `UtilitiesCS.Test`, and `TaskMaster.Test`; prior `#141` startup evidence for baseline comparison.
- **Success Criteria**: instrumentation clearly attributes startup and first-click time to specific segments; no Outlook COM access occurs from background-only stages; the first-email path no longer performs large conversation/table/materialization work on the UI thread beyond snapshot capture; new or updated tests cover the changed paths; Outlook remains responsive during startup and first-email interaction in manual validation.