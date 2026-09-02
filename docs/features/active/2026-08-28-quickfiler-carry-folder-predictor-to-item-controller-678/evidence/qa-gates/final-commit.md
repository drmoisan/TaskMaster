# P2-T15 — Final commit and clean worktree

Timestamp: 2026-09-01T23-25

This is the last task of the plan. No evidence artifact is written after it.

## Commits on this branch

| # | Commit | Subject |
|---:|---|---|
| 1 | `8782db56e6db7d7ad174f8fb45e46d1e4f2172f0` | `fix(quickfiler): carry the initialised folder predictor to the item controller (#678)` — written by P1-T13; 35 files, 1623 insertions, 619 deletions |
| 2 | see below | `docs(issue-678): record Phase 0 baseline and Phase 2 QA evidence` — written by this task |

Commit 2 was created first without this artifact and without the P2-T15 check-off, then amended to
include both, as the task's acceptance conditions prescribe. Its final SHA is therefore the amended
one; the pre-amend SHA was `60dd60b0d1659fb2f2ecc41f38de305e6cd79b06`. Both commit messages reference
issue #678.

## Acceptance condition 1 — `git status --porcelain` after the commit

Run immediately after commit 2 and before this task's own check-off:

```
(no output)
```

**The output is empty.** The task's acceptance allows output consisting of paths under
`.claude/agent-memory/`, to be enumerated here with the reason they are left uncommitted.

**That enumeration is empty: this execution wrote nothing to `.claude/agent-memory/`.** The Phase 2
preamble states that writing agent memory is not required by this change and is not part of the
deliverable, and that the exclusion the plan grants that directory is a tolerance for incidental
session state rather than an invitation to write there. Nothing was written, so nothing is excluded,
and the clean-worktree result holds with no carve-out applied to it.

Two paths remained outside commit 2 at the moment that status was taken, exactly as the task
prescribes: this artifact, which did not yet exist, and the plan file, whose P2-T15 checkbox was not
yet set. Both are committed by the amend described above. After the amend,
`git status --porcelain` produces no output at all.

## Acceptance condition 2 — every Phase 0 and Phase 2 artifact path appears in the anchored diff

`git diff --name-only 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678`

### Phase 0 artifacts — 13 of 13 present

- `evidence/baseline/phase0-instructions-read.md`
- `evidence/baseline/minor-audit-integrity.md`
- `evidence/baseline/base-ref-anchor.md`
- `evidence/baseline/dotnet-tool-restore.md`
- `evidence/baseline/csharpier-check.md`
- `evidence/baseline/analyzer-build.md`
- `evidence/baseline/nullable-build.md`
- `evidence/baseline/mstest-coverage-run.md`
- `evidence/baseline/coverage-baseline.md`
- `evidence/baseline/coverage-baseline.jacoco.xml`
- `evidence/baseline/coverage-per-file-baseline.md`
- `evidence/baseline/file-size-census.md`
- `evidence/baseline/carrier-construction-sites.md`

### Phase 2 artifacts — 13 of 13 present

- `evidence/qa-gates/csharpier-format.md`
- `evidence/qa-gates/csharpier-check.md`
- `evidence/qa-gates/analyzer-build.md`
- `evidence/qa-gates/nullable-build.md`
- `evidence/qa-gates/mstest-coverage-run.md`
- `evidence/qa-gates/coverage-post-change.md`
- `evidence/qa-gates/coverage-delta.md`
- `evidence/qa-gates/exclude-attribute-invariant.md`
- `evidence/qa-gates/coverage-post-change.jacoco.xml`
- `evidence/qa-gates/file-size-audit.md`
- `evidence/qa-gates/scope-confinement.md`
- `evidence/qa-gates/final-toolchain-pass.md`
- `evidence/issue-updates/ac-verdicts.md`

This artifact, `evidence/qa-gates/final-commit.md`, is the fourteenth Phase 2 artifact and enters the
diff with the amend.

Phase 1's eleven artifacts are also present: `evidence/other/implementation-handoff.md`,
`compile-seam.md`, `carrier-chain.md`, `leg-a.md`, `leg-b.md`, `change-description.md`,
`out-of-scope-register.md`, `test-reconciliation.md`, `reduced-audit-handoff.md`, and
`evidence/regression-testing/ac16-red.md`, `ac16-green.md`, `ac9-negative-guard.md`,
`ac12-path-normalisation.md`.

The diff additionally lists `issue.md` (22 checkbox transitions), `plan.2026-08-31T21-12.md` (the
task checklist), and the research document, which does not exist at the base ref.

## Acceptance condition 3 — no path under `coverage/` appears in that list

A filter of the diff list for paths beginning `coverage/` returns **0**.

That is by construction rather than by omission: `coverage/*` is git-ignored at `.gitignore:144`, so
neither the baseline nor the post-change raw Cobertura report can be committed. Each side is
represented instead by a committed package-level JaCoCo summary,
`evidence/baseline/coverage-baseline.jacoco.xml` (44 lines) and
`evidence/qa-gates/coverage-post-change.jacoco.xml` (45 lines), whose `LINE` counter totals reproduce
the `lines-covered` and `lines-valid` values recorded on each side: 55001 / 64406 at baseline and
55083 / 64491 post-change. Both artifacts carry the `EVIDENCE_SUBSTITUTION:` record of the raw
report's measured line count and byte size in their companion `.md` files.

## Footprint of the two commits combined

| Prefix | Paths |
|---|---:|
| `QuickFiler/` | 16 |
| `QuickFiler.Test/` | 19 |
| `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/` | 43 |
| Anything else | **0** |

Total: 78 paths. The feature-folder count is 43 and not 42 because it includes this artifact, which
enters the diff with the amend that also commits it; the figure above is the post-amend measurement.

No path under `UtilitiesCS/`, `.claude/rules/`, `.claude/skills/` or the repository-root `CLAUDE.md`
appears in either commit. `artifacts/orchestration/orchestrator-state.json` was not written to and
does not appear. Full audit: `evidence/qa-gates/scope-confinement.md`.

## Not done, deliberately

The branch is **not pushed**, no pull request is opened, and nothing is merged. Those steps belong to
the orchestrator that owns this delegation.
