# Token gates on the formatted SinkGuard partial (P1-T4)

Timestamp: 2026-10-03T09-23 (host clock read at correction; the label first written was composed, not read)
Command: Grep tool counts over TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs for every pattern of TOKENS-R2, TOKENS-R1 and TOKENS-FORBIDDEN, and for `^`.
EXIT_CODE: 0
Output Summary: every TOKENS-R2 and TOKENS-R1 count equals its true-after value; forbidden tokens 0; TaskCompletionSource 2; file is 210 lines (within 195 to 500).

TOKENS-R2 (after): `harness\.Engines\.VerifyNoOtherCalls\(\);` = 3; `harness\.Invalidations\.Should\(\)\.BeEmpty\("a refused click changes no state to display"\);` = 3; `Engines\.VerifyNoOtherCalls` = 3
TOKENS-R1 (after): `\[DataTestMethod\]` = 1; `\[DataRow\(null\)\]` = 1; `\[DataRow\(""\)\]` = 1; R1-NAME = 1; `\[TestMethod\]` = 4; `#region ` = 3; `#endregion ` = 3
TOKENS-FORBIDDEN: combined alternation of `Thread\.Sleep`, `Task\.Delay`, `DoNotParallelize`, `GetTempPath`, `File\.`, `DateTime\.Now`, `DateTime\.UtcNow` = 0 (each 0)
Positive control: `TaskCompletionSource` = 2
SINKGUARD-LINES-AFTER: 210
