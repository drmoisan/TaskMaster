# P10-T13 — `user-story.md` does not exist in the feature folder

Timestamp: 2026-08-28T01-59
Command: Test-Path -LiteralPath docs/features/active/itemviewer-surface-defects-489/user-story.md
EXIT_CODE: 0

## Result

```
TestPath=False
```

`Test-Path` on `docs/features/active/itemviewer-surface-defects-489/user-story.md` returns **False**.
Acceptance met.

## Full feature-folder listing

```
evidence/
issue.md
plan.2026-08-25T01-04.md
research/
spec.md
```

There is no `user-story.md` at the feature root, and the feature is not versioned — there are no
`v1/`, `v2/` subdirectories that could hold one — so the single check above is exhaustive.

## Why this matters

This is a **`full-bug`** feature. The work-mode marker in `issue.md` resolves to `full-bug`, and the
mode rules in `atomic-plan-contract` and `acceptance-criteria-tracking` make `spec.md` the **sole**
acceptance-criteria source for that mode; `user-story.md` is optional and absent by default. The
plan's own authority statement at `plan.2026-08-25T01-04.md:12-14` says the same: "`FEATURE/spec.md`
§ Acceptance Criteria is the sole acceptance-criteria source. `user-story.md` does not exist for this
feature and must not be created. `issue.md` § Acceptance Criteria is a pointer, not a second source."

`minor-audit` and `full-bug` execution both fail closed when `spec.md` or `user-story.md` is present
unexpectedly in the active folder. `spec.md` is present as required; `user-story.md` is absent as
required. Neither fail-closed condition is triggered.

Output Summary: `docs/features/active/itemviewer-surface-defects-489/user-story.md` **does not
exist** — `Test-Path` returns `False`. The feature folder contains only `evidence/`, `issue.md`,
`plan.2026-08-25T01-04.md`, `research/` and `spec.md`, and the feature is unversioned, so no
alternative location could hold one. This is a `full-bug` feature and `spec.md` is the sole
acceptance-criteria source, exactly as the plan's authority statement requires.
