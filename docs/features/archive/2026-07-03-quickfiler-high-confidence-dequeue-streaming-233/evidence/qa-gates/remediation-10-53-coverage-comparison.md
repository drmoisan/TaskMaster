Timestamp: 2026-07-04T11:18:13.6000461-04:00

Command:
Parse `docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-10-53-vstest.cobertura.xml` with `xml.etree.ElementTree` and compare against `docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\remediation-baseline\remediation-10-53-ac10-baseline.md`.

EXIT_CODE: 0

Output Summary:
- Raw Cobertura coverage: 15240/80079 = 19.03%.
- Repository-path coverage: 13093/57342 = 22.83%.
- Recorded baseline repository-path coverage: 13120/57396 = 22.86%.
- No-regression status: FAIL. Current repository-path coverage is 27 covered lines and 0.03 percentage points below the recorded baseline.
- Changed/new-code coverage: PASS for `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` at 57/60 = 95.00%.
- Repository-wide 80% floor status: FAIL.
- AC10 status: FAIL.

Changed/New-Code Coverage:
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: 57/60 = 95.00%; uncovered lines 29, 58, 59.
- `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs`: 23/25 = 92.00%; uncovered lines 24, 25.
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs`: 45/56 = 80.36%; uncovered lines 38, 39, 41, 42, 43, 44, 45, 47, 49, 50, 52.
- `QuickFiler/Controllers/QfcHomeController.cs`: 165/248 = 66.53%.
- `QuickFiler/Controllers/QfcFormController.Actions.cs`: 73/204 = 35.78%.
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`: not reported as a distinct Cobertura class/file entry.
- `QuickFiler/Controllers/IQfcCollectionController.cs`: not reported as a distinct Cobertura class/file entry.

Threshold Results:
- Changed/new non-COM-bound gate coverage status: PASS.
- Repository-path no-regression status: FAIL.
- Repository-wide 80% floor status: FAIL.
- Overall AC10 coverage status: FAIL because repository-path coverage remains below the documented repository-wide floor and no approved exception artifact exists.
