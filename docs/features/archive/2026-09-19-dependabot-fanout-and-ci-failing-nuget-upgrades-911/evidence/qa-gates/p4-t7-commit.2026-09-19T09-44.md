# P4-T7 — Batch B commit

Timestamp: 2026-09-20T01-24

Commands:

```
git -C <W> add -- "scripts/dependencies/PackageCompatibility.psm1" "scripts/vscode/Sync-PackageReferences.ps1" "tests/scripts/dependencies/PackageCompatibility.Tests.ps1" "tests/scripts/dependencies/DependabotConfig.Tests.ps1" "tests/scripts/vscode/Sync-PackageReferences.Tests.ps1" ".github/dependabot.yml" "docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/"
git -C <W> commit -F -
git -C <W> rev-parse HEAD
git -C <W> show --name-only --format= HEAD
git -C <W> status --porcelain --untracked-files=all
```

EXIT_CODE: 0

## Head SHA

```
596e7a70c78443861576f21a572bd2a919f02c66
```

Short form `596e7a70`. The commit reports `26 files changed, 3511 insertions(+), 207 deletions(-)`.

It differs from the value P2-T8 recorded, `48f0c710a9a970587ab8b17956be224513c1f7fd`, which was
also `HEAD` immediately before this commit.

## Ticked-task count at the moment of commit

```
TICKED=64
UNTICKED=64
TOTAL=128
```

**Exactly 64**, which is every task preceding this one: P0-T1 through P4-T6, across phases of 25,
14, 9, 10 and 6. The total of 128 matches the plan's Task Count field, so no task line was lost or
duplicated.

## `git show --name-only --format= HEAD`, all 26 paths

| # | Path | In pathspec set as |
|---|---|---|
| 1 | `.github/dependabot.yml` | named explicitly |
| 2 | `scripts/dependencies/PackageCompatibility.psm1` | named explicitly |
| 3 | `scripts/vscode/Sync-PackageReferences.ps1` | named explicitly |
| 4 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | named explicitly |
| 5 | `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` | named explicitly |
| 6 | `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | named explicitly |
| 7 | `docs/features/.../plan.2026-09-19T09-44.md` | under the feature folder |
| 8 | `docs/features/.../spec.md` | under the feature folder |
| 9 | `docs/features/.../evidence/other/p2-t9-batch-a-boundary.2026-09-19T09-44.md` | under the feature folder |
| 10 | `docs/features/.../evidence/qa-gates/p2-t8-commit.2026-09-19T09-44.md` | under the feature folder |
| 11-26 | the 16 `evidence/qa-gates/p3-t*` and `p4-t*` artifacts this run wrote | under the feature folder |

Every listed path is drawn from the pathspec set. **Nothing outside it appears.**

Two of the entries are the Batch A close-out artifacts `p2-t8-commit` and
`p2-t9-batch-a-boundary`, which were deliberately left uncommitted because no pathspec authorised
a commit between P2-T8 and this task. They land here, which is the first commit whose pathspec
covers them.

`scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` are **not**
listed, as the acceptance requires. Neither was modified at any point in Batch B: both are absent
from P4-T1's hash-difference set in both rounds.

## Post-commit porcelain

```
$ git status --porcelain --untracked-files=all
<no output>
```

**Empty.** No entry at all, and therefore in particular no entry outside `coverage/`. The
`coverage/` tree holds this phase's four JaCoCo documents and is ignored by `.gitignore:144`.

This capture was taken immediately after the commit and before this artifact was written; writing
this file makes porcelain non-empty again, which P4-T8 records as expected state.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `git show --name-only --format= HEAD` lists only pathspec-set paths | all 26 | all 26 accounted for above | PASS |
| Lists neither `scripts/vscode/Invoke-MSTest.ps1` nor `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | absent | both absent | PASS |
| `git status --porcelain --untracked-files=all` contains no entry outside `coverage/` | none | empty | PASS |
| Ticked-task count in the execution copy of the plan | exactly 64 | 64 | PASS |
| Head SHA differs from the value P2-T8 recorded | differs | `596e7a70…` against `48f0c710a…` | PASS |

Output Summary: Batch B is committed at **`596e7a70c78443861576f21a572bd2a919f02c66`**, 26 files
changed with 3511 insertions and 207 deletions. `git show --name-only` lists 26 paths, every one
drawn from the declared pathspec set, and lists neither `Invoke-MSTest.ps1` nor
`Invoke-MSTestWithCoverage.ps1`. The two Batch A close-out artifacts that no earlier pathspec
authorised are included. `git status --porcelain --untracked-files=all` is empty immediately after
the commit. The execution copy of the plan carried exactly **64** ticked tasks of 128 at the
moment of commit, which is every task preceding this one. The head SHA differs from the Batch A
head `48f0c710a9a970587ab8b17956be224513c1f7fd`.
