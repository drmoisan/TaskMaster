# Duplicate Identity Tests CSharpier

Timestamp: 2026-07-21T22-26Z
Command: `csharpier format UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`
EXIT_CODE: 0
Output Summary: The initial scoped command formatted all three authorized batch-A test sources. The formatter output was retained. The identical command was then rerun and completed with stable SHA-256 hashes for all three files.

## Initial scoped pass

- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs`: `A2964FA3308BC98E34353CEB95BC0C3686BAB1D49416F333759B7218BE368CE7` before, `E54A0CD1B2ACC8EC1AAD9C346CB564AA4EABB191853AD4ADC3DE5025A532659B` after.
- `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs`: `B93C38C1475EA694FB4776C33A9C666E72DD599FEE0B5AA9C9C6D3680296594F` before, `A9910CD1BF0F5662A1AF56764DC9BF6D55532D00C8DE7927FAD2BFFA57E6255F` after.
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`: `D21679E886AF43A370B7669B31CCC22EA1DC7421E81622E48151032064E8D061` before, `B41EC83FE58EEAF2CA19B48FD094958A7235694490FF504E77DA8C7C9AA3052C` after.

## Required rerun

- Rerun EXIT_CODE: 0.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs`: `E54A0CD1B2ACC8EC1AAD9C346CB564AA4EABB191853AD4ADC3DE5025A532659B` before and after.
- `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs`: `A9910CD1BF0F5662A1AF56764DC9BF6D55532D00C8DE7927FAD2BFFA57E6255F` before and after.
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`: `B41EC83FE58EEAF2CA19B48FD094958A7235694490FF504E77DA8C7C9AA3052C` before and after.

The required rerun produced no further formatter delta.

Superseded: The subsequent P1-T3 analyzer build identified C# 8 nullable syntax in the new C# 7.3 QuickFiler test source. The scoped compatibility correction changed that file, so this artifact is not the current P1-T2 acceptance source. The fresh restarted pass is recorded in `duplicate-identity-tests-csharpier.2026-07-21T22-27.md`.
