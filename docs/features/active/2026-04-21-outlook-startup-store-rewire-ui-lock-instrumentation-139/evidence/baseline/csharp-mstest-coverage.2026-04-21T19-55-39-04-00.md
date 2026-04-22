# Phase 0 MSTest coverage baseline

Timestamp: 2026-04-21T19:55:39.6462917-04:00
Command: vstest.console.exe <test-assembly-paths> /EnableCodeCoverage
EXIT_CODE: 0

## Output Summary
- Total tests: 3945
- Passed: 3943
- Failed: 0
- Skipped: 2
- Test assembly count: 7
- Resolved vstest path: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
- Saved raw coverage artifact path: `C:\Users\DanMoisan\repos\TaskMaster\TestResults\3955f5c8-c3ff-4d9d-88d7-06558e343ab9\DanMoisan_MEGALODON4_2026-04-21.19_53_38.coverage`
- Saved processed coverage artifact path: `C:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139\evidence\baseline\csharp-mstest-coverage.2026-04-21T19-53-38-04-00.cobertura.xml`
- Raw merged Cobertura path: `C:\Users\DanMoisan\repos\TaskMaster\coverage\p0-t7-raw-coverage.2026-04-21T19-53-38-04-00.cobertura.xml`
- Baseline line rate: 0.782068
- Lines covered: 158766
- Lines valid: 203008
- Baseline branch rate: 0
- Branches covered: 0
- Branches valid: 0

## Notes
- The direct baseline command was executed with the repository's discovered `*.Test.dll` assemblies from `bin\Debug` output folders.
- To derive numeric coverage headline values from the generated `.coverage` file, a supplemental post-processing step converted the binary coverage output into Cobertura XML using `dotnet-coverage merge`, then normalized it with `scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1`.
- The first supplemental conversion attempt failed because `C:\Users\DanMoisan\repos\TaskMaster\artifacts\tmp` is an existing file, not a directory. The conversion was retried successfully using `C:\Users\DanMoisan\repos\TaskMaster\coverage\...` as the temporary raw XML path.
- This step created evidence files only; no source files were modified.
