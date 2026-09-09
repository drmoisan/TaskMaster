---
name: epic-child-branch-anchored-diff-lists-inherited-commits
description: On an epic child branch cut from the integration branch, `git diff <merge-base HEAD origin/main>` lists every sibling commit's paths, so footprint gates written against it are unsatisfiable
metadata:
  type: project
---

A feature branch cut from an epic **integration** branch inherits every sibling feature's commits, so
`git merge-base HEAD origin/main` resolves below all of them. An anchored two-dot diff against that
base then lists the whole inherited set, not this feature's change.

Measured on issue #821 (`bug/...-821-exec`, 2026-09-09): merge base `6f08302a` = `origin/main` tip,
but **70 commits** and **184 paths** sat between it and HEAD. The anchored diff returned **192 paths**
against **38** authored ones. The inherited set included `UtilitiesCS.Test/UtilitiesCS.Test.csproj`,
entries under `.claude/agent-memory/`, and files named in the spec's own out-of-scope list.

**Why:** plans routinely write footprint gates as "zero `.csproj` in `git diff --name-only (git
merge-base HEAD origin/main)`" or "every path in that span belongs to the permitted set". On an epic
child branch both are **unsatisfiable regardless of what the executor does** — a sibling already put a
`.csproj` in the span. Plan #821 anticipated the mechanism but scoped it to four documents; the real
set was two orders of magnitude larger.

**How to apply:**
- Discriminate with `git diff --name-only HEAD` (authored, pre-commit) **plus** `git status
  --porcelain --untracked-files=all` for untracked paths. That pair is the change the feature authored.
- Prove any offending inherited path is genuinely inherited, don't just assert it: `git log --oneline
  <mb>..HEAD -- <path>` names the sibling commit, and `git diff --name-only HEAD -- <path>` returning
  empty proves the worktree never touched it.
- Run the anchored span anyway and report it in full. Record the condition as *not met as literally
  written*, with the discriminating evidence beside it. Do **not** silently substitute the narrower
  span and call the gate passed, and do not widen the permitted-path list to swallow the inherited set.
- Beware `git log (git merge-base HEAD origin/main)..HEAD` from Bash-invoked pwsh: the `..HEAD` suffix
  after a parenthesised expression does **not** concatenate and the command silently reports 0 commits.
  Substitute the literal SHA. See [[project-pwsh-command-quoting-from-bash]].
