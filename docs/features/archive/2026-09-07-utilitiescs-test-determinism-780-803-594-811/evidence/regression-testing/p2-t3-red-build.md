# P2-T3 — Build the test project to produce the assembly

Timestamp: 2026-09-08T09-43
Task: [P2-T3]
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:q "/flp:LogFile=coverage/msbuild-p2-t3.log;Verbosity=normal"
EXIT_CODE: 0

This is a project-file build used only to produce a test assembly for the scoped P2-T4 run. It is
not a gate, so it uses `/p:Platform=AnyCPU` without the space, which is the project-level spelling;
the solution-level alias `"/p:Platform=Any CPU"` is used by the two solution gates.

## Final observations

| Observation | Value |
|---|---|
| Build exit code | `0` |
| Count of lines in the file log exactly equal to `    0 Error(s)` | `1` |
| Warning count | `0` |
| `Test-Path UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` | `True` |
| Assembly `LastWriteTime` | `2026-09-08T09:43:46` |
| Task start | `2026-09-08T09:43:35` |
| Assembly is newer than the task start | `True` |

Summary lines quoted verbatim from `coverage/msbuild-p2-t3.log`:

```
    0 Warning(s)
    0 Error(s)
```

The `LastWriteTime` comparison is the non-vacuity control: it proves the run produced a fresh
assembly containing the new test class rather than leaving an earlier build in place.

## Two compile errors were fixed inside this task

The first two invocations of this task's command failed. Both are mechanical compile fixes to the
file P2-T1 created, made within this task because the task's stated outcome is a building
assembly. Neither changes the test's behaviour, its arrangement, or any P2-T1 acceptance count,
and both were re-verified against the full P2-T1 count set afterwards.

1. **`CS0104` on `Action` (2 occurrences, exit 1).** `'Action' is an ambiguous reference between
   'Microsoft.Office.Interop.Outlook.Action' and 'System.Action'`. The file carries
   `using Microsoft.Office.Interop.Outlook;`, which introduces an Outlook `Action` COM type.
   Fixed by qualifying the two gate-delegate parameters of the private `BuildExplorer` helper as
   `System.Action`, which is the same spelling
   `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` already uses for its own gate
   delegate field.
2. **`CS0246` on `SilentProgressTracker` (1 occurrence, exit 1).**
   `SilentProgressTracker` is a `private sealed` nested class of `DfDeedle_COM_Tests`, so it is
   not visible from another class. The plan's P2-T1 text specifies a `ProgressTracker` over
   `SilentProgressTracker` as part of the builders replicated from `DfDeedle_COM_Tests`; the
   replication was completed by adding the same nested class to `DfDeedleEtlTimeoutTests`, with
   the three no-op `Report` overrides unchanged.

Post-fix re-verification of every P2-T1 acceptance count: `[TestMethod]`=2,
`ArmingBarrierTimeProvider`=2, `Advance(250)`=1, `ThrowAsync<InvalidOperationException>`=1,
`Thread.Sleep`=0, `Task.Delay`=0, `CancelAfter`=0, `WaitOne`=0, timed-wait regex=0, line count
209 (cap 300). All pass.

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS
- The log contains `    0 Error(s)`. PASS
- `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` exists and its `LastWriteTime` is later than
  the task start. PASS

## Output Summary

Test project builds clean after two mechanical compile fixes (Outlook `Action` ambiguity, and a
private nested `SilentProgressTracker` that had to be replicated rather than referenced). Fresh
assembly produced at 09:43:46, 11 seconds after the task started.
