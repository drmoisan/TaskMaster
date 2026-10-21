# P2-T2 — Format the Seam Change

Timestamp: 2026-08-31T19-22
Command: dotnet tool run csharpier format "UtilitiesCS/To Depricate/FileIO2.cs"
Command: dotnet tool run csharpier check .
EXIT_CODE: 0
ExpectedExitCode: 0

The recorded `EXIT_CODE:` is that of the read-only `check` command, which is the governing terminating observation for this format step. The `format` command also exited 0, but its exit code observes nothing: it exits 0 whether or not it rewrote a file.

## Supporting evidence: SHA-256 before and after

`UtilitiesCS/To Depricate/FileIO2.cs`

- BEFORE: 4234ED66880C32DAE0D55E4854AA0B78E563B61E73C94263D382DC57A4BF2602
- AFTER: 30A2207BEEC404BF566F97D2E34ABD30C1CC055AD7A156E3BCB40F507127EADA
- REWRITTEN: True

REWRITTEN_FILE_COUNT: 1. This is the number of target files whose `Get-FileHash -Algorithm SHA256` value differs between the capture taken immediately before the invocation and the capture taken immediately after. It is supporting evidence only and is not the gate.

The console line the `format` command printed reads `Formatted 1 files in 743ms.` That is the count of files **processed**, not rewritten, and is recorded here only to note that it must not be read as the rewrite count. In this instance the two figures coincide at 1 because exactly one path was passed to the command; that coincidence is not a general property and no gate is asserted over it.

The one rewrite the formatter applied: the `delayAsync` declaration, which was hand-written across two lines, was collapsed onto a single line 83. The token `Func<int, CancellationToken, Task> delayAsync =` remains present on that line, so the P2-T1 acceptance conditions are unaffected by the reflow.

## Read-only verification

`dotnet tool run csharpier check .` transcribed final summary line:

```
Checked 1565 files in 4418ms.
```

CHECK_EXIT_CODE: 0. The repository is formatter-clean over the whole CSharpier target set.

CARRIED_BASELINE_FORMAT_DRIFT: not applicable. P0-T12 recorded `PRE_EXISTING_FORMAT_DRIFT: none`, so no carried-drift branch is available to this task and the `check` exit code of 0 is the only outcome that satisfies its acceptance. It is the observed outcome.

Post-format line count of the changed file: 257, within the 500-line limit.

Output Summary: The seam change was formatted, the formatter rewrote the one target file, and the read-only repository-wide check exited 0.
