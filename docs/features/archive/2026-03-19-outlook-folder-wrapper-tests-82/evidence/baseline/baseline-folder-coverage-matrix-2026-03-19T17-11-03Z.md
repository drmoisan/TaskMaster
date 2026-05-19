Timestamp: 2026-03-19T17:11:03.4420225Z
Command: pwsh -NoProfile -Command "$targets = @('UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs','UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs','UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs','UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs','UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs','UtilitiesCS/OutlookObjects/Folder/FolderTree.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs','UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs'); [xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; foreach ($target in $targets) { $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq $target } | Select-Object -First 1; if (-not $class) { Write-Output ('Missing=' + $target); exit 1 }; $rate = [math]::Round([double]$class.'line-rate' * 100, 2); Write-Output ($target + '=' + $rate + '%') }"
EXIT_CODE: 0
Output Summary:
UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs=42.32%
UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs=72%
UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs=100%
UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs=15.11%
UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs=17.21%
UtilitiesCS/OutlookObjects/Folder/FolderTree.cs=29.85%
UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs=70.6%
UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs=79.55%
UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs=100%
UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs=100%
UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs=82.42%
UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs=92.86%
UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs=0%
