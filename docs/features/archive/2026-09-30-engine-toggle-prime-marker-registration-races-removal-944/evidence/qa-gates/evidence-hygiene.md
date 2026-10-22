# Evidence Hygiene (P3-T13)

Timestamp: 2026-09-30T15-17
Command: the P3-T13 sweep over every *.md file under docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944 (account name and machine name derived at run time and not recorded; drive-path check on the backslash-normalised text with the CI hygiene guard's user-profile pattern, case-insensitive)
EXIT_CODE: 0
Output Summary: FILES_SCANNED=42 ACCOUNT_HITS=0 MACHINE_HITS=0 DRIVE_USERS_HITS=0. FILES_SCANNED meets the floor of 42 (spec, issue, research, the plan and 38 artifacts: twenty under baseline, five under regression-testing and thirteen under qa-gates). Every P3-T13 clause holds; no repair was needed. Substitution: the output line is printed by string concatenation rather than an interpolated string; the counting expressions are unchanged.

PRE-COMMIT-HYGIENE: (P3-T35, Timestamp: 2026-09-30T15-23, the P3-T13 command re-run unchanged before staging) FILES_SCANNED=45 ACCOUNT_HITS=0 MACHINE_HITS=0 DRIVE_USERS_HITS=0. No repair was needed.
