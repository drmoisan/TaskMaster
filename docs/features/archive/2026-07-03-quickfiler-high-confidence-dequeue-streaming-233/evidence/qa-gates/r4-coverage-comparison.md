Timestamp: 2026-07-03T22-05-04:00
Command: dotnet-coverage merge "docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\r4-vstest-results\01b8e2df-8bd7-4041-bb9b-20df582a4dc3\DanMoisan_MEGALODON4_2026-07-03.22_05_15.coverage" -o docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\r4-vstest.cobertura.xml -f cobertura
EXIT_CODE: 0
Output Summary: P3 coverage conversion and extraction passed. Repository-path coverage was 13120/57379 = 22.87%; focused new-code coverage for QfcStreamingDequeueConfidenceGate.cs was 57/60 = 95.00%.

Conversion Output:
```text
dotnet-coverage v18.5.2.0 [win-x64 - .NET 10.0.9]
Including file C:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\r4-vstest-results\01b8e2df-8bd7-4041-bb9b-20df582a4dc3\DanMoisan_MEGALODON4_2026-07-03.22_05_15.coverage.
Merged into file C:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\r4-vstest.cobertura.xml.
```

Numeric Coverage:
- Raw Cobertura lines: 15267/80116 = 19.06%.
- Repository-path lines: 13120/57379 = 22.87%.
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: 57/60 = 95.00%.
- `QuickFiler/Controllers/QfcHomeController.cs`: 165/248 = 66.53%.
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs`: 45/56 = 80.36%.
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`: not reported as a distinct Cobertura class/file entry.

Baseline Comparison:
- P0 baseline raw coverage: 15267/80133 = 19.05%.
- P0 baseline repository-path coverage: 13120/57396 = 22.86%.
- P3 post-change raw coverage: 15267/80116 = 19.06%.
- P3 post-change repository-path coverage: 13120/57379 = 22.87%.
- No-regression status: PASS.
- Changed/new non-COM-bound gate coverage: PASS for `QfcStreamingDequeueConfidenceGate.cs` at 95.00%.
- Repository-wide 80% floor status: FAIL at 22.87%.
