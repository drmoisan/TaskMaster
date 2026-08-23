# Baseline — `SVGControl.Test` Buildability (Issue #418)

Task: `[P0-T10]`
Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`

Timestamp: 2026-08-04T15-06

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath SVGControl.Test/SVGControl.Test.csproj -Configuration Debug -Platform AnyCPU`

Working directory: repository root (`c:\Users\DanMoisan\source\repos\drmoisan\TaskMaster`)

EXIT_CODE: 1

Output Summary: **Build FAILED** with `1 Error(s)` and `0 Warning(s)` in `00:00:00.23`. The
build stopped in the `EnsureNuGetPackageBuildImports` target before compilation, with the
verbatim error recorded below. `SVGControl.Test present in TaskMaster.sln: false` — a search
for the string `SVGControl.Test` in `TaskMaster.sln` returns **0** matches, while the
production project `SVGControl` is present at line 40. All **seven** pinned packages named by
`SVGControl.Test/packages.config` are **absent** from `packages/`; each pinned id has a
different version on disk. No `SVGControl.Test/bin` directory exists. This artifact records
the repository's real broken state as observed. Nothing was repaired.

## Verbatim `EnsureNuGetPackageBuildImports` Error

```text
C:\Users\DanMoisan\source\repos\drmoisan\TaskMaster\SVGControl.Test\SVGControl.Test.csproj(162,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\MSTest.TestAdapter.3.1.1\build\net462\MSTest.TestAdapter.props.
```

MSBuild attributed the error to the `EnsureNuGetPackageBuildImports` target:

```text
"...\SVGControl.Test\SVGControl.Test.csproj" (Build target) (1) ->
(EnsureNuGetPackageBuildImports target) ->
  ...SVGControl.Test.csproj(162,5): error : This project references NuGet package(s) that are
  missing on this computer. ... The missing file is
  ..\packages\MSTest.TestAdapter.3.1.1\build\net462\MSTest.TestAdapter.props.

Build FAILED.

  0 Warning(s)
  1 Error(s)

Time Elapsed 00:00:00.23
```

The only artifact produced was `obj\Debug\` (created by `_CleanRecordFileWrites`). No
compilation occurred.

## (a) Solution Membership

`SVGControl.Test present in TaskMaster.sln: false`

| Check | Command | Result |
| --- | --- | --- |
| Occurrences of `SVGControl.Test` in `TaskMaster.sln` | `grep -c "SVGControl.Test" TaskMaster.sln` | `0` |
| `SVGControl` entries in `TaskMaster.sln` | `grep -n "SVGControl" TaskMaster.sln` | one match, line 40 |

The single match is the production project only:

```text
40:Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "SVGControl", "SVGControl\SVGControl.csproj", "{D0FEE0D9-901A-4FB8-97D1-96A8F634B83C}"
```

Because `SVGControl.Test` is not a solution member, it is not built by
`msbuild TaskMaster.sln`, is not covered by the analyzer gate (`[P0-T7]`) or the nullable gate
(`[P0-T8]`), and its `packages.config` was not part of the `[P0-T5]` restore graph.

## (b) Pinned Package Presence Under `packages/`

All seven pinned packages are absent. Checked with a per-directory `test -d`.

| # | Pinned package directory | Present? | Version(s) actually on disk for that id |
| --- | --- | --- | --- |
| 1 | `packages/Castle.Core.5.1.1` | **ABSENT** | `Castle.Core.5.2.1` |
| 2 | `packages/FluentAssertions.6.12.0` | **ABSENT** | `FluentAssertions.8.3.0`, `FluentAssertions.8.8.0`, `FluentAssertions.8.9.0` |
| 3 | `packages/Moq.4.20.69` | **ABSENT** | `Moq.4.20.72` |
| 4 | `packages/MSTest.TestAdapter.3.1.1` | **ABSENT** | `MSTest.TestAdapter.3.9.3`, `MSTest.TestAdapter.4.1.0`, `MSTest.TestAdapter.4.2.2` |
| 5 | `packages/MSTest.TestFramework.3.1.1` | **ABSENT** | `MSTest.TestFramework.3.9.3`, `MSTest.TestFramework.4.1.0`, `MSTest.TestFramework.4.2.2` |
| 6 | `packages/System.Runtime.CompilerServices.Unsafe.6.0.0` | **ABSENT** | `System.Runtime.CompilerServices.Unsafe.6.1.2` |
| 7 | `packages/System.Threading.Tasks.Extensions.4.5.4` | **ABSENT** | `System.Threading.Tasks.Extensions.4.6.3` |

Present: **0 of 7**. Absent: **7 of 7**.

### `SVGControl.Test/packages.config` (verbatim, the source of the seven pins)

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Castle.Core" version="5.1.1" targetFramework="net481" />
  <package id="FluentAssertions" version="6.12.0" targetFramework="net481" />
  <package id="Moq" version="4.20.69" targetFramework="net481" />
  <package id="MSTest.TestAdapter" version="3.1.1" targetFramework="net481" />
  <package id="MSTest.TestFramework" version="3.1.1" targetFramework="net481" />
  <package id="System.Runtime.CompilerServices.Unsafe" version="6.0.0" targetFramework="net481" />
  <package id="System.Threading.Tasks.Extensions" version="4.5.4" targetFramework="net481" />
</packages>
```

