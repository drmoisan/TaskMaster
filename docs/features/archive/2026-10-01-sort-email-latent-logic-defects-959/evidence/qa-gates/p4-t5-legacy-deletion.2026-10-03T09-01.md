# P4-T5 Legacy Partial Deletion: DELETE CHANNEL REFUSED (stop record)

Timestamp: 2026-10-03T09-01
Command: CMD-DELETE with PATH UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs, issued as one pwsh -NoProfile -Command payload (Set-Location to the item worktree; Write-Output EXISTS-BEFORE from Test-Path; Remove-Item -LiteralPath on the path when present; Write-Output EXISTS-AFTER; Write-Output PORCELAIN from git status --porcelain on the path)
EXIT_CODE: NOT-RUN (the payload never executed: the PreToolUse hook refused the Bash tool call before any statement ran, so no process exit code exists)
Output Summary: DELETE CHANNEL REFUSED. A PreToolUse:Bash hook blocked the CMD-DELETE payload with EPIC_WORKTREE_REMOVAL_BLOCKED (TARGET_WORKTREE_NOT_DERIVABLE). The legacy partial was not deleted, Edit E-UCS-CSPROJ-REMOVE was not applied, and CMD-CSPROJ, the numstat and the porcelain commands of this task were not run. Per PD-3 and the HOOK RULE of the delegation, the executor stops here and does not substitute another deletion route or reword the payload.

Hook refusal text (verbatim):

PreToolUse:Bash hook error: EPIC_WORKTREE_REMOVAL_BLOCKED: TARGET_WORKTREE_NOT_DERIVABLE: the command names no usable worktree_path, so the epic run it belongs to cannot be identified. EPIC_WORKTREE_REMOVAL_BLOCKED: git worktree remove for '' requires either an epic checkpoint features[] record with merge_status in {merged, worktree_removed}, or a parallel-orchestrator checkpoint with route_id == "parallel" whose matching items[] record (matched by worktree_path) has merge_status in {merged, worktree_removed}. No checkpoint authorized this removal.

Observed state after the refusal (git status --porcelain --untracked-files=all, read-only): the legacy partial is not listed as deleted; the tracked changes are the P4-T1, P4-T2 and P4-T4 rewrites of SortEmail_SaveCase_Tests.cs, SortEmail_AttachmentSaving_Tests.cs and SortEmail.AttachmentSaving.cs, the plan check-offs, and the P4-T1, P4-T2, P4-T3 and P4-T4 evidence artifacts.

Observation on the cause (not a repair): the payload text contains both the word git (the git status statement of CMD-DELETE) and the word Remove (Remove-Item), which the epic worktree-removal hook appears to read as a worktree removal command, although the payload removes one tracked source file and runs no git worktree command.

- EXISTS-BEFORE: not printed (payload not executed)
- EXISTS-AFTER: not printed (payload not executed)
- PORCELAIN: not printed (payload not executed)

Acceptance check (P4-T5): not met. EXISTS-BEFORE True and EXISTS-AFTER False were not observed; the UCS rows were not re-derived; the numstat and porcelain lines were not produced. Stop label: DELETE CHANNEL REFUSED.
