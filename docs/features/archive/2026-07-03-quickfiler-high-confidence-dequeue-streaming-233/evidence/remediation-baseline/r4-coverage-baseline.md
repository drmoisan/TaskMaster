Timestamp: 2026-07-03T21-57-04:00
Command: Read docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-remediation-rerun.md and docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-comparison-remediation-final.md
EXIT_CODE: 0
Output Summary: Numeric baseline extracted from current QA artifacts. VSTest passed with 387/387 tests. Repository-path coverage was 22.86%, changed/new gate coverage for QfcStreamingDequeueConfidenceGate.cs was above 90%, and overall coverage status was FAIL because repository-path coverage was below the documented 80% floor.

Source Artifacts:
- docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-remediation-rerun.md
- docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-comparison-remediation-final.md

Numeric Baseline:
- VSTest: PASS, 387 total, 387 passed, 0 failed.
- Raw Cobertura coverage from remediation rerun: 15267/80133 = 19.05%.
- Repository-path coverage from remediation rerun: 13120/57396 = 22.86%.
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: 57/60 = 95.00%.
- `QuickFiler/Controllers/QfcHomeController.cs`: 165/248 = 66.53%.
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs`: 45/56 = 80.36%.
- Prior comparison baseline raw coverage: 14997/79844 = 18.78%.
- Prior comparison baseline repository-path coverage: 12850/57107 = 22.50%.
- Prior comparison post-change repository-path coverage: 13118/57394 = 22.86%.
- Prior comparison policy status: no-regression PASS; changed/new non-COM-bound gate coverage PASS; repository-wide 80% floor FAIL; overall coverage FAIL.
