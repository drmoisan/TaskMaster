# P5 authoritative focused coverage decision

Timestamp: `2026-07-22T09-06`

Command: `$ErrorActionPreference='Stop'; $path=(Resolve-Path 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-popup-ui-boundary-composition.2026-07-22T09-03.cobertura.xml').Path; $xml=[xml](Get-Content -Raw $path); function LineStat([object[]]$nodes){$groups=@($nodes | Group-Object { [int]$_.number }); $covered=@($groups | Where-Object { @($_.Group | Where-Object { [int]$_.hits -gt 0 }).Count -gt 0 }).Count; [pscustomobject]@{Covered=$covered;Valid=$groups.Count;Percent=if($groups.Count){100.0*$covered/$groups.Count}else{$null}}}; function BranchStat([object[]]$nodes){$covered=0;$valid=0; foreach($line in @($nodes)){if([string]$line.branch -eq 'true' -and [string]$line.'condition-coverage' -match '\((\d+)/(\d+)\)'){$covered += [int]$Matches[1]; $valid += [int]$Matches[2]}}; [pscustomobject]@{Covered=$covered;Valid=$valid;Percent=if($valid){100.0*$covered/$valid}else{$null}}}; function Metric($label,$lines,$branches){$lp=if($lines.Percent -eq $null){'N/A'}else{'{0:F2}' -f $lines.Percent};$bp=if($branches.Percent -eq $null){'N/A'}else{'{0:F2}' -f $branches.Percent}; "$label|LINES=$($lines.Covered)/$($lines.Valid)|LINE_PERCENT=$lp|BRANCHES=$($branches.Covered)/$($branches.Valid)|BRANCH_PERCENT=$bp"}; $scopes=@(@{Key='BreadcrumbUiDispatcher';File='BreadcrumbUiDispatcher.cs';Primary='QuickFiler.Viewers.BreadcrumbUiDispatcher'},@{Key='BreadcrumbWebViewSurfaceFactory';File='BreadcrumbWebViewSurfaceFactory.cs';Primary='QuickFiler.Viewers.BreadcrumbWebViewSurfaceFactory'},@{Key='BreadcrumbPopupUiOperations';File='BreadcrumbPopupUiOperations.cs';Primary='QuickFiler.Viewers.BreadcrumbPopupUiOperations'},@{Key='BreadcrumbDropDownOpenLifetime';File='BreadcrumbDropDownOpenLifetime.cs';Primary='QuickFiler.Viewers.BreadcrumbDropDownOpenLifetime'},@{Key='BreadcrumbDropDownHost';File='BreadcrumbDropDownHost.cs';Primary='QuickFiler.Viewers.BreadcrumbDropDownHost'},@{Key='BreadcrumbMessengerHub';File='BreadcrumbMessengerHub.cs';Primary='QuickFiler.Viewers.BreadcrumbMessengerHub'},@{Key='BreadcrumbCollapsedAttachment';File='BreadcrumbMessengerHub.cs';Primary='QuickFiler.Viewers.BreadcrumbCollapsedAttachment'},@{Key='ItemViewerBreadcrumb';File='ItemViewer.Breadcrumb.cs';Primary='QuickFiler.Viewers.ItemViewer'}); foreach($scope in $scopes){"SCOPE=$($scope.Key)"; $classes=@($xml.SelectNodes('//class') | Where-Object { [IO.Path]::GetFileName([string]$_.filename) -eq $scope.File -and [string]$_.filename -like '*\QuickFiler\Viewers\*' }); $sourceNodes=@($classes | ForEach-Object { @($_.lines.line) }); Metric 'SOURCE_UNION' (LineStat $sourceNodes) (BranchStat $sourceNodes); $primary=$classes | Where-Object { [string]$_.name -eq $scope.Primary } | Select-Object -First 1; if($null -eq $primary){'PRIMARY|UNAVAILABLE';'MEMBERS|UNAVAILABLE';'STATE_MACHINES|UNAVAILABLE';continue}; $primaryNodes=@($primary.lines.line); Metric 'PRIMARY' (LineStat $primaryNodes) (BranchStat $primaryNodes); $methods=@($primary.methods.method); $methodNodes=@($methods | ForEach-Object { @($_.lines.line) }); $methodAt90=@($methods | Where-Object { [double]$_.'line-rate' -ge 0.9 }).Count; $methodFull=@($methods | Where-Object { [double]$_.'line-rate' -ge 1.0 }).Count; Metric "MEMBER_LINES_METHODS=$methodAt90/$($methods.Count)_AT90_FULL=$methodFull/$($methods.Count)" (LineStat $methodNodes) (BranchStat $methodNodes); foreach($method in $methods | Where-Object { [double]$_.'line-rate' -lt 0.9 }){$ml=LineStat @($method.lines.line);$mb=BranchStat @($method.lines.line);Metric "LOW_METHOD=$($method.name)$($method.signature)" $ml $mb}; $stateClasses=@($classes | Where-Object { [string]$_.name -like "$($scope.Primary).<*>d__*" }); $stateNodes=@($stateClasses | ForEach-Object { @($_.lines.line) }); Metric "STATE_MACHINES=$($stateClasses.Count)" (LineStat $stateNodes) (BranchStat $stateNodes); foreach($class in @($primary)+$stateClasses){foreach($line in @($class.lines.line)){if([string]$line.branch -eq 'true' -and [string]$line.'condition-coverage' -match '\((\d+)/(\d+)\)' -and [int]$Matches[1] -lt [int]$Matches[2]){"UNCOVERED_BRANCH=$($class.name):$($line.number):$($Matches[1])/$($Matches[2])"}}}}; $attachment=$xml.SelectSingleNode("//class[@name='QuickFiler.Viewers.BreadcrumbCollapsedAttachment']"); $release=@($attachment.methods.method | Where-Object { [string]$_.name -eq 'Release' })[0]; Metric 'RELEASE(bool)' (LineStat @($release.lines.line)) (BranchStat @($release.lines.line)); foreach($line in @($release.lines.line)){if([string]$line.branch -eq 'true' -and [string]$line.'condition-coverage' -match '\((\d+)/(\d+)\)' -and [int]$Matches[1] -lt [int]$Matches[2]){"RELEASE_UNCOVERED_BRANCH=line$($line.number):$($Matches[1])/$($Matches[2])"}}; $adapters=@('ShowOwnedPopup','CreateProductionControl','BeginProductionInitialization','ReadProductionCore','BeginProductionNavigation','DisposeProductionSurface','NavigateToDocument'); $popup=$xml.SelectSingleNode("//class[@name='QuickFiler.Viewers.BreadcrumbPopupUiOperations']"); $factory=$xml.SelectSingleNode("//class[@name='QuickFiler.Viewers.BreadcrumbWebViewSurfaceFactory']"); foreach($adapter in $adapters){$popupMethods=@($popup.methods.method | Where-Object { [string]$_.name -eq $adapter }); if($popupMethods.Count -eq 0){"DIRECT_ADAPTER=BreadcrumbPopupUiOperations.$adapter|COBERTURA_ENTRY=OMITTED"}else{foreach($method in $popupMethods){Metric "DIRECT_ADAPTER=BreadcrumbPopupUiOperations.$adapter$($method.signature)" (LineStat @($method.lines.line)) (BranchStat @($method.lines.line))}}}; foreach($method in @($factory.methods.method | Where-Object { [string]$_.name -eq 'NavigateToDocument' })){Metric "DIRECT_ADAPTER=BreadcrumbWebViewSurfaceFactory.NavigateToDocument$($method.signature)" (LineStat @($method.lines.line)) (BranchStat @($method.lines.line))}; "XML_SHA256=$((Get-FileHash -Algorithm SHA256 $path).Hash)"`

