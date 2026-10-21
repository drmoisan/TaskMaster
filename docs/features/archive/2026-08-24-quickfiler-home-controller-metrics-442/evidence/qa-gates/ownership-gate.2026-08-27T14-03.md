# Phase 7 — Forbidden-File Ownership Gate (re-run; DOCUMENTED DEVIATION)

Timestamp: 2026-08-27T14-03
Task: [P7-T6]
Command: `git diff --name-only 363bfcdd4da5a24743ee665ea9fd124bc42239ff -- QuickFiler/Controllers/QfcHomeController.Iteration.cs QuickFiler/Controllers/QfcFormController.EventHandlers.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/EfcFormController.cs QuickFiler/Interfaces/IFilerHomeController.cs QuickFiler/Controllers/IQfcHomeController.cs QuickFiler/Controllers/EfcHomeControllerDependencies.cs QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`
EXIT_CODE: 0

## Result: DOCUMENTED DEVIATION — this gate is NOT reported clean

The acceptance condition for [P7-T6] is that the command produces no output lines. **It does not
hold.** This task is not checked off, and the forbidden-list gate is not claimed clean anywhere in
this feature's artifacts, its PR body, or its checkpoint.

## What the gate produces, at both comparison points

`BASELINE_SHA` (`363bfcdd4da5a24743ee665ea9fd124bc42239ff`, recorded by [P0-T2]) is no longer the
right comparison point. It was the branch point, but the branch has since merged the epic
integration branch (merge commit `c1826965`), so a diff against `BASELINE_SHA` also reports every
forbidden file that *integration* changed through its own siblings. Both figures are recorded so the
distinction is auditable rather than asserted.

**Against `BASELINE_SHA` — four output lines:**

```
QuickFiler.Test/Controllers/EfcHomeControllerTests.cs
QuickFiler/Controllers/EfcFormController.cs
QuickFiler/Controllers/QfcCollectionController.cs
QuickFiler/Controllers/QfcHomeController.Iteration.cs
```

Three of those four are not this feature's changes. They arrived through the recorded merge of
`origin/epic/quickfiler-bug-family-integration` and belong to sibling epic children (`EfcFormController.cs`
to 464, `QfcCollectionController.cs` to 468, `QfcHomeController.Iteration.cs` to 446). This feature
contributed no hunk to any of them.

**Against the merge base — one output line:**

```
QuickFiler.Test/Controllers/EfcHomeControllerTests.cs
```

`git merge-base HEAD origin/epic/quickfiler-bug-family-integration` resolves to
`0ddab4107b3b147e706a6c15856888b3b5d6404b`, which is the current origin integration tip;
`git rev-list --left-right --count origin/epic/quickfiler-bug-family-integration...HEAD` reports
`0 6`, so the branch is 6 ahead and 0 behind and the merge base equals that tip. A diff against it
therefore isolates exactly this feature's contribution. Command:

`git diff --name-only 0ddab4107b3b147e706a6c15856888b3b5d6404b -- <the same eight paths>`

That single line is the deviation. It is the only forbidden file this feature touched.

## The deviation

`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`, one line changed:

```diff
@@ -61,7 +61,7 @@ namespace QuickFiler.Controllers.Tests
             var controller = CreateMinimalController();
 
             // Simulate a concurrent invocation already in progress.
-            SetField(controller, "_isExecuting", true);
+            SetField(controller, "_isExecuting", 1);
```

Committed as `889fa298`.

### Why no production-side change could avoid it

[P3-T5] and AC-14 require `_isExecuting` to be declared `private int`, consumed through
`Interlocked.CompareExchange(ref _isExecuting, 1, 0)` at
`QuickFiler/Controllers/EfcHomeController.cs:393`. The forbidden test injects a value into that
field by name through reflection, and `FieldInfo.SetValue` performs a widening/identity type check
that rejects a boxed `System.Boolean` for a `System.Int32` field. The test therefore threw
`System.ArgumentException: Object of type 'System.Boolean' cannot be converted to type
'System.Int32'` and the suite could not reach zero failures.

No accessibility change, attribute, or overload on the production side alters that outcome, and the
field name is matched literally by the test, so renaming it does not help either. Reverting [P3-T5]
would satisfy the sibling test but violate AC-14 and leave root cause RC-6 unfixed, which is a core
deliverable of #451.

### Parent ratification

The parent epic-orchestrator ratified this one write after verifying the ban's stated rationale
independently. The ban exists to protect concurrent epic siblings from fan-in conflicts. For this
file that rationale is false: the integration history holds exactly four commits touching
`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` —

| Commit | Date | Subject area |
| --- | --- | --- |
| `23935185` | 2026-03-23 | historical |
| `ceadcd8a` | 2026-05-08 | historical |
| `44bfdf20` | 2026-07-04 | coverage seams, #236 |
| `88366ad4` | 2026-07-07 | store-disable-service, #261 |

— and **none** belongs to epic sibling 446, 468, 498 or 484. No concurrent child holds a claim on
this file, so the write cannot break fan-in.

This branch alone made `_isExecuting` a `private int`, so it is both the sole cause of the breakage
and the sole party positioned to repair it. The repository's own breaking-change rule (General Code
Change Policy, "Public APIs and Compatibility") requires updating in-repo callers when a
breaking change is necessary, which is exactly what the one-line change does.

The ratification covers **this one file on this one feature only**. It is not a general licence to
write plan-forbidden files, and it does not alter the status of the other seven forbidden paths.

## Status of the other seven forbidden paths

Verified unmodified by this feature (zero output lines against the merge base):

| # | Forbidden file | Modified by this feature |
| --- | --- | --- |
| 1 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | no |
| 2 | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | no |
| 3 | `QuickFiler/Controllers/QfcCollectionController.cs` | no |
| 4 | `QuickFiler/Controllers/EfcFormController.cs` | no |
| 5 | `QuickFiler/Interfaces/IFilerHomeController.cs` | no |
| 6 | `QuickFiler/Controllers/IQfcHomeController.cs` | no |
| 7 | `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` | no |
| 8 | `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` | **yes — the deviation above** |

## Effect on acceptance criteria

AC-19 is worded against the ownership boundary. Its check-off must reflect this deviation rather
than paper over it: see `evidence/qa-gates/acceptance-criteria-status.2026-08-27T14-03.md` for the
recorded disposition. The plan checklist entry for [P7-T6] remains `- [ ]` with a
`DOCUMENTED DEVIATION` annotation.