## Corroborating Script Warnings

`Invoke-VSBuild.ps1` emitted these seven warnings before invoking MSBuild, one per pinned
package, then reported `Sync-PackageReferences: All HintPaths are up to date` — confirming the
research artifact's section 8.3 finding that `Sync-PackageReferences.ps1` cannot repair this
condition, because it rewrites `<HintPath>` values only and does not touch `packages.config`
pins, `<Reference>` `Version=` attributes, or the `EnsureNuGetPackageBuildImports` `<Error>`
guard.

```text
WARNING:   [SVGControl.Test] Cannot resolve Castle.Core.dll from Castle.Core.5.1.1
WARNING:   [SVGControl.Test] Cannot resolve FluentAssertions.dll from FluentAssertions.6.12.0
WARNING:   [SVGControl.Test] Cannot resolve Microsoft.VisualStudio.TestPlatform.TestFramework.dll from MSTest.TestFramework.3.1.1
WARNING:   [SVGControl.Test] Cannot resolve Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll from MSTest.TestFramework.3.1.1
WARNING:   [SVGControl.Test] Cannot resolve Moq.dll from Moq.4.20.69
WARNING:   [SVGControl.Test] Cannot resolve System.Runtime.CompilerServices.Unsafe.dll from System.Runtime.CompilerServices.Unsafe.6.0.0
WARNING:   [SVGControl.Test] Cannot resolve System.Threading.Tasks.Extensions.dll from System.Threading.Tasks.Extensions.4.5.4
Sync-PackageReferences: All HintPaths are up to date
```

## Build Output State

`SVGControl.Test/bin` does not exist (`ls: cannot access 'SVGControl.Test/bin': No such file
or directory`). There is no `SVGControl.Test.dll` anywhere on disk, which is consistent with
the `[P0-T9]` coverage run discovering six test assemblies and not this one.

## Implication for Phase 1

Task `[P1-T3]`'s primary action is a solution-scoped restore. That restore will only reach
`SVGControl.Test/packages.config` after task `[P1-T1]` has added the project to
`TaskMaster.sln`. Even then, the seven pins name versions that no other project in the
repository currently uses; the on-disk versions differ for every one of the seven ids. The
task `[P1-T3]` authorized contingency (retarget the pins to versions verified present under
`packages/`, preferring the versions used by `UtilitiesCS.Test`) is therefore the likely route
rather than the exception. No action is taken here; Phase 0 records state only.
