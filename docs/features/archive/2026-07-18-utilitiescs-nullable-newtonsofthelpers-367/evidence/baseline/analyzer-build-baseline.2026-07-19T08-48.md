# Analyzer / Codestyle Build Baseline (P0-T3)

- Timestamp: 2026-07-19T08-48
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (VS18 full-framework MSBuild.exe, `/m`)
- EXIT_CODE: 0
- Output Summary: Build succeeded. `75 Warning(s), 0 Error(s)`.

## Baseline warning composition (all pre-existing, not introduced by #367)

- CS8632 "The annotation for nullable reference types should only be used in code within a '#nullable' annotations context" — the majority, in `*.Test` projects (`UtilitiesCS.Test`, `TaskMaster.Test`): `OlTableExtensions_Tests.cs`, `ConversationHelper_ExtendedTests.cs`, `ProgressTracker_Tests.cs`, `ManualFireTimerWrapper.cs`, `ApplicationGlobalsStartupTimingTests.cs`, `TestableApplicationGlobals.cs`, `StoreRehookCoordinatorTests.cs`, `AppToDoObjectsTests.cs`, `EngineInitTimingProbeTests.cs`. These are test files, not in-scope `NewtonsoftHelpers/` production files.
- CS0067 "event never used" — `SmartSerializableBase_Tests.cs`, `SmartSerializable_Tests.cs`, `StoreWrapperControllerTests.cs`.
- CS2002 "Source file specified multiple times" — `UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs`.
- CS0649 "field never assigned" — vendored `SVGControl/SvgImageSelector.cs`.

## Environment bootstrap note (no tracked-file edits)

The committed csprojs reference analyzer versions in their `<Analyzer Include>` items (Meziantou.Analyzer 3.0.101, SonarAnalyzer.CSharp 10.27.0.140913, Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4) that differ from the versions declared in `packages.config` (3.0.123 / 10.29.0.143774 / 5.6.0). This is a PRE-EXISTING inconsistency present on both this branch's HEAD and `origin/main` (the 3 commits this branch trails are memory/chore-only). `nuget.exe restore` restores only the packages.config versions, so the stale `<Analyzer Include>` paths do not resolve and the build fails with CS0006 until the referenced versions are also present. The referenced versions were installed into the gitignored `packages/` folder (`nuget.exe install <id> -Version <ver> -OutputDirectory packages`) as an environment bootstrap action. No tracked file (`.csproj`, `packages.config`, `.claude/rules/*`) was modified. This restores the environment to a buildable state matching the committed csproj expectations.
