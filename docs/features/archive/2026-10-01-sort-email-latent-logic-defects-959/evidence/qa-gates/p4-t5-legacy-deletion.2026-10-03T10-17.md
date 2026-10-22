# P4-T5 Legacy Partial Deletion and Project Entry Removal

Timestamp: 2026-10-03T10-17
Command: CMD-DELETE with PATH UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs (run by the coordinator via its PowerShell tool under the maintainer bypass of enforce-epic-worktree-removal-gate.ps1, payload unchanged, at HEAD dc5eac5a5ba0f8250b47b8dff4d504664710a0a7); then Edit E-UCS-CSPROJ-REMOVE on UtilitiesCS/UtilitiesCS.csproj; then CMD-CSPROJ; then git diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 -- UtilitiesCS/UtilitiesCS.csproj; then git status --porcelain -- UtilitiesCS/UtilitiesCS.csproj UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs
EXIT_CODE: 0 (scoped to the CMD-DELETE payload, its process exit code, as reported by the coordinator)
ITERATION: 2 (supersedes the stop record p4-t5-legacy-deletion.2026-10-03T09-01.md, which stays on disk unchanged as history of the DELETE CHANNEL REFUSED stop)
Output Summary: The legacy partial was deleted (EXISTS-BEFORE True, EXISTS-AFTER False), its Compile entry was removed, every UCS row matches the "after P4-T5" column, the numstat reads 0 added and 1 deleted, and the porcelain output shows the project file modified and the legacy partial as ` D`. P4-T5 acceptance met.

## Channel note

The executor's Bash route for CMD-DELETE was refused at 2026-10-03T09-01 by the PreToolUse hook enforce-epic-worktree-removal-gate.ps1 (EPIC_WORKTREE_REMOVAL_BLOCKED; see p4-t5-legacy-deletion.2026-10-03T09-01.md). The maintainer approved a bypass of that hook for the CMD-DELETE payload at P4-T5 and P5-T10 only. The coordinator ran the exact, unchanged P4-T5 CMD-DELETE payload through its PowerShell tool in the item worktree at HEAD dc5eac5a5ba0f8250b47b8dff4d504664710a0a7. The executor did not run CMD-DELETE in any form. The four printed lines below are the coordinator's output, transcribed verbatim.

## CMD-DELETE output (coordinator-run)

- EXISTS-BEFORE: True
- EXISTS-AFTER: False
- PORCELAIN:  D UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs
- EXIT_CODE of the payload: 0

## Edit E-UCS-CSPROJ-REMOVE

Applied once to UtilitiesCS/UtilitiesCS.csproj with the Edit tool (OLD lines 819 to 821 found exactly once; the LegacyAttachmentSaving Compile line removed).

## CMD-CSPROJ output (executor-run, exit 0)

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
QFT Controllers\EfcDataModelFilerCleanupTests.cs COUNT=0 LINE=
QFT Controllers\EfcDataModelIssue792CarryTests.cs COUNT=1 LINE=128
```

## Numstat (git diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 -- UtilitiesCS/UtilitiesCS.csproj)

```
0	1	UtilitiesCS/UtilitiesCS.csproj
```

## Porcelain (git status --porcelain -- UtilitiesCS/UtilitiesCS.csproj UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs)

```
 D UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs
 M UtilitiesCS/UtilitiesCS.csproj
```

## Acceptance (P4-T5, all four required)

1. EXISTS-BEFORE: True and EXISTS-AFTER: False: met.
2. Every UCS row matches the "after P4-T5" column (LegacyAttachmentSaving COUNT=0, MailItemSort 820, TrySaveAttachment 821, UndoAndMoveLog 822, FolderPredictor 823; MovedMailInfo 817, SortEmail 818, AttachmentSaving 819 unchanged): met.
3. The numstat line reads 0, 1 and the path: met.
4. The porcelain output shows the project file modified and the deleted file as ` D`: met.

Compile-red span note: P4-T5 lies inside the P4-T1 to P4-T7 compile-red span; no build has run since P4-T3. COMPILE-RED SPAN OPEN applies to the per-task checkpoint commit the delegation requires.
