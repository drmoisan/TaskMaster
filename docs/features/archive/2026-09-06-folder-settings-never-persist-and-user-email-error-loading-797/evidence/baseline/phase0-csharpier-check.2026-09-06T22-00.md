# Phase 0 — CSharpier Formatting Baseline (Issue #797)

Timestamp: 2026-09-07T09-16

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Run from the repository root of this worktree, through `dotnet tool run` so the manifest-pinned
CSharpier 1.2.6 is used rather than any global installation.

## Observed output

```text
Checked 1601 files in 6988ms.
```

The `Checked` summary line reports 1601 files. The run reported no file as unformatted, and the
read-only check subcommand exited 0, which is a real signal rather than a write-mode exit code.

PRE-EXISTING-FORMAT-DRIFT: NONE

Output Summary: The formatting baseline is clean. 1601 files checked, zero unformatted files, exit
code 0. Phase 5 consumes this determination: because the baseline recorded no drift, every path
appearing in the P5-T1 before-and-after difference must be a Write Set path, and P5-T2 must exit 0.
