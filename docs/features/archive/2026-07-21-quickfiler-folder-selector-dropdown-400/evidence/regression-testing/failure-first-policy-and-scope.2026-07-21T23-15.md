# Failure-First Policy and Scope Audit

Timestamp: 2026-07-21T23-15Z

Command: Read-only PowerShell inspection over the exact 12 Phase 1 test sources using `Get-Content`, regex counts for `[TestClass]`, `[TestMethod]`/`[DataTestMethod]`, `FluentAssertions`, `Moq`, and Arrange/Act/Assert markers; exact `Select-String -SimpleMatch` counts for the nine new legacy-project `Compile` paths; `Select-String` with `Thread\.Sleep|Task\.Delay|SpinWait|GetTempPath|GetTempFileName|Path\.GetTemp|HttpClient|Microsoft\.Office\.Interop|Outlook\.Application|CoreWebView2Environment\.CreateAsync|Process\.Start|vstest\.console|dotnet\s|MessageBox\.Show|File\.Write|Directory\.Create|WebView2\.EnsureCore|\[TestCategory\("LiveOutlook"`; and `Get-FileHash -Algorithm SHA256` comparison of the 13 existing P0-T13 production/resource paths plus `Test-Path` for the five planned production helpers.

EXIT_CODE: 0

Output Summary: All Phase 1 failure-first tests satisfy the repository test-policy and scope gate. The nine new files and three modified files use MSTest with FluentAssertions and focused Moq or deterministic fakes where an external boundary exists. Tests are independent, deterministic, and use shared Arrange-Act-Assert helpers where scenario wrappers delegate to a common assertion path. No prohibited live/external/time/temp dependency was found. All test files are at most 500 lines. Every new test has exactly one adjacent project include. No production or resource file changed during Phase 1.

## Test-source inventory

| Source | Lines | Test methods | Moq/fake boundary |
|---|---:|---:|---|
| `BreadcrumbDuplicateIdentityTests.cs` | 207 | 6 | Moq provider |
| `BreadcrumbDuplicateIdentityIntegrationTests.cs` | 211 | 4 | Moq provider and tracking messenger |
| `FolderBreadcrumbAssetContractTests.cs` | 302 | 13 | Source-contract fake/input |
| `BreadcrumbUiThreadDispatchTests.cs` | 303 | 3 | Moq provider and recording dispatcher |
| `BreadcrumbCollapsedSurfaceReadinessTests.cs` | 311 | 5 | Deterministic navigation/messenger fakes |
| `QfcItemControllerBreadcrumbDropDownTests.cs` | 268 | 5 | Moq viewer/controller dependencies |
| `FolderBreadcrumbRouterSelectionConcurrencyTests.cs` | 153 | 4 | Moq provider and controlled completion |
| `BreadcrumbCoordinatorLifecycleTests.cs` | 263 | 5 | Moq provider and tracking messenger |
| `BreadcrumbPendingOpenCloseTests.cs` | 336 | 5 | Moq host and deterministic WinForms fakes |
| `BreadcrumbSubfolderSelectorSessionTests.cs` | 147 | 4 | Pure host-neutral model/session |
| `BreadcrumbSubfolderActivationTests.cs` | 381 | 5 | Moq provider/host and tracking messenger |
| `BreadcrumbSelectorMessagesTests.cs` | 289 | 11 | Pure serializer boundary |

All 12 sources contain one `[TestClass]`, use MSTest attributes, import `FluentAssertions`, and remain below 500 lines. Pure host-neutral tests do not add Moq when no dependency needs mocking. Scenario wrapper methods for duplicate follow-up actions delegate to one shared Arrange-Act-Assert implementation; assert-only asset contracts do not require a separate mutation step.

## Determinism and external-dependency scan

`PROHIBITED_PATTERN_HITS=0`

The exact scan found no wall-clock sleep/delay/spin, temporary-file creation, network client, Outlook interop/application, live WebView environment creation, display/message box, external process launch, runtime test-runner launch, file write, directory creation, or `LiveOutlook` category in the 12 sources. Controlled `TaskCompletionSource`, inline/recording synchronization contexts, mocks, and in-memory fakes provide all concurrency and boundary control.

## New project includes

Each count is exactly `1`:

- `UtilitiesCS.Test.csproj`: `BreadcrumbDuplicateIdentityTests.cs`, `FolderBreadcrumbRouterSelectionConcurrencyTests.cs`, `BreadcrumbSubfolderSelectorSessionTests.cs`.
- `QuickFiler.Test.csproj`: `BreadcrumbDuplicateIdentityIntegrationTests.cs`, `BreadcrumbUiThreadDispatchTests.cs`, `BreadcrumbCollapsedSurfaceReadinessTests.cs`, `BreadcrumbCoordinatorLifecycleTests.cs`, `BreadcrumbPendingOpenCloseTests.cs`, `BreadcrumbSubfolderActivationTests.cs`.

## P0-T13 production/resource hash reconciliation

All current SHA-256 values exactly match the P0-T13 baseline:

- `BreadcrumbStateModel.cs`: `17F562BA13791E4B94DEF8D6A0707EB7A4568509664CC3BDE58817F3D1EA691E`
- `FolderBreadcrumbBridgeRouter.cs`: `FE311478A78156228226ED5455E0C48D7AA6525AD97685ACEFD6119C70C1AFFE`
- `BreadcrumbSelectionSession.cs`: `F6B8AAEB086A1FB816757B58B9AAA15208A576CF798B08F9BB8882183DF3394A`
- `BreadcrumbSelectorMessages.cs`: `0A99D4C44004D70DF67DA84F68A44FEC6BCCA408C54374C47D1C7EA808D13AAD`
- `BreadcrumbBridgeCoordinator.cs`: `C2D83A91A1340130D22D8300527097BC1E6C6FA9E6450471139ADA05EA752DCE`
- `FolderBreadcrumb.html`: `37939034435F6380E503F9612E96C601FB75EE69DFDEA973E4018FB0AA6F9488`
- `WebView2Messenger.cs`: `15A189D317B92EA2BC20973E7986D7B8DEB0DC3FF64A5DB4EE8A492A9D2D333F`
- `BreadcrumbWebViewSurfaceFactory.cs`: `4F840DFB2EA96C462E57C5F93D6D88EC9A156251751CE2459C71696B27F767A3`
- `ItemViewer.Breadcrumb.cs`: `C7B10073B247B376A5557198B06CF438C1FC8B1CA6BCABFE172D4959388557F1`
- `QfcItemController.ViewerSetup.cs`: `0576AE9A6AB4B360897C05A14B6C136E1A7DED320454F2716564F7731F2D9590`
- `BreadcrumbMessengerHub.cs`: `D3615839D39FCE10FA7173060171FC2C6FA63FE7E10CFA5B7DBBC3DBF20B89AD`
- `BreadcrumbDropDownHost.cs`: `B219D3F2D6DD05805ADFDA95932D3A7D29F2B59BDCB3B674722D804E46719ACC`
- `ItemViewer.Designer.cs`: `0AB37A8F78804DEF674F7E41C028BD14E634E166719FCE933F8758B55D356A5F`

Mismatch count: `0`.

The five planned production helpers—`BreadcrumbRowIdentity.cs`, `BreadcrumbUiDispatcher.cs`, `BreadcrumbCollapsedSurfaceController.cs`, `BreadcrumbCoordinatorUpgradeLifetime.cs`, and `BreadcrumbDropDownOpenLifetime.cs`—remain absent before their authorized implementation batches.
