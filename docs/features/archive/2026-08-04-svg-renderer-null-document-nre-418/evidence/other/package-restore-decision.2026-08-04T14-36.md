# Package Restore Decision — SVGControl.Test (Issue #418, task P1-T3)

Timestamp: 2026-08-04T18-05

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`

EXIT_CODE: 0

Output Summary: Restore succeeded. `Installed: 7 package(s) to packages.config projects` — the seven pins declared in `SVGControl.Test/packages.config` were downloaded from `https://api.nuget.org/v3/index.json` and expanded under `packages/`. `0 Error(s)`, `1 Warning(s)` (pre-existing `NU1902` for `AngleSharp 1.4.0` referenced by `UtilitiesCS/UtilitiesCS.csproj`, unrelated to #418 and present in the Phase 0 baseline restore). The Phase 0 baseline recorded these seven packages as absent because `SVGControl.Test` was not a member of `TaskMaster.sln`; task P1-T1 added the project entry, so `msbuild /t:Restore /p:RestorePackagesConfig=true` now walks `SVGControl.Test/packages.config` and the pinned versions resolve without substitution.

Route: restored pinned versions

The authorized retarget contingency was **not** exercised. No entry in `SVGControl.Test/packages.config` was changed, and no `<Reference>` `Version=`/`<HintPath>`, `<Import>`, or `<Error>` path in `SVGControl.Test/SVGControl.Test.csproj` was retargeted.

## Per-path resolution table

Every `..\packages\`-rooted path appearing in `SVGControl.Test/SVGControl.Test.csproj`, verified on disk after restore:

| # | Path in `SVGControl.Test.csproj` | Source line(s) | resolves |
|---|---|---|---|
| 1 | `..\packages\MSTest.TestAdapter.3.1.1\build\net462\MSTest.TestAdapter.props` | 8, 9, 163-164 | true |
| 2 | `..\packages\MSTest.TestAdapter.3.1.1\build\net462\MSTest.TestAdapter.targets` | 167-168, 172-173 | true |
| 3 | `..\packages\Castle.Core.5.1.1\lib\net462\Castle.Core.dll` | 123 | true |
| 4 | `..\packages\FluentAssertions.6.12.0\lib\net47\FluentAssertions.dll` | 126 | true |
| 5 | `..\packages\MSTest.TestFramework.3.1.1\lib\net462\Microsoft.VisualStudio.TestPlatform.TestFramework.dll` | 129 | true |
| 6 | `..\packages\MSTest.TestFramework.3.1.1\lib\net462\Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll` | 132 | true |
| 7 | `..\packages\Moq.4.20.69\lib\net462\Moq.dll` | 135 | true |
| 8 | `..\packages\System.Runtime.CompilerServices.Unsafe.6.0.0\lib\net461\System.Runtime.CompilerServices.Unsafe.dll` | 144 | true |
| 9 | `..\packages\System.Threading.Tasks.Extensions.4.5.4\lib\net461\System.Threading.Tasks.Extensions.dll` | 147 | true |

All nine paths report `resolves: true`. No substitutions were made, so there is no substituted-version list to record.

## Verified on-disk package folders

- `packages/Castle.Core.5.1.1`
- `packages/FluentAssertions.6.12.0`
- `packages/Moq.4.20.69`
- `packages/MSTest.TestAdapter.3.1.1`
- `packages/MSTest.TestFramework.3.1.1`
- `packages/System.Runtime.CompilerServices.Unsafe.6.0.0`
- `packages/System.Threading.Tasks.Extensions.4.5.4`

`packages/Svg.3.4.7` (required by task P1-T4) was already present on disk from the `SVGControl` project's own restore; its `lib/net481/` folder exists.

Contributes to AC-9.
