# Subfolder Composition Pass-After Gate

Timestamp: 2026-07-23T02:29:44.6377445Z

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbSubfolderSelectorSessionTests|FullyQualifiedName~BreadcrumbSubfolderActivationTests|FullyQualifiedName~BreadcrumbSelectorMessagesTests|FullyQualifiedName~FolderBreadcrumbAssetContractTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests" /Logger:'console;Verbosity=normal'`

EXIT_CODE: 0

Output Summary: Both assemblies were discovered and all 70 cases passed with 0 failures and 0 skips in 3.8703 seconds. Per-class counts were 4 `BreadcrumbSubfolderSelectorSessionTests`, 35 `BreadcrumbSelectorMessagesTests`, 12 `BreadcrumbSelectorCoordinatorTests`, 5 `BreadcrumbSubfolderActivationTests`, and 14 `FolderBreadcrumbAssetContractTests`.

The three independent activation follow-ups passed: Enter, Escape, and native automatic close each preserved the committed full subfolder path while the initial activation produced one `SelectionChanged`, one explicit-commit close, one focus return, and a closed selector session. Invalid identity/index/plain-row cases remained deterministic no-ops. The new compiled-resource test also passed for the shared click/Enter/Space stable-identity message path and its role, aria-selected, and tabindex contract.
