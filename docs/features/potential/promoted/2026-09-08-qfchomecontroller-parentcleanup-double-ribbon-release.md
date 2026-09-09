# qfchomecontroller-parentcleanup-double-ribbon-release (Issue #821)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfchomecontroller-parentcleanup-double-ribbon-release/ (Issue #821)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #821
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/821
- Last Updated: 2026-09-08
## Summary

`QfcHomeController.Cleanup` invokes the parent-cleanup callback at
`QuickFiler/Controllers/QfcHomeController.cs:403` as `ParentCleanup?.Invoke();` inside a `finally`,
without the read-into-local-then-clear idiom that issue 810's AC4 applied one level down. A repeated
direct `Cleanup()` therefore releases the ribbon twice. The existing test cannot observe this,
because its second `Cleanup()` call sits after the `Times.Once` assertion.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8 VSTO add-in hosted by Outlook
- Python version: not applicable; this is C# in `QuickFiler`
- Command/flags used: `vstest.console.exe` over `QuickFiler.Test.dll`
- Data source or fixture: no live Outlook host required for the unit-level reproduction

## Steps to Reproduce

1. Read `QuickFiler/Controllers/QfcHomeController.cs:401-405`. The `finally` block is:

   ```csharp
   finally
   {
       ParentCleanup?.Invoke();
       logger.Info("Home cleanup complete; ribbon release callback invoked.");
   }
   ```

2. Call `Cleanup()` twice on the same controller instance.
3. Observe the ribbon-release callback fires on both calls.
4. Inspect the existing test: its second `Cleanup()` call is placed after the `Times.Once`
   assertion, so the assertion is evaluated before the second invocation can affect it.

## Expected Behavior

The parent-cleanup callback fires at most once per controller. The established idiom for this in the
same subsystem, applied by issue 810 one level down, is to read the delegate into a local, clear the
field, then invoke the local — so a second call finds nothing to invoke.

## Actual Behavior

The callback fires on every `Cleanup()` call. The null-conditional operator guards against the
delegate being null, not against it having already run.

The test that appears to cover this is structurally blind to it: moving its second `Cleanup()` call
above the `Times.Once` assertion is what makes the defect visible, and doing so should be part of
the fix rather than a separate change.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none captured. The finding is from reading the call site; the double invocation has not
  been observed at runtime, and confirming it at runtime is part of the fix.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High for two reasons. The double release is a real resource-lifecycle defect in the ribbon teardown
path. More importantly, the test written to prevent exactly this cannot fail, so the defect is
protected from discovery by its own coverage — the same failure shape that let issues 784, 787 and
788 persist.

## Suspected Cause / Notes

**Consolidated 2026-09-08.** Promoted as issue 821, which was then widened to cover a second site:
the suppressed null check at `UtilitiesCS/Threading/ProgressViewer.cs:75`, originally filed as issue
822 and now closed into 821. Both are enumeration gaps in item 810's teardown hardening. This record
is the source for site A.


Raised as CR-1 in the code review for issue 810 and verified directly against the tree on
2026-09-08: `QfcHomeController.cs:403` reads `ParentCleanup?.Invoke();` inside a `finally` at lines
401-405. The claim about the test's assertion ordering is from the review and was not independently
re-derived; confirm it before relying on it.

Issue 810 applied the read-into-local-then-clear idiom one level down but did not carry it up to
this call site, so this is an enumeration gap in that fix rather than a new regression.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test that calls `Cleanup()` twice and asserts the ribbon-release
      callback fired exactly once, with both calls placed before the assertion.
- [ ] Integration scenario to retest: open and close QuickFiler repeatedly and confirm the ribbon
      state remains correct.
- [ ] Manual verification notes: confirm the existing test fails before the fix once its second
      `Cleanup()` call is moved above the assertion — a fix whose test passes both before and after
      has not demonstrated anything.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
