Timestamp: 2026-03-19T17:20:56.8219475Z
Command: pwsh -NoProfile -Command "$converter = Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs' -Raw; $mapi = Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/MAPIMethodsTests.cs' -Raw; $includeCount = (Select-String -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj' -Pattern 'OutlookObjects\\Folder\\MAPIMethodsTests.cs').Count; $checks = @(('ConverterArgument=' + [bool]($converter -match 'Argument')),('ConverterMapiFolder=' + [bool]($converter -match 'MAPIFolder')),('ConverterAlternative=' + [bool]($converter -match 'Alternative|Resolve|Path')),('MapiGuid=' + [bool]($mapi -match 'Guid')),('MapiInterface=' + [bool]($mapi -match 'Interface|Enum')),('MapiIncludeCount=' + $includeCount)); $checks | ForEach-Object { Write-Output $_ }; if ($checks -contains 'ConverterArgument=False' -or $checks -contains 'ConverterMapiFolder=False' -or $checks -contains 'ConverterAlternative=False' -or $checks -contains 'MapiGuid=False' -or $checks -contains 'MapiInterface=False' -or $includeCount -ne 1) { exit 1 }"
EXIT_CODE: 0
Output Summary:
ConverterArgument=True
ConverterMapiFolder=True
ConverterAlternative=True
MapiGuid=True
MapiInterface=True
MapiIncludeCount=1
