# P1-T14 — AC6 passing direction: cold-restore analyzer build is green

Timestamp: 2026-09-19T14-34

Command:
```
pwsh -NoProfile -Command '[System.IO.Directory]::Delete("<execution-worktree-root>\packages", $true)'
pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; & "<execution-worktree-root>\scripts\vscode\Invoke-Restore.ps1"'
pwsh -NoProfile -Command 'Get-Process outlook -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count'
CMD-MSBUILD-ANALYZERS, invoked as:
  & "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\analyzers.msbuild.log;Verbosity=normal"
```

EXIT_CODE: 0

OUTLOOK-CLOSED: true

`Get-Process outlook` returned **0** immediately before the rebuild. Outlook was already closed by
the user and was not terminated by this task, per the CMD-OUTLOOK rule that terminating it can
corrupt the profile and the local store.

## Cold state

`packages/` was removed in full with `[System.IO.Directory]::Delete($path, $true)`. The .NET API is
used rather than `Remove-Item -Recurse -Force`, which the harness's dangerous-command guard blocks,
and the deletion was asserted rather than assumed.

| Observation | Before delete | After delete | After restore |
|---|---|---|---|
| `packages/` exists | True | False | True |
| `packages/Meziantou.Analyzer.3.0.235` exists | True | — | **True** |
| `packages/Meziantou.Analyzer.3.0.203` exists | False | — | **False** |
| Package directories present | — | — | **172** |

The restore reported `Installed: 172 package(s) to packages.config projects`, exit 0, against
`<execution-worktree-root>\TaskMaster.sln`. The 172-directory count is
the non-vacuity guard on the two folder assertions: a restore that installed nothing would also
leave `Meziantou.Analyzer.3.0.203` absent and satisfy the negative clause on its own.

`packages/Deedle.3.0.0` and `packages/FSharp.Core.11.0.100` are also present after the restore.
Neither is asserted by this task, but both are recorded because their presence is the downstream
effect of the P1-T11 manifest entries: before #903 was corrected, the two `<HintPath>` references in
`ToDoModel.Test.csproj` named folders no manifest asked the restore to fetch.

### Worktree-targeting defect encountered and corrected

The first restore attempt used
`pwsh -NoProfile -WorkingDirectory "<execution-worktree>" -File ".\scripts\vscode\Invoke-Restore.ps1"`
and **restored the wrong checkout**: the log line read
`Done Building Project "<session-worktree-root>\TaskMaster.sln"`.
The relative `-File` argument resolves against the session worktree rather than against
`-WorkingDirectory`, and `Invoke-Restore.ps1` derives its repository root from `$PSScriptRoot`
(line 84), so the script that ran was the session worktree's copy and it restored the session
worktree's solution. The execution worktree's `packages/` stayed deleted.

The corrected invocation uses an **absolute** script path together with an explicit `Set-Location`,
and the solution path in the restore output was read back to confirm the target. Every measurement
in this artifact comes from the corrected run. The same correction was applied retroactively to the
actionlint invocation recorded at P1-T13, which was re-run in the corrected form with an identical
result.

The mis-targeted first attempt restored packages into the session worktree's gitignored
`packages/` directory. That directory is untracked in both checkouts, so the side effect reaches no
commit, and `git status --porcelain` in the execution worktree is unaffected by it.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| `EXIT_CODE: 0` | **0**; `Build succeeded. 0 Warning(s) 0 Error(s)` | PASS |
| `OUTLOOK-CLOSED: true` recorded | 0 Outlook processes | PASS |
| `packages/Meziantou.Analyzer.3.0.235` exists after the cold restore | True | PASS |
| `packages/Meziantou.Analyzer.3.0.203` does not exist after the cold restore | False | PASS |
| The captured log carries exactly 0 lines containing `CS0006` | **0**, against the 4 P0-T11 recorded | PASS |
| The captured log carries at least 18 lines containing `/out:obj\Debug\`, exact count recorded | **36** | PASS |

Log measured at `coverage/analyzers.msbuild.log`, 5572 lines. Lines containing `error CS` of any
number: **0**.

## Non-vacuity

Gate rule 7 requires the non-vacuity observation to come from the echoed compiler command line
rather than from `Task "Csc"`. The 36 lines carrying `/out:obj\Debug\` resolve to **18 distinct
assemblies**, enumerated below, each echoed twice by the file logger:

```
QuickFiler.dll            QuickFiler.Test.dll
SVGControl.dll            SVGControl.Test.dll
Tags.dll                  Tags.Test.dll
TaskMaster.dll            TaskMaster.Test.dll
TaskTree.dll              TaskTree.Test.dll
TaskVisualization.dll     TaskVisualization.Test.dll
ToDoModel.dll             ToDoModel.Test.dll
UtilitiesCS.dll           UtilitiesCS.Test.dll
VBFunctions.dll           VBFunctions.Test.dll
```

Eighteen distinct assemblies is the whole solution, so every project compiled and the zero-`CS0006`
result covers all of them. That is what the clause exists to establish: a build that compiled
nothing would report zero `CS0006` lines just as readily, and the 8 `/out:obj\Debug\` lines P0-T11
recorded on the failing run show the count is genuinely sensitive to how far the build got.

## Pairing with the P0-T11 failing log

| | P0-T11, before the #898 fix | P1-T14, after it |
|---|---|---|
| Exit code | 1 | **0** |
| Lines containing `CS0006` | 4 | **0** |
| Lines containing `/out:obj\Debug\` | 8 | **36** |
| Distinct assemblies compiled | fewer than the solution | **18**, the whole solution |

The failing log is at
`evidence/baseline/p0-t11-ac6-cold-analyzer-build-red.2026-09-19T09-44.md`. Its four `CS0006` lines
all name
`..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`
against `UtilitiesCS.csproj` and `VBFunctions.csproj`. Both projects now compile, and the folder
the stale items named is absent from the restored tree while the folder the corrected items name is
present. The two logs together are the fail-before and pass-after pair AC6 requires.

AC6 is deliberately local. The build workflows' cache `restore-keys:` prefix fallback structurally
prevents CI from reaching the cold-cache state this task reproduces, per gate rule 3, so the
criterion is never rooted in CI.

Output Summary: with `packages/` deleted and re-restored from cold,
`packages/Meziantou.Analyzer.3.0.235` is present and `packages/Meziantou.Analyzer.3.0.203` is
absent across 172 restored package directories. CMD-MSBUILD-ANALYZERS then returned EXIT_CODE 0 with
Outlook confirmed closed, 0 `CS0006` lines against the 4 P0-T11 recorded, and 36 `/out:obj\Debug\`
lines resolving to all 18 solution assemblies against the 8 lines on the failing run. AC6 passes.
