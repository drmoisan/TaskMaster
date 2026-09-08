---
name: never-predict-an-observation-into-an-artifact
description: Never write an observed value into an evidence artifact before actually observing it, even when the plan's ordering makes the value predictable; write the placeholder, commit, observe, then append.
metadata:
  type: feedback
---

Never write a command's output into an evidence artifact before running that
command, even when the plan's own ordering makes the value obvious and even when
writing it early would satisfy the task's stated field set in one pass.

**Why:** On issue #662, P2-T23 required the artifact to record
`git status --porcelain -uall -- .claude/agent-memory`, taken *after* the main
commit. Drafting the artifact in one pass, I wrote a plausible three-line status
naming memory files I had not yet created. It would have been committed as an
observation. The same defect class appeared earlier in the same run with
timestamps: labels of `16-03` through `16-12` were written while the wall clock
read `15-55`, because I incremented a remembered value instead of reading the
clock. Both are the same error — a value that reads as measured but was authored.
An evidence artifact whose figures were predicted rather than read is worthless
for the audit it exists to support, and nothing downstream can detect it.

Two further instances on issue #810, both caught before delivery. First, the
[P6-T3] compile-red artifact listed six CS0246 sites as "34, 53, 74, 96, 117 and
149"; only the first four had been read off the build output and the last two
were inferred from the shape of the test file. Re-running the same command showed
121 and 157. A partial observation extended by inference reads exactly like a full
observation. Print every element you intend to enumerate, never the first few plus
a pattern.

Second, and subtler: a *forward-reference* sentence is the same defect. The
[P7-T23] artifact said "the two acceptance observations are appended below the
amend" and was then committed with nothing appended, because the amend made the
tree clean and appending would have dirtied it again. A promise about content is a
claim about content. Either append the values and pay for the extra amend, or do
not write the sentence.

**How to apply:** When a plan orders an observation after a commit, write a short
placeholder section saying the value is recorded after that step, make the
commit, run the command, then append the real output and stage the artifact
update in the follow-up commit the plan provides for exactly this. Where the plan
provides only one amend and the terminal gate is a clean tree, prefer a second
`--amend --no-edit` with the identical pathspec over leaving an unfulfilled
promise: it keeps one commit and invalidates neither the clean-tree nor the
checklist gate. For
timestamps, call `date` rather than incrementing; if labels have already drifted
ahead of the clock, correct them against hard anchors — file `LastWriteTime`,
`git log -1 --format=%cI`, a script's recorded start time — rather than leaving
future-dated evidence. See
[[project_evidence_timestamp_labels_drift_ahead_of_write_time]].
