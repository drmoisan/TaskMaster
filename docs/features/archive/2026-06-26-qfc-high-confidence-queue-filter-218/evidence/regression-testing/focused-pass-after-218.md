Timestamp: 2026-06-26T20-47
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcDatamodelTests|FullyQualifiedName~RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch|FullyQualifiedName~RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter"
EXIT_CODE: 0
Output Summary:
- Targeted issue #218 tests passed.
- Total tests: 6.
- Passed: 6.
- Failed: 0.
- Covered behaviors: remaining-queue scoring before admission, equal-threshold admission, below-threshold rejection, disabled-mode direct admission, and initial GUI batch loading without high-confidence prefilter.
