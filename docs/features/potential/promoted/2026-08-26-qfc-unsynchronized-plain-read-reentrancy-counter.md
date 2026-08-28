# qfc-unsynchronized-plain-read-reentrancy-counter (Issue #634)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-unsynchronized-plain-read-reentrancy-counter/ (Issue #634)
- Captures: **follow-up candidate 8** of `## Follow-up Candidates` in
  `docs/features/active/qfc-collection-controller-defects-468/spec.md`
- Origin: issue **#468** defect family, task `[P14-T5]`

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #634
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/634
- Last Updated: 2026-08-26
## Summary

`QfcCollectionController` guards `RemoveSpecificControlGroupAsync` with a private static reentrancy
counter. Every write to that counter is interlocked — `Interlocked.Increment` at
`QuickFiler/Controllers/QfcCollectionController.cs:1015` and `Interlocked.Decrement` at `:1110` — but
the read that makes the guard decision is a **plain, unsynchronized read**:

```
QuickFiler/Controllers/QfcCollectionController.cs:1093
    if (removespecificcontrolgroupcounter > 1)
```

The field is a plain `static int` (`:1011`) with no `volatile` modifier, so the read is not
guaranteed to observe the most recent interlocked write from another thread.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Framework: .NET Framework 4.8.1, VSTO Outlook add-in
- Command/flags used: not reproducible from a command line
- Data source or fixture: concurrent invocation of `RemoveSpecificControlGroupAsync`

## Steps to Reproduce

No deterministic reproduction is known. The defect is a memory-visibility hazard, not a logic error;
it would present as an intermittent failure of the reentrancy guard to observe a concurrent entry.

## Expected Behavior

The read participates in the same synchronization discipline as the writes — for example
`Interlocked.CompareExchange(ref removespecificcontrolgroupcounter, 0, 0)`, or `Volatile.Read`, or
declaring the field `volatile`.

## Actual Behavior

The read is plain. On x86/x64 the practical risk is low because of the strong hardware memory model,
but the code does not express the guarantee it depends on, and the guarantee is not one the language
provides.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no captured log; identified by review during issue #286 triage and recorded in the
  promoted document `2026-07-09-qfc-collectioncontroller-removespecificcontrolgroup-counter-leak.md:56`
  as a secondary concern to the primary leak-on-exception defect, and restated at
  `docs/features/active/qfc-collection-controller-defects-468/spec.md:463-465`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Secondary to the primary #286 defect, which is fixed. The guard's purpose is to log a diagnostic when
reentrancy is detected, so a missed observation degrades a diagnostic rather than corrupting state.

## Suspected Cause / Notes

The counter was written interlocked because the increment and decrement must be atomic; the read was
left plain because it is a single-value comparison. That reasoning is correct about atomicity and
silent about visibility.

Issue #468's branch deliberately did **not** change this. Its spec states that the change is not
required by the feature and that "if the executor changes it, the change must be called out
separately." The branch left it untouched, so the audit trail for #286 records only the `finally`-block
fix.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: the existing tests
      `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` and
      `RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter` must continue to
      pass unchanged; a visibility fix should not alter observable single-threaded behaviour
- [ ] Integration scenario to retest: none — no deterministic concurrent scenario exists
- [ ] Manual verification notes: confirm the chosen form does not reintroduce a race between the read
      and the surrounding increment/decrement pair

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Source: issue #286 triage, carried forward through issue #468's `## Follow-up Candidates`.
