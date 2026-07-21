# Phase 2 — Final-QC Test-and-Coverage Pass (P2-T4)

- Timestamp: 2026-07-11T00-15
- Command: `MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /EnableCodeCoverage /ResultsDirectory:"TestResults\P2T4QC"`
- EXIT_CODE: 0
- Output Summary: `Test Run Successful.` Total tests: 4245, Passed: 4245, Failed: 0, Skipped: 0. Total time: 24.4866 seconds. This is exactly 23 fewer tests than the P0-T5 baseline (4268 -> 4245), which exactly matches the number of `[TestMethod]` attributes in the deleted `ScoSortedDictionary_Tests.cs` (verified via `git show HEAD:UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs | grep -c "\[TestMethod\]"` = 23). Zero occurrences of `ScoSortedDictionary` anywhere in the run output/test names — **confirmed the `ScoSortedDictionary_Tests` test class no longer appears in the run**.
  - Coverage `.coverage` file: `TestResults/P2T4QC/258c221a-4f73-4539-9196-d531be6a9bb6/DanMoisan_MEGALODON4_2026-07-10.23_15_40.coverage`, converted to Cobertura XML via `dotnet-coverage merge -o <out>.cobertura.xml -f cobertura <file>.coverage` at `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/evidence/qa-gates/qc-coverage.2026-07-11T00-15.cobertura.xml`.
  - **`UtilitiesCS.dll` module line-coverage (post-change): 88.23%** (`<package name="UtilitiesCS" line-rate="0.8822898745854838">`; lines-covered=... lines-valid=... complexity=1272, down from complexity=1276 at baseline, consistent with removing `ScoSortedDictionary.cs`).
  - **Overall line-coverage for this run: 60.54%** (root `<coverage line-rate="0.6054059721005958" lines-covered="98169" lines-valid="162154">`).
  - Full per-class delta analysis is in `coverage-delta.2026-07-11T00-15.md`.
