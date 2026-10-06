# P4-T4 C# format gate (final C# pass, iteration 1)

Timestamp: 2026-10-06T18-24
Command: dotnet tool run csharpier check . (CMD-CSHARPIER-CHECK: pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; dotnet tool run csharpier check .; "CSHARPIER-EXIT: " + $LASTEXITCODE'), preceded by CMD-CS-HASHSET, CMD-CSHARPIER-FORMAT-SCOPED over the six Write Set .cs files (pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; dotnet tool run csharpier format <six paths>; "CSHARPIER-FORMAT-EXIT: " + $LASTEXITCODE') and CMD-CS-HASHSET
EXIT_CODE: 0
Output Summary: Scoped format exit 0 (`Formatted 6 files in 5439ms.`, the processed count); the before and after hash sets are identical, FORMAT-REWROTE: none. Repo-wide check `Checked 1638 files in 5686ms.`, CSHARPIER-EXIT 0; 1638 equals CSHARPIER-BASELINE-COUNT 1637 plus 1 (the Part H file).

## CMD-CS-HASHSET before

CSHASH UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs 5b1dca734935543764e16fc2b66fed37673a3531
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs a6f6df937a306a568cd625c0acfe39767bcbb688
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs 7660140deb9f39295839c5641a3240a2f0b8831b
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs 8d0f1a46bec12b95aa323f84777682bab4b3a87a
CSHASH UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs 3a84b1a6897ce50db600b49d2a3d612feb2ba321
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs 14c605e2cf6ed26c3763a0f065f5671ee69606e4

## Scoped format output

Formatted 6 files in 5439ms.
CSHARPIER-FORMAT-EXIT: 0

## CMD-CS-HASHSET after

CSHASH UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs 5b1dca734935543764e16fc2b66fed37673a3531
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs a6f6df937a306a568cd625c0acfe39767bcbb688
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs 7660140deb9f39295839c5641a3240a2f0b8831b
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs 8d0f1a46bec12b95aa323f84777682bab4b3a87a
CSHASH UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs 3a84b1a6897ce50db600b49d2a3d612feb2ba321
CSHASH UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs 14c605e2cf6ed26c3763a0f065f5671ee69606e4

FORMAT-REWROTE: none

## Check output

Checked 1638 files in 5686ms.
CSHARPIER-EXIT: 0
CHECKED-COUNT: 1638
CSHARPIER-BASELINE-COUNT: 1637 (P0-T15, evidence/baseline/csharpier-check-baseline.2026-10-03T10-50.md)
CHECKED-BASELINE-PLUS-ONE: True
