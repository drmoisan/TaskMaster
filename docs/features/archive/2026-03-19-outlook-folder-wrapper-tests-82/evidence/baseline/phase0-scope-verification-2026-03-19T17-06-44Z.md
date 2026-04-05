Timestamp: 2026-03-19T17:06:44.0590146Z
Command: pwsh -NoProfile -Command "$prod = Select-String -Path 'UtilitiesCS/UtilitiesCS.csproj' -Pattern 'OutlookObjects\\Folder\\|OutlookObjects\\Folder\\MsgToMime\\MAPIMethods.cs' | Select-Object -ExpandProperty Line; $tests = Select-String -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj' -Pattern 'OutlookObjects\\Folder\\' | Select-Object -ExpandProperty Line; Write-Output ('ProdCount=' + $prod.Count); Write-Output ('TestIncludeCount=' + $tests.Count); Write-Output ('HasMapiMethodsTestsInclude=' + [bool]($tests -match 'MAPIMethodsTests.cs')); if ($prod.Count -ne 13) { exit 1 }"
EXIT_CODE: 0
Output Summary: ProdCount=13; TestIncludeCount=13; HasMapiMethodsTestsInclude=False
