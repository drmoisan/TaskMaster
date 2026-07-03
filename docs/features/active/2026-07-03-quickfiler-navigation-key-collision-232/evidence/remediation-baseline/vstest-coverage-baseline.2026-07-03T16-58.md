# vstest Coverage Baseline — Remediation Cycle 1 (Issue #232)

Timestamp: 2026-07-03T16-58

Command (plan-specified): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`

Command (actually executed): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Settings:<scratchpad>\cobertura.runsettings /ResultsDirectory:<scratchpad>\results-baseline`
(vstest.console.exe resolved to `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe`)

Tooling note: identical rationale to the prior #232 evidence (`evidence/qa-gates/vstest-final.md`) and the
ratified #214/#227 configuration. The plain `/EnableCodeCoverage` binary `.coverage` collector is not
reliably convertible to numeric per-file coverage offline in this environment, so the same Cobertura-format
runsettings (identical first-party + Swordfish module set and `[ExcludeFromCodeCoverage]`/`GeneratedCode`
attribute excludes) is used. `/InIsolation` is required for the Moq-based test assemblies to initialize the
test host. This baseline run is over the pre-change source state (before the Phase 1 caller-context string
correction). Because `QfcDatamodel.cs` carries `[ExcludeFromCodeCoverage]`, the Phase 1 string change has no
effect on coverage numerics.

EXIT_CODE: 1

Output Summary:
- Total tests: 4641
- Passed: 4640
- Failed: 1
- Total time: 51.60 seconds
- The single failure is the known pre-existing flaky test
  `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` (UtilitiesCS.Test), unrelated to the
  QuickFiler files under Issue #232. This matches the established baseline failure count of 1.
- Repository-wide line coverage (first-party + Swordfish module set): 76.5750%
  (line-rate 0.76574952561669829; lines-covered 40355 / lines-valid 52700; branch-rate 0.7208237986270023).
- `QfcHighConfidencePreFilter.cs` mapped classes — all six report line-rate="1":
  - `QuickFiler.Controllers.QfcHighConfidencePreFilter` = 1
  - `QuickFiler.Controllers.QfcPreScoredItem` = 1
  - `QuickFiler.Controllers.QfcHighConfidencePreFilter.<>c` = 1
  - `QuickFiler.Controllers.QfcHighConfidencePreFilter.<>c__DisplayClass1_0` = 1
  - `QuickFiler.Controllers.QfcHighConfidencePreFilter.<FilterAsync>d__1` = 1
  - `QuickFiler.Controllers.QfcHighConfidencePreFilter.<>c__DisplayClass1_0.<<FilterAsync>b__0>d` = 1
- Generated baseline XML: `<scratchpad>\results-baseline\d07e2ba8-46be-47d7-bcbb-da99d0dacd7a\DanMoisan_MEGALODON4_2026-07-03.17_09_38.cobertura.xml`
