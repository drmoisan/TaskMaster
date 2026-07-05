Timestamp: 2026-07-04T18-52
Command: pwsh -NoProfile -Command "Invoke-Pester -Path 'tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1' -CI"
EXIT_CODE: 1
Output Summary:
- Expected fail-before result for [P1-T2].
- Discovery found 7 tests.
- Tests Passed: 6
- Failed: 1
- Skipped: 0
- Failing test: ConvertTo-KoverageCoberturaXml.normalizes stale TaskMaster roots before merging duplicate production class entries.
- Failure proof: expected aggregate `lines-valid` to be `3`, but actual value was `4`, proving stale-root and relative entries remained separate before the helper fix.
