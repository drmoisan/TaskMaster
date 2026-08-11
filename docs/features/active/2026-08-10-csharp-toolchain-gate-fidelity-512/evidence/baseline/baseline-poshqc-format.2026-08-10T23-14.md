# Baseline — PoshQC format against the unmodified tree ([P0-T18])

Timestamp: 2026-08-10T23-14
Command: `mcp__drm-copilot__run_poshqc_format` with `workspace_root = "C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb"` and `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]`
EXIT_CODE: 0

## Return payload (verbatim)

```json
{
  "ok": true,
  "tool": "run_poshqc_format",
  "workspace_root": "C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-ac1a08c3569adb7eb",
  "summary": "Ran bundled PoshQC format against 'C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-ac1a08c3569adb7eb' with 2 selected scan folder(s)."
}
```

`run_poshqc_format` had never been measured in this repository, so no exit-code expectation was
asserted ahead of this task. **The measured value is `EXIT_CODE: 0`** (`ok: true`). This value is
recorded as the **sole comparison basis** for [P6-T1]'s exit-code condition.

## `git status --porcelain -- scripts/vscode tests/scripts/vscode`

| Capture | Result |
|---|---|
| Immediately **before** the run | (empty) |
| Immediately **after** the run | (empty) |

**Paths the formatter rewrote at the merge base: none.** No `git checkout -- <path>` revert was
required.

**The merge base is formatter-clean** for `scripts/vscode` and `tests/scripts/vscode`.

## Consequence for [P6-T1]

The merge-base rewrite list is **empty**. Therefore, at [P6-T1], any file the formatter leaves
modified other than the two files this feature edits
(`scripts/vscode/Invoke-VSBuild.ps1` and `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`) is **not**
pre-existing drift: it is attributable to this feature and triggers the loop restart that [P6-T1]
prescribes. There is no pre-existing-drift allowance to draw on.

[P6-T1]'s exit-code condition is "no worse than 0", i.e. it must be `0`.

## Output Summary

PoshQC format returned `EXIT_CODE: 0` against the unmodified tree and rewrote **zero** files in
`scripts/vscode` and `tests/scripts/vscode`; `git status --porcelain` for those paths is empty both
before and after. The merge base is formatter-clean, so the [P6-T1] pre-existing-drift comparison
list is empty and its exit-code floor is 0.
