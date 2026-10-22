# `user-story.md` Does Not Exist ([P7-T8])

Timestamp: 2026-08-28T06-17

Command: `ls -la <feature-folder>/user-story.md` and
`find <feature-folder> -maxdepth 1 -name 'user-story*'`, plus a checkbox count over every Markdown file
at the feature-folder root.
EXIT_CODE: 0

## Negative-evidence record

Per the negative-evidence rule, an absence claim must be auditable:

- **SearchScope:** `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/`

  This feature is **not** versioned — there is no `v1/`, `v2/`, or equivalent subfolder — so the feature
  root is the only scope that applies. The current-version-scope-plus-feature-root fallback the
  discovery rules describe collapses to a single directory here.

- **SearchPatterns:** `user-story.md` (exact name), and `user-story*` (prefix glob, to catch a
  differently suffixed variant such as `user-story.2026-08-25.md`)

- **SearchResult:** `none`

  `ls` reports `No such file or directory`. The `find` prefix glob returns no paths.

## Why this matters under `full-bug`

`issue.md` carries the marker `- Work Mode: full-bug`. Under that mode `spec.md` is the **sole**
acceptance-criteria source and `user-story.md` is intentionally absent. Its absence is therefore not a
gap to be filled: **a second checkbox-bearing document in this folder would be an integrity failure**,
because acceptance criteria would then live in two places and the two could disagree without either
being detectably wrong.

Corroborating check on the checkbox-bearing documents actually present at the feature-folder root:

| File | Checkbox lines | Role |
| --- | --- | --- |
| `issue.md` | **0** | pointer only — its `## Acceptance Criteria` section explicitly carries no criteria of its own |
| `spec.md` | **54** | the sole acceptance-criteria source |
| `plan.2026-08-25T09-53.md` | 145 | the plan's own task checklist, not acceptance criteria |

Exactly one document carries acceptance criteria, and its count is **54**, matching the total `[P0-T2]`
recorded and `[P9-T15]` reconciles. `issue.md` carries zero, so it cannot compete as a second source.

Output Summary: `SearchResult: none`. `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/user-story.md`
does not exist, under either the exact name or a `user-story*` prefix glob, searched at the feature root
— the only applicable scope, as this feature is not versioned. Under `full-bug`, `spec.md` is the sole
acceptance-criteria source with **54** criteria, `issue.md` carries **0** checkboxes, and a second
checkbox-bearing document in this folder would be an integrity failure.
