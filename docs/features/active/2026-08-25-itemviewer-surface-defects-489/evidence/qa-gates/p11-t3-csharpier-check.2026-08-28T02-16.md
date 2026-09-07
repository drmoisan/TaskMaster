# P11-T3 — CSharpier read-only check across the whole worktree (loop iteration 1)

Timestamp: 2026-08-28T02-16
Command: dotnet tool run csharpier check .
EXIT_CODE: 0
ExpectedExitCode: 0

Loop iteration: **1**.

FinalUnformattedSet:
(empty — `dotnet tool run csharpier check .` reported no unformatted file)

## Complete output

The command produced exactly **one** line of output:

```
Checked 1547 files in 7590ms.
```

No path was listed. CSharpier lists one line per unformatted file before its summary line; a
single-line output is therefore a complete record that the reported set is empty.

## Acceptance

**`FinalUnformattedSet:` is empty.** The acceptance condition is that it is either empty or a subset
of `BaselineUnformattedSet:` from P0-T9, containing no file absent from that baseline set. The empty
set is a subset of every set, and it contains no file at all, so both clauses hold trivially and
neither can be violated by an empty result.

For the record, `BaselineUnformattedSet:` in
`evidence/baseline/phase0-csharpier-check.2026-08-27T23-21.md` is also empty, over
`Checked 1543 files`. The final run checks **1547** files, four more than the baseline: exactly the
four new test files this feature adds —
`QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs`,
`QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs`,
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` and
`QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs`.
All four are in the checked set and none is reported unformatted.

**Exit code.** The observed exit code is `0`. `dotnet tool run csharpier check .` exits non-zero
whenever any file is unformatted; the reported set is empty, so `ExpectedExitCode: 0` is declared per
the task's branch rule and the artifact normalizes to `pass`. The gate is the recorded set, never the
exit code.

## Loop consequence

`check` is read-only and rewrote nothing; the stage passed. No restart is triggered. The loop
proceeds to P11-T4.

Output Summary: The read-only format check **passes** at loop iteration 1. `EXIT_CODE: 0` with the
single output line `Checked 1547 files in 7590ms.` and no path listed, so `FinalUnformattedSet:` is
**empty** — trivially a subset of the equally empty `BaselineUnformattedSet:` from P0-T9, and
containing no file absent from it. The checked count rose from 1543 at baseline to 1547, accounted
for exactly by this feature's four new test files, all of which are formatted. `ExpectedExitCode: 0`
is declared because the reported set is empty.
