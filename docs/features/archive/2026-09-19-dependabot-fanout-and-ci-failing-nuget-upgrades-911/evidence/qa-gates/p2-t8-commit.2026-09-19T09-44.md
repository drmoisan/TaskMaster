# P2-T8 — Batch A commit

Timestamp: 2026-09-19T15-52

Command:

```
git add .csharpierignore scripts/dependencies/PackageGraph.psm1 tests/scripts/dependencies/PackageGraph.Tests.ps1 "*.csproj" "*/packages.config" "*/app.config" .github/workflows/_build-analyzers.yml .github/workflows/_build-nullable.yml .github/workflows/_mstest-coverage.yml .github/workflows/_pester.yml docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/
git commit -F <message-file>
git rev-parse HEAD
git status --porcelain --untracked-files=all
git show --name-only --format= HEAD
```

EXIT_CODE: 0

## Head SHA

**`48f0c710a9a970587ab8b17956be224513c1f7fd`**

Commit summary line:

```
[bug/dependabot-fanout-and-ci-failing-nuget-upgrades-911 48f0c710] fix(deps): batch A — formatting scope, #898 analyzer realignment, #903 manifest entries, NuGet pin
 82 files changed, 4726 insertions(+), 6158 deletions(-)
```

## Porcelain after the commit, verbatim

```
```

Empty. The capture was taken immediately after the commit and **before** this task's own check-off
was written to the plan, which is the only order in which the clause is satisfiable: ticking
P2-T8 modifies the plan file, which the commit has just cleaned, so a capture taken afterwards
would list it and no commit could ever close the gap.

`coverage/` does not appear because `.gitignore:144` covers it and `--untracked-files=all` does not
list ignored paths. The clause "contains no entry outside `coverage/`" is therefore satisfied by an
empty capture rather than by one listing `coverage/` entries.

## Committed path set

`git show --name-only --format= HEAD` lists **82** paths. Partitioned against the pathspec set:

| Partition | Count |
|---|---|
| `*.csproj` | **15** |
| `*/packages.config` | **17** |
| `*/app.config` | **17** |
| `.github/workflows/` | **4** |
| `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/` | **26** |
| `.csharpierignore` | **1** |
| `scripts/dependencies/PackageGraph.psm1` | **1** |
| `tests/scripts/dependencies/PackageGraph.Tests.ps1` | **1** |
| **Total** | **82** |
| Paths outside the pathspec set | **0** |

The partition is exhaustive and sums to the listed total, so no path escaped classification.

### The manifest counts are 17 and 17, not 18 and 17

The pathspec names "the 18 `*/packages.config` and 17 `*/app.config` files", and 17 of the 18
appear in the commit. This is correct rather than a shortfall. `SVGControl/packages.config`
carried no wrapped `<package>` element, was already in canonical inline form before P1-T7 ran, and
was left byte-identical — so it has nothing to commit. P1-T7 recorded exactly this: 17 wrapped and
1 already inline for `packages.config`, 17 wrapped and 0 already inline for `app.config`, giving
expected changed counts of 17 and 17. The pathspec correctly names all 18 because it must not
exclude the file; the commit correctly contains 17 because git has nothing to record for the
eighteenth.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| `git status --porcelain --untracked-files=all` captured verbatim, no entry outside `coverage/` | empty capture | PASS |
| `git show --name-only --format= HEAD` lists only paths from the pathspec set | 0 paths outside it | PASS |
| Lists `scripts/dependencies/PackageGraph.psm1` | present | PASS |
| Lists `tests/scripts/dependencies/PackageGraph.Tests.ps1` | present | PASS |
| Does **not** list `scripts/vscode/Sync-PackageReferences.ps1` | absent | PASS |
| **Lists `evidence/qa-gates/p2-t7-coverage-projection.2026-09-19T09-44.jacoco.xml`** | present | PASS |
| **Lists `evidence/qa-gates/p2-t7-test-results.2026-09-19T09-44.summary.txt`** | present | PASS |
| Lists neither `scripts/vscode/Invoke-MSTest.ps1` nor `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | both absent | PASS |
| The ticked-task count in the execution copy of the plan is exactly 46 | **46** ticked, 82 unticked, 128 total | PASS |
| The head SHA differs from the value P0-T25 recorded | `48f0c710…` against `85f9a7b9…` | PASS |

### Ticked-count derivation

Counted in the execution copy of
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md`
immediately before the commit, by matching `^- \[[xX]\] \[P\d+-T\d+\]`:

```
TICKED=46  UNTICKED=82  TOTAL=128
```

Forty-six is every task preceding this one: **25** in Phase 0, **14** in Phase 1, **7** in Phase 2,
which is P0-T1 through P2-T7.

### The two `evidence/qa-gates/` listings

Both are asserted positively rather than left to the pathspec subset test, which is the point the
task text makes: a subset test over a pathspec set is satisfied by a commit from which the P2-T7
copies are simply absent, so without these two clauses Batch A could ship with no committed
coverage evidence and nothing would report it. P2-T7 recorded `TEST-RESULT-SUMMARY: produced`, so
the summary listing is required rather than conditional, and it is present.

### `Sync-PackageReferences.ps1` is deliberately absent

It is not in the pathspec and not in the commit. An earlier plan revision placed it here on a
prediction that the PoshQC formatter would rewrite it; P0-T15 measured 0 of 32 files rewritten and
P2-T1 measured 0 rewrites again across three passes, so no Batch A task modifies that file. It is
edited by P3-T4 and belongs to Batch B. Scope Decision 8 records the superseded prediction and why
it must not be reinstated.

## Line-ending normalisation note

`git add` emitted `LF will be replaced by CRLF the next time Git touches it` for the 26 new
feature-folder files and for the two new PowerShell files. That is `.gitattributes` normalising to
LF in the index and restoring CRLF on checkout; the working-tree bytes are unchanged by the commit
itself. It is recorded because a later checkout of these paths will change their on-disk hashes,
and P4-T1's format comparison must not read that as a formatter rewrite.

Output Summary: Batch A is committed at **`48f0c710a9a970587ab8b17956be224513c1f7fd`**, 82 files
changed, 4726 insertions and 6158 deletions. Every committed path is drawn from the task's pathspec
set with none outside it; the two new PowerShell files and both P2-T7 coverage-evidence copies are
present; `Sync-PackageReferences.ps1`, `Invoke-MSTest.ps1` and `Invoke-MSTestWithCoverage.ps1` are
absent. The post-commit porcelain is empty. The plan carried exactly 46 ticked tasks at commit
time, and the head SHA differs from the `85f9a7b9…` P0-T25 recorded.
