- Work Mode: full-bug

## Summary

Two sites where a teardown guard applied by item 810 was not carried to a sibling site. Both are
enumeration gaps in that fix rather than new regressions, both are High, and both are fixed by one
enumeration pass rather than two. Consolidates what was filed separately as #821 and #822.

**Site A — `QuickFiler/Controllers/QfcHomeController.cs:403`.** `Cleanup` invokes the parent-cleanup
callback as `ParentCleanup?.Invoke();` inside a `finally`, without the read-into-local-then-clear
idiom that item 810's AC4 applied one level down. A repeated direct `Cleanup()` therefore releases
the ribbon twice. The existing test cannot observe this, because its second `Cleanup()` call sits
after the `Times.Once` assertion.

**Site B — `UtilitiesCS/Threading/ProgressViewer.cs:75`.** `CancelButton_Click` calls
`_cancelSource!.Cancel()`. The null-forgiving operator suppresses the compiler's null check rather
than guarding the call, and the invariant its comment relies on is not enforced by the property
setter at line 60. This is a fourth sharer of the cancellation token source that the earlier review
never enumerated, and it is unaffected by item 810's AC3 fix because it holds its own captured
reference.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8 VSTO add-in hosted by Outlook
- Python version: not applicable; this is C# in `QuickFiler` and `UtilitiesCS`
- Command/flags used: `vstest.console.exe` over `QuickFiler.Test.dll` and `UtilitiesCS.Test.dll`
- Data source or fixture: no live Outlook host required for either reproduction

## Steps to Reproduce

Site A:

1. Read `QuickFiler/Controllers/QfcHomeController.cs:401-405`:

   ```csharp
   finally
   {
       ParentCleanup?.Invoke();
       logger.Info("Home cleanup complete; ribbon release callback invoked.");
   }
   ```

2. Call `Cleanup()` twice on the same controller instance; the ribbon-release callback fires on both.
3. Inspect the existing test: its second `Cleanup()` call is placed after the `Times.Once`
   assertion, so the assertion is evaluated before the second invocation can affect it.

Site B:

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

2. Read the property setter at line 60, `ButtonCancel.Enabled = value != null;` — a second path to
   enabling the button that does not go through `SetCancellationTokenSource`.
3. Reach the click handler by any path that enables the button without assigning `_cancelSource`,
   or after another sharer has disposed the source.

## Expected Behavior

Site A: the parent-cleanup callback fires at most once per controller. The established idiom in this
subsystem, applied by item 810 one level down, is to read the delegate into a local, clear the
field, then invoke the local, so a second call finds nothing to invoke.

Site B: clicking Cancel either cancels the operation or does nothing. It does not raise an unhandled
exception on a WinForms event handler.

## Actual Behavior

Site A: the callback fires on every `Cleanup()` call. The null-conditional operator guards against
the delegate being null, not against it having already run. The test written to prevent exactly this
cannot fail; moving its second `Cleanup()` call above the `Times.Once` assertion is what makes the
defect visible, and doing so belongs in this fix rather than in a separate change.

Site B: `_cancelSource!.Cancel()` throws when `_cancelSource` is null, and throws
`ObjectDisposedException` when another sharer has already disposed the source. The comment states an
invariant the setter at line 60 does not enforce, so it is asserted rather than guaranteed. The
comment says the null-forgiving operator "preserves the prior NRE-if-null behavior"; preserving an
NRE is a legitimate choice, but on a `#nullable enable` file the `!` also removes the compiler's
ability to warn about it, so the choice is now invisible to static analysis.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none captured for either site. Both findings are from reading the call sites; neither the
  double invocation nor the throw has been observed at runtime, and confirming them at runtime is
  part of the fix.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High for both sites. Site A is a real resource-lifecycle defect whose own coverage is structurally
blind to it — the same failure shape that let issues 784, 787 and 788 persist. Site B is an
unhandled exception on a UI event handler in a VSTO add-in, which surfaces to the user, and is an
enumeration gap in a fix that was believed complete.

## Suspected Cause / Notes

Both sites were raised during item 810's review and execution on run `bugs-2026-09-06`, and both
were verified directly against the tree on 2026-09-08: `QfcHomeController.cs:403` reads
`ParentCleanup?.Invoke();` inside a `finally` at lines 401-405, and `ProgressViewer.cs:75` reads
`_cancelSource!.Cancel();` with a second enabling path at line 60.

Two supporting claims are child-reported and were **not** independently re-derived — confirm each
before relying on it: the assertion ordering inside site A's existing test, and the
characterization of site B as the *fourth* sharer. Enumerate the sharers as the first step of the
fix rather than trusting the count.

These are consolidated into one issue because they are the same defect class from the same origin
item: a guard applied at one site and not carried to its siblings. One enumeration pass over the
cleanup-invoker and token-source-sharer sets addresses both; two separate fixes would each perform a
partial enumeration and could each miss the other's sites.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test that calls `Cleanup()` twice and asserts the ribbon-release
      callback fired exactly once, with both calls before the assertion; a test invoking the cancel
      click handler with `_cancelSource` null, and one with it already disposed, asserting neither
      throws out of the handler.
- [ ] Integration scenario to retest: open and close QuickFiler repeatedly, confirming ribbon state
      remains correct; cancel a long-running operation, then cancel again after the source has been
      disposed.
- [ ] Manual verification notes: confirm site A's existing test fails before the fix once its second
      `Cleanup()` call is moved above the assertion — a fix whose test passes both before and after
      has demonstrated nothing. Enumerate every holder of the shared `CancellationTokenSource` and
      record the list in the fix so the next reviewer inherits the enumeration. Decide explicitly
      whether the "preserve NRE-if-null" intent still stands; if it does, express it as a thrown
      exception with a message, not as a suppressed compiler check.

## Source

From: docs/features/potential/promoted/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release.md
and docs/features/potential/promoted/2026-09-08-progressviewer-cancel-suppressed-null-check-fourth-sharer.md

