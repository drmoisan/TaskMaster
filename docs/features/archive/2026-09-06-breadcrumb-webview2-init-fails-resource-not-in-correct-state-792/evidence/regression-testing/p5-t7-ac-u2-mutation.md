# [P5-T7] AC-U2 non-vacuity mutation: stash discard removed from the router failure path

- Issue: #792
- Timestamp: 2026-09-17T20-44
- Command: CMD-OUTLOOK (`OUTLOOK-CLOSED: true` before each build), then for the mutated tree and again for the restored tree: CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`), CMD-VSTEST, CMD-SCOPED-RUN with `<FILTER>` = `FullyQualifiedName~BreadcrumbBridgeRouterIssue792Tests.NotifyInitializationFailed_LeavesNoStashForALaterInitialization` and `<task>` = `p5-t7-mutation` / `p5-t7-restored`; restoration by `git checkout -- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`; all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; consoles in the gitignored `coverage/p5-t7-mutation-build.log`, `coverage/p5-t7-mutation-scoped.log`, `coverage/p5-t7-restore-build.log`, `coverage/p5-t7-restored-scoped.log`; TRX under the gitignored `coverage/test-results/p5-t7-mutation/` and `coverage/test-results/p5-t7-restored/`
- EXIT_CODE: 0
- Output Summary: mutated run `Test Run Failed.`, `Total tests: 1`, `Failed: 1` (exit 1) on the pre-predicted `_navigated` count assertion (expected 1, found 2: banner then the replayed stale stash); restored run `Test Run Successful.`, `Total tests: 1`, `Passed: 1` (exit 0); file restored byte-identical (SHA-256 equal before mutation and after restoration), UTF-8 BOM present at HEAD and preserved through the edit and the restoration, `git diff --numstat HEAD -- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` prints nothing.

## Mutation (temporary; restored)

MUTATION: in `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` line 354 (re-derived before the edit; inside `NotifyInitializationFailed` at lines 346-375, immediately after `bool hadPendingDocument = _pendingDocument != null;` at `:353`) the statement `_pendingDocument = null;` is deleted. The edit is anchored on the `:353` line so the sibling `_pendingDocument = null;` inside `NotifyCoreInitialized` (`:325`) is untouched. The file carries a UTF-8 BOM at HEAD.

PREDICTED-FAILING-ASSERTION (written before the mutated run): `BreadcrumbBridgeRouterIssue792Tests.NotifyInitializationFailed_LeavesNoStashForALaterInitialization` fails on `_navigated.Should().HaveCount(1, "only the error banner may be navigated")` (`BreadcrumbBridgeRouterIssue792Tests.cs:156`): expected 1, actual 2. The bound document is stashed in the router while the host reports uninitialized (`:147`); the failure call navigates the banner directly through `_host.NavigateToString` (`BreadcrumbBridgeRouter.cs:368`, first navigation); under the mutation the stash survives, so the later `NotifyCoreInitialized` (`:153`, host now initialized) replays it (`BreadcrumbBridgeRouter.cs:322-326`, second navigation). FluentAssertions raises on the first failing assertion, so the content assertions at `:157-160` are not reached.

Needle (the `:353` line plus `_pendingDocument = null;`, each with its CRLF terminator) matched once before and zero after; the `:353` anchor line is present once after the edit; 453 lines before, 452 after; `git diff --numstat HEAD` read `0 1`; the hunk was the deletion of line 354 only (`NotifyCoreInitialized` at `:320-329` untouched). `BOM-BEFORE-MUTATION: True`, `BOM-AFTER-MUTATION: True`, `BOM-AFTER-RESTORE: True`.

Mutated build: exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 25 `CoreCompile:` lines (unanchored), 10 `csc.exe` lines (1 producing `QuickFiler.dll`, 1 producing `QuickFiler.Test.dll`); production DLL rewritten 20:43:29 to 20:44:43, test DLL 20:43:32 to 20:44:45.

OBSERVED (first `Error Message` line, verbatim up to the item dump): `Expected _navigated to contain 1 item(s) because only the error banner may be navigated, but found 2: {"<!DOCTYPE html>...` The two dumped documents are, in order, the banner document (its row reads `==== Folder list unavailable: breadcrumb initialization failed`) and the replayed stale folder document (its row carries the `Inbox` and `Alpha` segments), which is the stash that the restored code discards.

Observed run: `Failed NotifyInitializationFailed_LeavesNoStashForALaterInitialization [401 ms]`; `Test Run Failed.`; `Total tests: 1`; `Failed: 1`; exit 1.

PREDICTION-MATCHES-OBSERVATION: true (`_navigated` count at `:156`, expected 1, actual 2, the second navigation being the stale stash replayed by the later `NotifyCoreInitialized`).

## Restoration proof

- SHA256-BEFORE-MUTATION: `B5EF211E5A37889A2DBF8B50CB6099A21E6DF2C1CEDEE52BDC2A822DEB786F2B` (equal to the gitignored snapshot `coverage/p5-t7-snapshot.cs`, and equal to the [P5-T4] pre-mutation hash of the same file)
- SHA256-AFTER-RESTORE: `B5EF211E5A37889A2DBF8B50CB6099A21E6DF2C1CEDEE52BDC2A822DEB786F2B`
- RESTORED-IDENTICAL: true; BOM-AFTER-RESTORE: true; needle count after restore 1
- RESTORED: `git diff --numstat HEAD -- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` prints nothing; `git diff --no-index --numstat` snapshot vs restored file exit 0
- Scoped porcelain (`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'`) after restoration: prints nothing

Restored build: exit 0, `0 Warning(s)`, `0 Error(s)`; 21 `CoreCompile:`, 10 `csc.exe` (1 + 1); production DLL 20:44:43 to 20:45:02, test DLL 20:44:45 to 20:45:05.

Restored run: `Passed NotifyInitializationFailed_LeavesNoStashForALaterInitialization [303 ms]`; `Test Run Successful.`; `Total tests: 1`; `Passed: 1`; `Failed: 0 (omitted category)`; exit 0.
