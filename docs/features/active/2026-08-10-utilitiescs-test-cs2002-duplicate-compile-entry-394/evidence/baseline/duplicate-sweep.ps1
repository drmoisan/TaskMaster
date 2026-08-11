[xml]$csproj = Get-Content -Raw 'UtilitiesCS.Test\UtilitiesCS.Test.csproj'
$ns = New-Object System.Xml.XmlNamespaceManager($csproj.NameTable)
$ns.AddNamespace('m', 'http://schemas.microsoft.com/developer/msbuild/2003')

$itemTypes = @('Compile', 'EmbeddedResource', 'None', 'Reference', 'ProjectReference', 'BootstrapperPackage', 'Analyzer', 'AdditionalFiles')

foreach ($itemType in $itemTypes) {
    $nodes = $csproj.SelectNodes("//m:$itemType", $ns)
    $includes = $nodes | ForEach-Object { $_.Include }
    $total = $includes.Count
    $dupGroups = $includes | Group-Object | Where-Object { $_.Count -gt 1 }
    $dupCount = $dupGroups.Count
    Write-Output "ItemType=$itemType Total=$total DuplicateIncludeValues=$dupCount"
    foreach ($g in $dupGroups) {
        Write-Output "  DUPLICATE: '$($g.Name)' x$($g.Count)"
    }
}

[xml]$pkgConfig = Get-Content -Raw 'UtilitiesCS.Test\packages.config'
$pkgNodes = $pkgConfig.SelectNodes('//package')
$pkgIds = $pkgNodes | ForEach-Object { $_.id }
$pkgTotal = $pkgIds.Count
$pkgDupGroups = $pkgIds | Group-Object | Where-Object { $_.Count -gt 1 }
Write-Output "packages.config Total=$pkgTotal DuplicateIds=$($pkgDupGroups.Count)"
foreach ($g in $pkgDupGroups) {
    Write-Output "  DUPLICATE: '$($g.Name)' x$($g.Count)"
}
