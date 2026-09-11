---
name: three-dot-degenerates-when-base-is-ancestor
description: three-dot diff notation gives NO protection once the supplied left ref is an ancestor of HEAD — it silently becomes two-dot and sweeps in sibling items merged from main; always re-derive the base with git merge-base HEAD origin/main
metadata:
  type: project
---

Three-dot notation only cancels content when the two refs have **diverged**. If the supplied left
operand is an ancestor of `HEAD` — which is exactly what happens after the branch merges
`origin/main` a second time — then `merge-base(left, HEAD) == left`, so `left...HEAD` degenerates
into `left..HEAD` and includes everything `HEAD` gained from `main` in the interim.

**Why:** measured on #735 (2026-09-03). The caller supplied
`a679cd08...HEAD`; `a679cd08` was the merge base before the second `origin/main` merge
(`30e66833`). Result: **184 changed paths** including 18 under `.github/`, `Directory.Build.props`,
`scripts/vscode/` and `tests/scripts/vscode/` that belonged to sibling parallel items #730 and #733.
Re-derived correctly against `git merge-base HEAD origin/main` (= `b13d5b7b`, also the `origin/main`
tip): **78 paths**, zero of them siblings'. A workflow-file finding raised off the bad anchor would
have been a costly false positive, because modified `.github/workflows/**` triggers a separate
green-run policy rule.

**How to apply:** never accept a caller-supplied SHA as the anchor, in either notation. Always run
`git rev-parse origin/main` and `git merge-base HEAD origin/main` first and diff against the result.
When they are equal, two-dot and three-dot are identical and either is correct. Record the resolution
and the measured path-count delta in `policy-audit` under `## Scope Resolution`, and state the exact
command in `code-review` so a later reader can tell the boundary was measured, not assumed.

This does **not** contradict [[epic-child-twodot-diff-divergence-noise]]: three-dot is right when the
refs have genuinely diverged (epic child vs an advancing integration branch), and inert when they
have not. The invariant that covers both is "re-derive the merge base," per
[[stale-caller-merge-base]].

Corollary worth checking each time: an anchor correction that REMOVES paths is not scope narrowing
if the removed paths are provably not the branch's own work. Verify with a single
`git diff --name-only <true-base> HEAD | grep -E '<the disputed prefixes>'` returning empty before
withdrawing any finding — per [[verify-the-callers-factual-correction]].
