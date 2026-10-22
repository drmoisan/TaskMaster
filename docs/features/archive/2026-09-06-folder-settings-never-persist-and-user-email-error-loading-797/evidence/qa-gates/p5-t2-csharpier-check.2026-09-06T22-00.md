# P5-T2 — CSharpier Check (Issue #797)

Timestamp: 2026-09-07T09-58

Command, run from the repository root of this worktree:

```text
dotnet tool run csharpier check .
```

EXIT_CODE: 0

## Output Summary

`Checked 1605 files in 6628ms.` The `Checked` summary line reports 1605 files, four more than the
1601 recorded at the Phase 0 baseline, which is exactly the four C# files this change creates. The run
reported no unformatted file.

The read-only check subcommand's exit code is a real signal, unlike the write-mode format
subcommand's, so this step rather than P5-T1 decides the formatting gate. The alternative acceptance
branch — a non-zero exit whose reported unformatted paths are a subset of a recorded
`PRE-EXISTING-FORMAT-DRIFT:` set — is not entered, because the P0-T6 artifact recorded
`PRE-EXISTING-FORMAT-DRIFT: NONE` and this run exited 0. No `ExpectedExitCode:` field is carried,
which is equivalent to declaring 0.

Output Summary: Formatting is clean. Exit code 0, 1605 files checked, zero unformatted files.
