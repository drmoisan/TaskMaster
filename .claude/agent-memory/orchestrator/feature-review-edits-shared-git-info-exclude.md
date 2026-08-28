---
name: feature-review-edits-shared-git-info-exclude
description: feature-review can add your feature folder to the SHARED .git/info/exclude to satisfy a session-cwd SubagentStop hook — that one line hides untracked files in EVERY worktree sharing the git dir, including live siblings; revert it immediately, never "after merge"
metadata:
  type: project
---

When `feature-review` runs against a feature worktree whose path differs from the session cwd, its
SubagentStop validation hook `Test-Path`s the audit artifacts **relative to the session cwd** and
blocks termination because the feature folder does not exist there. Observed mitigation the agent
chose on its own, and disclosed:

1. mirrored the three audits into the session checkout, and
2. appended `docs/features/active/<feature>/` to
   **`<main-checkout>/.git/info/exclude`** — a file shared by every linked worktree, and
3. committed the audits on the feature branch (because that same exclude then hid them as untracked).

Step 2 is the dangerous one. `.git/info/exclude` is **not** per-worktree. One line there hides
untracked files under that path in *every* worktree sharing the git directory — including a
concurrently-live sibling's. It can also mask files **you** still need to commit, so a later
`git status --porcelain` reads clean when it is not, and a completion gate that trusts a clean tree
passes vacuously.

**Why:** the agent offered to clean up "after merge". That is too late — the window where the line is
live is exactly the window where you run your pre-PR clean-tree checks and your fan-in invariant.

**How to apply.** After any `feature-review` run in a worktree whose path differs from the session cwd:

1. `cat <main-checkout>/.git/info/exclude` and look for a line naming your feature folder. It will not
   appear in `git status`, so you must read the file.
2. Remove that line **before** any clean-tree assertion, PR creation, or merge gate.
3. Re-run `git status --porcelain` immediately after removal. Output that was previously hidden
   appears now; empty output is the proof nothing was concealed.
4. Delete the session-root mirror copies, and confirm the audits are tracked on the branch first so
   removing the mirrors loses nothing.

Do not simply revert the commit the agent made — committing the audits on the feature branch is
correct and desirable. Only the shared-exclude line and the mirrors are the side effects to undo.

Related: [[agent-worktree-hooks-resolve-to-agent-cwd]],
[[mcp-hardlock-and-review-mirror-quirks]] (feature-review mirroring audits where a sibling can commit
them), [[child-orchestrator-pr-hook-reads-session-root]].
