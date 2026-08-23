# Post-Format File-Size Audit (P4-T5)

Timestamp: 2026-08-10T23-10

Enforces the 500-line file ceiling in `.claude/rules/general-code-change.md` § File Size Limit,
which applies to production code and test code alike. Measured **after** the P4-T1 format step, so
the figures are the ones that will be committed.

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
Get-ChildItem -LiteralPath `
    'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1', `
    'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1' |
    ForEach-Object { '{0}: {1}' -f $_.Name, (Get-Content -LiteralPath $_.FullName).Count }
```

EXIT_CODE: 0

Output Summary:

```
Invoke-MSTestWithCoverage.Helpers.ps1: 455
Invoke-MSTestWithCoverage.Helpers.Tests.ps1: 468
```

| File | Pre-change | Post-change | Ceiling | Headroom | Verdict |
| --- | --- | --- | --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 357 | **455** | 500 | 45 | **PASS** |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | 222 | **468** | 500 | 32 | **PASS** |

Both files are under 500 lines. The production file's net addition is +98 lines (99-line helper
inserted, 11-line inner loop replaced by 5 lines, 7-line synthetic-document block replaced by
10 lines), consistent with the spec's 50-70 line estimate order of magnitude and comfortably inside
the ceiling.

The test file grew by 246 lines and remains a single file: AC-18 pins the diff to exactly two source
files, so the budget was met by compacting the fixture here-strings per the plan's § Test-File Line
Budget rather than by adding a third test file. Per-block line counts, all within budget: F1 = 24
(<= 24), F2 = 28 (<= 28), F3 = 34 (<= 34), F4 = 22 (<= 26), F5 = 19 (<= 24), F6 = 34 (<= 34), and
the `Describe 'Get-CoberturaClassLineSummary'` block spans lines 401-468 = **68** lines (<= 80).
