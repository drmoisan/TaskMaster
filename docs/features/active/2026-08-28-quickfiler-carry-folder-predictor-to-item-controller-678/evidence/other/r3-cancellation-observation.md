# R3 — Cancellation observation on the adoption path

- Timestamp: 2026-09-02T01-26
- Issue: #678
- Task: [P1-T9]
- File: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`

## The invariant

> An already-cancelled token produces the same observable outcome on the adoption path as it
> did on the pre-change path.

## The pre-change outcome, restated as an observable

Every pre-change route into the predictor ran inside `await Task.Run(..., cancel)`. For an
already-cancelled token `Task.Run` returns a cancelled task and the await throws
`TaskCanceledException`, which is not an `ArgumentNullException`, so it falls to the
`catch (System.Exception e)` handler, is logged through `logger.Error` and rethrown. The
observable pre-change outcome is therefore: **an `OperationCanceledException` propagates out
of `LoadFolderHandlerAsync` and `_folderHandler` is not assigned.**

`TaskCanceledException` derives from `OperationCanceledException`, and both callers of this
member wrap it in a `Task.Run(..., token)` whose await surfaces the cancellation:
`QuickFiler/Controllers/QfcCollectionController.cs:519-525`, whose folder tasks are built as
`Task.Run(async () => await grp.ItemController.LoadFolderHandlerAsync(Token), Token)` and are
awaited through `Task.WhenAny`; and
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:187`,
`await Task.Run(() => LoadFolderHandlerAsync(token, varList), token);`, inside
`PopulateFolderComboBoxAsync`. That second citation is `:187` in the post-edit file; the plan
cites `:178`, which was its line number before this task inserted the eight-line comment and
the guard statement earlier in the same file.
A `cancel.ThrowIfCancellationRequested()` therefore reproduces at both call sites the same
`OperationCanceledException` the pre-change `Task.Run(..., cancel)` route produced.

## The edit

`cancel.ThrowIfCancellationRequested();` is inserted as the **first statement inside the
carried-handler adoption branch**, immediately after the `if (_carriedFolderHandler is not
null)` opening brace and before the `_folderHandler = _carriedFolderHandler;` assignment,
preceded by a comment carrying the token `#678 R3`.

## Why the observation is inside the branch and not at the top of the member

The `try` opens **after** that branch, and its `catch (System.Exception e)` covers the
`FromField` route only: the `varList is null` route that reaches the predictor through
`Task.Run(..., cancel)`. A guard at the top of the member would throw before that `try` is
entered, silently removing the `logger.Error` that the pre-change `FromField` route emitted
for an already-cancelled token. That is a second behaviour change this cycle is not
authorised to make.

The `FromArrayOrString` route is the `else` branch; it carries no `try` or `catch` of its own
and emits `logger.Debug` rather than `logger.Error`, so it is not the route this placement
protects. It is separately pinned by the existing test
`LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory`,
which P1-T10 re-runs.

## Acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | `#678 R3` occurs exactly once in the file, on a single line | PASS — **1** occurrence, **1** matching line |
| 2 | `cancel.ThrowIfCancellationRequested();` occurs at least once in the file | PASS — **1** occurrence (0 before this edit) |
| 3 | the analyzer build exits 0 | PASS — exit 0, `CoreCompile:` 67 |
| 4 | the nullable build exits 0 | PASS — exit 0, zero `CS86`, `CoreCompile:` 67 |

Clause 2 is falsifiable: the same search returned **0** occurrences against the pre-edit file
at P1-T8, so the count genuinely moved from 0 to 1 as a result of this task.

## Output Summary

One statement and one explanatory comment inserted into the carried-handler adoption branch.
Analyzer build exit 0, nullable build exit 0 with zero `CS86`. `#678 R3` occurs exactly once
on one line; `cancel.ThrowIfCancellationRequested();` occurs once, having occurred zero times
before this task.