EXIT_CODE: `0`

Output Summary: `REMEDIATION REQUIRED. Parsing succeeded against only the naturally completed 09-03 authoritative artifact, but multiple required source-union, primary-type, member/method, generated-state-machine, and branch values are below 90%. Changed ItemViewer breadcrumb behavior is omitted and therefore unavailable. P5-T67 and P5-T68 remain unchecked, P5-T101 is not authorized, and the workflow stops for atomic replanning without source correction.`

## Artifact authority

- Parsed artifact: `coverage-popup-ui-boundary-composition.2026-07-22T09-03.cobertura.xml`.
- SHA-256: `63246A377D836B51A5EE2FF87C75790F62E88873A6BC9BCEAD1530C6B293DD1F`.
- Coverage command result: natural completion, exit code zero, and 70/70 passing.
- Excluded evidence: the terminated 07-59 artifact and failed 08-44 artifact were not read or used.

## Numeric decision table

Line and branch counts are shown as covered/valid. Source-union line counts de-duplicate source line numbers across all first-party classes emitted for the named source file. Member status reports primary-type methods whose individual line rate is at least 90%.

| Required unit | Source union lines | Source union branches | Primary lines | Primary branches | Methods at least 90% | State-machine lines | State-machine branches | Decision |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| `BreadcrumbUiDispatcher` | 166/185 (89.73%) | 33/38 (86.84%) | 127/142 (89.44%) | 27/32 (84.38%) | 8/10 | 0/0 (N/A; none emitted) | 0/0 (N/A) | Below 90% |
| `BreadcrumbWebViewSurfaceFactory` | 123/156 (78.85%) | 36/48 (75.00%) | 14/25 (56.00%) | 9/10 (90.00%) | 1/3 | 28/35 (80.00%) | 4/8 (50.00%) | Below 90% |
| `BreadcrumbPopupUiOperations` | 216/244 (88.52%) | 93/120 (77.50%) | 72/76 (94.74%) | 27/36 (75.00%) | 23/24 | 86/91 (94.51%) | 29/36 (80.56%) | Below 90% |
| `BreadcrumbDropDownOpenLifetime` | 249/302 (82.45%) | 51/68 (75.00%) | 97/123 (78.86%) | 25/36 (69.44%) | 16/20 | 112/132 (84.85%) | 17/20 (85.00%) | Below 90% |
| `BreadcrumbDropDownHost` | 215/280 (76.79%) | 52/88 (59.09%) | 155/219 (70.78%) | 34/68 (50.00%) | 28/40 | 36/36 (100.00%) | 3/4 (75.00%) | Below 90% |
| `BreadcrumbMessengerHub` | 233/294 (79.25%) | 74/118 (62.71%) | 119/155 (76.77%) | 37/58 (63.79%) | 2/13 | 0/0 (N/A; none emitted) | 0/0 (N/A) | Below 90% |
| `BreadcrumbCollapsedAttachment` | 233/294 (79.25%) | 74/118 (62.71%) | 61/80 (76.25%) | 25/44 (56.82%) | 6/8 | 25/31 (80.65%) | 8/10 (80.00%) | Below 90% |
| Changed `ItemViewer.Breadcrumb` behavior | 0/0 (unavailable) | 0/0 (unavailable) | unavailable | unavailable | unavailable | unavailable | unavailable | Omitted; fail closed |

