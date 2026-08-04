# P9-T10 AC-18 nonnumeric adapter reconciliation

Timestamp: 2026-07-27T07-10
Command: derive the live C# ledger from the merge base plus untracked issue-400 C# files and the three planned C# paths; sort ordinal-ignore-case; LF-join; SHA-256.
Command: Get-FileHash spec.md coverage.config .csharpierignore -Algorithm SHA256.
Command: Select-String spec.md for 68-path, 2B63B4B315A68A72F23F8D5CDA3A055CEEB314BB9ADB7929B291477E7C7504A9, AC-18, and BreadcrumbItemViewerLifecycleCoordinator.
EXIT_CODE: 0

## Inspected specification lines

- spec.md:231 requires the exact 68-path issue-400 C# set, the three planned paths, both SpamBayes paths, ordinal-ignore-case ordering, and LF-joined SHA-256 2B63B4B315A68A72F23F8D5CDA3A055CEEB314BB9ADB7929B291477E7C7504A9.
- spec.md:256 is AC-18. Its marker remains [ ]; it repeats the same 68-path/hash contract, both SpamBayes paths, required CSharpier -> analyzer -> nullable -> coverage order, and preservation of coverage.config and .csharpierignore.
- spec.md SHA-256: BB416F8729990EEFDC336407EA945762FC79045A80239B78CB395B1DCA74DBBE.

## Derived planned ledger

Live merge base: e63ddc7c18ca71e2c968b3329e42d965d45af1eb.

The live 65-path branch ledger plus the three planned paths below produces exactly 68 ordered paths with LF-joined SHA-256 2B63B4B315A68A72F23F8D5CDA3A055CEEB314BB9ADB7929B291477E7C7504A9.

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
QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs
QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs
QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs
QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs
QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs
QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs
QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs
QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs
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
QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs
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

## Protected policy files

- coverage.config SHA-256: B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943.
- .csharpierignore SHA-256: 362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25.

Comparison result: PASS. The live merge-base ledger, all three planned paths, both SpamBayes paths, protected-hash set, current specification toolchain line, AC-18 wording, and unchanged AC-18 marker agree. This replaces P8-T83's historical 65-path reauthorization and authorizes only the subsequent bounded P9-T11 design task.
