# Baseline — MSTest Suite with Coverage

Timestamp: 2026-06-13T12-05

Command: pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.baseline.cobertura.xml
(Repo coverage pipeline: dotnet-coverage collect --settings coverage.config -- vstest.console.exe <*.Test.dll> /Settings:TaskMaster.cli.runsettings /InIsolation, then Koverage post-processing.)

EXIT_CODE: 1
(Non-zero because the vstest run reports 2 pre-existing failures; the pipeline script throws on the non-zero vstest exit code. The 2 failures are the known pre-existing flaky timing/threading tests, not a regression. The coverage cobertura was still produced.)

## Test results
- Total tests: 4068
- Passed: 4066
- Failed: 2
- Pre-existing failures (roadmap §0.1):
  - AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException
  - RequestTask_WithProvidedTask_InvokesTaskAfterInterval

## Coverage headline (production-only deduped, authoritative roadmap §0.2 convention)
Method: sum of `<line>` hits/total across all non-`.Test` first-party + vendored (Swordfish.NET.General, SVGControl held constant per memo §2.6) production packages in the Koverage-deduped Cobertura.

- Authoritative documented baseline (roadmap §0.2): 38,767 covered / 65,768 lines-valid = 58.95%
- Reproduced from committed artifacts/csharp/coverage-firstparty.cobertura.xml using the inclusive convention: 38,820 covered / 65,768 lines-valid = 59.03% (matches lines-valid exactly; covered within rounding granularity)
- Freshly regenerated baseline (coverage/coverage.baseline.firstparty.cobertura.xml) produces identical first-party package line counts, confirming the re-measurement method is deterministic and reproducible for the Phase 7 delta.

## Artifacts
- Raw dotnet-coverage output: coverage/coverage.baseline.cobertura.xml (lines-valid 140,254 incl. test+third-party; not the denominator)
- Koverage-deduped baseline: coverage/coverage.baseline.firstparty.cobertura.xml
- Authoritative baseline preserved in evidence: evidence/baseline/coverage-firstparty.baseline.cobertura.xml (P0-T7)
