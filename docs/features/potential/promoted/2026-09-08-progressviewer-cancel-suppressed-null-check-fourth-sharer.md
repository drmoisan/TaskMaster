# progressviewer-cancel-suppressed-null-check-fourth-sharer (Issue #822)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/progressviewer-cancel-suppressed-null-check-fourth-sharer/ (Issue #822)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #822
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/822
- Last Updated: 2026-09-08
## Summary

`ProgressViewer.CancelButton_Click` calls `_cancelSource!.Cancel()` at
`UtilitiesCS/Threading/ProgressViewer.cs:75`. The null-forgiving operator suppresses the compiler's
null check rather than guarding the call, and the invariant the comment above it relies on is not
enforced by the type. This is a fourth sharer of the cancellation token source that the earlier
review never enumerated, and it is unaffected by issue 810's AC3 fix because it holds its own
captured reference.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8 VSTO add-in hosted by Outlook
- Python version: not applicable; this is C# in `UtilitiesCS`
- Command/flags used: `vstest.console.exe` over `UtilitiesCS.Test.dll`
- Data source or fixture: no live Outlook host required

## Steps to Reproduce

1. Read `UtilitiesCS/Threading/ProgressViewer.cs:70-77`:

   ```csharp
   private void CancelButton_Click(object sender, EventArgs e)
   {
       // Invariant: ButtonCancel is enabled only after SetCancellationTokenSource assigns
       // _cancelSource, so a click here implies _cancelSource is non-null (preserves the prior
       // NRE-if-null behavior).
       _cancelSource!.Cancel();
       this.Close();
   }
   ```

2. Read the property setter at line 60, `ButtonCancel.Enabled = value != null;`, which is a second
   path to enabling the button and does not go through `SetCancellationTokenSource`.
3. Reach the click handler by any path that enables the button without assigning `_cancelSource`,
   or after the source has been disposed by another sharer.

## Expected Behavior

Clicking Cancel either cancels the operation or does nothing. It does not raise an unhandled
exception on a WinForms event handler.

## Actual Behavior

`_cancelSource!.Cancel()` throws when `_cancelSource` is null, and throws
`ObjectDisposedException` when another sharer has already disposed the source. The comment states an
invariant — that the button is enabled only after `SetCancellationTokenSource` runs — which the
setter at line 60 does not enforce, so the invariant is asserted rather than guaranteed.

The comment explicitly says the null-forgiving operator "preserves the prior NRE-if-null behavior".
Preserving an NRE is a deliberate choice, but on a `#nullable enable` file the `!` also removes the
compiler's ability to warn about it, so the choice is now invisible to static analysis.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none captured. The finding is from reading the call site; the throw has not been observed
  at runtime.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: an unhandled exception on a UI event handler in a VSTO add-in surfaces to the user, and this
is an enumeration gap in a fix that was believed complete — the third sharer was addressed while
this fourth one was never counted.

## Suspected Cause / Notes

**Consolidated 2026-09-08.** Promoted as issue 822, which was then closed and folded into issue 821
as site B. Issue 821 covers both this finding and the `QfcHomeController.cs:403` parent-cleanup
double invocation, because they are the same defect class from the same origin item and one
enumeration pass addresses both. This record is retained unchanged as the source for site B; work
the finding from issue 821.


Reported as a report-only item during issue 810's execution and verified directly against the tree
on 2026-09-08: line 75 is `_cancelSource!.Cancel();` and the setter at line 60 is a second enabling
path. The characterization of this as the *fourth* sharer comes from the child's report and was not
independently re-derived; enumerate the sharers as the first step of the fix rather than trusting
the count.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test invoking the click handler with `_cancelSource` null, and one with
      it already disposed, asserting neither throws out of the handler.
- [ ] Integration scenario to retest: cancel a long-running operation, then cancel again after the
      source has been disposed.
- [ ] Manual verification notes: enumerate every holder of the shared `CancellationTokenSource` and
      record the list in the fix, so the next reviewer inherits the enumeration rather than
      repeating it. Decide explicitly whether the "preserve NRE-if-null" intent still stands; if it
      does, express it as a thrown exception with a message, not as a suppressed compiler check.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
