# Prohibited-construct gate (issue #968, task P7-T3)

Timestamp: 2026-10-03T03-26
Command: pwsh -NoProfile -Command '<CMD-ADDED-SCAN payload>' (the Command Reference macro executed verbatim with PREFIX expanded, WORKTREE substituted and CODE14-GIT expanded to the fourteen Write Set code paths), run after the P6-T9 commit so the new files are tracked
Canonical command: added lines of `git diff 94287369908cc920b21b0e3256314f988ad7d2f5 -- CODE14-GIT`, scanned for the prohibited tokens and the two positive controls; then git -C WORKTREE diff --exit-code 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD -- scripts/vscode/TaskMaster.cli.runsettings TaskMaster.runsettings; git -C WORKTREE status --porcelain -- scripts/vscode/TaskMaster.cli.runsettings TaskMaster.runsettings; CMD-TOKEN-COUNT on FT
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (both payloads)
- GIT_DIFF_EXIT_CODE: 0
- ADDED_LINES: 696
- ADDED-TOKEN [Thread.Sleep] = 0
- ADDED-TOKEN [Task.Delay] = 0
- ADDED-TOKEN [DoNotParallelize] = 0
- ADDED-TOKEN [Retry(] = 0
- ADDED-TOKEN [Path.GetTempFileName] = 0
- ADDED-TOKEN [Path.GetTempPath] = 0
- ADDED-TOKEN [Workers] = 0
- ADDED-TOKEN [Timeout(] = 4
- ADDED-TOKEN [[Timeout(GateTimeoutMs)]] = 4 (every added timeout attribute is the sibling file's constant convention, in the pin-count class; the datamodel tests gain none)
- ADDED-TOKEN [GateTimeoutMs = ] = 1
- ADDED-LINE private const int GateTimeoutMs = 60000; (no timeout increase: the value equals the sibling constant)
- ADDED-TOKEN [await Task.Yield();] = 0
- ADDED-TOKEN [for (int i] = 0
- ADDED-TOKEN [using var ] = 0
- ADDED-TOKEN [_pinCount] = 4 (positive control)
- ADDED-TOKEN [ArmingFakeTimeProvider] = 6 (positive control)
- runsettings diff --exit-code: exit 0, no output (both runsettings files unchanged from BASE)
- runsettings porcelain: exit 0, prints nothing
- FT tokens: `[Timeout(GateTimeoutMs)]` 8, `private const int GateTimeoutMs = 60000;` 1 (equal to P0-T12)
- No PROHIBITED CONSTRUCT ADDED.
