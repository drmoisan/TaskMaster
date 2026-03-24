# Baseline Test + Coverage Capture

Timestamp: 2026-03-23T00:15:00Z

Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug

EXIT_CODE: 0

Output Summary:
- Total Tests: 3,179
- Passed: 3,177
- Failed: 0
- Skipped: 2
- Failing Tests: None

Coverage:
- UtilitiesCS line coverage: 60.72% (0.6072) — below 80% target
- UtilitiesCS.Test line coverage: 97.95%
- Coverage report: coverage/coverage.cobertura.xml

Notes:
- "Failed loading language 'eng'" warnings are Tesseract OCR library warnings during test execution, not test failures (pre-existing)
