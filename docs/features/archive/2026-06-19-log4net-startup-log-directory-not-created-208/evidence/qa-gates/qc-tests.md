# QC — MSTest + Coverage (Issue #208, [P2-T4])

Timestamp: 2026-07-09T09-42

Command: vstest.console.exe "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll" /EnableCodeCoverage
(Run via VS18 vstest.console.exe with MSYS_NO_PATHCONV=1 and a Windows-style DLL path. The emitted
binary `.coverage` was converted to Cobertura with `dotnet-coverage merge -f cobertura`.)

EXIT_CODE: 0

Output Summary:
- Test result: Test Run Successful. Total tests: 239, Passed: 239, Failed: 0 (224 pre-existing + 15
  new LogDirectoryInitializer tests, all passing).
- New-code coverage (the extracted unit): TaskMaster.Logging.LogDirectoryInitializer line-rate 1.00
  (100%). Every executable line of ResolveLogDirectory, EnsureLogDirectory, EnsureLogDirectoryForPath,
  and the constructor is covered. Exceeds the >=90% new-code policy floor.
- The thin host-bound I/O wrapper LogDirectoryFileSystem is [ExcludeFromCodeCoverage] and is correctly
  absent from the coverage report (its System.IO calls cannot be exercised without touching the real
  filesystem, which test policy prohibits).
- First-party targeted module (TaskMaster.dll): line-rate 67.27% post-change vs 66.53% baseline — no
  regression (slight increase from the added covered unit).
- Whole-process root line-rate (15.20% this run) is not comparable to the baseline root figure
  (56.51%): the collector instrumented a different module set between runs (lines-valid 85354 vs
  71851), so the stable comparison basis is the first-party TaskMaster module rate above.
- Post-change Cobertura preserved at evidence/qa-gates/post-change.cobertura.xml.
