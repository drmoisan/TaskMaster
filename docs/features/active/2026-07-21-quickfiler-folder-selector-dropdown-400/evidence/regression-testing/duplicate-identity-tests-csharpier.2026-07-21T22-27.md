# Duplicate Identity Tests CSharpier Restart

Timestamp: 2026-07-21T22-27Z
Command: `csharpier format UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`
EXIT_CODE: 0
Output Summary: P1-T2 was restarted after the P1-T3 build identified C# 8 nullable syntax in the new C# 7.3 QuickFiler test source. The scoped compatibility correction was formatted, retained, and the identical three-file command was rerun. The rerun completed with stable SHA-256 hashes for every authorized source.

## Restarted scoped pass

- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs`: `E54A0CD1B2ACC8EC1AAD9C346CB564AA4EABB191853AD4ADC3DE5025A532659B` before and after.
- `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs`: `06373395DB9FCEE9698E2EF0B1FD3886E1EAAE235E31D170E9C9531428D8D486` before, `022E2C10988C95F43D612AB154F80505649B5B0E13635EF7EB0741A866B69413` after.
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`: `B41EC83FE58EEAF2CA19B48FD094958A7235694490FF504E77DA8C7C9AA3052C` before and after.

## Required rerun

- Rerun EXIT_CODE: 0.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs`: `E54A0CD1B2ACC8EC1AAD9C346CB564AA4EABB191853AD4ADC3DE5025A532659B` before and after.
- `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs`: `022E2C10988C95F43D612AB154F80505649B5B0E13635EF7EB0741A866B69413` before and after.
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`: `B41EC83FE58EEAF2CA19B48FD094958A7235694490FF504E77DA8C7C9AA3052C` before and after.

The current P1-T2 files have no remaining formatter delta.

Superseded: The following P1-T3 restart exposed ambiguous regex `Match` references after adding Moq to the asset-contract source. The scoped correction changed that file, so the current P1-T2 acceptance source is `duplicate-identity-tests-csharpier.2026-07-21T22-29.md`.
