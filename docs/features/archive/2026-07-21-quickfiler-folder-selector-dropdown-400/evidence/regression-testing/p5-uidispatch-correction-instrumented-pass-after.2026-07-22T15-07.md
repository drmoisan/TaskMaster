# P5-T182 — Two consecutive instrumented focused runs of `BreadcrumbUiThreadDispatchTests`

Timestamp: 2026-07-22T15-07Z

Command: `$coverageConfig=(Resolve-Path 'coverage.config').Path; $cliRunSettings=(Resolve-Path 'scripts\vscode\TaskMaster.cli.runsettings').Path; $quickFilerTestAssembly=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $installation=& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $filter='FullyQualifiedName~BreadcrumbUiThreadDispatchTests'; foreach($i in 1,2){ $coverageOutput=Join-Path $env:TEMP "p5t182-run$i.cobertura.xml"; $coverageArgs = @('collect','--output',$coverageOutput,'--output-format','cobertura','--settings',$coverageConfig,'--',$vstestPath,$quickFilerTestAssembly,"/Settings:$cliRunSettings",'/InIsolation',"/TestCaseFilter:$filter"); & dotnet-coverage @coverageArgs; $LASTEXITCODE }`

EXIT_CODE: 0

Inputs: only `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, the unchanged repo-root `coverage.config`, and
`scripts\vscode\TaskMaster.cli.runsettings`. The filter was not narrowed.

## Run results

| Run | Exit code | Total | Passed | Failed | Skipped | `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` | Cobertura written |
|---|---:|---:|---:|---:|---:|---|---|
| 1 | 0 | 9 | 9 | 0 | 0 | Passed (296 ms) | yes |
| 2 | 0 | 9 | 9 | 0 | 0 | Passed (303 ms) | yes |

Both runs reached natural completion with `Test Run Successful.` and exit code `0`. Neither run was terminated,
timed out, or produced a partial artifact.

## Determinism conclusion

Combined with the uninstrumented P5-T181 result (9/9, exit 0), the correction is deterministic under **both**
instrumented `dotnet-coverage` execution and uninstrumented execution, and not only in isolation. No restart at P5-T172
was triggered.

Output Summary: Two consecutive `dotnet-coverage collect` instrumented runs of the unmodified
`FullyQualifiedName~BreadcrumbUiThreadDispatchTests` filter both reached natural completion with exit `0` and 9 passed,
0 failed, 0 skipped; the previously failing case passed in both (296 ms, 303 ms) and a complete Cobertura artifact was
produced by each run. The correction is proven deterministic under instrumentation. EXIT_CODE: 0.
