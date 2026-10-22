# Pass-after: BindingRedirectSync module tests (R1, CR-1 and CR-3, issue #985)

Timestamp: 2026-10-09T15-23
Command: pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-pester.ps1 -WorkspaceRoot WORKSPACE-ROOT -TestPath tests/scripts/dependencies/BindingRedirectSync.Tests.ps1
EXIT_CODE: 0
Output Summary:
- TEST-PATH-COUNT: 1; Pester 5.6.1
- PESTER Passed=24 Failed=0 Skipped=0 NotRun=0 Total=24
- FILE-TESTS BindingRedirectSync.Tests.ps1 Passed=24 Failed=0 Total=24
- No FAILED-TEST line. The seven tests that failed in `fail-before-sync-module.2026-10-09T15-22.md` (N1 to N6 and the modified report test) now pass.
