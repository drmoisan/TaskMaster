# Popup UI-boundary composition formatting gate

Timestamp: `2026-07-22T07:52Z`

Working directory: `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25`

Command: `@(<the 12 fully expanded P5 production/test source paths listed below>) | & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' pipe-files`

CSharpier version: `1.3.0`

Exit code: `0`

Result: CSharpier processed the exact P5 composition tuple and changed no file. The before and after SHA-256 values were identical.

| Source | SHA-256 before and after |
| --- | --- |
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | `D0E8A20F353040A75ECAD3440D11DC8738A7134A3305D33F8D3A6F7F54B259A0` |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | `A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396` |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28` |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | `E53DE9BE76CB7AC3F69B43C12088A7B4B6DA6F3F2455DCF7C6C10F5A010C53F1` |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | `4AF910250A32B3A037E1ECDAE71EECA10DA3E4432379A4D7F84202DDF27436A0` |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | `615FBF946DEA7E5D4AFE2A4BB75284996167016EC9607BE144AFAC7929DE44E3` |
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` | `E4BD60150636A83CE977681249E03C63A2FC7CA96C32C5F8EF5BBB760926E62E` |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs` | `6C910ED246150F2E27BAA6C1EC422B64E5638FB81EFEB3F8B333B37D8B9AF32E` |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | `5FD7983359427300F589C0D6A2E80FC00F028DB07613F8948465EB675E1D9AFC` |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | `46E602D89378582538FFA53B80338C186CC14BE87CF5F4E44BF550986B41B1F5` |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | `25EE741353DB8CFA625F5783ED7CA17697768FBAB826865F53D72F0DF4BBBD77` |

The composition gate did not modify package, project, settings, coverage, designer, production, or test content.
