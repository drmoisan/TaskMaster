# [P0-T15] Phase 0 Evidence Commit

Timestamp: 2026-09-08T09-31
Command: `git add docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence`; `git commit -m "evidence(810): phase 0 baseline" -- docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence`; then the same `git add` and `git commit --amend --no-edit -- docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence` to fold this artifact into the same commit
EXIT_CODE: 0
Output Summary: The first commit recorded the fourteen artifacts of [P0-T1] through [P0-T14]. The amend folded this fifteenth artifact into the same commit, so the Phase 0 evidence is a single commit and no artifact is left uncommitted.

COMMITTED-FILES: 15

## First commit

```
[bug/quickfiler-teardown-and-dropdown-residuals-810 65ff7826] evidence(810): phase 0 baseline
 14 files changed, 476 insertions(+)
```

The fourteen created paths were the [P0-T1] policy-read artifact and the [P0-T2] through [P0-T14] artifacts.

## Record-then-amend shape

The amend exists because an artifact written after its own commit would otherwise be left uncommitted. This file is written after the first commit and then folded into it, so the count asserted below is fifteen rather than fourteen.

## Post-amend verification

The `COMMITTED-FILES: 15` value is verified against `git show --name-only --format= HEAD` run after the amend. That command printed exactly fifteen paths:

```
evidence/baseline/p0-t10-msbuild-nullable.md
evidence/baseline/p0-t11-quickfiler-tests.md
evidence/baseline/p0-t12-coverage.md
evidence/baseline/p0-t13-line-counts.md
evidence/baseline/p0-t14-ac2-fence-baseline.md
evidence/baseline/p0-t15-baseline-commit.md
evidence/baseline/p0-t2-branch-and-base.md
evidence/baseline/p0-t3-dotnet-sdk.md
evidence/baseline/p0-t4-nuget-restore.md
evidence/baseline/p0-t5-dotnet-tool-restore.md
evidence/baseline/p0-t6-dotnet-coverage.md
evidence/baseline/p0-t7-vstest-resolution.md
evidence/baseline/p0-t8-csharpier-check.md
evidence/baseline/p0-t9-msbuild-analyzers.md
evidence/baseline/phase0-instructions-read.md
```

The common prefix `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/` is elided from each line above for readability; the command printed each path in full. The amend that folded this transcription into the commit is a second `git commit --amend --no-edit` carrying the same pathspec, so the Phase 0 evidence remains one commit.

## D7 compliance

Both commits carry an explicit `--` pathspec operand naming the feature-folder evidence directory. Neither is a zero-pathspec commit.
