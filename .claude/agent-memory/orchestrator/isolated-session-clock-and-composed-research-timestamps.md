---
name: isolated-session-clock-and-composed-research-timestamps
description: In a worktree-isolated orchestrator session pwsh Get-Date is refused; read the clock from git instead, and check subagent filename timestamps against it because task-researcher composed future-dated names on #973
metadata:
  type: project
---

On the #973 preparation run (2026-10-02) the session was worktree-isolated, so every `pwsh` call (including `Get-Date`) was refused by the Bash-tool isolation guard and `poetry` had no pyproject. Two working clock sources remained, both single `git` commands:

- `git -C <wt> log -1 --date=format-local:%Y-%m-%dT%H-%M --format=%cd` right after a commit gives that commit's local time.
- `git -C <wt> var GIT_COMMITTER_IDENT` prints the current epoch; add its difference from `git log -1 --format=%ct` to the last commit's local time.

Both task-researcher files on that run were named with composed timestamps (22-50 and 23-40) that were 15 and 47 minutes LATER than the commits that first added them (22-35, 22-53); prd-feature also wrote a future `Last Updated`. The run renamed them with `git mv` to the commit-time readings and updated every reference.

**Why:** the parent prompt required every timestamp to be read from the clock; a subagent that cannot run pwsh invents one, and the invented value looks plausible.

**How to apply:** after each delegated artifact lands, compare its filename/header timestamp with the commit-time clock reading; rename before other documents cite it (a later rename touches the spec, runbook and sibling research). The clock technique itself is in [[read-the-clock-with-git-var-when-pwsh-is-refused]]. Related: [[task-researcher-filename-regex-is-strict]], [[worktree-isolation-blocks-pwsh-per-agent-type]].
