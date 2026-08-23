# P9-T42 Analyzer Build Evidence

Timestamp: 2026-07-27T06:25:11-04:00

## Command

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

Result: exit code `0`; `0 Error(s)`; `6 Warning(s)`.

The warnings are the established System.Reactive `packages.config` compatibility warnings for `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, and `UtilitiesCS.Test`, plus the established duplicate-source-file warning for `UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs`. No analyzer error was reported.

## Assembly Freshness

Resolved assembly: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`

- Assembly UTC write time: `2026-07-27T10:24:58.9073946Z`
- Assembly SHA-256: `F1AA592428605212AE53F847F5E338DCBE20375401765AE136E3D315D47A7BCD`
- Newest required input UTC write time: `2026-07-27T10:23:47.0669982Z`
- Assembly newer than all eight P9-T41 scoped C# files and both listed project files: `True`

The source and project input hashes captured for this freshness proof are:

| Input | SHA-256 |
| --- | --- |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | `32FC3630C813E14DF55C702876AE2D5FCB0B713B0314D666B703F6BCBD892F31` |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | `19811E252ED35AAA0292AB3942DDD02E7F2C5620066B81C256AA97A5F4F2F9DA` |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | `709E50E6578FD8F97FD8CD56BCCE0A1B8E269A604E01F4B67E282AE7A9388760` |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | `8EB6AB9FBA022EF16EF7D1A4FC00FB137F91170ADE37458DDB0D3D560659D3C3` |
| `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | `0D92BBF92237FDC98FBD3A98F7AAC7C004E23D7A9784FF6023689D19EF578E14` |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | `601BCAD921C078F495096589C354C2FD4F247CD9C5A2F55BB6C662EBE24467DF` |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | `7BA48796D4AC13BC89E6F5285F628C606893C27564854E785DB3071D9F0472EF` |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | `1166374F7EB833D59EF877262CA2E603F5E33ACC1AFDD783812ABE8DDFC8AC63` |
| `QuickFiler/QuickFiler.csproj` | `6B28E88ED11608CF0B74A6958DDD0348A36364DA02DF9EC7A6B4D7A54A1FE079` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `44776FC699A217DAC7D2E8CDA41721F8C7696F683C49E4B484877E3EE73248B2` |
