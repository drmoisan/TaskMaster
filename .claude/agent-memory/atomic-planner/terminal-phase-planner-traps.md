---
name: terminal-phase-planner-traps
description: Three preflight findings that recur in the final phase of a plan — an unowned follow-up obligation, artifacts written after the clean-tree commit task, and a "plan-level clarification" that the spec already says
metadata:
  type: feedback
---

Three checks to run over the final phase of every plan before handing it to preflight.

**1. Every "a follow-up issue should carry it" sentence needs a task that files it.**
A latent-defect note in the preamble (analyzer skew, out-of-scope defect, re-attributed
half of an issue) states an obligation. If no task files the issue, the obligation is
never discharged and the defect is re-discovered instead of fixed. Add a `gh issue create`
task plus an `evidence/issue-updates/` mirror, and edit the preamble note so it names the
task ID that discharges it. `gh` is authenticated in this environment. When the epic
forbids children from writing under `docs/features/potential/**`, `gh issue create` is the
right instrument and `potential_to_issue` is wrong (it files a duplicate).

**Why:** #511 round 3 — the analyzer-note item 6 said a follow-up issue "should carry"
the 16-project `Analyzer`/`packages.config` realignment, and nothing filed it.

**2. Any task that writes an artifact AFTER the commit task needs a second commit task.**
The commit task's acceptance is normally "`git status --porcelain` produces zero output
lines". A review-handoff index or issue mirror written after it leaves the plan's terminal
state as a worktree with untracked evidence. The handoff task usually cannot move ahead of
the commit, because its acceptance cites the head sha that commit produced. Append a final
commit task with explicit pathspecs and re-assert both the clean tree and the scope lock.
Markdown under the feature folder does not enter a `.cs`/`.csproj`-filtered scope-lock set.

**Why:** repo standing practice is that all audit-trail evidence is committed and the work
is not done until `git status` is clean. Related: [[diff-gates-need-a-commit-task]].

**3. Never write "plan-level clarification recorded against the spec's wording" without
reading the spec sentence.** A false deviation claim invites a feature reviewer to score a
spec deviation that was never made. Read the AC line, and if the carve-out or condition is
already there verbatim, say "quoted from spec AC N" instead.

**Why:** #511 P6-T11 claimed it was adding the `.claude/agent-memory/` carve-out; spec.md
already carried it. Related: [[agent-memory-is-tracked-scope-git-gates]].

**How to apply:** run all three as a sweep over the last phase after the plan is otherwise
final; all three are appends or a one-sentence in-place replacement, so they never force a
renumber.
