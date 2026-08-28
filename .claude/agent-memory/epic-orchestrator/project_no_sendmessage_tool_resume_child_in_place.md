---
name: no-sendmessage-tool-resume-child-in-place
description: epic-orchestrator has no SendMessage tool, so a stopped child cannot be resumed by messaging it — launch a fresh Agent(orchestrator) that adopts the existing worktree and branch in place
metadata:
  type: project
---

`epic-orchestrator`'s tool set is `Agent`, `Read`, `Grep`, `Glob`, `Write`, `Edit`, `Bash`, and two
`drm-copilot` MCP tools. **There is no `SendMessage` tool**, despite the `Agent` tool description advertising
it. A child that stops (task-notification `completed`) therefore cannot be handed follow-up instructions.

**Why:** On the quickfiler-bug-family epic I tried to relay adjudications to a stopped feature-442 child by
forking a `fork` subagent and asking it to echo the message. That burned ~119k tokens for zero tool uses and
delivered nothing — a fork cannot message a sibling either. The only working mechanism is a new `Agent` call.

**How to apply:** To continue a stopped child, launch a **fresh `Agent(orchestrator)` with NO `isolation`
parameter** and instruct it to work directly in the existing worktree path and branch. Carry forward into the
prompt every fact the prior run established, marked as parent-verified vs. child-claimed, plus any
adjudications — the fresh agent has none of that context.

**The idleness test must be four-part. Git metadata alone is NOT sufficient** — I learned this the hard way
immediately after first writing this memory, by relaunching into a worktree whose original child was still
mid-run (see [[double-delegation-idleness-test]]). A child running a long coverage suite performs no git
writes for tens of minutes, so gitdir mtimes look stale while it is fully alive. Require ALL of:
1. no `*.lock` in the **real** gitdir `.git/worktrees/<name>/` (in a worktree `.git` is a *file*, so its own
   mtime reveals only creation time; `index`/`ORIG_HEAD` there are the useful ones);
2. `git status --porcelain` empty;
3. **no `vstest`/`testhost`/`MSBuild` process whose command line references that worktree** — check via
   `Get-CimInstance Win32_Process`, and note the process may belong to a *sibling* worktree, which is fine;
4. **no working-tree file written more recently than the git metadata** — a fresh evidence artifact under the
   feature folder while `index` sits an hour stale is proof of a live agent.

Prefer not relaunching at all while the original task may still be live; a task-notification means the agent
stopped, not that its spawned background processes did.
Related: [[api500-abandoned-child-fresh-redelegation]], [[host-crash-multichild-resume-in-place]],
[[stale-checkpoint-is-not-a-dead-agent]].
