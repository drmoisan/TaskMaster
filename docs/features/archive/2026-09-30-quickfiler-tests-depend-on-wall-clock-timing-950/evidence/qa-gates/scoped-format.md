# Scoped CSharpier format of the Write Set (P4-T1)

Timestamp: 2026-10-02T01-05
P4-RESTART: 0
Command: one pwsh -NoProfile -Command payload after PREFIX: CMD-HASH (before); `dotnet tool run csharpier format QuickFiler\Controllers\QfcDatamodel.cs QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"`; CMD-HASH (after), with the REWRITTEN list computed from the hash difference.
Canonical command: dotnet tool run csharpier format <the five CODE5 paths>
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
Before:
HASH QuickFiler\Controllers\QfcDatamodel.cs = B06004A654EB1630B4759D378271187BA252F1FCC44486B9DFF9B4B08C24491F
HASH QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs = 04A1963C8D577FB6FD43079DD05446600399832EC3438971D08503CB2B11870A
HASH QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs = 0F076832B8ACEC32BA61D822D19397E7ADF9FECE4424C2EC3F73B4D7D277D3F1
HASH QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs = 306D98796ABB7E80A12BF5707D17CF684E648918CEE5B9EA4DE2DE705FBE8C6A
HASH QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 15BDA8A55D70B7B651579DC8058B7315C2BC62E60E4E33403A4CF75682F83039
Formatted 5 files in 4480ms.   (processed-file count, not a rewrite count)
CSHARPIER_EXIT_CODE: 0
After:
HASH QuickFiler\Controllers\QfcDatamodel.cs = B06004A654EB1630B4759D378271187BA252F1FCC44486B9DFF9B4B08C24491F
HASH QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs = 04A1963C8D577FB6FD43079DD05446600399832EC3438971D08503CB2B11870A
HASH QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs = 0F076832B8ACEC32BA61D822D19397E7ADF9FECE4424C2EC3F73B4D7D277D3F1
HASH QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs = 4B3D6D3FD67ABC2F583EEABB6FAFEC22D3561A7B72AEC553556F31C2A30B32EE
HASH QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 05638767D69C12FE98B28DCE78B6827AD2C1EC64221F1E445862087668BF5DCA

REWRITTEN: QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs, QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs

The two rewrites are the expected layout reflows (the over-width Z0 act lambda and the re-indented R4 body); formatter output wins. The committed text is therefore formatter-stable. P4-T2 re-checks every census value after this pass.
