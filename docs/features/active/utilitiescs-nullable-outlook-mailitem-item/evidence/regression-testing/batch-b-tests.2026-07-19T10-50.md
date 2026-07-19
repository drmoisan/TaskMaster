# Batch B — CidImageResolver Tests + Coverage (P2-T4)

- Timestamp: 2026-07-19T10-50
- Task: [P2-T4]
- Command: `dotnet-coverage collect --output <cobertura> --output-format cobertura --settings coverage.config -- vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~CidImageResolver`
  - Equivalent to the plan-literal `vstest.console.exe ... /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~CidImageResolver"` but emits parseable Cobertura instead of a binary `.coverage`, so a numeric per-file figure is obtainable (required because CidImageResolver.cs is the one NON-exempt file). `coverage.config` module excludes prevent Deedle/FSharp instrumentation flakiness.
- EXIT_CODE: 0
- Cobertura XML: `evidence/regression-testing/batch-b-coverage.cobertura.xml`

## Output Summary

- Tests: Total 3, Passed 3, Failed 0 (`CidImageResolverTests.cs`).
- `CidImageResolver.cs` line coverage under the scoped 3-test filter: **89.47% (34/38)**. Uncovered lines 40–41 and 74–75 are the two pre-existing defensive early-return branches (`if (attachments is null) return map;`, `if (string.IsNullOrEmpty(html)) return html;`) that this 3-test filter does not drive; the full-suite baseline (P0-T5) covers them at **94.7% (36/38)** and the full-suite final gate (P10-T4) re-verifies file parity.
- Changed-line no-regression (AC4): the ONLY changed executable line is line 48 (`map[attachment!.ContentId] = attachment;`, the null-forgiving annotation). It is COVERED in this run (not among the uncovered lines) and was covered at baseline. No coverage regression on changed lines.
