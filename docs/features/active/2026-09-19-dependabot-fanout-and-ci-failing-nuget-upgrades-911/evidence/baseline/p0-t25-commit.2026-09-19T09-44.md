# P0-T25 — Phase 0 Evidence Commit

Timestamp: 2026-09-19T23-19

Commands:

```
git add -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/
git commit -m "docs(911): Phase 0 baselines P0-T11 through P0-T24" -m "<attribution trailer>"
git status --porcelain --untracked-files=all
git rev-parse HEAD
```

The `git add` pathspec is explicit and limited to
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`. No other
path was staged.

EXIT_CODE: 0

## Resulting head SHA

```
85f9a7b9e86f4f83df28bc04aaad144cc1d4d14c
```

| Reference | Value |
|---|---|
| Head SHA recorded by P0-T1 | `8b0afe2c48060804ded103db62a4c3e5eceef8f9` |
| Head SHA before this commit | `1ed87d668bd9680c03de16629a674cee48b54719` |
| **Head SHA after this commit** | **`85f9a7b9e86f4f83df28bc04aaad144cc1d4d14c`** |

The recorded head SHA **differs** from the value P0-T1 recorded.

## Commit contents

15 files changed, 1547 insertions, 14 deletions.

14 files created:

```
evidence/baseline/p0-t11-ac6-cold-analyzer-build-red.2026-09-19T09-44.md
evidence/baseline/p0-t12-nullable-build.2026-09-19T09-44.md
evidence/baseline/p0-t13-csharpier-check.2026-09-19T09-44.md
evidence/baseline/p0-t14-mstest-coverage.2026-09-19T09-44.md
evidence/baseline/p0-t15-poshqc-format.2026-09-19T09-44.md
evidence/baseline/p0-t16-format-revert.2026-09-19T09-44.md
evidence/baseline/p0-t17-poshqc-analyze.2026-09-19T09-44.md
evidence/baseline/p0-t18-pester.2026-09-19T09-44.md
evidence/baseline/p0-t19-analyzer-census.2026-09-19T09-44.md
evidence/baseline/p0-t20-manifest-census.2026-09-19T09-44.md
evidence/baseline/p0-t21-format-and-nuget-census.2026-09-19T09-44.md
evidence/baseline/p0-t22-dependabot-census.2026-09-19T09-44.md
evidence/baseline/p0-t23-pester-scope-census.2026-09-19T09-44.md
evidence/other/p0-t24-plan-sync-verification.2026-09-19T09-44.md
```

1 file modified: `plan.2026-09-19T09-44.md`, carrying the check-offs for P0-T11 through P0-T24.

All paths are under the feature folder. No `.xml`, `.trx`, `.coverage` or `.log` artifact is in the
commit; every collector and build document produced during Phase 0 stayed in `coverage/`, which
`.gitignore:144` ignores, per gate rule 12.

## Porcelain captured verbatim after the commit

```
git status --porcelain --untracked-files=all
```

produced **no output**. The capture is empty.

### Type condition, per gate rule 9

No entry in the capture matches `*.cs`, `*.csproj`, `*.sln`, `packages.config` or `app.config`.

The capture being empty makes that condition hold trivially, so it is recorded together with the
positive evidence that gives it content, rather than on its own:

- The pre-commit capture, taken immediately before `git add`, listed **15** entries — 14 untracked
  evidence artifacts and the modified plan file — and **none** of the 15 matched any of the five
  prohibited patterns. That capture is the non-vacuous form of the same assertion.
- All 15 are accounted for in the commit above, which is why the post-commit capture is empty.

Pre-commit capture, verbatim:

```
 M docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t11-ac6-cold-analyzer-build-red.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t12-nullable-build.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t13-csharpier-check.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t14-mstest-coverage.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t15-poshqc-format.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t16-format-revert.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t17-poshqc-analyze.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t18-pester.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t19-analyzer-census.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t20-manifest-census.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t21-format-and-nuget-census.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t22-dependabot-census.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t23-pester-scope-census.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/other/p0-t24-plan-sync-verification.2026-09-19T09-44.md
```

### Observation on the plan's expectation that the post-commit porcelain be non-empty

The task text states that "Phase 0 artifacts and the generated `coverage/` logs make it non-empty by
construction", and that is why no empty-porcelain assertion is placed here. The measured capture is
nevertheless empty, for a reason the task text did not anticipate: `coverage/` is ignored at
`.gitignore:144`, so `analyzers.msbuild.log`, `nullable.msbuild.log`,
`p0-t18-pester-coverage.xml`, `coverage.cobertura.xml` and the trx never appear in a porcelain
capture at all, and every Phase 0 artifact was committed by this task. Recorded as an observation.
No acceptance condition depends on it: the task asserts a type condition and a changed head SHA,
never a non-empty capture, so an empty one does not weaken either.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `git add` pathspec limited to the feature folder | required | explicit, single pathspec | PASS |
| Porcelain captured verbatim | required | captured, empty; pre-commit capture also recorded | PASS |
| No entry matches `*.cs`, `*.csproj`, `*.sln`, `packages.config`, `app.config` | required | holds for both captures; the 15-entry pre-commit capture is the non-vacuous form | PASS |
| Recorded head SHA differs from P0-T1's | required | `85f9a7b9…` against `8b0afe2c…` | PASS |

Output Summary: the 14 Phase 0 evidence artifacts and the updated plan file were committed under an
explicit pathspec limited to the feature folder, producing head SHA
**`85f9a7b9e86f4f83df28bc04aaad144cc1d4d14c`**, which differs from the `8b0afe2c48060804ded103db62a4c3e5eceef8f9`
P0-T1 recorded. 15 files changed, 1547 insertions, 14 deletions. The post-commit
`git status --porcelain --untracked-files=all` capture is empty; the 15-entry pre-commit capture is
recorded alongside it and contains no `*.cs`, `*.csproj`, `*.sln`, `packages.config` or `app.config`
entry. No collector or build document entered the commit; all remain under the gitignored
`coverage/`.
