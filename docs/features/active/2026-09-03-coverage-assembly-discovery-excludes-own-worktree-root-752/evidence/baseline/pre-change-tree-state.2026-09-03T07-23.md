# Pre-change Tree State ([P0-T4])

Timestamp: 2026-09-03T11-51

Command:
1. `git -C <repo-root> merge-base origin/main HEAD`
2. `git -C <repo-root> status --porcelain -uall -- scripts/vscode tests/scripts/vscode`
3. `git -C <repo-root> diff --stat 87233f867ad60c0a5c0d19b09cc121ae536d7ba1 -- scripts/vscode tests/scripts/vscode`
4. `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $f = @("scripts/vscode/Invoke-MSTestWithCoverage.ps1","tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1"); foreach ($x in $f) { "LINECOUNT " + $x + " " + (Get-Content -LiteralPath $x).Count }; exit 0'`

EXIT_CODE: 0

MERGE BASE SHA: 87233f867ad60c0a5c0d19b09cc121ae536d7ba1

## Scoped porcelain output (command 2), verbatim

```
```

(zero lines)

## Merge-base diffstat output (command 3), verbatim

```
```

(zero lines; expected — this branch carries exactly one commit past the merge base and it is a documentation-only feature-folder preparation commit, so the merge base and HEAD are identical over these two directories)

## Line counts (command 4), verbatim

```
LINECOUNT scripts/vscode/Invoke-MSTestWithCoverage.ps1 350
LINECOUNT tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 488
```

Output Summary: PRE-CHANGE SOURCE PORCELAIN: EMPTY. The merge-base `diff --stat` over `scripts/vscode` and `tests/scripts/vscode` is empty, which confirms the merge base and HEAD are byte-identical over those two directories and licenses `[P1-T7]`'s substitution of `HEAD` for the recorded merge base. Both `LINECOUNT` values match the figures this plan asserts (350 and 488).
