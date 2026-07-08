# Remediation Final Coverage Targets

Timestamp: 2026-07-04T18:49:32.0000000-04:00
Task: P4-T5
Command: Parse `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-final-coverage.cobertura.xml`, compare against `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/remediation-baseline/remediation-baseline-coverage.cobertura.xml`, and calculate issue #236 production target coverage.
EXIT_CODE: 0
MergeBase: 270e768db90c6c9e5a3a887856f1879ef436c074
BaselineCoverageXml: `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/remediation-baseline/remediation-baseline-coverage.cobertura.xml`
FinalCoverageXml: `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-final-coverage.cobertura.xml`
BaselineRepositoryLineCoverage: 45.59% (82214 / 180333)
RepositoryLineCoverage: 46.15% (83226 / 180333)
RepositoryLineCoverageDelta: +0.56 percentage points
Issue236ChangedNewCoverage: 95.74% (1260 / 1316)
Issue236ChangedNewCoverageStatus: PASS against 90.00%
PerFileChangedNewCoverageStatus: PASS against 90.00%
RepositoryWideCoverageStatus: FAIL against 80.00%

Output Summary:
- Repository-wide line coverage is 46.15%, which remains below the 80.00% AC8 floor.
- Issue #236 changed/new non-exempt production coverage is 95.74% (1260 / 1316).
- Every issue #236 changed/new production file is at or above 90.00%.
- Target coverage values are parsed from Cobertura class/file line entries.
- No production files are changed by this remediation worktree diff; remediation changes add tests and project-file test inclusions.

Per-File Changed/New Production Coverage:
| File | Covered Lines | Coverage | Status |
| --- | ---: | ---: | --- |
| QuickFiler\Controllers\EfcHomeController.cs | 223 / 228 | 97.81% | PASS |
| QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs | 66 / 73 | 90.41% | PASS |
| QuickFiler\Controllers\EfcHomeController.Metrics.cs | 43 / 44 | 97.73% | PASS |
| QuickFiler\Controllers\EfcHomeController.Timing.cs | 18 / 18 | 100.00% | PASS |
| QuickFiler\Controllers\EfcHomeControllerDependencies.cs | 195 / 207 | 94.20% | PASS |
| QuickFiler\Controllers\EfcHomeControllerDependencyFactories.cs | 91 / 96 | 94.79% | PASS |
| QuickFiler\Helper Classes\EfcViewerQueue.cs | 46 / 50 | 92.00% | PASS |
| QuickFiler\Helper Classes\ItemViewerQueue.cs | 59 / 64 | 92.19% | PASS |
| QuickFiler\Helper Classes\QfcThemeControlSet.cs | 46 / 46 | 100.00% | PASS |
| QuickFiler\Helper Classes\QfcThemeHelper.cs | 274 / 285 | 96.14% | PASS |
| QuickFiler\Helper Classes\TlpCellSnapShot.cs | 108 / 108 | 100.00% | PASS |
| QuickFiler\Helper Classes\ViewerQueueCore.cs | 91 / 97 | 93.81% | PASS |

Issue #236 Target Coverage:
| Target | CoverageSource | Covered Lines | Coverage | Status |
| --- | --- | ---: | ---: | --- |
| EfcViewerQueue | class/file aggregate | 46 / 50 | 92.00% | PASS |
| ItemViewerQueue | class/file aggregate | 59 / 64 | 92.19% | PASS |
| QfcThemeHelper | class/file aggregate | 274 / 285 | 96.14% | PASS |
| EfcHomeController | class/file aggregate | 636 / 666 | 95.50% | PASS |
| TlpCellStates | class/file aggregate | 108 / 108 | 100.00% | PASS |

Additional Existing Production Files Exercised By Remediation Tests:
| File | Covered Lines | File Coverage |
| --- | ---: | ---: |
| SVGControl\RelativePath.cs | 147 / 774 | 18.99% |
| UtilitiesCS\Extensions\ArrayExtensions.cs | 303 / 339 | 89.38% |
| UtilitiesCS\Extensions\IEnumerableExtensions.cs | 241 / 284 | 84.86% |
| UtilitiesCS\HelperClasses\PrettyPrint.cs | 375 / 440 | 85.23% |
| UtilitiesCS\ReusableTypeClasses\Serializable\SerializableList.cs | 340 / 353 | 96.32% |
| UtilitiesCS\Threading\TimeOutTask.cs | 563 / 601 | 93.68% |
| ToDoModel\Data Model\ToDo\ToDoItem.cs | 284 / 820 | 34.63% |
| Tags\TagController.cs | 249 / 578 | 43.08% |
| TaskMaster\AppGlobals\AppAutoFileObjects.cs | 100 / 403 | 24.81% |
| TaskMaster\AppGlobals\AppToDoObjects.cs | 200 / 315 | 63.49% |
| TaskMaster\AppGlobals\AppEvents.cs | 231 / 305 | 75.74% |
| UtilitiesCS\Interfaces\IWinForm\PropertyStore.cs | 559 / 663 | 84.31% |
| UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs | 360 / 396 | 90.91% |
| UtilitiesCS\OutlookObjects\Folder\FolderPredictor.cs | 375 / 440 | 85.23% |
| UtilitiesCS\OutlookObjects\Folder\FolderWrapper.cs | 0 / 0 | N/A |

Source Evidence:
- `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-final-coverage.cobertura.xml`
- `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/remediation-baseline/remediation-baseline-coverage.cobertura.xml`
