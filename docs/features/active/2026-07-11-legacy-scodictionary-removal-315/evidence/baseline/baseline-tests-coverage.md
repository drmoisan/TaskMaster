# Baseline — UtilitiesCS.Test Tests + Coverage

Timestamp: 2026-07-11T11-46
Command: `vstest.console.exe "C:\Users\DanMoisan\repos\TaskMaster-wt\legacy-scodictionary-removal-315\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /EnableCodeCoverage /InIsolation`
EXIT_CODE: 0
Output Summary:
- Test Run Successful.
- Total tests: 4255
- Passed: 4255
- Failed: 0
- Total time: 42.66 s
- Coverage attachment (.coverage): `TestResults/a485cb55-26aa-4ab7-982f-a78278518ede/DanMoisan_MEGALODON4_2026-07-11.11_46_42.coverage`
- Numeric coverage headline (whole-attachment, via `dotnet-coverage merge -f cobertura`):
  - Line coverage: 60.54% (line-rate 0.6054016; lines-covered 98382 / lines-valid 162507)
  - Branch coverage: not emitted as a numeric count by the dotnet-coverage cobertura converter for this attachment (branch-rate reported as 1 with no branch counts); line coverage is the authoritative headline here.
- Note: this whole-attachment figure spans all modules loaded during the UtilitiesCS.Test run (including vendored Swordfish/SVGControl), so it is a stable comparison anchor for P5-T7 rather than a first-party-only figure. `/InIsolation` was required for the Moq-based assemblies per prior environment findings.
