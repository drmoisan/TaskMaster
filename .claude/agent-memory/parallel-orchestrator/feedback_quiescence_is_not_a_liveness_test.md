---
name: quiescence-is-not-a-liveness-test
description: An idle worktree does not mean the child finished — an orchestrator between delegations writes nothing, so a quiescence watch cannot distinguish paused from dead and relaunching on it produces two live agents
metadata:
  type: feedback
---

Never conclude a delegated agent has terminated from filesystem quiescence alone. Wait for the
completion notification, or show from durable state that the work is genuinely abandoned.

**Why:** On run `bugs-2026-09-02`, item 565's orchestrator went idle waiting on its own
`atomic-executor`. I watched the WHOLE worktree (not just the feature folder, which is the usual
mistake) and required five consecutive stable samples. It reported quiescent, so I relaunched a
scoped replacement. The original then woke up and completed the item itself — **two orchestrators
were live on one worktree.**

The reasoning error is precise and worth stating: a quiescence watch detects an idle FILESYSTEM,
not a dead AGENT. An orchestrator sitting between delegations, or waiting on a subagent, writes
nothing at all. Five stable samples are equally consistent with "finished" and with "paused". File
activity is a strictly stronger signal than checkpoint staleness — see
[[stale-checkpoint-is-not-a-dead-agent]] — but it is still not a liveness test, and I treated it as
one.

**How to apply:**

- **The symptom to look for afterwards** is the other agent reporting an unexplained external
  change. Item 565's original orchestrator recorded an `unexpected_external_merge` in its own
  checkpoint: a second merge of `origin/main` arriving "from outside my orchestration session".
  That was the replacement child reconciling. If a child reports work it did not do, suspect a
  second writer before suspecting anything else.
- **Prefer waiting.** A notification arrives eventually and costs nothing but time. A relaunch that
  is wrong costs a duplicated run and risks interleaved writes.
- **If you must act, require a positive abandonment signal**, not an absence: a branch with no
  commits after hours of supposed work, a checkpoint whose `next_step` is impossible to progress
  from, or an explicit stop record. Absence of writes is not evidence of death.
- **Verify the outcome rather than assuming either way.** Here the result was benign and that was
  established, not hoped: single coherent history, clean tree, local head equal to remote, and a
  three-dot footprint against `origin/main` containing exactly the two declared source files.
  Check all four; a double-run can just as easily produce duplicated commits or a foreign path.
- **The residual risk outlives the incident.** The redundant child may still be live and may push
  later. That is contained by re-confirming the pull-request head against `git ls-remote`
  immediately before merging, which the merge procedure does unconditionally — so the containment
  already exists and does not need inventing.

See [[parallel-run-execution-playbook]] and
[[confirm-ci-by-conclusion-not-watch-exit-code]].
