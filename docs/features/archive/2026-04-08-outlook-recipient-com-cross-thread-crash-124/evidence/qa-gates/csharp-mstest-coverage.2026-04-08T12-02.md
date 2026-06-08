Timestamp: 2026-04-08T12-02
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
EXIT_CODE: 0
Output Summary: MSTest with coverage completed successfully; 3932 total tests, 3930 passed, 2 skipped, 0 failed, with line coverage 78.18% and branch coverage 63.26%.

Coverage Details:
- Coverage artifact: coverage/coverage.cobertura.xml
- Overall line coverage: 78.18%
- Overall branch coverage: 63.26%
- Final scoped line coverage: UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs => 82.95%; UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs => 83.44%
