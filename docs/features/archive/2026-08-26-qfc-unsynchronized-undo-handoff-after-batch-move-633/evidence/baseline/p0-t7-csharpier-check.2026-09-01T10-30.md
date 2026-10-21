# Baseline CSharpier check (P0-T7)

Timestamp: 2026-09-01T10-30
Task: [P0-T7]
Working directory: WORKTREE

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Verbatim final summary line printed by the command:

```
Checked 1565 files in 4598ms.
```

Count of files reported as unformatted: 0.

Output Summary: The baseline formatting state of this worktree is clean. CSharpier 1.2.6 checked 1565
files and exited 0, and the output carries no `Was not formatted` line, so the unformatted-file count is
0. That count was derived by selecting output lines matching `Was not formatted` rather than from the
summary line, because the summary reports files *checked*, not files rewritten.

Consequence carried forward to P7-T2 and P7-T3: there is no pre-existing formatting drift in this
worktree. The P7-T3 branch that records `REMEDIATION-REQUIRED: pre-existing formatting drift outside
scope` is therefore not expected to be reachable, and any file that `csharpier format .` rewrites in
P7-T2 will have been made unformatted by this change rather than inherited. That makes the P7-T2 set
difference an exact detector of an out-of-scope rewrite.
