# [P5-T4] AC-U7 non-vacuity mutation: outbound-queue discard skipped in the router failure path

- Issue: #792
- Timestamp: 2026-09-17T20-26
- Command: CMD-OUTLOOK (`OUTLOOK-CLOSED: true` before each build), then for the mutated tree and again for the restored tree: CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`), CMD-VSTEST, CMD-SCOPED-RUN with `<FILTER>` = `FullyQualifiedName~BreadcrumbOutboundQueueIssue792Tests.NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting` and `<task>` = `p5-t4-mutation` / `p5-t4-restored`; restoration by `git checkout -- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`; all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; consoles in the gitignored `coverage/p5-t4-mutation-build.log`, `coverage/p5-t4-mutation-scoped.log`, `coverage/p5-t4-restore-build.log`, `coverage/p5-t4-restored-scoped.log`; TRX under the gitignored `coverage/test-results/p5-t4-mutation/` and `coverage/test-results/p5-t4-restored/`
- EXIT_CODE: 0
- Output Summary: mutated run `Test Run Failed.`, `Total tests: 1`, `Failed: 1` (exit 1) on the pre-predicted `PendingCount` assertion (expected 0, found 2); restored run `Test Run Successful.`, `Total tests: 1`, `Passed: 1` (exit 0); file restored byte-identical (SHA-256 equal before mutation and after restoration), UTF-8 BOM present at HEAD and preserved through the edit and the restoration, `git diff --numstat HEAD -- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` prints nothing.

## Mutation (temporary; restored)

MUTATION: in `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` line 355 (re-derived before the edit; inside `NotifyInitializationFailed` at lines 346-376) `int discardedPayloads = _outboundQueue.DiscardPending();` became `int discardedPayloads = 0;`. Needle matched once before and zero after; replacement present once; 453 lines before and after; `git diff --numstat HEAD` read `1 1`. The file carries a UTF-8 BOM at HEAD: `BOM-BEFORE-MUTATION: True`, `BOM-AFTER-MUTATION: True`, `BOM-AFTER-RESTORE: True`.

PREDICTED-FAILING-ASSERTION: `BreadcrumbOutboundQueueIssue792Tests.NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting` fails on `queue.PendingCount.Should().Be(0, "a failed initialization must discard the buffered payloads")` (`BreadcrumbOutboundQueueIssue792Tests.cs:103-105`): expected 0, actual 2 (the two payloads buffered at `:95-97` are never discarded). The arrange-time `PendingCount.Should().Be(2)` at `:97` still passes.

Mutated build: exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 26 `CoreCompile:` lines (unanchored), 10 `csc.exe` lines (1 producing `QuickFiler.dll`, 1 producing `QuickFiler.Test.dll`); production DLL rewritten 20:25:50 to 20:26:40, test DLL 20:25:52 to 20:26:42.

OBSERVED (first `Error Message` line, verbatim): `Expected queue.PendingCount to be 0 because a failed initialization must discard the buffered payloads, but found 2 (difference of 2).`

Observed run: `Failed NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting [249 ms]`; `Test Run Failed.`; `Total tests: 1`; `Failed: 1`; exit 1.

PREDICTION-MATCHES-OBSERVATION: true.

## Restoration proof

- SHA256-BEFORE-MUTATION: `B5EF211E5A37889A2DBF8B50CB6099A21E6DF2C1CEDEE52BDC2A822DEB786F2B` (equal to the gitignored snapshot `coverage/p5-t4-snapshot.cs`)
- SHA256-AFTER-RESTORE: `B5EF211E5A37889A2DBF8B50CB6099A21E6DF2C1CEDEE52BDC2A822DEB786F2B`
- RESTORED-IDENTICAL: true; BOM-AFTER-RESTORE: true; needle count after restore 1, replacement count 0
- RESTORED: `git diff --numstat HEAD -- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` prints nothing; `git diff --no-index --numstat` snapshot vs restored file exit 0
- Scoped porcelain after restoration: prints nothing

Restored build: exit 0, `0 Warning(s)`, `0 Error(s)`; 25 `CoreCompile:`, 10 `csc.exe` (1 + 1); production DLL 20:26:40 to 20:26:57, test DLL 20:26:42 to 20:26:59.

Restored run: `Passed NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting [175 ms]`; `Test Run Successful.`; `Total tests: 1`; `Passed: 1`; `Failed: 0 (omitted category)`; exit 0.
