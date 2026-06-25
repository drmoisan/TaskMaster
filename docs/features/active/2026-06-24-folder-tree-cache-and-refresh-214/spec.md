# folder-tree-cache-and-refresh - Refactor Spec

- **Issue:** #214
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-24T15-42
- **Status:** Draft
- **Version:** 0.1

## Intent & Outcomes

TaskMaster currently rebuilds a full Outlook folder hierarchy (`FolderTree`) from scratch on demand. Each build performs synchronous recursive COM enumeration of an entire mailbox subtree on the Outlook STA. On a network-backed store this can take tens of seconds and can block the Outlook UI.

Issue #214 introduces a shared, cached, incrementally-refreshable Outlook folder hierarchy service. The service must build lazily, reuse published snapshots across in-scope callers, remain STA-safe, yield cooperatively through WPF dispatcher scheduling, and keep hierarchy state current across folder and store changes.

The supplied issue #214 context and `artifacts/research/2026-06-24T15-44-folder-tree-cache-refresh-214-research.md` are sufficient to complete this full-feature specification. No additional research handoff is required before planning implementation.

Verified current construction sites that perform full enumeration and should be migrated, excluding startup-specific junk-folder work under the issue #214 startup-scope exclusion:

- `TaskMaster/Ribbon/RibbonController.cs`
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs`
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs`
- `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`

Relevant implementation evidence:

- `UtilitiesCS/OutlookObjects/Folder/FolderTree.cs` performs recursive enumeration through `RootFromFolder` and `InitializeChildren`.
- `WireNotifications()` currently subscribes node `INotifyPropertyChanged` handlers for WPF binding only. It does not subscribe to Outlook folder add, remove, move, or rename notifications.
- No shared cache currently exists; each `FolderTree` construction is throwaway.

## Invariants (must not change)

The issue #214 implementation must preserve the following behavior and contracts unless a later approved plan explicitly changes them:

- Existing user-visible folder filtering, subject-map folder discovery, email-mining folder discovery, and ribbon folder comparison workflows continue to operate from the same Outlook folders as before.
- Existing Outlook folder identity behavior remains compatible with callers that need live `MAPIFolder` handles. Live handles should be resolved at the boundary from cached identity data rather than stored as mutable global cache state where possible.
- Existing store inclusion rules remain authoritative. The cache must reuse the current `StoresWrapper.ShouldIncludeStore` behavior for multi-store root selection.
- The `FolderWrapper.Selected` style of caller-specific selection must not become shared global state. Any selection needed by folder filtering or subject-map exclusion must be represented through a caller-local overlay or disposable compatibility view.
- Public APIs that existing callers depend on should remain stable where practical. If a caller must move from `FolderTree` to a new service contract, the migration must be explicit and covered by tests.
- No CLI, serialized user configuration, or external artifact format is introduced by issue #214.
- The issue #214 work mode remains `full-feature` in `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/issue.md`.

## Scope (structural changes)

Introduce a session-scoped cached Outlook folder hierarchy service that:

- Builds the hierarchy lazily and at most once per session or request scope under normal repeated use.
- Reuses the cached tree across ribbon folder operations, email data mining, folder filtering, and subject-map orchestration.
- Builds on the Outlook STA. Outlook Object Model objects are STA-bound; wrapping traversal in `Task.Run` does not offload COM enumeration and must not be used as the issue #214 responsiveness strategy.
- Uses iterative traversal with an explicit queue or stack instead of recursive descent through the hierarchy.
- Keeps Outlook responsive by using a `Stopwatch` or injected monotonic clock to gate calls to an injected dispatcher-yield seam backed by `Dispatcher.Yield(...)`.
- Avoids `Application.DoEvents`.
- Checks cancellation and deadline expiration at every yield point and discards partial builds instead of publishing incomplete snapshots.
- Keeps cache state current for folder add, remove, move, and rename notifications across multiple Outlook stores.
- Owns the notification-handler lifecycle for Outlook event sinks and node `PropertyChanged` handlers, including deterministic unsubscription and disposal.
- Handles concurrent callers and cache staleness while a refresh or rebuild is already in flight.
- Introduces narrow testability seams over live Microsoft.Office.Interop.Outlook types, including fake hierarchy model, clock, dispatcher yield, notification source, live folder resolver, and cancellation/deadline inputs.
- Produces deterministic unit coverage through MSTest, Moq, and FluentAssertions without live Outlook COM, external services, temporary files, or real timers.

## Non-Goals

The following are explicitly out of scope for issue #214:

