# [P11-T12] `user-story.md` absence

Timestamp: 2026-08-28T02-10
Task: [P11-T12]
Command: `find docs/features/active/efc-controller-surface-defects-464 -name 'user-story.md' -print`;
`ls docs/features/active/efc-controller-surface-defects-464/`;
`git ls-files 'docs/features/active/efc-controller-surface-defects-464/*user-story*'`
EXIT_CODE: 0

This is a negative evidence claim, so the search is recorded in auditable form per the
`evidence-and-timestamp-conventions` requirement.

SearchScope: `docs/features/active/efc-controller-surface-defects-464/` and, recursively, every
subdirectory of it. The feature is **not** versioned — there is no `v1/`, `v2/` or equivalent version
folder — so the feature root is the whole scope and there is no separate current-version scope to search.

SearchPatterns: the exact filename `user-story.md`, applied recursively by `find -name`. Additionally the
glob `*user-story*` applied to the git index by `git ls-files`, which would catch a tracked file under any
near-miss name.

SearchResult: **none**. Both commands produced no output lines.

## Complete listing of the feature root

```
evidence/
issue.md
plan.2026-08-25T07-01.md
plan-base-drift-addendum.2026-08-27T21-01.md
research/
spec.md
upstream-constraints-briefing.2026-08-27T23-12.md
```

`user-story.md` is absent from the working tree and absent from the git index.

## Why this is the correct state

`docs/features/active/efc-controller-surface-defects-464/issue.md:6` carries the marker
`- Work Mode: full-bug`. Under the `acceptance-criteria-tracking` resolution table, `full-bug` resolves
the acceptance-criteria source to **`spec.md` only**, with `user-story.md` optional and absent by
default. The absence is the expected state for this work mode, not a missing document.

`spec.md` is present and is the sole acceptance-criteria source, carrying all 74 criteria.

Output Summary: PASS. `user-story.md` does not exist anywhere under
`docs/features/active/efc-controller-surface-defects-464/`, in the working tree or in the git index.
SearchScope, SearchPatterns and SearchResult (`none`) are recorded above. The absence is correct for the
persisted `full-bug` work mode, under which `spec.md` is the sole acceptance-criteria source.
