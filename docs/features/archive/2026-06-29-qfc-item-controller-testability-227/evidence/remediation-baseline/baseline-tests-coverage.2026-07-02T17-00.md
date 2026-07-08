# Baseline — Test Run + Coverage (Cycle 5)

- **Timestamp:** 2026-07-02T17-00
- **Command:** `MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /EnableCodeCoverage /InIsolation`
- **EXIT_CODE:** 0
- **Output Summary:** Total tests: 4442. Passed: 4442. Failed: 0. Total time: 30.6168 seconds. Matches the plan's expected baseline (cycle-4 exit state: `UtilitiesCS.Test.dll` 4093 + `QuickFiler.Test.dll` 349 = 4442). No pre-existing flaky failure observed in this run. Coverage attachment: `TestResults/3d1d278a-14f4-4b47-95a2-5040ee0ee6a0/DanMoisan_MEGALODON4_2026-07-02.21_10_23.coverage`, converted via `dotnet-coverage merge -f cobertura` to `evidence/remediation-baseline/baseline-coverage.2026-07-02T17-00.cobertura.xml`. Repo-wide (whole-process, all modules including vendored) line coverage: **line-rate 0.6362 (63.62%)**, lines-covered 105053 / lines-valid 165126 — close to the plan's expected ~63.28%, reflecting incremental drift since cycle-4 delivery.
