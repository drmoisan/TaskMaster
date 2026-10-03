# Evidence hygiene gate (issue #968, task P8-T10)

Timestamp: 2026-10-03T03-34
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); $b = [char]92; $files = @(Get-ChildItem -LiteralPath "docs\features\active\2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968\evidence" -Recurse -File); "EVIDENCE_FILES=$($files.Count)"; "RAW_DOCUMENTS=$(@($files | Where-Object { $_.Extension -in @(".trx", ".xml", ".coverage", ".coveragexml", ".log") }).Count)"; $pattern = "[a-z]:[" + $b + $b + "/]+users[" + $b + $b + "/]+[a-z0-9_.~-]"; "PROFILE_PATH_LINES=$(@($files | Select-String -Pattern $pattern).Count)"'
Canonical command: scan of every file under FEATURE/evidence/ for raw test or coverage documents and for drive-letter user-profile paths (the repository hygiene rule's pattern)
EXIT_CODE: 0
Output Summary (final run, after redaction):
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- EVIDENCE_FILES=72
- RAW_DOCUMENTS=0
- PROFILE_PATH_LINES=0

First run (same command, exit 0): EVIDENCE_FILES=72, RAW_DOCUMENTS=0, PROFILE_PATH_LINES=10. The ten offending lines were all in inherited preflight reports under FEATURE/evidence/other/ that were committed before this run (P0-T3 INHERITED-COMMITTED):

- preflight-round1-report.2026-10-02T08-40.md: lines 19 (two occurrences of the item-worktree absolute path inside a quoted hook refusal), 204 and 205 (absolute paths of the plan and spec files)
- preflight-round3-report.2026-10-03T01-01.md: lines 160 and 161 (session-tree agent-memory paths)
- preflight-round4-report.2026-10-03T01-25.md: lines 120 and 121 (session-tree agent-memory paths)
- preflight-round5-report.2026-10-03T01-53.md: lines 111 and 112 (session-tree agent-memory paths)
- preflight-round6-report.2026-10-03T02-21.md: line 116 (a session-tree agent-memory path)

Redaction applied with the Edit tool as P8-T10 directs: the item-worktree absolute path was replaced by the token `WORKTREE` and the session-tree absolute prefix by `REDACTED-PATH`; no other text in those files changed. The task was then re-run (the final run above). A Grep of the whole feature folder for the same pattern (case-insensitive) found no file.

## Re-run after check-offs

Timestamp: 2026-10-03T03-36
Command: the same P8-T10 payload, re-run after P8-T43 (task P8-T44), exit 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- EVIDENCE_FILES=74
- RAW_DOCUMENTS=0
- PROFILE_PATH_LINES=0 (for the evidence tree as it will be committed)
