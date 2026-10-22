# P9-T7 — Solution analyzer build after the Phase 9 documentation and threading changes

Timestamp: 2026-08-28T01-44
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

`EXIT_CODE: 0`. The acceptance condition P9-T7 states is exactly that, and it is met.

`Build succeeded.` with `5 Warning(s)` and `0 Error(s)`.

## Non-vacuity

| Measure | Value |
|---|---|
| Occurrences of the literal `Skipping target "CoreCompile"` | **0** |
| Raw occurrences of `CoreCompile` in the `/v:normal` log | 101 |
| `csc.exe` invocations | 36 |
| Log lines | 12221 |
| `IItemViewer.cs` appearances in compile item lists | 2 |

`/t:Rebuild` is used, never `/t:Build`. The zero-occurrence count of `Skipping target "CoreCompile"`
is the proof the gate actually compiled: MSBuild's incremental up-to-date check does not invalidate
on a command-line `/p:` change, so a warm `/t:Build` would return exit 0 with `CoreCompile` skipped on
every project and would run no analyzers. A `csc.exe` count alone is not an acceptable substitute
because it is zero even on a real compile in some configurations; both are recorded here, and the
`Skipping target "CoreCompile"` count is the load-bearing one.

Twenty-seven `Skipping target` lines appear in the log for other targets (resource generation and
copy targets whose inputs were genuinely unchanged); none of them is `CoreCompile`.

## Diagnostics

A filter for `: (warning|error) [A-Z]+[0-9]+` over the full log returns **zero** lines. There is no
`CS`, `CA`, `IDE`, `MA` or `RCS` diagnostic anywhere in the build. In particular:

- The XML documentation P9-T3 and P9-T4 added to `QuickFiler/Viewers/IItemViewer.cs` produces no
  `CS1574` (dangling `cref`) and no `CS1591`. The two `<see cref="..."/>` elements written by P9-T4
  name `SetConversationItems` and `SortConversationByDate`, both members of the same interface, and
  both resolve.
- The P9-T2 rewrite of `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:79` produces no unused-`using`
  diagnostic. Removing `new Action(...)` does not orphan `using System;` in that file: `EventHandler`
  is still used at `:51` and `:67`.

All five warnings are the pre-existing `System.Reactive` `packages.config` advisory raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`, one each
for `QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS` and `UtilitiesCS.Test`. That is
character-for-character the P0-T11 analyzer baseline (`5 Warning(s)`, `0 Error(s)`) and the P8-T9
figure, so Phase 9 introduces no new diagnostic of any kind.

## Platform spelling

This is the solution-level command, so the spaced platform spelling `"/p:Platform=Any CPU"` is the
correct one and is used verbatim as the plan prints it. The single-project spelling defect recorded
at P7-T4 and P7-T8 — where `"/p:Platform=Any CPU"` on a bare `.csproj` fails at
`Microsoft.Common.CurrentVersion.targets(843,5)` with an unset `BaseOutputPath` and compiles nothing
— does not apply to a `.sln` invocation. No corrected variant was needed and none was run.

## Log handling

The `/v:normal` output was captured to a file in the system temp directory outside the repository, not
under `FEATURE/evidence/`. The counts above are readings from that capture. No `.log` file was written
under the evidence tree; `.gitignore:84` is `*.log`, so such an artifact could never be committed.
Absolute paths appear in the raw capture only and are not reproduced here.

Output Summary: The solution analyzer build **passes**. `EXIT_CODE: 0`, `Build succeeded.`,
`5 Warning(s)`, `0 Error(s)`, identical to the P0-T11 baseline and the P8-T9 reading. The gate is
non-vacuous: **0** occurrences of `Skipping target "CoreCompile"` against 101 raw `CoreCompile`
occurrences and 36 `csc.exe` invocations over a 12221-line `/v:normal` log. Zero `CS`, `CA`, `IDE`,
`MA` or `RCS` diagnostics, confirming that the XML documentation added to `IItemViewer.cs` emits no
`CS1574` and that the `ItemViewer.FolderSearch.cs` rewrite orphans no `using` directive.
