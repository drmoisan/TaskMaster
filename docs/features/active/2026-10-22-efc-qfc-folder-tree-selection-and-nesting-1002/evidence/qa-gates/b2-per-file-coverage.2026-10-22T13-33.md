# B-2 verification (P4-T8, #1002 cycle 1)

Timestamp: 2026-10-22T13-33
Command: pwsh -NoProfile -File SCRATCH/issue-1002-r-perfile.ps1 -Files BreadcrumbMessages.cs -ThrowLines
EXIT_CODE: 0
Output Summary:
- condition-coverage on each `throw new ArgumentNullException` line of UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs: lines 84, 85, 124, 144, 173, 174, 175 each 2/2 (100%). Seven lines.
- File totals: branches 14/14 (100.00%), lines 32/32 (100.00%). Threshold at least 11 of 14: met. Baseline 7/14.
