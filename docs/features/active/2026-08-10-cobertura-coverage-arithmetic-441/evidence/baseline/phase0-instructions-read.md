# Phase 0 — Instructions Read (2026-08-10-cobertura-coverage-arithmetic-441)

Timestamp: 2026-08-10T22-30

Policy Order: `CLAUDE.md` -> `.claude/rules/general-code-change.md` -> `.claude/rules/general-unit-test.md` -> `.claude/rules/powershell.md`

## Resolved `<ROOT>`

- `git rev-parse --show-toplevel` output: `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a1cc35d4011888c2a`
- Resolved `<ROOT>` (backslash form): `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a`
- Resolution acceptance: the directory contains both
  `scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1` and
  `docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\spec.md`. Both were read in
  full during Phase 0, which proves their presence.

## Files read (P0-T1 .. P0-T7)

| Task | File |
| --- | --- |
| P0-T1 | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a\CLAUDE.md` |
| P0-T2 | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a\.claude\rules\general-code-change.md` |
| P0-T3 | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a\.claude\rules\general-unit-test.md` |
| P0-T4 | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a\.claude\rules\powershell.md` |
| P0-T5 | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a\docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\spec.md` |
| P0-T6 | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a\docs\features\active\2026-08-10-cobertura-coverage-arithmetic-441\research\2026-08-10T14-20-cobertura-arithmetic-research.md` |
| P0-T7 | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1` |
| P0-T7 | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a\tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1` |

Seven distinct policy/spec/research/source paths are enumerated above (P0-T7 contributes two source
files, giving eight rows across seven tasks).

## P0-T5 — Acceptance-criteria count

`spec.md` § Acceptance Criteria contains exactly **20** unchecked AC items, numbered **AC-1 through
AC-20**. Verified by direct read of `spec.md:635-718`. Work mode is `full-bug`, so `spec.md` is the
sole authoritative AC source.

## P0-T7 — Pre-change line counts

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
foreach ($p in @(
    'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1',
    'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1')) {
    '{0}: {1}' -f $p, (Get-Content -LiteralPath (Join-Path $root $p)).Count
}
```

EXIT_CODE: 0

Output Summary:

```
scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1: 357
tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1: 222
```

Both figures match the plan's stated expectation exactly (357 and 222). No re-baselining is
required.
