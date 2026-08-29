---
name: pwsh-double-quoted-command-refused-in-worktree
description: A pwsh -NoProfile -Command block written with a bash double-quoted outer string fails twice over in an isolated agent worktree; use a single-quoted outer string with PowerShell double-quoted inner strings.
metadata:
  type: project
---

A `pwsh -NoProfile -Command "..."` invocation whose outer string is bash **double**-quoted and contains `$` fails two independent ways in an isolated agent worktree, and a plan full of them is unexecutable.

1. The worktree-isolation guard refuses the call outright with `This agent is isolated in the worktree ... but this command is too complex to verify that it stays inside the worktree.` The discriminator is the `$`, not the `;` — `pwsh -NoProfile -Command "Write-Output 'a'; Write-Output 'b'"` runs fine, while `pwsh -NoProfile -Command "Write-Output ('N=' + $PSVersionTable.PSVersion.Major)"` is refused.
2. Even where the guard does not fire, bash expands `$f`, `$_`, `$prod` and friends to empty strings before `pwsh` ever sees them, so the command silently computes wrong values rather than failing.

The working form is a bash **single**-quoted outer string with PowerShell **double**-quoted inner strings:
`pwsh -NoProfile -Command 'Write-Output ("N=" + $PSVersionTable.PSVersion.Major)'`

**Why:** on issue #635 an atomic plan reached preflight with all eight of its `pwsh` blocks in the broken form. I had "tested" the same commands myself and they worked — because in my own Bash calls I escaped the `$` as `\$`, which the plan text does not and cannot carry. My testing therefore validated a different string than the one the executor would run. This is the [[preflight-catches-vacuous-gates]] failure mode applied to the command itself rather than to the assertion.

**How to apply:** when handing tested command forms to a planner, hand over the exact bytes the executor will run, not a shell-escaped variant. Write `pwsh` blocks single-quoted-outer from the start, and state the quoting convention in the plan preamble so a later revision does not reintroduce the double-quoted form. Related: [[bash-tool-mangles-msbuild-switches]] and [[bash-tool-rejects-complex-commands-in-isolated-worktree]] are the same family — the Bash tool's handling of a command string is not transparent, so a command must be proven in the form it will actually be stored.
