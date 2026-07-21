## Baseline MSTest + Coverage Evidence (P0-T10 - P0-T13)

Timestamp: 2026-07-18T17-10

Command: `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-06-19-tesseract-engine-initialization-failure-209\evidence\baseline\coverage-baseline.cobertura.xml'` (console output piped via `Tee-Object` to `docs\features\active\2026-06-19-tesseract-engine-initialization-failure-209\evidence\baseline\mstest-baseline-console.2026-07-18T17-10.log`)

EXIT_CODE: 0

Output Summary:
- Discovered 8 test assemblies (ToDoModel.Test, UtilitiesCS.Test, QuickFiler.Test, TaskVisualization.Test, Tags.Test, TaskTree.Test, VBFunctions.Test, TaskMaster.Test), run in a single `/InIsolation` vstest.console.exe process via `dotnet-coverage collect`.
- Total tests: 5701 / Passed: 5701 / Failed: 0 / Skipped: 0. "Test Run Successful." Total time 34.6398 seconds.
- Tessdata-error occurrence count: `Failed loading language 'eng'` = 2 occurrences; `Error opening data file` = 2 occurrences (log lines 3879-3885, both against `C:\Users\DanMoisan\AppData\Local\TaskMaster\tessdata/eng.traineddata`). These errors are logged (caught) rather than surfaced as test failures, which is consistent with the two named OCR tests (`Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken`, `ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken`) asserting on the "no text found" fallback token that results when the live Tesseract engine fails to initialize — i.e., baseline tests pass today only because the current assertions tolerate the OCR-failure fallback path, not because OCR is functioning.
- Baseline coverage (from `coverage-baseline.cobertura.xml` root `<coverage>` element): line-rate = 0.837981 -> 83.7981%; branch-rate = 0.76337 -> 76.337%.
