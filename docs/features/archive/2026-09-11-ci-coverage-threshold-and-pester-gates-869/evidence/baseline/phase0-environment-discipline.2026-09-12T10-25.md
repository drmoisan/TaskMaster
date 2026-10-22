# Phase 0 — Environment discipline record (P0-T2)

Timestamp: 2026-09-14T17-50

## Statement 1 — Bash allowlist and per-segment checking

The Bash allowlist grants only `git`, `gh`, `pwsh`, `poetry run` and three scripts under the `.claude` lib directory. The permission engine checks every chained segment independently, so a command that changes directory and then runs something else is denied as a whole even when the second segment would be allowed on its own.

## Statement 2 — Git and file access without changing directory

Git is addressed with the repository-directory switch pointed at the item worktree root rather than by changing directory. File inspection uses the Read, Grep and Glob tools rather than `cat`, `grep`, `sed` or `find`, which are not on the allowlist.

## Statement 3 — No worktree isolation

The executor runs without worktree isolation, because the Bash tool's isolation filter refuses `pwsh` and this plan invokes `pwsh` throughout. The inherited working directory is therefore the coordinator session worktree, and every path is made absolute or is governed by the worktree prologue.

## Statement 4 — pwsh quoting shape

A `pwsh` command payload uses outer single quotes and inner double quotes, in one `pwsh -NoProfile -Command` invocation.

## Statement 5 — File size ceiling

No script or test file may exceed 500 lines.

## Statement 6 — PowerShell test-purity hook

`.claude/hooks/check-powershell-test-purity.ps1` denies a write to any test file whose content matches a temporary-file API, a temporary-path environment variable, a network API, a socket API, a process-start call, a sleep call, or a direct mock of `git`, `gh` or `actionlint`. The pattern table was read at lines 99 through 117 of that file and holds exactly those classes: `Mock git`, `Mock gh`, `Mock actionlint` and their quoted forms; `New-TemporaryFile`, `[System.IO.Path]::GetTempFileName`, `[System.IO.Path]::GetTempPath`, `$env:TEMP`, `$env:TMP`; `Invoke-WebRequest`, `Invoke-RestMethod`, `[System.Net.Http.`, `[System.Net.WebRequest]`, `[System.Net.Sockets.`; `Start-Process`; and `Start-Sleep`. Tests therefore mock the named wrapper seam and never the executable.

## Statement 7 — Pester helper scoping

A Pester helper function is defined inside the setup block and never at file scope, because each test case runs in a child scope of the containing block.

## Statement 8 — No Python toolchain

There is no Python toolchain in this repository, so no `poetry run python` step is performed anywhere in this plan.

## Statement 9 — Commit pathspecs and the pre-implementation gate

Every commit task in this plan names non-exempt pathspecs (`.github/workflows`, `scripts/vscode`, `tests/scripts/vscode`, and the active feature folder) and therefore requires the orchestration checkpoint to be present. If the first commit is denied by the pre-implementation gate, the executor halts and reports rather than reshaping the pathspec to an exempt form.

## Statement 10 — Byte-order mark on new PowerShell files carrying non-ASCII

A newly created PowerShell file that carries any non-ASCII character must be written with a UTF-8 byte-order mark, because the analyzer reports a new diagnostic otherwise and the per-batch analyzer gate is judged against the baseline diagnostic count recorded in P0-T12.

## Statement 11 — Reachability probe

Command: `pwsh -NoProfile -Command '<worktree prologue>; "PROBE OK"'`
EXIT_CODE: 0
Output: `PROBE OK`

The Bash tool did not refuse the probe, so the executor is not running under worktree isolation and the later `pwsh` tasks in this plan are reachable. The authorized halt branch for a refused probe did not fire.

## Statement 12 — Batch 1 change-budget reset, and the measured hook mechanics that bound it

Command: `pwsh -NoProfile -Command '<worktree prologue>; Get-ChildItem -Path ".claude/state" -Filter "powershell-batch-budget.*.json" -ErrorAction SilentlyContinue | Remove-Item -Force; Write-Output "BATCH BUDGET RESET"'`
EXIT_CODE: 0
Output carried the line: `BATCH BUDGET RESET`

