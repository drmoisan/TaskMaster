# P8-T83 AC-18 branch-scope reconciliation

Commands used:

```powershell
$base = git merge-base HEAD origin/main
$authorized = ((git diff --name-only "$base..HEAD" | Where-Object { $_ -match '\\.cs$' }) + (git ls-files --others --exclude-standard | Where-Object { $_ -match '^(QuickFiler|QuickFiler\\.Test|UtilitiesCS|UtilitiesCS\\.Test)/.*\\.cs$' })) | Sort-Object -Unique
$joined = [string]::Join("`n", $authorized)
Get-FileHash spec.md -Algorithm SHA256
```

The live merge base is `e63ddc7c18ca71e2c968b3329e42d965d45af1eb`. The ordered C# ledger contains 65 paths and LF-joined SHA-256 `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7`, matching `evidence/regression-testing/member-coverage-branch-scope-ledger.2026-07-27T04-10.md`.

## Ordered ledger

```text
QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs
QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs
QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs
QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs
QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs
QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleConcurrencyTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs
QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs
QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs
QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs
QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs
QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs
QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs
QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs
QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs
QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs
QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs
QuickFiler.Test/Viewers/BreadcrumbSubfolderActivationTests.cs
QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs
QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs
QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs
QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs
QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs
QuickFiler/Viewers/BreadcrumbDropDownHost.cs
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs
QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs
QuickFiler/Viewers/BreadcrumbMessengerHub.cs
QuickFiler/Viewers/BreadcrumbPopupPlacement.cs
QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs
QuickFiler/Viewers/BreadcrumbUiDispatcher.cs
QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs
QuickFiler/Viewers/IBreadcrumbDropDownHost.cs
QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
QuickFiler/Viewers/ItemViewer.FolderSearch.cs
QuickFiler/Viewers/WebView2Messenger.cs
UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeMessagesTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRenderProjectionSelectorTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSelectorTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSubfolderSelectorSessionTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbRouterSelectionConcurrencyTests.cs
UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowIdentity.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs
```

## Inspected specification lines

- `spec.md:231` requires the 65-path `StringComparer.OrdinalIgnoreCase` ledger, its exact hash, both named SpamBayes paths, and CSharpier execution.
- `spec.md:256` is AC-18. Its unchanged marker is `[ ]`, and its wording requires the same exact 65-path/hash contract, both named SpamBayes paths, and preservation of `coverage.config` and `.csharpierignore` hashes.

The only protected hashes are `coverage.config` `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` and `.csharpierignore` `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25`.

`spec.md` SHA-256 is `7FA44064FD931E47BBAB0F877A30636CAA51797739F51CFB479FAE1FE1CBF439`.

Comparison result: PASS. The live ledger, referenced ledger, specification wording, protected-hash set, named SpamBayes paths, and unchanged AC-18 marker agree. P9-T1 is authorized.