No-state-machine `0/0` values above are not applicable because no generated state-machine class was emitted for that primary type. The ItemViewer values are different: the required changed behavior has no first-party class or file entry and is unavailable.

## Below-threshold primary members

### BreadcrumbUiDispatcher

- `Dispatch(Action)`: 37/46 lines (80.43%); 4/6 branches (66.67%).
- `Report(Exception)`: 6/12 lines (50.00%); 1/2 branches (50.00%).

### BreadcrumbWebViewSurfaceFactory

- `Create(IWebViewCoreInitializer, string)`: 5/7 lines (71.43%); 3/4 branches (75.00%).
- `NavigateToDocument(CoreWebView2, Control, Action, string)`: 0/9 lines (0.00%); no emitted branch denominator.

### BreadcrumbPopupUiOperations

- `CaptureCurrentOrTests()`: 0/3 lines (0.00%); 0/2 branches (0.00%).

### BreadcrumbDropDownOpenLifetime

- `Schedule(Action)`: 0/3 lines (0.00%); no emitted branch denominator.
- `Schedule(Func<Task>)`: 0/10 lines (0.00%); 0/2 branches (0.00%).
- `Dispose()`: 0/10 lines (0.00%); 0/2 branches (0.00%).
- `RetainCurrentSurface(...)`: 8/9 lines (88.89%); 1/2 branches (50.00%).

### BreadcrumbDropDownHost

- Four legacy/production constructors: each 0/10 lines; emitted branch values are 0/4, 0/2, 0/0, and 0/0.
- `get_InstalledPopupMessenger()`: 0/1 line.
- `OpenAsync(...)`: 6/9 lines (66.67%); 1/2 branches (50.00%).
- `Close(...)`: 5/6 lines (83.33%); 2/4 branches (50.00%).
- `SetTheme(string)`: 0/6 lines; 0/2 branches.
- `CompleteClose(...)`: 7/8 lines (87.50%); 1/2 branches (50.00%).
- `OnDropDownClosed(...)`: 0/5 lines; 0/6 branches.
- `ThrowIfDisposed()`: 3/4 lines (75.00%); 1/2 branches (50.00%).
- `<OnDropDownClosed>b__77_0()`: 0/6 lines; 0/6 branches.

### BreadcrumbMessengerHub

- `Attach(...)`: 14/23 lines (60.87%); 2/4 branches (50.00%).
- `Detach(...)`: 9/13 lines (69.23%); 2/4 branches (50.00%).
- `PostJson(string)`: 13/15 lines (86.67%); 3/4 branches (75.00%).
- `Dispose()`: 12/14 lines (85.71%); 3/4 branches (75.00%).
- `OnSurfaceMessageReceived(...)`: 12/14 lines (85.71%); 4/8 branches (50.00%).
- `CacheState(...)`: 4/6 lines (66.67%); 5/6 branches (83.33%).
- `PostToSurface(...)`: 9/12 lines (75.00%); 2/2 branches (100.00%).
- `RewriteSelectorMode(...)`: 15/17 lines (88.24%); 7/12 branches (58.33%).
- `MessageType(string)`: 10/12 lines (83.33%); 4/8 branches (50.00%).
- `ThrowIfDisposed()`: 3/5 lines (60.00%); 1/2 branches (50.00%).
- `SafeUnsubscribe(...)`: 5/11 lines (45.45%); no emitted branch denominator.

