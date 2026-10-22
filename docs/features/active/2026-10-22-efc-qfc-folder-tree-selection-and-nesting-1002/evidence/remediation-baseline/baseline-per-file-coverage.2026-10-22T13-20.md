# Remediation baseline: per-file coverage (#1002, cycle 1)

Timestamp: 2026-10-22T13-20
Command: pwsh -NoProfile -File SCRATCH/issue-1002-r-perfile.ps1 -Files <names> (union of class line nodes of the post-processed Cobertura; branch union takes the larger covered count per line)
EXIT_CODE: 0
Output Summary:
- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs: lines 78/121 (64.46%), branches 22/42 (52.38%). Matches expected 78/121 and 22/42.
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs: branches 7/14 (50.00%), lines 32/32. Matches expected 7/14.
- BreadcrumbMessages.cs condition-coverage of each `?? throw new ArgumentNullException` line: lines 84, 85, 124, 144, 173, 174 and 175 each 1/2.
- QuickFiler/Controllers/EfcFormController.Actions.cs (for the B-3 comparison): lines 26/111 (23.42%), branches 12/36 (33.33%).
