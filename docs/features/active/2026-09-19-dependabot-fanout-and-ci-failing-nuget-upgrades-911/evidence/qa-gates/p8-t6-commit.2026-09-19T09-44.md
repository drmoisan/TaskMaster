# P8-T6 — Phase 8 evidence commit

Timestamp: 2026-09-20T09-44

Command:

```
git add -- docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/
git commit -F <message file>
git show --name-only --format= HEAD
git status --porcelain --untracked-files=all
```

EXIT_CODE: 0

Output Summary: Phase 8 evidence committed under an explicit feature-folder pathspec. Eight paths,
all inside the feature folder. Working tree clean afterwards. Ticked-task count in the execution
copy of the plan was 111 at commit time.

Phase 8 head SHA: `8bc97a13d21a803f3b750ae82778488fd244729e`

## `git show --name-only --format= HEAD`

```
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/issue-updates/p8-t5-followup-issue-body.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/issue-updates/p8-t5-followup-issue.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/other/p8-t1-credential-availability.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p7-t11-commit.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p8-t2-ac18-repair-identity.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p8-t3-ac19-required-checks.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p8-t4-ac20-disclosure.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md
```

Paths listed outside the pathspec folder, counted by filtering the listing above: 0.

## `git status --porcelain --untracked-files=all`

Empty. No entry anywhere, therefore none outside `coverage/`.

## Acceptance

| Criterion | Observed | Result |
|---|---|---|
| `git show --name-only --format= HEAD` lists only paths under the feature folder | 8 of 8 paths under it, 0 outside | PASS |
| `git status --porcelain --untracked-files=all` contains no entry outside `coverage/` | output empty | PASS |
| Ticked-task count in the execution copy of the plan is exactly 111 | 111, being P0-T1 through P8-T5 | PASS |
| Head SHA differs from the value P7-T11 recorded | `8bc97a13...` differs from `e3ea87babd60be04cbd2af50c6fcbe96d8b60518` | PASS |

P8-T6 is ticked after this artifact is written, which leaves the plan modified in the working tree
for the next commit. That is the documented fixpoint: the count this task asserts is the count of
tasks preceding it.
