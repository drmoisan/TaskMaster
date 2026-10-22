# Remediation baseline: file sizes (#1002, cycle 1)

Timestamp: 2026-10-22T13-19
Command: @(Get-Content -LiteralPath <path>).Count for each file below
EXIT_CODE: 0
Output Summary:
- QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs: 140
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs: 297
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs: 179
- All three counts are at or under 500.
