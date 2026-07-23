# Phase 8 focused-regression correction CSharpier gate

Timestamp: 2026-07-23T00:01:10.4346288-04:00

Command: `csharpier format 'QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs' 'QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs' 'QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs'`

EXIT_CODE: 0

Output Summary: CSharpier 1.3.0 formatted the exact three-file P8 correction tuple. A repeated pass retained each file byte-for-byte, proving stable formatter output.

Command: `csharpier check 'QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs' 'QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs' 'QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs'`

EXIT_CODE: 0

Output Summary: Scoped CSharpier check passed for all three files.

## Stable hashes and line limits

| File | Stable SHA-256 | Physical lines | Limit |
|---|---:|---:|---:|
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | `7C3DEAE9A4768C9ED9819787B2C7E3DE831C668094DDEA5A8546BA724AC1AC1B` | 477 | 480 |
| `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | `57EC681D71E4016D576265BE07BB3760DE82F08D2E54852FD13F60FCBF189777` | 385 | 500 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | `305C35FDCEAC1A4B52394B181A288D850FF09E6125EA51AEC9183A2C38BE0840` | 500 | 500 |

The four P8-T3 protected hashes remained unchanged after formatting.
