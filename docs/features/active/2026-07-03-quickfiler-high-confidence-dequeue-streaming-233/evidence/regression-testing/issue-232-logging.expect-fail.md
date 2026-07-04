Timestamp: 2026-07-03T17:30:39-04:00

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:ScoreRemainingQueueMailItemAsync_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndScore,FilterAsync_ProbabilityDebugLog_IncludesCallerSubjectEntryIdScoreAndTopFolder,LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore`

EXIT_CODE: 1

Output Summary:

- VSTest version 18.7.0 (x64).
- Test discovery matched `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.
- Total tests: 3.
- Failed: 3.
- `ScoreRemainingQueueMailItemAsync_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndScore` failed because `QfcDatamodel.cs` did not contain `Probability debug [QfcDatamodel.LoadRemainingEmailsToQueueAsync (master-queue admission)]`.
- `FilterAsync_ProbabilityDebugLog_IncludesCallerSubjectEntryIdScoreAndTopFolder` failed because `QfcHighConfidencePreFilter.cs` did not contain `Probability debug [QfcHighConfidencePreFilter.FilterAsync]`.
- `LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore` failed because `QfcItemController.FolderHandling.cs` did not contain `Probability debug [QfcItemController.LoadFolderHandler (FromField)]`.
- Result: expected failing regression established before porting the #232 logging behavior.
