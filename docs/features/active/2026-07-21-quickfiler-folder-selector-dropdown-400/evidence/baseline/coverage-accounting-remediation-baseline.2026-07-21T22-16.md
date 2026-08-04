Timestamp: 2026-07-21T22-16Z
Command: Parse `coverage-remediation-baseline.2026-07-21T22-13.cobertura.xml`; deduplicate `(filename,line)` sequence points by maximum hits; parse `git diff --no-color --unified=0 df5ad49c909f6b739edef45d0336151f44e827a6 -- '*.cs'` and `git diff --no-color --unified=0 HEAD -- QuickFiler/Viewers/BreadcrumbDropDownHost.cs`; enumerate the untracked helper directly; and reconcile complete Roslyn AST-to-source member attribution from `evidence/qa-gates/coverage-delta.2026-07-21T21-18.md` against the fresh line-hit map.
EXIT_CODE: 0
Output Summary: Fresh repository coverage is 89,240/106,048 = 84.1506%. Merge-base changed/new measurable production coverage is 1,141/1,143 = 99.8250%. Current uncommitted remediation host/helper coverage is 140/142 = 98.5915%. Complete member accounting identifies two pre-implementation router members below 90%: `ToggleAsync` at 87.5000% and `SubfolderResponseAsync` at 81.8182%. These are baseline remediation debt, not a passing threshold result. Bounded adapters are separately nonnumeric and have deterministic seams.

## Repository and changed-line accounting

| Scope | Covered/valid | Coverage | Result |
|---|---:|---:|---|
| Fresh repository | 89,240/106,048 | 84.1506% | PASS, >=80% |
| Merge-base modified tracked hunks | 367/367 | 100.0000% | PASS, baseline was 42/46 = 91.3043% |
| Merge-base tracked changed/new production | 1,134/1,136 | 99.8239% | PASS |
| Untracked helper measurable source | 7/7 | 100.0000% | PASS |
| Merge-base aggregate changed/new measurable production | 1,141/1,143 | 99.8250% | PASS |
| Current remediation, `HEAD` to worktree host/helper | 140/142 | 98.5915% | PASS |

The two uncovered current-remediation lines are `BreadcrumbDropDownHost.cs:230` and `:328`. Their complete members remain above 97%. No required number is unavailable.

## Measurable selector types

| Type | Covered/valid | Coverage |
|---|---:|---:|
| `BreadcrumbBridgeCoordinator` | 264/264 | 100.0000% |
| `BreadcrumbDropDownHost` | 307/309 | 99.3528% |
| `BreadcrumbMessengerHub` | 141/141 | 100.0000% |
| `BreadcrumbMessengerHub.Attachment` | 10/10 | 100.0000% |
| `BreadcrumbMessengerHub.CachedState` | 5/5 | 100.0000% |
| `BreadcrumbPopupPlacement` | 44/44 | 100.0000% |
| `BreadcrumbPopupPlacementResult` | 4/4 | 100.0000% |
| `BreadcrumbRenderProjection` | 85/85 | 100.0000% |
| `BreadcrumbCellRender` | 12/12 | 100.0000% |
| `BreadcrumbRowRender` | 20/20 | 100.0000% |
| `BreadcrumbSubfolderRender` | 6/6 | 100.0000% |
| `BreadcrumbSelectionSession` | 135/135 | 100.0000% |
| `BreadcrumbSelectorActivationMessage` | 11/11 | 100.0000% |
| `BreadcrumbSelectorKeyMessage` | 5/5 | 100.0000% |
| `BreadcrumbSelectorMessageSerializer` | 85/85 | 100.0000% |
| `BreadcrumbSelectorToggleMessage` | 1/1 | 100.0000% |
| `BreadcrumbSelectorViewMessage` | 19/19 | 100.0000% |
| `BreadcrumbStateModel` | 88/88 | 100.0000% |
| `BreadcrumbStateRow` | 135/135 | 100.0000% |
| `FolderBreadcrumbBridgeRouter` | 277/282 | 98.2270% |
| `BreadcrumbWebViewSurfaceFactory`, measurable span | 7/7 | 100.0000% |

## Every measurable current uncommitted host/helper member

