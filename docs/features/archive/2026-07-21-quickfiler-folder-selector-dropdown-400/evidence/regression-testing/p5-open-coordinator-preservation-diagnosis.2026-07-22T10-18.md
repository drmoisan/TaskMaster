# P5 Open Coordinator Preservation Diagnosis

Timestamp: 2026-07-22T10:18:00Z

Command: The following read-only PowerShell inventory was executed as one command.

~~~powershell
$paths = @(
    'QuickFiler/Viewers/BreadcrumbDropDownHost.cs',
    'QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs',
    'QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs',
    'QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs',
    'QuickFiler/Viewers/ItemViewer.Breadcrumb.cs',
    'QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs',
    'QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs',
    'QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs',
    'QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs',
    'QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs',
    'QuickFiler/QuickFiler.csproj',
    'QuickFiler.Test/QuickFiler.Test.csproj',
    'QuickFiler/packages.config',
    'QuickFiler.Test/packages.config',
    'scripts/vscode/TaskMaster.cli.runsettings',
    'coverage.config',
    'QuickFiler/Viewers/ItemViewer.Designer.cs'
)
foreach ($path in $paths) {
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash
    $lines = @(Get-Content -LiteralPath $path).Count
    '{0}|{1}|{2}' -f $path, $hash, $lines
}
git diff --numstat -- @paths
$diffText = git diff --no-ext-diff -- @paths
$diffBytes = [System.Text.Encoding]::UTF8.GetBytes(($diffText -join [Environment]::NewLine))
[System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData($diffBytes)).Replace('-', '')
$testFiles = [ordered]@{
    'ItemViewerBreadcrumbDropDownContractTests' = 'QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs'
    'BreadcrumbDropDownOpenCoordinatorTests' = 'QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs'
    'BreadcrumbSelectorOpenRetryTests' = 'QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs'
    'BreadcrumbSelectorToggleUiBoundaryTests' = 'QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs'
    'BreadcrumbDropDownIntegrationTests' = 'QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs'
}
foreach ($entry in $testFiles.GetEnumerator()) {
    $regular = @(Select-String -LiteralPath $entry.Value -Pattern '^\s*\[TestMethod\]').Count
    $dataRows = @(Select-String -LiteralPath $entry.Value -Pattern '^\s*\[DataRow').Count
    '{0}|regular={1}|dataRows={2}|cases={3}' -f $entry.Key, $regular, $dataRows, ($regular + $dataRows)
}
$needles = @(
    'BreadcrumbDropDownHost.cs',
    'BreadcrumbDropDownIntegrationTests.cs',
    'BreadcrumbPopupUiOperations.cs',
    'BreadcrumbDropDownOpenCoordinator.cs',
    'ItemViewer.Breadcrumb.cs',
    'BreadcrumbWebViewSurfaceFactory.cs',
    'ItemViewerBreadcrumbDropDownContractTests.cs',
    'BreadcrumbDropDownOpenCoordinatorTests.cs',
    'BreadcrumbSelectorOpenRetryTests.cs',
    'BreadcrumbSelectorToggleUiBoundaryTests.cs'
)
foreach ($project in @('QuickFiler/QuickFiler.csproj', 'QuickFiler.Test/QuickFiler.Test.csproj')) {
    Select-String -LiteralPath $project -Pattern $needles -SimpleMatch
}
Select-String -LiteralPath 'coverage.config' -Pattern 'threshold|minimum' -CaseSensitive:$false
foreach ($path in ($paths | Where-Object { $_ -like '*.cs' })) {
    Select-String -LiteralPath $path -Pattern 'ExcludeFromCodeCoverage'
}
Select-String -LiteralPath 'QuickFiler/Viewers/BreadcrumbDropDownHost.cs' -Pattern 'NormalizeFactory\(surfaceFactory\)' -Context 6,2
Select-String -LiteralPath 'QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs' -Pattern 'NormalizeFactory' -Context 0,7
$integrationLines = Get-Content -LiteralPath 'QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs'
for ($index = 112; $index -le 137; $index++) {
    if ($integrationLines[$index - 1] -match 'CommittedIdentity|PendingIdentity|GetSelectedFolder|SelectionCount|FocusCount') {
        'line={0}|{1}' -f $index, $integrationLines[$index - 1].Trim()
    }
}
git diff --check -- @paths
~~~

EXIT_CODE: 0

Output Summary: PASS. The read-only inventory established the complete J2 protected-surface baseline. The Host is 470 physical lines, the integration test is exactly 500 physical lines with ten non-data-row tests, the exact five-class filter contains 5+10+8+4+10 cases, and git diff validation returned zero. The public Host constructor forwards an unchecked null surfaceFactory into NormalizeFactory; NormalizeFactory names its parameter factory and therefore changes the public ArgumentNullException parameter contract. The named native-close test contains exactly two stale committed A identity expectations and one stale pending B identity expectation. The pending-after-close null assertion and output-path A assertion remain present. The authorized correction scope is exactly the Host and integration-test files.

## Protected file baseline

