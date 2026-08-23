# Scope, Project, File-Size, and Integrity Audit

This artifact supersedes `scope-project-file-size-integrity.2026-07-23T04-31.md`. The earlier artifact was valid for its source state but preceded the corrected P7 evidence-command rerun, so it is not authoritative for the required execution order.

Timestamp: `2026-07-23T04:43:28Z`

Command:

```powershell
$ErrorActionPreference='Stop'
function Require { param([bool]$Condition,[string]$Message) if(-not $Condition){throw $Message} }
function HashOf { param([string]$Path) (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash }
$base=(git merge-base HEAD origin/main).Trim(); $head=(git rev-parse HEAD).Trim()
Require ($base -eq 'df5ad49c909f6b739edef45d0336151f44e827a6') 'Unexpected merge base.'; Require ($head -eq '38506fc0e433e9fe809be8ad77d2e7bc6f8d2382') 'Unexpected HEAD.'
$spam='UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs'; Require ((Get-Content -LiteralPath $spam).Count -eq 118) 'SpamBayes line count changed.'; Require ((HashOf $spam) -eq '99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA') 'SpamBayes hash changed.'; & git diff --quiet HEAD -- $spam; Require ($LASTEXITCODE -eq 0) 'SpamBayes has an uncommitted delta.'; $headFiles=@(git diff-tree --no-commit-id --name-only -r HEAD); Require ($headFiles.Count -eq 1 -and $headFiles[0] -eq $spam) 'HEAD unrelated-change isolation changed.'
$patterns=@('QuickFiler/**/*.cs','QuickFiler.Test/**/*.cs','UtilitiesCS/**/*.cs','UtilitiesCS.Test/**/*.cs','QuickFiler/Resources/FolderBreadcrumb.html'); $changed=@(git diff --name-only $base -- $patterns); $untracked=@(git ls-files --others --exclude-standard -- 'QuickFiler/**/*.cs' 'QuickFiler.Test/**/*.cs' 'UtilitiesCS/**/*.cs' 'UtilitiesCS.Test/**/*.cs'); $all=@($changed+$untracked|Sort-Object -Unique); Require ($all.Count -eq 64) "Expected 64 changed/new C#/HTML files including SpamBayes; found $($all.Count)."; $issue=@($all|Where-Object{$_ -ne $spam}); Require ($issue.Count -eq 63) 'Issue #400 C#/HTML scope count changed.'; foreach($f in $issue){Require (Test-Path -LiteralPath $f) "Missing $f"; $n=(Get-Content -LiteralPath $f).Count; Require ($n -le 500) "$f exceeds 500 lines ($n)."}
$limits=[ordered]@{'QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs'=480;'QuickFiler/Viewers/BreadcrumbDropDownHost.cs'=480;'QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs'=480;'QuickFiler/Viewers/ItemViewer.Breadcrumb.cs'=460;'QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs'=480;'QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs'=480;'QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs'=480;'QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs'=480;'QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs'=480;'QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs'=480;'QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs'=480;'QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs'=480;'QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs'=480;'QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs'=480;'QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs'=480;'QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs'=486;'QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs'=500}; foreach($e in $limits.GetEnumerator()){$n=(Get-Content -LiteralPath $e.Key).Count;Require ($n -le $e.Value) "$($e.Key) exceeds approved bound $($e.Value) ($n)."}; Require ((Get-Content -LiteralPath 'QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs').Count -eq 500) 'Integration test no longer has its approved exact 500 lines.'
$added=@(git diff --diff-filter=A --name-only $base -- 'QuickFiler/**/*.cs' 'QuickFiler.Test/**/*.cs' 'UtilitiesCS/**/*.cs' 'UtilitiesCS.Test/**/*.cs'); $new=@($added+$untracked|Sort-Object -Unique); Require ($new.Count -eq 49) "Expected 49 new C# sources/tests; found $($new.Count)."; foreach($f in $new){if($f -like 'QuickFiler.Test/*'){$project='QuickFiler.Test/QuickFiler.Test.csproj';$include=$f.Substring(16).Replace('/','\')}elseif($f -like 'QuickFiler/*'){$project='QuickFiler/QuickFiler.csproj';$include=$f.Substring(11).Replace('/','\')}elseif($f -like 'UtilitiesCS.Test/*'){$project='UtilitiesCS.Test/UtilitiesCS.Test.csproj';$include=$f.Substring(17).Replace('/','\')}elseif($f -like 'UtilitiesCS/*'){$project='UtilitiesCS/UtilitiesCS.csproj';$include=$f.Substring(12).Replace('/','\')}else{throw "No project mapping for $f"};[xml]$xml=Get-Content -Raw -LiteralPath $project;$count=@($xml.Project.ItemGroup.Compile|Where-Object{$_.Include -eq $include}).Count;Require ($count -eq 1) "$f include count is $count in $project."}; $requiredIncludes=@('QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs','QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs','QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs','QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs','QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs'); foreach($f in $requiredIncludes){Require ($new -contains $f) "Required include inventory is missing $f."}
$hashes=[ordered]@{'QuickFiler/QuickFiler.csproj'='D05401B1F146FDF84D3B9323F2E05A97028DD511CE5FE7C6C3B438F52907F7BF';'QuickFiler.Test/QuickFiler.Test.csproj'='06663711C83A1FE5DE1B485D5B361DB9EDCE43501E0C37A5AF081DC0D0804FC7';'UtilitiesCS/UtilitiesCS.csproj'='6051C4074F238014746FAF4C0ACE4D6D9D5D72EF57125873589A5A11BFC33061';'UtilitiesCS.Test/UtilitiesCS.Test.csproj'='99106D06A4B026922C4FB9CDAF9C6335B92BE1FD280F664109D28650EB2B8226';'coverage.config'='B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943';'scripts/vscode/TaskMaster.cli.runsettings'='98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57';'QuickFiler/packages.config'='8A4F9EF928E58289ED0964A220FC8B7B33C166098CC46A97F1498D25E8922485';'QuickFiler.Test/packages.config'='869B58018BDA096154A669DE597036FCC0452A8B5DD75A2841BEBE1C42393A83';'QuickFiler/Viewers/ItemViewer.cs'='498D1781BE7DF3665D799A4DFC9837AD4F81D6A47B0DEC1CB1C0A84D025AB0E2';'QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs'='A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396';'QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs'='25EE741353DB8CFA625F5783ED7CA17697768FBAB826865F53D72F0DF4BBBD77';'QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs'='989BE280294875DCEFD2E936F6F48D65F3EAFED21B4AE4530D4E6288561AFC59';'QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs'='326D190D5BB4B3634A0ABDE6A786A25354349A925B7DE8226AEB11990C8E3B01';'QuickFiler/Viewers/ItemViewer.Designer.cs'='0AB37A8F78804DEF674F7E41C028BD14E634E166719FCE933F8758B55D356A5F';'scripts/vscode/Invoke-MSTestWithCoverage.ps1'='4782C4E3F00CEA7F852AC884387AE9FDD15615F888F132CB7E71F2F1D9868E26';'.editorconfig'='E19340D3A51E6B2CF90CB2669FDB1B85A5AAB96900C0F2ABB925BC2CC4CA96AA';'Directory.Build.targets'='94D5CE3889BA4F018C4717AA841E2E021288A6DF167B03D6C469D0F1BA03C013';'TaskMaster.sln'='8884A3C7B88B79C6A052FF7199D055297AF19B5BE3D5264DD32E8F9EAF7016E9';'QuickFiler/Viewers/IBreadcrumbDropDownHost.cs'='064FB82E176E138C511A3D3F45C0C82EB03AD3248320C485B046CC991E037B56';'QuickFiler/Viewers/IItemViewer.cs'='9E7BD9A5AFCCD9F06799A35BD998AD9415BFB8E15B21D84BDB143055A8064839'}; foreach($e in $hashes.GetEnumerator()){Require ((HashOf $e.Key) -eq $e.Value) "Protected hash changed: $($e.Key)"}
& git diff --quiet $base -- 'QuickFiler/Viewers/ItemViewer.Designer.cs'; Require ($LASTEXITCODE -eq 0) 'ItemViewer.Designer.cs changed from merge base.'; $configPaths=@(git ls-files | Where-Object{$_ -match '(^|/)(packages\.config|[^/]+\.runsettings|Settings\.settings|Settings\.Designer\.cs)$'})+@('coverage.config','.editorconfig','Directory.Build.targets','TaskMaster.sln','scripts/vscode/Invoke-MSTestWithCoverage.ps1'); & git diff --quiet $base -- $configPaths; Require ($LASTEXITCODE -eq 0) 'Package/config/filter/threshold/persisted-setting surface changed.'
$hostSource=Get-Content -Raw -LiteralPath 'QuickFiler/Viewers/BreadcrumbDropDownHost.cs'; Require ($hostSource -match 'public Task<bool> OpenAsync\(\s*Rectangle anchorScreenBounds,\s*Rectangle workingArea,\s*Size desiredSize\s*\)') 'Public Host OpenAsync signature changed.'; Require (([regex]::Matches($hostSource,'public ToolStripControlHost\? ControlHost\s*=>')).Count -eq 1) 'ControlHost public declaration shape changed.'; Require (([regex]::Matches($hostSource,'public bool IsOpen\s*=>')).Count -eq 1) 'IsOpen public declaration shape changed.'; $iface=Get-Content -Raw -LiteralPath 'QuickFiler/Viewers/IBreadcrumbDropDownHost.cs'; Require ($iface -match 'Task<bool> OpenAsync\(Rectangle anchorScreenBounds, Rectangle workingArea, Size desiredSize\);') 'Interface OpenAsync signature changed.'; $hostPublicDelta=@(git diff --unified=0 HEAD -- 'QuickFiler/Viewers/BreadcrumbDropDownHost.cs' | Where-Object{$_ -match '^[+-](?![+-])\s*public\s'}); $expectedHostDelta=@('-        public ToolStripControlHost? ControlHost => _controlHost;','+        public ToolStripControlHost? ControlHost => InstalledControlHost;','-        public bool IsOpen => _isOpen;','+        public bool IsOpen => OpenState;'); Require (($hostPublicDelta -join "`n") -ceq ($expectedHostDelta -join "`n")) 'Host public-line delta exceeds the two semantic-preserving getter implementation changes.'; $itemPublicDelta=@(git diff --unified=0 HEAD -- 'QuickFiler/Viewers/ItemViewer.Breadcrumb.cs' | Where-Object{$_ -match '^[+-](?![+-])\s*public\s'}); Require ($itemPublicDelta.Count -eq 0) 'ItemViewer public declaration changed.'
$exclusionDelta=@(git diff --unified=0 HEAD -- '*.cs' | Where-Object{$_ -match '^[+-](?![+-]).*ExcludeFromCodeCoverage'}); Require ($exclusionDelta.Count -eq 0) 'A post-P5 coverage exclusion attribute was added, removed, or moved.'; $popup=Get-Content -Raw -LiteralPath 'QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs'; Require (([regex]::Matches($popup,'ExcludeFromCodeCoverage')).Count -eq 7) 'Popup adapter exclusion count changed.'; foreach($name in @('ShowOwnedPopup','CreateProductionControl','BeginProductionInitialization','ReadProductionCore','BeginProductionNavigation','DisposeProductionSurface','NavigateToDocument')){Require ($popup -match "(?s)ExcludeFromCodeCoverage.*?$name") "Missing bounded Popup adapter $name."}; $item=Get-Content -Raw -LiteralPath 'QuickFiler/Viewers/ItemViewer.Breadcrumb.cs'; Require (([regex]::Matches($item,'ExcludeFromCodeCoverage')).Count -eq 2) 'ItemViewer breadcrumb adapter exclusion count changed.'; Require ($item -match '(?s)ExcludeFromCodeCoverage\]\s*internal Task<bool> AttachBreadcrumbWebViewAsync\(\)') 'Excluded Attach adapter changed.'; Require ($item -match '(?s)ExcludeFromCodeCoverage\]\s*private Tuple<.*?> CreateCollapsedBreadcrumbCandidate\(\)') 'Excluded collapsed candidate adapter changed.'; foreach($f in @('QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs','QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs','QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs','QuickFiler/Viewers/BreadcrumbMessengerHub.cs')){Require ((Select-String -LiteralPath $f -Pattern 'ExcludeFromCodeCoverage').Count -eq 0) "Excluded host-neutral body found in $f."}; foreach($needle in @('_breadcrumbDropDownOpenCoordinator.SetDroppedDown(droppedDown);','_breadcrumbDropDownOpenCoordinator?.Reset();','_breadcrumbDropDownOpenCoordinator?.HandleSelectorOpenStateChanged();','coordinator.Release();')){Require (([regex]::Matches($item,[regex]::Escape($needle))).Count -eq 1) "Coordinator delegation changed: $needle"}
"P8_T17_SCOPE_INTEGRITY_OK base=$base head=$head issue_files=$($issue.Count) new_cs=$($new.Count) exact_includes=$($new.Count) max_lines=500 headroom_files=$($limits.Count) protected_hashes=$($hashes.Count) public_signatures=semantic-match popup_adapters=7 itemviewer_adapters=2 host_neutral_exclusions=0 spam_lines=118 spam_isolated=true"
```

