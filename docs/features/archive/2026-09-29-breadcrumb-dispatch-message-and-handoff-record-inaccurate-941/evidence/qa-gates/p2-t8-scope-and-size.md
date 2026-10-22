# P2-T8 scope and size
Timestamp: 2026-10-01T07-29
Command: git diff --name-only 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f (filtered); git status --porcelain; [System.IO.File]::ReadAllLines line counts; git grep -n -F "cannot marshal" -- "*.cs"
EXIT_CODE: 0
Output Summary:
MERGE-BASE: 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f (recorded value; `git merge-base origin/main HEAD` still prints the same value)
FILTERED-NAME-LIST (feature-folder and .claude/agent-memory/ prefixes dropped):
  QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
  QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs
  QuickFiler/Viewers/BreadcrumbUiDispatcher.cs
  docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md
  docs/features/potential/promoted/2026-09-29-breadcrumb-dispatch-message-and-handoff-record-inaccurate.md
UNION-CHECK: equals PRE-EXISTING-PATHS (docs/features/potential/promoted/2026-09-29-breadcrumb-dispatch-message-and-handoff-record-inaccurate.md) plus the four footprint paths: held
PORCELAIN:
  ?? docs/features/active/2026-09-29-breadcrumb-dispatch-message-and-handoff-record-inaccurate-941/evidence/qa-gates/
PORCELAIN-PATHS-WITHIN-ALLOWED-PREFIXES: held (the single path lies under the feature-folder prefix)
PS1-PATHS-LISTED: 0
LINES QuickFiler/Viewers/BreadcrumbUiDispatcher.cs = 285
LINES QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs = 480
LINES QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs = 410
SIZE-GATE: all three at or below 500: held
CENSUS (git grep -n -F "cannot marshal" -- "*.cs"), exactly two hits:
  QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:86
  QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:101
Acceptance: held
