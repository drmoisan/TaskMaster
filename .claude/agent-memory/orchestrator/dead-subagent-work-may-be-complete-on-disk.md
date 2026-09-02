---
name: dead-subagent-work-may-be-complete-on-disk
description: A subagent killed mid-task may have already written and persisted its fix; diff the working tree against the last commit before relaunching, and never trust its self-reported metrics
metadata:
  type: feedback
---

When a rate limit or crash kills a subagent, its **file writes survive**. Recover by reading the
tree, not by assuming the work is lost.

**Why.** On the #287 parallel-preparation run (2026-09-01) a session rate limit killed both the
orchestrator and its `atomic-planner` child. The coordinator's resume brief said the planner "died
mid-fix on a defect it found in its own draft". `git status` showed the plan file modified against
the last commit, and `git diff --ignore-cr-at-eol` showed the fix was **already complete**: the new
decision record, the rescoped assertion, and a whole placeholder-hardening pass. Relaunching a
planner to redo it would have thrown away finished work and burned a round.

**How to apply.**

- On any dead-child resume, run `git status --porcelain` and `git diff <last-commit> -- <artifact>`
  **before** deciding what is missing. Commit the recovered work immediately so it is durable.
- Diff with `--ignore-cr-at-eol`. Under `core.autocrlf=true` an Edit-tool write flips the whole
  file's line endings and the raw diff looks like a full rewrite; the substantive delta is a few
  lines. See [[edit-tool-crlf-ifies-lf-markdown]].
- Then scope the relaunch to **only** what is genuinely still missing. On #287 that was three items,
  not a re-author.

**The converse trap: never treat a content poll as a completion signal.** Waiting on a subagent by
polling its output file for expected content will fire on a *partial* write. On this run the
orchestrator polled the plan for the last few deltas, saw them, measured 276 lines, and committed —
while the planner was still writing. The planner's own report then said 281 lines with 28 citations
and a 22-entry enumeration, and the disk agreed with the planner, not with the commit. The commit
captured a mid-write snapshot and a second commit was needed to capture the real output.

Poll only to decide whether to keep waiting. Commit, validate, or hand off a subagent's artifact
**only after its completion notification arrives**. If you must poll, re-measure everything after
the notification and diff the disk against what you committed; `git hash-object <file>` against
`git rev-parse HEAD:<file>` settles it in one command.

**A subagent's `.output` transcript path is NOT a liveness signal.** The harness names an
`output_file` per agent and warns against reading it, which invites using its size or mtime as a
proxy for progress. It is not one. On this run all four agent transcripts read **0 bytes**,
including the `task-researcher` and both `atomic-planner` instances that had already completed
successfully and returned full reports. Sampling the size twice, 45 seconds apart, showed no growth
for an agent that was running normally. Acting on that reading would have launched a duplicate
subagent against the same artifact. Always run the control — check the transcript size of an agent
you *know* finished — before treating a flat file as evidence of death. Wall-clock elapsed time
since the launch call is the more honest signal, and the only authoritative one is the completion
notification.

**Do not trust a subagent's self-reported counts.** The same planner reported the finished plan as
"256 lines"; it was 276, with the 20 enumeration entries it had itself described. The content was
correct and the metric was not. Re-measure any number that will land in a report or an acceptance
condition. Related: [[verify-subagent-capability-claims]],
[[reconcile-plan-numbers-against-your-own-measurements]].

**A dead child cannot be resumed in place and `SendMessage` is not on the orchestrator's tool
surface.** Relaunch `Agent(atomic-planner)` with an explicit resume brief that (a) names what is
already correct and must not be disturbed, and (b) lists only the outstanding corrections. Marking
the verified-good parts is what stops the fresh instance from rewriting them. See
[[no-sendmessage-relaunch-with-resume-brief]] and
[[agent-tool-cannot-course-correct-running-subagent]].