EXIT_CODE: `0`

Output Summary: `P8_T17_SCOPE_INTEGRITY_OK base=df5ad49c909f6b739edef45d0336151f44e827a6 head=38506fc0e433e9fe809be8ad77d2e7bc6f8d2382 issue_files=63 new_cs=49 exact_includes=49 max_lines=500 headroom_files=17 protected_hashes=20 public_signatures=semantic-match popup_adapters=7 itemviewer_adapters=2 host_neutral_exclusions=0 spam_lines=118 spam_isolated=true`

## Evidence-Order Reconciliation

- The corrected predecessor is `subfolder-scope-and-delivery-audit.2026-07-23T03-26.md`, SHA-256 `CD2DD09CB041E3BD210DD64DEFD9949DB37C811BE7360A287B3C18CB6D41F52B`.
- That P7 artifact now contains the corrected `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs` path, the complete executable delivery-cardinality assertions, and its recorded rerun result `EXIT_CODE: 0`.
- After that correction, the complete P8-T17 assertion embedded above was rerun byte-for-byte through PowerShell `Invoke-Expression` and returned the recorded success sentinel with numeric process exit code `0`.
- No source, test, project, package, configuration, filter, threshold, persisted-setting, exclusion, or generated designer file changed between the corrected P7 rerun and this P8-T17 rerun.

