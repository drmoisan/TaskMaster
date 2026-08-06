# 2026-08-04-folder-tree-dispatcher-thread-affinity (Spec)

- **Issue:** #420
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-06T18-38
- **Status:** Implementation and final QA complete; feature review pending
- **Version:** 0.1

## Context
The shared Outlook folder-tree service can invoke `WpfDispatcherYield` from a dispatcher-free worker after `EmailDataMiner` starts a cold cache build through `Task.Run`. `Dispatcher.Yield` then raises `InvalidOperationException`, and a fallback that only uses `Task.Yield` would not restore the required Outlook STA traversal contract.

Environment:
- OS/version: Windows with Outlook VSTO runtime
- Application: TaskMaster Outlook add-in
- Trigger: Cold folder-tree cache build or refresh that reaches a deadline-controlled yield point
- Data source or fixture: Outlook folder hierarchy through the live COM adapter

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Start the email-mining path while the shared folder-tree cache requires a build or refresh.
2. `EmailDataMiner.MineEmails` invokes `ExtractOlFolderChunks` through `Task.Run`.
3. The folder-tree reader reaches `WpfDispatcherYield.YieldAsync` after the deadline clock requests a yield.

Expected:
Live Outlook COM traversal and cooperative yielding remain on the captured Outlook STA dispatcher, and the caller receives a folder snapshot without a thread-affinity exception.

Actual:
`WpfDispatcherYield` invokes `Dispatcher.Yield(DispatcherPriority.Background)` on a worker thread that has no current dispatcher. WPF raises `InvalidOperationException` indicating that the calling thread does not have a current Dispatcher.

Logs / Screenshots:
- [x] Captured stack evidence reviewed
- Snippet: `EmailDataMiner -> OutlookFolderTreeService -> FolderTreeSnapshotBuilder -> OutlookFolderHierarchyReader -> WpfDispatcherYield -> Dispatcher.Yield`


## Scope & Non-Goals
- In scope:
  - Session-scoped composition of the shared folder-tree service, including the live reader and notification sink.
  - Cold-build and refresh dispatch in `OutlookFolderTreeService`.
  - Reader and builder continuation behavior around `WpfDispatcherYield`.
  - Asynchronous FilterOlFolders initialization required to avoid blocking the Outlook STA during a cold build.
  - Deterministic C# regression coverage for dispatcher affinity, worker-originated requests, and cold UI initialization.
- Out of scope / non-goals:
  - Replacing the shared folder-tree cache, changing `IOutlookFolderTreeService.GetSnapshotAsync`, or changing its snapshot data contract.
  - Removing all worker callers such as email mining, scrape, or SubjectMap.
  - Introducing a worker-local WPF dispatcher, a `Task.Yield` fallback for live traversal, new packages, or live Outlook integration tests.
- Explicitly excluded systems, integrations, or datasets:
  - Outlook account configuration, folder data migration, and external services.

The authoritative work mode is `full-bug` in `issue.md`; consequently, this specification is the sole acceptance-criteria source and `user-story.md` is intentionally absent.

## Root Cause Analysis
PR #215 added the shared folder-tree service and unconditionally wires `WpfDispatcherYield`. The earlier `EmailDataMiner` worker boundary remains. Issue #214 requires live Outlook COM traversal on the Outlook STA and dispatcher-backed cooperative yielding. `ConfigureAwait(false)` suppresses context capture and can move a continuation off the STA; it does not preserve one worker thread.


## Proposed Fix

### Design summary (what changes where):

`AppOlObjects` must compose the session-scoped service on the captured Outlook STA dispatcher, including creation of the live hierarchy reader and `OutlookFolderNotificationSink`. `OutlookFolderTreeService` must own dispatch of every uncached build and refresh to that dispatcher. `FolderTreeSnapshotBuilder` and `OutlookFolderHierarchyReader` must retain that dispatcher context across deadline-driven yields before performing any further live COM access.

### Boundaries and invariants to preserve:

