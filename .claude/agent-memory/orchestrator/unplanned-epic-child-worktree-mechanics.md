---
name: unplanned-epic-child-worktree-mechanics
description: Running a standalone orchestrator lifecycle as an unplanned epic-child from the integration worktree — cross-worktree delegation works, but atomic-executor needs explicit C# tool paths and collect_pr_context/hooks resolve against session root
metadata:
  type: project
---

Verified 2026-07-11 (#315, PR #316 merged to epic/swordfish-removal-integration @ 90c1ac03). When the epic-orchestrator launches an unplanned remediation/cleanup child that must run the full standalone lifecycle, the child orchestrator's session cwd is the integration worktree (on the epic branch). Create a SEPARATE feature-branch worktree (`git worktree add -b <type>/<name>-<issue> <sibling-path> <integration-tip>`); do NOT switch the session worktree's branch (parent owns it).

**Cross-worktree delegation works.** task-researcher, prd-feature, atomic-planner, atomic-executor, and feature-review all accept ABSOLUTE paths into the sibling feature worktree even though their nominal cwd is the session root. Their path-scoped Write globs (e.g. `docs/features/**/research/**`, `docs/features/active/**`) matched the absolute feature-worktree paths without refusal. Always pass absolute feature-worktree paths and tell each agent to `cd` there for git/build commands.

**atomic-executor cannot run the C# toolchain directly** — its Bash is scoped to git/poetry/npx/pwsh (no `msbuild`/`dotnet`/`vstest`/`csharpier`). Give it the tools via `pwsh -NoProfile -Command` with explicit paths, which you must probe first: msbuild is on PATH; `vstest.console.exe` is NOT (find it under `<VS>\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe`); csharpier via the GLOBAL `~/.dotnet/tools/csharpier.exe` because `dotnet tool run csharpier` fails when the manifest is a repo-root `dotnet-tools.json` (dotnet wants `.config/dotnet-tools.json`). A fresh worktree needs a NuGet restore first (`./scripts/vscode/Invoke-Restore.ps1`) or the analyzer build fails on missing packages.

**Session-root resolution of PR machinery.** `collect_pr_context` writes to the SESSION ROOT `artifacts/` regardless of the `workspace_root` arg, but it DOES resolve the head ref by branch name, so the diff is correct even though the session is on the epic branch (confirms [[collect-pr-context-lands-in-main-checkout]] and [[child-orchestrator-pr-hook-reads-session-root]]). Author `artifacts/pr_body_<issue>.md` + `.receipt.json` at the session root; the pr-author + epic-merge hooks read the session-root checkpoint. gh pr create/merge run fine from the session root with explicit `--base <epic-integration-branch> --head <feature-branch>`.

**feature-review writes mirror audit copies into the session root** to satisfy its own SubagentStop coverage hook (which resolves advertised paths against session cwd, where the #<issue> feature folder does not exist on the epic branch). These are disposable — `rm -rf` them from the session worktree after committing the authoritative copies to the feature branch. Subagents also persist their own `.claude/agent-memory/*` into the session worktree; migrate those into the feature branch and `git checkout --`/`rm` to restore the parent's worktree.

Merge-gate sequencing for the child self-merge is [[epic-mode-pr-merge-gate-sequencing]] (epic_mode:true + step9 "passed" for the merge hook, then "verified" for completion). Child->integration PRs get zero CI ([[project_epic_child_prs_no_ci]]); gate is feature-review blocking_count==0.
