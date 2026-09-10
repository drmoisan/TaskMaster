---
name: task-notification-is-not-proof-of-death
description: A "completed" task-notification can fire mid-run and the agent may continue and finish on its own; quiescence tests measure the workload, not the agent, so they cannot distinguish the two
metadata:
  type: feedback
---

A `<task-notification>` with `status: completed` does **not** prove the child is finished. The same
task-id can notify more than once, and an agent that notified can keep working and complete on its
own. Do not relaunch a "stopped" child on the strength of one notification plus a workload-quiescence
check.

**Why:** Verified 2026-09-09 on the review-residuals-2026-09-08 epic, feature 825. The child notified
with a narrative saying its PR was open and `vstest` was still running — not the bounded return shape.
I re-derived durable state (PR OPEN, worktree clean at the PR head), scanned for live processes (no
`vstest.console`, `testhost`, `CodeCoverage`, or `datacollector`), and double-sampled the coverage
file 20 seconds apart (identical mtime and length). Everything said "idle", so I launched a resume
agent. The original child merged **23 seconds after my ground-truth read**, while I was still
composing the resume prompt.

The flaw is precise: every one of those probes measures the **workload** — is a build or test process
running, is an output file still growing. None of them measures whether the **agent** is alive and
between tool calls. An agent that has just finished a long test run and is deciding what to do next
looks byte-for-byte identical to an agent that has died. The notification's own note states the rule
outright ("A task-notification fires each time this agent stops with no live background children...
the same task-id may notify more than once"); I under-weighted the text in front of me.

**How to apply:**
- Treat a notification whose payload is NOT the agreed bounded return shape as *in-flight*, not dead.
  The contract is the shape; a narrative status update is a progress report.
- Before relaunching, prefer a probe that is a genuine single-instance side effect (did the PR merge?
  did the branch move?) sampled over a real interval — minutes, not seconds — rather than process and
  mtime scans, which only prove no build is running right now.
- Weigh the cost asymmetry: waiting longer costs time; relaunching costs a double-delegation. Bound
  the damage first by asking what side effect the duplicate could repeat. A merge cannot happen twice
  (the second attempt fails on an already-merged PR), so that case is survivable; issue creation and
  PR creation are not, and are the sharpest probes. See [[double-delegation-the-idleness-test]].
- There is **no SendMessage tool** available to this agent, so a redundant child cannot be recalled.
  Let it exit rather than killing it, which risks a half-written shared checkpoint, and do not launch
  the next feature until it does — under non-isolated execution all children share one session-cwd
  `orchestrator-state.json`. See [[no-sendmessage-tool-resume-child-in-place]].
