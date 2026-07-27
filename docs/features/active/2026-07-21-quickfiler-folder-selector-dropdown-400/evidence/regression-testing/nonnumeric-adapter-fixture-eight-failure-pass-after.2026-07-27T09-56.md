# P9-T33 non-numeric adapter fixture focused pass-after gate

Timestamp: 2026-07-27T09:57:00.5146204Z

Command: captured in `nonnumeric-adapter-fixture-eight-failure-pass-after.2026-07-27T09-56.process-tree.json`; it invoked the resolved `vstest.console.exe` on `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` with `/InIsolation`, the exact eight-method `FullyQualifiedName` filter, `/Logger:Console;Verbosity=Detailed`, and the canonical TRX logger.

EXIT_CODE: 0

Output Summary: 10 discovered, 10 executed, 10 passed, 0 failed, and 0 skipped. The three rows of `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp` account for the two sibling cases in addition to the eight P9-T26 fail-before cases.

## Results

1. `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup` — Passed
2. `ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment` — Passed
3. `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp` — Passed
4. `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface` — Passed
5. `ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce` — Passed
6. `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (0)` — Passed
7. `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly` — Passed
8. `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (1)` — Passed
9. `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (2)` — Passed
10. `SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle` — Passed

## Receipts and cleanup

- TRX: `nonnumeric-adapter-fixture-eight-failure-pass-after.2026-07-27T09-56.trx`
- TRX SHA-256: `8BA91C55F1A82EFC86B7C6B26795FDDD3320FF15326019409429F4586AC70CB6`
- Stdout: `nonnumeric-adapter-fixture-eight-failure-pass-after.2026-07-27T09-56.stdout.txt` (`7FC5831B5200D55252547984A8751C69DFB64E36C74824B5736B1B0ECC436C36`)
- Stderr: `nonnumeric-adapter-fixture-eight-failure-pass-after.2026-07-27T09-56.stderr.txt` (`E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`)
- Process tree: `nonnumeric-adapter-fixture-eight-failure-pass-after.2026-07-27T09-56.process-tree.json` (`5595BDBFDA43673E7DF7F7A7B5F7E29883E1A53F5077CDBEEA80E97698F5D913`)
- Observed tree: runner `209704` → VSTest `270736` → testhost `251640` → conhost `256404`.
- Timed out: `False`; terminated descendants: none; post-run related VSTest/testhost/dotnet processes: 0.
