# Pre-change Tree Facts (R1, issue #985)

Timestamp: 2026-10-09T15-15
Command: Grep tool (count / content -o) over the four Write Set PowerShell files and FEATURE/plan.2026-10-09T13-06.md
EXIT_CODE: 0
Output Summary:
- All 14 observed values equal the plan's parenthesised values; no anchor re-derivation required.

| Fact | Pattern | Expected | Observed |
|---|---|---|---|
| Lines, `scripts/dependencies/BindingRedirectSync.psm1` | `^` | 306 | 306 |
| Lines, `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | `^` | 493 | 493 |
| Lines, `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` | `^` | 358 | 358 |
| Lines, `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` | `^` | 187 | 187 |
| CR, `BindingRedirectSync.psm1` | `\r$` | 0 | 0 |
| CR, `Repair-PackageManifestConsistency.ps1` | `\r$` | 493 | 493 |
| CR, `BindingRedirectSync.Tests.ps1` | `\r$` | 0 | 0 |
| CR, `Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` | `\r$` | 0 | 0 |
| `BindingRedirectSync.psm1` | `\$handled\.Contains\(` | 1 | 1 (line 121) |
| `BindingRedirectSync.psm1` | `function Get-RedirectDirection` | 0 | 0 |
| `BindingRedirectSync.psm1` | `Direction` | 0 | 0 |
| `Repair-PackageManifestConsistency.ps1` | `distinctWritten` | 0 | 0 |
| `plan.2026-10-09T13-06.md` (-o) | `claude.C--Users-` | 2, both line 26 | 2, both line 26 |
| `plan.2026-10-09T13-06.md` (-o) | `<encoded-worktree>` | 0 | 0 |
