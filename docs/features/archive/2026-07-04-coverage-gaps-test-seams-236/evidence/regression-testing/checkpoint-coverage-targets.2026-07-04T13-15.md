Timestamp: 2026-07-04T13-15
Task: P5-T6
Command: PowerShell XML parser over docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\regression-testing\checkpoint-coverage.cobertura.xml and docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\baseline\baseline-coverage.cobertura.xml
EXIT_CODE: 0

Output Summary:
- Parsed checkpoint and baseline Cobertura XML using `[xml]` and XPath class/line nodes.
- Compared checkpoint repository coverage with baseline repository coverage.
- Computed issue #236 changed/new-code coverage from production `.cs` changed lines against merge base `270e768db90c6c9e5a3a887856f1879ef436c074`; untracked new production `.cs` files were treated as new code.
- Computed target coverage from exact class entries. `EfcHomeController` includes its partial class files and dependency bundle because the Cobertura class entries share the target class/dependency names.

Coverage Comparison:
| Metric | Baseline | Checkpoint | Delta |
| --- | ---: | ---: | ---: |
| Repository line coverage | 44.60% | 44.17% | -0.43 percentage points |
| Issue #236 changed/new-code coverage | 3.04% (9/296 baseline-covered comparable lines) | 69.67% (425/610 covered lines) | +66.63 percentage points |

Target Coverage:
| Target | CoverageSource | Baseline | Checkpoint | Delta |
| --- | --- | ---: | ---: | ---: |
| EfcViewerQueue | class | 0.00% (0/57) | 92.31% (48/52) | +92.31 percentage points |
| ItemViewerQueue | class | 0.00% (0/118) | 94.87% (74/78) | +94.87 percentage points |
| QfcThemeHelper | class | 0.00% (0/424) | 88.48% (484/547) | +88.48 percentage points |
| EfcHomeController | class | 15.87% (70/441) | 44.49% (307/690) | +28.62 percentage points |
| TlpCellStates | class | 62.20% (79/127) | 92.09% (128/139) | +29.89 percentage points |

Changed/New-Code File Coverage:
| File | Covered Lines | Coverable Changed Lines | Percent |
| --- | ---: | ---: | ---: |
| QuickFiler/Controllers/EfcHomeController.cs | 33 | 50 | 66.00% |
| QuickFiler/Controllers/EfcHomeController.Metrics.cs | 3 | 27 | 11.11% |
| QuickFiler/Controllers/EfcHomeController.Timing.cs | 18 | 18 | 100.00% |
| QuickFiler/Controllers/EfcHomeControllerDependencies.cs | 24 | 124 | 19.35% |
| QuickFiler/Helper Classes/EfcViewerQueue.cs | 20 | 24 | 83.33% |
| QuickFiler/Helper Classes/ItemViewerQueue.cs | 27 | 31 | 87.10% |
| QuickFiler/Helper Classes/QfcThemeControlSet.cs | 46 | 46 | 100.00% |
| QuickFiler/Helper Classes/QfcThemeHelper.cs | 159 | 189 | 84.13% |
| QuickFiler/Helper Classes/TlpCellSnapShot.cs | 6 | 6 | 100.00% |
| QuickFiler/Helper Classes/ViewerQueueCore.cs | 89 | 95 | 93.68% |

Remediation Status:
- REMEDIATION_REQUIRED: none for checkpoint machine-checkable value extraction.
- Threshold status is not finalized in this checkpoint task. The checkpoint values show that additional work is required before P6-T7 can pass the repository `>= 80%` and changed/new-code `>= 90%` thresholds.
