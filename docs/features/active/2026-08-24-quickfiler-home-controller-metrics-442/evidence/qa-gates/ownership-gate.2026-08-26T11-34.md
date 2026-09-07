# Phase 7 — Forbidden-File Ownership Gate

Timestamp: 2026-08-26T11-34
Task: [P7-T6]
Command: `git diff --name-only 363bfcdd4da5a24743ee665ea9fd124bc42239ff -- QuickFiler/Controllers/QfcHomeController.Iteration.cs QuickFiler/Controllers/QfcFormController.EventHandlers.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/EfcFormController.cs QuickFiler/Interfaces/IFilerHomeController.cs QuickFiler/Controllers/IQfcHomeController.cs QuickFiler/Controllers/EfcHomeControllerDependencies.cs QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`
EXIT_CODE: 0

`363bfcdd4da5a24743ee665ea9fd124bc42239ff` is `BASELINE_SHA`, recorded by [P0-T2].

The two-dot form is deliberately omitted so the comparison includes uncommitted working-tree
changes. At the time this gate ran, nothing had been committed, so every change this feature made
was still in the working tree and fully visible to the command.

## Output Summary

**The command produced no output lines.** The acceptance condition holds.

All eight forbidden files are unmodified:

| # | Forbidden file | Modified |
| --- | --- | --- |
| 1 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | no |
| 2 | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | no |
| 3 | `QuickFiler/Controllers/QfcCollectionController.cs` | no |
| 4 | `QuickFiler/Controllers/EfcFormController.cs` | no |
| 5 | `QuickFiler/Interfaces/IFilerHomeController.cs` | no |
| 6 | `QuickFiler/Controllers/IQfcHomeController.cs` | no |
| 7 | `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` | no |
| 8 | `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` | no |

## Notes on three of these files

**`QuickFiler/Interfaces/IFilerHomeController.cs`** mandates the `QuickFileMetrics_WRITE(string)`
member at line 41. [P2-T8] implemented that member as guarded delegation without changing its
signature and without adding a seam, precisely so this file would not need to change. AC-15 requires
it to be left unchanged and it is.

**`QuickFiler/Controllers/EfcHomeControllerDependencies.cs`** supplies every collaborator factory
the [P1-T7] headless fixture injects. Because those factories are already constructor parameters of
that `internal` type, and `InternalsVisibleTo` grants `QuickFiler.Test` access, the whole fixture
lives inside the owned test file and this file needed no change.

**`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`** is unmodified, and that is the direct
cause of the single failing test recorded by [P6-T5]. Its line 64 injects a `System.Boolean` by
reflection into `_isExecuting`, which [P3-T5] converted to `private int` as AC-14 requires, so
`FieldInfo.SetValue` rejects the conversion. The one-line delta
`SetField(controller, "_isExecuting", 1);` would resolve it, but applying it would modify a file
this gate exists to protect and would turn this gate's output from zero lines to one. The gate was
honoured and the conflict escalated instead. Full diagnosis is in
`evidence/qa-gates/mstest-coverage.2026-08-26T11-30.md`.

This file was also read in full by [P0-T12] to resolve assumption A-10, which confirmed that the two
`QuickFileMetrics_WRITE` guard tests return through the null/empty guard before reaching the widened
argument and therefore survive the `int` to `double` change unchanged. Both of those tests pass in
[P6-T5]. Reading this file is expected; writing it is not.
