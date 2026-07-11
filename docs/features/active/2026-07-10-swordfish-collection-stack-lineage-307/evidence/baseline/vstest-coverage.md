# Phase 0 — Baseline vstest + Coverage (P0-T6)

Timestamp: 2026-07-10T23-18
Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /InIsolation /Settings:TaskMaster.runsettings /Logger:trx`
(VS18 vstest.console 18.7.x; `/InIsolation` required for the Moq test assemblies — see prior
STTE Setup FileNotFound finding; `/Settings:TaskMaster.runsettings` enables the Code Coverage
DataCollector and applies the repo module excludes for Deedle/FSharp/Castle.Core/FluentAssertions/
Moq/MSTest/Microsoft.Testing. Coverage headline derived by converting the emitted `.coverage`
binary to Cobertura via `dotnet-coverage merge --output-format cobertura`, because the `.coverage`
format is not directly numeric-readable offline.)
EXIT_CODE: 0

## Output Summary

- **Test result: Test Run Successful.**
- **Total tests: 4680 — Passed: 4680 — Failed: 0 — Skipped: 0.**
- Total wall time: 51.0 s.
- Baseline coverage headline (Cobertura, converted from `.coverage`, includes vendored code):
  - **Line rate: 76.59%** (lines-covered 106,550 / lines-valid 139,120).
  - Branch rate: reported as 1.0 by the `.coverage`→Cobertura conversion; this conversion path
    does not emit reliable per-branch data, so the branch figure is not treated as an authoritative
    baseline number. The authoritative no-regression signals for F2 are the analyzer gate (green),
    the vendored-only nullable baseline set (84 errors), and the fully-green test count above.

## No-Regression Baseline Set (pre-existing failures)

Under the canonical invocation above (repo runsettings + `/InIsolation`), the suite is **fully
green (4680/4680)** — there is **no pre-existing failing set** to preserve. The no-regression bar
for F2 is therefore the stricter condition: **zero test failures** after migration, and all newly
added tests pass.

### Note on the ~28 Deedle/DataFrame failures observed earlier

An earlier exploratory run surfaced approximately 28 failures clustered on Deedle / F# DataFrame
types. Those failures were an artifact of applying Code Coverage instrumentation to the Deedle and
FSharp modules (a tooling-side interaction, not first-party test logic). The repository's
`TaskMaster.runsettings` explicitly excludes `.*Deedle.*` and `.*FSharp.*` (and Castle.Core,
FluentAssertions, Moq, MSTest, Microsoft.Testing) module paths from the coverage collector, which
removes that instrumentation interaction. With the runsettings applied — the canonical policy
command form — the suite is fully green. These Deedle/FSharp interactions are out of F2 scope and
are not first-party defects; F2 must simply preserve the green suite.

## Baseline Toolchain State (Phase 0 rollup)

| Gate | Command artifact | EXIT | Result |
|---|---|---|---|
| Format (csharpier --check) | `csharpier.md` | see artifact | baseline recorded |
| Analyzers | `msbuild-analyzers.md` | 0 | Build succeeded, 0 errors (green) |
| Nullable (TWAE) | `msbuild-nullable.md` | 1 | 84 errors, ALL vendored (Swordfish 50 + SVGControl 34); first-party 0 |
| Tests + coverage | this artifact | 0 | 4680/4680 passed; line 76.59% |