Command: `pwsh -NoProfile -Command '<worktree prologue>; @(Get-ChildItem -Path ".claude/state" -Filter "powershell-batch-budget.*.json" -ErrorAction SilentlyContinue).Count'`
EXIT_CODE: 0
Output: `0`

Measured hook mechanics, re-derived against this worktree in this pass:

- `.claude/hooks/enforce-powershell-batch-budget.ps1` declares the `$Root` parameter default as `(Split-Path (Split-Path $PSScriptRoot -Parent) -Parent)` at line 269 and again at line 315, so the hook derives its state root from its own script location.
- Line 352 of that file forms the state directory as that root joined with `.claude/state`.
- Lines 279 through 281 discard an out-of-root candidate by returning an `allow` decision that consumes no slot and writes no state. The in-file comment at lines 277 and 278 states that intent explicitly.
- `.claude/settings.json` registers the hook at line 144 by the relative path `.claude/hooks/enforce-powershell-batch-budget.ps1`, which resolves against the session project directory rather than against the item worktree.

Under either resolution the reset's observables are uninformative. If the session copy runs, every write under the item worktree is out of its root and is allowed unconditionally at lines 279 to 281, so no denial can occur at all. If the item copy runs, a denial would mean a genuinely refilled cap rather than a mis-rooted state directory. Neither a `BATCH BUDGET RESET` line, nor a `.Count` of 0, nor the absence of a denial is therefore evidence that the reset governed the hook. The resets are recorded as a precaution whose effect is unobservable from the item worktree, and no task in this plan depends on them binding.

Authorized halt branch, recorded for later reference: if a PowerShell file write is denied by the batch-budget hook at any point in this plan, the executor records the denied path together with the hook's reported state-file path, which the denial reason carries because the reason string built at line 296 of that hook interpolates `$StateFile`, and halts. That reason string is the only observable that names the hook's actual state root. No further resets are issued and the write is not reshaped.

Side effect of the reset command itself, the same in all five resets: the state directory carries a gitignore entry, but one budget file in it was committed before that entry existed and is therefore tracked, so the reset's deletion produces an unstaged deletion in the unscoped porcelain output. That path lies outside the four roots `.github`, `scripts`, `tests` and `docs`, so it affects neither the scoped assertion in P0-T3 nor the four-root assertion in P10-T13 nor the anchored diff clause of P10-T14. P10-T14 restores it before its unscoped porcelain assertion runs.

## Statement 13 — Resolved item worktree root and branch

Item worktree root, host-specific leading directories redacted: `<repo-parent>/bugs-2026-09-11-item-869`.

Command: `git -C "<item worktree root>" rev-parse --abbrev-ref HEAD`
EXIT_CODE: 0
Output verbatim: `bug/ci-coverage-threshold-and-pester-gates-869`

This is exactly the required branch name, so the wrong-worktree halt branch did not fire and P0-T3 may create the anchor tag.

## Statement 14 — Contrasting pre-fix observation

The same query issued without the repository-directory switch, and therefore against the inherited working directory, prints `TaskMaster-wt-2026-09-12T10-15`. That value was measured by the executor in this run, not relayed: the command `git rev-parse --abbrev-ref HEAD` was issued with no repository-directory argument and returned that string. It names the coordinator session worktree's branch rather than this item's branch. The two values are therefore an observed pair, one measured without the working-directory rule applied and one required with it, and not a restatement of the rule.

## Statement 15 — Mechanical self-check on the plan document

Tool: Grep, count mode.
Target: `docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/plan.2026-09-12T10-25.md`
Pattern: git's repository-directory switch followed by a bare dot, written with the dot escaped so that it matches a literal dot and not any character.
Count: 0

The unescaped spelling was not used, because an unescaped dot matches the opening quote of the replacement form and would return a non-zero count on a fully repaired plan. This is a standing regression guard rather than a gate that can fail during this run: the figures 19 matching lines carrying 46 occurrences describe the plan document as it stood before the second revision round repaired it, and the repaired document carries 0 matching lines. The required count is 0 and the measured count is 0.

Output Summary: all fifteen statements recorded. The reachability probe returned `PROBE OK`, the branch check returned the required branch name, both batch-budget reset observables were as stated, and the plan self-check counted 0. No halt branch fired.
