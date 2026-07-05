Timestamp: 2026-07-04T13-15
Task: P6-T6
Command: PowerShell XML parser over docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\qa-gates\final-coverage.cobertura.xml and docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\baseline\baseline-coverage.cobertura.xml
EXIT_CODE: 0

Output Summary:
- Parsed final and baseline Cobertura XML using `[xml]` and XPath class/line nodes.
- Computed changed/new-code coverage using `git merge-base HEAD origin/main`.
- Merge base: `270e768db90c6c9e5a3a887856f1879ef436c074`
- Untracked new production `.cs` files were treated as new code.
- Target coverage was computed from exact class entries. `EfcHomeController` includes its partial class files and dependency bundle because the Cobertura class entries share the target class/dependency names.

Coverage Comparison:
| Metric | Baseline | Final | Delta |
| --- | ---: | ---: | ---: |
| Repository line coverage | 44.60% | 45.12% | +0.52 percentage points |
| Issue #236 changed/new-code coverage | 4.03% (12/298 baseline-covered comparable lines) | 71.19% (467/656 covered lines) | +67.16 percentage points |

Target Coverage:
| Target | CoverageSource | Baseline | Final | Delta |
| --- | --- | ---: | ---: | ---: |
| EfcViewerQueue | class | 0.00% (0/57) | 92.31% (48/52) | +92.31 percentage points |
| ItemViewerQueue | class | 0.00% (0/118) | 94.87% (74/78) | +94.87 percentage points |
| QfcThemeHelper | class | 0.00% (0/424) | 88.48% (484/547) | +88.48 percentage points |
| EfcHomeController | class | 15.87% (70/441) | 49.81% (387/777) | +33.94 percentage points |
| TlpCellStates | class | 62.20% (79/127) | 92.09% (128/139) | +29.89 percentage points |

Changed/New-Code File Coverage:
| File | Covered Lines | Coverable Changed Lines | Percent |
| --- | ---: | ---: | ---: |
| QuickFiler/Controllers/EfcHomeController.cs | 38 | 55 | 69.09% |
| QuickFiler/Controllers/EfcHomeController.Metrics.cs | 3 | 27 | 11.11% |
| QuickFiler/Controllers/EfcHomeController.Timing.cs | 18 | 18 | 100.00% |
| QuickFiler/Controllers/EfcHomeControllerDependencies.cs | 59 | 163 | 36.20% |
| QuickFiler/Helper Classes/EfcViewerQueue.cs | 20 | 24 | 83.33% |
| QuickFiler/Helper Classes/ItemViewerQueue.cs | 27 | 31 | 87.10% |
| QuickFiler/Helper Classes/QfcThemeControlSet.cs | 46 | 46 | 100.00% |
| QuickFiler/Helper Classes/QfcThemeHelper.cs | 159 | 189 | 84.13% |
| QuickFiler/Helper Classes/TlpCellSnapShot.cs | 6 | 6 | 100.00% |
| QuickFiler/Helper Classes/ViewerQueueCore.cs | 91 | 97 | 93.81% |

Remediation Status:
- REMEDIATION_REQUIRED: none for final machine-checkable value extraction.
- Threshold enforcement is recorded separately in `final-coverage-thresholds.2026-07-04T13-15.md`.
