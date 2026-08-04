# Runtime UI-boundary failure-first policy and scope audit

Timestamp: 2026-07-22T01:43:35.4029512Z

Command: `$tests=@('QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs','QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs'); foreach($f in $tests){ $text=Get-Content -LiteralPath $f -Raw; '{0}|LINES={1}|TestClass={2}|TestMethod={3}|Fluent={4}|Moq={5}|Arrange={6}|Act={7}|Assert={8}' -f $f,@(Get-Content -LiteralPath $f).Count,([regex]::Matches($text,'\[TestClass\]').Count),([regex]::Matches($text,'\[TestMethod\]').Count),($text -match 'using FluentAssertions'),($text -match 'using Moq'),([regex]::Matches($text,'// Arrange').Count),([regex]::Matches($text,'// Act').Count),([regex]::Matches($text,'// Assert').Count) }; foreach($name in @('BreadcrumbSelectorToggleUiBoundaryTests.cs','BreadcrumbPopupControlDispatchTests.cs','BreadcrumbSelectorOpenRetryTests.cs')){ $escaped=[regex]::Escape('Viewers\'+$name); $count=@(Select-String -LiteralPath 'QuickFiler.Test/QuickFiler.Test.csproj' -Pattern $escaped).Count; 'INCLUDE|{0}|COUNT={1}' -f $name,$count }; $prohibited='Thread\.Sleep|Task\.Delay|GetTemp|TempPath|File\.|Directory\.|HttpClient|WebRequest|Process\.Start|EnsureCoreWebView2Async|new WebView2|Screen\.PrimaryScreen|MessageBox|Screenshot'; $hits=@(Select-String -LiteralPath $tests -Pattern $prohibited); 'PROHIBITED_HITS=' + $hits.Count; $production=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs'); foreach($f in $production){ '{0}|SHA256={1}' -f $f,(Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash.ToLowerInvariant() }`

EXIT_CODE: 0

Output Summary: All three files are MSTest classes using FluentAssertions and deterministic focused fakes; the selector-toggle and retry files also use Moq. The four methods retain explicit Arrange/Act/Assert organization, have no shared mutable fixture state, and use controlled completion sources or queued synchronization contexts. Line counts are 211, 240, and 243. Each test source has exactly one legacy-project include. The prohibited-resource scan returned zero hits.

## Production immutability

Every P5-T3 production hash is unchanged:

| Production source | P5-T3 and current SHA-256 |
|---|---|
| `BreadcrumbUiDispatcher.cs` | `c69c2281cacede2d7169b0b3b701be4d5f9de2756b8dab2e01aa44a1ecd658d3` |
| `BreadcrumbWebViewSurfaceFactory.cs` | `597c5edb8cac41e653bf3d6b5507aa62134ad202f7fc1af3cc7fa7c014c8a9df` |
| `BreadcrumbDropDownHost.cs` | `b219d3f2d6dd05805adfda95932d3a7d29f2b59bdcb3b674722d804e46719acc` |
| `ItemViewer.Breadcrumb.cs` | `c33976cf76045d634e7c8cf50965eeb05c26cf6c3b9d61a8902597904cfc804b` |
| `BreadcrumbBridgeCoordinator.cs` | `224d5614b8a293665ec22b563a9c2d7421ca1e0046a369ab4d56a728347bd391` |

No production source changed during failure-first P5-T4 through P5-T9.
