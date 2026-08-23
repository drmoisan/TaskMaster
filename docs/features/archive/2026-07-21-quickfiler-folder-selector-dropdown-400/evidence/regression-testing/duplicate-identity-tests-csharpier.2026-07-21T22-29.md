# Duplicate Identity Tests CSharpier Final Restart

Timestamp: 2026-07-21T22-29Z
Command: `csharpier format UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`
EXIT_CODE: 0
Output Summary: P1-T2 was restarted after the analyzer build identified ambiguous regex `Match` references in the authorized asset-contract test source. The scoped qualification correction was formatted and retained. The identical command was rerun and completed with stable SHA-256 hashes for all three authorized files.

## Restarted scoped pass

- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs`: `E54A0CD1B2ACC8EC1AAD9C346CB564AA4EABB191853AD4ADC3DE5025A532659B` before and after.
- `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs`: `022E2C10988C95F43D612AB154F80505649B5B0E13635EF7EB0741A866B69413` before and after.
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`: `96D72759D3E29C36B2DEC1CDE519B3DA047B1316450C8BC3A48FB5AB6F888B57` before, `54C0281878559193B5B133C8627F066297C5576A493323C093750504F05721BD` after.

## Required rerun

- Rerun EXIT_CODE: 0.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDuplicateIdentityTests.cs`: `E54A0CD1B2ACC8EC1AAD9C346CB564AA4EABB191853AD4ADC3DE5025A532659B` before and after.
- `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs`: `022E2C10988C95F43D612AB154F80505649B5B0E13635EF7EB0741A866B69413` before and after.
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`: `54C0281878559193B5B133C8627F066297C5576A493323C093750504F05721BD` before and after.

The current batch-A test sources have no remaining formatter delta.

Superseded: The next P1-T3 run exposed the same Moq/regex `Match` ambiguity in the new integration test. The scoped correction changed that file. The current P1-T2 acceptance source is `duplicate-identity-tests-csharpier.2026-07-21T22-30.md`.
