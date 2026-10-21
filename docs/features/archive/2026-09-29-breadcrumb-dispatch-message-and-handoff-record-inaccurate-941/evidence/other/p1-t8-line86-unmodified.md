# P1-T8 Line 86 boundary assertion unmodified

Timestamp: 2026-10-01T07-14
Command: git grep -n -F 'Contain("cannot marshal")' -- QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs ; git diff --numstat 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f -- QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
EXIT_CODE: 0
Output Summary:
- The search prints exactly one hit, on line 86: `errors.Should().ContainSingle().Which.Message.Should().Contain("cannot marshal");`.
- git diff --numstat MERGE-BASE: 24 added, 0 deleted (no existing line removed or changed; the new test is an insertion after line 88).
- Passing run of Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction cited from FEATURE/evidence/regression-testing/p1-t7-three-tests-pass.md: Total 3, executed 3, passed 3, failed 0.
- AC4 checked off in FEATURE/issue.md.
