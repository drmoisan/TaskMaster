# P8-T12 — Final commit and clean-tree gate

Timestamp: 2026-09-08T10-45
Task: [P8-T12]
Command: the P8-T8 four counts re-run over `<FEATURE>` and `docs/features/potential/`, then `git add -A -- . ":(exclude).claude"`, then `git commit -m "docs(811): AC4 ten-run evidence, sanitisation, follow-ups, AC reconciliation"`
EXIT_CODE: 0

## Sanitisation re-count

The plan states the expected result as 0, 0, 0, 0. That holds for everything this item wrote. It
does not hold for the whole `docs/features/potential/` tree, because that tree contains 194 files,
almost all of them pre-existing and unrelated to this item. Both scopes are recorded.

### Scope A — this item's own written set

`<FEATURE>` (all 47 files) plus the two potential entries P8-T9 created.

| Check | Count |
|---|---|
| Files scanned | 49 |
| Account-token content matches | 0 |
| Machine-token content matches | 0 |
| Drive-rooted profile-path matches | 0 |
| Offending file or directory names | 0 |

**0, 0, 0, 0**, as the plan expects.

### Scope B — the whole `docs/features/potential/` tree

| Check | Count |
|---|---|
| Files scanned | 194 |
| Account-token content matches | 9 |
| Machine-token content matches | 0 |
| Drive-rooted profile-path matches | 16 |

All 25 hits lie in ten files, every one of them under `docs/features/potential/promoted/` and every
one of them present at the base commit `bb1c7d4b` (verified individually with `git cat-file -e`;
`EXISTS_AT_BASE=True` for all ten):

```
promoted/2026-08-10-mstest-coverage-discovery-claude-worktree-exclusion.md
promoted/2026-08-11-research-doc-cohort-library-false-negative.md
promoted/2026-08-14-orchestrator-hooks-reference-absent-python-validators.md
promoted/2026-08-14-potential-to-issue-promoted-copy-not-written.md
promoted/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary.md
promoted/2026-08-26-matchbestspecialfolder-substring-matching-codified-by-tests.md
promoted/2026-08-26-taskmaster-csproj-publishurl-leaks-user-profile-path-and-org-name.md
promoted/2026-08-28-trx-evidence-host-tokens-and-malformed-xml.md
promoted/2026-08-29-parallel-run-merge-gate-misparses-pr-number.md
promoted/2026-09-02-committed-host-identity-leaks.md
```

These are pre-existing committed host-identity leaks in other items' promoted entries. Several of
them are documents *about* host-identity leakage, which is why they quote the offending strings.
This item does not modify any of them, and rewriting ten unrelated committed documents would be a
scope escape well outside the 20-path write set. They are left untouched and reported instead. Note
that `docs/features/potential/promoted/2026-09-02-committed-host-identity-leaks.md` already tracks
this class of problem.

No file was rewritten by this task.

## Intended commit message

```
docs(811): AC4 ten-run evidence, sanitisation, follow-ups, AC reconciliation
```

## Acceptance evaluation

Recorded before the commit; the post-commit observations are reported in the executor's completion
message rather than written here, so that nothing is written after the commit.

- The four counts are re-run over `$Feature` and `docs/features/potential/` and recorded above,
  with the scope split made explicit. Scope A is 0, 0, 0, 0 as expected; scope B is non-zero
  solely through pre-existing files this change does not touch.
- The intended commit message is recorded.

## Output Summary

Everything this item wrote is free of account tokens, machine tokens and drive-rooted profile
paths. The 25 hits in the wider potential tree are pre-existing, all in `promoted/`, all present at
the base commit, and all outside this item's write set.
