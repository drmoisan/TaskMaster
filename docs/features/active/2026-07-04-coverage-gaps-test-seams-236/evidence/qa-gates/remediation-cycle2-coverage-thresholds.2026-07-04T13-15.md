# Remediation Cycle 2 Coverage Thresholds

Timestamp: 2026-07-04T16:56:58.2191110-04:00
Task: P12-T7
Command: Parse remediation-cycle2-coverage-targets.2026-07-04T13-15.md and enforce repository, issue changed/new, per-file changed/new, and target coverage thresholds
EXIT_CODE: 1
ThresholdStatus: FAIL
RepositoryLineCoverage: 45.43%
RepositoryLineCoverageThreshold: 80.00%
RepositoryLineCoverageResult: FAIL
Issue236ChangedNewCoverage: 95.76%
Issue236ChangedNewCoverageThreshold: 90.00%
Issue236ChangedNewCoverageResult: PASS
NoChangedLineCoverageRegressionAgainstBaseline: PASS
PerFileChangedNewCoverageResult: PASS
TargetCoverageResult: FAIL

Output Summary: REMEDIATION_REQUIRED - one or more remediation cycle 2 thresholds failed. AC8 must remain unchecked.

Per-File Threshold Failures:
- None

Target Coverage Failures:
- EfcHomeController: 85.49%
- TlpCellStates: 89.81%

Source Evidence:
- docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-cycle2-coverage-targets.2026-07-04T13-15.md
