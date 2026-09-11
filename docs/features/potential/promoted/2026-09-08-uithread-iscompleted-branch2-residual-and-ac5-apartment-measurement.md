# uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement (Issue #816)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement/ (Issue #816)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #816
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/816
- Last Updated: 2026-09-08
## Summary

Two residuals left by issue 809, both in `UtilitiesCS` threading. First, the branch-2 residual in
`SynchronizationContextAwaiter.IsCompleted` at `UtilitiesCS/Threading/UiThread.cs:177-178` is
unhardened. Second, 809's AC5 merged unchecked because its apartment-state measurement was never
taken: the probe inferred the apartment from a research premise that the same delivery falsified.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8
- Python version: not applicable
- Command/flags used: `vstest.console.exe` over `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll`
- Data source or fixture: no live Outlook host required

## Steps to Reproduce

1. Read `UtilitiesCS/Threading/UiThread.cs:177-178` and observe the second `IsCompleted` branch is
   not hardened the way branch 1 was by issue 809.
2. Read `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18` and confirm it carries the repository's only
   assembly-level `Parallelize` attribute.
3. Search for a `.runsettings` setting `ExecutionThreadApartmentState`; none exists.
4. Read issue 809's `[P0-T15]` probe evidence and confirm it never calls
   `Thread.CurrentThread.GetApartmentState()`.

## Expected Behavior

Both `IsCompleted` branches enforce the same contract, and AC5's claim about MTA behaviour rests on a
measurement of the executing thread's apartment state rather than on an inference.

## Actual Behavior

Branch 2 is unhardened, so the two branches do not enforce the same contract.

For AC5: because no `.runsettings` sets `ExecutionThreadApartmentState` and the only assembly-level
`Parallelize` attribute is the one at `AssemblyInfo.cs:18`, the probe most likely ran STA and no MTA
measurement exists. The probe inferred apartment state from research premise R4, which issue 809's
own delivery falsified.

Consequently the earlier statement that the issue 782 latch regression "did not reproduce" was
**withdrawn** during 809's execution. The correct status is **unknown**, not negative. Correction
sections were committed to three evidence artifacts (`p0-t15`, `p6-t4`, `p6-t13` section 6) so the
withdrawn claim did not merge as an unqualified assertion, and AC5 was left unchecked in `spec.md`.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: see issue 809's `p6-t13` section 5 for the branch-2 residual, and the three corrected
  evidence artifacts named above for the withdrawal.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: merged work carries one unmet acceptance criterion, and an asymmetric contract between two
branches of the same predicate is exactly the shape that produced issues 784, 787 and 788.

## Suspected Cause / Notes

Reported by the execution child for issue 809 and independently verified by the run orchestrator,
which checked the `Parallelize` attribute and the absence of a `.runsettings` apartment setting
directly rather than accepting the child's account.

Keep the hardening and its test in one change. The hardening changes what the test must assert, so
splitting them produces an intermediate state in which neither can be satisfied.

The predicate constraint from 809 still applies and must not be relaxed here: `IsCompleted` compares
`_context == SynchronizationContext.Current` by reference. A bare owning-thread-identity predicate is
unsafe, because a continuation resumed after `ConfigureAwait(false)` can land on a recycled
thread-pool thread whose managed id equals the owner's. See
`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:263-272`, and
`QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:183-199`, which fails under a bare-id predicate.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: harden `IsCompleted` branch 2 and assert both branches enforce the same
      contract; take the apartment-state measurement with an explicit
      `Thread.CurrentThread.GetApartmentState()` call under both STA and MTA hosts.
- [ ] Integration scenario to retest: the issue 782 latch-regression scenario, once the apartment
      state under which it runs is actually known.
- [ ] Manual verification notes: settle AC5 of issue 809 and record whether the 782 regression
      reproduces, replacing the current "unknown" with a measured answer.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
