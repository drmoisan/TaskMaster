# Cycle 3 / Pass 3 controlling acceptance-criteria mapping

Timestamp: 2026-08-05T05:35:00-04:00
Command: `Get-ChildItem evidence -Recurse -Filter *.md` schema sweep; `git diff origin/main -- '*.cs'`; current C# line-count, compile-entry, forbidden-pattern, and global-mutation inspections.
EXIT_CODE: 0
Output Summary: Every P5-owned root regression maps to current passing evidence, including M3. P5-T38 passed 90/90 with the required serialized runsettings. AC7/CR-007 remain owned by Phase 6 and AC8 by Phase 7, so Phase 6 is unblocked while Phase 7 remains blocked. This is cycle 3/pass 3; no cycle 4 is authorized.

## Evidence schema reconciliation

The canonical feature evidence root contains 121 Markdown artifacts. A literal-field scan confirmed zero artifacts missing `Timestamp:`, `Command:`, `EXIT_CODE:`, or `Output Summary:`. The 26 historical artifacts that lacked one or more fields were amended from their recorded commands and outcomes. Historical expected-red and non-command disposition records remain historical; no result was reconstructed or replaced.

## Controlling P5 functional mapping

P5-T39 controls the lifecycle, dispatcher, ownership, and terminal-state findings. A `PASS` below means current evidence supports that P5-owned requirement. AC7/CR-007 and AC8 are listed as downstream-owned gates, not as a P5 completion assertion; their source checkboxes remain unchecked.

| Root finding | Current red/green evidence and result | Status |
| --- | --- | --- |
| H1 — worker-first UI composition and terminal dispatcher operations | Red: `appolobjects-dispatcher-task-shutdown-fail-before.2026-08-05T00-42.md`. Green: `appolobjects-dispatcher-task-shutdown-green.2026-08-05T00-55.md`, `appolobjects-setup-retry-linearization-green.2026-08-05T02-12.md`, and P5-T38 05-29. The controlled per-invocation dispatcher proves observed terminal tasks; already-terminal and pending cancellation/fault transitions; faulted `OperationCanceledException` remains faulted; null-task reset/retry; exact terminal token/exception identity; terminal-hook signal before contained hook failure; transactional detach for factory/thread-check failure; retry; and late-callback discard with zero composition, load, publication, or newer-ownership overwrite. `InvokeAsync` is used and `BeginInvoke` remains unused for worker-first composition. | PASS |
| H2 — factory/viewer ownership and synchronous composition failure | Red: `filter-viewer-ownership-contract-rerun-fail-before.2026-08-05T00-18.md`. Green: `filter-controller-lifecycle-green.2026-08-05T02-15.md` and P5-T38 05-29. The current cases separately observe create/show/close/dispose counts, exact null-globals parameter identity, original synchronous exception identity, and the service getter occurring before the `FormClosed` add fault with zero retained trackable-service `SnapshotChanged` handlers. | PASS |
| H3 — worker notification refresh and controller close/commit races | Red: `filter-worker-snapshotchanged-fail-before.2026-08-04T21-18.md`, `filter-initial-archiveroot-close-fail-before.2026-08-04T23-26.md`, `filter-refresh-archiveroot-close-fail-before.2026-08-04T23-50.md`, and `filter-view-commit-and-subscription-race-fail-before.2026-08-05T00-36.md`. Green: `filter-worker-snapshotchanged-pass.2026-08-04T21-49.md`, `filter-controller-lifecycle-green.2026-08-05T02-15.md`, and P5-T38 05-29. These prove initial/refresh delayed-snapshot `ArchiveRoot` terminal rechecks plus candidate-view and stored-subscription task-signal barriers. | PASS |
| H4 — ribbon `async void` fault boundary | Red: `ribbon-h4-exact-once-reporter-fail-before.2026-08-05T04-13.md`. Green: `ribbon-h4-exact-once-reporter-green.2026-08-05T04-14.md` and P5-T38 05-29. An incomplete initialization reports zero faults before completion, the original fault is reported exactly once, delayed success reports zero, legacy identity is preserved, and a throwing reporter is contained. | PASS |
| M1 — setup retry and publication/disposal linearization | Red: `appolobjects-setup-retry-linearization-fail-before.2026-08-05T01-06.md`. Green: `appolobjects-setup-retry-linearization-green.2026-08-05T02-12.md` and P5-T38 05-29. Exact setup failure resets ownership, a fresh retry publishes one service, pre-linearization disposal has no externally visible incomplete service, coalesced callers receive one service, and after-getter disposal is terminal. | PASS |
| M2 — retained notification, scheduled-fault, and subscriber lifecycle | Red: `outlook-folder-tree-event-lifecycle-fail-before.2026-08-05T04-51.md`. Green: `outlook-folder-tree-event-lifecycle-green.2026-08-05T04-56.md` and P5-T38 05-29. Retained notifications and in-flight scheduled-refresh faults are suppressed after disposal; copied `SnapshotChanged` callbacks run outside `_gate` and stop before later subscribers after terminal state. | PASS |
| M3 — disposal cleanup linearization and exact-once staged fault follow-through | Red: `reentrant-dispose-cleanup-stage-fault-fail-before.2026-08-05T04-18.md`. Green: `reentrant-dispose-cleanup-stage-fault-green.2026-08-05T04-50.md`, `outlook-folder-tree-event-lifecycle-green.2026-08-05T04-56.md`, and P5-T38 05-29. The authorized traversal fixture proves zero post-cleanup COM access; no post-dispose publication, scheduling, event delivery, or notification reattachment; exact-once queued cleanup; every cleanup stage attempted after an earlier stage fault; and exact original stage-fault delivery once. | PASS |

