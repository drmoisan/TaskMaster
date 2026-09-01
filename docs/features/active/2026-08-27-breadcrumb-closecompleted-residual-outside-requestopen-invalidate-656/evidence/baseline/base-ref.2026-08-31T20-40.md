# Baseline — Base Ref Resolution (Issue #656)

Timestamp: 2026-09-01T14-34
Task: [P0-T2]

Command:
```
git rev-parse 2b85134b42872e405602e6064e02dc9cda6c319b
git rev-parse HEAD
git rev-parse --abbrev-ref HEAD
```

EXIT_CODE: 0

Resolved values:

- Plan base ref `2b85134b42872e405602e6064e02dc9cda6c319b` resolves to the 40-character object id
  `2b85134b42872e405602e6064e02dc9cda6c319b`.
- HEAD: `119a89f017e0787e8aa62914333d0a5bc04576fb`
- Branch: `bug/breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656`

## Base-Ref Discrepancy (recorded, not silently adapted)

The plan pins all diff assertions to `2b85134b42872e405602e6064e02dc9cda6c319b`. That base is stale
relative to the current branch. Measured in this worktree at the timestamp above:

- `git merge-base 2b85134b42872e405602e6064e02dc9cda6c319b HEAD` returns
  `2b85134b42872e405602e6064e02dc9cda6c319b`, so the three-dot form degenerates to a two-dot diff
  against that commit.
- `git diff --name-only 2b85134b42872e405602e6064e02dc9cda6c319b...HEAD` lists **299** paths.
  Nine of those are under `QuickFiler/` or `QuickFiler.Test/`, and one of them is
  `QuickFiler.Test/QuickFiler.Test.csproj`. None of the nine is this item's work: the branch was
  reconciled against `origin/main` before execution began, and the pinned base predates that merge,
  so the diff conflates every change `main` gained in the interval with this item's change set.
- `origin/main` resolves to `5670b3cfe6a52e3b890bf80f0cd85a20d4fe4723` and is an ancestor of HEAD
  (`git merge-base --is-ancestor origin/main HEAD` exits 0).
- `git diff --name-only origin/main...HEAD` lists exactly ten paths: six under
  `.claude/agent-memory/`, plus this feature folder's `issue.md`, `plan.2026-08-31T20-10.md`,
  `spec.md`, and `research/2026-08-31T20-15-*.md`. No path under `QuickFiler/` or
  `QuickFiler.Test/`, and no build-configuration path.

Consequence: the footprint acceptance stated by P4-T11 through P4-T14 and by AC-10, AC-11 and AC-12
cannot be evaluated against the pinned base, because that base reports nine unrelated pre-existing
paths and one `.csproj`. The authoritative footprint base for this run is therefore `origin/main`,
per the execution directive for this run, and the footprint tasks record both measurements so the
substitution is auditable rather than silent.

Output Summary: All three commands exited 0. The pinned base ref resolves. The pinned base is stale
by 299 paths relative to HEAD and is not usable as the footprint baseline; `origin/main` is used for
the footprint criteria and both measurements are recorded in the Phase 4 footprint artifacts.
