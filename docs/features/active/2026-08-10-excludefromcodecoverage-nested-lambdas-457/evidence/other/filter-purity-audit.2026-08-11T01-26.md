# [P2-T11] Filter purity audit

Timestamp: 2026-08-11T01-26
Command: `pwsh -NoProfile -File <scratchpad>/filter-purity.ps1` — a literal (regex-escaped)
substring count for each pattern over the full text of
`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`
EXIT_CODE: 0

Target file: `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` (387 lines)

## Pattern search results

### Filesystem access

| Pattern | Matches |
|---|---|
| `Get-Content` | 0 |
| `Set-Content` | 0 |
| `Add-Content` | 0 |
| `Out-File` | 0 |
| `Import-Csv` | 0 |
| `Export-` | 0 |
| `Get-ChildItem` | 0 |
| `Test-Path` | 0 |
| `Resolve-Path` | 0 |
| `[System.IO.File]` | 0 |
| `[System.IO.Directory]` | 0 |

### Process invocation

| Pattern | Matches |
|---|---|
| `Start-Process` | 0 |
| `Invoke-Expression` | 0 |
| `Invoke-Command` | 0 |

### Clock and entropy reads

| Pattern | Matches |
|---|---|
| `Get-Date` | 0 |
| `[datetime]::Now` | 0 |
| `[datetime]::UtcNow` | 0 |
| `Get-Random` | 0 |
| `[guid]::NewGuid` | 0 |

### Network calls

| Pattern | Matches |
|---|---|
| `Invoke-WebRequest` | 0 |
| `Invoke-RestMethod` | 0 |
| `System.Net` | 0 |

TOTAL_MATCHES: **0**
VERDICT: **PURE (zero matches across all 22 patterns)**

## Corroboration

The static pattern scan is corroborated behaviourally by regression case 10
(`Cobertura closure name derivation.is idempotent and silent when applied twice to the same
document`), which asserts that two invocations of `Remove-CoberturaExemptClosureCoverage` emit zero
objects on the success stream and zero records on the error, warning, verbose and information
streams, and that the second pass produces no further change to the document. Case 9 asserts the same
stream silence for `Get-CoberturaClosureDeclaringMemberName` across all seven of its inputs.

Together these establish spec AC 11: the filter is a pure XML-to-XML transform that reads no file,
invokes no process, reads no clock, and makes no network call, and running it twice over the same
document produces no further change.

## Output Summary

All 22 prohibited patterns return zero matches against
`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`. The module's only external
dependencies are the two pure helpers it reuses from
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
(`Get-CoberturaLineConditionCoverageParts` and `Get-CoberturaClassLineSummary`), both of which are
themselves documented as pure and perform no I/O.
