# Duplicate Identity Tests CSharpier Accepted Restart

Timestamp: 2026-07-21T22-30Z
Command: `csharpier format UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`
EXIT_CODE: 0
Output Summary: P1-T2 was restarted after the analyzer build identified the remaining Moq/regex `Match` ambiguity in the authorized integration test. The type-qualification correction was formatted and retained. The identical three-file command was rerun and completed with stable SHA-256 hashes for every authorized source.

## Restarted scoped pass

- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs`: `E54A0CD1B2ACC8EC1AAD9C346CB564AA4EABB191853AD4ADC3DE5025A532659B` before and after.
- `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs`: `39BFF841264C7EAFBF168123FFDC98EF1A1BE2997E9301E75070C0E51175D7DB` before, `2A09432B913163F6443D3BFDE5A6EEBE016AE3F8C1511425E8994003F6E8D74E` after.
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`: `54C0281878559193B5B133C8627F066297C5576A493323C093750504F05721BD` before and after.

## Required rerun

- Rerun EXIT_CODE: 0.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs`: `E54A0CD1B2ACC8EC1AAD9C346CB564AA4EABB191853AD4ADC3DE5025A532659B` before and after.
- `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs`: `2A09432B913163F6443D3BFDE5A6EEBE016AE3F8C1511425E8994003F6E8D74E` before and after.
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`: `54C0281878559193B5B133C8627F066297C5576A493323C093750504F05721BD` before and after.

The current P1-T2 batch has no remaining formatter delta.
