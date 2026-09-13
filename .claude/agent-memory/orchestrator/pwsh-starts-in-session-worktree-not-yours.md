---
name: pwsh-starts-in-session-worktree-not-yours
description: A pwsh -NoProfile -Command payload launched by you or a delegate starts in the COORDINATOR SESSION worktree, not your assigned worktree, and that tree has its own TaskMaster.sln — so an unqualified msbuild builds the wrong checkout and returns a vacuously green result
metadata:
  type: project
---

A `pwsh -NoProfile -Command` invocation starts with its working directory set to the **coordinator session worktree** (the inherited cwd), NOT the worktree a WORKTREE DIRECTIVE assigns you. Verified on issue #877: `pwsh -NoProfile -Command 'Get-Location'` returned `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15` from an agent bound to `bug-877-test-isolation`.

**Why:** every sibling worktree contains its own `TaskMaster.sln`, so an unqualified `msbuild TaskMaster.sln /t:Rebuild ...` silently builds the WRONG checkout and returns `0 Error(s)` that says nothing about your change. `dotnet tool run csharpier format .` is worse: it rewrites the coordinator's tree, which is a direct worktree-directive violation with real blast radius. Neither failure announces itself — both look like success.

**How to apply:** mandate the literal prefix `Set-Location -LiteralPath '<your-worktree>'; ` on EVERY pwsh payload in any plan or delegation prompt, and make the prefix an *acceptance condition* — require each evidence artifact's `Command:` row to record the full prefixed payload, so a missing prefix fails the gate instead of passing silently. Exempt only payloads that reference everything by absolute path (e.g. the build-lock acquire/release scripts).

Related trap found the same run: `vstest.console.exe` is NOT on PATH on this machine. `Get-Command vstest.console.exe` returns nothing. Resolve it through vswhere as `scripts/vscode/Invoke-MSTest.ps1` line 93 does. That payload contains `$vstest`, so it must use outer SINGLE quotes on `-Command` — see [[bash-expands-dollar-in-double-quoted-pwsh-command]].

Neither fact is derivable by a planner: `atomic-planner` has no shell. Both must come from the orchestrator or be found in preflight. See [[preflight-catches-vacuous-gates]].
