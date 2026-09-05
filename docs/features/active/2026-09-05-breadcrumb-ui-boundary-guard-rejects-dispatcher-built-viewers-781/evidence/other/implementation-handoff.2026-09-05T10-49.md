# Implementation Handoff (issue #781)

Timestamp: 2026-09-05T16-48

Task: [P1-T1]

EXIT_CODE: 0

## Executing Worker

The plan directs the executor to delegate the code-authoring tasks [P1-T2] through [P1-T9] to
`csharp-typed-engineer` and to re-verify each acceptance condition itself. No sub-agent
dispatch tool is present in this executor's tool surface for this session, so the code-authoring
edits are performed by `atomic-executor` directly, bounded by the task text of [P1-T2] through
[P1-T9] and by the six constraints restated below. The re-verification obligation is unchanged
and is discharged the same way: every acceptance condition of every task is evaluated against
the tree by an explicit command before that task is checked off. This substitution changes who
types the edit, not what the edit is permitted to be, and it is recorded here so a reviewer can
see it rather than infer it.

## Constraint 1 — Write Set (verbatim from the plan; anything outside this set is out of scope)

Production:

- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`

Tests and test build configuration:

- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` (new file)
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` (two test-method
  deletions plus one comment-clause correction in the `SetViewerSyncContext` `<summary>`; no
  other edit)
- `QuickFiler.Test/QuickFiler.Test.csproj` (one added `<Compile Include>` line)

Documentation and evidence:

- `FEATURE/issue.md` (AC checkbox state only)
- `FEATURE/plan.2026-09-05T10-49.md` (task checkbox state only)
- `FEATURE/evidence/**`

Local, git-ignored, never staged: `coverage/**`, `TestResults/**`, `artifacts/csharp/coverage.xml`,
and the throwaway session script named in [P0-T8].

## Constraint 2 — Fix direction

Ownership is proved by **owner-thread identity through the `Dispatcher` captured in the
`ItemViewer` constructor**, not by synchronization-context reference equality.
`QuickFiler/Viewers/ItemViewer.cs` line 27 captures `_uiDispatcher = Dispatcher.CurrentDispatcher`
in the constructor and exposes it as `UiDispatcher` at lines 64 through 68. The rewritten
`ThrowIfOffUiBoundary` reads that dispatcher, returns without effect when it is null, and
otherwise throws `InvalidOperationException` when `CheckAccess()` is false. The `UiSyncContext`
property and every other member are left in place.

## Constraint 3 — Prohibition on touching the controllers

`QuickFiler/Controllers/**` must not be edited. AC7 names
`QuickFiler/Controllers/QfcCollectionController.cs`,
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, and
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` explicitly; none of the three, and no
other file under that directory, may appear in the diff. [P1-T10] verifies this by observation.

## Constraint 4 — Test framework, mocking, and assertion libraries

New and modified C# tests use **MSTest**
(`Microsoft.VisualStudio.TestTools.UnitTesting`, with `[TestClass]` and `[TestMethod]`), **Moq**
for any mock or stub that a hand-written stub cannot express, and **FluentAssertions** for
assertions. MSTest `Assert` APIs are used only where FluentAssertions is not practical for a
specific assertion shape. xUnit and NUnit are not introduced.

## Constraint 5 — Arrange-Act-Assert

Every new test is organised into explicit Arrange, Act, and Assert sections, and every new test
carries a short XML documentation comment stating the scenario and the expected outcome.

## Constraint 6 — Banned constructs in the new tests

The new tests contain no `Thread.Sleep`, no `Task.Delay`, no timer, no wall-clock wait, no
temporary file, and no message pump. Specifically prohibited by token: `Thread.Sleep`,
`Task.Delay`, `Path.GetTempFileName`, `InvokeAsync`, and `PushFrame`. `Dispatcher.Run` is also
prohibited. Only the same-thread `Dispatcher.Invoke(Action)` fast path is used, which runs the
callback inline at `DispatcherPriority.Send` and needs no pump; an awaited `DispatcherOperation`
against a dispatcher that is never pumped would not complete and would hang the run instead of
failing it. Injected operations are built from an owner-thread-only boundary
(`BreadcrumbUiDispatcher.CreateForCurrentThreadTests()`) or the drainable-context shape already
present in `ItemViewerBreadcrumbLifecycleRegressionTests`, so no posted work escapes the test
thread. Every ambient-context substitution is confined to the test's own thread and is restored
in a `finally`.

Output Summary: Handoff recorded. All six constraints are named above: the verbatim Write Set,
the owner-thread-identity fix direction, the `QuickFiler/Controllers/**` prohibition, the
MSTest plus Moq plus FluentAssertions requirement, the Arrange-Act-Assert requirement, and the
ban on sleeps, timers, wall-clock waits, temporary files, and message pumps. The executing
worker is `atomic-executor` rather than `csharp-typed-engineer`, for the reason stated above.
