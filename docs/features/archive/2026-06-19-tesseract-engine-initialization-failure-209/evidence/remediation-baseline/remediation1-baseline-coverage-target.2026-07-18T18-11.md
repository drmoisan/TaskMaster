Timestamp: 2026-07-18T18-11

Command: `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-06-19-tesseract-engine-initialization-failure-209\evidence\remediation-baseline\remediation1-coverage-baseline.cobertura.xml'`

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 5701; Passed: 5701; Failed: 0; Skipped: 0.
- Total time: 56.2410 seconds.
- Repo-wide baseline coverage (root `<coverage>` element in `remediation1-coverage-baseline.cobertura.xml`): line-rate=0.837729 (83.7729%), branch-rate=0.763407 (76.3407%); lines-covered=86555, lines-valid=103321; branches-covered=19531, branches-valid=25584.
- Target-file baseline coverage: `<class name="UtilitiesCS.EmailIntelligence.TesseractOcrTextExtractor" filename="UtilitiesCS\EmailIntelligence\EmailParsingSorting\TesseractOcrTextExtractor.cs">` — line-rate="0", branch-rate="1", complexity="1". The single method `ExtractText(System.Drawing.Bitmap)` reports 13 `<line>` entries (line numbers 32, 33, 34, 37, 38, 39, 40, 41, 43, 44, 46, 47, 49), all `hits="0"`. This matches the expected value going into this cycle: 0 of 13 executable lines covered.

Note: An earlier attempt at this same baseline command (started 2026-07-18T17-56, using `Tee-Object` inside a nested `pwsh -Command` wrapper) stalled and produced no completion marker or coverage artifact; its partial, non-canonical console log was discarded. This artifact reflects the successful retry (redirect-to-file, single `pwsh -NoProfile -File` invocation) which completed cleanly with EXIT_CODE 0.
