Timestamp: 2026-07-03T17:49:20.1046660-04:00
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-results
EXIT_CODE: 0
Output Summary:
- VSTest version 18.7.0 (x64).
- Test Run Successful.
- Total tests: 382.
- Passed: 382.
- Total time: 8.6774 seconds.
- Coverage attachment:
  - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-results/d6d6f998-bf78-4a04-85d2-859e5219e314/DanMoisan_MEGALODON4_2026-07-03.17_49_06.coverage`
- Numeric coverage values: remediation required. VSTest emitted a binary `.coverage` file. The available `CodeCoverage.exe` converter was found at `C:\Program Files\Microsoft Visual Studio\18\Community\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe`, but the attempted conversion command exited 1 and returned only generic usage/deprecation output.
- Conversion command attempted: `$coverage = Get-ChildItem -Path 'docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-results' -Recurse -Filter '*.coverage' | Select-Object -First 1 -ExpandProperty FullName; & 'C:\Program Files\Microsoft Visual Studio\18\Community\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe' analyze /output:'docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-results\final.coveragexml' $coverage`
- Conversion exit code: 1.
