---
name: parallel-epic-children-name-collisions
description: Parallel epic children can invent identical C# type names in the same shared namespace; collisions surface only at rebase — resolve by renaming YOUR types, rerun full toolchain, no re-review needed per epic directive
metadata:
  type: project
---

Verified 2026-07-18 (epic folder-tree-breadcrumb-redesign, children #349/#351). Two siblings executing concurrently against the same integration base both created breadcrumb types in `UtilitiesCS.OutlookObjects.Folder`. Textual merge conflicts were confined to `.csproj` `<Compile Include>` blocks (union-resolve, keep both), but two SEMANTIC collisions appeared only at the post-sibling-merge rebase build: CS0101 (`BreadcrumbRow` defined by both) and CS0104 (`BreadcrumbBridgeRouter` ambiguous between `QuickFiler.Controllers` and `UtilitiesCS.OutlookObjects.Folder` in sibling test files importing both namespaces).

**Why:** epic planning did not assign type-name budgets/prefixes per child; both features independently coined natural names for the same domain. git cannot see same-name-new-file collisions across different paths.

**How to apply:**
- After a sibling merges first, rebase and expect the analyzer build to fail even when only csproj conflicts appeared; grep both branches' new type names in shared namespaces proactively (`git diff --name-only base..integration` vs your new files).
- Resolve by renaming YOUR OWN types (e.g. `BreadcrumbRow`->`BreadcrumbStateRow`, `BreadcrumbBridgeRouter`->`FolderBreadcrumbBridgeRouter` with git mv + csproj update), never editing sibling-owned merged files.
- Identifier-rename-only + full toolchain green in one pass counts as rebase conflict resolution under the epic directive ("rebase onto the updated integration tip and rerun the full toolchain before merging") — no feature-review re-run required; document it in the PR body and checkpoint.
- csharpier may need a re-format after renames (longer identifiers re-wrap lines).
- Also observed: `collect_pr_context` wrote pr_context.* INTO the agent worktree when `workspace_root` was the Agent-tool isolated worktree (cwd==worktree topology), consistent with [[agent-worktree-hooks-resolve-to-agent-cwd]] and contrasting [[collect-pr-context-lands-in-main-checkout]] (named-worktree topology).
