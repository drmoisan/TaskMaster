# P0-T17 Identifier and raw-document baseline (counts only)

Timestamp: 2026-09-29T09-10
Command: GATE1, GATE2, GATE3, GATE4, GATE5A, GATE5B, GATE5C, GATE9, GATE10 (plan "Gate command reference"); pwsh -NoProfile -Command '"TRACKED-TOTAL=" + @(git ls-files).Count'; the forbidden-extension probe, the UTF-16 census, the raw-population overlap, the projection census, the fixture census and the legacy-user any-case census, each exactly as written in P0-T17. All identifier values were derived at run time from the environment; no identifier value is written in this file.
EXIT_CODE: 0
Output Summary:
- Every gate observed failing before the change: GATE1=1213, GATE2=183, GATE4=1213, GATE5A=580, GATE5B=598, GATE5C=270, GATE9=88 (each greater than 0).
- GATE3=6 (an integer; the short name was non-empty).
- Differences from the plan's recorded expectations (expectations, not gates): GATE1 1213 against 1,215; GATE2 183 against 186; UTF16-PROFILE-FILES 0 against 1. All other expected figures matched (GATE4 1,213, GATE5A 580, GATE5B 598, GATE5C 270, GATE9 88, RAW-OVERLAP 243, RAW-UNION 625, PROJECTIONS 18, PREFIX-SITES 6, CS-PATTERN-LINES 37, PS1-PATTERN-LINES 4, UTF16-FILES 1).
- UTF-16 diagnostic (count-only): the single UTF-16 file (docs/features/archive/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/2026-05-14T12-41-05Z/msbuild-analyzers.txt) decodes to 33985 characters containing 151 occurrences of the word users and 0 occurrences of a drive letter followed by a colon and a separator, so it carries no match of the generic profile-path pattern. PROFILE-PATH-FILES is therefore GATE4 plus 0.
- FORBIDDEN=0: no solution, project, props, targets, packages.config, app.config or production C# file carries an identifier (no STOP: IDENTIFIER IN BUILD INPUT).
- LEGACY-USER-ANYCASE-FILES=103 is recorded and not gated (D18).
- The gates ran over the tracked tree at BASE-SHA with the plan-file check-off edits and the untracked Phase 0 artifacts present; git grep reads tracked paths only, so the untracked artifacts were not in scope of these counts.

GATE1: 1213
GATE2: 183
GATE3: 6
GATE4: 1213
GATE5A: 580
GATE5B: 598
GATE5C: 270
GATE9: 88
GATE10: 44
TRACKED-TOTAL: 16590
UNION: 1310
FORBIDDEN: 0
UTF16-FILES: 1
UTF16-PROFILE-FILES: 0
UTF16-IDENTIFIER-FILES: 1
RAW-OVERLAP: 243
RAW-UNION: 625
REPORT-ROOTS: 45
PROJECTIONS: 18
PREFIX-SITES: 6
CS-PATTERN-LINES: 37
PS1-PATTERN-LINES: 4
LEGACY-USER-ANYCASE-FILES: 103

RAW-POPULATION: 625
PROFILE-PATH-FILES: 1213

The eighteen retained package-level projections:

PROJECTION| docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/baseline/baseline-coverage.jacoco.xml
PROJECTION| docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/qa-gates/final-coverage.jacoco.xml
PROJECTION| docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/baseline/p0-t15-coverage.jacoco.xml
PROJECTION| docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/qa-gates/p2-t7-coverage.jacoco.xml
PROJECTION| docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/coverage-baseline.jacoco.xml
PROJECTION| docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/coverage-post-change.jacoco.xml
PROJECTION| docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/evidence/baseline/coverage-baseline.jacoco.2026-09-05T10-49.xml
PROJECTION| docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/evidence/qa-gates/coverage-final.jacoco.2026-09-05T10-49.xml
PROJECTION| docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p2-t7-coverage-projection.2026-09-19T09-44.jacoco.xml
PROJECTION| docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p5-t7-coverage-projection.2026-09-20T01-37.jacoco.xml
PROJECTION| docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-coverage-projection.2026-09-19T09-44.jacoco.xml
PROJECTION| docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/remediation-baseline/p0-t12-coverage-projection.2026-09-20T01-37.jacoco.xml
PROJECTION| docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/evidence/baseline/coverage-baseline.jacoco.xml
PROJECTION| docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/evidence/qa-gates/coverage-final.jacoco.xml
PROJECTION| docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/evidence/qa-gates/coverage-remediation-final.jacoco.xml
PROJECTION| docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/evidence/remediation-baseline/coverage-remediation-baseline.jacoco.xml
PROJECTION| docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/baseline/coverage-baseline.jacoco.xml
PROJECTION| docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/qa-gates/coverage-postchange.jacoco.xml
