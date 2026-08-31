# efc-ok-chain-unhandled-keyboard-paths-and-half-completed-teardown (Issue #695)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-ok-chain-unhandled-keyboard-paths-and-half-completed-teardown/ (Issue #695)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #695
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/695
- Last Updated: 2026-08-29
## Summary

Two defects on the EFC filing (OK) chain, both carried out of the issue #637 scoping analysis and
deliberately excluded from that fix. First, the two keyboard entry points to `ActionOkAsync` have no
exception handler, so an exception thrown during filing becomes an unhandled UI-thread exception.
Second, on the button entry point the exception is caught, but `ActionOkAsync` hides the form before
awaiting the filing operation and disposes it only afterwards, so a throw leaves the form hidden,
undisposed, and uncleaned with no user-visible message.

This entry also records a third, smaller item: `EfcDataModel.OpenOlFolderAsync` and
`OpenFsFolderAsync` assign `DestinationOlStem` verbatim in exactly the same shape that issue #637
corrects in the `string` overload of `MoveToFolderAsync`, and are not covered by that fix.

**Correction to the record.** Issue #637 states that an `InvalidOperationException` from
`Globals.Ol.ArchiveRootPath` "becomes an unhandled UI-thread exception" because `ButtonOK_Click` is
`async void` and rethrows. That premise is inaccurate against the current tree.
`ButtonOK_Click` is `async void`, but it delegates to `ButtonOkClickAsync`, which wraps the whole
chain in `try { ... } catch (System.Exception ex) { BoundaryErrorSink(ex.Message, ex); }`
(`QuickFiler/Controllers/EfcFormController.cs:460-475`). The exception is therefore logged, not
unhandled, on the button path. The genuine defects are the two described above, which is why they
were separated from issue #637 rather than folded into it.

## Environment

- OS/version: Windows 11 Pro 10.0.26200; .NET Framework 4.8.1 VSTO add-in.
- Python version: Not applicable; this is C#.
- Command/flags used: Static tracing of the EFC OK chain during issue #637 preparation research,
  against `origin/main` at `ecdb1c84ba8541ab67042985919cfed4df768c01`.
- Data source or fixture: Repository source at that commit.

## Steps to Reproduce

1. Put the add-in into a state where `Globals.Ol.ArchiveRootPath` is unresolvable or cross-store, so
   that `ArchiveRootPathGuard.RequireResolvedArchiveRoot`
   (`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:32-60`) throws `InvalidOperationException`. The
   value is cached only on success (`TaskMaster/AppGlobals/AppOlObjects.cs:253-267`), so the throw
   recurs on every read.
2. Open the Email Filer Control form with an item selected and a valid destination chosen.
3. Path A (keyboard): trigger filing with the always-on `Keys.Return` action
   (`QuickFiler/Controllers/EfcFormController.cs:392`) or the `'K'` character action routed through
   `KbdExecuteAsync(ActionOkAsync)` (`:623`, `:683`; `KbdExecuteAsync` is declared at `:894-904` and
   contains no try/catch). Observe an unhandled UI-thread exception.
4. Path B (button): trigger filing with the OK button. The exception is caught by
   `ButtonOkClickAsync` (`:462-475`) and routed to `BoundaryErrorSink`. Observe that the form was
   already hidden by `ActionOkAsync` at `:756` before the `await` at `:759`, and that
   `_formViewer.Dispose(); Cleanup();` at `:769-770` never run. The item is not filed and no message
   is shown.

## Expected Behavior

An archive-root configuration failure aborts the filing operation benignly: the user is told the
operation could not be completed, the form completes its teardown deterministically, and no
unhandled exception reaches the message pump from any entry point. Every entry point to
`ActionOkAsync` has the same exception posture.

## Actual Behavior

The three entry points to `ActionOkAsync` have two different exception postures. The button path
catches and logs but leaves a half-completed teardown and shows the user nothing. The two keyboard
paths do not catch at all, so the exception is unhandled on the UI thread.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not applicable; established by source tracing. See
  `QuickFiler/Controllers/EfcFormController.cs:392`, `:460-475`, `:738-772`, `:894-904`;
  `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:32-47`;
  `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:32-60`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The trigger requires a misconfigured or cross-store archive root, so it is not expected in normal
operation. Severity is Medium because one of the two outcomes is an unhandled UI-thread exception in
a VSTO add-in, and the other silently abandons a filing operation the user believes has completed.

## Suspected Cause / Notes

- `EfcHomeController.ExecuteMovesAsync` (`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:32-47`)
  uses `try`/`finally` with no `catch`. The `finally` releases the `Interlocked` re-entrancy guard,
  so a throw does not wedge the guard, but the exception is neither observed nor translated.
- `ActionOkAsync` (`QuickFiler/Controllers/EfcFormController.cs:738-772`) orders `Hide` at `:756`
  before the `await` at `:759` and `Dispose`/`Cleanup` at `:769-770` after it.
- The narrowest seam for a benign degrade is `ExecuteMovesAsync` itself: it is the single funnel for
  all three OK entry points, it already owns a `try` block, it sits below the `Hide`/`Dispose`
  sequence so catching there lets `ActionOkAsync` complete its teardown, and it is already driven
  headlessly by `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs` through the
  injectable `MoveToFolderAsyncAction` seam.
- The repository already has the right notification idiom and no new shape should be invented:
  `MoveFailureMessageAction` (`ExecuteMoves.cs:23-24`) and `MessageBoxShowAction`
  (`EfcHomeController.cs:299-305`), both `internal Action<...>` properties defaulting to
  `MessageBox.Show`.
- Any user-facing message must stay value-free. `ArchiveRootPathGuard.UnresolvableRule` and
  `CrossStoreRule` (`ArchiveRootPathGuard.cs:13-17`) are already redacted and are the appropriate
  text to surface.
- Separate but same defect class: `EfcDataModel.OpenOlFolderAsync` (`:299-316`, assignment at `:308`)
  and `OpenFsFolderAsync` (`:318-334`, assignment at `:326`) assign `DestinationOlStem` verbatim and
  read `ArchiveRootPath` the same way. Issue #637 corrects only the `string` overload of
  `MoveToFolderAsync`.
- Related: issue #637 (producer-side stem normalization) and issue #614 (the archive-relative-stem
  invariant and `ArchiveStemContract`).

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `ExecuteMovesAsync` catching an `InvalidOperationException` raised by the
      injected `MoveToFolderAsyncAction` seam and routing it to a captured notification action;
      the re-entrancy guard still released on the throwing path; the notification text carries no
      archive root or mailbox identifier.
- [ ] Unit coverage areas: `ActionOkAsync` teardown ordering, so that `Dispose`/`Cleanup` run on the
      throwing path as well as the succeeding one.
- [ ] Unit coverage areas: the two keyboard entry points share the button path's exception posture.
- [ ] Integration scenario to retest: an ordinary successful filing operation is unchanged, and the
      existing `EfcHomeControllerExecuteMovesTests` suite still passes.
- [ ] Manual verification notes: with an unresolvable archive root, confirm each of the three OK
      entry points shows the same message and leaves no undisposed form.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
