# Phase 6 — Projections-only audit: no raw coverage or result artifact entered the tree (P6-T9)

Task: [P6-T9]
All three commands were run from the item worktree root via Set-Location inside one pwsh invocation (inner quoting inverted to single quotes; semantics identical).

## Command 1 — tracked raw-artifact count in the working tree

Timestamp: 2026-09-13T03-51
Command: `pwsh -Command 'git ls-files -- "*.trx" "*.cobertura.xml" | Measure-Object | Select-Object -ExpandProperty Count'`
EXIT_CODE: 0
Output Summary: `570`

## Command 2 — uncommitted or untracked raw-artifact count

Timestamp: 2026-09-13T03-51
Command: `pwsh -Command 'git status --porcelain --untracked-files=all | Select-String -SimpleMatch -Pattern ".trx",".cobertura.xml" | Measure-Object | Select-Object -ExpandProperty Count'`
EXIT_CODE: 0
Output Summary: `0`

## Command 3 — tracked raw-artifact count at the self-anchor (the value before this item began)

Timestamp: 2026-09-13T03-51
Command: `pwsh -Command 'git ls-tree -r --name-only refs/plan/issue-743-base | Select-String -SimpleMatch -Pattern ".trx",".cobertura.xml" | Measure-Object | Select-Object -ExpandProperty Count'`
EXIT_CODE: 0
Output Summary: `570`

## Acceptance

- The second count is `0`: no `.trx` or `.cobertura.xml` path is modified, staged or untracked anywhere in the tree (the raw outputs this plan produced live under the ignored repository-root `coverage` directory and are not reported by porcelain status).
- The first count (`570`) is identical to the value at `refs/plan/issue-743-base` (`570`), so this item added no tracked raw artifact. The 570 pre-existing tracked paths are inherited from earlier items and are outside this plan's scope (D1 governs this item's additions only).
