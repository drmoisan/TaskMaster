# [P0-T16] Phase 0 commit

- Issue: #792
- Timestamp: 2026-09-17T18-50
- Command: `git add -- docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` then `git commit -m "chore(792): phase 0 baselines and plan"` (run with `git -C <repo-root>` against the item worktree on branch `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792`)
- EXIT_CODE: 0
- Output Summary: `[bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792 0d0275e9] chore(792): phase 0 baselines and plan`; `16 files changed, 503 insertions(+), 15 deletions(-)`; 15 files created under `evidence/` and the plan file modified (fifteen `[ ]` to `[x]` check-offs for [P0-T1] through [P0-T15]).

## Acceptance observations

- `git show --name-only --format= HEAD` listed 16 paths, every one under `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/` (13 under `evidence/baseline/`, 1 under `evidence/other/`, 2 under `evidence/regression-testing/`, plus `plan.2026-09-17T07-30.md`).
- `git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'` printed nothing.
- `git status --porcelain --untracked-files=all` printed nothing immediately after the commit.

COMMIT-SHA-OBSERVED: 0d0275e999efee6f643fef9f6e8206e0303c8c8b

## Residual (recorded, not an acceptance clause)

This artifact and the [P0-T16] check-off in the plan file are written after the commit they describe, so they remain uncommitted feature-folder changes at the end of Phase 0 (the plan's convention 9 treats docs and evidence as expected-dirty). No source path is dirty. They are left for the next feature-folder commit in the plan rather than committed here, because the plan authorizes exactly one commit in Phase 0.

Git printed sixteen `LF will be replaced by CRLF` warnings for the newly added Markdown files; this is the repository's autocrlf normalisation notice and does not affect the committed content.
