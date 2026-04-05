Timestamp: 2026-03-19T17:18:37.3550291Z
Command: pwsh -NoProfile -Command "$checks = @(@('UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs','Query','Array'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs','FolderArray','Recents','Suggestions')); foreach ($check in $checks) { $path = $check[0]; $content = Get-Content $path -Raw; $tokens = $check[1..($check.Length - 1)]; $missing = $tokens | Where-Object { $content -notmatch [regex]::Escape($_) }; Write-Output ($path + ':Missing=' + ($(if ($missing.Count -gt 0) { $missing -join ',' } else { 'none' }))); if ($missing.Count -gt 0) { exit 1 } }"
EXIT_CODE: 0
Output Summary:
UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs:Missing=none
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:Missing=none
