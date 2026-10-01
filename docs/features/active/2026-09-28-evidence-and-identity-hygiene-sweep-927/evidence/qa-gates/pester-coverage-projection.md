# P6-T38 CI Pester coverage projection (AC4)

Timestamp: 2026-09-29T23-37
Command: P6-T38 steps (1) to (5) as the plan states them, with RUN-ID transcribed as 36664415704: (1) GATE6; (2) the gh pr view / gh run list / gh run view --json jobs payload; (3) pwsh -NoProfile -Command 'gh run view 36664415704 --repo drmoisan/TaskMaster --log 2>&1 | Where-Object { $_ -match "PESTER Passed=[0-9]+ Failed=[0-9]+ Skipped=[0-9]+ Total=[0-9]+" -or $_ -match "COVERAGE LinePercent=[0-9]+[.][0-9]+ Covered=[0-9]+ Total=[0-9]+" } | ForEach-Object { "CI-LOG| " + $_ }; "LOG-EXIT=" + $LASTEXITCODE'; (3b) the baseline run 36591948327 read and drift guard payload; (4) gh run download 36664415704 --repo drmoisan/TaskMaster -n pester-coverage -D (Join-Path (Join-Path $env:TEMP "hygiene-927") "pester-coverage"); (5) the read-only JaCoCo projection payload over the downloaded pester-coverage.xml (the Pester callee's XPath reads /report/counter, /report/package and sourcefile LINE counters)
EXIT_CODE: 0
Output Summary:
- EXIT_CODE above is the step (5) projection payload's exit code. Labelled lines (C3): LOG-EXIT=0; BASELINE-LOG-EXIT=0; FETCH-EXIT=0; DOWNLOAD-EXIT=0.
- CONFIRMING-RUN: HYGIENE Findings=0 (GATE6 after the P6-T37 commit 1274c5d31; GUARD-EXIT=0; GUARD-SECONDS=38; enumeration is git ls-files, so every path P6-T37 committed was inside the scan)
- RUN-ID: 36664415704
- PR-NUMBER: 943
- HEAD-SHA: 67a69cb23916878c436fa570d587847b7a0745fa
- PR-BRANCH=bug/evidence-and-identity-hygiene-sweep-927; ANCESTRY-EXIT=0
- RUN-COUNT=1; RUN-HEAD=67a69cb23916878c436fa570d587847b7a0745fa (equal to PR-HEAD); RUN-WORKFLOW=CI; RUN-EVENT=pull_request; RUN-STATUS=completed; RUN-CONCLUSION=success
- Pester job line:

```text
JOB| pester / Run Pester suite with coverage	success
```

- CI log figure lines (transcribed verbatim; exactly two):

```text
CI-LOG| pester / Run Pester suite with coverage	UNKNOWN STEP	2026-09-30T03:28:13.1857293Z PESTER Passed=373 Failed=0 Skipped=0 Total=373
CI-LOG| pester / Run Pester suite with coverage	UNKNOWN STEP	2026-09-30T03:28:13.1923879Z COVERAGE LinePercent=94.51 Covered=1721 Total=1821
```

- Artifact: DOWNLOAD-EXIT=0; ARTIFACT-FILES=1; ARTIFACT-FILE| pester-coverage.xml | 147567; JACOCO-FILES=1
- ARTIFACT-COVERAGE LinePercent=94.51 Covered=1721 Total=1821 (equal to the three figures of the CI-LOG COVERAGE line, so the downloaded document is the one the log figures were computed from)

Package rows:

| Package | Covered | Missed |
|---|---|---|
| scripts/dependencies | 777 | 16 |
| scripts/hygiene | 95 | 6 |
| scripts/vscode | 849 | 78 |

The three new guard files (per-file line coverage, covered over covered plus missed):

| File | Covered | Missed | Percent | At or above 90.00 |
|---|---|---|---|---|
| scripts/hygiene/hygiene/Test-RepositoryHygiene.ps1 | 30 | 3 | 90.91 | MET |
| scripts/hygiene/hygiene/Test-RepositoryHygiene.Rules.ps1 | 31 | 0 | 100.00 | MET |
| scripts/hygiene/hygiene/Test-RepositoryHygiene.Git.ps1 | 34 | 3 | 91.89 | MET |
| Aggregate (three callee folders) | 1721 | 100 | 94.51 | at or above 80.00: MET |

- FILE-LINES-HYGIENE=3 (exactly three FILE| lines whose path contains scripts/hygiene)

Comparison lines (both operands recorded):

- PESTER-FAILED-SKIPPED: Failed=0 Skipped=0 -> MET
- PESTER-PASSED-FLOOR: Passed=373 against CI baseline Passed=342 plus 31 = 373 -> MET
- COVERAGE-ABSOLUTE-FLOOR: LinePercent=94.51 against 80.00 -> MET
- COVERAGE-BASELINE-FLOOR: LinePercent=94.51 against CI baseline LinePercent=94.53 minus 0.50 = 94.03 -> MET
- PER-FILE: Test-RepositoryHygiene.ps1 90.91, Test-RepositoryHygiene.Rules.ps1 100.00, Test-RepositoryHygiene.Git.ps1 91.89, each against 90.00 -> MET

CI baseline (step 3b):

- BASELINE-RUN-ID: 36591948327
- BASELINE-HEAD-SHA: c4ff0e2be0bc9c51acc43dacd2cc5954a448676c
- BASELINE-RUN-BRANCH=main; BASELINE-RUN-EVENT=push; BASELINE-RUN-STATUS=completed; BASELINE-RUN-CONCLUSION=success
- BASELINE-PESTER-PASSED: 342
- BASELINE-PS-LINE-PERCENT: 94.53
- Baseline CI log figure lines (transcribed verbatim; exactly two):

```text
BASELINE-CI-LOG| pester / Run Pester suite with coverage	UNKNOWN STEP	2026-09-29T15:42:15.3569151Z PESTER Passed=342 Failed=0 Skipped=0 Total=342
BASELINE-CI-LOG| pester / Run Pester suite with coverage	UNKNOWN STEP	2026-09-29T15:42:15.3629107Z COVERAGE LinePercent=94.53 Covered=1626 Total=1720
```

- No BASELINE-TRANSCRIPTION-MISMATCH: the observed baseline figures equal the plan's (Passed=342; LinePercent=94.53 Covered=1626 Total=1720), so the floors 373 and 94.03 stand without recomputation.
- MERGE-BASE-NOW=ddbab26a0149bf2ca5d0256e60686ad79e74d90c
- MAIN-TIP-NOW=ddbab26a0149bf2ca5d0256e60686ad79e74d90c
- MAIN-MOVED=0
- BASELINE-DRIFT-PS-FILES=0 (no re-anchoring branch was run; no BASELINE-REANCHORED record)
- P0-T15 direct-run figures, reference only: Passed 320; LinePercent 94.49 (they predate the merge of main and are not a comparison operand).

Handling statement: the JaCoCo document was downloaded to and read under the scratch directory named by the expression (Join-Path (Join-Path $env:TEMP "hygiene-927") "pester-coverage"), outside the repository, and was not copied into the tree. This artifact carries placeholders and expressions only and no absolute host path.

AC4: MET (P6-T20 executed as the closing step of this task).
