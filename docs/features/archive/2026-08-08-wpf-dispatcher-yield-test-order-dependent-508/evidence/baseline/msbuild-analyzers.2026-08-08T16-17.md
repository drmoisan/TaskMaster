# Baseline Analyzer Build (toolchain step 2)

Timestamp: 2026-08-08T16-17

Task: [P0-T8]

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /m`

MSBuild resolved to `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
(MSBuild 18.8.2 for .NET Framework), the same binary `Invoke-Restore.ps1` selected at P0-T7.

EXIT_CODE: 0

## Result

```
    6 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.90
```

## Warning breakdown (pre-existing, out of scope)

| Count | Diagnostic | Projects | Assessment |
|---|---|---|---|
| 5 | `System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.` | `QuickFiler.csproj`, `TaskMaster.csproj`, `UtilitiesCS.Test.csproj`, and two further packages.config projects | Pre-existing packaging warning unrelated to this change. Fixing it requires a PackageReference migration, which is out of scope. |
| 1 | `CSC : warning CS2002: Source file '...\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times` | `UtilitiesCS.Test.csproj` | Pre-existing duplicate `<Compile Include>` in the legacy non-SDK test project. Latent and out of scope; fixing it would require a `.csproj` edit, which the plan's scope boundary forbids. |

Zero analyzer-rule diagnostics (no CA/S/MA/RCS/AsyncFixer/RS IDs) were emitted, consistent with the
`.claude/rules/csharp.md` severity-first invariant that new analyzer rules are configured at
`severity = suggestion` (message level, not surfaced as warnings).

## Environment conditions recorded for the like-for-like comparison

The plan's execution note warns that `TaskMaster.Test` and `UtilitiesCS.Test` may fail to build with
four `CS0234` diagnostics in `ThisAddIn.Designer.cs` when the Office Tools v4.0 VSTO runtime is
absent. **That condition did NOT occur in this environment.** The build produced 0 errors and
`UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` exists on disk, so the test assemblies are built
and the coverage denominator at P0-T10 is not deflated by an unbuildable test project.

Output Summary: PASS, EXIT_CODE 0. Solution-wide analyzer build succeeded with 6 warnings and 0
errors in 16.90s. All 6 warnings are pre-existing and out of scope (5x System.Reactive
packages.config packaging warning, 1x CS2002 duplicate Compile item in `UtilitiesCS.Test.csproj`).
No VSTO CS0234 condition; `UtilitiesCS.Test.dll` built successfully. This 6/0 figure is the
comparand for P2-T3.
