# P0-T19 — Pre-fix workflow, runbook and README state; actionlint baseline

Timestamp: 2026-09-30T09-51
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $w = Get-Content ".github/workflows/dependabot-repair.yml"; "WF_APPID=" + ...; "WF_CLIENTID=" + ...; $rb = Get-Content "docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md"; "RB_APPID=" + ...; "RB_CLIENTID=" + ...; "RB_PARTB=" + ...; "README_NUMERIC=" + ...' (the plan's P0-T19 command verbatim) then CMD-ACTIONLINT
EXIT_CODE: 0
Output Summary:
- WF_APPID=1
- WF_CLIENTID=0
- RB_APPID=1
- RB_CLIENTID=0
- RB_PARTB=1 (the Part B heading at line 99)
- README_NUMERIC=1
- actionlint version line: "1.7.7" (followed by "installed by downloading from release page" and "built with go1.23.4 compiler for windows/amd64")
- Scoped lint of .github/workflows/dependabot-repair.yml: no output; SCOPED_EXIT=0
- Repository-wide run-actionlint.ps1: no output; REPO_EXIT=0
- Lint output recorded as empty: the baseline tree lints clean; the app-id deprecation is a runtime warning of the action, not a lint finding.
- CI actionlint version: 1.7.7 (.github/workflows/_actionlint.yml line 26); local version 1.7.7.
