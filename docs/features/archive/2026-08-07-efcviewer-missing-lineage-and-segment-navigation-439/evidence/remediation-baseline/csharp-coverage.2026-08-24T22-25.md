Timestamp: 2026-08-24T22-25
Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/remediation-baseline/issue-439-remediation-baseline.normalized.cobertura.xml`
EXIT_CODE: 0
Output Summary: The coverage wrapper executed 9 test assemblies with 6,474/6,474 tests passing and produced the normalized Cobertura comparison input. Repository coverage is 53,763/63,405 = 84.793%, with normalized sources, relative merged class filenames, and forbidden-package checks passing.
Comparison Input: `evidence/remediation-baseline/issue-439-remediation-baseline.normalized.cobertura.xml`
Coverage XML SHA256: `2A148B0E7E0A3A5A0DA519440AA499D72E88A5EC51947EF134F0F73A4C19BD60`
Test Assemblies: 9
Test Result: 6,474/6,474 passing
Repository Coverage: 53,763/63,405 = 84.793000%
Normalization Invariants: sources=`.`; classes=547; absolute filenames=0; duplicate filenames=0; forbidden packages=0; PASS.
Issue #439 changed production coverage:
- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs: 368/376 = 97.872340%
- QuickFiler/Controllers/EfcFormController.cs: 81/721 = 11.234397%
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs: structural asset-only source with no instrumented sequence-point class in normalized Cobertura XML.
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs: 129/133 = 96.992481%
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs: 99/103 = 96.116505%
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs: 30/30 = 100.000000%
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs: 153/155 = 98.709677%
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs: 116/116 = 100.000000%
