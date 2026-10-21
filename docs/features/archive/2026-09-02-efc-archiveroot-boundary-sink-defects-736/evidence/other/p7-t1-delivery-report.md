# P7-T1 — Delivery report for issue #736

Timestamp: 2026-09-04T02-26

Five in-scope findings are delivered: findings 1, 2, 4, 5, and 6. Finding 3, the `ActionOkAsync`
disposal reordering, is out of scope for this item and is owned by a sibling item.

All evidence paths below are relative to the repository root and abbreviate
`docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/` as `<FF>/`.

---

## Finding 1 — the archive-root read escaped its documented contract as a `COMException`

**Regression test file:** `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs`

| Test method |
|---|
| `ResolveValidatedArchiveRootPath_WhenComposedReadThrowsComException_NormalizesToInvalidOperation` |
| `ResolveValidatedArchiveRootPath_WhenResolvedReadThrowsComException_NormalizesToInvalidOperation` |
| `ResolveValidatedArchiveRootPath_WhenComReadFails_MessageWithholdsPathAndMailboxAddress` |
| `ResolveValidatedArchiveRootPath_WhenBothReadsResolve_ReturnsPathAndEmitsNoDiagnostic` |
| `ResolveValidatedArchiveRootPath_WhenResolvedFolderIsNull_ThrowsUnresolvableWithNoInnerException` |
| `ResolveValidatedArchiveRootPath_WhenComReadFailsTwice_ReReadsTheComposedPathOnTheSecondCall` |

- **Recorded failing run:** `<FF>evidence/regression-testing/p1-t7-finding1-red.md` — total 6, passed
  2, failed 4 against the defect-preserving seam.
- **Recorded passing run:** `<FF>evidence/regression-testing/p1-t9-finding1-green.md` — total 6,
  passed 6, failed 0 after the minimal fix.

---

## Finding 2 — the `KbdExecuteAsync` dispatch boundary contained nothing

**Regression test file:** `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs`

| Test method | Recorded failing run | Recorded passing run |
|---|---|---|
| `KbdExecuteAsync_FuncTaskOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow` | `<FF>evidence/regression-testing/p2-t4-finding2-red.md`, `<FF>evidence/regression-testing/p2-t8-finding2-red.md` | `<FF>evidence/regression-testing/p2-t10-finding2-green.md` |
| `KbdExecuteAsync_ActionOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow` | `<FF>evidence/regression-testing/p2-t4-finding2-red.md`, `<FF>evidence/regression-testing/p2-t8-finding2-red.md` | `<FF>evidence/regression-testing/p2-t10-finding2-green.md` |
| `RunKbdGuardedAsync_WhenBodyThrowsOperationCanceled_DoesNotReportAsFault` | `<FF>evidence/regression-testing/p2-t8-finding2-red.md` | `<FF>evidence/regression-testing/p2-t10-finding2-green.md` |
| `RunKbdGuardedAsync_WhenBodyThrowsInvalidOperation_ReportsExactlyOnce` | `<FF>evidence/regression-testing/p2-t8-finding2-red.md` | `<FF>evidence/regression-testing/p2-t10-finding2-green.md` |
| `RunKbdGuardedAsync_WhenBodyCompletes_InvokesBodyAndReportsNothing` | none — see note | `<FF>evidence/regression-testing/p6-t13-kbd-success-path.md` |
| `KbdExecuteAsync_FuncTaskOverload_WhenToggleSucceeds_AwaitsTheAction` | none — see note | `<FF>evidence/regression-testing/p6-t13-kbd-success-path.md` |
| `KbdExecuteAsync_ActionOverload_WhenToggleSucceeds_InvokesTheAction` | none — see note | `<FF>evidence/regression-testing/p6-t13-kbd-success-path.md` |

**Note on the last three.** They are the round-5 amendment authored by P6-T13, which closes a UT2
Scenario Completeness gap: every other test of the containment guard exercises a fault path, so the
guard had never been observed letting a normal call through. P6-T13 changes no production code, so a
failing-first run is structurally impossible rather than merely inconvenient; its artifact records
that in a `WhyFailingRunImpossible:` field per the fail-before exception convention, so the absence
of a red run is auditable rather than silent.

---

## Finding 4 — the default boundary sink wrote a log line and surfaced nothing to the user

**Regression test file:** `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs`

| Test method | Recorded failing run | Recorded passing run |
|---|---|---|
| `BoundaryErrorSink_DefaultDelegate_RoutesThroughTheUserFaultNotifier` | `<FF>evidence/regression-testing/p4-t4-finding4-red.md` | `<FF>evidence/regression-testing/p4-t6-finding4-green.md` |
| `BoundaryErrorSink_DefaultDelegate_ReturnsWithoutBlockingTheCallingThread` | `<FF>evidence/regression-testing/p4-t4-finding4-red.md` | `<FF>evidence/regression-testing/p4-t6-finding4-green.md` |
| `KbdExecuteAsync_WhenBoundaryErrorSinkIsNull_DoesNotThrow` | `<FF>evidence/regression-testing/p2-t4-finding2-red.md`, `<FF>evidence/regression-testing/p2-t8-finding2-red.md` | `<FF>evidence/regression-testing/p2-t10-finding2-green.md` |
| `KbdExecuteAsync_WhenBoundaryErrorSinkThrows_DoesNotThrow` | `<FF>evidence/regression-testing/p2-t4-finding2-red.md`, `<FF>evidence/regression-testing/p2-t8-finding2-red.md` | `<FF>evidence/regression-testing/p2-t10-finding2-green.md` |

The last two were authored in P2-T2 alongside the finding-2 tests and share their red and green
artifacts, but they exercise the null-sink and throwing-sink branches of `TryReportBoundaryFault`,
which is AC5's fourth conjunct and belongs to finding 4. They are listed here for that reason.

---

## Finding 5 — the breadcrumb-bind archive-root read reported to the log only

**Regression test file:** `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs`

| Test method |
|---|
| `BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow` |

- **Recorded failing run:** `<FF>evidence/regression-testing/p3-t2-finding5-red.md` — total 1, passed
  0, failed 1, the observed sink invocation count 0 where 1 was expected.
- **Recorded passing run:** `<FF>evidence/regression-testing/p3-t4-finding5-green.md` — total 2,
  passed 2, failed 0, naming both this test and its positive sibling
  `Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter`.

---

## Finding 6 — the data-model success-path test depended on an incidental downstream crash

**Regression test file:** `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`

| Test method |
|---|
| `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` |

- **Recorded failing run:** `<FF>evidence/regression-testing/p5-t2-finding6-red.md` — total 1, passed
  0, failed 1, the failure naming `NullReferenceException`, which is the incidental collaborator
  crash the rewrite exists to stop depending on.
- **Recorded passing run:** `<FF>evidence/regression-testing/p5-t5-finding6-green.md` — total 11,
  passed 11, failed 0 across the whole class, with the `InvokeFilerAsync` seam supplying the
  deliberate stopping point.

---

## Method-name union

The union of the method names listed above is **nineteen** names: the eighteen quoted in the plan's
literals block plus `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`, which is a
pre-existing test this item rewrote rather than one it created. The literals-block count is eighteen
rather than fifteen because P6-T13, the round-5 amendment, added three success-path names to that
block; those three belong to finding 2 and are listed under it, and their recorded run is the P6-T13
artifact.

| Finding | Method count |
|---|---|
| 1 | 6 |
| 2 | 7 |
| 4 | 4 |
| 5 | 1 |
| 6 | 1 |
| **Total** | **19** |

Every artifact path named in this report exists on disk under this feature folder's evidence
subdirectory.
