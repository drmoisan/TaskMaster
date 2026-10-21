# Final QA step 1: CSharpier format (P6-T1)

Timestamp: 2026-10-02T01-16
ITERATION: 1
Command: git -C WORKTREE status --porcelain --untracked-files=all; then one pwsh -NoProfile -Command payload after PREFIX: CMD-HASH (before), `dotnet tool run csharpier format .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"`, CMD-HASH (after) with REWRITTEN-WRITESET computed from the hash difference; then git -C WORKTREE status --porcelain --untracked-files=all.
Canonical command: dotnet tool run csharpier format .
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
Porcelain before: empty
Formatted 1637 files in 7462ms.   (processed-file count, not a rewrite count)
CSHARPIER_EXIT_CODE: 0
Write Set hashes before and after are identical:
HASH QuickFiler\Controllers\QfcDatamodel.cs = B06004A654EB1630B4759D378271187BA252F1FCC44486B9DFF9B4B08C24491F
HASH QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs = 04A1963C8D577FB6FD43079DD05446600399832EC3438971D08503CB2B11870A
HASH QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs = 0F076832B8ACEC32BA61D822D19397E7ADF9FECE4424C2EC3F73B4D7D277D3F1
HASH QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs = 4B3D6D3FD67ABC2F583EEABB6FAFEC22D3561A7B72AEC553556F31C2A30B32EE
HASH QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 05638767D69C12FE98B28DCE78B6827AD2C1EC64221F1E445862087668BF5DCA
Porcelain after: empty

REWRITTEN-WRITESET: NONE
REWRITTEN-OTHER: NONE
Clean pass; no restart.