- Fixing startup latency through startup-specific junk-folder work.
- Duplicating startup-specific junk-folder work excluded from issue #214.
- Modifying `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` or the `JunkCertain` / `JunkPotential` construction sites unless separately coordinated.
- Replacing Outlook folder APIs or removing the existing Outlook PIA dependency.
- Introducing a polling-only folder hierarchy refresh loop.
- Adding new user-facing configuration, UI controls, CLI flags, or serialized settings for the cache.
- Performing a broad rewrite of `FolderWrapper`, `TreeNode<T>`, or unrelated folder remap code beyond compatibility adapters required for issue #214.
- Changing email classification, subject-map scoring, or folder filtering semantics except where needed to consume the shared folder hierarchy.

## Dependencies / Touchpoints

Primary production touchpoints:

- `UtilitiesCS/OutlookObjects/Folder/FolderTree.cs`: current recursive traversal and property-notification aggregation.
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs`: current live `MAPIFolder` wrapper and `PropertyChanged` behavior.
- `UtilitiesCS/ReusableTypeClasses/Other/TreeNodeOfT.cs`: current recursive tree helpers used by existing views.
- `TaskMaster/AppGlobals/AppOlObjects.cs`: expected session lifetime owner for the cache service.
- `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs`: expected exposure point for in-scope utility callers.
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` and `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`: existing store inclusion and per-store behavior.
- `TaskMaster/AppGlobals/AppEvents.cs`: reference pattern for deterministic Outlook event subscription and unsubscription.

Caller migration touchpoints:

- `TaskMaster/Ribbon/RibbonController.cs`
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs`
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs`
- `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`

Required coordination:

- Issue #214 excludes startup-specific junk-folder paths. Coordinate separately before changing those paths or `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs`.
- Preserve issue #214 artifact references under `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/` and research references under `artifacts/research/2026-06-24T15-44-folder-tree-cache-refresh-214-research.md`.

## Risks & Mitigations

- Outlook OOM objects are STA-bound. Calls marshal to Outlook's main STA regardless of caller thread, so `Task.Run` does not offload the COM work. Mitigation: keep hierarchy acquisition on the Outlook STA behind an explicit service and use dispatcher yielding for responsiveness.
- Long hierarchy builds can block the UI. Mitigation: replace recursive traversal with iterative traversal and use a `Stopwatch` or injected clock to call `Dispatcher.Yield(...)` at a deterministic cadence.
- `Application.DoEvents` can re-enter application code unpredictably. Mitigation: do not use `Application.DoEvents`; use a dispatcher-yield abstraction backed by WPF `Dispatcher.Yield(...)`.
- Cancellation or deadlines can occur mid-build. Mitigation: check cancellation and deadline at every yield point and do not publish partial snapshots.
- Folder add, remove, move, and rename notifications are distributed across parent `Folders` collections and store events. Mitigation: subscribe to watched parent `Folders` event sources and `Stores` events, hold strong references to event sources, and mark affected store scopes stale when localized updates are insufficient.
- Multiple stores can contain similar folder paths. Mitigation: key cache nodes by store identity plus folder identity where available, with path metadata as display and fallback data only.
- Event handlers can accumulate in long-lived services. Mitigation: make the notification sink and compatibility views disposable, test handler counts through fakes, and require deterministic unsubscribe of Outlook sinks and node `PropertyChanged` handlers.
- Concurrent refreshes can expose inconsistent state. Mitigation: publish immutable snapshots atomically, coalesce in-flight builds, and define a stale-read policy for callers.
- Live COM types are difficult to unit test. Mitigation: isolate Microsoft.Office.Interop.Outlook access behind interfaces and use fake hierarchy, notification, clock, dispatcher-yield, and cancellation seams in tests.
- New files can exceed repository limits. Mitigation: keep production, test, and reusable script files under 500 lines and split responsibilities by service, snapshot model, adapter, notification sink, and tests.

## Technical Specifications

### Inputs

- Outlook session state already owned by `AppOlObjects`, including `Application`, `NameSpace`, `StoresWrapper`, root folders, archive root, and included stores.
- Store and folder hierarchy data read through a COM adapter on the Outlook STA.
- Folder notifications from Outlook `Folders` event sources for `FolderAdd`, `FolderChange`, and `FolderRemove`.
- Store notifications from Outlook `Stores` events for store add and remove.
- Caller requests for session-wide snapshots, store-scoped snapshots, archive-root views, or selected-root views.
- Cancellation tokens and optional deadline inputs supplied by callers or service policy.
- A dispatcher-yield abstraction backed by WPF `Dispatcher.Yield(...)` in production.
- A monotonic clock or `Stopwatch` abstraction used to determine yield cadence and deadline expiration.

