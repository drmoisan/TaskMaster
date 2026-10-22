# P2-T7 Production project Compile Include entries

Timestamp: 2026-10-01T21-00
Command: one Edit on UtilitiesCS/UtilitiesCS.csproj (after `EmailIntelligence\EmailParsingSorting\SortEmail.cs` added, in order, `SortEmail.AttachmentSaving.cs`, `SortEmail.LegacyAttachmentSaving.cs`, `SortEmail.MailItemSort.cs`, `SortEmail.TrySaveAttachment.cs`, `SortEmail.UndoAndMoveLog.cs`, each as `    <Compile Include="EmailIntelligence\EmailParsingSorting\<name>" />`); then CMD-CSPROJ; git diff --numstat MERGE-BASE -- UtilitiesCS/UtilitiesCS.csproj; git status --porcelain -- UtilitiesCS/UtilitiesCS.csproj (MERGE-BASE = f5b46df637de81a0f4a856152095544f859718cc)
EXIT_CODE: 0
Output Summary:
UCS Dialogs\NotImplementedDialog.cs COUNT=1 LINE=573
UCS Dialogs\YesNoToAll.cs COUNT=1 LINE=574
UCS Dialogs\YesNoToAllPromptSession.cs COUNT=0 LINE=
UCS EmailIntelligence\Bayesian\Obsolete\BayesianClassifier.cs COUNT=1 LINE=575
UCS EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs COUNT=1 LINE=816
UCS EmailIntelligence\EmailParsingSorting\SortEmail.cs COUNT=1 LINE=817
UCS EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs COUNT=1 LINE=818
UCS EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs COUNT=1 LINE=819
UCS EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs COUNT=1 LINE=820
UCS EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs COUNT=1 LINE=821
UCS EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs COUNT=1 LINE=822
UCS OutlookObjects\Folder\FolderPredictor.cs COUNT=1 LINE=823
UCT EmailIntelligence\Triage_OlLogic_Tests.cs COUNT=1 LINE=97
UCT EmailIntelligence\SortEmail_Tests.cs COUNT=1 LINE=98
UCT EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs COUNT=1 LINE=99
UCT EmailIntelligence\FilterOlFoldersController_Tests.cs COUNT=1 LINE=100
UCT Dialogs\YesNoToAll_Test.cs COUNT=1 LINE=442
UCT Dialogs\YesNoToAll_Tests.cs COUNT=1 LINE=443
UCT Dialogs\YesNoToAllPromptSession_Tests.cs COUNT=1 LINE=444
UCT ReusableTypeClasses\AsyncLazy_Tests.cs COUNT=1 LINE=445
NUMSTAT: 5	0	UtilitiesCS/UtilitiesCS.csproj
PORCELAIN: " M UtilitiesCS/UtilitiesCS.csproj"
Acceptance: every UCS line matches the "after P1-T3 / P2-T7" column (SortEmail.cs 817, the five new entries 818 to 822 in order, FolderPredictor.cs 823, YesNoToAllPromptSession.cs COUNT=0); numstat reads 5, 0 and the path; the porcelain line shows the file modified (all hold).
