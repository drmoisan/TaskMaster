---
name: worktree-root-breaks-dotclaude-exclusion
description: Never plan a "discovered assembly path contains \.claude\" assertion — the agent worktree root is itself under .claude\worktrees\, so the gate is unsatisfiable; use a workspace-root prefix test
metadata:
  type: project
---

The standard guidance "when globbing for `*.Test.dll`, exclude any path containing `\.claude\`" is **unsatisfiable when the executing workspace is an agent worktree**, because the worktree root is itself `...\TaskMaster\.claude\worktrees\agent-<id>\`. Every discovered assembly path then contains `\.claude\` and the assertion fails 100% of the time.

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` resolves its search root from `$PSScriptRoot\..\..` (line ~271), i.e. the worktree, and its discovery filter (lines ~296-302) excludes only `\obj\` and `\ref\`.

Correct assertion to write into a plan task: every discovered assembly path **begins with the workspace-root prefix**, and no discovered path contains a `\.claude\worktrees\` segment **after** that prefix (which would be a stale sibling worktree build).

**Why:** #508 preflight pass 1 returned this as a blocking finding on two tasks (baseline coverage capture and final-QC coverage capture). The substring rule is correct in the main checkout and wrong in every agent worktree, which is where plans actually execute.

**How to apply:** Whenever a plan task asserts something about discovered test-assembly paths, state the assertion as a prefix test against the literal workspace root supplied by the caller. Related: [[invoke-mstest-with-coverage-script]], [[invoke-mstest-single-searchroot-defect]].
