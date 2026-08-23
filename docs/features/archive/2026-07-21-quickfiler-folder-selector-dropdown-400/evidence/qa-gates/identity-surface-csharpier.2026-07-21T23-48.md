# Identity Surface CSharpier and HTML Integrity

Timestamp: 2026-07-21T23:48:27Z
Command: `csharpier format QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs`
HTML inspection: `git diff --check -- QuickFiler/Resources/FolderBreadcrumb.html`
EXIT_CODE: 0
Output Summary: This pass supersedes earlier identity-surface formatter evidence. The approved test helper now uses its existing regular-expression dependency to unescape JSON-encoded path separators without introducing a new assembly reference. CSharpier formatted the authorized C# files and a repeated pass left every hash stable. The HTML diff has no whitespace errors and remains below 500 lines. Git emitted only its configured LF-to-CRLF working-copy advisory.

## Stable hashes and line counts

- `BreadcrumbBridgeCoordinator.cs`: `BC099899354098E43BC9629BC798BB03BBEB159FA4E63A6F5F862F71281E9A3A`, 392 lines.
- `BreadcrumbDuplicateIdentityIntegrationTests.cs`: `02EB5702F706F73117F3E464576547EF564395E78260369886938A89BB7C087E`, 214 lines.
- `FolderBreadcrumbAssetContractTests.cs`: `DE4F5011537497309D90E22D6388D1CEFC6B46ED399A52C54CE169915C1A866B`, 309 lines.
- `BreadcrumbSelectorCoordinatorTests.cs`: `D3F1954432E63805C52C76BB118F5105C5E7B828241D00A2BEEDE045FDECFEC5`, 412 lines.
- `FolderBreadcrumb.html`: `036F6CA4A4DF20EA41D4F0342B0B1F27E89CD56D57FE09DFE46D485AC1141F79`, 432 lines.

Required formatter rerun EXIT_CODE: 0. HTML diff-check EXIT_CODE: 0.
