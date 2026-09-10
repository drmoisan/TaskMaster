# P0-T11 — Threshold And AC7 Fixture Baseline

Timestamp: 2026-09-09T10-50
Task: [P0-T11]
EXIT_CODE: 0

## Command 1 — SHA-256 pins

Command: `pwsh -NoProfile -Command '(Get-FileHash -Algorithm SHA256 -LiteralPath "scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1").Hash; (Get-FileHash -Algorithm SHA256 -LiteralPath "docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml").Hash'`
EXIT_CODE: 0

| File | SHA-256 |
| --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | `00D96099A91DC7B431414F27F3A3FD3A6BDAE4F8D9B8961CEFEA5F1782ED79C2` |
| `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml` | `803C4030F4FAD13352B694EEB7CBF9819D91CEF49A23ABBFDD77221E5D8E46BA` |

Both values are 64-character hexadecimal strings.

## Command 2 — the pinned failure message

Command: `git grep -c -F -e 'is below the required 80' -- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`
EXIT_CODE: 0

```
scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1:1
```

Exactly one line, count 1.

Output Summary: Both SHA-256 values were recorded as 64-character hexadecimal strings, and the
`git grep` output is exactly one line with count 1, pinning the existing threshold failure message
before any change is made. The threshold script hash `00D96099...ED79C2` is the value P4-T6
re-verifies to establish that no threshold constant or failure message was altered. The Cobertura
document hash `803C4030...5D8E46BA` is the value P3-T3 records alongside its corroborating
measurement, so a third party can confirm both aggregations ran over the identical document.
