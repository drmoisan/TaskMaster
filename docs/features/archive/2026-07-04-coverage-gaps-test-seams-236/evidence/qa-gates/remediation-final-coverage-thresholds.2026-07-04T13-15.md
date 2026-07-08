# Remediation Final Coverage Thresholds

Timestamp: 2026-07-04T13-15
Task: P10-T7
Command: Parse remediation-final-coverage-targets.2026-07-04T13-15.md and enforce repository, issue changed/new, per-file changed/new, and target coverage thresholds
EXIT_CODE: 1
ThresholdStatus: FAIL
RemediationStatus: REMEDIATION_REQUIRED
BaselineRepositoryLineCoverage: 44.60%
RepositoryLineCoverage: 45.33%
RepositoryLineCoverageThreshold: 80.00%
RepositoryLineCoverageResult: FAIL
Issue236ChangedNewCoverage: 81.50%
Issue236ChangedNewCoverageThreshold: 90.00%
Issue236ChangedNewCoverageResult: FAIL
NoChangedLineCoverageRegressionAgainstBaseline: PASS
Output Summary: FAIL - repository coverage is below 80.00%, issue #236 changed/new coverage is below 90.00%, and multiple per-file or target thresholds are below 90.00%.

Per-File Threshold Failures:
- File: QuickFiler/Controllers/EfcHomeControllerDependencies.cs | ChangedLines: 468 | ExecutableChangedLines: 247 | CoveredExecutableChangedLines: 129 | ChangedNewCoverage: 52.23% | UncoveredExecutableChangedLines: 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 169, 170, 180, 181, 182, 183, 184, 185, 186, 187, 204, 205, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 251, 252, 267, 268, 269, 270, 271, 272, 273, 274, 275, 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 329, 330, 344, 345, 346, 347, 348, 349, 350, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 362, 363, 364, 365, 366, 367, 368, 369, 398, 399, 409, 410, 411, 412, 413, 414, 415, 416, 433, 434, 443, 444, 455, 456, 457, 458, 459, 460, 461, 462, 463, 465
- File: QuickFiler/Helper Classes/EfcViewerQueue.cs | ChangedLines: 54 | ExecutableChangedLines: 32 | CoveredExecutableChangedLines: 28 | ChangedNewCoverage: 87.50% | UncoveredExecutableChangedLines: 48, 49, 50, 51
- File: QuickFiler/Helper Classes/ItemViewerQueue.cs | ChangedLines: 59 | ExecutableChangedLines: 39 | CoveredExecutableChangedLines: 35 | ChangedNewCoverage: 89.74% | UncoveredExecutableChangedLines: 68, 69, 70, 71

Target Coverage Failures:
- Target: EfcHomeController | ClassName: QuickFiler.EfcHomeController | File: QuickFiler/Controllers/EfcHomeController.cs | CoverageSource: class | Coverage: 88.14%
- Target: EfcHomeController | ClassName: QuickFiler.EfcHomeControllerDependencies | File: QuickFiler/Controllers/EfcHomeControllerDependencies.cs | CoverageSource: class | Coverage: 55.13%

Source Evidence:
- docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-final-coverage-targets.2026-07-04T13-15.md
