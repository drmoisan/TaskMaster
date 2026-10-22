# P3-T23 scoped CSharpier format of the Category files and repo-wide check (issue #973)

Timestamp: 2026-10-03T11-40
Command: CMD-CS-HASHSET (git -C <execution-worktree-root> hash-object --no-filters -- <six .cs paths>); CMD-CSHARPIER-FORMAT-SCOPED (pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; dotnet tool run csharpier format UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs; "CSHARPIER-FORMAT-EXIT: " + $LASTEXITCODE'); CMD-CS-HASHSET; CMD-CSHARPIER-CHECK (dotnet tool run csharpier check .)
EXIT_CODE: 0
Output Summary: the scoped format processed the two Category files and rewrote nothing (all six hashes identical before and after), so the D14 blank-line removal left the original formatter-stable and the new file was already formatted; the repo-wide check is clean with 1638 checked files, the P0-T15 baseline 1637 plus the one new file.

CSHASH-BEFORE:
CSHASH UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs 5b1dca734935543764e16fc2b66fed37673a3531
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs a6f6df937a306a568cd625c0acfe39767bcbb688
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs 7660140deb9f39295839c5641a3240a2f0b8831b
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs 8d0f1a46bec12b95aa323f84777682bab4b3a87a
CSHASH UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs 3a84b1a6897ce50db600b49d2a3d612feb2ba321
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs 14c605e2cf6ed26c3763a0f065f5671ee69606e4

CSHASH-AFTER: identical to CSHASH-BEFORE for all six paths.

Formatted 2 files in 1930ms.   (processed count, not a rewrite count)
CSHARPIER-FORMAT-EXIT: 0
FORMAT-REWROTE: none
NEWFILE-REWRITTEN: no

Checked 1638 files in 4966ms.
CSHARPIER-EXIT: 0
CHECKED-COUNT: 1638
CSHARPIER-BASELINE-COUNT: 1637
CHECKED-BASELINE-PLUS-ONE: True