| Member | Covered/valid | Coverage |
|---|---:|---:|
| `BreadcrumbDropDownHost` production constructor | 9/9 | 100.0000% |
| `BreadcrumbDropDownHost` legacy-factory constructor | 9/9 | 100.0000% |
| `BreadcrumbDropDownHost` readiness-aware constructor and initializers | 29/29 | 100.0000% |
| `ControlHost.get` | 1/1 | 100.0000% |
| `PopupMessenger.get` | 1/1 | 100.0000% |
| `IsOpen.get` | 1/1 | 100.0000% |
| `OpenAsync` | 24/24 | 100.0000% |
| `CompleteOpenAsync` | 29/29 | 100.0000% |
| `OpenCoreAsync` | 39/40 | 97.5000% |
| `Close` | 6/6 | 100.0000% |
| `SetTheme` | 6/6 | 100.0000% |
| `Reset` | 8/8 | 100.0000% |
| `Dispose` | 12/12 | 100.0000% |
| `EnsureSurfaceAsync` | 44/45 | 97.7778% |
| `InvalidateLifecycle` | 6/6 | 100.0000% |
| `IsCurrent` | 1/1 | 100.0000% |
| `WaitForReadinessAsync` | 7/7 | 100.0000% |
| `RejectCreatedSurface` | 6/6 | 100.0000% |
| `CompleteClose` | 16/16 | 100.0000% |
| `OnDropDownClosed` | 6/6 | 100.0000% |
| `FinishClose` | 5/5 | 100.0000% |
| `RestoreAfterOpenFailure` | 4/4 | 100.0000% |
| `DisposeSurface` | 16/16 | 100.0000% |
| `ThrowIfDisposed` | 4/4 | 100.0000% |
| `NormalizeFactory`, including returned lambda | 17/17 | 100.0000% |
| `NewCompletionSource` | 1/1 | 100.0000% |
| `BreadcrumbWebViewSurfaceFactory.Create(IWebViewCoreInitializer, string)` | 7/7 | 100.0000% |

## Complete merge-base selector member exceptions

Complete AST-to-source attribution includes async state-machine bodies and returned lambdas that Cobertura `<methods>` summaries omit. All measurable changed/new selector members are 100.0000% except the following five members:

| Member | Covered/valid | Coverage | Uncovered | Baseline result |
|---|---:|---:|---|---|
| `FolderBreadcrumbBridgeRouter.ToggleAsync` | 14/16 | 87.5000% | 322-323 | BELOW 90%; remediation debt |
| `FolderBreadcrumbBridgeRouter.ArrowAsync` | 21/22 | 95.4545% | 356 | PASS |
| `FolderBreadcrumbBridgeRouter.SubfolderResponseAsync` | 9/11 | 81.8182% | 407-408 | BELOW 90%; remediation debt |
| `BreadcrumbDropDownHost.OpenCoreAsync` | 39/40 | 97.5000% | 230 | PASS |
| `BreadcrumbDropDownHost.EnsureSurfaceAsync` | 44/45 | 97.7778% | 328 | PASS |

The Phase 2 and Phase 6 router batches must raise both below-threshold members to at least 90% in final accounting. No unavailable or below-threshold number is classified as PASS.

## Bounded nonnumeric adapters

- `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:31-116`, `CreateSurfaceAsync`: direct WebView2 construction, core initialization, correlated navigation-readiness coordination, handler cleanup, and partial-control disposal. Deterministic coverage is provided through `BreadcrumbDropDownReadinessTests`, `BreadcrumbDropDownLifecycleConcurrencyTests`, and `BreadcrumbDropDownCoverageThresholdTests` using injected factory/readiness seams.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:477-482`, `ShowOwnedPopup`: direct WinForms popup show adapter. Deterministic ownership/placement/show-call behavior is covered through host/integration fakes.
- `QuickFiler/Viewers/ItemViewer.cs:20` applies a pre-existing class-level exclusion to the WinForms `ItemViewer` partial type, including the changed `ItemViewer.Breadcrumb.cs` boundary. This is recorded as baseline nonnumeric scope and is not a new exclusion. Its selector/lifecycle behavior must remain verified through the injected ItemViewer/controller/host seam tests; it cannot be silently counted as numeric coverage.

No new class-level exclusion was added, no host-neutral selector logic is placed inside the two new method-level adapter exclusions, and no coverage threshold/filter/configuration waiver is present.
