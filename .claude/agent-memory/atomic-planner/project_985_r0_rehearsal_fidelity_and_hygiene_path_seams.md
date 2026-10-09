---
name: project-985-r0-rehearsal-fidelity-and-hygiene-path-seams
description: "#985 round 0 plan seams - a merge-the-fix-into-the-Dependabot-branch rehearsal is unfaithful when the fix adds package declarations (the test projects stay pinned to the old version; simulate Dependabot's bump with nuget update per new pair); the hygiene guard fails CI on any drive-rooted Users path in a tracked file, including the plan itself and caller-supplied scratchpad paths; Invoke-VSBuild runs Sync-PackageReferences which rewrites csproj HintPaths; rehearsal merges need -X ours plus content re-apply"
metadata:
  type: project
---

Planning #985 (2026-10-09; Dependabot repair: borrowed test packages + transitive binding redirects) surfaced four seams.

**Why:** each would have produced a false rehearsal failure or a red hygiene CI job unrelated to the fix.

**How to apply:**
- A rehearsal that merges a fix adding `packages.config` declarations into an existing Dependabot branch leaves the newly declaring projects at the pre-bump version while their production references are bumped; the post-merge `@dependabot recreate` would bump them too. Insert a simulated Dependabot step (`nuget update <proj>\packages.config -Id -Version -RepositoryPath`) for each new pair whose production sibling differs, leave redirects to the repair script, and flag the decision to the caller (the AC text did not mention it).
- `scripts/hygiene/Test-RepositoryHygiene.Rules.ps1` line 21 flags `[a-z]:[\\/]+users[\\/]+...` in every tracked file. A caller-supplied scratchpad path (C-drive Users temp dir) must be written as `$env:LOCALAPPDATA\Temp\...` in the plan, and every helper script must replace roots and profile paths before printing. Positive control for a zero-count scan: the linked worktree's `.git` pointer file matches once.
- `scripts/vscode/Invoke-VSBuild.ps1` lines 247-253 run `Sync-PackageReferences.ps1` before building, which can rewrite csproj HintPaths; for manifest-defect work resolve MSBuild via vswhere and run the CLAUDE.md arguments directly.
- Long helper payloads go into scratch `.ps1` files outside the repo (the PS batch-budget hook ignores paths outside the root, line 288), which also removes Bash-to-pwsh quoting hazards.
- Related: [[project-930-uithread-ilglobals-comments-plan-seams]] (runner dot-source + redefine for the shell-icon filter), [[poshqc-mcp-and-msbuild-invocation-facts]].
