# QA Gate — Test + Coverage (Issue #228)

Timestamp: 2026-06-30T22-52
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
(/InIsolation required for the Moq-referencing assembly; executed via Bash with MSYS_NO_PATHCONV=1.)
EXIT_CODE: 0

Output Summary:
- Total tests: 209
- Passed: 209
- Failed: 0
- Total time: 6.1054 s
- Test Run Successful.
- New EmailMoveMonitorTests: 8 tests, all passed (201 baseline + 8 new = 209).

Coverage (post-change, converted via dotnet-coverage merge -f cobertura):
- Whole-process line coverage (ALL loaded modules incl. vendored): 13.38% (lines-covered=10243, lines-valid=76570). Raw whole-process figure; not the policy gate (see coverage-delta artifact for the testable-denominator analysis).
- QuickFiler package line-rate (post): see coverage-delta artifact.
- EmailMoveMonitor.cs file-level (all classes incl. dormant async members + event-handler body): 44.03% (70/159). Diluted by the out-of-scope dormant UnhookItemAsync/GetParentFolderAsync state machines (0%) and the COM-host-bound BeforeItemMove handler body.
- Changed/new EmailMoveMonitor bookkeeping (in-scope: constructor, HookItem, UnhookItem, UnhookAll, EmailMoveAction ctor+properties): 96.92% (63/65). Exceeds the >=90% new/changed-code floor. The two uncovered lines (244 Mail getter, 250 MoveAction getter) are trivial auto-property getters not read by the bookkeeping path.

Test step is clean in the final pass; all tests pass.
