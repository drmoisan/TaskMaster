Timestamp: 2026-07-03T18:02:49-04:00
Command: Compare docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-remediation-final.cobertura.xml against docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/coverage-baseline.md
EXIT_CODE: 1
Output Summary:
- Result: FAIL.
- Baseline repository coverage: unavailable; baseline artifact records that no baseline VSTest coverage attachment was available.
- Post-change repository coverage from repo-path classes in Cobertura: 12848/57105 lines = 22.5%.
- Post-change raw Cobertura coverage: 14995/79842 lines = 18.78%.
- Changed/new non-COM-bound coverage for QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs: 54/56 lines = 96.43%.
- Changed/new non-COM-bound coverage threshold status: PASS against >= 90%.
- Repository coverage floor status: FAIL against >= 80% for repo-path classes; raw Cobertura floor status: FAIL.
- Baseline no-regression status: FAIL because numeric baseline coverage is required for no-regression comparison and was not available in docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\baseline\coverage-baseline.md.
- PASS criteria require repository coverage at or above the applicable numeric baseline and floor, and changed/new non-COM-bound coverage at or above 90%.
