# Production Line Counts and Implementation Counts (R1, issue #985)

Timestamp: 2026-10-09T15-23
Command: Grep tool counts (`^`, `\r$`, and the P3-T1 / P3-T2 tokens) over scripts/dependencies/BindingRedirectSync.psm1 and scripts/dependencies/Repair-PackageManifestConsistency.ps1
EXIT_CODE: 0
Output Summary:
- `scripts/dependencies/BindingRedirectSync.psm1`: 328 lines (limit 500), 0 carriage returns (LF kept).
- `scripts/dependencies/Repair-PackageManifestConsistency.ps1`: 495 lines (limit 500; expected 495), 495 carriage returns (CRLF kept).
- No SIZE-LIMIT condition.

P3-T1 counts (`BindingRedirectSync.psm1`):
| Pattern | Expected | Observed |
|---|---|---|
| `function Get-RedirectDirection` | 1 | 1 (line 76) |
| `HashSet\[string\]\]::new\(\[System\.StringComparer\]::OrdinalIgnoreCase\)` | 1 | 1 (line 135) |
| `\$handled\.Contains\(` | 0 | 0 |
| `if \(-not \$handled\.Add\(` | 1 | 1 (line 142) |
| `Direction\s+= \(Get-RedirectDirection` | 1 | 1 (line 175) |
| `\(\{4\}, \{5\}\)` | 1 | 1 (line 318) |
| `'Get-RedirectDirection'` (not exported) | 0 | 0 |
| `^Export-ModuleMember` | 1 | 1 (line 324) |
| `[^\x00-\x7F]` | 0 | 0 |

P3-T2 counts (`Repair-PackageManifestConsistency.ps1`):
| Pattern | Expected | Observed |
|---|---|---|
| `distinctWritten` | 2 | 2 (lines 467, 476) |
| `OrdinalIgnoreCase` on the `$distinctWritten =` line | at least 1 | 1 (line 467, read: `[System.StringComparer]::OrdinalIgnoreCase`) |
| `\$written\.ToArray\(\)` | 0 | 0 |
| `\r$` equal to line count | 495 | 495 |

Note: a two-line reflow of the `.DESCRIPTION` paragraph of `Invoke-BindingRedirectSync` was made after the item-6 wording change, so the paragraph wraps at the file's existing width; the counts above were taken after that reflow.
