# Remediation Cycle 2 Current Coverage Targets

Timestamp: 2026-07-04T20:44:46.0259506-04:00
Task: Current AC8 verification after stale coverage artifact was detected
Command: Parse remediation-cycle2-current-coverage.cobertura.xml and calculate AC8 coverage gates
EXIT_CODE: 0
RepositoryLineCoverage: 81.08% (79461 / 98006)
Issue236ChangedNewCoverage: 95.74% (1260 / 1316)

Output Summary:
- Repository-wide line coverage is 81.08%.
- Issue #236 changed/new non-exempt production coverage is 95.74%.
- Per-file and target coverage are listed below.

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

Source Evidence:
- docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-cycle2-current-coverage.cobertura.xml
