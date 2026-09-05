# Worktree Context (issue #781)

Timestamp: 2026-09-05T16-15

Task: [P0-T3]

Command: `git rev-parse --abbrev-ref HEAD`, `git rev-parse HEAD`,
`git rev-parse --verify --quiet refs/heads/main`, and `git merge-base main HEAD`, all issued
from inside a `pwsh -NoProfile -Command` process whose working directory is the repository root
of the worktree that contains this feature folder.

EXIT_CODE: 0

## Observed Values

Branch (verbatim output of `git rev-parse --abbrev-ref HEAD`):

`bug/breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781`

HEAD (verbatim output of `git rev-parse HEAD`):

`ef0b5253ed93147d3a85e89da96b7a13e0396fc2`

BASE_REF: main

A local `main` ref exists in this worktree at `a007f72e394ee3038c6c52bfdf91f007df96fd6c`, so the
recorded base ref is `main` rather than `origin/main`. Every later task that names `main` as a
diff base uses this recorded ref.

Merge base (verbatim output of `git merge-base main HEAD`):

`a007f72e394ee3038c6c52bfdf91f007df96fd6c`

TOPLEVEL CONTAINS FEATURE: YES

TOPLEVEL LEAF: TaskMaster

The verbatim output of `git rev-parse --show-toplevel` is deliberately not recorded. That value
is an absolute host path containing the operating-system account name, and this repository
forbids an absolute host path in any tracked artifact. The two headings above record the only
two properties of it that later tasks need: that the feature folder resides beneath it, obtained
by testing the existence of
`docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781`
under that path; and its final path segment.

Output Summary: All five acceptance conditions hold. The artifact exists;
`TOPLEVEL CONTAINS FEATURE:` is `YES`; `BASE_REF:` is present and is `main`; the abbreviated
branch name is recorded verbatim; and the artifact contains no absolute filesystem path. The
observed branch equals
`bug/breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781`, so no `BRANCH MISMATCH`
is reported and no branch was created or switched. HEAD `ef0b5253` and base `a007f72e` match the
values the plan's version 1.1 execution-location decision records.
