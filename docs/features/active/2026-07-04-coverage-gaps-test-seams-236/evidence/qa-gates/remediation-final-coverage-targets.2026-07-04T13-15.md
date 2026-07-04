# Remediation Final Coverage Targets

Timestamp: 2026-07-04T13-15
Task: P10-T6
Command: PowerShell Cobertura XML parser comparing final coverage against baseline coverage and git diff against origin/main merge-base
EXIT_CODE: 0
MergeBase: 270e768db90c6c9e5a3a887856f1879ef436c074
BaselineCoverageXml: docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\baseline\baseline-coverage.cobertura.xml
FinalCoverageXml: docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\qa-gates\remediation-final-coverage.cobertura.xml
BaselineRepositoryLineCoverage: 44.60%
RepositoryLineCoverage: 45.33%
Issue236ChangedNewCoverage: 81.50%
Issue236ChangedNewCoveredLines: 643
Issue236ChangedNewExecutableLines: 789
Output Summary: Repository line coverage 45.33%; issue #236 changed/new executable line coverage 81.50% (643/789).

Per-File Changed/New Coverage:
- File: QuickFiler/Controllers/EfcHomeController.cs | ChangedLines: 100 | ExecutableChangedLines: 61 | CoveredExecutableChangedLines: 59 | ChangedNewCoverage: 96.72% | UncoveredExecutableChangedLines: 25, 52
- File: QuickFiler/Controllers/EfcHomeController.Metrics.cs | ChangedLines: 87 | ExecutableChangedLines: 44 | CoveredExecutableChangedLines: 43 | ChangedNewCoverage: 97.73% | UncoveredExecutableChangedLines: 23
- File: QuickFiler/Controllers/EfcHomeController.Timing.cs | ChangedLines: 43 | ExecutableChangedLines: 18 | CoveredExecutableChangedLines: 18 | ChangedNewCoverage: 100.00% | UncoveredExecutableChangedLines: 
- File: QuickFiler/Controllers/EfcHomeControllerDependencies.cs | ChangedLines: 468 | ExecutableChangedLines: 247 | CoveredExecutableChangedLines: 129 | ChangedNewCoverage: 52.23% | UncoveredExecutableChangedLines: 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 169, 170, 180, 181, 182, 183, 184, 185, 186, 187, 204, 205, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 251, 252, 267, 268, 269, 270, 271, 272, 273, 274, 275, 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 329, 330, 344, 345, 346, 347, 348, 349, 350, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 362, 363, 364, 365, 366, 367, 368, 369, 398, 399, 409, 410, 411, 412, 413, 414, 415, 416, 433, 434, 443, 444, 455, 456, 457, 458, 459, 460, 461, 462, 463, 465
- File: QuickFiler/Helper Classes/EfcViewerQueue.cs | ChangedLines: 54 | ExecutableChangedLines: 32 | CoveredExecutableChangedLines: 28 | ChangedNewCoverage: 87.50% | UncoveredExecutableChangedLines: 48, 49, 50, 51
- File: QuickFiler/Helper Classes/ItemViewerQueue.cs | ChangedLines: 59 | ExecutableChangedLines: 39 | CoveredExecutableChangedLines: 35 | ChangedNewCoverage: 89.74% | UncoveredExecutableChangedLines: 68, 69, 70, 71
- File: QuickFiler/Helper Classes/QfcThemeHelper.cs | ChangedLines: 247 | ExecutableChangedLines: 199 | CoveredExecutableChangedLines: 188 | ChangedNewCoverage: 94.47% | UncoveredExecutableChangedLines: 52, 53, 54, 55, 65, 66, 69, 70, 89, 98, 99
- File: QuickFiler/Helper Classes/QfcThemeControlSet.cs | ChangedLines: 101 | ExecutableChangedLines: 46 | CoveredExecutableChangedLines: 46 | ChangedNewCoverage: 100.00% | UncoveredExecutableChangedLines: 
- File: QuickFiler/Helper Classes/ViewerQueueCore.cs | ChangedLines: 161 | ExecutableChangedLines: 97 | CoveredExecutableChangedLines: 91 | ChangedNewCoverage: 93.81% | UncoveredExecutableChangedLines: 152, 153, 154, 155, 156, 157
- File: QuickFiler/Helper Classes/TlpCellSnapShot.cs | ChangedLines: 10 | ExecutableChangedLines: 6 | CoveredExecutableChangedLines: 6 | ChangedNewCoverage: 100.00% | UncoveredExecutableChangedLines: 

Target Coverage:
- Target: EfcViewerQueue | ClassName: QuickFiler.EfcViewerQueue | File: QuickFiler/Helper Classes/EfcViewerQueue.cs | CoverageSource: class | Coverage: 94.12%
- Target: ItemViewerQueue | ClassName: QuickFiler.ItemViewerQueue | File: QuickFiler/Helper Classes/ItemViewerQueue.cs | CoverageSource: class | Coverage: 95.74%
- Target: QfcThemeHelper | ClassName: QuickFiler.QfcThemeHelper | File: QuickFiler/Helper Classes/QfcThemeHelper.cs | CoverageSource: class | Coverage: 96.30%
- Target: EfcHomeController | ClassName: QuickFiler.EfcHomeController | File: QuickFiler/Controllers/EfcHomeController.cs | CoverageSource: class | Coverage: 88.14%
- Target: EfcHomeController | ClassName: QuickFiler.EfcHomeController | File: QuickFiler/Controllers/EfcHomeController.Metrics.cs | CoverageSource: class | Coverage: 97.59%
- Target: EfcHomeController | ClassName: QuickFiler.EfcHomeController | File: QuickFiler/Controllers/EfcHomeController.Timing.cs | CoverageSource: class | Coverage: 100.00%
- Target: EfcHomeController | ClassName: QuickFiler.EfcHomeControllerDependencies | File: QuickFiler/Controllers/EfcHomeControllerDependencies.cs | CoverageSource: class | Coverage: 55.13%
- Target: TlpCellStates | ClassName: QuickFiler.TlpCellStates | File: QuickFiler/Helper Classes/TlpCellSnapShot.cs | CoverageSource: class | Coverage: 92.09%
