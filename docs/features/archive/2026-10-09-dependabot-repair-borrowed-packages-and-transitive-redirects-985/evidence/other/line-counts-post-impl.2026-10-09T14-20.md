# Post-implementation Line Counts (P3-T3)

Timestamp: 2026-10-09T14-20
Command: Grep tool, pattern `^`, output mode count, over scripts/dependencies/BindingRedirectSync.psm1 and scripts/dependencies/Repair-PackageManifestConsistency.ps1
EXIT_CODE: 0
Output Summary:
- scripts/dependencies/BindingRedirectSync.psm1: 307 lines (at most 500)
- scripts/dependencies/Repair-PackageManifestConsistency.ps1: 493 lines (at most 500; above the 475 baseline)
- Result: both within the 500-line limit; no SIZE-LIMIT.

## P3-T1 static checks (BindingRedirectSync.psm1)

- `^Export-ModuleMember` count 1 (line 303)
- `function` lines: Invoke-BindingRedirectSync (75), Invoke-SolutionBindingRedirectSync (170), Format-BindingRedirectSyncReport (276), one each
- `Import-Module .*-Force` count 0
- `[^\x00-\x7F]` count 0

## P3-T2 static checks (Repair-PackageManifestConsistency.ps1)

- `BindingRedirectSync\.psm1` 1 (line 73)
- `Invoke-SolutionBindingRedirectSync` 1 (line 449)
- `-WhatIf:\$WhatIfPreference` 2 (lines 450 and 456)
- `RedirectSync\s+=` 1 (line 487)
- `Format-BindingRedirectSyncReport` 1 (line 464)
- `\$projectTextOverride\[` 1 (line 421)
- The new call sits after the manifest loop and before normalisation, with no `-CandidateUpgrade` condition.
- Carriage-return count equals line count (493), so CRLF line endings are preserved.