### Outputs

- Immutable folder hierarchy snapshots that contain folder display name, store identity, folder identity where available, parent key, relative path, folder path, child relationship data, and staleness metadata.
- Caller-specific read-only views or disposable compatibility views for existing workflows that still need `FolderTree`-like behavior.
- Live `MAPIFolder` handles only at the consumption boundary through a resolver such as `(StoreId, EntryId) -> NameSpace.GetFolderFromID(...)`.
- Snapshot change notifications to subscribers, including reason and stale/current status.
- Structured log entries through the repository logging pattern for build start, build completion, cancellation, deadline abandonment, refresh scheduling, notification subscription failures, and disposal failures where actionable.

### API / CLI Surface

Issue #214 does not add a CLI surface.

Recommended internal contracts:

- `IOutlookFolderTreeService`
  - `Task<FolderTreeSnapshot> GetSnapshotAsync(FolderTreeRequest request, CancellationToken token)`
  - `Task<FolderTreeSnapshot> RefreshAsync(FolderTreeRefreshReason reason, CancellationToken token)`
  - `bool TryGetCurrent(out FolderTreeSnapshot snapshot)`
  - `event EventHandler<FolderTreeSnapshotChangedEventArgs> SnapshotChanged`
  - `IDisposable` or equivalent deterministic cleanup contract.
- `IOutlookFolderHierarchyReader`
  - Enumerates stores and child folders through primitive values only.
  - Production implementation owns direct reads from Microsoft.Office.Interop.Outlook types.
  - Test implementation uses an in-memory fake hierarchy.
- `IOutlookFolderNotificationSink`
  - Owns Outlook `Stores` subscriptions and watched parent `Folders` subscriptions.
  - Exposes add, remove, move/rename/change, and store-level invalidation events to the cache service.
  - Supports deterministic disposal.
- `IDispatcherYield`
  - `Task YieldAsync(CancellationToken token)` backed by `Dispatcher.Yield(...)` in production.
  - Fake implementation records yield counts and controlled cancellation in tests.
- `IDeadlineClock`
  - Provides monotonic elapsed time for yield cadence and deadline checks.
- `IFolderHandleResolver`
  - Resolves live `MAPIFolder` handles from cached identity metadata when a caller must enumerate mail items.

### Data & State

Recommended cache state model:

- `Empty`: no complete snapshot has been built.
- `Building`: one build task is in flight and concurrent callers coalesce onto it.
- `Current`: the published snapshot is complete and not known stale.
- `StaleCurrent`: a complete prior snapshot exists, but a folder or store notification has marked one or more scopes stale.
- `Refreshing`: a refresh is in flight while the prior complete snapshot remains available according to request policy.
- `Disposed`: event sinks have been unsubscribed and future service calls fail fast with a clear error.

Snapshot identity rules:

- Prefer `(StoreId, EntryId)` for stable folder keys when available.
- Include folder path and root-relative path as display, filtering, and fallback metadata.
- Include store identity in all cache keys so multiple stores with similar paths do not collide.
- Treat rename as stable identity with changed display/path metadata when identity is stable.
- Treat move as remove plus add when event ordering or payloads are insufficient; mark the affected store scopes stale and schedule a coalesced refresh.

Build behavior:

- Capture included stores on the Outlook STA using existing store inclusion rules.
- Traverse iteratively with an explicit queue or stack of parent folder records.
- Read only required primitive metadata during traversal.
- After a fixed count or elapsed budget, check cancellation, check deadline, and yield through `IDispatcherYield`.
- Publish only complete snapshots through an atomic state transition.
- Discard partial builds on cancellation or deadline expiration.

Refresh and invalidation behavior:

- Folder add: mark the parent store scope stale and schedule refresh; localized insert is allowed only when parent and new child identity are sufficient.
- Folder change: handle rename or metadata change; update localized metadata when identity is stable, otherwise mark the store scope stale.
- Folder remove: mark the parent store scope stale and schedule refresh because the removed folder identity can be unavailable.
- Folder move: handle as remove plus add across watched parents or stores; do not assume event ordering is complete.
- Store add: include the store only if existing inclusion rules permit it, subscribe to its root hierarchy, and schedule refresh.
- Store remove: unsubscribe store-specific event sinks before release, remove or stale-mark that store's nodes, and publish a complete snapshot when safe.

Concurrency behavior:

