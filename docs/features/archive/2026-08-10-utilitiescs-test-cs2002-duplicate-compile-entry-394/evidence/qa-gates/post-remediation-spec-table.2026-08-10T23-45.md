Timestamp: 2026-08-10T23-45

Command: `pwsh -NoProfile -Command "Select-String -Path 'docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/spec.md' -Pattern 'Analyzer.*\||Reference.*\||packages.config.*\|'"` (same command as P0-T6)

EXIT_CODE: 0

Raw matched lines:
```
| `Reference` | 126 | none (every `Include` assembly-name token is distinct) |
| `ProjectReference` | 2 | none |
| `Analyzer` | 11 | none |
| `PackageReference` | 0 | not applicable - legacy `packages.config`-style project, `PackageReference` is not used |
| `packages.config` `<package>` | 105 | none |
```

Output Summary: Post-remediation `spec.md` Root Cause Analysis "Duplicate Sweep Result" table shows all three corrected figures: `Analyzer` = 11, `Reference` = 126, `packages.config` = 105, matching the authoritative sweep totals in `<FEATURE>/evidence/baseline/duplicate-sweep.2026-08-10T22-31.md`. No `Duplicates found` cell changed from `none` relative to P0-T6's pre-remediation capture (`pre-remediation-spec-table.2026-08-10T23-45.md`), which recorded the same three rows as `none` before the value corrections. Acceptance satisfied.
