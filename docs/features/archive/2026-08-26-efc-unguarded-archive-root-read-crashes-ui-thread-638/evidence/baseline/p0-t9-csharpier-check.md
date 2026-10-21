# [P0-T9] CSharpier baseline check (Issue 638)

Timestamp: 2026-08-29T12-23

Command: `dotnet tool run csharpier check .` (run from the worktree root)

EXIT_CODE: 0

Output Summary:

Final summary line, quoted verbatim:

```
Checked 1560 files in 4829ms.
```

That line's N is the number of files processed, not the number of unformatted files. The
unformatted count is derived from the `Error <path> - Was not formatted.` lines, of which
the run emitted none.

BASELINE_UNFORMATTED_COUNT: 0

BASELINE_UNFORMATTED_FILES: none

Consequence for later phases: [P6-T1] takes the unscoped branch and runs
`dotnet tool run csharpier format .` against `.` as written; [P6-T2]'s acceptance is
`EXIT_CODE: 0`; and [P8-T15] takes the AC13 check-off branch rather than the
REMEDIATION-REQUIRED branch, provided [P6-T2] records `EXIT_CODE: 0`.
