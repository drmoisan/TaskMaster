# Phase 2 — CSharpier Check Stage (AC8)

Timestamp: 2026-09-13T15-30
Task: [P2-T2]

Command: dotnet tool run csharpier check .
EXIT_CODE: 0

CheckedFiles: 1625
UnformattedFileList: none

Output Summary: the run printed the single summary line `Checked 1625 files in 5142ms.`, printed no
per-file not-formatted line at all, and exited 0. AC8 demands that the check subcommand report no
unformatted file, and the absence of any per-file line beside that summary is the observation beyond
the exit code. The checked count is 1625, which is greater than one, so this is not a vacuous run that
visited no file and exited 0 for that reason.

## Relation To The P0-T5 Baseline

The P0-T5 baseline recorded `CheckedFiles: 1627` with `UnformattedFileList: none` and `EXIT_CODE: 0`.
The count falls by exactly two, which reconciles with the two C# source files that Defect C deletes:
`UtilitiesCS/Threading/ProgressTrackerAsync.cs` and
`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`. No file was added to the tree by this
delivery, so no offsetting increase is expected. The base tree carried no formatter drift, so this
gate is not masking pre-existing drift that the P2-T1 format stage repaired.

## Verification Method

The check subcommand is read-only: it reports drift and does not repair it, which is why it and not
the format subcommand is the gate for AC8. Had any file been unformatted, the tool would have printed
a per-file line naming it ahead of the summary line and exited non-zero. Neither occurred.

The build lock was acquired immediately before this invocation and released immediately after it
returned.
