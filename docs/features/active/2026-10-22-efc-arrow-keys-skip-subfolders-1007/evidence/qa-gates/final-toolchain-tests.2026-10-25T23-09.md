# Final Toolchain: Tests with Coverage (Issue #1007, P19-T4, clean pass)

Timestamp: 2026-10-25T23-09
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 (unscoped run; launched from a helper under SCRATCH with Set-Location to the worktree; outer dotnet-coverage collect around the inner vstest.console.exe run)
EXIT_CODE: 0
Output Summary:
- Test run (summary derived from the trx): total 7734, executed 7734, passed 7734, failed 0, not executed 0. The shell-icon carve-out was not needed; the run finished in 42.7 seconds with no stall. BASELINE-FAILURES is empty, so no failure is excused.
- One-line first-party coverage summary: `First-party coverage: lines 58327/67510 (86.40%), branches 14409/17848 (80.73%)`.
- Repository line coverage 86.40 percent (floor 80); repository branch coverage 80.73 percent (floor 75).
- Register check derived from the trx: all 155 named tests are present and Passed. That covers every C# test in the Test Name Register, G1 to G8, including the guards and the data-driven `SelectorKeyMessage_RoundTripsOnlySupportedKeys` rows, and every Disposition Register C# test. The trx and the Cobertura and JaCoCo documents stay under the ignored `coverage/` folder and in SCRATCH; none is committed.

Trx-derived list (all Passed): G1 8 tests; G2 13; G3 15; G4 20; G5 13 session, 8 router, 7 coordinator and 3 serializer; G6 7; G7 13; G8 14 C# string contracts; Disposition Register 34. The names are those of the plan's Test Name Register and Disposition Register; the register-check helper under SCRATCH compares each name with the trx.
