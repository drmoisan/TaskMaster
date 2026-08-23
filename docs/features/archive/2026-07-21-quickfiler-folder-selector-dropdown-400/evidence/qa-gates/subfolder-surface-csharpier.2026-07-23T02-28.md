# Subfolder Surface CSharpier Gate

Timestamp: 2026-07-23T02:28:35.0012867Z

Command: `csharpier format 'QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs' 'QuickFiler.Test/Viewers/BreadcrumbSubfolderActivationTests.cs' 'QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs' 'QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs'; csharpier check <same exact four-file tuple>; git diff --check -- 'QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs' 'QuickFiler/Resources/FolderBreadcrumb.html' <same three test files>`

EXIT_CODE: 0

Output Summary: The second mutating CSharpier pass changed zero of four scoped C# files and the authoritative scoped check reported `Checked 4 files`. `git diff --check` reported no whitespace error. Post-format line counts are 487 for `BreadcrumbBridgeCoordinator.cs`, 381 for `BreadcrumbSubfolderActivationTests.cs`, 339 for `FolderBreadcrumbAssetContractTests.cs`, 424 for `BreadcrumbSelectorCoordinatorTests.cs`, and 448 for `FolderBreadcrumb.html`; every batch file is at most 500 lines.
