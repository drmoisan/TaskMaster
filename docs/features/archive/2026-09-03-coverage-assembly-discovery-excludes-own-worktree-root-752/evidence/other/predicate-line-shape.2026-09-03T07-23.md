# Predicate Line Shape ([P2-T3])

Timestamp: 2026-09-03T12-07

Command: `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $all = Get-Content -LiteralPath "scripts/vscode/Invoke-MSTestWithCoverage.ps1"; "LINECOUNT=" + $all.Count; $line = $all[300]; "INDENT=" + ($line.Length - $line.TrimStart().Length); "TRIMMED=" + $line.TrimStart(); exit 0'`

EXIT_CODE: 0

## Emitted lines, verbatim (all three)

```
LINECOUNT=350
INDENT=16
TRIMMED=([System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)) -notmatch '(^|\\)\.claude\\'
```

Output Summary: The file is still 350 lines, so the edit displaced no other line; the zero-based index 300 addresses file line 301. The edited line carries 16 spaces of indentation, matching the three sibling clauses on lines 298 through 300 inside the `Where-Object` block opened at 12 spaces on line 297. The `TRIMMED=` value is character-for-character equal to the replacement expression this plan mandates, including both doubled backslash sequences in `(^|\\)` and in the trailing `\\`, so no editing layer collapsed a doubled backslash into the invalid regular expression `(^|\)\.claude\`. This command was run once immediately after the `[P2-T1]` edit as that task's hardening check, and again here to produce this record; both runs emitted identical values.
