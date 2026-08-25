Timestamp: 2026-08-25T14-18

Read inputs:
- remediation-inputs.2026-08-25T14-18.md
- spec.md
- artifacts/research/2026-08-25T13-36-efc-upstream-potential-folder-generator-research.md
- UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs
- UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs

Tested producer: FolderPredictor.FolderArray.

Allowed implementation paths:
- UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs, only after the required fail-before exit code 1.
- UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs.

Excluded targets: QuickFiler/Controllers/BreadcrumbBridgeRouter.cs, UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs, QuickFiler/Controllers/EfcDataModel.cs, QuickFiler/Controllers/EfcFormController.cs, persistence, COM, filesystem APIs, Store.FilePath, and mailbox @ handling.
