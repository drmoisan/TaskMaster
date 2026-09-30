# P2-T11 — Hygiene scan of the final footprint (CMD-HYGIENE)

Timestamp: 2026-09-30T11-10
Command: CMD-HYGIENE with FOLDER-LIST "docs/features/active/2026-09-28-package-manifest-consistency-residuals-929",".github/workflows/dependabot-repair.yml",".github/workflows/README.md","docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks","tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1" (account and machine values derived at run time and not recorded); plus a count of evidence .md files under the feature folder
EXIT_CODE: 0
Output Summary:
- PATTERNS=3
- SELFTEST=1
- SELFTEST_NEG=0
- SCANNED=61 HITS=0 (at least 45)
- Hit listing: empty
- PRE-FIX-HITS: 0
- Evidence .md artifacts under the feature folder before this artifact: 51 — the 44 the plan counts for a single-iteration run plus the seven iteration 2 artifacts of P2-T1 to P2-T7 (the plan states that a further iteration only raises the count)
- The two P2-T7 copies (projection and summary) and the two P0-T12 copies are included in the scan because their extensions are in the filter list.
