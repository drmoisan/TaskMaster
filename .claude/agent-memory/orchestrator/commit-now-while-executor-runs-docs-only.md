---
name: commit-now-while-executor-runs-docs-only
description: A mid-run COMMIT NOW request with no message channel to the running executor can be served by committing only feature-folder Markdown yourself; the plan's later commit task still passes
metadata:
  type: feedback
---

When the coordinator relays a maintainer COMMIT NOW request while an atomic-executor is running and the session has no SendMessage tool, commit it yourself: hygiene-grep the feature folder, `git add -- <feature folder>` (Markdown only), commit with the pathspec form, push. Do not touch code paths.

**Why:** on 948 (2026-10-02) this was done mid-Phase-0 at fda84e77c. The executor's own P0-T19 commit later ran normally on the remaining files, and its acceptance (cached listing is Markdown under the feature folder) was unaffected. The executor reported the commit as made "by another actor", so it does notice the interleave.

**How to apply:** check `git status` first (no staged code), keep the commit to the feature folder, record it in the checkpoint (`wip_commits`), and tell the parent. A plan whose acceptance asserts an exact commit listing could break this way, so read the next commit task before you commit. Related: [[no-sendmessage-relaunch-with-resume-brief]], [[one-executor-per-worktree]].
