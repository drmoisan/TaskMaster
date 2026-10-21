# P3-T2 Production project session Compile Include entry

Timestamp: 2026-10-01T21-04
Command: one Edit on UtilitiesCS/UtilitiesCS.csproj (after `    <Compile Include="Dialogs\YesNoToAll.cs" />` added `    <Compile Include="Dialogs\YesNoToAllPromptSession.cs" />`); then CMD-CSPROJ; git diff --numstat MERGE-BASE -- UtilitiesCS/UtilitiesCS.csproj; git status --porcelain -- UtilitiesCS/UtilitiesCS.csproj (MERGE-BASE = f5b46df637de81a0f4a856152095544f859718cc)
EXIT_CODE: 0
Output Summary:
UCS Dialogs\NotImplementedDialog.cs COUNT=1 LINE=573
UCS Dialogs\YesNoToAll.cs COUNT=1 LINE=574
UCS Dialogs\YesNoToAllPromptSession.cs COUNT=1 LINE=575
UCS EmailIntelligence\Bayesian\Obsolete\BayesianClassifier.cs COUNT=1 LINE=576
UCS EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs COUNT=1 LINE=817
UCS EmailIntelligence\EmailParsingSorting\SortEmail.cs COUNT=1 LINE=818
UCS EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs COUNT=1 LINE=819
UCS EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs COUNT=1 LINE=820
UCS EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs COUNT=1 LINE=821
UCS EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs COUNT=1 LINE=822
UCS EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs COUNT=1 LINE=823
UCS OutlookObjects\Folder\FolderPredictor.cs COUNT=1 LINE=824
UCT EmailIntelligence\Triage_OlLogic_Tests.cs COUNT=1 LINE=97
UCT EmailIntelligence\SortEmail_Tests.cs COUNT=1 LINE=98
UCT EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs COUNT=1 LINE=99
UCT EmailIntelligence\FilterOlFoldersController_Tests.cs COUNT=1 LINE=100
UCT Dialogs\YesNoToAll_Test.cs COUNT=1 LINE=442
UCT Dialogs\YesNoToAll_Tests.cs COUNT=1 LINE=443
UCT Dialogs\YesNoToAllPromptSession_Tests.cs COUNT=1 LINE=444
UCT ReusableTypeClasses\AsyncLazy_Tests.cs COUNT=1 LINE=445
NUMSTAT: 6	0	UtilitiesCS/UtilitiesCS.csproj
PORCELAIN: " M UtilitiesCS/UtilitiesCS.csproj"
Acceptance: every UCS and UCT line matches the "after P3-T2 (final)" column (UCS 573, 574, 575, 576, 817 to 824; UCT 97, 98, 99, 100, 442 to 445); numstat reads 6, 0 and the path; the porcelain line shows the file modified (all hold).