## Acceptance criteria

| ID | Current evidence | P5-T39 status |
| --- | --- | --- |
| AC1 | Worker-originated cold-build, strict-yield, and P5-T38 serialized evidence: `worker-cold-build-fail-before.2026-08-04T19-21.md` paired with current P5-T38 05-29. | PASS |
| AC2 | H1, M1, M2, WPF captured-instance evidence `wpf-ui-dispatcher-captured-instance-green.2026-08-04T23-24.md`, and P5-T38 05-29 cover composition, refresh, notification, and captured-STA dispatch. | PASS |
| AC3 | Current full changed-C# inspection found no `Task.Yield`, worker-local dispatcher, or caller-selected fallback in the production live traversal path; strict-yield tests are selected by P5-T38 05-29. | PASS |
| AC4 | H1, M1, M2, and M3 current green evidence plus P5-T38 05-29 cover the one-service, coalescing, cancellation, invalidation, publication, and disposal contract. | PASS |
| AC5 | H2/H3 current green evidence and P5-T38 05-29 cover awaited initialization, exact fault propagation, close-before-completion, and no closed-view wiring. | PASS |
| AC6 | P5-T38 05-29 passed 90/90 deterministic selected tests using fakes and dedicated STA hosts only; the artifact records no live Outlook, production UI, network, temporary file, sleep, polling, timer, or retry-loop dependency. | PASS |
| AC7 | Final full C# toolchain and coverage are Phase 6-owned and have not been rerun in this cycle. The AC remains unchecked and is not asserted by P5-T39. | DEFERRED TO P6 |
| AC8 | Final feature-document reconciliation is Phase 7-owned. The AC remains unchecked and is not asserted by P5-T39. | DEFERRED TO P7 |

## Code-review findings

| ID | Current evidence | P5-T39 status |
| --- | --- | --- |
| CR-001 | H1 current mapping and P5-T38 05-29 prove worker-first composition does not synchronously hold the service gate across dispatch and discard stale callbacks. | PASS |
| CR-002 | M3 current mapping and P5-T38 05-29 prove terminal disposal blocks post-dispose publication, event delivery, scheduling, and notification reattachment. | PASS |
| CR-003 | M2/M3 current mapping and P5-T38 05-29 prove notification cleanup and terminal suppression use the captured dispatcher path. | PASS |
| CR-004 | H2/H3 current mapping and P5-T38 05-29 prove viewer lifecycle, close-before-load, and composition-fault behavior. | PASS |
| CR-005 | H4 current mapping and P5-T38 05-29 prove defined and contained ribbon failure policy. | PASS |
| CR-006 | `wpf-ui-dispatcher-captured-instance-green.2026-08-04T23-24.md` and P5-T38 05-29 prove captured-instance `Invoke`, `BeginInvoke`, and both `InvokeAsync` overload behaviors. | PASS |
| CR-007 | Comparable full coverage and changed-behavior thresholds are Phase 6-owned. This finding remains unasserted until P6-T4/P6-T5; no waiver or exception is recorded. | DEFERRED TO P6 |

