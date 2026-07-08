# Phase 0 — VerificationException Baseline / Without-Exclusion Reproduction Attempt (P0-T5)

Timestamp: 2026-06-12T19-45

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" \
  "C:\Users\DanMoisan\repos\TaskMaster\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" \
  /Tests:Deedle /collect:"Code Coverage" \
  /Settings:"C:\Users\DanMoisan\repos\TaskMaster\TaskMaster.runsettings" /InIsolation
```
(Run against the pre-edit `TaskMaster.runsettings`, i.e. WITHOUT the `<DataCollectionRunSettings>` exclusion block.)

EXIT_CODE: 0

Output Summary:
- The WITHOUT-exclusion `/collect:"Code Coverage"` CLI run did NOT reproduce `System.Security.VerificationException` in this environment.
- Result: `Test Run Successful. Total tests: 42, Passed: 42`. All `DfDeedle_COM_Tests`, `DfDeedle_Tests`, and `DeedleTests.DeedleDoodles` tests passed (e.g. `DeedleDoodles [328 ms]`, `FromArray2D_*`, `FromDefaultFolder_*` all passed).
- A `.coverage` attachment was produced: `...\TestResults\81b976a6-...\DanMoisan_MEGALODON4_2026-06-12.19_29_02.coverage`.
- Interpretation: the CLI `vstest.console.exe /collect:"Code Coverage"` data collector (VSTest 18.7.0) in this environment instruments differently than Visual Studio's "Analyze Code Coverage for All Tests" path (`datacollector://microsoft/CodeCoverage/2.0`). The `VerificationException` reported in issue #189 manifests under the VS IDE coverage collector; it does not reproduce from this CLI `/collect` invocation. The scoped `/collect` reproduction of the failure is therefore IMPRACTICAL in this environment.
- Per P0-T5, the fail-before requirement is satisfied by the schema-valid fail-before exception dossier:
  `evidence/regression-testing/fail-before-exception.2026-06-12T19-45.md`.

Note: This executed-run artifact documents that the without-exclusion CLI run is not a valid fail-before reproduction (it passes rather than throwing), which is why the plan-permitted exception-dossier fallback is used.