- All live Outlook store, folder, and notification-subscription work occurs on the captured Outlook STA dispatcher.
- `WpfDispatcherYield` remains the production yielding mechanism for live traversal and is invoked only with its dispatcher context available.
- The cache continues to coalesce in-flight requests, publish immutable snapshots, preserve stale/current state transitions, propagate cancellation, and dispose notification subscriptions.
- Callers may begin a request from a worker or UI context; callers do not choose the live-execution strategy.
- Once the immutable snapshot is complete, callers may resume on their own permitted context.

### Dependencies or blocked work:

- Existing seams: `UiThread`, `IUiDispatcher`, `WpfUiDispatcher`, `DeadlineClock`, `IDispatcherYield`, fake hierarchy readers, and fake notification sinks.
- No package, configuration, or service dependency is required. The work depends on the dispatcher captured during add-in startup being available before session service composition.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `TaskMaster/AppGlobals/AppOlObjects.cs`: serialize first service creation and marshal the complete live composition root to the captured UI dispatcher when first requested from a worker.
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs`: receive the dispatcher execution seam and marshal all cold builds and invalidation-triggered refreshes before entering the builder.
- `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotBuilder.cs` and `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs`: preserve the dispatcher context for all awaits that precede or follow a live traversal yield.
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs` and the relevant Ribbon initialization path: replace synchronous cold initialization with an awaited path.
- Existing C# test files under `UtilitiesCS.Test/OutlookObjects/Folder/` and `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs`.

#### Functions/classes/CLI commands impacted:

- `AppOlObjects.LoadFolderTreeService` and its lazy accessor.
- `OutlookFolderTreeService.GetSnapshotAsync`, `BuildAndPublishAsync`, and notification-driven refresh handling.
- `FolderTreeSnapshotBuilder.BuildSnapshotAsync`, `OutlookFolderHierarchyReader.ReadRecordsAsync`, and `ReadStoreAsync`.
- `FilterOlFoldersController` initialization and the UI event path that creates it.
- `IOutlookFolderTreeService.GetSnapshotAsync` remains source-compatible.

#### Data flow and validation changes:

1. A worker or UI caller requests a snapshot.
2. First composition and each live cache build or refresh execute on the captured Outlook STA dispatcher.
3. The reader performs COM-like adapter access and deadline yielding on that dispatcher; post-yield live traversal remains on the same dispatcher.
4. The service publishes the completed immutable snapshot under its existing synchronization rules, after which callers may continue without a dispatcher requirement.
5. The FilterOlFolders UI awaits cold initialization rather than blocking the dispatcher that must service the build.

#### Error handling and logging updates:

- Preserve existing cancellation, stale-cache, and build-failure behavior.
- Do not mask a missing or failed UI-dispatch operation with a thread-pool fallback; propagate it through the existing request failure path.
- No new logging is required unless the implementation introduces a pre-existing repository logging pattern for dispatcher-dispatch failure diagnostics.

#### Rollback/feature-flag considerations (if applicable):

No feature flag is required. The repair is limited to internal execution context and initialization behavior; rollback is the normal code rollback if the change proves incompatible.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- `IOutlookFolderTreeService.GetSnapshotAsync(FolderTreeRequest, CancellationToken)` retains its request and snapshot contract.
- The concrete service gains an internal construction-time dispatcher execution dependency or equivalent seam. Production wiring uses the captured `WpfUiDispatcher`; unit tests may inject a deterministic dispatcher fake.
- The service must not expose dispatcher selection through the public snapshot request.

### Implemented design and evidence

