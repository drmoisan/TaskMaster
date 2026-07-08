Timestamp: 2026-07-03T19:08:01-04:00
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-remediation-rerun-results
EXIT_CODE: 0
Output Summary:
- VSTest completed successfully.
- VSTest executable directory added to PATH for this process: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform`
- Test output summary:
  - Test Run Successful.
  - Total tests: 387
  - Passed: 387
  - Failed: 0
  - Total time: 6.5258 seconds
- Coverage attachment path: `C:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-remediation-rerun-results\8efc792b-5656-400a-9453-f7c0d350aab0\DanMoisan_MEGALODON4_2026-07-03.19_07_05.coverage`
- Coverage conversion command: `dotnet-coverage merge <latest .coverage> -o docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-remediation-rerun.cobertura.xml -f cobertura`
- Coverage conversion output: `Merged into file C:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-remediation-rerun.cobertura.xml.`
- Numeric post-change coverage values from `vstest-remediation-rerun.cobertura.xml`:
  - Raw Cobertura lines: 15267/80133 = 19.05%
  - Repository-path lines: 13120/57396 = 22.86%
  - `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: 57/60 = 95.00%
  - `QuickFiler/Controllers/QfcHomeController.cs`: 165/248 = 66.53%
  - `QuickFiler/Controllers/QfcHomeController.Iteration.cs`: 45/56 = 80.36%
  - `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`: not reported as a distinct Cobertura class/file entry.
