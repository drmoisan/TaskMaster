---
name: no-sendmessage-relaunch-with-resume-brief
description: The orchestrator has no SendMessage tool in this repo, so a subagent that stops mid-plan cannot be resumed in place - launch a fresh one with a reconstructed resume brief, and never use a placeholder Agent prompt
metadata:
  type: feedback
---

`SendMessage` is **not** in this orchestrator's tool set, even though the `Agent` tool's own
description tells you to use it to continue a previously spawned agent. When a delegated
`atomic-executor` stops mid-plan on an authorized reporting branch, you cannot resume it. Launch a
**fresh** agent with a reconstructed resume brief.

**Why:** the stopped agent holds context a new one lacks — which tasks it completed, which evidence
it authored, what it already measured, and any deviation it applied. A resume prompt that omits that
state makes the new agent either redo completed work (destroying evidence and re-dirtying gates that
already passed) or, worse, restart a QA loop whose green pass is a precondition of a later task.

**How to apply:**

1. Before launching, read the plan's checkbox state on disk. It is authoritative and is the cheapest
   reconstruction of "what is done" — a stopped executor checks tasks off as it goes.
2. Read the evidence artifact for the task that stopped. It records the diagnostic in the executor's
   own words and is what you must tell the successor not to re-derive.
3. Run `git status` and explain every entry in the resume brief: which staged/unstaged hunks are the
   predecessor's legitimate work, and which are unrelated dirt from sibling runs. Otherwise the
   successor "cleans" the predecessor's work.
4. Restate every deviation the predecessor applied (substituted diff anchors, corrected line
   citations) so the successor stays consistent with artifacts already on disk.
5. Independently re-verify the predecessor's blocking claim before authorizing a re-run. An
   environmental diagnosis is checkable: re-take the process census and the load figures yourself.

**Never issue an `Agent` call with a placeholder prompt** intending to fill it in later. There is no
later — the agent starts immediately with whatever prompt you gave it. A fork inheriting your full
context and an empty directive is a live agent in your worktree with no task. It cannot be cancelled;
you have to wait for it to return before doing anything else, or risk two agents interleaving writes.

See [[one-executor-per-worktree]] and [[agent-tool-cannot-course-correct-running-subagent]].
