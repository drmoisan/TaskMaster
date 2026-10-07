# P8-T4 Repository-Wide Format on the Merged Tree

Timestamp: 2026-10-06T17-56
Command: dotnet tool run csharpier format . (CMD-FORMAT-REPO, TASKID p8-t4; run as pwsh -NoProfile -Command with Set-Location to the item worktree, with SHA-256 hashes of the twelve C# Write Set files and git status --porcelain --untracked-files=all taken before and after)
EXIT_CODE: 0 (the printed FORMAT_EXIT_CODE, the only invocation)
ITERATION: 1
Output Summary: on the merged tree (merge commit 9163994569e24c5c539a285724f9c8f9f6fd8a0e) the formatter exited 0 and changed no C# Write Set file and no other tracked or untracked file (porcelain empty before and after); the files main brought are formatter-clean; no loop restart is triggered.

- FORMAT_EXIT_CODE: 0
- WRITESET-CHANGED lines: none
- WRITESET-CHANGED-COUNT: 0
- PORCELAIN-BEFORE-COUNT: 0
- PORCELAIN-AFTER-COUNT: 0
- PORCELAIN-SAME: True
- Formatter summary line (observation): Formatted 1645 files in 3072ms.

## Acceptance (P8-T4, all three required)

1. FORMAT_EXIT_CODE: 0: met.
2. WRITESET-CHANGED-COUNT: 0: met.
3. PORCELAIN-SAME: True: met.