### BreadcrumbCollapsedAttachment

- `AttachAsync(...)`: 26/43 lines (60.47%); 13/28 branches (46.43%).
- `ThrowIfDisposed()`: 3/4 lines (75.00%); 1/2 branches (50.00%).

## BreadcrumbCollapsedAttachment.Release

- Lines: 15/16 (93.75%).
- Branches: 5/6 (83.33%).
- Uncovered branch: source line 402, 1/2 conditions covered.
- Decision: line coverage exceeds 90%, but the required branch value does not.

## Uncovered branch locations

These are the incomplete primary-type and generated-state-machine branch entries emitted by the authoritative artifact.

- `BreadcrumbUiDispatcher`: 26 (1/2), 39 (1/2), 73 (1/2), 97 (1/2), 240 (1/2).
- `BreadcrumbWebViewSurfaceFactory`: 168 (1/2); generated `CreateSurfaceAsync` lines 208 (3/6) and 229 (1/2).
- `BreadcrumbPopupUiOperations`: 63 (1/2), 64 (1/2), 65 (1/2), 67 (1/2), 68 (1/2), 69 (1/2), 79 (0/2), 371 (1/2); generated `CreateAndInstallSurfaceAsync` lines 238 (4/8) and 303 (1/2), `ObserveExternalAsync` line 309 (1/2), and `RetryAsync` line 343 (1/2).
- `BreadcrumbDropDownOpenLifetime`: 41 (1/2), 42 (1/2), 56 (1/2), 97 (0/2), 126 (0/2), 217 (2/4), 323 (1/2), 380 (1/2); generated `EnsureSurfaceAsync` lines 291 (1/2) and 314 (1/2), and `OpenCoreAsync` line 194 (1/2).
- `BreadcrumbDropDownHost`: 47 (0/4), 68 (0/2), 154 (1/2), 155 (1/2), 156 (1/2), 158 (1/2), 159 (1/2), 160 (1/2), 162 (1/2), 163 (1/2), 164 (1/2), 239 (1/2), 251 (2/4), 260 (0/2), 378 (1/2), 406 (0/6), 410 (0/6), 466 (1/2); generated `ResetCoreAsync` line 315 (3/4).
- `BreadcrumbMessengerHub`: 66 (1/2), 74 (1/2), 100 (1/2), 107 (1/2), 121 (1/2), 143 (1/2), 162 (3/6), 172 (1/2), 185 (5/6), 223 (1/2), 224 (1/2), 225 (1/2), 226 (1/2), 231 (1/2), 240 (1/2), 246 (1/2), 247 (1/2), 248 (1/2), 255 (1/2).
- `BreadcrumbCollapsedAttachment`: 293 (1/2), 294 (1/2), 301 (1/2), 320 (1/2), 321 (1/2), 322 (2/4), 326 (2/4), 329 (0/2), 340 (0/2), 341 (0/2), 342 (0/2), 402 (1/2), 427 (1/2); generated `CompleteAsync` line 382 (0/2).
- Changed `ItemViewer.Breadcrumb` behavior: no branch entries were emitted, so locations are unavailable rather than covered.

## Bounded direct WebView2 and WinForms adapters

The following `BreadcrumbPopupUiOperations` adapter members have no Cobertura method entries and are listed separately as nonnumeric direct-boundary adapters:

- `ShowOwnedPopup`
- `CreateProductionControl`
- `BeginProductionInitialization`
- `ReadProductionCore`
- `BeginProductionNavigation`
- `DisposeProductionSurface`
- `NavigateToDocument`

The instrumented `BreadcrumbWebViewSurfaceFactory.NavigateToDocument` wrapper is separately measurable at 0/9 lines (0.00%) and is below threshold. No broader member is reclassified as a nonnumeric adapter.

## Gate result

- P5-T67 remains unchecked.
- P5-T68 remains unchecked.
- P5-T73 through P5-T78 remain unchecked.
- P5-T87 through P5-T89 remain unchecked.
- P5-T101 remains unchecked and was not executed.
- No acceptance criterion is checked off from this decision.
- Required next action: atomic-planner coverage remediation revision. This parsing task authorizes no production or test correction.
