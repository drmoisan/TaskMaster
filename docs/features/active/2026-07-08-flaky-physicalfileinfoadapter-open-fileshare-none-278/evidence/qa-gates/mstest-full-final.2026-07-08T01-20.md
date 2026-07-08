Timestamp: 2026-07-08T01-20

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage

EXIT_CODE: 0

Output Summary: Total tests: 4173, Passed: 4173, Failed: 0. Total time: 21.84 seconds. No pre-existing flaky failures observed in this run (a clean pass).

Coverage: converted the produced .coverage file with Microsoft.CodeCoverage.Console.exe merge -f xml.
- Assembly-wide (UtilitiesCS.dll module) line coverage: 86.02% (lines_covered=36978, lines_partially_covered=992, lines_not_covered=5016; block_coverage=86.89%).
- `PhysicalFileInfoAdapter.Open(System.IO.FileMode, System.IO.FileAccess)`: line_coverage="100.00" in this full-suite run (the delegation line, formerly line 134, is covered).
- Aggregated `PhysicalFileInfoAdapter` class coverage across the full suite: covered 51, partially covered 18, not covered 6, total 75 lines -> 68.00% (covered-only) / 92.00% (covered+partial). The full suite also exercises `PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles`, which increases coverage of the missing-file branches beyond the single targeted-test run.
