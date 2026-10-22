# [P4-T1] Phase 4 PowerShell Change Batch Open

Timestamp: 2026-09-13T06-18

Command: `pwsh -NoProfile -Command "Get-ChildItem -Path '.claude/state/powershell-batch-budget.*.json' -ErrorAction SilentlyContinue | Where-Object Name -ne 'powershell-batch-budget.default.json' | Remove-Item -Force -ErrorAction SilentlyContinue; (Get-ChildItem -Path '.claude/state/powershell-batch-budget.*.json' -ErrorAction SilentlyContinue | Where-Object Name -ne 'powershell-batch-budget.default.json' | Measure-Object).Count"`

EXIT_CODE: 0

REMAINING_BATCH_BUDGET_STATE_FILE_COUNT: 0

Output Summary: The batch-budget state files for this worktree were removed and the
remaining count printed 0 before any edit in this task was attempted. The file named
`powershell-batch-budget.default.json` is excluded from both the deletion clause and the
count clause: the hook resolves its state file name from the session identifier and falls
back to a worktree-derived name, so it never reads the default-named file here, and that
file is tracked in this repository, so deleting it would stage a deletion that neither the
Phase 7 inventory gate nor the Phase 7 clean-tree gate admits.

## Builder signature change in this task

`Get-VsTestArgumentList` in `scripts/vscode/Invoke-MSTest.ps1` gained two mandatory
parameters, `ResultsDirectory` and `LogFileName`, and the returned array gained a
results-directory switch and a trx logger switch carrying the supplied explicit log file
name. Both new parameters are declared with `[Parameter(Mandatory = $true)]`. The
observation of the returned array shape is made by the tests added in P4-T5 and by the
exact-array assertion repaired in P4-T3 and P4-T4, not by this artifact.
