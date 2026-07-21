Timestamp: 2026-07-20T14-32
Command: `dotnet-coverage collect -f cobertura -s coverage-exclude-deedle.xml -o final-coverage.cobertura.xml -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation`
(equivalent, coverage-format-explicit version of the plan's stated
`vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`, matching the
same tooling pattern and Deedle/FSharp module-exclude settings used for the P0-T12 baseline, for a
directly comparable numeric result)
EXIT_CODE: 0
Output Summary:
- Total tests: 541. Passed: 541. Failed: 0. Total time: 6.8048 seconds. (541 = 539 baseline tests +
  2 new regression tests added in P1-T2/P1-T3.)
- Repository-wide totals (reference only, not the coverage-gate denominator): line-rate
  0.2444818576379399 (24.45%), branch-rate 0.12631494740210392 (12.63%). Consistent with baseline
  (24.43%/12.62%; the small increase reflects the two new tests and the fix's added conditional
  logic).
- `QuickFiler` package: line-rate 0.7367671800181183 (73.68%), branch-rate 0.6462324393358876
  (64.62%). Baseline: 73.67%/64.53%. No regression; branch coverage improved slightly.
- `QuickFiler.Test` package: line-rate 0.9552276559865093 (95.52%), branch-rate 0.9252199413489736
  (92.52%). Unchanged from baseline.
- Class-level coverage for `QuickFiler.Controllers.QfcItemController` sourced from
  `QfcItemController.FolderHandling.cs`: line-rate 0.918918918918919 (91.89%), branch-rate
  0.7380952380952381 (73.81%). Baseline: 91.55%/71.05%. No regression; both line and branch coverage
  improved.
- Method-level coverage for the two fixed methods:
  - `AssignFolderComboBox()`: line-rate 0.8928571428571429 (89.29%), branch-rate 0.875 (87.5%).
    Baseline: 88.46%/85.71%. Improved (the new conditional branch is now exercised by both the
    existing multi-suggestion test and the new single-suggestion test).
  - `PopulateAndSelectFolder(System.Windows.Forms.ComboBox, string[], string)`: line-rate 1 (100%),
    branch-rate 1 (100%). Unchanged from baseline (already fully covered; the new conditional branch
    is exercised by the new `PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_...` test and the
    pre-existing multi-item/predetermined tests).
