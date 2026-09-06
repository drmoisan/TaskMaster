---
name: feature-review-git-c-form-hangs-unattended
description: feature-review's Bash allow-list is only `git diff *` and `git log *`; a `git -C <path> ...` form matches neither and hangs forever in an unattended run
metadata:
  type: feedback
---

Never tell `Agent(feature-review)` to use `git -C <worktree> ...`. Its Bash allow-list is
only `Bash(git diff *)` and `Bash(git log *)`, which match on the literal **start** of the
command text. `git -C <path> diff ...` starts with `git -C`, so it matches neither pattern,
escalates to a permission prompt, and in an unattended run nothing ever answers it.

**Why:** on issue #584 (2026-09-03) a feature-review launched at 23:26 produced a zero-byte
transcript and wrote nothing for four hours before the parent killed it. Nothing landed on
disk. The relaunch, with all Bash removed, worked.

**How to apply:** when delegating to `feature-review`, state as a binding first directive
that it must NOT use the Bash tool at all, and give it the absolute worktree path for
Read/Grep/Glob instead. Two compounding reasons:

1. Its cwd is the SESSION root, not the item worktree (see
   [[preparation-child-cwd-is-session-root-not-item-worktree]]), so a bare `git diff` that
   *would* pass the allow-list silently reads a DIFFERENT checkout and returns false results.
   The allow-list and the cwd are jointly unsatisfiable: the only git form that passes
   permission is the one that reads the wrong repository.
2. Paste the diff into the prompt instead. The orchestrator has already run
   `git diff --name-status <BASE>..HEAD` during its own verification, so the reviewer does
   not need git at all — and a diff supplied verbatim is the same evidence it would have
   gathered.

Also tell it explicitly to record a check as UNVERIFIED and continue rather than stall, so a
single unavailable observation cannot silently consume the whole run.

Generalises beyond feature-review: any subagent whose allow-list is a narrow command-prefix
set will hang rather than error when handed a near-miss form. Check the agent's declared
`Bash(...)` patterns against the exact command text you are about to prescribe. See
[[feedback_no_cd_or_non_allowlisted_bash_segments]] for the repo-wide allow-list rule and
[[delegate-may-lack-bash-tool-verify-its-git-claims]] for the converse failure.
