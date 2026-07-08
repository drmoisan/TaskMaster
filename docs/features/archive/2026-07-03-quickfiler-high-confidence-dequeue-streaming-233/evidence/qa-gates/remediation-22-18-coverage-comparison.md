Timestamp: 2026-07-04T14-39
Command: Parse docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-22-18-vstest.cobertura.xml and compare against r4-final-coverage-comparison.md.
EXIT_CODE: 0
Output Summary:
- Raw Cobertura coverage: 15267/80116 = 19.06%.
- Repository-path coverage: 13120/57379 = 22.87%.
- Existing baseline repository-path coverage: 13120/57396 = 22.86%.
- No-regression status: PASS.
- Changed/new non-COM-bound gate coverage: PASS for QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs at 57/60 = 95.00%.
- Repository-wide 80% floor status: FAIL at 22.87%.
- AC10 status: FAIL.

Final Numeric Coverage:
- Raw Cobertura lines: 15267/80116 = 19.06%.
- Repository-path lines: 13120/57379 = 22.87%.
- QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs: 57/60 = 95.00%.
- QuickFiler/Controllers/QfcHomeController.cs: 165/248 = 66.53%.
- QuickFiler/Controllers/QfcHomeController.Iteration.cs: 45/56 = 80.36%.
- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs: not reported as a distinct Cobertura class/file entry.

Baseline Comparison:
- R4 baseline raw coverage: 15267/80133 = 19.05%.
- R4 baseline repository-path coverage: 13120/57396 = 22.86%.
- Remediation raw coverage: 15267/80116 = 19.06%.
- Remediation repository-path coverage: 13120/57379 = 22.87%.
- No-regression status: PASS.

Threshold Results:
- Changed/new non-COM-bound gate coverage: PASS for QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs at 95.00%.
- Repository-wide 80% floor status: FAIL at 22.87%.
- Overall AC10 coverage status: FAIL because repository-path coverage remains below the documented repository-wide floor.
