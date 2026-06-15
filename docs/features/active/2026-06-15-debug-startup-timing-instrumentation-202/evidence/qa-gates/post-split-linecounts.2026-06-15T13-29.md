# Post-Split Line Counts (Issue #202, P1-T7)

Timestamp: 2026-06-15T13-29

Command: `awk 'END{print NR}' TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` and `awk 'END{print NR}' TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs`

EXIT_CODE: 0

Output Summary (post-CSharpier format):

- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`: 483 lines (was 687). Strictly < 500.
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs`: 299 lines (new file). Strictly < 500.

Deterministic reduction applied to the original to reach < 500:
- Moved the four `[DoNotParallelize]` startup-timing wiring tests and the explanatory comment
  block to the new file.
- Removed three private static helpers used only by the moved tests
  (`SetEnginesMock`, `AttachMemoryAppender`, `DetachMemoryAppender`).
- Removed the `TimingRecorder` observation seam and the `LoadBasicMethod` override from the
  original's nested `TestableApplicationGlobals` (both consumed only by the moved timing tests;
  retained tests use `LoadSequentialAsync`/`InitializeEnginesPhaseAsync`, not `LoadAsync`).
- Removed now-unused `using` directives from the original: `System.Threading`, `log4net`,
  `log4net.Appender`, `log4net.Repository.Hierarchy`, and the bare
  `Microsoft.Office.Interop.Outlook` (the `OutlookApplication` alias is retained).

CSharpier produced no further line-count change. Both files are strictly < 500 lines.
No escalation required.