- Use a private state-transition gate, but do not hold it while awaiting COM reads or dispatcher yields.
- Keep one active build or refresh per service/scope.
- If invalidation arrives while a build is in flight, record a pending refresh and run one follow-up refresh after the active build publishes, cancels, or fails.
- Return immutable snapshots or read-only views so readers cannot observe partial mutation.
- Define request policy for stale reads. UI operations may use the last complete snapshot with stale status where acceptable; data-mining and subject-map rebuild flows should prefer fresh snapshots or explicit stale fallback.

### Migration Guidance

- `EmailDataMiner`: replace `GetOlFolderTree` construction with service calls returning snapshot entries or a compatibility view. Keep existing virtual seams as transitional tests, but stop constructing `FolderTree` internally for issue #214 paths.
- `FilterOlFoldersController`: request a snapshot and create a controller-local selected-path overlay from `_globals.TD.FilteredFolderScraping.Keys`. Dispose controller subscriptions to service snapshot changes or compatibility view events when the viewer closes.
- `SubjectMapSco.Orchestration`: replace `QueryOlFolders` tree construction with a snapshot query returning relative paths and, where needed, live handles resolved through `IFolderHandleResolver`.
- `RibbonController`: route folder info and compare operations through the service. For arbitrary selected roots, request a selected-root scope instead of forcing a full session rebuild when a narrower scope is sufficient.

### Logging/Telemetry

- Use the existing repository logging pattern.
- Log cache build start and completion with scope, node count, store count, elapsed time, and stale/current result.
- Log cancellation and deadline abandonment with scope and elapsed time.
- Log refresh coalescing and pending-refresh scheduling at an appropriate diagnostic level.
- Log notification subscription or unsubscription failures with enough context to identify the store or parent folder source.
- Do not write ad-hoc console output.

## Test Strategy

- Regression tests to add or update:
  - Cache hit after the first hierarchy build.
  - Coalesced concurrent first-build requests.
  - Iterative traversal over a deep fake hierarchy without recursive stack dependence.
  - Yield cadence controlled by a fake clock or `Stopwatch` seam.
  - Cancellation and deadline abandonment checked at yield points.
  - No partial snapshot publication after cancellation or deadline expiration.
  - Folder add, remove, move, and rename invalidation against a fake notification source.
  - Multi-store cache keying and invalidation.
  - In-flight invalidation followed by one coalesced refresh.
  - Disposal unsubscribes Outlook event sinks and node `PropertyChanged` handlers.
  - Caller migration assertions for `RibbonController`, `EmailDataMiner`, `FilterOlFoldersController`, and `SubjectMapSco.Orchestration`.
- Invariant validation tests:
  - Existing folder filtering selection behavior remains caller-local.
  - Existing subject-map and email-mining relative-path outputs remain equivalent for the same fake hierarchy.
  - Multiple stores with duplicate folder names or paths do not collide.
  - Rename updates path metadata without losing stable folder identity when identity remains available.
- Edge cases and negative scenarios:
  - Empty included-store set.
  - Store removed during refresh.
  - Folder removed before a localized update can resolve identity.
  - Notification arrives after disposal.
  - Dispatcher-yield seam throws or cancellation is requested during yield.
  - Live handle resolver cannot resolve a cached identity.
- Error handling and logging verification:
  - Cancellation returns or throws the selected deterministic cancellation result.
  - Deadline expiration returns or throws the selected deterministic timeout result.
  - Disposed service calls fail fast.
  - Recoverable notification subscription failures are logged and mark affected scope stale.
- Coverage impact and targets:
  - New modules, classes, and methods target >= 90% line coverage.
  - Repository-wide line coverage remains >= 80%.
  - Changed lines do not reduce coverage.
- Toolchain commands to run in final implementation validation:
  - `csharpier .`
  - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation:
  - In Outlook, exercise repeated ribbon folder operations, folder filtering, email-mining setup, and subject-map rebuild paths with diagnostic logging enabled.
  - Add, remove, rename, and move folders in at least two stores and confirm stale/refresh behavior through logs and visible folder lists.
  - Confirm issue #214 excludes startup-specific junk-folder paths and that startup-specific junk-folder behavior is unchanged unless separate coordination is recorded.

## Acceptance Criteria

