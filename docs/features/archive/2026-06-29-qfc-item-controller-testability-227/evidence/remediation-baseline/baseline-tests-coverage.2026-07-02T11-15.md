Timestamp: 2026-07-02T14:20
Command: MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /EnableCodeCoverage /InIsolation /Logger:"console;verbosity=minimal"
(coverage converted for numeric readout with: Microsoft.CodeCoverage.Console.exe merge <file>.coverage -f xml -o TestResults/baseline-cycle3.coverage.xml)
EXIT_CODE: 0
Output Summary:
- QuickFiler.Test.dll (net481): Passed 328, Failed 0, Skipped 0, Total 328, Duration 7s.
- UtilitiesCS.Test.dll (net481): Passed 4089, Failed 0, Skipped 0, Total 4089, Duration 42s. (No occurrence this run of the known pre-existing flaky dispatcher timing test noted in project memory; full pass.)
- Repo-wide module line_coverage (Microsoft.CodeCoverage.Console XML, per-module): QuickFiler.dll = 45.69% (3082/6746 lines, whole module incl. all clusters/exempt+non-exempt mixed), UtilitiesCS.dll = 85.62% (36756/42928 lines).
- QfcItemController affected-denominator coverage (current, cycle-2 delivered 41-member exemption boundary; sum of all instrumented `type_name` containing "QfcItemController" across the 9 partial-class functions — `[ExcludeFromCodeCoverage]` members are not instrumented and therefore excluded from this denominator automatically): lines_covered=989, lines_partially_covered=32, lines_not_covered=323, total=1344, coverage=73.59%.
- Baseline coverage artifact retained at: TestResults/baseline-cycle3.coverage.xml (working file, not committed evidence; numeric values captured above are the evidence of record).
