# Remediation Cycle 2 Coverage Targets

Timestamp: 2026-07-04T16:56:19.1410373-04:00
Task: P12-T6
Command: PowerShell XML parser over remediation-cycle2-coverage.cobertura.xml, baseline-coverage.cobertura.xml, and git diff 270e768db90c6c9e5a3a887856f1879ef436c074..HEAD
EXIT_CODE: 0

Output Summary:
- Parsed final and baseline Cobertura XML using PowerShell XML APIs.
- Merge base: `270e768db90c6c9e5a3a887856f1879ef436c074`.
- Full issue #236 production file set was used for changed/new-code coverage.

Coverage Comparison:
| Metric | Baseline | Cycle 2 Final | Delta |
| --- | ---: | ---: | ---: |
| Repository line coverage | 44.60% | 45.43% | +0.83 percentage points |
| Issue #236 changed/new-code coverage | 23.08% (15/65 baseline comparable lines) | 95.76% (316/330 covered executable changed lines) | +72.68 percentage points |

Changed/New-Code File Coverage:
| File | Changed Lines | Executable Changed Lines | Covered Lines | Percent | Uncovered Executable Lines |
| --- | ---: | ---: | ---: | ---: | --- |
| QuickFiler/Controllers/EfcHomeController.cs | 100 | 61 | 60 | 98.36% | 52 |
| QuickFiler/Controllers/EfcHomeController.Metrics.cs | 87 | 44 | 43 | 97.73% | 23 |
| QuickFiler/Controllers/EfcHomeController.Timing.cs | 43 | 18 | 18 | 100.00% | None |
| QuickFiler/Controllers/EfcHomeControllerDependencies.cs | 428 | 207 | 195 | 94.20% | 403, 404, 415, 416, 417, 418, 419, 420, 421, 422, 423, 425 |
| QuickFiler/Helper Classes/EfcViewerQueue.cs | 0 | 0 | 0 | 100.00% | None |
| QuickFiler/Helper Classes/ItemViewerQueue.cs | 0 | 0 | 0 | 100.00% | None |
| QuickFiler/Helper Classes/QfcThemeHelper.cs | 0 | 0 | 0 | 100.00% | None |
| QuickFiler/Helper Classes/QfcThemeControlSet.cs | 0 | 0 | 0 | 100.00% | None |
| QuickFiler/Helper Classes/ViewerQueueCore.cs | 0 | 0 | 0 | 100.00% | None |
| QuickFiler/Helper Classes/TlpCellSnapShot.cs | 0 | 0 | 0 | 100.00% | None |

Target Coverage:
| Target | Covered Lines | Executable Lines | Percent | Coverage Sources |
| --- | ---: | ---: | ---: | --- |
| EfcViewerQueue | 46 | 50 | 92.00% | QuickFiler.EfcViewerQueue|QuickFiler/Helper Classes/EfcViewerQueue.cs |
| ItemViewerQueue | 59 | 64 | 92.19% | QuickFiler.ItemViewerQueue|QuickFiler/Helper Classes/ItemViewerQueue.cs |
| QfcThemeHelper | 320 | 331 | 96.68% | QuickFiler.QfcThemeControlSet|QuickFiler/Helper Classes/QfcThemeControlSet.cs; QuickFiler.QfcThemeHelper|QuickFiler/Helper Classes/QfcThemeHelper.cs |
| EfcHomeController | 501 | 586 | 85.49% | QuickFiler.EfcHomeController|QuickFiler/Controllers/EfcHomeController.cs; QuickFiler.EfcHomeController|QuickFiler/Controllers/EfcHomeController.Metrics.cs; QuickFiler.EfcHomeController|QuickFiler/Controllers/EfcHomeController.Timing.cs; QuickFiler.EfcHomeControllerDependencies|QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs; QuickFiler.EfcHomeControllerDependencies|QuickFiler/Controllers/EfcHomeControllerDependencies.cs |
| TlpCellStates | 97 | 108 | 89.81% | QuickFiler.TlpCellStates|QuickFiler/Helper Classes/TlpCellSnapShot.cs |
