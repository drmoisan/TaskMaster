# P5-T210 — Nine-unit >=90% closure (from P5-T209 Cobertura)

Timestamp: 2026-07-22T19-39Z

Command: `python parse coverage-p5-deadcode-removal.2026-07-22T19-32.cobertura.xml -> per-unit covered/valid for the nine P5-T185 units and the seven never-regress passing units (dedup by line number; hit if any hits>0)`

EXIT_CODE: 0

## Source

Proven from the P5-T209 authoritative Cobertura
`coverage-p5-deadcode-removal.2026-07-22T19-32.cobertura.xml` alone (natural exit 0, 170/170).

## Nine P5-T185 units — all now at or above 90% line coverage

| # | Unit | Prev covered/valid | New covered/valid | New % | Closed by |
|---:|---|---:|---:|---:|---|
| 1 | `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` | 28/43 (65.12%) | 42/43 | 97.67% | P5-T195 cases (1) `OpenAsync_LeaseSupersededDuringInstall_DisposesInstalledSurfaceExactlyOnce` (lines 292-301), (2) `OpenAsync_CreationFailsAndCleanupSucceeds_DisposesOwnedSurfaceWithoutReport` (line 315), (3) `OpenAsync_CleanupDispatchFails_ReportsSecondaryOnceAndPreservesPrimary` (lines 310-313) |
| 2 | `BreadcrumbDropDownOpenCoordinator.<RollbackAsync>d__28` | 6/9 (66.67%) | 9/9 | 100.00% | P5-T188 case (5) `RequestOpen_RollbackOperationThrows_CompletesFalseWithoutSurfacingSecondary` (lines 224-226) |
| 3 | `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged()` | 4/5 (80.00%) | 5/5 | 100.00% | P5-T188 case (2) `HandleSelectorOpenStateChanged_AfterRelease_PostsNothingAndSkipsSelectorPredicate` (line 118) |
| 4 | `BreadcrumbDropDownOpenCoordinator.Reset()` | 4/5 (80.00%) | 5/5 | 100.00% | P5-T188 case (4) `Reset_AfterRelease_PostsNothingAndNeverDetachesOrResetsHost` (line 133) |
| 5 | `BreadcrumbDropDownOpenCoordinator.SetDroppedDown(bool)` | 5/6 (83.33%) | 6/6 | 100.00% | P5-T188 case (1) `SetDroppedDown_AfterRelease_PostsNothingAndLeavesHostStateUntouched` (line 99) |
| 6 | `BreadcrumbDropDownHost.<OnDropDownClosed>b__77_0()` | 5/6 (83.33%) | 6/6 | 100.00% | P5-T195 case (5) `NativeClosedCallback_HostClosedBeforeDrain_PerformsNoLateCloseWork` (line 413) |
| 7 | `BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` | 24/28 (85.71%) | 24/24 | 100.00% | **P5-T203 removal of unreachable former lines 153-156** (raw Cobertura reports the state machine 22/22 all-covered = `line-rate="1"`; the decision-tool covered/valid is 24/24 because the 4 uncovered dead lines left the denominator) |
| 8 | `BreadcrumbDropDownOpenCoordinator.<HandleSelectorOpenStateChanged>b__22_0()` | 7/8 (87.50%) | 8/8 | 100.00% | P5-T188 case (3) `HandleSelectorOpenStateChanged_QueuedBodyDrainedAfterRelease_PerformsNoWork` (line 122) |
| 9 | `BreadcrumbDropDownOpenLifetime.RetainCurrentSurface(...)` | 8/9 (88.89%) | 9/9 | 100.00% | P5-T195 case (1) `OpenAsync_LeaseSupersededDuringInstall_DisposesInstalledSurfaceExactlyOnce` (line 324) |

All nine units are at or above 90%. Eight units are at 100%; unit 1 (`<EnsureSurfaceAsync>d__21`) is at
97.67% (42/43; the single still-uncovered sequence point is not one of the P5-T185 target lines and does
not drop the unit below 90%).

## Seven never-regress passing units — no regression

| Unit | Cobertura class/member | Baseline | Now | Regression? |
|---|---|---:|---:|---|
| Dispatcher | `BreadcrumbUiDispatcher` | 144/144 | 144/144 | none |
| NavigationReadiness | `BreadcrumbNavigationReadiness` | 96/96 | 96/96 | none |
| Factory | `BreadcrumbWebViewSurfaceFactory` | 16/16 | 16/16 | none |
| Popup operations (host-neutral) | `BreadcrumbPopupUiOperations` | >=75/76 | 75/76 | none |
| Hub | `BreadcrumbMessengerHub` | 155/155 | 155/155 | none |
| Attachment | `BreadcrumbCollapsedAttachment` | 80/80 | 80/80 | none |
| Release | `BreadcrumbCollapsedAttachment.Release()` | 16/16 | 16/16 | none |

No previously-passing unit regressed. This is expected because the only production change is confined to
`BreadcrumbDropDownOpenLifetime.CompleteOpenAsync` (dead-code removal) and the test set is identical to
P5-T201.

## Output Summary

From the P5-T209 Cobertura alone, each of the nine P5-T185 units now reports at least 90% line coverage
(eight at 100%, `<EnsureSurfaceAsync>d__21` at 97.67%), with `<CompleteOpenAsync>d__16` raised to 100%
(24/24) by the P5-T203 removal of unreachable former lines 153-156 and the other eight raised by the ten
new deterministic P5-T188/P5-T195 cases mapped above. The seven never-regress passing units
(Dispatcher 144/144, NavigationReadiness 96/96, Factory 16/16, Popup operations 75/76, Hub 155/155,
Attachment 80/80, Release 16/16) are unchanged. No unit is below 90% and no unit regressed; no threshold,
exclusion, filter, or `coverage.config` change was made.
