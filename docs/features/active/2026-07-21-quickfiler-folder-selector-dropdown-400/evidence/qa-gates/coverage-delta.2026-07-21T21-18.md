# Final Numeric Coverage Delta

Timestamp: 2026-07-21T21-18Z
Run Identity: `final-pass-2026-07-21T21-07Z`
Command: Parse the baseline and final Cobertura reports; deduplicate `(filename,line)` sequence points using maximum hits; parse `git diff --no-color --unified=0 df5ad49c909f6b739edef45d0336151f44e827a6 -- '*.cs'`; and use Roslyn syntax spans to assign every current source sequence point to the innermost type and complete member, including async bodies and returned lambdas
EXIT_CODE: 0
Output Summary: Repository coverage is 84.1647%. Modified tracked-hunk coverage improves to 100%. Changed/new measurable production coverage is 99.8250%. All 21 measurable selector types and all 27 measurable host/helper members are at least 90%; the minimum member result is 97.5000%.

BaselineCommitSHA: `df5ad49c909f6b739edef45d0336151f44e827a6`

- Baseline Cobertura: `evidence/baseline/coverage-baseline.2026-07-21T16-00.cobertura.xml`, SHA-256 `b78b5f189bd47fe0ec4aa92d0a301336dd6f73bf14c42235330d19eb18bb43c4`.
- Final Cobertura: `evidence/qa-gates/coverage-final.2026-07-21T21-09.cobertura.xml`, SHA-256 `6d44e4ba3cf9c5fbc3d37b2bf43ffc540c618309955861b55aa2b09a6177c1f0`.

## Accounting Method

Cobertura filenames were normalized and duplicate source-line entries were merged using their maximum hit count. Added and modified production line ranges came from the zero-context base-to-worktree diff. Test-project paths were excluded. The untracked new helper was enumerated directly because it does not appear in `git diff` without `--no-index`.

Roslyn parsed the current source. Each deduplicated sequence point was assigned to the innermost type and complete member span. This captures lowered async bodies and lambdas that Cobertura's `<methods>` summaries omit. The `NormalizeFactory` span is lines 454-471 and therefore includes its returned async lambda. Source spans carrying `[ExcludeFromCodeCoverage]` were enumerated separately as nonnumeric and were not assigned an invented numeric result.

## Repository and Changed-Scope Results

| Scope | Baseline | Final | Delta | Result |
|---|---:|---:|---:|---|
| Repository line coverage | 87,397/104,178 = 83.8920% | 89,255/106,048 = 84.1647% | +1,858 covered; +1,870 valid; +0.2727 percentage points | PASS, at least 80% |
| Modified tracked production hunks | 42/46 = 91.3043% | 367/367 = 100.0000% | +8.6957 percentage points | PASS, no regression |
| Tracked changed/new production executable lines | No aggregate new-file baseline | 1,134/1,136 = 99.8239% | Two uncovered host lines | PASS, at least 90% |
| New helper measurable lines | Not present | 7/7 = 100.0000% | No uncovered line | PASS |
| All changed/new measurable production lines | No aggregate new-file baseline | 1,141/1,143 = 99.8250% | Two uncovered host lines | PASS, at least 90% |

The baseline modified-hunk misses were `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:134` and `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:105,107,108`. Final modified tracked hunks have no uncovered sequence point. The only uncovered changed/new measurable lines are `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:230,328`; their containing members remain above 97%.

Modified tracked-hunk detail:

| File | Baseline | Final |
|---|---:|---:|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 0/1 | 13/13 |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 9/12 | 164/164 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs` | 2/2 | 12/12 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 1/1 | 68/68 |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 30/30 | 110/110 |

## Measurable Selector Types

| Type | Covered/valid | Coverage | Uncovered | Result |
|---|---:|---:|---|---|
| `BreadcrumbBridgeCoordinator` | 264/264 | 100.0000% | None | PASS |
| `BreadcrumbDropDownHost` | 307/309 | 99.3528% | 230, 328 | PASS |
| `BreadcrumbMessengerHub` | 141/141 | 100.0000% | None | PASS |
| `BreadcrumbMessengerHub.Attachment` | 10/10 | 100.0000% | None | PASS |
| `BreadcrumbMessengerHub.CachedState` | 5/5 | 100.0000% | None | PASS |
| `BreadcrumbPopupPlacement` | 44/44 | 100.0000% | None | PASS |
| `BreadcrumbPopupPlacementResult` | 4/4 | 100.0000% | None | PASS |
| `BreadcrumbRenderProjection` | 85/85 | 100.0000% | None | PASS |
| `BreadcrumbCellRender` | 12/12 | 100.0000% | None | PASS |
| `BreadcrumbRowRender` | 20/20 | 100.0000% | None | PASS |
| `BreadcrumbSubfolderRender` | 6/6 | 100.0000% | None | PASS |
| `BreadcrumbSelectionSession` | 135/135 | 100.0000% | None | PASS |
| `BreadcrumbSelectorActivationMessage` | 11/11 | 100.0000% | None | PASS |
| `BreadcrumbSelectorKeyMessage` | 5/5 | 100.0000% | None | PASS |
| `BreadcrumbSelectorMessageSerializer` | 85/85 | 100.0000% | None | PASS |
| `BreadcrumbSelectorToggleMessage` | 1/1 | 100.0000% | None | PASS |
| `BreadcrumbSelectorViewMessage` | 19/19 | 100.0000% | None | PASS |
| `BreadcrumbStateModel` | 88/88 | 100.0000% | None | PASS |
| `BreadcrumbStateRow` | 135/135 | 100.0000% | None | PASS |
| `FolderBreadcrumbBridgeRouter` | 277/282 | 98.2270% | 322, 323, 356, 407, 408 | PASS |
| `BreadcrumbWebViewSurfaceFactory`, measurable span | 7/7 | 100.0000% | None | PASS |

Every measurable dedicated selector type is at least 98.2270%. The broad pre-existing `QfcItemController` is not a dedicated new selector type; its changed measurable hunk is 13/13. Interfaces, enums, abstract members, and auto-properties without independent sequence points remain nonnumeric declarations.

## Every Measurable `BreadcrumbDropDownHost` Member

| Member | Covered/valid | Coverage | Uncovered | Result |
|---|---:|---:|---|---|
| Production constructor | 9/9 | 100.0000% | None | PASS |
| Legacy-factory constructor | 9/9 | 100.0000% | None | PASS |
| Readiness-aware constructor, including instance initializer points at lines 32 and 128 | 29/29 | 100.0000% | None | PASS |
| `ControlHost.get` | 1/1 | 100.0000% | None | PASS |
| `PopupMessenger.get` | 1/1 | 100.0000% | None | PASS |
| `IsOpen.get` | 1/1 | 100.0000% | None | PASS |
| `OpenAsync` | 24/24 | 100.0000% | None | PASS |
| `CompleteOpenAsync` | 29/29 | 100.0000% | None | PASS |
| `OpenCoreAsync` | 39/40 | 97.5000% | 230 | PASS |
| `Close` | 6/6 | 100.0000% | None | PASS |
| `SetTheme` | 6/6 | 100.0000% | None | PASS |
| `Reset` | 8/8 | 100.0000% | None | PASS |
| `Dispose` | 12/12 | 100.0000% | None | PASS |
| `EnsureSurfaceAsync` | 44/45 | 97.7778% | 328 | PASS |
| `InvalidateLifecycle` | 6/6 | 100.0000% | None | PASS |
| `IsCurrent` | 1/1 | 100.0000% | None | PASS |
| `WaitForReadinessAsync` | 7/7 | 100.0000% | None | PASS |
| `RejectCreatedSurface` | 6/6 | 100.0000% | None | PASS |
| `CompleteClose` | 16/16 | 100.0000% | None | PASS |
| `OnDropDownClosed` | 6/6 | 100.0000% | None | PASS |
| `FinishClose` | 5/5 | 100.0000% | None | PASS |
| `RestoreAfterOpenFailure` | 4/4 | 100.0000% | None | PASS |
| `DisposeSurface` | 16/16 | 100.0000% | None | PASS |
| `ThrowIfDisposed` | 4/4 | 100.0000% | None | PASS |
| `NormalizeFactory`, including returned lambda | 17/17 | 100.0000% | None | PASS |
| `NewCompletionSource` | 1/1 | 100.0000% | None | PASS |

These 26 measurable members sum exactly to the host's 307/309 source points. `ShowOwnedPopup`, lines 477-482, remains a separately enumerated nonnumeric direct WinForms adapter.

## Every Measurable `BreadcrumbWebViewSurfaceFactory` Member

| Member | Covered/valid | Coverage | Uncovered | Result |
|---|---:|---:|---|---|
| `Create(IWebViewCoreInitializer, string)` | 7/7 | 100.0000% | None | PASS |

`CreateSurfaceAsync`, lines 31-116, is the moved `[ExcludeFromCodeCoverage]` direct WebView2/WinForms adapter and is explicitly nonnumeric. Raw merged Cobertura exposes zero-hit compiler-generated local-function residue at lines 53-57, 63-66, 72-77, 79-84, and 87-90. Complete AST attribution places all 25 raw residue lines inside the excluded adapter span. It would therefore be incorrect to report the raw merged 7/32 class node as the helper's measurable coverage. The measurable helper type and its only measurable member are 7/7.

## Required 20:38-to-Final Member Comparison

| Member | Superseded 20:38 result | Final 21:09 result | Delta | Result |
|---|---|---|---|---|
| `CompleteOpenAsync` | 20/29 = 68.9655%; uncovered 190-198 | 29/29 = 100.0000%; none uncovered | +9 covered; +31.0345 percentage points | PASS |
| `OpenCoreAsync` | 35/40 = 87.5000%; uncovered 220, 230, 246, 249, 256 | 39/40 = 97.5000%; uncovered 230 | +4 covered; +10.0000 percentage points | PASS |
| `WaitForReadinessAsync` | 6/7 = 85.7143%; uncovered 376 | 7/7 = 100.0000%; none uncovered | +1 covered; +14.2857 percentage points | PASS |
| `NormalizeFactory`, including returned lambda | 14/17 = 82.3529%; uncovered 462-464 | 17/17 = 100.0000%; none uncovered | +3 covered; +17.6471 percentage points | PASS |

P5-T6 result: PASS. Repository coverage exceeds 80%, modified-line coverage does not regress, and every measurable new/changed selector type and member exceeds 90%. No unavailable numeric value was converted into a passing result; the moved direct adapter is recorded separately as nonnumeric.