## Source and File-Size Integrity

- The inventory compared the current source/project/exclusion state with the live merge base and the P0/P5/P8 evidence named by P8-T17.
- All 63 issue-#400 modified or new C#/HTML files exist and are at most 500 physical lines.
- The largest applicable files remain within their approved limits: `BreadcrumbDropDownIntegrationTests.cs` is exactly 500 lines; `BreadcrumbDropDownHostTests.cs` is 499; `BreadcrumbDropDownReadinessTests.cs` is 498; `FolderBreadcrumb.html` and `FolderBreadcrumbBridgeRouterEdgeTests.cs` are 489; `BreadcrumbBridgeCoordinatorTests.cs` is 488; `BreadcrumbBridgeCoordinator.cs` is 487; `BreadcrumbCollapsedSurfaceReadinessTests.cs` and `FolderBreadcrumbBridgeRouter.cs` are 485.
- The following designated P5 bounds all pass:

| File | Current lines | Approved bound |
|---|---:|---:|
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 480 | 480 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 480 | 480 |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | 477 | 480 |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 399 | 460 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 480 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 480 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 473 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | 479 | 480 |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 309 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | 444 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 341 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | 361 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | 468 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs` | 478 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 485 | 486 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 500 | 500 |

## Project Inclusion Integrity

All 49 new C# source/test files have exactly one `Compile` include in their owning legacy project: 28 in `QuickFiler.Test`, 11 in `QuickFiler`, seven in `UtilitiesCS.Test`, and three in `UtilitiesCS`.

The explicitly required entries each have one include:

- `BreadcrumbDropDownOpenLifetime.cs`
- `BreadcrumbDropDownOpenCoordinator.cs`
- `BreadcrumbCoordinatorUpgradeLifetime.cs`
- `BreadcrumbDropDownOpenCoordinatorTests.cs`
- `BreadcrumbPopupBoundaryCoverageTests.cs`
- `BreadcrumbDropDownLifecycleCoverageTests.cs`
- `BreadcrumbMessengerHubCoverageTests.cs`
- `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`
- `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`

The four project hashes match their approved post-include state:

| Project | SHA-256 |
|---|---|
| `QuickFiler/QuickFiler.csproj` | `D05401B1F146FDF84D3B9323F2E05A97028DD511CE5FE7C6C3B438F52907F7BF` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `06663711C83A1FE5DE1B485D5B361DB9EDCE43501E0C37A5AF081DC0D0804FC7` |
| `UtilitiesCS/UtilitiesCS.csproj` | `6051C4074F238014746FAF4C0ACE4D6D9D5D72EF57125873589A5A11BFC33061` |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | `99106D06A4B026922C4FB9CDAF9C6335B92BE1FD280F664109D28650EB2B8226` |

## Protected Surfaces

- `ItemViewer.Designer.cs` is unchanged from the live merge base and retains SHA-256 `0AB37A8F78804DEF674F7E41C028BD14E634E166719FCE933F8758B55D356A5F`.
- `coverage.config`, the repository runsettings and coverage runner, both QuickFiler package manifests, `.editorconfig`, `Directory.Build.targets`, and `TaskMaster.sln` retain their approved hashes. The broader tracked package/runsettings/settings inventory has no merge-base delta.
- The protected coverage-threshold test retains SHA-256 `25EE741353DB8CFA625F5783ED7CA17697768FBAB826865F53D72F0DF4BBBD77`.
- The protected open-coordinator test and production files retain SHA-256 values `989BE280294875DCEFD2E936F6F48D65F3EAFED21B4AE4530D4E6288561AFC59` and `326D190D5BB4B3634A0ABDE6A786A25354349A925B7DE8226AEB11990C8E3B01`.
- The public `IBreadcrumbDropDownHost.cs` and `IItemViewer.cs` surfaces retain SHA-256 values `064FB82E176E138C511A3D3F45C0C82EB03AD3248320C485B046CC991E037B56` and `9E7BD9A5AFCCD9F06799A35BD998AD9415BFB8E15B21D84BDB143055A8064839`.
- Both the interface and production host retain `OpenAsync(Rectangle anchorScreenBounds, Rectangle workingArea, Size desiredSize)`.
- The only post-P5 public-line changes in `BreadcrumbDropDownHost.cs` redirect the unchanged getter signatures `ControlHost` and `IsOpen` to the current owned-state properties. `ItemViewer.Breadcrumb.cs` has no public declaration delta.

## Exclusion and Nonnumeric Adapter Integrity

- No coverage-exclusion attribute was added, removed, or moved after the post-P5 HEAD state.
- `BreadcrumbPopupUiOperations.cs` remains hash-identical at `A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396` and retains exactly seven bounded direct WebView2/WinForms adapter exclusions: `ShowOwnedPopup`, `CreateProductionControl`, `BeginProductionInitialization`, `ReadProductionCore`, `BeginProductionNavigation`, `DisposeProductionSurface`, and `NavigateToDocument`.
- `ItemViewer.Breadcrumb.cs` retains exactly two method exclusions, limited to `AttachBreadcrumbWebViewAsync()` and `CreateCollapsedBreadcrumbCandidate()`. Its four coordinator interactions remain the bounded delegation calls to `SetDroppedDown`, `Reset`, `HandleSelectorOpenStateChanged`, and `Release`.
- `BreadcrumbDropDownOpenCoordinator`, `BreadcrumbDropDownOpenLifetime`, `BreadcrumbDropDownHost`, `BreadcrumbCoordinatorUpgradeLifetime`, `BreadcrumbCollapsedSurfaceController`, and `BreadcrumbMessengerHub` contain zero exclusions. No host-neutral selector, open-lifecycle, or ownership body is excluded.

## Unrelated Committed Scope

HEAD `38506fc0e433e9fe809be8ad77d2e7bc6f8d2382` contains exactly one file, `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs`. It is an unrelated user change, is 118 lines, retains SHA-256 `99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA`, and has no uncommitted delta. It is explicitly excluded from issue-#400 remediation scope and was not modified or reverted.

P8-T17 result: PASS.
