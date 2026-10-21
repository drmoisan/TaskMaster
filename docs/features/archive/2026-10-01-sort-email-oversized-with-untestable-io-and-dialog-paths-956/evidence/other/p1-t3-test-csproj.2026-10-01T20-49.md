# P1-T3 Test project Compile Include entries

Timestamp: 2026-10-01T20-49
Command: two Edit calls on UtilitiesCS.Test/UtilitiesCS.Test.csproj (after `EmailIntelligence\SortEmail_Tests.cs` added `EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs`; after `Dialogs\YesNoToAll_Tests.cs` added `Dialogs\YesNoToAllPromptSession_Tests.cs`; four-space indent, backslash separators, self-closing, no DependentUpon); then CMD-CSPROJ; git diff --numstat MERGE-BASE -- UtilitiesCS.Test/UtilitiesCS.Test.csproj; git status --porcelain -- UtilitiesCS.Test/UtilitiesCS.Test.csproj (MERGE-BASE = f5b46df637de81a0f4a856152095544f859718cc)
EXIT_CODE: 0
Output Summary:
UCS Dialogs\NotImplementedDialog.cs COUNT=1 LINE=573
UCS Dialogs\YesNoToAll.cs COUNT=1 LINE=574
UCS Dialogs\YesNoToAllPromptSession.cs COUNT=0 LINE=
UCS EmailIntelligence\Bayesian\Obsolete\BayesianClassifier.cs COUNT=1 LINE=575
UCS EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs COUNT=1 LINE=816
UCS EmailIntelligence\EmailParsingSorting\SortEmail.cs COUNT=1 LINE=817
UCS EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs COUNT=0 LINE=
UCS EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs COUNT=0 LINE=
UCS EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs COUNT=0 LINE=
UCS EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs COUNT=0 LINE=
UCS EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs COUNT=0 LINE=
UCS OutlookObjects\Folder\FolderPredictor.cs COUNT=1 LINE=818
UCT EmailIntelligence\Triage_OlLogic_Tests.cs COUNT=1 LINE=97
UCT EmailIntelligence\SortEmail_Tests.cs COUNT=1 LINE=98
UCT EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs COUNT=1 LINE=99
UCT EmailIntelligence\FilterOlFoldersController_Tests.cs COUNT=1 LINE=100
UCT Dialogs\YesNoToAll_Test.cs COUNT=1 LINE=442
UCT Dialogs\YesNoToAll_Tests.cs COUNT=1 LINE=443
UCT Dialogs\YesNoToAllPromptSession_Tests.cs COUNT=1 LINE=444
UCT ReusableTypeClasses\AsyncLazy_Tests.cs COUNT=1 LINE=445
NUMSTAT: 2	0	UtilitiesCS.Test/UtilitiesCS.Test.csproj
PORCELAIN: " M UtilitiesCS.Test/UtilitiesCS.Test.csproj"
Note: the UCS lines are at their MERGE-BASE positions (UtilitiesCS.csproj is not edited until P2-T7), which is expected at this point.
Acceptance: every UCT line matches the "after P1-T3 / P2-T7" column (97, 98, 99, 100, 442, 443, 444, 445, each COUNT=1); numstat reads 2, 0 and the path; the porcelain line shows the file modified (all hold).
