---
name: shared-build-lock-coordinator-hold-and-waiter-kill
description: The parallel-run shared build lock has a COORDINATOR-HOLD holder that is never stale and must not be forced; and killing a waiter by matching the acquire script name terminates every sibling item's waiter at once
metadata:
  type: project
---

Parallel runs in this repo serialize msbuild/vstest/csharpier through a file-lock directory at
`<worktree-root-parent>/parallel-build-lock/` with `acquire.txt` and `release.txt` script bodies that
child agents dot-invoke via `[scriptblock]::Create((Get-Content -Raw ...))`. Two properties of it bit
on the bugs-2026-09-11 run, item 838 (2026-09-13).

**1. `COORDINATOR-HOLD` is a hard stop, not a stale lock.** `holder.txt` normally reads
`<item>|<iso-timestamp>`, and `acquire.txt` breaks a lock whose timestamp is older than 45 minutes.
But it special-cases a holder whose first field is the literal `COORDINATOR-HOLD`: that holder is
NEVER treated as stale at any age, and the waiter prints
`WAITING - COORDINATOR THROTTLE HOLD since <ts>. This is a deliberate pacing hold, not a stuck peer.
Keep waiting; do not force it.` It loops until the 60-minute deadline and then exits 1 with `TIMEOUT`.

So an item that finds this holder cannot make progress on ANY gate command, and every downstream plan
task that needs a build is unreachable. **Why:** the coordinator uses it to pace a whole cohort (for
example under a model-quota hold), so forcing it defeats the pacing for every sibling simultaneously.
**How to apply:** read `parallel-build-lock/LOCK/holder.txt` yourself before believing a child's
"lock unavailable" report — a `COORDINATOR-HOLD` there means stand down and report to the coordinator,
not retry, not break the lock, and not fall back to running the gate unlocked. Record it in the
checkpoint under a descriptive field: `blocked_reason`'s enum has no value for an external resource
hold, so the enum stays `none` (see [[blocked-reason-enum-cannot-express-substantive-halt]]).

**2. Killing "your own" waiter by script name kills all twelve.** Every item's waiter has a byte-
identical command line, because the item number is passed to the dot-invoked scriptblock and never
appears in the parent `pwsh` command line. An `atomic-executor` that matched processes on the
`acquire.txt` script name to clean up its own waiter terminated 12 processes — every sibling item's
queued waiter — and its own shell chain with it (exit 255). The lock itself was untouched, so nothing
was stranded, but every sibling had to re-acquire. Independently confirmed afterwards: zero
`acquire.txt` waiters remained running.

**How to apply:** tell every delegate that runs gate commands to leave its waiter alone — the waiter
exits on its own at `ACQUIRED` or at the 60-minute `TIMEOUT`. If a waiter genuinely must be killed,
match on the recorded process id captured at launch, never on the script name or command line. Add
this to the verbatim build-lock block passed to subagents.

**3. The `[scriptblock]::Create` form is forced, not stylistic.** The three lock bodies are `.txt`
files, and `pwsh -File` refuses any file without a `.ps1` extension outright: `Processing -File
'...acquire.txt' failed because the file does not have a '.ps1' extension.` So the only single-segment
Bash invocation that works is
`pwsh -NoProfile -Command "& ([scriptblock]::Create((Get-Content -Raw -LiteralPath '<path>/acquire.txt'))) -Item <n>; exit $LASTEXITCODE"`.
Verified end to end on 2026-09-17 (item 900): acquire printed `ACQUIRED 900 at <ts>` exit 0, release
printed `RELEASED by 900 at <ts>` exit 0. **How to apply:** put the literal working command in the
delegate's build-lock block rather than naming the script paths and leaving the child to work out the
invocation — a child that tries `-File` first burns a round on a refusal that looks like a permission
denial. Note `release.txt` refuses to release a lock held by another item (`RELEASE REFUSED`), so
passing the wrong `-Item` fails loudly rather than stealing the lock.

Related: a child's stop report may attribute a commit it did not author to "a hook on the staging
call". On this run the commit had also been PUSHED, which no hook does — the likelier actor is the
parent session committing into the child worktree
([[parent-session-can-commit-into-child-worktree]]). Verify `HEAD` against the remote ref before
accepting a child's explanation of provenance
([[subagent-self-reported-correction-can-be-false]]).
