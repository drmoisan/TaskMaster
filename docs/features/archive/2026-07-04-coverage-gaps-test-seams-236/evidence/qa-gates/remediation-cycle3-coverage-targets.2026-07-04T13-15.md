# Remediation Cycle 3 Coverage Targets

Timestamp: 2026-07-04T17:23:10.6942674-04:00
Task: P14-T5
Command: Parse docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-cycle3-coverage.cobertura.xml for issue #236 production files and targets
EXIT_CODE: 0

Output Summary:
- Repository line coverage: 43.84% (79004 / 180214).
- Issue #236 changed/new non-exempt production coverage: 95.74% (1260 / 1316).
- Per-file changed/new coverage includes EfcHomeController.ExecuteMoves.cs and EfcHomeControllerDependencyFactories.cs.
- Target coverage values are machine-parsed from Cobertura class/file entries.

Per-File Changed/New Production Coverage:
| File | Covered Lines | Coverage |
| --- | ---: | ---: |
| QuickFiler\Controllers\EfcHomeController.cs | 223 / 228 | 97.81% |
| QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs | 66 / 73 | 90.41% |
| QuickFiler\Controllers\EfcHomeController.Metrics.cs | 43 / 44 | 97.73% |
| QuickFiler\Controllers\EfcHomeController.Timing.cs | 18 / 18 | 100.00% |
| QuickFiler\Controllers\EfcHomeControllerDependencies.cs | 195 / 207 | 94.20% |
| QuickFiler\Controllers\EfcHomeControllerDependencyFactories.cs | 91 / 96 | 94.79% |
| QuickFiler\Helper Classes\EfcViewerQueue.cs | 46 / 50 | 92.00% |
| QuickFiler\Helper Classes\ItemViewerQueue.cs | 59 / 64 | 92.19% |
| QuickFiler\Helper Classes\QfcThemeControlSet.cs | 46 / 46 | 100.00% |
| QuickFiler\Helper Classes\QfcThemeHelper.cs | 274 / 285 | 96.14% |
| QuickFiler\Helper Classes\TlpCellSnapShot.cs | 108 / 108 | 100.00% |
| QuickFiler\Helper Classes\ViewerQueueCore.cs | 91 / 97 | 93.81% |

Issue #236 Target Coverage:
| Target | CoverageSource | Covered Lines | Coverage |
| --- | --- | ---: | ---: |
| EfcViewerQueue | class/file aggregate | 46 / 50 | 92.00% |
| ItemViewerQueue | class/file aggregate | 59 / 64 | 92.19% |
| QfcThemeHelper | class/file aggregate | 274 / 285 | 96.14% |
| EfcHomeController | class/file aggregate | 636 / 666 | 95.50% |
| TlpCellStates | class/file aggregate | 108 / 108 | 100.00% |

Source Evidence:
- docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-cycle3-coverage.cobertura.xml
