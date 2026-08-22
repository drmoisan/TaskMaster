# Baseline — CSharpier Check (read-only)

Timestamp: 2026-08-22T09-22

Command:

```
dotnet tool run csharpier check .
```

Run from the worktree root
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad37a256a0fb60243`. Read-only: the
`check` subcommand reports formatting divergence and writes no file.

EXIT_CODE: 0

Output Summary:

Full command output, verbatim:

```
Checked 1517 files in 5398ms.
```

- **Files reported as unformatted: 0.**
- Files checked: 1,517.
- Exit code recorded verbatim: **0**.

CSharpier emits one line per unformatted file when divergence exists; the output carries no such
line, so the count is zero. The baseline formatting state is clean and there is **no pre-existing
formatting condition** to record or defer. The task's provision for recording a non-zero baseline
exit code as a pre-existing condition rather than repairing it here did not need to be exercised.

Version provenance: the invocation went through `dotnet tool run`, so it used the manifest-pinned
CSharpier `1.2.6` restored by P0-T11 (`dotnet-tools.json` sets `"rollForward": false`). This is the
same version `.github/workflows/ci.yml` runs, so this baseline agrees with CI.
