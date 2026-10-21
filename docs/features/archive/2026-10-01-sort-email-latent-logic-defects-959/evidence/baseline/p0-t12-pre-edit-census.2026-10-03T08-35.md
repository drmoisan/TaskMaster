# P0-T12 Pre-Edit Census

Timestamp: 2026-10-03T08-35
Command: CMD-CENSUS with PATHS-A/TOKENS-A, PATHS-T/TOKENS-T, PATHS-U/TOKENS-U, PATHS-S/TOKENS-S, PATHS-M/TOKENS-M, PATHS-E/TOKENS-E, PATHS-TST1/TOKENS-TST1, PATHS-TST2/TOKENS-TST2 (one invocation per pair); CMD-CSPROJ; CMD-GREP-FACTS (STAGE base); CMD-LINES with PATHS-SIX, PATHS-E, PATHS-TD, PATHS-TST1 and PATHS-TST2 (each a pwsh -NoProfile -Command payload with Set-Location to the item worktree)
EXIT_CODE: 0 (scoped to the CMD-LINES payload, the last invocation; process exit code)
Output Summary: every census TOTAL equals the BASE column of its TOKENS table; every CMD-CSPROJ row matches BASE; every CMD-GREP-FACTS line matches its base expectation; each census SHA256 equals the P0-T4 pre-edit hash; LINES A 342, T 172, U 195, S 277, M 388, L 240, E 464, TD 402, TST1 457, TST2 375.

Each census ran over a single path, so each per-file TOKEN line equals its TOTAL line; both are recorded as one row per token in the form `token = per-file count / TOTAL count`.

