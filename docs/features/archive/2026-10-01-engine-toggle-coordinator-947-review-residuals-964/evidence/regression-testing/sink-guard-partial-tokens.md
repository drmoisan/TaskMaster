# SinkGuard Partial Registration and Tokens (P1-T11, P1-T13)

## TEST-CSPROJ-REGISTRATION: (P1-T11)

Timestamp: 2026-10-03T07-47
Task: P1-T11
Command: Grep tool count over TaskMaster.Test/TaskMaster.Test.csproj for `Ribbon\x5CEngineToggleStateCoordinatorTests\.SinkGuard\.cs"`; Read tool over lines 362-365; git diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 -- TaskMaster.Test/TaskMaster.Test.csproj
EXIT_CODE: 0

Output Summary:
- Grep count: 1 line (line 364).
- Read observation: line 364 directly follows the RepeatFaultSuppression entry at 363; line 365 is `Ribbon\EngineTogglePressedStateCacheTests.cs`.
- Numstat: `1	0	TaskMaster.Test/TaskMaster.Test.csproj` (1 inserted, 0 deleted).
- Verdict: PASS.

## FORMAT-AND-TOKENS: (P1-T13)

Timestamp: 2026-10-03T07-47
Task: P1-T13
Command: dotnet tool run csharpier format TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs; dotnet tool run csharpier check (same three paths); Grep tool counts over TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs
EXIT_CODE: 0

Output Summary:
- Format: `Formatted 3 files in 2931ms.` exit 0 (processed count).
- Check: `Checked 3 files in 1066ms.` exit 0, no path listed.
- SinkGuard partial banned-token counts: `Thread\.Sleep` 0, `Task\.Delay` 0, `DoNotParallelize` 0, `GetTempPath` 0, `File\.` 0, `DateTime\.Now` 0, `DateTime\.UtcNow` 0 (one alternation over all seven: 0 matches).
- Positive control: `TaskCompletionSource` 2 (lines 130, 131).
- Reason fragments, each whole on one physical line: `a throwing notification sink must not escape the refusal path` 1 (line 42); `the refusal path contains a failure of both sinks` 1 (line 100); `notify sink failed` 3 (lines 35, 56, 92).
- Verdict: PASS.
