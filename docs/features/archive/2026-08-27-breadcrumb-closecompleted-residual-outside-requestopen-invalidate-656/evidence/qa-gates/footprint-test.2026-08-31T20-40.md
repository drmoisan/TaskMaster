# QA Gate — Test Footprint (Issue #656)

Timestamp: 2026-09-01T14-56
Task: [P4-T13]
Satisfies: AC-12; together with P3-T4 also satisfies AC-5, AC-6, AC-7 and AC-8

Command (authoritative):
```
git diff --name-only origin/main...HEAD -- QuickFiler.Test
git status --porcelain -- QuickFiler.Test
```

EXIT_CODE: 0

## Authoritative diff output (base `origin/main`, verbatim)

```
QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs
```

## Porcelain output (verbatim)

```
```

(empty)

The diff output is exactly the single line
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` and the porcelain output
is empty. AC-12 is satisfied: no file under `QuickFiler.Test/` other than the sole authorized test
file appears in the change set.

## Mechanical proof that the standing guards were not edited

Neither `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` nor
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` appears in the diff
output. Those two files hold all five standing-guard tests:

| Standing guard | File | AC |
|---|---|---|
| `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` | `BreadcrumbDropDownOpenCoordinatorTests.cs` | AC-5 |
| `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` | `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | AC-6 |
| `RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync` | `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | AC-7 |
| `CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce` | `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | AC-8 |
| `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` | `BreadcrumbDropDownOpenCoordinatorTests.cs` | AC-9 |

Because neither file is in the change set, no assertion text in any of those tests was altered. That
is the "unchanged in the diff" half of AC-5 through AC-8. The "passes" half is established by
`evidence/qa-gates/standing-guards.2026-08-31T20-40.md`, where all five ran and all five passed.
The two artifacts together satisfy AC-5, AC-6, AC-7 and AC-8; the pass alone satisfies AC-9.

This matters because the remedy was chosen specifically to avoid a regression trade. Options that
cleared `_closeCompleted` on the successful-close path would have required editing the very tests
listed above; the fact that those files are absent from the diff is the evidence that no such trade
was made.

## Base-ref substitution (recorded, not silent)

Against the plan's stale pinned base the same query lists seven paths:

```
git diff --name-only 2b85134b42872e405602e6064e02dc9cda6c319b...HEAD -- QuickFiler.Test
QuickFiler.Test/Controllers/FilerQueueTests.cs
QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs
QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs
QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs
```

Six of those seven are pre-existing changes inherited from `main` through the pre-execution
reconciliation merge; none was touched by this item. `origin/main`
(`5670b3cfe6a52e3b890bf80f0cd85a20d4fe4723`, an ancestor of HEAD) isolates this branch's own
contribution and is used as authoritative. Both measurements are recorded so the substitution is
auditable.

Output Summary: Test footprint verified. Against `origin/main`, exactly one file under
`QuickFiler.Test/` changed:
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`. The scoped porcelain
output is empty. Neither file holding a standing guard appears. AC-12 is satisfied, and AC-5 through
AC-8 are satisfied jointly with the standing-guards run.
