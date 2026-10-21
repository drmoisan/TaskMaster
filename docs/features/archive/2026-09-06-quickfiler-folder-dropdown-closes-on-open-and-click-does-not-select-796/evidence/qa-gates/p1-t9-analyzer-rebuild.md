# P1-T9 — Analyzer gate over TaskMaster.sln with /t:Rebuild

Timestamp: 2026-09-07T14-23
Task: [P1-T9]
Issue: #796
Channel used: A

RunStartedUtc: 2026-09-07T14:22:44.9396533Z

Command: the P0-T8 command form with the log path
`TestResults\796\p1-t9\analyzer-rebuild.log`:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\796\p1-t9\analyzer-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

## Build summary, verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:23.41
```

## Comparison against the P0-T8 baseline

| Total | P0-T8 baseline | P1-T9 | Verdict |
|---|---|---|---|
| Analyzer warnings | 0 | 0 | no greater than baseline |
| Analyzer errors | 0 | 0 | no greater than baseline |

Baseline totals read from evidence/baseline/p0-t8-analyzer-rebuild-baseline.md.

## Compiler-invocation counts read back from the detailed log

Raw log (gitignored): TestResults/796/p1-t9/analyzer-rebuild.log

CscTaskCount=36
CscToolCount=36

Both greater than zero, so `CoreCompile` ran and the analyzers ran with it.

## Assembly-freshness corroboration

| Assembly | LastWriteTimeUtc | At or later than RunStartedUtc |
|---|---|---|
| QuickFiler/bin/Debug/QuickFiler.dll | 2026-09-07T14:22:56.5865620Z | yes |
| QuickFiler.Test/bin/Debug/QuickFiler.Test.dll | 2026-09-07T14:23:01.5358378Z | yes |

## What this run additionally establishes

This is the run that proves the Phase 1 source compiles. It is the evidence cited by
the P1-T4 acceptance condition ("the solution compiles under the P0-T8 command form",
which is what proves both the cast and the forward are well typed) and by the P1-T5
acceptance condition of the same wording. In particular it establishes that:

- the new partial part `QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs`
  compiles into the same `sealed partial class BreadcrumbDropDownHost` and reaches
  the private fields `_disposed`, `_programmaticClose` and `_openLifetime`, the
  internal `OpenState` property, and `DropDown.AutoClose`;
- the cast `(group.ItemController as QfcItemController)?.IsBreadcrumbSelectorOpen` is
  well typed and yields `bool?`;
- the new internal member `QfcItemController.IsBreadcrumbSelectorOpen` forwards to
  `IItemViewer.IsFolderDropDownOpen` without a type error;
- the new test class compiles against the two internal static formatters through the
  `InternalsVisibleTo("QuickFiler.Test")` grant.

Output Summary: EXIT_CODE 0 with 0 warnings and 0 errors, equal to the P0-T8
baseline; 36 Csc task and 36 csc.exe tool invocations; both touched assemblies
rebuilt after RunStartedUtc.