| Path | SHA-256 | Physical lines |
|---|---|---:|
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28 | 470 |
| QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs | 455A0B76AC2606FDA73FB0CF715FC370194CBCE5D5760A3DA99FB305538AFFDB | 500 |
| QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs | A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396 | 480 |
| QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs | A4B2822F8D27AC79609AE36DB1DC10CF25CAFAEC7CEE86D4956FC9405CE4BD9C | 277 |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 8D102C659C2B22E0C27684BDCA84152400CCED4CF937ED3DF3122A7DA1863DA0 | 396 |
| QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs | 28726E027BEEF4FE4633BA5BBF00AF6DA7E6C0D59CF6093657545997B3D574C9 | 226 |
| QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs | 56391C38A5A4EF599AA82D8BB981A29C61A968753D1604D3B7081CB49090C817 | 132 |
| QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs | 7BA72D6CBCBC462136DF6C6D5072182CCBBF4BD09EDC8BC79CFF1008E0F6D98A | 395 |
| QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs | 46E602D89378582538FFA53B80338C186CC14BE87CF5F4E44BF550986B41B1F5 | 473 |
| QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs | 98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104 | 480 |
| QuickFiler/QuickFiler.csproj | 1B9B9F0DA440D3CEA918CB6B178EAC1B603D0886D08E57552C90E89CDC54550E | 588 |
| QuickFiler.Test/QuickFiler.Test.csproj | 59DC70BC44CE50E9556738A1BB80B280977576E693D9F23B29943188B2AC96FC | 452 |
| QuickFiler/packages.config | 8A4F9EF928E58289ED0964A220FC8B7B33C166098CC46A97F1498D25E8922485 | 110 |
| QuickFiler.Test/packages.config | 869B58018BDA096154A669DE597036FCC0452A8B5DD75A2841BEBE1C42393A83 | 168 |
| scripts/vscode/TaskMaster.cli.runsettings | 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 | 9 |
| coverage.config | B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943 | 24 |
| QuickFiler/Viewers/ItemViewer.Designer.cs | 0AB37A8F78804DEF674F7E41C028BD14E634E166719FCE933F8758B55D356A5F | 6224 |

The combined captured-path diff SHA-256 was 5C026DF3FF27630BC58B50A106CA9120F6ED45E1FBCD1D02CECA54A3C12EAA93 before J2 corrections.

## Test and project inventory

- Exact filter: FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests
- ItemViewerBreadcrumbDropDownContractTests: 5 regular tests, 0 data rows, 5 cases.
- BreadcrumbDropDownOpenCoordinatorTests: 10 regular tests, 0 data rows, 10 cases.
- BreadcrumbSelectorOpenRetryTests: 4 regular tests plus 4 data rows, 8 cases.
- BreadcrumbSelectorToggleUiBoundaryTests: 4 regular tests, 0 data rows, 4 cases.
- BreadcrumbDropDownIntegrationTests: 10 regular tests, 0 data rows, 10 cases.
- Production includes: BreadcrumbPopupUiOperations.cs, BreadcrumbDropDownOpenCoordinator.cs, BreadcrumbDropDownHost.cs, BreadcrumbWebViewSurfaceFactory.cs, and ItemViewer.Breadcrumb.cs each occur once in QuickFiler.csproj.
- Test includes: BreadcrumbSelectorToggleUiBoundaryTests.cs, BreadcrumbSelectorOpenRetryTests.cs, ItemViewerBreadcrumbDropDownContractTests.cs, BreadcrumbDropDownOpenCoordinatorTests.cs, and BreadcrumbDropDownIntegrationTests.cs each occur once in QuickFiler.Test.csproj.

## Threshold and exclusion inventory

- Host maximum: 480 physical lines.
- Integration test: exactly 500 physical lines; no increase allowed.
- Applicable measurable line-coverage minimum: 90 percent.
- coverage.config contains zero threshold or minimum declarations and retains its protected hash above.
- BreadcrumbPopupUiOperations.cs has seven ExcludeFromCodeCoverage declarations at lines 97, 377, 380, 387, 394, 421, and 431.
- ItemViewer.Breadcrumb.cs has two ExcludeFromCodeCoverage declarations at lines 71 and 84.
- The two test-source occurrences are reflection-based exclusion assertions, not exclusion declarations: ItemViewerBreadcrumbDropDownContractTests.cs line 123 and BreadcrumbDropDownOpenCoordinatorTests.cs line 59.
- No other captured source contains an ExcludeFromCodeCoverage occurrence.

## Diagnosis

The public legacy-surface-factory Host constructor currently calls BreadcrumbPopupUiOperations.NormalizeFactory(surfaceFactory) without a public-boundary null guard. NormalizeFactory guards its own parameter with nameof(factory), which produces ParamName factory. The minimal correction is to guard surfaceFactory at the public Host boundary before NormalizeFactory; BreadcrumbPopupUiOperations.cs must remain unchanged.

Within NativeAutomaticClose_RestoresOriginalCommittedIdentityWithoutPendingPublicationAndReturnsFocusOnce, the only stale identity literals are:

- line 123 committed A, which must become plain:0:A;
- line 124 pending B, which must become plain:1:B;
- line 131 restored committed A, which must become plain:0:A.

Line 132 preserves pending null after close, and line 133 preserves GetSelectedFolder() output path A. Selection-count and focus-count behavior remain protected.
