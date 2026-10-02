# Phase 1 revert (P1-T7)

Timestamp: 2026-10-02T01-00
Command: git -C WORKTREE restore --source=HEAD --worktree -- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs; git -C WORKTREE diff --exit-code HEAD -- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs; git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test (separate calls); then CMD-HASH (pwsh payload with PREFIX)
EXIT_CODE: 0

Output Summary:
restore: exit 0
anchored diff: exit 0, printed nothing
porcelain span: printed nothing
WORKTREE-LEAF: agent-a7805823735145ca4
HASH QuickFiler\Controllers\QfcDatamodel.cs = 0C8F7E7DDEB0F1E52843DF18244A8E792CD6127B419737839F748976EEB12B94
HASH QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs = 390B28D669815DE30C1D0724938FA2E58C889F18AF8DFF62090DD5F2FACA2FDF
HASH QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs = 44A1952093125CB42891B01DC7FAD1A4C2B379E88D34082DFE040DD6BC74FAE6
HASH QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs = 4F350522F071BBF1B9890D570DC143027C979CA2CFC051054DE681E8A55EBB07
HASH QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 40EE455C7D2ACF6ADA5D01B8880B37A7BCBAF320CEC9F83A8B497015EAE56F52

All five HASH values equal the P0-T12 BASE-HASH values. The probe and the injected writer are gone; neither was committed.
