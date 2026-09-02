# P2-T15 — Final commit and clean worktree, remediation cycle 1

Timestamp: 2026-09-02T01-47

This is the last task of the plan. No evidence artifact is written after it.

## Commits this cycle made

| # | SHA | Subject |
|---|---|---|
| 1 | `be1e0b97` | `fix(quickfiler): reconcile leg A carriers, align projection, observe cancel (#678)` — P1-T14, the production, test and Phase 1 evidence changes |
| 2 | see note below | `docs(issue-678): record remediation cycle 1 QC evidence and close the plan` — P2-T15, the Phase 2 QC evidence plus the CSharpier reflow of two files |

Both messages name issue #678 and this remediation cycle. Neither commit was pushed; no PR
was opened and no merge was performed.

**Note on the second commit's SHA.** This clause cannot name it, and the omission is
structural rather than an oversight. The task requires this artifact and the plan file to be
committed by an **amend** performed after this task's check-off is written. An amend replaces
the commit object, so any SHA written into this artifact before the amend is invalidated by
the amend that commits the artifact. Writing one would state a fact that is false in the very
commit that carries it.

The commit is identified here by its subject line, which the amend preserves, and by its
parent `be1e0b97`. Its post-amend SHA is reported by the executor outside the commit, where a
self-reference is not required, and is recoverable at any time with `git rev-parse HEAD` on
this branch or with `git log --oneline -1`.

This is the same fixpoint class recorded at P2-T13, where correcting an artifact's timestamp
rewrites the mtime the correction is measured against. It is recorded rather than worked
around.

## Clause 1 — clean worktree

`git status --porcelain`, run after the commit, produced **no output at all**.

- No path under `.claude/agent-memory/` is left uncommitted, because **this executor wrote
  nothing to that directory**. The clause permits such paths to be left uncommitted and
  enumerated here with a reason; the enumerated set is empty.
- This artifact and the plan file are committed by an amend after this task's check-off is
  written, exactly as the clause provides.

## Clause 2 — every artifact path named in Phase 0, Phase 1 and Phase 2 is in the diff

Command:

```
git diff --name-only 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678
```

The diff lists **85** paths under the feature folder, **77** of them under `evidence/`.

Every one of the **37** artifact paths this plan names across its three phases was checked
against that list. **Missing: 0.**

The 37, by phase:

- **Phase 0 (12)** — all under `evidence/remediation-baseline/`:
  `phase0-instructions-read.md`, `base-ref-anchor.md`, `issue-ac-preimage.md`,
  `dotnet-tool-restore.md`, `csharpier-check.md`, `analyzer-build.md`, `nullable-build.md`,
  `mstest-coverage-run.md`, `coverage-baseline.md`, `coverage-per-file-baseline.md`,
  `file-size-census.md`, `qa-gates-timestamp-preimage.md`
- **Phase 1 (11)** — `evidence/regression-testing/`: `r1-test-added.md`, `r1-red.md`,
  `r1-green.md`, `r2-r3-tests-added.md`, `r2-r3-red.md`, `r2-r3-green.md`; `evidence/other/`:
  `r1-reconciliation.md`, `r2-projection-alignment.md`, `r2-decision.md`,
  `r3-cancellation-observation.md`, `r4-timestamp-correction.md`
- **Phase 2 (14)** — `evidence/issue-updates/remediation-ac-invariant.md`; `evidence/qa-gates/`:
  `remediation-csharpier-format.md`, `remediation-csharpier-check.md`,
  `remediation-analyzer-build.md`, `remediation-nullable-build.md`,
  `remediation-mstest-coverage-run.md`, `remediation-coverage-post-change.md`,
  `remediation-coverage-delta.md`, `remediation-exclude-attribute-invariant.md`,
  `remediation-file-size-audit.md`, `remediation-scope-confinement.md`,
  `remediation-doc-token-check.md`, `remediation-timestamp-fidelity.md`,
  `remediation-final-toolchain-pass.md`

This artifact, `remediation-final-commit.md`, is the 38th and is committed by the amend.

## Clause 3 — no `coverage/` or `TestResults/` path in the diff

Filtering the unscoped
`git diff --name-only 807fb0bb6e5e49f43efa6b256b05960bf078ca19` for paths beginning
`coverage/` or `TestResults/` returned **no matches**. Both trees are git-ignored and neither
raw nor post-processed coverage report, and no TRX, was ever committed.

## Clause 4 — the R4 correction is proved to have reached the branch

Each of the twelve corrected Markdown artifacts was read back out of the commit with
`git show HEAD:` followed by its path, and its `Timestamp:` value compared against the
corrected value tabulated in `evidence/other/r4-timestamp-correction.md`. **Twelve equalities,
twelve holding.**

| # | Artifact | Tabulated corrected value | Value read out of the commit | Equal |
|---|---|---|---|---|
| 1 | `analyzer-build.md` | `2026-09-01T22-43` | `2026-09-01T22-43` | yes |
| 2 | `coverage-delta.md` | `2026-09-01T23-17` | `2026-09-01T23-17` | yes |
| 3 | `coverage-post-change.md` | `2026-09-01T23-17` | `2026-09-01T23-17` | yes |
| 4 | `csharpier-check.md` | `2026-09-01T22-42` | `2026-09-01T22-42` | yes |
| 5 | `csharpier-format.md` | `2026-09-01T22-42` | `2026-09-01T22-42` | yes |
| 6 | `exclude-attribute-invariant.md` | `2026-09-01T23-18` | `2026-09-01T23-18` | yes |
| 7 | `file-size-audit.md` | `2026-09-01T23-19` | `2026-09-01T23-19` | yes |
| 8 | `final-commit.md` | `2026-09-01T23-25` | `2026-09-01T23-25` | yes |
| 9 | `final-toolchain-pass.md` | `2026-09-01T23-20` | `2026-09-01T23-20` | yes |
| 10 | `mstest-coverage-run.md` | `2026-09-01T23-03` | `2026-09-01T23-03` | yes |
| 11 | `nullable-build.md` | `2026-09-01T22-43` | `2026-09-01T22-43` | yes |
| 12 | `scope-confinement.md` | `2026-09-01T23-20` | `2026-09-01T23-20` | yes |

All twelve are under
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/`.

The read-back is used **instead of** a base-ref-anchored `--name-status` diff, which would
report these artifacts as **added** rather than modified, because they did not exist at
`807fb0bb6e5e49f43efa6b256b05960bf078ca19`, and would therefore say nothing about whether the
correction landed.

## Output Summary

Two commits: `be1e0b97` (production, tests, Phase 1 evidence) and its child (Phase 2 QC
evidence and the format reflow, SHA not self-referenceable because this artifact is committed
by the amend that would fix it), both naming issue #678 and this remediation cycle. The
worktree is clean with no `.claude/agent-memory/` residue. All 37 named artifact paths appear
in the base-anchored feature-folder diff, missing 0. No `coverage/` or `TestResults/` path is
committed. The R4 correction is confirmed on the branch by twelve read-back equalities out of
twelve.
