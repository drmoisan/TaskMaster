Timestamp: 2026-08-06T16-14
Command: Read `remediation-cycle3-pass3-targeted-regressions.2026-08-05T05-29.md`, inspect the named current test methods, and compare the affected source and test files with `git diff --`.
EXIT_CODE: 0
Output Summary: P5-T25, P5-T26, and P5-T27 each reconcile to the current P5-T38 serialized deterministic suite. Its exact two-assembly command passed 90/90 selected tests, and the affected controller and named test files have no uncommitted changes since that run. Each predecessor is checked individually below; no history was batch-checked.

## Shared current green evidence

- Evidence: `remediation-cycle3-pass3-targeted-regressions.2026-08-05T05-29.md`
- Assembly: `UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll`
- Result: 90/90 selected tests passed with `/InIsolation` and the recorded serialized `Workers=1`, `Scope=ClassLevel` runsettings.
- Exact selected class: `UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerRefreshDisposalTests` (including the lifecycle-races partial).
- Determinism boundary: mock-only services, dispatchers, globals, and viewers; no live Outlook, production viewer, or production message loop.

## P5-T25

Literal condition: candidate-view ownership before composition, exact-once synchronous-failure disposal with original exception identity, and terminal-state rechecks after `ArchiveRoot` and compatibility-view boundaries before commit/subscription.

| Required behavior | Current deterministic test | Assembly and result |
| --- | --- | --- |
| Null-globals candidate disposal | `CreateAsync_NullGlobals_DisposesFactoryViewer` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |
| Composition-fault disposal and original identity | `CreateAsync_CompositionFault_DisposesFactoryViewerAndRethrowsOriginal` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |
| Initial `ArchiveRoot` close before compatibility view | `InitialArchiveRootClose_BeforeCompatibilityView_DoesNotCommit` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |
| Refresh `ArchiveRoot` close before compatibility view | `RefreshArchiveRootClose_BeforeCompatibilityView_DoesNotCommit` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |

Conclusion: PASS. The mapped tests directly prove the literal P5-T25 ownership, identity, and terminal-recheck conditions.

## P5-T26

Literal condition: linearize candidate-view commit and `SnapshotChanged` subscription against disposal; dispatch and await initial/refresh work through the captured dispatcher without fire-and-forget or fallback.

| Required behavior | Current deterministic test | Assembly and result |
| --- | --- | --- |
| Candidate commit race | `DisposeDuringCandidateViewCommit_DoesNotRetainViewOrSubscription` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |
| Subscription race | `DisposeDuringSnapshotSubscription_DoesNotRetainViewOrSubscription` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |
| Queued refresh operations are awaited | `CloseDuringRefresh_OnPumpingSta_AwaitsEveryQueuedControllerOperation` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |
| Worker event refresh uses captured STA and observes original fault | `SnapshotChanged_FromWorker_RefreshesOnCapturedStaAndObservesOriginalFault` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |

Conclusion: PASS. The mapped tests prove both disposal interleavings and the captured-dispatcher observed-error boundary.

## P5-T27

Literal condition: P5-T9 and P5-T15 through P5-T17 pass with independent counts, exact exception/parameter identities, getter/subscription ordering, delayed-snapshot close races, and both task-signal interleavings.

| Required behavior | Current deterministic test(s) | Assembly and result |
| --- | --- | --- |
| Independent create/show/close/dispose observations | `CreateAsync_NullGlobals_DisposesFactoryViewer`; `CreateAsync_CompositionFault_DisposesFactoryViewerAndRethrowsOriginal` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |
| Null-globals parameter and original synchronous exception identity | `CreateAsync_NullGlobals_DisposesFactoryViewer`; `CreateAsync_CompositionFault_DisposesFactoryViewerAndRethrowsOriginal` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |
| Getter-before-subscription add-fault ordering | `CreateAsync_SynchronousFolderTreeServiceFault_ClosesFactoryViewerAndPreservesOriginalException` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |
| Initial and refresh delayed-snapshot close boundaries | `InitialArchiveRootClose_BeforeCompatibilityView_DoesNotCommit`; `RefreshArchiveRootClose_BeforeCompatibilityView_DoesNotCommit` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |
| Candidate and subscription task-signal interleavings | `DisposeDuringCandidateViewCommit_DoesNotRetainViewOrSubscription`; `DisposeDuringSnapshotSubscription_DoesNotRetainViewOrSubscription` | `UtilitiesCS.Test.dll`; included in 90/90 PASS |

Conclusion: PASS. The mapped tests preserve zero retained handler, no committed view, and no post-dispose refresh/application notification in both specified interleavings.
