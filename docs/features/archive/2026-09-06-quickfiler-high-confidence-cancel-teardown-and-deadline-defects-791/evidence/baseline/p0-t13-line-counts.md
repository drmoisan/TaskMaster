# [P0-T13] Baseline line counts of every file this plan edits or creates

Timestamp: 2026-09-06T14-31

Command: `foreach ($p in <the thirteen paths>) { (Get-Content -LiteralPath $p).Count }`

EXIT_CODE: 0

CEILING: 500 (applies to *.cs only)

## Production `.cs` (Write Set, seven paths)

QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs = 262
QuickFiler/Interfaces/IQfcDatamodel.cs = 133
QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs = 298
QuickFiler/Controllers/QfcDatamodel.cs = 480
QuickFiler/Controllers/QfcFormController.EventHandlers.cs = 408
QuickFiler/Controllers/QfcFormController.Deactivate.cs = 60
QuickFiler/Controllers/QfcHomeController.cs = 469

## Existing test `.cs` this plan modifies (five paths)

QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs = 477
QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs = 465
QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs = 280
QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs = 413
QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs = 477

## New test `.cs` this plan creates (four paths, zero at baseline)

QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs = 0 (does not exist yet)
QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs = 0 (does not exist yet)
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs = 0 (does not exist yet)
QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs = 0 (does not exist yet)

## PROJECT-FILE (exempt)

PROJECT-FILE (exempt): QuickFiler.Test/QuickFiler.Test.csproj = 524

Reason for the exemption: `.claude/rules/general-code-change.md` caps *production code, test code
and reusable script files* at 500 lines. `.csharpierignore` lines 9-14 record that project files are
owned by Visual Studio and are not C# source, and list `*.csproj` at line 12. The file already
stands at 524 lines today and becomes 528 after [P1-T7] adds four `<Compile Include>` entries, so
asserting it against the ceiling would be unsatisfiable regardless of what this plan does. Its count
is recorded as an observation and is never asserted (R8).

## The three tightest `.cs` files

1. `QuickFiler/Controllers/QfcDatamodel.cs` = 480, headroom 20.
2. `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` = 477, headroom 23.
   `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` = 477, headroom 23 (tied).
3. `QuickFiler/Controllers/QfcHomeController.cs` = 469, headroom 31.

`QfcDatamodel.cs` is the tightest file in the set, but [P2-T5] and [P2-T6] together remove more
lines from it than they add: `TryQueueRemainingMailItemAsync` is relocated out of it into the
QueueProcessing partial. `QfcHomeController.cs` at 31 lines of headroom is what forces D11's
two-guarded-block form for `Cleanup()` rather than three. The two 477-line test files are the ones
[P1-T5] and [P1-T11] must stay inside; [P1-T5] budgets at most 12 added lines and [P1-T11] adds one
small test method.

These counts are re-measured after the final format by [P3-T9], because CSharpier can change line
counts, and an interim measurement is taken by [P2-T16] before that format.
