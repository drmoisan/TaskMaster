# vstest Baseline (Issue #232)

Timestamp: 2026-07-03T11-34

Command (plan-specified): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`

Command (actually executed): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Settings:<cobertura.runsettings> /ResultsDirectory:<scratchpad>`

Tooling note: The plain `/EnableCodeCoverage` collector emits a binary `.coverage` file that is not
reliably convertible to numeric per-file coverage offline in this environment (confirmed during prior
#227 work). To satisfy the AC's numeric requirements, the run uses a Cobertura-format runsettings
(same first-party + Swordfish module set and `[ExcludeFromCodeCoverage]` attribute-exclude as the
ratified #214 coverage runsettings). `/InIsolation` is required because the Moq-based test assemblies
otherwise fail to initialize the test host in this repo.

EXIT_CODE: 1

Output Summary:
- Total tests: 4637
- Passed: 4636
- Failed: 1
- Failing test (pre-existing, out of scope): `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` (UtilitiesCS.Test). This is unrelated to the QuickFiler files touched by Issue #232 and is the baseline failure count.
- Repository-wide line coverage (first-party + Swordfish module set): 76.58% (line-rate 0.76575789793438642; lines-covered 40334 / lines-valid 52672).
- `QfcHighConfidencePreFilter.cs` module coverage: 100% — every Cobertura `<class>` mapped to this file (`QfcPreScoredItem`, `QfcHighConfidencePreFilter.<>c`, `QfcHighConfidencePreFilter.<>c__DisplayClass0_0`, `QfcHighConfidencePreFilter.<FilterAsync>d__0`, and the nested lambda state machine) reports line-rate="1". `FolderScoringService` carries `[ExcludeFromCodeCoverage]` and is excluded from the denominator.
