---
name: issue-merge-and-removal-commands-bare
description: Issue `gh pr merge` with no path prefix — the merge gate parses the first digit run in the WHOLE command text, so a path containing digits is read as the PR number
metadata:
  type: feedback
---

Issue every `gh pr merge --merge <N>` as a BARE command with no `cd` prefix, no other
digit-bearing token, and nothing chained after it. Same for `git worktree remove`.

**Why:** `Get-EpicMergeGateCommandPrNumber` in `.claude/hooks/enforce-epic-merge-gate.ps1`
falls back to `$CommandText -match '(?<![-\w])(\d+)\b'`, which scans the ENTIRE command
string, not just the token after `merge`. A prefix such as
`cd .../TaskMaster-wt/2026-08-29T00-11 && gh pr merge --merge 688` makes the gate parse
`2026` as the PR number — `2026` is preceded by `/`, which is neither `-` nor a word
character, so the negative lookbehind does not exclude it. The gate then matches no
`items[]` record and denies with `EPIC_MERGE_GATE_BLOCKED` even though the checkpoint is
correct. Observed on run bugs-635-440, 2026-08-29. The negative lookbehind does correctly
exclude flag values like `tail -5`, so only unprefixed digit runs are hazardous.

**How to apply:** Update the checkpoint in one call, then issue the merge alone in the next.
Do not diagnose the denial as a checkpoint defect before re-issuing bare — the checkpoint is
usually fine. Two related mechanics on the same path:

- A finished child leaves its worktree LOCKED (`lock reason: claude agent <id> (pid <n>)`,
  where the pid is the still-live parent session). Both removal gates ALLOW the removal; git
  itself refuses. Run `git worktree unlock <path>` first, then remove. Prefer plain `remove`
  over `-f` so a dirty tree still refuses.
- The Bash tool's MSYS path conversion mangles a `git show <ref>:<path>` operand into
  `<ref>;<path>` with backslashes, which fails and silently produces an EMPTY output file if
  redirected. A diff against that empty file reports the whole other file as added, which
  reads like a real result. Verify such a redirect is non-empty before trusting a diff built
  from it.

See [[parallel-run-execution-playbook]] and [[preimplementation-gate-scope]] for the other
command-shape constraints on this surface.
