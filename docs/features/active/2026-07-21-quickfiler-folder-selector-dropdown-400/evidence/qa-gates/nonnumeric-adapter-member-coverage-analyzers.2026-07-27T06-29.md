# P9-T42 Analyzer Build Evidence (Method-Group Successor)

Timestamp: 2026-07-27T06:29:43-04:00

The earlier P9-T42 record is superseded by `nonnumeric-adapter-member-coverage-superseded.2026-07-27T06-26.md`.

## Command and Result

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

Exit code: `0`; `0 Error(s)`; `6 Warning(s)`. The warnings are the established five System.Reactive `packages.config` compatibility warnings and the established duplicate `PercentageFormatterTests.cs` source warning. No analyzer error was reported.

## Assembly Freshness

- Resolved assembly: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
- Assembly UTC write time: `2026-07-27T10:29:31.5643407Z`
- Assembly SHA-256: `5BC6D4D7C0476646AFD4F3AF6114735B75F251BF834986FAF696AA135A86A14C`
- Newest required input UTC write time: `2026-07-27T10:28:36.0819265Z`
- Assembly newer than the eight P9-T41 scoped C# files plus `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj`: `True`

Current source hashes: coordinator `32FC3630C813E14DF55C702876AE2D5FCB0B713B0314D666B703F6BCBD892F31`; item viewer `19811E252ED35AAA0292AB3942DDD02E7F2C5620066B81C256AA97A5F4F2F9DA`; popup operations `1728E0A62E4B2B4775F20BD5460C5F365AFF8B097ED0AF6169F222A07ED86746`; lifecycle tests `8EB6AB9FBA022EF16EF7D1A4FC00FB137F91170ADE37458DDB0D3D560659D3C3`; popup tests `3EE05089236DEE9CA591ED1282FC6EE3F14D694B2CF82C7E566D1C4CE167237A`.
