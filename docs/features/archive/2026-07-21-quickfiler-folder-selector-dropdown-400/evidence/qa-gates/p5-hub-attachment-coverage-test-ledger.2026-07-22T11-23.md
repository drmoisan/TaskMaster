# P5 Hub and Attachment Coverage Test Ledger

Timestamp: 2026-07-22T11:23:54Z

Revalidated Timestamp: 2026-07-22T11:30:51Z

Command: A deterministic read-only PowerShell inventory recorded physical lines, SHA-256 values, TestMethod/DataTestMethod/DataRow counts and names, the one project include and its in-memory reversal hash, prohibited-resource and exclusion counts, and protected production, existing-test, project, package, runsettings, coverage-configuration, and designer hashes; a read-only CSharpier `format --skip-write --write-stdout` projection then counted the exact formatted output without modifying the source.

EXIT_CODE: 0

Output Summary: PASS. Batch M changes exactly one new test source and one adjacent `QuickFiler.Test.csproj` Compile include, with zero production files and zero existing-test edits. `BreadcrumbMessengerHubCoverageTests.cs` is CSharpier-stable at exactly 478 physical lines, within the 480-line hard cap. It contains exactly ten non-data-row TestMethod cases, zero DataTestMethod/DataRow declarations, and zero prohibited live-UI, timing, temporary-file, process, network, or external-resource usage. The one include occurs immediately after the completed batch-L lifecycle coverage include; removing only the mixed-line-ending include restores the exact batch-L project SHA-256. The ten cases map every P5-T147 Hub/Attachment sequence and additionally exercise both collapsed-controller AttachAsync overloads, all pending sharing/conflict guards with exact error text, direct ready bypass, ThrowIfDisposed, contained SafeDispose failure, and exact invalid-candidate error/cleanup. Production, existing tests, exclusions, packages, runsettings, coverage configuration, designer, filters, and thresholds remain unchanged.

## Authorized batch

| Path | Action | Lines | Tests | SHA-256 |
|---|---|---:|---:|---|
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs` | Added | 478 stable formatted | 10 | `4387E3B3F98CE0FA5DB06488D117DBFFE214DC7212E2518D721A0134FC631EB3` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | One adjacent Compile include | 455 | n/a | `CCC27A208C1C66C72EC53CCDF51918B6BDFE868FAF2F387B61380CE09B8D627F` |

The include occurs exactly once immediately after `BreadcrumbDropDownLifecycleCoverageTests.cs`. Removing only `    <Compile Include="Viewers\BreadcrumbMessengerHubCoverageTests.cs" />` and its LF terminator in memory restores batch-L SHA-256 `BF5D92B819F14301151410A7E470C851FAA148BFA0092B79C95696409A04BB66`.

## Exact case inventory and member matrix

| Case | Explicit target |
|---|---|
| `Hub_NullDuplicateAndDisposedOperations_FollowExactContracts` | Null Attach/Detach/PostJson contracts, duplicate attach, repeated Dispose, unknown detach after disposal, and Attach/PostJson use-after-dispose |
| `Hub_SubscribeAndUnsubscribeFailures_RollBackWithoutStaleInbound` | Failing subscription rollback, contained SafeUnsubscribe failure, exact subscribe/unsubscribe counts, unknown repeated detach, and stale inbound suppression |
| `Hub_CachedAndNoncachedPosts_ReplayOnlyLatestStateInSequence` | Cached render/theme/selector state, noncached outbound broadcast, replacement sequence ordering, replay, and expanded selector rewrite |
| `Hub_MalformedMissingAndSameModeMessages_AreParsedWithoutMutation` | Missing/malformed type markers and values, malformed selector parsing, same-mode preservation, replacement rewrite, and escaped/missing literal mode marker |
| `Hub_InvalidUnknownAndStaleInboundSenders_AreIgnoredExactly` | Null, nonmessenger, unknown-messenger, detached, and disposed/stale inbound senders plus exactly one current inbound publication |
| `Attachment_ConstructorFactoryAndCandidateGuards_AllowRetry` | Attachment constructor and factory guards, factory throw, null candidate, both null-item shapes with exact cleanup, retry, both controller overload null guards, Task-overload success, AttachCore, Reset, and contained SafeDispose failure |
| `Attachment_SharedPendingAndReadyBypass_ReuseOneCandidate` | Controller same-messenger/same-readiness shared task, both pending ownership conflicts, direct ready bypass, controller reuse, attachment shared pending task, one factory call, exact readiness success, hub attachment, and ready-messenger factory bypass |
| `Attachment_StaleFactoryCandidateAndReadyReset_CleanExactlyOnce` | Reentrant stale generation, candidate readiness/messenger cleanup, repeated Reset, successful reuse, ready Release(false), repeated Dispose, and exact detach/disposal |
| `Attachment_ControllerAndHubFailures_ResetAndPermitRetry` | Controller readiness failure, controller-owned rejection cleanup, hub replay/attach rollback, controller reset, successful retry, and exact subscription/disposal counts |
| `Attachment_PendingDisposeIsIdempotentAndBlocksLaterAttach` | Pending Release(true), readiness cancellation, exact pending cleanup, repeated Dispose/Reset, attachment use-after-dispose, factory suppression, and controller ThrowIfDisposed |

Every case is a distinct `[TestMethod]`; the file contains zero `[DataTestMethod]` and zero `[DataRow]` declarations. All completions are explicitly controlled in memory. No case creates a live WebView/WinForms surface, sleeps, delays, polls, creates a temporary file, starts a process, uses network I/O, or depends on an external service.

## Protected surfaces

| Path | Lines | SHA-256 | Result |
|---|---:|---|---|
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 456 | `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2` | Unchanged |
| `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs` | 308 | `92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE` | Unchanged |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | 414 | `0B0EAC5A57FCE56900083B46E0FCE13EDEDB0B67EA1BC33676D3E310AA7EDF11` | Unchanged; retains exactly 12 tests for P5-T152 |
| `QuickFiler/QuickFiler.csproj` | 588 | `1B9B9F0DA440D3CEA918CB6B178EAC1B603D0886D08E57552C90E89CDC54550E` | Unchanged |
| `coverage.config` | 24 | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` | Unchanged |
| `scripts/vscode/TaskMaster.cli.runsettings` | 9 | `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57` | Unchanged |
| `QuickFiler/packages.config` | 110 | `8A4F9EF928E58289ED0964A220FC8B7B33C166098CC46A97F1498D25E8922485` | Unchanged |
| `QuickFiler.Test/packages.config` | 168 | `869B58018BDA096154A669DE597036FCC0452A8B5DD75A2841BEBE1C42393A83` | Unchanged |
| `QuickFiler/Viewers/ItemViewer.Designer.cs` | 6224 | `0AB37A8F78804DEF674F7E41C028BD14E634E166719FCE933F8758B55D356A5F` | Unchanged |

The new test, `BreadcrumbMessengerHub.cs`, and `BreadcrumbCollapsedSurfaceController.cs` contain zero `ExcludeFromCodeCoverage` declarations. Batch M changes no exclusion, configuration, filter, threshold, package, runsettings, designer, production, or existing-test source. No coverage threshold is weakened.
