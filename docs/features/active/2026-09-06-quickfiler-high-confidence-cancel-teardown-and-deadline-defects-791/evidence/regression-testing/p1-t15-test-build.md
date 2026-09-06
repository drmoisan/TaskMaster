# [P1-T15] Test build with the new and retargeted tests in place

Timestamp: 2026-09-06T14-45

Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`

EXIT_CODE: 0

Output Summary: `Build succeeded. 0 Warning(s) 0 Error(s).` `QuickFiler.Test.dll` was rewritten at
14:44:48 by this build, so the assembly the Phase 1 expect-fail runs load is the one containing the
new and retargeted tests.

This proves every new test compiles against the Phase 1 seams: the four new files
(`QfcStreamingDequeueConfidenceGateTests.Part4.cs`, `QfcFormControllerCancelTeardownTests.cs`,
`QfcHomeControllerCleanupTests.cs`, `QfcDatamodelTeardownTests.cs`) are wired by the four
`<Compile Include>` entries [P1-T7] added, and the retargeted tests in `.cs`, `.Part2.cs`,
`.Part3.cs`, `QfcQueuePurePathsTests.cs` and `QfcHomeControllerIterationTests.cs` bind to the
widened seams.

## First attempt and its two compile errors

The first invocation of this command exited 1 with two distinct diagnostics. Both were repaired as
micro-actions inside the tasks that introduced them, and the command was then re-run from the start:

1. `QfcStreamingDequeueConfidenceGateTests.Part2.cs` — three `error CS0103: The name 'QfcDequeueStop'
   does not exist in the current context`. The retargeted assertions in [P1-T8] are the first uses of
   that enum in this file, and the file carried no `using QuickFiler.Interfaces;`. The directive was
   added; the three sibling parts of the class already carried it. One assertion was collapsed onto a
   single line at the same time so the file lands at 498 lines rather than 500, keeping headroom
   under the ceiling for the final format.
2. `QfcDatamodelTeardownTests.cs` — one `error CS0104: 'Action' is an ambiguous reference between
   'Microsoft.Office.Interop.Outlook.Action' and 'System.Action'`. The file has
   `using Microsoft.Office.Interop.Outlook;` for `MailItem`, which brings the interop `Action` type
   into scope. The declaration was qualified as `System.Action` with an explanatory comment,
   following the identical convention already recorded at
   `QfcStreamingDequeueConfidenceGateTests.Part2.cs` in
   `Constructor_NonPositiveNonSentinelDeadline_IsRejectedByGuardClause`.

Neither repair changed an assertion's meaning or an acceptance target.

## File sizes after the repairs (`.cs` ceiling 500)

| Path | Lines |
|---|---|
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | 487 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | 498 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | 287 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` | 341 |
| `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs` | 391 |
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | 118 |
| `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` | 231 |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | 418 |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | 497 |
