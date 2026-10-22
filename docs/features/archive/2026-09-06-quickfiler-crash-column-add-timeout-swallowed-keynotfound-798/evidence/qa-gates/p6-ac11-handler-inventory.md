# P6-T4 — Ribbon handler inventory (AC11)

Timestamp: 2026-09-07T03-15
Task: [P6-T4]
Base commit: c431dc32
File under audit: `TaskMaster/Ribbon/RibbonViewer.cs`

Absolute host paths are redacted to `<repo-root>` tokens throughout.

---

## Counted search

Command: counted search of `TaskMaster/Ribbon/RibbonViewer.cs` for the literal `async void`

EXIT_CODE: 0

Observed count: **24**

Baseline count at the base commit: 24.

Output Summary: 24 occurrences, unchanged from the base commit. AC11 requires this count to be
stable because the three handlers this task routes through the boundary must keep the awaited
`async void` shape that Office binds against. Routing changes each handler's body, never its
signature, so the count is invariant across the change. A count of 27 would mean new `async void`
members were introduced; a count of 21 would mean the three handlers had been converted to a
different shape and Office would silently stop binding them.

---

## Members changed by this task — exactly three

| Member | Shape before | Shape after |
|---|---|---|
| `QuickFiler_Click` | `async void`, block body, awaits `_controller.LoadQuickFilerAsync()` | `async void`, block body, awaits `_commandBoundary.RunAsync` |
| `QuickFilerHighConfidence_Click` | `async void`, block body, awaits `_controller.LoadQuickFilerHighConfidenceAsync()` | `async void`, block body, awaits `_commandBoundary.RunAsync` |
| `SortEmail_Click` | `async void`, expression body, awaits `_controller.SortEmailAsync()` | `async void`, expression body, awaits `_commandBoundary.RunAsync` |

Each handler keeps its own existing body style: the two block-bodied handlers stay block-bodied
and the expression-bodied handler stays expression-bodied. No handler is renamed, reordered or
reformatted, and the original controller call is preserved verbatim inside the `Func<Task>`
passed to the boundary, so the work performed is unchanged and only its failure handling differs.

## Members deliberately left unchanged

- `RunFolderFilterCallback` — already guarded by its own try/catch with a contained reporter. It
  is one of the 24 `async void` members and is untouched.
- The remaining 20 out-of-scope `async void` members in this file are untouched.
- Every `async void` member in the sibling ribbon partial is untouched. Their awaited tasks are
  contractually non-faulting.

## Additions that are not `async void`

Two members were added by P6-T3 and neither contributes to the `async void` count:

- `private static RibbonCommandBoundary CreateCommandBoundary()` — a synchronous factory.
- `private static void ReportRibbonCommandFailure(string commandName, System.Exception exception)`
  — a synchronous log sink.

One field was added: `private readonly RibbonCommandBoundary _commandBoundary;`.

---

## Type attributes retained

| Literal | Present | Line |
|---|---|---|
| `[System.Runtime.InteropServices.ComVisible(true)]` | yes | 31 |
| `[ExcludeFromCodeCoverage]` | yes | 32 |

Both attributes are retained on the `RibbonViewer` declaration. The coverage exemption is correct
for this type: it is the COM-visible ribbon shim. The boundary decision logic it delegates to
lives in `TaskMaster/Ribbon/RibbonCommandBoundary.cs`, which carries no coverage-exemption
attribute and is unit-tested, satisfying AC11's requirement that the decision logic be
coverage-visible.

---

## Test result

`TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary`
passes in the P6-T5 run:

```
Passed NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary [2 ms]
```

That test asserts all three named handlers return `void` and carry the compiler-emitted
`AsyncStateMachineAttribute`, which is the observable proof that each awaits its work rather than
discarding the returned task, and that `RibbonViewer` declares a field of type
`RibbonCommandBoundary`. P2-T10 recorded it as failing before this phase because no such field
existed.

## Post-change line count

`TaskMaster/Ribbon/RibbonViewer.cs` = 432 lines, up from 388 at the base commit and below the
500-line cap.
