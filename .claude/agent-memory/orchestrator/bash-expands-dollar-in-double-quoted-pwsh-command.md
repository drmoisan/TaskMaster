---
name: bash-expands-dollar-in-double-quoted-pwsh-command
description: Bash expands $vars inside a double-quoted pwsh -Command, silently deleting PowerShell variables and the -f operator; use single quotes outside, double inside
metadata:
  type: reference
---

`pwsh -NoProfile -Command "... $s ..."` run through the Bash tool has its `$` sigils eaten by bash before pwsh ever sees them. The failure is silent and confusing: `$s='path'` arrives as `='path'` and PowerShell reports `The term '=path' is not recognized`, while `-f $_` arrives as `-f` with no operand and raises `You must provide a value expression following the '-f' operator`. Worst case the command *succeeds* having assigned an empty value, so a count comes back `0` and reads as a real measurement.

**Working form:** single-quote the outer string for bash and double-quote paths inside for PowerShell:

`pwsh -NoProfile -Command 'Set-Location "C:/path"; @("a","b") | ForEach-Object { "{0} {1}" -f $_, (Get-Content -LiteralPath $_).Count }'`

Two related traps in the same area:
- Import a PowerShell module by **absolute** path. `Import-Module .claude/lib/model-routing/ModelRouting.psm1` fails with "no valid module file was found in any module directory" even after a successful `Set-Location` to the worktree root; the absolute path resolves fine.
- For counting checkbox or literal matches in a file, prefer the **Grep tool** over shelling out to `Select-String`. It sidesteps the quoting layer entirely and its counts are trustworthy.

See [[pwsh-double-quoted-command-refused-in-worktree]] and [[feedback_no_cd_or_non_allowlisted_bash_segments]].
