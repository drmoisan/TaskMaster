# P9-T7 nonnumeric adapter accounting root cause

Timestamp: 2026-07-27T06-54
Command: `git cat-file -t 314358197`
Command: `git merge-base --is-ancestor 314358197 HEAD`
Command: `git cat-file -e origin/main:QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`
Command: `git grep -n 'ExcludeFromCodeCoverage' 314358197 -- QuickFiler/Viewers/ItemViewer.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`
Command: `git diff --unified=3 314358197..HEAD -- QuickFiler/Viewers/ItemViewer.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`
EXIT_CODE: 0

## Commands

```powershell
git cat-file -t 314358197
git merge-base --is-ancestor 314358197 HEAD
git cat-file -e origin/main:QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs
git grep -n 'ExcludeFromCodeCoverage' 314358197 -- QuickFiler/Viewers/ItemViewer.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs
git diff --unified=3 314358197..HEAD -- QuickFiler/Viewers/ItemViewer.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs
```

## Failed accounting artifact preservation

`nonnumeric-adapter-remediation-accounting.2026-07-27T06-34.md` remains unchanged with
SHA-256 `0E926C2C8CF7CB70CA23F38F174CAE8237EE15BEDF76F72B719F1E61862C35E6`.
It is failed historical accounting evidence: its assertion that no exclusion changed or
widened did not compare the required `origin/main` and P5-T104 baseline states, and it
incorrectly accounted excluded host-neutral bodies as direct adapters.

## Three-state provenance comparison

`314358197` resolves to a commit and is an ancestor of `HEAD`. `origin/main` does not
contain `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`; therefore all seven popup
exclusions are branch additions relative to `origin/main`. The P5-T104 baseline and the
current worktree contain the same seven attributes at these inclusive ranges:

| Method | Current inclusive range | P5-T104 baseline result |
| --- | --- | --- |
| `ShowOwnedPopup` | 97-102 | Present, unchanged |
| `CreateProductionControl` | 377-378 | Present, unchanged |
| `BeginProductionInitialization` | 380-385 | Present, unchanged |
| `ReadProductionCore` | 387-392 | Present, unchanged |
| `BeginProductionNavigation` | 394-419 | Present, unchanged |
| `DisposeProductionSurface` | 421-423 | Present, unchanged |
| `NavigateToDocument` | 431-478 | Present, unchanged |

The ItemViewer type-level exclusion is inherited from `QuickFiler/Viewers/ItemViewer.cs:20`
in `origin/main`, P5-T104, and the current worktree. `ItemViewer.Breadcrumb.cs` has grown
from 141 physical lines on `origin/main` to 399 physical lines currently. The method-level
exclusions `AttachBreadcrumbWebViewAsync` (71-73) and
`CreateCollapsedBreadcrumbCandidate` (84-116) are branch additions relative to
`origin/main`, are already present at P5-T104, and are unchanged after that baseline.

The P5-T104-to-HEAD comparison has no added or widened exclusion. This is the controlling
comparison: after P5-T104 only removal or narrowing is permitted. `origin/main` establishes
provenance but is not used as proof that the branch-local baseline was unchanged.

## Omitted host-neutral bodies and correction decision

The prior accounting omitted the current ItemViewer touchpoints 25-30, 33-37, 58-59,
75-82, 118-153, 169-186, 189-230, 244-254, 256-275, 280-289, 299-397, and 365-375.
It also treated the full `NavigateToDocument` body as a direct adapter even though it owns
argument validation, readiness construction, event translation, detach handling, and
failure cleanup.

The required correction is therefore:

1. Retain the ItemViewer type exclusion only for wider legacy UI ownership.
2. Remove the two ItemViewer.Breadcrumb method exclusions.
3. Extract all breadcrumb host-neutral selector/configuration/lifecycle branches and the
   host-neutral `NavigateToDocument` body into unexcluded
   `BreadcrumbItemViewerLifecycleCoordinator.cs`.
4. Retain only seven thin direct WebView2/WinForms popup-call exclusions.
5. Add controlled seams for navigation events/readiness, messenger construction/disposal,
   collapsed-candidate creation, geometry, popup focus, and focus guards.

No additional exclusion or widening is authorized. An injected high-level host or provider
does not prove direct adapter execution; tests must instead enter the production seam and
assert its direct probe/operation.

## Required deterministic branch mapping

| Required seam or branch | P9-T13 deterministic test |
| --- | --- |
| Host replacement, exact event identity, messenger replacement/disposal | `HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder` |
| Collapsed-candidate failure cleanup | `CandidateFailure_CleansMessengerAndReadiness` |
| Reset/dispose late callback invalidation | `ResetDispose_LateCallbackDoesNotReattach` |
| Selector delegation | `SelectorDelegation_UsesCoordinator` |
| Queued geometry and focus guards on creator thread | `QueuedGeometryAndFocusGuards_RunOnCreatorThread` |
| Production core absent/present probes | `CoreProbe_AbsentAndPresentPaths` |
| Initializer throw and null-task paths | `Initializer_ThrowAndNullTaskPaths` |
| Messenger construction failure disposes readiness | `MessengerConstructionFailure_DisposesReadiness` |
| Navigation events, detachment, and throw cleanup | `NavigationBinder_TranslatesDetachesAndCleansOnThrow` |
| Two-resource cleanup preserves first failure after all cleanup | `TwoResourceCleanup_ReportsFirstFailureAfterAllCleanup` |

The specified tests must use an explicit queued synchronization context, drained by its
creator test thread; ambient/base `SynchronizationContext` behavior is not acceptable.

Result: PASS. The current accounting defect, its exact provenance, the baseline constraint,
and the bounded mechanism-based correction are evidenced. No implementation, requirements,
project, policy, checkpoint, filter, or coverage configuration file was changed.
