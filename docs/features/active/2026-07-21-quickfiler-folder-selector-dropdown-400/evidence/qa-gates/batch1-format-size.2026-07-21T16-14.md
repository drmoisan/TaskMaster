# Batch 1 format and size

Timestamp: 2026-07-21T16-14Z

Format Command: `csharpier format UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSelectorTests.cs`

Format EXIT_CODE: 0

Format Result: 6 files formatted.

Size Command: `Get-Item -LiteralPath 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs','UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs','UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs','UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs','UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs','UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSelectorTests.cs' | ForEach-Object { [pscustomobject]@{ Path = $_.FullName; Lines = (Get-Content -LiteralPath $_.FullName).Count } }`

Size EXIT_CODE: 0

| File | Lines |
|---|---:|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | 210 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs` | 237 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 449 |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs` | 203 |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs` | 143 |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSelectorTests.cs` | 121 |

Output Summary: CSharpier completed successfully, and all six batch files are at or below the 500-line repository limit.
