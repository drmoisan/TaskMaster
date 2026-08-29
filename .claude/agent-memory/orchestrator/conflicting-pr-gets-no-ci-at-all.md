---
name: conflicting-pr-gets-no-ci-at-all
description: A PR whose mergeable state is CONFLICTING spawns ZERO check runs, so "no checks reported" means diagnose mergeability, not wait longer for CI
metadata:
  type: project
---

`gh pr checks <N>` printing `no checks reported on the '<branch>' branch`, together with
`gh run list --branch <branch>` returning nothing and
`gh api repos/<owner>/<repo>/commits/<sha>/check-runs --jq .total_count` returning `0`,
is NOT a slow queue. Check `gh pr view <N> --json mergeable,mergeStateStatus` first.

**Why:** a `pull_request`-triggered workflow runs against the *merge commit* GitHub computes
from the PR head and its base. When the PR is `CONFLICTING` / `DIRTY`, that merge commit cannot
be computed, so GitHub never creates the check runs at all. There is no failure surfaced anywhere
in the Actions UI — the run simply does not exist. Waiting or re-running produces nothing, and
`gh pr checks --watch` returns immediately rather than blocking.

In a parallel cohort this is the normal state after a sibling item merges to `main`: the base
moves, the shared `.claude/agent-memory/*/MEMORY.md` index lines collide, and the PR flips to
CONFLICTING minutes after it was created green. Confirmed on PR #689 for issue #440, where the
sibling #688 merge invalidated the base and produced exactly this signature.

**How to apply:** after `gh pr create`, if no checks appear within a minute, read
`mergeStateStatus` before anything else. `BLOCKED` means checks exist but have not passed;
`DIRTY`/`CONFLICTING` means resolve the conflict and push, after which CI starts on its own.
Re-verify with `gh api .../check-runs` bound to the *live* head SHA, since the PR head moves
when you push the resolution. See [[force-push-guard-blocks-rebase-use-merge]] for the
resolution mechanics, and [[parallel-epic-children-conflict-on-agent-memory-index]] for why
the conflict is almost always only the memory index.
