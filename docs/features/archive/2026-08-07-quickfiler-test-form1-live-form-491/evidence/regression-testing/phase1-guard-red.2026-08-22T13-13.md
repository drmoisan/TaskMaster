Timestamp: 2026-08-22T13-13
Command: & $vstest .\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~NoLiveFormInTestAssemblyTests"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Total tests: 1. Passed: 0 (implicit; only a Failed count is printed by vstest when all tests fail). Failed: 1. `EXIT_CODE` (1) equals `ExpectedExitCode` (1). The guard is demonstrably red while `Form1` still exists, satisfying this task's numeric acceptance condition (total 1, passed 0, failed 1, EXIT_CODE == ExpectedExitCode).

IMPORTANT FINDING carried into P1-T6 below: the FluentAssertions failure names a type OTHER than `QuickFiler.Test.Form1`. See the P1-T6 record for the verbatim message and its consequence for later phases.

---

[P1-T6] Confirm the guard failed for the correct reason.

Verbatim assertion-failure message, read from `coverage/logs/phase1-guard-red.log`:

```
Expected formDerivedTypeNames to be empty because a unit-test assembly must not compile a live System.Windows.Forms.Form type, but found at least one item {"QuickFiler.Controllers.Tests.QfcHomeControllerTests+QfcFormViewerDerived"}.
```

Acceptance check against the plan's stated condition ("the recorded failure message names the type `QuickFiler.Test.Form1`. A failure naming any other type... fails this task."): **NOT SATISFIED.** The failure names `QuickFiler.Controllers.Tests.QfcHomeControllerTests+QfcFormViewerDerived`, a nested test-double class declared in `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:243-252`, which extends the production type `QuickFiler.Viewers.QfcFormViewer` (itself `: Form, IQfcFormViewer`, `QuickFiler/Viewers/QfcFormViewer.cs:18`). `QuickFiler.Test.Form1` does not appear in the reported collection at all.

The failure IS caused by the FluentAssertions `BeEmpty` assertion (not a load or reflection exception), so the second disjunct of the acceptance condition ("or a failure caused by a load or reflection exception") does not apply either; this is a assertion failure that legitimately fires, just on a different (pre-existing) Form-derived type than the one the plan targets for removal.

This is a genuine, pre-existing second Form-derived type compiled into the `QuickFiler.Test` assembly, independent of `Form1` and not scheduled for removal by this plan (this plan's scope is limited to the two owned csproj regions and the three `Form1.*` files; `QfcHomeControllerTests.cs` and `QfcFormViewer.cs` are outside that scope). Because this second type will remain in the assembly after Phase 2's `Form1` deletion, the guard test as specified by this plan is expected to remain permanently red in Phase 3 (P3-T7) and after, for a reason wholly unrelated to `Form1`. This is a plan-scope gap discovered during execution, not a defect in the guard's implementation or in the Phase 2 removal work. Per the executor's anti-replanning and no-mid-execution-blocking rules, this task is left unchecked, execution continues with the plan exactly as written, and this finding is escalated in the final completion report rather than acted on by widening scope or altering the guard's assertion.

