# Cycle 2 P5-T7: coverage comparison

Timestamp: 2026-10-22T14-06
Command: SCRATCH/issue-1002-r2-perfile.ps1 -Files EfcFormController.Actions.cs,BreadcrumbDocumentAssets.cs on the P0-T3 and P5-T5 post-processed Cobertura documents
EXIT_CODE: 0
Output Summary:

| Item | Baseline | Final | Delta |
|---|---|---|---|
| EfcFormController.Actions.cs lines | 26/28 (92.86%) | 26/28 (92.86%) | 0 |
| EfcFormController.Actions.cs branches | 13/16 (81.25%) | 13/16 (81.25%) | 0 |
| BreadcrumbDocumentAssets.cs lines | 0/0 (no instrumented line) | 0/0 | 0 |
| BreadcrumbDocumentAssets.cs branches | 0/0 | 0/0 | 0 |
| Repository first-party lines | 57542/66677 (86.30%) | 57546/66677 (86.31%) | +4 covered |
| Repository first-party branches | 14038/17419 (80.59%) | 14038/17419 (80.59%) | 0 |

- Repository line and branch totals are not lowered.
- EfcFormController.Actions.cs meets 80% line and 75% branch. BreadcrumbDocumentAssets.cs holds only string constants: the static class has no executable member, no class node exists in the Cobertura document, and the floor is not evaluable (recorded as measured, not adjusted). The change adds string constant text only, so no executable line was added or uncovered.
- No exclusion added; earlier comparisons are not edited.
