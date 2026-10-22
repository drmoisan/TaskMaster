# [P0-T9] Baseline CSharpier check

Timestamp: 2026-09-08T00-25

Command: `dotnet tool run csharpier check .` (run after the SDK preamble)

EXIT_CODE: 0

BASELINE_CHECKED_FILES: 1608

Output Summary: the tool printed its count line and reported no unformatted file.

```
Checked 1608 files in 7446ms.
```

This is a read-only invocation, so its exit code distinguishes a clean tree from a drifted one on its own. [P5-T2] derives its expected value from the `BASELINE_CHECKED_FILES:` line above rather than from any figure tabled in the plan.
