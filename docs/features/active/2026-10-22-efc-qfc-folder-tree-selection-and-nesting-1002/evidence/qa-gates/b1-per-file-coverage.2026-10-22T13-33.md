# B-1 verification (P4-T7, #1002 cycle 1)

Timestamp: 2026-10-22T13-33
Command: pwsh -NoProfile -File SCRATCH/issue-1002-r-perfile.ps1 -Files EfcFormController.Breadcrumb.cs -Classes (union of class line nodes of the post-processed Cobertura)
EXIT_CODE: 0
Output Summary:
- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs: lines 93/101 (92.08%), branches 34/42 (80.95%).
- Threshold: covered lines at least 84 (93 observed), covered branches at least 24 (34 observed). Met.
- Baseline 78/121 lines, 22/42 branches. The valid-line count fell from 121 to 101 because ConfigureBreadcrumbControl now carries [ExcludeFromCodeCoverage].
