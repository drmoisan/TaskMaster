# P0-T3 anchors and census
Timestamp: 2026-10-01T06-35
Command: git merge-base origin/main HEAD; git diff --name-only <MERGE-BASE>; git status --porcelain; git diff --numstat <MERGE-BASE> -- <4 footprint paths>; ReadAllLines; git grep -c -F
EXIT_CODE: 0
Output Summary:
MERGE-BASE: 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f
PRE-EXISTING-PATHS: docs/features/potential/promoted/2026-09-29-breadcrumb-dispatch-message-and-handoff-record-inaccurate.md
PORCELAIN-AT-P0:
   M docs/features/active/2026-09-29-breadcrumb-dispatch-message-and-handoff-record-inaccurate-941/plan.2026-09-29T23-03.md
  ?? docs/features/active/2026-09-29-breadcrumb-dispatch-message-and-handoff-record-inaccurate-941/evidence/baseline/
NUMSTAT-FOOTPRINT-COUNT: 0
LINES QuickFiler/Viewers/BreadcrumbUiDispatcher.cs = 285
LINES QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs = 480
LINES QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs = 386
LINES docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md = 180
COUNT [cannot marshal cross-thread UI work] in QuickFiler/Viewers/BreadcrumbUiDispatcher.cs = 2 (expected 2)
COUNT [cannot marshal cross-thread UI work] in QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs = 1 (expected 1)
COUNT [cannot marshal] in QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs = 1 (expected 1)
COUNT [outside an executing Dispatch callback] in *.cs = 0 (expected 0)
COUNT [DispatchValue_OwnerOnlyOnOwnerThread_FaultsOutsideExecutingCallback] in *.cs = 0 (expected 0)
COUNT [owner-thread-id check rather than against] in docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md = 1 (expected 1)
COUNT [expected to throw a cross-thread marshalling] in docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md = 1 (expected 1)
COUNT [Its exposure is lower but not zero] in docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md = 1 (expected 1)
COUNT [: the owner check compares] in docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md = 1 (expected 1)
COUNT [Only the first is exercising] in docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md = 0 (expected 0)
COUNT [:276-277] in docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md = 0 (expected 0)
COUNT [no idle-thread-reuse exposure] in docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md = 0 (expected 0)
