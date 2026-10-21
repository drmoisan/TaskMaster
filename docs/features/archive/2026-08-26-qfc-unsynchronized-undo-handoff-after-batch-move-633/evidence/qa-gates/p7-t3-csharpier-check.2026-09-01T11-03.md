# CSharpier check (P7-T3)

Timestamp: 2026-09-01T11-03
Task: [P7-T3]
Working directory: WORKTREE

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Verbatim summary line:

```
Checked 1566 files in 4735ms.
```

Count of files reported as unformatted: 0. The output carries no `Was not formatted` line.

Output Summary: The formatting gate passes. This is the read-only, CI-parity form of the check, run
through `dotnet tool run` so the manifest-pinned CSharpier 1.2.6 is used rather than any global install.
Exit code 0 with zero unformatted files means the P7-T2 format run reached a fixed point: re-running the
formatter would change nothing.

The `REMEDIATION-REQUIRED: pre-existing formatting drift outside scope` branch was **not** taken, and
was not reachable. The P0-T7 baseline recorded the same figures — exit 0 and zero unformatted files —
so this worktree carried no pre-existing drift for the carve-out to apply to. The Phase 7 restart rule
therefore applies normally from here on.

## Post-format re-verification of the P2-T3 and P5-T9 acceptance clause

CSharpier rewrapped several lines inside the blast radius, so the `using`-statement clause that P2-T3 and
P5-T9 assert was re-checked against the formatted text rather than assumed to have survived:

| Measure | Value |
|---|---|
| Lines in `QfcFormControllerUndoHandoffTests.cs` containing `BeginTransactionAsync` | 3 |
| Of those, lines that also contain `using (` | **3** |
| Banned wait API matches in that file | 0 |

The three acquisitions are at lines 232, 283, and 339 after formatting, each written as one physical
line. None was broken onto a continuation, which is what would have happened had a
`.ConfigureAwait(false)` been appended, and which would have left `BeginTransactionAsync` on a line
carrying no `using (`. The file is 428 lines, still under 500.
