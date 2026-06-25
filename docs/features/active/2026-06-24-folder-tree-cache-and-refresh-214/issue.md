# folder-tree-cache-and-refresh (Issue #214)

- Date captured: 2026-06-24
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/ (Issue #214)

- Issue: #214
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/214
- Last Updated: 2026-06-24
- Work Mode: full-feature

## Problem / Why

TaskMaster rebuilds a full Outlook folder hierarchy (`FolderTree`) from scratch on demand. Each build performs synchronous recursive COM enumeration of an entire mailbox subtree on the Outlook STA. On a network-backed store this can take tens of seconds and freeze the UI.

Issue #214 startup-scope exclusion treats startup-specific junk-folder work as related background only. Issue #214 excludes startup-specific junk-folder paths and must not modify `JunkCertain` or `JunkPotential` startup call sites unless separately coordinated.

Verified current construction sites that perform full enumeration and should be migrated, excluding startup-specific junk-folder work:

- `TaskMaster/Ribbon/RibbonController.cs`
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs`
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs`
- `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`

Relevant implementation evidence:

- `UtilitiesCS/OutlookObjects/Folder/FolderTree.cs` performs recursive enumeration through `RootFromFolder` and `InitializeChildren`.
- `WireNotifications()` currently subscribes node `INotifyPropertyChanged` handlers for WPF binding only. It does not subscribe to Outlook folder add, remove, move, or rename notifications.
- No shared cache currently exists; each `FolderTree` construction is throwaway.

## Proposed Behavior

Introduce a shared cached Outlook folder hierarchy service that:

- Builds the hierarchy lazily and at most once per session under normal use.
- Reuses the cached tree across ribbon folder operations, email data mining, folder filtering, and subject-map orchestration.
- Builds on the Outlook STA while cooperatively yielding through `Dispatcher.Yield(...)` so the UI message pump remains responsive.
- Uses iterative traversal instead of recursive descent.
- Supports cancellation or deadline-based abandonment checked at each yield point.
- Keeps cache state current for folder add, remove, move, and rename notifications across multiple stores.
- Provides deterministic disposal for Outlook event sinks and node `PropertyChanged` handlers.
- Handles concurrency and staleness while rebuilds are in flight.
- Introduces narrow test seams over Outlook folder hierarchy access, notifications, clock/deadline behavior, and dispatcher yielding so cache, invalidation, cancellation, and lifecycle behavior can be covered by MSTest, Moq, and FluentAssertions without live COM.

## Acceptance Criteria (early draft)

- [ ] Under normal use, full Outlook folder enumeration runs at most once per session, and repeated ribbon folder operations reuse the shared cache.
- [ ] Folder hierarchy construction is iterative, STA-safe, cooperatively yielding with `Dispatcher.Yield(...)`, and bounded by cancellation or a deadline checked at yield points.
- [ ] The cache updates correctly after folder add, remove, move, and rename events, including multiple-store scenarios.
- [ ] Notification sinks and node `PropertyChanged` handlers are unsubscribed on dispose, with unit coverage proving no unbounded handler accumulation.
- [ ] All in-scope full-enumeration callers reuse the shared cached hierarchy instead of constructing throwaway `FolderTree` instances.
- [ ] Unit tests cover the cache, invalidation, yield, cancellation, disposal, concurrency, and staleness behavior through fake hierarchy and clock seams without live Outlook COM.
- [ ] The implementation follows the issue #214 startup-scope exclusion and does not modify `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` or `JunkCertain` / `JunkPotential` startup call sites unless separately coordinated.
- [ ] Full C# toolchain passes in order: CSharpier, .NET analyzers, nullable/TreatWarningsAsErrors, and MSTest with coverage.

## Constraints & Risks

- Outlook OOM objects are STA-bound. Calls marshal to Outlook's main STA regardless of caller thread, so `Task.Run` does not offload the COM work.
- The build must yield the WPF dispatcher message pump, not use `Application.DoEvents`.
- Build cancellation and deadline behavior must be deterministic and testable.
- Cache invalidation must account for add, remove, move, and rename operations, including multiple stores.
- Long-lived cache services must own and release Outlook event subscriptions and node property-change subscriptions.
- Rebuild concurrency must prevent callers from observing inconsistent or unboundedly stale state.
- The live COM boundary must be isolated behind narrow adapters so logic can be unit tested without Outlook.
- Touched files must stay within the repository 500-line file limit.
- New C# logic must meet the repository coverage requirements, including >= 90% coverage for new modules, classes, or methods where applicable.

## Test Conditions to Consider

- [ ] Cache-hit behavior after the first hierarchy build.
- [ ] Deterministic yield cadence using an injected clock.
- [ ] Cancellation or deadline abandonment during traversal.
- [ ] Folder add, remove, move, and rename event handling against a fake folder hierarchy.
- [ ] Multi-store cache keying and invalidation.
- [ ] In-flight rebuild coalescing and stale-cache behavior.
- [ ] Dispose unsubscribes Outlook notification sinks and node property handlers.
- [ ] Caller migration assertions using the hierarchy seam rather than live COM.
- [ ] Final C# toolchain and coverage execution.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/` folder from the template