- `AppOlObjects` uses a worker-first `IUiDispatcher.InvokeAsync(Action)` composition protocol: the first caller creates the shared completion state, queues complete live composition to the captured Outlook dispatcher, and waits for that task without changing the existing `BeginInvoke` compatibility behavior. Composition failure, dispatcher failure, cancellation, and disposal complete the same terminal state without publishing a candidate service.
- Factory-created FilterOlFolders viewers are disposed or closed on initialization failure while preserving the original synchronous exception identity and parameter name. The delayed-snapshot path linearizes ArchiveRoot inspection, compatibility-view commit, viewer wiring, and snapshot-subscription attachment so a close or disposal cannot mutate a detached view.
- `OutlookFolderTreeService` keeps the M2 in-flight refresh-fault lifecycle and the M3 queued-cleanup and terminal-isolation behavior. Its authorization, candidate-disposal, and cleanup observers preserve a single terminal result while preventing cleanup-observer failures from escaping a completed request.
- Builder and reader traversal awaits retain the captured dispatcher until immutable snapshot publication. `WpfDispatcherYield` remains strict without a dispatcher. The ribbon H4 path awaits the asynchronous FilterOlFolders initialization and surfaces initialization failures through the task-returning path.
- P5 implementation and regression evidence: `evidence/regression-testing/remediation-cycle4-predecessor-reconciliation.2026-08-06T16-14.md`, `remediation-cycle4-testability-seam.2026-08-06T18-20.md`, and `remediation-cycle4-acceptance-criteria-mapping.2026-08-06T18-20.md`.
- Final QA evidence: `evidence/qa-gates/remediation-cycle4-csharpier.2026-08-06T18-33.md`, `remediation-cycle4-analyzers.2026-08-06T18-34.md`, `remediation-cycle4-nullable.2026-08-06T18-34.md`, `remediation-cycle4-mstest-coverage.2026-08-06T18-35.md`, `remediation-cycle4-coverage-and-quality-delta.2026-08-06T18-36.md`, and `remediation-cycle4-diff-check.2026-08-06T18-37.md`.
- Final QA passed without a coverage waiver: 6,166/6,166 tests passed; repository line coverage was 93,687/110,478 (84.8015%); changed production coverage was 879/881 (99.7730%).

#### Required configuration keys and defaults:

No new configuration keys or defaults are introduced.

#### Backward-compatibility expectations:

Existing callers, cache state semantics, request coverage, cancellation behavior, and notification invalidation behavior remain compatible. Worker callers remain supported but no longer execute live traversal directly.

#### Performance constraints (latency/throughput/memory):

The repair must retain cooperative `Dispatcher.Yield(DispatcherPriority.Background)` behavior so lengthy folder traversals continue to permit UI message processing. It must not create additional session-scoped notification sinks or duplicate in-flight cold builds.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access): The Outlook add-in initializes `UiThread.Dispatcher` before `ApplicationGlobals` is composed; unit tests can use in-process fakes and a dedicated STA dispatcher without Outlook.
- Constraints (budget, performance, compatibility): All touched C# files remain below 500 lines. The implementation uses MSTest, Moq, and FluentAssertions, and tests must not use Outlook, network resources, temporary files, timers, sleeps, or retry loops.
- External dependencies (services, libraries, releases): Existing .NET Framework 4.8.1 and `WindowsBase` references only.

## Data / API / Config Impact
- User-facing or API changes: None. The visible behavior changes only by eliminating the unmanaged dispatcher exception and avoiding a cold-load UI deadlock.
- Data or migration considerations: None. Folder snapshots and cache state are not migrated.
- Logging/telemetry updates (if any): No logging change is required by the current evidence.
- Compatibility notes (CLI flags, config schemas, versioning): No public configuration, CLI, or schema change is expected.

## Test Strategy
Research is sufficient to complete this specification. The implementation plan must treat the following as required tests, using deterministic fakes and no live Outlook dependency.

- Regression tests to add or update:
  - Add a worker-originated cold-build test to the existing `OutlookFolderTreeService` test surface. Start from a dispatcher-free worker and assert that service composition, every recording store/folder adapter access, and the continuation after a forced yield occur on a dedicated Outlook-STA dispatcher.
  - Extend `OutlookFolderHierarchyReaderTests.cs` and `FolderTreeSnapshotBuilderYieldTests.cs` to assert that access after a forced cooperative yield remains on the dispatcher thread.
  - Update the existing service concurrency, invalidation, disposal, cancellation, and exception-state tests to retain their current behavior with the dispatch seam.
  - Update `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs` to await cold initialization, assert the viewer is connected after snapshot acquisition, and assert no synchronous UI wait is used.
