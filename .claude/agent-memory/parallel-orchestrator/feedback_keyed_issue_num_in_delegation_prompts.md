---
name: keyed-issue-num-in-delegation-prompts
description: Write `issue_num: <N>` in every delegation prompt and never a bare `#<N>` for another item — the preimplementation gate's extractor falls back to the first hash-number and resolves the wrong item
metadata:
  type: feedback
---

In an `Agent(orchestrator)` delegation prompt, write the item key as `issue_num: <N>.` and never
write a bare `#<N>` referring to any OTHER item of the run.

**Why:** `PREIMPLEMENTATION_GATE_BLOCKED ... the failed readiness predicate is 'merge_status'` reads
like a checkpoint defect, but it is usually a prompt-parsing artifact. Two mechanisms compose, both
in `.claude/hooks/enforce-orchestration-preimplementation-gate-modes.ps1`:

- `Find-OrchestrationDelegationIssueNumber` matches the keyed form with
  `issue[_-]?num(?:ber)?\s*[:=]\s*#?(\d+)`. The separator class is `[_-]` ONLY, so `issue_num:`,
  `issue-num:` and `issuenumber:` match but **`Issue number:` with a space does not**. On that miss
  it falls back to a bare `#(\d+)` scan of the whole prompt and returns the first hash-number in the
  prose — which on a parallel run is typically a sibling item you mentioned in passing.
- `Find-OrchestrationModeRecord` then iterates `items[]` in ARRAY ORDER and checks
  `TargetFolder` then `IssueNumber` **per record**, not folder-across-all-records first. So a wrong
  issue number that matches an EARLIER item beats the correct `feature_folder` match on a later one.
  Its docstring says "feature_folder basename first and issue_num second", which is true only within
  a single record and is misleading across the collection.

Observed 2026-08-30 on run bugs-638-644-647: the prompt said `Issue number: 644.` and mentioned
`#638` in a justification sentence. The extractor returned `638`, resolved item 638 whose
`merge_status` was the terminal `worktree_removed`, and denied. The checkpoint was correct
throughout.

**How to apply:**

- Use `issue_num: <N>.` verbatim, and spell sibling items in prose without a hash ("the archive-root
  guard fix that is now on main"), not as `#638`.
- **Diagnose with the hook's own functions before touching the checkpoint.** Dot-source
  `.claude/hooks/enforce-orchestration-preimplementation-gate-modes.ps1` and call
  `Find-OrchestrationDelegationIssueNumber`, `Find-OrchestrationDelegationTargetFolder`,
  `Find-OrchestrationModeRecord` and `Get-ParallelOrchestrationReadinessFailure` against the real
  prompt text. PowerShell needs `-ExecutionPolicy Bypass`; without it dot-sourcing dies with
  `PSSecurityException` and every later call fails as an unrecognized cmdlet.
- **Do not "fix" the checkpoint in response to this deny.** Only `merged` and `worktree_removed` are
  terminal; every other `merge_status`, including the failure members, is pre-merge and allowed, so
  re-delegation after a blocked state is legitimate and is not what the gate is objecting to.
- Two Bash-matcher traps while probing: a heredoc whose body contains `gh pr create` trips
  `PR_AUTHOR_SKILL_BLOCKED`, and a denied heredoc leaves any pre-existing file at that path intact,
  so the next run prints stale output that looks like a real result. Verify the probe file is the
  one you just wrote.

This is the same family as [[issue-merge-and-removal-commands-bare]]: a hook scanning whole command
or prompt text for a digit run and binding it to the wrong record. See
[[parallel-run-execution-playbook]].
