# P9-T19 Non-numeric adapter final MSTest and coverage gate

Timestamp: 2026-07-27T08-43

Command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-nonnumeric-adapter-final.2026-07-27T08-43.cobertura.xml
```

EXIT_CODE: 1

## Result

The wrapper discovered eight test assemblies and reported 6,066 total tests: 6,058 passed and 8 failed. It exited with code 1 and reported `MSTest with coverage failed with exit code 1`.

The wrapper did not emit a result file containing the individual failed test names. A read-only inspection of `TestResults` found only the earlier `p5t171.trx` result; no new TRX was available for this run. No retry was performed.

The eight discovered assemblies were:

- `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
- `Tags.Test/bin/Debug/Tags.Test.dll`
- `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
- `TaskTree.Test/bin/Debug/TaskTree.Test.dll`
- `TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll`
- `ToDoModel.Test/bin/Debug/ToDoModel.Test.dll`
- `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
- `VBFunctions.Test/bin/Debug/VBFunctions.Test.dll`

## Coverage outputs

- Cobertura file: `coverage-nonnumeric-adapter-final.2026-07-27T08-43.cobertura.xml`
- Cobertura SHA-256: `5EF8F331EF5D2AD128645DF49F383FCFDAC00BFF41355F3F786164D427659F31`
- `coverage.config` SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
- Effective coverage settings deletion proof: the output-adjacent `coverage-nonnumeric-adapter-final.2026-07-27T08-43.cobertura.xml.effective-coverage.config` path does not exist after wrapper completion; no effective coverage settings files remain in the evidence directory.
- Lines covered: 54,777
- Lines valid: 79,129
- Line coverage: 69.2249%

The zero-failure acceptance criterion was not met. The measured line coverage is also below the required 80% threshold.

## Cleanup and continuation

No live issue-400 VSTest, testhost, or dotnet processes remained after the wrapper exited, so no process cleanup was required.

`[P9-T19]` remains unchecked. Per the plan, this failure invalidates P9-T16 through P9-T21 and requires a restart at `[P9-T12]`. P9-T20 and P9-T21 were not run.
