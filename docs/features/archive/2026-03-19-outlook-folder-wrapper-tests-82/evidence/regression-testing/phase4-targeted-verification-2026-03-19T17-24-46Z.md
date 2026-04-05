Timestamp: 2026-03-19T17:24:46.6108584Z
Command: pwsh -NoProfile -Command "$predictor = if (Test-Path 'UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs') { Get-Content 'UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs' -Raw } else { '' }; $predictorTests = if (Test-Path 'UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs') { Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs' -Raw } else { '' }; $converter = if (Test-Path 'UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs') { Get-Content 'UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs' -Raw } else { '' }; $converterTests = if (Test-Path 'UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs') { Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs' -Raw } else { '' }; Write-Output ('PredictorHasNonPublicSeam=' + [bool]($predictor -match 'internal|protected')); Write-Output ('PredictorHasInjectedTests=' + [bool]($predictorTests -match 'Injected|Prompt|Ui|Directory')); Write-Output ('ConverterHasNonPublicSeam=' + [bool]($converter -match 'internal|protected')); Write-Output ('ConverterHasInjectedTests=' + [bool]($converterTests -match 'Injected|Prompt|Dialog|Input'))"
EXIT_CODE: 0
Output Summary:
PredictorHasNonPublicSeam=True
PredictorHasInjectedTests=True
ConverterHasNonPublicSeam=True
ConverterHasInjectedTests=True
