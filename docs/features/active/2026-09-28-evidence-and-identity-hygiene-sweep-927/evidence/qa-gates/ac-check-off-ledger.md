# Acceptance-criteria check-off ledger (P6-T17 to P6-T36)

Timestamp: 2026-09-29T23-35
Command: git grep -c -e "^- \[x\] AC" -- docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/spec.md
EXIT_CODE: 0
Output Summary:
- The count command printed 18 for spec.md.
- TOTAL: 18 of 20
- UNMET: AC4 (AC4: PENDING P6-T38), AC13 (AC13: NOT MET)

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/spec.md
- Total AC items: 20
- Checked off (delivered): 18
- Remaining (unchecked): 2
- Items remaining:
  - AC4: PENDING P6-T38 (Ruling 1: the check-off waits on the CI Pester coverage figure; the tick and this line's update land inside the P6-T38 commit)
  - AC13: NOT MET

Per-criterion record:
- AC1: MET (P6-T17). guard-pre-sweep-run.md: ExpectedExitCode: 1, EXIT_CODE: 1, HYGIENE Findings=1839, which equals RAW-POPULATION: 625 (P0-T17) plus PROFILE-PATH-FILES-NOW: 1214 (the P1-T12 MEASUREMENT-CORRECTION).
- AC2: MET (P6-T18). pester-hygiene-fail-before.md: ExpectedExitCode: 1, EXIT_CODE: 1, Failed=31.
- AC3: MET (P6-T19). pester-hygiene-pass-after.md: Failed=0 Skipped=0 with all thirty spec-listed It names Passed; P6-T3 HYGIENE Failed=0.
- AC4: PENDING P6-T38
- AC5: MET (P6-T21). P6-T12: PATTERN=0, ACCOUNT=0, HOST=0, SHORT=0, ENV=0, CLOCK=0, ALLOW=0, FILE-IO=0, MOCK-GIT=0; P6-T11: OVER-500=0.
- AC6: MET (P6-T22). P6-T3 IT| lines for the three named orchestration tests read Passed.
- AC7: MET (P6-T23). raw-document-removal.md: GATE5A=0, GATE5B=0, GATE5C=0, PROJECTIONS-TRACKED=18, RUNLOG-TRACKED=0, TRX-TRACKED=0.
- AC8: MET (P6-T24). GATE8-TRX-EXIT: 0, GATE8-COBERTURA-EXIT: 0; git diff -U0 origin/main...HEAD -- .gitignore shows six added lines (three comment lines, the trx pattern, the cobertura pattern, a blank line) and no removed line.
- AC9: MET (P6-T25). identifier-residual-scan.md: GATE1=0, GATE2=0, GATE3=0, GATE4=0, PLAN-FILE-HITS=0, Scope: paragraph present; POST-CLEANUP RX-LENGTH=40, UTF16-PROFILE-FILES=0, UTF16-IDENTIFIER-FILES=0; PRE-CLEANUP RX-LENGTH=40, UTF16-PROFILE-FILES=1; no PRE-CLEANUP-CENSUS: NOT OBSERVED line.
- AC10: MET (P6-T26). GATE9=0; D18 residual LEGACY-USER-ANYCASE-FILES=9 (recorded, not a condition of the check-off).
- AC11: MET (P6-T27). redaction-fidelity.md: EOL-MISMATCH=0, BOM-MISMATCH=0, WRITTEN-COMPARED=1052, BOM-COMPARED=1052 (P4-T4 FILES-WRITTEN=1052), REMOVED-LINES=31761 equal to ADDED-LINES=31761, MULTISET-FILES=1052, MULTISET-UNMATCHED=0, LINECOUNT-MISMATCH=0, MULTISET-UTF16-DECODED=1, CONTROL-MULTISET-UNMATCHED=1, REMOVED-UNMATCHED=8 recorded with the eight CoreClean lines and the moved-not-changed explanation, XML-REWRITTEN=0, VERIFY-DECODED-FILES=1 (UTF16-SCANNED=1), VERIFY-WRITTEN-SET=1052, second run FILES-WRITTEN=0 with RULE1=0 to RULE8=0 and equal numstat hashes; P4-T9 EOL-INDEX-MISMATCH=0.
- AC12: MET (P6-T28). P3-T16 failed=0 with all thirteen classes present; P3-T17 two named tests Passed; P3-T9 census PREFIX-SITES=0; the fixture profile-path grep printed nothing; the research note grep printed three placeholder lines, each containing repos.
- AC13: NOT MET (P6-T29). The csharpier check exit code is 0. P6-T7 and P6-T8 each show ZERO-ERRORS=1 and SKIP-CORECOMPILE=0. The summary shows failed 0 with passed 7346, not below BASELINE-PASSED: 7343. The coverage clause fails: first-party line 85.92 is below the P0-T14 85.93, and branch 80.08 is below the P0-T14 80.09. The denominators are comparable under D10 (lines-valid 65736 against 65737). The whole difference is in the UtilitiesCS package, and the branch changes no production C# file (csharp-coverage-projection.md). The spec names the CI mstest-coverage context on the pull-request head as the authoritative pass; P6-T37 records it.
- AC14: MET (P6-T30). powershell-toolchain-pass.md: final-iteration REWRITTEN=0; the line PoshQC analyze: pass (0 findings); tool reports no count; both P6-T2 channel lines ok=true.
- AC15: MET (P6-T31). P5-T1 every KEY| count 1 with CONCURRENCY=0, NEEDS=0, LASTEXIT=0; P5-T2 single uses line with no needs and no steps; P1-T10 both hygiene entries (the _pester.yml lines 41 and 45 re-read in this run); P5-T3 CONTEXT-LINE=1, PREDICTED=1, PESTER-ROW=1, ROW=1; P5-T4 ACTIONLINT-EXIT=0.
- AC16: MET (P6-T37). ci-hygiene-context.md: on head 67a69cb23916878c436fa570d587847b7a0745fa exactly one check-run begins hygiene / (CAPTURED-CONTEXT: hygiene / Repository hygiene guard, conclusion success); the six pre-existing contexts each report success; the ruleset GET lists no hygiene / context; RULESET-MODIFIED: no; CONFIRMING-RUN: HYGIENE Findings=0 with exit 0.
- AC17: MET (P6-T32). guard-post-sweep-run.md: EXIT_CODE: 0, FINDING-LINES=0, HYGIENE Findings=0, enumeration statement present.
- AC18: MET (P6-T33). p6-t13-evidence-form.md: NON-MD=0, MISSING-FIELDS=0, ADDED-RAW=0, ADDED-RAW-UNTRACKED=0, and the statement that the helper log and raw tool outputs are under the scratch expression or the ignored directories.
- AC19: MET (P6-T34). p6-t14-scope-containment.md: OUTSIDE-WRITE-SET=0, GOVERNANCE=0, MCP-CONFIG=0, BUILD-INPUTS=0, PROD-CS=0, SIBLING-2026-09-28=0, ANCESTRY-1-EXIT=0, ANCESTRY-2-EXIT=0, ANCESTRY-3-EXIT=0, ANCESTRY-CONTROL-EXIT=1, no ANCESTRY-CONTROL: NOT OBSERVED line, NO-FORCE-PUSH recorded.
- AC20: MET (P6-T35). P6-T3 IT| lines for the four named tests read Passed; P6-T12 ALLOW=0 and PREFIX-LITERAL=1.
