# P5 collapsed-readiness disposal-ownership CSharpier gate

Timestamp: `2026-07-22T08:41:42.9249804+00:00`

Command: `$files=@((Resolve-Path 'QuickFiler/Viewers/BreadcrumbMessengerHub.cs').Path,(Resolve-Path 'QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs').Path); @($files) | & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' pipe-files`

EXIT_CODE: `0`

Output Summary: `PASS. CSharpier completed on exactly the two authorized files and made no change. BreadcrumbMessengerHub.cs retained SHA-256 AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2; BreadcrumbCollapsedSurfaceReadinessTests.cs retained SHA-256 DAEA37BB2DA09CDA8E1B845DA4336D6CF4DEEE803B7BBEF89D9E9BB9486832B3.`

## Hash verification

| File | Before | After | Result |
|---|---|---|---|
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2` | `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2` | Unchanged |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | `DAEA37BB2DA09CDA8E1B845DA4336D6CF4DEEE803B7BBEF89D9E9BB9486832B3` | `DAEA37BB2DA09CDA8E1B845DA4336D6CF4DEEE803B7BBEF89D9E9BB9486832B3` | Unchanged |
