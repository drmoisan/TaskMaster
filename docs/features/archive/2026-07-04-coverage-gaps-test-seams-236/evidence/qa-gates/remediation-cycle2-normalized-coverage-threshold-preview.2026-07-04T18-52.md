# Remediation Cycle 2 Normalized Coverage Threshold Preview

Timestamp: 2026-07-04T18:52:00-04:00
Command: Parse docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\qa-gates\remediation-cycle2-normalized-coverage.cobertura.xml and calculate repository, issue #236 changed/new, per-file changed/new, and original target coverage.
EXIT_CODE: 0
Output Summary:
- Repository-wide line coverage: 79.47% (77907 / 98039).
- Required covered lines for 80.00%: 78432.
- Residual covered-line gap: 525.
- Issue #236 changed/new coverage: 95.74% (1260 / 1316).
- Per-file changed/new coverage minimum: 90.41%.
- Original issue #236 target coverage: PASS.
- Phase 2 residual C# tests required: True.

Threshold Preview:
| Check | Required | Actual | Status |
| --- | ---: | ---: | --- |
| Repository-wide line coverage | >= 80.00% | 79.47% | FAIL |
| Issue #236 changed/new coverage | >= 90.00% | 95.74% | PASS |
| Per-file changed/new coverage minimum | >= 90.00% | 90.41% | PASS |
| Original issue #236 target coverage | PASS | PASS | PASS |

Per-File Changed/New Production Coverage:
| File | Covered Lines | Coverage | Status |
| --- | ---: | ---: | --- |
| `QuickFiler\Controllers\EfcHomeController.cs` | 223 / 228 | 97.81% | PASS |
| `QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs` | 66 / 73 | 90.41% | PASS |
| `QuickFiler\Controllers\EfcHomeController.Metrics.cs` | 43 / 44 | 97.73% | PASS |
| `QuickFiler\Controllers\EfcHomeController.Timing.cs` | 18 / 18 | 100% | PASS |
| `QuickFiler\Controllers\EfcHomeControllerDependencies.cs` | 195 / 207 | 94.2% | PASS |
| `QuickFiler\Controllers\EfcHomeControllerDependencyFactories.cs` | 91 / 96 | 94.79% | PASS |
| `QuickFiler\Helper Classes\EfcViewerQueue.cs` | 46 / 50 | 92% | PASS |
| `QuickFiler\Helper Classes\ItemViewerQueue.cs` | 59 / 64 | 92.19% | PASS |
| `QuickFiler\Helper Classes\QfcThemeControlSet.cs` | 46 / 46 | 100% | PASS |
| `QuickFiler\Helper Classes\QfcThemeHelper.cs` | 274 / 285 | 96.14% | PASS |
| `QuickFiler\Helper Classes\TlpCellSnapShot.cs` | 108 / 108 | 100% | PASS |
| `QuickFiler\Helper Classes\ViewerQueueCore.cs` | 91 / 97 | 93.81% | PASS |

Original Issue #236 Target Coverage:
| Target | Covered Lines | Coverage | Status |
| --- | ---: | ---: | --- |
| `EfcViewerQueue` | 46 / 50 | 92% | PASS |
| `ItemViewerQueue` | 59 / 64 | 92.19% | PASS |
| `QfcThemeHelper` | 274 / 285 | 96.14% | PASS |
| `EfcHomeController` | 636 / 666 | 95.5% | PASS |
| `TlpCellStates` | 108 / 108 | 100% | PASS |

Phase 2 Decision:
- Phase 2 residual C# tests are required because repository-wide line coverage is 79.47%, below 80.00%.
- Residual covered-line gap before buffer: 525.
