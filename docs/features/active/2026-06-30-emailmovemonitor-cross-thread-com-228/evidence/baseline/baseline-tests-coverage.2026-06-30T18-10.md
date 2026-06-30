# Baseline — Test + Coverage State (Issue #228)

Timestamp: 2026-06-30T22-20
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
(/InIsolation required because QuickFiler.Test references Moq; without it the in-process test host raises a Setup FileNotFound under the VS18 test platform. Executed via Bash with MSYS_NO_PATHCONV=1 so /EnableCodeCoverage and /InIsolation are not path-converted.)
EXIT_CODE: 0

Output Summary:
- Total tests: 201
- Passed: 201
- Failed: 0
- Total time: 6.2273 s
- Test Run Successful.

Coverage (converted from the binary .coverage via dotnet-coverage merge -f cobertura):
- Whole-process line coverage (ALL loaded modules incl. vendored/third-party): 13.10% (lines-covered=10003, lines-valid=76355). This raw figure is NOT the policy gate; per CLAUDE.md the >=80% floor applies to the testable first-party denominator, and a single-assembly QuickFiler.Test run loads many un-exercised vendored modules (System.Interactive, System.Linq.Async, log4net, SVGControl, FluentAssertions, Swordfish) that deflate the whole-process number.
- Per first-party package line-rate from this run:
  - QuickFiler: 32.94%
  - QuickFiler.Test: 92.41%
  - UtilitiesCS: 3.81% (barely exercised by the QuickFiler.Test scope)
- The authoritative testable-denominator repo-wide figure and the changed-code delta are computed in P10-T1 against the same Cobertura schema.

Note: this baseline run exercises only the QuickFiler.Test assembly per the plan's P0-T5 command. EmailMoveMonitor.cs file-level baseline coverage is recorded separately in baseline-emailmovemonitor-coverage.2026-06-30T18-10.md.
