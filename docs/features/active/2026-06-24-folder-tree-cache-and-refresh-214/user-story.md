# `folder-tree-cache-and-refresh` - User Story

- Issue: #214
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-06-24T15-42

## Story Statement

- As a TaskMaster Outlook user working with a large or network-backed mailbox, I want folder-based operations to reuse a responsive cached hierarchy, so that ribbon actions, filtering, mining, and subject-map workflows do not repeatedly freeze Outlook while the same folder tree is rebuilt.
- As a TaskMaster maintainer, I want the Outlook folder hierarchy to be represented by a disposable, incrementally-refreshable, testable service, so that cache invalidation, COM threading, concurrency, cancellation, and notification lifecycles can be verified without live Outlook COM.

## Problem / Why

TaskMaster currently constructs throwaway `FolderTree` instances in multiple workflows, and each construction recursively enumerates Outlook folders through live COM on the Outlook STA. `Task.Run` does not remove that STA dependency because Outlook Object Model calls marshal back to the main STA. A shared cached service with cooperative dispatcher yielding, bounded traversal, event-driven invalidation, and deterministic disposal is needed to preserve responsiveness and reduce repeated full enumeration.

Startup-specific junk-folder work is related background only. Issue #214 excludes startup-specific junk-folder paths and must not modify `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` unless separately coordinated.

## Personas & Scenarios

- Persona: Outlook add-in user with a large mailbox
  - Who: A TaskMaster user whose Outlook profile includes a large archive, network-backed store, or multiple stores.
  - What they care about: Folder operations should remain responsive and should not repeat expensive full hierarchy enumeration for each workflow.
  - Constraints: The user works inside Outlook while the add-in shares the Outlook main STA. They may add, remove, rename, or move folders during a session.
  - Goals and frustrations: Wants ribbon folder actions, filtering, mining, and subject-map workflows to use current folder data without long UI stalls. Frustrated when the same folder tree is rebuilt repeatedly.
  - Context and motivations: The user expects TaskMaster to remain usable during normal Outlook work even when the folder hierarchy is large.

- Persona: TaskMaster maintainer
  - Who: The developer responsible for C# implementation, test coverage, and Outlook COM lifecycle correctness.
  - What they care about: A narrow service boundary that isolates live Microsoft.Office.Interop.Outlook types, preserves STA rules, and can be covered with deterministic unit tests.
  - Constraints: Tests must use MSTest, Moq, and FluentAssertions; unit tests must not require live Outlook COM, external services, temporary files, or real timers. Repository file-size and coverage policies apply.
  - Goals and frustrations: Wants cache behavior, invalidation, cancellation, dispatcher yielding, notification disposal, and concurrency behavior to be testable. Frustrated by current direct `new FolderTree(...)` construction sites and missing event-unsubscription lifecycle.
  - Context and motivations: The maintainer needs a design that can be implemented incrementally while keeping startup-specific junk-folder work out of scope under the issue #214 startup-scope exclusion.

- Scenario: Repeated folder operation reuses the cache
  - Who is acting: The Outlook add-in user.
  - What triggered the action: The user runs a ribbon folder action, opens folder filtering, starts email mining, or rebuilds subject-map data after the folder hierarchy has already been requested in the current session.
  - Steps: (1) The first in-scope workflow requests the folder hierarchy. (2) The service builds a complete snapshot lazily on the Outlook STA with cooperative dispatcher yields. (3) Later in-scope workflows request compatible views from the same service. (4) The service returns the published snapshot or a caller-specific view instead of rebuilding the whole hierarchy.
  - Obstacles or decisions: If the snapshot is stale because a folder notification arrived, the caller follows the request policy for waiting, reading stale data with explicit status, or triggering refresh.
  - Expected outcome: Repeated operations do not create new throwaway `FolderTree` instances for the same session or scope under normal use.

- Scenario: Folder hierarchy changes during a session
  - Who is acting: The Outlook add-in user and Outlook itself.
  - What triggered the action: A folder is added, removed, renamed, or moved, or a store is added or removed.
  - Steps: (1) The notification sink receives the folder or store event from a watched source. (2) The service updates localized metadata only when identity context is sufficient. (3) Otherwise, the service marks the affected store scope stale and schedules one coalesced refresh. (4) Published snapshots remain immutable while refresh is in flight.
  - Obstacles or decisions: Move can appear as remove plus add and can cross watched parents or stores, so the service must not assume a single event contains the full move.
  - Expected outcome: Cache state remains correct for add, remove, move, rename, and multiple-store cases without exposing partially mutated snapshots to readers.

- Scenario: Build is cancelled or reaches its deadline
  - Who is acting: A caller or service policy.
  - What triggered the action: The caller cancels the operation or the service reaches its configured build deadline while traversal is still running.
  - Steps: (1) The iterative builder reaches a yield point. (2) The builder checks cancellation and deadline before or at the dispatcher yield. (3) The build exits with the selected cancellation or timeout result. (4) The service keeps the previous complete snapshot if one exists.
  - Obstacles or decisions: Partial traversal data must not be published as current.
  - Expected outcome: Cancellation and deadline behavior are deterministic, testable, and do not corrupt cache state.

