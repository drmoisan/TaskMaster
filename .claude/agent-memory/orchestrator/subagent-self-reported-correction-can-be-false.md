---
name: subagent-self-reported-correction-can-be-false
description: A subagent's "further defect found and fixed" can replace a TRUE statement with a FALSE one — verify a self-reported correction with the same rigor as a self-reported defect
metadata:
  type: feedback
---

On the issue #648 preparation run, `atomic-planner` reported, under "Further defect found and fixed", that
a sentence in the reviewer's mandated delta was false because
`artifacts/orchestration/orchestrator-state.json` "is untracked, since `.gitignore:57` ignores
`artifacts/`, and cannot appear in `git status --porcelain` output at any scope."

That correction was itself false, and it had already been written into the plan. Two commands settle it:

- `git ls-files --error-unmatch artifacts/orchestration/orchestrator-state.json` → prints the path, exits `0`
- `git check-ignore -v artifacts/orchestration/orchestrator-state.json` → prints nothing, exits `1`

**Why:** `.gitignore` governs UNTRACKED paths only and has no effect on a path already in the index. That
file was force-added by an unrelated commit, so it is a tracked file that `artifacts/` would otherwise have
ignored. Reasoning forward from a `.gitignore` line to "therefore untracked" is invalid whenever the path
may have been force-added. `git check-ignore` is itself index-aware unless `--no-index` is passed, which is
why it exits 1 here and why it is a usable discriminator.

Two corroborations were available for free and both were ignored by the subagent: the file had appeared as
` M artifacts/orchestration/orchestrator-state.json` in `git status --short` all run (an ignored untracked
path never appears in `git status` at all), and the preceding preflight round had independently recorded
that it "carries an uncommitted modification, so the working tree does not exactly match HEAD."

**How to apply:**
- Treat a subagent's *correction* with the same scepticism as its *findings*. A correction arrives framed
  as diligence, which is exactly what makes it easy to accept unchecked, and it lands directly in the
  artifact rather than in a report you would review.
- Re-derive any correction that flips a fact you previously measured yourself. Here the orchestrator had
  measured tracked-ness at the start of the run, so the contradiction was detectable.
- When returning the correction, hand the subagent the *commands and their exit codes*, not the conclusion,
  and require it to re-derive independently. The planner then confirmed it against the worktree's
  `.git/worktrees/<id>/index` with a negative control (`.claude/settings.local.json`, ignored and absent
  from the index) — a stronger derivation than the one it had gotten wrong.
- A subagent with no shell cannot verify a commit SHA. Do not ask it to assert one; the planner correctly
  declined to write an SHA it could not read.

Related: [[orchestrator-state-json-is-tracked-in-git]], [[feedback_verify_subagent_capability_claims]],
[[reconcile-plan-numbers-against-your-own-measurements]].
