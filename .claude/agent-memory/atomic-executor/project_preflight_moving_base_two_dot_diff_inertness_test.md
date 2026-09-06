---
name: preflight-moving-base-two-dot-diff-inertness-test
description: A plan gate using `git diff origin/main -- <paths>` is only safe when origin/main has not touched those paths since the merge base; test inertness with `git diff --name-only <merge-base> origin/main -- <same paths>` before calling it blocking, and prefer making that test a second diff the executor runs and records under an INHERITED PATHS heading
metadata:
  type: project
---

A plan task that asserts "`git diff origin/main -- <file>` produces no output, proving the file is
unmodified" is comparing the WORKING TREE against a ref that keeps moving. If `origin/main` has
advanced past the branch's merge base, upstream edits to that file read as local edits, and the gate
fails for work the executor never did. Plans commonly add a guard of the form "if `git rev-parse
origin/main` differs from the value Phase 0 recorded, re-run against the merge base" — that guard
does not fire in the common case, because Phase 0 records the ref AFTER its own `git fetch`, so the
already-existing divergence is baked into the recorded value.

**Why:** the defect is conditional, not structural. The gate is wrong only if the upstream delta
intersects the paths the gate names, so a blanket "this form is wrong" finding over-reports and a
blanket acceptance under-reports.

**How to apply:** during preflight, resolve `git merge-base origin/main HEAD` and `git rev-parse
origin/main`; if they differ, run `git diff --name-only <merge-base> <origin/main> -- <exactly the
paths the gate names>`. An empty result means the gate is currently inert and equivalent to a
merge-base diff — report it as a non-blocking latent hazard with the measurement, not as a blocker.
A non-empty result makes the gate unsatisfiable and is blocking. Measured on #633, 2026-08-31:
merge base `9b6aff2e` vs `origin/main` `2b85134b` differed by 77 files including a third
`QuickFiler/` production file, but zero of the six in-scope paths, so two `git diff origin/main`
gates stayed correct. Relates to [[baseline-sha-diff-conflates-merged-base]] and
[[preflight-mergebase-diff-gates-need-commit-cadence]].

**Run the inertness test once per gate SCOPE, not once per plan.** The outcome is routinely mixed
within a single plan, so a single measurement generalised to the whole plan is wrong in one
direction or the other. Measured on #584, 2026-09-02, after an orchestrator merged `origin/main`
into the branch post-authoring: of five spans anchored to the plan's stale BASE `5ebaaf10`, the
three *per-file* spans (`-- UtilitiesCS/Threading/UiThread.cs`,
`-- UtilitiesCS/Threading/ProgressTrackerAsync.cs`) were inert because the merge never touched those
files; the *two-directory* span (`-- UtilitiesCS UtilitiesCS.Test`) picked up 18 merge-induced paths
but was still inert *for its own assertion*, because that gate greps added lines for seven timing
tokens and the merge delta contained none of them; and only the *unscoped* span
(`git diff --name-status <BASE>..HEAD`) was unsatisfiable, because its acceptance enumerates an
exact five-path set and the delta carried ~40 foreign source paths plus `.claude/agent-memory/**`,
which a second clause separately forbids. So the test has three tiers, not two: paths untouched,
paths touched but assertion-inert, and assertion-broken. Only the third blocks.

**Corollary — an assertion-inert directory span still degrades.** That same `-- UtilitiesCS
UtilitiesCS.Test` gate also required its diff to be *non-empty*, justified in the plan as "an empty
diff means the gate had nothing to inspect". Post-merge that clause is satisfied by foreign content
regardless of what the executor writes, so it stops proving the change exists. Report it as a
degraded-but-passing gate and fold the re-anchor into the delta; do not silently re-anchor it
yourself, per [[baseline-sha-diff-conflates-merged-base]].

## Better remedy: make the inertness test a task the executor runs

Do not settle for a preflight-time measurement plus a plan sentence asserting the result. The
preflight observation is about the tree at review time, and the branch can gain commits before the
task runs. Instead, have the plan carry BOTH diffs in the same task:

- `git diff --name-status <BASE_REF> -- <paths>` — two-dot, base commit against the WORKING TREE.
  This is what the scope gate is actually about. Pair it with `git add --intent-to-add` so a
  newly created file is visible, and with `git status --porcelain --untracked-files=all` as the
  independent untracked observation.
- `git diff --name-only <BASE_REF>..HEAD -- <the same paths>` — the commit range, recorded under a
  literal heading such as `INHERITED PATHS:`, with an acceptance clause requiring it to list no
  path. This isolates what the branch's own commits already changed under those paths.

The union the gate evaluates must be taken from the worktree diff and the porcelain span ONLY. Folding
the commit-range diff into the union makes a non-empty inherited list fail the scope clause spuriously.

**Why:** it converts an assertion about commit contents into an observation the executor makes at run
time, so it stays correct if the branch advances between planning and execution. It also survives a
planner session that has no git tool available and therefore cannot re-derive a commit-contents claim
at authoring time — a real constraint seen on #781, where the planner replaced exactly such a claim
with this construction rather than assert it unverified.

**How to apply:** both diffs carry a ref operand, so neither trips G8; the `git add` and
`git status --porcelain` spans in the same task exonerate both under G8b. Keep the `--` pathspec
separator on every invocation, or G8b reads the pathspec as a ref operand.

Do NOT use the three-dot `<BASE_REF>...HEAD` form for the scope diff. It compares two commits and
never reads the working tree, so in a plan that stages without committing it returns an empty list
however the executor edits those files.
