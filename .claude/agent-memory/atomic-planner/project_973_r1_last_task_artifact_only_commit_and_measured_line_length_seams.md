---
name: project-973-r1-last-task-artifact-only-commit-and-measured-line-length-seams
description: "#973 remediation cycle 1 preflight round 1 seams - under commit-per-task the last task's commit stages its artifact alone (the plan carries no change until its own flip); a fail-before dossier may not cite a later task's result; flip the spec box before writing a check-off artifact that transcribes the flipped line; measure line lengths with Grep ^.{N,} (CR adds 1) instead of estimating; every Glob-based zero gate under .claude/worktrees needs a sibling positive-control Glob; a loop-fix commit needs an explicit <paths>; reviewer delta text can miscount the file ('all five' where the file read 'all four')"
metadata:
  type: project
---

Preflight round 1 for the #973 remediation plan (2026-10-06): nine reviewer deltas applied in worktree agent-a24d410b914bcefd7, no Bash, no validator.

**Why:** the round-0 plan carried six classes of defect that the same adversarial pass could have caught: a last-task commit that assumed the plan was dirty, a dossier citing a result produced two tasks later, an artifact that quoted a line "after the flip" before the flip happened, an estimated line length (237) against a measured one (270 with indent; longest existing 208), a Glob zero-gate with no proof the tool could see the folder, and a loop-fix commit with no path list.

**How to apply:**
- Commit-per-task with flip-before-commit: every task's commit carries its own check-off, so when the LAST task reaches CMD-COMMIT the plan file is clean. Its `<paths>` is the artifact alone and `COMMITTED-PATHS:` equals that one path; the plan-staging clause in CMD-COMMIT must name the two exceptions (last task, loop-fix commit). Restate C8-R's last sentence in the same round.
- A fail-before exception dossier written in task N may cite results of tasks before N only; name a later gate's artifact path as "the after-edit run" without stating its result.
- When a check-off artifact must transcribe the flipped spec line, order the task: Edit the box, Grep `^- \[x\] ACnn ` with `-n` (Read at that line with `limit` 1 when the Grep tool omits the long line), then write the artifact with `SPEC-LINE:` from that read.
- Never estimate a line length. Grep `^.{N,}` on a CRLF file counts the CR, so "longest line L" is `^.{L+1,}` count 1 and `^.{L+2,}` count 0; count the replacement line character by character and record both in section 4.
- A Glob zero-gate (`**/*.{xml,trx,coverage}` returns nothing) in a worktree under `.claude/worktrees/` needs a sibling positive control in the same task: Glob a folder known to hold one file (here `runbooks/*.{xml,md}` returning the runbook), `GLOB-CONTROL: 1`, zero is STOP: GLOB-BLIND. State the `path` for both Globs.
- A loop-fix CMD-COMMIT inside a QC loop rule needs its `<paths>` spelled (exactly the rewritten file); otherwise the "COMMITTED-PATHS equal to <paths>" gate has no operand.
- Reviewer delta text can misquote the file (it said update "all five as stated"; the file read "all four"). Apply by meaning, replace the numeral with an enumerated label list, and report the discrepancy.
- When a spec sentence is corrected in a revision round, append a dated note to the Planner Amendment entry that owns the sentence, worded so it does not self-hit the P0-T3 count Greps for the new or old wording.
- Related: [[project-973-cycle1-premise-correction-refscan-and-tautology-fold-seams]], [[project-964-r2-preparation-record-closed-evidence-set]], [[self-referential-evidence-enumeration]].
