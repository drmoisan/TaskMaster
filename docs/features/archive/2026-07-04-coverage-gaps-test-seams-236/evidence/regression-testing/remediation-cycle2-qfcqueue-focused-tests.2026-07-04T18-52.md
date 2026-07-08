Timestamp: 2026-07-04T18-52
Task: [P2-T4]

Command: dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs
EXIT_CODE: 0
Output Summary:
- Formatted 1 C# file.

Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
Output Summary:
- Build succeeded.
- Existing warning remained: QuickFiler.Test\Controllers\QfcFormControllerTests.cs(694,13) MSTEST0032.
- No errors.

Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~QfcQueueCoverageExpansionTests" /InIsolation
EXIT_CODE: 0
Output Summary:
- Test Run Successful.
- Total tests: 8.
- Passed: 8.

Command: & 'C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe' collect --output 'docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\regression-testing\remediation-cycle2-qfcqueue-focused-coverage.cobertura.xml' --output-format cobertura -- 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~QfcQueueCoverageExpansionTests" /InIsolation
EXIT_CODE: 0
Output Summary:
- Test Run Successful.
- Total tests: 8.
- Passed: 8.
- Coverage output: docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\regression-testing\remediation-cycle2-qfcqueue-focused-coverage.cobertura.xml.

Coverage Comparison:
- Baseline: docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\remediation-baseline\remediation-cycle2-baseline-coverage.cobertura.xml
- Focused file: QuickFiler\Controllers\QfcQueue.cs
- Focused valid lines: 386.
- Focused covered lines: 133.
- Focused line rate: 34.46%.
- Newly covered lines versus cycle 2 baseline: 107.
- Newly covered line numbers: 55, 57, 61, 62, 63, 64, 65, 66, 67, 68, 69, 74, 75, 76, 77, 78, 79, 80, 81, 82, 99, 100, 101, 102, 103, 105, 106, 108, 110, 111, 112, 113, 114, 115, 116, 117, 118, 123, 124, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 149, 150, 151, 152, 160, 356, 357, 358, 359, 360, 361, 362, 363, 364, 365, 366, 367, 368, 377, 378, 500, 501, 502, 503, 504, 505, 506, 507, 515, 516, 517, 519, 521, 526, 527, 528, 529, 530, 531, 532, 533, 534, 535, 536, 537, 538, 539, 541, 543, 544, 545, 546, 547.

Acceptance Verification:
- Added QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs.
- Updated QuickFiler.Test/QuickFiler.Test.csproj to include the new test file.
- Queue sizing covered by AdjustTlp_WhenRowsIncrease_GrowsRowCountAndMinimumHeight.
- Dequeue covered by Dequeue_WithQueuedEntry_UnhooksItemsRaisesRemoveAndUpdatesCount and TryDequeueAsync_WithCompletedPendingEntry_UnhooksItemsAndRaisesRemove.
- Empty queue and cancellation behavior covered by TryDequeueAsync_WithRunningJobAndCancellation_ReturnsDefault.
- High-confidence carrier path covered by Dequeue_WithHighConfidenceCarrier_PreservesPredeterminedFolder.
- State-reset paths covered by GrowEntry_WhenTargetHasCapacity_MovesControlAndGroupThenResetsSourceState and CompleteAddingAsync_WhenFunctionTimeoutExpires_ThrowsAndLeavesQueueOpen.
- No live Outlook dependency was used; MailItem and queue collaborators were mocked or in-memory.
- No coverage exclusions or coverage configuration changes were made.
