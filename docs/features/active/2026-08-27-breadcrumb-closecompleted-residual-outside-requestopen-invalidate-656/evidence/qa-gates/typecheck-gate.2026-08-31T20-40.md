# QA Gate — Type-Check / Nullable Gate (Issue #656)

Timestamp: 2026-09-01T14-51
Task: [P4-T6] (toolchain loop pass 1, step 3)
Satisfies: AC-17

Gate Start: 2026-09-01T14:50:46.1887917-04:00

Command:
```
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults\msbuild\p4-t6-typecheck.log;Verbosity=normal"
```

EXIT_CODE: 0

## Acceptance measurements

| Condition | Required | Observed | Met |
|---|---|---|---|
| Exit code | 0 | 0 | yes |
| `@(Select-String -Path TestResults\msbuild\p4-t6-typecheck.log -SimpleMatch '0 Error(s)').Count` | > 0 | 1 | yes |
| `Command:` contains `/t:Rebuild` | yes | yes | yes |
| `Command:` contains `/p:Nullable=enable` | no | no | yes |
| `Command:` contains `/t:Build` | no | no | yes |

Elapsed 00:00:11.79.

## Why the two omissions are load-bearing

**No `/p:Nullable=enable`.** Nullable enforcement in this repository is per-file opt-in: a file
participates when it carries a `#nullable enable` directive, and `/p:TreatWarningsAsErrors=true`
then promotes its `CS86xx` diagnostics to build errors. No project carries a `<Nullable>` element
and there is no `Directory.Build.props`, so `/p:Nullable=enable` would be a solution-wide opt-in
conscripting every file that has never adopted the pragma. CI omits it deliberately, and this
command is character-for-character CI's nullable step.

**No `/t:Build`.** MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so
a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every project: the gate could not
fail. The log confirms this did not happen here —
`@(Select-String -Path TestResults\msbuild\p4-t6-typecheck.log -SimpleMatch 'Skipping target "CoreCompile"').Count`
is **0**, so every project was genuinely recompiled under warnings-as-errors.

## Nullable coverage of the changed file

`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` carries `#nullable enable` on line 1, so
it is inside the per-file nullable gate and its `CS86xx` diagnostics are promoted to errors by this
command. The statement this change added, `bool hostOpen = _host.IsOpen;`, declares a non-nullable
`bool` from a non-nullable `bool` property and introduces no null state, and the narrowed guard
`if (_closeCompleted && !hostOpen)` reads two non-nullable `bool` values. The clean result is
therefore a genuine observation about the changed lines rather than a vacuous pass over an unopted
file.

Output Summary: Type-check gate passed with `0 Error(s)` under `/p:TreatWarningsAsErrors=true` and
`/t:Rebuild`, with no `CoreCompile` skipped. The command carries neither `/p:Nullable=enable` nor
`/t:Build`. AC-17 is satisfied.
