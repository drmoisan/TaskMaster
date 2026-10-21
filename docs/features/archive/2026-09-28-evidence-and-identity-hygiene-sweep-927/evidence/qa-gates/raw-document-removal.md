# P2-T3 Raw-document removal and ignore rules (AC7, AC8)

Timestamp: 2026-09-29T19-42
Command: GATE5A, GATE5B, GATE5C (Gate command reference); git check-ignore -q docs/features/x/a.trx; git check-ignore -q docs/features/x/b.cobertura.xml; the P2-T3 projection and run-log payload
EXIT_CODE: 0
Output Summary:
- GATE5A=0
- GATE5B=0
- GATE5C=0
- GATE8-TRX-EXIT: 0
- GATE8-COBERTURA-EXIT: 0 (this is the EXIT_CODE row)
- PROJECTIONS-EXPECTED=18
- PROJECTIONS-TRACKED=18
- RUNLOG-TRACKED=0
- TRX-TRACKED=0
- Measured over the index and working tree after the P2-T1 staged deletions and the P2-T2 ignore edit, before the P2-T4 commit.

REMOVED-BY-CLASS:
- KIND| cobertura | 243
- KIND| dotnet-coverage | 23
- KIND| jacoco-raw | 27
- KIND| trx | 332
- LIST-LINES=625 (sum of the four classes); OUTSIDE-DOCS-FEATURES=0; no jacoco-projection or none class.
- P2-T1 labelled lines: RM-EXIT=0; STAGED-DELETIONS=626 (LIST-LINES plus test-output.txt); STAGED-OTHER=0.

PROJECTIONS-RETAINED: 18

P2-T2 ignore-file observation (recorded here for AC8):
- git diff --numstat HEAD -- .gitignore printed 6 added, 0 removed.
- git diff -U0 HEAD -- .gitignore carries no removed line; the three comment lines, the trx pattern and the cobertura pattern are added after the existing coverage and coveragexml patterns (unchanged) and before the coverage/* rule.
- Select-String counts: the trx pattern 1 line, the cobertura pattern 1 line.
