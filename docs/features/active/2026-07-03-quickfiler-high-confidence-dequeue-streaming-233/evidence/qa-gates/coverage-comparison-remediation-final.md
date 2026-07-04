# Numeric Coverage Comparison

- Timestamp: 2026-07-03T19:03:49-04:00
- Issue: 233
- Baseline artifact: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-results/final.cobertura.xml`
- Post-change artifact: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/p4-coverage-final.cobertura.xml`
- VSTest command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /EnableCodeCoverage /ResultsDirectory:'docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\p4-coverage-results'`
- Conversion command: `dotnet-coverage merge <latest .coverage> -o 'docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\p4-coverage-final.cobertura.xml' -f cobertura`
- Command status: VSTest PASS, conversion PASS, extraction PASS.

## Baseline Coverage

- Raw Cobertura coverage: 14997/79844 lines = 18.78%.
- Repository-path class coverage: 12850/57107 lines = 22.50%.
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: 54/56 lines = 96.43%.
- `QuickFiler/Controllers/QfcHomeController.cs`: 156/239 lines = 65.27%.
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs`: 41/52 lines = 78.85%.
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`: not emitted as a distinct Cobertura class entry.

## Post-Change Coverage

- Raw Cobertura coverage: 15265/80131 lines = 19.05%.
- Repository-path class coverage: 13118/57394 lines = 22.86%.
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: 60/63 lines = 95.24%.
- `QuickFiler/Controllers/QfcHomeController.cs`: 165/248 lines = 66.53%.
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs`: 45/56 lines = 80.36%.
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`: not emitted as a distinct Cobertura class entry.

## Policy Status

- No-regression status: PASS. Raw and repository-path coverage increased against the numeric remediation baseline.
- Changed/new non-COM-bound gate coverage: PASS. `QfcStreamingDequeueConfidenceGate.cs` is 95.24%, above the 90% changed/new-code target.
- Repository-wide 80% floor status: FAIL. The current repo-path class coverage is 22.86%, below 80%.
- Overall coverage status: FAIL because repository-wide coverage is below the documented policy floor, even though no-regression and changed/new-code coverage passed.
