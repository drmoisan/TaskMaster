---
name: file-relay-and-slot-lock-for-hook-payloads
description: A file relay directory outside all worktrees lets executors hand hook-blocked payloads to the coordinator and wait alive; a slot lock file in the same directory serializes expensive subagents across items
metadata:
  type: project
---

On `bugs-2026-09-28` (2026-10-06) the standing maintainer approval said a hook-blocked false-positive payload is
run by the coordinator (the main session), not by any agent. This persona has no message tool, and ending its turn
stalls the run, so a file relay was used: `TaskMaster-wt/coordinator-relay-<slug>/PROTOCOL.md`. The executor writes
`request.<item>-<task>.payload.txt` (exact text) plus a `.md` record, then polls for `response.<item>-<task>.md`
with a pwsh Start-Sleep loop. Once told, the coordinator watched the directory itself (pickup about a minute);
the first request waited 105 minutes because the coordinator did not yet know about the directory.

**Why:** the driver must not run the payload itself (that would be an agent routing around the hook), and
relaunching a child per payload costs a full context rebuild.

**How to apply:** set up the relay before the first launch and tell the coordinator its path in the first report.
The same directory carries a `slot.<item>.lock` file that an item must hold before any subagent delegation, which
kept the subtree under a cap of 4 with two items live. Expect the first item to hold the slot through consecutive
delegations, so the second item effectively runs after it. Children also used the directory for unprompted
`status.<item>.md` answers. Related: [[removal-gates-false-positive-on-readonly-pwsh-payloads]],
[[child-stalls-waiting-on-grandchild-notification]].
