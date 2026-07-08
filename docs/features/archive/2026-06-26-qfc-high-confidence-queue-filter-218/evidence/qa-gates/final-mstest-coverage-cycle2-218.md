# Final MSTest Coverage — Cycle 2, Issue #218

Timestamp: 2026-06-28T17-31

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation /ResultsDirectory:TestResults\issue218-remediation-cycle2-final`
(run in git-bash with `MSYS_NO_PATHCONV=1` so the `/ResultsDirectory` switch argument is not path-mangled)

EXIT_CODE: 0

Test results:
- Total tests: 4270
- Passed: 4270
- Failed: 0
- Total time: 42.86 s

Test-count reconciliation: cycle entry was 4269. The test split rewiring preserved the QfcHomeController suite compiled active count at 32 (27 moved tests relocated from QfcHomeControllerTests.cs into the four newly-wired split files + 3 residual + 2 Issue218 = 32, unchanged). The net +1 is the single new QfcDatamodelTests admission test `TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook` added in P4-T2. 4269 + 1 = 4270.

Coverage attachment path: `TestResults\issue218-remediation-cycle2-final\22036a90-56ec-4696-b768-942ab7028136\DanMoisan_MEGALODON4_2026-06-28.17_53_03.coverage`

Repo-wide coverage headline (from the P5-T5 Cobertura conversion of the above attachment): line-rate 0.6212100678830588 = 62.12100678830588% (lines-covered 100846 / lines-valid 162338).

Output Summary: All 4270 tests passed, 0 failed. The completed test split and the new admission test compile and run green. Repo-wide line coverage = 62.12100678830588% (100846/162338). No assertion weakening. Coverage attachment converted to Cobertura in P5-T5.
