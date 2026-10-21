# Fail-before: Orphaned HintPath Gate (P1-T3) [expect-fail]

Timestamp: 2026-10-09T14-15
Command: pwsh -NoProfile -File CMDDIR\985-pester.ps1 -WorkspaceRoot WORKSPACE-ROOT -TestPath tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- TEST-PATH-COUNT: 1; COVERAGE-PATH-COUNT: 0
- PESTER Passed=4 Failed=1 Skipped=0 NotRun=0 Total=5
- FILE-TESTS RepositoryTreeConsistency.Tests.ps1 Passed=4 Failed=1 Total=5
- FAILED-TEST: Repository tree consistency (issue 929).reports no HintPath whose package folder the sibling manifest does not declare, for every project directory that carries a manifest
- FAILED-MESSAGE contains `QuickFiler.Test.csproj: line`, `UtilitiesCS.Test.csproj: line` and `TaskTree.Test.csproj: line` (right reason: borrowed packages before the manifest edits)
- Finding count: 7 (research predicted 7; recorded, not gated)
- P1-T1 static checks: Grep `It 'reports no HintPath whose package folder the sibling manifest does not declare` count 1; `Find-OrphanedHintPath -ProjectText` count 1; `AC[0-9]` in Describe/Context/It lines 0; file 178 lines, 178 CRLF.

## Findings list (from FAILED-MESSAGE)

1. QuickFiler.Test.csproj: line 389 Microsoft.Web.WebView2.1.0.4191.47
2. QuickFiler.Test.csproj: line 392 Microsoft.Web.WebView2.1.0.4191.47
3. QuickFiler.Test.csproj: line 407 ObjectListView.Official.2.9.1
4. QuickFiler.Test.csproj: line 530 Microsoft.Web.WebView2.1.0.4191.47
5. TaskTree.Test.csproj: line 178 ObjectListView.Official.2.9.1
6. UtilitiesCS.Test.csproj: line 815 Microsoft.Web.WebView2.1.0.4191.47
7. UtilitiesCS.Test.csproj: line 818 Microsoft.Web.WebView2.1.0.4191.47

## FAILED-MESSAGE (verbatim)

```
FAILED-MESSAGE: Expected 0, because these HintPath elements name a package folder the sibling manifest does not declare: QuickFiler.Test.csproj: line 389 Microsoft.Web.WebView2.1.0.4191.47; QuickFiler.Test.csproj: line 392 Microsoft.Web.WebView2.1.0.4191.47; QuickFiler.Test.csproj: line 407 ObjectListView.Official.2.9.1; QuickFiler.Test.csproj: line 530 Microsoft.Web.WebView2.1.0.4191.47; TaskTree.Test.csproj: line 178 ObjectListView.Official.2.9.1; UtilitiesCS.Test.csproj: line 815 Microsoft.Web.WebView2.1.0.4191.47; UtilitiesCS.Test.csproj: line 818 Microsoft.Web.WebView2.1.0.4191.47, but got 7.
```
