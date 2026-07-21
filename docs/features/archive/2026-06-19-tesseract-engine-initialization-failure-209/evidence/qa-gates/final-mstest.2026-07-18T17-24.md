## Final-QC MSTest + Coverage Evidence (P2-T8 - P2-T13)

Timestamp: 2026-07-18T17-24

Command: `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-06-19-tesseract-engine-initialization-failure-209\evidence\qa-gates\coverage-final.cobertura.xml'` (console output piped via `Tee-Object` to `docs\features\active\2026-06-19-tesseract-engine-initialization-failure-209\evidence\qa-gates\mstest-final-console.2026-07-18T17-24.log`)

EXIT_CODE: 0

Output Summary:
- Discovered 8 test assemblies (identical set to baseline), run in a single `/InIsolation` vstest.console.exe process via `dotnet-coverage collect`.
- Post-change Total tests: 5701 / Passed: 5701 / Failed: 0 / Skipped: 0. "Test Run Successful." Total time 38.5656 seconds.
- Both named OCR tests pass: `Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken` [7 ms], `ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken` [1 ms].

### AC3 verification (P2-T10)

Grepped `mstest-final-console.2026-07-18T17-24.log` for `Failed loading language 'eng'` and `Error opening data file`: **0 occurrences of either string** (baseline had 2 occurrences of each). AC3 PASSES — the tessdata/Tesseract initialization-failure error lines that were present at baseline no longer appear in the post-fix run.

### AC4 verification (P2-T11) — itemized delta vs. baseline

- Total tests: 5701 (baseline) -> 5701 (final). No change in total test count.
- Passed: 5701 (baseline) -> 5701 (final). No change.
- Failed: 0 (baseline) -> 0 (final). No change; no test that passed at baseline now fails.
- Skipped: 0 (baseline) -> 0 (final). No change.
- Itemized delta for the two directly-affected tests: `Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken` and `ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken` both PASSED at baseline (via the live-engine-failure fallback path, evidenced by the 2+2 tessdata error occurrences recorded in `baseline-mstest.2026-07-18T17-10.md`) and both PASS at final (via the injected `Mock<IOcrTextExtractor>` returning `string.Empty`, which exercises the same "no text found" fallback token deterministically, with zero reliance on the live Tesseract engine). This is the expected, explained status change: the *mechanism* by which each test reaches its "no text found" assertion changed (live-engine-failure fallback -> deterministic mock), but the test's pass/fail *outcome* did not change (PASS -> PASS).
- No other test in the suite changed status. AC4 PASSES — no unexplained new failure, no unexplained new pass, no regression.

### Coverage delta (P2-T12)

- Baseline (from `coverage-baseline.cobertura.xml`): line-rate = 0.837981 -> 83.7981%; branch-rate = 0.76337 -> 76.337%.
- Final (from `coverage-final.cobertura.xml`): line-rate = 0.837806 -> 83.7806%; branch-rate = 0.763524 -> 76.3524%.
- Delta: line coverage -0.0175 percentage points; branch coverage +0.0154 percentage points. Both deltas are within the small-magnitude noise band previously documented for this repo's `dotnet-coverage` denominator (see prior-session memory on `dotnet-coverage` denominator nondeterminism); the new `TesseractOcrTextExtractor.cs` file adds a small amount of uncovered production code (the live-engine `ExtractText` body, which is intentionally not exercised by unit tests per the Moq-seam design — it requires a live Tesseract engine and tessdata to execute), while the two updated tests now exercise the same call paths deterministically. No coverage regression on the changed lines within `ImageStripper.cs` itself (the delegation body `return _ocrTextExtractor.ExtractText(bitmap);` is directly exercised by both updated tests via the mock).
