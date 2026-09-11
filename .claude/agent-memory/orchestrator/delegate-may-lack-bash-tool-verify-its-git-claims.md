---
name: delegate-may-lack-bash-tool-verify-its-git-claims
description: An atomic-planner session can have NO Bash tool, so it cannot run a single git command; verify worktree cleanliness, diff surgicality and line endings yourself rather than accepting its report
metadata:
  type: feedback
---

A delegated `atomic-planner` may have no Bash tool at all in its surface. It reported: *"No such tool
available: Bash... in subagents as well as here."* That leaves it unable to run `git rev-parse`,
`git status`, or `git diff --numstat`, so every git-shaped confirmation a delegation prompt asks for
comes back unanswerable.

**Why this is a trap rather than a nuisance.** The requests that go unanswered are exactly the safety
checks: "confirm you edited the right worktree", "confirm the diff is surgical", "confirm line
endings did not flip", "confirm the plan file is the only modified path". A delegate that cannot run
them may reason its way to an answer instead. The #736 planner did the right thing — it stated the
limitation explicitly and substituted a byte-level control it *could* perform (comparing against an
untouched copy made before its edits) — but the orchestrator still has to do the real check.

**How to apply.** When a delegation prompt asks for git confirmations, treat their absence as
expected, not as evasion, and run them yourself the moment the delegate returns:

- `git -C <worktree> status --porcelain` — is the intended file the only change?
- `git -C <worktree> diff --numstat` — is the count small, or did the whole file get rewritten? This
  is also the cheapest CRLF detector: a line-ending flip shows as every line changed, so a small
  numstat disproves it without reading a byte.
- `git -C <worktree> diff --cached --numstat` after staging, to confirm `.gitattributes`
  normalisation did what you expected.

Do not skip these because the delegate sounded careful. Related:
[[subagent-self-reported-correction-can-be-false]] and
[[feedback_verify_subagent_capability_claims]]. Note the converse also holds and is worth the same
scepticism: on the same run this planner **overrode** a delta the orchestrator relayed, and was
right to — see [[reconcile-plan-numbers-against-your-own-measurements]]. Verify the correction too,
in both directions.
