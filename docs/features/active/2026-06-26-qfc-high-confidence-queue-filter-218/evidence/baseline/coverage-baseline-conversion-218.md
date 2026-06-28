Timestamp: 2026-06-26T20-43
Command: pwsh -NoProfile -Command "$coverage = Get-ChildItem -LiteralPath 'TestResults\issue218-baseline' -Recurse -Filter '*.coverage' | Sort-Object LastWriteTime -Descending | Select-Object -First 1; if ($null -eq $coverage) { throw 'No coverage file found in TestResults\issue218-baseline' }; dotnet-coverage merge $coverage.FullName -f cobertura -o 'docs\features\active\2026-06-26-qfc-high-confidence-queue-filter-218\evidence\baseline\coverage-baseline-218.cobertura.xml'"
EXIT_CODE: 0
Output Summary:
- dotnet-coverage version: 18.5.2.0 [win-x64 - .NET 10.0.9].
- Input coverage file: C:\Users\DanMoisan\repos\TaskMaster\TestResults\issue218-baseline\66001d66-6732-4adf-8cf3-a0c2f7574488\DanMoisan_MEGALODON4_2026-06-26.20_42_21.coverage.
- Output Cobertura XML: C:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-06-26-qfc-high-confidence-queue-filter-218\evidence\baseline\coverage-baseline-218.cobertura.xml.
- Output Cobertura XML size: 29383952 bytes.
- Baseline line coverage: 62.03% (100491 / 162006 lines).
- Baseline branch coverage reported by Cobertura root: 100.00%.
- QuickFiler\Controllers\QfcDatamodel.cs baseline line coverage: 0.00% (0 / 49 lines).
- QuickFiler\Controllers\QfcHomeController.cs baseline line coverage: 55.89% (242 / 433 lines).
- QuickFiler\Controllers\QfcHighConfidencePreFilter.cs baseline line coverage: 100.00% (27 / 27 lines).
- Issue #218 test seam in QuickFiler\Controllers\QfcDatamodel.cs baseline line coverage: 0.00% (0 / 0 seam-specific lines; seam not present in baseline source).
