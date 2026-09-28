# Phase 0 — CSharpier Baseline (read-only check)

Timestamp: 2026-09-13T14-51
Task: [P0-T5]

Command: dotnet tool run csharpier check .
EXIT_CODE: 0

CheckedFiles: 1627
UnformattedFileList: none

Output Summary: the tool printed the single summary line `Checked 1627 files in 5124ms.` and exited 0.
It printed no per-file not-formatted line, so the base tree carries no pre-existing formatter drift.
The checked count is greater than zero, so the run was not vacuous.

## Derivation Of The Two Fields

`CheckedFiles:` is transcribed from the count in the single summary line the tool prints in the form
`Checked ` followed by a count and an elapsed time. `UnformattedFileList:` records the word none
because the tool named no file as not formatted; the value is an enumeration of the tool's per-file
output rather than an inference from the exit code.

The check subcommand is read-only. It reports drift and does not repair it, which is why the baseline
uses it and not the format subcommand: a baseline captured after a write-mode formatter has already
repaired pre-existing drift is not a baseline. The exit code is recorded as observed and is not
asserted to be zero by this task; P0-T14 evaluates it.

## Re-Run Note, Per D15

This artifact overwrites a superseded capture taken before the main branch carrying the fix for issue
#877 was merged into this branch. That merge added a source file under the repository-root TestSupport
directory and changed a UtilitiesCS test source file, so the checked-file population is not the same
population the superseded capture measured, and the count is re-measured rather than carried forward.

## Invocation Note

The command was issued inside a single pwsh invocation whose first statement sets the location to this
worktree root, so the `.` argument resolved to this worktree and not to any sibling. The tool was
invoked through `dotnet tool run`, so the manifest-pinned version 1.2.6 recorded by P0-T3 is the
version that ran. The build lock was held across this single command and released immediately
afterwards.
