# P2-T2 Test Project Registration (UndoAndMoveLog)

Timestamp: 2026-10-03T08-44
Command: Edit E-UCT-CSPROJ-2 on UtilitiesCS.Test/UtilitiesCS.Test.csproj; then CMD-CSPROJ; git diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 -- UtilitiesCS.Test/UtilitiesCS.Test.csproj; git status --porcelain -- UtilitiesCS.Test/UtilitiesCS.Test.csproj
EXIT_CODE: 0 (scoped to the git status --porcelain invocation, the last command)
Output Summary: the UndoAndMoveLog test file is registered at line 102; FilterOlFoldersController_Tests moved to 103; numstat 3 added, 0 deleted against the merge base; the file shows modified. This task opens the compile-red span P2-T1 to P2-T5 (the new test file calls the four-parameter seam that P2-T3 lands).

CMD-CSPROJ:
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
- UCT EmailIntelligence\SortEmail_SaveCase_Tests.cs COUNT=1 LINE=100
- UCT EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs COUNT=1 LINE=101
- UCT EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs COUNT=1 LINE=102
- UCT EmailIntelligence\FilterOlFoldersController_Tests.cs COUNT=1 LINE=103
- QFT Controllers\EfcDataModelArchiveRootTests.cs COUNT=1 LINE=127
- QFT Controllers\EfcDataModelFilerCleanupTests.cs COUNT=0 LINE=
- QFT Controllers\EfcDataModelIssue792CarryTests.cs COUNT=1 LINE=128

- NUMSTAT: 3	0	UtilitiesCS.Test/UtilitiesCS.Test.csproj
- PORCELAIN:  M UtilitiesCS.Test/UtilitiesCS.Test.csproj

Acceptance check: UCT rows match the after-P2-T2 column (SaveCase 100, AttachmentSaving 101, UndoAndMoveLog 102, FilterOlFoldersController_Tests 103); numstat reads 3, 0 and the path; the porcelain line shows the file modified. All three hold.
