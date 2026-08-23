Timestamp: 2026-08-10T23-45

Command: `pwsh -NoProfile -Command "Select-String -Path 'docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/spec.md' -Pattern 'Analyzer.*\||Reference.*\||packages.config.*\|'"` (repository-root-relative path substituted for `<FEATURE>`)

EXIT_CODE: 0

Raw matched lines:
```
| `Reference` | ~114 | none (every `Include` assembly-name token is distinct) |
| `ProjectReference` | 2 | none |
| `Analyzer` | 9 | none |
| `PackageReference` | 0 | not applicable - legacy `packages.config`-style project, `PackageReference` is not used |
| `packages.config` `<package>` | ~99 | none |
```

Output Summary: Pre-remediation `spec.md` Root Cause Analysis "Duplicate Sweep Result" table shows the stale values `Analyzer` = 9, `Reference` = ~114, `packages.config` = ~99, matching the discrepancy cited in `code-review.2026-08-10T23-45.md` against the authoritative sweep totals recorded in `<FEATURE>/evidence/baseline/duplicate-sweep.2026-08-10T22-31.md` (Analyzer Total=11, Reference Total=126, packages.config Total=105). This is the "before" state that Phase 2's P2-T4 verification task must show has been corrected.