## Required capacity, test-seam, and scope evidence

- P5-T14 literal-command evidence is retained in `filter-controller-lifecycle-races-capacity.2026-08-05T00-18.md`: it records the two exact `Get-Content` line-count commands, the exact `Select-String` compile-entry count command, and the exact scoped `git diff --check` command. Current inspection is 497 lines for the original controller test, 234 for the lifecycle-races partial, exactly one adjacent compile entry, and zero `[TestClass]` attributes in the partial; both remain within their 500/300 limits.
- `BlockingUiDispatcher` and `QueuedStaDispatcher` now implement the existing `InvokeAsync(Action): Task` path. `appolobjects-setup-retry-linearization-green.2026-08-05T02-12.md` records one `InvokeAsync`, zero `BeginInvoke` for worker-first composition, and P5-T38 selects both AppOlObjects fixtures.
- P5-T6 instance-local and dedicated-STA proof is retained in `filter-p5t6-instance-dispatcher.2026-08-04T23-04.md` and `filter-p5t6-sta-hang-green.2026-08-04T23-24.md`; P5-T38 reruns the original and lifecycle-races controller partials.
- Neither AppOlObjects test source contains `UiThread._dispatcher`, `UiDispatcherScope`, `GetFolderTreeServiceGate`, reflection binding flags, or `GetField`; no process-global dispatcher mutation remains.
- The four changed project files are `TaskMaster.Test/TaskMaster.Test.csproj`, `TaskMaster/TaskMaster.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, and `UtilitiesCS/UtilitiesCS.csproj`. The newly added sources have one matching compile entry each: `AppOlObjectsFolderTreeServiceLifecycleTests.cs`, `TryFunctionalityInConstructionTests.cs`, `AppOlObjects.FolderTreeService.cs`, `FilterOlFoldersControllerInitializationTests.cs`, both `FilterOlFoldersControllerRefreshDisposalTests` partials, `OutlookFolderTreeServiceTraversalCancellationTests.cs`, `WpfUiDispatcherTests.cs`, and `FilterOlFoldersController.Lifecycle.cs`.

## Current changed C# capacity inventory

| File | Lines |
| --- | ---: |
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | 490 |
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` | 490 |
| `TaskMaster.Test/Ribbon/TryFunctionalityInConstructionTests.cs` | 188 |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 448 |
| `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` | 418 |
| `TaskMaster/Ribbon/RibbonViewer.cs` | 487 |
| `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs` | 296 |
| `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs` | 489 |
| `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerInitializationTests.cs` | 492 |
| `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.cs` | 497 |
| `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.LifecycleRaces.cs` | 234 |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderYieldTests.cs` | 149 |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyReaderTests.cs` | 433 |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs` | 190 |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceDisposalTests.cs` | 440 |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceInvalidationTests.cs` | 435 |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceTraversalCancellationTests.cs` | 498 |
| `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | 39 |
| `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs` | 171 |
| `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs` | 191 |
| `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs` | 481 |
| `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotBuilder.cs` | 84 |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs` | 274 |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs` | 497 |
| `UtilitiesCS/Threading/IUiDispatcher.cs` | 43 |
| `UtilitiesCS/Threading/WpfUiDispatcher.cs` | 63 |

## Decision

P5-T39 PASS: all P5-owned H1-H4, M1-M3, AC1-AC6, and CR-001 through CR-006 have current passing evidence. M3 is explicitly PASS. AC7/CR-007 must be evaluated only by Phase 6; AC8 must be evaluated only by Phase 7. Phase 6 may start at P6-T1. Phase 7 remains blocked until P6 and its own documentation gate are complete. No cycle 4 is authorized.
