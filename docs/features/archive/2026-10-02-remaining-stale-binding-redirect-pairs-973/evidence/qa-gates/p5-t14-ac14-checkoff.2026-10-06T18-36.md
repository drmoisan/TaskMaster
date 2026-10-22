# P5-T14 AC14 check-off (as amended, D2 and D13; spec Planner Amendments 2 and 4)

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC14 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifacts
EXIT_CODE: 0
Output Summary: AC14 met and checked off. CSharpier check exited 0. The test summary shows 7361 tests with failed 0 and `Failed tests: none`, and the direct route lines name the four excluded classes (Planner Amendment 4). The first-party summary is lines 85.35% and branches 79.74%, with both floors MET. Comparability is branch A: the denominators are equal and both deltas are -0.01, within the 0.10 tolerance. No .xml, .trx or .coverage file is in the diff.

Artifacts read:
- evidence/qa-gates/csharpier-check.md: `Command: dotnet tool run csharpier check .`; EXIT_CODE 0; `Checked 1638 files in 5686ms.`
- evidence/qa-gates/mstest-test-results-summary.md: EXIT_CODE 0. The SUMMARY second line is `Total 7361, executed 7361, passed 7361, failed 0.` and the fifth line is `Failed tests: none`. The file carries `COVERAGE-ROUTE: DIRECT (Planner Amendment 4)`, `ROUTE-FILTER:` naming HelperClasses.ShellUtilities_Tests, HelperClasses.ShellUtilitiesStatic_Tests, HelperClasses.SysImageListHelperTests and EmailIntelligence.OSBrowser_Tests, and `CI-COVERS-EXCLUDED: .github/workflows/_mstest-coverage.yml`.
- evidence/qa-gates/mstest-coverage-projection.md: EXIT_CODE 0; the package projection; `First-party coverage: lines 56207/65855 (85.35%), branches 13618/17078 (79.74%)`; LINE-FLOOR MET; BRANCH-FLOOR MET; the same route lines.
- evidence/qa-gates/p4-t10-coverage-comparison.2026-10-06T18-30.md: EXIT_CODE 0. The baseline line `First-party coverage: lines 56212/65855 (85.36%), branches 13620/17078 (79.75%)` matches the baseline artifact figure for figure. `LINES-VALID ... EQUAL=True`, `LINE-PERCENT-DELTA: -0.01 WITHIN-TOLERANCE=True`, `BRANCH-PERCENT-DELTA: -0.01 WITHIN-TOLERANCE=True`, `COMPARABILITY: A`.
- evidence/qa-gates/p4-t12-footprint.2026-10-06T18-34.md: the diff and porcelain over '*.xml' '*.trx' '*.coverage' are empty.

SPEC-LINE: `- [x] AC14 (format gate, test route, coverage, projections only).` (criterion text unchanged)
