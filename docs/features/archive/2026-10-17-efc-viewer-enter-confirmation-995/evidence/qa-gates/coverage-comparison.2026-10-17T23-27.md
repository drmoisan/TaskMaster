# Issue 995 coverage-comparison

Timestamp: 2026-10-17T23-27
Command: pwsh -NoProfile -File SCRATCH/issue-995-compare-coverage.ps1
EXIT_CODE: 0
Output Summary: C#: baseline 86.0212% -> final 86.0331%; delta 0.0119 points.
PowerShell expanded: baseline 84.0257% -> final 94.9465%; delta 10.9208 points.
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs: 30/30 (100.0000%) -> 30/30 (100.0000%).
Changed executable hunks: 0/0 -> 0/0; compiler-elided declarations/comments excluded from executable denominators.
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs: 99/103 (96.1165%) -> 107/107 (100.0000%).
Changed executable hunks: 1/1 -> 5/5; compiler-elided declarations/comments excluded from executable denominators.
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs: 237/239 (99.1632%) -> 239/241 (99.1701%).
Changed executable hunks: 0/0 -> 2/2; compiler-elided declarations/comments excluded from executable denominators.
QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs: 140/143 (97.9021%) -> 150/153 (98.0392%).
Changed executable hunks: 0/0 -> 10/10; compiler-elided declarations/comments excluded from executable denominators.
QuickFiler/Controllers/EfcFormController.Breadcrumb.cs: 62/97 (63.9175%) -> 78/115 (67.8261%).
Changed executable hunks: 0/6 -> 16/24; compiler-elided declarations/comments excluded from executable denominators.
QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs: 47/123 (38.2114%) -> 48/124 (38.7097%).
Changed executable hunks: 0/0 -> 1/1; compiler-elided declarations/comments excluded from executable denominators.
New C# method ConfirmRow: 10/10 = 100.00%; branch 100.00%.
New C# method ConfigureBreadcrumbConfirmation: 9/9 = 100.00%; branch 100.00%.
New C# method DetachBreadcrumbConfirmation: 7/7 = 100.00%; branch 100.00%.
PowerShell CodexDeployment.psm1: baseline 0/68; final 68/68=100.0000%; baseline all executable lines unhit, changed-line coverage cannot decrease from zero.
PowerShell record-subagent-routing-attestation.ps1: baseline 0/204; final 187/204=91.6667%; baseline all executable lines unhit, changed-line coverage cannot decrease from zero.
New PowerShell command wrapper: 36/40=90.00%.
Default PowerShell CI population unchanged at1962/2063=95.1042%; tests excluded from both collectors. Constant-only BreadcrumbDocumentAssets.cs is absent from instrumentation:0/0, percentage N/A, proven by const-only source and renderer fail-before/pass-after; no executable exclusions introduced.
