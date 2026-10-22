# [P5-T3] AC-U6 non-vacuity mutation: shared constant loses its trailing space

- Issue: #792
- Timestamp: 2026-09-17T20-25
- Command: CMD-OUTLOOK (`OUTLOOK-CLOSED: true` before each build), then for the mutated tree and again for the restored tree: CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`), CMD-VSTEST, CMD-SCOPED-RUN with `<FILTER>` = `FullyQualifiedName~WebView2EnvironmentContractTests|FullyQualifiedName~EfcItemControllerTests.IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace|FullyQualifiedName~WebView2BreadcrumbHostIssue792Tests.InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam` and `<task>` = `p5-t3-mutation` / `p5-t3-restored`; restoration by `git checkout -- QuickFiler/Viewers/WebView2EnvironmentContract.cs`; all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; consoles in the gitignored `coverage/p5-t3-mutation-build.log`, `coverage/p5-t3-mutation-scoped.log`, `coverage/p5-t3-restore-build.log`, `coverage/p5-t3-restored-scoped.log`; TRX under the gitignored `coverage/test-results/p5-t3-mutation/` and `coverage/test-results/p5-t3-restored/`
- EXIT_CODE: 0
- Output Summary: mutated run `Test Run Failed.`, `Total tests: 6`, `Passed: 3`, `Failed: 3` (exit 1): the three pre-predicted tests failed on their string-equality assertions and the three pre-predicted contract-relative tests passed; restored run `Test Run Successful.`, `Total tests: 6`, `Passed: 6` (exit 0); file restored byte-identical (SHA-256 equal before mutation and after restoration), no BOM at HEAD and none introduced, `git diff --numstat HEAD -- QuickFiler/Viewers/WebView2EnvironmentContract.cs` prints nothing.

## Mutation (temporary; restored)

MUTATION: in `QuickFiler/Viewers/WebView2EnvironmentContract.cs` line 24 (re-derived before the edit) `internal const string AdditionalBrowserArguments = "--incognito ";` became `internal const string AdditionalBrowserArguments = "--incognito";` (trailing space removed). Needle matched once before and zero after; replacement present once; 53 lines before and after; `git diff --numstat HEAD` read `1 1`. `BOM-BEFORE-MUTATION: False`, `BOM-AFTER-MUTATION: False`, `BOM-AFTER-RESTORE: False`.

PREDICTED-FAILING-ASSERTION (three tests, each on its first string-equality assertion):

- `WebView2EnvironmentContractTests.AdditionalBrowserArguments_IsAsciiDoubleHyphenIncognitoWithTrailingSpace` on `actual.Should().Be(expected, ...)` (`WebView2EnvironmentContractTests.cs:43-48`): expected `"--incognito "`, actual `"--incognito"`.
- `EfcItemControllerTests.IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace` on `actual.Should().Be(expected, ...)` (`EfcItemControllerTests.cs:381-386`): the alias `EfcItemController.IncognitoArgument = WebView2EnvironmentContract.AdditionalBrowserArguments` carries the mutation, expected `"--incognito "`, actual `"--incognito"`.
- `WebView2BreadcrumbHostIssue792Tests.InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam` on `capturedOptions.AdditionalBrowserArguments.Should().Be("--incognito ", ...)` (`WebView2BreadcrumbHostIssue792Tests.cs:93-98`): actual `"--incognito"`.

PREDICTED PASSING (asserting relative to the contract, so they move with it): `ResolveUserDataFolder_CombinesLocalApplicationDataWithTheSharedLeafName`, `CreateOptions_CarriesTheSharedArgumentsOnAFreshInstance` (`:101-112`, compares to `WebView2EnvironmentContract.AdditionalBrowserArguments`), `EfcItemController_InitializeWebViewAsync_PassesTheContractValuesThroughTheSeam` (`:180`, compares to the contract value).

Mutated build: exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 26 `CoreCompile:` lines (unanchored), 10 `csc.exe` lines (1 producing `QuickFiler.dll`, 1 producing `QuickFiler.Test.dll`); production DLL rewritten 20:24:16 to 20:25:33, test DLL 20:24:17 to 20:25:35.

OBSERVED (first `Error Message` line of each failed test, verbatim):

- `IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace`: `Expected actual to be "--incognito " because Chromium command-line switches are introduced by two ASCII hyphen-minus characters, but it misses some extra whitespace at the end.`
- `AdditionalBrowserArguments_IsAsciiDoubleHyphenIncognitoWithTrailingSpace`: `Expected actual to be "--incognito " because Chromium command-line switches are introduced by two ASCII hyphen-minus characters, but it misses some extra whitespace at the end.`
- `InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam`: `Expected capturedOptions.AdditionalBrowserArguments to be "--incognito " because every WebView2 site must share the same incognito browser argument, but it misses some extra whitespace at the end.`

Observed run: `Failed IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace [148 ms]`; `Failed AdditionalBrowserArguments_IsAsciiDoubleHyphenIncognitoWithTrailingSpace [146 ms]`; `Passed ResolveUserDataFolder_CombinesLocalApplicationDataWithTheSharedLeafName [< 1 ms]`; `Passed CreateOptions_CarriesTheSharedArgumentsOnAFreshInstance [1 ms]`; `Passed EfcItemController_InitializeWebViewAsync_PassesTheContractValuesThroughTheSeam [30 ms]`; `Failed InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam [199 ms]`; `Test Run Failed.`; `Total tests: 6`; `Passed: 3`; `Failed: 3`; exit 1.

PREDICTION-MATCHES-OBSERVATION: true (all three predicted failures on the predicted assertions, expressed by FluentAssertions as "misses some extra whitespace at the end", which is its rendering of expected `"--incognito "` versus actual `"--incognito"`; all three predicted passes observed passing).

## Restoration proof

- SHA256-BEFORE-MUTATION: `14EFB6386CF4F25B5AAC28C1081212CEE25C6E0987DBE69DE695F4ECA0ED55FB` (equal to the gitignored snapshot `coverage/p5-t3-snapshot.cs`)
- SHA256-AFTER-RESTORE: `14EFB6386CF4F25B5AAC28C1081212CEE25C6E0987DBE69DE695F4ECA0ED55FB`
- RESTORED-IDENTICAL: true; needle count after restore 1, replacement count 0
- RESTORED: `git diff --numstat HEAD -- QuickFiler/Viewers/WebView2EnvironmentContract.cs` prints nothing; `git diff --no-index --numstat` snapshot vs restored file exit 0
- Scoped porcelain after restoration: prints nothing

Restored build: exit 0, `0 Warning(s)`, `0 Error(s)`; 26 `CoreCompile:`, 10 `csc.exe` (1 + 1); production DLL 20:25:33 to 20:25:50, test DLL 20:25:35 to 20:25:52.

Restored run: all six `Passed`; `Test Run Successful.`; `Total tests: 6`; `Passed: 6`; `Failed: 0 (omitted category)`; exit 0.
