---
name: pwsh-command-payload-quoting
description: A plan task embedding `pwsh -NoProfile -Command "...$var..."` is quote-fragile — the parent shell expands $var before pwsh sees it; single-quote the payload and double the inner literals
metadata:
  type: feedback
---

Any command written into a plan as `pwsh -NoProfile -Command "$c = ...; ... $c"` is a defect. From a PowerShell parent `$c` interpolates to empty before `pwsh` receives the string; from the Bash tool bash expands it. Both yield a syntax error in the child. Write the payload single-quoted and escape inner string literals as doubled single quotes:

`pwsh -NoProfile -Command '$c = New-PesterConfiguration; $c.Run.Path = ''tests/scripts/vscode''; ... Invoke-Pester -Configuration $c'`

**Why:** Caught as an advisory in #512 preflight iteration 2. One malformed command in `[P0-T16]` propagated by reference into four other tasks that said "the direct Pester command defined in [P0-T16]", so a single quoting defect would have failed five tasks.

**How to apply:** Applies to Pester-with-`New-PesterConfiguration` runs, and to any `-Command` payload containing `$`. Add a parenthetical at the command site stating the payload is single-quoted so the parent does not interpolate, and that re-quoting must preserve that property — otherwise an executor "fixes" the quoting back to the broken form. When a later task reuses a command by reference, the defect multiplies; prefer defining such commands once and citing the defining task.
