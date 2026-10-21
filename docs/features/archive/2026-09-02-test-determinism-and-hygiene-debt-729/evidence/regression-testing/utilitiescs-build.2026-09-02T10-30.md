# UtilitiesCS.Test build after orphan removal and guard registration (P4-T5)

Timestamp: 2026-09-02T23-28

Command: `& $msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`

EXIT_CODE: 0

Output Summary:

- `5 Warning(s)` and `0 Error(s)`; the build succeeded.
- `Test-Path UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` returns `True`.
- The five warnings are pre-existing and unrelated to this change. They come from the
  `System.Reactive` 7.0.0 `PackagesConfigCheck` target, which warns that the legacy
  `packages.config` project format is unsupported by that package version. No warning names a
  file this plan writes.
- The ten orphan files deleted by P4-T2 were never in the `<Compile>` list (confirmed by P4-T1),
  so their deletion produces no `CS2001` and no csproj edit was required for them. The only csproj
  change in this phase is the P4-T4 `<Compile Include="NoLiveFormInTestAssemblyTests.cs" />`
  registration, which this build compiles.
