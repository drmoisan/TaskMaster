# P9-T13 — Phase 9 commit

Timestamp: 2026-09-20T09-44

Commands:

```
git add -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/ tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1
git commit -F <message file>
git status --porcelain --untracked-files=all
git show --name-only --format= HEAD
```

EXIT_CODE: 0

Output Summary: 20 paths committed under two explicit pathspecs. Both permitted coverage-evidence
forms are in the commit. The working tree is clean afterwards.

Phase 9 head SHA: `655e6ec14696b6ad7b66e88231c4914ce276e972`

## The two permitted coverage-evidence forms, confirmed present in HEAD

```
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-coverage-projection.2026-09-19T09-44.jacoco.xml
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-test-results.2026-09-19T09-44.summary.txt
```

They are named explicitly because they are the delivered tree's committed coverage evidence. Without
them the plan would have stopped committing the prohibited collector document without committing the
permitted form in its place, which gate rule 12 states does not satisfy the section.

## `git show --name-only --format= HEAD` — 20 paths

```
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p8-t6-commit.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t1-poshqc-format.iter1.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t1-poshqc-format.iter2.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t10-file-size-audit.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t11-ac-status-summary.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t12-change-footprint.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t2-poshqc-analyze.iter1.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t2-poshqc-analyze.iter2.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t3-pester.iter1.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t4-csharpier-check.iter1.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t5-msbuild-analyzers.iter1.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t6-msbuild-nullable.iter1.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-coverage-projection.2026-09-19T09-44.jacoco.xml
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-mstest-coverage.iter1.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-test-results.2026-09-19T09-44.summary.txt
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t8-ac25-csharp-toolchain.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t9-coverage-reconciliation.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md
tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1
```

`tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` is in the commit because the
P9-T2 analyzer step required a correction to it. It is a spec `## Write Set` member, so it is inside
the declared footprint.

## `git status --porcelain --untracked-files=all`, verbatim

Empty. No entry anywhere, therefore none outside `coverage/`.

## Acceptance

| Clause | Required | Observed | Result |
|---|---|---|---|
| Porcelain captured verbatim and contains no entry outside `coverage/` | yes | output empty | PASS |
| `git show --name-only --format= HEAD` lists the coverage projection | yes | listed | PASS |
| `git show --name-only --format= HEAD` lists the test-result summary | yes | listed | PASS |
| Head SHA differs from the value P8-T6 recorded | yes | `655e6ec1...` differs from `8bc97a13d21a803f3b750ae82778488fd244729e` | PASS |
