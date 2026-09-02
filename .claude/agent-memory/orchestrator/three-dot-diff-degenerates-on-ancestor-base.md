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

This is distinct from [[stale-base-anchor-passes-ancestry-vacuously]], which is about an ancestry
*check* passing vacuously. Here the ancestry is real and the *diff operator itself* collapses.
Related: [[orchestrator-state-json-is-tracked-in-git]].
