# P6-T13 AC5 Identity and Feature-Folder Sweep: HOOK BLOCK (stop record)

Timestamp: 2026-10-03T12-56
ITERATION: 1
Command: CMD-TST-IDENTITY (MERGE-BASE 94287369908cc920b21b0e3256314f988ad7d2f5), issued as one pwsh -NoProfile -Command payload with Set-Location to the item worktree; CMD-SWEEP, issued the same way; CMD-EVIDENCE-FIELDS not issued (the run stopped at the hook block)
EXIT_CODE: NOT-RUN (scoped to the CMD-EVIDENCE-FIELDS payload, which was not issued; the CMD-TST-IDENTITY payload never executed because the PreToolUse hook refused the Bash tool call before any statement ran)
Output Summary: HOOK BLOCK. A PreToolUse:Bash hook (enforce-epic-worktree-removal-gate.ps1) refused the CMD-TST-IDENTITY payload with EPIC_WORKTREE_REMOVAL_BLOCKED (TARGET_WORKTREE_NOT_DERIVABLE). The payload is read-only (git diff, git status and string filtering). The CMD-SWEEP payload ran and printed all-zero counts. The maintainer bypass covers the CMD-DELETE payload at P4-T5 and P5-T10 only, and the standing hook approval covers enforce-promotion-mcp-only.ps1 false positives only. Under the HOOK RULE of the delegation the executor stops here, does not reword the payload, and does not run CMD-EVIDENCE-FIELDS. P6-T13 is not checked off.

Hook refusal text (verbatim):

PreToolUse:Bash hook error: EPIC_WORKTREE_REMOVAL_BLOCKED: TARGET_WORKTREE_NOT_DERIVABLE: the command names no usable worktree_path, so the epic run it belongs to cannot be identified. EPIC_WORKTREE_REMOVAL_BLOCKED: git worktree remove for '' requires either an epic checkpoint features[] record with merge_status in {merged, worktree_removed}, or a parallel-orchestrator checkpoint with route_id == "parallel" whose matching items[] record (matched by worktree_path) has merge_status in {merged, worktree_removed}. No checkpoint authorized this removal.

Observation on the cause (not a repair): the payload text contains the word git (git diff, git status) and the substring remove in the variable name $removed1 and the labels TST1-REMOVED-*. The epic worktree-removal hook appears to read that combination as a worktree removal command. The payload removes nothing and runs no git worktree command. This is the same hook mechanism that refused CMD-DELETE at P4-T5 (p4-t5-legacy-deletion.2026-10-03T09-01.md).

## CMD-TST-IDENTITY

Not executed. NUMSTAT-TST2, TST2-DELETED-LINES, TST1-REMOVED-LINES, TST1-REMOVED-SANITIZE-LINES, TST1-REMOVED-TRYSAVE-LINES and TST1-PORCELAIN-LINES were not printed.

## CMD-SWEEP (executed; counts only, tokens never written)

```
FILES: 80
ACCOUNT-TOKEN-FILES: 0
PROFILE-LEAF-FILES: 0
MACHINE-TOKEN-FILES: 0
WORKTREE-ROOT-FILES: 0
USERS-PATH-FILES: 0
RAW-DOCUMENT-FILES: 0
```

## CMD-EVIDENCE-FIELDS

Not issued.

## Acceptance (P6-T13)

Not met: clauses 1 and 2 (the TST2 and TST1 identity counts) and clause 4 (the evidence-field and subfolder checks) were not observed. Clause 3 (the six sweep counts) was observed at 0. Stop label: HOOK BLOCK at P6-T13 (enforce-epic-worktree-removal-gate.ps1).
