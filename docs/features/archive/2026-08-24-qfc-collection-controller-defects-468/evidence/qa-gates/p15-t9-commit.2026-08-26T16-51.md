# [P15-T9] Commit of the final QA loop evidence

Timestamp: 2026-08-26T16-51

Command:

```
git add -- <11 explicit paths>          # no `git add -A`, no `git add .`
git commit -m "docs(468): final QA loop evidence and coverage comparison"
git show --name-only HEAD
git status --porcelain
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Commit `e265a26872c313066f7bb084f870bbc2240ce378` —
`docs(468): final QA loop evidence and coverage comparison`.
11 files changed, 192,688 insertions(+), 9 deletions(-).

`git show --name-only HEAD` path list, verbatim and complete:

```
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/coverage-final.cobertura.xml
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p14-t14-commit.2026-08-26T16-41.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p15-t1-format.2026-08-26T16-43.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p15-t2-format-check.2026-08-26T16-44.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p15-t3-analyzers.2026-08-26T16-45.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p15-t4-nullable.2026-08-26T16-46.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p15-t5-tests-coverage.2026-08-26T16-47.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p15-t6-loop-record.2026-08-26T16-48.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p15-t7-file-size-audit.2026-08-26T16-49.md
docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/p15-t8-coverage-delta.2026-08-26T16-50.md
docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md
```

Eleven paths, all under `docs/features/active/qfc-collection-controller-defects-468/`. No `.cs`, no
`.csproj`, no path outside the feature folder.

The large insertion count is `coverage-final.cobertura.xml` — 10,669,373 bytes of generated Cobertura
XML, committed as audit-trail evidence. `git check-ignore -v` confirms it is matched by no
`.gitignore` rule, so it is genuinely tracked rather than silently dropped.

## Evidence-location compliance

Every path written by Phase 15 is under `<FEATURE>/evidence/qa-gates/`. In particular the coverage
document was written to
`docs/features/active/qfc-collection-controller-defects-468/evidence/qa-gates/coverage-final.cobertura.xml`
and **not** to `artifacts/csharp/coverage.xml`, `artifacts/coverage/`, or any other non-canonical
location.

No helper script — `.ps1`, `.py`, or `.sh` — was written anywhere under `<FEATURE>/evidence/`. The
two throwaway PowerShell helpers used during this phase (a vstest wrapper and a TRX sanitiser) were
created in the system temporary directory and are not part of the repository.

## Staging hygiene

The `git add` used an explicit 11-path pathspec. `git status --porcelain` after the commit, verbatim:

```
?? .claude/state/
```

`.claude/state/` remains untracked and deliberately uncommitted.
