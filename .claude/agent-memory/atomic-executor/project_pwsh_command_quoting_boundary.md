---
name: pwsh-command-quoting-boundary
description: A plan task whose command is `pwsh -NoProfile -Command "...$var...$(...)..."` is not executable from either the Bash tool or PowerShell; the calling shell eats the variables first. Use outer single quotes with inner double quotes.
metadata:
  type: project
---

`pwsh -NoProfile -Command "<script containing $var and $(...)>"` is broken in this environment
regardless of which shell the agent uses:

- **Bash tool**: `$c` expands to empty and `$($r.PassedCount)` becomes *command substitution*,
  so bash tries to run `.PassedCount` as a command. Measured output of the plan-432 `[P0-T12]`
  string: `[ = New-PesterConfiguration; .Run.Path = 'tests/scripts/vscode'; "/"]`.
- **PowerShell (primary shell)**: identical damage. `pwsh` then reports
  `The term '=' is not recognized...`, `The term '.Run.Path' is not recognized...`, `LASTEXITCODE=1`.
  No Pester run happens and no output file is emitted.

The working form is **outer single quotes, inner double quotes** — verified in both shells:

```
pwsh -NoProfile -Command '$c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/vscode/X.ps1"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "<repo-relative>.xml"; $r = Invoke-Pester -Configuration $c; "PASSED=$($r.PassedCount) FAILED=$($r.FailedCount)"'
```

Also measured while proving this out:
- Pester 5.6.1 and `New-PesterConfiguration` resolve here; Pester 3.4.0 is also installed.
- `$c.CodeCoverage.OutputPath` accepts a **repo-relative** path and **creates the directory**
  if absent. It requires cwd = worktree root.
- The emitted JaCoCo carries `<class name="vscode/X" sourcefilename="X.ps1">` with per-class
  `INSTRUCTION` / `LINE` / `METHOD` / `CLASS` counters and **no `BRANCH` counter** — so direct
  `Invoke-Pester` is a valid per-file line-coverage mechanism for paths the bundled PoshQC
  Pester config does not instrument (it instruments only `.claude/hooks/`, `.claude/lib/`,
  `.codex/hooks/`).

**Why:** issue-432 preflight iterations 2 and 3 both produced a Blocking finding of exactly this
class — first `Select-String -Recurse` (a parameter that does not exist), then this quoting
boundary. Command strings in plans are routinely written but never executed by the planner.

**How to apply:** during preflight, never accept a command-bearing task on inspection alone.
Actually run every `pwsh -Command`, `Select-String`, and pipeline command verbatim in a scratch
script and report the observed output. Treat any command that fails to parse as Blocking, because
it reproduces a spurious halt at execution time.

Related: [[project_poshqc_pester_mcp_exit_minus1]]
