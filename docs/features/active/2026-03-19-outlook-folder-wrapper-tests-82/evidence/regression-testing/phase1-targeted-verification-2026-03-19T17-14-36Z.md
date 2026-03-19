Timestamp: 2026-03-19T17:14:36.8391079Z
Command: pwsh -NoProfile -Command "$checks = @(@('UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparerTests.cs','Null','ParentName'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderMinimalWrapperTests.cs','Unc','Restore'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperStateTests.cs','FolderSize','State'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperTraversalTests.cs','Compare','Load'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeTests.cs','Selection','Progress')); foreach ($check in $checks) { $path = $check[0]; $content = Get-Content $path -Raw; $tokens = $check[1..($check.Length - 1)]; $missing = $tokens | Where-Object { $content -notmatch [regex]::Escape($_) }; Write-Output ($path + ':Missing=' + ($(if ($missing.Count -gt 0) { $missing -join ',' } else { 'none' }))); if ($missing.Count -gt 0) { exit 1 } }"
EXIT_CODE: 0
Output Summary:
UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparerTests.cs:Missing=none
UtilitiesCS.Test/OutlookObjects/Folder/FolderMinimalWrapperTests.cs:Missing=none
UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperStateTests.cs:Missing=none
UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperTraversalTests.cs:Missing=none
UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeTests.cs:Missing=none
