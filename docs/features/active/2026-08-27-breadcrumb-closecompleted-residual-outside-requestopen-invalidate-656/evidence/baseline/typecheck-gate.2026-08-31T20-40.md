# Baseline — Type-Check / Nullable Gate (Issue #656)

Timestamp: 2026-09-01T14-38
Task: [P0-T10]

Gate Start: 2026-09-01T14:38:05.7352079-04:00
Gate End:   2026-09-01T14:38:18.2526653-04:00

Command:
```
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults\msbuild\p0-t9-typecheck.log;Verbosity=normal"
```

EXIT_CODE: 0

Command-shape verification (acceptance condition of this task):

- The `Command:` string above contains `/t:Rebuild`.
- The `Command:` string above contains **no** `/p:Nullable=enable`.
- The `Command:` string above contains **no** `/t:Build`.

Both omissions are deliberate and are required by `.claude/rules/csharp.md` and by CI parity. No
project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`,
so `/p:Nullable=enable` is a solution-wide opt-in that conscripts files which never adopted the
pragma and can never pass. `/t:Build` would let MSBuild's up-to-date check skip `CoreCompile` and
exit 0 without running the compiler, making the gate vacuous.

Build summary: `5 Warning(s)` / `0 Error(s)`, elapsed 00:00:12.40. The five warnings are the
pre-existing System.Reactive `packages.config` diagnostic emitted once per affected project; they
are not promoted to errors because they are emitted by an imported targets file rather than by the
compiler.

Nullable coverage of the file under change: `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`
carries `#nullable enable` on line 1, so it participates in nullable analysis and its `CS86xx`
diagnostics are promoted to errors by this gate. The baseline is therefore a genuine per-file
nullable gate for the file this item edits.

Output Summary: Baseline type-check gate passed with `0 Error(s)` under
`/p:TreatWarningsAsErrors=true`. The command carries `/t:Rebuild` and carries neither
`/p:Nullable=enable` nor `/t:Build`.