- [x] **AC1 - Work mode and scope.** Issue #214 remains a `full-feature` work item, and all implementation artifacts and cross-references use `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/` and issue #214 identifiers.
- [x] **AC2 - Shared lazy cache.** Under normal repeated use, full Outlook folder enumeration runs at most once per session or request scope, and repeated ribbon folder operations, email data mining, folder filtering, and subject-map orchestration reuse the shared published cache instead of constructing throwaway `FolderTree` instances.
- [x] **AC3 - STA-safe traversal.** Live Outlook Object Model folder traversal runs on the Outlook STA; `Task.Run` is not used as the mechanism for offloading live COM hierarchy enumeration.
- [x] **AC4 - Cooperative responsiveness.** Hierarchy construction is iterative and yields through an injected dispatcher-yield seam backed by `Dispatcher.Yield(...)`, with yield cadence gated by `Stopwatch` or an injected monotonic clock.
- [x] **AC5 - No Application.DoEvents.** The issue #214 implementation does not use `Application.DoEvents` for responsiveness.
- [x] **AC6 - Bounded build.** Cancellation and deadline expiration are checked at every yield point, and cancelled or expired builds do not publish partial snapshots as current.
- [x] **AC7 - Cache invalidation correctness.** Folder add, remove, move, and rename notifications update or stale-mark the correct store scope and schedule a refresh when localized update is not sufficient.
- [x] **AC8 - Multiple-store correctness.** Cache keys and invalidation include store identity so folders with similar names or paths in different stores do not collide.
- [x] **AC9 - Notification lifecycle.** Outlook event sinks are subscribed through a deterministic notification owner and unsubscribed on disposal, including store-level and watched parent `Folders` event sources.
- [x] **AC10 - PropertyChanged lifecycle.** Node `PropertyChanged` handlers used by compatibility views or mutable tree views are unsubscribed on disposal, with unit coverage proving handler counts do not accumulate.
- [x] **AC11 - Concurrency and staleness.** Concurrent callers coalesce onto one in-flight build or refresh per scope, published snapshots remain immutable, stale state is explicit, and invalidation during an in-flight build schedules exactly one follow-up refresh.
- [x] **AC12 - Caller-local selection.** Caller-specific selection state for filtering and subject-map exclusions is represented through overlays or disposable views and does not mutate shared cache nodes.
- [x] **AC13 - Testability seam.** Live Microsoft.Office.Interop.Outlook types are isolated behind testable interfaces, and unit tests cover cache, invalidation, yield, cancellation, disposal, concurrency, staleness, and caller migration through fake hierarchy, fake clock, fake dispatcher yield, fake notifications, and cancellation seams without live Outlook COM.
- [x] **AC14 - Out-of-scope protection.** The implementation follows the issue #214 startup-scope exclusion and does not modify `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` or `JunkCertain` / `JunkPotential` startup call sites unless separately coordinated.
- [x] **AC15 - Toolchain and coverage.** The full C# toolchain passes in order: CSharpier, .NET analyzers, nullable analysis with `TreatWarningsAsErrors`, and MSTest through `vstest.console.exe` with coverage, while meeting repository coverage and file-size requirements.

## Definition of Done

- [x] Acceptance criteria in this spec and `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/user-story.md` are mapped to implementation tasks and verification evidence.
- [x] Shared Outlook folder hierarchy service is exposed through the appropriate application lifetime boundary and consumed by all in-scope callers.
- [x] Recursive full-hierarchy build paths are retired or redirected for issue #214 callers.
- [x] STA, dispatcher-yield, cancellation, deadline, and stale-state behavior are validated by deterministic tests.
- [x] Folder add, remove, move, rename, store add, and store remove behavior is validated with fake notification sources.
- [x] Notification sink and node `PropertyChanged` disposal behavior is validated with handler-count or equivalent lifecycle tests.
- [x] Caller-specific selection state is validated as isolated from shared cache state.
- [x] Issue #214 excludes startup-specific junk-folder paths, and `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` remains unchanged unless separate coordination is recorded.
- [x] Docs updated under `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/`.
- [x] Full C# toolchain pass completed in order: CSharpier -> .NET analyzers -> nullable/TreatWarningsAsErrors -> MSTest with coverage.

## Seeded Test Conditions (from potential)

- [x] Cache-hit behavior after the first hierarchy build.
- [x] Deterministic yield cadence using an injected clock.
- [x] Cancellation or deadline abandonment during traversal.
- [x] Folder add, remove, move, and rename event handling against a fake folder hierarchy.
- [x] Multi-store cache keying and invalidation.
- [x] In-flight rebuild coalescing and stale-cache behavior.
- [x] Dispose unsubscribes Outlook notification sinks and node property handlers.
- [x] Caller migration assertions using the hierarchy seam rather than live COM.
- [x] Final C# toolchain and coverage execution.
