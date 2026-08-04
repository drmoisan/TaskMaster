# Scope and Legacy Project Baseline

Timestamp: 2026-07-21T18-55Z
Command: Count lines in the in-scope production/test files with Get-Content and inspect QuickFiler/QuickFiler.csproj plus QuickFiler.Test/QuickFiler.Test.csproj Compile includes
EXIT_CODE: 0
Output Summary: The one production file is 399 lines. Existing in-scope test files range from 100 to 499 lines. New focused readiness and lifecycle-concurrency test files begin at 0 lines and require exactly one new QuickFiler.Test Compile include each.

## Pre-Change Line Counts

| Path | Baseline lines | Intended disposition |
|---|---:|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 399 | Only production implementation file; must remain at or below 500 lines |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` | 499 | Existing coverage reference; do not expand because it is at the limit |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs` | 277 | Existing lifecycle regression home; may receive only bounded additions if it remains at or below 500 lines |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 357 | Semantic native-close and closed-surface composition tests; must remain at or below 500 lines |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 314 | Inbound Up composition test; must remain at or below 500 lines |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | 100 | Available closed-surface contract home if required; must remain at or below 500 lines |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs` | 0 | New focused readiness failure-first file if required; exactly one project include required |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 0 | New focused pending-lifecycle failure-first file if required; exactly one project include required |

The two focused new test files are demonstrably required to preserve the 500-line policy: `BreadcrumbDropDownHostTests.cs` is already 499 lines, and the remediation requires multiple independent readiness and incomplete-factory lifecycle cases.

## Current Legacy Compile Includes

`QuickFiler/QuickFiler.csproj` currently contains exactly one applicable include for each existing production type:

- `Viewers\BreadcrumbMessengerHub.cs`
- `Viewers\IBreadcrumbDropDownHost.cs`
- `Viewers\BreadcrumbDropDownHost.cs`
- `Viewers\ItemViewer.Breadcrumb.cs`

`QuickFiler.Test/QuickFiler.Test.csproj` currently contains exactly one include for each existing in-scope test file:

- `Viewers\BreadcrumbSelectorCoordinatorTests.cs`
- `Viewers\BreadcrumbDropDownHostTests.cs`
- `Viewers\BreadcrumbDropDownLifecycleTests.cs`
- `Viewers\ItemViewerBreadcrumbDropDownContractTests.cs`
- `Viewers\BreadcrumbDropDownIntegrationTests.cs`
- `Controllers\QfcItemControllerBreadcrumbDropDownTests.cs`

The two not-yet-present focused test files have zero includes at baseline. Each must receive exactly one include when added. No out-of-scope production file is required by the intended remediation, and the host implementation must be kept within its remaining 101-line budget through focused restructuring rather than scope expansion.
