---
name: three-dot-diff-degenerates-on-ancestor-base
description: A three-dot diff against a PINNED base gives no protection once that base is an ancestor of HEAD - it silently becomes a two-dot diff and bills sibling merges to your footprint
metadata:
  type: project
---

Verified on #656, 2026-09-01. A plan wrote every footprint gate as `<PINNED_SHA>...HEAD`, which reads
as the safe form. It is not safe when the pinned SHA is an **ancestor** of HEAD.

**Mechanism.** `A...B` means `merge-base(A,B)..B`. When `A` is an ancestor of `B`,
`merge-base(A,B) == A`, so `A...HEAD` is *identical* to `A..HEAD`. The three-dot form contributes
nothing. Any base-reconciliation merge you perform after the plan was written makes the pinned SHA an
ancestor, so the gate silently starts billing everything `main` gained in the interval to your change
set.

Measured on #656: `<PINNED>...HEAD` = **299 paths**, including 9 under `QuickFiler/`+`QuickFiler.Test/`
and one `.csproj`. `origin/main...HEAD` = **10**. Four acceptance gates asserting "exactly the single
line" and "both outputs empty" were unsatisfiable as written, and three footprint ACs would have
failed for files the change never touched.

**How to apply.**
- Measure a footprint against a *branch ref* that is a genuine divergence point, normally
  `origin/main...HEAD`. Do not measure against a SHA the plan pinned before your reconciliation merge.
- The tell is cheap: `git merge-base <PINNED> HEAD`. If it prints `<PINNED>` back, the three-dot form
  has degenerated and the gate is measuring the wrong thing.
- Record both measurements when substituting, and say which one is authoritative. Do not substitute
  silently; the plan text is still the plan of record.
- Re-measure the footprint yourself before asserting any footprint AC. A subagent's measurement can
  be taken at a different commit than the one you are certifying.

**Re-derive the anchor at DELEGATION time, not once per run (added #735, 2026-09-03).** Having this
memory did not prevent the error, because I applied it only to my own footprint gates. I briefed
`feature-review` with `a679cd08...HEAD` — correct when execution started — and then merged
`origin/main` a second time *before* the review ran. That merge made `a679cd08` an ancestor, so the
anchor I had already handed over degenerated. Measured: **184** paths under the stale anchor versus
**78** under the correct one, with 18 paths from two unrelated sibling items (#730, #733) that the
reviewer would have billed to my change.

The prompt text is frozen at the moment you send it; the repository is not. So:

- Run `git merge-base <ANCHOR> HEAD` in the same turn as the delegation, not earlier in the run.
- After ANY reconciliation merge, treat every anchor already quoted in a pending or future prompt as
  invalid until re-derived. A merge invalidates prompts you have already written.
- When main is fully merged into the branch, the simplest correct anchor for "what this PR shows" is
  the two-dot `git diff <origin/main SHA> HEAD`. Prefer it over a three-dot form whose safety depends
  on a divergence that your own merge just removed.
- Correcting this mid-flight is expensive: there is no tool to message a running subagent, so the
  correction starts a *second* agent. See [[agent-tool-cannot-course-correct-running-subagent]].

This is distinct from [[stale-base-anchor-passes-ancestry-vacuously]], which is about an ancestry
*check* passing vacuously. Here the ancestry is real and the *diff operator itself* collapses.
Related: [[orchestrator-state-json-is-tracked-in-git]].
