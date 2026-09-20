# P1-T13 — `_pester.yml` widened to the two-member scope

Timestamp: 2026-09-19T14-18

Command:
```
git grep -n -e "Run.Path" -e "CodeCoverage.Path" -e "Invoke-Pester" -e "linePercent -lt 80" -- ".github/workflows/_pester.yml"
git grep -c -F "Invoke-Pester" -- ".github/workflows/_pester.yml"
pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911"; & "C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911\scripts\dev-tools\run-actionlint.ps1"'
git diff --numstat 734112ed25bba293cb074e71fee2286bc3b72fae -- ".github/workflows/_pester.yml"
git status --porcelain --untracked-files=all -- ".github/workflows/_pester.yml"
```

EXIT_CODE: 0

## The edit

Two one-for-one line substitutions, made with the `Edit` tool rather than `sed` through the Bash
tool, per gate rule 15.

| Line | Before | After |
|---|---|---|
| 41 | `$configuration.Run.Path = 'tests/scripts/vscode'` | `$configuration.Run.Path = @('tests/scripts/dependencies', 'tests/scripts/vscode')` |
| 45 | `$configuration.CodeCoverage.Path = 'scripts/vscode'` | `$configuration.CodeCoverage.Path = @('scripts/dependencies', 'scripts/vscode')` |

The substitutions are one line for one line, so nothing below them moves. That is load-bearing
here: the 80 percent line gate the task requires left alone is still at line 71, and the
`Upload coverage document` step still begins at line 74. Both were re-measured after the edit
rather than assumed.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| `Run.Path` names both members | line 41 names `tests/scripts/dependencies` and `tests/scripts/vscode` | PASS |
| `CodeCoverage.Path` names both members | line 45 names `scripts/dependencies` and `scripts/vscode` | PASS |
| CMD-ACTIONLINT returns `EXIT_CODE: 0` | exit 0, no output | PASS |
| The file contains exactly one `Invoke-Pester` invocation | `git grep -c -F "Invoke-Pester"` reports `.github/workflows/_pester.yml:1` | PASS |
| The 80 percent line gate is left at line 71 | `if ($linePercent -lt 80) { exit 1 }` measured at line 71 after the edit | PASS |

The stated failing condition — either array left single-valued, which would leave the new suite
unexecuted in CI — is reachable and was checked against the post-edit text rather than the intent
of the edit.

## Actionlint non-vacuity

Per gate rule 10, `actionlint` prints nothing at all on a clean run: no file count and no summary
line, so no non-vacuity observation can be read from its output. **The count below is an
independent filesystem enumeration, not actionlint output.**

```
pwsh -NoProfile -Command 'Get-ChildItem -LiteralPath "C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911\.github\workflows" -File | Where-Object { $_.Extension -in ".yml",".yaml" }'
WORKFLOW_FILE_COUNT=8
  _actionlint.yml
  _build-analyzers.yml
  _build-nullable.yml
  _format-check.yml
  _mstest-coverage.yml
  _pester.yml
  ci.yml
  codex-web-setup-test.yml
ACTIONLINT_BIN_EXISTS=True
```

Eight workflow files are present for the linter to read, `_pester.yml` among them, and
`actionlint-bin\actionlint.exe` exists. The script throws rather than passing silently when the
binary is absent, so an absent binary is a task failure and not a vacuous zero. The four files this
change has edited so far — `_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml`
and `_pester.yml` — are all inside the enumerated set, so the clean result covers this change's own
edits rather than an unrelated population.

The run was invoked with an **absolute** script path and an explicit `Set-Location` to the
execution worktree, and the resolved working directory was printed and recorded as
`C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911`. Both are required rather than cosmetic,
and the first was established by a measured failure in this run rather than by assumption.

`pwsh -NoProfile -WorkingDirectory <execution-worktree> -File ".\scripts\..."` was tried first and
**resolves the relative `-File` argument against the session worktree, not against
`-WorkingDirectory`**. The effect was observed directly at P1-T14, where the same invocation shape
launched `scripts\vscode\Invoke-Restore.ps1` from the session worktree and the script — which
derives its repository root from `$PSScriptRoot` — restored
`C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\TaskMaster.sln` instead of the execution
worktree's solution. `run-actionlint.ps1` derives its binary path the same way, so the earlier
actionlint invocation was re-run in the corrected form before this artifact was finalised. The
result was identical, exit 0 with no output, but the earlier form is not sound evidence and is not
what this artifact records.

Both halves of the corrected form matter. The absolute path fixes which checkout's script runs;
the `Set-Location` fixes which checkout's workflows `actionlint` discovers, because the binary
takes its project root from the process working directory rather than from its own location.

## Diff shape

`git diff --numstat 734112ed25bba293cb074e71fee2286bc3b72fae -- ".github/workflows/_pester.yml"`:

```
2	2	.github/workflows/_pester.yml
```

Two added and two deleted lines — one per substituted assignment, with no net line movement, which
is what keeps the line-71 citation valid. `git status --porcelain --untracked-files=all` lists
` M .github/workflows/_pester.yml` as the gate rule 8 companion.

Output Summary: `_pester.yml` now runs the two-member test path
`tests/scripts/dependencies` and `tests/scripts/vscode` and instruments the two-member coverage
path `scripts/dependencies` and `scripts/vscode`. The 80 percent line gate remains at line 71 and
the artifact upload step is unchanged. Actionlint returned exit 0 over an independently enumerated
8 workflow files, and the file carries exactly 1 `Invoke-Pester` invocation.
