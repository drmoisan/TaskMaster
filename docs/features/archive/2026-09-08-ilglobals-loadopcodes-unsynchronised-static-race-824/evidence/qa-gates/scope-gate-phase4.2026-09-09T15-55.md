# Phase 4 D6 Owned Write Set scope gate (Issue #824, task P4-T8)

Timestamp: 2026-09-09T15-55

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $b = git merge-base HEAD origin/main; git add -N .; git diff --name-status $b; Write-Output "---"; git status --porcelain --untracked-files=all'`, issued with the name-status diff anchored on `HEAD` per the adaptation recorded in `evidence/other/executor-deviations.2026-09-09T15-28.md`, with the merge-base form run alongside it and its path count recorded.

EXIT_CODE: 0

## Anchor disposition

| Anchor | Paths listed |
|---|---|
| `git diff --name-status HEAD` | 36 |
| `git diff --name-status <merge-base>` | 339 |

The gate is judged on the `HEAD`-anchored listing, which is this run's own footprint. The merge-base
listing adds the 304 paths of already-merged sibling work that P0-T15 enumerated, minus overlaps with
this run's 36, and no acceptance condition in this plan can distinguish an inherited path in it from
one this run introduced. The two counts are recorded together so the difference is visible rather
than implied: 339 = 304 inherited + 36 from this run − 1 path appearing in both, the plan file, which
the epic committed and this run has since modified.

## Classification of every path in the `HEAD`-anchored listing

All 36 paths satisfy a D6 class. No path lies outside the three classes, so there is no scope breach
and nothing to revert.

### D6 class 1 — the two modified source files named in Scope (2 paths)

| Status | Path |
|---|---|
| M | `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` |
| M | `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` |

These are exactly the two files the plan's Scope section names as modified. No third source file was
touched.

### D6 class 2 — under the #824 feature folder (34 paths)

One modified:

| Status | Path |
|---|---|
| M | `.../plan.2026-09-08T23-51.md` |

Thirty-three added, all under `.../evidence/`:

- `evidence/baseline/` (15): `base-anchor.2026-09-09T14-58.md`,
  `base-inertness.2026-09-09T15-19.md`, `coverage-baseline.2026-09-09T15-14.md`,
  `coverage-classes-baseline.2026-09-09T15-16.md`, `coverage-contingency.2026-09-09T15-15.md`,
  `csharpier-check-baseline.2026-09-09T15-06.md`, `file-line-counts-baseline.2026-09-09T15-17.md`,
  `msbuild-analyzers-baseline.2026-09-09T15-08.md`,
  `msbuild-nullable-baseline.2026-09-09T15-09.md`, `phase0-instructions-read.md`,
  `toolchain-analyzer-paths.2026-09-09T15-03.md`,
  `toolchain-dotnet-coverage.2026-09-09T15-05.md`, `toolchain-restore.2026-09-09T15-00.md`,
  `toolchain-sdk.2026-09-09T15-02.md`, `toolchain-tool-restore.2026-09-09T15-04.md`
- `evidence/other/` (3): `executor-deviations.2026-09-09T15-28.md`,
  `p2t2-line-range-record.2026-09-09T15-34.md`, `phase3-completion-notes.2026-09-09T15-42.md`
- `evidence/qa-gates/` (7): `ac1-assignment-sweep.2026-09-09T15-46.md`,
  `ac5-loadopcodes-retained.2026-09-09T15-48.md`, `ac6-nullable-and-comment.2026-09-09T15-49.md`,
  `ac7-test-rework.2026-09-09T15-50.md`, `ac8-parallelism-preserved.2026-09-09T15-51.md`,
  `ac9-no-synchronisation-primitive.2026-09-09T15-52.md`,
  `ac10-build-file-discipline.2026-09-09T15-53.md`
- `evidence/regression-testing/` (8): `ac2-fail-before.2026-09-09T15-25.md`,
  `ac3-fail-before.2026-09-09T15-30.md`, `ac2-ac3-pass-after.2026-09-09T15-38.md`,
  `build-p1t2.2026-09-09T15-24.md`, `build-p1t5.2026-09-09T15-29.md`,
  `build-p2t4.2026-09-09T15-36.md`, `build-p3t4.2026-09-09T15-43.md`,
  `ilglobals-tests-after-rework.2026-09-09T15-44.md`

Every evidence path uses one of the six canonical kinds. No artifact was written under
`evidence/coverage/` or under any path beginning `artifacts/`.

### D6 class 3 — under `.claude/agent-memory/` (0 paths)

No agent-memory write has been made at this point in the run. P6-T15 handles that path if any
appears later.

## Porcelain listing

The `git status --porcelain --untracked-files=all` output enumerates the same 36 paths with the same
statuses. It is the companion observation that covers newly created files, which an anchored
name-listing diff cannot see on its own; `git add -N .` was run first so the additions appear as `A`
in both listings rather than only in porcelain.

Git emitted a `LF will be replaced by CRLF` warning for each newly added markdown artifact. That is
the repository's configured end-of-line normalisation applying to new files, not a modification this
run made to any existing file, and it does not change any path's classification.

## Result

Every path in both listings satisfies one of the three D6 classes. The scope gate passes.
