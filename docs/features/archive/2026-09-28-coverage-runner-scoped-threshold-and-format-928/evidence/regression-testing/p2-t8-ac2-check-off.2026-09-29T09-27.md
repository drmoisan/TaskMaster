# P2-T8 AC2 Check-Off

Timestamp: 2026-09-29T09-27
Task: P2-T8
Command: Edit tool on issue.md (AC2 checkbox only); Grep count of `^- \[x\] AC` over issue.md before and after
EXIT_CODE: 0

Evidence read:

- evidence/regression-testing/p1-t6-pass-after.2026-09-29T09-21.md: It 11 `still terminates with an error when collection returns a non-zero exit code on a scoped run` passes. It passed before the fix too, as a control.
- evidence/qa-gates/p2-t3-test-coverage.iter1.2026-09-29T09-23.md: the new suite reports 14 tests and 0 failures in the full-population run, so It 11 passes there too.
- Test mechanics (tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1): the executable seam `Invoke-DotnetCoverageExe` is mocked to set `$global:LASTEXITCODE = 7`. The real `Invoke-DotnetCoverageCollection` wrapper runs, so the throw `MSTest with coverage failed with exit code 7` is raised by production code. `ConvertTo-KoverageCoberturaXml` is asserted not to run.

Check-off: the AC2 line changed from `- [ ] AC2:` to `- [x] AC2:`; no other character changed. Grep count of `^- \[x\] AC` rose from 1 to 2.

Output Summary: AC2 checked off. A scoped run whose collector exits with a non-zero code still terminates with an error through the real collection wrapper.
