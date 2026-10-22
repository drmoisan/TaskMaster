# Remediation coverage comparison (P4-T12, #1002 cycle 1)

Timestamp: 2026-10-22T13-33
Command: SCRATCH/issue-1002-r-perfile.ps1 on the baseline (P0-T4) and final (P4-T5) post-processed Cobertura documents; first-party totals from the coverage script summary line
EXIT_CODE: 0
Output Summary:

| Item | Baseline | Final | Delta |
|---|---|---|---|
| B-1 EfcFormController.Breadcrumb.cs lines | 78/121 (64.46%) | 93/101 (92.08%) | +15 covered, +27.62 pts |
| B-1 EfcFormController.Breadcrumb.cs branches | 22/42 (52.38%) | 34/42 (80.95%) | +12 covered, +28.57 pts |
| B-2 BreadcrumbMessages.cs branches | 7/14 (50.00%) | 14/14 (100.00%) | +7 covered, +50.00 pts |
| B-3/CR-4 EfcFormController.Actions.cs lines | 26/111 (23.42%) | 26/28 (92.86%) | 0 covered, valid 111 to 28 |
| B-3/CR-4 EfcFormController.Actions.cs branches | 12/36 (33.33%) | 13/16 (81.25%) | +1 covered, valid 36 to 16 |
| Repository first-party lines | 57527/66780 (86.14%) | 57541/66677 (86.30%) | +0.16 pts |
| Repository first-party branches | 14018/17439 (80.38%) | 14036/17419 (80.58%) | +0.20 pts |

- Residual compiler-generated classes (P4-T9): none; one class node per file.
- Repository totals are not lowered and exceed the 80% line and 75% branch floors.
- B-3 exemption is recorded as member-level [ExcludeFromCodeCoverage] attributes only (five named members); no coverage.config entry (`git diff --stat` of coverage.config against the merge base is empty).
- The earlier coverage-comparison.2026-10-22T10-26.md is not edited.
