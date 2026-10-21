# P5-T5 QuickFiler.Test.csproj Registration of EfcDataModelFilerCleanupTests

Timestamp: 2026-10-03T12-22
Command: Edit E-QFT-CSPROJ applied to QuickFiler.Test\QuickFiler.Test.csproj (one Compile Include line added after line 127); then CMD-CSPROJ; then git diff --numstat MERGE-BASE -- QuickFiler.Test/QuickFiler.Test.csproj and git status --porcelain -- QuickFiler.Test/QuickFiler.Test.csproj (MERGE-BASE 94287369908cc920b21b0e3256314f988ad7d2f5, recorded by P0-T3; both taken before this task's commit)
EXIT_CODE: 0 (scoped to the CMD-CSPROJ payload, its process exit code)
Output Summary: The Edit anchor occurred once and the TEF Compile entry was added at line 128. Every QFT, UCS and UCT row matches the final column of the CMD-CSPROJ table. The working-tree numstat is one line added and none deleted, and the porcelain shows the project file modified. COMPILE-RED SPAN OPEN: the last completed task is P5-T5; QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs does not compile until P5-T6 lands the `ResetFilerPromptState` seam.

## CMD-CSPROJ output

```
UCS EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs COUNT=1 LINE=817
UCS EmailIntelligence\EmailParsingSorting\SortEmail.cs COUNT=1 LINE=818
UCS EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs COUNT=1 LINE=819
UCS EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs COUNT=0 LINE=
UCS EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs COUNT=1 LINE=820
UCS EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs COUNT=1 LINE=821
UCS EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs COUNT=1 LINE=822
UCS OutlookObjects\Folder\FolderPredictor.cs COUNT=1 LINE=823
UCT EmailIntelligence\SortEmail_Tests.cs COUNT=1 LINE=98
UCT EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs COUNT=1 LINE=99
UCT EmailIntelligence\SortEmail_SaveCase_Tests.cs COUNT=1 LINE=100
UCT EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs COUNT=1 LINE=101
UCT EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs COUNT=1 LINE=102
UCT EmailIntelligence\FilterOlFoldersController_Tests.cs COUNT=1 LINE=103
QFT Controllers\EfcDataModelArchiveRootTests.cs COUNT=1 LINE=127
QFT Controllers\EfcDataModelFilerCleanupTests.cs COUNT=1 LINE=128
QFT Controllers\EfcDataModelIssue792CarryTests.cs COUNT=1 LINE=129
```

## git diff --numstat MERGE-BASE (working-tree form)

```
1	0	QuickFiler.Test/QuickFiler.Test.csproj
```

## git status --porcelain

```
 M QuickFiler.Test/QuickFiler.Test.csproj
```

## Acceptance (P5-T5, all three required)

1. Every QFT row matches the final column (ArchiveRoot 127, FilerCleanup 128, Issue792Carry 129), and every UCS and UCT row matches the final column: met.
2. The numstat line reads `1`, `0` and the path: met.
3. The porcelain line shows the file modified: met.
