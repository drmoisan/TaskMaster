Timestamp: 2026-07-03T22-01-04:00
Command: dotnet-coverage merge docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\remediation-baseline\r4-baseline-vstest.coverage -o docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\remediation-baseline\r4-baseline-vstest.cobertura.xml -f cobertura
EXIT_CODE: 0
Output Summary: Baseline coverage conversion passed. Raw Cobertura coverage was 15267/80133 = 19.05%; repository-path coverage was 13120/57396 = 22.86%; changed/new non-COM-bound coverage for QfcStreamingDequeueConfidenceGate.cs was 57/60 = 95.00%.

Resolved Coverage Input:
- docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-baseline-vstest.coverage

Resolved Coverage Output:
- docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-baseline-vstest.cobertura.xml

Conversion Output:
```text
dotnet-coverage v18.5.2.0 [win-x64 - .NET 10.0.9]
Including file C:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\remediation-baseline\r4-baseline-vstest.coverage.
Merged into file C:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\remediation-baseline\r4-baseline-vstest.cobertura.xml.
```

Numeric Baseline Coverage:
- Raw Cobertura lines: 15267/80133 = 19.05%.
- Repository-path lines: 13120/57396 = 22.86%.
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: 57/60 = 95.00%.
- `QuickFiler/Controllers/QfcHomeController.cs`: 165/248 = 66.53%.
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs`: 45/56 = 80.36%.
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`: not reported as a distinct Cobertura class/file entry.

Comparison Fields:
- Baseline coverage: repository-path 13120/57396 = 22.86%.
- Post-change coverage: not yet recalculated for final remediation; Phase 3 and Phase 4 will write post-change values.
- New-code coverage: `QfcStreamingDequeueConfidenceGate.cs` 57/60 = 95.00%.
- Baseline threshold status: changed/new-code coverage PASS against 90%; repository-wide 80% floor FAIL at this baseline.
