# Baseline — Environment Preconditions (Issue #449, [P0-T6])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command: see the per-finding `Command:` lines below; each finding names the exact command that
established it. Aggregate exit code for the verification set:
EXIT_CODE: 0

---

## (a) No Python toolchain — `poetry run python -m scripts.dev_tools.*` is unrunnable by absence

Command: `ls -d scripts/dev_tools`
EXIT_CODE: 2
Output: `ls: cannot access 'scripts/dev_tools': No such file or directory`

Command: `ls -1 pyproject.toml poetry.lock`
EXIT_CODE: 2
Output: `ls: cannot access 'pyproject.toml': No such file or directory` /
`ls: cannot access 'poetry.lock': No such file or directory`

Command: `ls -1 scripts/`
EXIT_CODE: 0
Output: `dev-tools/`, `temp-extract-coverage.ps1`, `vscode/`

Command: `ls -1 scripts/dev-tools/`
EXIT_CODE: 0
Output: `run-actionlint.ps1`

Command: `git ls-files "*.py"`
EXIT_CODE: 0
Output (2 files, both inside an ARCHIVED feature folder, neither a dev-tools module):
```
docs/features/archive/2026-07-18-stale-app-config-binding-redirects-354/scripts/fix_binding_redirects.py
docs/features/archive/2026-07-18-stale-app-config-binding-redirects-354/tests/scripts/test_fix_binding_redirects.py
```

**Finding.** There is no `scripts/dev_tools/` directory (the only similar path is
`scripts/dev-tools/`, hyphenated, containing a single PowerShell script), no `pyproject.toml`, and no
`poetry.lock`. The importable package `scripts.dev_tools` therefore does not exist, and there is no
Poetry environment to run it in. Any skill or process step naming
`poetry run python -m scripts.dev_tools.*` is **unrunnable by absence** in this repository. It is
recorded here as such; no result is fabricated for it and it is not silently omitted. See [P7-T15]
for the corresponding final-QC record.

## (b) No `quality-tiers.yml` at the WORKTREE root — no QuickFiler tier classification can be cited

Command: `ls -1 quality-tiers.yml`
EXIT_CODE: 2
Output: `ls: cannot access 'quality-tiers.yml': No such file or directory`

**Finding.** `.claude/rules/quality-tiers.md` names `quality-tiers.yml` at the repository root as the
source of truth for the T1-T4 tier map, but that file does not exist in this repository. No tier
classification for `QuickFiler.csproj` can be cited, and no tier-dependent gate (property-test
density, mutation score, untyped-escape-hatch budget) is enforceable here. This child cites no tier.

## (c) The only machine-enforced numeric coverage gate is the repo-wide 80% line rate

Command: `grep -rn -E "below the required|branch-rate|branchRate|-lt 8[0-9]|-lt 7[0-9]|-lt 9[0-9]" scripts/`
EXIT_CODE: 0
Output:
```
scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1:386:  $classNode.SetAttribute('branch-rate', $retainedBranchRate)
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:375:      $mergedClassNode.SetAttribute('branch-rate', $mergedBranchRate)
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:443:      $xml.coverage.SetAttribute('branch-rate', $coverageSummary.BranchRate)
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487:      if ($percentage -lt 80) {
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:489:      throw "Cobertura line coverage $formattedPercentage% is below the required 80% threshold."
```

Command: `sed -n '480,495p' scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
EXIT_CODE: 0
Output:
```powershell
    if ($lineRate -lt 0 -or $lineRate -gt 1) {
        throw 'Cobertura line-rate must be between 0 and 1.'
    }

    $percentage = $lineRate * 100
    if ($percentage -lt 80) {
        $formattedPercentage = $percentage.ToString('0.####', [System.Globalization.CultureInfo]::InvariantCulture)
        throw "Cobertura line coverage $formattedPercentage% is below the required 80% threshold."
    }
```

**Finding.** The single machine-enforced numeric coverage threshold anywhere under `scripts/` is the
repo-wide Cobertura root `line-rate` compared against 80% at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489`. The three other `branch-rate`
occurrences are attribute WRITES during Cobertura post-processing, not comparisons, so no
branch-coverage gate exists. There is no per-file gate, no per-assembly gate, and no per-class gate.
Consequently the `QuickFiler` package figure and the `QfcExplorerController` figure recorded by this
plan are reported values, not gated values, and the plan's [P7-T9]/[P7-T10] treatment of them as
delta comparisons rather than absolute threshold checks is correct.

## (d) PreToolUse hooks are inert; no gate in this plan relies on one

Command: `grep -rln 'toolInput.command' .claude/hooks/`
EXIT_CODE: 0
Output:
```
.claude/hooks/enforce-epic-merge-gate.ps1
.claude/hooks/enforce-epic-worktree-removal-gate.ps1
.claude/hooks/enforce-parallel-abandon-gate.ps1
.claude/hooks/enforce-parallel-worktree-removal-gate.ps1
.claude/hooks/enforce-pr-author-skill.ps1
.claude/hooks/enforce-promotion-mcp-only.ps1
```

Command: `grep -rln 'tool_input.command' .claude/hooks/`
EXIT_CODE: 1
Output: (no match)

**Finding.** Every command-inspecting hook reads `$toolInput.command`, and no hook reads the nested
`$toolInput.tool_input.command` where the payload actually carries the value. Each therefore sees an
empty command string and returns `permissionDecision: allow`. No gate in this plan relies on a hook:
every gate in Phases 1 through 7 is verified from durable `git` state, from a command's own exit
code, or from a parsed build/coverage artifact. `.claude/**` is read-only for this child, so no hook
is repaired here.

---

## Output Summary

All four preconditions verified. (a) No Python toolchain exists: no `scripts/dev_tools/`, no
`pyproject.toml`, no `poetry.lock`; the two tracked `.py` files live in an archived feature folder.
Python steps are recorded as unrunnable by absence rather than fabricated or skipped. (b)
`quality-tiers.yml` is absent from the WORKTREE root, so no QuickFiler tier is cited. (c) The only
machine-enforced numeric coverage gate under `scripts/` is the repo-wide 80% line rate at
`Invoke-MSTestWithCoverage.Helpers.ps1:487-489`; there is no per-file, per-assembly, or
branch-coverage gate. (d) All six command-inspecting PreToolUse hooks read the wrong payload key and
are inert; no gate in this plan depends on one.
