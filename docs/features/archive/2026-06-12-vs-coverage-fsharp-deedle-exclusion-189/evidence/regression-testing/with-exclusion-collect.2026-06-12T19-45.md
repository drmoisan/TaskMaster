# WITH-Exclusion /collect Verification (P2-T1, AC4 pass-after)

Timestamp: 2026-06-12T19-45

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" \
  "C:\Users\DanMoisan\repos\TaskMaster\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" \
  /Tests:Deedle /collect:"Code Coverage" \
  /Settings:"C:\Users\DanMoisan\repos\TaskMaster\TaskMaster.runsettings" /InIsolation
```
(Run against the EDITED `TaskMaster.runsettings` with the `<DataCollectionRunSettings>` Exclude block active.)

EXIT_CODE: 0

Output Summary:
- `Test Run Successful. Total tests: 42, Passed: 42, Failed: 0`.
- No `System.Security.VerificationException` occurred. All Deedle test classes selected by `/Tests:Deedle` passed: `DfDeedle_COM_Tests`, `DfDeedle_Tests`, and `DeedleTests.DeedleDoodles` (e.g. `DeedleDoodles`, `FromArray2D_*`, `FromDefaultFolder_*`, `Email2dArrayToDf_*`, `GetEmailDataInView*` all Passed).
- Coverage attachment produced (collector active under `/collect`): `...\TestResults\ff160084-...\DanMoisan_MEGALODON4_2026-06-12.19_31_04.coverage`.
- AC4 pass-after satisfied: with the exclusion in place, the Deedle tests pass under `/collect:"Code Coverage"` and the VerificationException does not occur.

Note on scope: `/Tests:Deedle` substring-matches all three Deedle test classes (`DfDeedle_COM_Tests`, `DfDeedle_Tests`, `DeedleTests.DeedleDoodles`), broader than `/Tests:DfDeedle` (which would omit `DeedleDoodles`). This satisfies AC4's "or equivalent scoped to the Deedle test classes" and includes the full issue-reported set.

Note on CLI vs VS divergence: In this environment the CLI `/collect` collector did not reproduce the VerificationException even WITHOUT the exclusion (see `evidence/baseline/verificationexception-baseline.2026-06-12T19-45.md`). The WITH-exclusion run therefore confirms the exclusion does not break or alter the passing Deedle tests under coverage; the VS-native pass-after confirmation is recorded as pending in P2-T5 (AC6).
