# Baseline — Analyzer Gate (Issue #656)

Timestamp: 2026-09-01T14-37
Task: [P0-T9]

Gate Start: 2026-09-01T14:37:04.6422606-04:00
Gate End:   2026-09-01T14:37:18.6623695-04:00

Command:
```
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\msbuild\p0-t8-analyzer.log;Verbosity=normal"
```

Resolved msbuild: Visual Studio 18 Community, `MSBuild\Current\Bin\MSBuild.exe`.

EXIT_CODE: 0

Build summary: `5 Warning(s)` / `0 Error(s)`, elapsed 00:00:13.90.

## Baseline Warning Codes For BreadcrumbDropDownOpenCoordinator.cs:

none

Derivation:
```
Select-String -Path TestResults\msbuild\p0-t8-analyzer.log -SimpleMatch 'BreadcrumbDropDownOpenCoordinator.cs' | Select-String -SimpleMatch 'warning'
```
returned a match count of **0**. The pre-change file therefore carries no analyzer or compiler
warning attributed to it, so the post-change subset condition asserted by P4-T5 reduces to requiring
zero warnings for that file after the change as well.

## Baseline solution warnings (context, not attributed to the file under change)

All five baseline warnings are the same diagnostic emitted once per affected project by
`System.Reactive.PackagesConfigCheck.targets`: the project contains a `packages.config` file, which
is unsupported by System.Reactive v7.0 or later. Affected projects: `ToDoModel`, `QuickFiler`,
`TaskMaster`, `UtilitiesCS.Test`, and one further project reported in the same form. This is a
pre-existing repository-wide condition unrelated to this item and outside its authorized footprint.

## Non-vacuity of this baseline

`Select-String -SimpleMatch 'Skipping target "CoreCompile"'` over the log returned **0**, so no
project skipped compilation and the analyzers actually ran. `/t:Rebuild` is mandatory for this
reason: MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so a warm
`/t:Build` exits 0 with `CoreCompile` skipped on every project and runs no analyzers.
`/p:Nullable=enable` was not passed, in line with `.claude/rules/csharp.md` and CI.

Output Summary: Baseline analyzer gate passed with `0 Error(s)` and 5 pre-existing System.Reactive
`packages.config` warnings, none attributed to `BreadcrumbDropDownOpenCoordinator.cs`. The baseline
warning-code set for the file under change is empty.
