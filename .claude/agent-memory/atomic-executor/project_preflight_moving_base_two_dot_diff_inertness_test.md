---
name: preflight-moving-base-two-dot-diff-inertness-test
description: A plan gate using `git diff origin/main -- <paths>` is only safe when origin/main has not touched those paths since the merge base; test inertness with `git diff --name-only <merge-base> origin/main -- <same paths>` before calling it blocking
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