- Unit tests (MSTest) for the fixed behavior and boundaries: use MSTest with Moq and FluentAssertions. Do not add pytest tests.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): cancellation before dispatch, dispatch failure before a build starts, cache refresh after invalidation, concurrent callers coalescing one build, and a UI-initiated cold filter load.
- Error handling and logging verification: verify existing request failure and cancellation behavior; assert no `Task.Yield` fallback is selected for live traversal.
- Coverage impact and targets for changed lines/modules: repository-wide coverage remains at least 80%; new or modified behavior targets at least 90% coverage without reducing coverage on changed lines.
- Toolchain commands to run (format → lint → type-check → test): `dotnet tool run csharpier .`; analyzer `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; nullable `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`; then the repository MSTest command with code coverage. Restart the sequence if a step changes files or fails.
- Manual validation steps (if required): exercise a cold email-mining and FilterOlFolders request in an Outlook VSTO host after automated validation, if an appropriate Outlook test environment is available.


## Acceptance Criteria
- [x] A dispatcher-free worker can initiate a cold folder-tree request without `WpfDispatcherYield` throwing `InvalidOperationException`.
- [x] Service composition, notification-sink construction, every live hierarchy adapter access, and every post-yield continuation for a cold build or refresh execute on the captured Outlook STA dispatcher.
- [x] The production live traversal path uses `WpfDispatcherYield` on the captured dispatcher and does not select `Task.Yield`, a worker-local dispatcher, or caller-specific yield fallback logic.
- [x] The folder-tree service retains one session-scoped instance, coalesces concurrent cold requests, and preserves cancellation, stale/current, invalidation, publication, and disposal behavior.
- [x] FilterOlFolders cold initialization awaits the snapshot without synchronously blocking the UI dispatcher, and the viewer is wired only after snapshot acquisition.
- [x] Deterministic MSTest coverage proves worker-started cold build affinity, continuation affinity after a forced yield, service-composition and notification-sink affinity, and nonblocking cold filter initialization without Outlook, network, temporary files, sleeps, or retry loops.
- [x] The final C# toolchain passes in one uninterrupted final pass: CSharpier, analyzer build, nullable build, and MSTest with code coverage; changed behavior meets the repository coverage requirements.
- [x] The feature documentation records the final implementation decisions, validation evidence, and any approved deviation from this scope.

## Risks & Mitigations
- Technical or operational risks:
  - Dispatching only the cache build, rather than first composition, would leave live COM notification-subscription construction outside the STA boundary.
  - Retaining context suppression around deadline yields could move later live COM access off the captured dispatcher.
  - Synchronously waiting for a cold build on the UI dispatcher would deadlock once the build cooperatively yields.
  - Removing individual `Task.Run` callers would not cover existing or future worker-originated callers.
- Mitigations and rollbacks:
  - Test composition and notification-sink affinity separately from build affinity.
  - Test adapter access after a forced yield and retain strict `WpfDispatcherYield` behavior.
  - Convert only the FilterOlFolders cold initialization path to asynchronous UI flow and test it deterministically.
  - Keep dispatch ownership in the service rather than in callers; roll back the internal implementation if required without changing the public snapshot contract.

## Rollout & Follow-up
- Release/rollout steps: Deliver through the standard pull-request and CI workflow. No migration, feature flag, or configuration rollout is required.
- Post-fix monitoring or clean-up tasks: Confirm in an Outlook VSTO environment, when available, that cold email mining and FilterOlFolders no longer report the dispatcher exception. Review future folder-tree callers for attempts to bypass the service-owned dispatch boundary.
- Links: [Issue #420](https://github.com/drmoisan/TaskMaster/issues/420), [Issue #214](https://github.com/drmoisan/TaskMaster/issues/214), [PR #215](https://github.com/drmoisan/TaskMaster/pull/215), `artifacts/research/2026-08-04T19-02-folder-tree-dispatcher-thread-affinity-research.md`.