- Scenario: Service is disposed
  - Who is acting: Application lifetime management or a view/controller owning a compatibility view.
  - What triggered the action: Outlook shutdown, add-in shutdown, controller close, or service replacement.
  - Steps: (1) The owner calls dispose. (2) Outlook `Stores` and watched parent `Folders` event handlers are unsubscribed. (3) Node `PropertyChanged` handlers used by compatibility views are unsubscribed. (4) Future service calls fail fast with a clear disposed-state result.
  - Obstacles or decisions: COM event sources must be retained strongly enough to unsubscribe reliably.
  - Expected outcome: Handler counts do not accumulate across rebuilds, refreshes, or view lifetimes.

## Acceptance Criteria

- [x] **AC1 - Full-feature scope is preserved.** Issue #214 remains marked as `full-feature`, and the requirements and implementation artifacts reference `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/` and issue #214 consistently.
- [x] **AC2 - Shared lazy cache.** Under normal repeated use, the Outlook folder hierarchy is fully enumerated at most once per session or request scope, and repeated ribbon folder operations, email data mining, folder filtering, and subject-map orchestration reuse the shared cached hierarchy.
- [x] **AC3 - Direct construction retired for in-scope callers.** `TaskMaster/Ribbon/RibbonController.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs`, `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs`, and `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs` no longer construct throwaway `FolderTree` instances for issue #214 paths.
- [x] **AC4 - STA-safe COM traversal.** Live Outlook Object Model hierarchy traversal remains on the Outlook STA, and `Task.Run` is not used as the mechanism for offloading live COM folder enumeration.
- [x] **AC5 - Cooperative responsiveness.** Hierarchy construction uses iterative traversal and a `Stopwatch` or injected monotonic clock to gate calls to a dispatcher-yield seam backed by `Dispatcher.Yield(...)`.
- [x] **AC6 - No `Application.DoEvents`.** The implementation does not use `Application.DoEvents` for folder hierarchy build responsiveness.
- [x] **AC7 - Bounded traversal.** Cancellation and deadline expiration are checked at every yield point, and cancelled or deadline-expired builds do not publish partial snapshots.
- [x] **AC8 - Folder invalidation.** Folder add, remove, move, and rename notifications update localized cache state when safe or mark the affected store scope stale and schedule a coalesced refresh when localized update is insufficient.
- [x] **AC9 - Multiple-store correctness.** Cache identity and invalidation include store identity so folders with similar names or paths in different stores remain distinct.
- [x] **AC10 - Notification disposal.** Outlook `Stores` and watched parent `Folders` event sinks are owned by a deterministic lifecycle component and unsubscribed on disposal.
- [x] **AC11 - Node handler disposal.** Node `PropertyChanged` handlers used for WPF binding or compatibility views are unsubscribed on disposal, with tests proving handlers do not accumulate across view or service lifetimes.
- [x] **AC12 - Concurrency and staleness.** Concurrent requests coalesce onto a single in-flight build or refresh per scope, invalidation during a build records pending refresh work, stale state is explicit, and readers never observe partially mutated snapshots.
- [x] **AC13 - Caller-local selection state.** Folder filtering and subject-map exclusion state is represented through caller-local overlays or disposable views and does not mutate shared cached nodes.
- [x] **AC14 - Testability without live Outlook.** Unit tests cover cache hits, invalidation, dispatcher yield cadence, cancellation/deadline behavior, disposal lifecycle, concurrency, stale-read policy, multi-store behavior, and caller migration through fake hierarchy, fake clock, fake dispatcher yield, fake notification source, and cancellation seams without live Microsoft.Office.Interop.Outlook objects.
- [x] **AC15 - Issue #214 startup-scope exclusion.** The implementation does not modify startup-specific junk-folder work, `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs`, or `JunkCertain` / `JunkPotential` startup call sites unless separate coordination is recorded.
- [x] **AC16 - Toolchain and coverage.** The full C# toolchain passes in order: CSharpier, .NET analyzers, nullable analysis with `TreatWarningsAsErrors`, and MSTest with coverage, while repository coverage and file-size requirements remain satisfied.

## Non-Goals

The following are explicitly excluded from issue #214:

- Fixing startup latency through startup-specific junk-folder work.
- Duplicating startup-specific junk-folder work excluded from issue #214.
- Modifying `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` or startup-specific `JunkCertain` / `JunkPotential` construction sites unless separately coordinated.
- Adding new user-facing settings, UI controls, CLI flags, or serialized configuration for the cache.
- Replacing Outlook PIA usage or changing the underlying Outlook folder model.
- Rewriting email classification, folder filtering semantics, subject-map scoring, or unrelated folder remap behavior.
- Using polling as the primary refresh mechanism when Outlook folder and store events are available.
