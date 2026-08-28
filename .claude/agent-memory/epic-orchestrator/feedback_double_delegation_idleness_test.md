---
name: double-delegation-idleness-test
description: I launched a second orchestrator into a worktree whose child was still mid-run because I judged idleness from git metadata; assess damage by counting single-instance side effects, not by asking the agents
metadata:
  type: feedback
---

Never authorize a second `Agent(orchestrator)` against a worktree on the strength of git metadata. Use the
four-part idleness test in [[no-sendmessage-tool-resume-child-in-place]].

**Why:** On the quickfiler-bug-family epic, feature 442's child notified as stopped. I checked the real gitdir
(`index` 13:43:50Z, no `*.lock`, clean `git status`), declared it idle, and launched a replacement. The same
task-id then notified again reporting a coverage run in progress — the original had never finished. Working-tree
evidence had been available and I did not look at it: evidence artifacts were written at 13:59, 14:03 and 14:18
while the gitdir index stayed frozen at 13:43. A 6518-test suite does no git writes for tens of minutes.

**How to apply — assessing damage after the fact.** Do not ask either agent whether they collided; count
side effects that can only exist once:
- **Issue creation** is the sharpest probe, because the MCP lifecycle tool always creates a *new* issue. One
  entry (#645 at 13:58:35Z) proved a single run; two would have proved a collision.
- Then check for new commits, `MERGE_HEAD`, lock files, and duplicate PRs.
- Attribute live test processes by command line — mine turned out to belong to a *sibling* worktree, which is
  harmless, and it also independently corroborated the child's own claim about them.

A task-notification means the *agent* stopped, not its spawned background processes; a stopped agent will not
self-resume, which is what kept this from becoming real damage. Record the near-miss and the forward rule even
when nothing broke — the risk was real and the reasoning error is the reusable part. Related:
[[live-child-at-pr-author-not-hung]], [[stale-checkpoint-is-not-a-dead-agent]].
