Timestamp: 2026-07-20T13-45
Command: `dotnet-coverage collect -f cobertura -s coverage-exclude-deedle.xml -o baseline-coverage.cobertura.xml -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation`
(equivalent, coverage-format-explicit version of the plan's stated
`vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`; `dotnet-coverage`
wrapping was used to obtain a numeric, parseable Cobertura line/branch-rate report instead of the
binary `.coverage` format that `/EnableCodeCoverage` alone produces, per established repo tooling
pattern. `/InIsolation` is required for this Moq-based test assembly per prior-session repo guidance.)
EXIT_CODE: 0
Output Summary:
- Total tests: 539. Passed: 539. Failed: 0. Total time: 6.5993 seconds.
- A first attempt without the module-exclude settings file produced 3 spurious failures
  (`InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`,
  `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`,
  `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`) with
  `System.TypeInitializationException` -> `Deedle.Reflection` -> `System.Security.VerificationException:
  Operation could destabilize the runtime` — a known, pre-existing coverage-instrumentation artifact of
  byte-rewriting the Deedle/F# assemblies (documented in repo agent memory across prior sessions), not a
  real regression and unrelated to the in-scope files. Passing a `-s` settings XML that excludes
  `.*Deedle.*` and `.*FSharp\.Core.*` from instrumentation (module-path exclude only, no test/production
  source change) eliminated the spurious failures; the reported `539 passed, 0 failed` result above is
  from that corrected run.
- Repository-wide totals (all instrumented first- and third-party assemblies, not the coverage-gate
  denominator): line-rate 0.2443166285770522 (24.43%), branch-rate 0.12617514101692204 (12.62%). This
  raw aggregate is expected to be low because it includes large third-party/vendored packages
  (log4net, System.Linq.Async, System.Interactive, FluentAssertions, Mono.Reflection,
  Microsoft.IO.RecyclableMemoryStream) with 0% or near-0% coverage; it is recorded here for reference
  only, not as the plan's coverage gate figure.
- `QuickFiler` package (production assembly containing the in-scope files): line-rate
  0.7366649404453651 (73.67%), branch-rate 0.645326504481434 (64.53%).
- `QuickFiler.Test` package: line-rate 0.955163387655155 (95.52%), branch-rate 0.9252199413489736
  (92.52%).
- Class-level baseline for `QuickFiler.Controllers.QfcItemController` sourced from
  `QfcItemController.FolderHandling.cs` specifically: line-rate 0.9154929577464789 (91.55%),
  branch-rate 0.7105263157894737 (71.05%).
- Method-level baseline for the two in-scope methods (from the same class/file):
  - `AssignFolderComboBox()`: line-rate 0.8846153846153846 (88.46%), branch-rate 0.8571428571428571
    (85.71%).
  - `PopulateAndSelectFolder(System.Windows.Forms.ComboBox, string[], string)`: line-rate 1 (100%),
    branch-rate 1 (100%).

These class/method-level figures are the baseline reference used by P2-T5's changed-line coverage
delta comparison.
