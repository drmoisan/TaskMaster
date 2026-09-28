# Fail-before exception dossier — the apartment measurement

Timestamp: 2026-09-13T23-22

Subject: `UtilitiesCS.Test.Threading.UiThreadApartmentMeasurement_Tests.SyncContextFormShow_OnAThreadMeasuredAsMta_RecordsTheOutcome`

WhyFailingRunImpossible: Clause (i) of issue #809's AC5 is a measurement obligation rather than a
gate. The measurement test asserts only that the thread it created is MTA, and it creates that
thread itself with an explicit apartment, so there is no pre-change state in which that assertion
fails and a post-change state in which it passes. The change this delivery makes is confined to one
condition in `SynchronizationContextAwaiter.IsCompleted` and does not touch apartment selection
anywhere, so no ordering of the plan's tasks can produce a red-then-green pair for this test.

## Alternative proof section — absence of any prior in-tree apartment measurement

The obligation this test discharges is that the apartment be **read on the executing thread at
runtime** rather than inferred. The proof that no prior test discharged it is an absence claim, so
it is recorded auditably.

SearchScope: the whole worktree at
`docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816`'s
repository root, recursively, over every `*.cs` file.

SearchPatterns: `GetApartmentState\(\)`

SearchResult: three matches, and only three.

| Path | Line | Nature of the read |
|---|---|---|
| `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` | 244 | The only pre-existing **test** read |
| `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` | 133 | The new measurement added by this delivery |
| `UtilitiesCS/Threading/UiThread.cs` | 30 | A **production** read inside `UiThread.Init()`, not a test |

The single pre-existing test read at line 244 cannot settle the ambient case. It is the first
Arrange statement of `Init_OnStaThread_DoesNotThrowAndPopulatesAllFourCaptureFields`, which is
declared `[STATestMethod]`. That attribute forces the method onto an STA thread, so the assertion
`Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA)` measures a **forced STA
thread**. It therefore reports nothing about what happens when the production capture form is shown
on a thread that is MTA, which is precisely the scenario issue #782 raised and issue #809's AC5
left unsettled.

## What stands in place of a failing run

The measurement's own guard assertion is the falsifiable part, and it is a PASS condition rather
than a fail-before condition: P3-T1 and AC6 both require the recorded guard value to be `MTA`, and
both declare the run **void** if it is anything else. That is the mechanism that prevents the defect
the earlier probe suffered, which was a probe that inferred its own apartment from a research
premise the same delivery falsified. A probe that silently ran on an STA thread would now fail the
guard assertion inside the test and be recorded Failed, rather than reporting a conclusion it never
measured.

The settling value is recorded and not asserted, because AC5 of issue #809 asks what happens, not
that a particular thing happens.