## CMD-CENSUS A (UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs)
- [ExcludeFromCodeCoverage] = 10 / TOTAL 10
- caseYesNoToAllResponse.NoToAll: = 0 / TOTAL 0
- caseYesNoToAllResponse.No: = 0 / TOTAL 0
- caseYesNoToAllResponse.Yes: = 0 / TOTAL 0
- caseYesNoToAllResponse.YesToAll: = 0 / TOTAL 0
- |YesNoToAllResponse. = 2 / TOTAL 2
- HasFlag = 0 / TOTAL 0
- _attachmentsAltName=YesNoToAllResponse.Empty; = 2 / TOTAL 2
- YesNoToAllResponse_ = 4 / TOTAL 4
- YesNoToAll.ShowDialog( = 6 / TOTAL 6
- new(YesNoToAll.ShowDialog) = 0 / TOTAL 0
- AllPromptSessions = 0 / TOTAL 0
- RedirectSaveFolder( = 0 / TOTAL 0
- FolderPathSave=destinationPath; = 1 / TOTAL 1
- FilePathHelperSaveAlt.FolderPath=destinationPath; = 0 / TOTAL 0
- IsPicture = 1 / TOTAL 1
- _responseSaveFile = 2 / TOTAL 2
- Func<Attachment,string,Task<bool>>trySave = 0 / TOTAL 0
- Func<string,bool>fileExists = 0 / TOTAL 0
- File.Exists = 2 / TOTAL 2
- TrySaveAttachmentAsync = 3 / TOTAL 3
- SaveCaseAsync( = 3 / TOTAL 3
- usingSystem.Diagnostics; = 1 / TOTAL 1
- usingDeedle; = 1 / TOTAL 1
- usingSDILReader; = 1 / TOTAL 1
- usingOutlook= = 1 / TOTAL 1
- usingUtilitiesCS; = 1 / TOTAL 1
- #nullableenable = 1 / TOTAL 1
- LINES = 342
- SHA256 = A619C1A7C1B98F50B39DB066CA8C4F081410AFB2CB587AA9894C919F02D8305B

## CMD-CENSUS T (UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs)
- [ExcludeFromCodeCoverage] = 2 / TOTAL 2
- Debug.WriteLine( = 3 / TOTAL 3
- catch(System.Exception){throw;} = 1 / TOTAL 1
- catch( = 3 / TOTAL 3
- catch(System.UnauthorizedAccessExceptione) = 1 / TOTAL 1
- catch(System.Exceptioninner) = 1 / TOTAL 1
- throw; = 2 / TOTAL 2
- TrySaveAttachmentCoreAsync( = 0 / TOTAL 0
- privatestaticasyncTask<bool>TrySaveAttachmentCoreAsync( = 0 / TOTAL 0
- internalstaticasyncTask<bool>TrySaveAttachmentAsync( = 1 / TOTAL 1
- internalstaticTask<bool>TrySaveAttachmentAsync( = 2 / TOTAL 2
- boolisRetryAfterClear = 0 / TOTAL 0
- isRetryAfterClear:false = 0 / TOTAL 0
- isRetryAfterClear:true = 0 / TOTAL 0
- isRetryAfterClear&&removeReadOnlyPrompt.Response==YesNoToAllResponse.YesToAll = 0 / TOTAL 0
- logger.Warn( = 0 / TOTAL 0
- logger.Error( = 0 / TOTAL 0
- createDirectory(Path.GetDirectoryName(filePathSave)); = 1 / TOTAL 1
- System.IO.Directory.CreateDirectory(path) = 1 / TOTAL 1
- removeReadOnlyPrompt.ReleaseSingleAnswer(); = 2 / TOTAL 2
- RemoveReadOnlyPrompt=new( = 1 / TOTAL 1
- usingSystem.Diagnostics; = 1 / TOTAL 1
- usingDeedle; = 1 / TOTAL 1
- usingSDILReader; = 1 / TOTAL 1
- usingOutlook= = 1 / TOTAL 1
- usingUtilitiesCS; = 1 / TOTAL 1
- #nullableenable = 1 / TOTAL 1
- LINES = 172
- SHA256 = B1E3570AF37D4EBCB0118C39DE283F485D4AFF6401F04BFDDC2A50CBD798719E

## CMD-CENSUS U (UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs)
- [ExcludeFromCodeCoverage] = 6 / TOTAL 6
- Path.Combine(strFileName,strFileLocation) = 1 / TOTAL 1
- fileExists(Path.Combine(strFileLocation,strFileName)) = 0 / TOTAL 0
- SanitizeArray( = 2 / TOTAL 2
- MovedMailsHeader = 0 / TOTAL 0
- string.Join("\t",MovedMailsHeader) = 0 / TOTAL 0
- "Triage" = 1 / TOTAL 1
- "FlaggedAsTask" = 1 / TOTAL 1
- string[14,2] = 1 / TOTAL 1
- Func<string,bool>fileExists = 0 / TOTAL 0
- Action<string,string[],string>writeTextFile = 0 / TOTAL 0
- writeTextFile( = 0 / TOTAL 0
- File.Exists = 1 / TOTAL 1
- FileIO2.WriteTextFile = 1 / TOTAL 1
- publicstaticvoidWriteCSV_StartNewFileIfDoesNotExist( = 1 / TOTAL 1
- internalstaticvoidWriteCSV_StartNewFileIfDoesNotExist( = 0 / TOTAL 0
- Debug.WriteLine( = 1 / TOTAL 1
- usingSystem.Diagnostics; = 1 / TOTAL 1
- usingSystem.Collections.Generic; = 1 / TOTAL 1
- usingDeedle; = 1 / TOTAL 1
- usingSDILReader; = 1 / TOTAL 1
- usingOutlook= = 1 / TOTAL 1
- usingUtilitiesCS; = 1 / TOTAL 1
- usingUtilitiesCS.EmailIntelligence; = 1 / TOTAL 1
- usingUtilitiesCS.OutlookExtensions; = 1 / TOTAL 1
- #nullableenable = 1 / TOTAL 1
- LINES = 195
- SHA256 = E67C57F42FC7CFB8E72CCFE5BBE3E896A46628123DA6F3214612BA4D86F3B634

## CMD-CENSUS S (UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs)
- [ExcludeFromCodeCoverage] = 4 / TOTAL 4
- usingDeedle; = 1 / TOTAL 1
- usingSDILReader; = 1 / TOTAL 1
- usingOutlook= = 1 / TOTAL 1
- usingUtilitiesCS; = 1 / TOTAL 1
- usingSystem.Diagnostics; = 1 / TOTAL 1
- usingSystem.Text.RegularExpressions; = 1 / TOTAL 1
- usingUtilitiesCS.EmailIntelligence; = 1 / TOTAL 1
- usingUtilitiesCS.ReusableTypeClasses = 1 / TOTAL 1
- usingSystem.Windows.Forms; = 1 / TOTAL 1
- usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder; = 1 / TOTAL 1
- usingUtilitiesCS.OutlookExtensions; = 1 / TOTAL 1
- usingSystem; = 1 / TOTAL 1
- #nullableenable = 1 / TOTAL 1
- LINES = 277
- SHA256 = D81EF7DA1573DAC2F6D6392F7BB07D46FCC930D83B53832291D54A48659BBA61

## CMD-CENSUS M (UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs)
- [ExcludeFromCodeCoverage] = 5 / TOTAL 5
- usingDeedle; = 1 / TOTAL 1
- usingSDILReader; = 1 / TOTAL 1
- usingOutlook= = 1 / TOTAL 1
- usingUtilitiesCS; = 1 / TOTAL 1
- usingSystem.Diagnostics; = 1 / TOTAL 1
- usingSystem.Text.RegularExpressions; = 1 / TOTAL 1
- usingUtilitiesCS.EmailIntelligence; = 1 / TOTAL 1
- usingUtilitiesCS.ReusableTypeClasses = 1 / TOTAL 1
- usingSystem.Windows.Forms; = 1 / TOTAL 1
- usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder; = 1 / TOTAL 1
- usingUtilitiesCS.OutlookExtensions; = 1 / TOTAL 1
- usingSystem; = 1 / TOTAL 1
- #nullableenable = 1 / TOTAL 1
- LINES = 388
- SHA256 = 94F7FADEF4160906B86F566F83011E5313E22F308BBA37103D21F2F48A3CCF8C

## CMD-CENSUS E (QuickFiler\Controllers\EfcDataModel.cs)
- SortEmail.Cleanup_Files(); = 1 / TOTAL 1
- ResetFilerPromptState(); = 0 / TOTAL 0
- protectedinternalvirtualvoidResetFilerPromptState() = 0 / TOTAL 0
- varresult=awaitInvokeFilerAsync(config,mailHelpers); = 1 / TOTAL 1
- result=awaitInvokeFilerAsync(config,mailHelpers); = 1 / TOTAL 1
- boolresult; = 0 / TOTAL 0
- finally{ = 0 / TOTAL 0
- returnresult; = 1 / TOTAL 1
- LINES = 464
- SHA256 = E40AB978F8E0C7242873F2120F0B27EE1D2F998472C48CE814D6BD6CA571437A

## CMD-CENSUS TST1 (UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs)
- [TestMethod] = 15 / TOTAL 15
- [DataTestMethod] = 0 / TOTAL 0
- [DataRow( = 0 / TOTAL 0
- DisplayName= = 0 / TOTAL 0
- SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows = 1 / TOTAL 1
- "SanitizeArray" = 1 / TOTAL 1
- "SanitizeArrayLineTSV" = 1 / TOTAL 1
- saveAttachments:false,savePictures:true = 1 / TOTAL 1
- saveAttachments:saveAttachments,savePictures:savePictures = 0 / TOTAL 0
- "photo.jpg,report.pdf" = 0 / TOTAL 0
- TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = 1 / TOTAL 1
- TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = 1 / TOTAL 1
- C:\Sortemail945Sandbox = 1 / TOTAL 1
- LINES = 457
- SHA256 = 791E2B9E00C565C428939F0A3A26B8DE4A613AC492B5D16F80A2CD9627F2957E

## CMD-CENSUS TST2 (UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs)
- [TestMethod] = 11 / TOTAL 11
- CreateDirectoryLimit = 0 / TOTAL 0
- TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear = 0 / TOTAL 0
- "retryboundexceeded" = 0 / TOTAL 0
- .Throws(denied) = 0 / TOTAL 0
- BeSameAs(denied) = 0 / TOTAL 0
- SetupSequence = 11 / TOTAL 11
- Times.Exactly(2) = 4 / TOTAL 4
- newSeams( = 11 / TOTAL 11
- DoNotParallelize = 0 / TOTAL 0
- Thread.Sleep = 0 / TOTAL 0
- Task.Delay = 0 / TOTAL 0
- Timeout = 0 / TOTAL 0
- MemoryAppender = 0 / TOTAL 0
- LINES = 375
- SHA256 = BFACBB41EEEC99935D7FB00B4AC94BFD2C810BAFE4C27F9D4B40DFE85F9B0645

## CMD-CSPROJ
- UCS EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs COUNT=1 LINE=817
- UCS EmailIntelligence\EmailParsingSorting\SortEmail.cs COUNT=1 LINE=818
- UCS EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs COUNT=1 LINE=819
- UCS EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs COUNT=1 LINE=820
- UCS EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs COUNT=1 LINE=821
- UCS EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs COUNT=1 LINE=822
- UCS EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs COUNT=1 LINE=823
- UCS OutlookObjects\Folder\FolderPredictor.cs COUNT=1 LINE=824
- UCT EmailIntelligence\SortEmail_Tests.cs COUNT=1 LINE=98
- UCT EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs COUNT=1 LINE=99
- UCT EmailIntelligence\SortEmail_SaveCase_Tests.cs COUNT=0 LINE=
- UCT EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs COUNT=0 LINE=
- UCT EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs COUNT=0 LINE=
- UCT EmailIntelligence\FilterOlFoldersController_Tests.cs COUNT=1 LINE=100
- QFT Controllers\EfcDataModelArchiveRootTests.cs COUNT=1 LINE=127
- QFT Controllers\EfcDataModelFilerCleanupTests.cs COUNT=0 LINE=
- QFT Controllers\EfcDataModelIssue792CarryTests.cs COUNT=1 LINE=128

## CMD-GREP-FACTS (STAGE base)
- CS-FILES: 1706
- DEAD-MEMBERS-CS: 2
- TODOMODEL-CSPROJ-MATCHES: 2
- TODOMODEL-CSPROJ-FILES: ToDoModel.Test\ToDoModel.Test.csproj
- NEW-IDENTIFIERS: 0
- SORTEMAIL-FILES: SortEmail.AttachmentSaving.cs,SortEmail.cs,SortEmail.LegacyAttachmentSaving.cs,SortEmail.MailItemSort.cs,SortEmail.TrySaveAttachment.cs,SortEmail.UndoAndMoveLog.cs
- SORTEMAIL-FILE-COUNT: 6
- SHOWDIALOG-CALLS-PARTIALS: 9
- ENUM-FIELDS-A: 4
- DEAD-TOKENS-PARTIALS: 12
- BANNED-USINGS-PARTIALS: 30
- USING-SYSTEM-PARTIAL-FILES: 6
- DEBUG-WRITELINE-T: 3
- EFCC-PARTIALS: 28
- TESTS6-PRESENT: 2
- BANNED-TEST-APIS: 0
- NON-APPROVED-FRAMEWORKS: 0
- LEGACY-FILE-EXISTS: True
- TODOMODEL-FILE-EXISTS: True

## CMD-LINES
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 342
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 172
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = 195
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = 277
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 388
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs = 240
- LINES QuickFiler\Controllers\EfcDataModel.cs = 464
- LINES ToDoModel\Email Utilities\SortItemsToExistingFolder.cs = 402
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 457
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = 375
- MAX-LINES: 464

Acceptance check: every TOTAL equals the BASE column; every CMD-CSPROJ row matches BASE; every CMD-GREP-FACTS line matches its base expectation; each census SHA256 equals the PRE-EDIT-HASH of P0-T4 (A, T, U, S, M, E, TST1, TST2); the LINES values are A 342, T 172, U 195, S 277, M 388, L 240, E 464, TD 402, TST1 457, TST2 375. All five hold.
