# R1 P0-T7 Part-File Credit Diagnostic Gate

Timestamp: 2026-09-29T10-47
Task: P0-T7 (remediation-plan.2026-09-29T10-00.md)
Command: CMD-DIAG-CONTROL then CMD-DIAG-ORDERED through the Bash tool, each with the single `<worktree-root>` substitution (the absolute worktree path, transcribed as `<repo-root>`); no other change to either command, and no `UseBreakpoints` or other configuration key added. The EXIT_CODE row is the ordered run's exit.
EXIT_CODE: 0

## CMD-DIAG-CONTROL

Set-Location argument transcribed as `"<repo-root>"`; `$o` = `coverage/r1-p0-t7-diag-control.jacoco.xml` (ignored coverage directory; not committed).

- DIRECT_PESTER_EXIT_CONTROL: 0
- PESTER_COUNTS: passed=5 failed=0 skipped=0
- CONTAINER_ORDER: Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
- FILE_LINE: Invoke-MSTestWithCoverage.Scope.ps1 total=5 uncovered=0 uncovered_lines=
- FILE_LINE: Invoke-MSTestWithCoverage.Threshold.ps1 total=33 uncovered=11 uncovered_lines=35,44,48,53,54,95,104,108,118,123,124

## CMD-DIAG-ORDERED

Set-Location argument transcribed as `"<repo-root>"`; `$o` = `coverage/r1-p0-t7-diag-ordered.jacoco.xml`; Run.Path = the AssemblyDiscovery suite then the Scope suite, in that order.

- DIRECT_PESTER_EXIT: 0
- PESTER_COUNTS: passed=19 failed=0 skipped=0
- CONTAINER_ORDER: Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1,Invoke-MSTestWithCoverage.Scope.Tests.ps1
- FILE_LINE: Invoke-MSTestWithCoverage.Scope.ps1 total=5 uncovered=0 uncovered_lines=
- FILE_LINE: Invoke-MSTestWithCoverage.Threshold.ps1 total=33 uncovered=7 uncovered_lines=35,44,48,95,104,108,118

## Derivations

- CONTROL-THROW-LINES-UNCOVERED: yes (53, 54, 123 and 124 are each listed in the control run's Threshold uncovered_lines)
- ORDERED-THROW-LINES-CREDITED: yes (none of 53, 54, 123 and 124 is listed in the ordered run's Threshold uncovered_lines)
- ORDERED-SCOPE-PART-FILE: total=5 uncovered=0

PART-FILE-CREDIT-PREMISE: confirmed

Output Summary:
- Control (AssemblyDiscovery suite alone): 5 passed; the four Threshold throw lines 53, 54, 123, 124 are uncovered.
- Ordered (AssemblyDiscovery then Scope suite, container order confirmed): 19 passed; the four throw lines, reached only by the later-sorting Scope suite, are credited. Scope part file 5 of 5.
- A path-loaded part-file line reached only by a later-sorting suite is credited under Pester 5.6.1 breakpoint coverage. Verdict: PART-FILE-CREDIT-PREMISE confirmed; the plan proceeds to Phase 1.
