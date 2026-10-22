Timestamp: 2026-08-24T19:51:30-04:00
Coverage Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/qa-gates/issue-439-final.cobertura.xml`
Coverage EXIT_CODE: `0`
Extractor Command: read-only PowerShell XML extraction grouped by normalized source path and unique line number, retaining the highest hit count where generated classes share a source line.
Extractor EXIT_CODE: `0`
Repository Line Coverage: `70.14%`
QuickFiler/Controllers/EfcFormController.cs: `9.99%` (`72/721`)
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs: `95.74%` (`360/376`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs: `97.42%` (`151/155`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs: `100%` (`116/116`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs: `100%` (`30/30`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs: `96.12%` (`99/103`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs: `96.99%` (`129/133`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs: `COVERAGE_UNAVAILABLE — structural no-sequence-point exception`
Output Summary: The final post-remediation Cobertura XML is present and all required numeric source entries are available except the expected DocumentAssets structural exception. Repository coverage is numeric but below the P4-T6 threshold and is not final-QA approval.

---
## P4-T5 authoritative normalized coverage restart

Timestamp: 2026-08-24T20:35:41-04:00
Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/qa-gates/issue-439-final.normalized.cobertura.xml`
EXIT_CODE: `0`
Output Summary: The retained parent PowerShell session actually exited only after all nine assemblies completed: 6474 passed, 0 failed. The wrapper emitted `Post-processing coverage XML for Koverage compatibility...` and `Done. Coverage artifact:`. No worktree-owned coverage/test runner remained after that parent exit.

Normalized XML: `evidence/qa-gates/issue-439-final.normalized.cobertura.xml`
SHA256: `0FB03C3B7AD0019004D623F288A9BA6A59992A2E7549FE83DA79636DF172AD5B`
Normalization Invariants: exactly 9 first-party packages (`QuickFiler`, `SVGControl`, `Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`, `UtilitiesCS`, `VBFunctions`); 547 merged classes with no duplicate filename; relative filenames only; exactly `<sources><source>.</source></sources>`; no `log4net`, `Mono.Reflection`, `Microsoft.IO.RecyclableMemoryStream`, `System.Interactive`, or `System.Linq.Async`; and no adjacent effective coverage configuration after wrapper completion.

Repository Line Coverage: `84.7835%` (`53757/63405`)
QuickFiler/Controllers/EfcFormController.cs: `11.234397%` (`81/721`)
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs: `97.87234%` (`368/376`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs: `98.709677%` (`153/155`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs: `100%` (`116/116`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs: `100%` (`30/30`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs: `96.116505%` (`99/103`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs: `96.992481%` (`129/133`)
UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs: `COVERAGE_UNAVAILABLE — sole structural no-sequence-point exception`

The raw `70.14%` XML and prior comparison remain historical non-comparable evidence only. This normalized artifact is the sole P4-T6 comparison input.
