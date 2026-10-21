# P7-T7 — Coverage gap closure

Timestamp: 2026-09-08T10-17
Task: [P7-T7]
Command: two tests added, then the toolchain loop restarted at P7-T1 and P7-T5 and P7-T6 re-run
EXIT_CODE: 0

GAP CLOSURE: REQUIRED AND COMPLETED

The first evaluation of P7-T6 failed clause 2 with two uncovered added lines. Both are now
covered.

## The two gaps and the tests that closed them

### 1. `UtilitiesCS/Extensions/DfDeedle.cs:70` — the `DefaultTableEtl` lambda body

The added line is `> DefaultTableEtl = t => ((Outlook.Table)t).ETL();`, the production ETL delegate
P1-T3 introduced to replace the deleted `TableEtlInvoker` static. Its lambda body executes only
when `GetEmailDataInView` is called with no `etl` argument. Every pre-existing test of that method
passes an injected delegate, so the production default was never invoked and the line reported
zero hits.

Test added, in `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` (the write-set test file
that owns `DfDeedle.cs` per this task's ownership rule):

`UtilitiesCS.Test.Extensions.DfDeedleEtlTimeoutTests.GetEmailDataInView_NoEtlArgument_UsesProductionDefaultDelegate`

It calls `DfDeedle.GetEmailDataInView(explorer.Object)` with no `etl` argument over the same mock
Explorer the class already builds, and asserts the frame has one row. This is the delegate
counterpart of the null-writer default tests added for AC3: it proves the optional parameter's
default reproduces the previous behaviour, which is exactly the invariant spec.md requires of every
new optional parameter.

### 2. `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs:116` — the `GetArray` branch deadline

The added line is `.TimeoutAfter(milliseconds, timeProvider);` on the `else` branch of `EtlAsync`,
taken when the column set contains no `BinaryToStringFields` member and no object converter
matches. It is an added line because P1-T1 changed its second argument from `attempts` to
`timeProvider`. The corresponding pre-change line was also uncovered at baseline (P0-T11 recorded
line 114 among the fifteen uncovered lines of the `EtlAsync` span), so this branch has never had a
test.

Test added, in `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` (the
write-set test file that owns `OlTableExtensions.Etl.cs`):

`UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensionsEtlClockTests.EtlAsync_NoBinaryOrObjectFields_UsesGetArrayBranchOnControlledClock`

It supplies columns `EntryID` and `Subject` — neither is in
`MAPIFields.BinaryToStringFields`, whose members are `ConversationIndex`, `ConversationId` and
`Store` — and passes a null converter dictionary, so the branch condition is false on both
disjuncts and the `GetArray` path is taken. The clock is an un-advanced `FakeTimeProvider`, so the
250 ms deadline is armed but cannot fire. The test asserts the transformed array and that the token
source was not cancelled.

Neither test uses a sleep, a retry, a timed wait, or a mock value chosen to outrun a deadline; both
are covered by the P7-T10 AC5 search over the added lines.

## Loop restart

Adding the two tests required restarting the toolchain loop at P7-T1, which was done. Pass 3
recorded: `P7T1_REWRITTEN_COUNT: 0`, `P7T2_CHECK_EXIT: 0`, `P7T3_EXIT: 0` with `    0 Error(s)` and
0 warnings, `P7T4_EXIT: 0` with `    0 Error(s)` and 0 warnings.

## Re-run results

`p7-t5-vstest-coverage.md` (re-run): `total` 7162, `passed` 7162, `failed` 0, exit 0. The delta
over the P0-T10 baseline of 7153 is +9: the seven C4 tests plus these two gap-closure tests. Both
new tests report `Passed`.

`p7-t6-coverage-delta.md` (re-run): all six clauses pass.
`ADDED_TOTAL=76 EXECUTABLE_COVERED=38 NON_EXECUTABLE=38 UNCOVERED=0`.

## Acceptance evaluation

- The artifact exists and records the outcome. PASS
- The added test names are listed, fully qualified. PASS
- A passing re-run of P7-T5 and P7-T6 accompanies them. PASS

## Output Summary

Two coverage gaps closed with two tests, both placed in the write-set test file that owns the
production file. Clause 2 moved from 2 uncovered added lines to 0. The toolchain loop was restarted
and completed clean on pass 3.
